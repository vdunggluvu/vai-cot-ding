namespace DataFlowApp.Domain.Models;

/// <summary>
/// Represents a single data record in the system.
/// This is the core domain model that can be customized for specific use cases.
/// </summary>
public class DataRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Description { get; set; }
    
    // Processed fields
    public decimal ProcessedValue { get; set; }
    public bool IsValid { get; set; }
    public string? ValidationMessage { get; set; }
    
    public DataRecord()
    {
        CreatedDate = DateTime.Now;
    }
    
    public DataRecord Clone()
    {
        return new DataRecord
        {
            Id = this.Id,
            Name = this.Name,
            Category = this.Category,
            Value = this.Value,
            CreatedDate = this.CreatedDate,
            Status = this.Status,
            Description = this.Description,
            ProcessedValue = this.ProcessedValue,
            IsValid = this.IsValid,
            ValidationMessage = this.ValidationMessage
        };
    }
}
