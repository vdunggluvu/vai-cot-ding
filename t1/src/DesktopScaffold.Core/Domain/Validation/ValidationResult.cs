namespace DesktopScaffold.Core.Domain.Validation;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;

    public static ValidationResult Ok() => new();

    public static ValidationResult FromError(string code, string message)
    {
        var r = new ValidationResult();
        r.Add(code, message);
        return r;
    }

    public void Add(string code, string message) => _errors.Add(new ValidationError(code, message));

    public override string ToString()
        => IsValid ? "OK" : string.Join(Environment.NewLine, _errors.Select(e => $"{e.Code}: {e.Message}"));
}

