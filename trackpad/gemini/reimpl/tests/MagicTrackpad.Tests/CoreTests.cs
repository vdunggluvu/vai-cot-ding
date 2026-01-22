using MagicTrackpad.Core.Models;
using MagicTrackpad.Core.Services;
using Xunit;

namespace MagicTrackpad.Tests;

public class CoreTests
{
    [Fact]
    public void Config_LoadDate_Defaults()
    {
        var path = "test_config.json";
        if (File.Exists(path)) File.Delete(path);

        var config = new FileConfigProvider(path);
        Assert.True(config.IsGestureEnabled(GestureType.Tap));
        Assert.True(config.IsGestureEnabled(GestureType.ScrollVertical));
    }

    [Fact]
    public void Config_SaveAndLoad_PreservesValues()
    {
        var path = "test_config_persist.json";
        if (File.Exists(path)) File.Delete(path);

        var config = new FileConfigProvider(path);
        config.SetGestureEnabled(GestureType.Pinch, true);
        config.Save();

        var config2 = new FileConfigProvider(path);
        Assert.True(config2.IsGestureEnabled(GestureType.Pinch));
    }

    [Fact]
    public void Gesture_TapDetection()
    {
        var engine = new GestureStateMachine();
        Gesture? detected = null;
        engine.GestureDetected += (s, g) => detected = g;

        // 1. Touch Down (1 finger)
        engine.ProcessFrame(new TouchFrame 
        { 
            Timestamp = DateTime.UtcNow,
            Points = new List<TouchPoint> { new TouchPoint { Id = 1, IsContact = true } }
        });

        // 2. Touch Up (0 fingers)
        engine.ProcessFrame(new TouchFrame 
        { 
            Timestamp = DateTime.UtcNow.AddMilliseconds(100),
            Points = new List<TouchPoint>()
        });

        Assert.NotNull(detected);
        Assert.Equal(GestureType.Tap, detected.Type);
    }

    [Fact]
    public void Action_MockExecution()
    {
        var executor = new MockActionExecutor();
        var gesture = new Gesture { Type = GestureType.Tap };
        
        executor.Execute(gesture);
        
        Assert.Single(executor.ExecutedGestures);
        Assert.Equal(GestureType.Tap, executor.ExecutedGestures[0].Type);
    }
}
