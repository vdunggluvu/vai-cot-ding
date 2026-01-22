using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MagicKeyboardUtilities.App.Config;

/// <summary>
/// Root configuration object loaded from config.json
/// Traceability: Section 4.4 Configuration Flow, Section 8.1 Core Requirements Phase 3
/// </summary>
public class AppConfig
{
    [JsonPropertyName("app")]
    public AppSettings App { get; set; } = new();

    [JsonPropertyName("features")]
    public FeatureSettings Features { get; set; } = new();

    [JsonPropertyName("remapping")]
    public List<KeyRemapping> Remapping { get; set; } = new();

    [JsonPropertyName("hotkeys")]
    public List<HotkeyDefinition> Hotkeys { get; set; } = new();

    [JsonPropertyName("device")]
    public DeviceSettings Device { get; set; } = new();
}

public class AppSettings
{
    [JsonPropertyName("startMinimized")]
    public bool StartMinimized { get; set; } = true;

    [JsonPropertyName("autoSave")]
    public bool AutoSave { get; set; } = true;

    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "Information";
}

public class FeatureSettings
{
    [JsonPropertyName("trayIcon")]
    public bool TrayIcon { get; set; } = true;

    /// <summary>
    /// INFERRED from flow report 4.2 INPUT HOOK FLOW
    /// Evidence: "Hook implementation bị ẩn trong protected code"
    /// Status: Default OFF for safety
    /// </summary>
    [JsonPropertyName("enableHooks")]
    public bool EnableHooks { get; set; } = false;

    /// <summary>
    /// INFERRED from flow report 4.2 INPUT HOOK FLOW
    /// Evidence: Speculative hotkey functionality
    /// Status: Default OFF for safety
    /// </summary>
    [JsonPropertyName("enableHotkeys")]
    public bool EnableHotkeys { get; set; } = false;

    /// <summary>
    /// INFERRED from flow report 4.5 DEVICE DETECTION FLOW
    /// Evidence: "Detect khi Apple Magic Keyboard được connect/disconnect"
    /// Status: Default OFF, stub implementation
    /// </summary>
    [JsonPropertyName("enableDeviceMonitor")]
    public bool EnableDeviceMonitor { get; set; } = false;

    /// <summary>
    /// From flow report 4.7 UPDATE FLOW
    /// Evidence: "Không thấy network activity trong runtime analysis"
    /// Status: Disabled by default, stub only
    /// </summary>
    [JsonPropertyName("enableUpdater")]
    public bool EnableUpdater { get; set; } = false;

    /// <summary>
    /// Controls whether remapping sends input or just logs
    /// Safety feature for testing
    /// </summary>
    [JsonPropertyName("enableSendInput")]
    public bool EnableSendInput { get; set; } = false;
}

/// <summary>
/// Key remapping definition
/// Traceability: Section 4.2 INPUT HOOK FLOW Step 3, Section 8.3 Key Implementation Details
/// Note: Actual mappings are EXAMPLES only - flow report shows encrypted config
/// </summary>
public class KeyRemapping
{
    [JsonPropertyName("fromVk")]
    public int FromVk { get; set; }

    [JsonPropertyName("toVk")]
    public int ToVk { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Hotkey definition
/// Traceability: Section 4.2 INPUT HOOK FLOW, RegisterHotKey implementation
/// </summary>
public class HotkeyDefinition
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; set; } = new();

    [JsonPropertyName("vk")]
    public int Vk { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class DeviceSettings
{
    /// <summary>
    /// From flow report 4.5 DEVICE DETECTION FLOW
    /// Evidence: "check Apple VID 0x05AC"
    /// </summary>
    [JsonPropertyName("appleVendorId")]
    public string AppleVendorId { get; set; } = "0x05AC";

    [JsonPropertyName("notifyOnConnect")]
    public bool NotifyOnConnect { get; set; } = true;

    [JsonPropertyName("notifyOnDisconnect")]
    public bool NotifyOnDisconnect { get; set; } = true;
}
