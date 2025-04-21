using MAHLE.SystemMonitor;
using MAHLE.SystemMonitor.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Add database context
var dbProvider = builder.Configuration.GetValue<string>("MonitoringSettings:DatabaseProvider");
var connectionString = builder.Configuration.GetConnectionString(dbProvider);

builder.Services.AddDbContext<MonitoringDbContext>(options =>
{
    if (dbProvider == "PostgreSQL")
    {
        options.UseNpgsql(connectionString);
    }
    else if (dbProvider == "SQLServer")
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        throw new ArgumentException($"Unsupported database provider: {dbProvider}");
    }
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
