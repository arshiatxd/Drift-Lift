using System;
using DriftLift.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
namespace DriftLift.Core.Output
{
    public class VirtualController : IDisposable
    {
        private ViGEmClient? _client;
        private IXbox360Controller? _target;
        private bool _isCreated;
        public bool IsActive => _isCreated && _target != null;
        public void EnsureCreated()
        {
            if (_isCreated) return;
            try
            {
                _client = new ViGEmClient();
                _target = _client.CreateXbox360Controller();
                _target.Connect();
                _isCreated = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ViGEm Client not available: {ex.Message}");
                _isCreated = false;
            }
        }
        public void SendState(ControllerState state)
        {
            if (!_isCreated || _target == null) return;
            try
            {
                _target.SetAxisValue(Xbox360Axis.LeftThumbX, (short)(state.LeftThumbX * 32767));
                _target.SetAxisValue(Xbox360Axis.LeftThumbY, (short)(state.LeftThumbY * 32767));
                _target.SetAxisValue(Xbox360Axis.RightThumbX, (short)(state.RightThumbX * 32767));
                _target.SetAxisValue(Xbox360Axis.RightThumbY, (short)(state.RightThumbY * 32767));
                _target.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(state.LeftTrigger * 255));
                _target.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(state.RightTrigger * 255));
                ushort b = state.Buttons;
                _target.SetButtonState(Xbox360Button.A, (b & 0x1000) != 0);
                _target.SetButtonState(Xbox360Button.B, (b & 0x2000) != 0);
                _target.SetButtonState(Xbox360Button.X, (b & 0x4000) != 0);
                _target.SetButtonState(Xbox360Button.Y, (b & 0x8000) != 0);
                _target.SetButtonState(Xbox360Button.Up, (b & 0x0001) != 0);
                _target.SetButtonState(Xbox360Button.Down, (b & 0x0002) != 0);
                _target.SetButtonState(Xbox360Button.Left, (b & 0x0004) != 0);
                _target.SetButtonState(Xbox360Button.Right, (b & 0x0008) != 0);
                _target.SetButtonState(Xbox360Button.LeftShoulder, (b & 0x0100) != 0);
                _target.SetButtonState(Xbox360Button.RightShoulder, (b & 0x0200) != 0);
                _target.SetButtonState(Xbox360Button.LeftThumb, (b & 0x0040) != 0);
                _target.SetButtonState(Xbox360Button.RightThumb, (b & 0x0080) != 0);
                _target.SetButtonState(Xbox360Button.Start, (b & 0x0010) != 0);
                _target.SetButtonState(Xbox360Button.Back, (b & 0x0020) != 0);
                _target.SubmitReport();
            }
            catch { }
        }
        public void Dispose()
        {
            if (_target != null)
            {
                try
                {
                    _target.Disconnect();
                }
                catch { }
                _target = null;
            }
            if (_client != null)
            {
                try
                {
                    _client.Dispose();
                }
                catch { }
                _client = null;
            }
            _isCreated = false;
        }
    }
}
