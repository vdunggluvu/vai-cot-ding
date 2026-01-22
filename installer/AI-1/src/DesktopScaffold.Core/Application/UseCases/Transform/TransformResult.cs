using DesktopScaffold.Core.Domain.Models;

namespace DesktopScaffold.Core.Application.UseCases.Transform;

public sealed record TransformResult(DataSet Output, TransformSummary Summary);

