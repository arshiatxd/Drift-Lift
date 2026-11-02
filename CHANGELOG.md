# Changelog — Drift Lift

All notable changes to **Drift Lift** will be documented in this file.

---

## [1.1.0] — 2026-11-03

### Highlights & Major Improvements
- **Upgraded Stick Drift Elimination Engine**: Implemented 64-sample ring-buffer statistical noise analysis (mean, peak radial deviation, and standard deviation), asymmetric cardinal span normalization (`[-1.0, +1.0]` full travel preservation after center offset subtraction), and velocity-adaptive low-pass micro-jitter smoothing with zero input lag on fast movements.
- **Instant Auto Fix & Persistent Calibration**: Upgraded **Auto Fix** (`AutoFixStickDrift`) across the Dashboard, Calibration Wizard, and TitleBar to immediately zero resting stick drift, synchronize active game profiles, and persist center offsets and inner/outer deadzones to disk (`settings.json` and preset files).
- **Pixel-Perfect Controller Visualizers**: Calibrated all button, bumper, trigger, D-Pad, system button, and thumbstick overlays across PS5 DualSense, PS4 DualShock 4, Xbox One / Series X|S, and Xbox 360 visualizers, and fixed hotspot click-remapping bitmasks for `View`/`Share`/`Create`, `Menu`/`Options`, `Guide`/`Home`, and `Mute`.
- **Asynchronous HID Output & Anti-Cheat Safe Runtime**: Offloaded PlayStation rumble and RGB lightbar HID writes to coalesced background worker threads, eliminated 60Hz UI brush allocations, fixed Turbo + Remap ordering in the 1000Hz input loop, and guaranteed deterministic `Process` handle disposal during passive foreground game detection.

---

## [1.0.9] — 2026-10-31

### Highlights & Major Improvements
- **Analog Baseline Calibration Preservation**: Retains calibrated baseline deadzones and stick sensitivity metrics across profile switches and device reconnects.
- **Xbox 360 Physical/Virtual Collision Filter**: Excludes virtual ViGEm controller slots from physical XInput enumeration to eliminate feedback loops and double-input issues.
- **Driver Security & Validation**: Added SHA-256 integrity verification before downloading and launching driver packages.
- **Startup Customization**: Added setting and prompt toggle to configure driver detection on startup.
- **Official Links**: Integrated direct links to the official website and GitHub repository in Settings.
- **Codebase & Repository Cleanup**: Removed obsolete test scripts, raw asset packs, and development dumps.
- **Input Engine & Virtual Output Stability**: Fixed virtual controller destruction issue where disabling virtual output permanently severed the ViGEm target; now safely zeroes outputs while preserving device state.
- **Remapping & Button Disabling**: Full support for blank/disabled button mappings via the circled `✕` action button and proper persistence in profile save/load routines.
- **Single-Click Clear All**: Instant wipe and disk synchronization for all active button mappings without state resurrection.
- **Performance & UI Hot-Loop Optimization**: Consolidated all button state feeds directly from the corrected input state.
- **HidHide Thread Safety**: Dispatched controller shielding and device synchronization to the UI dispatcher thread to prevent COM cross-thread exceptions.

---

## [1.0.7] — 2026-08-24

### Highlights & Major Improvements
- **Xbox One & Series X/S Visualizer Alignment**: Pixel-exact positioning and scaling for all face buttons, D-Pad directions, bumpers, analog triggers, and system buttons.
- **Circled Action Button Updates**: Configured circled `✕` buttons across all remap cards to disable physical button outputs.
- **Controller Hot-Plug Crash Protection**: Robust disconnect/reconnect handling during active gameplay without freezing the application.

---

## [1.0.6] — 2026-08-20

### Highlights & Major Improvements
- **DualSense & DualShock Touchpad Filter**: Completely decoupled touchpad inputs from analog stick drift calculations and remapping triggers.
- **Multi-Controller Profile Auto-Switching**: Enhanced game watcher engine to automatically activate per-game deadzone, sensitivity, and remapping profiles upon foreground window detection.

---

## [1.0.5] — 2026-08-14

### Highlights & Major Improvements
- **Sub-Millisecond Low Latency Architecture**: Completely eliminated thread contention on physical HID handles by isolating the 1000Hz `InputLoop` thread from UI polling. Replaced heap allocations with fixed-size circular ring buffers for zero GC pause in the hot loop.
- **PlayStation RGB Lightbar Memory**: Added persistent storage for custom DualShock 4 and DualSense lightbar colors and brightness. Controller lightbar color restores automatically upon reconnection. Updated default color to pure red (`#FF0000`).
- **Color Wheel & Indicator Calibration**: Fixed the indicator thumb ring coordinate math and clamping in the LED customization popup so it tracks the mouse with zero offset and never crosses the wheel border.
- **Full Digital Trigger & Special Button Remapping**: Added digital L2/R2 bitmasks, PlayStation Guide, and DualSense Mute button mappings.
- **ViGEm Driver IOCTL Optimization**: Immediate dispatch upon any axis/button change with resting heartbeat throttling, cutting kernel driver overhead by ~90%.

---

## [1.0.4] — 2026-08-14

### Highlights & Major Improvements
- **Dedicated Xbox 360 Controller Support & Native Visualizer**: Added automatic hardware detection and a dedicated Xbox 360 visualizer layout with authentic textures, responsive button highlights, and real-time stick tracking.
- **Micro-Precision Controller Button & Trigger Alignment**: Completely re-calibrated button overlay positions across both Xbox and PlayStation layouts based on original hardware templates.

### Detailed Changes

#### 🎮 Controller Visualizer & Layouts
- **Xbox 360 Visualizer**:
  - Implemented automatic layout switching when an Xbox 360 controller is connected.
  - Added physical trigger tab shapes (LT & RT) permanently visible on the top shoulder housing.
  - Centered left and right analog stick caps inside their respective circular well housings with zero offset drift.
  - Aligned all physical face buttons (A, B, X, Y), D-Pad wings, Back, Start, and Guide jewel indicators.
- **PlayStation 4 / DualShock 4 Visualizer**:
  - Re-aligned **L1** and **R1** bumper highlights directly onto the physical shoulder curves.
  - Aligned **L2** and **R2** trigger highlights precisely onto the top trigger extensions.
  - Aligned **SHARE** and **OPTIONS** pill button overlays directly beneath their respective labels.
  - Centered all four face action buttons (Triangle, Circle, Cross, Square) and directional D-Pad wings.

#### ⚙️ Input Engine & Hardware Detection
- Enhanced HID enumeration to automatically identify Xbox 360 controllers via Product IDs (`0x028E`, `0x028F`, `0x0291`, `0x02A1`, `0x0719`, `0x02A0`) and third-party vendor definitions.
- Refined battery telemetry and polling pipeline for low-latency wireless and USB gameplay.

---

## [1.0.3] — 2026-08-06

- Rebranded suite identity to **Drift Lift**.
- Improved button indicator sizes across Xbox Series X/S visualizers.
- Updated telemetry and crash log capture mechanisms (`%LOCALAPPDATA%\DriftLift\crash.log`).

---

## [1.0.2] — 2026-08-04

- Initial stable release.
