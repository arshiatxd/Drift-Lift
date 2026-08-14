# 🚀 Microsoft Store Release Guide — Drift Lift v1.0.5

This document contains everything needed to publish **Drift Lift v1.0.5** to the **Microsoft Store** via Windows Partner Center as a Win32 application.

---

## 📦 1. Store Package Details

| Setting | Value / Details |
| :--- | :--- |
| **App Name** | `Drift Lift - Gamepad Controller Suite` |
| **Package Installer** | `DriftLift_Setup.exe` (Inno Setup Win32 Installer) |
| **Installer Type** | `Inno Setup` |
| **Silent Install Command** | `/VERYSILENT /NORESTART /SUPPRESSMSGBOXES` |
| **Silent Uninstall Command** | `/VERYSILENT /NORESTART /SUPPRESSMSGBOXES` |
| **Architecture** | `x64` |
| **Minimum OS** | `Windows 10 version 1809 (Build 17763)` |
| **Target OS** | `Windows 10 / Windows 11` |

---

## 📝 2. Store Listing Assets & Text

### **App Name**
`Drift Lift - Gamepad Controller Suite`

### **Short Description** (Max 258 chars)
> Professional gamepad calibration, real-time stick drift elimination, and button remapping suite for Xbox 360, Xbox One/Series, and PlayStation DualShock 4 / DualSense controllers.

### **Full Description**
```text
Take complete control of your gaming experience with Drift Lift — the ultimate Windows gamepad calibration, anti-drift, and remapping software suite.

Whether you suffer from analog stick drift on DualShock 4, DualSense PS5, Xbox 360, Xbox One, or Xbox Series X/S controllers, Drift Lift neutralizes stick drift in real-time with micro-precision deadzone calculation and dynamic center offset subtraction.

KEY FEATURES:
• REAL-TIME STICK DRIFT ELIMINATION: Automatic 1-click calibration measures resting analog noise and subtracts center drift down to 0.001 precision without widening in-game deadzones.
• HARDWARE-ACCURATE VISUALIZERS: Dedicated, authentic visual layouts for Xbox 360, Xbox One / Series, and PlayStation DualShock 4 controllers with real-time deflection tracking.
• FULL BUTTON & D-PAD REMAPPING: Rebind any button, trigger, shoulder bumper, or D-Pad direction with customizable actions.
• HIDHIDE DOUBLE-INPUT PREVENTION: Seamless integration with the HidHide driver prevents games and emulators from receiving duplicate physical/virtual inputs.
• BATTERY & TELEMETRY MONITORING: Real-time battery status and connection metrics for wireless Bluetooth and wired USB gamepads.
• PLAYSTATION LIGHTBAR RGB CONTROL: Complete control over LED colors and brightness on DualShock 4 and DualSense gamepads.
• VIBRATION MOTOR TESTER: Multi-pattern rumble motor tests (Heavy, Light, Pulse, Burst).
• CLEAN AMOLED THEME: Lightweight, responsive, and resource-efficient desktop UI built on modern .NET.

Designed for gamers, competitive esports players, and controller enthusiasts.
```

### **Search Keywords** (Up to 7 keywords)
1. `controller`
2. `stick drift`
3. `gamepad`
4. `playstation`
5. `xbox`
6. `deadzone`
7. `remap`

### **Category & Subcategory**
- **Category**: `Utilities & Tools`
- **Subcategory**: `Game Tools` or `System Utilities`

---

## 🛡️ 3. Privacy Policy & Terms (Mandatory for Store)

- **Privacy Policy URL**: `https://github.com/arshiatxd/Drift-Lift#privacy-policy` or host `PRIVACY.md`
- **Privacy Statement**:
  > Drift Lift does not collect, store, transmit, or share any personal identifiable information (PII), controller telemetry, or usage data. All calibration profiles and configuration files are stored entirely locally on the user's computer (`%LOCALAPPDATA%\DriftLift`).

---

## 📋 4. IARC Age Rating Questionnaire Responses

When filling the IARC questionnaire in Partner Center:
1. **Category**: `Utility, Productivity, Communication, or Other`
2. **User Content / Sharing**: `No`
3. **Location Sharing**: `No`
4. **Digital Purchases / In-App Purchases**: `No`
5. **Violence / Crude Humor / Profanity**: `None`
6. **Result**: Rated `Everyone` (ESRB: E, PEGI: 3, USK: 0).

---

## 🚀 5. Direct Download URL for Win32 Submission

When submitting via Win32 package URL in Partner Center:
- Use the direct binary release URL from GitHub Releases:
  `https://github.com/arshiatxd/Drift-Lift/releases/download/v1.0.5/DriftLift_Setup.exe`
- Or upload `DriftLift_Setup.exe` directly if prompted.
