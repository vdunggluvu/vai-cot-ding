using MagicMouseClone.Core.Models;

namespace MagicMouseClone.Core.Interfaces;

/// <summary>
/// Interface cho configuration manager
/// Evidence: High - Từ section 7.6
/// </summary>
public interface IConfigurationManager
{
    Task<AppConfig> LoadConfigurationAsync();
    Task SaveConfigurationAsync(AppConfig config);
    Task<ActionProfile> LoadProfileAsync(string profileName);
    Task SaveProfileAsync(ActionProfile profile);
    Task<IReadOnlyList<string>> GetProfileNamesAsync();
}
