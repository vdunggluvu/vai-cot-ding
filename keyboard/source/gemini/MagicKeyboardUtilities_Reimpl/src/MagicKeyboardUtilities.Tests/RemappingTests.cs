using System.Collections.Generic;
using Xunit;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Core;

namespace MagicKeyboardUtilities.Tests
{
    public class RemappingTests
    {
        [Fact]
        public void FindMap_ReturnsCorrectEntry_WhenMatchExists()
        {
            // Arrange
            var mappings = new List<RemapEntry>
            {
                new RemapEntry { FromVk = 100, ToVk = 200 },
                new RemapEntry { FromVk = 101, ToVk = 201 }
            };
            var engine = new RemappingEngine(mappings);

            // Act
            var result = engine.FindMap(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.ToVk);
        }

        [Fact]
        public void FindMap_ReturnsNull_WhenNoMatch()
        {
            // Arrange
            var mappings = new List<RemapEntry>
            {
                new RemapEntry { FromVk = 100, ToVk = 200 }
            };
            var engine = new RemappingEngine(mappings);

            // Act
            var result = engine.FindMap(999);

            // Assert
            Assert.Null(result);
        }
    }
}
