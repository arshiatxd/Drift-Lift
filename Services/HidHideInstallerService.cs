using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nefarius.Drivers.HidHide;
using Nefarius.Utilities.DeviceManagement.PnP;
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
                if (!string.IsNullOrEmpty(Environment.ProcessPath))
                {
                    try { svc.AddApplicationPath(Environment.ProcessPath); } catch { }
                }

                try
                {
                    string? mainModule = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(mainModule))
                    {
                        svc.AddApplicationPath(mainModule);
                    }
                }
                catch { }

                try
                {
                    string baseDirApp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DriftliftApp.exe");
                    if (File.Exists(baseDirApp))
                    {
                        svc.AddApplicationPath(baseDirApp);
                    }
                }
                catch { }

                try
                {
                    string pfApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DriftLift", "DriftliftApp.exe");
                    if (File.Exists(pfApp))
                    {
                        svc.AddApplicationPath(pfApp);
                    }
                }
                catch { }
            }
            catch { }
        }

        public static bool AutoShieldAllControllers(HidHideControlService? existingSvc = null)
        {
            try
            {
                var svc = existingSvc ?? new HidHideControlService();
                if (!svc.IsInstalled) return false;

                svc.IsActive = true;
                try { svc.IsAppListInverted = false; } catch { }
                WhitelistCurrentProcess(svc);

                // Clean up any legacy or invalid entries
                try
                {
                    foreach (var id in svc.BlockedInstanceIds)
                    {
                        if (string.IsNullOrWhiteSpace(id) || id.StartsWith("XINPUT_", StringComparison.OrdinalIgnoreCase))
                        {
                            try { svc.RemoveBlockedInstanceId(id); } catch { }
                        }
                    }
                }
                catch { }

                var controllerIds = DeviceEnumerator.GetAllPhysicalControllerInstanceIds();
                foreach (var id in controllerIds)
                {
                    if (string.IsNullOrWhiteSpace(id) || id.StartsWith("XINPUT_", StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        svc.AddBlockedInstanceId(id);
                    }
                    catch { }

                    try
                    {
                        PnPDevice? pnp = null;
                        try { pnp = PnPDevice.GetDeviceByInstanceId(id); } catch { }
                        if (pnp is not null)
                        {
                            try
                            {
                                var parent = pnp.Parent;
                                if (parent != null && !string.IsNullOrEmpty(parent.InstanceId)
                                    && !parent.InstanceId.StartsWith("USB\\ROOT_HUB", StringComparison.OrdinalIgnoreCase)
                                    && !parent.InstanceId.StartsWith("PCI\\", StringComparison.OrdinalIgnoreCase))
                                {
                                    try { svc.AddBlockedInstanceId(parent.InstanceId); } catch { }
                                }
                            }
                            catch { }

                            try
                            {
#pragma warning disable CS0618
                                pnp.Restart();
#pragma warning restore CS0618
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AutoShieldPlayStationControllers() => AutoShieldAllControllers();

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

