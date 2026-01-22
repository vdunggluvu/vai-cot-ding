# Hướng Dẫn Kiểm Thử (Manual Testing Guide)

Tài liệu này hướng dẫn chi tiết quy trình kiểm thử thủ công (Manual Test) cho ứng dụng **MagicKeyboardUtilities (Clone)**.

## 1. Chuẩn Bị Môi Trường

### 1.1. Build Ứng Dụng
Mở PowerShell tại thư mục gốc của dự án và chạy:
```powershell
./scripts/build.ps1
```
Đảm bảo build thành công (Exit code: 0).

### 1.2. Reset Config
Trước khi test, đảm bảo file `config.json` (nằm cùng thư mục với file `.exe` sau khi build, thường là `src\MagicKeyboardUtilities.App\bin\Release\net8.0-windows\`) có nội dung mặc định hoặc reset về:
```json
{
  "app": {
    "startMinimized": true,
    "autoSave": true
  },
  "features": {
    "trayIcon": true,
    "enableHooks": false,
    "enableHotkeys": false,
    "enableDeviceMonitor": true,
    "enableSendInput": false
  },
  "remapping": [
    { "fromVk": 65, "toVk": 66, "description": "A -> B (Test)" }
  ],
  "hotkeys": [
    { "id": 1, "modifiers": ["Control", "Alt"], "vk": 75, "action": "ToggleEnabled" }
  ]
}
```
*Lưu ý: `fromVk: 65` là phím 'A', `toVk: 66` là phím 'B'. `vk: 75` là phím 'K'.*

---

## 2. Các Kịch Bản Test (Test Scenarios)

### Scenario 1: Basic Lifecycle & Tray Icon
**Mục tiêu:** Kiểm tra ứng dụng khởi chạy, hiện Tray Icon và thoát đúng cách.

1.  **Chạy App:** `./scripts/run.ps1`
2.  **Quan Sát:**
    *   Không có cửa sổ nào hiện lên (đúng yêu cầu background).
    *   Kiểm tra System Tray (góc dưới phải màn hình, mũi tên ^): Có icon hình vuông/application mặc định.
    *   Hover chuột vào icon: Thấy tooltip "Magic Keyboard Utilities: Disabled".
3.  **Tương tác Menu:**
    *   Chuột phải vào icon -> Chọn **Settings...** -> Hiện thông báo "Configuration loaded...".
    *   Chuột phải vào icon -> Chọn **Exit**.
4.  **Kết quả mong đợi:** Icon biến mất, process kết thúc hoàn toàn (check Task Manager nếu cần).

### Scenario 2: Hotkey Toggle
**Mục tiêu:** Kiểm tra chức năng đăng ký Hotkey toàn cục (Ctrl+Alt+K).

1.  **Cấu hình:** Sửa `config.json` -> `"enableHotkeys": true`.
2.  **Chạy App.**
3.  **Thao tác:**
    *   Nhấn tổ hợp phím **Ctrl + Alt + K**.
    *   Quan sát Console Log (cửa sổ chạy script) hoặc file `logs/app.log`.
    *   Kiểm tra Tray Icon Tooltip: Trạng thái sẽ đổi từ Disabled sang Enabled (hoặc ngược lại nếu có logic toggle connected). *Lưu ý: Trong phiên bản hiện tại, hotkey chỉ log ra console "Hotkey detected: 1", cần verify log.*
4.  **Kết quả mong đợi:** Log xuất hiện dòng thông báo nhận Hotkey.

### Scenario 3: Keyboard Remapping (An Toàn - Log Only)
**Mục tiêu:** Kiểm tra Hook phím hoạt động nhận diện đúng phím, nhưng **không** gây loạn phím (Safety First).

1.  **Cấu hình:** Sửa `config.json`:
    *   `"enableHooks": true`
    *   `"enableSendInput": false` (QUAN TRỌNG: Giữ false để chỉ Log, không thay đổi phím thật).
2.  **Chạy App.**
3.  **Thao tác:**
    *   Mở Notepad.
    *   Gõ phím **'A'**.
4.  **Quan Sát:**
    *   Trên Notepad vẫn hiện chữ 'a' (vì `enableSendInput` = false).
    *   Trên cửa sổ Console/Log: Hiện dòng `[Hook] Remapped VK 65 -> 66`.
5.  **Kết quả mong đợi:** Hook bắt được phím A và log đúng mapping target là 66 (B).

### Scenario 4: Keyboard Remapping (Active - Cảnh Báo)
**Mục tiêu:** Kiểm tra chức năng thay đổi phím thực tế.
**Cảnh báo:** Có thể gây khó gõ phím nếu cấu hình sai.

1.  **Cấu hình:** Sửa `config.json`:
    *   `"enableHooks": true`
    *   `"enableSendInput": true`
2.  **Chạy App.**
3.  **Thao tác:**
    *   Mở Notepad.
    *   Gõ phím **'A'**.
4.  **Quan Sát:**
    *   Trên Notepad hiện chữ **'b'** (hoặc không hiện gì nếu Code chặn 'A' và gửi 'B' nhưng máy chưa nhận kịp, tùy tốc độ máy ảo).
    *   *Lưu ý: Logic hiện tại trong code mẫu trả về `(IntPtr)1` để chặn phím gốc nếu SendInput được bật.*
5.  **Restore:** Tắt app bằng Tray Icon -> Exit ngay sau khi test xong.

### Scenario 5: Device Detection (Stub)
**Mục tiêu:** Verify app lắng nghe được sự kiện cắm thiết bị USB.

1.  **Cấu hình:** `"enableDeviceMonitor": true`.
2.  **Chạy App.**
3.  **Thao tác:**
    *   Cắm hoặc rút một thiết bị USB bất kỳ (chuột, phím, USB drive...).
4.  **Quan Sát:**
    *   Log Console xuất hiện: `Device change detected: Event 8000` (Arrival) hoặc `8004` (Removal).
5.  **Kết quả mong đợi:** App phản hồi với sự kiện phần cứng.

---

## 3. Khắc Phục Sự Cố (Troubleshooting)

*   **Không chạy được?**
    *   Kiểm tra `.NET 8 SDK`: `dotnet --version`.
    *   Kiểm tra file `config.json` có đúng cú pháp JSON không.
*   **Không thấy log?**
    *   Log file nằm tại `./logs/app.log`.
*   **App không thoát hẳn?**
    *   Mở Task Manager, tìm `MagicKeyboardUtilities.App.exe` và End Task.
    *   Hoặc chạy lệnh: `Stop-Process -Name "MagicKeyboardUtilities.App" -Force` trong PowerShell.
