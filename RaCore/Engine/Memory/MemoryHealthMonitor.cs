using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaCore.Engine.Memory
{
    /// <summary>
    /// Continuous health monitoring service for memory system.
    /// Tracks metrics, evaluates alerts, and provides observability.
    /// </summary>
    public class MemoryHealthMonitor : BackgroundService
    {
        private readonly ILogger<MemoryHealthMonitor> _logger;
        private readonly MemoryModule _memoryModule;
        private readonly MemoryAlertManager _alertManager;
        private readonly TimeSpan _checkInterval;
        private readonly string? _dbPath;
        
        private MemoryMetrics _currentMetrics = new();
        private DateTime _serviceStartTime;
        private int _consecutiveFailures = 0;
        
        public MemoryHealthMonitor(
            ILogger<MemoryHealthMonitor> logger,
            MemoryModule memoryModule,
            MemoryAlertConfig? alertConfig = null,
            TimeSpan? checkInterval = null,
            string? dbPath = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryModule = memoryModule ?? throw new ArgumentNullException(nameof(memoryModule));
            _alertManager = new MemoryAlertManager(alertConfig);
            _checkInterval = checkInterval ?? TimeSpan.FromMinutes(5); // Default: check every 5 minutes
            _dbPath = dbPath;
            
            // Subscribe to alert events
            _alertManager.OnAlertRaised += OnAlertRaised;
            _alertManager.OnAlertCleared += OnAlertCleared;
        }
        
        /// <summary>
        /// Gets the current metrics snapshot.
        /// </summary>
        public MemoryMetrics GetCurrentMetrics()
        {
            return _currentMetrics;
        }
        
        /// <summary>
        /// Gets all active alerts.
        /// </summary>
        public List<MemoryAlert> GetActiveAlerts()
        {
            return _alertManager.GetActiveAlerts();
        }
        
        /// <summary>
        /// Gets the alert manager for direct access to alert events.
        /// </summary>
        public MemoryAlertManager GetAlertManager()
        {
            return _alertManager;
        }
        
        /// <summary>
        /// Forces an immediate health check.
        /// </summary>
        public MemoryMetrics CheckHealthNow()
        {
            CollectMetrics();
            _alertManager.EvaluateMetrics(_currentMetrics);
            return _currentMetrics;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceStartTime = DateTime.UtcNow;
            _logger.LogInformation("Memory Health Monitor started. Checking every {Interval}", _checkInterval);
            
            // Initial health check
            try
            {
                CollectMetrics();
                _logger.LogInformation("Initial health check: {Metrics}", _currentMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial health check");
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                    
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    
                    // Collect metrics
                    CollectMetrics();
                    
                    // Evaluate alerts
                    var alerts = _alertManager.EvaluateMetrics(_currentMetrics);
                    
                    // Log health status
                    if (_currentMetrics.IsHealthy)
                    {
                        _logger.LogInformation("Health check: {Status} - {Summary}", 
                            "✓ HEALTHY", _currentMetrics);
                        _consecutiveFailures = 0;
                    }
                    else
                    {
                        _logger.LogWarning("Health check: {Status} - {Summary} - {AlertCount} active alerts", 
                            "⚠ ATTENTION NEEDED", _currentMetrics, alerts.Count);
                    }
                    
                    // Log detailed metrics periodically (every hour)
                    if (DateTime.UtcNow.Minute == 0)
                    {
                        _logger.LogInformation("Hourly metrics report:\n{Report}", 
                            _currentMetrics.GetDetailedReport());
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    _logger.LogError(ex, "Error during health check (consecutive failures: {Count})", 
                        _consecutiveFailures);
                    
                    // Update metrics to reflect failure
                    _currentMetrics.LastMaintenanceSuccess = false;
                    _currentMetrics.LastMaintenanceError = ex.Message;
                    _currentMetrics.FailedMaintenanceCycles++;
                }
            }
            
            _logger.LogInformation("Memory Health Monitor stopped");
        }
        
        private void CollectMetrics()
        {
            try
            {
                var items = _memoryModule.GetAllItems().ToList();
                
                // Update current state metrics
                _currentMetrics.TotalItems = items.Count;
                
                if (items.Any())
                {
                    _currentMetrics.OldestItemDate = items.Min(i => i.CreatedAt);
                    _currentMetrics.NewestItemDate = items.Max(i => i.CreatedAt);
                    _currentMetrics.AverageItemAgeHours = items.Average(i => 
                        (DateTime.UtcNow - i.CreatedAt).TotalHours);
                }
                else
                {
                    _currentMetrics.OldestItemDate = null;
                    _currentMetrics.NewestItemDate = null;
                    _currentMetrics.AverageItemAgeHours = 0;
                }
                
                // Get database size if path is available
                if (!string.IsNullOrEmpty(_dbPath) && File.Exists(_dbPath))
                {
                    var fileInfo = new FileInfo(_dbPath);
                    _currentMetrics.DatabaseSizeBytes = fileInfo.Length;
                }
                
                // Calculate rates based on time since service start
                var runningTimeHours = (DateTime.UtcNow - _serviceStartTime).TotalHours;
                if (runningTimeHours > 0)
                {
                    _currentMetrics.PruneRate = _currentMetrics.TotalItemsPruned / runningTimeHours;
                    _currentMetrics.DeduplicationRate = _currentMetrics.TotalItemsDeduplicated / runningTimeHours;
                    
                    // Storage rate is calculated as items added per hour
                    // This is approximate based on current count
                    _currentMetrics.StorageRate = items.Count / runningTimeHours;
                }
                
                // Update configuration values from module
                _currentMetrics.ConfiguredMaxItems = _memoryModule.MaxItems;
                _currentMetrics.ConfiguredMaxAgeHours = _memoryModule.MaxAge.TotalHours;
                
                _logger.LogDebug("Metrics collected: {Metrics}", _currentMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting metrics");
                throw;
            }
        }
        
        /// <summary>
        /// Called when maintenance completes. Updates metrics tracking.
        /// </summary>
        public void RecordMaintenanceCycle(int pruned, int deduplicated, int limitRemoved, bool success, string? error = null)
        {
            _currentMetrics.ItemsPrunedLastCycle = pruned;
            _currentMetrics.ItemsDeduplicatedLastCycle = deduplicated;
            _currentMetrics.ItemsRemovedByLimitLastCycle = limitRemoved;
            _currentMetrics.LastMaintenanceTime = DateTime.UtcNow;
            _currentMetrics.LastMaintenanceSuccess = success;
            _currentMetrics.LastMaintenanceError = error;
            
            _currentMetrics.TotalMaintenanceCycles++;
            _currentMetrics.TotalItemsPruned += pruned;
            _currentMetrics.TotalItemsDeduplicated += deduplicated;
            _currentMetrics.TotalItemsRemovedByLimit += limitRemoved;
            
            if (!success)
            {
                _currentMetrics.FailedMaintenanceCycles++;
            }
            
            _logger.LogInformation(
                "Maintenance cycle recorded: Pruned={Pruned}, Deduplicated={Dedup}, Limited={Limit}, Success={Success}",
                pruned, deduplicated, limitRemoved, success);
        }
        
        private void OnAlertRaised(MemoryAlert alert)
        {
            switch (alert.Severity)
            {
                case MemoryAlertSeverity.Critical:
                    _logger.LogCritical("ALERT RAISED: {Alert}", alert);
                    break;
                case MemoryAlertSeverity.Warning:
                    _logger.LogWarning("ALERT RAISED: {Alert}", alert);
                    break;
                case MemoryAlertSeverity.Info:
                    _logger.LogInformation("ALERT RAISED: {Alert}", alert);
                    break;
            }
            
            // Raise diagnostic event
            MemoryDiagnostics.RaiseEvent($"Alert: [{alert.Severity}] {alert.Type} - {alert.Message}");
        }
        
        private void OnAlertCleared(MemoryAlert alert)
        {
            _logger.LogInformation("ALERT CLEARED: {Alert}", alert);
            MemoryDiagnostics.RaiseEvent($"Alert cleared: {alert.Type}");
        }
    }
}
