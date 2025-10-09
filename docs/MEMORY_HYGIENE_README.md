# Memory Hygiene System - Quick Reference

## Overview

The RaOS Memory Hygiene System provides comprehensive observability, metrics, alerting, and automatic maintenance for the memory management subsystem. This system ensures bounded memory growth, detects anomalies, and provides auditable evidence for security compliance (Security Gate #235).

## Quick Start

### 1. Basic Usage (Existing Features)

```csharp
using var memory = new MemoryModule();

// Store and recall
await memory.RememberAsync("key", "value");
var result = await memory.RecallAsync("key");

// Manual maintenance
memory.PerformMaintenance();
```

### 2. With Health Monitoring (New)

```csharp
services.AddSingleton<MemoryModule>();
services.AddSingleton<MemoryHealthMonitor>();
services.AddHostedService(sp => sp.GetRequiredService<MemoryHealthMonitor>());
services.AddHostedService<MemoryMaintenanceService>();
```

### 3. With Custom Alerts (New)

```csharp
var alertConfig = new MemoryAlertConfig
{
    CapacityCriticalThresholdPercent = 90.0,
    DiskUsageCriticalThresholdMb = 500
};
var monitor = new MemoryHealthMonitor(logger, memory, alertConfig);
monitor.GetAlertManager().OnAlertRaised += alert => 
{
    Console.WriteLine($"Alert: {alert.Message}");
};
```

## Key Components

### 1. MemoryModule (Enhanced)
**Location**: `RaCore/Engine/Memory/MemoryModule.cs`

Core memory storage with maintenance capabilities.

**New Features**:
- Methods now return operation counts
- Configuration accessors: `MaxAge`, `MaxItems`, `DatabasePath`

```csharp
int pruned = memory.PruneOldItems();
int deduped = memory.DeduplicateItems();
int limited = memory.EnforceItemLimit();
var (p, d, l) = memory.PerformMaintenance();
```

### 2. MemoryMetrics (New)
**Location**: `RaCore/Engine/Memory/MemoryMetrics.cs`

Comprehensive metrics tracking.

**Metrics Tracked**:
- Current state (items, size, capacity)
- Maintenance operations (counts, rates)
- Health indicators (utilization, success rates)

```csharp
var metrics = healthMonitor.GetCurrentMetrics();
Console.WriteLine(metrics.GetDetailedReport());
```

### 3. MemoryAlerts (New)
**Location**: `RaCore/Engine/Memory/MemoryAlerts.cs`

Configurable alerting system.

**Alert Types**:
- Capacity threshold exceeded
- Disk usage threshold exceeded
- Maintenance failure
- Old items accumulating
- High prune rate

```csharp
var alertManager = new MemoryAlertManager(alertConfig);
alertManager.OnAlertRaised += alert => { /* handle */ };
var alerts = alertManager.EvaluateMetrics(metrics);
```

### 4. MemoryHealthMonitor (New)
**Location**: `RaCore/Engine/Memory/MemoryHealthMonitor.cs`

Background service for continuous monitoring.

**Features**:
- Periodic health checks (default: 5 minutes)
- Automatic metric collection
- Alert evaluation
- Structured logging

```csharp
var monitor = new MemoryHealthMonitor(logger, memory, alertConfig);
var metrics = monitor.CheckHealthNow();
var alerts = monitor.GetActiveAlerts();
```

## Testing

### Test Suites

1. **MemoryManagementTests** - Basic functionality
2. **MemoryObservabilityTests** - Metrics and alerts  
3. **MemorySoakTests** - Long-running validation
4. **MemoryHygieneTestRunner** - Comprehensive suite

### Running Tests

```csharp
// Run all tests
await MemoryHygieneTestRunner.RunAll();

// Run specific suites
MemoryManagementTests.RunAll();
await MemoryObservabilityTests.RunAll();
await MemorySoakTests.RunAll();
```

## Documentation

| Document | Purpose |
|----------|---------|
| [MEMORY_MANAGEMENT.md](MEMORY_MANAGEMENT.md) | Basic memory features |
| [MEMORY_HYGIENE_OBSERVABILITY.md](MEMORY_HYGIENE_OBSERVABILITY.md) | Complete observability guide |
| [MEMORY_HYGIENE_EVIDENCE.md](MEMORY_HYGIENE_EVIDENCE.md) | Evidence collection template |
| [MEMORY_HYGIENE_INTEGRATION.md](MEMORY_HYGIENE_INTEGRATION.md) | Integration examples |

## Default Configuration

```csharp
// MemoryModule defaults
MaxAge: 90 days
MaxItems: 10,000
MaintenanceInterval: 24 hours

// Alert thresholds
CapacityWarning: 75%
CapacityCritical: 90%
DiskWarning: 100 MB
DiskCritical: 500 MB

// Health monitoring
CheckInterval: 5 minutes
```

## Security Gate #235 Compliance

This system provides all required evidence:

✅ **Automatic pruning/retention**: PruneOldItems() with 90-day default  
✅ **Storage quotas**: 10,000 item limit enforced  
✅ **Deduplication**: Automatic duplicate removal  
✅ **Metrics export**: Real-time metrics collection  
✅ **Alerting**: Configurable thresholds and notifications  
✅ **Soak tests**: Validates bounded growth over time  
✅ **Evidence collection**: Templates and automation  

See [MEMORY_HYGIENE_EVIDENCE.md](MEMORY_HYGIENE_EVIDENCE.md) for evidence collection checklist.

## Common Scenarios

### Check System Health

```csharp
var monitor = serviceProvider.GetRequiredService<MemoryHealthMonitor>();
var metrics = monitor.GetCurrentMetrics();

if (metrics.IsHealthy)
{
    Console.WriteLine("✓ System is healthy");
}
else
{
    Console.WriteLine("⚠ Attention needed:");
    foreach (var alert in monitor.GetActiveAlerts())
    {
        Console.WriteLine($"  - {alert.Message}");
    }
}
```

### Handle Critical Alert

```csharp
monitor.GetAlertManager().OnAlertRaised += alert =>
{
    if (alert.Severity == MemoryAlertSeverity.Critical)
    {
        logger.LogCritical("ALERT: {Message}", alert.Message);
        
        // Run emergency maintenance
        memory.PerformMaintenance();
        
        // Notify ops team
        SendAlert(alert);
    }
};
```

### Collect Evidence

```csharp
var metrics = monitor.GetCurrentMetrics();
var report = metrics.GetDetailedReport();
var alerts = monitor.GetActiveAlerts();

File.WriteAllText("evidence/metrics.txt", report);
File.WriteAllLines("evidence/alerts.txt", 
    alerts.Select(a => a.ToString()));
```

## Performance Impact

- **Metrics Collection**: ~1-2ms per check (every 5 minutes)
- **Alert Evaluation**: <1ms (threshold checks only)
- **Health Monitoring**: Background service, non-blocking
- **Maintenance**: Scales with item count (O(n))

## Troubleshooting

| Issue | Solution |
|-------|----------|
| High capacity alerts | Run `memory.PerformMaintenance()` |
| Failed maintenance | Check logs for error, verify DB permissions |
| No alerts raised | Verify alert config thresholds |
| High prune rate | Review data retention policy |

## Support

- **Issues**: Create issue with `memory-hygiene` label
- **Documentation**: See `docs/MEMORY_HYGIENE_*.md` files
- **Tests**: Run `MemoryHygieneTestRunner.RunAll()`
- **Evidence**: Follow `docs/MEMORY_HYGIENE_EVIDENCE.md`

---

**Status**: ✅ Ready for Security Gate #235 validation  
**Version**: 1.0  
**Last Updated**: 2024-12-20
