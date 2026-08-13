# Changelog

All notable changes to Drift Lift are documented here.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [Semantic Versioning](https://semver.org/)

## [1.0.3] — 2026-08-13

### Added
- **Dynamic PlayStation / Xbox Button Glyphs**: Remap section testing grid dynamically switches glyphs between PlayStation (`✕`, `◯`, `▢`, `△`) and Xbox (`A`, `B`, `X`, `Y`) based on connected controller type.
- **Transparent Light Mode Header Logo**: Processed header logo to make background fully transparent, removing white boundary box when switching to Light theme.
- **Theme-Adaptive Scrollbars**: Custom thin scrollbars dynamically styled per theme — Red (`#FF1744`) thumb for Dark Mode and Sky Blue (`#0284C7`) thumb for Light Mode.

### Fixed & Enhanced
- **Trigger Remap Hotspot Fix**: Corrected bitmask mapping bug in `RemapHotspot` for L2/LT and R2/RT triggers (fixed bit shift overlap with L1/LB and R1/RB).
- **Theme-Specific Slider Colors**: Updated slider thumb and track accent fills to vibrant red in Dark Theme and sky-blue in Light Theme.
- **Embedded Pack URI Asset Loading**: Refactored logo path bindings to WPF `pack://application:,,,/` URIs to guarantee embedded assembly resource loading in release builds.

---

## [1.0.2] — 2026-08-13

### Added
- **PlayStation Controller LED Customization**: Added dedicated LED Light Settings popup dialog (`PsLedWindow.xaml`), accessible directly from the Remap section whenever a PS4 (DualShock 4) or PS5 (DualSense) controller is connected.
- **RGB Color Selection & Brightness**: Interactive circular color wheel spectrum with 1:1 mouse tracking, preset color swatches (Red, Electric Blue, Neon Purple, Neon Green, Yellow, White, Off), individual RGB sliders (0–255), and a Brightness slider (0% OFF to 100% Brightest).
- **Remap Section Input Testing Icons**: Replaced generic text badges with authentic Playstation/Xbox shape icons (`L2`, `R2`, `L1`, `R1`, `△`, `◯`, `✕`, `▢`) for real-time input testing.
- **Header RESET Confirmation Dialog**: Added safety confirmation popup when clicking the RESET button in the header bar.
- **High-Quality Custom Installer Logo**: Updated setup wizard branding with clean high-resolution logo.

### Fixed & Enhanced
- **Sharp 90-Degree UI Styling**: Enforced crisp 90° rectangular geometry across all buttons, cards, popups, fields, lists, and custom scrollbars for both Light and Dark themes.
- **Thin Red Custom Scrollbars**: Integrated sleek, 4px red thumb scrollbars matching the app theme in both Light and Dark modes.
- **1000Hz Thread Safety**: Resolved thread race condition in `DriftProcessor.cs` queue enumeration during high-frequency input processing.
- **WPF Resource Cleanup**: Fixed XAML static/dynamic resource resolution issues for consistent light theme and dark theme runtime switching.
- **Custom Macro Sequence Recording**: Enhanced macro recorder engine for recording and replaying custom key sequences.
- **Cable-Connected Controller Support**: Restored battery & connection reading logic for controllers without internal batteries.
- **Sidebar & Header Cleanup**: Updated Settings sidebar icon to a gear icon and made menu background transparent.

---

## [1.0.1] — 2026-08-07

### Changed
- Installer: custom branded icon applied to `DriftLift_Setup.exe`
- Installer: added optional "Launch on startup" task during install
- Installer: added proper uninstall support (kills running process, removes registry keys)
- Installer: added `AppPublisherURL`, `AppSupportURL`, `AppUpdatesURL` metadata
- Codebase: removed all redundant inline comments; section/category indicators standardized to `##== Title ==##`

---


## [1.0.0] — 2026-08-07

### Added
- Sub-millisecond 1000Hz input polling loop with Windows Multimedia Timer precision (`timeBeginPeriod(1)`)
- Auto Stick Drift Fix engine — samples resting analog noise, computes center offsets, adjusts inner deadzones dynamically
- Full button remapping for DualShock 4, DualSense, Xbox 360, Xbox One, and Xbox Series X/S controllers
- Live input visualizer with real-time button, trigger, and thumbstick state display
- Vertical color-coded battery indicator (🔴 0–45% · 🟡 45–65% · 🟢 65–100%)
- HidHide automatic detection, download, installation, and zero-touch auto-configuration
- Per-stick inner/outer deadzone sliders with live preview
- Macro sequence recording and replay with configurable timing
- Red Neon dark mode and clean Light theme, switchable at runtime
- Named calibration profile save/load/switch
- System tray minimize with optional close-to-tray behavior
- Close behavior prompt with "remember my choice" toggle
- Sliding sidebar navigation with icon-only collapsed mode
- Multi-controller support with background device watcher thread (no input-thread blocking)

### Technical
- Decoupled HID/USB device enumeration to a dedicated low-priority background thread
- Cached battery reads during primary HID report parse to eliminate redundant `ReadReport()` calls
- ViGEmBus persistent virtual Xbox 360 pad for zero-latency output
- CommunityToolkit.Mvvm MVVM source generators
