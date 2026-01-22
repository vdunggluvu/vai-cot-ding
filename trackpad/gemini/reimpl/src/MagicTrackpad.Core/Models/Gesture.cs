namespace MagicTrackpad.Core.Models;

public enum GestureType
{
    None = 0,
    Tap,
    DoubleTap,
    TwoFingerTap,
    ScrollVertical,
    ScrollHorizontal,
    SwipeThreeFingerUp,
    SwipeThreeFingerDown,
    SwipeThreeFingerLeft,
    SwipeThreeFingerRight,
    Pinch,
    Rotate
}

public class Gesture
{
    public GestureType Type { get; set; }
    public double Velocity { get; set; } // For swipes
    public double Scale { get; set; }    // For pinch
    public double Rotation { get; set; } // For rotate
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
