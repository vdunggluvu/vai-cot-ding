using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Domain.Models;
using DesktopScaffold.Core.Domain.Validation;

namespace DesktopScaffold.Core.Application.UseCases.ImportCsv;

public sealed class ImportCsvUseCase
{
    private readonly IFileSystem _fs;
    private readonly IAppLogger _log;

    public ImportCsvUseCase(IFileSystem fs, IAppLogger log)
    {
        _fs = fs;
        _log = log;
    }

    public async Task<(ValidationResult Validation, ImportCsvResult? Result)> ExecuteAsync(
        ImportCsvRequest request,
        CancellationToken ct)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
            return (validation, null);

        try
        {
            _log.Info($"Import CSV started: {request.FilePath}");
            var content = await _fs.ReadAllTextAsync(request.FilePath, ct).ConfigureAwait(false);
            var rows = ParseCsv(content, request.Delimiter);
            var dataSet = new DataSet(rows);
            _log.Info($"Import CSV completed: {dataSet.Count} rows");
            return (ValidationResult.Ok(), new ImportCsvResult(dataSet, request.FilePath));
        }
        catch (Exception ex)
        {
            _log.Error("Import CSV failed", ex);
            return (ValidationResult.FromError("IMPORT_FAILED", ex.Message), null);
        }
    }

    public static ValidationResult Validate(ImportCsvRequest request)
    {
        var r = new ValidationResult();
        if (string.IsNullOrWhiteSpace(request.FilePath))
            r.Add("FILE_PATH_EMPTY", "File path is required.");
        if (request.Delimiter == '\0')
            r.Add("DELIMITER_INVALID", "Delimiter is invalid.");
        return r;
    }

    private static List<DataRow> ParseCsv(string content, char delimiter)
    {
        // Expected header: Id,Name,Value
        var rows = new List<DataRow>();
        using var sr = new StringReader(content);

        var header = sr.ReadLine();
        if (header is null)
            return rows;

        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(delimiter);
            if (parts.Length < 3)
                continue;

            if (!int.TryParse(parts[0].Trim(), out var id))
                continue;
            var name = parts[1].Trim();
            if (!double.TryParse(parts[2].Trim(), out var value))
                continue;

            rows.Add(new DataRow(id, name, value));
        }

        return rows;
    }
}

