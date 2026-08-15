using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Win32;

namespace DriftLift.Services
{
    public class ScannedGameInfo
    {
        public string Title { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string ExecutableName => Path.GetFileName(ExecutablePath).ToLowerInvariant();
        public string Platform { get; set; } = "PC Game";
        public string Category { get; set; } = "Action / Sports";
        public ImageSource? Icon { get; set; }
        public bool IsSelected { get; set; } = true;
    }

    public static class InstalledGameScannerService
    {
        private static readonly HashSet<string> ExcludedExeNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "unins000.exe", "uninstall.exe", "installer.exe", "setup.exe", "crashmailer.exe",
            "crashreport.exe", "crashreporter.exe", "unitycrashhandler32.exe", "unitycrashhandler64.exe",
            "dxsetup.exe", "vcredist_x64.exe", "vcredist_x86.exe", "steam.exe", "steamservice.exe",
            "steamwebhelper.exe", "eadesktop.exe", "ealauncher.exe", "epicgameslauncher.exe",
            "easyanticheat.exe", "easyanticheat_setup.exe", "battleye.exe", "upc.exe"
        };

        public static async Task<List<ScannedGameInfo>> ScanAllInstalledGamesAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<ScannedGameInfo>();
                var seenExes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var libraryPaths = GetKnownGameDirectories();

                foreach (var libPath in libraryPaths)
                {
                    if (!Directory.Exists(libPath)) continue;

                    try
                    {
                        var gameFolders = Directory.GetDirectories(libPath);
                        foreach (var folder in gameFolders)
                        {
                            try
                            {
                                var folderName = Path.GetFileName(folder);
                                if (string.IsNullOrWhiteSpace(folderName) || folderName.StartsWith(".")) continue;

                                var mainExe = FindMainGameExecutable(folder);
                                if (!string.IsNullOrEmpty(mainExe) && File.Exists(mainExe) && seenExes.Add(mainExe))
                                {
                                    var icon = GameIconExtractor.GetExecutableIcon(mainExe);
                                    string platform = DetectPlatform(libPath);
                                    string category = DetectCategory(folderName);

                                    results.Add(new ScannedGameInfo
                                    {
                                        Title = CleanGameTitle(folderName),
                                        ExecutablePath = mainExe,
                                        Platform = platform,
                                        Category = category,
                                        Icon = icon,
                                        IsSelected = true
                                    });
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                return results.OrderBy(g => g.Title).ToList();
            });
        }

        private static List<string> GetKnownGameDirectories()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key?.GetValue("SteamPath") is string steamPath)
                {
                    string steamApps = Path.Combine(steamPath.Replace('/', '\\'), "steamapps", "common");
                    if (Directory.Exists(steamApps)) paths.Add(steamApps);

                    string vdfPath = Path.Combine(steamPath.Replace('/', '\\'), "steamapps", "libraryfolders.vdf");
                    if (File.Exists(vdfPath))
                    {
                        var content = File.ReadAllText(vdfPath);
                        var matches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
                        foreach (Match m in matches)
                        {
                            if (m.Groups.Count > 1)
                            {
                                string extraPath = Path.Combine(m.Groups[1].Value.Replace(@"\\", @"\"), "steamapps", "common");
                                if (Directory.Exists(extraPath)) paths.Add(extraPath);
                            }
                        }
                    }
                }
            }
            catch { }

            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            paths.Add(Path.Combine(progFilesX86, "Steam", "steamapps", "common"));
            paths.Add(Path.Combine(progFiles, "Steam", "steamapps", "common"));
            paths.Add(Path.Combine(progFiles, "EA Games"));
            paths.Add(Path.Combine(progFilesX86, "Origin Games"));
            paths.Add(Path.Combine(progFiles, "Epic Games"));
            paths.Add(Path.Combine(progFilesX86, "Ubisoft", "Ubisoft Game Launcher", "games"));
            paths.Add(Path.Combine(progFilesX86, "GOG Galaxy", "Games"));
            paths.Add(@"C:\Games");
            paths.Add(@"C:\GOG Games");
            paths.Add(@"C:\XboxGames");

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;
                string root = drive.RootDirectory.FullName;
                paths.Add(Path.Combine(root, "SteamLibrary", "steamapps", "common"));
                paths.Add(Path.Combine(root, "Games"));
                paths.Add(Path.Combine(root, "EA Games"));
                paths.Add(Path.Combine(root, "Epic Games"));
                paths.Add(Path.Combine(root, "XboxGames"));
            }

            return paths.ToList();
        }

        private static string? FindMainGameExecutable(string folder)
        {
            try
            {
                var exes = Directory.GetFiles(folder, "*.exe", SearchOption.TopDirectoryOnly);
                var valid = exes.Where(e => !ExcludedExeNames.Contains(Path.GetFileName(e))).ToList();

                if (valid.Count == 1) return valid[0];
                if (valid.Count > 1)
                {
                    string folderName = Path.GetFileName(folder);
                    var exact = valid.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e).Replace(" ", "").Contains(folderName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                    if (exact != null) return exact;

                    return valid.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                }

                var subExes = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories)
                    .Where(e => !ExcludedExeNames.Contains(Path.GetFileName(e)) && !e.Contains(@"\Engine\", StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .ToList();

                if (subExes.Count > 0)
                {
                    return subExes.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                }
            }
            catch { }

            return null;
        }

        private static string DetectPlatform(string path)
        {
            if (path.Contains("Steam", StringComparison.OrdinalIgnoreCase)) return "Steam";
            if (path.Contains("EA", StringComparison.OrdinalIgnoreCase) || path.Contains("Origin", StringComparison.OrdinalIgnoreCase)) return "EA App";
            if (path.Contains("Epic", StringComparison.OrdinalIgnoreCase)) return "Epic Games";
            if (path.Contains("Ubisoft", StringComparison.OrdinalIgnoreCase)) return "Ubisoft Connect";
            if (path.Contains("GOG", StringComparison.OrdinalIgnoreCase)) return "GOG Galaxy";
            if (path.Contains("Xbox", StringComparison.OrdinalIgnoreCase)) return "Xbox PC";
            return "Installed PC Game";
        }

        private static string DetectCategory(string name)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("fc") || n.Contains("fifa") || n.Contains("pes") || n.Contains("efootball") || n.Contains("nba") || n.Contains("madden") || n.Contains("rocket")) return "Sports / Racing";
            if (n.Contains("cod") || n.Contains("duty") || n.Contains("apex") || n.Contains("fortnite") || n.Contains("halo") || n.Contains("battlefield") || n.Contains("counter")) return "FPS / Shooter";
            if (n.Contains("forza") || n.Contains("f1") || n.Contains("need for speed") || n.Contains("dirt") || n.Contains("crew")) return "Racing Simulation";
            if (n.Contains("gta") || n.Contains("cyberpunk") || n.Contains("witcher") || n.Contains("elden") || n.Contains("souls") || n.Contains("god of war") || n.Contains("red dead")) return "Action / RPG";
            return "Action / Adventure";
        }

        private static string CleanGameTitle(string rawName)
        {
            return rawName.Replace('_', ' ').Trim();
        }
    }
}
