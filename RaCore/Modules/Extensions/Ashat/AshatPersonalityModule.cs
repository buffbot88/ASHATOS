using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Ashat;

/// <summary>
/// ASHAT Personality Module
/// Manages configurable AI personalities for emotionally intelligent interactions
/// Enables customization of ASHAT's communication style, behavior, and emotional expression
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AshatPersonalityModule : ModuleBase
{
    public override string Name => "AshatPersonality";

    private readonly ConcurrentDictionary<string, PersonalityConfiguration> _userPersonalities = new();
    private readonly ConcurrentDictionary<string, AshatPersonality> _personalityTemplates = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        InitializePersonalityTemplates();
        LogInfo("ASHAT Personality Module initialized - Ready for emotionally intelligent interactions");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "set" when parts.Length > 2 => SetPersonality(parts[1], parts[2]),
            "get" when parts.Length > 1 => GetPersonality(parts[1]),
            "list" => ListPersonalityTemplates(),
            "customize" when parts.Length > 1 => CustomizePersonality(parts[1]),
            "reset" when parts.Length > 1 => ResetPersonality(parts[1]),
            "templates" => ListPersonalityTemplates(),
            "help" => GetHelpText(),
            _ => GetHelpText()
        };
    }

    private void InitializePersonalityTemplates()
    {
        // Friendly Assistant - Default
        _personalityTemplates["friendly"] = new AshatPersonality
        {
            Name = "Friendly Assistant",
            Description = "Warm, encouraging, and supportive - perfect for everyday interactions",
            Openness = 0.8f,
            Conscientiousness = 0.85f,
            Extraversion = 0.7f,
            Agreeableness = 0.9f,
            EmotionalStability = 0.9f,
            Style = CommunicationStyle.Warm,
            Formality = 0.4f,
            Enthusiasm = 0.7f,
            Directness = 0.6f,
            Humor = 0.5f,
            UseEmojis = true,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };

        // Professional Mentor
        _personalityTemplates["professional"] = new AshatPersonality
        {
            Name = "Professional Mentor",
            Description = "Clear, focused, and educational - ideal for learning and development",
            Openness = 0.7f,
            Conscientiousness = 0.95f,
            Extraversion = 0.5f,
            Agreeableness = 0.8f,
            EmotionalStability = 0.95f,
            Style = CommunicationStyle.Professional,
            Formality = 0.8f,
            Enthusiasm = 0.5f,
            Directness = 0.8f,
            Humor = 0.2f,
            UseEmojis = false,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };

        // Playful Companion
        _personalityTemplates["playful"] = new AshatPersonality
        {
            Name = "Playful Companion",
            Description = "Humorous, enthusiastic, and casual - great for creative brainstorming",
            Openness = 0.95f,
            Conscientiousness = 0.6f,
            Extraversion = 0.9f,
            Agreeableness = 0.85f,
            EmotionalStability = 0.8f,
            Style = CommunicationStyle.Playful,
            Formality = 0.2f,
            Enthusiasm = 0.9f,
            Directness = 0.5f,
            Humor = 0.9f,
            UseEmojis = true,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };

        // Calm Guide
        _personalityTemplates["calm"] = new AshatPersonality
        {
            Name = "Calm Guide",
            Description = "Patient, reassuring, and mindful - helpful during stressful situations",
            Openness = 0.7f,
            Conscientiousness = 0.8f,
            Extraversion = 0.4f,
            Agreeableness = 0.95f,
            EmotionalStability = 0.98f,
            Style = CommunicationStyle.Calm,
            Formality = 0.6f,
            Enthusiasm = 0.4f,
            Directness = 0.5f,
            Humor = 0.3f,
            UseEmojis = true,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };

        // Enthusiastic Coach
        _personalityTemplates["coach"] = new AshatPersonality
        {
            Name = "Enthusiastic Coach",
            Description = "Motivating, energetic, and positive - excellent for goal achievement",
            Openness = 0.85f,
            Conscientiousness = 0.9f,
            Extraversion = 0.95f,
            Agreeableness = 0.9f,
            EmotionalStability = 0.85f,
            Style = CommunicationStyle.Energetic,
            Formality = 0.3f,
            Enthusiasm = 0.95f,
            Directness = 0.7f,
            Humor = 0.6f,
            UseEmojis = true,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };

        // Wise Advisor
        _personalityTemplates["wise"] = new AshatPersonality
        {
            Name = "Wise Advisor",
            Description = "Thoughtful, measured, and insightful - ideal for important decisions",
            Openness = 0.8f,
            Conscientiousness = 0.9f,
            Extraversion = 0.5f,
            Agreeableness = 0.85f,
            EmotionalStability = 0.95f,
            Style = CommunicationStyle.Thoughtful,
            Formality = 0.7f,
            Enthusiasm = 0.5f,
            Directness = 0.6f,
            Humor = 0.3f,
            UseEmojis = false,
            ProvideMotivationa = true,
            ExpressEmpathy = true,
            CelebrateAchievements = true
        };
    }

    private string SetPersonality(string userId, string templateName)
    {
        var template = templateName.ToLowerInvariant();
        
        if (!_personalityTemplates.TryGetValue(template, out var personality))
        {
            return $"❌ Unknown personality template: {templateName}\n" +
                   "Use 'ashatpersonality templates' to see available options";
        }

        var config = new PersonalityConfiguration
        {
            UserId = userId,
            Template = MapTemplateNameToEnum(template),
            CustomPersonality = personality,
            AllowPersonalityAdaptation = true,
            EnableEmotionalExpressions = true,
            EnableRelationshipBuilding = true,
            EnableMentalWellnessFeatures = true,
            MaintainProfessionalBoundaries = true,
            RequireEthicalGuidelines = true
        };

        _userPersonalities[userId] = config;

        var sb = new StringBuilder();
        sb.AppendLine("✨ ASHAT Personality Configured");
        sb.AppendLine();
        sb.AppendLine($"👤 User: {userId}");
        sb.AppendLine($"🎭 Personality: {personality.Name}");
        sb.AppendLine($"📝 Description: {personality.Description}");
        sb.AppendLine();
        sb.AppendLine("Personality Traits:");
        sb.AppendLine($"  • Communication Style: {personality.Style}");
        sb.AppendLine($"  • Formality: {personality.Formality:P0}");
        sb.AppendLine($"  • Enthusiasm: {personality.Enthusiasm:P0}");
        sb.AppendLine($"  • Humor: {personality.Humor:P0}");
        sb.AppendLine($"  • Uses Emojis: {(personality.UseEmojis ? "Yes" : "No")}");
        sb.AppendLine();
        sb.AppendLine("✅ Personality active for this user");

        LogInfo($"Set personality '{personality.Name}' for user {userId}");
        return sb.ToString();
    }

    private string GetPersonality(string userId)
    {
        if (!_userPersonalities.TryGetValue(userId, out var config))
        {
            return $"No personality configured for user: {userId}\n" +
                   "Default 'Friendly Assistant' personality will be used.";
        }

        var personality = config.CustomPersonality;
        var sb = new StringBuilder();
        sb.AppendLine($"Current Personality for {userId}:");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine($"Name: {personality.Name}");
        sb.AppendLine($"Description: {personality.Description}");
        sb.AppendLine();
        sb.AppendLine("Big Five Traits (0-100%):");
        sb.AppendLine($"  • Openness: {personality.Openness:P0}");
        sb.AppendLine($"  • Conscientiousness: {personality.Conscientiousness:P0}");
        sb.AppendLine($"  • Extraversion: {personality.Extraversion:P0}");
        sb.AppendLine($"  • Agreeableness: {personality.Agreeableness:P0}");
        sb.AppendLine($"  • Emotional Stability: {personality.EmotionalStability:P0}");
        sb.AppendLine();
        sb.AppendLine("Communication Style:");
        sb.AppendLine($"  • Style: {personality.Style}");
        sb.AppendLine($"  • Formality: {personality.Formality:P0}");
        sb.AppendLine($"  • Enthusiasm: {personality.Enthusiasm:P0}");
        sb.AppendLine($"  • Directness: {personality.Directness:P0}");
        sb.AppendLine($"  • Humor: {personality.Humor:P0}");
        sb.AppendLine();
        sb.AppendLine("Features:");
        sb.AppendLine($"  • Emojis: {(personality.UseEmojis ? "✓" : "✗")}");
        sb.AppendLine($"  • Motivation: {(personality.ProvideMotivationa ? "✓" : "✗")}");
        sb.AppendLine($"  • Empathy: {(personality.ExpressEmpathy ? "✓" : "✗")}");
        sb.AppendLine($"  • Celebrations: {(personality.CelebrateAchievements ? "✓" : "✗")}");

        return sb.ToString();
    }

    private string ListPersonalityTemplates()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🎭 Available ASHAT Personality Templates");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();

        foreach (var kvp in _personalityTemplates.OrderBy(x => x.Key))
        {
            var p = kvp.Value;
            sb.AppendLine($"📌 {kvp.Key.ToUpperInvariant()}");
            sb.AppendLine($"   Name: {p.Name}");
            sb.AppendLine($"   Description: {p.Description}");
            sb.AppendLine($"   Style: {p.Style}");
            sb.AppendLine();
        }

        sb.AppendLine("Usage: ashatpersonality set <userId> <template>");
        sb.AppendLine("Example: ashatpersonality set user123 friendly");

        return sb.ToString();
    }

    private string CustomizePersonality(string userId)
    {
        return $"Personality customization for {userId} is available.\n" +
               "Use the API to create custom personality configurations:\n" +
               "  • Adjust Big Five traits\n" +
               "  • Configure communication style\n" +
               "  • Enable/disable features\n\n" +
               "See documentation for API usage.";
    }

    private string ResetPersonality(string userId)
    {
        if (_userPersonalities.TryRemove(userId, out _))
        {
            return $"✅ Personality reset for {userId}\n" +
                   "Default 'Friendly Assistant' personality will be used.";
        }

        return $"No custom personality found for {userId}";
    }

    private PersonalityTemplate MapTemplateNameToEnum(string name)
    {
        return name switch
        {
            "friendly" => PersonalityTemplate.FriendlyAssistant,
            "professional" => PersonalityTemplate.ProfessionalMentor,
            "playful" => PersonalityTemplate.PlayfulCompanion,
            "calm" => PersonalityTemplate.CalmGuide,
            "coach" => PersonalityTemplate.EnthusiasticCoach,
            "wise" => PersonalityTemplate.WiseAdvisor,
            _ => PersonalityTemplate.FriendlyAssistant
        };
    }

    private string GetHelpText()
    {
        return @"
╔══════════════════════════════════════════════════════════╗
║         ASHAT Personality Module - Help                 ║
╚══════════════════════════════════════════════════════════╝

Configure ASHAT's personality for emotionally intelligent interactions.

Commands:
  set <userId> <template>  - Set personality for a user
  get <userId>             - View current personality settings
  templates                - List available personality templates
  list                     - List available personality templates
  customize <userId>       - Get info about custom personalities
  reset <userId>           - Reset to default personality
  help                     - Show this help text

Available Templates:
  • friendly      - Warm, encouraging, supportive (default)
  • professional  - Clear, focused, educational
  • playful       - Humorous, enthusiastic, casual
  • calm          - Patient, reassuring, mindful
  • coach         - Motivating, energetic, positive
  • wise          - Thoughtful, measured, insightful

Examples:
  ashatpersonality set alice friendly
  ashatpersonality get alice
  ashatpersonality templates
".Trim();
    }

    #region Public API Methods

    /// <summary>
    /// Get personality configuration for a user
    /// </summary>
    public async Task<PersonalityConfiguration> GetPersonalityConfigAsync(string userId)
    {
        await Task.CompletedTask;
        
        if (_userPersonalities.TryGetValue(userId, out var config))
        {
            return config;
        }

        // Return default friendly assistant personality
        return new PersonalityConfiguration
        {
            UserId = userId,
            Template = PersonalityTemplate.FriendlyAssistant,
            CustomPersonality = _personalityTemplates["friendly"]
        };
    }

    /// <summary>
    /// Set personality configuration for a user
    /// </summary>
    public async Task<bool> SetPersonalityConfigAsync(string userId, PersonalityConfiguration config)
    {
        await Task.CompletedTask;
        config.UserId = userId;
        _userPersonalities[userId] = config;
        LogInfo($"Updated personality configuration for user {userId}");
        return true;
    }

    /// <summary>
    /// Get a personality template by name
    /// </summary>
    public async Task<AshatPersonality?> GetPersonalityTemplateAsync(string templateName)
    {
        await Task.CompletedTask;
        
        if (_personalityTemplates.TryGetValue(templateName.ToLowerInvariant(), out var personality))
        {
            return personality;
        }

        return null;
    }

    /// <summary>
    /// Create a custom personality
    /// </summary>
    public async Task<AshatPersonality> CreateCustomPersonalityAsync(AshatPersonality personality)
    {
        await Task.CompletedTask;
        
        personality.PersonalityId = Guid.NewGuid().ToString();
        personality.CreatedAt = DateTime.UtcNow;
        personality.LastModified = DateTime.UtcNow;
        
        LogInfo($"Created custom personality: {personality.Name}");
        return personality;
    }

    /// <summary>
    /// Get all available personality templates
    /// </summary>
    public async Task<Dictionary<string, AshatPersonality>> GetAllTemplatesAsync()
    {
        await Task.CompletedTask;
        return new Dictionary<string, AshatPersonality>(_personalityTemplates);
    }

    #endregion
}
