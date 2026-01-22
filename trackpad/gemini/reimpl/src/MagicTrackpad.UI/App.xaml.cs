using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using MagicTrackpad.Core.Interfaces;
using MagicTrackpad.Core.Services;

namespace MagicTrackpad.UI
{
    public partial class App : Application
    {
        private Mutex? _mutex;
        public static IConfigProvider Config { get; private set; } = null!;
        public static IGestureEngine GestureEngine { get; private set; } = null!;
        public static IActionExecutor ActionExecutor { get; private set; } = null!;
        public static IDeviceExposer DeviceExposer { get; private set; } = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            const string appName = "MagicTrackpadUtilities_Clone";
            bool createdNew;
            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Application is already running.", "Magic Trackpad Utilities", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            // Initialize Services
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configPath = Path.Combine(appData, "MagicTrackpadUtilities", "config.json");
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

            Config = new FileConfigProvider(configPath);
            GestureEngine = new GestureStateMachine();
            // In real app, we would use RealActionExecutor and RealDeviceExposer
            // For this clone/demo, we use Mocks or Partial Implementations as per plan
            ActionExecutor = new MockActionExecutor();
            DeviceExposer = new MockDeviceExposer();

            // Wire up events
            DeviceExposer.StartMonitoring();
            // Start input loop (mocked for now)
            
            // Show Main Window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DeviceExposer?.StopMonitoring();
            Config?.Save();
            _mutex?.ReleaseMutex();
        }
    }
}
