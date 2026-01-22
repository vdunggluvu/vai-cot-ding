using MagicTrackpad.Core.Interfaces;
using MagicTrackpad.Core.Models;
using System.Diagnostics;

namespace MagicTrackpad.Core.Services;

public class MockActionExecutor : IActionExecutor
{
    private bool _mockMode = true;
    public List<Gesture> ExecutedGestures { get; } = new();

    public void EnableMockMode(bool enabled)
    {
        _mockMode = enabled;
    }

    public void Execute(Gesture gesture)
    {
        ExecutedGestures.Add(gesture);
        Debug.WriteLine($"[ActionExecutor] Executing {gesture.Type}");
        
        if (!_mockMode)
        {
            // Real SendInput implementation would go here
            // user32.dll SendInput...
        }
    }
}
