using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Device;
using MagicKeyboardUtilities.App.Input;
using MagicKeyboardUtilities.App.Interop;

namespace MagicKeyboardUtilities.App.Messaging;

/// <summary>
/// Hidden message window for receiving WM_DEVICECHANGE and WM_HOTKEY
/// Traceability: Section 4.1 Step 7, Section 3.1 "Create message-only window"
/// Evidence: "MainWindowHandle = 0" but needs message loop for notifications
/// </summary>
public class HiddenMessageWindow : NativeWindow, IDisposable
{
    private readonly ILogger<HiddenMessageWindow> _logger;
    private readonly DeviceMonitor? _deviceMonitor;
    private readonly HotkeyService? _hotkeyService;
    private const int WM_HOTKEY = 0x0312;

    public HiddenMessageWindow(
        ILogger<HiddenMessageWindow> logger,
        DeviceMonitor? deviceMonitor = null,
        HotkeyService? hotkeyService = null)
    {
        _logger = logger;
        _deviceMonitor = deviceMonitor;
        _hotkeyService = hotkeyService;
    }

    /// <summary>
    /// Create the hidden window
    /// </summary>
    public void CreateWindow()
    {
        try
        {
            // Create message-only window
            CreateHandle(new CreateParams
            {
                Caption = "MagicKeyboardUtilities_MessageWindow",
                Parent = new IntPtr(-3), // HWND_MESSAGE
                Style = 0,
                ExStyle = 0
            });

            _logger.LogInformation("Hidden message window created, handle: {Handle}", Handle);

            // Initialize device monitor with window handle
            if (_deviceMonitor != null && _deviceMonitor.IsEnabled)
            {
                _deviceMonitor.Initialize(Handle);
            }

            // Set hotkey window handle
            if (_hotkeyService != null)
            {
                _hotkeyService.SetWindowHandle(Handle);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create hidden message window");
        }
    }

    /// <summary>
    /// Window procedure - handles messages
    /// Traceability: Section 7.2 "Window Procedure (WndProc)"
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        try
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_DEVICECHANGE:
                    _logger.LogDebug("WM_DEVICECHANGE received");
                    _deviceMonitor?.HandleDeviceChange(m.WParam, m.LParam);
                    break;

                case WM_HOTKEY:
                    int hotkeyId = m.WParam.ToInt32();
                    _logger.LogDebug("WM_HOTKEY received, ID: {Id}", hotkeyId);
                    _hotkeyService?.HandleHotkeyMessage(hotkeyId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in WndProc for message {Msg}", m.Msg);
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            DestroyHandle();
            _logger.LogInformation("Hidden message window destroyed");
        }
    }
}
