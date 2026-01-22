using MagicMouseClone.Core.Models;

namespace MagicMouseClone.Core.Interfaces;

/// <summary>
/// Interface cho device backend
/// Evidence: High - Từ section 7.2
/// </summary>
public interface IDeviceBackend
{
    event EventHandler<DeviceInfo>? DeviceConnected;
    event EventHandler? DeviceDisconnected;
    event EventHandler<TouchFrame>? TouchFrameReceived;

    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
    Task StartDiscoveryAsync(CancellationToken cancellationToken = default);
    Task StopDiscoveryAsync();
    Task<bool> ConnectAsync(string devicePath, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    DeviceInfo? GetCurrentDevice();
    bool IsConnected();
}
