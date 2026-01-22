using System;
using System.Windows.Forms;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Tray;
using MagicKeyboardUtilities.App.Messaging;
using MagicKeyboardUtilities.App.Input;
using MagicKeyboardUtilities.App.Device;

namespace MagicKeyboardUtilities.App
{
    public class AppHost
    {
        private readonly ConfigStore _configStore;
        private TrayIconController _trayController;
        private HiddenMessageWindow _messageWindow;
        private KeyboardHookService _hookService;
        private HotkeyService _hotkeyService;
        private DeviceMonitor _deviceMonitor;

        public AppHost()
        {
            _configStore = new ConfigStore();
            
            // Components
            _trayController = new TrayIconController(this);
            _messageWindow = new HiddenMessageWindow(this);
            _hookService = new KeyboardHookService(_configStore.Current);
            _hotkeyService = new HotkeyService(_messageWindow.Handle, _configStore.Current);
            _deviceMonitor = new DeviceMonitor(_messageWindow.Handle, _configStore.Current);
        }

        public void Run()
        {
            // 4.1 STARTUP FLOW
            
            // Init Tray
            if (_configStore.Current.Features.TrayIcon)
            {
                _trayController.Initialize();
            }

            // Init Hooks
            if (_configStore.Current.Features.EnableHooks)
            {
                _hookService.Install();
            }

            // Init Hotkeys
            if (_configStore.Current.Features.EnableHotkeys)
            {
                _hotkeyService.RegisterAll();
            }

            // Init Device Monitor
            if (_configStore.Current.Features.EnableDeviceMonitor)
            {
                _deviceMonitor.StartMonitoring();
            }

            // Enter Message Loop
            Application.Run();
        }

        public void Stop()
        {
            // 4.6 SHUTDOWN FLOW
            _hookService.Uninstall();
            _hotkeyService.UnregisterAll();
            _deviceMonitor.StopMonitoring();
            _trayController.Dispose();
            
            if (_configStore.Current.App.AutoSave)
            {
                _configStore.Save();
            }
            Application.Exit();
        }

        public ConfigStore Config => _configStore;
        public KeyboardHookService HookService => _hookService;
        
        // Command dispatches
        public void ToggleHooks()
        {
            if (_hookService.IsActive)
                _hookService.Uninstall();
            else
                _hookService.Install();
            
            _configStore.Current.Features.EnableHooks = _hookService.IsActive;
            _trayController.UpdateStatus();
        }
        
        public void HandleDeviceChange(int eventType, IntPtr data)
        {
            _deviceMonitor.ProcessMessage(eventType, data);
        }

        public void HandleHotkey(int id)
        {
            // Dispatch hotkey action
            Console.WriteLine($"Hotkey detected: {id}");
        }
    }
}
