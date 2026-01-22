using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone;

public class TrayIconManager : IDisposable
{
    private readonly ILogger _logger;
    private readonly AppHost _appHost;
    private TaskbarIcon? _trayIcon;

    public TrayIconManager(ILogger logger, AppHost appHost)
    {
        _logger = logger;
        _appHost = appHost;
    }

    public void Initialize()
    {
        try
        {
            _trayIcon = new TaskbarIcon
            {
                IconSource = CreateIcon(),
                ToolTipText = "Magic Trackpad Clone"
            };
            
            var contextMenu = new ContextMenu();
            
            var enableItem = new MenuItem { Header = "Enable Gestures", IsCheckable = true, IsChecked = true };
            enableItem.Click += (s, e) => _appHost.ToggleGestures();
            contextMenu.Items.Add(enableItem);
            
            contextMenu.Items.Add(new Separator());
            
            var settingsItem = new MenuItem { Header = "Settings..." };
            settingsItem.Click += (s, e) => _appHost.ShowSettingsWindow();
            contextMenu.Items.Add(settingsItem);
            
            var aboutItem = new MenuItem { Header = "About" };
            aboutItem.Click += (s, e) => ShowAbout();
            contextMenu.Items.Add(aboutItem);
            
            contextMenu.Items.Add(new Separator());
            
            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (s, e) => Application.Current.Shutdown();
            contextMenu.Items.Add(exitItem);
            
            _trayIcon.ContextMenu = contextMenu;
            
            _logger.LogInfo("Tray icon initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to initialize tray icon", ex);
        }
    }

    public void UpdateGestureStatus(bool enabled)
    {
        if (_trayIcon?.ContextMenu?.Items[0] is MenuItem item)
        {
            item.IsChecked = enabled;
        }
    }

    private System.Windows.Media.ImageSource CreateIcon()
    {
        // Create a simple colored circle as icon
        var drawingGroup = new System.Windows.Media.DrawingGroup();
        var geometryDrawing = new System.Windows.Media.GeometryDrawing
        {
            Geometry = new System.Windows.Media.EllipseGeometry(new Point(8, 8), 6, 6),
            Brush = System.Windows.Media.Brushes.DodgerBlue
        };
        drawingGroup.Children.Add(geometryDrawing);
        
        var drawingImage = new System.Windows.Media.DrawingImage(drawingGroup);
        drawingImage.Freeze();
        
        return drawingImage;
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Magic Trackpad Clone\n" +
            "Version 3.1.5.6\n\n" +
            "A clean-room implementation based on flow analysis.\n" +
            "Copyright © 2026",
            "About Magic Trackpad Clone",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
