using System;
using HidLibrary;
using DriftLift.Models;

namespace DriftLift.Core.Input
{
    public class PlayStationController : IPhysicalController
    {
        // ##== Fields & Identity ==##
        private readonly HidDevice _device;
        private readonly bool _isBluetooth;
        private int _cachedBatteryLevel = -1;
        private long _lastBatteryCheckTicks;

        public HidDevice RawDevice => _device;
        public string DeviceId => _device.DevicePath;
        public string InstanceId => DeviceEnumerator.ExtractInstanceId(_device.DevicePath);
        public string DeviceName { get; }
        public ControllerType Type { get; }
        public bool IsConnected => _device.IsConnected;
        public int VendorId => _device.Attributes.VendorId;
        public int ProductId => _device.Attributes.ProductId;

        public PlayStationController(HidDevice device)
        {
            _device = device;
            _device.OpenDevice();

            string path = device.DevicePath?.ToLowerInvariant() ?? string.Empty;
            _isBluetooth = path.Contains("bluetooth") || path.Contains("{00001124") || (!path.Contains("&mi_") && !path.Contains("usb"));

            string desc = device.Description ?? string.Empty;

            if (desc.Contains("DualSense", StringComparison.OrdinalIgnoreCase) || device.Attributes.ProductId == 0x0CE6)
            {
                Type = ControllerType.DualSense;
                DeviceName = "PS5 DualSense Controller";
            }
            else
            {
                Type = ControllerType.DualShock4;
                DeviceName = "PS4 DualShock Controller";
            }
        }

        // ##== Input Reading ==##
        public ControllerState GetCurrentState()
        {
            var state = new ControllerState
            {
                DeviceName = DeviceName,
                Type = Type
            };

            if (!_device.IsConnected)
            {
                state.IsConnected = false;
                return state;
            }

            var report = _device.ReadReport();
            if (report.ReadStatus != HidDeviceData.ReadStatus.Success)
                return state;

            byte[] data = report.Data;
            if (data == null || data.Length < 8)
                return state;

            int offset = _isBluetooth ? 2 : (data[0] == 0x01 ? 1 : 0);

            if (data.Length < offset + 9)
                return state;

            state.IsConnected = true;
            state.LeftThumbX = (data[offset + 0] - 128) / 128.0;
            state.LeftThumbY = (128 - data[offset + 1]) / 128.0;
            state.RightThumbX = (data[offset + 2] - 128) / 128.0;
            state.RightThumbY = (128 - data[offset + 3]) / 128.0;

            byte btn1 = data[offset + 4];
            byte btn2 = data[offset + 5];
            ushort mask = 0;

            byte dpad = (byte)(btn1 & 0x0F);
            switch (dpad)
            {
                case 0: mask |= 0x0001; break;
                case 1: mask |= 0x0001 | 0x0008; break;
                case 2: mask |= 0x0008; break;
                case 3: mask |= 0x0002 | 0x0008; break;
                case 4: mask |= 0x0002; break;
                case 5: mask |= 0x0002 | 0x0004; break;
                case 6: mask |= 0x0004; break;
                case 7: mask |= 0x0001 | 0x0004; break;
            }

            if ((btn1 & 0x20) != 0) mask |= 0x1000;
            if ((btn1 & 0x40) != 0) mask |= 0x2000;
            if ((btn1 & 0x10) != 0) mask |= 0x4000;
            if ((btn1 & 0x80) != 0) mask |= 0x8000;

            if ((btn2 & 0x01) != 0) mask |= 0x0100;
            if ((btn2 & 0x02) != 0) mask |= 0x0200;
            if ((btn2 & 0x04) != 0) mask |= 0x0400;
            if ((btn2 & 0x08) != 0) mask |= 0x0800;
            if ((btn2 & 0x10) != 0) mask |= 0x0020;
            if ((btn2 & 0x20) != 0) mask |= 0x0010;
            if ((btn2 & 0x40) != 0) mask |= 0x0040;
            if ((btn2 & 0x80) != 0) mask |= 0x0080;

            state.Buttons = mask;

            if (data.Length > offset + 8)
            {
                state.LeftTrigger = data[offset + 7] / 255.0;
                state.RightTrigger = data[offset + 8] / 255.0;
            }

            if (data.Length > offset + 6)
            {
                byte specialBytes = data[offset + 6];
                state.Touchpad = (specialBytes & 0x02) != 0;
            }

            long now = Environment.TickCount64;
            if (now - _lastBatteryCheckTicks > 500)
            {
                _lastBatteryCheckTicks = now;
                ParseBatteryReport(data, offset);
            }

            return state;
        }

        private void ParseBatteryReport(byte[] data, int offset)
        {
            if (data.Length > offset + 30)
            {
                int batIdx = (data.Length >= offset + 53 && Type == ControllerType.DualSense) ? offset + 53 : offset + 30;
                if (data.Length > batIdx)
                {
                    byte batByte = data[batIdx];
                    int rawLevel = batByte & 0x0F;
                    int level;

                    if (Type == ControllerType.DualShock4)
                        level = Math.Clamp(rawLevel * 20, 0, 100);
                    else
                        level = Math.Clamp(rawLevel * 10, 0, 100);

                    if (level > 0)
                        _cachedBatteryLevel = level;
                }
            }
            else if (data.Length > offset + 12)
            {
                byte b = data[offset + 12];
                int level = Math.Clamp((b & 0x0F) * 10, 0, 100);
                if (level > 0) _cachedBatteryLevel = level;
            }
        }

        // ##== Output: Vibration & LED ==##
        public void SetVibration(double leftMotor, double rightMotor)
        {
            if (!_device.IsConnected) return;
            byte left = (byte)(Math.Clamp(leftMotor, 0.0, 1.0) * 255);
            byte right = (byte)(Math.Clamp(rightMotor, 0.0, 1.0) * 255);

            if (Type == ControllerType.DualShock4)
            {
                try
                {
                    var hidReport = _device.CreateReport();
                    if (_device.Capabilities.InputReportByteLength > 64)
                    {
                        hidReport.ReportId = 0x11;
                        if (hidReport.Data.Length >= 10)
                        {
                            hidReport.Data[0] = 0xC0;
                            hidReport.Data[1] = 0x20;
                            hidReport.Data[2] = 0xF0;
                            hidReport.Data[3] = 0x04;
                            hidReport.Data[5] = right;
                            hidReport.Data[6] = left;
                        }
                    }
                    else
                    {
                        hidReport.ReportId = 0x05;
                        if (hidReport.Data.Length >= 6)
                        {
                            hidReport.Data[0] = 0xFF;
                            hidReport.Data[3] = right;
                            hidReport.Data[4] = left;
                        }
                    }
                    _device.WriteReport(hidReport);
                }
                catch { }
            }
        }

        public void SetLedColor(byte r, byte g, byte b)
        {
            if (!_device.IsConnected) return;

            try
            {
                var hidReport = _device.CreateReport();

                if (Type == ControllerType.DualShock4)
                {
                    if (_device.Capabilities.InputReportByteLength > 64)
                    {
                        hidReport.ReportId = 0x11;
                        if (hidReport.Data.Length >= 10)
                        {
                            hidReport.Data[0] = 0xC0;
                            hidReport.Data[1] = 0x20;
                            hidReport.Data[2] = 0xF0;
                            hidReport.Data[3] = 0x04;
                            hidReport.Data[7] = r;
                            hidReport.Data[8] = g;
                            hidReport.Data[9] = b;
                        }
                    }
                    else
                    {
                        hidReport.ReportId = 0x05;
                        if (hidReport.Data.Length >= 8)
                        {
                            hidReport.Data[0] = 0xFF;
                            hidReport.Data[5] = r;
                            hidReport.Data[6] = g;
                            hidReport.Data[7] = b;
                        }
                    }
                }
                else if (Type == ControllerType.DualSense)
                {
                    if (_device.Capabilities.InputReportByteLength > 64)
                    {
                        hidReport.ReportId = 0x31;
                        if (hidReport.Data.Length >= 11)
                        {
                            hidReport.Data[0] = 0x02;
                            hidReport.Data[1] = 0x02;
                            hidReport.Data[8] = r;
                            hidReport.Data[9] = g;
                            hidReport.Data[10] = b;
                        }
                    }
                    else
                    {
                        hidReport.ReportId = 0x02;
                        if (hidReport.Data.Length >= 11)
                        {
                            hidReport.Data[0] = 0xFF;
                            hidReport.Data[1] = 0x15;
                            hidReport.Data[8] = r;
                            hidReport.Data[9] = g;
                            hidReport.Data[10] = b;
                        }
                    }
                }

                _device.WriteReport(hidReport);
            }
            catch { }
        }

        // ##== Battery Info ==##
        public (string Text, double Percentage, bool IsWireless) GetBatteryInfo()
        {
            if (!_device.IsConnected) return ("Disconnected", 0.0, false);

            if (!_isBluetooth)
                return ("USB Power (Cable Connected)", 1.0, false);

            int level = _cachedBatteryLevel > 0 ? Math.Clamp(_cachedBatteryLevel, 0, 100) : 50;
            return ($"Battery: {level}%", level / 100.0, true);
        }

        public void Dispose()
        {
            _device.CloseDevice();
        }
    }
}
