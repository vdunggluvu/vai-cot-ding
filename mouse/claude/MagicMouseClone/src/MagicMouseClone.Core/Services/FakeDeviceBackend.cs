using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;

namespace MagicMouseClone.Core.Services;

/// <summary>
/// Fake device backend for testing and demo
/// Evidence: N/A - Implementation for testing
/// </summary>
public class FakeDeviceBackend : IDeviceBackend
{
    private readonly ILogger<FakeDeviceBackend> _logger;
    private DeviceInfo? _currentDevice;
    private CancellationTokenSource? _simulationCts;
    private bool _isConnected;

    public event EventHandler<DeviceInfo>? DeviceConnected;
    public event EventHandler? DeviceDisconnected;
    public event EventHandler<TouchFrame>? TouchFrameReceived;

    public FakeDeviceBackend(ILogger<FakeDeviceBackend> logger)
    {
        _logger = logger;
    }

    public Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FakeDeviceBackend initialized");
        return Task.FromResult(true);
    }

    public async Task StartDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting fake device discovery...");
        await Task.Delay(500, cancellationToken);

        _currentDevice = new DeviceInfo
        {
            DeviceName = "Fake Magic Mouse",
            DevicePath = "FAKE\\DEVICE\\PATH",
            VendorId = 0x05AC,
            ProductId = 0x030D,
            BatteryLevel = 85,
            State = DeviceState.Disconnected
        };

        _logger.LogInformation("Fake device discovered: {DeviceName}", _currentDevice.DeviceName);
    }

    public async Task<bool> ConnectAsync(string devicePath, CancellationToken cancellationToken = default)
    {
        if (_currentDevice == null)
        {
            return false;
        }

        _logger.LogInformation("Connecting to fake device...");
        await Task.Delay(300, cancellationToken);

        _currentDevice.State = DeviceState.Connected;
        _isConnected = true;

        DeviceConnected?.Invoke(this, _currentDevice);
        _logger.LogInformation("Connected to fake device");

        // Start simulation
        StartTouchSimulation();

        return true;
    }

    public Task DisconnectAsync()
    {
        if (_currentDevice != null)
        {
            _currentDevice.State = DeviceState.Disconnected;
            _isConnected = false;
            _simulationCts?.Cancel();
            DeviceDisconnected?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Disconnected from fake device");
        }

        return Task.CompletedTask;
    }

    public Task StopDiscoveryAsync()
    {
        _logger.LogInformation("Stopped fake device discovery");
        return Task.CompletedTask;
    }

    public DeviceInfo? GetCurrentDevice() => _currentDevice;

    public bool IsConnected() => _isConnected;

    private void StartTouchSimulation()
    {
        _simulationCts?.Cancel();
        _simulationCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            var random = new Random();
            var token = _simulationCts.Token;

            while (!token.IsCancellationRequested && _isConnected)
            {
                // Simulate random gestures every 3-5 seconds
                await Task.Delay(random.Next(3000, 5000), token);

                if (token.IsCancellationRequested || !_isConnected)
                    break;

                // Generate a simple touch frame (1 touch point)
                var touchFrame = new TouchFrame(
                    new[]
                    {
                        new TouchPoint(
                            Id: 1,
                            X: (float)random.NextDouble(),
                            Y: (float)random.NextDouble(),
                            Timestamp: DateTime.UtcNow
                        )
                    },
                    DateTime.UtcNow
                );

                TouchFrameReceived?.Invoke(this, touchFrame);
            }
        }, _simulationCts.Token);
    }
}
