using MagicTrackpad.Core.Interfaces;
using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Services;

public class GestureStateMachine : IGestureEngine
{
    public event EventHandler<Gesture> GestureDetected;

    private int _prevFingerCount = 0;
    private DateTime _touchStartTime;
    private bool _possibleTap = false;

    public void ProcessFrame(TouchFrame frame)
    {
        int fingerCount = frame.Points.Where(p => p.IsContact).Count();

        // Simple Tap Detection Logic
        if (fingerCount > 0 && _prevFingerCount == 0)
        {
            // Touch Down
            _touchStartTime = frame.Timestamp;
            _possibleTap = true;
        }
        else if (fingerCount == 0 && _prevFingerCount > 0)
        {
            // Touch Up
            if (_possibleTap)
            {
                var duration = (frame.Timestamp - _touchStartTime).TotalMilliseconds;
                if (duration < 300) // 300ms tap threshold
                {
                     // Determine tap type based on max fingers seen (simplification)
                     // Here we assume _prevFingerCount was the max.
                     if (_prevFingerCount == 1) 
                         Emit(GestureType.Tap);
                     else if (_prevFingerCount == 2)
                         Emit(GestureType.TwoFingerTap);
                }
            }
            _possibleTap = false;
        }
        
        // TODO: Implement Scroll/Swipe logic here
        
        _prevFingerCount = fingerCount;
    }

    private void Emit(GestureType type)
    {
        GestureDetected?.Invoke(this, new Gesture { Type = type, Timestamp = DateTime.UtcNow });
    }

    public void Reset()
    {
        _prevFingerCount = 0;
        _possibleTap = false;
    }
}
