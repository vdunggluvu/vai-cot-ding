using System.Text;
using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Domain.Validation;

namespace DesktopScaffold.Core.Application.UseCases.ExportCsv;

public sealed class ExportCsvUseCase
{
    private readonly IFileSystem _fs;
    private readonly IAppLogger _log;

    public ExportCsvUseCase(IFileSystem fs, IAppLogger log)
    {
        _fs = fs;
        _log = log;
    }

    public async Task<(ValidationResult Validation, ExportCsvResult? Result)> ExecuteAsync(
        ExportCsvRequest request,
        CancellationToken ct)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
            return (validation, null);

        try
        {
            _log.Info($"Export CSV started: {request.OutputPath}");
            var sb = new StringBuilder();
            sb.AppendLine($"Id{request.Delimiter}Name{request.Delimiter}Value");
            foreach (var row in request.Data.Rows)
                sb.AppendLine($"{row.Id}{request.Delimiter}{Escape(row.Name)}{request.Delimiter}{row.Value}");

            await _fs.WriteAllTextAsync(request.OutputPath, sb.ToString(), ct).ConfigureAwait(false);
            _log.Info($"Export CSV completed: {request.Data.Count} rows");
            return (ValidationResult.Ok(), new ExportCsvResult(request.OutputPath, request.Data.Count));
        }
        catch (Exception ex)
        {
            _log.Error("Export CSV failed", ex);
            return (ValidationResult.FromError("EXPORT_FAILED", ex.Message), null);
        }
    }

    public static ValidationResult Validate(ExportCsvRequest request)
    {
        var r = new ValidationResult();
        if (request.Data is null)
            r.Add("DATA_NULL", "Data is required.");
        if (string.IsNullOrWhiteSpace(request.OutputPath))
            r.Add("OUTPUT_PATH_EMPTY", "Output path is required.");
        if (request.Delimiter == '\0')
            r.Add("DELIMITER_INVALID", "Delimiter is invalid.");
        return r;
    }

    private static string Escape(string s)
    {
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
        {
            var escaped = s.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
        return s;
    }
}

