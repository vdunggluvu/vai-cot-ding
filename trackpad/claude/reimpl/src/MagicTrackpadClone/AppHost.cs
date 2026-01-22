using System.Threading;
using System.Windows;
using System.IO;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone;

public class AppHost : IDisposable
{
    private readonly ILogger _logger;
    private readonly IDeviceEnumerator _deviceEnumerator;
    private readonly IInputSource _inputSource;
    private readonly IGestureEngine _gestureEngine;
    private readonly IActionExecutor _actionExecutor;
    private readonly IConfigStore _configStore;
    private TrayIconManager? _trayIcon;
    private AppConfiguration? _config;
    private Mutex? _singletonMutex;

    public AppHost()
    {
        // Initialize logging
        var logPath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".",
            "..", "..", "logs", $"app_{DateTime.Now:yyyyMMdd}.log"
        );
        
        _logger = new FileLogger(logPath, "Debug");
        
        // Initialize core components
        _deviceEnumerator = new DeviceEnumerator(_logger);
        _inputSource = new InputSource(_logger);
        _gestureEngine = new GestureEngine(_logger);
        _actionExecutor = new ActionExecutor(_logger);
        _configStore = new ConfigStore(_logger);
    }

    public bool Initialize()
    {
        try
        {
            _logger.LogInfo("=== Application Starting ===");
            _logger.LogInfo($"Version: 3.1.5.6");
            _logger.LogInfo($"OS: {Environment.OSVersion}");
            _logger.LogInfo($"CLR: {Environment.Version}");
            
            // Check singleton
            if (!CheckSingleInstance())
            {
                _logger.LogWarning("Another instance is already running");
                MessageBox.Show("Magic Trackpad Clone is already running.", 
                    "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            
            // Load configuration
            _config = _configStore.LoadConfigurationAsync().Result;
            if (_config == null)
            {
                _logger.LogError("Failed to load configuration");
                return false;
            }
            
            // Initialize gesture engine with config
            _gestureEngine.LoadConfiguration(_config.Gestures);
            
            // Wire up events
            _inputSource.InputReceived += OnInputReceived;
            _gestureEngine.GestureDetected += OnGestureDetected;
            
            // Initialize tray icon
            _trayIcon = new TrayIconManager(_logger, this);
            _trayIcon.Initialize();
            
            // Scan for devices
            var devices = _deviceEnumerator.EnumerateDevicesAsync().Result;
            _logger.LogInfo($"Found {devices.Count} devices");
            
            if (devices.Count > 0)
            {
                // Start monitoring first available device
                var device = devices[0];
                _logger.LogInfo($"Starting monitoring: {device.DeviceName}");
                _inputSource.StartMonitoringAsync(device.DeviceId).Wait();
            }
            else
            {
                _logger.LogWarning("No Magic Trackpad devices found");
                MessageBox.Show(
                    "No Magic Trackpad devices found.\n\nThe application will continue running and monitor for device connections.",
                    "Device Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            
            _logger.LogInfo("Application initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Initialization failed", ex);
            MessageBox.Show($"Failed to initialize application:\n{ex.Message}", 
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public void Shutdown()
    {
        _logger.LogInfo("=== Application Shutting Down ===");
        
        try
        {
            _inputSource?.StopMonitoringAsync().Wait();
            _trayIcon?.Dispose();
            _singletonMutex?.ReleaseMutex();
            _singletonMutex?.Dispose();
            
            _logger.LogInfo("Shutdown complete");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during shutdown", ex);
        }
    }

    public void ShowSettingsWindow()
    {
        _logger.LogInfo("Opening settings window");
        
        // Create and show settings window
        var settingsWindow = new SettingsWindow(_config, _configStore, _logger);
        settingsWindow.ShowDialog();
        
        // Reload config if changed
        _config = _configStore.LoadConfigurationAsync().Result;
        _gestureEngine.LoadConfiguration(_config!.Gestures);
    }

    public void ToggleGestures()
    {
        if (_config == null) return;
        
        _config.General.EnableGestures = !_config.General.EnableGestures;
        _configStore.SaveConfigurationAsync(_config).Wait();
        
        var status = _config.General.EnableGestures ? "enabled" : "disabled";
        _logger.LogInfo($"Gestures {status}");
        
        _trayIcon?.UpdateGestureStatus(_config.General.EnableGestures);
    }

    private bool CheckSingleInstance()
    {
        const string mutexName = "Global\\MagicTrackpadClone_SingleInstance_Mutex";
        
        try
        {
            _singletonMutex = new Mutex(true, mutexName, out bool createdNew);
            return createdNew;
        }
        catch
        {
            return false;
        }
    }

    private void OnInputReceived(object? sender, InputEventArgs e)
    {
        if (_config?.General.EnableGestures == true)
        {
            _gestureEngine.ProcessInput(e);
        }
    }

    private void OnGestureDetected(object? sender, GestureEventArgs e)
    {
        _logger.LogDebug($"Gesture: {e.Type} {e.FingerCount}F {e.Direction}");
        
        // Find matching gesture mapping
        var key = $"{e.Type.ToString().ToLower()}_{e.FingerCount}";
        if (e.Direction != GestureDirection.None)
        {
            key += $"_{e.Direction.ToString().ToLower()}";
        }
        
        if (_config?.Gestures.Mappings.TryGetValue(key, out var mapping) == true)
        {
            if (mapping.Enabled)
            {
                _actionExecutor.ExecuteActionAsync(mapping.Action);
            }
        }
    }

    public void Dispose()
    {
        Shutdown();
    }
}
