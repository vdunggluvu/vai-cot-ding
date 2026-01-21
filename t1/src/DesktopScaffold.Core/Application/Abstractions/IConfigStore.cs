using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.Abstractions;

public interface IConfigStore
{
    Task<AppConfig> LoadConfigAsync(CancellationToken ct);
}

