using System.Text.Json;
using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Domain.Models;
using DesktopScaffold.Infrastructure.Serialization;
using DesktopScaffold.Infrastructure.Utils;

namespace DesktopScaffold.Infrastructure.State;

public sealed class JsonStateStore : IStateStore
{
    private readonly IFileSystem _fs;
    private readonly IAppLogger _log;

    public JsonStateStore(IFileSystem fs, IAppLogger log)
    {
        _fs = fs;
        _log = log;
    }

    public async Task<AppState> LoadAsync(CancellationToken ct)
    {
        var path = PathProvider.StateFile;
        try
        {
            if (!_fs.FileExists(path))
            {
                _log.Info("State not found, using new state.");
                return new AppState();
            }

            var json = await _fs.ReadAllTextAsync(path, ct).ConfigureAwait(false);
            var state = JsonSerializer.Deserialize<AppState>(json, JsonSerializerOptionsProvider.Create());
            return state ?? new AppState();
        }
        catch (Exception ex)
        {
            _log.Error("Failed to load state, using new state.", ex);
            return new AppState();
        }
    }

    public async Task SaveAsync(AppState state, CancellationToken ct)
    {
        var path = PathProvider.StateFile;
        try
        {
            var json = JsonSerializer.Serialize(state, JsonSerializerOptionsProvider.Create());
            await _fs.WriteAllTextAsync(path, json, ct).ConfigureAwait(false);
            _log.Info("State saved.");
        }
        catch (Exception ex)
        {
            _log.Error("Failed to save state.", ex);
        }
    }
}

