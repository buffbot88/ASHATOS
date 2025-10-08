using System.Collections.Concurrent;
using System.Text;
using Abstractions;

namespace RaCore.Modules.Core.Ashat.RuntimeMonitoring;

/// <summary>
/// Runtime Monitoring Component for ASHAT
/// Integrates with SelfHealing module and provides AI-specific runtime diagnostics
/// Monitors AI consciousness, encryption integrity, and module health
/// </summary>
public sealed class AshatRuntimeMonitor
{
    private readonly ConcurrentQueue<MonitoringSnapshot> _snapshots = new();
    private readonly ConcurrentDictionary<string, MetricTimeSeries> _metrics = new();
    private ISelfHealingModule? _selfHealingModule;
    private DateTime _monitoringStarted = DateTime.UtcNow;
    private bool _isActive = true;

    /// <summary>
    /// Initializes the runtime monitor with SelfHealing integration
    /// </summary>
    public void Initialize(ISelfHealingModule? selfHealingModule)
    {
        _selfHealingModule = selfHealingModule;
        
        // Initialize metric time series
        _metrics["cpu_usage"] = new MetricTimeSeries("CPU Usage");
        _metrics["memory_usage"] = new MetricTimeSeries("Memory Usage");
        _metrics["consciousness_level"] = new MetricTimeSeries("Consciousness Level");
        _metrics["encryption_operations"] = new MetricTimeSeries("Encryption Operations");
        _metrics["player_interactions"] = new MetricTimeSeries("Player Interactions");
        
        Console.WriteLine("[AshatRuntimeMonitor] Initialized with SelfHealing integration");
    }

    /// <summary>
    /// Records a monitoring snapshot
    /// </summary>
    public void RecordSnapshot(MonitoringSnapshot snapshot)
    {
        _snapshots.Enqueue(snapshot);
        
        // Update time series metrics
        _metrics["consciousness_level"].AddDataPoint(snapshot.ConsciousnessLevel);
        _metrics["encryption_operations"].AddDataPoint(snapshot.EncryptionOperationCount);
        _metrics["player_interactions"].AddDataPoint(snapshot.ActivePlayerCount);
        
        // Keep only last 1000 snapshots
        while (_snapshots.Count > 1000)
        {
            _snapshots.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Performs comprehensive health check
    /// </summary>
    public async Task<HealthCheckReport> PerformHealthCheckAsync()
    {
        var report = new HealthCheckReport
        {
            Timestamp = DateTime.UtcNow,
            MonitoringUptime = DateTime.UtcNow - _monitoringStarted
        };

        // Check SelfHealing module integration
        if (_selfHealingModule != null)
        {
            try
            {
                var selfHealingStatus = await _selfHealingModule.PerformSelfCheckAsync();
                report.SelfHealingStatus = selfHealingStatus.State.ToString();
                report.Checks.Add("SelfHealing Integration", true);
            }
            catch (Exception ex)
            {
                report.Checks.Add("SelfHealing Integration", false);
                report.Issues.Add($"SelfHealing check failed: {ex.Message}");
            }
        }
        else
        {
            report.Checks.Add("SelfHealing Integration", false);
            report.Issues.Add("SelfHealing module not connected");
        }

        // Check monitoring activity
        report.Checks.Add("Monitoring Active", _isActive);
        
        // Check snapshot collection
        var hasRecentSnapshots = _snapshots.Any(s => (DateTime.UtcNow - s.Timestamp).TotalMinutes < 10);
        report.Checks.Add("Recent Snapshots", hasRecentSnapshots);
        if (!hasRecentSnapshots)
        {
            report.Issues.Add("No snapshots recorded in the last 10 minutes");
        }

        // Check metrics
        foreach (var metric in _metrics.Values)
        {
            var metricHealth = metric.DataPoints.Count > 0;
            report.Checks.Add($"Metric: {metric.Name}", metricHealth);
        }

        // Overall health determination
        report.IsHealthy = report.Issues.Count == 0 && report.Checks.Values.Count(v => v) >= report.Checks.Count * 0.8;

        return report;
    }

    /// <summary>
    /// Detects anomalies in AI behavior or performance
    /// </summary>
    public AnomalyDetectionResult DetectAnomalies()
    {
        var result = new AnomalyDetectionResult
        {
            Timestamp = DateTime.UtcNow
        };

        var recentSnapshots = _snapshots
            .Where(s => (DateTime.UtcNow - s.Timestamp).TotalMinutes < 30)
            .ToList();

        if (recentSnapshots.Count < 5)
        {
            result.Warnings.Add("Insufficient data for anomaly detection");
            return result;
        }

        // Check for consciousness level drops
        var consciousnessLevels = recentSnapshots.Select(s => s.ConsciousnessLevel).ToList();
        if (consciousnessLevels.Any(level => level < 2))
        {
            result.Anomalies.Add(new Anomaly
            {
                Type = "Consciousness Degradation",
                Severity = AnomalySeverity.High,
                Description = "AI consciousness level has dropped below Active threshold",
                DetectedAt = DateTime.UtcNow
            });
        }

        // Check for encryption operation spikes
        var avgEncryption = recentSnapshots.Average(s => s.EncryptionOperationCount);
        var maxEncryption = recentSnapshots.Max(s => s.EncryptionOperationCount);
        if (maxEncryption > avgEncryption * 3)
        {
            result.Anomalies.Add(new Anomaly
            {
                Type = "Encryption Spike",
                Severity = AnomalySeverity.Medium,
                Description = $"Encryption operations exceeded normal levels: {maxEncryption} vs avg {avgEncryption:F1}",
                DetectedAt = DateTime.UtcNow
            });
        }

        // Check for memory pressure
        if (recentSnapshots.Any(s => s.MemoryUsageMB > 1024))
        {
            result.Anomalies.Add(new Anomaly
            {
                Type = "Memory Pressure",
                Severity = AnomalySeverity.Medium,
                Description = "Memory usage exceeds 1GB threshold",
                DetectedAt = DateTime.UtcNow
            });
        }

        return result;
    }

    /// <summary>
    /// Gets monitoring statistics and trends
    /// </summary>
    public string GetMonitoringStatistics()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("  ASHAT Runtime Monitoring Statistics");
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine();
        
        var uptime = DateTime.UtcNow - _monitoringStarted;
        sb.AppendLine($"Monitoring Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
        sb.AppendLine($"Snapshots Recorded: {_snapshots.Count}");
        sb.AppendLine($"Status: {(_isActive ? "ACTIVE ✓" : "INACTIVE")}");
        sb.AppendLine();
        
        sb.AppendLine("Metric Time Series:");
        foreach (var metric in _metrics.Values)
        {
            if (metric.DataPoints.Count > 0)
            {
                var avg = metric.DataPoints.Average();
                var min = metric.DataPoints.Min();
                var max = metric.DataPoints.Max();
                
                sb.AppendLine($"  {metric.Name}:");
                sb.AppendLine($"    Avg: {avg:F2} | Min: {min:F2} | Max: {max:F2}");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine($"SelfHealing Integration: {(_selfHealingModule != null ? "Connected ✓" : "Not Connected")}");
        sb.AppendLine("═══════════════════════════════════════");
        
        return sb.ToString();
    }

    /// <summary>
    /// Integrates with SelfHealing module for recovery actions
    /// </summary>
    public async Task<bool> IntegrateWithSelfHealingAsync(RecoveryAction action)
    {
        if (_selfHealingModule == null)
        {
            Console.WriteLine("[AshatRuntimeMonitor] SelfHealing module not available");
            return false;
        }

        try
        {
            var result = await _selfHealingModule.AttemptRecoveryAsync(action);
            Console.WriteLine($"[AshatRuntimeMonitor] Recovery action '{action.ActionType}' result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AshatRuntimeMonitor] Recovery action failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Generates diagnostic report
    /// </summary>
    public DiagnosticReport GenerateDiagnosticReport()
    {
        var report = new DiagnosticReport
        {
            GeneratedAt = DateTime.UtcNow,
            MonitoringUptime = DateTime.UtcNow - _monitoringStarted,
            TotalSnapshots = _snapshots.Count
        };

        // Recent activity summary
        var recentSnapshots = _snapshots
            .Where(s => (DateTime.UtcNow - s.Timestamp).TotalMinutes < 60)
            .ToList();

        if (recentSnapshots.Any())
        {
            report.RecentActivity.Add($"Snapshots in last hour: {recentSnapshots.Count}");
            report.RecentActivity.Add($"Avg Consciousness Level: {recentSnapshots.Average(s => s.ConsciousnessLevel):F1}");
            report.RecentActivity.Add($"Avg Player Interactions: {recentSnapshots.Average(s => s.ActivePlayerCount):F1}");
            report.RecentActivity.Add($"Total Encryption Ops: {recentSnapshots.Sum(s => s.EncryptionOperationCount)}");
        }

        // System recommendations
        if (_selfHealingModule == null)
        {
            report.Recommendations.Add("Connect SelfHealing module for enhanced monitoring");
        }

        if (recentSnapshots.Count < 10)
        {
            report.Recommendations.Add("Increase snapshot frequency for better monitoring");
        }

        var avgMemory = recentSnapshots.Any() ? recentSnapshots.Average(s => s.MemoryUsageMB) : 0;
        if (avgMemory > 512)
        {
            report.Recommendations.Add("Memory usage is elevated - consider optimization");
        }

        return report;
    }

    /// <summary>
    /// Stops monitoring
    /// </summary>
    public void Stop()
    {
        _isActive = false;
        Console.WriteLine("[AshatRuntimeMonitor] Monitoring stopped");
    }
}

#region Supporting Types

/// <summary>
/// Represents a point-in-time monitoring snapshot
/// </summary>
public class MonitoringSnapshot
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ConsciousnessLevel { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public int EncryptionOperationCount { get; set; }
    public int ActivePlayerCount { get; set; }
    public int ThoughtStreamDepth { get; set; }
}

/// <summary>
/// Health check report
/// </summary>
public class HealthCheckReport
{
    public DateTime Timestamp { get; set; }
    public TimeSpan MonitoringUptime { get; set; }
    public string SelfHealingStatus { get; set; } = "Unknown";
    public Dictionary<string, bool> Checks { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public bool IsHealthy { get; set; }
}

/// <summary>
/// Anomaly detection result
/// </summary>
public class AnomalyDetectionResult
{
    public DateTime Timestamp { get; set; }
    public List<Anomaly> Anomalies { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Represents a detected anomaly
/// </summary>
public class Anomaly
{
    public string Type { get; set; } = string.Empty;
    public AnomalySeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
}

public enum AnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Time series for metric tracking
/// </summary>
public class MetricTimeSeries
{
    public string Name { get; }
    public List<double> DataPoints { get; } = new();
    private readonly object _lock = new();

    public MetricTimeSeries(string name)
    {
        Name = name;
    }

    public void AddDataPoint(double value)
    {
        lock (_lock)
        {
            DataPoints.Add(value);
            
            // Keep only last 1000 data points
            if (DataPoints.Count > 1000)
            {
                DataPoints.RemoveAt(0);
            }
        }
    }
}

/// <summary>
/// Diagnostic report
/// </summary>
public class DiagnosticReport
{
    public DateTime GeneratedAt { get; set; }
    public TimeSpan MonitoringUptime { get; set; }
    public int TotalSnapshots { get; set; }
    public List<string> RecentActivity { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

#endregion
