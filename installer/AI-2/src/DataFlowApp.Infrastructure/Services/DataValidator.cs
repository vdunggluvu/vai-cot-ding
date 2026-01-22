using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;

namespace DataFlowApp.Infrastructure.Services;

/// <summary>
/// Data validation service.
/// Implements business rules for validating DataRecords.
/// Customize validation rules here for your specific use case.
/// </summary>
public class DataValidator : IValidator
{
    private readonly ILogger _logger;

    public DataValidator(ILogger logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateRecord(DataRecord record)
    {
        var result = new ValidationResult { IsValid = true };

        // Rule 1: Name is required and must be at least 2 characters
        if (string.IsNullOrWhiteSpace(record.Name))
        {
            result.AddError("Name is required");
        }
        else if (record.Name.Length < 2)
        {
            result.AddError("Name must be at least 2 characters");
        }

        // Rule 2: Category must be valid
        var validCategories = new[] { "Type A", "Type B", "Type C", "Other" };
        if (string.IsNullOrWhiteSpace(record.Category))
        {
            result.AddError("Category is required");
        }
        else if (!validCategories.Contains(record.Category))
        {
            result.AddWarning($"Category '{record.Category}' is not in standard list");
        }

        // Rule 3: Value must be positive
        if (record.Value < 0)
        {
            result.AddError("Value cannot be negative");
        }
        else if (record.Value == 0)
        {
            result.AddWarning("Value is zero");
        }

        // Rule 4: Value must be within reasonable range
        if (record.Value > 1000000)
        {
            result.AddWarning("Value is unusually high (> 1,000,000)");
        }

        // Rule 5: Created date should not be in the future
        if (record.CreatedDate > DateTime.Now)
        {
            result.AddError("Created date cannot be in the future");
        }

        // Rule 6: Status must be valid
        var validStatuses = new[] { "Pending", "Processed", "Completed", "Failed" };
        if (!validStatuses.Contains(record.Status))
        {
            result.AddWarning($"Status '{record.Status}' is not standard");
        }

        // Update record validation fields
        record.IsValid = result.IsValid;
        record.ValidationMessage = result.IsValid 
            ? "Valid" 
            : string.Join("; ", result.Errors);

        return result;
    }

    public ValidationResult ValidateRecords(List<DataRecord> records)
    {
        _logger.LogInfo($"Validating {records.Count} records");
        
        var overallResult = new ValidationResult { IsValid = true };
        int validCount = 0;
        int invalidCount = 0;

        foreach (var record in records)
        {
            var recordResult = ValidateRecord(record);
            
            if (!recordResult.IsValid)
            {
                invalidCount++;
                overallResult.IsValid = false;
                foreach (var error in recordResult.Errors)
                {
                    overallResult.AddError($"Record {record.Id}: {error}");
                }
            }
            else
            {
                validCount++;
            }

            foreach (var warning in recordResult.Warnings)
            {
                overallResult.AddWarning($"Record {record.Id}: {warning}");
            }
        }

        _logger.LogInfo($"Validation complete: {validCount} valid, {invalidCount} invalid");
        
        if (overallResult.Warnings.Count > 0)
        {
            _logger.LogWarning($"Validation produced {overallResult.Warnings.Count} warnings");
        }

        return overallResult;
    }
}
