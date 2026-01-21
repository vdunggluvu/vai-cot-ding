using DataFlowApp.Domain.Models;

namespace DataFlowApp.Domain.Interfaces;

/// <summary>
/// Interface for data persistence operations (CSV, Database, etc.)
/// </summary>
public interface IDataService
{
    Task<List<DataRecord>> LoadFromCsvAsync(string filePath);
    Task SaveToCsvAsync(string filePath, List<DataRecord> records);
    Task<List<DataRecord>> GetAllRecordsAsync();
}
