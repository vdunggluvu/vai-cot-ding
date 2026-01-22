# TRACEABILITY MATRIX

| Feature | Module/Class | Evidence (Flow Report) | Status | Note |
|---------|--------------|------------------------|--------|------|
| **Startup - Single Instance** | `Program.cs` | 4.1 STARTUP FLOW (Step 4) | Implemented | Uses Mutex |
| **Startup - Infrastructure** | `AppHost.cs` | 4.1 STARTUP FLOW (Step 7) | Implemented | Message loop, hidden window |
| **Tray Icon** | `TrayIconController.cs` | 4.3 TRAY ICON FLOW | Implemented | Context menu: Enable, Settings, Exit |
| **Input Hook (Low Level)** | `KeyboardHookService.cs` | 4.2 INPUT HOOK FLOW (Step 1) | Implemented | `WH_KEYBOARD_LL` via P/Invoke |
| **Hotkeys** | `HotkeyService.cs` | 4.2 INPUT HOOK FLOW (Step 1 - Option B) | Implemented | `RegisterHotKey` |
| **Remapping Logic** | `RemappingEngine.cs` | 4.2 INPUT HOOK FLOW (Step 3) | Implemented | Basic vkCode remapping |
| **Configuration** | `AppConfig.cs` | 4.4 CONFIGURATION FLOW | Implemented | JSON-based (Assumed/Replaced embedded) |
| **Device Detection** | `DeviceMonitor.cs` | 4.5 DEVICE DETECTION FLOW | Stubbed | Wraps `RegisterDeviceNotification` |
| **Shutdown** | `AppHost.cs` | 4.6 SHUTDOWN FLOW | Implemented | Clean unhook & exit |
| **Update Check** | `UpdaterService.cs` | 4.7 UPDATE FLOW | Stubbed | Log only, network disabled |

## Assumptions & Deviations
1. **Configuration**: Flow report suggests embedded/encrypted config. Re-implementation uses `config.json` for transparency and ease of use.
2. **Device Detection**: Flow report implies VID 0x05AC check. Implemented as a check on device path strings in `WM_DEVICECHANGE` for safety, stubbed if complex setupapi is needed.
3. **Network**: Update flow is stubbed to ensure no unauthorized network activity.
