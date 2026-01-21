using DataFlowApp.Domain.Interfaces;
using System.Globalization;
using System.Text;

namespace DataFlowApp.Infrastructure.Logging;

/// <summary>
/// File-based logger implementation.
/// Logs are written to a file in the Logs directory.
/// </summary>
public class FileLogger : ILogger, IDisposable
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new();
    private StreamWriter? _writer;
    private bool _disposed;

    public FileLogger(string logDirectory = "Logs")
    {
        // Create logs directory if it doesn't exist
        if (!Path.IsPathRooted(logDirectory))
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory);
        }

        Directory.CreateDirectory(logDirectory);

        // Create log file with timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        _logFilePath = Path.Combine(logDirectory, $"app_{timestamp}.log");

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            _writer = new StreamWriter(_logFilePath, append: true, Encoding.UTF8)
            {
                AutoFlush = true
            };
            LogInfo($"Logger initialized. Log file: {_logFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize logger: {ex.Message}");
        }
    }

    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("WARN", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        string fullMessage = exception != null 
            ? $"{message} | Exception: {exception.GetType().Name} - {exception.Message}\n{exception.StackTrace}"
            : message;
        WriteLog("ERROR", fullMessage);
    }

    public void LogDebug(string message)
    {
        WriteLog("DEBUG", message);
    }

    private void WriteLog(string level, string message)
    {
        if (_disposed || _writer == null) return;

        lock (_lockObject)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                string logEntry = $"[{timestamp}] [{level}] {message}";
                _writer.WriteLine(logEntry);
                
                // Also write to console for debugging
                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lockObject)
        {
            if (_writer != null)
            {
                LogInfo("Logger shutting down.");
                _writer.Dispose();
                _writer = null;
            }
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
