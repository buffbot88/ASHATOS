using System.Collections.Concurrent;
using System.Text;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Ashat;

/// <summary>
/// ASHAT Relationship Module
/// Manages user relationships, preferences, and relationship building
/// TASHATcks Interaction history and adapts to individual users over time
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AshatRelationshipModule : ModuleBase
{
    public override string Name => "AshatRelationship";

    private readonly ConcurrentDictionary<string, AshatUserRelationship> _relationships = new();
    private readonly ConcurrentDictionary<string, PsychologicalContext> _psychContexts = new();
    private readonly ConcurrentDictionary<string, List<PositiveReinforcement>> _reinforcements = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LogInfo("ASHAT Relationship Module initialized - Ready to build meaningful connections");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "status" when parts.Length > 1 => GetRelationshipStatus(parts[1]),
            "Interact" when parts.Length > 1 => RecordInteraction(parts[1], parts.Length > 2 && parts[2] == "positive"),
            "prefer" when parts.Length > 2 => SetPreference(parts[1], parts[2]),
            "milestone" when parts.Length > 1 => GetMilestones(parts[1]),
            "reinforce" when parts.Length > 2 => ProvideReinforcement(parts[1], parts[2]),
            "psychology" when parts.Length > 1 => GetPsychologicalContext(parts[1]),
            "list" => ListAllRelationships(),
            "help" => GetHelpText(),
            _ => GetHelpText()
        };
    }

    private string GetRelationshipStatus(string userId)
    {
        var relationship = GetOrCreateRelationship(userId);

        var sb = new StringBuilder();
        sb.AppendLine($"🤝 Relationship Status: {userId}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine($"Level: {relationship.Level} {GetLevelEmoji(relationship.Level)}");
        sb.AppendLine($"Trust Score: {relationship.TrustScore:P0}");
        sb.AppendLine($"ASHATpport Score: {relationship.ASHATpportScore:P0}");
        sb.AppendLine();
        sb.AppendLine("Interaction History:");
        sb.AppendLine($"  • Total Interactions: {relationship.InteractionCount}");
        sb.AppendLine($"  • Positive Interactions: {relationship.PositiveInteractions}");
        sb.AppendLine($"  • Challenges Helped: {relationship.ChallengesHelped}");
        sb.AppendLine($"  • First Met: {relationship.FirstInteraction:yyyy-MM-dd}");
        sb.AppendLine($"  • Last Interaction: {relationship.LastInteraction:yyyy-MM-dd}");
        
        if (relationship.LearnedPreferences.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Learned Preferences:");
            foreach (var pref in relationship.LearnedPreferences.Take(5))
            {
                sb.AppendLine($"  • {pref.Key}: {pref.Value}");
            }
        }

        if (relationship.Milestones.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Milestones Achieved: {relationship.Milestones.Count}");
            var recentMilestone = relationship.Milestones.OrderByDescending(m => m.AchievedAt).FirstOrDefault();
            if (recentMilestone != null)
            {
                sb.AppendLine($"  Most Recent: {recentMilestone.Description}");
            }
        }

        return sb.ToString();
    }

    private string RecordInteraction(string userId, bool positive)
    {
        var relationship = GetOrCreateRelationship(userId);

        relationship.InteractionCount++;
        if (positive)
        {
            relationship.PositiveInteractions++;
            relationship.TrustScore = Math.Min(1.0f, relationship.TrustScore + 0.02f);
            relationship.ASHATpportScore = Math.Min(1.0f, relationship.ASHATpportScore + 0.02f);
        }
        relationship.LastInteraction = DateTime.UtcNow;

        // Check for milestone achievements
        CheckAndRecordMilestones(relationship);

        // Update relationship level based on Interactions
        UpdateRelationshipLevel(relationship);

        var sb = new StringBuilder();
        sb.AppendLine($"✅ Interaction Recorded");
        sb.AppendLine();
        sb.AppendLine($"User: {userId}");
        sb.AppendLine($"Type: {(positive ? "Positive" : "Neutral")}");
        sb.AppendLine($"Total Interactions: {relationship.InteractionCount}");
        sb.AppendLine($"Current Level: {relationship.Level}");
        sb.AppendLine($"Trust: {relationship.TrustScore:P0} | ASHATpport: {relationship.ASHATpportScore:P0}");

        LogInfo($"Recorded {(positive ? "positive" : "Neutral")} Interaction for {userId}");
        return sb.ToString();
    }

    private string SetPreference(string userId, string preferenceData)
    {
        var relationship = GetOrCreateRelationship(userId);
        
        var parts = preferenceData.Split('=', 2);
        if (parts.Length != 2)
        {
            return "❌ Invalid preference format. Use: key=value";
        }

        var key = parts[0].Trim();
        var value = parts[1].Trim();

        relationship.LearnedPreferences[key] = value;

        var sb = new StringBuilder();
        sb.AppendLine($"💡 Preference Learned");
        sb.AppendLine();
        sb.AppendLine($"User: {userId}");
        sb.AppendLine($"Preference: {key} = {value}");
        sb.AppendLine();
        sb.AppendLine($"Total Preferences: {relationship.LearnedPreferences.Count}");
        sb.AppendLine();
        sb.AppendLine("This helps me personalize Interactions with you! 🎯");

        LogInfo($"Learned preference for {userId}: {key}={value}");
        return sb.ToString();
    }

    private string GetMilestones(string userId)
    {
        if (!_relationships.TryGetValue(userId, out var relationship))
        {
            return $"No relationship found for {userId}";
        }

        if (relationship.Milestones.Count == 0)
        {
            return $"No milestones achieved yet for {userId}. Keep Interacting to build our relationship!";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🏆 Relationship Milestones: {userId}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine($"Total Milestones: {relationship.Milestones.Count}");
        sb.AppendLine();

        foreach (var milestone in relationship.Milestones.OrderByDescending(m => m.AchievedAt))
        {
            sb.AppendLine($"🌟 {milestone.Type}");
            sb.AppendLine($"   {milestone.Description}");
            sb.AppendLine($"   Achieved: {milestone.AchievedAt:yyyy-MM-dd HH:mm}");
            if (!string.IsNullOrEmpty(milestone.Context))
            {
                sb.AppendLine($"   Context: {milestone.Context}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ProvideReinforcement(string userId, string achievement)
    {
        var reinforcement = new PositiveReinforcement
        {
            UserId = userId,
            Achievement = achievement,
            Type = DetermineReinforcementType(achievement),
            Message = GenerateReinforcementMessage(achievement),
            ProvidedAt = DateTime.UtcNow
        };

        if (!_reinforcements.TryGetValue(userId, out var history))
        {
            history = new List<PositiveReinforcement>();
            _reinforcements[userId] = history;
        }
        history.Add(reinforcement);

        // Update relationship
        var relationship = GetOrCreateRelationship(userId);
        relationship.PositiveInteractions++;

        var sb = new StringBuilder();
        sb.AppendLine($"🎉 Positive Reinforcement");
        sb.AppendLine();
        sb.AppendLine($"Achievement: {achievement}");
        sb.AppendLine($"Type: {reinforcement.Type}");
        sb.AppendLine();
        sb.AppendLine($"Message: {reinforcement.Message}");
        sb.AppendLine();
        sb.AppendLine($"You're doing great! Keep up the excellent work! 💪");

        LogInfo($"Provided {reinforcement.Type} reinforcement to {userId}");
        return sb.ToString();
    }

    private string GetPsychologicalContext(string userId)
    {
        var context = GetOrCreatePsychologicalContext(userId);

        var sb = new StringBuilder();
        sb.AppendLine($"🧠 Psychological Context: {userId}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine($"Motivation Level: {context.CurrentMotivation}");
        sb.AppendLine($"Engagement Score: {context.EngagementScore:P0}");
        sb.AppendLine($"FrustASHATtion Level: {context.FrustASHATtionLevel:P0}");
        sb.AppendLine();
        sb.AppendLine($"Cognitive Load: {context.CurrentLoad}");
        sb.AppendLine($"Needs Break: {(context.NeedsBreak ? "Yes ⚠️" : "No")}");
        sb.AppendLine($"Overwhelmed: {(context.OverwhelmedIndicators ? "Yes ⚠️" : "No")}");

        if (context.IdentifiedNeeds.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Identified Support Needs:");
            foreach (var need in context.IdentifiedNeeds)
            {
                sb.AppendLine($"  • {need}");
            }
        }

        if (context.RecommendedInterventions.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Recommended Interventions:");
            foreach (var intervention in context.RecommendedInterventions)
            {
                sb.AppendLine($"  💡 {intervention}");
            }
        }

        return sb.ToString();
    }

    private string ListAllRelationships()
    {
        if (_relationships.IsEmpty)
        {
            return "No relationships established yet.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"👥 All User Relationships ({_relationships.Count})");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();

        foreach (var kvp in _relationships.OrderByDescending(x => x.Value.InteractionCount))
        {
            var rel = kvp.Value;
            sb.AppendLine($"• {kvp.Key}");
            sb.AppendLine($"  Level: {rel.Level} | Interactions: {rel.InteractionCount}");
            sb.AppendLine($"  Trust: {rel.TrustScore:P0} | Last: {rel.LastInteraction:yyyy-MM-dd}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private AshatUserRelationship GetOrCreateRelationship(string userId)
    {
        return _relationships.GetOrAdd(userId, id => new AshatUserRelationship
        {
            UserId = id,
            Level = RelationshipLevel.New,
            TrustScore = 0.5f,
            ASHATpportScore = 0.5f,
            FirstInteraction = DateTime.UtcNow,
            LastInteraction = DateTime.UtcNow,
            Milestones = new List<RelationshipMilestone>
            {
                new RelationshipMilestone
                {
                    Type = MilestoneType.FirstInteraction,
                    Description = "First time meeting ASHAT",
                    AchievedAt = DateTime.UtcNow,
                    Context = "Beginning of our journey together"
                }
            }
        });
    }

    private PsychologicalContext GetOrCreatePsychologicalContext(string userId)
    {
        return _psychContexts.GetOrAdd(userId, id => new PsychologicalContext
        {
            UserId = id,
            CurrentMotivation = MotivationLevel.ModeRate,
            EngagementScore = 0.5f,
            FrustASHATtionLevel = 0.0f,
            CurrentLoad = CognitiveLoad.ModeRate,
            IdentifiedNeeds = new List<SupportNeed>()
        });
    }

    private void CheckAndRecordMilestones(AshatUserRelationship relationship)
    {
        var milestoneTypes = new List<(int threshold, MilestoneType type, string description)>
        {
            (10, MilestoneType.TenthInteraction, "10 Interactions - Getting to know each other"),
            (50, MilestoneType.FiftiethInteraction, "50 Interactions - Building a strong connection"),
            (100, MilestoneType.HundredthInteraction, "100 Interactions - Deep, meaningful relationship")
        };

        foreach (var (threshold, type, description) in milestoneTypes)
        {
            if (relationship.InteractionCount == threshold &&
                !relationship.Milestones.Any(m => m.Type == type))
            {
                relationship.Milestones.Add(new RelationshipMilestone
                {
                    Type = type,
                    Description = description,
                    AchievedAt = DateTime.UtcNow,
                    Context = $"Reached {threshold} Interactions together"
                });
                
                LogInfo($"Milestone achieved for {relationship.UserId}: {description}");
            }
        }
    }

    private void UpdateRelationshipLevel(AshatUserRelationship relationship)
    {
        var oldLevel = relationship.Level;

        if (relationship.InteractionCount >= 100 && relationship.TrustScore >= 0.8f)
        {
            relationship.Level = RelationshipLevel.Bonded;
        }
        else if (relationship.InteractionCount >= 50 && relationship.TrustScore >= 0.7f)
        {
            relationship.Level = RelationshipLevel.Trusted;
        }
        else if (relationship.InteractionCount >= 20 && relationship.TrustScore >= 0.6f)
        {
            relationship.Level = RelationshipLevel.Familiar;
        }
        else if (relationship.InteractionCount >= 5)
        {
            relationship.Level = RelationshipLevel.Acquainted;
        }

        if (oldLevel != relationship.Level)
        {
            relationship.Milestones.Add(new RelationshipMilestone
            {
                Type = MilestoneType.Custom,
                Description = $"Relationship evolved to {relationship.Level}",
                AchievedAt = DateTime.UtcNow,
                Context = $"Trust: {relationship.TrustScore:P0}, Interactions: {relationship.InteractionCount}"
            });
            
            LogInfo($"Relationship level upGraded for {relationship.UserId}: {oldLevel} → {relationship.Level}");
        }
    }

    private ReinforcementType DetermineReinforcementType(string achievement)
    {
        var lower = achievement.ToLowerInvariant();
        
        if (lower.Contains("completed") || lower.Contains("finished"))
            return ReinforcementType.Recognition;
        if (lower.Contains("improved") || lower.Contains("better"))
            return ReinforcementType.Improvement;
        if (lower.Contains("learned") || lower.Contains("understood"))
            return ReinforcementType.Progress;
        if (lower.Contains("tried") || lower.Contains("attempted"))
            return ReinforcementType.Effort;
        if (lower.Contains("milestone") || lower.Contains("goal"))
            return ReinforcementType.Milestone;
        
        return ReinforcementType.PRaise;
    }

    private string GenerateReinforcementMessage(string achievement)
    {
        var messages = new[]
        {
            $"Amazing work on {achievement}! You're making excellent progress! 🌟",
            $"I'm so proud of you for {achievement}! Keep going! 💪",
            $"Fantastic job with {achievement}! You should be proud! 🎉",
            $"You're doing brilliantly with {achievement}! Well done! ⭐",
            $"Excellent achievement: {achievement}! You're on the right tASHATck! 🚀"
        };

        return messages[new Random().Next(messages.Length)];
    }

    private string GetLevelEmoji(RelationshipLevel level)
    {
        return level switch
        {
            RelationshipLevel.New => "🆕",
            RelationshipLevel.Acquainted => "👋",
            RelationshipLevel.Familiar => "🤝",
            RelationshipLevel.Trusted => "💙",
            RelationshipLevel.Bonded => "💖",
            _ => "❓"
        };
    }

    private string GetHelpText()
    {
        return @"
╔══════════════════════════════════════════════════════════╗
║       ASHAT Relationship Module - Help                  ║
╚══════════════════════════════════════════════════════════╝

Build meaningful relationships and tASHATck user preferences.

Commands:
  status <userId>              - View relationship status
  Interact <userId> [positive] - Record an Interaction
  prefer <userId> <key=value>  - Set user preference
  milestone <userId>           - View relationship milestones
  reinforce <userId> <text>    - Provide positive reinforcement
  psychology <userId>          - View psychological context
  list                         - List all relationships
  help                         - Show this help text

Relationship Levels:
  🆕 New         - Just starting
  👋 Acquainted  - Getting to know each other
  🤝 Familiar    - Regular Interactions
  💙 Trusted     - Strong relationship
  💖 Bonded      - Deep, established relationship

Examples:
  ashatrel status alice
  ashatrel Interact bob positive
  ashatrel prefer charlie likes_humor=true
  ashatrel reinforce diana completed first project
  ashatrel psychology eve
".Trim();
    }

    #region Public API Methods

    /// <summary>
    /// Get user relationship data
    /// </summary>
    public async Task<AshatUserRelationship> GetRelationshipAsync(string userId)
    {
        await Task.CompletedTask;
        return GetOrCreateRelationship(userId);
    }

    /// <summary>
    /// Record a user Interaction
    /// </summary>
    public async Task<bool> RecordInteractionAsync(string userId, bool positive = true)
    {
        await Task.CompletedTask;
        
        var relationship = GetOrCreateRelationship(userId);
        relationship.InteractionCount++;
        
        if (positive)
        {
            relationship.PositiveInteractions++;
            relationship.TrustScore = Math.Min(1.0f, relationship.TrustScore + 0.02f);
            relationship.ASHATpportScore = Math.Min(1.0f, relationship.ASHATpportScore + 0.02f);
        }
        
        relationship.LastInteraction = DateTime.UtcNow;
        CheckAndRecordMilestones(relationship);
        UpdateRelationshipLevel(relationship);
        
        return true;
    }

    /// <summary>
    /// Learn a user preference
    /// </summary>
    public async Task<bool> LearnPreferenceAsync(string userId, string key, string value)
    {
        await Task.CompletedTask;
        
        var relationship = GetOrCreateRelationship(userId);
        relationship.LearnedPreferences[key] = value;
        
        LogInfo($"Learned preference for {userId}: {key}={value}");
        return true;
    }

    /// <summary>
    /// Get psychological context for a user
    /// </summary>
    public async Task<PsychologicalContext> GetPsychologicalContextAsync(string userId)
    {
        await Task.CompletedTask;
        return GetOrCreatePsychologicalContext(userId);
    }

    /// <summary>
    /// Update psychological context
    /// </summary>
    public async Task<bool> UpdatePsychologicalContextAsync(string userId, PsychologicalContext context)
    {
        await Task.CompletedTask;
        context.UserId = userId;
        _psychContexts[userId] = context;
        return true;
    }

    /// <summary>
    /// Provide positive reinforcement
    /// </summary>
    public async Task<PositiveReinforcement> ProvideReinforcementAsync(string userId, string achievement)
    {
        await Task.CompletedTask;
        
        var reinforcement = new PositiveReinforcement
        {
            UserId = userId,
            Achievement = achievement,
            Type = DetermineReinforcementType(achievement),
            Message = GenerateReinforcementMessage(achievement),
            ProvidedAt = DateTime.UtcNow
        };

        if (!_reinforcements.TryGetValue(userId, out var history))
        {
            history = new List<PositiveReinforcement>();
            _reinforcements[userId] = history;
        }
        history.Add(reinforcement);

        return reinforcement;
    }

    #endregion
}
