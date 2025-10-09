using System;

namespace RaCore.Engine.Memory
{
    /// <summary>
    /// Severity levels for memory alerts.
    /// </summary>
    public enum MemoryAlertSeverity
    {
        Info,
        Warning,
        Critical
    }
    
    /// <summary>
    /// Types of memory alerts.
    /// </summary>
    public enum MemoryAlertType
    {
        CapacityThresholdExceeded,
        DiskUsageThresholdExceeded,
        MaintenanceFailure,
        OldItemsAccumulating,
        HighPruneRate,
        UnusualGrowth
    }
    
    /// <summary>
    /// Represents a memory system alert.
    /// </summary>
    public class MemoryAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public MemoryAlertType Type { get; set; }
        public MemoryAlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        public override string ToString()
        {
            return $"[{Severity}] {Type}: {Message} (at {Timestamp:yyyy-MM-dd HH:mm:ss})";
        }
    }
    
    /// <summary>
    /// Configuration for memory alert thresholds.
    /// </summary>
    public class MemoryAlertConfig
    {
        // Capacity thresholds
        public double CapacityWarningThresholdPercent { get; set; } = 75.0;
        public double CapacityCriticalThresholdPercent { get; set; } = 90.0;
        
        // Disk usage thresholds (in MB)
        public long DiskUsageWarningThresholdMb { get; set; } = 100;
        public long DiskUsageCriticalThresholdMb { get; set; } = 500;
        
        // Age thresholds (in hours)
        public double AgeWarningThresholdPercent { get; set; } = 80.0;
        public double AgeCriticalThresholdPercent { get; set; } = 95.0;
        
        // Rate thresholds (items per hour)
        public double HighPruneRateThreshold { get; set; } = 100.0;
        public double HighDeduplicationRateThreshold { get; set; } = 50.0;
        
        // Maintenance failure threshold
        public int ConsecutiveFailuresBeforeAlert { get; set; } = 2;
        
        // Growth rate threshold (percentage increase per hour)
        public double UnusualGrowthRatePercent { get; set; } = 10.0;
        
        public override string ToString()
        {
            return $"MemoryAlertConfig: Capacity={CapacityWarningThresholdPercent}/{CapacityCriticalThresholdPercent}%, " +
                   $"Disk={DiskUsageWarningThresholdMb}/{DiskUsageCriticalThresholdMb}MB, " +
                   $"Age={AgeWarningThresholdPercent}/{AgeCriticalThresholdPercent}%";
        }
    }
    
    /// <summary>
    /// Alert manager for memory system.
    /// </summary>
    public class MemoryAlertManager
    {
        private readonly MemoryAlertConfig _config;
        private readonly List<MemoryAlert> _activeAlerts = new();
        private readonly object _lock = new();
        
        public event Action<MemoryAlert>? OnAlertRaised;
        public event Action<MemoryAlert>? OnAlertCleared;
        
        public MemoryAlertManager(MemoryAlertConfig? config = null)
        {
            _config = config ?? new MemoryAlertConfig();
        }
        
        /// <summary>
        /// Evaluates current metrics and raises alerts if thresholds are exceeded.
        /// </summary>
        public List<MemoryAlert> EvaluateMetrics(MemoryMetrics metrics)
        {
            var newAlerts = new List<MemoryAlert>();
            
            // Check capacity thresholds
            if (metrics.CapacityUtilizationPercent >= _config.CapacityCriticalThresholdPercent)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.CapacityThresholdExceeded,
                    MemoryAlertSeverity.Critical,
                    $"Memory capacity at critical level: {metrics.CapacityUtilizationPercent:F1}%",
                    $"Current: {metrics.TotalItems:N0} items, Max: {metrics.ConfiguredMaxItems:N0}",
                    new Dictionary<string, object>
                    {
                        ["CapacityPercent"] = metrics.CapacityUtilizationPercent,
                        ["CurrentItems"] = metrics.TotalItems,
                        ["MaxItems"] = metrics.ConfiguredMaxItems
                    }
                ));
            }
            else if (metrics.CapacityUtilizationPercent >= _config.CapacityWarningThresholdPercent)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.CapacityThresholdExceeded,
                    MemoryAlertSeverity.Warning,
                    $"Memory capacity approaching limit: {metrics.CapacityUtilizationPercent:F1}%",
                    $"Current: {metrics.TotalItems:N0} items, Max: {metrics.ConfiguredMaxItems:N0}",
                    new Dictionary<string, object>
                    {
                        ["CapacityPercent"] = metrics.CapacityUtilizationPercent,
                        ["CurrentItems"] = metrics.TotalItems,
                        ["MaxItems"] = metrics.ConfiguredMaxItems
                    }
                ));
            }
            
            // Check disk usage thresholds
            var diskUsageMb = metrics.DatabaseSizeBytes / 1024.0 / 1024.0;
            if (diskUsageMb >= _config.DiskUsageCriticalThresholdMb)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.DiskUsageThresholdExceeded,
                    MemoryAlertSeverity.Critical,
                    $"Disk usage at critical level: {diskUsageMb:F1} MB",
                    $"Database size exceeds {_config.DiskUsageCriticalThresholdMb} MB threshold",
                    new Dictionary<string, object>
                    {
                        ["DiskUsageMb"] = diskUsageMb,
                        ["ThresholdMb"] = _config.DiskUsageCriticalThresholdMb
                    }
                ));
            }
            else if (diskUsageMb >= _config.DiskUsageWarningThresholdMb)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.DiskUsageThresholdExceeded,
                    MemoryAlertSeverity.Warning,
                    $"Disk usage elevated: {diskUsageMb:F1} MB",
                    $"Database size approaching {_config.DiskUsageWarningThresholdMb} MB threshold",
                    new Dictionary<string, object>
                    {
                        ["DiskUsageMb"] = diskUsageMb,
                        ["ThresholdMb"] = _config.DiskUsageWarningThresholdMb
                    }
                ));
            }
            
            // Check maintenance failures
            if (!metrics.LastMaintenanceSuccess)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.MaintenanceFailure,
                    MemoryAlertSeverity.Critical,
                    "Memory maintenance cycle failed",
                    metrics.LastMaintenanceError ?? "Unknown error",
                    new Dictionary<string, object>
                    {
                        ["LastMaintenanceTime"] = metrics.LastMaintenanceTime ?? DateTime.MinValue,
                        ["Error"] = metrics.LastMaintenanceError ?? "Unknown"
                    }
                ));
            }
            
            // Check age thresholds
            if (metrics.AgeUtilizationPercent >= _config.AgeCriticalThresholdPercent)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.OldItemsAccumulating,
                    MemoryAlertSeverity.Critical,
                    $"Old items accumulating: {metrics.AgeUtilizationPercent:F1}% of max age",
                    $"Oldest item: {metrics.OldestItemDate:yyyy-MM-dd}, configured max age: {metrics.ConfiguredMaxAgeHours:F0}h",
                    new Dictionary<string, object>
                    {
                        ["AgePercent"] = metrics.AgeUtilizationPercent,
                        ["OldestItemDate"] = metrics.OldestItemDate ?? DateTime.MinValue
                    }
                ));
            }
            else if (metrics.AgeUtilizationPercent >= _config.AgeWarningThresholdPercent)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.OldItemsAccumulating,
                    MemoryAlertSeverity.Warning,
                    $"Old items accumulating: {metrics.AgeUtilizationPercent:F1}% of max age",
                    $"Oldest item: {metrics.OldestItemDate:yyyy-MM-dd}, configured max age: {metrics.ConfiguredMaxAgeHours:F0}h",
                    new Dictionary<string, object>
                    {
                        ["AgePercent"] = metrics.AgeUtilizationPercent,
                        ["OldestItemDate"] = metrics.OldestItemDate ?? DateTime.MinValue
                    }
                ));
            }
            
            // Check prune rate
            if (metrics.PruneRate > _config.HighPruneRateThreshold)
            {
                newAlerts.Add(CreateAlert(
                    MemoryAlertType.HighPruneRate,
                    MemoryAlertSeverity.Warning,
                    $"High prune rate detected: {metrics.PruneRate:F1} items/hour",
                    "May indicate data retention issues or excessive data creation",
                    new Dictionary<string, object>
                    {
                        ["PruneRate"] = metrics.PruneRate,
                        ["Threshold"] = _config.HighPruneRateThreshold
                    }
                ));
            }
            
            lock (_lock)
            {
                foreach (var alert in newAlerts)
                {
                    _activeAlerts.Add(alert);
                    OnAlertRaised?.Invoke(alert);
                }
            }
            
            return newAlerts;
        }
        
        /// <summary>
        /// Gets all active alerts.
        /// </summary>
        public List<MemoryAlert> GetActiveAlerts()
        {
            lock (_lock)
            {
                return new List<MemoryAlert>(_activeAlerts);
            }
        }
        
        /// <summary>
        /// Clears an alert by ID.
        /// </summary>
        public bool ClearAlert(Guid alertId)
        {
            lock (_lock)
            {
                var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
                if (alert != null)
                {
                    _activeAlerts.Remove(alert);
                    OnAlertCleared?.Invoke(alert);
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Clears all alerts.
        /// </summary>
        public void ClearAllAlerts()
        {
            lock (_lock)
            {
                var alertsToClear = new List<MemoryAlert>(_activeAlerts);
                _activeAlerts.Clear();
                foreach (var alert in alertsToClear)
                {
                    OnAlertCleared?.Invoke(alert);
                }
            }
        }
        
        private MemoryAlert CreateAlert(
            MemoryAlertType type, 
            MemoryAlertSeverity severity, 
            string message, 
            string details,
            Dictionary<string, object>? metadata = null)
        {
            return new MemoryAlert
            {
                Type = type,
                Severity = severity,
                Message = message,
                Details = details,
                Metadata = metadata ?? new Dictionary<string, object>()
            };
        }
    }
}
