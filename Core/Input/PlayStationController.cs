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

        public ControllerState GetCurrentState()
        {
            var state = new ControllerState
            {
                DeviceName = DeviceName,
                Type = Type
            };

            try
            {
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

                int lxIdx, lyIdx, rxIdx, ryIdx, ltIdx, rtIdx, btn1Idx, btn2Idx, specialIdx;

                if (Type == ControllerType.DualSense)
                {
                    if (data[0] == 0x31 && data.Length >= 12)
                    {
                        lxIdx = 2; lyIdx = 3; rxIdx = 4; ryIdx = 5;
                        ltIdx = 6; rtIdx = 7;
                        btn1Idx = 9; btn2Idx = 10; specialIdx = 11;
                    }
                    else
                    {
                        int offset = data[0] == 0x01 ? 1 : 0;
                        lxIdx = offset + 0; lyIdx = offset + 1; rxIdx = offset + 2; ryIdx = offset + 3;
                        ltIdx = offset + 4; rtIdx = offset + 5;
                        btn1Idx = offset + 7; btn2Idx = offset + 8; specialIdx = offset + 9;
                    }
                }
                else
                {
                    if (data[0] == 0x11 && data.Length >= 12)
                    {
                        lxIdx = 3; lyIdx = 4; rxIdx = 5; ryIdx = 6;
                        btn1Idx = 7; btn2Idx = 8; specialIdx = 9;
                        ltIdx = 10; rtIdx = 11;
                    }
                    else
                    {
                        int offset = data[0] == 0x01 ? 1 : 0;
                        lxIdx = offset + 0; lyIdx = offset + 1; rxIdx = offset + 2; ryIdx = offset + 3;
                        btn1Idx = offset + 4; btn2Idx = offset + 5; specialIdx = offset + 6;
                        ltIdx = offset + 7; rtIdx = offset + 8;
                    }
                }

                if (data.Length <= Math.Max(Math.Max(btn1Idx, btn2Idx), Math.Max(specialIdx, Math.Max(ltIdx, rtIdx))))
                    return state;

                state.IsConnected = true;
                state.LeftThumbX = (data[lxIdx] - 128) / 128.0;
                state.LeftThumbY = (128 - data[lyIdx]) / 128.0;
                state.RightThumbX = (data[rxIdx] - 128) / 128.0;
                state.RightThumbY = (128 - data[ryIdx]) / 128.0;
                state.LeftTrigger = data[ltIdx] / 255.0;
                state.RightTrigger = data[rtIdx] / 255.0;

                byte btn1 = data[btn1Idx];
                byte btn2 = data[btn2Idx];
                byte specialBytes = data[specialIdx];
                uint mask = 0;

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

                if ((specialBytes & 0x01) != 0)
                {
                    mask |= 0x00040000;
                }
                if ((specialBytes & 0x04) != 0)
                {
                    mask |= 0x00020000;
                }

                state.Buttons = mask;

                long now = Environment.TickCount64;
                if (now - _lastBatteryCheckTicks > 500)
                {
                    _lastBatteryCheckTicks = now;
                    ParseBatteryReport(data, data[0] == 0x01 ? 1 : 0);
                }
            }
            catch
            {
                state.IsConnected = false;
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
                }
                else if (Type == ControllerType.DualSense)
                {
                    if (_device.Capabilities.InputReportByteLength > 64)
                    {
                        hidReport.ReportId = 0x31;
                        if (hidReport.Data.Length >= 5)
                        {
                            hidReport.Data[0] = 0x02;
                            hidReport.Data[1] = 0x03;
                            hidReport.Data[2] = right;
                            hidReport.Data[3] = left;
                        }
                    }
                    else
                    {
                        hidReport.ReportId = 0x02;
                        if (hidReport.Data.Length >= 5)
                        {
                            hidReport.Data[0] = 0xFF;
                            hidReport.Data[1] = 0x03;
                            hidReport.Data[2] = right;
                            hidReport.Data[3] = left;
                        }
                    }
                }

                _device.WriteReport(hidReport);
            }
            catch { }
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
