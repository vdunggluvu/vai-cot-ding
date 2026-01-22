using System.Windows;
using MagicMouseClone.Core;

namespace MagicMouseClone.App
{
    public partial class App : Application
    {
        private TrayIconManager? _trayIconManager;
        private MainWindow? _mainWindow;
        private FakeDeviceProvider? _deviceProvider;
        private IGestureEngine? _gestureEngine;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _gestureEngine = new GestureEngine();
            _gestureEngine.GestureDetected += OnGestureDetected;

            _deviceProvider = new FakeDeviceProvider(_gestureEngine);

            _mainWindow = new MainWindow(_deviceProvider);
            _trayIconManager = new TrayIconManager(_mainWindow);
            
            // Note: MainWindow is NOT shown on startup, only Tray Icon.
            // Unless user requests it or first run.
            // _mainWindow.Show(); 
        }

        private void OnGestureDetected(object? sender, GestureEventArgs e)
        {
            _mainWindow?.Log($"Gesture Detected: {e.Type} (Mag: {e.Magnitude:F2})");
            // Here we would call the ActionExecutor
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIconManager?.Dispose();
            _deviceProvider?.Stop();
            base.OnExit(e);
        }
    }
}
