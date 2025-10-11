using ASHATCore.Engine.Memory;
using Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for memory metrics, health monitoring, and observability features.
/// </summary>
public static class MemoryObservabilityTests
{
    public static async Task RunAll()
    {
        Console.WriteLine("=== Memory Observability Tests ===\n");
        
        TestMetricsCollection();
        TestAlertEvaluation();
        await TestHealthMonitorintegration();
        TestMetricsReporting();
        
        Console.WriteLine("\n=== All Memory Observability Tests Completed ===");
    }
    
    private static void TestMetricsCollection()
    {
        Console.WriteLine("Test: Metrics collection");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"metrics_test_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add test data
            for (int i = 0; i < 50; i++)
            {
                memory.RememberAsync($"key{i}", $"value{i}").Wait();
            }
            
            // Collect metrics manually
            var items = memory.GetAllItems().ToList();
            var metrics = new MemoryMetrics
            {
                TotalItems = items.Count,
                ConfiguredMaxItems = memory.MaxItems,
                ConfiguredMaxAgeHours = memory.MaxAge.TotalHours,
                LastMaintenanceSuccess = true,
                LastMaintenanceTime = DateTime.UtcNow
            };
            
            if (items.Any())
            {
                metrics.OldestItemDate = items.Min(i => i.CreatedAt);
                metrics.NewestItemDate = items.Max(i => i.CreatedAt);
                metrics.AverageItemAgeHours = items.Average(i => (DateTime.UtcNow - i.CreatedAt).TotalHours);
            }
            
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                metrics.DatabaseSizeBytes = fileInfo.Length;
            }
            
            Console.WriteLine($"  Collected metrics: {metrics}");
            Console.WriteLine($"  Total items: {metrics.TotalItems}");
            Console.WriteLine($"  Database size: {metrics.DatabaseSizeBytes / 1024.0:F1} KB");
            Console.WriteLine($"  Capacity utilization: {metrics.CapacityUtilizationPercent:F1}%");
            Console.WriteLine($"  Health status: {(metrics.IsHealthy ? "✓ Healthy" : "⚠ Unhealthy")}");
            
            if (metrics.TotalItems == 50 && metrics.IsHealthy)
            {
                Console.WriteLine("  ✓ PASS: Metrics collected correctly");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Metrics collection issue");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
    
    private static void TestAlertEvaluation()
    {
        Console.WriteLine("Test: Alert evaluation");
        
        var config = new MemoryAlertConfig
        {
            CapacityWarningThresholdPercent = 50,
            CapacityCriticalThresholdPercent = 75,
            DiskUsageWarningThresholdMb = 1,
            DiskUsageCriticalThresholdMb = 5
        };
        
        var alertManager = new MemoryAlertManager(config);
        var alertsRaised = new List<MemoryAlert>();
        
        alertManager.OnAlertRaised += alert => alertsRaised.Add(alert);
        
        // Test warning threshold
        var metrics = new MemoryMetrics
        {
            TotalItems = 6000,
            ConfiguredMaxItems = 10000,
            DatabaseSizeBytes = 2 * 1024 * 1024, // 2 MB
            LastMaintenanceSuccess = true
        };
        
        var alerts = alertManager.EvaluateMetrics(metrics);
        Console.WriteLine($"  Scenario 1 (60% capacity): {alerts.Count} alerts");
        
        if (alerts.Any(a => a.Type == MemoryAlertType.CapacityThresholdExceeded && 
                           a.Severity == MemoryAlertSeverity.Warning))
        {
            Console.WriteLine("  ✓ PASS: Warning alert triggered at 60% capacity");
        }
        else
        {
            Console.WriteLine("  ✗ FAIL: Expected warning alert not triggered");
        }
        
        // Test critical threshold
        alertManager.CleaASHATllAlerts();
        alertsRaised.Clear();
        
        metrics.TotalItems = 8000; // 80% capacity
        alerts = alertManager.EvaluateMetrics(metrics);
        Console.WriteLine($"  Scenario 2 (80% capacity): {alerts.Count} alerts");
        
        if (alerts.Any(a => a.Type == MemoryAlertType.CapacityThresholdExceeded && 
                           a.Severity == MemoryAlertSeverity.Critical))
        {
            Console.WriteLine("  ✓ PASS: Critical alert triggered at 80% capacity");
        }
        else
        {
            Console.WriteLine("  ✗ FAIL: Expected critical alert not triggered");
        }
        
        // Test maintenance failure alert
        alertManager.CleaASHATllAlerts();
        alertsRaised.Clear();
        
        metrics.LastMaintenanceSuccess = false;
        metrics.LastMaintenanceError = "Test error";
        alerts = alertManager.EvaluateMetrics(metrics);
        Console.WriteLine($"  Scenario 3 (maintenance failure): {alerts.Count} alerts");
        
        if (alerts.Any(a => a.Type == MemoryAlertType.MaintenanceFailure))
        {
            Console.WriteLine("  ✓ PASS: Maintenance failure alert triggered");
        }
        else
        {
            Console.WriteLine("  ✗ FAIL: Expected maintenance failure alert not triggered");
        }
        
        Console.WriteLine();
    }
    
    private static async Task TestHealthMonitorintegration()
    {
        Console.WriteLine("Test: Health monitor integration");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"health_test_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Create a simple logger factory
            using var loggerFactory = LoggerFactory.Create(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            var logger = loggerFactory.CreateLogger<MemoryHealthMonitor>();
            var alertConfig = new MemoryAlertConfig
            {
                CapacityWarningThresholdPercent = 30,
                CapacityCriticalThresholdPercent = 50
            };
            
            var healthMonitor = new MemoryHealthMonitor(
                logger, 
                memory, 
                alertConfig, 
                TimeSpan.FromSeconds(1),
                dbPath);
            
            // Add some data
            for (int i = 0; i < 20; i++)
            {
                await memory.RememberAsync($"key{i}", $"value{i}");
            }
            
            // Manually check health
            var metrics = healthMonitor.CheckHealthNow();
            
            Console.WriteLine($"  Health check result: {metrics}");
            Console.WriteLine($"  Items: {metrics.TotalItems}");
            Console.WriteLine($"  Capacity: {metrics.CapacityUtilizationPercent:F1}%");
            Console.WriteLine($"  Healthy: {metrics.IsHealthy}");
            
            // Record a maintenance cycle
            healthMonitor.RecordMaintenanceCycle(5, 2, 1, true);
            
            var updatedMetrics = healthMonitor.GetCurrentMetrics();
            Console.WriteLine($"  After maintenance record:");
            Console.WriteLine($"    Pruned: {updatedMetrics.ItemsPrunedLastCycle}");
            Console.WriteLine($"    Deduplicated: {updatedMetrics.ItemsDeduplicatedLastCycle}");
            Console.WriteLine($"    Limited: {updatedMetrics.ItemsRemovedByLimitLastCycle}");
            Console.WriteLine($"    Total cycles: {updatedMetrics.TotalMaintenanceCycles}");
            
            if (updatedMetrics.TotalMaintenanceCycles == 1 &&
                updatedMetrics.ItemsPrunedLastCycle == 5 &&
                updatedMetrics.ItemsDeduplicatedLastCycle == 2)
            {
                Console.WriteLine("  ✓ PASS: Health monitor tracking working correctly");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Health monitor tracking issue");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
    
    private static void TestMetricsReporting()
    {
        Console.WriteLine("Test: Metrics reporting");
        
        var metrics = new MemoryMetrics
        {
            TotalItems = 7500,
            ConfiguredMaxItems = 10000,
            ConfiguredMaxAgeHours = 2160, // 90 days
            DatabaseSizeBytes = 5 * 1024 * 1024, // 5 MB
            OldestItemDate = DateTime.UtcNow.AddDays(-30),
            NewestItemDate = DateTime.UtcNow,
            AverageItemAgeHours = 360, // 15 days
            ItemsPrunedLastCycle = 150,
            ItemsDeduplicatedLastCycle = 25,
            ItemsRemovedByLimitLastCycle = 10,
            LastMaintenanceTime = DateTime.UtcNow.AddHours(-1),
            LastMaintenanceSuccess = true,
            TotalItemsPruned = 500,
            TotalItemsDeduplicated = 100,
            TotalItemsRemovedByLimit = 50,
            TotalMaintenanceCycles = 10,
            FailedMaintenanceCycles = 0,
            PruneRate = 50.0,
            DeduplicationRate = 10.0,
            StorageRate = 100.0
        };
        
        Console.WriteLine("  Summary report:");
        Console.WriteLine($"    {metrics}");
        
        Console.WriteLine("\n  Detailed report:");
        var report = metrics.GetDetailedReport();
        foreach (var line in report.Split('\n'))
        {
            Console.WriteLine($"    {line}");
        }
        
        if (report.Contains("7,500 items") && 
            report.Contains("5.00 MB") && 
            report.Contains("✓ HEALTHY"))
        {
            Console.WriteLine("  ✓ PASS: Metrics reporting working correctly");
        }
        else
        {
            Console.WriteLine("  ✗ FAIL: Metrics report formatting issue");
        }
        
        Console.WriteLine();
    }
}
