# Memory/Data Hygiene: Observability and Alerting

## Overview

This document describes the comprehensive observability, metrics collection, and alerting system for RaOS memory management, implemented to support Security Gate #235.

## Features Implemented

### 1. Metrics Collection and Tracking

The `MemoryMetrics` class provides comprehensive tracking of memory system health and performance:

#### Current State Metrics
- **Total Items**: Current number of items in memory store
- **Database Size**: Physical size of SQLite database in bytes
- **Oldest/Newest Item Dates**: Age range of stored items
- **Average Item Age**: Mean age of all items in hours
- **Capacity Utilization**: Percentage of configured maximum items in use
- **Age Utilization**: Percentage of maximum age threshold reached

#### Maintenance Operation Metrics
- **Items Pruned/Deduplicated/Limited**: Counts per maintenance cycle
- **Last Maintenance Time**: Timestamp of most recent maintenance
- **Maintenance Success/Failure**: Status and error tracking
- **Cumulative Totals**: Running totals of all maintenance operations
- **Operation Rates**: Items processed per hour (pruning, deduplication, storage)

#### Health Indicators
- **IsHealthy**: Boolean flag indicating overall system health
- **Configuration Limits**: Max items (10,000), max age (90 days)

### 2. Alert System

The `MemoryAlertManager` provides configurable threshold-based alerting:

#### Alert Types
- **CapacityThresholdExceeded**: Item count approaching or exceeding limits
- **DiskUsageThresholdExceeded**: Database size exceeding thresholds
- **MaintenanceFailure**: Failed maintenance cycle detected
- **OldItemsAccumulating**: Items approaching max age
- **HighPruneRate**: Unusually high pruning rate detected
- **UnusualGrowth**: Rapid growth detected

#### Alert Severities
- **Info**: Informational alerts
- **Warning**: Warning-level alerts (75-90% thresholds)
- **Critical**: Critical alerts (>90% thresholds)

#### Configurable Thresholds
```csharp
var config = new MemoryAlertConfig
{
    CapacityWarningThresholdPercent = 75.0,    // Warning at 75% capacity
    CapacityCriticalThresholdPercent = 90.0,   // Critical at 90% capacity
    DiskUsageWarningThresholdMb = 100,         // Warning at 100 MB
    DiskUsageCriticalThresholdMb = 500,        // Critical at 500 MB
    AgeWarningThresholdPercent = 80.0,         // Warning at 80% of max age
    AgeCriticalThresholdPercent = 95.0,        // Critical at 95% of max age
    HighPruneRateThreshold = 100.0,            // Alert if >100 items/hour pruned
    ConsecutiveFailuresBeforeAlert = 2,        // Alert after 2 consecutive failures
    UnusualGrowthRatePercent = 10.0            // Alert if >10% growth/hour
};
```

### 3. Health Monitoring Service

The `MemoryHealthMonitor` background service provides continuous monitoring:

#### Features
- **Continuous Health Checks**: Configurable interval (default: 5 minutes)
- **Automatic Metrics Collection**: Gathers metrics from memory module
- **Alert Evaluation**: Evaluates thresholds and raises alerts
- **Logging Integration**: Structured logging for all events
- **Event Hooks**: Subscribes to `MemoryDiagnostics` events

#### Usage
```csharp
// In Program.cs or Startup.cs
services.AddSingleton<MemoryModule>();
services.AddSingleton<MemoryHealthMonitor>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MemoryHealthMonitor>>();
    var memory = sp.GetRequiredService<MemoryModule>();
    var alertConfig = new MemoryAlertConfig(); // Use defaults or customize
    var checkInterval = TimeSpan.FromMinutes(5);
    var dbPath = memory.DatabasePath;
    
    return new MemoryHealthMonitor(logger, memory, alertConfig, checkInterval, dbPath);
});
services.AddHostedService(sp => sp.GetRequiredService<MemoryHealthMonitor>());

// Also add maintenance service with health monitor integration
services.AddHostedService<MemoryMaintenanceService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MemoryMaintenanceService>>();
    var memory = sp.GetRequiredService<MemoryModule>();
    var healthMonitor = sp.GetRequiredService<MemoryHealthMonitor>();
    
    return new MemoryMaintenanceService(logger, memory, TimeSpan.FromHours(24), healthMonitor);
});
```

### 4. Diagnostic Events

All memory operations emit diagnostic events via `MemoryDiagnostics`:

```csharp
// Subscribe to diagnostic events
MemoryDiagnostics.OnMemoryEvent += (message) => {
    Console.WriteLine($"[Memory] {message}");
};

MemoryDiagnostics.OnMemoryError += (exception) => {
    Console.WriteLine($"[Memory Error] {exception.Message}");
};

// Events emitted:
// - "Pruned X old memory items (older than YYYY-MM-DD)"
// - "Deduplicated X memory items"
// - "Enforced item limit: removed X oldest items"
// - "Memory maintenance completed"
// - "Alert: [Severity] AlertType - Message"
```

## API Reference

### MemoryModule Enhanced Methods

All maintenance methods now return operation counts:

```csharp
// Returns number of items pruned
int pruned = memory.PruneOldItems();
int pruned = memory.PruneOldItems(TimeSpan.FromDays(30));

// Returns number of items deduplicated
int deduplicated = memory.DeduplicateItems();

// Returns number of items removed by limit enforcement
int limited = memory.EnforceItemLimit();
int limited = memory.EnforceItemLimit(5000);

// Returns (pruned, deduplicated, limited) tuple
var (p, d, l) = memory.PerformMaintenance();
```

### Configuration Access

MemoryModule now exposes configuration for monitoring:

```csharp
TimeSpan maxAge = memory.MaxAge;      // e.g., 90 days
int maxItems = memory.MaxItems;        // e.g., 10,000
string dbPath = memory.DatabasePath;   // Path to SQLite database
```

### Health Monitor API

```csharp
// Get current metrics snapshot
MemoryMetrics metrics = healthMonitor.GetCurrentMetrics();

// Get active alerts
List<MemoryAlert> alerts = healthMonitor.GetActiveAlerts();

// Force immediate health check
MemoryMetrics metrics = healthMonitor.CheckHealthNow();

// Record maintenance cycle (called automatically by MemoryMaintenanceService)
healthMonitor.RecordMaintenanceCycle(
    pruned: 10,
    deduplicated: 5,
    limitRemoved: 2,
    success: true,
    error: null
);
```

### Metrics Reporting

```csharp
var metrics = healthMonitor.GetCurrentMetrics();

// Summary string
Console.WriteLine(metrics.ToString());
// Output: "MemoryMetrics: 7500 items, 5.00 MB, Capacity: 75.0%, Healthy: True, Last Maintenance: 2024-01-15 10:30:00"

// Detailed report
Console.WriteLine(metrics.GetDetailedReport());
// Output: Multi-line detailed report with all metrics
```

## Testing

### Test Suites

Three comprehensive test suites are provided:

1. **MemoryManagementTests** - Basic functionality tests
   - Pruning, deduplication, item limits
   - Automatic maintenance
   - Statistics reporting

2. **MemoryObservabilityTests** - Observability feature tests
   - Metrics collection
   - Alert evaluation
   - Health monitor integration
   - Metrics reporting

3. **MemorySoakTests** - Long-running validation tests
   - Bounded growth under sustained load
   - Maintenance effectiveness
   - Alert generation under stress

### Running Tests

```csharp
// Run basic tests
MemoryManagementTests.RunAll();

// Run observability tests
await MemoryObservabilityTests.RunAll();

// Run soak tests (30+ seconds)
await MemorySoakTests.RunAll();
```

## Monitoring Best Practices

### 1. Enable Health Monitoring in Production

Always run the `MemoryHealthMonitor` service in production environments for continuous observability.

### 2. Configure Appropriate Thresholds

Adjust alert thresholds based on your environment:
- **Low-traffic systems**: Use tighter thresholds (50%/70% warning/critical)
- **High-traffic systems**: Use looser thresholds (80%/95% warning/critical)
- **Storage-constrained systems**: Set lower disk usage thresholds

### 3. Subscribe to Diagnostic Events

Integrate with your logging/monitoring infrastructure:

```csharp
MemoryDiagnostics.OnMemoryEvent += (message) => {
    logger.LogInformation("[Memory] {Message}", message);
    // Send to monitoring system (e.g., Application Insights, DataDog)
};

// For alert manager events
alertManager.OnAlertRaised += (alert) => {
    logger.LogWarning("Alert raised: {Alert}", alert);
    // Send to alerting system (e.g., PagerDuty, OpsGenie)
};
```

### 4. Regular Metrics Review

Review the detailed metrics report at least daily:

```csharp
// Log detailed metrics report
var metrics = healthMonitor.GetCurrentMetrics();
logger.LogInformation("Daily metrics report:\n{Report}", metrics.GetDetailedReport());
```

### 5. Maintenance Scheduling

- Run maintenance during low-traffic periods
- Default interval: 24 hours
- Consider more frequent maintenance (12 hours) for high-traffic systems
- Monitor maintenance cycle success rate

## Performance Impact

The observability features are designed for minimal performance impact:

- **Metrics Collection**: O(n) where n = number of items (run every 5 minutes by default)
- **Alert Evaluation**: O(1) threshold checks on collected metrics
- **Health Monitoring**: Background service, non-blocking
- **Diagnostic Events**: Event-based, only incurs cost if subscribed

Recommended settings for minimal overhead:
- Health check interval: 5-10 minutes
- Maintenance interval: 12-24 hours
- Soak test duration: 30-60 seconds

## Troubleshooting

### High Memory Usage

1. Check metrics: `healthMonitor.GetCurrentMetrics()`
2. Review capacity utilization: Should be <90%
3. Check alerts: `healthMonitor.GetActiveAlerts()`
4. Force maintenance: `memory.PerformMaintenance()`
5. Adjust thresholds if needed: Lower `_maxItems` in MemoryModule

### Failed Maintenance Cycles

1. Check error message: `metrics.LastMaintenanceError`
2. Review logs for exceptions
3. Verify database file permissions
4. Check disk space availability
5. Test manual operations: `memory.PruneOldItems()`, etc.

### No Alerts Generated

1. Verify alert config thresholds are appropriate
2. Check if alerts are being cleared: `alertManager.GetActiveAlerts()`
3. Ensure health monitor is running
4. Subscribe to alert events: `alertManager.OnAlertRaised`

## Security Considerations

- All metrics are stored in-memory, not persisted
- Database path is exposed for monitoring purposes only
- No sensitive data is included in metrics or alerts
- Maintenance operations are atomic and transaction-safe
- Alert messages do not expose data content

## Evidence Collection for Security Gate

The observability system provides auditable evidence for Security Gate #235:

1. **Metrics History**: Continuous tracking proves bounded growth
2. **Alert Logs**: Demonstrate proactive monitoring
3. **Maintenance Records**: Show regular hygiene operations
4. **Soak Test Results**: Validate long-term stability
5. **Health Reports**: Periodic evidence of system health

See `MEMORY_HYGIENE_EVIDENCE.md` for evidence collection template.
