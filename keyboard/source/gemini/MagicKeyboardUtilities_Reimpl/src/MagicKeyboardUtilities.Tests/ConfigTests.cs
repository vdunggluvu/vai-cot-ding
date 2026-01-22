using System.IO;
using System.Text.Json;
using Xunit;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.Tests
{
    public class ConfigTests
    {
        [Fact]
        public void CanParseValidJsonConfig()
        {
            var json = @"{
                ""app"": { ""startMinimized"": false },
                ""features"": { ""enableHooks"": true },
                ""remapping"": [
                    { ""fromVk"": 65, ""toVk"": 66, ""description"": ""A to B"" }
                ]
            }";

            var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(config);
            Assert.False(config.App.StartMinimized);
            Assert.True(config.Features.EnableHooks);
            Assert.Single(config.Remapping);
            Assert.Equal(65, config.Remapping[0].FromVk);
        }

        [Fact]
        public void DefaultValuesAreSafe()
        {
            var config = new AppConfig();
            Assert.True(config.App.StartMinimized); // Default should be true as per code
            Assert.False(config.Features.EnableHooks); // Default false for safety
        }
    }
}
