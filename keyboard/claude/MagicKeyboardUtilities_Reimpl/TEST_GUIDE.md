# Magic Keyboard Utilities - Hướng Dẫn Kiểm Thử Chi Tiết

**Phiên bản**: 3.1.5.6  
**Ngày cập nhật**: 22/01/2026  
**Dành cho**: QA Testers, Developers

---

## 📋 Mục Lục

1. [Chuẩn Bị Môi Trường Test](#1-chuẩn-bị-môi-trường-test)
2. [Test Compilation & Build](#2-test-compilation--build)
3. [Test Unit Tests](#3-test-unit-tests)
4. [Test Khởi Động Ứng Dụng](#4-test-khởi-động-ứng-dụng)
5. [Test System Tray Icon](#5-test-system-tray-icon)
6. [Test Configuration](#6-test-configuration)
7. [Test Logging](#7-test-logging)
8. [Test Keyboard Hooks](#8-test-keyboard-hooks)
9. [Test Hotkeys](#9-test-hotkeys)
10. [Test Device Monitoring](#10-test-device-monitoring)
11. [Test Memory & Performance](#11-test-memory--performance)
12. [Test Edge Cases](#12-test-edge-cases)
13. [Test Checklist](#13-test-checklist)

---

## 1. Chuẩn Bị Môi Trường Test

### 1.1 Yêu Cầu Hệ Thống
- **OS**: Windows 10/11 (64-bit)
- **.NET SDK**: 8.0 trở lên
- **RAM**: Tối thiểu 4GB
- **Quyền**: Administrator (để test hooks)

### 1.2 Kiểm Tra .NET SDK
```powershell
dotnet --version
# Expected: 8.0.x hoặc cao hơn
```

### 1.3 Clone & Navigate
```powershell
cd D:\CTF\test\keyboard\source\claude\MagicKeyboardUtilities_Reimpl
```

### 1.4 Khởi Tạo Môi Trường
```powershell
# Restore dependencies
dotnet restore src\MagicKeyboardUtilities.Reimpl.sln
```

---

## 2. Test Compilation & Build

### 2.1 Build Debug
```powershell
dotnet build src\MagicKeyboardUtilities.Reimpl.sln -c Debug
```

**Expected Result**:
- ✅ Build succeeded
- ✅ 0 errors, 0 warnings
- ✅ Output: `MagicKeyboardUtilities.dll`

### 2.2 Build Release
```powershell
dotnet build src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

**Expected Result**:
- ✅ Build succeeded với optimizations
- ✅ Binary size < 500KB

### 2.3 Verify Output Files
```powershell
Get-ChildItem src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\ -Recurse -File
```

**Expected Files**:
- `MagicKeyboardUtilities.dll` (main app)
- `config.json` (default config)
- `Microsoft.Extensions.Logging.dll`
- `System.Text.Json.dll`

---

## 3. Test Unit Tests

### 3.1 Run All Tests
```powershell
dotnet test src\MagicKeyboardUtilities.Reimpl.sln -c Release --verbosity normal
```

**Expected Result**:
- ✅ Total: 11 tests
- ✅ Passed: 11
- ✅ Failed: 0
- ✅ Duration: < 1s

### 3.2 Run Specific Test Class
```powershell
# Test ConfigStore
dotnet test --filter "FullyQualifiedName~ConfigTests"

# Test RemappingEngine
dotnet test --filter "FullyQualifiedName~RemappingEngineTests"

# Test ActionDispatcher
dotnet test --filter "FullyQualifiedName~ActionDispatcherTests"
```

### 3.3 Test Coverage Analysis
```powershell
# Run với detailed output
dotnet test src\MagicKeyboardUtilities.Reimpl.sln --logger "console;verbosity=detailed"
```

**Test Breakdown**:
- **ConfigTests**: Load, Save, Default config
- **RemappingEngineTests**: Mapping lookup, fallback, empty checks
- **ActionDispatcherTests**: Action execution, unknown actions

---

## 4. Test Khởi Động Ứng Dụng

### 4.1 First Launch (Debug)
```powershell
dotnet run --project src\MagicKeyboardUtilities.App -c Debug
```

**Expected Behavior**:
1. ✅ Console output: "=== Magic Keyboard Utilities Reimplementation ==="
2. ✅ Version displayed: 3.1.5.6
3. ✅ Config loaded from `config.json`
4. ✅ Tray icon appears (system tray)
5. ✅ No errors/exceptions
6. ✅ App runs in background

### 4.2 Check Process
```powershell
Get-Process dotnet | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-1) }
```

**Expected**:
- ✅ Process running
- ✅ Working Set < 100MB
- ✅ Handles < 1000

### 4.3 Verify Logs Created
```powershell
Get-Content src\MagicKeyboardUtilities.App\bin\Debug\net8.0-windows\logs\app.log -Tail 20
```

**Expected Log Entries**:
```
[Information] [Program] === Magic Keyboard Utilities Reimplementation ===
[Information] [Program] Version: 3.1.5.6
[Information] [ConfigStore] Configuration loaded from ...
[Information] [AppHost] === Starting MagicKeyboardUtilities Reimplementation ===
[Information] [RemappingEngine] Loaded 3 key mappings
[Information] [HiddenMessageWindow] Hidden message window created, handle: XXXXX
[Information] [TrayIconController] Tray icon created successfully
[Information] [AppHost] Hooks disabled by default (safety)
[Information] [AppHost] === Application started successfully ===
```

### 4.4 Single Instance Test
```powershell
# Launch app lần 1
dotnet run --project src\MagicKeyboardUtilities.App -c Release

# Mở terminal mới, launch lần 2 (nên fail)
dotnet run --project src\MagicKeyboardUtilities.App -c Release
```

**Expected**:
- ✅ Lần 1: App starts
- ✅ Lần 2: Error message "Application is already running"
- ✅ Only 1 instance running

---

## 5. Test System Tray Icon

### 5.1 Verify Icon Appears
**Manual Test**:
1. Launch app: `dotnet run --project src\MagicKeyboardUtilities.App -c Release`
2. Check system tray (bottom-right taskbar)
3. ✅ Icon visible với text "🎹 Magic Keyboard"

### 5.2 Test Context Menu
**Right-click tray icon**:
- ✅ Menu items appear:
  - `☐ Enable Hooks`
  - `⚙ Settings`
  - `ℹ About`
  - `✖ Exit`

### 5.3 Test "About" Dialog
1. Right-click icon → "About"
2. **Expected**:
   - ✅ Dialog shows version 3.1.5.6
   - ✅ Shows traceability info
   - ✅ "OK" button closes dialog

### 5.4 Test "Exit"
1. Right-click icon → "Exit"
2. **Expected**:
   - ✅ App closes gracefully
   - ✅ Process terminates
   - ✅ Logs show: `[Information] [AppHost] === Shutting down ===`

---

## 6. Test Configuration

### 6.1 Test Default Config
```powershell
# Delete config if exists
Remove-Item src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\config.json -ErrorAction SilentlyContinue

# Launch app (should create default)
dotnet run --project src\MagicKeyboardUtilities.App -c Release
```

**Expected**:
- ✅ `config.json` created
- ✅ Contains default mappings (F13→VolumeUp, F14→VolumeDown, F15→Mute)
- ✅ Hooks disabled by default

### 6.2 Test Config Load
```powershell
# Verify config exists
Get-Content src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\config.json | ConvertFrom-Json
```

**Expected JSON**:
```json
{
  "KeyMappings": [
    {
      "SourceVirtualKeyCode": 124,
      "TargetVirtualKeyCode": 175,
      "Description": "F13 -> Volume Up"
    },
    ...
  ],
  "EnableHooks": false,
  "Hotkeys": []
}
```

### 6.3 Test Config Modify
1. **Edit** `config.json` (add new mapping):
```json
{
  "SourceVirtualKeyCode": 127,
  "TargetVirtualKeyCode": 176,
  "Description": "F16 -> Next Track"
}
```
2. **Restart app**
3. **Expected**:
   - ✅ Logs show: "Loaded 4 key mappings"
   - ✅ New mapping active

### 6.4 Test Invalid Config
1. Corrupt `config.json` (invalid JSON)
2. Launch app
3. **Expected**:
   - ✅ Error logged: "[Error] Failed to load configuration"
   - ✅ App uses default config
   - ✅ App still runs (graceful degradation)

---

## 7. Test Logging

### 7.1 Check Log File Created
```powershell
Test-Path src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\logs\app.log
```

**Expected**: `True`

### 7.2 Verify Log Content
```powershell
Get-Content src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\logs\app.log
```

**Expected Format**:
- ✅ Timestamp: `[2026-01-22 HH:MM:SS.mmm]`
- ✅ Level: `[Information]`, `[Warning]`, `[Error]`
- ✅ Source: `[Program]`, `[AppHost]`, etc.
- ✅ Message: Clear and descriptive

### 7.3 Test Log Rotation
```powershell
# Generate many log entries
for ($i=1; $i -le 1000; $i++) {
    # Trigger log writes (restart app multiple times)
}
```

**Expected**:
- ✅ Old logs archived with timestamp
- ✅ Current log < 10MB

---

## 8. Test Keyboard Hooks

### 8.1 Enable Hooks via Tray Menu
**Manual Test**:
1. Launch app
2. Right-click tray icon → "Enable Hooks"
3. **Expected**:
   - ✅ Menu item changes to `☑ Enable Hooks` (checked)
   - ✅ Logs show: `[Information] [KeyboardHookService] Keyboard hook installed, ID: XXXXX`

### 8.2 Test Key Remapping
**Test F13 → Volume Up**:
1. Enable hooks
2. Press **F13 key** on Magic Keyboard
3. **Expected**:
   - ✅ Volume increases
   - ✅ Logs show: `[Debug] Key down: F13 (VK 124) → remapped to VK 175`
   - ✅ Original F13 blocked

**Test F14 → Volume Down**:
1. Press **F14 key**
2. **Expected**:
   - ✅ Volume decreases

**Test F15 → Mute**:
1. Press **F15 key**
2. **Expected**:
   - ✅ Audio muted/unmuted

### 8.3 Test Non-Remapped Keys
1. Press normal keys (A, B, 1, 2, etc.)
2. **Expected**:
   - ✅ Keys work normally
   - ✅ No logs for unmapped keys
   - ✅ No performance impact

### 8.4 Disable Hooks
1. Right-click tray icon → "Enable Hooks" (uncheck)
2. **Expected**:
   - ✅ Hooks disabled
   - ✅ Logs show: `[Information] [KeyboardHookService] Keyboard hook removed`
   - ✅ F13/F14/F15 work as original keys

---

## 9. Test Hotkeys

### 9.1 Add Hotkey to Config
**Edit** `config.json`:
```json
{
  "Hotkeys": [
    {
      "KeyCode": 112,
      "ModifierKeys": "Control,Shift",
      "ActionName": "ShowAbout"
    }
  ]
}
```

### 9.2 Test Hotkey Trigger
1. Restart app
2. Press **Ctrl+Shift+F1**
3. **Expected**:
   - ✅ About dialog appears
   - ✅ Logs show: `[Information] Hotkey triggered: Ctrl+Shift+F1 → ShowAbout`

### 9.3 Test Invalid Hotkey
1. Add invalid action: `"ActionName": "InvalidAction"`
2. Restart app
3. Press hotkey
4. **Expected**:
   - ✅ Warning logged: "Unknown action: InvalidAction"
   - ✅ No crash

---

## 10. Test Device Monitoring

### 10.1 Check DeviceMonitor Stub
```powershell
# Check logs for device monitor messages
Get-Content src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\logs\app.log | Select-String "DeviceMonitor"
```

**Expected**:
- ✅ Logs show: `[Information] [DeviceMonitor] Device monitoring is a stub (requires WMI/WinRT implementation)`

### 10.2 Test USB Device Connect/Disconnect
**Manual Test** (if implemented):
1. Unplug Magic Keyboard
2. **Expected**: Logs show device disconnected
3. Plug back in
4. **Expected**: Logs show device reconnected

---

## 11. Test Memory & Performance

### 11.1 Memory Usage
```powershell
# Launch app
dotnet run --project src\MagicKeyboardUtilities.App -c Release

# Check memory
Get-Process dotnet | Select-Object Name, WorkingSet64, PrivateMemorySize64
```

**Expected**:
- ✅ Working Set < 50MB (idle)
- ✅ Working Set < 100MB (with hooks enabled)

### 11.2 CPU Usage
```powershell
Get-Process dotnet | Select-Object Name, CPU
```

**Expected**:
- ✅ CPU < 1% (idle)
- ✅ CPU < 5% (active typing)

### 11.3 Handle Leaks
```powershell
# Launch app, run for 10 minutes
Start-Sleep -Seconds 600

# Check handles
Get-Process dotnet | Select-Object Name, Handles
```

**Expected**:
- ✅ Handles stable (< 1000)
- ✅ No continuous increase

### 11.4 Long-Running Test
1. Launch app
2. Leave running overnight
3. **Expected**:
   - ✅ App still running
   - ✅ No crashes
   - ✅ Log file size reasonable
   - ✅ Memory stable

---

## 12. Test Edge Cases

### 12.1 Test Rapid Key Presses
1. Enable hooks
2. Rapidly press F13 (10 times/second)
3. **Expected**:
   - ✅ All presses processed
   - ✅ No dropped events
   - ✅ No crashes

### 12.2 Test Hook Crash Recovery
1. Simulate hook failure (corrupt config)
2. **Expected**:
   - ✅ Error logged
   - ✅ App continues running
   - ✅ Hooks disabled automatically

### 12.3 Test Config Save During Runtime
1. Launch app
2. Modify config file while running
3. Restart app
4. **Expected**:
   - ✅ New config loaded
   - ✅ No corruption

### 12.4 Test Low Disk Space
1. Fill disk to < 100MB
2. Launch app
3. **Expected**:
   - ✅ Warning logged: "Low disk space"
   - ✅ App continues (limited logging)

### 12.5 Test Multiple Monitors
1. Connect 2+ monitors
2. Launch app
3. **Expected**:
   - ✅ Tray icon appears on primary monitor
   - ✅ Dialogs centered correctly

---

## 13. Test Checklist

### ✅ Pre-Release Checklist

#### Build & Compilation
- [ ] Debug build succeeds (0 errors)
- [ ] Release build succeeds (0 errors)
- [ ] All dependencies resolved
- [ ] Output files present

#### Unit Tests
- [ ] All 11 tests pass
- [ ] Config tests pass (3/3)
- [ ] Remapping tests pass (5/5)
- [ ] Action tests pass (3/3)

#### Application Startup
- [ ] App launches successfully
- [ ] Single instance enforced
- [ ] Tray icon appears
- [ ] Default config created
- [ ] Logs initialized

#### Tray Icon & UI
- [ ] Icon visible in system tray
- [ ] Context menu functional
- [ ] About dialog displays correctly
- [ ] Exit works gracefully

#### Configuration
- [ ] Default config loads
- [ ] Custom config loads
- [ ] Invalid config handled
- [ ] Config persists across restarts

#### Logging
- [ ] Log file created
- [ ] Log format correct
- [ ] All levels logged (Info, Warning, Error)
- [ ] No sensitive data logged

#### Keyboard Hooks
- [ ] Hooks enable/disable via tray
- [ ] F13 → Volume Up works
- [ ] F14 → Volume Down works
- [ ] F15 → Mute works
- [ ] Non-remapped keys unaffected
- [ ] Hooks disabled on exit

#### Hotkeys
- [ ] Hotkeys registered
- [ ] Hotkeys trigger actions
- [ ] Invalid hotkeys handled

#### Performance
- [ ] Memory < 100MB
- [ ] CPU < 5%
- [ ] No handle leaks
- [ ] Stable over time

#### Edge Cases
- [ ] Rapid key presses handled
- [ ] Crash recovery works
- [ ] Low resources handled
- [ ] Multi-monitor support

---

## 📝 Test Report Template

### Test Session
- **Date**: YYYY-MM-DD
- **Tester**: [Name]
- **Version**: 3.1.5.6
- **OS**: Windows [10/11]
- **.NET SDK**: [Version]

### Test Results
| Test Category | Total | Passed | Failed | Skipped |
|---------------|-------|--------|--------|---------|
| Build         | X     | X      | X      | X       |
| Unit Tests    | 11    | 11     | 0      | 0       |
| Startup       | X     | X      | X      | X       |
| Tray Icon     | X     | X      | X      | X       |
| Configuration | X     | X      | X      | X       |
| Logging       | X     | X      | X      | X       |
| Hooks         | X     | X      | X      | X       |
| Hotkeys       | X     | X      | X      | X       |
| Performance   | X     | X      | X      | X       |
| Edge Cases    | X     | X      | X      | X       |

### Issues Found
1. **[Severity]** [Issue description]
   - **Steps to reproduce**: ...
   - **Expected**: ...
   - **Actual**: ...
   - **Logs**: ...

### Sign-Off
- [ ] All critical tests passed
- [ ] All blockers resolved
- [ ] Ready for release

**Approved by**: _______________  
**Date**: _______________

---

## 🔧 Troubleshooting

### App Won't Start
```powershell
# Check .NET SDK
dotnet --version

# Check process conflicts
Get-Process | Where-Object { $_.ProcessName -like "*MagicKeyboard*" }

# Check logs
Get-Content src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\logs\app.log
```

### Hooks Not Working
1. Run as **Administrator**
2. Check config: `"EnableHooks": true`
3. Check logs for hook installation errors
4. Verify keyboard device connected

### Tests Failing
```powershell
# Clean and rebuild
dotnet clean
dotnet build -c Release
dotnet test
```

### Log File Issues
```powershell
# Check permissions
Get-Acl src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\logs\
```

---

**END OF TEST GUIDE**
