namespace MagicMouseClone.Core.Models;

/// <summary>
/// Các loại cử chỉ có thể phát hiện được
/// Evidence: Medium - Suy luận từ flow md section 7.4
/// </summary>
public enum GestureType
{
    None,
    ScrollUp,
    ScrollDown,
    ScrollLeft,
    ScrollRight,
    SwipeLeft,
    SwipeRight,
    SwipeUp,
    SwipeDown,
    PinchIn,
    PinchOut,
    RotateClockwise,
    RotateCounterClockwise,
    TapSingle,
    TapDouble,
    TapTriple
}

/// <summary>
/// Sự kiện cử chỉ đã được phát hiện
/// </summary>
public record GestureEvent(
    GestureType Type,
    float Parameter,
    DateTime Timestamp
);

/// <summary>
/// Một điểm chạm trên bề mặt chuột
/// Evidence: Low - Suy luận từ section 9.1
/// </summary>
public record TouchPoint(
    byte Id,
    float X,
    float Y,
    DateTime Timestamp
);

/// <summary>
/// Frame chứa tất cả touch points tại một thời điểm
/// </summary>
public record TouchFrame(
    IReadOnlyList<TouchPoint> Touches,
    DateTime FrameTime
);
