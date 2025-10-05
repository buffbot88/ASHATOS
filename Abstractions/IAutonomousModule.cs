namespace Abstractions;

/// <summary>
/// Decision recommendation from a module
/// </summary>
public class DecisionRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string FromModule { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public double Confidence { get; set; }
    public bool RequiresUserConsent { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Reasoning { get; set; }
}

/// <summary>
/// Result of a decision
/// </summary>
public class DecisionResult
{
    public string DecisionId { get; set; } = Guid.NewGuid().ToString();
    public DecisionRecommendation? Recommendation { get; set; }
    public bool Approved { get; set; }
    public bool Executed { get; set; }
    public string? Result { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Interface for autonomous decision-making modules
/// </summary>
public interface IAutonomousModule
{
    /// <summary>
    /// Analyze a situation and provide recommendations
    /// </summary>
    Task<DecisionRecommendation> AnalyzeAndRecommendAsync(string situation, Dictionary<string, object>? context = null);
    
    /// <summary>
    /// Execute a decision if approved
    /// </summary>
    Task<DecisionResult> ExecuteDecisionAsync(DecisionRecommendation recommendation, bool userApproved);
}
