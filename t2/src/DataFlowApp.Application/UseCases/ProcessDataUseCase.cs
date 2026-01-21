using DataFlowApp.Application.DTOs;
using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;

namespace DataFlowApp.Application.UseCases;

/// <summary>
/// Use case for processing and transforming data.
/// This is where your core business logic goes.
/// </summary>
public class ProcessDataUseCase
{
    private readonly IDataService _dataService;
    private readonly IValidator _validator;
    private readonly ILogger _logger;

    public ProcessDataUseCase(IDataService dataService, IValidator validator, ILogger logger)
    {
        _dataService = dataService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ProcessResult> ExecuteAsync()
    {
        _logger.LogInfo("Starting data processing");

        try
        {
            // Get all records from data service
            var records = await _dataService.GetAllRecordsAsync();

            if (records.Count == 0)
            {
                _logger.LogWarning("No records to process");
                return ProcessResult.CreateFailure("No records available to process");
            }

            // Validate records
            _logger.LogInfo($"Validating {records.Count} records");
            var validationResult = _validator.ValidateRecords(records);

            // Process valid records
            int processedCount = 0;
            int failedCount = 0;

            foreach (var record in records)
            {
                try
                {
                    if (record.IsValid)
                    {
                        TransformRecord(record);
                        record.Status = "Processed";
                        processedCount++;
                    }
                    else
                    {
                        record.Status = "Failed";
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to process record {record.Id}", ex);
                    record.Status = "Failed";
                    record.ValidationMessage = ex.Message;
                    failedCount++;
                }
            }

            _logger.LogInfo($"Processing complete: {processedCount} processed, {failedCount} failed");

            var result = ProcessResult.CreateSuccess(
                records.Count,
                processedCount,
                $"Processed {processedCount} of {records.Count} records"
            );

            result.FailedRecords = failedCount;
            result.Errors = validationResult.Errors;
            result.Warnings = validationResult.Warnings;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Data processing failed", ex);
            return ProcessResult.CreateFailure($"Processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Transform/process a single record.
    /// Customize this method with your specific business logic.
    /// </summary>
    private void TransformRecord(DataRecord record)
    {
        // Example transformation: apply a multiplier based on category
        decimal multiplier = record.Category switch
        {
            "Type A" => 1.1m,
            "Type B" => 1.25m,
            "Type C" => 1.5m,
            _ => 1.0m
        };

        record.ProcessedValue = record.Value * multiplier;

        // Example: add computed description
        if (string.IsNullOrEmpty(record.Description))
        {
            record.Description = $"Processed on {DateTime.Now:yyyy-MM-dd}";
        }

        _logger.LogDebug($"Transformed record {record.Id}: Value {record.Value} -> {record.ProcessedValue}");
    }
}
