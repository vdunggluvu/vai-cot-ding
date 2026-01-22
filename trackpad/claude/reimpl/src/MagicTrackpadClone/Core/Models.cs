namespace MagicTrackpadClone.Core;

public class DeviceInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DevicePath { get; set; } = string.Empty;
    public ushort VendorId { get; set; }
    public ushort ProductId { get; set; }
    public DeviceConnectionType ConnectionType { get; set; }
    public bool IsConnected { get; set; }
}

public enum DeviceConnectionType
{
    Unknown,
    Usb,
    Bluetooth
}

public class InputEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public List<TouchPoint> TouchPoints { get; set; } = new();
    public ButtonState ButtonState { get; set; } = new();
    public byte[] RawData { get; set; } = Array.Empty<byte>();
}

public class TouchPoint
{
    public int ContactId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Pressure { get; set; }
    public TouchState State { get; set; }
}

public enum TouchState
{
    None,
    Down,
    Move,
    Up
}

public class ButtonState
{
    public bool LeftButton { get; set; }
    public bool RightButton { get; set; }
    public bool MiddleButton { get; set; }
}

public class GestureEventArgs : EventArgs
{
    public GestureType Type { get; set; }
    public int FingerCount { get; set; }
    public GestureDirection Direction { get; set; }
    public double Velocity { get; set; }
    public double Distance { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum GestureType
{
    Unknown,
    Tap,
    Swipe,
    Scroll,
    Pinch,
    Rotate,
    LongPress
}

public enum GestureDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}

public class GestureAction
{
    public ActionType Type { get; set; }
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public enum ActionType
{
    None,
    KeyboardShortcut,
    MouseClick,
    MouseScroll,
    SystemCommand,
    ApplicationCommand
}

public class AppConfiguration
{
    public GeneralSettings General { get; set; } = new();
    public GestureConfiguration Gestures { get; set; } = new();
    public DeviceSettings Device { get; set; } = new();
}

public class GeneralSettings
{
    public bool StartWithWindows { get; set; } = true;
    public bool StartMinimized { get; set; } = true;
    public bool EnableGestures { get; set; } = true;
    public string LogLevel { get; set; } = "Info";
}

public class GestureConfiguration
{
    public Dictionary<string, GestureMapping> Mappings { get; set; } = new();
}

public class GestureMapping
{
    public GestureType Type { get; set; }
    public int FingerCount { get; set; }
    public GestureDirection Direction { get; set; }
    public GestureAction Action { get; set; } = new();
    public bool Enabled { get; set; } = true;
}

public class DeviceSettings
{
    public string? PreferredDeviceId { get; set; }
    public double Sensitivity { get; set; } = 1.0;
    public bool ReverseScrollDirection { get; set; }
}
