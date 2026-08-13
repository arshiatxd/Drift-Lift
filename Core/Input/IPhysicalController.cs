using System;
using DriftLift.Models;

namespace DriftLift.Core.Input
{

    public interface IPhysicalController : IDisposable
    {
        string DeviceId { get; }
        string DeviceName { get; }
        ControllerType Type { get; }
        bool IsConnected { get; }
        ControllerState GetCurrentState();
        void SetVibration(double leftMotor, double rightMotor);
        void SetLedColor(byte r, byte g, byte b);
        (string Text, double Percentage, bool IsWireless) GetBatteryInfo();
    }
}
