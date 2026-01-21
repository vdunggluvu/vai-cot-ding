using DesktopScaffold.Core.Application.Abstractions;
using DesktopScaffold.Core.Domain.Models;
using DesktopScaffold.Core.Domain.Validation;

namespace DesktopScaffold.Core.Application.UseCases.Transform;

public sealed class TransformUseCase
{
    private readonly IAppLogger _log;

    public TransformUseCase(IAppLogger log)
    {
        _log = log;
    }

    public Task<(ValidationResult Validation, TransformResult? Result)> ExecuteAsync(
        TransformRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var validation = Validate(request);
        if (!validation.IsValid)
            return Task.FromResult<(ValidationResult, TransformResult?)>((validation, null));

        try
        {
            _log.Info($"Transform started: factor={request.MultiplyFactor}, filterNonPositive={request.FilterNonPositive}");

            var input = request.Input.Rows;
            var transformed = new List<DataRow>(input.Count);

            foreach (var row in input)
            {
                var value = row.Value * request.MultiplyFactor;
                if (request.FilterNonPositive && value <= 0)
                    continue;

                transformed.Add(row with { Value = value });
            }

            var summary = BuildSummary(input.Count, transformed);
            var output = new DataSet(transformed);

            _log.Info($"Transform completed: {summary.InputCount} -> {summary.OutputCount} rows");
            return Task.FromResult<(ValidationResult, TransformResult?)>((ValidationResult.Ok(), new TransformResult(output, summary)));
        }
        catch (Exception ex)
        {
            _log.Error("Transform failed", ex);
            return Task.FromResult<(ValidationResult, TransformResult?)>((ValidationResult.FromError("TRANSFORM_FAILED", ex.Message), null));
        }
    }

    public static ValidationResult Validate(TransformRequest request)
    {
        var r = new ValidationResult();
        if (request.Input is null)
            r.Add("INPUT_NULL", "Input dataset is required.");
        if (double.IsNaN(request.MultiplyFactor) || double.IsInfinity(request.MultiplyFactor))
            r.Add("FACTOR_INVALID", "Multiply factor must be a finite number.");
        return r;
    }

    private static TransformSummary BuildSummary(int inputCount, List<DataRow> outputRows)
    {
        if (outputRows.Count == 0)
            return new TransformSummary(inputCount, 0, 0, 0, 0, 0);

        var values = outputRows.Select(r => r.Value).ToList();
        var sum = values.Sum();
        var avg = sum / values.Count;
        var min = values.Min();
        var max = values.Max();
        return new TransformSummary(inputCount, values.Count, sum, avg, min, max);
    }
}

