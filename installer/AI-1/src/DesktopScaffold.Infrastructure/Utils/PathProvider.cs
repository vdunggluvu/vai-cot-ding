namespace DesktopScaffold.Infrastructure.Utils;

public static class PathProvider
{
    public static string AppBaseDir => AppContext.BaseDirectory;

    public static string LogsDir => Path.Combine(AppBaseDir, "logs");
    public static string StateFile => Path.Combine(AppBaseDir, "state.json");
    public static string ConfigFile => Path.Combine(AppBaseDir, "appsettings.json");
}

