using System.Text.Json;
using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Domain.Models;
using DesktopScaffold.Infrastructure.Serialization;
using DesktopScaffold.Infrastructure.Utils;

namespace DesktopScaffold.Infrastructure.Config;

public sealed class JsonConfigStore : IConfigStore
{
    private readonly IFileSystem _fs;
    private readonly IAppLogger _log;

    public JsonConfigStore(IFileSystem fs, IAppLogger log)
    {
        _fs = fs;
        _log = log;
    }

    public async Task<AppConfig> LoadConfigAsync(CancellationToken ct)
    {
        var path = PathProvider.ConfigFile;
        try
        {
            if (!_fs.FileExists(path))
            {
                _log.Warn($"Config not found at '{path}', using defaults.");
                return new AppConfig { App = new AppConfig.AppSection() };
            }

            var json = await _fs.ReadAllTextAsync(path, ct).ConfigureAwait(false);
            var cfg = JsonSerializer.Deserialize<AppConfig>(json, JsonSerializerOptionsProvider.Create());
            if (cfg is null)
                throw new InvalidOperationException("Config file is empty or invalid.");

            if (cfg.App is null)
                cfg = new AppConfig { App = new AppConfig.AppSection() };

            return cfg;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to load config from '{path}', using defaults.", ex);
            return new AppConfig { App = new AppConfig.AppSection() };
        }
    }
}

