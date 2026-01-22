# MagicKeyboardUtilities - Reimplementation

## Mục Tiêu

Tái tạo lại ứng dụng **MagicKeyboardUtilities.exe** (phiên bản 3.1.5.6 của Magic Utilities Pty Ltd) dựa trên phân tích luồng chạy (flow report) từ file `flow-claude.md`.

Đây là một clone chức năng được viết bằng **C# .NET 8** với mục đích:
- Hiểu rõ hành vi của ứng dụng gốc thông qua static/dynamic analysis
- Tái implement các tính năng đã suy luận được theo cách minh bạch, có thể điều chỉnh
- Phục vụ mục đích học tập và nghiên cứu reverse engineering

## Traceability

Mọi feature trong dự án này đều có traceability trở lại **flow report** (file `flow-claude.md`), cụ thể:
- Startup Flow → Section 4.1
- Input Hook Flow → Section 4.2
- Tray Icon Flow → Section 4.3
- Configuration Flow → Section 4.4
- Device Detection Flow → Section 4.5
- Shutdown Flow → Section 4.6

Chi tiết đầy đủ xem file [TRACEABILITY.md](TRACEABILITY.md).

## Tính Năng

### ✅ Đã Implement
- ✅ Chạy nền không có main window (background tray application)
- ✅ Tray icon với context menu (Enable/Disable/Settings/Exit)
- ✅ Single instance check (chỉ chạy 1 instance)
- ✅ Configuration system (JSON-based, relative path `config.json`)
- ✅ Keyboard remapping engine
- ✅ Low-level keyboard hook (WH_KEYBOARD_LL)
- ✅ Hotkey registration (RegisterHotKey)
- ✅ Logging system (file-based, relative path `logs/app.log`)
- ✅ Graceful shutdown

### ⚠️ Partial/Stub
- ⚠️ Device detection (WM_DEVICECHANGE stub - không đầy đủ SetupAPI)
- ⚠️ Apple VID check (stub - chỉ log, không parse device path)

### ❌ Không Implement
- ❌ Update flow (flow report xác nhận không có network activity)
- ❌ Registry/Persistence (flow report xác nhận không ghi registry)

## Cấu Trúc Thư Mục

```
MagicKeyboardUtilities_Reimpl/
├── src/
│   ├── MagicKeyboardUtilities.Reimpl.sln
│   ├── MagicKeyboardUtilities.App/           # Main application
│   │   ├── Program.cs                        # Entry point
│   │   ├── AppHost.cs                        # Lifecycle orchestrator
│   │   ├── Config/                           # Configuration
│   │   │   ├── AppConfig.cs
│   │   │   └── ConfigStore.cs
│   │   ├── Core/                             # Business logic
│   │   │   ├── RemappingEngine.cs
│   │   │   └── ActionDispatcher.cs
│   │   ├── Input/                            # Hook & hotkey
│   │   │   ├── KeyboardHookService.cs
│   │   │   └── HotkeyService.cs
│   │   ├── Device/                           # Device monitoring
│   │   │   └── DeviceMonitor.cs
│   │   ├── Messaging/                        # Message window
│   │   │   └── HiddenMessageWindow.cs
│   │   ├── Tray/                             # Tray icon
│   │   │   └── TrayIconController.cs
│   │   ├── Interop/                          # P/Invoke
│   │   │   └── NativeMethods.cs
│   │   ├── Diagnostics/                      # Logging
│   │   │   └── FileLogger.cs
│   │   └── config.json                       # Configuration file
│   └── MagicKeyboardUtilities.Tests/         # Unit tests
│       ├── ConfigTests.cs
│       ├── RemappingEngineTests.cs
│       └── ActionDispatcherTests.cs
├── docs/
│   ├── TRACEABILITY.md                       # Feature → Evidence mapping
│   └── DESIGN.md                             # Architecture & diagrams
├── scripts/
│   ├── build.ps1                             # Build script
│   ├── run.ps1                               # Run script
│   └── test.ps1                              # Test script
├── README.md                                 # This file
└── CHANGELOG.md                              # Version history
```

## Yêu Cầu Hệ Thống

- **OS**: Windows 10/11 x64
- **.NET**: .NET 8.0 SDK hoặc cao hơn
- **Quyền**: Administrator không bắt buộc, nhưng cần quyền install global hooks

## Cách Build & Run

### Build (1 lệnh)

```powershell
.\scripts\build.ps1
```

Hoặc trực tiếp:

```powershell
dotnet build src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

### Run (1 lệnh)

```powershell
.\scripts\run.ps1
```

Hoặc trực tiếp:

```powershell
dotnet run --project src\MagicKeyboardUtilities.App -c Release
```

### Test (1 lệnh)

```powershell
.\scripts\test.ps1
```

Hoặc trực tiếp:

```powershell
dotnet test src\MagicKeyboardUtilities.Reimpl.sln -c Release
```

## Cấu Hình (config.json)

File `config.json` nằm trong thư mục output (cùng với EXE sau khi build).

### Schema

```json
{
  "app": {
    "startMinimized": true,
    "autoSave": true,
    "logLevel": "Information"
  },
  "features": {
    "trayIcon": true,
    "enableHooks": false,           // ⚠️ Mặc định OFF vì lý do an toàn
    "enableHotkeys": false,          // ⚠️ Mặc định OFF vì lý do an toàn
    "enableDeviceMonitor": false,    // ⚠️ Stub implementation
    "enableUpdater": false,          // ❌ Không implement (no network)
    "enableSendInput": false         // ⚠️ Chỉ bật khi test remapping
  },
  "remapping": [
    { "fromVk": 124, "toVk": 175, "description": "F13 -> Volume Up (example)" }
  ],
  "hotkeys": [
    {
      "id": 1,
      "modifiers": ["Control", "Alt"],
      "vk": 75,
      "action": "ToggleEnabled",
      "description": "Ctrl+Alt+K -> Toggle"
    }
  ],
  "device": {
    "appleVendorId": "0x05AC",
    "notifyOnConnect": true,
    "notifyOnDisconnect": true
  }
}
```

### Cách Bật Hook/Hotkey An Toàn

**Bước 1:** Mở `config.json`

**Bước 2:** Đổi các flags:
```json
"enableHooks": true,      // Bật keyboard hook
"enableHotkeys": true,    // Bật hotkeys
"enableSendInput": false  // KHÔNG bật nếu chưa test kỹ
```

**Bước 3:** Lưu file và khởi động lại app

**Bước 4:** Test remapping:
- Khi `enableSendInput = false`: Hook chỉ log sự kiện, không thay đổi input
- Khi `enableSendInput = true`: Hook sẽ block key gốc và gửi key mới

**⚠️ CẢNH BÁO:**
- Nếu mapping sai, có thể làm bàn phím không hoạt động bình thường
- Luôn có cách thoát: `Ctrl+Alt+Del` → Task Manager → Kill process
- Hoặc dùng tray menu "Exit" hoặc "Disable"

## Hotkey Mặc Định

| Hotkey | Chức Năng | Trạng Thái |
|--------|-----------|------------|
| `Ctrl+Alt+K` | Toggle Enable/Disable | Example (có thể thay đổi) |

## Log Files

Log được ghi vào: `logs/app.log` (relative path)

Cấu hình log level trong `config.json`:
- `"Trace"`: Rất chi tiết (debug hooks)
- `"Debug"`: Chi tiết
- `"Information"`: Bình thường (mặc định)
- `"Warning"`: Chỉ cảnh báo
- `"Error"`: Chỉ lỗi

## Cách Test

### Test 1: Tray Icon
1. Chạy ứng dụng
2. Kiểm tra tray icon xuất hiện
3. Right-click → menu hiển thị "Enable", "Disable", "Settings", "Exit"
4. Double-click → hiển thị About dialog

### Test 2: Remapping (Safe Mode)
1. Đặt `enableHooks: true`, `enableSendInput: false`
2. Chạy ứng dụng, click "Enable" trong tray menu
3. Nhấn F13/F14 → Xem log, không có thay đổi input thật
4. Xác nhận remapping table hoạt động

### Test 3: Remapping (Live)
1. **Backup config** trước
2. Đặt `enableHooks: true`, `enableSendInput: true`
3. Chạy ứng dụng, click "Enable"
4. Nhấn F13 → Nên nghe volume up
5. Nếu lỗi: Click "Disable" hoặc Exit

### Test 4: Hotkey
1. Đặt `enableHotkeys: true`
2. Chạy ứng dụng
3. Nhấn `Ctrl+Alt+K` → Toggle enable/disable
4. Xem notification balloon

### Test 5: Device Detection (Stub)
1. Đặt `enableDeviceMonitor: true`
2. Chạy ứng dụng
3. Cắm/rút USB keyboard → Xem log
4. Expect: Log "Device arrival/removal" nhưng không identify chính xác

## Known Issues / Limitations

### 🔴 Critical
- **Remapping có thể block input nếu config sai** → Luôn test với `enableSendInput: false` trước

### 🟡 Medium
- **Device detection không đầy đủ**: Chỉ nhận WM_DEVICECHANGE, không parse device path → Không xác định được Apple Magic Keyboard chính xác
- **Icon mặc định**: Dùng system icon, chưa có custom icon (cần file `.ico`)

### 🟢 Low
- **No persistence**: App không tự khởi động cùng Windows (theo thiết kế, giống original)
- **No network**: Không có update check (theo thiết kế, giống original)

## Nguyên Tắc Assumption

Mọi feature đánh dấu **INFERRED**, **SPECULATIVE**, hoặc **UNKNOWN** trong flow report đều được implement với:
1. Toggle trong config (mặc định OFF)
2. Stub có logging đầy đủ
3. Ghi rõ "Assumption" trong code comments và docs

Ví dụ:
- Hook logic: **INFERRED** từ flow report → Implement nhưng default OFF
- Device VID check: **SPECULATIVE** → Stub có log nhưng không đầy đủ

## Đóng Góp / Cải Tiến

Các cải tiến có thể làm (nếu có thêm evidence):
- [ ] Hoàn thiện device detection với SetupAPI
- [ ] Parse device path để check VID/PID chính xác
- [ ] Custom icon resource
- [ ] Settings UI dialog thay vì mở JSON trực tiếp
- [ ] Installer/portable package

## License & Disclaimer

Dự án này là **reimplementation** cho mục đích học tập và nghiên cứu.

**DISCLAIMER:**
- Không liên kết với Magic Utilities Pty Ltd
- Không sử dụng code từ binary gốc (binary bị protect/packed)
- Chỉ dựa trên phân tích hành vi quan sát được
- Apple, Magic Keyboard là trademark của Apple Inc.

Sử dụng tự chịu trách nhiệm. Tác giả không chịu trách nhiệm về thiệt hại từ việc sử dụng phần mềm này.

## References

- Flow Report: `flow-claude.md` (22/01/2026)
- Original Binary: `MagicKeyboardUtilities.exe` v3.1.5.6
- Traceability Matrix: `docs/TRACEABILITY.md`
- Design Document: `docs/DESIGN.md`

---

**Phiên bản**: 1.0.0  
**Ngày tạo**: 22/01/2026  
**Tác giả**: Re-implementation Project based on Flow Analysis
