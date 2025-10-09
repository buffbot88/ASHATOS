using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RaCore.Engine.Memory;

/// <summary>
/// Background service that performs periodic memory maintenance.
/// Runs cleanup tasks on a scheduled interval to prevent memory bloat.
/// </summary>
public class MemoryMaintenanceService : BackgroundService
{
    private readonly ILogger<MemoryMaintenanceService> _logger;
    private readonly MemoryModule _memoryModule;
    private readonly TimeSpan _interval;

    public MemoryMaintenanceService(
        ILogger<MemoryMaintenanceService> logger,
        MemoryModule memoryModule,
        TimeSpan? interval = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryModule = memoryModule ?? throw new ArgumentNullException(nameof(memoryModule));
        _interval = interval ?? TimeSpan.FromHours(24); // Default: run once per day
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Memory Maintenance Service started. Running every {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                _logger.LogInformation("Starting scheduled memory maintenance...");
                
                var beforeCount = _memoryModule.Count();
                _memoryModule.PerformMaintenance();
                var afterCount = _memoryModule.Count();

                _logger.LogInformation(
                    "Memory maintenance completed. Items: {Before} → {After} (removed {Removed})",
                    beforeCount, afterCount, beforeCount - afterCount);
            }
            catch (TaskCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during memory maintenance");
                // Continue running despite errors
            }
        }

        _logger.LogInformation("Memory Maintenance Service stopped");
    }
}
