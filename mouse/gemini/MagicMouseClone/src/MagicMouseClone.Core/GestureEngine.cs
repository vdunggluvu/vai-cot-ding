using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicMouseClone.Core
{
    public class GestureEngine : IGestureEngine
    {
        public event EventHandler<GestureEventArgs>? GestureDetected;

        private TouchFrame? _lastFrame;
        private const float SCROLL_THRESHOLD = 0.05f;
        private const float SWIPE_VELOCITY_THRESHOLD = 1.5f;

        public void ProcessTouchFrame(TouchFrame frame)
        {
            if (_lastFrame == null || frame.Points.Length == 0)
            {
                _lastFrame = frame;
                return;
            }

            // Simple 1-finger scroll detection
            if (frame.Points.Length == 1 && _lastFrame.Points.Length == 1)
            {
                var current = frame.Points[0];
                var last = _lastFrame.Points[0];

                if (current.Id == last.Id)
                {
                    float dy = current.Y - last.Y;
                    float dx = current.X - last.X;

                    // Invert Y for natural scrolling typically
                    if (Math.Abs(dy) > SCROLL_THRESHOLD)
                    {
                        var type = dy > 0 ? GestureType.ScrollDown : GestureType.ScrollUp;
                        GestureDetected?.Invoke(this, new GestureEventArgs(type, Math.Abs(dy)));
                    }
                    else if (Math.Abs(dx) > SCROLL_THRESHOLD)
                    {
                        var type = dx > 0 ? GestureType.ScrollRight : GestureType.ScrollLeft;
                        GestureDetected?.Invoke(this, new GestureEventArgs(type, Math.Abs(dx)));
                    }
                }
            }

            // TODO: Implement Pinch/Swipe logic here

            _lastFrame = frame;
        }
    }
}
