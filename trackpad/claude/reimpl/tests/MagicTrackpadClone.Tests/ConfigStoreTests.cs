using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class ConfigStoreTests
{
    private class TestLogger : ILogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) { }
    }

    [Fact]
    public async Task LoadConfiguration_ReturnsDefaultWhenNoConfigExists()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        var logger = new TestLogger();
        var store = new ConfigStore(logger, tempPath);

        // Act
        var config = await store.LoadConfigurationAsync();

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config!.General);
        Assert.NotNull(config.Gestures);
        Assert.NotNull(config.Device);
        Assert.True(config.General.EnableGestures);
    }

    [Fact]
    public async Task SaveAndLoadConfiguration_PreservesData()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        var logger = new TestLogger();
        var store = new ConfigStore(logger, tempPath);

        var originalConfig = new AppConfiguration
        {
            General = new GeneralSettings
            {
                StartWithWindows = false,
                EnableGestures = true,
                LogLevel = "Debug"
            },
            Device = new DeviceSettings
            {
                Sensitivity = 1.5,
                ReverseScrollDirection = true
            }
        };

        // Act
        await store.SaveConfigurationAsync(originalConfig);
        var loadedConfig = await store.LoadConfigurationAsync();

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(originalConfig.General.StartWithWindows, loadedConfig!.General.StartWithWindows);
        Assert.Equal(originalConfig.General.EnableGestures, loadedConfig.General.EnableGestures);
        Assert.Equal(originalConfig.Device.Sensitivity, loadedConfig.Device.Sensitivity);
        Assert.Equal(originalConfig.Device.ReverseScrollDirection, loadedConfig.Device.ReverseScrollDirection);

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public async Task ValidateConfiguration_ValidConfigReturnsTrue()
    {
        // Arrange
        var logger = new TestLogger();
        var store = new ConfigStore(logger);
        var config = new AppConfiguration
        {
            General = new GeneralSettings(),
            Gestures = new GestureConfiguration
            {
                Mappings = new Dictionary<string, GestureMapping>
                {
                    ["test"] = new GestureMapping
                    {
                        Type = GestureType.Tap,
                        FingerCount = 2,
                        Action = new GestureAction { Type = ActionType.MouseClick }
                    }
                }
            },
            Device = new DeviceSettings()
        };

        // Act
        var isValid = await store.ValidateConfigurationAsync(config);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateConfiguration_NullConfigReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var store = new ConfigStore(logger);

        // Act
        var isValid = await store.ValidateConfigurationAsync(null!);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateConfiguration_InvalidFingerCountReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var store = new ConfigStore(logger);
        var config = new AppConfiguration
        {
            General = new GeneralSettings(),
            Gestures = new GestureConfiguration
            {
                Mappings = new Dictionary<string, GestureMapping>
                {
                    ["test"] = new GestureMapping
                    {
                        Type = GestureType.Tap,
                        FingerCount = 10, // Invalid
                        Action = new GestureAction()
                    }
                }
            },
            Device = new DeviceSettings()
        };

        // Act
        var isValid = await store.ValidateConfigurationAsync(config);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task LoadConfiguration_CorruptedFileReturnsDefault()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        File.WriteAllText(tempPath, "{ invalid json content }");
        
        var logger = new TestLogger();
        var store = new ConfigStore(logger, tempPath);

        // Act
        var config = await store.LoadConfigurationAsync();

        // Assert
        Assert.NotNull(config);
        Assert.True(config!.General.EnableGestures); // Default value

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }
}
