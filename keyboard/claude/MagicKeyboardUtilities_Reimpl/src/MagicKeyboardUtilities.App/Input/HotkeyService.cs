using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Core;
using MagicKeyboardUtilities.App.Interop;

namespace MagicKeyboardUtilities.App.Input;

/// <summary>
/// Hotkey registration service
/// Traceability: Section 4.2 INPUT HOOK FLOW, Section 8.1 Phase 4
/// Evidence: RegisterHotKey API usage (inferred)
/// Status: INFERRED - default OFF
/// </summary>
public class HotkeyService : IDisposable
{
    private readonly ILogger<HotkeyService> _logger;
    private readonly ActionDispatcher _actionDispatcher;
    private readonly Dictionary<int, HotkeyDefinition> _registeredHotkeys;
    private IntPtr _windowHandle;

    public HotkeyService(ILogger<HotkeyService> logger, ActionDispatcher actionDispatcher)
    {
        _logger = logger;
        _actionDispatcher = actionDispatcher;
        _registeredHotkeys = new Dictionary<int, HotkeyDefinition>();
    }

    /// <summary>
    /// Set the window handle for hotkey registration
    /// </summary>
    public void SetWindowHandle(IntPtr handle)
    {
        _windowHandle = handle;
        _logger.LogDebug("Window handle set for hotkeys: {Handle}", handle);
    }

    /// <summary>
    /// Register hotkeys from configuration
    /// </summary>
    public void RegisterHotkeys(List<HotkeyDefinition> hotkeys)
    {
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.LogWarning("Cannot register hotkeys without window handle");
            return;
        }

        foreach (var hotkey in hotkeys)
        {
            RegisterHotkey(hotkey);
        }
    }

    /// <summary>
    /// Register a single hotkey
    /// Traceability: Section 4.2 Step 1, RegisterHotKey API
    /// </summary>
    public bool RegisterHotkey(HotkeyDefinition hotkey)
    {
        if (_windowHandle == IntPtr.Zero)
        {
            _logger.LogWarning("Cannot register hotkey without window handle");
            return false;
        }

        try
        {
            uint modifiers = ParseModifiers(hotkey.Modifiers);
            modifiers |= NativeMethods.MOD_NOREPEAT; // Prevent repeated firing

            var result = NativeMethods.RegisterHotKey(_windowHandle, hotkey.Id, modifiers, (uint)hotkey.Vk);
            
            if (result)
            {
                _registeredHotkeys[hotkey.Id] = hotkey;
                _logger.LogInformation("Registered hotkey ID {Id}: {Desc}", hotkey.Id, hotkey.Description);
                return true;
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError("Failed to register hotkey ID {Id}, error: {Error}", hotkey.Id, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception registering hotkey ID {Id}", hotkey.Id);
            return false;
        }
    }

    /// <summary>
    /// Unregister all hotkeys
    /// Traceability: Section 4.6 Step 2 "Unhook Input Hooks"
    /// </summary>
    public void UnregisterAll()
    {
        if (_windowHandle == IntPtr.Zero)
        {
            return;
        }

        foreach (var id in _registeredHotkeys.Keys.ToList())
        {
            try
            {
                var result = NativeMethods.UnregisterHotKey(_windowHandle, id);
                if (result)
                {
                    _logger.LogInformation("Unregistered hotkey ID {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to unregister hotkey ID {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception unregistering hotkey ID {Id}", id);
            }
        }

        _registeredHotkeys.Clear();
    }

    /// <summary>
    /// Handle hotkey message (WM_HOTKEY)
    /// Called by message loop
    /// </summary>
    public void HandleHotkeyMessage(int hotkeyId)
    {
        if (_registeredHotkeys.TryGetValue(hotkeyId, out var hotkey))
        {
            _logger.LogInformation("Hotkey triggered: {Desc}", hotkey.Description);
            _actionDispatcher.Execute(hotkey.Action);
        }
        else
        {
            _logger.LogWarning("Unknown hotkey ID: {Id}", hotkeyId);
        }
    }

    /// <summary>
    /// Parse modifier strings to flags
    /// </summary>
    private uint ParseModifiers(List<string> modifiers)
    {
        uint result = 0;

        foreach (var mod in modifiers)
        {
            switch (mod.ToLowerInvariant())
            {
                case "alt":
                    result |= NativeMethods.MOD_ALT;
                    break;
                case "control":
                case "ctrl":
                    result |= NativeMethods.MOD_CONTROL;
                    break;
                case "shift":
                    result |= NativeMethods.MOD_SHIFT;
                    break;
                case "win":
                case "windows":
                    result |= NativeMethods.MOD_WIN;
                    break;
            }
        }

        return result;
    }

    public void Dispose()
    {
        UnregisterAll();
    }
}
