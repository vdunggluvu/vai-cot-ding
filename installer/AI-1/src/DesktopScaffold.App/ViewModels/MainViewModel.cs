using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Application.UseCases.ExportCsv;
using DesktopScaffold.Core.Application.UseCases.ImportCsv;
using DesktopScaffold.Core.Application.UseCases.Transform;
using DesktopScaffold.Core.Domain.Models;
using Microsoft.Win32;

namespace DesktopScaffold.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IAppLogger _log;
    private readonly IConfigStore _configStore;
    private readonly IStateStore _stateStore;
    private readonly ImportCsvUseCase _importUc;
    private readonly TransformUseCase _transformUc;
    private readonly ExportCsvUseCase _exportUc;

    private AppConfig _config = new() { App = new AppConfig.AppSection() };
    private AppState _state = new();

    private DataSet? _currentData;
    private TransformSummary? _summary;

    private string _title = "Desktop Scaffold Sample";
    private string _statusText = "Ready";
    private string _errorText = "";
    private string _messageLog = "";

    private string _inputPath = "";
    private string _outputPath = "";
    private string _delimiterText = ",";
    private string _multiplyFactorText = "1";
    private bool _filterNonPositive = true;
    private bool _isBusy;

    public MainViewModel(
        IAppLogger log,
        IConfigStore configStore,
        IStateStore stateStore,
        ImportCsvUseCase importUc,
        TransformUseCase transformUc,
        ExportCsvUseCase exportUc)
    {
        _log = log;
        _configStore = configStore;
        _stateStore = stateStore;
        _importUc = importUc;
        _transformUc = transformUc;
        _exportUc = exportUc;

        BrowseInputCommand = new RelayCommand(BrowseInput, () => CanInteract);
        BrowseOutputCommand = new RelayCommand(BrowseOutput, () => CanInteract);

        ImportCommand = new AsyncRelayCommand(ImportAsync, () => CanInteract);
        TransformCommand = new AsyncRelayCommand(TransformAsync, () => CanInteract);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => CanInteract);

        CurrentRows = new ObservableCollection<DataRow>();
        RecentFiles = new ObservableCollection<string>();

        _ = InitializeAsync();
    }

    public string Title { get => _title; private set => SetProperty(ref _title, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string ErrorText { get => _errorText; private set => SetProperty(ref _errorText, value); }
    public string MessageLog { get => _messageLog; private set => SetProperty(ref _messageLog, value); }

    public string InputPath { get => _inputPath; set => SetProperty(ref _inputPath, value); }
    public string OutputPath { get => _outputPath; set => SetProperty(ref _outputPath, value); }
    public string DelimiterText { get => _delimiterText; set => SetProperty(ref _delimiterText, value); }
    public string MultiplyFactorText { get => _multiplyFactorText; set => SetProperty(ref _multiplyFactorText, value); }
    public bool FilterNonPositive { get => _filterNonPositive; set => SetProperty(ref _filterNonPositive, value); }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaisePropertyChanged(nameof(CanInteract));
                RaiseCommandCanExecutes();
            }
        }
    }

    public bool CanInteract => !IsBusy;

    public ObservableCollection<DataRow> CurrentRows { get; }
    public ObservableCollection<string> RecentFiles { get; }

    public RelayCommand BrowseInputCommand { get; }
    public RelayCommand BrowseOutputCommand { get; }
    public AsyncRelayCommand ImportCommand { get; }
    public AsyncRelayCommand TransformCommand { get; }
    public AsyncRelayCommand ExportCommand { get; }

    public string SummaryText
    {
        get
        {
            if (_summary is null)
                return "No summary yet.";
            return
                $"InputCount: {_summary.InputCount}\n" +
                $"OutputCount: {_summary.OutputCount}\n" +
                $"Sum: {_summary.Sum}\n" +
                $"Average: {_summary.Average}\n" +
                $"Min: {_summary.Min}\n" +
                $"Max: {_summary.Max}";
        }
    }

    public async Task OnClosingAsync()
    {
        try
        {
            _state.LastOpenedAt = DateTimeOffset.UtcNow;
            await _stateStore.SaveAsync(_state, CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // best-effort
        }
    }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            AppendMessage("Startup: loading config/state...");
            _config = await _configStore.LoadConfigAsync(CancellationToken.None).ConfigureAwait(false);
            _state = await _stateStore.LoadAsync(CancellationToken.None).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Title = _config.App.Title;
                DelimiterText = _config.App.DefaultCsvDelimiter;
                RecentFiles.Clear();
                foreach (var f in _state.RecentFiles)
                    RecentFiles.Add(f);
            });

            _log.Info("App initialized.");
            StatusText = "Ready";
        }
        catch (Exception ex)
        {
            _log.Error("Initialization failed", ex);
            ErrorText = ex.Message;
            StatusText = "Init failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BrowseInput()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select input CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
            InputPath = dlg.FileName;
    }

    private void BrowseOutput()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Select output CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            FileName = "export.csv"
        };

        if (dlg.ShowDialog() == true)
            OutputPath = dlg.FileName;
    }

    private async Task ImportAsync()
    {
        ClearError();
        IsBusy = true;
        try
        {
            StatusText = "Importing...";
            var delimiter = ParseDelimiter(DelimiterText);
            var (validation, result) = await _importUc.ExecuteAsync(
                new ImportCsvRequest(InputPath, delimiter),
                CancellationToken.None).ConfigureAwait(false);

            if (!validation.IsValid || result is null)
            {
                ShowValidation(validation);
                return;
            }

            _currentData = result.Data;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CurrentRows.Clear();
                foreach (var r in _currentData.Rows)
                    CurrentRows.Add(r);
                _summary = null;
                RaisePropertyChanged(nameof(SummaryText));
            });

            TrackRecentFile(result.SourcePath);
            AppendMessage($"Imported {_currentData.Count} rows.");
            StatusText = "Imported";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TransformAsync()
    {
        ClearError();
        if (_currentData is null)
        {
            ErrorText = "No data. Please import first.";
            return;
        }

        IsBusy = true;
        try
        {
            StatusText = "Transforming...";
            var factor = ParseDouble(MultiplyFactorText);
            var (validation, result) = await _transformUc.ExecuteAsync(
                new TransformRequest(_currentData, factor, FilterNonPositive),
                CancellationToken.None).ConfigureAwait(false);

            if (!validation.IsValid || result is null)
            {
                ShowValidation(validation);
                return;
            }

            _currentData = result.Output;
            _summary = result.Summary;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CurrentRows.Clear();
                foreach (var r in _currentData.Rows)
                    CurrentRows.Add(r);
                RaisePropertyChanged(nameof(SummaryText));
            });

            AppendMessage($"Transform completed: {_summary.InputCount} -> {_summary.OutputCount}.");
            StatusText = "Transformed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportAsync()
    {
        ClearError();
        if (_currentData is null)
        {
            ErrorText = "No data. Please import/transform first.";
            return;
        }

        IsBusy = true;
        try
        {
            StatusText = "Exporting...";
            var delimiter = ParseDelimiter(DelimiterText);
            var (validation, result) = await _exportUc.ExecuteAsync(
                new ExportCsvRequest(_currentData, OutputPath, delimiter),
                CancellationToken.None).ConfigureAwait(false);

            if (!validation.IsValid || result is null)
            {
                ShowValidation(validation);
                return;
            }

            AppendMessage($"Exported {result.RowsWritten} rows to {result.OutputPath}");
            StatusText = "Exported";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void TrackRecentFile(string path)
    {
        var max = Math.Max(1, _config.App.RecentFilesMax);
        _state.RecentFiles.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        _state.RecentFiles.Insert(0, path);
        if (_state.RecentFiles.Count > max)
            _state.RecentFiles.RemoveRange(max, _state.RecentFiles.Count - max);

        Application.Current.Dispatcher.Invoke(() =>
        {
            RecentFiles.Clear();
            foreach (var f in _state.RecentFiles)
                RecentFiles.Add(f);
        });
    }

    private void ShowValidation(DesktopScaffold.Core.Domain.Validation.ValidationResult validation)
    {
        ErrorText = validation.ToString();
        AppendMessage($"Validation failed:\n{validation}");
        StatusText = "Validation error";
    }

    private void ClearError()
    {
        ErrorText = "";
    }

    private void AppendMessage(string msg)
    {
        _log.Info(msg);
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageLog = string.IsNullOrWhiteSpace(MessageLog)
                ? msg
                : MessageLog + Environment.NewLine + msg;
        });
    }

    private static char ParseDelimiter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ',';
        return text.Trim()[0];
    }

    private static double ParseDouble(string text)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return d;
        if (double.TryParse(text, out d))
            return d;
        return 1.0;
    }

    private void RaiseCommandCanExecutes()
    {
        BrowseInputCommand.RaiseCanExecuteChanged();
        BrowseOutputCommand.RaiseCanExecuteChanged();
        ImportCommand.RaiseCanExecuteChanged();
        TransformCommand.RaiseCanExecuteChanged();
        ExportCommand.RaiseCanExecuteChanged();
    }
}

