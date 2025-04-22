using System;
using Microsoft.EntityFrameworkCore;
using MAHLE.SystemMonitor.Models;

namespace MAHLE.SystemMonitor.Data
{
    public class MonitoringDbContext : DbContext
    {
        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
            : base(options)
        {
        }

        public DbSet<WorkerStatus> WorkerStatuses { get; set; }
        public DbSet<DiskSpace> DiskSpaces { get; set; }
        public DbSet<ServiceStatus> ServiceStatuses { get; set; }
        public DbSet<BackupInfo> BackupInfos { get; set; }
        public DbSet<PingStatus> PingStatuses { get; set; }
        public DbSet<SystemParameters> SystemParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes and constraints
            modelBuilder.Entity<WorkerStatus>()
                .HasIndex(w => w.MachineName);

            modelBuilder.Entity<DiskSpace>()
                .HasIndex(d => new { d.MachineName, d.DriveName });

            modelBuilder.Entity<ServiceStatus>()
                .HasIndex(s => new { s.MachineName, s.ServiceName });

            modelBuilder.Entity<BackupInfo>()
                .HasIndex(b => new { b.MachineName, b.DatabaseName });

            modelBuilder.Entity<PingStatus>()
                .HasIndex(p => new { p.SourceMachineName, p.TargetMachineName });

            modelBuilder.Entity<SystemParameters>()
                .HasIndex(p => new { p.MachineName, p.ParameterKey })
                .IsUnique();
        }
    }
}