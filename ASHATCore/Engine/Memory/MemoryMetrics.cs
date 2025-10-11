using System;

namespace ASHATCore.Engine.Memory
{
    /// <summary>
    /// TASHATcks memory management metrics for observability and alerting.
    /// </summary>
    public class MemoryMetrics
    {
        // Current state metrics
        public int TotalItems { get; set; }
        public long DatabaseSizeBytes { get; set; }
        public DateTime? OldestItemDate { get; set; }
        public DateTime? NewestItemDate { get; set; }
        public double AverageItemAgeHours { get; set; }
        
        // Maintenance Operation metrics
        public int ItemsPrunedLastCycle { get; set; }
        public int ItemsDeduplicatedLastCycle { get; set; }
        public int ItemsRemovedByLimitLastCycle { get; set; }
        public DateTime? LastMaintenanceTime { get; set; }
        public bool LastMaintenanceSuccess { get; set; } = true;
        public string? LastMaintenanceError { get; set; }
        
        // Cumulative metrics (since service start)
        public long TotalItemsPruned { get; set; }
        public long TotalItemsDeduplicated { get; set; }
        public long TotalItemsRemovedByLimit { get; set; }
        public long TotalMaintenanceCycles { get; set; }
        public long FailedMaintenanceCycles { get; set; }
        
        // Rate metrics (items per hour)
        public double PruneRate { get; set; }
        public double DeduplicationRate { get; set; }
        public double StorageRate { get; set; }
        
        // Configuration limits
        public int ConfiguredMaxItems { get; set; }
        public double ConfiguredMaxAgeHours { get; set; }
        
        // Calculated health indicators
        public double CapacityUtilizationPercent => ConfiguredMaxItems > 0 
            ? (TotalItems * 100.0 / ConfiguredMaxItems) 
            : 0;
        
        public double AgeUtilizationPercent => ConfiguredMaxAgeHours > 0 && OldestItemDate.HasValue
            ? ((DateTime.UtcNow - OldestItemDate.Value).TotalHours * 100.0 / ConfiguredMaxAgeHours)
            : 0;
        
        public bool IsHealthy => CapacityUtilizationPercent < 90 && 
                                 AgeUtilizationPercent < 90 && 
                                 LastMaintenanceSuccess;
        
        public override string ToString()
        {
            return $"MemoryMetrics: {TotalItems} items, " +
                   $"{DatabaseSizeBytes / 1024.0 / 1024.0:F2} MB, " +
                   $"Capacity: {CapacityUtilizationPercent:F1}%, " +
                   $"Healthy: {IsHealthy}, " +
                   $"Last Maintenance: {LastMaintenanceTime?.ToString() ?? "Never"}";
        }
        
        /// <summary>
        /// Gets a detailed report of all metrics in human-readable format.
        /// </summary>
        public string GetDetailedReport()
        {
            return $@"=== Memory Metrics Report ===
Current State:
  Total Items: {TotalItems:N0}
  Database Size: {DatabaseSizeBytes / 1024.0 / 1024.0:F2} MB
  Capacity Utilization: {CapacityUtilizationPercent:F1}% ({TotalItems:N0}/{ConfiguredMaxItems:N0})
  Age Utilization: {AgeUtilizationPercent:F1}%
  Oldest Item: {OldestItemDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}
  Newest Item: {NewestItemDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}
  Average Age: {AverageItemAgeHours:F1} hours

Last Maintenance Cycle:
  Time: {LastMaintenanceTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}
  Status: {(LastMaintenanceSuccess ? "✓ Success" : "✗ Failed")}
  Items Pruned: {ItemsPrunedLastCycle:N0}
  Items Deduplicated: {ItemsDeduplicatedLastCycle:N0}
  Items Removed by Limit: {ItemsRemovedByLimitLastCycle:N0}
  {(string.IsNullOrEmpty(LastMaintenanceError) ? "" : $"Error: {LastMaintenanceError}")}

Cumulative Statistics:
  Total Maintenance Cycles: {TotalMaintenanceCycles:N0}
  Failed Cycles: {FailedMaintenanceCycles:N0}
  Total Items Pruned: {TotalItemsPruned:N0}
  Total Items Deduplicated: {TotalItemsDeduplicated:N0}
  Total Items Removed by Limit: {TotalItemsRemovedByLimit:N0}

Rates (per hour):
  Prune Rate: {PruneRate:F2}
  Deduplication Rate: {DeduplicationRate:F2}
  Storage Rate: {StorageRate:F2}

Health Status: {(IsHealthy ? "✓ HEALTHY" : "⚠ ATTENTION NEEDED")}
============================";
        }
    }
}
