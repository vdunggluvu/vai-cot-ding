using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class InputSourceTests
{
    private class TestLogger : ILogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) { }
    }

    [Fact]
    public async Task StartMonitoringAsync_UpdatesIsMonitoringProperty()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);

        // Act
        var started = await inputSource.StartMonitoringAsync("TEST_DEVICE");

        // Assert
        Assert.True(started);
        Assert.True(inputSource.IsMonitoring);

        // Cleanup
        await inputSource.StopMonitoringAsync();
    }

    [Fact]
    public async Task StartMonitoringAsync_WhileMonitoring_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);
        await inputSource.StartMonitoringAsync("TEST_DEVICE");

        // Act
        var startedAgain = await inputSource.StartMonitoringAsync("TEST_DEVICE_2");

        // Assert
        Assert.False(startedAgain);

        // Cleanup
        await inputSource.StopMonitoringAsync();
    }

    [Fact]
    public async Task StopMonitoringAsync_UpdatesIsMonitoringProperty()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);
        await inputSource.StartMonitoringAsync("TEST_DEVICE");

        // Act
        await inputSource.StopMonitoringAsync();

        // Assert
        Assert.False(inputSource.IsMonitoring);
    }

    [Fact]
    public async Task InputReceived_WithMockInput_FiresEvent()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);
        var eventFired = false;
        InputEventArgs? receivedEvent = null;

        inputSource.InputReceived += (s, e) =>
        {
            eventFired = true;
            receivedEvent = e;
        };

        Environment.SetEnvironmentVariable("GENERATE_MOCK_INPUT", "1");

        // Act
        await inputSource.StartMonitoringAsync("TEST_DEVICE");
        await Task.Delay(100); // Wait for events
        await inputSource.StopMonitoringAsync();

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(receivedEvent);
        Assert.NotEmpty(receivedEvent!.TouchPoints);

        // Cleanup
        Environment.SetEnvironmentVariable("GENERATE_MOCK_INPUT", null);
    }

    [Fact]
    public async Task Dispose_StopsMonitoring()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);
        await inputSource.StartMonitoringAsync("TEST_DEVICE");

        // Act
        inputSource.Dispose();

        // Assert
        Assert.False(inputSource.IsMonitoring);
    }

    [Fact]
    public async Task StopMonitoringAsync_WhenNotMonitoring_DoesNotThrow()
    {
        // Arrange
        var logger = new TestLogger();
        var inputSource = new InputSource(logger);

        // Act & Assert
        await inputSource.StopMonitoringAsync();
        Assert.True(true); // No exception means success
    }
}
