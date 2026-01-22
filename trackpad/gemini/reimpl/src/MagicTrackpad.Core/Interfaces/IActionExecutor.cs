using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Interfaces;

public interface IActionExecutor
{
    void Execute(Gesture gesture);
    void EnableMockMode(bool enabled);
}
