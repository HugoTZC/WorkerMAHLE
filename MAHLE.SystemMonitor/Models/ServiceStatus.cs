namespace MAHLE.SystemMonitor.Models;

public class ServiceStatus
{
    public int Id { get; set; }
    public string MachineName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public bool IsRunning { get; set; }
    public DateTime CheckedAt { get; set; }
}