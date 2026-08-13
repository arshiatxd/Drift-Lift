<div align="center">

<img src="https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white"/>
<img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/License-MIT-22C55E?style=for-the-badge"/>
<img src="https://img.shields.io/badge/Status-Active-FF1744?style=for-the-badge"/>

<br/><br/>

# 🎮 Drift Lift

**The professional controller manager for Windows.**  
Fix stick drift, remap buttons, calibrate inputs, and eliminate double-input conflicts — all in one sleek app.

<br/>

![Drift Lift Banner](Assets/banner.jpg)

<br/>

[Download](#-installation) · [Features](#-features) · [Contributing](#-contributing) · [License](#-license)

</div>

---

## 🚀 What's New in Version 1.0.3

- 🎮 **Dynamic Xbox vs PlayStation Glyphs**: Remap input testing grid dynamically toggles between Xbox (`A`, `B`, `X`, `Y`) and PlayStation (`✕`, `◯`, `▢`, `△`) button badges based on connected hardware.
- 🎨 **Theme-Adaptive Custom Scrollbars**: Integrated custom thin scrollbars with red thumb (`#FF1744`) in Dark Mode and sky-blue thumb (`#0284C7`) in Light Mode.
- 🖼️ **Transparent Light Mode Header Logo**: Erased solid background box around header logo in Light Mode for seamless header blending.
- 🎯 **Trigger Remap Hotspot Fix**: Corrected trigger bitmask mappings for L2/LT (`0x0400`) and R2/RT (`0x0800`) in interactive hotspot remapping.
- 📦 **Embedded Pack URI Asset Resolution**: Updated asset path handling to WPF `pack://` URIs for flawless resource loading in published binaries.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🧲 **Auto Stick Drift Fix** | Analyzes live analog resting noise and auto-corrects center offsets down to 0.001 precision |
| 💡 **PS Controller LED Lights** | RGB color picker & brightness slider (0% = OFF, 100% = Brightest) for DualShock 4 & DualSense |
| 🔋 **Live Battery Indicator** | Functional HID battery monitor reading real percentage numbers (🔴 0–45% · 🟡 45–65% · 🟢 65–100%) |
| 🎯 **Button Remapping** | Full per-button remap support for PlayStation (DS4 / DualSense) and Xbox controllers |
| 📊 **Live Input Visualizer** | Real-time button, trigger, and thumbstick state visualizer |
| 🛡️ **HidHide Integration** | Automatic download, installation, and zero-touch configuration of the HidHide driver to eliminate double-input in games |
| ⚡ **1000Hz Input Loop** | Sub-millisecond polling rate (1ms / 1000Hz) using Windows Multimedia Timer precision |
| 🎛️ **Deadzone Tuning** | Per-stick inner/outer deadzone sliders with live preview |
| 🔁 **Macro Sequences** | Record and replay button macro sequences with configurable timing |
| 🌓 **Dark / Light Themes** | Red Neon dark mode and clean light mode, switchable at runtime |
| 🗂️ **Profile Manager** | Save, load, and switch named calibration profiles |
| 🔕 **System Tray Support** | Minimize to tray with optional close-to-tray behavior |

---

## 📦 Installation

### Option 1 — Installer (Recommended)

1. Download the latest **`DriftLift_Setup.exe`** from [Releases](https://github.com/arshiatxd/Drift-Lift/releases).
2. Run the installer and follow the setup wizard.
3. Launch **Drift Lift** from the Start Menu or Desktop shortcut.

> **Note:** On first launch, Drift Lift will detect if the [HidHide](https://github.com/nefarius/HidHide) driver is missing and offer to download and install it automatically.

### Option 2 — Build from Source

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases) (required for virtual controller output)
- Windows 10 / 11 (x64)

```bash
git clone https://github.com/arshiatxd/Drift-Lift.git
cd Drift-Lift
dotnet publish -c Release -o ./publish
```

---

## 🖥️ Requirements

| Requirement | Minimum |
|---|---|
| OS | Windows 10 (1903+) / Windows 11 |
| Architecture | x64 |
| .NET Runtime | .NET 10.0 (bundled with installer) |
| Drivers | ViGEmBus (required) · HidHide (optional, auto-installed) |
| Controllers | PlayStation DualShock 4, DualSense · Xbox 360, Xbox One, Series X/S |

---

## 🏗️ Architecture

```
Drift-Lift/
├── Core/
│   ├── Input/                  # HID device enumeration & 1000Hz input loop
│   │   ├── InputLoop.cs        # High-precision polling engine
│   │   ├── PlayStationController.cs
│   │   ├── XboxController.cs
│   │   └── DeviceEnumerator.cs
│   ├── Calibration/
│   │   └── DriftProcessor.cs   # Auto drift correction engine
│   └── Output/
│       └── VirtualController.cs # ViGEmBus Xbox 360 output
├── ViewModels/
│   └── DashboardViewModel.cs   # MVVM core logic
├── Views/                      # WPF UI pages & windows
├── Themes/                     # Red Neon & Light theme dictionaries
├── Services/
│   └── HidHideInstallerService.cs
└── Models/                     # Data contracts & profiles
```

**Tech Stack:** WPF · .NET 10 · CommunityToolkit.Mvvm · HidLibrary · ViGEmBus (Nefarius) · HidHide

---

## 🤝 Contributing

Contributions, bug reports, and feature requests are welcome!

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a PR.

---

## 🐛 Bug Reports

Found a bug? Open an [issue](https://github.com/arshiatxd/Drift-Lift/issues) with:
- Your Windows version
- Controller model
- Steps to reproduce
- Crash logs from `crash.log` (in the app installation folder)

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgements

- [Nefarius / ViGEmBus](https://github.com/nefarius/ViGEmBus) — Virtual controller driver
- [Nefarius / HidHide](https://github.com/nefarius/HidHide) — HID device hiding driver
- [HidLibrary](https://github.com/mikeobrien/HidLibrary) — HID device communication
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — MVVM source generators

---

<div align="center">

Made with ❤️ by [arshiatxd](https://github.com/arshiatxd)

</div>
