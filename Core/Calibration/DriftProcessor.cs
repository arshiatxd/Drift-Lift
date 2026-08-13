using System;
using System.Collections.Generic;
using DriftLift.Models;
namespace DriftLift.Core.Calibration
{
    public class DriftProcessor
    {
        // ##== Fields & Properties ==##
        private readonly int _windowSize;
        private readonly Queue<(double x, double y)> _leftHistory = new();
        private readonly Queue<(double x, double y)> _rightHistory = new();
        private readonly object _leftLock = new();
        private readonly object _rightLock = new();
        private readonly double[] _leftMaxRadiusBuckets = new double[360];
        private readonly double[] _rightMaxRadiusBuckets = new double[360];
        private int _leftBucketCount = 0;
        private int _rightBucketCount = 0;
        public CalibrationProfile Profile { get; set; } = new CalibrationProfile();
        public StickDriftMetrics LeftMetrics { get; } = new StickDriftMetrics();
        public StickDriftMetrics RightMetrics { get; } = new StickDriftMetrics();
        public double SimulatedLeftOffsetX { get; set; }
        public double SimulatedLeftOffsetY { get; set; }
        public double SimulatedRightOffsetX { get; set; }
        public double SimulatedRightOffsetY { get; set; }
        public DriftProcessor(int historyWindowSamples = 120)
        {
            _windowSize = historyWindowSamples;
            ResetMetrics();
        }
        public void ResetMetrics()
        {
            Array.Clear(_leftMaxRadiusBuckets, 0, 360);
            Array.Clear(_rightMaxRadiusBuckets, 0, 360);
            _leftBucketCount = 0;
            _rightBucketCount = 0;
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
            double leftX = 0, leftY = 0, leftMax = 0;
            double rightX = 0, rightY = 0, rightMax = 0;
            lock (_leftLock)
            {
                _leftHistory.Clear();
                leftX = currentLx;
                leftY = currentLy;
                leftMax = Math.Sqrt(currentLx * currentLx + currentLy * currentLy);
            }
            lock (_rightLock)
            {
                _rightHistory.Clear();
                rightX = currentRx;
                rightY = currentRy;
                rightMax = Math.Sqrt(currentRx * currentRx + currentRy * currentRy);
            }
            Profile.LeftStick.CenterOffsetX = Math.Round(leftX, 3);
            Profile.LeftStick.CenterOffsetY = Math.Round(leftY, 3);
            Profile.LeftStick.DeadzoneRadius = Math.Clamp(Math.Round(leftMax + 0.02, 2), 0.03, 0.25);
            Profile.RightStick.CenterOffsetX = Math.Round(rightX, 3);
            Profile.RightStick.CenterOffsetY = Math.Round(rightY, 3);
            Profile.RightStick.DeadzoneRadius = Math.Clamp(Math.Round(rightMax + 0.02, 2), 0.03, 0.25);
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
            UpdateHistory(_leftHistory, _leftLock, rxLeftX, rxLeftY);
            UpdateHistory(_rightHistory, _rightLock, rxRightX, rxRightY);
            if (Profile.LeftStick.AutoCalibrate && state.Buttons == 0 && state.LeftTrigger < 0.1 && state.RightTrigger < 0.1)
            {
                if (IsStationary(_leftHistory, _leftLock, Profile.LeftStick.MaxDriftThreshold, out double cx, out double cy, out double noise))
                {
                    Profile.LeftStick.CenterOffsetX = cx;
                    Profile.LeftStick.CenterOffsetY = cy;
                    LeftMetrics.CenterOffsetX = cx;
                    LeftMetrics.CenterOffsetY = cy;
                    LeftMetrics.RestingNoiseVariance = noise;
                }
            }
            if (Profile.RightStick.AutoCalibrate && state.Buttons == 0 && state.LeftTrigger < 0.1 && state.RightTrigger < 0.1)
            {
                if (IsStationary(_rightHistory, _rightLock, Profile.RightStick.MaxDriftThreshold, out double cx, out double cy, out double noise))
                {
                    Profile.RightStick.CenterOffsetX = cx;
                    Profile.RightStick.CenterOffsetY = cy;
                    RightMetrics.CenterOffsetX = cx;
                    RightMetrics.CenterOffsetY = cy;
                    RightMetrics.RestingNoiseVariance = noise;
                }
            }
            UpdateCircularityMetrics(rxLeftX, rxLeftY, _leftMaxRadiusBuckets, ref _leftBucketCount, LeftMetrics);
            UpdateCircularityMetrics(rxRightX, rxRightY, _rightMaxRadiusBuckets, ref _rightBucketCount, RightMetrics);
            ApplyCorrection(rxLeftX, rxLeftY, Profile.LeftStick, out outLeftX, out outLeftY);
            ApplyCorrection(rxRightX, rxRightY, Profile.RightStick, out outRightX, out outRightY);
        }
        private void UpdateHistory(Queue<(double x, double y)> history, object lockObj, double x, double y)
        {
            lock (lockObj)
            {
                history.Enqueue((x, y));
                if (history.Count > _windowSize)
                {
                    history.Dequeue();
                }
            }
        }
        private bool IsStationary(Queue<(double x, double y)> history, object lockObj, double maxDriftThreshold, out double centerX, out double centerY, out double noiseVariance)
        {
            centerX = 0;
            centerY = 0;
            noiseVariance = 0;
            (double x, double y)[] samples;
            lock (lockObj)
            {
                if (history.Count < _windowSize) return false;
                samples = history.ToArray();
            }
            double sumX = 0, sumY = 0;
            foreach (var p in samples)
            {
                sumX += p.x;
                sumY += p.y;
            }
            double meanX = sumX / samples.Length;
            double meanY = sumY / samples.Length;
            double varianceSum = 0;
            foreach (var p in samples)
            {
                varianceSum += (p.x - meanX) * (p.x - meanX) + (p.y - meanY) * (p.y - meanY);
            }
            noiseVariance = varianceSum / samples.Length;
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
        private void UpdateCircularityMetrics(double x, double y, double[] buckets, ref int bucketCount, StickDriftMetrics metrics)
        {
            double r = Math.Sqrt(x * x + y * y);
            if (r >= 0.70)
            {
                double angleRad = Math.Atan2(y, x);
                if (angleRad < 0) angleRad += 2 * Math.PI;
                int deg = Math.Clamp((int)(angleRad * 180.0 / Math.PI), 0, 359);
                if (buckets[deg] == 0) bucketCount++;
                if (r > buckets[deg]) buckets[deg] = r;
                double instantError = Math.Abs(r - 1.0) * 100.0;
                metrics.LiveCircularityError = Math.Round(instantError, 1);
                if (bucketCount > 0)
                {
                    double sumErr = 0;
                    int validBuckets = 0;
                    for (int i = 0; i < 360; i++)
                    {
                        if (buckets[i] > 0)
                        {
                            sumErr += Math.Abs(buckets[i] - 1.0) * 100.0;
                            validBuckets++;
                        }
                    }
                    if (validBuckets > 0)
                    {
                        double avgErr = sumErr / validBuckets;
                        metrics.AverageCircularityError = Math.Round(avgErr, 1);
                        if (avgErr < metrics.MinCircularityError) metrics.MinCircularityError = Math.Round(avgErr, 1);
                        if (avgErr > metrics.MaxCircularityError) metrics.MaxCircularityError = Math.Round(avgErr, 1);
                    }
                }
            }
        }
        private void ApplyCorrection(double inX, double inY, AxisSettings settings, out double outX, out double outY)
        {
            double x = inX - settings.CenterOffsetX;
            double y = inY - settings.CenterOffsetY;
            if (Math.Abs(x) < settings.AxialDeadzoneX) x = 0;
            else x = (Math.Abs(x) - settings.AxialDeadzoneX) / (1.0 - settings.AxialDeadzoneX) * Math.Sign(x);
            if (Math.Abs(y) < settings.AxialDeadzoneY) y = 0;
            else y = (Math.Abs(y) - settings.AxialDeadzoneY) / (1.0 - settings.AxialDeadzoneY) * Math.Sign(y);
            double r = Math.Sqrt(x * x + y * y);
            if (r <= settings.DeadzoneRadius || r == 0)
            {
                outX = 0;
                outY = 0;
                return;
            }
            if (r >= settings.OuterDeadzone)
            {
                r = settings.OuterDeadzone;
            }
            double scale = (r - settings.DeadzoneRadius) / (settings.OuterDeadzone - settings.DeadzoneRadius);
            if (settings.AntiDeadzone > 0 && scale > 0)
            {
                scale = settings.AntiDeadzone + (1.0 - settings.AntiDeadzone) * scale;
            }
            scale = settings.CurveType switch
            {
                ResponseCurveType.Aggressive => Math.Pow(scale, 0.7),
                ResponseCurveType.Smooth => scale * scale * (3 - 2 * scale),
                _ => scale
            };
            scale *= settings.Sensitivity;
            outX = Math.Clamp((x / r) * scale, -1.0, 1.0);
            outY = Math.Clamp((y / r) * scale, -1.0, 1.0);
        }
    }
}
