using System.Text;
using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Infrastructure.Utils;

namespace DesktopScaffold.Infrastructure.Logging;

public sealed class FileAppLogger : IAppLogger
{
    private readonly object _lock = new();
    private readonly string _logFilePath;

    public FileAppLogger()
    {
        Directory.CreateDirectory(PathProvider.LogsDir);
        var fileName = $"app-{DateTime.Now:yyyyMMdd}.log";
        _logFilePath = Path.Combine(PathProvider.LogsDir, fileName);
    }

    public void Info(string message) => Write("INFO", message, null);
    public void Warn(string message) => Write("WARN", message, null);
    public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        var sb = new StringBuilder();
        sb.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");
        sb.Append(level).Append(' ').Append(message);
        if (ex is not null)
        {
            sb.AppendLine();
            sb.Append(ex);
        }
        sb.AppendLine();

        lock (_lock)
        {
            File.AppendAllText(_logFilePath, sb.ToString());
        }
    }
}

