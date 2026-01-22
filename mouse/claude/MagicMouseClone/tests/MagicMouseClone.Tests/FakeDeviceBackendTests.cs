using Xunit;
using FluentAssertions;
using MagicMouseClone.Core.Services;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicMouseClone.Tests;

public class FakeDeviceBackendTests
{
    private readonly FakeDeviceBackend _backend;

    public FakeDeviceBackendTests()
    {
        _backend = new FakeDeviceBackend(NullLogger<FakeDeviceBackend>.Instance);
    }

    [Fact]
    public async Task InitializeAsync_ShouldSucceed()
    {
        // Act
        var result = await _backend.InitializeAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task StartDiscoveryAsync_ShouldDiscoverDevice()
    {
        // Act
        await _backend.StartDiscoveryAsync();
        var device = _backend.GetCurrentDevice();

        // Assert
        device.Should().NotBeNull();
        device!.DeviceName.Should().Be("Fake Magic Mouse");
        device.VendorId.Should().Be(0x05AC);
    }

    [Fact]
    public async Task ConnectAsync_ShouldTriggerDeviceConnectedEvent()
    {
        // Arrange
        DeviceInfo? connectedDevice = null;
        _backend.DeviceConnected += (s, e) => connectedDevice = e;

        await _backend.StartDiscoveryAsync();
        var device = _backend.GetCurrentDevice();

        // Act
        var result = await _backend.ConnectAsync(device!.DevicePath);

        // Assert
        result.Should().BeTrue();
        connectedDevice.Should().NotBeNull();
        _backend.IsConnected().Should().BeTrue();
    }

    [Fact]
    public async Task DisconnectAsync_ShouldTriggerDisconnectedEvent()
    {
        // Arrange
        var eventTriggered = false;
        _backend.DeviceDisconnected += (s, e) => eventTriggered = true;

        await _backend.StartDiscoveryAsync();
        var device = _backend.GetCurrentDevice();
        await _backend.ConnectAsync(device!.DevicePath);

        // Act
        await _backend.DisconnectAsync();

        // Assert
        eventTriggered.Should().BeTrue();
        _backend.IsConnected().Should().BeFalse();
    }

    [Fact]
    public async Task ConnectAsync_ShouldSimulateTouchFrames()
    {
        // Arrange
        var touchFramesReceived = 0;
        _backend.TouchFrameReceived += (s, e) => touchFramesReceived++;

        await _backend.StartDiscoveryAsync();
        var device = _backend.GetCurrentDevice();

        // Act
        await _backend.ConnectAsync(device!.DevicePath);
        await Task.Delay(6000); // Wait for at least one simulated touch
        await _backend.DisconnectAsync();

        // Assert
        touchFramesReceived.Should().BeGreaterThan(0);
    }
}
