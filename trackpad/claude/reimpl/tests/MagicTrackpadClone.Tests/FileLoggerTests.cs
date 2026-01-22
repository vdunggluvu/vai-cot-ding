using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class FileLoggerTests
{
    [Fact]
    public void LogInfo_CreatesLogFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath);

        // Act
        logger.LogInfo("Test message");

        // Assert
        Assert.True(File.Exists(tempPath));

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void LogInfo_WritesMessageToFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath);

        // Act
        logger.LogInfo("Test info message");

        // Assert
        var content = File.ReadAllText(tempPath);
        Assert.Contains("Test info message", content);
        Assert.Contains("[INFO]", content);

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void LogError_WritesErrorWithException()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath);

        // Act
        logger.LogError("Test error", new Exception("Test exception"));

        // Assert
        var content = File.ReadAllText(tempPath);
        Assert.Contains("Test error", content);
        Assert.Contains("Test exception", content);
        Assert.Contains("[ERROR]", content);

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void LogDebug_WithDebugLevel_WritesMessage()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath, "Debug");

        // Act
        logger.LogDebug("Test debug message");

        // Assert
        var content = File.ReadAllText(tempPath);
        Assert.Contains("Test debug message", content);
        Assert.Contains("[DEBUG]", content);

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void LogDebug_WithInfoLevel_DoesNotWriteMessage()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath, "Info");

        // Act
        logger.LogDebug("Test debug message");

        // Assert
        if (File.Exists(tempPath))
        {
            var content = File.ReadAllText(tempPath);
            Assert.DoesNotContain("Test debug message", content);
        }

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void MultipleLogCalls_AppendsToSameFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        var logger = new FileLogger(tempPath);

        // Act
        logger.LogInfo("First message");
        logger.LogInfo("Second message");
        logger.LogInfo("Third message");

        // Assert
        var content = File.ReadAllText(tempPath);
        Assert.Contains("First message", content);
        Assert.Contains("Second message", content);
        Assert.Contains("Third message", content);

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void LoggerCreation_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempPath = Path.Combine(tempDir, "test.log");

        // Act
        var logger = new FileLogger(tempPath);
        logger.LogInfo("Test");

        // Assert
        Assert.True(Directory.Exists(tempDir));
        Assert.True(File.Exists(tempPath));

        // Cleanup
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }
}
