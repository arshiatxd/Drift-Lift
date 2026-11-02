using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DriftLift.Services
{
    public class GameWatcherService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private readonly Thread _watcherThread;
        private volatile bool _running;
        private string _lastActiveExe = string.Empty;
        private IntPtr _lastHwnd = IntPtr.Zero;

        public event Action<string>? ActiveGameChanged;
        public bool IsEnabled { get; set; } = true;

        public GameWatcherService()
        {
            _watcherThread = new Thread(WatcherLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
                Name = "DriftLift.GameWatcherThread"
            };
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _watcherThread.Start();
        }

        public void Stop()
        {
            _running = false;
        }

        private void WatcherLoop()
        {
            while (_running)
            {
                if (IsEnabled)
                {
                    try
                    {
                        IntPtr hwnd = GetForegroundWindow();
                        if (hwnd != IntPtr.Zero && hwnd != _lastHwnd)
                        {
                            _lastHwnd = hwnd;
                            string currentExe = GetActiveForegroundProcessExe(hwnd);
                            if (!string.IsNullOrEmpty(currentExe) && !string.Equals(currentExe, _lastActiveExe, StringComparison.OrdinalIgnoreCase))
                            {
                                _lastActiveExe = currentExe;
                                ActiveGameChanged?.Invoke(currentExe);
                            }
                        }
                    }
                    catch { }
                }

                Thread.Sleep(1000);
            }
        }

        private static string GetActiveForegroundProcessExe(IntPtr hwnd)
        {
            try
            {
                if (hwnd == IntPtr.Zero) return string.Empty;

                GetWindowThreadProcessId(hwnd, out uint pid);
                if (pid == 0 || pid == (uint)Environment.ProcessId) return string.Empty;

                Process[]? allProcesses = null;
                try
                {
                    allProcesses = Process.GetProcesses();
                    foreach (var proc in allProcesses)
                    {
                        try
                        {
                            if (proc.Id == (int)pid)
                            {
                                return proc.ProcessName.ToLowerInvariant() + ".exe";
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                finally
                {
                    if (allProcesses != null)
                    {
                        foreach (var p in allProcesses)
                        {
                            try { p.Dispose(); } catch { }
                        }
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
