using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MagicMouseClone.Core.Services;

/// <summary>
/// Action mapper and executor
/// Evidence: High - Based on section 7.5 & 9.3
/// </summary>
public class ActionMapper : IActionMapper
{
    private readonly ILogger<ActionMapper> _logger;
    private ActionProfile _currentProfile;

    public ActionMapper(ILogger<ActionMapper> logger)
    {
        _logger = logger;
        _currentProfile = CreateDefaultProfile();
    }

    public void LoadProfile(ActionProfile profile)
    {
        _currentProfile = profile;
        _logger.LogInformation("Loaded profile: {ProfileName}", profile.Name);
    }

    public ActionProfile GetCurrentProfile()
    {
        return _currentProfile;
    }

    public async Task ExecuteActionAsync(GestureEvent gestureEvent, CancellationToken cancellationToken = default)
    {
        var binding = _currentProfile.GetBinding(gestureEvent.Type);
        if (binding == null)
        {
            _logger.LogDebug("No binding for gesture: {Gesture}", gestureEvent.Type);
            return;
        }

        _logger.LogInformation("Executing action: {ActionType} for gesture: {Gesture}",
            binding.ActionType, gestureEvent.Type);

        try
        {
            switch (binding.ActionType)
            {
                case ActionType.KeyboardShortcut:
                    await ExecuteKeyboardShortcutAsync(binding.ActionParameter, cancellationToken);
                    break;

                case ActionType.MouseEvent:
                    await ExecuteMouseEventAsync(binding.ActionParameter, gestureEvent.Parameter, cancellationToken);
                    break;

                case ActionType.ExecuteApp:
                    await ExecuteAppAsync(binding.ActionParameter, cancellationToken);
                    break;

                case ActionType.SystemCommand:
                    await ExecuteSystemCommandAsync(binding.ActionParameter, cancellationToken);
                    break;

                case ActionType.MediaControl:
                    await ExecuteMediaControlAsync(binding.ActionParameter, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unknown action type: {ActionType}", binding.ActionType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action: {ActionType}", binding.ActionType);
        }
    }

    private Task ExecuteKeyboardShortcutAsync(string shortcut, CancellationToken cancellationToken)
    {
        // In real implementation, use SendInput API
        _logger.LogInformation("Would send keyboard shortcut: {Shortcut}", shortcut);
        return Task.CompletedTask;
    }

    private Task ExecuteMouseEventAsync(string eventType, float parameter, CancellationToken cancellationToken)
    {
        // In real implementation, use SendInput API with MOUSEEVENTF_WHEEL
        _logger.LogInformation("Would send mouse event: {EventType} with parameter: {Parameter}",
            eventType, parameter);
        return Task.CompletedTask;
    }

    private async Task ExecuteAppAsync(string appPath, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo);
            _logger.LogInformation("Launched app: {AppPath}", appPath);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch app: {AppPath}", appPath);
        }
    }

    private Task ExecuteSystemCommandAsync(string command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Would execute system command: {Command}", command);
        return Task.CompletedTask;
    }

    private Task ExecuteMediaControlAsync(string control, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Would execute media control: {Control}", control);
        return Task.CompletedTask;
    }

    private static ActionProfile CreateDefaultProfile()
    {
        var profile = new ActionProfile { Name = "Default" };

        // Default bindings
        profile.SetBinding(GestureType.ScrollUp,
            new ActionBinding(GestureType.ScrollUp, ActionType.MouseEvent, "WheelUp"));
        profile.SetBinding(GestureType.ScrollDown,
            new ActionBinding(GestureType.ScrollDown, ActionType.MouseEvent, "WheelDown"));
        profile.SetBinding(GestureType.SwipeLeft,
            new ActionBinding(GestureType.SwipeLeft, ActionType.KeyboardShortcut, "Browser_Back"));
        profile.SetBinding(GestureType.SwipeRight,
            new ActionBinding(GestureType.SwipeRight, ActionType.KeyboardShortcut, "Browser_Forward"));

        return profile;
    }
}
