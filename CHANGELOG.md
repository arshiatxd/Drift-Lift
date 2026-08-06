# Changelog

All notable changes to Drift Lift are documented here.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [Semantic Versioning](https://semver.org/)

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
