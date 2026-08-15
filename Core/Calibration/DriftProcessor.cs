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
            double leftX = currentLx;
            double leftY = currentLy;
            double rightX = currentRx;
            double rightY = currentRy;

            lock (_leftLock)
            {
                if (_leftCount > 10)
                {
                    leftX = _leftSumX / _leftCount;
                    leftY = _leftSumY / _leftCount;
                }
            }

            lock (_rightLock)
            {
                if (_rightCount > 10)
                {
                    rightX = _rightSumX / _rightCount;
                    rightY = _rightSumY / _rightCount;
                }
            }

            double leftResidual = Math.Sqrt((currentLx - leftX) * (currentLx - leftX) + (currentLy - leftY) * (currentLy - leftY));
            double rightResidual = Math.Sqrt((currentRx - rightX) * (currentRx - rightX) + (currentRy - rightY) * (currentRy - rightY));

            double lDz = Math.Clamp(Math.Round(Math.Max(0.04, leftResidual + 0.03), 2), 0.04, 0.25);
            double rDz = Math.Clamp(Math.Round(Math.Max(0.04, rightResidual + 0.03), 2), 0.04, 0.25);

            Profile.LeftStick.CenterOffsetX = Math.Round(leftX, 4);
            Profile.LeftStick.CenterOffsetY = Math.Round(leftY, 4);
            Profile.LeftStick.DeadzoneRadius = lDz;

            Profile.RightStick.CenterOffsetX = Math.Round(rightX, 4);
            Profile.RightStick.CenterOffsetY = Math.Round(rightY, 4);
            Profile.RightStick.DeadzoneRadius = rDz;

            LeftMetrics.CenterOffsetX = Profile.LeftStick.CenterOffsetX;
            LeftMetrics.CenterOffsetY = Profile.LeftStick.CenterOffsetY;
            RightMetrics.CenterOffsetX = Profile.RightStick.CenterOffsetX;
            RightMetrics.CenterOffsetY = Profile.RightStick.CenterOffsetY;

            return (Profile.LeftStick.CenterOffsetX, Profile.LeftStick.CenterOffsetY, Profile.LeftStick.DeadzoneRadius,
                    Profile.RightStick.CenterOffsetX, Profile.RightStick.CenterOffsetY, Profile.RightStick.DeadzoneRadius);
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
                    Profile.LeftStick.CenterOffsetX = cx;
                    Profile.LeftStick.CenterOffsetY = cy;
                    LeftMetrics.CenterOffsetX = cx;
                    LeftMetrics.CenterOffsetY = cy;
                    LeftMetrics.RestingNoiseVariance = noise;
                }
            }

            if (Profile.RightStick.AutoCalibrate && noInput)
            {
                if (IsStationaryRing(_rightRing, _rightCount, _rightSumX, _rightSumY, _rightLock, Profile.RightStick.MaxDriftThreshold, out double cx, out double cy, out double noise))
                {
                    Profile.RightStick.CenterOffsetX = cx;
                    Profile.RightStick.CenterOffsetY = cy;
                    RightMetrics.CenterOffsetX = cx;
                    RightMetrics.CenterOffsetY = cy;
                    RightMetrics.RestingNoiseVariance = noise;
                }
            }

            UpdateCircularityMetrics(rxLeftX, rxLeftY, _leftMaxRadiusBuckets, ref _leftBucketCount, ref _leftBucketSumErr, LeftMetrics);
            UpdateCircularityMetrics(rxRightX, rxRightY, _rightMaxRadiusBuckets, ref _rightBucketCount, ref _rightBucketSumErr, RightMetrics);

            ApplyCorrection(rxLeftX, rxLeftY, Profile.LeftStick, out outLeftX, out outLeftY);
            ApplyCorrection(rxRightX, rxRightY, Profile.RightStick, out outRightX, out outRightY);
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
            double x = inX - settings.CenterOffsetX;
            double y = inY - settings.CenterOffsetY;

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
