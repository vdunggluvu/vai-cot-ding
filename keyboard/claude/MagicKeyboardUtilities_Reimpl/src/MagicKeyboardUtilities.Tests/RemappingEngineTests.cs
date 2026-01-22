using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Core;
using Xunit;

namespace MagicKeyboardUtilities.Tests;

/// <summary>
/// Unit tests for remapping engine
/// Traceability: Section 4.2 INPUT HOOK FLOW Step 3
/// </summary>
public class RemappingEngineTests
{
    [Fact]
    public void RemappingEngine_LoadMappings_Success()
    {
        // Arrange
        var engine = new RemappingEngine(NullLogger<RemappingEngine>.Instance);
        var mappings = new List<KeyRemapping>
        {
            new() { FromVk = 124, ToVk = 175, Description = "F13 -> Volume Up" },
            new() { FromVk = 125, ToVk = 174, Description = "F14 -> Volume Down" }
        };

        // Act
        engine.LoadMappings(mappings);

        // Assert
        var loadedMappings = engine.GetMappings();
        Assert.Equal(2, loadedMappings.Count);
        Assert.Equal(175, loadedMappings[124]);
        Assert.Equal(174, loadedMappings[125]);
    }

    [Fact]
    public void RemappingEngine_TryRemap_Found()
    {
        // Arrange
        var engine = new RemappingEngine(NullLogger<RemappingEngine>.Instance);
        engine.LoadMappings(new List<KeyRemapping>
        {
            new() { FromVk = 124, ToVk = 175 }
        });

        // Act
        var result = engine.TryRemap(124, out var remappedVk);

        // Assert
        Assert.True(result);
        Assert.Equal(175, remappedVk);
    }

    [Fact]
    public void RemappingEngine_TryRemap_NotFound()
    {
        // Arrange
        var engine = new RemappingEngine(NullLogger<RemappingEngine>.Instance);
        engine.LoadMappings(new List<KeyRemapping>());

        // Act
        var result = engine.TryRemap(124, out var remappedVk);

        // Assert
        Assert.False(result);
        Assert.Equal(124, remappedVk); // Should return original
    }

    [Fact]
    public void RemappingEngine_ClearMappings_Success()
    {
        // Arrange
        var engine = new RemappingEngine(NullLogger<RemappingEngine>.Instance);
        engine.LoadMappings(new List<KeyRemapping>
        {
            new() { FromVk = 124, ToVk = 175 }
        });

        // Act
        engine.ClearMappings();

        // Assert
        Assert.Empty(engine.GetMappings());
    }
}
