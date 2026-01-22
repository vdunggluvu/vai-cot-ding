# MagicKeyboardUtilities Clone

A re-implementation of the magic keyboard utility flow, focusing on safe, transparent input management and device detection.

## Objective
To act as a functional clone of the specified utility, running in the background with a system tray icon, providing keyboard remapping and hotkey capabilities.

## Structure
- `src/`: Source code.
- `docs/`: Design and traceability documentation.
- `scripts/`: Build and run scripts.
- `config.json`: Configuration file.

## Build & Run
Prerequisite: .NET 8.0 SDK.

Run from the `MagicKeyboardUtilities_Reimpl` directory:

**Build:**
```powershell
./scripts/build.ps1
```

**Run:**
```powershell
./scripts/run.ps1
```

## Configuration
Edit `config.json` to change settings.
- `enableHooks`: Set to `true` to activate keyboard remapping.
- `remapping`: List of from/to virtual key codes.

## Safety
- Network is DISABLED by default.
- Hooks are DISABLED by default.
- No registry persistence is written.
