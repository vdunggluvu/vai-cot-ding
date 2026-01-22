using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MagicKeyboardUtilities.App.Config;

/// <summary>
/// Configuration store - loads/saves config.json
/// Traceability: Section 4.4 CONFIGURATION FLOW
/// Evidence: "Configuration embedded trong binary (encrypted) - giả thuyết mạnh"
/// Implementation: Using external JSON file for transparency and ease of modification
/// </summary>
public class ConfigStore
{
    private readonly ILogger<ConfigStore> _logger;
    private readonly string _configPath;
    private AppConfig _currentConfig;
    private bool _isDirty;

    public ConfigStore(ILogger<ConfigStore> logger, string? configPath = null)
    {
        _logger = logger;
        _configPath = configPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        _currentConfig = new AppConfig();
        _isDirty = false;
    }

    public AppConfig Current => _currentConfig;

    /// <summary>
    /// Load configuration from disk
    /// Traceability: Section 4.4 Step 2 "Configuration Loading"
    /// </summary>
    public bool Load()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger.LogWarning("Config file not found at {Path}, using defaults", _configPath);
                _currentConfig = CreateDefaultConfig();
                return false;
            }

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (config != null)
            {
                _currentConfig = config;
                _logger.LogInformation("Configuration loaded from {Path}", _configPath);
                _isDirty = false;
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to deserialize config, using defaults");
                _currentConfig = CreateDefaultConfig();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration from {Path}", _configPath);
            _currentConfig = CreateDefaultConfig();
            return false;
        }
    }

    /// <summary>
    /// Save configuration to disk
    /// Traceability: Section 4.4 Step 5 "Configuration Saving"
    /// Evidence: "Trigger: User saves settings hoặc app exit"
    /// </summary>
    public bool Save(bool force = false)
    {
        if (!_isDirty && !force)
        {
            _logger.LogDebug("Config not dirty, skipping save");
            return true;
        }

        try
        {
            var json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(_configPath, json);
            _logger.LogInformation("Configuration saved to {Path}", _configPath);
            _isDirty = false;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to {Path}", _configPath);
            return false;
        }
    }

    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    /// Create default configuration
    /// Traceability: Section 4.4 Step 2, Section 6 CONFIG.JSON schema
    /// </summary>
    private AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            App = new AppSettings
            {
                StartMinimized = true,
                AutoSave = true,
                LogLevel = "Information"
            },
            Features = new FeatureSettings
            {
                TrayIcon = true,
                EnableHooks = false,
                EnableHotkeys = false,
                EnableDeviceMonitor = false,
                EnableUpdater = false,
                EnableSendInput = false
            },
            Remapping = new List<KeyRemapping>
            {
                new() { FromVk = 124, ToVk = 175, Description = "F13 -> Volume Up (example)" },
                new() { FromVk = 125, ToVk = 174, Description = "F14 -> Volume Down (example)" }
            },
            Hotkeys = new List<HotkeyDefinition>
            {
                new()
                {
                    Id = 1,
                    Modifiers = new List<string> { "Control", "Alt" },
                    Vk = 75,
                    Action = "ToggleEnabled",
                    Description = "Ctrl+Alt+K -> Toggle"
                }
            },
            Device = new DeviceSettings
            {
                AppleVendorId = "0x05AC",
                NotifyOnConnect = true,
                NotifyOnDisconnect = true
            }
        };
    }
}
