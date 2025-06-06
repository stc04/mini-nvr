using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AIIT.NVR.Web.Services;
using System.Threading;
using System.Threading.Tasks;

namespace AIIT.NVR.Web.Services.BackgroundServices
{
    public class SystemMonitoringService : BackgroundService
    {
        private readonly ISignalRNotificationService _notificationService;
        private readonly ILogger<SystemMonitoringService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(5);

        public SystemMonitoringService(
            ISignalRNotificationService notificationService,
            ILogger<SystemMonitoringService> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System Monitoring Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _notificationService.SendPerformanceUpdate();
                    await _notificationService.NotifySystemStatusUpdate();
                    
                    // Check for storage alerts
                    var storageUsage = GetStorageUsage();
                    if (storageUsage > 85)
                    {
                        await _notificationService.NotifyStorageAlert(
                            $"Storage usage is at {storageUsage:F1}%", 
                            storageUsage);
                    }

                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in system monitoring service");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("System Monitoring Service stopped");
        }

        private double GetStorageUsage()
        {
            // Simulate storage usage calculation
            return Math.Round(Random.Shared.NextDouble() * 20 + 65, 1);
        }
    }
}
