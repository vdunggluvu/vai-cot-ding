# MagicMouseClone — Tester Guide

**Version:** 1.0  
**Target Build:** .NET 8 Release  
**Last Updated:** January 22, 2026

---

## 1. Objectives & Scope

### Testing Goals

This guide enables testers to:
1. **Verify Build Integrity:** Ensure application builds and runs without errors
2. **Validate Core Functionality:** Test gesture detection, action mapping, configuration
3. **Assess UI/UX:** Evaluate tray integration, notifications, responsiveness
4. **Identify Edge Cases:** Discover bugs, crashes, unexpected behavior
5. **Measure Performance:** Check CPU/memory usage, responsiveness

### Scope

**In Scope:**
- ✅ Application lifecycle (startup, shutdown, single instance)
- ✅ System tray integration
- ✅ Fake device backend (demo mode)
- ✅ Gesture detection framework
- ✅ Action mapping logic
- ✅ Configuration save/load
- ✅ Profile management

**Out of Scope:**
- ❌ Real Bluetooth device integration (not implemented)
- ❌ Actual SendInput execution (demo mode only)
- ❌ HID communication with hardware

---

## 2. Environment Matrix

Test on the following configurations:

| OS | Version | DPI | Multi-Monitor | Priority |
|----|---------|-----|---------------|----------|
| Windows 11 | 23H2 | 100% | No | **HIGH** |
| Windows 11 | 23H2 | 150% | Yes | **MEDIUM** |
| Windows 10 | 22H2 | 100% | No | **MEDIUM** |
| Windows 10 | 22H2 | 125% | Yes | **LOW** |

**Additional Considerations:**
- User privileges: Standard user (recommended), Admin (if needed)
- Antivirus: Windows Defender enabled
- Other tray apps: Test with multiple tray icons present

---

## 3. Build & Run Instructions

### Prerequisites

1. Install **.NET 8.0 SDK:**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify: `dotnet --version` (should show 8.0.x)

2. Clone or extract project to `D:\Test\MagicMouseClone\`

### Build Steps

```powershell
cd D:\Test\MagicMouseClone
.\build.ps1
```

**Expected Output:**
```
============================================
 MagicMouseClone Build Script
============================================

[1/4] Skipping clean
[2/4] Restoring packages...
[3/4] Building (Release)...
✅ Build succeeded
[4/4] Skipping tests

============================================
✅ Build completed successfully!
============================================
```

**If build fails:**
- Check .NET SDK version
- Ensure no file locks (close Visual Studio)
- Delete `bin/` and `obj/` folders, retry

### Run Application

**Option 1: Script**
```powershell
.\run_demo.ps1
```

**Option 2: Manual**
```powershell
dotnet run --project src\MagicMouseClone.App -c Release
```

**Option 3: Executable**
```powershell
.\src\MagicMouseClone.App\bin\Release\net8.0-windows\MagicMouseClone.exe
```

---

## 4. Observation & Debug

### Log Locations

- **Console Output:** Visible when running from terminal
- **Application Logs:** Not implemented (uses console logger)
- **Crash Dumps:** Check Windows Event Viewer → Application logs

### Config Location

```
%APPDATA%\MagicMouseClone\
├── config.json           (Application settings)
└── Profiles\
    ├── Default.json      (Default gesture mappings)
    ├── Gaming.json       (Custom profile)
    └── ...
```

**To reset configuration:**
```powershell
Remove-Item -Recurse "$env:APPDATA\MagicMouseClone"
```

### Expected Tray Behavior

1. **On Launch:**
   - Blue tray icon appears in system tray
   - Balloon tip: "Application started. Running in system tray."
   - Main window minimizes immediately

2. **Device Connection:**
   - Balloon tip: "Device Connected - Fake Magic Mouse is now connected."
   - Icon remains blue (no state change in current version)

3. **Gestures:**
   - Window shows gesture log if visible
   - Console logs gestures: `[12:34:56] SwipeRight`

### Troubleshooting

**Problem:** Tray icon doesn't appear  
**Solution:** Check Task Manager → More details → Background processes → Look for "MagicMouseClone"

**Problem:** "Already running" message  
**Solution:** Kill existing process: `taskkill /IM MagicMouseClone.exe /F`

**Problem:** No gestures detected  
**Solution:** Expected behavior - FakeDeviceBackend generates gestures every 3-5s automatically

---

## 5. Test Cases

### TC-01: Application Startup

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch `MagicMouseClone.exe` | Application starts, no errors |  |
| 2 | Check system tray | Blue tray icon appears |  |
| 3 | Check taskbar | Application not in taskbar |  |
| 4 | Check balloon tip | "Application started" notification |  |
| 5 | Wait 2-3 seconds | "Device Connected" notification |  |

**Priority:** CRITICAL

---

### TC-02: Single Instance Check

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch first instance | App starts normally |  |
| 2 | Launch second instance | MessageBox: "MagicMouseClone is already running." |  |
| 3 | Click OK on MessageBox | Second instance exits |  |
| 4 | Check Task Manager | Only 1 process running |  |

**Priority:** HIGH

---

### TC-03: Tray Icon Interaction

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Right-click tray icon | Context menu appears |  |
| 2 | Verify menu items | "Show Window", "Device Status", "Exit" present |  |
| 3 | Click "Show Window" | Main window appears |  |
| 4 | Double-click tray icon | Main window appears (if hidden) |  |

**Priority:** HIGH

---

### TC-04: Main Window

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Show main window | Window displays with "Status: Running" |  |
| 2 | Check device label | "Device: Fake Magic Mouse (Connected)" |  |
| 3 | Check gesture log | TextBox present, scrollable |  |
| 4 | Wait 5-10 seconds | Gestures appear in log |  |
| 5 | Minimize window | Window hides to tray (not taskbar) |  |
| 6 | Click [X] close button | Window hides to tray (does NOT exit) |  |

**Priority:** HIGH

---

### TC-05: Gesture Detection

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Show main window | - |  |
| 2 | Wait 30 seconds | At least 3-5 gestures appear in log |  |
| 3 | Check gesture types | SwipeUp, SwipeDown, SwipeLeft, SwipeRight visible |  |
| 4 | Check timestamps | Format: `[HH:mm:ss]` |  |

**Priority:** HIGH  
**Note:** FakeDeviceBackend generates random gestures every 3-5 seconds

---

### TC-06: Device Status

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Right-click tray icon | - |  |
| 2 | Click "Device Status" | MessageBox appears |  |
| 3 | Check message content | Contains: "Device: Fake Magic Mouse", "State: Connected", "Battery: 85%" |  |
| 4 | Click OK | MessageBox closes |  |

**Priority:** MEDIUM

---

### TC-07: Configuration Persistence

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch app | - |  |
| 2 | Navigate to `%APPDATA%\MagicMouseClone\` | Folder exists |  |
| 3 | Check `config.json` | File exists, valid JSON |  |
| 4 | Check `Profiles\Default.json` | File exists, valid JSON |  |
| 5 | Exit app | - |  |
| 6 | Relaunch app | App loads without errors |  |

**Priority:** HIGH

---

### TC-08: Graceful Shutdown

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Right-click tray icon | - |  |
| 2 | Click "Exit" | - |  |
| 3 | Check tray | Icon disappears |  |
| 4 | Check Task Manager | No MagicMouseClone process |  |
| 5 | Check config folder | Files still exist (not deleted) |  |

**Priority:** CRITICAL

---

### TC-09: Invalid Configuration Recovery

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Exit app | - |  |
| 2 | Edit `config.json`, insert syntax error | - |  |
| 3 | Launch app | App starts, no crash |  |
| 4 | Check `config.json` | File overwritten with defaults |  |

**Priority:** MEDIUM

---

### TC-10: Performance - Idle

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch app | - |  |
| 2 | Minimize to tray | - |  |
| 3 | Open Task Manager → Details | - |  |
| 4 | Find MagicMouseClone.exe | - |  |
| 5 | Check CPU usage | < 2% on average |  |
| 6 | Check memory usage | < 50 MB |  |

**Priority:** HIGH  
**Note:** FakeDeviceBackend uses timer, slight CPU spikes expected

---

### TC-11: Performance - Active

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Show main window | - |  |
| 2 | Let gestures generate for 60 seconds | - |  |
| 3 | Check Task Manager | CPU < 5%, Memory < 60 MB |  |
| 4 | Check UI responsiveness | Window resizes smoothly, no lag |  |

**Priority:** MEDIUM

---

### TC-12: Multi-Monitor Handling

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch on primary monitor | - |  |
| 2 | Show main window | - |  |
| 3 | Move window to secondary monitor | Window moves correctly |  |
| 4 | Minimize to tray | Tray icon on primary taskbar |  |
| 5 | Show window again | Window appears on last known monitor |  |

**Priority:** LOW  
**Requirements:** Multi-monitor setup

---

### TC-13: High DPI Scaling

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Set display scaling to 150% | - |  |
| 2 | Launch app | - |  |
| 3 | Show main window | Text readable, not blurry |  |
| 4 | Check tray icon | Icon not pixelated |  |

**Priority:** MEDIUM  
**Requirements:** High DPI display or settings

---

### TC-14: Long-Running Stability

| Step | Action | Expected Result | Pass/Fail |
|------|--------|-----------------|-----------|
| 1 | Launch app | - |  |
| 2 | Let run for 1 hour | - |  |
| 3 | Check memory usage | No significant increase (< 100 MB) |  |
| 4 | Check gesture log | Continues to populate |  |
| 5 | Exit app | Closes gracefully |  |

**Priority:** MEDIUM  
**Requirements:** Dedicated test time

---

## 6. Negative / Edge Cases

### Edge-01: Corrupt Profile File

**Steps:**
1. Exit app
2. Edit `Profiles\Default.json`, delete half the content
3. Launch app

**Expected:** App starts, Default profile recreated with default bindings

---

### Edge-02: No Write Permissions

**Steps:**
1. Remove write permissions from `%APPDATA%\MagicMouseClone\`
2. Launch app

**Expected:** Error logged to console, app continues running (in-memory config)

---

### Edge-03: Rapid Start/Stop

**Steps:**
1. Launch app
2. Immediately click "Exit" (within 1 second)

**Expected:** App exits cleanly, no zombie process

---

### Edge-04: System Shutdown During Execution

**Steps:**
1. Launch app
2. Initiate Windows shutdown (Start → Power → Shut down)

**Expected:** App exits within 5 seconds, config saved

---

### Edge-05: Invalid Profile Switch

**Steps:**
1. App running
2. Manually create empty file: `Profiles\Broken.json`
3. Attempt to load "Broken" profile (future feature)

**Expected:** Error logged, profile not loaded, app continues with current profile

---

## 7. Bug Report Template

When reporting bugs, include:

```markdown
**Title:** [Brief description]

**Environment:**
- OS: Windows 11 23H2
- .NET Version: 8.0.15
- DPI: 150%
- Multi-Monitor: Yes/No

**Steps to Reproduce:**
1. Launch app
2. ...
3. ...

**Expected Behavior:**
[What should happen]

**Actual Behavior:**
[What actually happens]

**Screenshots:**
[If applicable]

**Console Output:**
```
[Paste relevant log lines]
```

**Config Files:**
[Attach config.json if relevant]

**Severity:**
- [ ] Critical (crash, data loss)
- [ ] High (feature broken)
- [ ] Medium (minor issue)
- [ ] Low (cosmetic)
```

---

## 8. Traceability: Test ↔ Flow

This table maps test cases to flows documented in `magicmouseutilities_rebuild.md`:

| Test Case | Maps to Flow | Evidence Confidence |
|-----------|-------------|---------------------|
| TC-01, TC-02 | Flow 1: Startup & Initialization | HIGH |
| TC-07 | Flow 2: Configuration Load/Save | HIGH |
| TC-06 | Flow 3: Device Detection | MEDIUM (simulated) |
| TC-05 | Flow 4: Input Capture | MEDIUM (simulated) |
| TC-05 | Flow 5: Gesture Recognition | MEDIUM |
| N/A | Flow 6: Action Mapping | MEDIUM (no execution) |
| TC-03, TC-04 | Flow 7: Tray & UI | HIGH |
| N/A | Flow 8: Profile Management | MEDIUM (manual test) |
| TC-07 | Flow 9: Persistence | HIGH |

**Note:** Flow 6 (Action Execution) cannot be fully tested as SendInput is not implemented (demo mode).

---

## 9. Test Execution Checklist

Use this checklist to track progress:

```
[ ] Environment setup complete
[ ] .NET 8 SDK installed and verified
[ ] Build successful (no errors)
[ ] All CRITICAL priority tests passed
[ ] All HIGH priority tests passed
[ ] At least 50% MEDIUM priority tests passed
[ ] Performance baselines recorded
[ ] Edge cases attempted (at least 3)
[ ] Bug reports filed (if any)
[ ] Final smoke test on clean system
```

---

## 10. Test Report Format

After testing, provide summary:

```markdown
## Test Execution Report

**Date:** YYYY-MM-DD
**Tester:** [Name]
**Environment:** Windows 11 23H2, 100% DPI, Single Monitor

**Summary:**
- Total Cases: 14
- Passed: X
- Failed: Y
- Skipped: Z
- Blocked: W

**Critical Issues:**
1. [Issue description + ID]

**Observations:**
- [Performance notes]
- [UX feedback]

**Recommendation:**
- [ ] Approve for release
- [ ] Require fixes before release
- [ ] Needs additional testing
```

---

## Appendix A: Quick Reference

### Common Commands

```powershell
# Build
.\build.ps1

# Run
.\run_demo.ps1

# Test
dotnet test -c Release

# Clean config
Remove-Item -Recurse "$env:APPDATA\MagicMouseClone"

# Kill process
taskkill /IM MagicMouseClone.exe /F

# View logs
dotnet run --project src\MagicMouseClone.App
```

### Config File Locations

```
%APPDATA%\MagicMouseClone\config.json
%APPDATA%\MagicMouseClone\Profiles\*.json
```

---

**Document Version:** 1.0  
**For Questions:** Refer to `magicmouseutilities_rebuild.md` Section 8 (Testing Strategy)
