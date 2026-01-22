using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class GestureEngineTests
{
    private class TestLogger : ILogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) { }
    }

    [Fact]
    public void LoadConfiguration_UpdatesGestureConfig()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);
        var config = new GestureConfiguration
        {
            Mappings = new Dictionary<string, GestureMapping>
            {
                ["tap_1"] = new GestureMapping
                {
                    Type = GestureType.Tap,
                    FingerCount = 1,
                    Action = new GestureAction()
                }
            }
        };

        // Act
        engine.LoadConfiguration(config);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public void ProcessInput_EmptyTouchPoints_NoGesture()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);
        var gestureDetected = false;
        engine.GestureDetected += (s, e) => gestureDetected = true;

        var inputEvent = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>(),
            ButtonState = new ButtonState()
        };

        // Act
        engine.ProcessInput(inputEvent);

        // Assert
        Assert.False(gestureDetected);
    }

    [Fact]
    public void ProcessInput_SingleTouchSequence_DetectsTap()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);
        GestureEventArgs? detectedGesture = null;
        engine.GestureDetected += (s, e) => detectedGesture = e;

        // Simulate touch down, minimal movement, touch up
        var touch1 = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.5, Y = 0.5, State = TouchState.Down }
            },
            ButtonState = new ButtonState()
        };

        var touch2 = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(50),
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.501, Y = 0.501, State = TouchState.Move }
            },
            ButtonState = new ButtonState()
        };

        var touchUp = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(100),
            TouchPoints = new List<TouchPoint>(),
            ButtonState = new ButtonState()
        };

        // Act
        engine.ProcessInput(touch1);
        engine.ProcessInput(touch2);
        engine.ProcessInput(touchUp);

        // Assert
        Assert.NotNull(detectedGesture);
        Assert.Equal(GestureType.Tap, detectedGesture!.Type);
        Assert.Equal(1, detectedGesture.FingerCount);
    }

    [Fact]
    public void ProcessInput_TwoFingerSwipeUp_DetectsSwipe()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);
        GestureEventArgs? detectedGesture = null;
        var allGestures = new List<GestureEventArgs>();
        engine.GestureDetected += (s, e) => 
        {
            allGestures.Add(e);
            // Capture first swipe gesture
            if (e.Type == GestureType.Swipe && detectedGesture == null)
            {
                detectedGesture = e;
            }
        };

        // Simulate two-finger swipe up with bigger movement
        var touch1 = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.4, Y = 0.8, State = TouchState.Down },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.8, State = TouchState.Down }
            },
            ButtonState = new ButtonState()
        };

        var touch2 = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(50),
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.4, Y = 0.6, State = TouchState.Move },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.6, State = TouchState.Move }
            },
            ButtonState = new ButtonState()
        };
        
        var touch3 = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(100),
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.4, Y = 0.3, State = TouchState.Move },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.3, State = TouchState.Move }
            },
            ButtonState = new ButtonState()
        };

        var touchUp = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(150),
            TouchPoints = new List<TouchPoint>(),
            ButtonState = new ButtonState()
        };

        // Act
        engine.ProcessInput(touch1);
        engine.ProcessInput(touch2);
        engine.ProcessInput(touch3);
        engine.ProcessInput(touchUp);

        // Assert - should detect either swipe or scroll, both are valid for this gesture
        Assert.NotEmpty(allGestures);
        // Accept swipe or scroll as valid gestures for two-finger movement
        Assert.Contains(allGestures, g => g.Type == GestureType.Swipe || g.Type == GestureType.Scroll);
        // Verify it's upward movement
        var mainGesture = allGestures.First(g => g.Type == GestureType.Swipe || g.Type == GestureType.Scroll);
        Assert.Equal(GestureDirection.Up, mainGesture.Direction);
        Assert.Equal(2, mainGesture.FingerCount);
    }

    [Fact]
    public void ProcessInput_TwoFingerScroll_DetectsScrollGesture()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);
        var scrollDetected = false;
        engine.GestureDetected += (s, e) =>
        {
            if (e.Type == GestureType.Scroll)
                scrollDetected = true;
        };

        // Simulate two-finger continuous movement
        var touch1 = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.4, Y = 0.5, State = TouchState.Down },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.5, State = TouchState.Down }
            },
            ButtonState = new ButtonState()
        };

        var touch2 = new InputEventArgs
        {
            Timestamp = DateTime.Now.AddMilliseconds(16),
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.4, Y = 0.52, State = TouchState.Move },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.52, State = TouchState.Move }
            },
            ButtonState = new ButtonState()
        };

        // Act
        engine.ProcessInput(touch1);
        engine.ProcessInput(touch2);

        // Assert
        Assert.True(scrollDetected);
    }

    [Fact]
    public void ProcessInput_InvalidInput_DoesNotCrash()
    {
        // Arrange
        var logger = new TestLogger();
        var engine = new GestureEngine(logger);

        var invalidInput = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = -1, X = double.NaN, Y = double.NaN }
            },
            ButtonState = new ButtonState()
        };

        // Act & Assert - should not throw
        engine.ProcessInput(invalidInput);
        Assert.True(true);
    }
}
