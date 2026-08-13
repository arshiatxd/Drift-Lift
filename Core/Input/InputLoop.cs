using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DriftLock.Core.Calibration;
using DriftLock.Core.Output;
using DriftLock.Models;
namespace DriftLock.Core.Input
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
                Name = "DriftLock.HighPrecisionInputLoop"
            };
            _watcherThread = new Thread(DeviceWatcherLoop) 
            { 
                IsBackground = true, 
                Priority = ThreadPriority.BelowNormal,
                Name = "DriftLock.DeviceWatcherThread"
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
        private void Loop()
        {
            while (_running)
            {
                try
                {
                    foreach (var pair in _devices.Values)
                    {
                        if (pair != null && pair.Physical != null && pair.Physical.IsConnected)
                        {
                            var rawState = pair.Physical.GetCurrentState();
                            pair.Drift.Process(rawState, out double clx, out double cly, out double crx, out double cry);
                            ushort finalButtons = 0;
                            ushort rb = rawState.Buttons;
                            foreach (var kvp in pair.Remaps)
                            {
                                if ((rb & kvp.Key) != 0)
                                {
                                    finalButtons |= kvp.Value;
                                }
                            }
                            ushort mappedSources = 0;
                            foreach (var k in pair.Remaps.Keys) mappedSources |= k;
                            finalButtons |= (ushort)(rb & ~mappedSources);
                            var outState = new ControllerState
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
                            _persistentVirtualPad.EnsureCreated();
                            _persistentVirtualPad.SendState(outState);
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.LogException(ex, "InputLoop");
                }
                Thread.Sleep(1);
            }
        }
        private void DeviceWatcherLoop()
        {
            // ##== Decoupled background device enumeration - zero impact on input thread ==##
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
            catch { }
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
                            removedPair.Physical.Dispose();
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
                {
                    DevicesChanged?.Invoke();
                }
            }
            catch { }
        }
    }
}
