using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Core;
using MagicKeyboardUtilities.App.Device;
using MagicKeyboardUtilities.App.Input;
using MagicKeyboardUtilities.App.Messaging;
using MagicKeyboardUtilities.App.Tray;

namespace MagicKeyboardUtilities.App;

/// <summary>
/// Application host - orchestrates lifecycle
/// Traceability: Section 3 "BẢNĐỒ LUỒNG CHẠY TỔNG THỂ", Section 4.1 STARTUP FLOW
/// </summary>
public class AppHost : IDisposable
{
    private readonly ILogger<AppHost> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConfigStore _configStore;
    private readonly RemappingEngine _remappingEngine;
    private readonly ActionDispatcher _actionDispatcher;
    private readonly KeyboardHookService _keyboardHookService;
    private readonly HotkeyService _hotkeyService;
    private readonly DeviceMonitor _deviceMonitor;
    private readonly TrayIconController _trayIconController;
    private HiddenMessageWindow? _messageWindow;
    private bool _isRunning;

    public AppHost(
        ILogger<AppHost> logger,
        ILoggerFactory loggerFactory,
        ConfigStore configStore,
        RemappingEngine remappingEngine,
        ActionDispatcher actionDispatcher,
        KeyboardHookService keyboardHookService,
        HotkeyService hotkeyService,
        DeviceMonitor deviceMonitor,
        TrayIconController trayIconController)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _configStore = configStore;
        _remappingEngine = remappingEngine;
        _actionDispatcher = actionDispatcher;
        _keyboardHookService = keyboardHookService;
        _hotkeyService = hotkeyService;
        _deviceMonitor = deviceMonitor;
        _trayIconController = trayIconController;
        _isRunning = false;
    }

    /// <summary>
    /// Start the application
    /// Traceability: Section 4.1 STARTUP FLOW, All steps
    /// </summary>
    public bool Start()
    {
        try
        {
            _logger.LogInformation("=== Starting MagicKeyboardUtilities Reimplementation ===");

            // Step 1-5: Configuration loading (already done in Program.cs)
            var config = _configStore.Current;

            // Step 6: Register actions
            RegisterActions();

            // Step 7: Load remapping table
            _logger.LogInformation("Loading key mappings...");
            _remappingEngine.LoadMappings(config.Remapping);

            // Step 8: Create hidden message window
            _logger.LogInformation("Creating message window...");
            _messageWindow = new HiddenMessageWindow(
                _loggerFactory.CreateLogger<HiddenMessageWindow>(),
                _deviceMonitor,
                _hotkeyService);
            _messageWindow.CreateWindow();

            // Step 9: Create tray icon
            if (config.Features.TrayIcon)
            {
                _logger.LogInformation("Creating tray icon...");
                _trayIconController.Create();
            }

            // Step 10: Enable features based on config
            ApplyConfiguration(config);

            _isRunning = true;
            _logger.LogInformation("=== Application started successfully ===");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start application");
            return false;
        }
    }

    /// <summary>
    /// Register action handlers
    /// Traceability: Section 4.2 Step 4 "Action Execution"
    /// </summary>
    private void RegisterActions()
    {
        _actionDispatcher.RegisterAction("Enable", OnEnable);
        _actionDispatcher.RegisterAction("Disable", OnDisable);
        _actionDispatcher.RegisterAction("ToggleEnabled", OnToggle);
        _actionDispatcher.RegisterAction("Exit", OnExit);

        _logger.LogInformation("Registered {Count} actions", 4);
    }

    /// <summary>
    /// Apply configuration to services
    /// </summary>
    private void ApplyConfiguration(AppConfig config)
    {
        // Device monitor
        _deviceMonitor.IsEnabled = config.Features.EnableDeviceMonitor;
        _deviceMonitor.AppleVendorId = config.Device.AppleVendorId;
        if (_deviceMonitor.IsEnabled)
        {
            _logger.LogInformation("Device monitor enabled");
            _deviceMonitor.ScanExistingDevices();
        }

        // Enable hooks/hotkeys if configured
        if (config.Features.EnableHooks)
        {
            EnableFeatures();
        }
        else
        {
            _logger.LogInformation("Hooks disabled by default (safety)");
        }
    }

    /// <summary>
    /// Enable keyboard hooks and hotkeys
    /// </summary>
    private void EnableFeatures()
    {
        var config = _configStore.Current;

        if (config.Features.EnableHooks)
        {
            _logger.LogInformation("Installing keyboard hook...");
            if (_keyboardHookService.Install())
            {
                _keyboardHookService.IsEnabled = true;
                _keyboardHookService.SendInputEnabled = config.Features.EnableSendInput;
                _logger.LogInformation("Keyboard hook enabled (SendInput: {Send})", 
                    _keyboardHookService.SendInputEnabled);
            }
        }

        if (config.Features.EnableHotkeys)
        {
            _logger.LogInformation("Registering hotkeys...");
            _hotkeyService.RegisterHotkeys(config.Hotkeys);
        }

        _trayIconController.IsEnabled = true;
    }

    /// <summary>
    /// Disable keyboard hooks and hotkeys
    /// </summary>
    private void DisableFeatures()
    {
        _logger.LogInformation("Disabling features...");

        if (_keyboardHookService.IsInstalled)
        {
            _keyboardHookService.IsEnabled = false;
            _keyboardHookService.Uninstall();
            _logger.LogInformation("Keyboard hook disabled");
        }

        _hotkeyService.UnregisterAll();
        _logger.LogInformation("Hotkeys unregistered");

        _trayIconController.IsEnabled = false;
    }

    /// <summary>
    /// Action: Enable features
    /// </summary>
    private void OnEnable()
    {
        _logger.LogInformation("Enabling features...");
        EnableFeatures();
        _trayIconController.ShowNotification("Magic Keyboard", "Features enabled", ToolTipIcon.Info);
    }

    /// <summary>
    /// Action: Disable features
    /// </summary>
    private void OnDisable()
    {
        _logger.LogInformation("Disabling features...");
        DisableFeatures();
        _trayIconController.ShowNotification("Magic Keyboard", "Features disabled", ToolTipIcon.Info);
    }

    /// <summary>
    /// Action: Toggle features
    /// </summary>
    private void OnToggle()
    {
        if (_trayIconController.IsEnabled)
        {
            OnDisable();
        }
        else
        {
            OnEnable();
        }
    }

    /// <summary>
    /// Action: Exit application
    /// </summary>
    private void OnExit()
    {
        _logger.LogInformation("Exit requested");
        Stop();
        Application.Exit();
    }

    /// <summary>
    /// Stop the application
    /// Traceability: Section 4.6 SHUTDOWN FLOW
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _logger.LogInformation("=== Stopping application ===");

        try
        {
            // Step 2: Unhook
            DisableFeatures();

            // Step 3: Save config if needed
            if (_configStore.Current.App.AutoSave)
            {
                _logger.LogInformation("Auto-saving configuration...");
                _configStore.Save();
            }

            // Step 4: Remove tray icon
            _trayIconController.Dispose();

            // Step 5: Cleanup COM (not needed in .NET)

            // Step 6: Destroy window
            _messageWindow?.Dispose();

            _isRunning = false;
            _logger.LogInformation("=== Application stopped ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shutdown");
        }
    }

    public void Dispose()
    {
        Stop();
        _keyboardHookService.Dispose();
        _hotkeyService.Dispose();
    }
}
