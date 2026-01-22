using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App;
using MagicKeyboardUtilities.App.Config;
using MagicKeyboardUtilities.App.Core;
using MagicKeyboardUtilities.App.Device;
using MagicKeyboardUtilities.App.Diagnostics;
using MagicKeyboardUtilities.App.Input;
using MagicKeyboardUtilities.App.Tray;

/// <summary>
/// Entry point
/// Traceability: Section 4.1 STARTUP FLOW, Section 8.1 Phase 1 "Basic Structure"
/// </summary>
internal static class Program
{
    private static Mutex? _singleInstanceMutex;
    private static ILoggerFactory? _loggerFactory;
    private static AppHost? _appHost;

    /// <summary>
    /// The main entry point for the application.
    /// Traceability: Section 4.1 Step 1-4
    /// </summary>
    [System.STAThread]
    private static void Main()
    {
        // Step 1: Single instance check
        // Traceability: Section 4.1 Step 4
        if (!CheckSingleInstance())
        {
            MessageBox.Show(
                "Magic Keyboard Utilities is already running.",
                "Already Running",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            // Step 2: Initialize logging
            InitializeLogging();
            var logger = _loggerFactory!.CreateLogger("Program");
            logger.LogInformation("=== Magic Keyboard Utilities Reimplementation ===");
            logger.LogInformation("Version: {Version}", Assembly.GetExecutingAssembly().GetName().Version);
            logger.LogInformation("Traceability: Flow Report Analysis 22/01/2026");

            // Step 3: Load configuration
            // Traceability: Section 4.1 Step 5, Section 4.4 Configuration Flow
            logger.LogInformation("Loading configuration...");
            var configStore = new ConfigStore(_loggerFactory.CreateLogger<ConfigStore>());
            configStore.Load();

            // Step 4: Initialize application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Step 5: Create services
            logger.LogInformation("Initializing services...");
            var remappingEngine = new RemappingEngine(_loggerFactory.CreateLogger<RemappingEngine>());
            var actionDispatcher = new ActionDispatcher(_loggerFactory.CreateLogger<ActionDispatcher>());
            var keyboardHookService = new KeyboardHookService(
                _loggerFactory.CreateLogger<KeyboardHookService>(),
                remappingEngine);
            var hotkeyService = new HotkeyService(
                _loggerFactory.CreateLogger<HotkeyService>(),
                actionDispatcher);
            var deviceMonitor = new DeviceMonitor(_loggerFactory.CreateLogger<DeviceMonitor>());
            var trayIconController = new TrayIconController(
                _loggerFactory.CreateLogger<TrayIconController>(),
                actionDispatcher);

            // Step 6: Create and start application host
            _appHost = new AppHost(
                _loggerFactory.CreateLogger<AppHost>(),
                _loggerFactory,
                configStore,
                remappingEngine,
                actionDispatcher,
                keyboardHookService,
                hotkeyService,
                deviceMonitor,
                trayIconController);

            if (!_appHost.Start())
            {
                logger.LogError("Failed to start application");
                MessageBox.Show(
                    "Failed to start application. Check logs for details.",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Step 7: Run message loop
            // Traceability: Section 3 "Main Operation Phase"
            logger.LogInformation("Entering message loop...");
            Application.Run();

            logger.LogInformation("Message loop exited");
        }
        catch (Exception ex)
        {
            var logger = _loggerFactory?.CreateLogger("Program");
            logger?.LogError(ex, "Unhandled exception in main");
            
            MessageBox.Show(
                $"Fatal error: {ex.Message}\n\nSee logs for details.",
                "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            // Cleanup
            _appHost?.Dispose();
            _loggerFactory?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
    }

    /// <summary>
    /// Check single instance using Mutex
    /// Traceability: Section 4.1 Step 4 "Single Instance Check"
    /// Evidence: "Check if another instance is running"
    /// </summary>
    private static bool CheckSingleInstance()
    {
        const string mutexName = "Global\\MagicKeyboardUtilities.Reimpl";

        try
        {
            _singleInstanceMutex = new Mutex(true, mutexName, out var createdNew);
            return createdNew;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Initialize logging system
    /// </summary>
    private static void InitializeLogging()
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
        
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole()
                .AddFileLogger(logPath);
        });
    }
}
