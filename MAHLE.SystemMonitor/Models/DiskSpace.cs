using System;

namespace MAHLE.SystemMonitor.Models
{
    public class DiskSpace
    {
        public int Id { get; set; }
        public string MachineName { get; set; } = null!;
        public string DriveName { get; set; } = null!;
        public long TotalSpaceBytes { get; set; }
        public long UsedSpaceBytes { get; set; }
        public DateTime CollectedAt { get; set; }
    }
}