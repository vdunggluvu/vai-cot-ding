namespace DataFlowApp.Domain.Interfaces;

/// <summary>
/// Interface for configuration management.
/// </summary>
public interface IConfigurationManager
{
    T GetValue<T>(string key, T defaultValue = default!);
    void SetValue<T>(string key, T value);
    void LoadConfiguration();
    void SaveConfiguration();
    string GetConfigFilePath();
}
