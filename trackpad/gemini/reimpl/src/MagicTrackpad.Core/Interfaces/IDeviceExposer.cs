namespace MagicTrackpad.Core.Interfaces;

public interface IDeviceExposer
{
    IEnumerable<string> EnumerateDevices();
    event EventHandler<string> DeviceConnected;
    event EventHandler<string> DeviceDisconnected;
    void StartMonitoring();
    void StopMonitoring();
    
    // Mocking support
    void SimulateConnection(string deviceId);
}
