using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using MagicKeyboardUtilities.App.Core;
using Xunit;

namespace MagicKeyboardUtilities.Tests;

/// <summary>
/// Unit tests for action dispatcher
/// Traceability: Section 4.2 Step 4 "Action Execution"
/// </summary>
public class ActionDispatcherTests
{
    [Fact]
    public void ActionDispatcher_RegisterAndExecute_Success()
    {
        // Arrange
        var dispatcher = new ActionDispatcher(NullLogger<ActionDispatcher>.Instance);
        var executed = false;
        dispatcher.RegisterAction("TestAction", () => executed = true);

        // Act
        var result = dispatcher.Execute("TestAction");

        // Assert
        Assert.True(result);
        Assert.True(executed);
    }

    [Fact]
    public void ActionDispatcher_Execute_UnknownAction()
    {
        // Arrange
        var dispatcher = new ActionDispatcher(NullLogger<ActionDispatcher>.Instance);

        // Act
        var result = dispatcher.Execute("UnknownAction");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ActionDispatcher_Execute_CaseInsensitive()
    {
        // Arrange
        var dispatcher = new ActionDispatcher(NullLogger<ActionDispatcher>.Instance);
        var executed = false;
        dispatcher.RegisterAction("TestAction", () => executed = true);

        // Act
        var result = dispatcher.Execute("testaction");

        // Assert
        Assert.True(result);
        Assert.True(executed);
    }

    [Fact]
    public void ActionDispatcher_GetRegisteredActions_ReturnsAll()
    {
        // Arrange
        var dispatcher = new ActionDispatcher(NullLogger<ActionDispatcher>.Instance);
        dispatcher.RegisterAction("Action1", () => { });
        dispatcher.RegisterAction("Action2", () => { });

        // Act
        var actions = dispatcher.GetRegisteredActions().ToList();

        // Assert
        Assert.Equal(2, actions.Count);
        Assert.Contains("Action1", actions);
        Assert.Contains("Action2", actions);
    }
}
