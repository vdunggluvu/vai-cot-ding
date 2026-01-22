namespace MagicTrackpadClone.Core;

public class InputSource : IInputSource, IDisposable
{
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private string? _currentDeviceId;

    public event EventHandler<InputEventArgs>? InputReceived;
    public bool IsMonitoring { get; private set; }

    public InputSource(ILogger logger)
    {
        _logger = logger;
    }

    public Task<bool> StartMonitoringAsync(string deviceId)
    {
        if (IsMonitoring)
        {
            _logger.LogWarning("Already monitoring a device");
            return Task.FromResult(false);
        }

        _currentDeviceId = deviceId;
        _cts = new CancellationTokenSource();
        IsMonitoring = true;

        _monitorTask = Task.Run(() => MonitorDeviceAsync(_cts.Token));
        
        _logger.LogInfo($"Started monitoring device: {deviceId}");
        return Task.FromResult(true);
    }

    public Task StopMonitoringAsync()
    {
        if (!IsMonitoring)
        {
            return Task.CompletedTask;
        }

        _cts?.Cancel();
        IsMonitoring = false;
        
        _logger.LogInfo("Stopped device monitoring");
        return Task.CompletedTask;
    }

    private async Task MonitorDeviceAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Entering monitor loop");
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Simulate reading HID reports
                // Real implementation would use CreateFile + ReadFile on device path
                await Task.Delay(16, cancellationToken); // ~60Hz polling
                
                // Generate mock input events for testing
                if (Environment.GetEnvironmentVariable("GENERATE_MOCK_INPUT") == "1")
                {
                    GenerateMockInputEvent();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Monitor loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError("Monitor loop error", ex);
        }
    }

    private void GenerateMockInputEvent()
    {
        var inputEvent = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint
                {
                    ContactId = 1,
                    X = 0.5,
                    Y = 0.5,
                    Pressure = 0.7,
                    State = TouchState.Move
                }
            },
            ButtonState = new ButtonState()
        };
        
        InputReceived?.Invoke(this, inputEvent);
    }

    public void Dispose()
    {
        StopMonitoringAsync().Wait();
        _cts?.Dispose();
    }
}
