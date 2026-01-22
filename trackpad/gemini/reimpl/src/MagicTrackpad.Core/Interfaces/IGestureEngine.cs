using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Interfaces;

public interface IGestureEngine
{
    event EventHandler<Gesture> GestureDetected;
    void ProcessFrame(TouchFrame frame);
    void Reset();
}
