using MagicMouseClone.Core.Models;

namespace MagicMouseClone.Core.Interfaces;

/// <summary>
/// Interface cho gesture engine
/// Evidence: High - Từ section 7.4
/// </summary>
public interface IGestureEngine
{
    event EventHandler<GestureEvent>? GestureDetected;

    void ProcessTouchFrame(TouchFrame frame);
    void SetSensitivity(float sensitivity);
    void ResetState();
}
