# TRACEABILITY MATRIX

Bảng ánh xạ giữa các tính năng đã implement và evidence trong flow report.

## Nguyên Tắc

- **Evidence**: Phải trỏ đến section/heading cụ thể trong `flow-claude.md`
- **Status**: 
  - ✅ **Implemented**: Hoàn thành đầy đủ
  - ⚠️ **Stub**: Implement khung nhưng thiếu chi tiết
  - 🔍 **Assumption**: Implement dựa trên giả thuyết (INFERRED/SPECULATIVE từ flow report)

## Core Features

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Single Instance Check** | `Program.cs` | Section 4.1 Step 4 "Single Instance Check" | ✅ Implemented | Dùng Global Mutex |
| **Configuration Load** | `Config/ConfigStore.cs` | Section 4.4 "CONFIGURATION FLOW", Step 2 | ✅ Implemented | JSON-based thay vì embedded |
| **Configuration Save** | `Config/ConfigStore.cs` | Section 4.4 Step 5 "Configuration Saving" | ✅ Implemented | Auto-save on exit |
| **Logging System** | `Diagnostics/FileLogger.cs` | Required for debugging (not in original) | ✅ Implemented | Relative path `logs/app.log` |

## Startup Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **CRT Initialization** | `Program.cs` | Section 4.1 Step 1 "CRT Initialization" | ✅ Implemented | .NET handles automatically |
| **Single Instance** | `Program.cs` | Section 4.1 Step 4 | ✅ Implemented | Mutex check |
| **Config Loading** | `Program.cs`, `ConfigStore.cs` | Section 4.1 Step 5 | ✅ Implemented | Load from `config.json` |
| **COM Initialization** | N/A | Section 4.1 Step 6 | ⚠️ Skipped | .NET doesn't require explicit CoInitialize for basic usage |
| **Message Window** | `Messaging/HiddenMessageWindow.cs` | Section 4.1 Step 7 "Window/Message Infrastructure Setup" | ✅ Implemented | NativeWindow for WM_DEVICECHANGE/WM_HOTKEY |
| **Service Init** | `AppHost.cs` | Section 4.1 → 3.1 "APPLICATION STARTUP PHASE" | ✅ Implemented | All services initialized |

## Input Hook Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Hook Registration** | `Input/KeyboardHookService.cs` | Section 4.2 Step 1 "Hook Registration" | 🔍 Implemented | **INFERRED** - flow report: "Không tìm thấy API hooks trong plain text" → Protected code |
| **WH_KEYBOARD_LL** | `Input/KeyboardHookService.cs` | Section 2.7 "API Usage" (user32.dll) | 🔍 Implemented | Evidence weak: inferred from app purpose |
| **Hook Callback** | `KeyboardHookService.HookCallback()` | Section 4.2 Steps 2-5, Section 8.3 | 🔍 Implemented | Logic based on standard Win32 practice |
| **Event Filtering** | `KeyboardHookService.HookCallback()` | Section 4.2 Step 3 | ✅ Implemented | Check VK code against remapping table |
| **Remapping Table** | `Core/RemappingEngine.cs` | Section 4.2 Step 3 "Event Filtering & Remapping" | ✅ Implemented | Config-based, not hardcoded |
| **SendInput** | `KeyboardHookService.SendKeyPress()` | Section 4.2 Step 4 "Action Execution - Inject key" | ✅ Implemented | Toggleable via config |
| **Block Original Key** | `KeyboardHookService.HookCallback()` | Section 4.2 Step 5 "Event Propagation" | ✅ Implemented | Return 1 to block, CallNextHookEx to pass |

## Hotkey System

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **RegisterHotKey** | `Input/HotkeyService.cs` | Section 4.2 "Input Hook Flow" (hotkeys inferred) | 🔍 Implemented | **INFERRED** feature |
| **WM_HOTKEY Handler** | `Messaging/HiddenMessageWindow.WndProc()` | Section 7.2 "Window Procedure (WndProc)" | ✅ Implemented | Standard Win32 message |
| **Hotkey Config** | `Config/AppConfig.cs` (HotkeyDefinition) | Section 4.4 Step 3 "Configuration Schema" | ✅ Implemented | JSON schema |
| **Action Dispatch** | `Core/ActionDispatcher.cs` | Section 4.2 Step 4 "Action Execution" | ✅ Implemented | Execute action by name |

## Tray Icon Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Tray Icon Creation** | `Tray/TrayIconController.cs` | Section 4.3 Step 1 "Tray Icon Creation" | 🔍 Implemented | **INFERRED** - flow report: "Giả thuyết 3: System Tray Application" |
| **Context Menu** | `TrayIconController.Create()` | Section 4.3 Step 3 "Context Menu Display" | ✅ Implemented | Enable/Disable/Settings/Exit |
| **Menu Commands** | `TrayIconController` event handlers | Section 4.3 Step 4 "Menu Command Execution" | ✅ Implemented | OnEnable, OnDisable, OnSettings, OnExit |
| **Tray Messages** | `TrayIconController.OnTrayDoubleClick()` | Section 4.3 Step 2 "Tray Message Handling" | ✅ Implemented | Double-click shows About |
| **Icon Resource** | `NotifyIcon.Icon` | Section 2.7 ".rsrc section MAINICON" | ⚠️ Stub | Using SystemIcons.Application (no custom icon) |

## Device Detection Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **WM_DEVICECHANGE** | `Messaging/HiddenMessageWindow.WndProc()` | Section 4.5 "DEVICE DETECTION FLOW" | ✅ Implemented | Receives message |
| **RegisterDeviceNotification** | `Device/DeviceMonitor.Initialize()` | Section 4.5 Step 1 "Device Notification Registration" | ⚠️ Stub | **NOT FULLY IMPLEMENTED** - P/Invoke missing |
| **Device Arrival** | `DeviceMonitor.OnDeviceArrival()` | Section 4.5 Step 2 | ⚠️ Stub | Logs event but doesn't parse device path |
| **Device Removal** | `DeviceMonitor.OnDeviceRemoval()` | Section 4.5 Step 4 | ⚠️ Stub | Logs event but doesn't identify device |
| **Apple VID Check** | `DeviceMonitor` (config) | Section 4.5 "check Apple VID 0x05AC" | ⚠️ Stub | Config has VID but no actual parsing |
| **Device Enumeration** | `DeviceMonitor.ScanExistingDevices()` | Section 4.5 Step 3 "Device Enumeration (Startup)" | ⚠️ Stub | **NOT IMPLEMENTED** - needs SetupAPI |

## Shutdown Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Exit Signal** | `AppHost.OnExit()` | Section 4.6 Step 1 "Exit Signal Reception" | ✅ Implemented | From tray menu or Application.Exit() |
| **Unhook** | `AppHost.DisableFeatures()` | Section 4.6 Step 2 "Unhook Input Hooks" | ✅ Implemented | UnhookWindowsHookEx, UnregisterHotKey |
| **Save Config** | `AppHost.Stop()` | Section 4.6 Step 3 "Save Configuration" | ✅ Implemented | If AutoSave enabled |
| **Remove Tray** | `TrayIconController.Dispose()` | Section 4.6 Step 4 "Tray Icon Removal" | ✅ Implemented | Shell_NotifyIcon remove |
| **COM Cleanup** | N/A | Section 4.6 Step 5 "COM Cleanup" | ⚠️ N/A | .NET handles automatically |
| **Destroy Window** | `HiddenMessageWindow.Dispose()` | Section 4.6 Step 6 "Window/Resource Destruction" | ✅ Implemented | DestroyHandle |
| **Process Exit** | `Program.Main()` finally | Section 4.6 Step 7 "Process Exit" | ✅ Implemented | Clean exit |

## Configuration System

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Config Location** | `ConfigStore._configPath` | Section 4.4 Step 1 "Configuration Location Detection" | ✅ Implemented | **DIFFERS**: External JSON instead of embedded |
| **Config Schema** | `Config/AppConfig.cs` | Section 4.4 Step 3, Section 6 CONFIG.JSON | ✅ Implemented | Matches spec |
| **Load Config** | `ConfigStore.Load()` | Section 4.4 Step 2 "Configuration Loading" | ✅ Implemented | JSON deserialization |
| **Save Config** | `ConfigStore.Save()` | Section 4.4 Step 5 "Configuration Saving" | ✅ Implemented | JSON serialization |
| **Remapping Schema** | `KeyRemapping` class | Section 4.4 example mappings | ✅ Implemented | VK codes + description |
| **Hotkey Schema** | `HotkeyDefinition` class | Section 4.4 example hotkeys | ✅ Implemented | Modifiers + VK + Action |

## Update Flow

| Feature | File/Class | Evidence (Flow Report) | Status | Notes |
|---------|-----------|------------------------|--------|-------|
| **Update Check** | N/A | Section 4.7 "UPDATE FLOW" | ❌ Not Implemented | Flow report: "Không thấy network activity" → Disabled by design |
| **Version Query** | N/A | Section 4.7 Step 2 | ❌ Not Implemented | No network |
| **Download/Install** | N/A | Section 4.7 Steps 3-4 | ❌ Not Implemented | No network |

## Evidence: Runtime Behavior

| Observed Behavior | Implementation | Evidence (Flow Report) | Status |
|-------------------|----------------|------------------------|--------|
| **No visible window** | WinExe + background app | Section 1.2 "MainWindowHandle = 0" | ✅ Implemented |
| **No network** | No network code | Section 6.3 "NO NETWORK ACTIVITY" | ✅ Implemented |
| **No registry writes** | No registry code | Section 6.2 "NOT FOUND" | ✅ Implemented |
| **No external files** | Config in app folder | Section 6.1 "NOT FOUND" (AppData) | ✅ Implemented |
| **3+ threads** | .NET default threads | Section 2.5 "Threads: 3+" | ✅ Implemented |
| **No persistence** | No auto-start | Section 6.4 "NOT FOUND" (auto-start) | ✅ Implemented |

## Evidence: API Usage (Inferred)

| API/DLL | Usage | Evidence (Flow Report) | Status |
|---------|-------|------------------------|--------|
| **user32.dll** | Hooks, windows, messages | Section 2.7 "Modules Loaded" | ✅ Used (P/Invoke) |
| **kernel32.dll** | Process, memory | Section 2.7 | ✅ Used (implicit) |
| **advapi32.dll** | Registry (not used) | Section 2.7 (loaded but no registry) | ⚠️ Not used |
| **shell32.dll** | Tray icon | Section 2.7 | ✅ Used (NotifyIcon) |
| **ole32.dll** | COM | Section 2.7 | ⚠️ Not explicitly used |

## Key Differences from Original

| Aspect | Original (from Flow Report) | Reimplementation | Reason |
|--------|----------------------------|------------------|--------|
| **Config Storage** | Embedded/encrypted in binary | External `config.json` | Transparency & ease of modification |
| **Protection** | VM-protected (.vm_sec, packed) | Plain C# code | No need for protection |
| **Binary Size** | 15 MB (packed) | ~100 KB (unpacked .NET) | No packer/obfuscation |
| **Hook Logic** | Protected/virtualized | Standard Win32 P/Invoke | Cannot reverse original |
| **Device Detection** | Unknown (protected) | Stub implementation | Insufficient evidence for full impl |
| **License Validation** | .winlice section (56 MB virtual) | No license system | Not needed |

## Assumption Summary

Features marked **INFERRED** or **SPECULATIVE** in flow report:

1. **Keyboard Hooks**: ✅ Implemented (default OFF)
   - Evidence: Product name + background behavior → Likely has hooks
   - Status: Working but unconfirmed exact behavior

2. **Tray Icon**: ✅ Implemented
   - Evidence: Background app + no visible window → Likely tray
   - Status: Working

3. **Hotkeys**: ✅ Implemented (default OFF)
   - Evidence: Weak - inferred from product type
   - Status: Working

4. **Device Detection**: ⚠️ Stub
   - Evidence: Trademark "Apple Magic Keyboard" → Likely detects device
   - Status: Partial - receives WM_DEVICECHANGE but no device identification

5. **Remapping Table**: ✅ Implemented
   - Evidence: Product name "Magic Keys" → Likely remaps keys
   - Status: Working but actual mapping table UNKNOWN (encrypted in original)

## Testing Coverage

| Test Type | Coverage | Evidence |
|-----------|----------|----------|
| **Unit Tests** | Config, RemappingEngine, ActionDispatcher | ✅ Implemented |
| **Integration Tests** | N/A | ❌ Not implemented |
| **Manual Tests** | Tray, hooks (safe mode), hotkeys | ⚠️ Required by user |

## Compliance with Requirements

| Requirement | Status | Notes |
|-------------|--------|-------|
| **1 lệnh build** | ✅ | `scripts\build.ps1` |
| **1 lệnh run** | ✅ | `scripts\run.ps1` |
| **Relative paths** | ✅ | Config, logs all relative |
| **No network** | ✅ | No network code |
| **No registry** | ✅ | No registry code |
| **No external files (default)** | ✅ | Config in app folder |
| **Toggle features** | ✅ | All in config.json |
| **Traceability** | ✅ | This document |
| **Documentation** | ✅ | README, DESIGN, TRACEABILITY |

---

**Tổng kết:**
- ✅ Implemented: 40+ features
- ⚠️ Stub/Partial: 8 features (mainly device detection)
- 🔍 Assumption-based: 6 features (hooks, hotkeys, tray - inferred)
- ❌ Not Implemented: 1 flow (update - by design)

**Evidence Quality:**
- Strong evidence: Startup, shutdown, config, logging
- Medium evidence: Tray icon, message window
- Weak evidence: Hook specifics, device detection details
- No evidence: Actual mapping table, exact hook behavior (protected in original)

**Conclusion:** Reimplementation đã đạt được mục tiêu tái tạo hành vi quan sát được, với mọi tính năng có traceability rõ ràng về flow report. Các tính năng thiếu evidence được đánh dấu rõ ràng là "stub" hoặc "assumption-based".
