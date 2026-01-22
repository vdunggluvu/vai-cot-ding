using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MagicMouseClone.Core.Interfaces;
using MagicMouseClone.Core.Services;
using System.Diagnostics;

namespace MagicMouseClone.App;

internal static class Program
{
    private static Mutex? _singleInstanceMutex;
    private const string MutexName = "MagicMouseClone_SingleInstance";

    [STAThread]
    static void Main()
    {
        // Check for single instance
        _singleInstanceMutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "MagicMouseClone is already running.",
                "Already Running",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();

        // Run the application
        try
        {
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fatal error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core services
        services.AddSingleton<IDeviceBackend, FakeDeviceBackend>();
        services.AddSingleton<IGestureEngine, GestureEngine>();
        services.AddSingleton<IActionMapper, ActionMapper>();
        services.AddSingleton<IConfigurationManager, JsonConfigurationManager>();
        services.AddSingleton<AppHost>();

        // UI
        services.AddTransient<MainForm>();
    }
}
