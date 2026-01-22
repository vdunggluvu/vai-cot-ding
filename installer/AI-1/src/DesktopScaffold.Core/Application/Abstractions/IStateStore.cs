using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.Abstractions;

public interface IStateStore
{
    Task<AppState> LoadAsync(CancellationToken ct);
    Task SaveAsync(AppState state, CancellationToken ct);
}

