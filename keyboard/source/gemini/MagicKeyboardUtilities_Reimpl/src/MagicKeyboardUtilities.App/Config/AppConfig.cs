using System.Collections.Generic;

namespace MagicKeyboardUtilities.App.Config
{
    public class AppConfig
    {
        public AppSettings App { get; set; } = new AppSettings();
        public FeatureSettings Features { get; set; } = new FeatureSettings();
        public List<RemapEntry> Remapping { get; set; } = new List<RemapEntry>();
        public List<HotkeyEntry> Hotkeys { get; set; } = new List<HotkeyEntry>();
    }

    public class AppSettings
    {
        public bool StartMinimized { get; set; } = true;
        public bool AutoSave { get; set; } = true;
    }

    public class FeatureSettings
    {
        public bool TrayIcon { get; set; } = true;
        public bool EnableHooks { get; set; } = false;
        public bool EnableHotkeys { get; set; } = false;
        public bool EnableDeviceMonitor { get; set; } = false;
        public bool EnableUpdater { get; set; } = false;
        public bool EnableSendInput { get; set; } = false;
    }

    public class RemapEntry
    {
        public int FromVk { get; set; }
        public int ToVk { get; set; }
        public string Description { get; set; } = "";
    }

    public class HotkeyEntry
    {
        public int Id { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public int Vk { get; set; }
        public string Action { get; set; } = "";
    }
}
