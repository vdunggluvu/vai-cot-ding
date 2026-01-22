using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.App.Input
{
    public class HotkeyService
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _hwnd;
        private AppConfig _config;

        public HotkeyService(IntPtr hwnd, AppConfig config)
        {
            _hwnd = hwnd;
            _config = config;
        }

        public void RegisterAll()
        {
            foreach (var hk in _config.Hotkeys)
            {
                uint mods = 0;
                foreach (var m in hk.Modifiers)
                {
                    if (m.Equals("Alt", StringComparison.OrdinalIgnoreCase)) mods |= 0x0001;
                    if (m.Equals("Control", StringComparison.OrdinalIgnoreCase)) mods |= 0x0002;
                    if (m.Equals("Shift", StringComparison.OrdinalIgnoreCase)) mods |= 0x0004;
                    if (m.Equals("Win", StringComparison.OrdinalIgnoreCase)) mods |= 0x0008;
                }
                
                bool result = RegisterHotKey(_hwnd, hk.Id, mods, (uint)hk.Vk);
                if (!result)
                {
                    Console.WriteLine($"Failed to register hotkey ID {hk.Id}");
                }
            }
        }

        public void UnregisterAll()
        {
            foreach (var hk in _config.Hotkeys)
            {
                UnregisterHotKey(_hwnd, hk.Id);
            }
        }
    }
}
