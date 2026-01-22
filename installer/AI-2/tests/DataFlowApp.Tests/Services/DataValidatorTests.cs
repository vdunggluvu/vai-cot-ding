using DataFlowApp.Domain.Models;
using DataFlowApp.Infrastructure.Logging;
using DataFlowApp.Infrastructure.Services;
using Xunit;

namespace DataFlowApp.Tests.Services;

/// <summary>
/// Unit tests for DataValidator.
/// </summary>
public class DataValidatorTests
{
    private readonly DataValidator _validator;

    public DataValidatorTests()
    {
        var logger = new FileLogger("TestLogs");
        _validator = new DataValidator(logger);
    }

    [Fact]
    public void ValidateRecord_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var record = new DataRecord
        {
            Id = 1,
            Name = "Valid Name",
            Category = "Type A",
            Value = 100.50m,
            CreatedDate = DateTime.Now.AddDays(-1),
            Status = "Pending"
        };

        // Act
        var result = _validator.ValidateRecord(record);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.True(record.IsValid);
    }

    [Fact]
    public void ValidateRecord_WithEmptyName_ReturnsError()
    {
        // Arrange
        var record = new DataRecord
        {
            Id = 1,
            Name = "",
            Category = "Type A",
            Value = 100.50m
        };

        // Act
        var result = _validator.ValidateRecord(record);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Name is required"));
        Assert.False(record.IsValid);
    }

    [Fact]
    public void ValidateRecord_WithNegativeValue_ReturnsError()
    {
        // Arrange
        var record = new DataRecord
        {
            Id = 1,
            Name = "Test",
            Category = "Type A",
            Value = -50.00m
        };

        // Act
        var result = _validator.ValidateRecord(record);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("negative"));
    }

    [Fact]
    public void ValidateRecord_WithFutureDate_ReturnsError()
    {
        // Arrange
        var record = new DataRecord
        {
            Id = 1,
            Name = "Test",
            Category = "Type A",
            Value = 100m,
            CreatedDate = DateTime.Now.AddDays(1)
        };

        // Act
        var result = _validator.ValidateRecord(record);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("future"));
    }

    [Fact]
    public void ValidateRecord_WithInvalidCategory_ReturnsWarning()
    {
        // Arrange
        var record = new DataRecord
        {
            Id = 1,
            Name = "Test",
            Category = "Unknown Category",
            Value = 100m
        };

        // Act
        var result = _validator.ValidateRecord(record);

        // Assert
        Assert.True(result.IsValid); // Still valid, just warning
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void ValidateRecords_WithMultipleRecords_ValidatesAll()
    {
        // Arrange
        var records = new List<DataRecord>
        {
            new DataRecord { Id = 1, Name = "Valid", Category = "Type A", Value = 100m },
            new DataRecord { Id = 2, Name = "", Category = "Type B", Value = 200m }, // Invalid
            new DataRecord { Id = 3, Name = "Also Valid", Category = "Type C", Value = 300m }
        };

        // Act
        var result = _validator.ValidateRecords(records);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(2, records.Count(r => r.IsValid));
        Assert.Equal(1, records.Count(r => !r.IsValid));
    }
}
