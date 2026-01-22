namespace MagicMouseClone.Core.Models;

/// <summary>
/// Cấu hình ứng dụng
/// Evidence: High - Từ section 7.6
/// </summary>
public class AppConfig
{
    public bool AutoStart { get; set; } = false;
    public bool EnableGestures { get; set; } = true;
    public float ScrollSpeed { get; set; } = 5.0f;
    public float GestureSensitivity { get; set; } = 5.0f;
    public string ActiveProfile { get; set; } = "Default";
    public bool ShowNotifications { get; set; } = true;
    public int BatteryWarningThreshold { get; set; } = 20;
}

/// <summary>
/// Trạng thái thiết bị
/// </summary>
public enum DeviceState
{
    Disconnected,
    Searching,
    Connecting,
    Connected,
    Error
}

/// <summary>
/// Thông tin thiết bị
/// Evidence: Medium - Suy luận từ section 5 & 7.2
/// </summary>
public class DeviceInfo
{
    public string DeviceName { get; set; } = "Unknown";
    public string DevicePath { get; set; } = "";
    public ushort VendorId { get; set; }
    public ushort ProductId { get; set; }
    public int BatteryLevel { get; set; } = -1;
    public DeviceState State { get; set; } = DeviceState.Disconnected;
    public string? LastConnectedMAC { get; set; }
}
