using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;

namespace MagicMouseClone.Core.Services;

/// <summary>
/// Gesture recognition engine
/// Evidence: Medium - Algorithm patterns from section 9.2
/// </summary>
public class GestureEngine : IGestureEngine
{
    private readonly ILogger<GestureEngine> _logger;
    private readonly List<TouchPoint> _touchHistory = new();
    private float _sensitivity = 5.0f;
    private const int MaxHistorySize = 20;
    private const float ScrollVelocityThreshold = 1.5f;
    private const float SwipeVelocityThreshold = 3.0f;

    public event EventHandler<GestureEvent>? GestureDetected;

    public GestureEngine(ILogger<GestureEngine> logger)
    {
        _logger = logger;
    }

    public void ProcessTouchFrame(TouchFrame frame)
    {
        if (frame.Touches.Count == 0)
        {
            return;
        }

        // Add to history
        foreach (var touch in frame.Touches)
        {
            _touchHistory.Add(touch);
        }

        // Trim history
        if (_touchHistory.Count > MaxHistorySize)
        {
            _touchHistory.RemoveRange(0, _touchHistory.Count - MaxHistorySize);
        }

        // Detect gesture if enough history
        if (_touchHistory.Count >= 3)
        {
            var gesture = DetectGesture();
            if (gesture != GestureType.None)
            {
                var gestureEvent = new GestureEvent(gesture, _sensitivity, DateTime.UtcNow);
                GestureDetected?.Invoke(this, gestureEvent);
                _logger.LogDebug("Gesture detected: {Gesture}", gesture);

                // Clear history after detection
                _touchHistory.Clear();
            }
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        _sensitivity = Math.Clamp(sensitivity, 1.0f, 10.0f);
        _logger.LogInformation("Gesture sensitivity set to {Sensitivity}", _sensitivity);
    }

    public void ResetState()
    {
        _touchHistory.Clear();
        _logger.LogDebug("Gesture engine state reset");
    }

    private GestureType DetectGesture()
    {
        if (_touchHistory.Count < 3)
        {
            return GestureType.None;
        }

        // Simple gesture detection based on movement direction and velocity
        var first = _touchHistory[0];
        var last = _touchHistory[^1];

        var deltaX = last.X - first.X;
        var deltaY = last.Y - first.Y;
        var distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

        var duration = (last.Timestamp - first.Timestamp).TotalSeconds;
        if (duration <= 0)
        {
            return GestureType.None;
        }

        var velocity = distance / (float)duration;

        // Determine primary direction
        var absX = MathF.Abs(deltaX);
        var absY = MathF.Abs(deltaY);

        if (velocity < ScrollVelocityThreshold)
        {
            // Slow movement = scroll
            if (absY > absX)
            {
                return deltaY > 0 ? GestureType.ScrollDown : GestureType.ScrollUp;
            }
            else
            {
                return deltaX > 0 ? GestureType.ScrollRight : GestureType.ScrollLeft;
            }
        }
        else if (velocity >= SwipeVelocityThreshold)
        {
            // Fast movement = swipe
            if (absY > absX)
            {
                return deltaY > 0 ? GestureType.SwipeDown : GestureType.SwipeUp;
            }
            else
            {
                return deltaX > 0 ? GestureType.SwipeRight : GestureType.SwipeLeft;
            }
        }

        return GestureType.None;
    }
}
