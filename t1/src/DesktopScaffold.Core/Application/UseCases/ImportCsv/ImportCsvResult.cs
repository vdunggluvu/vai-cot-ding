using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.UseCases.ImportCsv;

public sealed record ImportCsvResult(DataSet Data, string SourcePath);

