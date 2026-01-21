using DataFlowApp.Infrastructure.Configuration;
using DataFlowApp.Infrastructure.Logging;
using DataFlowApp.Infrastructure.Services;
using DataFlowApp.Application.UseCases;
using DataFlowApp.ViewModels;
using System.Windows;
using DataFlowApp.Domain.Interfaces;

namespace DataFlowApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private FileLogger? _logger;
    private ConfigurationManager? _configManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Initialize logger first
            _logger = new FileLogger();
            _logger.LogInfo("=== Application Starting ===");

            // Load configuration
            _configManager = new ConfigurationManager(_logger);
            _configManager.LoadConfiguration();
            _logger.LogInfo("Configuration loaded successfully");

            // Initialize services (Dependency Injection would be better, but keeping it simple)
            var dataService = new CsvDataService(_logger);
            var validator = new DataValidator(_logger);

            // Initialize use cases
            var importUseCase = new ImportDataUseCase(dataService, validator, _logger);
            var processUseCase = new ProcessDataUseCase(dataService, validator, _logger);
            var exportUseCase = new ExportDataUseCase(dataService, _logger);

            // Create and show main window
            var mainWindow = new MainWindow();
            var viewModel = new MainViewModel(
                importUseCase,
                processUseCase,
                exportUseCase,
                _configManager,
                _logger
            );
            mainWindow.DataContext = viewModel;
            mainWindow.Show();

            _logger.LogInfo("Application started successfully");
        }
        catch (Exception ex)
        {
            var message = $"Application startup failed: {ex.Message}";
            _logger?.LogError(message, ex);
            MessageBox.Show(message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger?.LogInfo("=== Application Shutting Down ===");
            _configManager?.SaveConfiguration();
            _logger?.Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during shutdown: {ex.Message}", "Shutdown Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        base.OnExit(e);
    }
}
