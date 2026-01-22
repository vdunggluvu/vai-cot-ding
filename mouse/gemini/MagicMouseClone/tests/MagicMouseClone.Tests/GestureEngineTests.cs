using System;
using Xunit;
using MagicMouseClone.Core;
using System.Collections.Generic;

namespace MagicMouseClone.Tests
{
    public class GestureEngineTests
    {
        [Fact]
        public void ScrollDown_Detected_When_Moving_Down()
        {
            // Arrange
            var engine = new GestureEngine();
            GestureEventArgs? result = null;
            engine.GestureDetected += (s, e) => result = e;

            // Act
            // Frame 1
            engine.ProcessTouchFrame(new TouchFrame 
            { 
                Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.5f, IsDown = true } },
                Timestamp = 1000
            });

            // Frame 2 (Moved down by 0.1)
            engine.ProcessTouchFrame(new TouchFrame 
            { 
                Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.6f, IsDown = true } },
                Timestamp = 1016
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GestureType.ScrollDown, result.Type);
        }

        [Fact]
        public void No_Gesture_When_Movement_Is_Small()
        {
            // Arrange
            var engine = new GestureEngine();
            GestureEventArgs? result = null;
            engine.GestureDetected += (s, e) => result = e;

            // Act
            engine.ProcessTouchFrame(new TouchFrame 
            { 
                Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.5f, IsDown = true } },
                Timestamp = 1000
            });

            engine.ProcessTouchFrame(new TouchFrame 
            { 
                Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.501f, IsDown = true } },
                Timestamp = 1016
            });

            // Assert
            Assert.Null(result);
        }
    }
}
