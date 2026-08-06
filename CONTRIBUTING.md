# Contributing to Drift Lift

Thank you for considering contributing to Drift Lift! This document outlines the process and guidelines for contributing.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Code Style](#code-style)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

---

## 📜 Code of Conduct

By participating in this project, you agree to maintain a respectful and constructive environment for everyone. Harassment, discrimination, and toxic behavior will not be tolerated.

---

## 🤝 How Can I Contribute?

### Bug Reports

Use the [issue tracker](https://github.com/arshiatxd/Drift-Lift/issues) with the **Bug Report** template. Include:
- Windows version (`winver`)
- Controller model and connection type (USB / Bluetooth)
- Steps to reproduce the issue
- Contents of `crash.log` from the install directory
- Screenshots or screen recordings if applicable

### Feature Requests

Open an issue with the **Feature Request** template. Describe:
- The problem you're trying to solve
- Your proposed solution
- Any alternatives you've considered

### Code Contributions

1. Check [existing issues](https://github.com/arshiatxd/Drift-Lift/issues) to avoid duplicates
2. Comment on the issue to indicate you're working on it
3. Follow the [Development Setup](#development-setup) guide
4. Submit a Pull Request referencing the issue (`Closes #123`)

---

## 🛠️ Development Setup

### Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/) | Latest |
| [ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases) | Latest |
| [HidHide Driver](https://github.com/nefarius/HidHide/releases) | Latest (optional) |
| Windows 10 / 11 x64 | — |

### Clone and Build

```bash
git clone https://github.com/arshiatxd/Drift-Lift.git
cd Drift-Lift
dotnet restore
dotnet build
dotnet run
```

### Project Structure

```
Drift-Lift/
├── Core/           # Input pipeline, calibration engine, virtual output
├── ViewModels/     # MVVM ViewModels (CommunityToolkit.Mvvm)
├── Views/          # WPF pages, user controls, and popup windows
├── Themes/         # ResourceDictionary theme files
├── Services/       # HidHide installer, settings manager
└── Models/         # Data models, profiles, calibration state
```

---

## 📝 Pull Request Guidelines

1. **Branch from `main`** — name your branch `feature/description` or `fix/description`
2. **One concern per PR** — keep changes focused and reviewable
3. **Write clean commits** — use clear, imperative commit messages: `Add deadzone visualizer to CalibrateView`
4. **Test your changes** — verify with at least one physical controller before submitting
5. **No commented-out code** — remove dead code before submitting
6. **Update docs** — if your change affects user-facing behavior, update `README.md`

### PR Title Format

```
[Type] Short description

Types: feat | fix | refactor | perf | style | docs | chore
```

---

## 🎨 Code Style

- **C#**: Follow standard Microsoft C# conventions
- **XAML**: Group properties logically (layout → appearance → behavior)
- **No magic numbers**: Extract constants with descriptive names
- **Section indicators**: Use `// ##== Section Name ==##` for logical groupings in C# and `<!-- ##== Section ==## -->` in XAML
- **Error handling**: Never swallow exceptions silently without at minimum logging them

---

## 💡 Suggesting Features

Before opening a feature request, check:
- Is this already in the [roadmap](https://github.com/arshiatxd/Drift-Lift/issues)?
- Has someone else already requested it?

Good feature requests explain the **use case**, not just the implementation idea.

---

Thank you for helping make Drift Lift better! 🎮
