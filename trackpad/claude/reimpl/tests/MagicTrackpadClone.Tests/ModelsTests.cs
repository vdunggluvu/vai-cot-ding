using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class ModelsTests
{
    [Fact]
    public void DeviceInfo_CanBeInstantiated()
    {
        // Act
        var device = new DeviceInfo
        {
            DeviceId = "TEST_ID",
            DeviceName = "Test Device",
            VendorId = 0x05AC,
            ProductId = 0x030E,
            ConnectionType = DeviceConnectionType.Usb,
            IsConnected = true
        };

        // Assert
        Assert.Equal("TEST_ID", device.DeviceId);
        Assert.Equal("Test Device", device.DeviceName);
        Assert.Equal((ushort)0x05AC, device.VendorId);
        Assert.True(device.IsConnected);
    }

    [Fact]
    public void InputEventArgs_CanContainMultipleTouchPoints()
    {
        // Act
        var inputEvent = new InputEventArgs
        {
            Timestamp = DateTime.Now,
            TouchPoints = new List<TouchPoint>
            {
                new TouchPoint { ContactId = 1, X = 0.5, Y = 0.5 },
                new TouchPoint { ContactId = 2, X = 0.6, Y = 0.6 }
            },
            ButtonState = new ButtonState()
        };

        // Assert
        Assert.Equal(2, inputEvent.TouchPoints.Count);
        Assert.Equal(1, inputEvent.TouchPoints[0].ContactId);
        Assert.Equal(2, inputEvent.TouchPoints[1].ContactId);
    }

    [Fact]
    public void GestureEventArgs_HasAllRequiredProperties()
    {
        // Act
        var gesture = new GestureEventArgs
        {
            Type = GestureType.Swipe,
            FingerCount = 3,
            Direction = GestureDirection.Up,
            Velocity = 1.5,
            Distance = 0.8,
            Timestamp = DateTime.Now
        };

        // Assert
        Assert.Equal(GestureType.Swipe, gesture.Type);
        Assert.Equal(3, gesture.FingerCount);
        Assert.Equal(GestureDirection.Up, gesture.Direction);
        Assert.Equal(1.5, gesture.Velocity);
        Assert.Equal(0.8, gesture.Distance);
    }

    [Fact]
    public void AppConfiguration_DefaultConfiguration_IsValid()
    {
        // Act
        var config = new AppConfiguration
        {
            General = new GeneralSettings(),
            Gestures = new GestureConfiguration(),
            Device = new DeviceSettings()
        };

        // Assert
        Assert.NotNull(config.General);
        Assert.NotNull(config.Gestures);
        Assert.NotNull(config.Device);
    }

    [Fact]
    public void GestureMapping_CanMapGestureToAction()
    {
        // Act
        var mapping = new GestureMapping
        {
            Type = GestureType.Tap,
            FingerCount = 2,
            Direction = GestureDirection.None,
            Action = new GestureAction
            {
                Type = ActionType.MouseClick,
                Command = "Right"
            },
            Enabled = true
        };

        // Assert
        Assert.Equal(GestureType.Tap, mapping.Type);
        Assert.Equal(2, mapping.FingerCount);
        Assert.Equal(ActionType.MouseClick, mapping.Action.Type);
        Assert.Equal("Right", mapping.Action.Command);
        Assert.True(mapping.Enabled);
    }

    [Fact]
    public void TouchPoint_DefaultState_IsNone()
    {
        // Act
        var touch = new TouchPoint();

        // Assert
        Assert.Equal(TouchState.None, touch.State);
        Assert.Equal(0, touch.ContactId);
    }

    [Fact]
    public void ButtonState_DefaultValues_AreFalse()
    {
        // Act
        var buttonState = new ButtonState();

        // Assert
        Assert.False(buttonState.LeftButton);
        Assert.False(buttonState.RightButton);
        Assert.False(buttonState.MiddleButton);
    }

    [Fact]
    public void GestureAction_CanHaveParameters()
    {
        // Act
        var action = new GestureAction
        {
            Type = ActionType.SystemCommand,
            Command = "VolumeUp",
            Parameters = new Dictionary<string, string>
            {
                ["Amount"] = "10",
                ["Duration"] = "100"
            }
        };

        // Assert
        Assert.Equal(2, action.Parameters.Count);
        Assert.Equal("10", action.Parameters["Amount"]);
        Assert.Equal("100", action.Parameters["Duration"]);
    }

    [Fact]
    public void GeneralSettings_DefaultValues_AreReasonable()
    {
        // Act
        var settings = new GeneralSettings();

        // Assert
        Assert.True(settings.StartWithWindows);
        Assert.True(settings.StartMinimized);
        Assert.True(settings.EnableGestures);
        Assert.Equal("Info", settings.LogLevel);
    }

    [Fact]
    public void DeviceSettings_DefaultSensitivity_IsOne()
    {
        // Act
        var settings = new DeviceSettings();

        // Assert
        Assert.Equal(1.0, settings.Sensitivity);
        Assert.False(settings.ReverseScrollDirection);
    }
}
