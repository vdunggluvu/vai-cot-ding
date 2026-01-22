using Xunit;
using FluentAssertions;
using MagicMouseClone.Core.Services;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MagicMouseClone.Tests;

public class JsonConfigurationManagerTests
{
    private readonly JsonConfigurationManager _manager;

    public JsonConfigurationManagerTests()
    {
        _manager = new JsonConfigurationManager(NullLogger<JsonConfigurationManager>.Instance);
    }

    [Fact]
    public async Task LoadConfigurationAsync_ShouldReturnValidConfig()
    {
        // Act
        var config = await _manager.LoadConfigurationAsync();

        // Assert
        config.Should().NotBeNull();
        config.ActiveProfile.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SaveAndLoadConfiguration_ShouldPersist()
    {
        // Arrange
        var config = new AppConfig
        {
            EnableGestures = false,
            ScrollSpeed = 7.5f,
            GestureSensitivity = 3.0f,
            ActiveProfile = "Gaming"
        };

        // Act
        await _manager.SaveConfigurationAsync(config);
        var loaded = await _manager.LoadConfigurationAsync();

        // Assert
        loaded.EnableGestures.Should().BeFalse();
        loaded.ScrollSpeed.Should().Be(7.5f);
        loaded.GestureSensitivity.Should().Be(3.0f);
        loaded.ActiveProfile.Should().Be("Gaming");
    }

    [Fact]
    public async Task LoadProfileAsync_WithNonExistentProfile_ShouldCreateDefault()
    {
        // Act
        var profile = await _manager.LoadProfileAsync("NonExistent");

        // Assert
        profile.Should().NotBeNull();
        profile.Name.Should().Be("NonExistent");
        profile.Bindings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveAndLoadProfile_ShouldPersist()
    {
        // Arrange
        var profile = new ActionProfile { Name = "TestProfile" };
        profile.SetBinding(GestureType.SwipeLeft,
            new ActionBinding(GestureType.SwipeLeft, ActionType.ExecuteApp, "calc.exe"));

        // Act
        await _manager.SaveProfileAsync(profile);
        var loaded = await _manager.LoadProfileAsync("TestProfile");

        // Assert
        loaded.Name.Should().Be("TestProfile");
        loaded.Bindings.Should().ContainKey(GestureType.SwipeLeft);
        loaded.Bindings[GestureType.SwipeLeft].ActionParameter.Should().Be("calc.exe");
    }

    [Fact]
    public async Task GetProfileNamesAsync_ShouldReturnAvailableProfiles()
    {
        // Arrange
        await _manager.SaveProfileAsync(new ActionProfile { Name = "Profile1" });
        await _manager.SaveProfileAsync(new ActionProfile { Name = "Profile2" });

        // Act
        var names = await _manager.GetProfileNamesAsync();

        // Assert
        names.Should().Contain("Profile1");
        names.Should().Contain("Profile2");
    }
}
