using MagicTrackpad.Core.Services;
using MagicTrackpad.Core.Models;
using Xunit;

namespace MagicTrackpad.Tests;

public class IntegrationTests
{
    [Fact]
    public void Device_Enumeration_Mock()
    {
        var exposer = new MockDeviceExposer();
        bool connected = false;
        exposer.DeviceConnected += (s, id) => connected = true;

        exposer.SimulateConnection("Device1");
        Assert.True(connected);
    }

    [Fact]
    public void FullFlow_GestureToAction()
    {
        // Assemble
        var engine = new GestureStateMachine();
        var executor = new MockActionExecutor();
        
        // Connect
        engine.GestureDetected += (s, gesture) => executor.Execute(gesture);

        // Act (Simulate Tap)
        engine.ProcessFrame(new TouchFrame 
        { 
            Timestamp = DateTime.UtcNow, 
            Points = new List<TouchPoint> { new TouchPoint { Id=1, IsContact=true } } 
        });
        engine.ProcessFrame(new TouchFrame 
        { 
            Timestamp = DateTime.UtcNow.AddMilliseconds(50), 
            Points = new List<TouchPoint>() 
        });

        // Assert
        Assert.Single(executor.ExecutedGestures);
        Assert.Equal(GestureType.Tap, executor.ExecutedGestures[0].Type);
    }
}
