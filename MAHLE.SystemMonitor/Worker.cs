using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using MAHLE.SystemMonitor.Data;
using MAHLE.SystemMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MAHLE.SystemMonitor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _machineName;
        private readonly int _checkIntervalSeconds;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _machineName = Environment.MachineName;
            _checkIntervalSeconds = int.Parse(_configuration["MonitoringSettings:CheckIntervalSeconds"] ?? "900");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System Monitor starting on machine: {MachineName}", _machineName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting monitoring cycle at: {time}", DateTimeOffset.Now);
                    
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();

                    // Update worker status
                    await UpdateWorkerStatus(dbContext);
                    _logger.LogInformation("Worker status updated successfully");

                    // Collect system information
                    await CollectDiskSpace(dbContext);
                    await CheckServices(dbContext);
                    await CheckBackups(dbContext);
                    await CheckPingTargets(dbContext);

                    _logger.LogInformation("Monitoring cycle completed. Next check in {interval} seconds", _checkIntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(_checkIntervalSeconds), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while monitoring system");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait before retrying
                }
            }
        }

        private async Task UpdateWorkerStatus(MonitoringDbContext dbContext)
        {
            var status = await dbContext.WorkerStatuses
                .FirstOrDefaultAsync(w => w.MachineName == _machineName);

            if (status == null)
            {
                status = new WorkerStatus
                {
                    MachineName = _machineName,
                    IsRunning = true,
                    LastUpdated = DateTime.UtcNow
                };
                dbContext.WorkerStatuses.Add(status);
                _logger.LogInformation("Created new worker status record for {MachineName}", _machineName);
            }
            else
            {
                status.IsRunning = true;
                status.LastUpdated = DateTime.UtcNow;
                _logger.LogInformation("Updated existing worker status for {MachineName}", _machineName);
            }

            await dbContext.SaveChangesAsync();
        }

        private async Task CollectDiskSpace(MonitoringDbContext dbContext)
        {
            _logger.LogInformation("Collecting disk space information...");
            
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady) continue;

                    var diskSpace = new DiskSpace
                    {
                        MachineName = _machineName,
                        DriveName = drive.Name,
                        TotalSpaceBytes = drive.TotalSize,
                        UsedSpaceBytes = drive.TotalSize - drive.AvailableFreeSpace,
                        CollectedAt = DateTime.UtcNow
                    };

                    dbContext.DiskSpaces.Add(diskSpace);
                    
                    var usedGB = Math.Round(diskSpace.UsedSpaceBytes / (1024.0 * 1024.0 * 1024.0), 2);
                    var totalGB = Math.Round(diskSpace.TotalSpaceBytes / (1024.0 * 1024.0 * 1024.0), 2);
                    var usagePercentage = Math.Round((double)diskSpace.UsedSpaceBytes / diskSpace.TotalSpaceBytes * 100, 2);
                    
                    _logger.LogInformation("Drive {DriveName}: {UsedGB}GB used of {TotalGB}GB ({UsagePercentage}%)",
                        drive.Name, usedGB, totalGB, usagePercentage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting disk space for drive {DriveName}", drive.Name);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Disk space collection completed");
        }

        private async Task CheckServices(MonitoringDbContext dbContext)
        {
            _logger.LogInformation("Checking monitored services...");
            
            var servicesToMonitor = await dbContext.SystemParameters
                .Where(p => p.MachineName == _machineName && p.ParameterKey.StartsWith("Service:"))
                .Select(p => p.ParameterValue)
                .ToListAsync();

            _logger.LogInformation("Found {Count} services to monitor", servicesToMonitor.Count);

            foreach (var serviceName in servicesToMonitor)
            {
                try
                {
                    var serviceController = new ServiceController(serviceName);
                    var status = new ServiceStatus
                    {
                        MachineName = _machineName,
                        ServiceName = serviceName,
                        IsRunning = serviceController.Status == ServiceControllerStatus.Running,
                        CheckedAt = DateTime.UtcNow
                    };

                    dbContext.ServiceStatuses.Add(status);
                    
                    _logger.LogInformation("Service {ServiceName} is {Status}",
                        serviceName, status.IsRunning ? "running" : "stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking service {ServiceName}", serviceName);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Service status check completed");
        }

        private async Task CheckBackups(MonitoringDbContext dbContext)
        {
            _logger.LogInformation("Checking database backups...");
            
            var backupParams = await dbContext.SystemParameters
                .Where(p => p.MachineName == _machineName && p.ParameterKey.StartsWith("Backup:"))
                .ToListAsync();

            _logger.LogInformation("Found {Count} backup locations to check", backupParams.Count);

            foreach (var param in backupParams)
            {
                try
                {
                    var dbName = param.ParameterKey.Replace("Backup:", "");
                    var backupPath = param.ParameterValue;

                    if (!Directory.Exists(backupPath))
                    {
                        _logger.LogWarning("Backup path does not exist: {Path}", backupPath);
                        continue;
                    }

                    var backupFiles = Directory.GetFiles(backupPath, "*.bak")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .Take(3);

                    foreach (var file in backupFiles)
                    {
                        var backupInfo = new BackupInfo
                        {
                            MachineName = _machineName,
                            DatabaseName = dbName,
                            BackupPath = file.FullName,
                            SizeBytes = file.Length,
                            BackupDate = file.LastWriteTime,
                            CollectedAt = DateTime.UtcNow
                        };

                        dbContext.BackupInfos.Add(backupInfo);
                        
                        var sizeGB = Math.Round(file.Length / (1024.0 * 1024.0 * 1024.0), 2);
                        _logger.LogInformation("Found backup for {Database}: {FileName}, Size: {Size}GB, Date: {Date}",
                            dbName, file.Name, sizeGB, file.LastWriteTime);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking backups for parameter {Key}", param.ParameterKey);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Backup check completed");
        }

        private async Task CheckPingTargets(MonitoringDbContext dbContext)
        {
            _logger.LogInformation("Checking ping targets...");
            
            var pingTargets = await dbContext.SystemParameters
                .Where(p => p.MachineName == _machineName && p.ParameterKey.StartsWith("PingTarget:"))
                .ToListAsync();

            _logger.LogInformation("Found {Count} targets to ping", pingTargets.Count);

            using var ping = new Ping();

            foreach (var target in pingTargets)
            {
                try
                {
                    var targetName = target.ParameterKey.Replace("PingTarget:", "");
                    var ipAddress = target.ParameterValue;

                    var reply = await ping.SendPingAsync(ipAddress);
                    var status = new PingStatus
                    {
                        SourceMachineName = _machineName,
                        TargetMachineName = targetName,
                        TargetIpAddress = ipAddress,
                        IsReachable = reply.Status == IPStatus.Success,
                        ResponseTimeMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1,
                        CheckedAt = DateTime.UtcNow
                    };

                    dbContext.PingStatuses.Add(status);
                    
                    _logger.LogInformation("Ping to {Target} ({IP}): {Status}, Response time: {Time}ms",
                        targetName, ipAddress, 
                        status.IsReachable ? "Success" : "Failed",
                        status.ResponseTimeMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pinging target {Target}", target.ParameterValue);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Ping check completed");
        }
    }
}
