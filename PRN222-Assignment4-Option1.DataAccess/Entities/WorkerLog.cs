using System;

namespace PRN222_Assignment4_Option1.DataAccess.Entities;

public class WorkerLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
}
