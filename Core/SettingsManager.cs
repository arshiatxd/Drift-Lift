using System;
using System.IO;
using System.Text.Json;
using DriftLift.Models;
namespace DriftLift.Core
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; } = true;
        public bool MinimizeToTrayOnClose { get; set; } = false;
        public bool RememberCloseChoice { get; set; } = false;
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
        public string ActiveProfileId { get; set; } = string.Empty;
    }
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        public AppSettings Settings { get; private set; } = new AppSettings();
        public SettingsManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "DriftLift");
            Directory.CreateDirectory(folder);
            _settingsFilePath = Path.Combine(folder, "settings.json");
            Load();
        }
        public void Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }
        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch { }
        }
    }
}
