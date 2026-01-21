namespace DesktopScaffold.Core.Application.UseCases.ImportCsv;

public sealed record ImportCsvRequest(string FilePath, char Delimiter);

