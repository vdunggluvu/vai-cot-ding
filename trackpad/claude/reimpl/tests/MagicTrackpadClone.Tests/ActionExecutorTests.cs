using Xunit;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone.Tests;

public class ActionExecutorTests
{
    private class TestLogger : ILogger
    {
        public List<string> InfoLogs { get; } = new();
        public List<string> DebugLogs { get; } = new();

        public void LogInfo(string message) => InfoLogs.Add(message);
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) => DebugLogs.Add(message);
    }

    [Fact]
    public async Task ExecuteActionAsync_KeyboardShortcut_LogsAction()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.KeyboardShortcut,
            Command = "Win+D"
        };

        // Act
        await executor.ExecuteActionAsync(action);

        // Assert
        Assert.Contains(logger.InfoLogs, log => log.Contains("Win+D"));
    }

    [Fact]
    public async Task ExecuteActionAsync_MouseClick_LogsAction()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.MouseClick,
            Command = "Left"
        };

        // Act
        await executor.ExecuteActionAsync(action);

        // Assert
        Assert.Contains(logger.InfoLogs, log => log.Contains("Mouse click"));
    }

    [Fact]
    public async Task ExecuteActionAsync_MouseScroll_LogsAction()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.MouseScroll,
            Command = "100"
        };

        // Act
        await executor.ExecuteActionAsync(action);

        // Assert
        Assert.Contains(logger.InfoLogs, log => log.Contains("Mouse scroll"));
    }

    [Fact]
    public async Task ExecuteActionAsync_SystemCommand_LogsAction()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.SystemCommand,
            Command = "ShowDesktop"
        };

        // Act
        await executor.ExecuteActionAsync(action);

        // Assert
        Assert.Contains(logger.InfoLogs, log => log.Contains("System command"));
    }

    [Fact]
    public void CanExecuteAction_ValidAction_ReturnsTrue()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.KeyboardShortcut,
            Command = "Ctrl+C"
        };

        // Act
        var canExecute = executor.CanExecuteAction(action);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void CanExecuteAction_NoneType_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.None,
            Command = "Test"
        };

        // Act
        var canExecute = executor.CanExecuteAction(action);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void CanExecuteAction_EmptyCommand_ReturnsFalse()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = ActionType.KeyboardShortcut,
            Command = ""
        };

        // Act
        var canExecute = executor.CanExecuteAction(action);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public async Task ExecuteActionAsync_InvalidAction_DoesNotThrow()
    {
        // Arrange
        var logger = new TestLogger();
        var executor = new ActionExecutor(logger);
        var action = new GestureAction
        {
            Type = (ActionType)999, // Invalid type
            Command = "Test"
        };

        // Act & Assert
        await executor.ExecuteActionAsync(action);
        Assert.True(true); // No exception means success
    }
}
