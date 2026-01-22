using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;

namespace MagicTrackpadClone.Core;

public class ConfigStore : IConfigStore
{
    private readonly ILogger _logger;
    private readonly string _configFilePath;
    private const string RegistryKeyPath = @"Software\MagicTrackpadClone";

    public ConfigStore(ILogger logger, string? configPath = null)
    {
        _logger = logger;
        
        if (configPath == null)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appData, "MagicTrackpadClone");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            _configFilePath = Path.Combine(configDir, "config.json");
        }
        else
        {
            _configFilePath = configPath;
        }
    }

    public Task<AppConfiguration?> LoadConfigurationAsync()
    {
        return Task.Run<AppConfiguration?>(() =>
        {
            try
            {
                // Try file first
                if (File.Exists(_configFilePath))
                {
                    _logger.LogInfo($"Loading config from file: {_configFilePath}");
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfiguration>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
                
                // Try registry as fallback
                var regConfig = LoadFromRegistry();
                if (regConfig != null)
                {
                    _logger.LogInfo("Loaded config from registry");
                    return regConfig;
                }
                
                // Return default config
                _logger.LogInfo("Using default configuration");
                return CreateDefaultConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load configuration", ex);
                return CreateDefaultConfiguration();
            }
        });
    }

    public Task SaveConfigurationAsync(AppConfiguration config)
    {
        return Task.Run(() =>
        {
            try
            {
                // Save to file
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
                _logger.LogInfo($"Saved config to file: {_configFilePath}");
                
                // Also save to registry
                SaveToRegistry(config);
                _logger.LogInfo("Saved config to registry");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save configuration", ex);
                throw;
            }
        });
    }

    public Task<bool> ValidateConfigurationAsync(AppConfiguration config)
    {
        return Task.Run(() =>
        {
            try
            {
                if (config == null) return false;
                if (config.General == null) return false;
                if (config.Gestures == null) return false;
                if (config.Device == null) return false;
                
                // Validate gesture mappings
                foreach (var mapping in config.Gestures.Mappings.Values)
                {
                    if (mapping.Action == null) return false;
                    if (mapping.FingerCount < 1 || mapping.FingerCount > 5) return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    private AppConfiguration CreateDefaultConfiguration()
    {
        var config = new AppConfiguration
        {
            General = new GeneralSettings
            {
                StartWithWindows = true,
                StartMinimized = true,
                EnableGestures = true,
                LogLevel = "Info"
            },
            Device = new DeviceSettings
            {
                Sensitivity = 1.0,
                ReverseScrollDirection = false
            },
            Gestures = new GestureConfiguration
            {
                Mappings = new Dictionary<string, GestureMapping>
                {
                    ["tap_1"] = new GestureMapping
                    {
                        Type = GestureType.Tap,
                        FingerCount = 1,
                        Direction = GestureDirection.None,
                        Action = new GestureAction
                        {
                            Type = ActionType.MouseClick,
                            Command = "Left"
                        }
                    },
                    ["tap_2"] = new GestureMapping
                    {
                        Type = GestureType.Tap,
                        FingerCount = 2,
                        Direction = GestureDirection.None,
                        Action = new GestureAction
                        {
                            Type = ActionType.MouseClick,
                            Command = "Right"
                        }
                    },
                    ["scroll_2"] = new GestureMapping
                    {
                        Type = GestureType.Scroll,
                        FingerCount = 2,
                        Direction = GestureDirection.None,
                        Action = new GestureAction
                        {
                            Type = ActionType.MouseScroll,
                            Command = "Vertical"
                        }
                    },
                    ["swipe_3_up"] = new GestureMapping
                    {
                        Type = GestureType.Swipe,
                        FingerCount = 3,
                        Direction = GestureDirection.Up,
                        Action = new GestureAction
                        {
                            Type = ActionType.KeyboardShortcut,
                            Command = "Win+D"
                        }
                    }
                }
            }
        };
        
        return config;
    }

    private AppConfiguration? LoadFromRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null) return null;
            
            var json = key.GetValue("Configuration") as string;
            if (string.IsNullOrEmpty(json)) return null;
            
            return JsonConvert.DeserializeObject<AppConfiguration>(json);
        }
        catch
        {
            return null;
        }
    }

    private void SaveToRegistry(AppConfiguration config)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            var json = JsonConvert.SerializeObject(config);
            key.SetValue("Configuration", json);
            
            // Save autostart setting
            if (config.General.StartWithWindows)
            {
                SetAutoStart(true);
            }
            else
            {
                SetAutoStart(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to save to registry: {ex.Message}");
        }
    }

    private void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            
            const string appName = "MagicTrackpadClone";
            
            if (enable)
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(appName, $"\"{exePath}\" /background");
                }
            }
            else
            {
                key.DeleteValue(appName, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to set autostart: {ex.Message}");
        }
    }
}
