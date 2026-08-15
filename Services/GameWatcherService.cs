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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int flags, StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

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

        private void WatcherLoop()
        {
            while (_running)
            {
                if (IsEnabled)
                {
                    try
                    {
                        string currentExe = GetActiveForegroundProcessExe();
                        if (!string.Equals(currentExe, _lastActiveExe, StringComparison.OrdinalIgnoreCase))
                        {
                            _lastActiveExe = currentExe;
                            ActiveGameChanged?.Invoke(currentExe);
                        }
                    }
                    catch { }
                }

                Thread.Sleep(500);
            }
        }

        private static string GetActiveForegroundProcessExe()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return string.Empty;

                GetWindowThreadProcessId(hwnd, out uint pid);
                if (pid == 0 || pid == (uint)Environment.ProcessId) return string.Empty;

                IntPtr hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                if (hProcess == IntPtr.Zero) return string.Empty;

                try
                {
                    var sb = new StringBuilder(1024);
                    int size = sb.Capacity;
                    if (QueryFullProcessImageName(hProcess, 0, sb, ref size))
                    {
                        return Path.GetFileName(sb.ToString()).ToLowerInvariant();
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
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
