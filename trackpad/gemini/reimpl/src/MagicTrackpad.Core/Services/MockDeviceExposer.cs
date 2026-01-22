using MagicTrackpad.Core.Interfaces;

namespace MagicTrackpad.Core.Services;

public class MockDeviceExposer : IDeviceExposer
{
    public event EventHandler<string> DeviceConnected;
    public event EventHandler<string> DeviceDisconnected;

    private bool _monitoring = false;

    public IEnumerable<string> EnumerateDevices()
    {
        // Return empty unless simulated
        return Enumerable.Empty<string>();
    }

    public void StartMonitoring()
    {
        _monitoring = true;
    }

    public void StopMonitoring()
    {
        _monitoring = false;
    }

    public void SimulateConnection(string deviceId)
    {
        DeviceConnected?.Invoke(this, deviceId);
    }
}
