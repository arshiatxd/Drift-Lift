using System;
using System.Collections.Generic;
namespace DriftLock.Models
{
    public enum ResponseCurveType
    {
        Linear,
        Aggressive,
        Smooth,
        Custom
    }
    public class AxisSettings
    {
        public double CenterOffsetX { get; set; }
        public double CenterOffsetY { get; set; }
        public double DeadzoneRadius { get; set; } = 0.05; 
        public double OuterDeadzone { get; set; } = 0.99; 
        public double AntiDeadzone { get; set; } = 0.0; 
        public double AxialDeadzoneX { get; set; } = 0.0; 
        public double AxialDeadzoneY { get; set; } = 0.0; 
        public bool AutoCalibrate { get; set; } = true;
        public double Sensitivity { get; set; } = 1.0;
        public double MaxDriftThreshold { get; set; } = 0.3; 
        public ResponseCurveType CurveType { get; set; } = ResponseCurveType.Linear;
    }
    public class TriggerSettings
    {
        public double MinThreshold { get; set; } = 0.0;
        public double MaxThreshold { get; set; } = 1.0;
        public double Deadzone { get; set; } = 0.02;
    }
    public class CalibrationProfile
    {
        public string ProfileId { get; set; } = Guid.NewGuid().ToString();
        public string ProfileName { get; set; } = "Default Profile";
        public string ControllerId { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public AxisSettings LeftStick { get; set; } = new AxisSettings();
        public AxisSettings RightStick { get; set; } = new AxisSettings();
        public TriggerSettings LeftTrigger { get; set; } = new TriggerSettings();
        public TriggerSettings RightTrigger { get; set; } = new TriggerSettings();
        public Dictionary<ushort, ushort> ButtonMap { get; set; } = new Dictionary<ushort, ushort>();
    }
}
