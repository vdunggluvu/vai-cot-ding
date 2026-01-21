using DesktopScaffold.Core.Application.UseCases.Transform;
using DesktopScaffold.Core.Domain.Models;
using DesktopScaffold.Tests.Fakes;
using Xunit;

namespace DesktopScaffold.Tests;

public sealed class TransformUseCaseTests
{
    [Fact]
    public async Task Transform_MultiplyAndFilter_Works()
    {
        var log = new TestLogger();
        var uc = new TransformUseCase(log);

        var input = new DataSet(new List<DataRow>
        {
            new(1, "A", 1),
            new(2, "B", -2),
            new(3, "C", 3)
        });

        var (validation, result) = await uc.ExecuteAsync(new TransformRequest(input, 2, filterNonPositive: true), CancellationToken.None);

        Assert.True(validation.IsValid);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Summary.InputCount);
        Assert.Equal(2, result.Summary.OutputCount);
        Assert.Equal(2, result.Output.Rows.First(r => r.Id == 1).Value);
        Assert.Equal(6, result.Output.Rows.First(r => r.Id == 3).Value);
    }
}

