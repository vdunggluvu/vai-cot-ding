using DataFlowApp.Domain.Interfaces;
using DataFlowApp.Domain.Models;
using System.Globalization;
using System.Text;

namespace DataFlowApp.Infrastructure.Services;

/// <summary>
/// CSV file data service implementation.
/// Handles reading and writing DataRecords to/from CSV files.
/// </summary>
public class CsvDataService : IDataService
{
    private readonly ILogger _logger;
    private List<DataRecord> _cachedRecords = new();

    public CsvDataService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<DataRecord>> LoadFromCsvAsync(string filePath)
    {
        _logger.LogInfo($"Loading data from CSV: {filePath}");
        
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var records = new List<DataRecord>();
            var lines = await File.ReadAllLinesAsync(filePath);

            if (lines.Length == 0)
            {
                _logger.LogWarning("CSV file is empty");
                return records;
            }

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var record = ParseCsvLine(lines[i], i);
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to parse line {i + 1}: {ex.Message}");
                }
            }

            _cachedRecords = records;
            _logger.LogInfo($"Successfully loaded {records.Count} records from CSV");
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load CSV file: {filePath}", ex);
            throw;
        }
    }

    public async Task SaveToCsvAsync(string filePath, List<DataRecord> records)
    {
        _logger.LogInfo($"Saving {records.Count} records to CSV: {filePath}");
        
        try
        {
            var lines = new List<string>
            {
                // Header
                "Id,Name,Category,Value,CreatedDate,Status,Description,ProcessedValue,IsValid,ValidationMessage"
            };

            foreach (var record in records)
            {
                lines.Add(FormatCsvLine(record));
            }

            await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
            _logger.LogInfo($"Successfully saved {records.Count} records to CSV");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save CSV file: {filePath}", ex);
            throw;
        }
    }

    public Task<List<DataRecord>> GetAllRecordsAsync()
    {
        return Task.FromResult(_cachedRecords);
    }

    private DataRecord? ParseCsvLine(string line, int lineNumber)
    {
        var parts = SplitCsvLine(line);
        
        if (parts.Length < 6)
        {
            _logger.LogWarning($"Line {lineNumber} has insufficient columns");
            return null;
        }

        return new DataRecord
        {
            Id = int.TryParse(parts[0], out var id) ? id : lineNumber,
            Name = parts[1],
            Category = parts[2],
            Value = decimal.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0,
            CreatedDate = DateTime.TryParse(parts[4], out var date) ? date : DateTime.Now,
            Status = parts.Length > 5 ? parts[5] : "Pending",
            Description = parts.Length > 6 ? parts[6] : null,
            ProcessedValue = parts.Length > 7 && decimal.TryParse(parts[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal : 0,
            IsValid = parts.Length > 8 && bool.TryParse(parts[8], out var isValid) && isValid,
            ValidationMessage = parts.Length > 9 ? parts[9] : null
        };
    }

    private string FormatCsvLine(DataRecord record)
    {
        return $"{record.Id}," +
               $"\"{EscapeCsv(record.Name)}\"," +
               $"\"{EscapeCsv(record.Category)}\"," +
               $"{record.Value.ToString(CultureInfo.InvariantCulture)}," +
               $"{record.CreatedDate:yyyy-MM-dd HH:mm:ss}," +
               $"\"{EscapeCsv(record.Status)}\"," +
               $"\"{EscapeCsv(record.Description ?? "")}\"," +
               $"{record.ProcessedValue.ToString(CultureInfo.InvariantCulture)}," +
               $"{record.IsValid}," +
               $"\"{EscapeCsv(record.ValidationMessage ?? "")}\"";
    }

    private string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim('"'));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim('"'));
        return result.ToArray();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\"", "\"\"");
    }
}
