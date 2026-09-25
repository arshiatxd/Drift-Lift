using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private IntPtr _lastHwnd = IntPtr.Zero;

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

                try
                {
                    var allProcesses = Process.GetProcesses();
                    foreach (var proc in allProcesses)
                    {
                        if (proc.Id == (int)pid)
                        {
                            string name = proc.ProcessName.ToLowerInvariant() + ".exe";
                            foreach (var p in allProcesses) p.Dispose();
                            return name;
                        }
                    }
                    foreach (var p in allProcesses) p.Dispose();
                }
                catch { }
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
