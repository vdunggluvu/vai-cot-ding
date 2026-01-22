using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Interfaces;

public interface IInputSource
{
    event EventHandler<TouchFrame> TouchDataReceived;
    void Start();
    void Stop();
    
    // For testing/injection
    void PushFrame(TouchFrame frame);
}
