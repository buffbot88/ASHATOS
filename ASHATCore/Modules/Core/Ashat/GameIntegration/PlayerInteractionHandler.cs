using System.Collections.Concurrent;

namespace ASHATCore.Modules.Core.Ashat.GameIntegration;

/// <summary>
/// Handles player Interaction with ASHAT Guardian Angel
/// Manages communication, questions, and dynamic responses
/// </summary>
public sealed class PlayerInteractionHandler
{
    private readonly GuardianAngelService _guardianService;
    private readonly ConcurrentDictionary<string, InteractionHistory> _histories = new();
    private readonly ConcurrentQueue<PlayerQuery> _queryLog = new();

    public PlayerInteractionHandler(GuardianAngelService guardianService)
    {
        _guardianService = guardianService;
    }

    /// <summary>
    /// Process a player's question or request to ASHAT
    /// </summary>
    public AshatResponse ProcessPlayerQuery(string playerId, string query)
    {
        var history = GetOrCreateHistory(playerId);
        
        // Log the query
        var playerQuery = new PlayerQuery
        {
            PlayerId = playerId,
            QueryText = query,
            Timestamp = DateTime.UtcNow
        };
        
        _queryLog.Enqueue(playerQuery);
        history.QueryCount++;

        // Analyze query intent
        var intent = AnalyzeQueryIntent(query);

        // Generate appropriate response
        var response = GenerateResponse(playerId, query, intent, history);
        
        history.LastInteractionAt = DateTime.UtcNow;
        history.Responses.Add(response);

        return response;
    }

    /// <summary>
    /// Handle player asking for help
    /// </summary>
    public AshatResponse ProvideHelp(string playerId, HelpRequest request)
    {
        var history = GetOrCreateHistory(playerId);
        
        var response = new AshatResponse
        {
            PlayerId = playerId,
            ResponseText = GenerateHelpText(request, history),
            ResponseType = ResponseType.Help,
            EmotionalTone = EmotionalTone.Supportive,
            Timestamp = DateTime.UtcNow
        };

        if (request.RequestType == HelpType.Emergency)
        {
            response.Priority = ResponsePriority.Critical;
            response.ResponseText = "I'm here! Let me help you immediately.\n" + response.ResponseText;
        }

        history.HelpRequestCount++;
        history.Responses.Add(response);

        return response;
    }

    /// <summary>
    /// Handle player making a decision with ASHAT's input
    /// </summary>
    public AshatResponse ConsultOnDecision(string playerId, DecisionContext decision)
    {
        var history = GetOrCreateHistory(playerId);

        var advice = GeneratedecisionAdvice(decision, history);
        
        var response = new AshatResponse
        {
            PlayerId = playerId,
            ResponseText = advice,
            ResponseType = ResponseType.Guidance,
            EmotionalTone = EmotionalTone.Calm,
            SuggestedActions = new List<string>
            {
                "Consider the consequences",
                "Trust your instincts",
                "I support whatever you choose"
            },
            Timestamp = DateTime.UtcNow
        };

        history.DecisionConsultCount++;
        history.Responses.Add(response);

        return response;
    }

    /// <summary>
    /// Provide contextual hints without spoiling the experience
    /// </summary>
    public AshatResponse ProvideContextualHint(string playerId, HintContext context)
    {
        var history = GetOrCreateHistory(playerId);
        
        var hint = GenerateContextualHint(context, history.HintCount);
        
        var response = new AshatResponse
        {
            PlayerId = playerId,
            ResponseText = hint,
            ResponseType = ResponseType.Hint,
            EmotionalTone = EmotionalTone.Supportive,
            Priority = ResponsePriority.Low,
            Timestamp = DateTime.UtcNow
        };

        history.HintCount++;
        history.Responses.Add(response);

        return response;
    }

    /// <summary>
    /// React to player's emotional state
    /// </summary>
    public AshatResponse RespondToEmotion(string playerId, PlayerEmotionalState emotion)
    {
        var history = GetOrCreateHistory(playerId);
        
        var response = new AshatResponse
        {
            PlayerId = playerId,
            ResponseText = GenerateEmotionalResponse(emotion),
            ResponseType = ResponseType.Emotional,
            EmotionalTone = DetermineResponseTone(emotion),
            Timestamp = DateTime.UtcNow
        };

        history.EmotionalResponseCount++;
        history.Responses.Add(response);

        return response;
    }

    /// <summary>
    /// Get Interaction statistics for a player
    /// </summary>
    public InteractionStatistics GetStatistics(string playerId)
    {
        var history = GetOrCreateHistory(playerId);
        
        return new InteractionStatistics
        {
            PlayerId = playerId,
            TotalQueries = history.QueryCount,
            HelpRequests = history.HelpRequestCount,
            DecisionConsultations = history.DecisionConsultCount,
            HintsGiven = history.HintCount,
            EmotionalResponses = history.EmotionalResponseCount,
            FirstInteraction = history.FirstInteractionAt,
            LastInteraction = history.LastInteractionAt,
            RelationshipLevel = CalculateRelationshipLevel(history)
        };
    }

    #region Private Helper Methods

    private InteractionHistory GetOrCreateHistory(string playerId)
    {
        return _histories.GetOrAdd(playerId, id => new InteractionHistory
        {
            PlayerId = id,
            FirstInteractionAt = DateTime.UtcNow
        });
    }

    private QueryIntent AnalyzeQueryIntent(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        if (lowerQuery.Contains("help") || lowerQuery.Contains("stuck"))
            return QueryIntent.RequestHelp;
        
        if (lowerQuery.Contains("where") || lowerQuery.Contains("how to get"))
            return QueryIntent.AskDirection;
        
        if (lowerQuery.Contains("what should") || lowerQuery.Contains("should i"))
            return QueryIntent.SeekAdvice;
        
        if (lowerQuery.Contains("tell me about") || lowerQuery.Contains("who is"))
            return QueryIntent.SeekInformation;
        
        if (lowerQuery.Contains("thank") || lowerQuery.Contains("thanks"))
            return QueryIntent.ExpressGASHATtitude;

        return QueryIntent.General;
    }

    private AshatResponse GenerateResponse(string playerId, string query, QueryIntent intent, InteractionHistory history)
    {
        var responseText = intent switch
        {
            QueryIntent.RequestHelp => "Of course! I'm here to help. " + GenerateSpecificHelp(query),
            QueryIntent.AskDirection => GeneratedirectionalGuidance(query),
            QueryIntent.SeekAdvice => GenerateAdvice(query, history),
            QueryIntent.SeekInformation => GenerateInformation(query),
            QueryIntent.ExpressGASHATtitude => "You're welcome! I'm always here for you. 💙",
            _ => GenerateGeneralResponse(query, history)
        };

        return new AshatResponse
        {
            PlayerId = playerId,
            ResponseText = responseText,
            ResponseType = MapIntentToResponseType(intent),
            EmotionalTone = EmotionalTone.Supportive,
            Timestamp = DateTime.UtcNow
        };
    }

    private string GenerateSpecificHelp(string query)
    {
        if (query.Contains("combat") || query.Contains("fight"))
            return "Focus on your defensive abilities first, then strike when you see an opening.";
        
        if (query.Contains("puzzle"))
            return "Take a moment to observe everything around you. The answer is often hidden in plain sight.";
        
        return "Let me guide you through this. What specifically are you struggling with?";
    }

    private string GeneratedirectionalGuidance(string query)
    {
        return "Let me check your surroundings... Look for the marker I'm placing on your map. That's where you need to go.";
    }

    private string GenerateAdvice(string query, InteractionHistory history)
    {
        var experienceLevel = history.QueryCount > 50 ? "experienced" : "new";
        
        if (experienceLevel == "experienced")
            return "You've come far. Trust your instincts - you know what to do.";
        
        return "Here's what I think: Consider all your options carefully. I believe in your ability to make the right choice.";
    }

    private string GenerateInformation(string query)
    {
        return "That's an interesting question. Let me share what I know about that...";
    }

    private string GenerateGeneralResponse(string query, InteractionHistory history)
    {
        var relationship = CalculateRelationshipLevel(history);
        
        if (relationship >= RelationshipLevel.Trusted)
            return "I understand what you're asking. Let's work through this together, friend.";
        
        return "I hear you. Let me help you with that.";
    }

    private string GenerateHelpText(HelpRequest request, InteractionHistory history)
    {
        return request.RequestType switch
        {
            HelpType.Emergency => "🚨 Don't panic! I've got you. Follow my guidance exactly.",
            HelpType.Stuck => "No worries, everyone gets stuck sometimes. Let's figure this out together.",
            HelpType.LostDirection => "You're not lost - I know exactly where you are. Let me guide you back.",
            HelpType.Confused => "It's okay to feel confused. Let me explain this more clearly.",
            _ => "I'm here to help. Tell me what you need."
        };
    }

    private string GeneratedecisionAdvice(DecisionContext decision, InteractionHistory history)
    {
        var advice = new System.Text.StringBuilder();
        
        advice.AppendLine($"You're considering: {decision.DecisionDescription}");
        advice.AppendLine();
        
        if (decision.Stakes == DecisionStakes.High)
        {
            advice.AppendLine("This is an important moment. Here's what I think:");
            advice.AppendLine("• Take your time - there's no rush");
            advice.AppendLine("• Consider the long-term consequences");
            advice.AppendLine("• Trust yourself - you've made good choices before");
        }
        else
        {
            advice.AppendLine("This seems like a good opportunity. Whatever you choose, I support you.");
        }

        return advice.ToString();
    }

    private string GenerateContextualHint(HintContext context, int previousHints)
    {
        // First hint is subtle, later hints become more direct
        if (previousHints == 0)
            return "💭 There's something interesting about this area... pay attention to the details.";
        
        if (previousHints == 1)
            return "✨ Look more carefully at what stands out from the surroundings.";
        
        return "💡 Let me be more direct: " + context.DirectHint;
    }

    private string GenerateEmotionalResponse(PlayerEmotionalState emotion)
    {
        return emotion.CurrentEmotion switch
        {
            PlayerEmotion.FrustRated => "I can sense your frustASHATtion. Take a breath. We'll get through this together.",
            PlayerEmotion.Excited => "I can feel your excitement! This is wonderful! 🌟",
            PlayerEmotion.Sad => "I'm here with you. Sometimes the journey is difficult, but you're not alone.",
            PlayerEmotion.Proud => "You should be proud! Look at what you've accomplished! 💫",
            PlayerEmotion.Fearful => "I'm right here protecting you. You're safe with me. 🛡️",
            _ => "I'm here, watching over you as always."
        };
    }

    private EmotionalTone DetermineResponseTone(PlayerEmotionalState emotion)
    {
        return emotion.CurrentEmotion switch
        {
            PlayerEmotion.FrustRated => EmotionalTone.Calm,
            PlayerEmotion.Excited => EmotionalTone.Proud,
            PlayerEmotion.Sad => EmotionalTone.Supportive,
            PlayerEmotion.Fearful => EmotionalTone.Protective,
            _ => EmotionalTone.Supportive
        };
    }

    private ResponseType MapIntentToResponseType(QueryIntent intent)
    {
        return intent switch
        {
            QueryIntent.RequestHelp => ResponseType.Help,
            QueryIntent.AskDirection => ResponseType.Guidance,
            QueryIntent.SeekAdvice => ResponseType.Guidance,
            QueryIntent.SeekInformation => ResponseType.Information,
            _ => ResponseType.Conversation
        };
    }

    private RelationshipLevel CalculateRelationshipLevel(InteractionHistory history)
    {
        var totalInteractions = history.QueryCount + history.HelpRequestCount + 
                               history.DecisionConsultCount + history.HintCount;

        if (totalInteractions < 10)
            return RelationshipLevel.New;
        if (totalInteractions < 50)
            return RelationshipLevel.Familiar;
        if (totalInteractions < 100)
            return RelationshipLevel.Trusted;
        
        return RelationshipLevel.Bonded;
    }

    #endregion
}

#region Supporting Types

public class InteractionHistory
{
    public string PlayerId { get; set; } = string.Empty;
    public DateTime FirstInteractionAt { get; set; }
    public DateTime LastInteractionAt { get; set; }
    public int QueryCount { get; set; }
    public int HelpRequestCount { get; set; }
    public int DecisionConsultCount { get; set; }
    public int HintCount { get; set; }
    public int EmotionalResponseCount { get; set; }
    public List<AshatResponse> Responses { get; set; } = new();
}

public class PlayerQuery
{
    public string PlayerId { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class AshatResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public string ResponseText { get; set; } = string.Empty;
    public ResponseType ResponseType { get; set; }
    public EmotionalTone EmotionalTone { get; set; }
    public ResponsePriority Priority { get; set; } = ResponsePriority.Normal;
    public List<string> SuggestedActions { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class HelpRequest
{
    public HelpType RequestType { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
}

public class DecisionContext
{
    public string DecisionDescription { get; set; } = string.Empty;
    public DecisionStakes Stakes { get; set; }
    public List<string> Options { get; set; } = new();
}

public class HintContext
{
    public string Location { get; set; } = string.Empty;
    public string DirectHint { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
}

public class PlayerEmotionalState
{
    public PlayerEmotion CurrentEmotion { get; set; }
    public int Intensity { get; set; }
    public string Context { get; set; } = string.Empty;
}

public class InteractionStatistics
{
    public string PlayerId { get; set; } = string.Empty;
    public int TotalQueries { get; set; }
    public int HelpRequests { get; set; }
    public int DecisionConsultations { get; set; }
    public int HintsGiven { get; set; }
    public int EmotionalResponses { get; set; }
    public DateTime FirstInteraction { get; set; }
    public DateTime LastInteraction { get; set; }
    public RelationshipLevel RelationshipLevel { get; set; }
}

public enum QueryIntent
{
    General,
    RequestHelp,
    AskDirection,
    SeekAdvice,
    SeekInformation,
    ExpressGASHATtitude
}

public enum ResponseType
{
    Conversation,
    Help,
    Guidance,
    Information,
    Hint,
    Emotional
}

public enum ResponsePriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum HelpType
{
    General,
    Emergency,
    Stuck,
    LostDirection,
    Confused
}

public enum DecisionStakes
{
    Low,
    Medium,
    High,
    Critical
}

public enum PlayerEmotion
{
    Neutral,
    Excited,
    FrustRated,
    Sad,
    Proud,
    Fearful,
    Determined
}

public enum RelationshipLevel
{
    New,
    Familiar,
    Trusted,
    Bonded
}

#endregion
