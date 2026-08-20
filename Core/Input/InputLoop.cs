using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DriftLift.Core.Calibration;
using DriftLift.Core.Output;
using DriftLift.Models;

namespace DriftLift.Core.Input
{
    public class ControllerProfilePair
    {
        public IPhysicalController Physical { get; set; } = null!;
        public DriftProcessor Drift { get; set; } = null!;
        public VirtualController Virtual { get; set; } = null!;
        public ConcurrentDictionary<uint, uint> Remaps { get; } = new();
        public ControllerState LatestRawState { get; set; } = new();
        public ControllerState LatestCorrectedState { get; set; } = new();
    }

    public class InputLoop
    {
        // ##== Fields & Setup ==##
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        private readonly ConcurrentDictionary<string, ControllerProfilePair> _devices = new();
        private volatile ControllerProfilePair[] _activePairsCache = Array.Empty<ControllerProfilePair>();
        private readonly Thread _loopThread;
        private readonly Thread _watcherThread;
        private volatile bool _running;
        private volatile bool _isVirtualOutputEnabled = true;
        private volatile uint _injectedMacroButtons = 0;
        private readonly VirtualController _persistentVirtualPad = new();

        public ConcurrentDictionary<uint, bool> TurboButtons { get; } = new();
        public void SetInjectedMacroButtons(uint buttons) => _injectedMacroButtons = buttons;

        public event Action? DevicesChanged;
        public IReadOnlyDictionary<string, ControllerProfilePair> Devices => _devices;

        public bool IsVirtualOutputEnabled
        {
            get => _isVirtualOutputEnabled;
            set
            {
                _isVirtualOutputEnabled = value;
                if (!value)
                {
                    try { _persistentVirtualPad.Dispose(); } catch { }
                }
            }
        }

        public InputLoop()
        {
            _persistentVirtualPad.FeedbackReceived += PersistentVirtualPad_FeedbackReceived;
            _loopThread = new Thread(Loop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "DriftLift.HighPrecisionInputLoop"
            };
            _watcherThread = new Thread(DeviceWatcherLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "DriftLift.DeviceWatcherThread"
            };
        }

        private void PersistentVirtualPad_FeedbackReceived(double left, double right)
        {
            var pairs = _activePairsCache;
            for (int i = 0; i < pairs.Length; i++)
            {
                try { pairs[i]?.Physical?.SetVibration(left, right); } catch { }
            }
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            try { TimeBeginPeriod(1); } catch { }
            _loopThread.Start();
            _watcherThread.Start();
        }

        public void Stop()
        {
            _running = false;
            try { TimeEndPeriod(1); } catch { }
            _persistentVirtualPad.Dispose();
        }

        // ##== High Precision Input Loop ==##
        private void Loop()
        {
            while (_running)
            {
                var pairs = _activePairsCache;
                if (pairs.Length == 0)
                {
                    Thread.Sleep(2);
                    continue;
                }

                try
                {
                    ControllerState? outState = null;
                    bool turboCycleOn = (Environment.TickCount64 / 25) % 2 == 0;

                    for (int i = 0; i < pairs.Length; i++)
                    {
                        var pair = pairs[i];
                        if (pair == null || pair.Physical == null || !pair.Physical.IsConnected)
                            continue;

                        var rawState = pair.Physical.GetCurrentState();
                        pair.LatestRawState = rawState;

                        pair.Drift.Process(rawState, out double clx, out double cly, out double crx, out double cry);

                        uint rb = rawState.Buttons;
                        uint mappedSources = 0;
                        uint finalButtons = 0;

                        foreach (var kvp in pair.Remaps)
                        {
                            if ((rb & kvp.Key) != 0)
                                finalButtons |= kvp.Value;
                            mappedSources |= kvp.Key;
                        }

                        finalButtons |= (uint)(rb & ~mappedSources);

                        if (!turboCycleOn && TurboButtons.Count > 0)
                        {
                            foreach (var tb in TurboButtons)
                            {
                                if (tb.Value)
                                    finalButtons &= ~tb.Key;
                            }
                        }

                        finalButtons |= _injectedMacroButtons;

                        var correctedState = new ControllerState
                        {
                            DeviceName = rawState.DeviceName,
                            Type = rawState.Type,
                            LeftThumbX = clx,
                            LeftThumbY = cly,
                            RightThumbX = crx,
                            RightThumbY = cry,
                            LeftTrigger = (finalButtons & 0x0400) != 0 ? 1.0 : rawState.LeftTrigger,
                            RightTrigger = (finalButtons & 0x0800) != 0 ? 1.0 : rawState.RightTrigger,
                            Buttons = finalButtons,
                            Touchpad = (finalButtons & 0x00010000) != 0,
                            IsConnected = true
                        };

                        pair.LatestCorrectedState = correctedState;

                        if (outState == null)
                        {
                            outState = correctedState;
                        }
                    }

                    if (outState != null && _isVirtualOutputEnabled)
                    {
                        _persistentVirtualPad.EnsureCreated();
                        _persistentVirtualPad.SendState(outState);
                    }
                }
                catch (Exception ex)
                {
                    App.LogException(ex, "InputLoop");
                }

                Thread.Sleep(1);
            }
        }

        // ##== Device Watcher Thread ==##
        private void DeviceWatcherLoop()
        {
            while (_running)
            {
                RefreshDevices();
                Thread.Sleep(1500);
            }
        }

        public void ForceRefreshDevices()
        {
            Task.Run(() => RefreshDevices());
        }

        public void SendSimulatedState(ControllerState state)
        {
            try
            {
                _persistentVirtualPad.EnsureCreated();
                _persistentVirtualPad.SendState(state);
            }
            catch (Exception ex)
            {
                App.LogException(ex, "SendSimulatedState");
            }
        }

        private void RefreshDevices()
        {
            try
            {
                var current = DeviceEnumerator.GetConnectedControllers();
                bool changed = false;

                foreach (var id in _devices.Keys)
                {
                    if (!current.Exists(c => c.DeviceId == id))
                    {
                        if (_devices.TryRemove(id, out var removedPair))
                        {
                            try { removedPair.Physical.Dispose(); } catch { }
                        }
                        changed = true;
                    }
                }

                foreach (var phys in current)
                {
                    if (!_devices.ContainsKey(phys.DeviceId))
                    {
                        var pair = new ControllerProfilePair
                        {
                            Physical = phys,
                            Drift = new DriftProcessor(),
                            Virtual = _persistentVirtualPad
                        };
                        _devices.TryAdd(phys.DeviceId, pair);
                        changed = true;
                    }
                    else
                    {
                        try { phys.Dispose(); } catch { }
                    }
                }

                if (changed)
                {
                    _activePairsCache = _devices.Values.ToArray();
                    DevicesChanged?.Invoke();
                }
            }
            catch { }
        }
    }
}
