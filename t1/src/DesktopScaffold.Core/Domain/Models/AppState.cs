namespace DesktopScaffold.Core.Domain.Models;

public sealed class AppState
{
    public List<string> RecentFiles { get; init; } = new();
    public DateTimeOffset LastOpenedAt { get; set; } = DateTimeOffset.UtcNow;
}

