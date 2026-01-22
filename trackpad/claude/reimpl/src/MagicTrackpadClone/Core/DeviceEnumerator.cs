using System.Runtime.InteropServices;

namespace MagicTrackpadClone.Core;

public class DeviceEnumerator : IDeviceEnumerator
{
    private readonly ILogger _logger;

    public DeviceEnumerator(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<DeviceInfo>> EnumerateDevicesAsync()
    {
        return await Task.Run(() =>
        {
            var devices = new List<DeviceInfo>();
            
            try
            {
                _logger.LogInfo("Starting device enumeration");
                
                // Enumerate HID devices
                var hidDevices = EnumerateHidDevices();
                devices.AddRange(hidDevices);
                
                // Enumerate Bluetooth devices
                var btDevices = EnumerateBluetoothDevices();
                devices.AddRange(btDevices);
                
                _logger.LogInfo($"Found {devices.Count} total devices");
            }
            catch (Exception ex)
            {
                _logger.LogError("Device enumeration failed", ex);
            }
            
            return devices;
        });
    }

    public async Task<bool> IsDeviceAvailableAsync(string deviceId)
    {
        var devices = await EnumerateDevicesAsync();
        return devices.Any(d => d.DeviceId == deviceId && d.IsConnected);
    }

    private List<DeviceInfo> EnumerateHidDevices()
    {
        var devices = new List<DeviceInfo>();
        
        try
        {
            // Apple Magic Trackpad VID/PID
            const ushort AppleVendorId = 0x05AC;
            
            // Mock enumeration for testing
            // Real implementation would use SetupDi* APIs
            _logger.LogDebug("Enumerating HID devices");
            
            // Add mock device for testing
            if (Environment.GetEnvironmentVariable("MOCK_DEVICE_AVAILABLE") == "1")
            {
                devices.Add(new DeviceInfo
                {
                    DeviceId = "HID\\VEN_05AC&DEV_030E",
                    DeviceName = "Apple Magic Trackpad 2",
                    DevicePath = @"\\?\HID#VEN_05AC&DEV_030E#Mock",
                    VendorId = AppleVendorId,
                    ProductId = 0x030E,
                    ConnectionType = DeviceConnectionType.Usb,
                    IsConnected = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"HID enumeration error: {ex.Message}");
        }
        
        return devices;
    }

    private List<DeviceInfo> EnumerateBluetoothDevices()
    {
        var devices = new List<DeviceInfo>();
        
        try
        {
            _logger.LogDebug("Enumerating Bluetooth devices");
            
            // Mock enumeration for testing
            // Real implementation would use Bluetooth APIs
            if (Environment.GetEnvironmentVariable("MOCK_DEVICE_AVAILABLE") == "1")
            {
                devices.Add(new DeviceInfo
                {
                    DeviceId = "BTHENUM\\{00001124-0000-1000-8000-00805f9b34fb}",
                    DeviceName = "Magic Trackpad (Bluetooth)",
                    DevicePath = @"\\?\BTH#Mock",
                    VendorId = 0x05AC,
                    ProductId = 0x030E,
                    ConnectionType = DeviceConnectionType.Bluetooth,
                    IsConnected = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Bluetooth enumeration error: {ex.Message}");
        }
        
        return devices;
    }
}
