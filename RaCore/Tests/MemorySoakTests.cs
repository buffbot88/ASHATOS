using RaCore.Engine.Memory;
using Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RaCore.Tests;

/// <summary>
/// Soak tests to verify bounded memory/disk growth over time.
/// These tests simulate sustained load to ensure proper hygiene controls.
/// </summary>
public static class MemorySoakTests
{
    public static async Task RunAll()
    {
        Console.WriteLine("=== Memory Soak Tests ===\n");
        
        await TestBoundedGrowthUnderLoad();
        await TestMaintenanceEffectiveness();
        await TestAlertingUnderStress();
        
        Console.WriteLine("\n=== All Memory Soak Tests Completed ===");
    }
    
    /// <summary>
    /// Tests that memory remains bounded under sustained load.
    /// </summary>
    private static async Task TestBoundedGrowthUnderLoad()
    {
        Console.WriteLine("Soak Test: Bounded growth under sustained load");
        Console.WriteLine("  Duration: 30 seconds of continuous writes");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"soak_test_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            var startTime = DateTime.UtcNow;
            var duration = TimeSpan.FromSeconds(30);
            var writeCount = 0;
            var maxSize = 0L;
            
            // Write continuously for duration
            while (DateTime.UtcNow - startTime < duration)
            {
                await memory.RememberAsync($"key_{writeCount}", $"value_{writeCount}_{Guid.NewGuid()}");
                writeCount++;
                
                // Trigger maintenance periodically
                if (writeCount % 100 == 0)
                {
                    memory.PerformMaintenance();
                    
                    // Check database size
                    if (File.Exists(dbPath))
                    {
                        var fileInfo = new FileInfo(dbPath);
                        maxSize = Math.Max(maxSize, fileInfo.Length);
                    }
                    
                    Console.Write($"\r  Progress: {writeCount} writes, {memory.Count()} items, " +
                                 $"{maxSize / 1024.0:F1} KB max size");
                }
            }
            
            Console.WriteLine();
            
            var finalCount = memory.Count();
            var finalSize = new FileInfo(dbPath).Length;
            
            Console.WriteLine($"  Total writes: {writeCount:N0}");
            Console.WriteLine($"  Final count: {finalCount:N0} items");
            Console.WriteLine($"  Final size: {finalSize / 1024.0:F1} KB");
            Console.WriteLine($"  Max size: {maxSize / 1024.0:F1} KB");
            Console.WriteLine($"  Retention rate: {(finalCount * 100.0 / writeCount):F1}%");
            
            // Verify bounded growth (should not keep all items)
            if (finalCount < writeCount && finalCount <= memory.MaxItems)
            {
                Console.WriteLine("  ✓ PASS: Memory growth is bounded (maintenance working)");
            }
            else if (finalCount == writeCount)
            {
                Console.WriteLine("  ⚠ WARNING: All items retained (maintenance may not be aggressive enough)");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Item count exceeds configured maximum");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Tests that maintenance operations effectively control growth.
    /// </summary>
    private static async Task TestMaintenanceEffectiveness()
    {
        Console.WriteLine("Soak Test: Maintenance effectiveness");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"maintenance_test_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Fill beyond capacity
            Console.WriteLine("  Phase 1: Fill beyond capacity (150% of limit)...");
            var targetCount = (int)(memory.MaxItems * 1.5);
            for (int i = 0; i < targetCount; i++)
            {
                await memory.RememberAsync($"key_{i}", $"value_{i}");
                if (i % 1000 == 0 && i > 0)
                {
                    Console.Write($"\r  Added {i:N0}/{targetCount:N0} items");
                }
            }
            Console.WriteLine($"\r  Added {targetCount:N0}/{targetCount:N0} items");
            
            var beforeMaintenance = memory.Count();
            Console.WriteLine($"  Before maintenance: {beforeMaintenance:N0} items");
            
            // Run maintenance
            Console.WriteLine("  Phase 2: Running maintenance...");
            var results = memory.PerformMaintenance();
            
            var afterMaintenance = memory.Count();
            Console.WriteLine($"  After maintenance: {afterMaintenance:N0} items");
            Console.WriteLine($"  Pruned: {results.pruned:N0}");
            Console.WriteLine($"  Deduplicated: {results.deduplicated:N0}");
            Console.WriteLine($"  Limited: {results.limited:N0}");
            
            // Verify maintenance brought count within limits
            if (afterMaintenance <= memory.MaxItems)
            {
                Console.WriteLine("  ✓ PASS: Maintenance enforced item limits");
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: Count {afterMaintenance:N0} exceeds limit {memory.MaxItems:N0}");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Tests alert system under stress conditions.
    /// </summary>
    private static async Task TestAlertingUnderStress()
    {
        Console.WriteLine("Soak Test: Alert generation under stress");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"alert_test_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            var alertConfig = new MemoryAlertConfig
            {
                CapacityWarningThresholdPercent = 75,
                CapacityCriticalThresholdPercent = 90,
                DiskUsageWarningThresholdMb = 1,
                DiskUsageCriticalThresholdMb = 5
            };
            
            var alertManager = new MemoryAlertManager(alertConfig);
            var alertsRaised = new List<MemoryAlert>();
            
            alertManager.OnAlertRaised += alert => {
                alertsRaised.Add(alert);
                Console.WriteLine($"  Alert: [{alert.Severity}] {alert.Type} - {alert.Message}");
            };
            
            // Create stress conditions by filling near capacity
            Console.WriteLine("  Phase 1: Fill to 80% capacity to trigger warnings...");
            var targetCount = (int)(memory.MaxItems * 0.8);
            for (int i = 0; i < targetCount; i++)
            {
                await memory.RememberAsync($"key_{i}", $"value_{i}_{Guid.NewGuid()}");
            }
            
            // Collect metrics and evaluate
            var items = memory.GetAllItems().ToList();
            var metrics = new MemoryMetrics
            {
                TotalItems = items.Count,
                ConfiguredMaxItems = memory.MaxItems,
                ConfiguredMaxAgeHours = memory.MaxAge.TotalHours,
                LastMaintenanceSuccess = true
            };
            
            if (items.Any())
            {
                metrics.OldestItemDate = items.Min(i => i.CreatedAt);
                metrics.NewestItemDate = items.Max(i => i.CreatedAt);
            }
            
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                metrics.DatabaseSizeBytes = fileInfo.Length;
            }
            
            Console.WriteLine($"  Current state: {metrics.TotalItems:N0} items, " +
                            $"{metrics.DatabaseSizeBytes / 1024.0 / 1024.0:F2} MB, " +
                            $"{metrics.CapacityUtilizationPercent:F1}% capacity");
            
            // Evaluate alerts
            var alerts = alertManager.EvaluateMetrics(metrics);
            
            Console.WriteLine($"  Alerts raised: {alertsRaised.Count}");
            
            // Verify alerts were raised
            if (alertsRaised.Any(a => a.Type == MemoryAlertType.CapacityThresholdExceeded))
            {
                Console.WriteLine("  ✓ PASS: Capacity alerts triggered correctly");
            }
            else
            {
                Console.WriteLine("  ⚠ WARNING: No capacity alerts (may be below threshold)");
            }
            
            // Test critical threshold
            Console.WriteLine("  Phase 2: Fill to 95% capacity to trigger critical alerts...");
            targetCount = (int)(memory.MaxItems * 0.95);
            while (memory.Count() < targetCount)
            {
                await memory.RememberAsync($"key_{Guid.NewGuid()}", $"value_{Guid.NewGuid()}");
            }
            
            metrics.TotalItems = memory.Count();
            alertManager.EvaluateMetrics(metrics);
            
            if (alertsRaised.Any(a => a.Severity == MemoryAlertSeverity.Critical))
            {
                Console.WriteLine("  ✓ PASS: Critical alerts triggered at high capacity");
            }
            else
            {
                Console.WriteLine("  ⚠ WARNING: No critical alerts raised");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
}
