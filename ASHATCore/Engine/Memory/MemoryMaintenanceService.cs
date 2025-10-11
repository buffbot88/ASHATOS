using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASHATCore.Engine.Memory;

/// <summary>
/// Background service that performs periodic memory maintenance.
/// Runs cleanup tasks on a scheduled interval to prevent memory bloat.
/// </summary>
public class MemoryMaintenanceService : BackgroundService
{
    private readonly ILogger<MemoryMaintenanceService> _logger;
    private readonly MemoryModule _memoryModule;
    private readonly TimeSpan _interval;
    private readonly MemoryHealthMonitor? _healthMonitor;

    public MemoryMaintenanceService(
        ILogger<MemoryMaintenanceService> logger,
        MemoryModule memoryModule,
        TimeSpan? interval = null,
        MemoryHealthMonitor? healthMonitor = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryModule = memoryModule ?? throw new ArgumentNullException(nameof(memoryModule));
        _interval = interval ?? TimeSpan.FromHours(24); // Default: run once per day
        _healthMonitor = healthMonitor;
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
                bool success = true;
                string? error = null;
                int pruned = 0, deduplicated = 0, limited = 0;
                
                try
                {
                    var results = _memoryModule.PerformMaintenance();
                    pruned = results.pruned;
                    deduplicated = results.deduplicated;
                    limited = results.limited;
                }
                catch (Exception ex)
                {
                    success = false;
                    error = ex.Message;
                    _logger.LogError(ex, "Maintenance cycle failed");
                }
                
                var afterCount = _memoryModule.Count();

                // Update health monitor with results
                _healthMonitor?.RecordMaintenanceCycle(pruned, deduplicated, limited, success, error);

                _logger.LogInformation(
                    "Memory maintenance completed. Items: {Before} → {After} (pruned: {Pruned}, dedup: {Dedup}, limited: {Limited})",
                    beforeCount, afterCount, pruned, deduplicated, limited);
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
