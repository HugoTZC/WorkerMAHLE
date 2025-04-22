using System;

namespace MAHLE.SystemMonitor.Models
{
    public class BackupInfo
    {
        public int Id { get; set; }
        public string MachineName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string BackupPath { get; set; } = null!;
        public long SizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public DateTime CollectedAt { get; set; }
    }
}