using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MagicKeyboardUtilities.App.Core;

/// <summary>
/// Action dispatcher - executes commands from hotkeys/menu
/// Traceability: Section 4.2 INPUT HOOK FLOW Step 4 "Action Execution"
/// </summary>
public class ActionDispatcher
{
    private readonly ILogger<ActionDispatcher> _logger;
    private readonly Dictionary<string, Action> _actions;

    public ActionDispatcher(ILogger<ActionDispatcher> logger)
    {
        _logger = logger;
        _actions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Register an action handler
    /// </summary>
    public void RegisterAction(string actionName, Action handler)
    {
        _actions[actionName] = handler;
        _logger.LogDebug("Registered action: {Action}", actionName);
    }

    /// <summary>
    /// Execute an action by name
    /// </summary>
    public bool Execute(string actionName)
    {
        if (_actions.TryGetValue(actionName, out var handler))
        {
            try
            {
                _logger.LogInformation("Executing action: {Action}", actionName);
                handler.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action: {Action}", actionName);
                return false;
            }
        }
        else
        {
            _logger.LogWarning("Unknown action: {Action}", actionName);
            return false;
        }
    }

    /// <summary>
    /// Get all registered action names
    /// </summary>
    public IEnumerable<string> GetRegisteredActions() => _actions.Keys;
}
