namespace Abstractions;

/// <summary>
/// Phase 6: AI Creation, Analytics, Education, and Compliance Interfaces
/// Provides conversational building, content curation, player modeling, adaptive difficulty, 
/// documentation generation, AI mentoring, and automated compliance
/// </summary>

#region Conversational Builder Module

/// <summary>
/// Interface for natural dialogue-based game creation
/// </summary>
public interface IConversationalBuilderModule : IDisposable
{
    /// <summary>
    /// Start a new building session
    /// </summary>
    Task<BuilderSession> StartSessionAsync(Guid userId);
    
    /// <summary>
    /// Send a message in the building conversation
    /// </summary>
    Task<BuilderResponse> SendMessageAsync(Guid sessionId, string message);
    
    /// <summary>
    /// End a building session
    /// </summary>
    Task<bool> EndSessionAsync(Guid sessionId);
    
    /// <summary>
    /// Get current session context
    /// </summary>
    Task<BuilderContext> GetSessionContextAsync(Guid sessionId);
    
    /// <summary>
    /// Update session context
    /// </summary>
    Task<bool> UpdateContextAsync(Guid sessionId, ContextUpdate update);
    
    /// <summary>
    /// Get preview of current game state
    /// </summary>
    Task<Phase6GamePreview> GetCurrentPreviewAsync(Guid sessionId);
    
    /// <summary>
    /// Apply changes and create/update game
    /// </summary>
    Task<bool> ApplyChangesAsync(Guid sessionId);
}

public class BuilderSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public List<ConversationTurn> Conversation { get; set; } = new();
    public BuilderContext Context { get; set; } = new();
    public bool IsActive { get; set; }
}

public class ConversationTurn
{
    public string UserMessage { get; set; } = string.Empty;
    public string AIResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class BuilderResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<ClarificationQuestion> Questions { get; set; } = new();
    public Phase6GamePreview? UpdatedPreview { get; set; }
}

public class ClarificationQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public class BuilderContext
{
    public Guid? GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public Dictionary<string, object> Decisions { get; set; } = new();
}

public class ContextUpdate
{
    public string? GameName { get; set; }
    public string? GameType { get; set; }
    public string? Theme { get; set; }
    public List<string>? AddFeatures { get; set; }
    public Dictionary<string, object>? UpdateDecisions { get; set; }
}

// GamePreview type already exists but may differ - using Phase6GamePreview for clarity
public class Phase6GamePreview
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public List<string> Characters { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public string EstimatedComplexity { get; set; } = string.Empty;
    public byte[]? ConceptArt { get; set; }
}

#endregion

#region Content Curator Module

/// <summary>
/// Interface for AI-driven quality assessment and improvement
/// </summary>
public interface IContentCuratorModule : IDisposable
{
    /// <summary>
    /// Assess quality of content
    /// </summary>
    Task<QualityScore> AssessContentAsync(ContentItem content);
    
    /// <summary>
    /// Suggest improvements for content
    /// </summary>
    Task<IEnumerable<Improvement>> SuggestImprovementsAsync(ContentItem content);
    
    /// <summary>
    /// Apply AI improvements to content
    /// </summary>
    Task<ContentItem> ImproveContentAsync(ContentItem content);
    
    /// <summary>
    /// Validate content against rules
    /// </summary>
    Task<bool> ValidateContentAsync(ContentItem content, ValidationRules rules);
    
    /// <summary>
    /// Moderate content for appropriateness
    /// </summary>
    Task<Phase6ModerationResult> ModerateContentAsync(ContentItem content);
}

public class ContentItem
{
    public Guid Id { get; set; }
    public ContentItemType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class QualityScore
{
    public float OverallScore { get; set; } // 0-1
    public Dictionary<string, float> DimensionScores { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class Improvement
{
    public ImprovementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public float ImpactScore { get; set; }
    public string SuggestedChange { get; set; } = string.Empty;
}

public class ValidationRules
{
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public List<string> RequiredElements { get; set; } = new();
    public List<string> ForbiddenWords { get; set; } = new();
}

// Phase6ModerationResult to avoid conflict  
public class Phase6ModerationResult
{
    public bool IsAppropriate { get; set; }
    public List<ContentFlag> Flags { get; set; } = new();
    public float ConfidenceScore { get; set; }
}

public class ContentFlag
{
    public string Category { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public float Severity { get; set; }
}

public enum ContentItemType
{
    Text,
    Dialogue,
    Description,
    Quest,
    Story,
    Code
}

public enum ImprovementType
{
    Grammar,
    Clarity,
    Engagement,
    Consistency,
    Creativity,
    Balance
}

#endregion

#region Style Transfer Module

/// <summary>
/// Interface for applying artistic styles to content
/// </summary>
public interface IStyleTransferModule : IDisposable
{
    /// <summary>
    /// Apply style to content
    /// </summary>
    Task<StyledContent> ApplyStyleAsync(ContentItem content, StyleDefinition style);
    
    /// <summary>
    /// Get available styles
    /// </summary>
    Task<IEnumerable<StyleDefinition>> GetAvailableStylesAsync();
    
    /// <summary>
    /// Create custom style from examples
    /// </summary>
    Task<StyleDefinition> CreateCustomStyleAsync(List<ContentItem> examples);
}

public class StyleDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StyleCategory Category { get; set; }
    public Dictionary<string, float> Parameters { get; set; } = new();
}

public class StyledContent
{
    public ContentItem OriginalContent { get; set; } = new();
    public string StyledText { get; set; } = string.Empty;
    public StyleDefinition AppliedStyle { get; set; } = new();
}

public enum StyleCategory
{
    Formal,
    Casual,
    Poetic,
    Technical,
    Dramatic,
    Humorous
}

#endregion

#region Player Modeling Module

/// <summary>
/// Interface for ML-based player profiling
/// </summary>
public interface IPlayerModelingModule : IDisposable
{
    /// <summary>
    /// Get comprehensive player model
    /// </summary>
    Task<PlayerModel> GetPlayerModelAsync(Guid playerId);
    
    /// <summary>
    /// Update player model with new activity
    /// </summary>
    Task<bool> UpdatePlayerModelAsync(Guid playerId, PlayerActivity activity);
    
    /// <summary>
    /// Predict churn probability
    /// </summary>
    Task<float> PredictChurnProbabilityAsync(Guid playerId);
    
    /// <summary>
    /// Get personalized recommendations
    /// </summary>
    Task<IEnumerable<Recommendation>> GetRecommendationsAsync(Guid playerId);
    
    /// <summary>
    /// Get player segment
    /// </summary>
    Task<PlayerSegment> GetPlayerSegmentAsync(Guid playerId);
    
    /// <summary>
    /// Get all player segments
    /// </summary>
    Task<IEnumerable<PlayerSegment>> GetAllSegmentsAsync();
}

public class PlayerModel
{
    public Guid PlayerId { get; set; }
    public SkillLevel SkillLevel { get; set; }
    public PlayStyle PreferredPlayStyle { get; set; }
    public Dictionary<string, float> Preferences { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();
    public EngagementMetrics Engagement { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class EngagementMetrics
{
    public int TotalPlayTimeMinutes { get; set; }
    public int SessionsPlayed { get; set; }
    public float AverageSessionLengthMinutes { get; set; }
    public int DaysSinceLastPlay { get; set; }
    public float RetentionScore { get; set; }
}

public class Achievement
{
    public string Name { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
}

public class Recommendation
{
    public RecommendationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public float RelevanceScore { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public class PlayerSegment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public Dictionary<string, float> Characteristics { get; set; } = new();
}

public enum SkillLevel
{
    Beginner,
    Novice,
    Intermediate,
    Advanced,
    Expert,
    Master
}

public enum PlayStyle
{
    Explorer,
    Achiever,
    Socializer,
    Competitor,
    Creator,
    Collector
}

public enum RecommendationType
{
    Quest,
    Item,
    Feature,
    SocialConnection,
    Content
}

#endregion

#region Adaptive Difficulty Module

/// <summary>
/// Interface for dynamic difficulty adjustment
/// </summary>
public interface IAdaptiveDifficultyModule : IDisposable
{
    /// <summary>
    /// Calculate appropriate difficulty level
    /// </summary>
    Task<DifficultyLevel> CalculateDifficultyAsync(Guid playerId, GameContext context);
    
    /// <summary>
    /// Apply difficulty adjustment to game
    /// </summary>
    Task<bool> ApplyDifficultyAsync(Guid gameId, Guid playerId, DifficultyLevel level);
    
    /// <summary>
    /// Assess player skill level
    /// </summary>
    Task<SkillAssessment> AssessPlayerSkillAsync(Guid playerId);
    
    /// <summary>
    /// Get player's flow state
    /// </summary>
    Task<FlowState> GetFlowStateAsync(Guid playerId);
    
    /// <summary>
    /// Set difficulty parameters for a game
    /// </summary>
    Task<bool> SetDifficultyParametersAsync(Guid gameId, DifficultyParameters parameters);
}

public class DifficultyLevel
{
    public float OverallDifficulty { get; set; } // 0-1
    public Dictionary<string, float> ComponentDifficulties { get; set; } = new();
    public List<string> Adjustments { get; set; } = new();
}

public class SkillAssessment
{
    public Guid PlayerId { get; set; }
    public Dictionary<string, float> SkillScores { get; set; } = new();
    public SkillLevel OverallSkill { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class FlowState
{
    public Guid PlayerId { get; set; }
    public FlowZone CurrentZone { get; set; }
    public float ChallengeLevel { get; set; }
    public float SkillLevel { get; set; }
    public float EngagementScore { get; set; }
}

public class DifficultyParameters
{
    public float MinDifficulty { get; set; } = 0.2f;
    public float MaxDifficulty { get; set; } = 0.9f;
    public float AdjustmentRate { get; set; } = 0.1f;
    public int SampleSize { get; set; } = 10;
}

public enum FlowZone
{
    Boredom,
    Relaxation,
    Flow,
    Anxiety,
    Frustration
}

#endregion

#region Reward Optimization Module

/// <summary>
/// Interface for personalized reward systems
/// </summary>
public interface IRewardOptimizationModule : IDisposable
{
    /// <summary>
    /// Calculate optimal rewards for a player
    /// </summary>
    Task<RewardBundle> CalculateOptimalRewardsAsync(Guid playerId, RewardContext context);
    
    /// <summary>
    /// Track reward effectiveness
    /// </summary>
    Task<bool> TrackRewardEffectivenessAsync(Guid playerId, RewardGiven reward, RewardImpact impact);
    
    /// <summary>
    /// Get reward preferences for a player
    /// </summary>
    Task<RewardPreferences> GetRewardPreferencesAsync(Guid playerId);
}

public class RewardContext
{
    public string Achievement { get; set; } = string.Empty;
    public float DifficultyCompleted { get; set; }
    public int TimeSpentMinutes { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RewardBundle
{
    public List<Reward> Rewards { get; set; } = new();
    public float TotalValue { get; set; }
    public float PersonalizationScore { get; set; }
}

public class Reward
{
    public RewardType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public float Value { get; set; }
}

public class RewardGiven
{
    public Guid PlayerId { get; set; }
    public Reward Reward { get; set; } = new();
    public DateTime GivenAt { get; set; }
}

public class RewardImpact
{
    public bool WasUsed { get; set; }
    public float SatisfactionScore { get; set; }
    public bool LedToEngagement { get; set; }
}

public class RewardPreferences
{
    public Guid PlayerId { get; set; }
    public Dictionary<RewardType, float> TypePreferences { get; set; } = new();
    public List<string> FavoriteRewards { get; set; } = new();
}

public enum RewardType
{
    Currency,
    Item,
    Experience,
    Achievement,
    Cosmetic,
    Access
}

#endregion

#region Story Personalization Module

/// <summary>
/// Interface for tailored narrative generation
/// </summary>
public interface IStoryPersonalizationModule : IDisposable
{
    /// <summary>
    /// Generate personalized story for a player
    /// </summary>
    Task<PersonalizedStory> GeneratePersonalizedStoryAsync(Guid playerId, StoryContext context);
    
    /// <summary>
    /// Adapt existing story for a player
    /// </summary>
    Task<AdaptedStory> AdaptStoryAsync(Guid storyId, Guid playerId);
    
    /// <summary>
    /// Get story preferences for a player
    /// </summary>
    Task<StoryPreferences> GetStoryPreferencesAsync(Guid playerId);
}

public class StoryContext
{
    public StoryTheme Theme { get; set; }
    public int DesiredLengthMinutes { get; set; }
    public List<string> RequiredElements { get; set; } = new();
}

public class PersonalizedStory
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<StoryChoice> Choices { get; set; } = new();
    public float PersonalizationScore { get; set; }
}

public class StoryChoice
{
    public string ChoiceText { get; set; } = string.Empty;
    public string Consequence { get; set; } = string.Empty;
}

public class AdaptedStory
{
    public Guid OriginalStoryId { get; set; }
    public string AdaptedContent { get; set; } = string.Empty;
    public List<string> Modifications { get; set; } = new();
}

public class StoryPreferences
{
    public Guid PlayerId { get; set; }
    public List<StoryTheme> PreferredThemes { get; set; } = new();
    public Dictionary<string, float> ElementPreferences { get; set; } = new();
}

#endregion

#region Market Analytics Module

/// <summary>
/// Interface for economic analysis and forecasting
/// </summary>
public interface IMarketAnalyticsModule : IDisposable
{
    /// <summary>
    /// Analyze market trends
    /// </summary>
    Task<MarketAnalysis> AnalyzeMarketAsync(Guid worldId);
    
    /// <summary>
    /// Forecast price movements
    /// </summary>
    Task<PriceForecast> ForecastPriceAsync(Guid itemId, int daysAhead);
    
    /// <summary>
    /// Detect market manipulation
    /// </summary>
    Task<IEnumerable<ManipulationAlert>> DetectManipulationAsync(Guid worldId);
    
    /// <summary>
    /// Get market health metrics
    /// </summary>
    Task<MarketHealth> GetMarketHealthAsync(Guid worldId);
}

public class MarketAnalysis
{
    public Guid WorldId { get; set; }
    public Dictionary<Guid, ItemAnalysis> ItemAnalyses { get; set; } = new();
    public MarketTrend OverallTrend { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

public class ItemAnalysis
{
    public Guid ItemId { get; set; }
    public decimal CurrentPrice { get; set; }
    public MarketTrend Trend { get; set; } = new();
    public float Volatility { get; set; }
    public int TradeVolume { get; set; }
}

public class PriceForecast
{
    public Guid ItemId { get; set; }
    public List<PricePoint> ForecastedPrices { get; set; } = new();
    public float ConfidenceLevel { get; set; }
}

public class PricePoint
{
    public DateTime Date { get; set; }
    public decimal PredictedPrice { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
}

public class ManipulationAlert
{
    public Guid Id { get; set; }
    public ManipulationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<Guid> SuspectedPlayers { get; set; } = new();
    public float ConfidenceScore { get; set; }
}

public class MarketHealth
{
    public float HealthScore { get; set; } // 0-1
    public float Liquidity { get; set; }
    public float Stability { get; set; }
    public List<string> Concerns { get; set; } = new();
}

public class MarketTrend
{
    public TrendDirection Direction { get; set; }
    public float Strength { get; set; }
    public int DurationDays { get; set; }
}

public enum ManipulationType
{
    PriceFixing,
    MarketCornering,
    PumpAndDump,
    Collusion
}

public enum TrendDirection
{
    Bullish,
    Bearish,
    Sideways
}

#endregion

#region Documentation Generator Module

/// <summary>
/// Interface for automatic documentation generation
/// </summary>
public interface IDocumentationGeneratorModule : IDisposable
{
    /// <summary>
    /// Generate comprehensive documentation for a project
    /// </summary>
    Task<Documentation> GenerateDocsAsync(Guid projectId);
    
    /// <summary>
    /// Generate API documentation
    /// </summary>
    Task<APIDocumentation> GenerateAPIDocsAsync(Guid apiId);
    
    /// <summary>
    /// Export documentation in specific format
    /// </summary>
    Task<string> ExportDocsAsync(Guid docId, DocumentationFormat format);
    
    /// <summary>
    /// Get supported documentation formats
    /// </summary>
    Task<IEnumerable<DocumentationFormat>> GetSupportedFormatsAsync();
    
    /// <summary>
    /// Update documentation
    /// </summary>
    Task<bool> UpdateDocsAsync(Guid docId);
    
    /// <summary>
    /// Enable/disable automatic updates
    /// </summary>
    Task<bool> SetAutoUpdateAsync(Guid projectId, bool enabled);
}

public class Documentation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<DocumentationSection> Sections { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class DocumentationSection
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<CodeSample> CodeSamples { get; set; } = new();
    public List<DiagramData> Diagrams { get; set; } = new();
}

public class CodeSample
{
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DiagramData
{
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; }
}

public class APIDocumentation
{
    public Guid ApiId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<EndpointDocumentation> Endpoints { get; set; } = new();
    public List<TypeDocumentation> Types { get; set; } = new();
}

public class EndpointDocumentation
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterDocumentation> Parameters { get; set; } = new();
    public List<CodeSample> Examples { get; set; } = new();
}

public class ParameterDocumentation
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TypeDocumentation
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PropertyDocumentation> Properties { get; set; } = new();
}

public class PropertyDocumentation
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

#endregion

#region AI Mentor Module

/// <summary>
/// Interface for personalized AI teaching and guidance
/// </summary>
public interface IAIMentorModule : IDisposable
{
    /// <summary>
    /// Start a mentoring session
    /// </summary>
    Task<MentorSession> StartMentoringAsync(Guid userId, LearningGoal goal);
    
    /// <summary>
    /// Ask a question in the mentoring session
    /// </summary>
    Task<MentorResponse> AskQuestionAsync(Guid sessionId, string question);
    
    /// <summary>
    /// Generate a learning path
    /// </summary>
    Task<LearningPath> GenerateLearningPathAsync(Guid userId, Skill targetSkill);
    
    /// <summary>
    /// Get learning progress
    /// </summary>
    Task<Progress> GetLearningProgressAsync(Guid userId);
    
    /// <summary>
    /// Assess user skill level
    /// </summary>
    Task<SkillLevel> AssessSkillAsync(Guid userId, Skill skill);
    
    /// <summary>
    /// Get next steps recommendation
    /// </summary>
    Task<IEnumerable<Recommendation>> GetNextStepsAsync(Guid userId);
}

public class LearningGoal
{
    public string Goal { get; set; } = string.Empty;
    public Skill TargetSkill { get; set; }
    public int TimeframeWeeks { get; set; }
}

public class MentorSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public LearningGoal Goal { get; set; } = new();
    public List<MentorInteraction> Interactions { get; set; } = new();
    public DateTime StartedAt { get; set; }
}

public class MentorInteraction
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class MentorResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> RelatedTopics { get; set; } = new();
    public List<CodeSample> Examples { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
}

public class LearningPath
{
    public Guid Id { get; set; }
    public Skill TargetSkill { get; set; }
    public List<LearningStep> Steps { get; set; } = new();
    public int EstimatedWeeks { get; set; }
}

public class LearningStep
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Resources { get; set; } = new();
    public bool IsCompleted { get; set; }
}

public class Progress
{
    public Guid UserId { get; set; }
    public Dictionary<Skill, float> SkillProgress { get; set; } = new();
    public int TotalStepsCompleted { get; set; }
    public int TotalSteps { get; set; }
    public float OverallProgress { get; set; }
}

public enum Skill
{
    GameDesign,
    Programming,
    Art,
    Sound,
    Writing,
    Marketing,
    ProjectManagement
}

#endregion

#region Compliance Modules

/// <summary>
/// Interface for AI-driven regulatory compliance monitoring
/// </summary>
public interface IRegulatoryMonitorModule : IDisposable
{
    /// <summary>
    /// Check compliance status
    /// </summary>
    Task<ComplianceStatus> CheckComplianceAsync(Guid entityId);
    
    /// <summary>
    /// Get active compliance violations
    /// </summary>
    Task<IEnumerable<ComplianceViolation>> GetViolationsAsync();
    
    /// <summary>
    /// Enforce a policy
    /// </summary>
    Task<bool> EnforcePolicyAsync(Guid policyId);
    
    /// <summary>
    /// Remediate a violation
    /// </summary>
    Task<bool> RemediateViolationAsync(Guid violationId);
    
    /// <summary>
    /// Generate compliance report
    /// </summary>
    Task<Phase6ComplianceReport> GenerateReportAsync(ReportPeriod period);
    
    /// <summary>
    /// Assess regulatory risk
    /// </summary>
    Task<RiskAssessment> AssessRiskAsync(Guid entityId);
}

public class ComplianceStatus
{
    public Guid EntityId { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> ComplianceAreas { get; set; } = new();
    public List<ComplianceIssue> Issues { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class ComplianceIssue
{
    public string Area { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
}

public class ComplianceViolation
{
    public Guid Id { get; set; }
    public string Regulation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public ViolationStatus Status { get; set; }
}

// Phase6ComplianceReport to avoid conflict
public class Phase6ComplianceReport
{
    public ReportPeriod Period { get; set; } = new();
    public int TotalChecks { get; set; }
    public int ViolationsFound { get; set; }
    public int ViolationsRemediated { get; set; }
    public Dictionary<string, int> ViolationsByType { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ReportPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class RiskAssessment
{
    public Guid EntityId { get; set; }
    public RiskLevel OverallRisk { get; set; }
    public Dictionary<string, RiskLevel> RiskByArea { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public enum ViolationStatus
{
    Active,
    Remediated,
    Pending,
    Dismissed
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Interface for automatic content rating
/// </summary>
public interface IContentRatingModule : IDisposable
{
    /// <summary>
    /// Rate content automatically
    /// </summary>
    Task<Phase6ContentRating> RateContentAsync(Guid gameId);
    
    /// <summary>
    /// Get content descriptors
    /// </summary>
    Task<IEnumerable<Phase6ContentDescriptor>> GetDescriptorsAsync(Guid gameId);
    
    /// <summary>
    /// Apply age gate based on rating
    /// </summary>
    Task<bool> ApplyAgeGateAsync(Guid gameId, Phase6ContentRating rating);
    
    /// <summary>
    /// Validate assigned rating
    /// </summary>
    Task<bool> ValidateRatingAsync(Guid gameId);
    
    /// <summary>
    /// Appeal a content rating
    /// </summary>
    Task<Phase6AppealResult> AppealRatingAsync(Guid gameId, Phase6AppealRequest appeal);
}

// Phase6ContentRating to avoid conflict
public class Phase6ContentRating
{
    public Guid GameId { get; set; }
    public string ESRBRating { get; set; } = string.Empty;
    public string PEGIRating { get; set; } = string.Empty;
    public List<Phase6ContentDescriptor> Descriptors { get; set; } = new();
    public int MinimumAge { get; set; }
    public DateTime RatedAt { get; set; }
}

public class Phase6ContentDescriptor
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DescriptorSeverity Severity { get; set; }
}

// Phase6AppealRequest to avoid conflict
public class Phase6AppealRequest
{
    public string Reason { get; set; } = string.Empty;
    public List<string> Evidence { get; set; } = new();
}

public class Phase6AppealResult
{
    public bool Approved { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Phase6ContentRating? NewRating { get; set; }
}

public enum DescriptorSeverity
{
    Mild,
    Moderate,
    Strong,
    Intense
}

#endregion
