using System;
using System.Threading;
using Vortice.XInput;
using DriftLift.Models;

namespace DriftLift.Core.Input
{
    public class XboxController : IPhysicalController
    {
        // ##== Fields & Identity ==##
        private readonly uint _userIndex;
        private volatile bool _isConnected = true;
        private long _lastCheckTicks;
        private long _lastBatteryCheckTicks;
        private (string Text, double Percentage, bool IsWireless) _cachedBatteryInfo = ("USB Power (Cable Connected)", 1.0, false);
        private readonly ControllerState[] _stateBuffers = { new(), new() };
        private int _stateBufferIndex;

        public string DeviceId => $"XINPUT_{_userIndex}";
        public string DeviceName { get; }
        public ControllerType Type { get; }
        public bool IsConnected
        {
            get
            {
                long now = Environment.TickCount64;
                if (now - Volatile.Read(ref _lastCheckTicks) > 50)
                {
                    _isConnected = XInput.GetState(_userIndex, out _);
                    Volatile.Write(ref _lastCheckTicks, now);
                }
                return _isConnected;
            }
        }

        public XboxController(uint userIndex, ControllerType type = ControllerType.Xbox, string? customName = null)
        {
            _userIndex = userIndex;
            Type = type;
            if (!string.IsNullOrEmpty(customName))
            {
                DeviceName = customName;
            }
            else
            {
                DeviceName = type == ControllerType.Xbox360 
                    ? $"Xbox 360 Controller ({userIndex + 1})" 
                    : $"Xbox Wireless Controller ({userIndex + 1})";
            }

            for (int i = 0; i < _stateBuffers.Length; i++)
            {
                _stateBuffers[i].DeviceName = DeviceName;
                _stateBuffers[i].Type = Type;
            }
        }

        // ##== Input Reading ==##
        public ControllerState GetCurrentState()
        {
            _stateBufferIndex ^= 1;
            var state = _stateBuffers[_stateBufferIndex];
            state.DeviceName = DeviceName;
            state.Type = Type;

            bool connected = XInput.GetState(_userIndex, out var xState);
            _isConnected = connected;
            Volatile.Write(ref _lastCheckTicks, Environment.TickCount64);

            if (connected)
            {
                state.IsConnected = true;
                var gamepad = xState.Gamepad;
                state.LeftThumbX = NormalizeAxis(gamepad.LeftThumbX);
                state.LeftThumbY = NormalizeAxis(gamepad.LeftThumbY);
                state.RightThumbX = NormalizeAxis(gamepad.RightThumbX);
                state.RightThumbY = NormalizeAxis(gamepad.RightThumbY);
                state.LeftTrigger = gamepad.LeftTrigger / 255.0;
                state.RightTrigger = gamepad.RightTrigger / 255.0;

                uint mask = 0;
                var btns = gamepad.Buttons;
                if ((btns & GamepadButtons.DPadUp) != 0) mask |= 0x0001;
                if ((btns & GamepadButtons.DPadDown) != 0) mask |= 0x0002;
                if ((btns & GamepadButtons.DPadLeft) != 0) mask |= 0x0004;
                if ((btns & GamepadButtons.DPadRight) != 0) mask |= 0x0008;
                if ((btns & GamepadButtons.Start) != 0) mask |= 0x0010;
                if ((btns & GamepadButtons.Back) != 0) mask |= 0x0020;
                if ((btns & GamepadButtons.LeftThumb) != 0) mask |= 0x0040;
                if ((btns & GamepadButtons.RightThumb) != 0) mask |= 0x0080;
                if ((btns & GamepadButtons.LeftShoulder) != 0) mask |= 0x0100;
                if ((btns & GamepadButtons.RightShoulder) != 0) mask |= 0x0200;
                if ((btns & GamepadButtons.A) != 0) mask |= 0x1000;
                if ((btns & GamepadButtons.B) != 0) mask |= 0x2000;
                if ((btns & GamepadButtons.X) != 0) mask |= 0x4000;
                if ((btns & GamepadButtons.Y) != 0) mask |= 0x8000;
                state.Buttons = mask;
            }
            else
            {
                state.IsConnected = false;
            }
            return state;
        }

        private static double NormalizeAxis(short val)
        {
            if (val < 0) return val / 32768.0;
            return val / 32767.0;
        }

        // ##== Vibration & Output ==##
        public void SetVibration(double leftMotor, double rightMotor)
        {
            ushort left = (ushort)(Math.Clamp(leftMotor, 0.0, 1.0) * 65535);
            ushort right = (ushort)(Math.Clamp(rightMotor, 0.0, 1.0) * 65535);
            XInput.SetVibration(_userIndex, new Vibration(left, right));
        }

        public void SetLedColor(byte r, byte g, byte b)
        {
        }

        // ##== Battery Info ==##
        public (string Text, double Percentage, bool IsWireless) GetBatteryInfo()
        {
            long now = Environment.TickCount64;
            if (_lastBatteryCheckTicks != 0 && (now - _lastBatteryCheckTicks) < 5000)
            {
                return _cachedBatteryInfo;
            }

            _lastBatteryCheckTicks = now;
            if (XInput.GetBatteryInformation(_userIndex, BatteryDeviceType.Gamepad, out var batteryInfo))
            {
                if (batteryInfo.BatteryType == BatteryType.Disconnected || batteryInfo.BatteryType == BatteryType.Wired || batteryInfo.BatteryType == BatteryType.Unknown)
                {
                    _cachedBatteryInfo = ("USB Power (Cable Connected)", 1.0, false);
                    return _cachedBatteryInfo;
                }
                double pct = batteryInfo.BatteryLevel switch
                {
                    BatteryLevel.Empty => 0.10,
                    BatteryLevel.Low => 0.35,
                    BatteryLevel.Medium => 0.70,
                    BatteryLevel.Full => 1.0,
                    _ => 1.0
                };
                int pInt = (int)(pct * 100);
                _cachedBatteryInfo = ($"Battery: {pInt}%", pct, true);
                return _cachedBatteryInfo;
            }
            _cachedBatteryInfo = ("USB Power (Cable Connected)", 1.0, false);
            return _cachedBatteryInfo;
        }

        public void Dispose() { }
    }
}
