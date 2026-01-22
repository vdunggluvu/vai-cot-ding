using MagicMouseClone.Core.Models;

namespace MagicMouseClone.Core.Interfaces;

/// <summary>
/// Interface cho action mapper
/// Evidence: High - Từ section 7.5
/// </summary>
public interface IActionMapper
{
    void LoadProfile(ActionProfile profile);
    ActionProfile GetCurrentProfile();
    Task ExecuteActionAsync(GestureEvent gestureEvent, CancellationToken cancellationToken = default);
}
