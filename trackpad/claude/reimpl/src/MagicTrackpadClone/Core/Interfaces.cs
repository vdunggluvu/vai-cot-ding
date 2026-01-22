namespace MagicTrackpadClone.Core;

public interface IDeviceEnumerator
{
    Task<List<DeviceInfo>> EnumerateDevicesAsync();
    Task<bool> IsDeviceAvailableAsync(string deviceId);
}

public interface IInputSource
{
    event EventHandler<InputEventArgs>? InputReceived;
    Task<bool> StartMonitoringAsync(string deviceId);
    Task StopMonitoringAsync();
    bool IsMonitoring { get; }
}

public interface IGestureEngine
{
    event EventHandler<GestureEventArgs>? GestureDetected;
    void ProcessInput(InputEventArgs inputEvent);
    void LoadConfiguration(GestureConfiguration config);
}

public interface IActionExecutor
{
    Task ExecuteActionAsync(GestureAction action);
    bool CanExecuteAction(GestureAction action);
}

public interface IConfigStore
{
    Task<AppConfiguration?> LoadConfigurationAsync();
    Task SaveConfigurationAsync(AppConfiguration config);
    Task<bool> ValidateConfigurationAsync(AppConfiguration config);
}

public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
    void LogDebug(string message);
}
