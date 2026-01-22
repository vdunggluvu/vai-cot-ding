# Tester Guide: Magic Mouse Utilities Clone

## 1. Objective & Scope
**Objective**: Verify the functional correctness and stability of the "Magic Mouse Utilities Clone" application, specifically focusing on the Gesture Recognition Engine and basic application lifecycle (Tray, Settings).
**Scope**:
- **Core Logic**: Gesture detection (Scroll, Swipe), Action mapping.
- **UI/UX**: Tray icon behavior, Settings dialog interactions, Logging.
- **Stability**: Startup/Shutdown, Single Instance enforcement.
- **Excluded**: Real Bluetooth Hardware connectivity (Simulated only).

## 2. Environment Matrix
| Category | Requirement | Notes |
| :--- | :--- | :--- |
| **OS** | Windows 10/11 (x64) | Tested on 21H2+ |
| **Framework** | .NET 8.0 SDK | Required for building |
| **Permissions** | Standard User | Admin not strictly required yet |
| **Hardware** | Mouse/Trackpad | For interacting with UI |

## 3. Build & Run Instructions

### Prerequisites
- Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### Build Steps
1. Open PowerShell in `MagicMouseClone` directory.
2. Run automated build script:
   ```powershell
   .\build.ps1
   ```
3. **Expected Output**: "Build succeeded" and "Passed!" for tests.

### Run Steps
1. Navigate to output directory:
   ```powershell
   cd .\src\MagicMouseClone.App\bin\Release\net8.0-windows\
   ```
2. Execute:
   ```powershell
   .\MagicMouseClone.App.exe
   ```
3. Application should start **minimized to tray**.

## 4. Observations & Debugging

### Tray Icon
- **Location**: System Tray (near clock).
- **Icon**: Generic Application Icon (Default).
- **Tooltip**: "Magic Mouse Utilities Clone".

### Logs
- **Internal**: Visible in the "Diagnostic / Debug" section of the Settings Window.
- **Console**: N/A (GUI App).

### Configuration
- Currently strictly In-Memory (reset on restart).
- Defaults: Gestures Enabled, Scroll Speed 1.0.

## 5. Detailed Test Cases

| ID | Test Case | Steps | Expected Result | Pass/Fail |
| :--- | :--- | :--- | :--- | :--- |
| **TC-01** | **Startup to Tray** | 1. Run App.<br>2. Check Taskbar.<br>3. Check System Tray. | App does NOT appear in Taskbar.<br>Icon appears in Tray. | |
| **TC-02** | **Single Instance** | 1. Run App.<br>2. Run App again. | Second instance should exit immediately (or focus first). | |
| **TC-03** | **Open Settings** | 1. Double-click Tray Icon.<br>OR<br>2. Right-click > Settings. | Settings Window opens centered. | |
| **TC-04** | **Enable/Disable** | 1. Uncheck "Enable Gestures".<br>2. Check Logs. | Logic should update (verify via fake device if possible). | |
| **TC-05** | **Fake Device** | 1. Open Settings > Diagnostic.<br>2. Click "Connect Fake Device". | Log: "Fake Device Connected."<br>Log: "Gesture Detected: ScrollDown..." every 1s. | |
| **TC-06** | **Scroll Speed** | 1. Adjust slider.<br>2. Connect Fake Device. | (Future) Gesture magnitude in logs scales with slider. | |
| **TC-07** | **Tray Exit** | 1. Right-click Tray > Exit. | Icon disappears.<br>Process terminates. | |
| **TC-08** | **Window Close** | 1. Open Settings.<br>2. Click "X". | Window hides.<br>App remains in Tray. | |

## 6. Negative / Edge Cases
- **Minimize Memory**: Run app for 1 hour, check RAM usage (Target < 50MB).
- **Rapid Clicking**: Spam "Connect Fake Device". Should not crash, just restart or ignore.
- **Invalid Config**: (Not applicable yet as config is in-memory).

## 7. Bug Report Template
```markdown
**Title**: [Short description]
**Severity**: [Critical/Major/Minor]
**Steps to Reproduce**:
1. ...
2. ...
**Expected**: ...
**Actual**: ...
**Logs/Screenshots**: ...
```

## 8. Traceability
- **Startup Flow** -> TC-01, TC-02
- **Config Flow** -> TC-04, TC-06
- **Gesture Flow** -> TC-05
