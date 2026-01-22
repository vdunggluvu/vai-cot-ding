using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Interfaces;

public interface IConfigProvider
{
    void Load();
    void Save();
    T GetValue<T>(string key, T defaultValue);
    void SetValue<T>(string key, T value);
    
    // Typed config access
    bool IsGestureEnabled(GestureType type);
    void SetGestureEnabled(GestureType type, bool enabled);
    // ... maps gesture to action
}
