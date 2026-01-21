namespace DesktopScaffold.Core.Domain.Models;

public sealed class AppConfig
{
    public required AppSection App { get; init; }

    public sealed class AppSection
    {
        public string Title { get; init; } = "Desktop Scaffold Sample";
        public string DefaultCsvDelimiter { get; init; } = ",";
        public int RecentFilesMax { get; init; } = 10;
    }
}

