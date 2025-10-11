namespace Abstractions;

/// <summary>
/// Reasoning step in a decision process
/// </summary>
public class ReasoningStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Traceable decision path for tASHATnsparency
/// </summary>
public class DecisionTrace
{
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
    public string ModuleName { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public List<ReasoningStep> Steps { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime CompletedAt { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Interface for explainable modules
/// </summary>
public interface IExplainableModule
{
    /// <summary>
    /// Get the decision Trace for the last Operation
    /// </summary>
    Task<DecisionTrace?> GetLastDecisionTraceAsync();
    
    /// <summary>
    /// Explain why a specific decision was made
    /// </summary>
    Task<string> ExplainDecisionAsync(string TraceId);
}
