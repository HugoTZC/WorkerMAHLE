namespace MAHLE.SystemMonitor.Models;

public class SystemParameters
{
    public int Id { get; set; }
    public string MachineName { get; set; } = null!;
    public string ParameterKey { get; set; } = null!;
    public string ParameterValue { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime LastModified { get; set; }
}