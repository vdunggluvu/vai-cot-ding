using System.Text.Json;
using MagicTrackpad.Core.Interfaces;
using MagicTrackpad.Core.Models;

namespace MagicTrackpad.Core.Services;

public class FileConfigProvider : IConfigProvider
{
    private readonly string _filePath;
    private Dictionary<string, object> _configData = new();

    public FileConfigProvider(string filePath)
    {
        _filePath = filePath;
        if (!File.Exists(_filePath))
        {
             // Default config
             _configData["Gesture_Tap"] = true;
             _configData["Gesture_ScrollVertical"] = true;
             Save();
        }
        else
        {
            Load();
        }
    }

    public void Load()
    {
        try 
        {
            var json = File.ReadAllText(_filePath);
            _configData = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
        }
        catch 
        {
            // Corruption fallback
            _configData = new();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_configData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        if (_configData.TryGetValue(key, out var val))
        {
            if (val is JsonElement je)
            {
                 // Handle simple types
                 if (typeof(T) == typeof(bool)) return (T)(object)je.GetBoolean();
                 if (typeof(T) == typeof(int)) return (T)(object)je.GetInt32();
                 if (typeof(T) == typeof(string)) return (T)(object)je.GetString();
            }
            if (val is T tVal) return tVal;
        }
        return defaultValue;
    }

    public void SetValue<T>(string key, T value)
    {
        _configData[key] = value;
    }

    public bool IsGestureEnabled(GestureType type)
    {
        return GetValue($"Gesture_{type}", false);
    }

    public void SetGestureEnabled(GestureType type, bool enabled)
    {
        SetValue($"Gesture_{type}", enabled);
    }
}
