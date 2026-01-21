using DataFlowApp.Application.UseCases;
using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;
using DataFlowApp.Infrastructure.Logging;
using DataFlowApp.Infrastructure.Services;
using Xunit;

namespace DataFlowApp.Tests.UseCases;

/// <summary>
/// Unit tests for ProcessDataUseCase.
/// </summary>
public class ProcessDataUseCaseTests
{
    private readonly ILogger _logger;
    private readonly IDataService _dataService;
    private readonly IValidator _validator;

    public ProcessDataUseCaseTests()
    {
        _logger = new FileLogger("TestLogs");
        _dataService = new CsvDataService(_logger);
        _validator = new DataValidator(_logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoRecords_ReturnsFailure()
    {
        // Arrange
        var useCase = new ProcessDataUseCase(_dataService, _validator, _logger);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No records", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRecords_ProcessesSuccessfully()
    {
        // Arrange
        var testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
        await CreateTestCsvFile(testFilePath);

        // Import data first
        var importUseCase = new ImportDataUseCase(_dataService, _validator, _logger);
        await importUseCase.ExecuteAsync(testFilePath, validateOnImport: false);

        var useCase = new ProcessDataUseCase(_dataService, _validator, _logger);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedRecords > 0);

        // Cleanup
        if (File.Exists(testFilePath))
            File.Delete(testFilePath);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRecords()
    {
        // Arrange
        var testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
        await CreateTestCsvWithInvalidData(testFilePath);

        var importUseCase = new ImportDataUseCase(_dataService, _validator, _logger);
        await importUseCase.ExecuteAsync(testFilePath, validateOnImport: false);

        var useCase = new ProcessDataUseCase(_dataService, _validator, _logger);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Errors.Count > 0 || result.Warnings.Count > 0);

        // Cleanup
        if (File.Exists(testFilePath))
            File.Delete(testFilePath);
    }

    private async Task CreateTestCsvFile(string filePath)
    {
        var csvContent = @"Id,Name,Category,Value,CreatedDate,Status,Description
1,""Test Item 1"",""Type A"",100.50,2024-01-01 10:00:00,Pending,""Test description""
2,""Test Item 2"",""Type B"",250.75,2024-01-02 11:00:00,Pending,""Another test""
3,""Test Item 3"",""Type C"",500.00,2024-01-03 12:00:00,Pending,""""";

        await File.WriteAllTextAsync(filePath, csvContent);
    }

    private async Task CreateTestCsvWithInvalidData(string filePath)
    {
        var csvContent = @"Id,Name,Category,Value,CreatedDate,Status,Description
1,"""",""Type A"",100.50,2024-01-01 10:00:00,Pending,""Missing name""
2,""X"",""Invalid Category"",-50.00,2025-01-01 10:00:00,Pending,""Negative value and future date""";

        await File.WriteAllTextAsync(filePath, csvContent);
    }
}
