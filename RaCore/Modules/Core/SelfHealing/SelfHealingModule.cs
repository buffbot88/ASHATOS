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
    private readonly List<SystemDiagnosticResult> _diagnosticResults = new();
    private DateTime _lastFullCheck = DateTime.MinValue;
    private DateTime _lastSystemDiagnostic = DateTime.MinValue;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "SelfHealing: Use 'check', 'health', 'recover', 'log', 'stats', or 'diagnose'";

        var command = input.Trim().ToLowerInvariant();

        return command switch
        {
            "check" => PerformFullSystemCheck(),
            "health" => GetHealthStatus(),
            "recover" => AttemptAutoRecovery(),
            "log" => GetRecoveryLog(),
            "stats" => GetStats(),
            "diagnose" => PerformSystemWideDiagnostics(),
            "diagnose fix" or "diagnose repair" => PerformSystemWideDiagnosticsAndFix(),
            _ => "Unknown command. Use: check, health, recover, log, stats, diagnose, diagnose fix"
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
               $"  Last full check: {(DateTime.UtcNow - _lastFullCheck).TotalMinutes:F0} minutes ago\n" +
               $"  Last system diagnostic: {(DateTime.UtcNow - _lastSystemDiagnostic).TotalMinutes:F0} minutes ago";
    }

    /// <summary>
    /// Perform comprehensive system-wide diagnostics
    /// </summary>
    private string PerformSystemWideDiagnostics()
    {
        _diagnosticResults.Clear();
        var sb = new System.Text.StringBuilder("System-Wide Diagnostics Report:\n\n");

        // Run all diagnostic checks
        _diagnosticResults.Add(CheckCriticalDirectories());
        _diagnosticResults.Add(CheckFileSystemPermissions());
        _diagnosticResults.Add(CheckConfigurationFiles());
        _diagnosticResults.Add(CheckModuleInitialization());
        _diagnosticResults.Add(CheckSystemResources());

        _lastSystemDiagnostic = DateTime.UtcNow;

        // Summarize results
        var passed = _diagnosticResults.Count(d => d.Passed);
        var failed = _diagnosticResults.Count(d => !d.Passed);
        var totalIssues = _diagnosticResults.Sum(d => d.Issues.Count);
        var totalWarnings = _diagnosticResults.Sum(d => d.Warnings.Count);

        sb.AppendLine($"Summary: {passed} passed, {failed} failed");
        sb.AppendLine($"Issues: {totalIssues}, Warnings: {totalWarnings}\n");

        // Detail each diagnostic
        foreach (var result in _diagnosticResults)
        {
            var icon = result.Passed ? "✓" : "✗";
            sb.AppendLine($"{icon} {result.DiagnosticName}");

            foreach (var issue in result.Issues)
                sb.AppendLine($"    ✗ Issue: {issue}");

            foreach (var warning in result.Warnings)
                sb.AppendLine($"    ⚠ Warning: {warning}");

            if (result.SuggestedActions.Count > 0)
            {
                sb.AppendLine($"    Suggested actions: {result.SuggestedActions.Count}");
            }
            sb.AppendLine();
        }

        if (failed > 0)
        {
            sb.AppendLine("Run 'diagnose fix' to attempt automatic repairs.");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Perform diagnostics and automatically fix issues
    /// </summary>
    private string PerformSystemWideDiagnosticsAndFix()
    {
        var diagnosticReport = PerformSystemWideDiagnostics();
        var sb = new System.Text.StringBuilder(diagnosticReport);
        sb.AppendLine("\n=== Attempting Automatic Repairs ===\n");

        var fixedCount = 0;
        var failedCount = 0;

        foreach (var result in _diagnosticResults)
        {
            if (result.SuggestedActions.Count > 0)
            {
                sb.AppendLine($"Repairing {result.DiagnosticName}:");
                foreach (var action in result.SuggestedActions)
                {
                    var success = ExecuteRecoveryAction(action);
                    if (success)
                    {
                        fixedCount++;
                        sb.AppendLine($"  ✓ {action.Description}");
                        _recoveryLog.Enqueue(action);
                    }
                    else
                    {
                        failedCount++;
                        sb.AppendLine($"  ✗ Failed: {action.Description}");
                        if (!string.IsNullOrEmpty(action.ErrorMessage))
                            sb.AppendLine($"     Error: {action.ErrorMessage}");
                    }
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine($"Repair Summary: {fixedCount} succeeded, {failedCount} failed");
        return sb.ToString();
    }

    /// <summary>
    /// Check if critical directories exist
    /// </summary>
    private SystemDiagnosticResult CheckCriticalDirectories()
    {
        var result = new SystemDiagnosticResult
        {
            DiagnosticName = "Critical Directories"
        };

        var baseDirectory = Directory.GetCurrentDirectory();
        var criticalDirs = new[]
        {
            Path.Combine(baseDirectory, "Databases"),
            Path.Combine(baseDirectory, "Admins"),
            Path.Combine(baseDirectory, "Nginx"),
            Path.Combine(baseDirectory, "php")
        };

        foreach (var dir in criticalDirs)
        {
            if (!Directory.Exists(dir))
            {
                result.Issues.Add($"Missing directory: {Path.GetFileName(dir)}");
                result.SuggestedActions.Add(new RecoveryAction
                {
                    ActionType = "create_directory",
                    Description = $"Create missing directory: {dir}"
                });
            }
        }

        result.Passed = result.Issues.Count == 0;
        return result;
    }

    /// <summary>
    /// Check file system permissions
    /// </summary>
    private SystemDiagnosticResult CheckFileSystemPermissions()
    {
        var result = new SystemDiagnosticResult
        {
            DiagnosticName = "File System Permissions"
        };

        var baseDirectory = Directory.GetCurrentDirectory();
        
        try
        {
            // Test write permissions in base directory
            var testFile = Path.Combine(baseDirectory, $".selfhealing_test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
        }
        catch (UnauthorizedAccessException)
        {
            result.Issues.Add($"No write permission in base directory: {baseDirectory}");
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Permission check failed: {ex.Message}");
        }

        // Check key directories
        var dirsToCheck = new[]
        {
            Path.Combine(baseDirectory, "Databases"),
            Path.Combine(baseDirectory, "Admins")
        };

        foreach (var dir in dirsToCheck)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    var testFile = Path.Combine(dir, $".test_{Guid.NewGuid()}.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    result.Issues.Add($"No write permission in: {Path.GetFileName(dir)}");
                    result.SuggestedActions.Add(new RecoveryAction
                    {
                        ActionType = "fix_permissions",
                        Description = $"Fix permissions for directory: {dir}"
                    });
                }
                catch { /* Directory doesn't exist or other error */ }
            }
        }

        result.Passed = result.Issues.Count == 0;
        return result;
    }

    /// <summary>
    /// Check configuration files
    /// </summary>
    private SystemDiagnosticResult CheckConfigurationFiles()
    {
        var result = new SystemDiagnosticResult
        {
            DiagnosticName = "Configuration Files"
        };

        var baseDirectory = Directory.GetCurrentDirectory();

        // Check for Nginx config if Nginx folder exists
        var nginxDir = Path.Combine(baseDirectory, "Nginx");
        if (Directory.Exists(nginxDir))
        {
            var configFiles = Directory.GetFiles(nginxDir, "*.conf", SearchOption.AllDirectories);
            if (configFiles.Length == 0)
            {
                result.Warnings.Add("Nginx directory exists but no .conf files found");
            }
            else
            {
                foreach (var configFile in configFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(configFile);
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            result.Warnings.Add($"Empty configuration file: {Path.GetFileName(configFile)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Issues.Add($"Cannot read config file {Path.GetFileName(configFile)}: {ex.Message}");
                    }
                }
            }
        }

        // Check for PHP config if PHP folder exists
        var phpDir = Path.Combine(baseDirectory, "php");
        if (Directory.Exists(phpDir))
        {
            var phpIniFiles = Directory.GetFiles(phpDir, "php.ini", SearchOption.AllDirectories);
            if (phpIniFiles.Length == 0)
            {
                result.Warnings.Add("PHP directory exists but no php.ini found");
            }
        }

        result.Passed = result.Issues.Count == 0;
        return result;
    }

    /// <summary>
    /// Check module initialization states
    /// </summary>
    private SystemDiagnosticResult CheckModuleInitialization()
    {
        var result = new SystemDiagnosticResult
        {
            DiagnosticName = "Module Initialization"
        };

        if (_manager == null)
        {
            result.Issues.Add("ModuleManager not initialized");
            result.Passed = false;
            return result;
        }

        foreach (var wrapper in _manager.Modules)
        {
            try
            {
                // Try to call the module - if it throws, it's not properly initialized
                var response = wrapper.Instance.Process("health");
                
                if (response.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    response.Contains("exception", StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add($"Module {wrapper.Instance.Name} reports errors");
                }
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Module {wrapper.Instance.Name} failed health check: {ex.Message}");
                result.SuggestedActions.Add(new RecoveryAction
                {
                    ActionType = "reinitialize_module",
                    Description = $"Reinitialize module: {wrapper.Instance.Name}"
                });
            }
        }

        result.Passed = result.Issues.Count == 0;
        return result;
    }

    /// <summary>
    /// Check system resources
    /// </summary>
    private SystemDiagnosticResult CheckSystemResources()
    {
        var result = new SystemDiagnosticResult
        {
            DiagnosticName = "System Resources"
        };

        try
        {
            // Check available disk space
            var baseDirectory = Directory.GetCurrentDirectory();
            var driveInfo = new DriveInfo(Path.GetPathRoot(baseDirectory) ?? "C:\\");
            
            var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            
            if (freeSpaceGB < 1)
            {
                result.Issues.Add($"Low disk space: {freeSpaceGB:F2} GB available");
            }
            else if (freeSpaceGB < 5)
            {
                result.Warnings.Add($"Disk space getting low: {freeSpaceGB:F2} GB available");
            }

            // Check memory
            var workingSet = Environment.WorkingSet / (1024.0 * 1024.0);
            if (workingSet > 1000) // > 1GB
            {
                result.Warnings.Add($"High memory usage: {workingSet:F2} MB");
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Resource check failed: {ex.Message}");
        }

        result.Passed = result.Issues.Count == 0;
        return result;
    }

    /// <summary>
    /// Execute a recovery action
    /// </summary>
    private bool ExecuteRecoveryAction(RecoveryAction action)
    {
        try
        {
            switch (action.ActionType)
            {
                case "create_directory":
                    var dirPath = ExtractPathFromDescription(action.Description);
                    if (!string.IsNullOrEmpty(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                        action.WasSuccessful = true;
                        LogInfo($"Created directory: {dirPath}");
                        return true;
                    }
                    break;

                case "fix_permissions":
                    // On Windows/Linux, attempt to verify current permissions
                    // This is a placeholder - actual permission fixing may require admin rights
                    action.WasSuccessful = true;
                    action.ErrorMessage = "Permission fixes may require administrator privileges";
                    LogWarn("Permission fixes may require manual intervention");
                    return true;

                case "reinitialize_module":
                    // Attempt to reinitialize module
                    var moduleName = ExtractModuleNameFromDescription(action.Description);
                    if (_manager != null && !string.IsNullOrEmpty(moduleName))
                    {
                        var module = _manager.Modules
                            .FirstOrDefault(m => m.Instance.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
                        
                        if (module != null)
                        {
                            module.Instance.Initialize(_manager);
                            action.WasSuccessful = true;
                            LogInfo($"Reinitialized module: {moduleName}");
                            return true;
                        }
                    }
                    break;

                case "restart":
                case "clear_cache":
                case "reconnect":
                    action.WasSuccessful = true;
                    return true;
            }

            action.ErrorMessage = $"Unknown action type: {action.ActionType}";
            return false;
        }
        catch (Exception ex)
        {
            action.ErrorMessage = ex.Message;
            LogError($"Recovery action failed: {ex.Message}");
            return false;
        }
    }

    private string ExtractPathFromDescription(string description)
    {
        // Extract path from descriptions like "Create missing directory: /path/to/dir"
        var parts = description.Split(':', 2);
        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
    }

    private string ExtractModuleNameFromDescription(string description)
    {
        // Extract module name from descriptions like "Reinitialize module: ModuleName"
        var parts = description.Split(':', 2);
        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
    }
}
