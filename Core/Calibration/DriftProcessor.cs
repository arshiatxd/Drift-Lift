using System;
using DriftLift.Models;

namespace DriftLift.Core.Calibration
{
    public class DriftProcessor
    {
        // ##== Fields & Zero-Allocation Ring Buffers ==##
        private readonly int _windowSize;
        private readonly (double x, double y)[] _leftRing;
        private readonly (double x, double y)[] _rightRing;
        private int _leftHead;
        private int _rightHead;
        private int _leftCount;
        private int _rightCount;
        private double _leftSumX, _leftSumY;
        private double _rightSumX, _rightSumY;
        private readonly object _leftLock = new();
        private readonly object _rightLock = new();

        private readonly double[] _leftMaxRadiusBuckets = new double[360];
        private readonly double[] _rightMaxRadiusBuckets = new double[360];
        private int _leftBucketCount = 0;
        private int _rightBucketCount = 0;
        private double _leftBucketSumErr = 0;
        private double _rightBucketSumErr = 0;

        private double _filteredLeftX, _filteredLeftY;
        private double _filteredRightX, _filteredRightY;

        public CalibrationProfile Profile { get; set; } = new CalibrationProfile();
        public StickDriftMetrics LeftMetrics { get; } = new StickDriftMetrics();
        public StickDriftMetrics RightMetrics { get; } = new StickDriftMetrics();

        public double SimulatedLeftOffsetX { get; set; }
        public double SimulatedLeftOffsetY { get; set; }
        public double SimulatedRightOffsetX { get; set; }
        public double SimulatedRightOffsetY { get; set; }

        public DriftProcessor(int historyWindowSamples = 120)
        {
            _windowSize = Math.Max(10, historyWindowSamples);
            _leftRing = new (double x, double y)[_windowSize];
            _rightRing = new (double x, double y)[_windowSize];
            ResetMetrics();
        }

        public void ResetMetrics()
        {
            Array.Clear(_leftMaxRadiusBuckets, 0, 360);
            Array.Clear(_rightMaxRadiusBuckets, 0, 360);
            _leftBucketCount = 0;
            _rightBucketCount = 0;
            _leftBucketSumErr = 0;
            _rightBucketSumErr = 0;

            LeftMetrics.LiveCircularityError = 0;
            LeftMetrics.AverageCircularityError = 0;
            LeftMetrics.MinCircularityError = 999;
            LeftMetrics.MaxCircularityError = 0;
            RightMetrics.LiveCircularityError = 0;
            RightMetrics.AverageCircularityError = 0;
            RightMetrics.MinCircularityError = 999;
            RightMetrics.MaxCircularityError = 0;
        }

        public (double lx, double ly, double lDeadzone, double rx, double ry, double rDeadzone) AutoFixStickDrift(double currentLx = 0, double currentLy = 0, double currentRx = 0, double currentRy = 0)
        {
            return AutoCalibrateBoth(currentLx, currentLy, currentRx, currentRy);
        }

        // ##== Auto Calibration & Calculation ==##
        public (double lx, double ly, double lDeadzone, double rx, double ry, double rDeadzone) AutoCalibrateBoth(double currentLx, double currentLy, double currentRx, double currentRy)
        {
            AnalyzeRingAndCurrent(_leftRing, _leftCount, _leftSumX, _leftSumY, _leftLock,
                currentLx + SimulatedLeftOffsetX, currentLy + SimulatedLeftOffsetY,
                Profile.LeftStick.MaxDriftThreshold,
                out double leftX, out double leftY, out double lDz, out double leftVar);

            AnalyzeRingAndCurrent(_rightRing, _rightCount, _rightSumX, _rightSumY, _rightLock,
                currentRx + SimulatedRightOffsetX, currentRy + SimulatedRightOffsetY,
                Profile.RightStick.MaxDriftThreshold,
                out double rightX, out double rightY, out double rDz, out double rightVar);

            Profile.LeftStick.CenterOffsetX = Math.Round(leftX, 4);
            Profile.LeftStick.CenterOffsetY = Math.Round(leftY, 4);
            Profile.LeftStick.DeadzoneRadius = lDz;

            Profile.RightStick.CenterOffsetX = Math.Round(rightX, 4);
            Profile.RightStick.CenterOffsetY = Math.Round(rightY, 4);
            Profile.RightStick.DeadzoneRadius = rDz;

            LeftMetrics.CenterOffsetX = Profile.LeftStick.CenterOffsetX;
            LeftMetrics.CenterOffsetY = Profile.LeftStick.CenterOffsetY;
            LeftMetrics.RestingNoiseVariance = leftVar;

            RightMetrics.CenterOffsetX = Profile.RightStick.CenterOffsetX;
            RightMetrics.CenterOffsetY = Profile.RightStick.CenterOffsetY;
            RightMetrics.RestingNoiseVariance = rightVar;

            _filteredLeftX = 0;
            _filteredLeftY = 0;
            _filteredRightX = 0;
            _filteredRightY = 0;

            return (Profile.LeftStick.CenterOffsetX, Profile.LeftStick.CenterOffsetY, Profile.LeftStick.DeadzoneRadius,
                    Profile.RightStick.CenterOffsetX, Profile.RightStick.CenterOffsetY, Profile.RightStick.DeadzoneRadius);
        }

        private static void AnalyzeRingAndCurrent(
            (double x, double y)[] ring,
            int count,
            double sumX,
            double sumY,
            object lockObj,
            double currentX,
            double currentY,
            double maxDriftThreshold,
            out double centerX,
            out double centerY,
            out double recommendedDeadzone,
            out double variance)
        {
            double clampedCurX = Math.Clamp(currentX, -1.0, 1.0);
            double clampedCurY = Math.Clamp(currentY, -1.0, 1.0);
            double meanX = clampedCurX;
            double meanY = clampedCurY;
            double maxRadialDeviation = 0;
            variance = 0;

            lock (lockObj)
            {
                if (count > 5)
                {
                    meanX = sumX / count;
                    meanY = sumY / count;
                    double varSum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        double dx = ring[i].x - meanX;
                        double dy = ring[i].y - meanY;
                        double distSq = dx * dx + dy * dy;
                        varSum += distSq;
                        double dist = Math.Sqrt(distSq);
                        if (dist > maxRadialDeviation)
                            maxRadialDeviation = dist;
                    }
                    variance = varSum / count;
                }
            }

            double instantResidual = Math.Sqrt((clampedCurX - meanX) * (clampedCurX - meanX) + (clampedCurY - meanY) * (clampedCurY - meanY));
            if (instantResidual > maxRadialDeviation)
                maxRadialDeviation = instantResidual;

            double limit = Math.Clamp(maxDriftThreshold, 0.15, 0.45);
            double centerMag = Math.Sqrt(meanX * meanX + meanY * meanY);
            if (centerMag > limit && centerMag > 0)
            {
                double scale = limit / centerMag;
                meanX *= scale;
                meanY *= scale;
                centerMag = limit;
            }

            centerX = meanX;
            centerY = meanY;

            double stdDev = Math.Sqrt(variance);
            double hysteresisMargin = 0.03 + (centerMag * 0.10);
            double rawDz = maxRadialDeviation + (2.0 * stdDev) + hysteresisMargin;
            recommendedDeadzone = Math.Clamp(Math.Round(Math.Max(0.04, rawDz), 2), 0.04, 0.25);
        }

        // ##== Drift Processing Pipeline ==##
        public void Process(ControllerState state, out double outLeftX, out double outLeftY, out double outRightX, out double outRightY)
        {
            double rxLeftX = Math.Clamp(state.LeftThumbX + SimulatedLeftOffsetX, -1.0, 1.0);
            double rxLeftY = Math.Clamp(state.LeftThumbY + SimulatedLeftOffsetY, -1.0, 1.0);
            double rxRightX = Math.Clamp(state.RightThumbX + SimulatedRightOffsetX, -1.0, 1.0);
            double rxRightY = Math.Clamp(state.RightThumbY + SimulatedRightOffsetY, -1.0, 1.0);

            UpdateHistoryRing(_leftRing, ref _leftHead, ref _leftCount, ref _leftSumX, ref _leftSumY, _leftLock, rxLeftX, rxLeftY);
            UpdateHistoryRing(_rightRing, ref _rightHead, ref _rightCount, ref _rightSumX, ref _rightSumY, _rightLock, rxRightX, rxRightY);

            bool noInput = state.Buttons == 0 && state.LeftTrigger < 0.1 && state.RightTrigger < 0.1;

            if (Profile.LeftStick.AutoCalibrate && noInput)
            {
                if (IsStationaryRing(_leftRing, _leftCount, _leftSumX, _leftSumY, _leftLock, Profile.LeftStick.MaxDriftThreshold, out double cx, out double cy, out double noise))
                {
                    LeftMetrics.RestingNoiseVariance = noise;
                    double driftFromCalibrated = Math.Sqrt(
                        (cx - Profile.LeftStick.CenterOffsetX) * (cx - Profile.LeftStick.CenterOffsetX) +
                        (cy - Profile.LeftStick.CenterOffsetY) * (cy - Profile.LeftStick.CenterOffsetY));
                    double allowBand = Math.Max(0.08, Profile.LeftStick.DeadzoneRadius * 1.25);
                    if (driftFromCalibrated <= allowBand || (Profile.LeftStick.CenterOffsetX == 0 && Profile.LeftStick.CenterOffsetY == 0))
                    {
                        Profile.LeftStick.CenterOffsetX = cx;
                        Profile.LeftStick.CenterOffsetY = cy;
                        LeftMetrics.CenterOffsetX = cx;
                        LeftMetrics.CenterOffsetY = cy;
                    }
                }
            }

            if (Profile.RightStick.AutoCalibrate && noInput)
            {
                if (IsStationaryRing(_rightRing, _rightCount, _rightSumX, _rightSumY, _rightLock, Profile.RightStick.MaxDriftThreshold, out double cx, out double cy, out double noise))
                {
                    RightMetrics.RestingNoiseVariance = noise;
                    double driftFromCalibrated = Math.Sqrt(
                        (cx - Profile.RightStick.CenterOffsetX) * (cx - Profile.RightStick.CenterOffsetX) +
                        (cy - Profile.RightStick.CenterOffsetY) * (cy - Profile.RightStick.CenterOffsetY));
                    double allowBand = Math.Max(0.08, Profile.RightStick.DeadzoneRadius * 1.25);
                    if (driftFromCalibrated <= allowBand || (Profile.RightStick.CenterOffsetX == 0 && Profile.RightStick.CenterOffsetY == 0))
                    {
                        Profile.RightStick.CenterOffsetX = cx;
                        Profile.RightStick.CenterOffsetY = cy;
                        RightMetrics.CenterOffsetX = cx;
                        RightMetrics.CenterOffsetY = cy;
                    }
                }
            }

            UpdateCircularityMetrics(rxLeftX, rxLeftY, _leftMaxRadiusBuckets, ref _leftBucketCount, ref _leftBucketSumErr, LeftMetrics);
            UpdateCircularityMetrics(rxRightX, rxRightY, _rightMaxRadiusBuckets, ref _rightBucketCount, ref _rightBucketSumErr, RightMetrics);

            ApplyCorrection(rxLeftX, rxLeftY, Profile.LeftStick, out double rawOutLx, out double rawOutLy);
            ApplyCorrection(rxRightX, rxRightY, Profile.RightStick, out double rawOutRx, out double rawOutRy);

            ApplyAdaptiveSmoothing(rawOutLx, rawOutLy, ref _filteredLeftX, ref _filteredLeftY, out outLeftX, out outLeftY);
            ApplyAdaptiveSmoothing(rawOutRx, rawOutRy, ref _filteredRightX, ref _filteredRightY, out outRightX, out outRightY);
        }

        private static void ApplyAdaptiveSmoothing(double targetX, double targetY, ref double stateX, ref double stateY, out double outX, out double outY)
        {
            if (targetX == 0 && targetY == 0)
            {
                stateX = 0;
                stateY = 0;
                outX = 0;
                outY = 0;
                return;
            }

            double dx = targetX - stateX;
            double dy = targetY - stateY;
            double delta = Math.Sqrt(dx * dx + dy * dy);

            if (delta >= 0.04 || (stateX == 0 && stateY == 0))
            {
                stateX = targetX;
                stateY = targetY;
            }
            else
            {
                double alpha = Math.Clamp(0.35 + (delta * 16.0), 0.35, 1.0);
                stateX += dx * alpha;
                stateY += dy * alpha;
            }

            outX = Math.Clamp(stateX, -1.0, 1.0);
            outY = Math.Clamp(stateY, -1.0, 1.0);
        }

        private void UpdateHistoryRing((double x, double y)[] ring, ref int head, ref int count, ref double sumX, ref double sumY, object lockObj, double x, double y)
        {
            lock (lockObj)
            {
                if (count >= _windowSize)
                {
                    sumX -= ring[head].x;
                    sumY -= ring[head].y;
                }
                else
                {
                    count++;
                }

                ring[head] = (x, y);
                sumX += x;
                sumY += y;

                head = (head + 1) % _windowSize;
            }
        }

        private bool IsStationaryRing((double x, double y)[] ring, int count, double sumX, double sumY, object lockObj, double maxDriftThreshold, out double centerX, out double centerY, out double noiseVariance)
        {
            centerX = 0;
            centerY = 0;
            noiseVariance = 0;

            if (count < _windowSize) return false;

            double meanX;
            double meanY;
            double varianceSum = 0;

            lock (lockObj)
            {
                meanX = sumX / count;
                meanY = sumY / count;

                for (int i = 0; i < count; i++)
                {
                    double dx = ring[i].x - meanX;
                    double dy = ring[i].y - meanY;
                    varianceSum += dx * dx + dy * dy;
                }
            }

            noiseVariance = varianceSum / count;

            if (noiseVariance < 0.0001)
            {
                double dist = Math.Sqrt(meanX * meanX + meanY * meanY);
                if (dist <= maxDriftThreshold)
                {
                    centerX = meanX;
                    centerY = meanY;
                    return true;
                }
            }

            return false;
        }

        private void UpdateCircularityMetrics(double x, double y, double[] buckets, ref int bucketCount, ref double bucketSumErr, StickDriftMetrics metrics)
        {
            double r = Math.Sqrt(x * x + y * y);
            if (r < 0.70) return;

            double angleRad = Math.Atan2(y, x);
            if (angleRad < 0) angleRad += 2 * Math.PI;
            int deg = Math.Clamp((int)(angleRad * 180.0 / Math.PI), 0, 359);

            double instantError = Math.Abs(r - 1.0) * 100.0;
            metrics.LiveCircularityError = Math.Round(instantError, 1);

            if (r > buckets[deg])
            {
                if (buckets[deg] > 0)
                {
                    bucketSumErr -= Math.Abs(buckets[deg] - 1.0) * 100.0;
                }
                else
                {
                    bucketCount++;
                }

                buckets[deg] = r;
                bucketSumErr += instantError;

                if (bucketCount > 0)
                {
                    double avgErr = bucketSumErr / bucketCount;
                    metrics.AverageCircularityError = Math.Round(avgErr, 1);
                    if (avgErr < metrics.MinCircularityError) metrics.MinCircularityError = Math.Round(avgErr, 1);
                    if (avgErr > metrics.MaxCircularityError) metrics.MaxCircularityError = Math.Round(avgErr, 1);
                }
            }
        }

        private static void ApplyCorrection(double inX, double inY, AxisSettings settings, out double outX, out double outY)
        {
            double rawX = inX - settings.CenterOffsetX;
            double rawY = inY - settings.CenterOffsetY;

            double spanX = rawX >= 0
                ? Math.Max(0.25, 1.0 - settings.CenterOffsetX)
                : Math.Max(0.25, 1.0 + settings.CenterOffsetX);
            double spanY = rawY >= 0
                ? Math.Max(0.25, 1.0 - settings.CenterOffsetY)
                : Math.Max(0.25, 1.0 + settings.CenterOffsetY);

            double x = Math.Clamp(rawX / spanX, -1.0, 1.0);
            double y = Math.Clamp(rawY / spanY, -1.0, 1.0);

            double axialX = Math.Clamp(settings.AxialDeadzoneX, 0.0, 0.99);
            double axialY = Math.Clamp(settings.AxialDeadzoneY, 0.0, 0.99);

            if (Math.Abs(x) < axialX) x = 0;
            else x = (Math.Abs(x) - axialX) / (1.0 - axialX) * Math.Sign(x);

            if (Math.Abs(y) < axialY) y = 0;
            else y = (Math.Abs(y) - axialY) / (1.0 - axialY) * Math.Sign(y);

            double r = Math.Sqrt(x * x + y * y);
            double innerDz = Math.Clamp(settings.DeadzoneRadius, 0.0, 0.99);
            double outerDz = Math.Clamp(settings.OuterDeadzone, innerDz + 0.001, 1.0);

            if (r <= innerDz || r == 0)
            {
                outX = 0;
                outY = 0;
                return;
            }

            double rClamped = Math.Min(r, outerDz);
            double range = outerDz - innerDz;
            double scale = (rClamped - innerDz) / range;

            if (settings.AntiDeadzone > 0 && scale > 0)
                scale = settings.AntiDeadzone + (1.0 - settings.AntiDeadzone) * scale;

            scale = settings.CurveType switch
            {
                ResponseCurveType.Aggressive => Math.Pow(scale, 0.7),
                ResponseCurveType.Smooth => scale * scale * (3 - 2 * scale),
                _ => scale
            };

            scale = Math.Clamp(scale * settings.Sensitivity, 0.0, 1.0);

            outX = Math.Clamp((x / r) * scale, -1.0, 1.0);
            outY = Math.Clamp((y / r) * scale, -1.0, 1.0);
        }
    }
}
