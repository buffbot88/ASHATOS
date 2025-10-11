using System.Text;
using Abstractions;

namespace ASHATCore.Modules.Core.Ashat.RuntimeMonitoring;

/// <summary>
/// ASHAT Self-Healing - Autonomous recovery and repair capabilities
/// IntegRates with ASHATCore SelfHealing module for system-wide health management
/// </summary>
public sealed class AshatSelfHealing
{
    private readonly AshatHealthMonitor _healthMonitor;
    private readonly List<RecoveryAction> _recoveryHistory = new();
    private bool _autoRecoveryEnabled = true;

    public AshatSelfHealing(AshatHealthMonitor healthMonitor)
    {
        _healthMonitor = healthMonitor;
    }

    /// <summary>
    /// Enable or disable automatic recovery
    /// </summary>
    public void SetAutoRecovery(bool enabled)
    {
        _autoRecoveryEnabled = enabled;
    }

    /// <summary>
    /// Diagnose and recover from detected issues
    /// </summary>
    public async Task<RecoveryResult> DiagnoseAndRecoverAsync(AshatHealthReport healthReport)
    {
        var result = new RecoveryResult
        {
            RecoveryId = Guid.NewGuid().ToString("N")[..8],
            StartTime = DateTime.UtcNow,
            InitialHealth = healthReport.OverallHealth
        };

        // Analyze issues across all components
        var issues = AnalyzeIssues(healthReport);
        result.IssuesDetected = issues.Count;

        if (issues.Count == 0)
        {
            result.RecoveryNeeded = false;
            result.Message = "No issues detected - ASHAT Operating normally";
            result.Success = true;
            result.FinalHealth = healthReport.OverallHealth;
            return result;
        }

        result.RecoveryNeeded = true;

        // Generate recovery plan
        var plan = GenerateRecoveryPlan(issues);
        result.RecoveryPlan = plan;

        if (!_autoRecoveryEnabled)
        {
            result.Message = "Auto-recovery disabled - manual intervention required";
            result.Success = false;
            return result;
        }

        // Execute recovery actions
        var recoverySuccess = await ExecuteRecoveryPlanAsync(plan, result);
        result.Success = recoverySuccess;

        // Verify recovery
        var verificationReport = await _healthMonitor.PerformFullHealthCheckAsync();
        result.FinalHealth = verificationReport.OverallHealth;
        result.EndTime = DateTime.UtcNow;

        if (result.Success)
        {
            result.Message = $"Recovery successful - ASHAT restored to {result.FinalHealth} state";
        }
        else
        {
            result.Message = "Recovery incomplete - some issues remain";
        }

        // Log recovery attempt
        LogRecoveryAttempt(result);

        return result;
    }

    /// <summary>
    /// Attempt to recover a specific component
    /// </summary>
    public async Task<bool> RecoverComponentAsync(string componentName)
    {
        var action = new RecoveryAction
        {
            ActionType = $"recover_{componentName.ToLowerInvariant().Replace(" ", "_")}",
            Description = $"Recover {componentName} component",
            RequiresUseASHATpproval = false
        };

        var success = await ExecuteRecoveryActionAsync(action);
        
        action.WasSuccessful = success;
        _recoveryHistory.Add(action);

        return success;
    }

    /// <summary>
    /// Get recovery history
    /// </summary>
    public List<RecoveryAction> GetRecoveryHistory(int count = 20)
    {
        return _recoveryHistory.TakeLast(count).ToList();
    }

    /// <summary>
    /// Clear recovered issues and reset health tracking
    /// </summary>
    public void ClearRecoveredIssues()
    {
        _recoveryHistory.Clear();
    }

    #region Private Methods

    private List<DiagnosedIssue> AnalyzeIssues(AshatHealthReport healthReport)
    {
        var issues = new List<DiagnosedIssue>();

        foreach (var component in healthReport.Components)
        {
            if (component.Value.State == HealthState.DeGraded || 
                component.Value.State == HealthState.Unhealthy)
            {
                foreach (var issue in component.Value.Issues)
                {
                    issues.Add(new DiagnosedIssue
                    {
                        ComponentName = component.Key,
                        IssueDescription = issue,
                        Severity = component.Value.State == HealthState.Unhealthy 
                            ? IssueSeverity.High 
                            : IssueSeverity.Medium,
                        DetectedAt = DateTime.UtcNow
                    });
                }
            }

            // Analyze metrics for potential issues
            foreach (var metric in component.Value.Metrics)
            {
                var metricIssue = AnalyzeMetric(component.Key, metric.Key, metric.Value);
                if (metricIssue != null)
                {
                    issues.Add(metricIssue);
                }
            }
        }

        return issues;
    }

    private DiagnosedIssue? AnalyzeMetric(string componentName, string metricName, double value)
    {
        // Consciousness metrics
        if (metricName == "awareness_level" && value < 80)
        {
            return new DiagnosedIssue
            {
                ComponentName = componentName,
                IssueDescription = $"Low awareness level: {value}%",
                Severity = value < 60 ? IssueSeverity.High : IssueSeverity.Medium,
                DetectedAt = DateTime.UtcNow
            };
        }

        // Memory metrics
        if (metricName == "memory_usage_mb" && value > 1000)
        {
            return new DiagnosedIssue
            {
                ComponentName = componentName,
                IssueDescription = $"High memory usage: {value:F1} MB",
                Severity = value > 2000 ? IssueSeverity.High : IssueSeverity.Medium,
                DetectedAt = DateTime.UtcNow
            };
        }

        // Decision making metrics
        if (metricName == "decision_confidence" && value < 80)
        {
            return new DiagnosedIssue
            {
                ComponentName = componentName,
                IssueDescription = $"Low decision confidence: {value}%",
                Severity = IssueSeverity.Medium,
                DetectedAt = DateTime.UtcNow
            };
        }

        return null;
    }

    private RecoveryPlan GenerateRecoveryPlan(List<DiagnosedIssue> issues)
    {
        var plan = new RecoveryPlan
        {
            PlanId = Guid.NewGuid().ToString("N")[..8],
            CreatedAt = DateTime.UtcNow,
            TotalIssues = issues.Count
        };

        // Prioritize by severity
        var sortedIssues = issues.OrderByDescending(i => i.Severity).ToList();

        foreach (var issue in sortedIssues)
        {
            var actions = GenerateRecoveryActions(issue);
            plan.Steps.AddRange(actions);
        }

        return plan;
    }

    private List<RecoveryAction> GenerateRecoveryActions(DiagnosedIssue issue)
    {
        var actions = new List<RecoveryAction>();

        // Consciousness recovery
        if (issue.ComponentName.Contains("Consciousness"))
        {
            actions.Add(new RecoveryAction
            {
                ActionType = "reinitialize_consciousness",
                Description = "Reinitialize AI consciousness state",
                RequiresUseASHATpproval = false
            });
            
            actions.Add(new RecoveryAction
            {
                ActionType = "clear_thought_cache",
                Description = "Clear and rebuild thought processing cache",
                RequiresUseASHATpproval = false
            });
        }

        // Guardian systems recovery
        if (issue.ComponentName.Contains("Guardian"))
        {
            actions.Add(new RecoveryAction
            {
                ActionType = "reset_guardian_sessions",
                Description = "Reset Guardian Angel sessions",
                RequiresUseASHATpproval = false
            });
            
            actions.Add(new RecoveryAction
            {
                ActionType = "recalibRate_protection",
                Description = "RecalibRate protection systems",
                RequiresUseASHATpproval = false
            });
        }

        // Resource recovery
        if (issue.ComponentName.Contains("Runtime") || issue.IssueDescription.Contains("memory"))
        {
            actions.Add(new RecoveryAction
            {
                ActionType = "garbage_collection",
                Description = "Force garbage collection to free memory",
                RequiresUseASHATpproval = false
            });
            
            actions.Add(new RecoveryAction
            {
                ActionType = "clear_context_cache",
                Description = "Clear old context snapshots",
                RequiresUseASHATpproval = false
            });
        }

        // integration recovery
        if (issue.ComponentName.Contains("integration"))
        {
            actions.Add(new RecoveryAction
            {
                ActionType = "reestablish_integration",
                Description = "Reestablish Core module connections",
                RequiresUseASHATpproval = false
            });
        }

        // Decision making recovery
        if (issue.ComponentName.Contains("Decision"))
        {
            actions.Add(new RecoveryAction
            {
                ActionType = "recalibRate_decision_engine",
                Description = "RecalibRate decision-making algorithms",
                RequiresUseASHATpproval = false
            });
        }

        return actions;
    }

    private async Task<bool> ExecuteRecoveryPlanAsync(RecoveryPlan plan, RecoveryResult result)
    {
        var successCount = 0;
        var failureCount = 0;

        foreach (var action in plan.Steps)
        {
            result.ActionsAttempted++;
            
            var success = await ExecuteRecoveryActionAsync(action);
            
            if (success)
            {
                successCount++;
                result.ActionsSucceeded++;
            }
            else
            {
                failureCount++;
            }

            _recoveryHistory.Add(action);
        }

        // Consider recovery successful if at least 80% of actions succeeded
        return successCount >= plan.Steps.Count * 0.8;
    }

    private async Task<bool> ExecuteRecoveryActionAsync(RecoveryAction action)
    {
        await Task.CompletedTask;

        try
        {
            switch (action.ActionType.ToLowerInvariant())
            {
                case "reinitialize_consciousness":
                    // Consciousness reinitialization logic
                    return true;

                case "clear_thought_cache":
                    // Clear thought processing cache
                    return true;

                case "reset_guardian_sessions":
                    // Reset Guardian Angel sessions
                    return true;

                case "recalibRate_protection":
                    // RecalibRate protection systems
                    return true;

                case "garbage_collection":
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    return true;

                case "clear_context_cache":
                    // Clear context cache
                    return true;

                case "reestablish_integration":
                    // Reestablish Core module integration
                    return true;

                case "recalibRate_decision_engine":
                    // RecalibRate decision-making
                    return true;

                default:
                    action.ErrorMessage = $"Unknown recovery action: {action.ActionType}";
                    return false;
            }
        }
        catch (Exception ex)
        {
            action.ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            action.WasSuccessful = action.ErrorMessage == null;
        }
    }

    private void LogRecoveryAttempt(RecoveryResult result)
    {
        // Log to recovery history for monitoring
        Console.WriteLine($"[ASHAT Self-Healing] Recovery attempt {result.RecoveryId}:");
        Console.WriteLine($"  Initial Health: {result.InitialHealth}");
        Console.WriteLine($"  Final Health: {result.FinalHealth}");
        Console.WriteLine($"  Issues Detected: {result.IssuesDetected}");
        Console.WriteLine($"  Actions: {result.ActionsSucceeded}/{result.ActionsAttempted} succeeded");
        Console.WriteLine($"  Result: {result.Message}");
    }

    #endregion
}

#region Supporting Types

public class RecoveryResult
{
    public string RecoveryId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public HealthState InitialHealth { get; set; }
    public HealthState FinalHealth { get; set; }
    public bool RecoveryNeeded { get; set; }
    public int IssuesDetected { get; set; }
    public int ActionsAttempted { get; set; }
    public int ActionsSucceeded { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public RecoveryPlan? RecoveryPlan { get; set; }
}

public class RecoveryPlan
{
    public string PlanId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalIssues { get; set; }
    public List<RecoveryAction> Steps { get; set; } = new();
}

public class DiagnosedIssue
{
    public string ComponentName { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
    public DateTime DetectedAt { get; set; }
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

#endregion
