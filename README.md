# Drift Lift

Drift Lift is a Windows application for gamepad controller calibration, stick drift correction, button remapping, and double-input prevention.

---

## Key Features

- **Stick Drift Calibration**: Measures analog resting noise and calculates center offsets to eliminate drift without increasing in-game deadzones.
- **Button Remapping**: Custom button mapping for Xbox and PlayStation (DualShock 4 and DualSense) controllers.
- **Input Visualizer**: Real-time display of analog stick positions, trigger values, and button presses.
- **PS Controller LED Control**: Custom RGB color wheel and brightness controls for PlayStation controllers.
- **HidHide Integration**: Automatic detection and setup for HidHide to prevent double-input in games.
- **Macro Recorder**: Record and play back button sequences with timing delays.
- **Profile Management**: Save and load custom calibration configurations.
- **Theme Support**: Dark mode (Red Neon) and Light mode options.

---

## Installation

### Method 1 — Setup Installer
1. Download **`DriftLift_Setup.exe`** from [Releases](https://github.com/arshiatxd/Drift-Lift/releases).
2. Run the installer and follow the setup prompt.

### Method 2 — Build from Source
**Requirements:**
- Windows 10 or 11 (x64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases)

```bash
git clone https://github.com/arshiatxd/Drift-Lift.git
cd Drift-Lift
dotnet publish -c Release -o ./publish
```

---

## System Requirements

- **OS**: Windows 10 (x64) or Windows 11
- **Runtime**: .NET 10.0
- **Drivers**: ViGEmBus (required for virtual output) · HidHide (optional, for hiding physical controller)
- **Supported Gamepads**: DualShock 4, DualSense, Xbox 360, Xbox One, Xbox Series X/S

---

## Project Structure

```
DriftLift/
├── Core/
│   ├── Input/                  # HID enumeration & 1000Hz polling loop
│   ├── Calibration/            # Stick drift auto-correction logic
│   └── Output/                 # ViGEmBus virtual controller handler
├── ViewModels/                 # MVVM view models
├── Views/                      # UI pages & custom windows
├── Themes/                     # Dark and light XAML resource dictionaries
├── Services/                   # Driver installer helper service
└── Models/                     # Data contracts & calibration profiles
```

---

## Troubleshooting & Crash Logs

If you encounter an issue or crash, log files are stored locally at:
`%LOCALAPPDATA%\DriftLift\crash.log`

You can open a bug report on the [Issues](https://github.com/arshiatxd/Drift-Lift/issues) page.

---

## License

Distributed under the **MIT License**. See [LICENSE](LICENSE) for more details.
