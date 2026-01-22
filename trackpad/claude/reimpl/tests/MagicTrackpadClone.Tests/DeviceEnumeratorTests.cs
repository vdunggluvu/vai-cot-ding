using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class DeviceEnumeratorTests
{
    private class TestLogger : ILogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) { }
    }

    [Fact]
    public async Task EnumerateDevicesAsync_ReturnsEmptyListByDefault()
    {
        // Arrange
        var logger = new TestLogger();
        var enumerator = new DeviceEnumerator(logger);

        // Act
        var devices = await enumerator.EnumerateDevicesAsync();

        // Assert
        Assert.NotNull(devices);
        Assert.IsType<List<DeviceInfo>>(devices);
    }

    [Fact]
    public async Task EnumerateDevicesAsync_WithMockDevice_ReturnsDevices()
    {
        // Arrange
        var logger = new TestLogger();
        var enumerator = new DeviceEnumerator(logger);
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", "1");

        // Act
        var devices = await enumerator.EnumerateDevicesAsync();

        // Assert
        Assert.NotEmpty(devices);
        Assert.Contains(devices, d => d.DeviceName.Contains("Magic Trackpad"));

        // Cleanup
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", null);
    }

    [Fact]
    public async Task IsDeviceAvailableAsync_ExistingDevice_ReturnsTrue()
    {
        // Arrange
        var logger = new TestLogger();
        var enumerator = new DeviceEnumerator(logger);
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", "1");

        var devices = await enumerator.EnumerateDevicesAsync();
        var deviceId = devices.First().DeviceId;

        // Act
        var isAvailable = await enumerator.IsDeviceAvailableAsync(deviceId);

        // Assert
        Assert.True(isAvailable);

        // Cleanup
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", null);
    }

    [Fact]
    public async Task IsDeviceAvailableAsync_NonExistingDevice_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var enumerator = new DeviceEnumerator(logger);

        // Act
        var isAvailable = await enumerator.IsDeviceAvailableAsync("NONEXISTENT_DEVICE");

        // Assert
        Assert.False(isAvailable);
    }

    [Fact]
    public async Task EnumerateDevicesAsync_DeviceHasCorrectProperties()
    {
        // Arrange
        var logger = new TestLogger();
        var enumerator = new DeviceEnumerator(logger);
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", "1");

        // Act
        var devices = await enumerator.EnumerateDevicesAsync();
        var device = devices.FirstOrDefault();

        // Assert
        Assert.NotNull(device);
        Assert.False(string.IsNullOrEmpty(device!.DeviceId));
        Assert.False(string.IsNullOrEmpty(device.DeviceName));
        Assert.True(device.IsConnected);
        Assert.Equal((ushort)0x05AC, device.VendorId); // Apple vendor ID

        // Cleanup
        Environment.SetEnvironmentVariable("MOCK_DEVICE_AVAILABLE", null);
    }
}
