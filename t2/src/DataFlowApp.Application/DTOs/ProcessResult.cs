namespace DataFlowApp.Application.DTOs;

/// <summary>
/// Data Transfer Object for process results.
/// </summary>
public class ProcessResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ProcessResult CreateSuccess(int total, int processed, string message = "Success")
    {
        return new ProcessResult
        {
            Success = true,
            Message = message,
            TotalRecords = total,
            ProcessedRecords = processed,
            FailedRecords = total - processed
        };
    }
    
    public static ProcessResult CreateFailure(string message, List<string>? errors = null)
    {
        return new ProcessResult
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
