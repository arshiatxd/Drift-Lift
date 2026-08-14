using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nefarius.Drivers.HidHide;
using DriftLift.Core.Input;

namespace DriftLift.Services
{
    public class HidHideInstallerService
    {
        private const string HidHideDownloadUrl = "https://github.com/nefarius/HidHide/releases/download/v1.5.230.0/HidHide_1.5.230.0_x64.exe";

        public static bool IsHidHideInstalled()
        {
            try
            {
                var svc = new HidHideControlService();
                if (svc.IsInstalled) return true;
            }
            catch { }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Nefarius Software Solutions e.U.\HidHide");
                if (key != null) return true;
            }
            catch { }

            try
            {
                using var serviceKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HidHide");
                if (serviceKey != null) return true;
            }
            catch { }

            return false;
        }

        public static void WhitelistCurrentProcess(HidHideControlService svc)
        {
            try
            {
                if (Environment.ProcessPath != null)
                {
                    svc.AddApplicationPath(Environment.ProcessPath);
                }

                try
                {
                    string? mainModule = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(mainModule) && !string.Equals(mainModule, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase))
                    {
                        svc.AddApplicationPath(mainModule);
                    }
                }
                catch { }
            }
            catch { }
        }

        public static bool AutoShieldPlayStationControllers()
        {
            try
            {
                var svc = new HidHideControlService();
                if (!svc.IsInstalled) return false;

                svc.IsActive = true;
                WhitelistCurrentProcess(svc);

                var psInstanceIds = DeviceEnumerator.GetAllPlayStationDeviceInstanceIds();
                foreach (var id in psInstanceIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        try
                        {
                            svc.AddBlockedInstanceId(id);
                        }
                        catch { }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task DownloadAndInstallAsync(IProgress<(double ProgressPercentage, string DownloadStats)> progress, Action<string> statusCallback)
        {
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "HidHide_1.5.230.0_x64.exe");
            statusCallback("Downloading HidHide Driver...");
            using (var client = new HttpClient())
            {
                using var response = await client.GetAsync(HidHideDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempInstallerPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                byte[] buffer = new byte[8192];
                long totalRead = 0L;
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    if (totalBytes > 0)
                    {
                        double pct = (double)totalRead / totalBytes * 100.0;
                        string stats = $"{totalRead / (1024.0 * 1024.0):F1} MB / {totalBytes / (1024.0 * 1024.0):F1} MB";
                        progress.Report((pct, stats));
                    }
                    else
                    {
                        string stats = $"{totalRead / (1024.0 * 1024.0):F1} MB";
                        progress.Report((50.0, stats));
                    }
                }
            }
            statusCallback("Installing HidHide Driver (Please accept UAC prompt)...");
            var startInfo = new ProcessStartInfo
            {
                FileName = tempInstallerPath,
                Arguments = "/passive /norestart",
                UseShellExecute = true,
                Verb = "runas"
            };
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
            statusCallback("Configuring HidHide Driver & Shielding Controllers...");
            ConfigureHidHide();
            try
            {
                if (File.Exists(tempInstallerPath))
                {
                    File.Delete(tempInstallerPath);
                }
            }
            catch { }
        }

        public static bool ConfigureHidHide()
        {
            try
            {
                var svc = new HidHideControlService();
                if (svc.IsInstalled)
                {
                    svc.IsActive = true;
                    WhitelistCurrentProcess(svc);
                    AutoShieldPlayStationControllers();
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}

