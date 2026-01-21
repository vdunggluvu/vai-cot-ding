namespace DesktopScaffold.Core.Domain.Models;

public sealed record TransformSummary(
    int InputCount,
    int OutputCount,
    double Sum,
    double Average,
    double Min,
    double Max
);

