using System;
using System.IO;
using System.Text.Json;

namespace MagicKeyboardUtilities.App.Config
{
    public class ConfigStore
    {
        private const string ConfigFileName = "config.json";
        private string _configPath;
        private AppConfig _currentConfig = new AppConfig();

        public AppConfig Current => _currentConfig;

        public ConfigStore()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            Load();
        }

        public void Load()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    var loaded = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _currentConfig = loaded ?? new AppConfig();
                }
                catch
                {
                    // Fallback to default if corrupt
                    _currentConfig = new AppConfig();
                }
            }
            else
            {
                _currentConfig = new AppConfig();
                Save(); // Create default file
            }
        }

        public void Save()
        {
            if (_currentConfig == null) return;
            try
            {
                string json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
