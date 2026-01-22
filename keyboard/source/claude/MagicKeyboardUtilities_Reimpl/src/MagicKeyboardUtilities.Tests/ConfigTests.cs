using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using MagicKeyboardUtilities.App.Config;
using Xunit;

namespace MagicKeyboardUtilities.Tests;

/// <summary>
/// Unit tests for configuration system
/// Traceability: Section 8.1 Phase 8 "Testing & Polish"
/// </summary>
public class ConfigTests
{
    [Fact]
    public void ConfigStore_LoadDefaultConfig_Success()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "nonexistent_config.json");
        var store = new ConfigStore(NullLogger<ConfigStore>.Instance, tempPath);

        // Act
        var result = store.Load();

        // Assert
        Assert.False(result); // Should return false for non-existent file
        Assert.NotNull(store.Current);
        Assert.NotNull(store.Current.App);
        Assert.True(store.Current.App.StartMinimized);
    }

    [Fact]
    public void ConfigStore_SaveAndLoad_RoundTrip()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var store = new ConfigStore(NullLogger<ConfigStore>.Instance, tempPath);
        
        try
        {
            store.Current.App.StartMinimized = false;
            store.Current.Features.EnableHooks = true;
            store.MarkDirty();

            // Act
            var saveResult = store.Save();
            Assert.True(saveResult);

            var loadStore = new ConfigStore(NullLogger<ConfigStore>.Instance, tempPath);
            var loadResult = loadStore.Load();

            // Assert
            Assert.True(loadResult);
            Assert.False(loadStore.Current.App.StartMinimized);
            Assert.True(loadStore.Current.Features.EnableHooks);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void AppConfig_DefaultValues_Valid()
    {
        // Arrange & Act
        var config = new AppConfig();

        // Assert
        Assert.NotNull(config.App);
        Assert.NotNull(config.Features);
        Assert.NotNull(config.Remapping);
        Assert.NotNull(config.Hotkeys);
        Assert.NotNull(config.Device);
    }
}
