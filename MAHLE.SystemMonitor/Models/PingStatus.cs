namespace MAHLE.SystemMonitor.Models;

public class PingStatus
{
    public int Id { get; set; }
    public string SourceMachineName { get; set; } = null!;
    public string TargetMachineName { get; set; } = null!;
    public string TargetIpAddress { get; set; } = null!;
    public bool IsReachable { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
}