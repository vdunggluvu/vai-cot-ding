using DataFlowApp.Domain.Models;

namespace DataFlowApp.Domain.Interfaces;

/// <summary>
/// Interface for data validation logic.
/// </summary>
public interface IValidator
{
    ValidationResult ValidateRecord(DataRecord record);
    ValidationResult ValidateRecords(List<DataRecord> records);
}
