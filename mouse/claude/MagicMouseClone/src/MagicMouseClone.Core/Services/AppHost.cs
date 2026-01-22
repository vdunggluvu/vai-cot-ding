using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;

namespace MagicMouseClone.Core.Services;

/// <summary>
/// Main application host coordinating all components
/// Evidence: High - Based on section 6.1 application lifecycle
/// </summary>
public class AppHost : IDisposable
{
    private readonly ILogger<AppHost> _logger;
    private readonly IDeviceBackend _deviceBackend;
    private readonly IGestureEngine _gestureEngine;
    private readonly IActionMapper _actionMapper;
    private readonly IConfigurationManager _configManager;

    private AppConfig? _config;
    private bool _isRunning;
    private readonly CancellationTokenSource _cts = new();

    public event EventHandler<DeviceInfo>? DeviceConnected;
    public event EventHandler? DeviceDisconnected;
    public event EventHandler<GestureEvent>? GestureDetected;

    public AppHost(
        ILogger<AppHost> logger,
        IDeviceBackend deviceBackend,
        IGestureEngine gestureEngine,
        IActionMapper actionMapper,
        IConfigurationManager configManager)
    {
        _logger = logger;
        _deviceBackend = deviceBackend;
        _gestureEngine = gestureEngine;
        _actionMapper = actionMapper;
        _configManager = configManager;

        // Wire up events
        _deviceBackend.DeviceConnected += OnDeviceConnected;
        _deviceBackend.DeviceDisconnected += OnDeviceDisconnected;
        _deviceBackend.TouchFrameReceived += OnTouchFrameReceived;
        _gestureEngine.GestureDetected += OnGestureDetected;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing AppHost...");

            // Load configuration
            _config = await _configManager.LoadConfigurationAsync();
            _logger.LogInformation("Configuration loaded");

            // Load active profile
            var profile = await _configManager.LoadProfileAsync(_config.ActiveProfile);
            _actionMapper.LoadProfile(profile);
            _logger.LogInformation("Profile loaded: {ProfileName}", _config.ActiveProfile);

            // Set gesture sensitivity
            _gestureEngine.SetSensitivity(_config.GestureSensitivity);

            // Initialize device backend
            var deviceInitialized = await _deviceBackend.InitializeAsync(_cts.Token);
            if (!deviceInitialized)
            {
                _logger.LogError("Failed to initialize device backend");
                return false;
            }

            _logger.LogInformation("AppHost initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initialization");
            return false;
        }
    }

    public async Task StartAsync()
    {
        if (_isRunning)
        {
            _logger.LogWarning("AppHost is already running");
            return;
        }

        _logger.LogInformation("Starting AppHost...");
        _isRunning = true;

        // Start device discovery
        await _deviceBackend.StartDiscoveryAsync(_cts.Token);

        // Auto-connect to first discovered device
        var device = _deviceBackend.GetCurrentDevice();
        if (device != null)
        {
            await _deviceBackend.ConnectAsync(device.DevicePath, _cts.Token);
        }

        _logger.LogInformation("AppHost started");
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            return;
        }

        _logger.LogInformation("Stopping AppHost...");
        _isRunning = false;

        await _deviceBackend.DisconnectAsync();
        await _deviceBackend.StopDiscoveryAsync();

        // Save configuration
        if (_config != null)
        {
            await _configManager.SaveConfigurationAsync(_config);
        }

        _logger.LogInformation("AppHost stopped");
    }

    public AppConfig? GetConfiguration() => _config;

    public DeviceInfo? GetCurrentDevice() => _deviceBackend.GetCurrentDevice();

    public bool IsDeviceConnected() => _deviceBackend.IsConnected();

    private void OnDeviceConnected(object? sender, DeviceInfo device)
    {
        _logger.LogInformation("Device connected: {DeviceName}", device.DeviceName);
        DeviceConnected?.Invoke(this, device);
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Device disconnected");
        DeviceDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnTouchFrameReceived(object? sender, TouchFrame frame)
    {
        if (_config?.EnableGestures == true)
        {
            _gestureEngine.ProcessTouchFrame(frame);
        }
    }

    private async void OnGestureDetected(object? sender, GestureEvent gestureEvent)
    {
        _logger.LogDebug("Gesture detected: {Gesture}", gestureEvent.Type);
        GestureDetected?.Invoke(this, gestureEvent);

        // Execute action
        await _actionMapper.ExecuteActionAsync(gestureEvent, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _deviceBackend.DeviceConnected -= OnDeviceConnected;
        _deviceBackend.DeviceDisconnected -= OnDeviceDisconnected;
        _deviceBackend.TouchFrameReceived -= OnTouchFrameReceived;
        _gestureEngine.GestureDetected -= OnGestureDetected;
    }
}
