using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.UseCases.Transform;

public sealed record TransformRequest(DataSet Input, double MultiplyFactor, bool FilterNonPositive);

