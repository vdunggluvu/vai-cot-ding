using DataFlowApp.Application.DTOs;
using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;

namespace DataFlowApp.Application.UseCases;

/// <summary>
/// Use case for importing data from CSV files.
/// </summary>
public class ImportDataUseCase
{
    private readonly IDataService _dataService;
    private readonly IValidator _validator;
    private readonly ILogger _logger;

    public ImportDataUseCase(IDataService dataService, IValidator validator, ILogger logger)
    {
        _dataService = dataService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ProcessResult> ExecuteAsync(string filePath, bool validateOnImport = true)
    {
        _logger.LogInfo($"Starting import from: {filePath}");

        try
        {
            // Load data from CSV
            var records = await _dataService.LoadFromCsvAsync(filePath);

            if (records.Count == 0)
            {
                _logger.LogWarning("No records found in file");
                return ProcessResult.CreateFailure("No records found in the selected file");
            }

            // Optionally validate on import
            if (validateOnImport)
            {
                _logger.LogInfo("Validating imported records");
                var validationResult = _validator.ValidateRecords(records);
                
                var result = ProcessResult.CreateSuccess(
                    records.Count,
                    records.Count,
                    $"Successfully imported {records.Count} records"
                );

                if (!validationResult.IsValid)
                {
                    result.Errors = validationResult.Errors;
                    result.Warnings = validationResult.Warnings;
                    _logger.LogWarning($"Import completed with {validationResult.Errors.Count} validation errors");
                }
                else if (validationResult.Warnings.Count > 0)
                {
                    result.Warnings = validationResult.Warnings;
                    _logger.LogWarning($"Import completed with {validationResult.Warnings.Count} warnings");
                }

                return result;
            }

            _logger.LogInfo($"Successfully imported {records.Count} records without validation");
            return ProcessResult.CreateSuccess(records.Count, records.Count, 
                $"Successfully imported {records.Count} records");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError("File not found", ex);
            return ProcessResult.CreateFailure($"File not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Import failed", ex);
            return ProcessResult.CreateFailure($"Import failed: {ex.Message}");
        }
    }
}
