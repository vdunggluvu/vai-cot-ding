using DesktopScaffold.Core.Application.Abstractions;

namespace DesktopScaffold.Infrastructure.FileSystem;

public sealed class PhysicalFileSystem : IFileSystem
{
    public Task<string> ReadAllTextAsync(string path, CancellationToken ct)
        => System.IO.File.ReadAllTextAsync(path, ct);

    public async Task WriteAllTextAsync(string path, string content, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        await System.IO.File.WriteAllTextAsync(path, content, ct).ConfigureAwait(false);
    }

    public bool FileExists(string path) => System.IO.File.Exists(path);

    public void EnsureDirectory(string path) => Directory.CreateDirectory(path);
}

