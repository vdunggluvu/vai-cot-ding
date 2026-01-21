using System.Windows;
using DesktopScaffold.App.ViewModels;
using DesktopScaffold.App.Views;
using DesktopScaffold.Core.Application.UseCases.ExportCsv;
using DesktopScaffold.Core.Application.UseCases.ImportCsv;
using DesktopScaffold.Core.Application.UseCases.Transform;
using DesktopScaffold.Infrastructure.Config;
using DesktopScaffold.Infrastructure.FileSystem;
using DesktopScaffold.Infrastructure.Logging;
using DesktopScaffold.Infrastructure.State;

namespace DesktopScaffold.App;

public sealed class Bootstrapper
{
    public Window BuildMainWindow()
    {
        // Minimal DI wiring (no external containers)
        var logger = new FileAppLogger();
        var fs = new PhysicalFileSystem();
        var configStore = new JsonConfigStore(fs, logger);
        var stateStore = new JsonStateStore(fs, logger);

        var importUc = new ImportCsvUseCase(fs, logger);
        var transformUc = new TransformUseCase(logger);
        var exportUc = new ExportCsvUseCase(fs, logger);

        var vm = new MainViewModel(logger, configStore, stateStore, importUc, transformUc, exportUc);
        var w = new MainWindow { DataContext = vm };

        w.Closing += async (_, __) => await vm.OnClosingAsync();

        return w;
    }
}

