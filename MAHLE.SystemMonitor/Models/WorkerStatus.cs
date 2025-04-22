using System;

namespace MAHLE.SystemMonitor.Models
{
    public class WorkerStatus
    {
        public int Id { get; set; }
        public string MachineName { get; set; } = null!;
        public bool IsRunning { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}