namespace Abstractions;

/// <summary>
/// Represents a user's appeal request for a suspension.
/// </summary>
public class AppealRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid SuspensionId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string InitialStatement { get; set; } = string.Empty;
    public AppealStatus Status { get; set; } = AppealStatus.Pending;
}

/// <summary>
/// Represents an AI-driven appeal session with interview Q&A.
/// </summary>
public class AppealSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppealRequestId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<AppealInteraction> Interactions { get; set; } = new();
    public AppealSessionStatus Status { get; set; } = AppealSessionStatus.Active;
}

/// <summary>
/// Represents a single Q&A Interaction in an appeal session.
/// </summary>
public class AppealInteraction
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Question { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? AiAnalysis { get; set; }
}

/// <summary>
/// Represents the final decision made by AI on an appeal.
/// </summary>
public class AppealDecision
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppealRequestId { get; set; }
    public Guid SessionId { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
    public AppealOutcome Outcome { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; } // 0.0 to 1.0
    public bool RequiresHumanReview { get; set; }
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// Appeal request status.
/// </summary>
public enum AppealStatus
{
    Pending,
    InProgress,
    Completed,
    Rejected,
    Escalated
}

/// <summary>
/// Appeal session status.
/// </summary>
public enum AppealSessionStatus
{
    Active,
    Completed,
    Abandoned,
    Escalated
}

/// <summary>
/// Outcome of an appeal decision.
/// </summary>
public enum AppealOutcome
{
    Approved,           // Suspension lifted
    PartiallyApproved,  // Suspension reduced
    Denied,             // Appeal rejected
    EscalateToHuman,    // Requires human Moderator review
    RequiresMoreInfo    // Need additional information
}

/// <summary>
/// Interface for support chat module with appeal functionality.
/// </summary>
public interface ISupportChatModule
{
    /// <summary>
    /// Start an appeal process for a suspended user.
    /// </summary>
    Task<AppealRequest> StartAppealAsync(string userId, string initialStatement);
    
    /// <summary>
    /// Get active appeal session for a user.
    /// </summary>
    Task<AppealSession?> GetActiveSessionAsync(string userId);
    
    /// <summary>
    /// Submit a response to an appeal question.
    /// </summary>
    Task<string> SubmitAppealResponseAsync(string userId, string response);
    
    /// <summary>
    /// Get appeal decision for a user.
    /// </summary>
    Task<AppealDecision?> GetAppealDecisionAsync(string userId);
    
    /// <summary>
    /// Get all pending appeals requiring human review.
    /// </summary>
    Task<List<AppealDecision>> GetPendingHumanReviewsAsync();
    
    /// <summary>
    /// Process final human review for escalated appeal.
    /// </summary>
    Task<bool> ReviewEscalatedAppealAsync(Guid appealId, AppealOutcome outcome, string reviewNotes, string reviewerId);
}
