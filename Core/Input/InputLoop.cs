using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public ConcurrentDictionary<ushort, ushort> Remaps { get; } = new();
    }

    public class InputLoop
    {
        // ##== Fields & Setup ==##
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        private readonly ConcurrentDictionary<string, ControllerProfilePair> _devices = new();
        private readonly Thread _loopThread;
        private readonly Thread _watcherThread;
        private volatile bool _running;
        private readonly VirtualController _persistentVirtualPad = new();

        public event Action? DevicesChanged;
        public IReadOnlyDictionary<string, ControllerProfilePair> Devices => _devices;

        public InputLoop()
        {
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
                var pairs = _devices.Values;
                if (pairs.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                try
                {
                    ControllerState? outState = null;

                    foreach (var pair in pairs)
                    {
                        if (pair == null || pair.Physical == null || !pair.Physical.IsConnected)
                            continue;

                        var rawState = pair.Physical.GetCurrentState();

                        pair.Drift.Process(rawState, out double clx, out double cly, out double crx, out double cry);

                        ushort rb = rawState.Buttons;
                        ushort mappedSources = 0;
                        ushort finalButtons = 0;

                        foreach (var kvp in pair.Remaps)
                        {
                            if ((rb & kvp.Key) != 0)
                                finalButtons |= kvp.Value;
                            mappedSources |= kvp.Key;
                        }

                        finalButtons |= (ushort)(rb & ~mappedSources);

                        outState = new ControllerState
                        {
                            LeftThumbX = clx,
                            LeftThumbY = cly,
                            RightThumbX = crx,
                            RightThumbY = cry,
                            LeftTrigger = rawState.LeftTrigger,
                            RightTrigger = rawState.RightTrigger,
                            Buttons = finalButtons,
                            Touchpad = rawState.Touchpad,
                            IsConnected = true
                        };

                        break;
                    }

                    if (outState != null)
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
                }

                if (changed)
                    DevicesChanged?.Invoke();
            }
            catch { }
        }
    }
}
