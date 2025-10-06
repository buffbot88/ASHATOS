namespace Abstractions;

/// <summary>
/// Health status of a module
/// </summary>
public class ModuleHealthStatus
{
    public string ModuleName { get; set; } = string.Empty;
    public HealthState State { get; set; } = HealthState.Unknown;
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public string? Message { get; set; }
}

/// <summary>
/// Module health states
/// </summary>
public enum HealthState
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Critical = 4
}

/// <summary>
/// Recovery action that can be performed
/// </summary>
public class RecoveryAction
{
    public string ActionId { get; set; } = Guid.NewGuid().ToString();
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresUserApproval { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool WasSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// System-wide diagnostic result
/// </summary>
public class SystemDiagnosticResult
{
    public string DiagnosticName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<RecoveryAction> SuggestedActions { get; set; } = new();
}

/// <summary>
/// Interface for self-healing modules
/// </summary>
public interface ISelfHealingModule
{
    /// <summary>
    /// Perform a self-check and return health status
    /// </summary>
    Task<ModuleHealthStatus> PerformSelfCheckAsync();
    
    /// <summary>
    /// Attempt to recover from detected issues
    /// </summary>
    Task<bool> AttemptRecoveryAsync(RecoveryAction action);
}
