using System.Text;

namespace RaCore.Modules.Core.Ashat.GameIntegration;

/// <summary>
/// Guardian Angel Game Integration Component
/// Provides gameplay hooks, narrative generation, and player interaction features
/// for "Guardian Angel Arielle: Pretense to the World of RA"
/// </summary>
public sealed class GuardianAngelGameIntegration
{
    private readonly Dictionary<string, NarrativeElement> _narrativeLibrary = new();
    private readonly List<GameEvent> _eventHistory = new();
    private int _narrativeSequence;

    public GuardianAngelGameIntegration()
    {
        InitializeNarrativeLibrary();
    }

    /// <summary>
    /// Generates context-aware narrative based on game state
    /// </summary>
    public NarrativeResponse GenerateNarrative(GameContext context)
    {
        var narrative = context.EventType switch
        {
            GameEventType.PlayerEntering => GenerateWelcomeNarrative(context),
            GameEventType.CombatStarting => GenerateCombatGuidance(context),
            GameEventType.PuzzleSolving => GeneratePuzzleHint(context),
            GameEventType.LevelComplete => GenerateVictoryNarrative(context),
            GameEventType.PlayerDanger => GenerateProtectionNarrative(context),
            GameEventType.Discovery => GenerateDiscoveryNarrative(context),
            _ => GenerateDefaultNarrative(context)
        };

        RecordEvent(new GameEvent
        {
            EventType = context.EventType,
            PlayerId = context.PlayerId,
            Timestamp = DateTime.UtcNow,
            NarrativeGenerated = narrative.Text
        });

        return narrative;
    }

    /// <summary>
    /// Provides Guardian Angel guidance for player actions
    /// </summary>
    public GuidanceResponse ProvideGuidance(PlayerAction action)
    {
        var guidance = new GuidanceResponse
        {
            ActionType = action.ActionType,
            Timestamp = DateTime.UtcNow
        };

        switch (action.ActionType)
        {
            case "combat":
                guidance.Message = "Stay focused, brave one. Strike with precision and defend with wisdom.";
                guidance.StrengthBonus = 5;
                break;
            case "exploration":
                guidance.Message = "Your curiosity serves you well. I sense something significant nearby.";
                guidance.PerceptionBonus = 10;
                break;
            case "dialogue":
                guidance.Message = "Choose your words carefully. Truth and compassion will guide you.";
                guidance.CharismaBonus = 8;
                break;
            case "puzzle":
                guidance.Message = "Think deeply. The answer lies in patterns and patience.";
                guidance.IntelligenceBonus = 12;
                break;
            default:
                guidance.Message = "I am with you. Trust in your journey.";
                break;
        }

        return guidance;
    }

    /// <summary>
    /// Handles player protection mechanics
    /// </summary>
    public ProtectionResult ActivateProtection(DangerContext danger)
    {
        var protection = new ProtectionResult
        {
            DangerLevel = danger.Severity,
            ProtectionApplied = true,
            Timestamp = DateTime.UtcNow
        };

        if (danger.Severity >= DangerLevel.High)
        {
            protection.ShieldStrength = 100;
            protection.Message = "⚠️ DANGER DETECTED! I am shielding you with my full power!";
            protection.Effects.Add("Divine Shield Active");
            protection.Effects.Add("Damage Reduction: 50%");
        }
        else if (danger.Severity == DangerLevel.Medium)
        {
            protection.ShieldStrength = 50;
            protection.Message = "Be cautious! I am here to protect you.";
            protection.Effects.Add("Guardian's Ward");
            protection.Effects.Add("Damage Reduction: 25%");
        }
        else
        {
            protection.ShieldStrength = 25;
            protection.Message = "Stay alert. I sense potential danger ahead.";
            protection.Effects.Add("Watchful Eye");
        }

        return protection;
    }

    /// <summary>
    /// Generates player progression narrative
    /// </summary>
    public string GenerateProgressionNarrative(PlayerProgress progress)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine("  Guardian Angel Arielle's Assessment");
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine();

        if (progress.Level > progress.PreviousLevel)
        {
            sb.AppendLine($"🌟 Congratulations, brave soul! You have ascended to Level {progress.Level}!");
            sb.AppendLine("Your growth fills me with pride. Together, we grow stronger.");
        }

        sb.AppendLine();
        sb.AppendLine($"Current Level: {progress.Level}");
        sb.AppendLine($"Experience: {progress.Experience}/{progress.NextLevelExperience}");
        sb.AppendLine($"Challenges Overcome: {progress.ChallengesCompleted}");
        sb.AppendLine($"Guardian Bond Strength: {CalculateBondStrength(progress)}%");
        sb.AppendLine();

        if (progress.AchievementsUnlocked.Count > 0)
        {
            sb.AppendLine("Recent Achievements:");
            foreach (var achievement in progress.AchievementsUnlocked.Take(3))
            {
                sb.AppendLine($"  ✓ {achievement}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("I remain by your side, always watching, always protecting.");
        sb.AppendLine("═══════════════════════════════════════");

        return sb.ToString();
    }

    #region Private Helper Methods

    private void InitializeNarrativeLibrary()
    {
        _narrativeLibrary["welcome"] = new NarrativeElement
        {
            Title = "The Guardian's Welcome",
            Variants = new[]
            {
                "Welcome, brave soul, to The World of RA. I am Arielle, your Guardian Angel. Together, we shall write a legend.",
                "You have entered a realm of wonder and danger. Fear not, for I am with you every step of the way.",
                "I sense great potential within you. Let us embark on this journey together, hand in hand."
            }
        };

        _narrativeLibrary["combat"] = new NarrativeElement
        {
            Title = "Combat Guidance",
            Variants = new[]
            {
                "Steel yourself! Your enemy approaches. Strike true and I shall guard your back!",
                "Remember your training! Each move counts. I believe in your strength!",
                "The battle is joined! Fight with courage, for I am your shield!"
            }
        };

        _narrativeLibrary["victory"] = new NarrativeElement
        {
            Title = "Victory Celebration",
            Variants = new[]
            {
                "Magnificent! Your skill and determination have prevailed!",
                "Victory is yours! I knew you could do it. Your power grows with each triumph!",
                "Well done, champion! Another challenge conquered. Your legend grows!"
            }
        };

        _narrativeLibrary["protection"] = new NarrativeElement
        {
            Title = "Divine Protection",
            Variants = new[]
            {
                "DANGER! I am channeling my power to protect you! Stay behind my light!",
                "Fear not the darkness! My shield surrounds you! No harm shall reach you!",
                "You are in peril! I will not let you fall! My protection is absolute!"
            }
        };
    }

    private NarrativeResponse GenerateWelcomeNarrative(GameContext context)
    {
        var element = _narrativeLibrary["welcome"];
        return new NarrativeResponse
        {
            Text = element.Variants[_narrativeSequence++ % element.Variants.Length],
            Emotion = EmotionType.Welcoming,
            Duration = 5000
        };
    }

    private NarrativeResponse GenerateCombatGuidance(GameContext context)
    {
        var element = _narrativeLibrary["combat"];
        return new NarrativeResponse
        {
            Text = element.Variants[new Random().Next(element.Variants.Length)],
            Emotion = EmotionType.Encouraging,
            Duration = 3000,
            VisualEffect = "guardian_aura"
        };
    }

    private NarrativeResponse GeneratePuzzleHint(GameContext context)
    {
        var hints = new[]
        {
            "Look closely at the patterns. The answer reveals itself to patient eyes.",
            "Sometimes the solution lies in simplicity. Trust your intuition.",
            "I sense the answer is within your reach. Think about what you've learned.",
            "Each element has its place. Consider how they connect to one another."
        };

        return new NarrativeResponse
        {
            Text = hints[new Random().Next(hints.Length)],
            Emotion = EmotionType.Thoughtful,
            Duration = 4000
        };
    }

    private NarrativeResponse GenerateVictoryNarrative(GameContext context)
    {
        var element = _narrativeLibrary["victory"];
        return new NarrativeResponse
        {
            Text = element.Variants[new Random().Next(element.Variants.Length)],
            Emotion = EmotionType.Proud,
            Duration = 4000,
            VisualEffect = "victory_sparkle"
        };
    }

    private NarrativeResponse GenerateProtectionNarrative(GameContext context)
    {
        var element = _narrativeLibrary["protection"];
        return new NarrativeResponse
        {
            Text = element.Variants[new Random().Next(element.Variants.Length)],
            Emotion = EmotionType.Protective,
            Duration = 3000,
            VisualEffect = "divine_shield",
            AudioCue = "guardian_protection"
        };
    }

    private NarrativeResponse GenerateDiscoveryNarrative(GameContext context)
    {
        var discoveries = new[]
        {
            "Remarkable! You've uncovered something truly special! This discovery will serve you well.",
            "I sense the significance of this finding. Your keen perception is admirable!",
            "What fortune! This treasure shall aid us on our journey together.",
            "Your exploration has been rewarded! Such discoveries are rare and precious."
        };

        return new NarrativeResponse
        {
            Text = discoveries[new Random().Next(discoveries.Length)],
            Emotion = EmotionType.Excited,
            Duration = 4000,
            VisualEffect = "discovery_glow"
        };
    }

    private NarrativeResponse GenerateDefaultNarrative(GameContext context)
    {
        return new NarrativeResponse
        {
            Text = "I am here with you, watching over your journey.",
            Emotion = EmotionType.Calm,
            Duration = 3000
        };
    }

    private void RecordEvent(GameEvent gameEvent)
    {
        _eventHistory.Add(gameEvent);
        
        // Keep only last 100 events
        if (_eventHistory.Count > 100)
        {
            _eventHistory.RemoveAt(0);
        }
    }

    private static int CalculateBondStrength(PlayerProgress progress)
    {
        // Bond strength increases with time played, challenges completed, and level
        var baseStrength = Math.Min(progress.Level * 5, 50);
        var challengeBonus = Math.Min(progress.ChallengesCompleted / 2, 30);
        var timeBonus = Math.Min(progress.TotalPlayTime.TotalHours * 2, 20);
        
        return (int)(baseStrength + challengeBonus + timeBonus);
    }

    #endregion
}

#region Supporting Types

public class GameContext
{
    public Guid PlayerId { get; set; }
    public GameEventType EventType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum GameEventType
{
    PlayerEntering,
    CombatStarting,
    PuzzleSolving,
    LevelComplete,
    PlayerDanger,
    Discovery,
    Dialogue,
    RestPoint
}

public class NarrativeResponse
{
    public string Text { get; set; } = string.Empty;
    public EmotionType Emotion { get; set; }
    public int Duration { get; set; }
    public string? VisualEffect { get; set; }
    public string? AudioCue { get; set; }
}

public enum EmotionType
{
    Welcoming,
    Encouraging,
    Protective,
    Proud,
    Thoughtful,
    Excited,
    Calm,
    Urgent
}

public class NarrativeElement
{
    public string Title { get; set; } = string.Empty;
    public string[] Variants { get; set; } = Array.Empty<string>();
}

public class GameEvent
{
    public GameEventType EventType { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    public string NarrativeGenerated { get; set; } = string.Empty;
}

public class PlayerAction
{
    public string ActionType { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class GuidanceResponse
{
    public string ActionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int StrengthBonus { get; set; }
    public int PerceptionBonus { get; set; }
    public int CharismaBonus { get; set; }
    public int IntelligenceBonus { get; set; }
}

public class DangerContext
{
    public Guid PlayerId { get; set; }
    public DangerLevel Severity { get; set; }
    public string ThreatType { get; set; } = string.Empty;
}

public enum DangerLevel
{
    Low,
    Medium,
    High,
    Critical
}

public class ProtectionResult
{
    public DangerLevel DangerLevel { get; set; }
    public bool ProtectionApplied { get; set; }
    public int ShieldStrength { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Effects { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class PlayerProgress
{
    public int Level { get; set; }
    public int PreviousLevel { get; set; }
    public int Experience { get; set; }
    public int NextLevelExperience { get; set; }
    public int ChallengesCompleted { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public List<string> AchievementsUnlocked { get; set; } = new();
}

#endregion
