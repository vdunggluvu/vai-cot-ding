using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicKeyboardUtilities.App.Tray
{
    public class TrayIconController : IDisposable
    {
        private readonly AppHost _appHost;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _enableItem;

        public TrayIconController(AppHost appHost)
        {
            _appHost = appHost;
        }

        public void Initialize()
        {
            _contextMenu = new ContextMenuStrip();
            
            _enableItem = new ToolStripMenuItem("Enable Hooks", null, (s, e) => _appHost.ToggleHooks());
            _enableItem.Checked = _appHost.Config.Current.Features.EnableHooks;
            
            var settingsItem = new ToolStripMenuItem("Settings...", null, (s, e) => ShowSettingsStub());
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => _appHost.Stop());

            _contextMenu.Items.Add(_enableItem);
            _contextMenu.Items.Add(settingsItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(exitItem);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Application; // Placeholder, real app would load from .ico
            _notifyIcon.Text = "Magic Keyboard Utilities (Reimpl)";
            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.Visible = true;
        }

        public void UpdateStatus()
        {
            if (_enableItem != null)
            {
                _enableItem.Checked = _appHost.Config.Current.Features.EnableHooks;
                _notifyIcon.Text = _appHost.Config.Current.Features.EnableHooks 
                    ? "Magic Keyboard Utilities: Enabled" 
                    : "Magic Keyboard Utilities: Disabled";
            }
        }

        private void ShowSettingsStub()
        {
            MessageBox.Show($"Configuration loaded from JSON.\nEdit config.json to change settings.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
