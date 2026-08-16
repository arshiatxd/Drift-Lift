using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DriftLift.Core;
using DriftLift.Core.Icons;
using DriftLift.Core.Input;
using DriftLift.Models;
using DriftLift.Services;
using DriftLift.Views;
using Nefarius.Drivers.HidHide;
namespace DriftLift.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // ##== Service & State Fields ==##
        private readonly InputLoop _inputLoop = null!;
        private readonly SettingsManager _settingsManager = null!;
        private readonly DispatcherTimer _uiTimer = null!;
        private readonly DispatcherTimer _vibrationTimer = null!;
        private readonly GameWatcherService _gameWatcher = null!;
        private ControllerProfilePair? _activeProfile;
        private HidHideControlService? _hidHideService;
        public event Action<string, string>? NotificationRequested;
        // ##== Observable Properties ==##
        [ObservableProperty]
        private ObservableCollection<CustomMapping> _activeMappings = new ObservableCollection<CustomMapping>();
        public ObservableCollection<GameProfileModel> GameProfiles { get; } = new();
        [ObservableProperty] private GameProfileModel? _selectedGameProfile;
        partial void OnSelectedGameProfileChanged(GameProfileModel? oldValue, GameProfileModel? newValue)
        {
            if (oldValue != null)
            {
                SaveGameProfilesToDisk();
            }
        }
        [ObservableProperty] private bool _isAutoSwitchingGlobalEnabled = true;
        [ObservableProperty] private object _currentView = null!;
        [ObservableProperty] private bool _isWaitingForInput;
        [ObservableProperty] private string _waitingTargetText = "";
        private ushort _waitingTargetBit;
        private ushort _lastRawButtons;
        [ObservableProperty] private int _selectedTabIndex = 0;
        [ObservableProperty] private bool _isSidebarExpanded = true;
        [ObservableProperty] private int _sidebarColumnWidth = 250;
        [ObservableProperty] private string _hidHideStatusText = "Unknown";
        [ObservableProperty] private Brush _hidHideStatusColor = Brushes.Gray;
        [ObservableProperty] private string _hidHideButtonText = "Download HidHide";
        [ObservableProperty] private bool _isHidHideEnabled;
        [ObservableProperty] private string _btnLabelCrossOrA = "A";
        [ObservableProperty] private string _btnLabelCircleOrB = "B";
        [ObservableProperty] private string _btnLabelSquareOrX = "X";
        [ObservableProperty] private string _btnLabelTriangleOrY = "Y";
        [ObservableProperty] private string _btnLabelShareOrBack = "BACK";
        [ObservableProperty] private string _btnLabelOptionsOrStart = "START";
        [ObservableProperty] private string _btnLabelL1OrLB = "LB";
        [ObservableProperty] private string _btnLabelR1OrRB = "RB";
        [ObservableProperty] private string _btnLabelL2OrLT = "LT";
        [ObservableProperty] private string _btnLabelR2OrRT = "RT";
        [ObservableProperty] private string _targetNameL2 = "LT";
        [ObservableProperty] private string _targetNameL1 = "LB";
        [ObservableProperty] private string _targetNameShare = "BACK";
        [ObservableProperty] private string _targetNameOptions = "START";
        [ObservableProperty] private string _targetNameTriangle = "Y";
        [ObservableProperty] private string _targetNameCircle = "B";
        [ObservableProperty] private string _targetNameCross = "A";
        [ObservableProperty] private string _targetNameSquare = "X";
        [ObservableProperty] private string _targetNameR1 = "RB";
        [ObservableProperty] private string _targetNameR2 = "RT";
        [ObservableProperty] private string _leftStickTextX = "X: 0%";
        [ObservableProperty] private string _leftStickTextY = "Y: 0%";
        [ObservableProperty] private string _rightStickTextX = "X: 0%";
        [ObservableProperty] private string _rightStickTextY = "Y: 0%";
        [ObservableProperty] private double _leftStickValX = 50;
        [ObservableProperty] private double _leftStickValY = 50;
        [ObservableProperty] private double _rightStickValX = 50;
        [ObservableProperty] private double _rightStickValY = 50;
        [ObservableProperty] private bool _isVirtualOutputEnabled = true;

        partial void OnIsVirtualOutputEnabledChanged(bool value)
        {
            _inputLoop.IsVirtualOutputEnabled = value;
            if (_settingsManager != null)
            {
                _settingsManager.Settings.IsVirtualOutputEnabled = value;
                _settingsManager.Save();
            }
        }

        partial void OnIsHidHideEnabledChanged(bool value)
        {
            if (_hidHideService != null)
            {
                try
                {
                    _hidHideService.IsActive = value;
                    if (value)
                    {
                        SyncHidHideBlockedDevices();
                    }
                }
                catch { }
            }
        }
        [ObservableProperty] private bool _isDarkTheme = true;
        public string ThemeToggleIcon => IsDarkTheme ? "🌙" : "☀️";
        partial void OnIsDarkThemeChanged(bool value) => OnPropertyChanged(nameof(ThemeToggleIcon));
        [ObservableProperty] private string _appLogoSource = "pack://application:,,,/DriftliftApp;component/icon.ico";
        [ObservableProperty] private int _calibrationStep = 1;
        [ObservableProperty] private string _stepPromptText = "Push both sticks to the top-left corner, then release";
        [ObservableProperty] private string _stepSubPromptText = "Release the sticks completely, then press Next";
        [ObservableProperty] private double _stickSensitivity = 1.0;
        [ObservableProperty] private double _leftStickDeadzone = 5.0;
        [ObservableProperty] private double _rightStickDeadzone = 5.0;
        [ObservableProperty] private bool _minimizeToTrayOnClose;
        [ObservableProperty] private bool _startWithWindows;
        [ObservableProperty] private bool _startMinimized;
        
        partial void OnStickSensitivityChanged(double value)
        {
            if (_activeProfile != null && _activeProfile.Drift != null && _activeProfile.Drift.Profile != null)
            {
                _activeProfile.Drift.Profile.LeftStick.Sensitivity = value;
                _activeProfile.Drift.Profile.RightStick.Sensitivity = value;
            }
        }
        partial void OnLeftStickDeadzoneChanged(double value)
        {
            if (_activeProfile != null && _activeProfile.Drift != null && _activeProfile.Drift.Profile != null)
            {
                _activeProfile.Drift.Profile.LeftStick.DeadzoneRadius = value / 100.0;
            }
            if (Math.Abs(LeftInnerDeadzone - value / 100.0) > 0.001)
            {
                LeftInnerDeadzone = value / 100.0;
            }
        }
        partial void OnRightStickDeadzoneChanged(double value)
        {
            if (_activeProfile != null && _activeProfile.Drift != null && _activeProfile.Drift.Profile != null)
            {
                _activeProfile.Drift.Profile.RightStick.DeadzoneRadius = value / 100.0;
            }
            if (Math.Abs(RightInnerDeadzone - value / 100.0) > 0.001)
            {
                RightInnerDeadzone = value / 100.0;
            }
        }
        partial void OnMinimizeToTrayOnCloseChanged(bool value)
        {
            if (_settingsManager != null)
            {
                _settingsManager.Settings.MinimizeToTrayOnClose = value;
                _settingsManager.Save();
            }
        }
        partial void OnStartWithWindowsChanged(bool value)
        {
            if (_settingsManager != null)
            {
                _settingsManager.Settings.StartWithWindows = value;
                _settingsManager.Save();
                SetStartupRegistry(value);
            }
        }
        partial void OnStartMinimizedChanged(bool value)
        {
            if (_settingsManager != null)
            {
                _settingsManager.Settings.StartMinimized = value;
                _settingsManager.Save();
            }
        }
        private static void SetStartupRegistry(bool enable)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    if (enable)
                    {
                        string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                        if (!string.IsNullOrEmpty(exe)) key.SetValue("DriftLift", $"\"{exe}\"");
                    }
                    else
                    {
                        key.DeleteValue("DriftLift", false);
                    }
                }
            }
            catch { }
        }

        public ObservableCollection<MacroItem> ActiveMacros { get; } = new();
        [ObservableProperty] private MacroItem? _activeMacro;
        [ObservableProperty] private bool _isRecordingMacro;
        [ObservableProperty] private bool _isMacroPlaying;
        [ObservableProperty] private string _recordingStatusText = "▶ RECORD MACRO";
        [ObservableProperty] private string _macroStatusText = "IDLE";
        private int _lastRecordTick = 0;
        [ObservableProperty] private double _macroDeadzone = 0.05;
        [ObservableProperty] private double _leftGraphicTranslateX;
        [ObservableProperty] private double _leftGraphicTranslateY;
        [ObservableProperty] private double _rightGraphicTranslateX;
        [ObservableProperty] private double _rightGraphicTranslateY;
        [ObservableProperty] private double _psLeftGraphicTranslateX;
        [ObservableProperty] private double _psLeftGraphicTranslateY;
        [ObservableProperty] private double _psRightGraphicTranslateX;
        [ObservableProperty] private double _psRightGraphicTranslateY;
        [ObservableProperty] private double _rawLeftX;
        [ObservableProperty] private double _rawLeftY;
        [ObservableProperty] private double _rawRightX;
        [ObservableProperty] private double _rawRightY;
        [ObservableProperty] private double _correctedLeftX;
        [ObservableProperty] private double _correctedLeftY;
        [ObservableProperty] private double _correctedRightX;
        [ObservableProperty] private double _correctedRightY;
        [ObservableProperty] private double _triggerL;
        [ObservableProperty] private double _triggerR;
        [ObservableProperty] private double _leftInnerDeadzone = 0.05;
        [ObservableProperty] private double _leftOuterDeadzone = 0.98;
        [ObservableProperty] private double _leftAntiDeadzone = 0.0;
        [ObservableProperty] private double _leftAxialDeadzone = 0.0;
        [ObservableProperty] private double _rightInnerDeadzone = 0.05;
        [ObservableProperty] private double _rightOuterDeadzone = 0.98;
        [ObservableProperty] private double _rightAntiDeadzone = 0.0;
        [ObservableProperty] private double _leftLiveCircularity;
        [ObservableProperty] private double _rightLiveCircularity;
        [ObservableProperty] private double _leftAvgCircularity;
        [ObservableProperty] private double _rightAvgCircularity;
        [ObservableProperty] private double _leftNoiseVariance;
        [ObservableProperty] private double _rightNoiseVariance;
        [ObservableProperty] private string _controllerConnectionIcon = "❌ NONE";
        [ObservableProperty] private string _macroErrorNotifier = "";
        [ObservableProperty] private bool _isAPressed;
        [ObservableProperty] private bool _isBPressed;
        [ObservableProperty] private bool _isXPressed;
        [ObservableProperty] private bool _isYPressed;
        [ObservableProperty] private bool _isDpadUpPressed;
        [ObservableProperty] private bool _isDpadDownPressed;
        [ObservableProperty] private bool _isDpadLeftPressed;
        [ObservableProperty] private bool _isDpadRightPressed;
        [ObservableProperty] private bool _isLbPressed;
        [ObservableProperty] private bool _isRbPressed;
        [ObservableProperty] private bool _isL1Pressed;
        [ObservableProperty] private bool _isR1Pressed;
        [ObservableProperty] private bool _isL2Pressed;
        [ObservableProperty] private bool _isR2Pressed;
        [ObservableProperty] private bool _isL3Pressed;
        [ObservableProperty] private bool _isR3Pressed;
        [ObservableProperty] private bool _isStartPressed;
        [ObservableProperty] private bool _isSelectPressed;
        [ObservableProperty] private bool _isOptionsPressed;
        [ObservableProperty] private bool _isSharePressed;
        [ObservableProperty] private bool _isTouchpadPressed;
        [ObservableProperty] private bool _isMutePressed;
        [ObservableProperty] private bool _isGuidePressed;
        [ObservableProperty] private bool _isVibrating;
        [ObservableProperty] private double _vibrationTimeRemaining;
        [ObservableProperty] private string _selectedVibrationMode = "Heavy";
        [ObservableProperty] private string _selectedVibrationDuration = "5";
        [ObservableProperty] private string _connectionStatusText = "DISCONNECTED";
        [ObservableProperty] private Brush _connectionStatusColor = new SolidColorBrush(Color.FromRgb(255, 23, 68));
        [ObservableProperty] private string _activeControllerImagePath = "pack://application:,,,/DriftliftApp;component/Assets/ps4_placeholder.png";
        [ObservableProperty] private bool _isPlayStation = true;
        [ObservableProperty] private bool _isPs4 = true;
        [ObservableProperty] private bool _isPs5;
        [ObservableProperty] private bool _isXbox360;
        [ObservableProperty] private bool _isXboxOne;
        [ObservableProperty] private string _deviceModelText = "No Controller Connected";
        [ObservableProperty] private string _controllerModelIconKey = "IconControllerDisconnected";
        [ObservableProperty] private string _deviceSerialText = "-";
        [ObservableProperty] private string _deviceFirmwareText = "Please connect a device via USB or Bluetooth.";
        [ObservableProperty] private string _batteryPercentageText = "Battery: --%";
        [ObservableProperty] private string _batteryPercentageShortText = "--%";
        [ObservableProperty] private double _batteryLevelWidth = 0.0;
        [ObservableProperty] private double _batteryLevelHeight = 0.0;
        [ObservableProperty] private Brush _batteryFillColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
        [ObservableProperty] private int _activePlayerIndex = 0;
        [ObservableProperty] private bool _isP1Active = true;
        [ObservableProperty] private bool _isP2Active;
        [ObservableProperty] private bool _isP3Active;
        [ObservableProperty] private bool _isP4Active;
        [ObservableProperty] private bool _isP1Connected;
        [ObservableProperty] private bool _isP2Connected;
        [ObservableProperty] private bool _isP3Connected;
        [ObservableProperty] private bool _isP4Connected;
        [ObservableProperty] private string _rawAxesText = "AXES 0: +0.00  1: +0.00  2: +0.00  3: +0.00";
        [ObservableProperty] private string _rawButtonsText = "BUTTONS: 0:OFF 1:OFF 2:OFF 3:OFF 4:OFF 5:OFF 6:OFF 7:OFF";
        public ObservableCollection<MappingItem> Mappings { get; } = new();
        public ObservableCollection<string> SavedConfigFiles { get; } = new();
        public ControllerProfilePair? ActiveProfile => _activeProfile;
        public HomeView HomeViewInstance { get; } = new();
        public ProfilesView ProfilesViewInstance { get; } = new();
        public RemapView RemapViewInstance { get; } = new();
        public CalibrateView CalibrateViewInstance { get; } = new();
        public MacrosView MacrosViewInstance { get; } = new();
        public SettingsView SettingsViewInstance { get; } = new();
        // ##== Constructor & Initialization ==##
        public DashboardViewModel(InputLoop inputLoop, SettingsManager settingsManager)
        {
            _inputLoop = inputLoop;
            _settingsManager = settingsManager;
            _inputLoop.DevicesChanged += OnDevicesChanged;
            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
            _vibrationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _vibrationTimer.Tick += VibrationTimer_Tick;

            InitializeDefaultGameProfiles();
            LoadUserMappingsAndMacros();
            _gameWatcher = new GameWatcherService();
            _gameWatcher.ActiveGameChanged += OnActiveGameChanged;
            _gameWatcher.Start();

            CurrentView = HomeViewInstance;

            if (_settingsManager != null)
            {
                IsDarkTheme = _settingsManager.Settings.IsDarkTheme;
                ApplyTheme(IsDarkTheme);
                IsVirtualOutputEnabled = _settingsManager.Settings.IsVirtualOutputEnabled;
                _inputLoop.IsVirtualOutputEnabled = IsVirtualOutputEnabled;
                PsLedRed = _settingsManager.Settings.PsLedRed;
                PsLedGreen = _settingsManager.Settings.PsLedGreen;
                PsLedBlue = _settingsManager.Settings.PsLedBlue;
                PsLedBrightness = _settingsManager.Settings.PsLedBrightness;
                MinimizeToTrayOnClose = _settingsManager.Settings.MinimizeToTrayOnClose;
                StartWithWindows = _settingsManager.Settings.StartWithWindows;
                StartMinimized = _settingsManager.Settings.StartMinimized;
            }
            else
            {
                UpdateMappingsForControllerType(true);
            }
            UpdateActiveProfile();
            RefreshSavedConfigFiles();
            CheckHidHide();
            try
            {
                _hidHideService = new HidHideControlService();
                if (_hidHideService.IsInstalled)
                {
                    _hidHideService.IsActive = true;
                    try { _hidHideService.IsAppListInverted = false; } catch { }
                    IsHidHideEnabled = true;
                    HidHideInstallerService.WhitelistCurrentProcess(_hidHideService);
                    HidHideInstallerService.AutoShieldAllControllers(_hidHideService);
                    SyncHidHideBlockedDevices();
                }
            }
            catch
            {
            }
        }
        public void TriggerDeviceRefresh()
        {
            try
            {
                _inputLoop.ForceRefreshDevices();
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateActiveProfile();
                        SyncHidHideBlockedDevices();
                    });
                }
            }
            catch { }
        }
        private static string ExtractInstanceId(string path)
        {
            return DeviceEnumerator.ExtractInstanceId(path);
        }
        public void SyncHidHideBlockedDevices()
        {
            if (_hidHideService == null || !_hidHideService.IsInstalled) return;
            try
            {
                _hidHideService.IsActive = IsHidHideEnabled;
                try { _hidHideService.IsAppListInverted = false; } catch { }
                if (!IsHidHideEnabled) return;

                HidHideInstallerService.AutoShieldAllControllers(_hidHideService);
            }
            catch { }
        }
        public void RefreshSavedConfigFiles()
        {
            SavedConfigFiles.Clear();
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Configs");
                Directory.CreateDirectory(folder);
                foreach (var f in Directory.GetFiles(folder, "*.json"))
                {
                    SavedConfigFiles.Add(Path.GetFileName(f));
                }
            }
            catch { }
        }
        public ObservableCollection<RemapRowViewModel> FaceButtonsRemap { get; } = new();
        public ObservableCollection<RemapRowViewModel> DPadRemap { get; } = new();
        public ObservableCollection<RemapRowViewModel> ShouldersRemap { get; } = new();
        public ObservableCollection<RemapRowViewModel> SpecialSticksRemap { get; } = new();
        private void UpdateMappingsForControllerType(bool isPs)
        {
            BtnLabelCrossOrA = isPs ? "✕" : "A";
            BtnLabelCircleOrB = isPs ? "◯" : "B";
            BtnLabelSquareOrX = isPs ? "▢" : "X";
            BtnLabelTriangleOrY = isPs ? "△" : "Y";
            BtnLabelShareOrBack = isPs ? "SHARE" : "BACK";
            BtnLabelOptionsOrStart = isPs ? "OPTIONS" : "START";
            BtnLabelL1OrLB = isPs ? "L1" : "LB";
            BtnLabelR1OrRB = isPs ? "R1" : "RB";
            BtnLabelL2OrLT = isPs ? "L2" : "LT";
            BtnLabelR2OrRT = isPs ? "R2" : "RT";
            TargetNameL2 = isPs ? "L2" : "LT";
            TargetNameL1 = isPs ? "L1" : "LB";
            TargetNameShare = isPs ? "SHARE" : "BACK";
            TargetNameOptions = isPs ? "OPTIONS" : "START";
            TargetNameTriangle = isPs ? "TRIANGLE" : "Y";
            TargetNameCircle = isPs ? "CIRCLE" : "B";
            TargetNameCross = isPs ? "CROSS" : "A";
            TargetNameSquare = isPs ? "SQUARE" : "X";
            TargetNameR1 = isPs ? "R1" : "RB";
            TargetNameR2 = isPs ? "R2" : "RT";
            BuildRemapRows(isPs);
            UpdateActiveMappingsTable();
        }
        private void BuildRemapRows(bool isPs)
        {
            FaceButtonsRemap.Clear();
            DPadRemap.Clear();
            ShouldersRemap.Clear();
            SpecialSticksRemap.Clear();
            List<string> options = new()
            {
                isPs ? "Cross" : "A",
                isPs ? "Circle" : "B",
                isPs ? "Square" : "X",
                isPs ? "Triangle" : "Y",
                isPs ? "L1" : "LB",
                isPs ? "R1" : "RB",
                isPs ? "L2" : "LT",
                isPs ? "R2" : "RT",
                "D-Pad Up",
                "D-Pad Down",
                "D-Pad Left",
                "D-Pad Right",
                "L3",
                "R3",
                isPs ? "Share" : "Back",
                isPs ? "Options" : "Start"
            };

            FaceButtonsRemap.Add(new RemapRowViewModel(this, 0x1000, isPs ? "Cross" : "A", options));
            FaceButtonsRemap.Add(new RemapRowViewModel(this, 0x2000, isPs ? "Circle" : "B", options));
            FaceButtonsRemap.Add(new RemapRowViewModel(this, 0x4000, isPs ? "Square" : "X", options));
            FaceButtonsRemap.Add(new RemapRowViewModel(this, 0x8000, isPs ? "Triangle" : "Y", options));

            DPadRemap.Add(new RemapRowViewModel(this, 0x0001, "D-Pad Up", options));
            DPadRemap.Add(new RemapRowViewModel(this, 0x0002, "D-Pad Down", options));
            DPadRemap.Add(new RemapRowViewModel(this, 0x0004, "D-Pad Left", options));
            DPadRemap.Add(new RemapRowViewModel(this, 0x0008, "D-Pad Right", options));

            ShouldersRemap.Add(new RemapRowViewModel(this, 0x0100, isPs ? "L1" : "LB", options));
            ShouldersRemap.Add(new RemapRowViewModel(this, 0x0200, isPs ? "R1" : "RB", options));
            ShouldersRemap.Add(new RemapRowViewModel(this, 0x0400, isPs ? "L2" : "LT", options));
            ShouldersRemap.Add(new RemapRowViewModel(this, 0x0800, isPs ? "R2" : "RT", options));

            SpecialSticksRemap.Add(new RemapRowViewModel(this, 0x0040, "L3", options));
            SpecialSticksRemap.Add(new RemapRowViewModel(this, 0x0080, "R3", options));
            SpecialSticksRemap.Add(new RemapRowViewModel(this, 0x0020, isPs ? "Share" : "Back", options));
            SpecialSticksRemap.Add(new RemapRowViewModel(this, 0x0010, isPs ? "Options" : "Start", options));
        }
        public void UpdateActiveMappingsTable()
        {
            if (_activeProfile != null)
            {
                if (_activeProfile.Remaps.Count == 0 && ActiveMappings.Count > 0)
                {
                    foreach (var map in ActiveMappings)
                    {
                        ushort src = GetBitFromName(map.SourceButton);
                        ushort tgt = GetBitFromName(map.TargetButton);
                        if (src != 0 && tgt != 0)
                        {
                            _activeProfile.Remaps[src] = tgt;
                        }
                    }
                }
                else
                {
                    ActiveMappings.Clear();
                    foreach (var kvp in _activeProfile.Remaps)
                    {
                        ActiveMappings.Add(new CustomMapping
                        {
                            SourceButton = GetButtonName(kvp.Key),
                            TargetButton = GetButtonName(kvp.Value)
                        });
                    }
                    SaveUserMappingsAndMacros();
                }
            }
            foreach (var r in FaceButtonsRemap) r.RefreshTarget();
            foreach (var r in DPadRemap) r.RefreshTarget();
            foreach (var r in ShouldersRemap) r.RefreshTarget();
            foreach (var r in SpecialSticksRemap) r.RefreshTarget();
        }
        public ushort GetBitFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            string n = name.Trim().ToUpperInvariant();
            if (n == "CROSS" || n == "A" || n.Contains("CROSS") || n.EndsWith(" A")) return 0x1000;
            if (n == "CIRCLE" || n == "B" || n.Contains("CIRCLE") || n.EndsWith(" B")) return 0x2000;
            if (n == "SQUARE" || n == "X" || n.Contains("SQUARE") || n.EndsWith(" X")) return 0x4000;
            if (n == "TRIANGLE" || n == "Y" || n.Contains("TRIANGLE") || n.EndsWith(" Y")) return 0x8000;
            if (n == "L1" || n == "LB") return 0x0100;
            if (n == "R1" || n == "RB") return 0x0200;
            if (n == "L2" || n == "LT") return 0x0400;
            if (n == "R2" || n == "RT") return 0x0800;
            if (n.Contains("UP")) return 0x0001;
            if (n.Contains("DOWN")) return 0x0002;
            if (n.Contains("LEFT")) return 0x0004;
            if (n.Contains("RIGHT")) return 0x0008;
            if (n == "L3") return 0x0040;
            if (n == "R3") return 0x0080;
            if (n == "SHARE" || n == "BACK") return 0x0020;
            if (n == "OPTIONS" || n == "START") return 0x0010;
            return 0;
        }
        private void OnDevicesChanged()
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateActiveProfile();
                    SyncHidHideBlockedDevices();
                });
            }
        }
        [ObservableProperty] private string _p1Label = "P1";
        [ObservableProperty] private string _p2Label = "P2";
        [ObservableProperty] private string _p3Label = "P3";
        [ObservableProperty] private string _p4Label = "P4";
        private static string GetShortName(ControllerProfilePair pair)
        {
            if (pair?.Physical == null) return "PAD";
            return pair.Physical.Type switch
            {
                ControllerType.DualSense => "PS5",
                ControllerType.DualShock4 => "PS4",
                _ => "XBOX"
            };
        }
        [ObservableProperty] private byte _psLedRed = 255;
        [ObservableProperty] private byte _psLedGreen = 0;
        [ObservableProperty] private byte _psLedBlue = 0;
        [ObservableProperty] private double _psLedBrightness = 1.0;

        [RelayCommand]
        public void OpenPsLedSettings()
        {
            try
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IPhysicalController? phys = _activeProfile?.Physical;
                        var win = new DriftLift.Views.Windows.PsLedWindow(phys, PsLedRed, PsLedGreen, PsLedBlue, PsLedBrightness);
                        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                        {
                            win.Owner = Application.Current.MainWindow;
                        }
                        if (win.ShowDialog() == true)
                        {
                            PsLedRed = win.SelectedR;
                            PsLedGreen = win.SelectedG;
                            PsLedBlue = win.SelectedB;
                            PsLedBrightness = win.Brightness;

                            if (_settingsManager != null)
                            {
                                _settingsManager.Settings.PsLedRed = PsLedRed;
                                _settingsManager.Settings.PsLedGreen = PsLedGreen;
                                _settingsManager.Settings.PsLedBlue = PsLedBlue;
                                _settingsManager.Settings.PsLedBrightness = PsLedBrightness;
                                _settingsManager.Save();
                            }

                            byte finalR = (byte)(PsLedRed * PsLedBrightness);
                            byte finalG = (byte)(PsLedGreen * PsLedBrightness);
                            byte finalB = (byte)(PsLedBlue * PsLedBrightness);
                            if (phys != null && phys.IsConnected)
                            {
                                phys.SetLedColor(finalR, finalG, finalB);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex, "OpenPsLedSettings");
            }
        }

        private void UpdateBatteryMetrics()
        {
            if (_activeProfile != null && _activeProfile.Physical != null && _activeProfile.Physical.IsConnected)
            {
                var bat = _activeProfile.Physical.GetBatteryInfo();
                double pct = bat.Percentage;
                int pctInt = (int)Math.Round(pct * 100.0);

                if (!bat.IsWireless)
                {
                    BatteryPercentageText = "USB Cable (Direct Power)";
                    BatteryPercentageShortText = "CABLE";
                    BatteryFillColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    BatteryLevelWidth = 110.0;
                    BatteryLevelHeight = 19.0;
                }
                else
                {
                    BatteryPercentageText = bat.Text;
                    BatteryPercentageShortText = $"{pctInt}%";

                    if (pct <= 0.20)
                    {
                        BatteryFillColor = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                    }
                    else if (pct <= 0.50)
                    {
                        BatteryFillColor = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                    }
                    else
                    {
                        BatteryFillColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    }
                    BatteryLevelWidth = Math.Max(5.0, pct * 110.0);
                    BatteryLevelHeight = Math.Max(3.0, pct * 19.0);
                }
            }
        }

        private void UpdateActiveProfile()
        {
            var devs = _inputLoop.Devices.Values.ToList();
            IsP1Connected = devs.Count > 0;
            IsP2Connected = devs.Count > 1;
            IsP3Connected = devs.Count > 2;
            IsP4Connected = devs.Count > 3;
            if (devs.Count > 0) P1Label = $"P1 ({GetShortName(devs[0])})";
            if (devs.Count > 1) P2Label = $"P2 ({GetShortName(devs[1])})";
            if (devs.Count > 2) P3Label = $"P3 ({GetShortName(devs[2])})";
            if (devs.Count > 3) P4Label = $"P4 ({GetShortName(devs[3])})";

            if (ActivePlayerIndex >= devs.Count && devs.Count > 0)
                ActivePlayerIndex = 0;
            else if (devs.Count == 0)
                ActivePlayerIndex = 0;

            IsP1Active = ActivePlayerIndex == 0;
            IsP2Active = ActivePlayerIndex == 1;
            IsP3Active = ActivePlayerIndex == 2;
            IsP4Active = ActivePlayerIndex == 3;
            _activeProfile = devs.Count > ActivePlayerIndex ? devs[ActivePlayerIndex] : null;
            if (_activeProfile != null)
            {
                ConnectionStatusText = $"CONNECTED: {_activeProfile.Physical.DeviceName.ToUpper()}";
                ConnectionStatusColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                bool isPs5 = _activeProfile.Physical.Type == ControllerType.DualSense;
                bool isPs4 = _activeProfile.Physical.Type == ControllerType.DualShock4;
                bool is360 = _activeProfile.Physical.Type == ControllerType.Xbox360;
                bool isXbOne = _activeProfile.Physical.Type == ControllerType.Xbox;

                IsPs5 = isPs5;
                IsPs4 = isPs4;
                IsPlayStation = isPs4 || isPs5;
                IsXbox360 = is360;
                IsXboxOne = isXbOne;

                if (_activeProfile.Physical.Type == ControllerType.DualSense)
                {
                    DeviceModelText = "PS5 DualSense";
                    ControllerConnectionIcon = "PS5";
                    ControllerModelIconKey = "IconControllerDualSense";
                }
                else if (_activeProfile.Physical.Type == ControllerType.DualShock4)
                {
                    DeviceModelText = "PS4 DualShock 4";
                    ControllerConnectionIcon = "PS4";
                    ControllerModelIconKey = "IconControllerDualShock4";
                }
                else if (is360 || _activeProfile.Physical.DeviceName.Contains("360"))
                {
                    IsXbox360 = true;
                    IsXboxOne = false;
                    DeviceModelText = "Xbox 360 Controller";
                    ControllerConnectionIcon = "X360";
                    ControllerModelIconKey = "IconControllerXbox";
                }
                else
                {
                    DeviceModelText = "Xbox Wireless Controller";
                    ControllerConnectionIcon = "XBOX";
                    ControllerModelIconKey = "IconControllerXbox";
                }
                var batInfo = _activeProfile.Physical.GetBatteryInfo();
                string connType = batInfo.IsWireless ? "Bluetooth" : "USB";
                DeviceFirmwareText = $"{connType} • v1.0.7";
                
                UpdateBatteryMetrics();
                UpdateMappingsForControllerType(IsPlayStation);
                ActiveControllerImagePath = IsPlayStation ? "pack://application:,,,/DriftliftApp;component/Assets/ps4_placeholder.png" : "pack://application:,,,/DriftliftApp;component/Assets/xbox_placeholder.png";

                if (IsPlayStation)
                {
                    byte finalR = (byte)(PsLedRed * PsLedBrightness);
                    byte finalG = (byte)(PsLedGreen * PsLedBrightness);
                    byte finalB = (byte)(PsLedBlue * PsLedBrightness);
                    _activeProfile.Physical.SetLedColor(finalR, finalG, finalB);
                }
            }
            else
            {
                ConnectionStatusText = "DISCONNECTED";
                ConnectionStatusColor = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                IsPlayStation = false;
                IsPs4 = false;
                IsPs5 = false;
                IsXbox360 = false;
                IsXboxOne = true;
                DeviceModelText = "No Controller Connected";
                ControllerConnectionIcon = "NONE";
                ControllerModelIconKey = "IconControllerDisconnected";
                DeviceFirmwareText = "";
                BatteryPercentageText = "Battery: --%";
                BatteryPercentageShortText = "--%";
                BatteryLevelWidth = 0.0;
                BatteryLevelHeight = 0.0;
                BatteryFillColor = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                ActiveControllerImagePath = "pack://application:,,,/DriftliftApp;component/Assets/ps4_placeholder.png";
                UpdateMappingsForControllerType(true);
            }
        }
        [RelayCommand]
        private void SelectPlayer(string indexStr)
        {
            if (int.TryParse(indexStr, out int index))
            {
                var devs = _inputLoop.Devices.Values.ToList();
                if (index < devs.Count)
                {
                    ActivePlayerIndex = index;
                    UpdateActiveProfile();
                }
            }
        }
        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var profile = _activeProfile;
                if (profile != null && profile.Physical != null && profile.Physical.IsConnected)
                {
                    var rawState = profile.LatestRawState ?? profile.Physical.GetCurrentState();
                    var corrState = profile.LatestCorrectedState ?? rawState;

                    TriggerL = rawState.LeftTrigger;
                    TriggerR = rawState.RightTrigger;
                    RawLeftX = rawState.LeftThumbX;
                    RawLeftY = rawState.LeftThumbY;
                    RawRightX = rawState.RightThumbX;
                    RawRightY = rawState.RightThumbY;

                    CorrectedLeftX = corrState.LeftThumbX;
                    CorrectedLeftY = corrState.LeftThumbY;
                    CorrectedRightX = corrState.RightThumbX;
                    CorrectedRightY = corrState.RightThumbY;

                    LeftStickTextX = $"X: {(int)(CorrectedLeftX * 100)}%";
                    LeftStickTextY = $"Y: {(int)(CorrectedLeftY * 100)}%";
                    RightStickTextX = $"X: {(int)(CorrectedRightX * 100)}%";
                    RightStickTextY = $"Y: {(int)(CorrectedRightY * 100)}%";
                    LeftStickValX = Math.Clamp((CorrectedLeftX + 1.0) / 2.0 * 100.0, 0, 100);
                    LeftStickValY = Math.Clamp((CorrectedLeftY + 1.0) / 2.0 * 100.0, 0, 100);
                    RightStickValX = Math.Clamp((CorrectedRightX + 1.0) / 2.0 * 100.0, 0, 100);
                    RightStickValY = Math.Clamp((CorrectedRightY + 1.0) / 2.0 * 100.0, 0, 100);

                    if (profile.Drift != null)
                    {
                        LeftLiveCircularity = profile.Drift.LeftMetrics.LiveCircularityError;
                        RightLiveCircularity = profile.Drift.RightMetrics.LiveCircularityError;
                        LeftAvgCircularity = profile.Drift.LeftMetrics.AverageCircularityError;
                        RightAvgCircularity = profile.Drift.RightMetrics.AverageCircularityError;
                        LeftNoiseVariance = Math.Round(profile.Drift.LeftMetrics.RestingNoiseVariance * 10000.0, 2);
                        RightNoiseVariance = Math.Round(profile.Drift.RightMetrics.RestingNoiseVariance * 10000.0, 2);
                    }

                    ushort rawButtons = rawState.Buttons;
                    if (IsWaitingForInput)
                    {
                        ushort newPresses = (ushort)(rawButtons & ~_lastRawButtons);
                        if (newPresses != 0)
                        {
                            ushort pressedBit = (ushort)(newPresses & -newPresses);
                            profile.Remaps[pressedBit] = _waitingTargetBit;
                            IsWaitingForInput = false;
                            string sourceStr = GetButtonName(pressedBit);
                            string targetStr = GetButtonName(_waitingTargetBit);
                            var existing = ActiveMappings.FirstOrDefault(m => m.SourceButton == sourceStr);
                            if (existing != null)
                            {
                                existing.TargetButton = targetStr;
                            }
                            else
                            {
                                ActiveMappings.Add(new CustomMapping { SourceButton = sourceStr, TargetButton = targetStr });
                            }
                            UpdateMappingsForControllerType(IsPlayStation);
                        }
                    }
                    if (IsRecordingMacro && ActiveMacro != null)
                    {
                        ushort newPresses = (ushort)(rawButtons & ~_lastRawButtons);
                        if (newPresses != 0)
                        {
                            ushort pressedBit = (ushort)(newPresses & -newPresses);
                            string bName = GetButtonName(pressedBit);
                            int elapsed = _lastRecordTick == 0 ? 50 : Math.Clamp(Environment.TickCount - _lastRecordTick, 10, 500);
                            _lastRecordTick = Environment.TickCount;
                            ActiveMacro.Steps.Add(new MacroStep
                            {
                                ButtonName = bName,
                                ButtonMask = pressedBit,
                                DelayMs = elapsed
                            });
                        }
                    }

                    if (!IsMacroPlaying && !IsRecordingMacro && ActiveMacros.Count > 0)
                    {
                        ushort newPresses = (ushort)(rawButtons & ~_lastRawButtons);
                        foreach (var m in ActiveMacros)
                        {
                            if (m.IsEnabled && m.TriggerMask != 0 && (rawButtons & m.TriggerMask) == m.TriggerMask && (newPresses & m.TriggerMask) != 0)
                            {
                                _ = PlayMacro(m);
                                break;
                            }
                        }
                    }

                    _lastRawButtons = rawButtons;

                    if (_inputLoop != null && ActiveMappings.Count > 0)
                    {
                        foreach (var m in ActiveMappings)
                        {
                            ushort bit = GetBitFromName(m.SourceButton);
                            if (bit != 0)
                            {
                                if (!string.IsNullOrEmpty(m.TurboMode) && m.TurboMode.Contains("Rapid", StringComparison.OrdinalIgnoreCase))
                                    _inputLoop.TurboButtons[bit] = true;
                                else
                                    _inputLoop.TurboButtons.TryRemove(bit, out _);
                            }
                        }
                    }

                    ushort mappedB = 0;
                    foreach (var kvp in profile.Remaps)
                    {
                        if ((rawButtons & kvp.Key) != 0)
                            mappedB |= kvp.Value;
                    }
                    ushort mappedSources = 0;
                    foreach (var k in profile.Remaps.Keys) mappedSources |= k;
                    mappedB |= (ushort)(rawButtons & ~mappedSources);
                    LeftGraphicTranslateX = CorrectedLeftX * 12.0;
                    LeftGraphicTranslateY = -CorrectedLeftY * 12.0;
                    RightGraphicTranslateX = CorrectedRightX * 12.0;
                    RightGraphicTranslateY = -CorrectedRightY * 12.0;
                    PsLeftGraphicTranslateX = CorrectedLeftX * 12.0;
                    PsLeftGraphicTranslateY = -CorrectedLeftY * 12.0;
                    PsRightGraphicTranslateX = CorrectedRightX * 12.0;
                    PsRightGraphicTranslateY = -CorrectedRightY * 12.0;
                    ushort b = mappedB;
                    IsDpadUpPressed = (b & 0x0001) != 0;
                    IsDpadDownPressed = (b & 0x0002) != 0;
                    IsDpadLeftPressed = (b & 0x0004) != 0;
                    IsDpadRightPressed = (b & 0x0008) != 0;
                    IsStartPressed = (b & 0x0010) != 0;
                    IsOptionsPressed = IsStartPressed;
                    IsSelectPressed = (b & 0x0020) != 0;
                    IsSharePressed = IsSelectPressed;
                    IsL3Pressed = (b & 0x0040) != 0;
                    IsR3Pressed = (b & 0x0080) != 0;
                    IsLbPressed = (b & 0x0100) != 0;
                    IsL1Pressed = IsLbPressed;
                    IsRbPressed = (b & 0x0200) != 0;
                    IsR1Pressed = IsRbPressed;
                    IsL2Pressed = TriggerL > 0.1;
                    IsR2Pressed = TriggerR > 0.1;
                    IsAPressed = (b & 0x1000) != 0;
                    IsBPressed = (b & 0x2000) != 0;
                    IsXPressed = (b & 0x4000) != 0;
                    IsYPressed = (b & 0x8000) != 0;
                    IsTouchpadPressed = rawState.Touchpad;
                    RawAxesText = $"AXES 0: {CorrectedLeftX:+0.00;-0.00}  1: {CorrectedLeftY:+0.00;-0.00}  2: {CorrectedRightX:+0.00;-0.00}  3: {CorrectedRightY:+0.00;-0.00}";
                    RawButtonsText = $"BUTTONS: A:{(IsAPressed ? "ON" : "OFF")} B:{(IsBPressed ? "ON" : "OFF")} X:{(IsXPressed ? "ON" : "OFF")} Y:{(IsYPressed ? "ON" : "OFF")} L1:{(IsL1Pressed ? "ON" : "OFF")} R1:{(IsR1Pressed ? "ON" : "OFF")}";
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex, "UiTimer_Tick");
            }
        }
        private void VibrationTimer_Tick(object? sender, EventArgs e)
        {
            VibrationTimeRemaining = Math.Max(0.0, Math.Round(VibrationTimeRemaining - 0.1, 2));
            if (VibrationTimeRemaining <= 0)
            {
                StopVibration();
                return;
            }
            if (_activeProfile != null && _activeProfile.Physical != null && _activeProfile.Physical.IsConnected)
            {
                int ticksRemaining = (int)Math.Round(VibrationTimeRemaining * 10);
                if (SelectedVibrationMode == "Burst")
                {
                    if (ticksRemaining % 4 < 2)
                        _activeProfile.Physical.SetVibration(1.0, 0.0);
                    else
                        _activeProfile.Physical.SetVibration(0.0, 0.0);
                }
                else if (SelectedVibrationMode == "Pulse")
                {
                    if (ticksRemaining % 6 < 3)
                        _activeProfile.Physical.SetVibration(0.0, 1.0);
                    else
                        _activeProfile.Physical.SetVibration(0.0, 0.0);
                }
            }
        }
        private void StopVibration()
        {
            _vibrationTimer.Stop();
            IsVibrating = false;
            VibrationTimeRemaining = 0;
            _activeProfile?.Physical.SetVibration(0, 0);
        }
        // ##== Vibration Controls ==##
        [RelayCommand]
        private void SetVibrationMode(string mode)
        {
            if (IsVibrating) StopVibration();
            SelectedVibrationMode = mode;
        }
        [RelayCommand]
        private void ToggleVibration()
        {
            if (IsVibrating) StopVibration();
            else StartVibration();
        }
        private void StartVibration()
        {
            if (_activeProfile == null || !_activeProfile.Physical.IsConnected) return;
            double left = 0;
            double right = 0;
            switch (SelectedVibrationMode)
            {
                case "Heavy": left = 1.0; right = 1.0; break;
                case "Light": left = 0.3; right = 0.3; break;
                case "Burst": left = 1.0; right = 0.0; break;
                case "Pulse": left = 0.0; right = 1.0; break;
            }
            _activeProfile.Physical.SetVibration(left, right);
            if (int.TryParse(SelectedVibrationDuration, out int duration))
                VibrationTimeRemaining = duration;
            else
                VibrationTimeRemaining = 5.0;
            IsVibrating = true;
            _vibrationTimer.Start();
        }
        // ##== Profile & Mapping Management ==##
        public void SaveUserMappingsAndMacros()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift");
                Directory.CreateDirectory(folder);
                string mapPath = Path.Combine(folder, "user_mappings.json");
                string macroPath = Path.Combine(folder, "user_macros.json");

                File.WriteAllText(mapPath, JsonSerializer.Serialize(ActiveMappings, new JsonSerializerOptions { WriteIndented = true }));
                File.WriteAllText(macroPath, JsonSerializer.Serialize(ActiveMacros, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                App.LogException(ex, "SaveUserMappingsAndMacros");
            }
        }

        public void LoadUserMappingsAndMacros()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift");
                string mapPath = Path.Combine(folder, "user_mappings.json");
                string macroPath = Path.Combine(folder, "user_macros.json");

                if (File.Exists(mapPath))
                {
                    string json = File.ReadAllText(mapPath);
                    var loadedMaps = JsonSerializer.Deserialize<ObservableCollection<CustomMapping>>(json);
                    if (loadedMaps != null && loadedMaps.Count > 0)
                    {
                        ActiveMappings.Clear();
                        foreach (var m in loadedMaps) ActiveMappings.Add(m);
                    }
                }

                if (File.Exists(macroPath))
                {
                    string json = File.ReadAllText(macroPath);
                    var loadedMacros = JsonSerializer.Deserialize<ObservableCollection<MacroItem>>(json);
                    if (loadedMacros != null && loadedMacros.Count > 0)
                    {
                        ActiveMacros.Clear();
                        foreach (var m in loadedMacros) ActiveMacros.Add(m);
                        ActiveMacro = ActiveMacros.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex, "LoadUserMappingsAndMacros");
            }
        }

        [RelayCommand]
        private void NewProfile()
        {
            ActiveMappings.Clear();
            if (_activeProfile != null)
            {
                _activeProfile.Remaps.Clear();
                UpdateMappingsForControllerType(IsPlayStation);
            }
            SaveUserMappingsAndMacros();
            DriftLift.Views.Windows.CustomMessageDialog.Show("New Profile created.", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        [RelayCommand]
        private void LoadProfile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Profiles (*.json)|*.json|All files (*.*)|*.*",
                Title = "Load Controller Profile",
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Profiles")
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<CustomMapping>>(json);
                    if (loaded != null)
                    {
                        ActiveMappings = loaded;
                        if (_activeProfile != null)
                        {
                            _activeProfile.Remaps.Clear();
                            foreach (var map in loaded)
                            {
                                ushort src = GetBitFromName(map.SourceButton);
                                ushort tgt = GetBitFromName(map.TargetButton);
                                if (src != 0 && tgt != 0)
                                {
                                    _activeProfile.Remaps[src] = tgt;
                                }
                            }
                            UpdateMappingsForControllerType(IsPlayStation);
                        }
                        SaveUserMappingsAndMacros();
                        DriftLift.Views.Windows.CustomMessageDialog.Show($"Profile '{Path.GetFileNameWithoutExtension(openFileDialog.FileName)}' loaded successfully!", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    DriftLift.Views.Windows.CustomMessageDialog.Show("Failed to load profile: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        [RelayCommand]
        private void SaveProfile()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Profiles (*.json)|*.json",
                Title = "Save Controller Profile",
                FileName = "profile.json",
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Profiles")
            };
            Directory.CreateDirectory(saveFileDialog.InitialDirectory);
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = JsonSerializer.Serialize(ActiveMappings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveFileDialog.FileName, json);
                    DriftLift.Views.Windows.CustomMessageDialog.Show($"Profile '{Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}' saved successfully!", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    DriftLift.Views.Windows.CustomMessageDialog.Show("Failed to save profile: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        [RelayCommand]
        private void ImportProfile() => LoadProfile();
        [RelayCommand]
        private void ExportProfile() => SaveProfile();
        [RelayCommand]
        private void AutoMap()
        {
            bool confirm = DriftLift.Views.Windows.CustomMessageDialog.Show(
                "Are you sure you want to reset all button mappings to default 1:1 layout?",
                "RESET MAPPINGS", true);
            if (confirm)
            {
                if (_activeProfile != null)
                {
                    _activeProfile.Remaps.Clear();
                    UpdateMappingsForControllerType(IsPlayStation);
                }
                SaveUserMappingsAndMacros();
                DriftLift.Views.Windows.CustomMessageDialog.Show("Mappings reset to default 1:1 layout.", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        [RelayCommand]
        private void AddMapping()
        {
            ActiveMappings.Add(new CustomMapping { SourceButton = "NEW", TargetButton = "UNMAPPED" });
            SaveUserMappingsAndMacros();
        }
        [RelayCommand]
        private void AddMacro()
        {
            CreateNewMacro();
        }
        // ##== Macro Recorder Engine ==##
        [RelayCommand]
        private void CreateNewMacro()
        {
            var newM = new MacroItem
            {
                Name = $"Custom Macro #{ActiveMacros.Count + 1}",
                TriggerButtonName = "L1 + R1",
                TriggerMask = 0x0300
            };
            newM.Steps.Add(new MacroStep { ButtonName = "Cross", ButtonMask = 0x1000, DelayMs = 50 });
            ActiveMacros.Add(newM);
            ActiveMacro = newM;
            SaveUserMappingsAndMacros();
        }
        [RelayCommand]
        private void ToggleRecordMacro()
        {
            if (_activeProfile == null || _activeProfile.Physical == null || !_activeProfile.Physical.IsConnected)
            {
                MacroErrorNotifier = "Connect your controller first.";
                IsRecordingMacro = false;
                RecordingStatusText = "▶ RECORD MACRO";
                return;
            }

            MacroErrorNotifier = "";
            IsRecordingMacro = !IsRecordingMacro;
            if (IsRecordingMacro)
            {
                _lastRecordTick = Environment.TickCount;
                RecordingStatusText = "⏹ STOP RECORDING";
                if (ActiveMacro == null)
                {
                    CreateNewMacro();
                }
            }
            else
            {
                RecordingStatusText = "▶ RECORD MACRO";
            }
        }
        [RelayCommand]
        private async Task PlayMacro(MacroItem? macro)
        {
            var target = macro ?? ActiveMacro;
            if (target == null || target.Steps.Count == 0) return;
            IsMacroPlaying = true;
            MacroStatusText = $"PLAYING '{target.Name.ToUpper()}'...";

            try
            {
                foreach (var step in target.Steps)
                {
                    _inputLoop.SetInjectedMacroButtons(step.ButtonMask);
                    await Task.Delay(Math.Max(10, step.DelayMs));
                    _inputLoop.SetInjectedMacroButtons(0);
                    await Task.Delay(25);
                }
            }
            catch { }
            finally
            {
                _inputLoop.SetInjectedMacroButtons(0);
                IsMacroPlaying = false;
                MacroStatusText = "IDLE";
            }
        }
        [RelayCommand]
        private void DeleteMacro(MacroItem? macro)
        {
            var target = macro ?? ActiveMacro;
            if (target != null && ActiveMacros.Contains(target))
            {
                ActiveMacros.Remove(target);
                ActiveMacro = ActiveMacros.FirstOrDefault();
            }
        }
        [RelayCommand]
        private void AddStepToActiveMacro()
        {
            if (ActiveMacro != null)
            {
                ActiveMacro.Steps.Add(new MacroStep { ButtonName = "Cross", ButtonMask = 0x1000, DelayMs = 50 });
            }
        }
        [RelayCommand]
        private void RemoveStepFromActiveMacro(MacroStep step)
        {
            if (ActiveMacro != null && step != null && ActiveMacro.Steps.Contains(step))
            {
                ActiveMacro.Steps.Remove(step);
            }
        }
        [RelayCommand]
        private void RemoveMapping(CustomMapping mapping)
        {
            if (mapping != null)
            {
                ushort srcBit = GetBitFromName(mapping.SourceButton);
                if (_activeProfile != null && srcBit != 0)
                {
                    _activeProfile.Remaps.TryRemove(srcBit, out _);
                }
                ActiveMappings.Remove(mapping);
                UpdateMappingsForControllerType(IsPlayStation);
            }
        }
        [RelayCommand]
        private void SwitchRemapTab(string tabName)
        {
            DriftLift.Views.Windows.CustomMessageDialog.Show($"Switched to {tabName} configuration (Coming Soon)", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        // ##== Theme & Navigation Controls ==##
        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ApplyTheme(IsDarkTheme);
            if (_settingsManager != null)
            {
                _settingsManager.Settings.IsDarkTheme = IsDarkTheme;
                _settingsManager.Save();
            }
        }

        public void ApplyTheme(bool isDark)
        {
            try
            {
                var appResources = Application.Current.Resources.MergedDictionaries;
                ResourceDictionary? oldTheme = null;
                foreach (var dict in appResources)
                {
                    if (dict.Source != null && (dict.Source.OriginalString.Contains("RedNeonTheme") || dict.Source.OriginalString.Contains("LightTheme")))
                    {
                        oldTheme = dict;
                        break;
                    }
                }
                if (oldTheme != null)
                {
                    appResources.Remove(oldTheme);
                }
                string targetUri = isDark ? "Themes/RedNeonTheme.xaml" : "Themes/LightTheme.xaml";
                appResources.Add(new ResourceDictionary { Source = new Uri(targetUri, UriKind.Relative) });
                AppLogoSource = isDark ? "pack://application:,,,/DriftliftApp;component/icon.ico" : "pack://application:,,,/DriftliftApp;component/Assets/logo_light.png";
                UpdateMappingsForControllerType(IsPlayStation);
            }
            catch { }
        }
        [RelayCommand]
        private void SelectTab(string index)
        {
            if (int.TryParse(index, out int idx))
            {
                SelectedTabIndex = idx;
                CurrentView = idx switch
                {
                    0 => HomeViewInstance,
                    1 => ProfilesViewInstance,
                    2 => RemapViewInstance,
                    3 => CalibrateViewInstance,
                    4 => MacrosViewInstance,
                    5 => SettingsViewInstance,
                    _ => HomeViewInstance
                };
            }
        }
        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            SidebarColumnWidth = IsSidebarExpanded ? 250 : 60;
        }

        // ##== Game Profile Auto-Switching Engine & Persistent Disk Cache ==##
        private static readonly JsonSerializerOptions ProfileJsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public void SaveGameProfilesToDisk()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, "game_profiles.json");
                var list = GameProfiles.ToList();
                string json = JsonSerializer.Serialize(list, ProfileJsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                App.LogException(ex, "SaveGameProfilesToDisk");
            }
        }

        public void LoadGameProfilesFromDisk()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift");
                string filePath = Path.Combine(folder, "game_profiles.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<List<GameProfileModel>>(json, ProfileJsonOptions);
                    if (loaded != null && loaded.Count > 0)
                    {
                        GameProfiles.Clear();
                        foreach (var p in loaded)
                        {
                            p.IsActive = false;
                            if (!string.IsNullOrEmpty(p.CustomLogoPath) && File.Exists(p.CustomLogoPath))
                            {
                                try
                                {
                                    var bi = new System.Windows.Media.Imaging.BitmapImage();
                                    bi.BeginInit();
                                    bi.UriSource = new Uri(p.CustomLogoPath);
                                    bi.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                    bi.EndInit();
                                    bi.Freeze();
                                    p.GameIcon = bi;
                                }
                                catch { }
                            }
                            else if (!string.IsNullOrEmpty(p.FullExecutablePath) && File.Exists(p.FullExecutablePath))
                            {
                                p.GameIcon = GameIconExtractor.GetExecutableIcon(p.FullExecutablePath);
                            }
                            GameProfiles.Add(p);
                        }
                        SelectedGameProfile = GameProfiles.FirstOrDefault();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex, "LoadGameProfilesFromDisk");
            }

            GameProfiles.Clear();
            SelectedGameProfile = null;
        }

        private void InitializeDefaultGameProfiles()
        {
            LoadGameProfilesFromDisk();
            GameProfiles.CollectionChanged += (s, e) => SaveGameProfilesToDisk();
        }

        private void OnActiveGameChanged(string activeExe)
        {
            if (!IsAutoSwitchingGlobalEnabled || Application.Current == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrWhiteSpace(activeExe))
                {
                    RevertToDefaultProfile();
                    return;
                }

                var matched = GameProfiles.FirstOrDefault(p =>
                    p.IsAutoSwitchEnabled &&
                    p.ExecutableName.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(exe => exe.Trim().Equals(activeExe, StringComparison.OrdinalIgnoreCase)));

                if (matched != null)
                {
                    ApplyGameProfile(matched);
                }
                else
                {
                    RevertToDefaultProfile();
                }
            });
        }

        private void ApplyGameProfile(GameProfileModel profile)
        {
            foreach (var p in GameProfiles) p.IsActive = (p == profile);

            LeftStickDeadzone = profile.LeftStickDeadzone;
            RightStickDeadzone = profile.RightStickDeadzone;
            StickSensitivity = profile.StickSensitivity;

            if (_activeProfile != null)
            {
                _activeProfile.Drift.Profile.LeftStick.DeadzoneRadius = profile.LeftStickDeadzone / 100.0;
                _activeProfile.Drift.Profile.RightStick.DeadzoneRadius = profile.RightStickDeadzone / 100.0;
                _activeProfile.Drift.Profile.LeftStick.Sensitivity = profile.StickSensitivity;
                _activeProfile.Drift.Profile.RightStick.Sensitivity = profile.StickSensitivity;
            }

            NotificationRequested?.Invoke("Drift Lift Game Profile", $"🎮 {profile.GameName} profile auto-activated");
        }

        private void RevertToDefaultProfile()
        {
            bool hadActive = false;
            foreach (var p in GameProfiles)
            {
                if (p.IsActive) hadActive = true;
                p.IsActive = false;
            }

            if (hadActive)
            {
                LeftStickDeadzone = 5.0;
                RightStickDeadzone = 5.0;
                StickSensitivity = 1.0;
                if (_activeProfile != null)
                {
                    _activeProfile.Drift.Profile.LeftStick.DeadzoneRadius = 0.05;
                    _activeProfile.Drift.Profile.RightStick.DeadzoneRadius = 0.05;
                    _activeProfile.Drift.Profile.LeftStick.Sensitivity = 1.0;
                    _activeProfile.Drift.Profile.RightStick.Sensitivity = 1.0;
                }
            }
        }

        [RelayCommand]
        private void SelectGameProfile(GameProfileModel? profile)
        {
            if (profile != null) SelectedGameProfile = profile;
        }

        [RelayCommand]
        private void ActivateSelectedGameProfile()
        {
            if (SelectedGameProfile != null)
            {
                ApplyGameProfile(SelectedGameProfile);
            }
        }

        [RelayCommand]
        private void DeactivateSelectedGameProfile()
        {
            if (SelectedGameProfile != null)
            {
                SelectedGameProfile.IsActive = false;
                RevertToDefaultProfile();
                NotificationRequested?.Invoke("Drift Lift Game Profile", $"🎮 {SelectedGameProfile.GameName} profile deactivated");
            }
        }

        [RelayCommand]
        private async Task ScanInstalledGames()
        {
            var dlg = new Views.Windows.ScanGamesDialog
            {
                Owner = Application.Current?.MainWindow
            };
            if (dlg.ShowDialog() == true && dlg.SelectedGames.Count > 0)
            {
                foreach (var g in dlg.SelectedGames)
                {
                    var existing = GameProfiles.FirstOrDefault(p =>
                        p.ExecutableName.Equals(g.ExecutableName, StringComparison.OrdinalIgnoreCase) ||
                        p.GameName.Equals(g.Title, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.GameIcon = g.Icon ?? existing.GameIcon;
                        existing.FullExecutablePath = g.ExecutablePath;
                        existing.IsAutoSwitchEnabled = true;
                        continue;
                    }

                    var newProfile = new GameProfileModel
                    {
                        GameName = g.Title,
                        ExecutableName = g.ExecutableName,
                        Category = g.Category,
                        IconGlyph = "🎮",
                        GameIcon = g.Icon,
                        FullExecutablePath = g.ExecutablePath,
                        LeftStickDeadzone = g.Category.Contains("FPS") ? 3.0 : (g.Category.Contains("Racing") ? 2.0 : 4.0),
                        RightStickDeadzone = g.Category.Contains("FPS") ? 3.0 : (g.Category.Contains("Racing") ? 2.0 : 4.0),
                        StickSensitivity = 1.0,
                        IsAutoSwitchEnabled = true
                    };
                    GameProfiles.Add(newProfile);
                }

                if (SelectedGameProfile == null) SelectedGameProfile = GameProfiles.FirstOrDefault();
                SaveGameProfilesToDisk();
                NotificationRequested?.Invoke("Installed Games Imported", $"Successfully linked {dlg.SelectedGames.Count} games with auto-profiles.");
            }
        }

        [RelayCommand]
        private void AddCustomGame()
        {
            var newProfile = new GameProfileModel
            {
                GameName = "Custom Game",
                ExecutableName = "game.exe",
                Category = "Custom",
                IconGlyph = "🎮",
                LeftStickDeadzone = 5.0,
                RightStickDeadzone = 5.0,
                StickSensitivity = 1.0,
                IsAutoSwitchEnabled = true
            };
            GameProfiles.Add(newProfile);
            SelectedGameProfile = newProfile;
            SaveGameProfilesToDisk();
        }

        [RelayCommand]
        private void BrowseGameExe()
        {
            if (SelectedGameProfile == null) return;
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "Select Game Executable"
            };
            if (dialog.ShowDialog() == true)
            {
                SelectedGameProfile.FullExecutablePath = dialog.FileName;
                SelectedGameProfile.ExecutableName = Path.GetFileName(dialog.FileName).ToLowerInvariant();
                SelectedGameProfile.GameIcon = GameIconExtractor.GetExecutableIcon(dialog.FileName);
                if (SelectedGameProfile.GameName == "Custom Game" || string.IsNullOrWhiteSpace(SelectedGameProfile.GameName))
                {
                    SelectedGameProfile.GameName = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
                SaveGameProfilesToDisk();
            }
        }

        [RelayCommand]
        private void ChangeSelectedGameLogo()
        {
            if (SelectedGameProfile == null) return;
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.ico;*.webp;*.bmp)|*.png;*.jpg;*.jpeg;*.ico;*.webp;*.bmp|All Files (*.*)|*.*",
                Title = "Select Custom Game Logo"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var bi = new System.Windows.Media.Imaging.BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(dialog.FileName);
                    bi.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();

                    SelectedGameProfile.GameIcon = bi;
                    SelectedGameProfile.CustomLogoPath = dialog.FileName;
                    SaveGameProfilesToDisk();
                }
                catch { }
            }
        }

        [RelayCommand]
        private void DeleteSelectedGameProfile()
        {
            if (SelectedGameProfile != null && GameProfiles.Count > 0)
            {
                var toRemove = SelectedGameProfile;
                int idx = GameProfiles.IndexOf(toRemove);
                GameProfiles.Remove(toRemove);
                SelectedGameProfile = GameProfiles.ElementAtOrDefault(Math.Max(0, idx - 1)) ?? GameProfiles.FirstOrDefault();
                SaveGameProfilesToDisk();
            }
        }

        [RelayCommand]
        private void ResetGameProfiles()
        {
            bool confirm = DriftLift.Views.Windows.CustomMessageDialog.Show(
                "Are you sure you want to clear all linked game profiles?",
                "CLEAR PROFILES", true);
            if (confirm)
            {
                GameProfiles.Clear();
                SelectedGameProfile = null;
                try
                {
                    string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift");
                    string filePath = Path.Combine(folder, "game_profiles.json");
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                catch { }
            }
        }
        // ##== Calibration & Auto-Fix Engine ==##
        [RelayCommand]
        private void NextCalibrationStep()
        {
            if (CalibrationStep < 4)
            {
                CalibrationStep++;
                switch (CalibrationStep)
                {
                    case 2:
                        if (_activeProfile != null)
                        {
                            _activeProfile.Drift.AutoCalibrateBoth(RawLeftX, RawLeftY, RawRightX, RawRightY);
                            _activeProfile.Drift.ResetMetrics();
                        }
                        StepPromptText = "Rotate both sticks 6 to 7 times in full circles (clockwise & counter-clockwise)";
                        StepSubPromptText = "Ensure maximum outer boundary reaches 1.0 range, then press Next";
                        break;
                    case 3:
                        StepPromptText = "Verify zero resting center error and circularity range bounds";
                        StepSubPromptText = "Check circularity error metrics, then press Next";
                        break;
                    case 4:
                        if (_activeProfile != null)
                        {
                            LeftInnerDeadzone = _activeProfile.Drift.Profile.LeftStick.DeadzoneRadius;
                            RightInnerDeadzone = _activeProfile.Drift.Profile.RightStick.DeadzoneRadius;
                            LeftStickDeadzone = Math.Round(LeftInnerDeadzone * 100.0, 0);
                            RightStickDeadzone = Math.Round(RightInnerDeadzone * 100.0, 0);
                        }
                        StepPromptText = "Save permanent calibration data to controller profile";
                        StepSubPromptText = "Calibration complete!";
                        break;
                }
            }
            else
            {
                CalibrationStep = 1;
                StepPromptText = "Push both sticks to the top-left corner, then release";
                StepSubPromptText = "Release the sticks completely, then press Next";
                SaveConfigPreset();
                DriftLift.Views.Windows.CustomMessageDialog.Show("Calibration profile successfully saved and locked!", "Calibration Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        [RelayCommand]
        private void AutoFixStickDrift()
        {
            if (_activeProfile != null)
            {
                var (lx, ly, lDz, rx, ry, rDz) = _activeProfile.Drift.AutoCalibrateBoth(RawLeftX, RawLeftY, RawRightX, RawRightY);
                LeftInnerDeadzone = lDz;
                RightInnerDeadzone = rDz;
                LeftStickDeadzone = Math.Round(lDz * 100.0, 0);
                RightStickDeadzone = Math.Round(rDz * 100.0, 0);
            }
            DriftLift.Views.Windows.CustomMessageDialog.Show("Quick Auto Calibrate completed! Center offset & deadzones locked.", "DriftLift Auto Fix", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        [RelayCommand]
        private void RemapHotspot(string buttonName)
        {
            string norm = (buttonName ?? "").ToUpper().Replace(" ", "").Replace("-", "").Replace("_", "");
            ushort bit = norm switch
            {
                "CROSS" or "A" => 0x1000,
                "CIRCLE" or "B" => 0x2000,
                "SQUARE" or "X" => 0x4000,
                "TRIANGLE" or "Y" => 0x8000,
                "L1" or "LB" => 0x0100,
                "R1" or "RB" => 0x0200,
                "L2" or "LT" => 0x0400,
                "R2" or "RT" => 0x0800,
                "SHARE" or "BACK" or "SELECT" => 0x0020,
                "OPTIONS" or "START" => 0x0010,
                "L3" or "LEFTTHUMB" => 0x0040,
                "R3" or "RIGHTTHUMB" => 0x0080,
                "DPADUP" or "UP" => 0x0001,
                "DPADDOWN" or "DOWN" => 0x0002,
                "DPADLEFT" or "LEFT" => 0x0004,
                "DPADRIGHT" or "RIGHT" => 0x0008,
                _ => 0x1000
            };
            BeginRemap(bit.ToString());
        }
        [RelayCommand]
        private void SaveConfigPreset()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Configs");
                Directory.CreateDirectory(folder);
                string file = Path.Combine(folder, $"Calibration_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                var data = new { DeviceModelText, LeftInnerDeadzone, LeftOuterDeadzone, RightInnerDeadzone, RightOuterDeadzone, StickSensitivity };
                File.WriteAllText(file, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
                RefreshSavedConfigFiles();
                DriftLift.Views.Windows.CustomMessageDialog.Show($"Configuration saved to {Path.GetFileName(file)}!", "DriftLift Config Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DriftLift.Views.Windows.CustomMessageDialog.Show($"Failed to save config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void OpenSavedConfigsFolder()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Configs");
                Directory.CreateDirectory(folder);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                DriftLift.Views.Windows.CustomMessageDialog.Show($"Failed to open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void DeleteSavedConfigFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            bool confirm = DriftLift.Views.Windows.CustomMessageDialog.Show(
                $"Are you sure you want to permanently delete preset '{fileName}'?",
                "DELETE PRESET", true);
            if (confirm)
            {
                try
                {
                    string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Configs");
                    string path = Path.Combine(folder, fileName);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        RefreshSavedConfigFiles();
                        DriftLift.Views.Windows.CustomMessageDialog.Show($"Preset '{fileName}' deleted.", "Drift Lift", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    DriftLift.Views.Windows.CustomMessageDialog.Show($"Failed to delete preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        [RelayCommand]
        private void LoadSavedConfigFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriftLift", "Configs");
                string path = Path.Combine(folder, fileName);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("LeftInnerDeadzone", out var lid)) LeftInnerDeadzone = lid.GetDouble();
                    if (root.TryGetProperty("RightInnerDeadzone", out var rid)) RightInnerDeadzone = rid.GetDouble();
                    if (root.TryGetProperty("LeftOuterDeadzone", out var lod)) LeftOuterDeadzone = lod.GetDouble();
                    if (root.TryGetProperty("RightOuterDeadzone", out var rod)) RightOuterDeadzone = rod.GetDouble();
                    if (root.TryGetProperty("StickSensitivity", out var ss)) StickSensitivity = ss.GetDouble();

                    LeftStickDeadzone = Math.Round(LeftInnerDeadzone * 100.0, 0);
                    RightStickDeadzone = Math.Round(RightInnerDeadzone * 100.0, 0);

                    if (_activeProfile != null)
                    {
                        _activeProfile.Drift.Profile.LeftStick.DeadzoneRadius = LeftInnerDeadzone;
                        _activeProfile.Drift.Profile.RightStick.DeadzoneRadius = RightInnerDeadzone;
                        _activeProfile.Drift.Profile.LeftStick.Sensitivity = StickSensitivity;
                        _activeProfile.Drift.Profile.RightStick.Sensitivity = StickSensitivity;
                    }
                    DriftLift.Views.Windows.CustomMessageDialog.Show($"Preset '{fileName}' loaded successfully!", "Calibration Preset Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DriftLift.Views.Windows.CustomMessageDialog.Show($"Failed to load preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void ResetCalibration()
        {
            bool confirm = DriftLift.Views.Windows.CustomMessageDialog.Show(
                "Are you sure you want to reset all controller calibration deadzones and settings to factory defaults?",
                "RESET CONFIGURATION", true);
            if (confirm)
            {
                LeftInnerDeadzone = 0.05; LeftOuterDeadzone = 0.98;
                RightInnerDeadzone = 0.05; RightOuterDeadzone = 0.98;
                CalibrationStep = 1;
                StepPromptText = "Push both sticks to the top-left corner, then release";
                StepSubPromptText = "Release the sticks completely, then press Next";
                DriftLift.Views.Windows.CustomMessageDialog.Show("Controller calibration and deadzones reset to defaults.", "RESET COMPLETE");
            }
        }
        [RelayCommand]
        public void BeginRemap(string parameter)
        {
            if (ushort.TryParse(parameter, out ushort targetBit))
            {
                _waitingTargetBit = targetBit;
                WaitingTargetText = $"Press a physical button to map to {GetButtonName(targetBit)}...";
                IsWaitingForInput = true;
            }
        }
        [RelayCommand]
        private void CancelRemap()
        {
            IsWaitingForInput = false;
        }
        [RelayCommand]
        private void ClearRemaps()
        {
            if (_activeProfile != null)
            {
                _activeProfile.Remaps.Clear();
                UpdateMappingsForControllerType(IsPlayStation);
            }
        }
        public string GetButtonName(ushort bit)
        {
            return bit switch
            {
                0x0001 => "D-Pad Up",
                0x0002 => "D-Pad Down",
                0x0004 => "D-Pad Left",
                0x0008 => "D-Pad Right",
                0x0010 => IsPlayStation ? "Options" : "Start",
                0x0020 => IsPlayStation ? "Share" : "Back",
                0x0040 => "L3",
                0x0080 => "R3",
                0x0100 => IsPlayStation ? "L1" : "LB",
                0x0200 => IsPlayStation ? "R1" : "RB",
                0x1000 => IsPlayStation ? "Cross" : "A",
                0x2000 => IsPlayStation ? "Circle" : "B",
                0x4000 => IsPlayStation ? "Square" : "X",
                0x8000 => IsPlayStation ? "Triangle" : "Y",
                _ => "Unknown"
            };
        }
        [RelayCommand]
        public void CheckHidHide()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string clientPath = Path.Combine(programFiles, "Nefarius Software Solutions", "HidHide", "x64", "HidHideClient.exe");
            if (File.Exists(clientPath) || HidHideInstallerService.IsHidHideInstalled())
            {
                HidHideStatusText = "SHIELD ACTIVE";
                HidHideStatusColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                HidHideButtonText = "Shield Controllers";
            }
            else
            {
                HidHideStatusText = "NOT INSTALLED";
                HidHideStatusColor = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                HidHideButtonText = "Download HidHide";
            }
        }
        [RelayCommand]
        public void AutoShieldPlayStationControllers()
        {
            if (HidHideInstallerService.IsHidHideInstalled())
            {
                bool success = HidHideInstallerService.AutoShieldAllControllers(_hidHideService);
                SyncHidHideBlockedDevices();

                if (success)
                {
                    DriftLift.Views.Windows.CustomMessageDialog.Show(
                        "Double-Input Shield is now ACTIVE!\n\nDrift Lift has whitelisted itself and hidden your physical controller from games and Steam.\n\nYour inputs will now cleanly route through the virtual Xbox 360 controller with calibrated zero stick drift and no double presses.",
                        "Double Input Shield Active", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    DriftLift.Views.Windows.CustomMessageDialog.Show(
                        "HidHide driver is installed but could not be configured automatically.\n\nPlease open the HidHide Configurator as Administrator to verify permissions.",
                        "Shield Notification", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            else
            {
                InstallHidHide();
            }
        }
        [RelayCommand]
        private void InstallHidHide()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string clientPath = Path.Combine(programFiles, "Nefarius Software Solutions", "HidHide", "x64", "HidHideClient.exe");
            if (File.Exists(clientPath) || HidHideInstallerService.IsHidHideInstalled())
            {
                HidHideInstallerService.AutoShieldAllControllers(_hidHideService);
                SyncHidHideBlockedDevices();

                DriftLift.Views.Windows.CustomMessageDialog.Show(
                    "Controller Double-Input Shield is configured!\n\nOpening HidHide Configurator so you can verify blocked devices and application whitelist.",
                    "HidHide Double Input Shield", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                try
                {
                    if (File.Exists(clientPath))
                    {
                        System.Diagnostics.Process.Start(clientPath);
                    }
                }
                catch { }
            }
            else
            {
                var result = DriftLift.Views.Windows.CustomMessageDialog.Show(
                    "HidHide driver is required to hide your physical PlayStation controller from games and prevent the 'Double Input' bug in Steam and PC games.\n\nWould you like to open the official download page now?",
                    "HidHide Required", true);
                if (result)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://github.com/nefarius/HidHide/releases/latest",
                            UseShellExecute = true
                        });
                    }
                    catch { }
                }
            }
        }
    }
    public partial class RemapRowViewModel : ObservableObject
    {
        private readonly DashboardViewModel _parent;
        public ushort SourceBit { get; }
        public string SourceButtonName { get; }
        public ObservableCollection<string> TargetOptions { get; }
        public RemapRowViewModel(DashboardViewModel parent, ushort sourceBit, string sourceButtonName, List<string> options)
        {
            _parent = parent;
            SourceBit = sourceBit;
            SourceButtonName = sourceButtonName;
            TargetOptions = new ObservableCollection<string>(options);
        }
        public string SelectedTarget
        {
            get
            {
                if (_parent.ActiveProfile != null && _parent.ActiveProfile.Remaps.TryGetValue(SourceBit, out ushort targetBit))
                {
                    return _parent.GetButtonName(targetBit);
                }
                return SourceButtonName;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ushort bit = _parent.GetBitFromName(value);
                if (_parent.ActiveProfile != null)
                {
                    if (bit == SourceBit || bit == 0)
                    {
                        _parent.ActiveProfile.Remaps.TryRemove(SourceBit, out _);
                    }
                    else
                    {
                        _parent.ActiveProfile.Remaps[SourceBit] = bit;
                    }
                    _parent.UpdateActiveMappingsTable();
                    OnPropertyChanged(nameof(SelectedTarget));
                }
            }
        }
        public void RefreshTarget()
        {
            OnPropertyChanged(nameof(SelectedTarget));
        }
        [RelayCommand]
        private void Listen()
        {
            _parent.BeginRemap(SourceBit.ToString());
        }
        [RelayCommand]
        private void Reset()
        {
            if (_parent.ActiveProfile != null)
            {
                _parent.ActiveProfile.Remaps.TryRemove(SourceBit, out _);
                _parent.UpdateActiveMappingsTable();
                OnPropertyChanged(nameof(SelectedTarget));
            }
        }
    }
}
