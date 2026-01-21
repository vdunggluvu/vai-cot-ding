using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.UseCases.ExportCsv;

public sealed record ExportCsvRequest(DataSet Data, string OutputPath, char Delimiter);

