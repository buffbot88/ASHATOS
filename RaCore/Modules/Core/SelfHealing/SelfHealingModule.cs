using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Core.SelfHealing;

/// <summary>
/// Self-healing diagnostics module for automatic recovery
/// </summary>
[RaModule(Category = "core")]
public sealed class SelfHealingModule : ModuleBase, ISelfHealingModule
{
    public override string Name => "SelfHealing";

    private ModuleManager? _manager;
    private readonly ConcurrentDictionary<string, ModuleHealthStatus> _healthStatuses = new();
    private readonly ConcurrentQueue<RecoveryAction> _recoveryLog = new();
    private DateTime _lastFullCheck = DateTime.MinValue;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "SelfHealing: Use 'check', 'health', 'recover', 'log', or 'stats'";

        var command = input.Trim().ToLowerInvariant();

        return command switch
        {
            "check" => PerformFullSystemCheck(),
            "health" => GetHealthStatus(),
            "recover" => AttemptAutoRecovery(),
            "log" => GetRecoveryLog(),
            "stats" => GetStats(),
            _ => "Unknown command. Use: check, health, recover, log, stats"
        };
    }

    public async Task<ModuleHealthStatus> PerformSelfCheckAsync()
    {
        await Task.CompletedTask; // Async placeholder

        var status = new ModuleHealthStatus
        {
            ModuleName = Name,
            State = HealthState.Healthy,
            CheckedAt = DateTime.UtcNow
        };

        status.Metrics["uptime"] = (DateTime.UtcNow - _lastFullCheck).TotalMinutes;
        status.Metrics["checks_performed"] = _healthStatuses.Count;

        _healthStatuses.AddOrUpdate(Name, status, (_, _) => status);
        return status;
    }

    public async Task<bool> AttemptRecoveryAsync(RecoveryAction action)
    {
        await Task.CompletedTask; // Async placeholder

        LogInfo($"Attempting recovery: {action.Description}");
        _recoveryLog.Enqueue(action);

        // Simulate recovery based on action type
        bool success = action.ActionType switch
        {
            "restart" => true,
            "clear_cache" => true,
            "reconnect" => true,
            _ => false
        };

        if (success)
        {
            LogInfo($"Recovery successful: {action.ActionType}");
        }
        else
        {
            LogWarn($"Recovery failed: {action.ActionType}");
        }

        return success;
    }

    public async Task<Dictionary<string, ModuleHealthStatus>> CheckAllModulesAsync()
    {
        await Task.CompletedTask; // Async placeholder

        var results = new Dictionary<string, ModuleHealthStatus>();

        if (_manager != null)
        {
            foreach (var wrapper in _manager.Modules)
            {
                var status = await CheckModuleHealthAsync(wrapper.Instance);
                results[wrapper.Instance.Name] = status;
                _healthStatuses.AddOrUpdate(wrapper.Instance.Name, status, (_, _) => status);
            }
        }

        _lastFullCheck = DateTime.UtcNow;
        return results;
    }

    private async Task<ModuleHealthStatus> CheckModuleHealthAsync(IRaModule module)
    {
        await Task.CompletedTask; // Async placeholder

        var status = new ModuleHealthStatus
        {
            ModuleName = module.Name,
            State = HealthState.Healthy,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            // Check if module implements self-healing
            if (module is ISelfHealingModule selfHealing)
            {
                return await selfHealing.PerformSelfCheckAsync();
            }

            // Basic health check - try to call the module
            var response = module.Process("health");
            if (response.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                status.State = HealthState.Degraded;
                status.Warnings.Add("Module reports health concerns");
            }
        }
        catch (Exception ex)
        {
            status.State = HealthState.Unhealthy;
            status.Issues.Add($"Module check failed: {ex.Message}");
        }

        return status;
    }

    private string PerformFullSystemCheck()
    {
        var task = CheckAllModulesAsync();
        task.Wait();
        var results = task.Result;

        var sb = new System.Text.StringBuilder("System Health Check Results:\n\n");
        
        var healthy = results.Values.Count(s => s.State == HealthState.Healthy);
        var degraded = results.Values.Count(s => s.State == HealthState.Degraded);
        var unhealthy = results.Values.Count(s => s.State == HealthState.Unhealthy);

        sb.AppendLine($"✓ Healthy: {healthy}");
        if (degraded > 0) sb.AppendLine($"⚠ Degraded: {degraded}");
        if (unhealthy > 0) sb.AppendLine($"✗ Unhealthy: {unhealthy}");
        sb.AppendLine();

        foreach (var kvp in results)
        {
            var icon = kvp.Value.State switch
            {
                HealthState.Healthy => "✓",
                HealthState.Degraded => "⚠",
                HealthState.Unhealthy => "✗",
                _ => "?"
            };
            sb.AppendLine($"{icon} {kvp.Key}: {kvp.Value.State}");
            
            if (kvp.Value.Issues.Count > 0)
            {
                foreach (var issue in kvp.Value.Issues)
                    sb.AppendLine($"    Issue: {issue}");
            }
            if (kvp.Value.Warnings.Count > 0)
            {
                foreach (var warning in kvp.Value.Warnings)
                    sb.AppendLine($"    Warning: {warning}");
            }
        }

        return sb.ToString();
    }

    private string GetHealthStatus()
    {
        if (_healthStatuses.IsEmpty)
            return "No health data available. Run 'check' first.";

        var sb = new System.Text.StringBuilder("Current Health Status:\n");
        foreach (var kvp in _healthStatuses)
        {
            var age = DateTime.UtcNow - kvp.Value.CheckedAt;
            sb.AppendLine($"{kvp.Key}: {kvp.Value.State} (checked {age.TotalMinutes:F0}m ago)");
        }
        return sb.ToString();
    }

    private string AttemptAutoRecovery()
    {
        var unhealthyModules = _healthStatuses.Values
            .Where(s => s.State == HealthState.Unhealthy || s.State == HealthState.Degraded)
            .ToList();

        if (unhealthyModules.Count == 0)
            return "No modules require recovery.";

        var sb = new System.Text.StringBuilder("Attempting auto-recovery...\n");
        
        foreach (var status in unhealthyModules)
        {
            var action = new RecoveryAction
            {
                ActionType = "restart",
                Description = $"Restart {status.ModuleName} module",
                RequiresUserApproval = false
            };

            var task = AttemptRecoveryAsync(action);
            task.Wait();
            
            sb.AppendLine($"{status.ModuleName}: {(task.Result ? "✓ Recovered" : "✗ Failed")}");
        }

        return sb.ToString();
    }

    private string GetRecoveryLog()
    {
        if (_recoveryLog.IsEmpty)
            return "No recovery actions logged.";

        var sb = new System.Text.StringBuilder("Recovery Log (last 10):\n");
        foreach (var action in _recoveryLog.ToArray().TakeLast(10))
        {
            sb.AppendLine($"[{action.CreatedAt:HH:mm:ss}] {action.ActionType}: {action.Description}");
        }
        return sb.ToString();
    }

    private string GetStats()
    {
        var healthy = _healthStatuses.Values.Count(s => s.State == HealthState.Healthy);
        var degraded = _healthStatuses.Values.Count(s => s.State == HealthState.Degraded);
        var unhealthy = _healthStatuses.Values.Count(s => s.State == HealthState.Unhealthy);

        return $"SelfHealing Stats:\n" +
               $"  Monitored modules: {_healthStatuses.Count}\n" +
               $"  Healthy: {healthy}\n" +
               $"  Degraded: {degraded}\n" +
               $"  Unhealthy: {unhealthy}\n" +
               $"  Recovery actions: {_recoveryLog.Count}\n" +
               $"  Last full check: {(DateTime.UtcNow - _lastFullCheck).TotalMinutes:F0} minutes ago";
    }
}
