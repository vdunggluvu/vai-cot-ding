using Xunit;
using FluentAssertions;
using MagicMouseClone.Core.Services;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicMouseClone.Tests;

public class ActionMapperTests
{
    private readonly ActionMapper _mapper;

    public ActionMapperTests()
    {
        _mapper = new ActionMapper(NullLogger<ActionMapper>.Instance);
    }

    [Fact]
    public void GetCurrentProfile_ShouldReturnDefaultProfile()
    {
        // Act
        var profile = _mapper.GetCurrentProfile();

        // Assert
        profile.Should().NotBeNull();
        profile.Name.Should().Be("Default");
        profile.Bindings.Should().NotBeEmpty();
    }

    [Fact]
    public void LoadProfile_ShouldUpdateCurrentProfile()
    {
        // Arrange
        var customProfile = new ActionProfile { Name = "Custom" };
        customProfile.SetBinding(GestureType.SwipeUp,
            new ActionBinding(GestureType.SwipeUp, ActionType.ExecuteApp, "notepad.exe"));

        // Act
        _mapper.LoadProfile(customProfile);
        var loaded = _mapper.GetCurrentProfile();

        // Assert
        loaded.Name.Should().Be("Custom");
        loaded.Bindings.Should().ContainKey(GestureType.SwipeUp);
    }

    [Fact]
    public async Task ExecuteActionAsync_WithNoBinding_ShouldNotThrow()
    {
        // Arrange
        var gestureEvent = new GestureEvent(GestureType.TapTriple, 1.0f, DateTime.UtcNow);

        // Act
        var act = async () => await _mapper.ExecuteActionAsync(gestureEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteActionAsync_WithValidBinding_ShouldExecute()
    {
        // Arrange
        var profile = new ActionProfile { Name = "Test" };
        profile.SetBinding(GestureType.ScrollUp,
            new ActionBinding(GestureType.ScrollUp, ActionType.MouseEvent, "WheelUp"));

        _mapper.LoadProfile(profile);

        var gestureEvent = new GestureEvent(GestureType.ScrollUp, 1.0f, DateTime.UtcNow);

        // Act
        var act = async () => await _mapper.ExecuteActionAsync(gestureEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
