# MAGICKEYBOARDUTILITIES_FLOW.md

**Environment & Scope:**
- **Target File:** `MagicKeyboardUtilities.exe`
- **Analysis Date:** 2026-01-22
- **Hash (SHA256):** `EF22E1004B37CBF2001D64A80DCEDE13AE46B54AE91E43D3CF18B2ED8064FDD8`
- **Architecture:** x64 Native PE32+ (Windows)

---

## 1. Tổng quan nhanh (Executive Summary)

**MagicKeyboardUtilities.exe** là một ứng dụng Native Desktop (C++/Win32) chạy nền (background helper), được thiết kế để mang các tính năng của bàn phím Apple Magic Keyboard lên Windows.

**Chức năng chính (Dựa trên evidence):**
- **Quản lý kết nối Bluetooth:** Sử dụng thư viện `bthprops.cpl` và `SetupApi` để giao tiếp với hardware adapter.
- **Xử lý Input:** Mapping phím và hỗ trợ các phím chức năng (Fn keys) của Apple.
- **Cấp phép/Bản quyền:** Hệ thống kiểm tra bản quyền thương mại (Strings: "Trial", "Copyright Magic Utilities Pty Ltd", Digital Signatures).
- **Giao diện:** Ứng dụng khay hệ thống (Tray Application) sử dụng Windows Forms/User32 controls.

---

## 2. Bằng chứng thu thập được (Evidence Index)

### A. Static Analysis (File & Strings)
1.  **Dependencies (Imports):**
    -   `bthprops.cpl`: **Bluetooth Control Panel Applet** (Hàm: `BluetoothFindFirstRadio` - Quét thiết bị Bluetooth).
    -   `SetupApi.dll`: **Device Installation** (Hàm: `SetupDiEnumDeviceInterfaces` - Liệt kê thiết bị phần cứng/driver).
    -   `WS2_32.dll`, `Iphlpapi.dll`: **Networking** (Giao tiếp mạng mức thấp).
    -   `User32.dll`, `Gdi32.dll`, `ComCtl32.dll`: **Standard UI** (MessageBox, Windows, Controls).
    -   `Ucrtbase.dll`: **C++ Runtime** (Khẳng định đây là ứng dụng Native C++, không phải .NET hay Electron).
2.  **Strings & Resources:**
    -   **Identity:** "Magic Utilities for Windows", "macOS magic for Windows".
    -   **Legal:** "Magic Keyboard is a registered trademark of Apple Inc.", "Trial".
    -   **Security:** Digital Signature (DigiCert URLs và timestamps).
    -   **Manifest:** Có `uiAccess="true"` trong manifest (yêu cầu quyền truy cập giao diện mức cao để tương tác với các cửa sổ khác - typical cho keyboard mapping apps).

### B. Dynamic Analysis (Runtime Behavior)
1.  **Process Life-cycle:**
    -   Khởi chạy và giữ process chạy ngầm (Resident Process).
    -   Load các module đồ họa (`uxtheme.dll`, `CoreUIComponents.dll`, `textinputframework.dll`).
    -   Load stack mạng (`WinInet` inferred, `WS2_32`).
2.  **Input/Output:**
    -   Không tạo cửa sổ console.
    -   Tương tác với subsystem Bluetooth ngay khi khởi động.
    -   Tương tác với `TextShaping.dll` (xử lý font/text rendering).

---

## 3. Bản đồ luồng chạy tổng thể (High-level Flow Map)

Ứng dụng hoạt động theo mô hình **Event-Driven Loop** cổ điển của Windows App.

```text
┌───────────────┐
│   Start EXE   │
└───────┬───────┘
        │
        v
    ┌───┴──────────────────────┐
    │ Check Mutex / Instance?  │
    └───────┬──────────┬───────┘
            │          │
   (Exists) │          │ (New)
            v          v
┌───────────────┐  ┌────────────────────────┐
│ Show Existing │  │ Case: New Instance     │
└───────┬───────┘  ├────────────────────────┤
        │          │ [1] Init C++ Runtime   │
        v          │ [2] Load Config        │
    ┌──────┐       │ [3] Init Bluetooth     │
    │ Exit │       └───────────┬────────────┘
    └──────┘                   │
                               v
                   ┌────────────────────────┐
                   │ Start Input Listener   │
                   │ (SetWindowsHookEx)     │
                   └───────────┬────────────┘
                               │
                               v
                   ┌────────────────────────┐
                   │ Create Tray Icon       │
                   │ & Enter Message Loop   │
                   └───────────┬────────────┘
                               │
            ┌──────────────────┴──────────────────┐
            v                                     v
 ┌─────────────────────┐               ┌─────────────────────┐
 │    Input Event      │               │   Hardware Event    │
 ├─────────────────────┤               ├─────────────────────┤
 │ - Detect Hotkey     │               │ - BT Disconnected   │
 │ - Lookup Mapping    │               │ - BT Reconnected    │
 │ - Inject Key stroke │               │ - Update Status     │
 └──────────┬──────────┘               └──────────┬──────────┘
            │                                     │
            └────────► [ EVENT LOOP ] ◄───────────┘
```

---

## 4. Luồng chi tiết theo từng giai đoạn (Detailed Flows)

### Flow A: Startup & Initialization
**Mục đích:** Thiết lập môi trường và kiểm tra thiết bị.
1.  **Entry Point:** `mainCRTStartup` / `WinMain`.
2.  **Runtime Init:** Khởi tạo heap, C++ exceptions (MSVCP).
3.  **Check License:** Kiểm tra trạng thái "Trial" (String evidence). Có thể kết nối mạng (WS2_32) để verify.
4.  **Hardware Scan:**
    -   Gọi `BluetoothFindFirstRadio` để tìm Bluetooth Adapter.
    -   Gọi `SetupDiEnumDeviceInterfaces` để tìm driver bàn phím Apple.
    -   *Trigger:* Ngay khi khởi động.

### Flow B: Input Handling (Trọng tâm)
Do `uiAccess="true"` trong manifest và chức năng remapping, luồng này hoạt động như sau:
1.  **Initialization:** Đăng ký Hooks (khả năng cao là `SetWindowsHookEx(WH_KEYBOARD_LL)` hoặc `RegisterRawInputDevices`).
    -   *Evidence:* Load `textinputframework.dll`, `user32.dll`.
2.  **Detection:**
    -   Khi phím tắt được nhấn (ví dụ Fn + Key), ngắt sự kiện hệ thống.
3.  **Substitution:**
    -   Tra bảng mapping (Layout map).
    -   Gửi phím thay thế (SendInput/PostMessage).

### Flow C: Bluetooth Management
1.  **Monitoring:** Lắng nghe sự kiện kết nối thiết bị.
2.  **Logic:** Tự động áp dụng setting khi Magic Keyboard kết nối lại.
3.  **Battery Check:** Polling trạng thái pin qua HID report (Implied functionality).

---

## 5. Sơ đồ tuần tự (Sequence Diagram) - Startup

```text
User                  EXE Analysis               OS (Windows)           Bluetooth HW
 │                         │                          │                       │
 │ 1. Launch App           │                          │                       │
 ├────────────────────────►│                          │                       │
 │                         │ 2. CreateMutex           │                       │
 │                         ├─────────────────────────►│                       │
 │                         │                          │                       │
 │                         │   < Instance Exists? >   │                       │
 │                         │◄──────────(Yes)──────────┤                       │
 │                         │◄──────────(No)───────────┤                       │
 │                         │                          │                       │
 │                         │ 3. Load Config           │                       │
 │                         ├─(Read Registry/File)────►│                       │
 │                         │                          │                       │
 │                         │ 4. Find Radio            │                       │
 │                         ├─────────────────────────────────────────────────►│
 │                         │                          │                       │
 │                         │ 5. Init Driver           │                       │
 │                         ├─────────────────────────►│                       │
 │                         │                          │                       │
 │                         │ 6. Set Hooks (HookEx)    │                       │
 │                         ├─────────────────────────►│                       │
 │                         │                          │                       │
 │                         │ 7. Shell_NotifyIcon      │                       │
 │                         ├─────────────────────────►│                       │
 │                         │                          │                       │
 │                         │     [ MESSAGE LOOP ]     │                       │
 │                         │◄──────(WM_INPUT)─────────┤                       │
 │                         │                          │                       │
 │                         │ 8. Process Map           │                       │
 │                         ├───(SendInput)───────────►│                       │
```

---

## 6. Bảng I/O (Input/Output Table)

| Loại | Chi tiết | Mục đích |
| :--- | :--- | :--- |
| **Registry** | `HKCU\Software\Magic Utilities` (Dự đoán) | Lưu license, setting cá nhân. |
| **Network** | `digicert.com` (CRL check), Server update | Kiểm tra bản quyền, update. |
| **API Calls** | `BluetoothFindFirstRadio`, `SetupDiEnum...` | Giao tiếp phần cứng cấp thấp. |
| **Process** | `MagicKeyboardUtilities.exe` | Main process (Single instance). |
| **UI** | Tray Icon, Settings Window (WinForms/Native) | Giao diện cấu hình. |

---

## 7. Điểm vào quan trọng (Entry Points)

-   **Main Entry:** Standard PE Entry Point (C++ CRT).
-   **Hardware Entry:** Callback từ Bluetooth stack.
-   **User Entry:** Click chuột vào Tray Icon.

---

## 8. Checklist để tái-implement (Reimplementation Checklist)

Để xây dựng lại phần mềm này (Clone), bạn cần:

-   [ ] **Ngôn ngữ:** C++ (hiệu năng cao nhất cho Hook) hoặc C# (.NET 6+ với P/Invoke).
-   [ ] **Bluetooth Stack:** Sử dụng `Bluetooth APIS` (Windows.Devices.Bluetooth hoặc `bthprops.lib` native).
-   [ ] **Device Enumeration:** Dùng `SetupAPI` để lọc GUID của thiết bị Apple HID.
-   [ ] **Input Interception:**
    -   Cách 1 (Dễ): Dùng `Global Low Level Keyboard Hook` (WH_KEYBOARD_LL).
    -   Cách 2 (Mạnh): Dùng `Raw Input` (WM_INPUT) để phân biệt bàn phím Apple với bàn phím Laptop.
-   [ ] **Admin/UI Access:** Cần Manifest `uiAccess="true"` và ký số (Self-sign certificate) để tương tác với các ứng dụng Admin khác.

---

## 9. Phụ lục: Log/Command Output (Rút gọn)

```text
File Hash: EF22E1004B37CBF2001D64A80DCEDE13AE46B54AE91E43D3CF18B2ED8064FDD8
Version: 3.1.5.6
Copyright: (C) Copyright 2024 Magic Utilities Pty Ltd

Important Imports:
- SetupApi.dll
- bthprops.cpl
- user32.dll (MessageBoxA detected explicitly)
- WS2_32.dll
```
