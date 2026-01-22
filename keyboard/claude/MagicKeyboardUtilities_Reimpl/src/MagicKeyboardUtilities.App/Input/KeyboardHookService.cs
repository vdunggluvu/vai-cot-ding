using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Core;
using MagicKeyboardUtilities.App.Interop;

namespace MagicKeyboardUtilities.App.Input;

/// <summary>
/// Low-level keyboard hook service
/// Traceability: Section 4.2 INPUT HOOK FLOW, Section 8.1 Phase 4 "Input Hooking"
/// Evidence: "SetWindowsHookEx WH_KEYBOARD_LL" (inferred from flow report)
/// Status: INFERRED - original binary has protected hook logic
/// </summary>
public class KeyboardHookService : IDisposable
{
    private readonly ILogger<KeyboardHookService> _logger;
    private readonly RemappingEngine _remappingEngine;
    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookProc;
    private bool _isEnabled;
    private bool _sendInputEnabled;

    public KeyboardHookService(ILogger<KeyboardHookService> logger, RemappingEngine remappingEngine)
    {
        _logger = logger;
        _remappingEngine = remappingEngine;
        _isEnabled = false;
        _sendInputEnabled = false;
    }

    public bool IsInstalled => _hookId != IntPtr.Zero;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public bool SendInputEnabled
    {
        get => _sendInputEnabled;
        set => _sendInputEnabled = value;
    }

    /// <summary>
    /// Install the keyboard hook
    /// Traceability: Section 4.2 Step 1 "Hook Registration"
    /// </summary>
    public bool Install()
    {
        if (_hookId != IntPtr.Zero)
        {
            _logger.LogWarning("Hook already installed");
            return false;
        }

        try
        {
            // Keep reference to prevent GC
            _hookProc = HookCallback;
            
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            
            if (curModule?.ModuleName == null)
            {
                _logger.LogError("Cannot get current module name");
                return false;
            }

            _hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _hookProc,
                NativeMethods.GetModuleHandle(curModule.ModuleName),
                0);

            if (_hookId == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError("Failed to install keyboard hook, error code: {Error}", error);
                return false;
            }

            _logger.LogInformation("Keyboard hook installed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while installing keyboard hook");
            return false;
        }
    }

    /// <summary>
    /// Uninstall the keyboard hook
    /// Traceability: Section 4.6 Step 2 "Unhook Input Hooks"
    /// </summary>
    public bool Uninstall()
    {
        if (_hookId == IntPtr.Zero)
        {
            _logger.LogWarning("Hook not installed");
            return false;
        }

        try
        {
            var result = NativeMethods.UnhookWindowsHookEx(_hookId);
            if (result)
            {
                _hookId = IntPtr.Zero;
                _hookProc = null;
                _logger.LogInformation("Keyboard hook uninstalled successfully");
                return true;
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError("Failed to uninstall keyboard hook, error code: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while uninstalling keyboard hook");
            return false;
        }
    }

    /// <summary>
    /// Hook callback - processes keyboard events
    /// Traceability: Section 4.2 Steps 2-5, Section 8.3 "Keyboard Remapping Logic"
    /// </summary>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            // Must call next hook if nCode < 0
            if (nCode < 0 || !_isEnabled)
            {
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            // Parse keyboard event
            int wParamInt = wParam.ToInt32();
            if (wParamInt == NativeMethods.WM_KEYDOWN || wParamInt == NativeMethods.WM_SYSKEYDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                var vkCode = (int)hookStruct.vkCode;

                _logger.LogDebug("Key event: VK {VK}", vkCode);

                // Try to remap
                if (_remappingEngine.TryRemap(vkCode, out var remappedVk))
                {
                    _logger.LogInformation("Remapping VK {From} -> VK {To}", vkCode, remappedVk);

                    // Send new key if enabled
                    if (_sendInputEnabled)
                    {
                        SendKeyPress(remappedVk);
                    }
                    else
                    {
                        _logger.LogDebug("SendInput disabled, not sending remapped key");
                    }

                    // Block original key
                    return (IntPtr)1;
                }
            }

            // Pass through if no remapping
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in hook callback");
            // Always pass through on exception
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }

    /// <summary>
    /// Send a key press using SendInput
    /// Traceability: Section 4.2 Step 4 "Action Execution - Inject key"
    /// </summary>
    private void SendKeyPress(int vkCode)
    {
        try
        {
            var inputs = new NativeMethods.INPUT[2];
            
            // Key down
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].union.ki.wVk = (ushort)vkCode;
            inputs[0].union.ki.wScan = 0;
            inputs[0].union.ki.dwFlags = 0;
            inputs[0].union.ki.time = 0;
            inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

            // Key up
            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].union.ki.wVk = (ushort)vkCode;
            inputs[1].union.ki.wScan = 0;
            inputs[1].union.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;
            inputs[1].union.ki.time = 0;
            inputs[1].union.ki.dwExtraInfo = IntPtr.Zero;

            var sent = NativeMethods.SendInput(2, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            if (sent != 2)
            {
                _logger.LogWarning("SendInput failed to send all events, sent: {Count}", sent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending key press");
        }
    }

    public void Dispose()
    {
        Uninstall();
    }
}
