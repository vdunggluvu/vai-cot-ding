using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MagicKeyboardUtilities.App.Diagnostics;

/// <summary>
/// Simple file logger wrapper
/// Traceability: Required for diagnostics as per implementation requirements
/// Evidence: Flow report shows no external logging observed, but needed for debugging reimplementation
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logPath;
    private static readonly object _lockObj = new();

    public FileLogger(string categoryName, string? logPath = null)
    {
        _categoryName = categoryName;
        _logPath = logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
        
        // Ensure logs directory exists
        var logDir = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";
        
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        lock (_lockObj)
        {
            try
            {
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if can't write log
            }
        }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string? _logPath;

    public FileLoggerProvider(string? logPath = null)
    {
        _logPath = logPath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logPath);
    }

    public void Dispose() { }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string? logPath = null)
    {
        builder.AddProvider(new FileLoggerProvider(logPath));
        return builder;
    }
}
