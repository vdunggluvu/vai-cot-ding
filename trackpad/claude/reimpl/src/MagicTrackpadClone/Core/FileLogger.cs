using System.IO;

namespace MagicTrackpadClone.Core;

public class FileLogger : ILogger
{
    private readonly string _logPath;
    private readonly object _lock = new();
    private readonly string _logLevel;

    public FileLogger(string logPath, string logLevel = "Info")
    {
        _logPath = logPath;
        _logLevel = logLevel;
        
        var logDir = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
    }

    public void LogInfo(string message) => Log("INFO", message);
    public void LogWarning(string message) => Log("WARN", message);
    public void LogError(string message, Exception? ex = null)
    {
        var fullMessage = ex != null ? $"{message}: {ex.Message}\n{ex.StackTrace}" : message;
        Log("ERROR", fullMessage);
    }
    public void LogDebug(string message)
    {
        if (_logLevel == "Debug")
        {
            Log("DEBUG", message);
        }
    }

    private void Log(string level, string message)
    {
        lock (_lock)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logLine = $"[{timestamp}] [{level}] {message}";
                File.AppendAllText(_logPath, logLine + Environment.NewLine);
                
                // Console output for debugging
                Console.WriteLine(logLine);
            }
            catch
            {
                // Fail silently if logging fails
            }
        }
    }
}
