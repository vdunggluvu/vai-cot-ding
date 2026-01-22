# 1. Tổng quan ứng dụng

- **Mục đích chính**: Cung cấp driver/interface để sử dụng Apple Magic Trackpad trên Windows với đầy đủ tính năng gesture (cuộn mượt, zoom, tap-to-click) giống như trên macOS.
- **Đối tượng sử dụng**: Người dùng Windows muốn sử dụng Magic Trackpad (Bluetooth/USB).
- **Vai trò trong hệ thống**: User-mode Utility (chạy quyền User hoặc Admin), đóng vai trò "Driver Bridge". Nó không phải là kernel driver thuần túy mà là một ứng dụng nền đọc dữ liệu Raw HID từ thiết bị và inject Mouse/Keyboard Input vào OS.
- **Công nghệ**: Native Windows Application (C++/MFC), được đóng gói/nén (Packed) để bảo vệ bản quyền (sản phẩm thương mại của "Magic Utilities Pty Ltd").

# 2. Entry Point & Startup Flow

- **Điểm vào (Entry Point)**: Standard PE Entry (Packer Stub) -> Unpacking Routine -> `WinMain` / `AfxWinMain` (MFC).
- **Trình tự khởi động**:
    1.  **System Check**: Kiểm tra OS version, quyền Administrator (cần thiết để cài đặt driver hoặc ghi registry hệ thống).
    2.  **Mutex Check**: Tạo Named Mutex (ví dụ `Global\MagicUtilities...`) để đảm bảo chỉ có 1 instance chạy.
    3.  **Config Loading**: Đọc cấu hình từ Registry hoặc File Settings.
    4.  **Driver/Service Check**: Kiểm tra xem Driver USB/Bluetooth kèn theo đã được cài đặt chưa (sử dụng `SetupDiEnumDeviceInterfaces`).
    5.  **UI Initialization**: Khởi tạo Hidden Window để xử lý message loop và Tray Icon.

**ASCII DIAGRAM: Startup Sequence**

```ascii
[Launch EXE]
     |
     v
[Unpack/Decrypt Code]
     |
     v
[Check Singleton (Mutex)] --(Exists)--> [Focus Old Instance] -> [Exit]
     |
     v
[Load Config (Registry/XML)]
     |
     v
[Check Drivers (SetupAPI)] --(Missing)--> [Prompt Install]
     |
     v
[Init Bluetooth/HID Listener]
     |
     v
[Create Tray Icon & Hidden Window]
     |
     v
[Enter Main Message Loop]
```

# 3. Configuration & Persistence Flow

- **Vị trí lưu cấu hình**:
    - **Registry**: `HKEY_CURRENT_USER\Software\Magic Utilities\Magic Trackpad Utilities` (Suy luận từ chuẩn "Magic Utilities").
    - **File**: Có thể sử dụng XML manifest (như đã thấy `asmv3:windowsSettings`) và file cấu hình XML/INI trong `%APPDATA%` (dựa trên strings `XML`, `INI` tìm thấy).
- **Khi nào load**: Ngay khi khởi động (`InitInstance`).
- **Khi nào save**: Khi người dùng thay đổi settings qua UI hoặc khi thoát ứng dụng (`ExitInstance`).

# 4. Device / Input Handling Flow (TRỌNG TÂM)

- **Cơ chế phát hiện**:
    - Sử dụng **SetupAPI** (`SetupDiEnumDeviceInterfaces`) và **Bluetooth APIs** (`BluetoothFindFirstRadio`) để quét thiết bị.
    - Lắng nghe sự kiện cắm rút thiết bị (`WM_DEVICECHANGE`).
- **Luồng dữ liệu (Data Flow)**:
    - **Input**: Đọc Raw Input Data từ HID Driver (HID Report). Magic Trackpad gửi dữ liệu cảm ứng đa điểm qua HID.
    - **Processing**: Engine tính toán vị trí ngón tay, vận tốc, gesture (zoom, scroll, swipe).
    - **Output**: Gọi Windows API (`SendInput`) để giả lập chuột/bàn phím tương ứng.

**ASCII DIAGRAM: Input Processing**

```ascii
[Hardware: Magic Trackpad]
          | (Bluetooth/USB HID Report)
          v
[OS Kernel: HID Driver]
          | (Raw Input / ReadFile)
          v
[App: HID Reader Thread]
          | (Raw Touch Data)
          v
[Logic: Gesture Engine] <--- [Config: Sensitivity/Gestures]
          | (Recognized Gesture: e.g. 2-finger scroll)
          v
[Action: Input Injector]
          | (SendInput)
          v
[OS: Cursor Move / Wheel Event]
```

# 5. UI Interaction Flow

- **UI Framework**: Native C++ (MFC/Windows API). Có sử dụng Common Dialogs (`comdlg32.dll`).
- **Giao diện chính**:
    - Thường ẩn (Background process).
    - Tương tác qua **System Tray Icon**: Context Menu (Settings, Battery status, Exit).
    - **Settings Dialog**: Hộp thoại cấu hình độ nhạy, bật tắt tap-to-click.
- **Phản hồi**: Update biểu tượng pin trên Tray Icon dựa trên dữ liệu pin đọc từ Trackpad.

# 6. Background / Service / Tray Flow

- **Chạy nền**: Có. Đây là chế độ hoạt động chính.
- **Tray Icon**: Có.
- **Service**: Có thể có một Service đi kèm (`MagicUtilitiesService`) để chạy ở quyền cao hơn (bypass UAC khi inject input vào ứng dụng Admin), giao tiếp với App qua Named Pipe hoặc Shared Memory. (Strings gợi ý có xử lý `Service` nhưng bị obfuscated).

# 7. External Interaction Flow

| Interaction | API/Method | Mục đích | Thời điểm |
| :--- | :--- | :--- | :--- |
| **Driver** | `SetupDiEnumDeviceInterfaces`, `Bluetooth...` | Tìm thiết bị, kiểm tra kết nối | Startup & Runtime (Polling/Event) |
| **Registry** | `RegOpenKey`, `RegQueryValue` (inferred) | Đọc/Ghi license và settings | Startup & Change Settings |
| **User Input** | `SendInput` (User32 - inferred) | Điều khiển con trỏ chuột, cuộn | Liên tục khi có touch |
| **UI** | `CommDlgExtendedError`, `Shell_NotifyIcon` | Hiển thị lỗi, Tray Icon | Startup, Error, User Interaction |

# 8. Error Handling & Edge Cases

- **Lỗi thiết bị**: Nếu không tìm thấy Trackpad, App sẽ chờ (idle) và lắng nghe `WM_DEVICECHANGE`. Không crash.
- **Lỗi quyền**: Nếu thiếu quyền (không thể ghi Registry hoặc cài driver), sẽ hiện Dialog báo lỗi hoặc yêu cầu Elevation (UAC).
- **Lỗi Config**: Fallback về default settings nếu file config lỗi.
- **Obfuscation**: Nếu phát hiện debugger, app có thể tự đóng hoặc hoạt động sai lệch (Anti-Debug behavior phổ biến ở app thương mại).

# 9. Evidence Mapping

| Kết luận | Evidence (Bằng chứng từ File Executable) |
| :--- | :--- |
| **App Identity** | String: `macOS magic for Windows`, `Magic Trackpad Utilities`, `ProductVersion` |
| **Technology** | PE Header: `MZ`, Imports: `mfc42.dll` (implicit via AFX strings `AFXRAVC`, `^aFX$`) hoặc Static Link MFC. Không có .NET Header. |
| **Device Access** | Strings/Imports: `SetupDiEnumDeviceInterfaces` (SetupAPI), `BluetoothFindFirstRadio`. |
| **UI/Dialogs** | Imports: `comdlg32.dll`, Strings: `CommDlgExtendedError`. |
| **Packing/Protection** | Strings: Dữ liệu rác (garbage) chiếm đa số, thiếu imports function names quan trọng (GetMessage), chuỗi `10$~`, `(hyRS`. |

# 10. Reimplementation Blueprint (CLONE APP)

Để viết lại ứng dụng này (Clone), bạn cần thiết kế theo kiến trúc sau:

**Modules cần có:**
1.  **Core / Main Loop**: Quản lý vòng đời ứng dụng, Tray Icon, Single Instance.
2.  **Hardware Layer (HID)**:
    - Sử dụng `HidCls` hoặc `Windows.Devices.HumanInterfaceDevice` (nếu dùng UWP/C#) hoặc `SetupAPI` + `CreateFile` (C++).
    - Parse HID Report Descriptor của Apple Magic Trackpad.
3.  **Gesture Engine**:
    - Input: Tọa độ ngón tay (ID, X, Y, State).
    - Output: Events (Tap, Scroll, Zoom, Swipe).
    - Logic: State Machine theo dõi chuyển động ngón tay theo thời gian.
4.  **Injector**: Map Events thành `SendInput` calls.
5.  **Config / UI**: Lưu settings, giao diện chỉnh sửa.

**Architecture Diagram:**

```ascii
[ HID Listener Thread ] ---> [ Gesture Analyzer ] ---> [ Input Injector ]
       ^                             ^                        |
       |                             |                        v
[ Device Monitor ]            [ Config Manager ]       [ Windows OS ]
       |                             ^
       v                             |
[ UI / Tray Thread ] <--------> [ Settings ]
```

# 11. Những phần CHƯA XÁC ĐỊNH & Cách Kiểm Chứng

| Unknown | Lý do | Cách xác minh (Verified) |
| :--- | :--- | :--- |
| **Persistence Path** | String bị nén/mã hóa. | Dùng **ProcMon (Process Monitor)**, filter path chứa "Magic" khi app chạy. |
| **Exact Gesture Logic** | Code logic bị compile và pack. | Dùng **Wireshark + USBPcap** để bắt gói tin HID, so sánh input tay và action trên màn hình để reverse thuật toán. |
| **Service IPC** | Không thấy rõ string Pipe/Socket. | Dùng **Process Explorer** xem Handles của process, tìm Named Pipes. |
