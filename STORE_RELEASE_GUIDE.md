# 🚀 Microsoft Store Release Guide — Drift Lift v1.0.2

This document provides everything needed to publish **Drift Lift** to the **Microsoft Store** via Windows Partner Center.

---

## 📦 1. Pre-Packaged Release Files Ready for Store Submission

| File / Asset | Location | Description |
| :--- | :--- | :--- |
| **Win32 Store Installer** | `C:\Users\Parsian\Desktop\DriftLift_Setup.exe` | Ready for direct Win32 Store App submission |
| **AppX Manifest** | `Package.appxmanifest` | Packaged app manifest (`arshiatxd.DriftLift`) |
| **Store Logos & Tiles** | `Assets/Store/` | Complete 44x44, 150x150, 310x150, 50x50, 620x300 logos |

---

## 📝 2. Ready-to-Use Store Listing Copy

Copy and paste these exact details directly into your Microsoft Partner Center Store submission forms:

### **App Name**
`Drift Lift - Gamepad Controller Suite`

### **Short Description** (Maximum 258 characters)
> Professional gamepad calibration, real-time anti-drift engine, dual controller remapping (Xbox & PlayStation DS4/PS5), custom deadzones, macros, and double-input protection suite.

### **Full Description**
```text
Take complete control of your gaming experience with Drift Lift — the ultimate Windows gamepad calibration, anti-drift, and remapping software suite.

Whether you suffer from analog stick drift on DualShock 4, DualSense PS5, Xbox One, or Xbox Series X/S controllers, Drift Lift neutralizes stick drift in real-time with micro-precision deadzone calculation and dynamic center offset subtraction.

KEY FEATURES:
• REAL-TIME STICK DRIFT ELIMINATION: Automatic 1-click input analysis measures resting noise and offset variance down to 0.001 precision.
• DUAL CONTROLLER REMAPPING: Connect both Xbox and PlayStation controllers simultaneously and swap between P1/P2 indicators instantly.
• D-PAD & ACTION BUTTON REMAPPING: Completely customizable button remapping for D-Pad directions, triggers, bumpers, and face buttons.
• HIDHIDE DOUBLE INPUT FIX: Built-in integration with HidHide driver to prevent double-input conflicts in games.
• ANALOG STICK & CIRCULARITY ANALYZER: Visual circularity error calculation, live axis monitoring, and outer boundary calibration.
• VIBRATION MOTOR TESTER: Dual motor test routines (Heavy, Light, Burst, Pulse) with custom duration settings.
• MODERN AMOLED DARK RED & LIGHT THEMES: Fluid WPF UI with customizable glassmorphic themes, smartphone battery visualizer, and custom audio cues.

Designed for gamers, competitive esports players, and controller enthusiasts.
```

### **Search Keywords** (Up to 7 keywords)
`controller`, `stick drift`, `gamepad`, `playstation`, `xbox`, `deadzone`, `remap`

### **Category & Subcategory**
- **Category**: `Utilities & Tools`
- **Subcategory**: `Game Tools` or `System Utilities`

### **Copyright & Author Info**
- **Publisher Display Name**: `arshiatxd`
- **Copyright**: `© 2026 arshiatxd. All rights reserved.`
- **Support Contact**: `arshiatxd Developer Support`

---

## ⚙️ 3. Silent Installer Parameters (For Win32 Store Submission)

When submitting `DriftLift_Setup.exe` directly via Microsoft Partner Center Win32 Submission:

- **Installer Type**: `Inno Setup`
- **Silent Install Parameters**: `/VERYSILENT /NORESTART /SUPPRESSMSGBOXES`
- **Silent Uninstall Parameters**: `/VERYSILENT /NORESTART /SUPPRESSMSGBOXES`
- **Install Architecture**: `x64`
- **Minimum OS Version**: `Windows 10 version 1809 (10.0.17763)`

---

## 🌐 4. Partner Center Submission Step-by-Step

1. **Sign in to Microsoft Partner Center**:
   Navigate to [partner.microsoft.com/dashboard](https://partner.microsoft.com/dashboard).
2. **Create a Developer Account** *(If not already created)*:
   Individual account fee is ~$19 USD (one-time payment).
3. **Create a New App**:
   Click **Apps and Games** ➔ **Create a new app** ➔ Reserve name **Drift Lift**.
4. **Choose Package / App Type**:
   - Select **Upload Win32 Installer** (`DriftLift_Setup.exe`) or **Upload MSIX package**.
5. **Fill App Submission Listing**:
   Paste the short description, full description, search keywords, and logos from `Assets/Store/`.
6. **Submit for Certification**:
   Microsoft automated testing takes ~24 to 48 hours. Once approved, **Drift Lift** will be live globally on the Windows 10 & 11 Microsoft Store!

---
*Created by arshiatxd · Drift Lift Release Suite*
