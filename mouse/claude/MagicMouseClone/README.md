# MagicMouseClone

A reimplementation of MagicMouseUtilities for Windows, providing enhanced gesture support for Apple Magic Mouse.

## Evidence-Based Rebuild

This project is based on reverse engineering analysis of MagicMouseUtilities.exe (v3.1.5.6).
See `magicmouseutilities_rebuild.md` for detailed analysis and evidence.

## Architecture

- **Core Library** (`MagicMouseClone.Core`): Business logic, gesture detection, action mapping
- **Windows App** (`MagicMouseClone.App`): WinForms tray application
- **Tests** (`MagicMouseClone.Tests`): xUnit test suite

## Key Features

- ✅ System tray integration
- ✅ Gesture recognition engine (scroll, swipe, pinch, rotate)
- ✅ Action mapping (keyboard shortcuts, mouse events, app launch)
- ✅ Profile management
- ✅ JSON-based configuration
- ✅ Fake device backend for testing/demo

## Requirements

- .NET 8.0 SDK
- Windows 10/11

## Building

```powershell
.\build.ps1
```

Or manually:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

## Running

```powershell
.\run_demo.ps1
```

Or manually:

```powershell
dotnet run --project src\MagicMouseClone.App
```

## Testing

See `tester_guide.md` for detailed testing instructions.

## Configuration

Configuration stored in: `%APPDATA%\MagicMouseClone\`

- `config.json` - Application settings
- `Profiles\*.json` - Gesture-to-action mappings

## Current Status

✅ Core architecture implemented  
✅ Gesture detection (simplified)  
✅ Action mapping framework  
✅ Fake device backend for demo  
⚠️ Real Bluetooth/HID integration not implemented  
⚠️ Advanced gesture algorithms need tuning  

## Legal Notice

This is an educational reimplementation. "Magic Mouse" is a registered trademark of Apple Inc.
This project is not affiliated with Apple Inc. or Magic Utilities Pty Ltd.

## License

MIT License - For educational and personal use only.
