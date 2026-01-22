namespace MagicTrackpadClone.Core;

public class GestureEngine : IGestureEngine
{
    private readonly ILogger _logger;
    private GestureConfiguration _config = new();
    private readonly List<TouchPoint> _previousTouches = new();
    private DateTime _lastEventTime = DateTime.Now;
    private readonly Dictionary<int, List<TouchPoint>> _touchHistory = new();

    public event EventHandler<GestureEventArgs>? GestureDetected;

    public GestureEngine(ILogger logger)
    {
        _logger = logger;
    }

    public void LoadConfiguration(GestureConfiguration config)
    {
        _config = config;
        _logger.LogInfo($"Loaded gesture configuration with {config.Mappings.Count} mappings");
    }

    public void ProcessInput(InputEventArgs inputEvent)
    {
        try
        {
            var touchCount = inputEvent.TouchPoints.Count;
            var timeDelta = (inputEvent.Timestamp - _lastEventTime).TotalMilliseconds;
            
            if (touchCount == 0)
            {
                // Touch released - analyze gesture
                AnalyzeGesture();
                _touchHistory.Clear();
            }
            else
            {
                // Track touch points
                foreach (var touch in inputEvent.TouchPoints)
                {
                    if (!_touchHistory.ContainsKey(touch.ContactId))
                    {
                        _touchHistory[touch.ContactId] = new List<TouchPoint>();
                    }
                    _touchHistory[touch.ContactId].Add(touch);
                }
                
                // Detect ongoing gestures
                DetectGestureInProgress(inputEvent.TouchPoints);
            }
            
            _previousTouches.Clear();
            _previousTouches.AddRange(inputEvent.TouchPoints);
            _lastEventTime = inputEvent.Timestamp;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing input", ex);
        }
    }

    private void AnalyzeGesture()
    {
        if (_touchHistory.Count == 0) return;

        var touchCount = _touchHistory.Count;
        
        // Detect tap
        if (IsTapGesture())
        {
            EmitGesture(GestureType.Tap, touchCount, GestureDirection.None);
            return;
        }
        
        // Detect swipe
        var (isSwipe, direction, velocity, distance) = DetectSwipe();
        if (isSwipe)
        {
            EmitGesture(GestureType.Swipe, touchCount, direction, velocity, distance);
            return;
        }
    }

    private bool IsTapGesture()
    {
        // Simple tap detection: short duration, minimal movement
        foreach (var history in _touchHistory.Values)
        {
            if (history.Count < 2) continue;
            
            var distance = Math.Sqrt(
                Math.Pow(history[^1].X - history[0].X, 2) +
                Math.Pow(history[^1].Y - history[0].Y, 2)
            );
            
            if (distance > 0.05) return false; // Moved too much
        }
        
        return true;
    }

    private (bool isSwipe, GestureDirection direction, double velocity, double distance) DetectSwipe()
    {
        if (_touchHistory.Count == 0) return (false, GestureDirection.None, 0, 0);

        var firstHistory = _touchHistory.Values.First();
        if (firstHistory.Count < 3) return (false, GestureDirection.None, 0, 0);

        var deltaX = firstHistory[^1].X - firstHistory[0].X;
        var deltaY = firstHistory[^1].Y - firstHistory[0].Y;
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        if (distance < 0.1) return (false, GestureDirection.None, 0, 0);

        var direction = Math.Abs(deltaX) > Math.Abs(deltaY)
            ? (deltaX > 0 ? GestureDirection.Right : GestureDirection.Left)
            : (deltaY > 0 ? GestureDirection.Down : GestureDirection.Up);

        var velocity = distance / firstHistory.Count;
        
        return (true, direction, velocity, distance);
    }

    private void DetectGestureInProgress(List<TouchPoint> currentTouches)
    {
        if (currentTouches.Count == 2 && _previousTouches.Count == 2)
        {
            // Detect scroll (two-finger drag)
            var deltaY = (currentTouches[0].Y + currentTouches[1].Y) / 2 - 
                        (_previousTouches[0].Y + _previousTouches[1].Y) / 2;
            
            if (Math.Abs(deltaY) > 0.01)
            {
                var direction = deltaY > 0 ? GestureDirection.Down : GestureDirection.Up;
                EmitGesture(GestureType.Scroll, 2, direction, Math.Abs(deltaY) * 100);
            }
        }
    }

    private void EmitGesture(GestureType type, int fingerCount, GestureDirection direction, 
        double velocity = 0, double distance = 0)
    {
        var gestureEvent = new GestureEventArgs
        {
            Type = type,
            FingerCount = fingerCount,
            Direction = direction,
            Velocity = velocity,
            Distance = distance,
            Timestamp = DateTime.Now
        };
        
        _logger.LogDebug($"Detected gesture: {type} ({fingerCount} fingers) {direction}");
        GestureDetected?.Invoke(this, gestureEvent);
    }
}
