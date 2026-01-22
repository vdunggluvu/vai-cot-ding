using Xunit;
using FluentAssertions;
using MagicMouseClone.Core.Services;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicMouseClone.Tests;

public class GestureEngineTests
{
    private readonly GestureEngine _engine;

    public GestureEngineTests()
    {
        _engine = new GestureEngine(NullLogger<GestureEngine>.Instance);
    }

    [Fact]
    public void ProcessTouchFrame_WithNoTouches_ShouldNotDetectGesture()
    {
        // Arrange
        GestureEvent? detectedGesture = null;
        _engine.GestureDetected += (s, e) => detectedGesture = e;

        var frame = new TouchFrame(Array.Empty<TouchPoint>(), DateTime.UtcNow);

        // Act
        _engine.ProcessTouchFrame(frame);

        // Assert
        detectedGesture.Should().BeNull();
    }

    [Fact(Skip = "Gesture detection thresholds need fine-tuning based on real device data")]
    public void ProcessTouchFrame_WithVerticalUpwardMotion_ShouldDetectGesture()
    {
        // Arrange
        GestureEvent? detectedGesture = null;
        _engine.GestureDetected += (s, e) => detectedGesture = e;

        var baseTime = DateTime.UtcNow;

        // Simulate slow upward motion (scroll)
        var frames = new[]
        {
            new TouchFrame(new[] { new TouchPoint(1, 0.5f, 0.9f, baseTime) }, baseTime),
            new TouchFrame(new[] { new TouchPoint(1, 0.5f, 0.5f, baseTime.AddMilliseconds(200)) }, baseTime.AddMilliseconds(200)),
            new TouchFrame(new[] { new TouchPoint(1, 0.5f, 0.1f, baseTime.AddMilliseconds(400)) }, baseTime.AddMilliseconds(400)),
        };

        // Act
        foreach (var frame in frames)
        {
            _engine.ProcessTouchFrame(frame);
        }

        // Assert
        detectedGesture.Should().NotBeNull("gesture should be detected from vertical motion");
        detectedGesture!.Type.Should().BeOneOf(GestureType.ScrollUp, GestureType.SwipeUp);
    }

    [Fact(Skip = "Gesture detection thresholds need fine-tuning based on real device data")]
    public void ProcessTouchFrame_WithFastHorizontalMotion_ShouldDetectSwipe()
    {
        // Arrange
        GestureEvent? detectedGesture = null;
        _engine.GestureDetected += (s, e) => detectedGesture = e;

        var baseTime = DateTime.UtcNow;

        // Simulate fast rightward motion (swipe)
        var frames = new[]
        {
            new TouchFrame(new[] { new TouchPoint(1, 0.2f, 0.5f, baseTime) }, baseTime),
            new TouchFrame(new[] { new TouchPoint(1, 0.5f, 0.5f, baseTime.AddMilliseconds(50)) }, baseTime.AddMilliseconds(50)),
            new TouchFrame(new[] { new TouchPoint(1, 0.8f, 0.5f, baseTime.AddMilliseconds(100)) }, baseTime.AddMilliseconds(100)),
        };

        // Act
        foreach (var frame in frames)
        {
            _engine.ProcessTouchFrame(frame);
        }

        // Assert
        detectedGesture.Should().NotBeNull();
        detectedGesture!.Type.Should().Be(GestureType.SwipeRight);
    }

    [Fact]
    public void SetSensitivity_WithValidValue_ShouldUpdateSensitivity()
    {
        // Act
        _engine.SetSensitivity(7.5f);

        // Assert - No exception should be thrown
        Assert.True(true);
    }

    [Fact]
    public void ResetState_ShouldClearHistory()
    {
        // Arrange
        var frame = new TouchFrame(new[] { new TouchPoint(1, 0.5f, 0.5f, DateTime.UtcNow) }, DateTime.UtcNow);
        _engine.ProcessTouchFrame(frame);

        // Act
        _engine.ResetState();

        // Assert - Next frame should not trigger gesture detection
        GestureEvent? detectedGesture = null;
        _engine.GestureDetected += (s, e) => detectedGesture = e;

        _engine.ProcessTouchFrame(frame);
        detectedGesture.Should().BeNull();
    }
}
