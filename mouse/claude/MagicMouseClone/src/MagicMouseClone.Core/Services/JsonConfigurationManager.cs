using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MagicMouseClone.Core.Services;

/// <summary>
/// JSON-based configuration manager
/// Evidence: High - Based on section 7.6 (using JSON instead of Registry for portability)
/// </summary>
public class JsonConfigurationManager : IConfigurationManager
{
    private readonly ILogger<JsonConfigurationManager> _logger;
    private readonly string _configDirectory;
    private readonly string _configFilePath;
    private readonly string _profilesDirectory;

    public JsonConfigurationManager(ILogger<JsonConfigurationManager> logger)
    {
        _logger = logger;

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appDataPath, "MagicMouseClone");
        _configFilePath = Path.Combine(_configDirectory, "config.json");
        _profilesDirectory = Path.Combine(_configDirectory, "Profiles");

        EnsureDirectoriesExist();
    }

    public async Task<AppConfig> LoadConfigurationAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogInformation("Config file not found, creating default configuration");
                var defaultConfig = new AppConfig();
                await SaveConfigurationAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            _logger.LogInformation("Configuration loaded successfully");
            return config ?? new AppConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration, using defaults");
            return new AppConfig();
        }
    }

    public async Task SaveConfigurationAsync(AppConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configFilePath, json);
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            throw;
        }
    }

    public async Task<ActionProfile> LoadProfileAsync(string profileName)
    {
        try
        {
            var profilePath = Path.Combine(_profilesDirectory, $"{profileName}.json");

            if (!File.Exists(profilePath))
            {
                _logger.LogWarning("Profile not found: {ProfileName}, creating default", profileName);
                var defaultProfile = CreateDefaultProfile(profileName);
                await SaveProfileAsync(defaultProfile);
                return defaultProfile;
            }

            var json = await File.ReadAllTextAsync(profilePath);
            var profile = JsonSerializer.Deserialize<ActionProfile>(json);
            _logger.LogInformation("Profile loaded: {ProfileName}", profileName);
            return profile ?? CreateDefaultProfile(profileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile: {ProfileName}", profileName);
            return CreateDefaultProfile(profileName);
        }
    }

    public async Task SaveProfileAsync(ActionProfile profile)
    {
        try
        {
            var profilePath = Path.Combine(_profilesDirectory, $"{profile.Name}.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(profile, options);
            await File.WriteAllTextAsync(profilePath, json);
            _logger.LogInformation("Profile saved: {ProfileName}", profile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile: {ProfileName}", profile.Name);
            throw;
        }
    }

    public Task<IReadOnlyList<string>> GetProfileNamesAsync()
    {
        try
        {
            var files = Directory.GetFiles(_profilesDirectory, "*.json");
            var names = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
            return Task.FromResult<IReadOnlyList<string>>(names);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile names");
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_configDirectory);
        Directory.CreateDirectory(_profilesDirectory);
    }

    private static ActionProfile CreateDefaultProfile(string name)
    {
        var profile = new ActionProfile { Name = name };

        profile.SetBinding(GestureType.ScrollUp,
            new ActionBinding(GestureType.ScrollUp, ActionType.MouseEvent, "WheelUp"));
        profile.SetBinding(GestureType.ScrollDown,
            new ActionBinding(GestureType.ScrollDown, ActionType.MouseEvent, "WheelDown"));
        profile.SetBinding(GestureType.SwipeLeft,
            new ActionBinding(GestureType.SwipeLeft, ActionType.KeyboardShortcut, "Browser_Back"));
        profile.SetBinding(GestureType.SwipeRight,
            new ActionBinding(GestureType.SwipeRight, ActionType.KeyboardShortcut, "Browser_Forward"));

        return profile;
    }
}
