namespace Abstractions;

/// <summary>
/// Extended memory item with context chaining and relevance scoring
/// </summary>
public class ContextualMemoryItem : MemoryItem
{
    public double RelevanceScore { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public List<string> RelatedKeys { get; set; } = new();
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

/// <summary>
/// Session tracking for contextual memory
/// </summary>
public class MemorySession
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public Dictionary<string, string> Context { get; set; } = new();
    public int InteractionCount { get; set; }
}
