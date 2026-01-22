using System.Windows;

namespace MagicTrackpadClone;

public partial class App : Application
{
    private AppHost? _appHost;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _appHost = new AppHost();
        if (!_appHost.Initialize())
        {
            Shutdown(1);
            return;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _appHost?.Shutdown();
        base.OnExit(e);
    }
}
