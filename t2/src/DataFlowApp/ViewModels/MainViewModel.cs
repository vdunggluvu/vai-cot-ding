using DataFlowApp.Application.UseCases;
using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DataFlowApp.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// Implements MVVM pattern with Commands for user interactions.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ImportDataUseCase _importUseCase;
    private readonly ProcessDataUseCase _processUseCase;
    private readonly ExportDataUseCase _exportUseCase;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger _logger;

    private string _statusMessage = "Ready";
    private bool _isBusy;
    private ObservableCollection<DataRecord> _records = new();
    private string _recordCount = "0 records";

    public MainViewModel(
        ImportDataUseCase importUseCase,
        ProcessDataUseCase processUseCase,
        ExportDataUseCase exportUseCase,
        IConfigurationManager configManager,
        ILogger logger)
    {
        _importUseCase = importUseCase;
        _processUseCase = processUseCase;
        _exportUseCase = exportUseCase;
        _configManager = configManager;
        _logger = logger;

        // Initialize commands
        ImportCommand = new RelayCommand(async _ => await ImportDataAsync(), _ => !IsBusy);
        ProcessCommand = new RelayCommand(async _ => await ProcessDataAsync(), _ => !IsBusy && Records.Count > 0);
        ExportCommand = new RelayCommand(async _ => await ExportDataAsync(), _ => !IsBusy && Records.Count > 0);
        ClearCommand = new RelayCommand(_ => ClearData(), _ => !IsBusy && Records.Count > 0);
    }

    #region Properties

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                // Notify command can-execute changed
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ObservableCollection<DataRecord> Records
    {
        get => _records;
        set
        {
            if (SetProperty(ref _records, value))
            {
                UpdateRecordCount();
            }
        }
    }

    public string RecordCount
    {
        get => _recordCount;
        set => SetProperty(ref _recordCount, value);
    }

    #endregion

    #region Commands

    public ICommand ImportCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ClearCommand { get; }

    #endregion

    #region Command Implementations

    private async Task ImportDataAsync()
    {
        _logger.LogInfo("User initiated import");

        var dialog = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Select CSV File to Import"
        };

        if (dialog.ShowDialog() != true)
            return;

        IsBusy = true;
        StatusMessage = "Importing data...";

        try
        {
            var result = await _importUseCase.ExecuteAsync(dialog.FileName, validateOnImport: true);

            if (result.Success)
            {
                // Reload records from data service
                var dataService = _importUseCase.GetType()
                    .GetField("_dataService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(_importUseCase) as Domain.Interfaces.IDataService;

                if (dataService != null)
                {
                    var records = await dataService.GetAllRecordsAsync();
                    Records = new ObservableCollection<DataRecord>(records);
                }

                StatusMessage = result.Message;

                if (result.Warnings.Count > 0)
                {
                    MessageBox.Show(
                        $"{result.Message}\n\nWarnings:\n{string.Join("\n", result.Warnings.Take(5))}",
                        "Import Completed with Warnings",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(result.Message, "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                StatusMessage = "Import failed";
                MessageBox.Show(result.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Import error", ex);
            StatusMessage = "Import failed";
            MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ProcessDataAsync()
    {
        _logger.LogInfo("User initiated processing");

        IsBusy = true;
        StatusMessage = "Processing data...";

        try
        {
            var result = await _processUseCase.ExecuteAsync();

            if (result.Success)
            {
                // Refresh the UI with updated records
                OnPropertyChanged(nameof(Records));

                StatusMessage = result.Message;

                string message = result.Message;
                if (result.Warnings.Count > 0)
                {
                    message += $"\n\nWarnings: {result.Warnings.Count}";
                }
                if (result.Errors.Count > 0)
                {
                    message += $"\n\nErrors: {result.Errors.Count}";
                }

                MessageBox.Show(message, "Processing Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Processing failed";
                MessageBox.Show(result.Message, "Processing Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Processing error", ex);
            StatusMessage = "Processing failed";
            MessageBox.Show($"Processing failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportDataAsync()
    {
        _logger.LogInfo("User initiated export");

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Export Data to CSV",
            FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        IsBusy = true;
        StatusMessage = "Exporting data...";

        try
        {
            var result = await _exportUseCase.ExecuteAsync(dialog.FileName);

            if (result.Success)
            {
                StatusMessage = result.Message;
                MessageBox.Show(result.Message, "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Export failed";
                MessageBox.Show(result.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Export error", ex);
            StatusMessage = "Export failed";
            MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearData()
    {
        var result = MessageBox.Show(
            "Are you sure you want to clear all loaded data?",
            "Confirm Clear",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Records.Clear();
            StatusMessage = "Data cleared";
            _logger.LogInfo("User cleared data");
        }
    }

    #endregion

    private void UpdateRecordCount()
    {
        RecordCount = $"{Records.Count} record{(Records.Count != 1 ? "s" : "")}";
    }

    public void Dispose()
    {
        _logger?.LogInfo("MainViewModel disposing");
    }
}

/// <summary>
/// Simple RelayCommand implementation for MVVM.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
