using System;

namespace DriftLift.Models
{
    public enum ControllerType
    {
        Generic,
        Xbox360,
        Xbox,
        DualShock4,
        DualSense
    }

    public class ControllerState
    {
        public double LeftThumbX { get; set; }
        public double LeftThumbY { get; set; }
        public double RightThumbX { get; set; }
        public double RightThumbY { get; set; }
        public double LeftTrigger { get; set; }
        public double RightTrigger { get; set; }
        public uint Buttons { get; set; }
        public bool IsConnected { get; set; }
        public string DeviceName { get; set; } = "Generic Controller";
        public ControllerType Type { get; set; } = ControllerType.Generic;
    }
}
