using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DriftLift.Core.Calibration;

namespace DriftLift.Models
{
    public partial class GameProfileModel : ObservableObject
    {
        [ObservableProperty] private string _id = Guid.NewGuid().ToString();
        [ObservableProperty] private string _gameName = "New Game Profile";
        [ObservableProperty] private string _executableName = "game.exe";
        [ObservableProperty] private string _category = "Sports";
        [ObservableProperty] private string _iconGlyph = "🎮";
        private System.Windows.Media.ImageSource? _gameIcon;
        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Media.ImageSource? GameIcon
        {
            get => _gameIcon;
            set => SetProperty(ref _gameIcon, value);
        }
        [ObservableProperty] private string? _fullExecutablePath;
        [ObservableProperty] private string? _customLogoPath;
        [ObservableProperty] private bool _isAutoSwitchEnabled = true;
        [ObservableProperty] private bool _isActive;

        [ObservableProperty] private double _leftStickDeadzone = 5.0;
        [ObservableProperty] private double _rightStickDeadzone = 5.0;
        [ObservableProperty] private double _stickSensitivity = 1.0;
        [ObservableProperty] private ResponseCurveType _curveType = ResponseCurveType.Linear;

        public ObservableCollection<CustomMapping> CustomMappings { get; set; } = new();
    }
}
