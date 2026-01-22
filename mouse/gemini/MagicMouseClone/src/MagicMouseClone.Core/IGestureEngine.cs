using System;

namespace MagicMouseClone.Core
{
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
        PinchOut
    }

    public class GestureEventArgs : EventArgs
    {
        public GestureType Type { get; }
        public float Magnitude { get; }

        public GestureEventArgs(GestureType type, float magnitude = 1.0f)
        {
            Type = type;
            Magnitude = magnitude;
        }
    }

    public interface IGestureEngine
    {
        event EventHandler<GestureEventArgs> GestureDetected;
        void ProcessTouchFrame(TouchFrame frame);
    }

    public struct TouchPoint
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool IsDown { get; set; }
    }

    public class TouchFrame
    {
        public TouchPoint[] Points { get; set; } = Array.Empty<TouchPoint>();
        public long Timestamp { get; set; }
    }
}
