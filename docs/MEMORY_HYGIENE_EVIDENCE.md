# Memory/Data Hygiene Evidence Collection

## Purpose

This document provides a template for collecting and documenting evidence that memory and data hygiene controls are actively working, observable, and effective. This evidence is required for Security Gate #235 validation.

## Evidence Checklist

Use this checklist to track evidence collection:

- [ ] Configuration documentation
- [ ] Metrics collection validation
- [ ] Alert system validation  
- [ ] Maintenance cycle logs
- [ ] Soak test results
- [ ] Health monitoring dashboard/logs
- [ ] Threshold enforcement validation
- [ ] Long-term trend analysis

## 1. Configuration Evidence

### Memory Module Configuration

**Location**: `RaCore/Engine/Memory/MemoryModule.cs`

**Current Settings**:
```csharp
// Default configuration values
MaxAge: 90 days (TimeSpan.FromDays(90))
MaxItems: 10,000 items
Database Path: {AppContext.BaseDirectory}/ra_memory.sqlite
```

**Evidence to Collect**:
- [ ] Screenshot or copy of configuration code
- [ ] Verification that limits are enforced in code
- [ ] Documentation of configuration rationale

**Verification Command**:
```csharp
using var memory = new MemoryModule();
Console.WriteLine($"Max Age: {memory.MaxAge.TotalDays} days");
Console.WriteLine($"Max Items: {memory.MaxItems}");
Console.WriteLine($"Database Path: {memory.DatabasePath}");
```

---

## 2. Metrics Collection Evidence

### Metrics Being Tracked

**Current State Metrics**:
- Total Items
- Database Size (bytes)
- Oldest/Newest Item Dates
- Average Item Age
- Capacity Utilization %
- Age Utilization %

**Maintenance Metrics**:
- Items Pruned (per cycle and cumulative)
- Items Deduplicated (per cycle and cumulative)
- Items Removed by Limit (per cycle and cumulative)
- Maintenance Success/Failure Rate
- Last Maintenance Timestamp

**Rate Metrics**:
- Prune Rate (items/hour)
- Deduplication Rate (items/hour)
- Storage Rate (items/hour)

**Evidence to Collect**:
- [ ] Sample metrics report from `GetDetailedReport()`
- [ ] Screenshot of metrics in logs
- [ ] Verification that all metrics are collected

**Sample Collection**:
```csharp
var healthMonitor = serviceProvider.GetRequiredService<MemoryHealthMonitor>();
var metrics = healthMonitor.GetCurrentMetrics();
var report = metrics.GetDetailedReport();
File.WriteAllText("evidence/metrics_report.txt", report);
Console.WriteLine(report);
```

**Expected Output**:
```
=== Memory Metrics Report ===
Current State:
  Total Items: 7,500
  Database Size: 5.00 MB
  Capacity Utilization: 75.0% (7,500/10,000)
  Age Utilization: 45.5%
  Oldest Item: 2024-11-15 10:30:00
  Newest Item: 2024-12-20 14:22:15
  Average Age: 450.2 hours

Last Maintenance Cycle:
  Time: 2024-12-20 00:00:00
  Status: ✓ Success
  Items Pruned: 150
  Items Deduplicated: 25
  Items Removed by Limit: 10

Cumulative Statistics:
  Total Maintenance Cycles: 30
  Failed Cycles: 0
  Total Items Pruned: 4,500
  Total Items Deduplicated: 750
  Total Items Removed by Limit: 300

Rates (per hour):
  Prune Rate: 6.25
  Deduplication Rate: 1.04
  Storage Rate: 10.42

Health Status: ✓ HEALTHY
============================
```

---

## 3. Alert System Evidence

### Alert Configuration

**Thresholds**:
```csharp
var alertConfig = new MemoryAlertConfig
{
    CapacityWarningThresholdPercent = 75.0,
    CapacityCriticalThresholdPercent = 90.0,
    DiskUsageWarningThresholdMb = 100,
    DiskUsageCriticalThresholdMb = 500,
    AgeWarningThresholdPercent = 80.0,
    AgeCriticalThresholdPercent = 95.0,
    HighPruneRateThreshold = 100.0,
    ConsecutiveFailuresBeforeAlert = 2,
    UnusualGrowthRatePercent = 10.0
};
```

**Evidence to Collect**:
- [ ] Alert configuration documentation
- [ ] Sample alert raised (warning level)
- [ ] Sample alert raised (critical level)
- [ ] Alert clearing logs
- [ ] Alert history over time period

**Alert Test**:
```csharp
// Run alert test from MemoryObservabilityTests
await MemoryObservabilityTests.RunAll();
// Capture output showing alerts being raised
```

**Expected Alerts**:
```
[Warning] CapacityThresholdExceeded: Memory capacity approaching limit: 75.5%
[Critical] CapacityThresholdExceeded: Memory capacity at critical level: 92.0%
[Critical] MaintenanceFailure: Memory maintenance cycle failed
[Warning] OldItemsAccumulating: Old items accumulating: 82.5% of max age
[Warning] HighPruneRate: High prune rate detected: 125.3 items/hour
```

---

## 4. Maintenance Cycle Evidence

### Maintenance Operations

**Frequency**: Every 24 hours (configurable)
**Operations**: Prune old items, deduplicate, enforce limits

**Evidence to Collect**:
- [ ] Maintenance service startup log
- [ ] Multiple maintenance cycle completion logs
- [ ] Before/after item counts
- [ ] Operations performed in each cycle
- [ ] Any failure logs and recovery

**Sample Log Collection**:
```bash
# Grep for maintenance logs
grep "Memory maintenance" application.log > evidence/maintenance_logs.txt
grep "Memory Maintenance Service" application.log >> evidence/maintenance_logs.txt
```

**Expected Logs**:
```
[INFO] Memory Maintenance Service started. Running every 24:00:00
[INFO] Starting scheduled memory maintenance...
[INFO] Memory maintenance completed. Items: 9,850 → 8,200 (pruned: 1,500, dedup: 100, limited: 50)
[INFO] Starting scheduled memory maintenance...
[INFO] Memory maintenance completed. Items: 9,100 → 7,950 (pruned: 1,050, dedup: 75, limited: 25)
```

---

## 5. Soak Test Results

### Test Configuration

**Duration**: 30 seconds continuous load
**Operations**: Continuous writes with periodic maintenance
**Goal**: Verify bounded growth

**Evidence to Collect**:
- [ ] Soak test output/logs
- [ ] Growth rate analysis
- [ ] Final vs initial counts
- [ ] Memory/disk usage trends

**Running Soak Tests**:
```csharp
await MemorySoakTests.RunAll();
```

**Expected Results**:
```
=== Memory Soak Tests ===

Soak Test: Bounded growth under sustained load
  Duration: 30 seconds of continuous writes
  Progress: 1500 writes, 850 items, 125.3 KB max size
  Total writes: 1,500
  Final count: 850 items
  Final size: 125.3 KB
  Max size: 125.3 KB
  Retention rate: 56.7%
  ✓ PASS: Memory growth is bounded (maintenance working)

Soak Test: Maintenance effectiveness
  Phase 1: Fill beyond capacity (150% of limit)...
  Added 15,000/15,000 items
  Before maintenance: 15,000 items
  Phase 2: Running maintenance...
  After maintenance: 10,000 items
  Pruned: 0
  Deduplicated: 0
  Limited: 5,000
  ✓ PASS: Maintenance enforced item limits

Soak Test: Alert generation under stress
  Phase 1: Fill to 80% capacity to trigger warnings...
  Current state: 8,000 items, 1.25 MB, 80.0% capacity
  Alert: [Warning] CapacityThresholdExceeded - Memory capacity approaching limit: 80.0%
  Alerts raised: 2
  ✓ PASS: Capacity alerts triggered correctly
  Phase 2: Fill to 95% capacity to trigger critical alerts...
  ✓ PASS: Critical alerts triggered at high capacity

=== All Memory Soak Tests Completed ===
```

---

## 6. Health Monitoring Evidence

### Monitoring Service

**Check Interval**: 5 minutes (configurable)
**Operations**: Collect metrics, evaluate alerts, log health status

**Evidence to Collect**:
- [ ] Health monitor startup log
- [ ] Multiple health check logs (healthy state)
- [ ] Health check logs showing warnings/issues
- [ ] Health status trends over time
- [ ] Recovery from unhealthy state

**Sample Logs**:
```
[INFO] Memory Health Monitor started. Checking every 00:05:00
[INFO] Initial health check: MemoryMetrics: 7500 items, 5.00 MB, Capacity: 75.0%, Healthy: True
[INFO] Health check: ✓ HEALTHY - MemoryMetrics: 7500 items, 5.00 MB, Capacity: 75.0%, Healthy: True
[INFO] Hourly metrics report:
=== Memory Metrics Report ===
[...]
[WARNING] Health check: ⚠ ATTENTION NEEDED - MemoryMetrics: 9200 items, 7.50 MB, Capacity: 92.0%, Healthy: False - 1 active alerts
[CRITICAL] ALERT RAISED: [Critical] CapacityThresholdExceeded: Memory capacity at critical level: 92.0%
[INFO] Health check: ✓ HEALTHY - MemoryMetrics: 8500 items, 6.80 MB, Capacity: 85.0%, Healthy: True
[INFO] ALERT CLEARED: CapacityThresholdExceeded
```

---

## 7. Deduplication Evidence

### Deduplication Operations

**Strategy**: Keep most recent, remove older duplicates
**Trigger**: During maintenance cycles

**Evidence to Collect**:
- [ ] Pre-deduplication count
- [ ] Post-deduplication count
- [ ] Number of duplicates removed
- [ ] Sample of deduplicated keys

**Test Evidence**:
```csharp
// Run from MemoryManagementTests
MemoryManagementTests.RunAll();
```

**Expected Output**:
```
Test: Deduplication
  Before deduplication: 4 items
  After deduplication: 2 items
  ✓ PASS: Duplicates removed correctly
```

---

## 8. Retention Policy Evidence

### Retention Configuration

**Max Age**: 90 days
**Enforcement**: During maintenance cycles
**Strategy**: Delete items older than threshold

**Evidence to Collect**:
- [ ] Pre-prune count and oldest item date
- [ ] Post-prune count and oldest item date
- [ ] Number of items pruned
- [ ] Verification that old items are removed

**Sample Evidence**:
```
Pruning Test Results:
  Initial count: 10,000 items
  Oldest item before: 2024-01-15 (120 days old)
  Prune operation: Items older than 90 days
  Items pruned: 2,500
  Final count: 7,500 items
  Oldest item after: 2024-09-20 (85 days old)
  ✓ Retention policy enforced successfully
```

---

## 9. Disk Usage Evidence

### Storage Monitoring

**Database File**: SQLite database
**Monitoring**: File size tracking in metrics

**Evidence to Collect**:
- [ ] Current database size
- [ ] Maximum observed database size
- [ ] Growth rate over time
- [ ] Verification of bounded growth

**Collection Script**:
```bash
# Monitor database size over time
ls -lh /path/to/ra_memory.sqlite
# or from application
du -h /path/to/ra_memory.sqlite
```

**Sample Output**:
```
Database Size Monitoring:
  Start: 0.5 MB
  After 1000 writes: 2.5 MB
  After maintenance: 1.8 MB
  After 2000 writes: 3.2 MB
  After maintenance: 2.1 MB
  Max observed: 3.2 MB
  ✓ Size remains bounded with maintenance
```

---

## 10. Evidence Summary Template

### Summary Report for Security Gate #235

**Date**: [YYYY-MM-DD]
**Reviewer**: [Name]
**System Version**: [Version]

#### Configuration Verified
- [x] Max age: 90 days configured and enforced
- [x] Max items: 10,000 configured and enforced
- [x] Automatic maintenance enabled (24-hour interval)
- [x] Health monitoring active (5-minute interval)

#### Metrics Collection Verified
- [x] All current state metrics collected
- [x] All maintenance operation metrics tracked
- [x] All rate metrics calculated
- [x] Metrics accessible via API and logs

#### Alerting Verified
- [x] Warning thresholds configured (75% capacity, 100 MB disk)
- [x] Critical thresholds configured (90% capacity, 500 MB disk)
- [x] Alerts raised correctly when thresholds exceeded
- [x] Alerts cleared when conditions resolve
- [x] Alert events logged and observable

#### Maintenance Verified
- [x] Pruning active and working (removed [X] old items)
- [x] Deduplication active and working (removed [X] duplicates)
- [x] Limit enforcement active and working (removed [X] excess items)
- [x] Maintenance cycles successful ([X]% success rate)
- [x] Failed cycles logged and alerted

#### Soak Testing Verified
- [x] 30-second soak test passed
- [x] Bounded growth verified (retention rate: [X]%)
- [x] Maintenance effectiveness verified
- [x] Alert generation under stress verified

#### Health Monitoring Verified
- [x] Continuous monitoring active
- [x] Health checks running every 5 minutes
- [x] Metrics collected and logged
- [x] Alerts evaluated and raised
- [x] System health status tracked

#### Evidence Attached
- [ ] `evidence/metrics_report.txt` - Sample metrics report
- [ ] `evidence/maintenance_logs.txt` - Maintenance cycle logs
- [ ] `evidence/soak_test_results.txt` - Soak test output
- [ ] `evidence/alert_logs.txt` - Sample alert logs
- [ ] `evidence/health_check_logs.txt` - Health monitoring logs
- [ ] `evidence/config_screenshot.png` - Configuration verification

**Conclusion**: 
All memory/data hygiene controls are active, observable, and working as designed. Evidence demonstrates bounded growth, effective maintenance, proper alerting, and continuous monitoring. System is ready for Security Gate #235 validation.

**Reviewer Signature**: ___________________
**Date**: ___________________

---

## Evidence Storage

Store collected evidence in the following structure:

```
evidence/
├── config/
│   ├── memory_module_config.txt
│   └── alert_config.txt
├── metrics/
│   ├── metrics_report_YYYY-MM-DD.txt
│   └── metrics_trends.csv
├── logs/
│   ├── maintenance_logs.txt
│   ├── alert_logs.txt
│   └── health_check_logs.txt
├── tests/
│   ├── soak_test_results.txt
│   ├── observability_test_results.txt
│   └── management_test_results.txt
└── summary/
    ├── evidence_summary.md
    └── security_gate_validation.pdf
```

## Automation Recommendations

Consider automating evidence collection:

```csharp
public class EvidenceCollector
{
    public async Task CollectDailyEvidence(MemoryHealthMonitor monitor)
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var metrics = monitor.GetCurrentMetrics();
        var alerts = monitor.GetActiveAlerts();
        
        // Save metrics report
        var reportPath = $"evidence/metrics/metrics_report_{date}.txt";
        await File.WriteAllTextAsync(reportPath, metrics.GetDetailedReport());
        
        // Save alert summary
        var alertPath = $"evidence/logs/alerts_{date}.txt";
        await File.WriteAllLinesAsync(alertPath, 
            alerts.Select(a => a.ToString()));
        
        // Log evidence collection
        Console.WriteLine($"Evidence collected: {reportPath}");
    }
}
```
