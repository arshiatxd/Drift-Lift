# Changelog

All notable changes to Drift Lift are documented here.

## [1.0.3] — 2026-08-13

### Added
- **Dynamic Controller Glyphs**: Testing grid switches button icons between PlayStation (`✕`, `◯`, `▢`, `△`) and Xbox (`A`, `B`, `X`, `Y`) automatically.
- **Transparent Light Logo**: Erased square boundary background around header logo in Light Theme.
- **Theme Scrollbars**: Slim scrollbars styled with Red thumb (`#FF1744`) in Dark Mode and Sky Blue (`#0284C7`) in Light Mode.

### Fixed & Refactored
- **Trigger Hotspot Remapping**: Fixed L2/LT (`0x0400`) and R2/RT (`0x0800`) bitmask trigger mapping overlap bug.
- **Theme Accent Colors**: Harmonized slider thumb and track colors across both Dark and Light themes.
- **Pack URI Asset Loading**: Standardized asset bindings to WPF `pack://` URIs for standalone builds.
- **Codebase Organization**: Restored clean section indicators (`// ##== ... ==##`) across core source files.

---

## [1.0.2] — 2026-08-13

### Added
- **PS Controller LED Control**: Color wheel, RGB sliders, brightness control, and quick swatches for DS4 & DualSense.
- **Input Testing Glyphs**: Authentic PlayStation/Xbox button badges in remap section.
- **Safety Dialogs**: Confirmation modal for header RESET action.

### Fixed & Refactored
- **90° UI Geometry**: Enforced clean rectangular corners across all cards, containers, buttons, and popups.
- **Thread Safety**: Fixed thread synchronization in high-frequency 1000Hz input polling loop.

---

## [1.0.1] — 2026-08-07

### Added & Fixed
- **Custom Installer Branding**: Integrated app icon and wizard imagery into installer executable.
- **Cleaned Codebase**: Removed developer scratch comments and organized core architecture.

---

## [1.0.0] — 2026-08-07

### Initial Release
- **1000Hz Polling Engine**: Sub-millisecond input loop using Windows Multimedia Timer.
- **Auto Stick Drift Fix**: Automatic resting noise measurement and deadzone lock.
- **HidHide Integration**: Zero-touch installation and configuration to eliminate double-input in games.
- **Profiles & Macros**: Named profile management and macro sequence recording/replay.
