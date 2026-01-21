namespace DataFlowApp.Infrastructure.Configuration;

/// <summary>
/// Application configuration model.
/// Add custom settings here as needed.
/// </summary>
public class AppConfiguration
{
    public string ApplicationName { get; set; } = "DataFlow Desktop App";
    public string Version { get; set; } = "1.0.0";
    
    // Logging settings
    public LoggingSettings Logging { get; set; } = new();
    
    // Data processing settings
    public DataSettings Data { get; set; } = new();
    
    // UI settings
    public UiSettings Ui { get; set; } = new();
    
    public class LoggingSettings
    {
        public string LogDirectory { get; set; } = "Logs";
        public string LogLevel { get; set; } = "Info";
        public int MaxLogFileSizeMB { get; set; } = 10;
    }
    
    public class DataSettings
    {
        public string DefaultImportPath { get; set; } = "";
        public string DefaultExportPath { get; set; } = "";
        public int MaxRecordsToProcess { get; set; } = 10000;
        public bool ValidateOnImport { get; set; } = true;
    }
    
    public class UiSettings
    {
        public string Theme { get; set; } = "Light";
        public int WindowWidth { get; set; } = 1200;
        public int WindowHeight { get; set; } = 800;
        public string LastOpenedFile { get; set; } = "";
    }
}
