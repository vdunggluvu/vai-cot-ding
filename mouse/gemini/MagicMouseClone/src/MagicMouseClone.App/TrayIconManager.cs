using System;
using System.Drawing;
using System.Windows.Forms;
using MagicMouseClone.Core;
using Application = System.Windows.Application;

namespace MagicMouseClone.App
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;

        public TrayIconManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Initialize();
        }

        private void Initialize()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Placeholder icon
                Visible = true,
                Text = "Magic Mouse Utilities Clone"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, ShowSettings_Click);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, Exit_Click);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += ShowSettings_Click;
        }

        private void ShowSettings_Click(object? sender, EventArgs e)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = System.Windows.WindowState.Normal;
            _mainWindow.Activate();
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}
