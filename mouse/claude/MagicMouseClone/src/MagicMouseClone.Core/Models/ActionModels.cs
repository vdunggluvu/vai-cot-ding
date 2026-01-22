namespace MagicMouseClone.Core.Models;

/// <summary>
/// Loại hành động có thể thực thi
/// Evidence: High - Từ section 7.5
/// </summary>
public enum ActionType
{
    None,
    KeyboardShortcut,
    MouseEvent,
    ExecuteApp,
    SystemCommand,
    MediaControl
}

/// <summary>
/// Binding giữa cử chỉ và hành động
/// </summary>
public record ActionBinding(
    GestureType Gesture,
    ActionType ActionType,
    string ActionParameter,
    bool RequiresModifier = false
);

/// <summary>
/// Profile chứa tập hợp các action bindings
/// </summary>
public class ActionProfile
{
    public string Name { get; set; } = "Default";
    public Dictionary<GestureType, ActionBinding> Bindings { get; set; } = new();

    public ActionBinding? GetBinding(GestureType gesture)
    {
        return Bindings.TryGetValue(gesture, out var binding) ? binding : null;
    }

    public void SetBinding(GestureType gesture, ActionBinding binding)
    {
        Bindings[gesture] = binding;
    }
}
