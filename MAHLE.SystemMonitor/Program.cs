using System;
using MAHLE.SystemMonitor;
using MAHLE.SystemMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MAHLE.SystemMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add database context
                    var dbProvider = hostContext.Configuration["MonitoringSettings:DatabaseProvider"];
                    var connectionString = hostContext.Configuration.GetConnectionString(dbProvider);

                    if (dbProvider == "PostgreSQL")
                    {
                        services.AddDbContext<MonitoringDbContext>(options =>
                            options.UseNpgsql(connectionString));
                    }
                    else if (dbProvider == "SQLServer")
                    {
                        services.AddDbContext<MonitoringDbContext>(options =>
                            options.UseSqlServer(connectionString));
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported database provider: {dbProvider}");
                    }

                    services.AddHostedService<Worker>();
                });
    }
}
