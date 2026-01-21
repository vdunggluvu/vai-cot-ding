namespace DesktopScaffold.Core.Application.UseCases.ExportCsv;

public sealed record ExportCsvResult(string OutputPath, int RowsWritten);

