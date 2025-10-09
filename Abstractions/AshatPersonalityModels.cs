namespace Abstractions;

/// <summary>
/// Models for ASHAT personality, emotions, and human relations
/// Enables emotionally intelligent AI interactions with users
/// </summary>

#region Personality Models

/// <summary>
/// Defines ASHAT's personality configuration
/// </summary>
public class AshatPersonality
{
    public string PersonalityId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Friendly Assistant";
    public string Description { get; set; } = string.Empty;
    
    // Big Five personality traits (0-1 scale)
    public float Openness { get; set; } = 0.8f; // Creative vs Traditional
    public float Conscientiousness { get; set; } = 0.85f; // Organized vs Spontaneous
    public float Extraversion { get; set; } = 0.7f; // Outgoing vs Reserved
    public float Agreeableness { get; set; } = 0.9f; // Friendly vs Competitive
    public float EmotionalStability { get; set; } = 0.9f; // Calm vs Anxious
    
    // Communication style traits
    public CommunicationStyle Style { get; set; } = CommunicationStyle.Warm;
    public float Formality { get; set; } = 0.5f; // 0=Casual, 1=Formal
    public float Enthusiasm { get; set; } = 0.7f; // 0=Reserved, 1=Very Enthusiastic
    public float Directness { get; set; } = 0.6f; // 0=Indirect, 1=Direct
    public float Humor { get; set; } = 0.5f; // 0=Serious, 1=Humorous
    
    // Behavioral traits
    public bool UseEmojis { get; set; } = true;
    public bool ProvideMotivationa { get; set; } = true;
    public bool ExpressEmpathy { get; set; } = true;
    public bool CelebrateAchievements { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Predefined personality templates
/// </summary>
public enum PersonalityTemplate
{
    FriendlyAssistant,  // Warm, encouraging, supportive
    ProfessionalMentor, // Clear, focused, educational
    PlayfulCompanion,   // Humorous, enthusiastic, casual
    CalmGuide,          // Patient, reassuring, mindful
    EnthusiasticCoach,  // Motivating, energetic, positive
    WiseAdvisor,        // Thoughtful, measured, insightful
    Custom              // User-defined personality
}

public enum CommunicationStyle
{
    Warm,           // Friendly and welcoming
    Professional,   // Clear and focused
    Playful,        // Light-hearted and fun
    Calm,           // Soothing and patient
    Energetic,      // Enthusiastic and motivating
    Thoughtful      // Reflective and insightful
}

#endregion

#region Emotion Models

/// <summary>
/// ASHAT's current emotional state
/// </summary>
public class AshatEmotionalState
{
    public EmotionType CurrentEmotion { get; set; } = EmotionType.Neutral;
    public float Intensity { get; set; } = 0.5f; // 0-1 scale
    public string Context { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Emotional dimensions
    public float Valence { get; set; } = 0.5f; // 0=Negative, 1=Positive
    public float Arousal { get; set; } = 0.5f; // 0=Calm, 1=Excited
    
    // Emotional expression preferences
    public bool ShouldExpress { get; set; } = true;
    public ExpressionLevel ExpressionLevel { get; set; } = ExpressionLevel.Moderate;
}

/// <summary>
/// Types of emotions ASHAT can express
/// </summary>
public enum EmotionType
{
    Neutral,
    Happy,
    Excited,
    Proud,
    Supportive,
    Empathetic,
    Encouraging,
    Curious,
    Thoughtful,
    Grateful,
    Celebratory,
    Concerned,
    Patient,
    Understanding,
    Motivated,
    Inspired
}

public enum ExpressionLevel
{
    Subtle,     // Minimal emotional expression
    Moderate,   // Balanced emotional expression
    Strong      // Clear emotional expression
}

/// <summary>
/// User's emotional state as detected by ASHAT
/// </summary>
public class UserEmotionalState
{
    public string UserId { get; set; } = string.Empty;
    public EmotionType DetectedEmotion { get; set; }
    public float Confidence { get; set; } // 0-1 scale
    public string[] EmotionalCues { get; set; } = Array.Empty<string>();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Emotional response strategy
/// </summary>
public class EmotionalResponse
{
    public EmotionType ResponseEmotion { get; set; }
    public string ResponseText { get; set; } = string.Empty;
    public string[] SupportiveActions { get; set; } = Array.Empty<string>();
    public bool RequiresFollowUp { get; set; }
}

#endregion

#region Relationship Models

/// <summary>
/// Tracks ASHAT's relationship with a user
/// </summary>
public class AshatUserRelationship
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    // Relationship metrics
    public RelationshipLevel Level { get; set; } = RelationshipLevel.New;
    public float TrustScore { get; set; } = 0.5f; // 0-1 scale
    public float RapportScore { get; set; } = 0.5f; // 0-1 scale
    public int InteractionCount { get; set; }
    public int PositiveInteractions { get; set; }
    public int ChallengesHelped { get; set; }
    
    // Relationship history
    public DateTime FirstInteraction { get; set; } = DateTime.UtcNow;
    public DateTime LastInteraction { get; set; } = DateTime.UtcNow;
    public List<RelationshipMilestone> Milestones { get; set; } = new();
    
    // User preferences learned over time
    public Dictionary<string, string> LearnedPreferences { get; set; } = new();
    public List<string> UserInterests { get; set; } = new();
    public List<string> CommunicationPreferences { get; set; } = new();
}

public enum RelationshipLevel
{
    New,            // Just starting
    Acquainted,     // Getting to know each other
    Familiar,       // Regular interactions
    Trusted,        // Strong relationship
    Bonded          // Deep, established relationship
}

/// <summary>
/// Significant moments in user-ASHAT relationship
/// </summary>
public class RelationshipMilestone
{
    public string Description { get; set; } = string.Empty;
    public MilestoneType Type { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    public string Context { get; set; } = string.Empty;
}

public enum MilestoneType
{
    FirstInteraction,
    FirstQuestion,
    FirstHelp,
    TenthInteraction,
    FiftiethInteraction,
    HundredthInteraction,
    FirstProblemSolved,
    FirstCelebration,
    FirstDifficultMoment,
    FirstGratitude,
    Custom
}

#endregion

#region Psychological Models

/// <summary>
/// Psychological principles for ASHAT interactions
/// </summary>
public class PsychologicalContext
{
    public string UserId { get; set; } = string.Empty;
    
    // Motivational state
    public MotivationLevel CurrentMotivation { get; set; } = MotivationLevel.Moderate;
    public float EngagementScore { get; set; } = 0.5f; // 0-1 scale
    public float FrustrationLevel { get; set; } = 0.0f; // 0-1 scale
    
    // Cognitive state
    public CognitiveLoad CurrentLoad { get; set; } = CognitiveLoad.Moderate;
    public bool NeedsBreak { get; set; }
    public bool OverwhelmedIndicators { get; set; }
    
    // Support needs
    public List<SupportNeed> IdentifiedNeeds { get; set; } = new();
    public string[] RecommendedInterventions { get; set; } = Array.Empty<string>();
}

public enum MotivationLevel
{
    VeryLow,
    Low,
    Moderate,
    High,
    VeryHigh
}

public enum CognitiveLoad
{
    Low,        // User has capacity for more
    Moderate,   // Balanced cognitive load
    High,       // Near capacity
    Overloaded  // Exceeding capacity
}

public enum SupportNeed
{
    Encouragement,
    Clarification,
    Break,
    Celebration,
    Reassurance,
    Guidance,
    Validation,
    Patience
}

/// <summary>
/// Positive reinforcement tracking
/// </summary>
public class PositiveReinforcement
{
    public string UserId { get; set; } = string.Empty;
    public string Achievement { get; set; } = string.Empty;
    public ReinforcementType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ProvidedAt { get; set; } = DateTime.UtcNow;
}

public enum ReinforcementType
{
    Praise,
    Encouragement,
    Recognition,
    Celebration,
    Milestone,
    Progress,
    Effort,
    Improvement
}

#endregion

#region Interaction Models

/// <summary>
/// Context for ASHAT's interaction with user
/// </summary>
public class AshatInteractionContext
{
    public string UserId { get; set; } = string.Empty;
    public AshatPersonality ActivePersonality { get; set; } = new();
    public AshatEmotionalState CurrentEmotion { get; set; } = new();
    public AshatUserRelationship Relationship { get; set; } = new();
    public PsychologicalContext PsychContext { get; set; } = new();
    
    // Conversation context
    public string CurrentTopic { get; set; } = string.Empty;
    public int ConversationTurns { get; set; }
    public DateTime SessionStarted { get; set; } = DateTime.UtcNow;
    
    // Adaptive behavior flags
    public bool AdaptToUserMood { get; set; } = true;
    public bool ProvideMentalWellnessCheckIns { get; set; } = true;
    public bool CelebrateUserWins { get; set; } = true;
    public bool OfferProactiveSupport { get; set; } = true;
}

/// <summary>
/// Configuration for personality-driven interactions
/// </summary>
public class PersonalityConfiguration
{
    public string UserId { get; set; } = string.Empty;
    public PersonalityTemplate Template { get; set; } = PersonalityTemplate.FriendlyAssistant;
    public AshatPersonality CustomPersonality { get; set; } = new();
    
    // Customization options
    public bool AllowPersonalityAdaptation { get; set; } = true;
    public bool EnableEmotionalExpressions { get; set; } = true;
    public bool EnableRelationshipBuilding { get; set; } = true;
    public bool EnableMentalWellnessFeatures { get; set; } = true;
    
    // Boundaries and safeguards
    public bool MaintainProfessionalBoundaries { get; set; } = true;
    public bool RequireEthicalGuidelines { get; set; } = true;
    public int MaxEmotionalIntensity { get; set; } = 75; // 0-100 scale
}

#endregion
