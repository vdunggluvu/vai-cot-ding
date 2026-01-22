using DataFlowApp.Domain.Interfaces;
using System.Text.Json;

namespace DataFlowApp.Infrastructure.Configuration;

/// <summary>
/// Manages application configuration using JSON file.
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    private readonly string _configFilePath;
    private AppConfiguration _configuration;
    private readonly ILogger? _logger;

    public ConfigurationManager(ILogger? logger = null, string? configFilePath = null)
    {
        _logger = logger;
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
        _configuration = new AppConfiguration();
    }

    private static string GetDefaultConfigPath()
    {
        string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "appsettings.json");
    }

    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                string json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                if (config != null)
                {
                    _configuration = config;
                    _logger?.LogInfo($"Configuration loaded from {_configFilePath}");
                }
            }
            else
            {
                _logger?.LogWarning($"Configuration file not found: {_configFilePath}. Using defaults.");
                SaveConfiguration(); // Create default config file
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to load configuration from {_configFilePath}", ex);
            _configuration = new AppConfiguration();
        }
    }

    public void SaveConfiguration()
    {
        try
        {
            string directory = Path.GetDirectoryName(_configFilePath) ?? "";
            Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            string json = JsonSerializer.Serialize(_configuration, options);
            File.WriteAllText(_configFilePath, json);
            
            _logger?.LogInfo($"Configuration saved to {_configFilePath}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to save configuration to {_configFilePath}", ex);
        }
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            // Simple key-based access using reflection
            var property = typeof(AppConfiguration).GetProperty(key);
            if (property != null)
            {
                var value = property.GetValue(_configuration);
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public void SetValue<T>(string key, T value)
    {
        try
        {
            var property = typeof(AppConfiguration).GetProperty(key);
            if (property != null && property.PropertyType == typeof(T))
            {
                property.SetValue(_configuration, value);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to set configuration value for key: {key}", ex);
        }
    }

    public string GetConfigFilePath()
    {
        return _configFilePath;
    }

    public AppConfiguration GetConfiguration()
    {
        return _configuration;
    }
}
