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
        public bool IsVirtualOutputEnabled { get; set; } = true;
        public byte PsLedRed { get; set; } = 255;
        public byte PsLedGreen { get; set; } = 0;
        public byte PsLedBlue { get; set; } = 0;
        public double PsLedBrightness { get; set; } = 1.0;
        public string ActiveProfileId { get; set; } = string.Empty;
        public bool SuppressHidHidePrompt { get; set; } = false;
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
            catch (Exception ex)
            {
                App.LogException(ex, "SettingsManager.Load");
                Settings = new AppSettings();
            }
        }
        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                string tempPath = _settingsFilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _settingsFilePath, overwrite: true);
            }
            catch (Exception ex)
            {
                App.LogException(ex, "SettingsManager.Save");
            }
        }
    }
}
