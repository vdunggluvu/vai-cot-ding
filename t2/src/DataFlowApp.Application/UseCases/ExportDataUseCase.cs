using DataFlowApp.Application.DTOs;
using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;

namespace DataFlowApp.Application.UseCases;

/// <summary>
/// Use case for exporting data to CSV files.
/// </summary>
public class ExportDataUseCase
{
    private readonly IDataService _dataService;
    private readonly ILogger _logger;

    public ExportDataUseCase(IDataService dataService, ILogger logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<ProcessResult> ExecuteAsync(string filePath, List<DataRecord>? recordsToExport = null)
    {
        _logger.LogInfo($"Starting export to: {filePath}");

        try
        {
            // If no specific records provided, export all
            var records = recordsToExport ?? await _dataService.GetAllRecordsAsync();

            if (records.Count == 0)
            {
                _logger.LogWarning("No records to export");
                return ProcessResult.CreateFailure("No records available to export");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Export to CSV
            await _dataService.SaveToCsvAsync(filePath, records);

            _logger.LogInfo($"Successfully exported {records.Count} records to {filePath}");
            return ProcessResult.CreateSuccess(
                records.Count,
                records.Count,
                $"Successfully exported {records.Count} records to {Path.GetFileName(filePath)}"
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError("Access denied", ex);
            return ProcessResult.CreateFailure($"Access denied: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Export failed", ex);
            return ProcessResult.CreateFailure($"Export failed: {ex.Message}");
        }
    }

    public async Task<ProcessResult> ExportProcessedOnlyAsync(string filePath)
    {
        _logger.LogInfo("Exporting processed records only");

        var allRecords = await _dataService.GetAllRecordsAsync();
        var processedRecords = allRecords.Where(r => r.Status == "Processed" || r.Status == "Completed").ToList();

        return await ExecuteAsync(filePath, processedRecords);
    }

    public async Task<ProcessResult> ExportValidOnlyAsync(string filePath)
    {
        _logger.LogInfo("Exporting valid records only");

        var allRecords = await _dataService.GetAllRecordsAsync();
        var validRecords = allRecords.Where(r => r.IsValid).ToList();

        return await ExecuteAsync(filePath, validRecords);
    }
}
