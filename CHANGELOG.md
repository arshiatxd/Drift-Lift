# Changelog — Drift Lift

All notable changes to **Drift Lift** will be documented in this file.

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

#### 📦 Packaging & Installation
- Updated Inno Setup installer wizard and standalone publish bundles to **v1.0.4**.
- Prepared full Microsoft Store package artifacts, manifests, and silent deployment configurations.

---

## [1.0.3] — 2026-08-06

- Rebranded suite identity to **Drift Lift**.
- Improved button indicator sizes across Xbox Series X/S visualizers.
- Updated telemetry and crash log capture mechanisms (`%LOCALAPPDATA%\DriftLift\crash.log`).

---

## [1.0.2] — 2026-08-04

- Initial multi-controller architecture with DualShock 4, DualSense, and Xbox controller support.
- ViGEmBus virtual controller output and HidHide double-input prevention integration.
- Real-time stick drift compensation engine and profile saving.
