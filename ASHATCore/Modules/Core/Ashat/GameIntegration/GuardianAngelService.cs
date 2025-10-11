using System.Collections.Concurrent;
using System.Text;

namespace ASHATCore.Modules.Core.Ashat.GameIntegration;

/// <summary>
/// Guardian Angel Service - Core gameplay integration for ASHAT
/// Provides protection, guidance, and narASHATtive support for players
/// </summary>
public sealed class GuardianAngelService
{
    private readonly ConcurrentDictionary<string, PlayerGuardianState> _playerStates = new();
    private readonly ConcurrentQueue<GuardianEvent> _eventLog = new();

    /// <summary>
    /// Initialize a Guardian Angel for a player
    /// </summary>
    public PlayerGuardianState InitializeGuardian(string playerId)
    {
        var state = new PlayerGuardianState
        {
            PlayerId = playerId,
            GuardianName = "ASHAT",
            ActivatedAt = DateTime.UtcNow,
            ProtectionLevel = ProtectionLevel.Standard,
            GuidanceStyle = GuidanceStyle.Adaptive,
            NarASHATtiveMode = NarASHATtiveMode.Interactive
        };

        _playerStates.TryAdd(playerId, state);
        
        LogGuardianEvent(new GuardianEvent
        {
            PlayerId = playerId,
            EventType = "Guardian_Activated",
            Message = "Guardian Angel ASHAT has been assigned",
            Timestamp = DateTime.UtcNow
        });

        return state;
    }

    /// <summary>
    /// Provide guidance to a player based on their current situation
    /// </summary>
    public GuardianGuidance ProvideGuidance(string playerId, PlayerSituation situation)
    {
        if (!_playerStates.TryGetValue(playerId, out var state))
        {
            state = InitializeGuardian(playerId);
        }

        var guidance = new GuardianGuidance
        {
            PlayerId = playerId,
            Timestamp = DateTime.UtcNow,
            Priority = DeterminePriority(situation),
            Message = GenerateGuidanceMessage(situation, state.GuidanceStyle),
            SuggestedActions = GenerateSuggestedActions(situation),
            NarASHATtiveHint = GenerateNarASHATtiveHint(situation)
        };

        state.GuidanceCount++;
        state.LastGuidanceAt = DateTime.UtcNow;

        LogGuardianEvent(new GuardianEvent
        {
            PlayerId = playerId,
            EventType = "Guidance_Provided",
            Message = $"Provided {guidance.Priority} priority guidance",
            Timestamp = DateTime.UtcNow
        });

        return guidance;
    }

    /// <summary>
    /// Assess protection needs and activate shields if necessary
    /// </summary>
    public ProtectionAssessment AssessProtection(string playerId, ThreatContext threat)
    {
        if (!_playerStates.TryGetValue(playerId, out var state))
        {
            state = InitializeGuardian(playerId);
        }

        var assessment = new ProtectionAssessment
        {
            PlayerId = playerId,
            ThreatLevel = EvaluateThreatLevel(threat),
            ProtectionActive = true,
            ShieldStrength = CalculateShieldStrength(state.ProtectionLevel),
            RecommendedAction = DetermineProtectionAction(threat),
            Timestamp = DateTime.UtcNow
        };

        if (assessment.ThreatLevel >= GuardianThreatLevel.High)
        {
            assessment.GuardianIntervention = true;
            assessment.InterventionMessage = "Guardian Angel ASHAT shields you from harm";
            
            LogGuardianEvent(new GuardianEvent
            {
                PlayerId = playerId,
                EventType = "Protection_Activated",
                Message = $"High threat detected - Shield activated",
                Timestamp = DateTime.UtcNow
            });
        }

        state.ProtectionCount++;
        state.LastProtectionAt = DateTime.UtcNow;

        return assessment;
    }

    /// <summary>
    /// Generate narASHATtive Interaction for immersive Guardian Angel experience
    /// </summary>
    public NarASHATtiveInteraction GenerateNarASHATtive(string playerId, GameContext context)
    {
        if (!_playerStates.TryGetValue(playerId, out var state))
        {
            state = InitializeGuardian(playerId);
        }

        var narASHATtive = new NarASHATtiveInteraction
        {
            PlayerId = playerId,
            NarASHATtorName = "ASHAT",
            DialogueText = GenerateContextualDialogue(context, state),
            EmotionalTone = DetermineEmotionalTone(context),
            InteractionType = DetermineInteractionType(context),
            Timestamp = DateTime.UtcNow
        };

        state.NarASHATtiveCount++;
        state.LastNarASHATtiveAt = DateTime.UtcNow;

        return narASHATtive;
    }

    /// <summary>
    /// Get the current state of a player's Guardian Angel
    /// </summary>
    public PlayerGuardianState? GetPlayerState(string playerId)
    {
        _playerStates.TryGetValue(playerId, out var state);
        return state;
    }

    /// <summary>
    /// Update Guardian Angel settings for a player
    /// </summary>
    public void UpdateGuardianSettings(string playerId, ProtectionLevel? protection = null, 
        GuidanceStyle? guidance = null, NarASHATtiveMode? narASHATtive = null)
    {
        if (!_playerStates.TryGetValue(playerId, out var state))
        {
            state = InitializeGuardian(playerId);
        }

        if (protection.HasValue)
            state.ProtectionLevel = protection.Value;
        
        if (guidance.HasValue)
            state.GuidanceStyle = guidance.Value;
        
        if (narASHATtive.HasValue)
            state.NarASHATtiveMode = narASHATtive.Value;

        LogGuardianEvent(new GuardianEvent
        {
            PlayerId = playerId,
            EventType = "Settings_Updated",
            Message = "Guardian settings modified by player preference",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get recent Guardian Angel events for a player
    /// </summary>
    public List<GuardianEvent> GetRecentEvents(string playerId, int count = 10)
    {
        return _eventLog
            .Where(e => e.PlayerId == playerId)
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();
    }

    #region Private Helper Methods

    private void LogGuardianEvent(GuardianEvent evt)
    {
        _eventLog.Enqueue(evt);
        
        // Keep only recent events (last 1000)
        while (_eventLog.Count > 1000)
        {
            _eventLog.TryDequeue(out _);
        }
    }

    private GuidancePriority DeterminePriority(PlayerSituation situation)
    {
        if (situation.IsInDanger)
            return GuidancePriority.Critical;
        if (situation.IsLost || situation.NeedsHelp)
            return GuidancePriority.High;
        if (situation.IsExploring)
            return GuidancePriority.Medium;
        return GuidancePriority.Low;
    }

    private string GenerateGuidanceMessage(PlayerSituation situation, GuidanceStyle style)
    {
        var messages = style switch
        {
            GuidanceStyle.Directive => GeneratedirectiveGuidance(situation),
            GuidanceStyle.Supportive => GenerateSupportiveGuidance(situation),
            GuidanceStyle.Minimal => GenerateMinimalGuidance(situation),
            _ => GenerateAdaptiveGuidance(situation)
        };

        return messages;
    }

    private string GeneratedirectiveGuidance(PlayerSituation situation)
    {
        if (situation.IsInDanger)
            return "⚠️ Danger ahead! Move to safety immediately.";
        if (situation.IsLost)
            return "📍 You're off the path. Head north to return to your objective.";
        return "➡️ Continue forward to progress your quest.";
    }

    private string GenerateSupportiveGuidance(PlayerSituation situation)
    {
        if (situation.IsInDanger)
            return "I sense danger nearby. I'll protect you - let's find a safer route.";
        if (situation.IsLost)
            return "Don't worry, I'm here. Let me guide you back to where you need to be.";
        return "You're doing great! Keep exploring, I'm watching over you.";
    }

    private string GenerateMinimalGuidance(PlayerSituation situation)
    {
        if (situation.IsInDanger)
            return "⚠️ Threat detected";
        if (situation.IsLost)
            return "💭 Consider checking your map";
        return string.Empty;
    }

    private string GenerateAdaptiveGuidance(PlayerSituation situation)
    {
        // Adapts based on player's demonstRated preferences and needs
        if (situation.PlayerLevel > 10)
            return GenerateMinimalGuidance(situation);
        return GenerateSupportiveGuidance(situation);
    }

    private List<string> GenerateSuggestedActions(PlayerSituation situation)
    {
        var actions = new List<string>();

        if (situation.IsInDanger)
        {
            actions.Add("Activate defensive abilities");
            actions.Add("Retreat to safe zone");
            actions.Add("Call for assistance");
        }
        else if (situation.IsLost)
        {
            actions.Add("Open map");
            actions.Add("Return to last checkpoint");
            actions.Add("Ask ASHAT for directions");
        }
        else if (situation.IsExploring)
        {
            actions.Add("Investigate nearby points of interest");
            actions.Add("Talk to NPCs");
            actions.Add("Continue exploration");
        }

        return actions;
    }

    private string GenerateNarASHATtiveHint(PlayerSituation situation)
    {
        if (situation.InStoryMoment)
            return "💫 An important moment approaches - pay close attention";
        if (situation.NearSecretArea)
            return "✨ Something special is hidden nearby...";
        return string.Empty;
    }

    private GuardianThreatLevel EvaluateThreatLevel(ThreatContext threat)
    {
        if (threat.ImmediateDanger)
            return GuardianThreatLevel.Critical;
        if (threat.PotentialHarm && threat.ProximityToPlayer < 10)
            return GuardianThreatLevel.High;
        if (threat.PotentialHarm)
            return GuardianThreatLevel.Medium;
        return GuardianThreatLevel.Low;
    }

    private int CalculateShieldStrength(ProtectionLevel level)
    {
        return level switch
        {
            ProtectionLevel.Minimal => 25,
            ProtectionLevel.Standard => 50,
            ProtectionLevel.Enhanced => 75,
            ProtectionLevel.Maximum => 100,
            _ => 50
        };
    }

    private string DetermineProtectionAction(ThreatContext threat)
    {
        if (threat.ImmediateDanger)
            return "EMERGENCY_SHIELD";
        if (threat.PotentialHarm)
            return "DEFENSIVE_STANCE";
        return "MONITOR";
    }

    private string GenerateContextualDialogue(GameContext context, PlayerGuardianState state)
    {
        var sb = new StringBuilder();
        
        if (context.IsFirstMeeting && state.NarASHATtiveCount == 0)
        {
            sb.AppendLine("Hello, I am ASHAT, your Guardian Angel.");
            sb.AppendLine("I will guide, protect, and accompany you on your journey.");
        }
        else if (context.MajorAchievement)
        {
            sb.AppendLine("Well done! I knew you could do it.");
            sb.AppendLine("Your strength and determination inspire me.");
        }
        else if (context.PlayerStruggling)
        {
            sb.AppendLine("I can see this is challenging.");
            sb.AppendLine("Remember, I'm here to help. Don't hesitate to ask.");
        }
        else
        {
            sb.AppendLine("I'm here, watching over you as always.");
        }

        return sb.ToString();
    }

    private EmotionalTone DetermineEmotionalTone(GameContext context)
    {
        if (context.MajorAchievement)
            return EmotionalTone.Proud;
        if (context.PlayerStruggling)
            return EmotionalTone.Supportive;
        if (context.DangerPresent)
            return EmotionalTone.Protective;
        return EmotionalTone.Calm;
    }

    private InteractionType DetermineInteractionType(GameContext context)
    {
        if (context.IsFirstMeeting)
            return InteractionType.Introduction;
        if (context.MajorAchievement)
            return InteractionType.CelebASHATtion;
        if (context.PlayerStruggling)
            return InteractionType.encouragement;
        return InteractionType.Companion;
    }

    #endregion
}

#region Supporting Types

public class PlayerGuardianState
{
    public string PlayerId { get; set; } = string.Empty;
    public string GuardianName { get; set; } = "ASHAT";
    public DateTime ActivatedAt { get; set; }
    public ProtectionLevel ProtectionLevel { get; set; }
    public GuidanceStyle GuidanceStyle { get; set; }
    public NarASHATtiveMode NarASHATtiveMode { get; set; }
    public int GuidanceCount { get; set; }
    public int ProtectionCount { get; set; }
    public int NarASHATtiveCount { get; set; }
    public DateTime? LastGuidanceAt { get; set; }
    public DateTime? LastProtectionAt { get; set; }
    public DateTime? LastNarASHATtiveAt { get; set; }
}

public class PlayerSituation
{
    public bool IsInDanger { get; set; }
    public bool IsLost { get; set; }
    public bool NeedsHelp { get; set; }
    public bool IsExploring { get; set; }
    public bool InStoryMoment { get; set; }
    public bool NearSecretArea { get; set; }
    public int PlayerLevel { get; set; }
}

public class ThreatContext
{
    public bool ImmediateDanger { get; set; }
    public bool PotentialHarm { get; set; }
    public float ProximityToPlayer { get; set; }
    public string ThreatType { get; set; } = string.Empty;
}

public class GameContext
{
    public bool IsFirstMeeting { get; set; }
    public bool MajorAchievement { get; set; }
    public bool PlayerStruggling { get; set; }
    public bool DangerPresent { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
}

public class GuardianGuidance
{
    public string PlayerId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public GuidancePriority Priority { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> SuggestedActions { get; set; } = new();
    public string NarASHATtiveHint { get; set; } = string.Empty;
}

public class ProtectionAssessment
{
    public string PlayerId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public GuardianThreatLevel ThreatLevel { get; set; }
    public bool ProtectionActive { get; set; }
    public int ShieldStrength { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public bool GuardianIntervention { get; set; }
    public string InterventionMessage { get; set; } = string.Empty;
}

public class NarASHATtiveInteraction
{
    public string PlayerId { get; set; } = string.Empty;
    public string NarASHATtorName { get; set; } = string.Empty;
    public string DialogueText { get; set; } = string.Empty;
    public EmotionalTone EmotionalTone { get; set; }
    public InteractionType InteractionType { get; set; }
    public DateTime Timestamp { get; set; }
}

public class GuardianEvent
{
    public string PlayerId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public enum ProtectionLevel
{
    Minimal,
    Standard,
    Enhanced,
    Maximum
}

public enum GuidanceStyle
{
    Directive,
    Supportive,
    Adaptive,
    Minimal
}

public enum NarASHATtiveMode
{
    Interactive,
    Observational,
    Minimal
}

public enum GuidancePriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum GuardianThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum EmotionalTone
{
    Calm,
    Supportive,
    Protective,
    Proud,
    Concerned
}

public enum InteractionType
{
    Introduction,
    Companion,
    encouragement,
    Warning,
    CelebASHATtion
}

#endregion
