using System.Windows;

namespace DesktopScaffold.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var bootstrapper = new Bootstrapper();
        var mainWindow = bootstrapper.BuildMainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}

