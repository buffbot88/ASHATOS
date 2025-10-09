# Memory Hygiene System Integration Example

This example demonstrates how to integrate all memory hygiene components in a production application.

## Complete Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaCore.Engine.Memory;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // 1. Register MemoryModule as singleton
                services.AddSingleton<MemoryModule>(sp =>
                {
                    var dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "RaOS",
                        "memory.sqlite"
                    );
                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                    return new MemoryModule(dbPath);
                });

                // 2. Configure alert thresholds
                services.AddSingleton<MemoryAlertConfig>(sp =>
                {
                    return new MemoryAlertConfig
                    {
                        // Capacity thresholds
                        CapacityWarningThresholdPercent = 75.0,
                        CapacityCriticalThresholdPercent = 90.0,
                        
                        // Disk usage thresholds
                        DiskUsageWarningThresholdMb = 100,
                        DiskUsageCriticalThresholdMb = 500,
                        
                        // Age thresholds
                        AgeWarningThresholdPercent = 80.0,
                        AgeCriticalThresholdPercent = 95.0,
                        
                        // Rate thresholds
                        HighPruneRateThreshold = 100.0,
                        
                        // Failure threshold
                        ConsecutiveFailuresBeforeAlert = 2
                    };
                });

                // 3. Register MemoryHealthMonitor
                services.AddSingleton<MemoryHealthMonitor>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<MemoryHealthMonitor>>();
                    var memory = sp.GetRequiredService<MemoryModule>();
                    var alertConfig = sp.GetRequiredService<MemoryAlertConfig>();
                    
                    var monitor = new MemoryHealthMonitor(
                        logger,
                        memory,
                        alertConfig,
                        TimeSpan.FromMinutes(5),  // Check every 5 minutes
                        memory.DatabasePath
                    );
                    
                    // Subscribe to alert events
                    monitor.GetAlertManager().OnAlertRaised += alert =>
                    {
                        // Send to external monitoring system
                        logger.LogWarning("ALERT: {Alert}", alert);
                        // e.g., SendToPagerDuty(alert);
                        // e.g., SendToSlack(alert);
                    };
                    
                    return monitor;
                });
                
                // 4. Start HealthMonitor as background service
                services.AddHostedService(sp => sp.GetRequiredService<MemoryHealthMonitor>());

                // 5. Register MemoryMaintenanceService with health monitor integration
                services.AddHostedService<MemoryMaintenanceService>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<MemoryMaintenanceService>>();
                    var memory = sp.GetRequiredService<MemoryModule>();
                    var healthMonitor = sp.GetRequiredService<MemoryHealthMonitor>();
                    
                    return new MemoryMaintenanceService(
                        logger,
                        memory,
                        TimeSpan.FromHours(24),  // Run maintenance every 24 hours
                        healthMonitor
                    );
                });
                
                // 6. Subscribe to diagnostic events
                MemoryDiagnostics.OnMemoryEvent += message =>
                {
                    Console.WriteLine($"[Memory] {message}");
                };
                
                MemoryDiagnostics.OnMemoryError += exception =>
                {
                    Console.WriteLine($"[Memory Error] {exception.Message}");
                };
            })
            .Build();

        await host.RunAsync();
    }
}
```

## Quick API Usage Examples

### Storing and Recalling Data

```csharp
var memory = serviceProvider.GetRequiredService<MemoryModule>();

// Store data
await memory.RememberAsync("user_preference", "dark_mode");
await memory.RememberAsync("last_login", DateTime.UtcNow.ToString("o"));

// Recall data
var result = await memory.RecallAsync("user_preference");
Console.WriteLine(result.Text); // Output: Memory recall: "user_preference" = "dark_mode".

// Get statistics
var stats = memory.GetStats();
Console.WriteLine(stats);
```

### Checking Health Status

```csharp
var healthMonitor = serviceProvider.GetRequiredService<MemoryHealthMonitor>();

// Get current metrics
var metrics = healthMonitor.GetCurrentMetrics();
Console.WriteLine($"Items: {metrics.TotalItems}");
Console.WriteLine($"Capacity: {metrics.CapacityUtilizationPercent:F1}%");
Console.WriteLine($"Healthy: {metrics.IsHealthy}");

// Get detailed report
var report = metrics.GetDetailedReport();
Console.WriteLine(report);

// Get active alerts
var alerts = healthMonitor.GetActiveAlerts();
foreach (var alert in alerts)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Message}");
}

// Force health check
var currentMetrics = healthMonitor.CheckHealthNow();
```

### Manual Maintenance Operations

```csharp
var memory = serviceProvider.GetRequiredService<MemoryModule>();

// Prune old items (older than 30 days)
int pruned = memory.PruneOldItems(TimeSpan.FromDays(30));
Console.WriteLine($"Pruned {pruned} items");

// Remove duplicates
int deduplicated = memory.DeduplicateItems();
Console.WriteLine($"Removed {deduplicated} duplicates");

// Enforce item limit (max 5000 items)
int limited = memory.EnforceItemLimit(5000);
Console.WriteLine($"Removed {limited} items to enforce limit");

// Run full maintenance
var (p, d, l) = memory.PerformMaintenance();
Console.WriteLine($"Maintenance: Pruned {p}, Deduplicated {d}, Limited {l}");
```

## Production Monitoring Integration

### Application Insights

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
var healthMonitor = serviceProvider.GetRequiredService<MemoryHealthMonitor>();

// Send metrics to Application Insights
void SendMetricsToAppInsights()
{
    var metrics = healthMonitor.GetCurrentMetrics();
    
    telemetryClient.TrackMetric("Memory.TotalItems", metrics.TotalItems);
    telemetryClient.TrackMetric("Memory.DatabaseSizeMB", metrics.DatabaseSizeBytes / 1024.0 / 1024.0);
    telemetryClient.TrackMetric("Memory.CapacityPercent", metrics.CapacityUtilizationPercent);
    telemetryClient.TrackMetric("Memory.PruneRate", metrics.PruneRate);
    
    if (!metrics.IsHealthy)
    {
        telemetryClient.TrackEvent("Memory.UnhealthyState", new Dictionary<string, string>
        {
            ["Capacity"] = $"{metrics.CapacityUtilizationPercent:F1}%",
            ["Items"] = metrics.TotalItems.ToString()
        });
    }
}

// Run periodically
var timer = new System.Threading.Timer(_ => SendMetricsToAppInsights(), null, 
    TimeSpan.Zero, TimeSpan.FromMinutes(5));
```

### Prometheus Metrics

```csharp
using Prometheus;

var itemGauge = Metrics.CreateGauge("memory_items_total", "Total items in memory store");
var capacityGauge = Metrics.CreateGauge("memory_capacity_percent", "Memory capacity utilization percentage");
var dbSizeGauge = Metrics.CreateGauge("memory_database_size_bytes", "Database size in bytes");
var healthGauge = Metrics.CreateGauge("memory_health_status", "Memory health status (1=healthy, 0=unhealthy)");

var healthMonitor = serviceProvider.GetRequiredService<MemoryHealthMonitor>();

// Update metrics periodically
void UpdatePrometheusMetrics()
{
    var metrics = healthMonitor.GetCurrentMetrics();
    
    itemGauge.Set(metrics.TotalItems);
    capacityGauge.Set(metrics.CapacityUtilizationPercent);
    dbSizeGauge.Set(metrics.DatabaseSizeBytes);
    healthGauge.Set(metrics.IsHealthy ? 1 : 0);
}

// Expose metrics endpoint
app.UseMetricServer(); // Exposes /metrics endpoint
```

### Custom Alerting (Slack, PagerDuty, etc.)

```csharp
var alertManager = healthMonitor.GetAlertManager();

alertManager.OnAlertRaised += async alert =>
{
    // Send to Slack
    await SendSlackNotification(new SlackMessage
    {
        Channel = "#alerts",
        Username = "Memory Monitor",
        Text = $"🚨 {alert.Severity}: {alert.Message}",
        Attachments = new[]
        {
            new SlackAttachment
            {
                Color = alert.Severity == MemoryAlertSeverity.Critical ? "danger" : "warning",
                Fields = new[]
                {
                    new SlackField { Title = "Type", Value = alert.Type.ToString() },
                    new SlackField { Title = "Details", Value = alert.Details },
                    new SlackField { Title = "Time", Value = alert.Timestamp.ToString("o") }
                }
            }
        }
    });
    
    // Send to PagerDuty for critical alerts
    if (alert.Severity == MemoryAlertSeverity.Critical)
    {
        await SendPagerDutyIncident(new PagerDutyIncident
        {
            IncidentKey = $"memory-{alert.Type}",
            Description = alert.Message,
            Details = alert.Details
        });
    }
};
```

## Evidence Collection for Security Gate

### Automated Evidence Collection

```csharp
public class EvidenceCollectionService : BackgroundService
{
    private readonly MemoryHealthMonitor _healthMonitor;
    private readonly ILogger<EvidenceCollectionService> _logger;
    private readonly string _evidencePath;
    
    public EvidenceCollectionService(
        MemoryHealthMonitor healthMonitor,
        ILogger<EvidenceCollectionService> logger)
    {
        _healthMonitor = healthMonitor;
        _logger = logger;
        _evidencePath = Path.Combine(AppContext.BaseDirectory, "evidence");
        Directory.CreateDirectory(_evidencePath);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Collect evidence daily at midnight
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.Date.AddDays(1);
            var delay = tomorrow - now;
            
            await Task.Delay(delay, stoppingToken);
            
            await CollectDailyEvidence();
        }
    }
    
    private async Task CollectDailyEvidence()
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        // 1. Collect metrics report
        var metrics = _healthMonitor.GetCurrentMetrics();
        var reportPath = Path.Combine(_evidencePath, $"metrics_{date}.txt");
        await File.WriteAllTextAsync(reportPath, metrics.GetDetailedReport());
        
        // 2. Collect active alerts
        var alerts = _healthMonitor.GetActiveAlerts();
        var alertPath = Path.Combine(_evidencePath, $"alerts_{date}.txt");
        await File.WriteAllLinesAsync(alertPath, 
            alerts.Select(a => $"[{a.Timestamp:o}] [{a.Severity}] {a.Type}: {a.Message}"));
        
        // 3. Create summary
        var summaryPath = Path.Combine(_evidencePath, $"summary_{date}.txt");
        var summary = $@"Memory Hygiene Evidence Summary - {date}

Health Status: {(metrics.IsHealthy ? "✓ HEALTHY" : "⚠ ATTENTION NEEDED")}
Total Items: {metrics.TotalItems:N0}
Capacity: {metrics.CapacityUtilizationPercent:F1}%
Database Size: {metrics.DatabaseSizeBytes / 1024.0 / 1024.0:F2} MB
Active Alerts: {alerts.Count}

Maintenance Statistics:
  Total Cycles: {metrics.TotalMaintenanceCycles:N0}
  Failed Cycles: {metrics.FailedMaintenanceCycles:N0}
  Success Rate: {(metrics.TotalMaintenanceCycles > 0 ? (1 - (double)metrics.FailedMaintenanceCycles / metrics.TotalMaintenanceCycles) * 100 : 100):F1}%
  Total Pruned: {metrics.TotalItemsPruned:N0}
  Total Deduplicated: {metrics.TotalItemsDeduplicated:N0}
  Total Limited: {metrics.TotalItemsRemovedByLimit:N0}

For detailed information, see:
  - Metrics: {reportPath}
  - Alerts: {alertPath}
";
        await File.WriteAllTextAsync(summaryPath, summary);
        
        _logger.LogInformation("Daily evidence collected: {Path}", summaryPath);
    }
}

// Register in services
services.AddHostedService<EvidenceCollectionService>();
```

## Testing in Production

Run the comprehensive test suite periodically to validate the system:

```csharp
public class ProductionValidationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run weekly validation
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            
            try
            {
                // Run non-disruptive tests
                await RunValidationTests();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production validation failed");
            }
        }
    }
    
    private async Task RunValidationTests()
    {
        // Run observability tests (non-destructive)
        await MemoryObservabilityTests.RunAll();
        
        _logger.LogInformation("Production validation completed successfully");
    }
}
```

## Troubleshooting

### High Memory Alert

```csharp
// Immediate action when capacity alert is raised
alertManager.OnAlertRaised += async alert =>
{
    if (alert.Type == MemoryAlertType.CapacityThresholdExceeded && 
        alert.Severity == MemoryAlertSeverity.Critical)
    {
        _logger.LogCritical("CRITICAL: Memory capacity exceeded, running emergency maintenance");
        
        // Run emergency maintenance
        var (p, d, l) = memory.PerformMaintenance();
        
        _logger.LogInformation(
            "Emergency maintenance completed: Pruned {Pruned}, Deduplicated {Dedup}, Limited {Limited}",
            p, d, l);
    }
};
```

### Failed Maintenance

```csharp
// Monitor for maintenance failures
var metrics = healthMonitor.GetCurrentMetrics();
if (!metrics.LastMaintenanceSuccess)
{
    _logger.LogError("Maintenance failed: {Error}", metrics.LastMaintenanceError);
    
    // Try manual recovery
    try
    {
        var (p, d, l) = memory.PerformMaintenance();
        _logger.LogInformation("Manual maintenance succeeded");
    }
    catch (Exception ex)
    {
        _logger.LogCritical(ex, "Manual maintenance also failed, escalating");
        // Send critical alert to ops team
    }
}
```

## Configuration Recommendations

### Low-Traffic System (< 1000 items/day)
```csharp
MaxItems: 5000
MaxAge: 180 days
MaintenanceInterval: 24 hours
HealthCheckInterval: 10 minutes
CapacityWarning: 60%
CapacityCritical: 80%
```

### Medium-Traffic System (1000-10000 items/day)
```csharp
MaxItems: 10000
MaxAge: 90 days
MaintenanceInterval: 12 hours
HealthCheckInterval: 5 minutes
CapacityWarning: 75%
CapacityCritical: 90%
```

### High-Traffic System (> 10000 items/day)
```csharp
MaxItems: 20000
MaxAge: 30 days
MaintenanceInterval: 6 hours
HealthCheckInterval: 3 minutes
CapacityWarning: 80%
CapacityCritical: 95%
```
