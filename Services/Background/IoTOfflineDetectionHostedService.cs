using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Services.Security;

namespace SecureTransparentDataExchange.Services.Background;

public class IoTOfflineDetectionHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public IoTOfflineDetectionHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var audit = scope.ServiceProvider.GetRequiredService<AuditLogService>();

            var now = DateTime.UtcNow;
            var threshold = now.AddMinutes(-2);

            var offline = await db.IoTDevices
                .Where(d => d.LastUpdated < threshold)
                .ToListAsync(stoppingToken);

            foreach (var d in offline)
            {
                await audit.LogWarningAsync(
                    "IoT device offline",
                    $"Device {d.Id} tracking={d.TrackingNumber} last={d.LastUpdated:O}"
                );
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
