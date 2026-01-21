namespace DesktopScaffold.Core.Application.Abstractions;

public interface IFileSystem
{
    Task<string> ReadAllTextAsync(string path, CancellationToken ct);
    Task WriteAllTextAsync(string path, string content, CancellationToken ct);
    bool FileExists(string path);
    void EnsureDirectory(string path);
}

