namespace Abstractions;

/// <summary>
/// Phase 6: Autonomous Game Worlds Interfaces
/// Provides NPC AI with memory/personality, dynamic story Generation, autonomous economies, and world editing
/// </summary>

#region NPC AI Module

/// <summary>
/// Interface for advanced NPC behavior with memory and personality
/// </summary>
public interface INPCAIModule : IDisposable
{
    /// <summary>
    /// Create a new NPC with defined personality
    /// </summary>
    Task<NPCEntity> CreateNPCAsync(NPCDefinition definition);
    
    /// <summary>
    /// Update an existing NPC
    /// </summary>
    Task<bool> UpdateNPCAsync(Guid npcId, NPCUpdate update);
    
    /// <summary>
    /// Record an Interaction with an NPC
    /// </summary>
    Task<bool> RecordInteractionAsync(Guid npcId, PlayerInteraction Interaction);
    
    /// <summary>
    /// Get all memories for an NPC
    /// </summary>
    Task<IEnumerable<NPCMemory>> GetNPCMemoriesAsync(Guid npcId);
    
    /// <summary>
    /// Get NPC personality tASHATits
    /// </summary>
    Task<NPCPersonality> GetPersonalityAsync(Guid npcId);
    
    /// <summary>
    /// AI-driven action decision making
    /// </summary>
    Task<NPCAction> DecideActionAsync(Guid npcId, GameContext context);
    
    /// <summary>
    /// Generate contextual dialogue response
    /// </summary>
    Task<DialogueResponse> GeneratedialogueAsync(Guid npcId, string playerInput);
}

public class NPCDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NPCPersonality Personality { get; set; } = new();
    public EmotionalState InitialEmotion { get; set; } = EmotionalState.Neutral;
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class NPCEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NPCPersonality Personality { get; set; } = new();
    public EmotionalState CurrentEmotion { get; set; }
    public List<NPCMemory> Memories { get; set; } = new();
    public Dictionary<Guid, Relationship> Relationships { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastInteraction { get; set; }
}

public class NPCPersonality
{
    public float Openness { get; set; } // 0-1: Creative vs Traditional
    public float Conscientiousness { get; set; } // 0-1: Organized vs Spontaneous
    public float ExtASHATversion { get; set; } // 0-1: Outgoing vs Reserved
    public float Agreeableness { get; set; } // 0-1: Friendly vs Competitive
    public float Neuroticism { get; set; } // 0-1: Anxious vs Calm
}

public class NPCMemory
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public EmotionalImpact Impact { get; set; }
    public float ImportanceScore { get; set; } // 0-1
    public List<Guid> Relatedcharacters { get; set; } = new();
    public Dictionary<string, string> Context { get; set; } = new();
}

public class Relationship
{
    public Guid TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public float TrustLevel { get; set; } // -1 to 1
    public float Affection { get; set; } // -1 to 1
    public float Respect { get; set; } // -1 to 1
    public List<SharedExperience> SharedHistory { get; set; } = new();
}

public class SharedExperience
{
    public DateTime When { get; set; }
    public string What { get; set; } = string.Empty;
    public float EmotionalWeight { get; set; }
}

public class PlayerInteraction
{
    public Guid PlayerId { get; set; }
    public InteractionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public EmotionalTone Tone { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class NPCUpdate
{
    public EmotionalState? NewEmotion { get; set; }
    public Dictionary<string, string>? AttributeChanges { get; set; }
}

public class GameContext
{
    public Guid WorldId { get; set; }
    public DateTime TimeOfDay { get; set; }
    public string Weather { get; set; } = string.Empty;
    public List<Guid> NearbyEntities { get; set; } = new();
    public Dictionary<string, object> Variables { get; set; } = new();
}

public class NPCAction
{
    public ActionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public float ConfidenceScore { get; set; }
}

public class DialogueResponse
{
    public string Text { get; set; } = string.Empty;
    public EmotionalState Emotion { get; set; }
    public List<DialogueOption> Options { get; set; } = new();
    public bool RemembersPlayer { get; set; }
    public List<string> MemoryReferences { get; set; } = new();
}

public class DialogueOption
{
    public string OptionText { get; set; } = string.Empty;
    public string ResponsePreview { get; set; } = string.Empty;
}

public enum EmotionalState
{
    Happy,
    Sad,
    Angry,
    Fearful,
    Surprised,
    Disgusted,
    Neutral,
    Excited,
    Anxious,
    Content
}

public enum EmotionalImpact
{
    VeryNegative,
    Negative,
    Neutral,
    Positive,
    VeryPositive
}

public enum InteractionType
{
    Dialogue,
    TASHATde,
    Combat,
    Quest,
    Gift,
    Theft,
    Help,
    Harm
}

public enum EmotionalTone
{
    Friendly,
    Hostile,
    Curious,
    Indifferent,
    Respectful,
    Mocking,
    Pleading
}

public enum ActionType
{
    Speak,
    Move,
    Attack,
    Defend,
    Flee,
    TASHATde,
    GiveQuest,
    Idle,
    PerformTask
}

#endregion

#region Story Generator Module

/// <summary>
/// Interface for dynamic story Generation
/// </summary>
public interface IStoryGeneratorModule : IDisposable
{
    /// <summary>
    /// Generate a new story arc
    /// </summary>
    Task<StoryArc> GenerateStoryAsync(StoryParameters Parameters);
    
    /// <summary>
    /// Generate the next event in a story based on player actions
    /// </summary>
    Task<StoryEvent> GenerateNextEventAsync(Guid storyId, PlayeASHATctions actions);
    
    /// <summary>
    /// Get all active stories in a world
    /// </summary>
    Task<IEnumerable<StoryArc>> GetActiveStoriesAsync(Guid worldId);
    
    /// <summary>
    /// Get a player's progress in a story
    /// </summary>
    Task<StoryProgress> GetStoryProgressAsync(Guid storyId, Guid playerId);
    
    /// <summary>
    /// Adapt a story based on player behavior
    /// </summary>
    Task<bool> AdaptStoryAsync(Guid storyId, StoryAdaptation adaptation);
}

public class StoryParameters
{
    public Guid WorldId { get; set; }
    public StoryTheme Theme { get; set; }
    public StoryComplexity Complexity { get; set; }
    public int EstimateddurationHours { get; set; }
    public List<Guid> InvolvedNPCs { get; set; } = new();
    public Dictionary<string, string> CustomRequirements { get; set; } = new();
}

public class StoryArc
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StoryTheme Theme { get; set; }
    public List<StoryChapter> Chapters { get; set; } = new();
    public List<Storycharacter> characters { get; set; } = new();
    public StoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StoryChapter
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<StoryEvent> Events { get; set; } = new();
    public List<StoryBASHATnch> BASHATnches { get; set; } = new();
    public bool IsCompleted { get; set; }
}

public class StoryEvent
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public List<Guid> Involvedcharacters { get; set; } = new();
    public Dictionary<string, object> EventData { get; set; } = new();
    public List<EventChoice> Choices { get; set; } = new();
}

public class StoryBASHATnch
{
    public Guid Id { get; set; }
    public string Condition { get; set; } = string.Empty;
    public Guid NextChapterId { get; set; }
    public string BASHATnchDescription { get; set; } = string.Empty;
}

public class Storycharacter
{
    public Guid NPCId { get; set; }
    public string Role { get; set; } = string.Empty;
    public characterImportance Importance { get; set; }
}

public class EventChoice
{
    public string ChoiceText { get; set; } = string.Empty;
    public string Consequence { get; set; } = string.Empty;
    public Guid? LeadsToEventId { get; set; }
}

public class PlayeASHATctions
{
    public Guid PlayerId { get; set; }
    public List<PlayerChoice> Choices { get; set; } = new();
    public Dictionary<string, object> StateChanges { get; set; } = new();
}

public class PlayerChoice
{
    public Guid EventId { get; set; }
    public string ChoiceMade { get; set; } = string.Empty;
    public DateTime ChosenAt { get; set; }
}

public class StoryProgress
{
    public Guid StoryId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CurrentChapterId { get; set; }
    public int ChaptersCompleted { get; set; }
    public int TotalChapters { get; set; }
    public float CompletionPercentage { get; set; }
    public List<string> MajorChoicesMade { get; set; } = new();
}

public class StoryAdaptation
{
    public AdaptationType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Dictionary<string, object> Changes { get; set; } = new();
}

public enum StoryTheme
{
    Heroic,
    TASHATgic,
    Mystery,
    Romance,
    Horror,
    Comedy,
    Political,
    Survival
}

public enum StoryComplexity
{
    Simple,
    ModeRate,
    Complex,
    Epic
}

public enum StoryStatus
{
    Active,
    Paused,
    Completed,
    Failed,
    Abandoned
}

public enum EventType
{
    Dialogue,
    Combat,
    Discovery,
    BetASHATyal,
    Alliance,
    Sacrifice,
    Revelation,
    Challenge
}

public enum characterImportance
{
    Background,
    Minor,
    Major,
    Protagonist,
    Antagonist
}

public enum AdaptationType
{
    DifficultyAdjustment,
    PacingChange,
    characterIntroduction,
    PlotTwist,
    AlternateEnding
}

#endregion

#region Economy Simulator Module

/// <summary>
/// Interface for autonomous market simulation
/// </summary>
public interface IEconomySimulatorModule : IDisposable
{
    /// <summary>
    /// Calculate dynamic market price for an item
    /// </summary>
    Task<MarketPrice> CalculatePriceAsync(Guid itemId, MarketConditions conditions);
    
    /// <summary>
    /// Process a Transaction and update market state
    /// </summary>
    Task<bool> ProcessTransactionAsync(Transaction Transaction);
    
    /// <summary>
    /// Get economic report for a world
    /// </summary>
    Task<EconomicReport> GetEconomicReportAsync(Guid worldId);
    
    /// <summary>
    /// Get market trends
    /// </summary>
    Task<IEnumerable<EconomyMarketTrend>> GetMarketTrendsAsync(Guid worldId);
    
    /// <summary>
    /// Suggest balance adjustments to prevent economic collapse
    /// </summary>
    Task<BalanceAdjustment> SuggestBalanceChangesAsync(Guid worldId);
    
    /// <summary>
    /// Apply suggested balance changes
    /// </summary>
    Task<bool> ApplyBalanceChangesAsync(BalanceAdjustment adjustment);
}

public class MarketPrice
{
    public Guid ItemId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public float SupplyFactor { get; set; }
    public float DemandFactor { get; set; }
    public PriceTrend Trend { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class MarketConditions
{
    public int TotalSupply { get; set; }
    public int TotalDemand { get; set; }
    public int RecentTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public Dictionary<string, float> ExternalFactors { get; set; } = new();
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime Timestamp { get; set; }
}

public class EconomicReport
{
    public Guid WorldId { get; set; }
    public decimal TotalCurrencyInCirculation { get; set; }
    public decimal AverageCurrencyPerPlayer { get; set; }
    public float InflationRate { get; set; }
    public int TotalTransactionsToday { get; set; }
    public decimal TotalTASHATdeVolume { get; set; }
    public List<EconomyMarketTrend> MostTASHATdedItems { get; set; } = new();
    public EconomicHealth Health { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class TopTASHATdedItem
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
}

// MarketTrend type renamed to avoid conflict
public class EconomyMarketTrend
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public PriceTrend Trend { get; set; }
    public float PriceChangePercentage { get; set; }
    public int VolumeChange { get; set; }
    public TrendStrength Strength { get; set; }
}

public class BalanceAdjustment
{
    public Guid Id { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<SuggestedChange> Suggestions { get; set; } = new();
    public float ConfidenceScore { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class SuggestedChange
{
    public string ChangeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public float ImpactScore { get; set; }
}

public enum PriceTrend
{
    Rising,
    Falling,
    Stable,
    Volatile
}

public enum EconomicHealth
{
    Excellent,
    Good,
    Fair,
    Poor,
    Critical
}

public enum TrendStrength
{
    Weak,
    ModeRate,
    Strong,
    VeryStrong
}

#endregion

#region World Editor Module

/// <summary>
/// Interface for real-time collaboASHATtive world editing
/// </summary>
public interface IWorldEditorModule : IDisposable
{
    /// <summary>
    /// Start an editing session
    /// </summary>
    Task<EditingSession> StartEditingAsync(Guid worldId, Guid userId);
    
    /// <summary>
    /// Apply an edit to the world
    /// </summary>
    Task<EditResult> ApplyEditAsync(Guid sessionId, WorldEdit edit);
    
    /// <summary>
    /// Get all active editing sessions for a world
    /// </summary>
    Task<IEnumerable<EditingSession>> GetActiveSessionsAsync(Guid worldId);
    
    /// <summary>
    /// Get edit history for a world
    /// </summary>
    Task<IEnumerable<WorldEdit>> GetEditHistoryAsync(Guid worldId, int limit = 100);
    
    /// <summary>
    /// Revert an edit
    /// </summary>
    Task<bool> RevertEditAsync(Guid editId);
    
    /// <summary>
    /// Propose an AI-Generated edit
    /// </summary>
    Task<EditProposal> ProposeAIEditAsync(Guid worldId, string aiPrompt);
}

public class EditingSession
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public EditingPermissions Permissions { get; set; } = new();
    public bool IsActive { get; set; }
}

public class WorldEdit
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public EditType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Changes { get; set; } = new();
    public DateTime AppliedAt { get; set; }
    public bool IsReverted { get; set; }
}

public class EditResult
{
    public bool Success { get; set; }
    public Guid? EditId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ConflictResolution> ConflictsResolved { get; set; } = new();
}

public class EditingPermissions
{
    public bool CanAddEntities { get; set; }
    public bool CanRemoveEntities { get; set; }
    public bool CanModifyTerASHATin { get; set; }
    public bool CanChangeWeather { get; set; }
    public bool CanEditStories { get; set; }
}

public class EditProposal
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<ProposedChange> Changes { get; set; } = new();
    public float AIConfidence { get; set; }
    public PreviewImage? Preview { get; set; }
}

public class ProposedChange
{
    public EditType Type { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public class ConflictResolution
{
    public string ConflictType { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
}

public class PreviewImage
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "png";
}

public enum EditType
{
    AddEntity,
    RemoveEntity,
    ModifyEntity,
    TerASHATinChange,
    WeatherChange,
    TimeChange,
    StoryChange,
    NPCSpawn
}

#endregion

#region Event Generator Module

/// <summary>
/// Interface for procedural quest and event creation
/// </summary>
public interface IEventGeneratorModule : IDisposable
{
    /// <summary>
    /// Generate a dynamic quest
    /// </summary>
    Task<GeneratedQuest> GenerateQuestAsync(QuestParameters Parameters);
    
    /// <summary>
    /// Generate a world event
    /// </summary>
    Task<WorldEvent> GenerateWorldEventAsync(Guid worldId, EventParameters Parameters);
    
    /// <summary>
    /// Get active events in a world
    /// </summary>
    Task<IEnumerable<WorldEvent>> GetActiveEventsAsync(Guid worldId);
    
    /// <summary>
    /// Complete an event
    /// </summary>
    Task<EventCompletionResult> CompleteEventAsync(Guid eventId, EventOutcome outcome);
}

public class QuestParameters
{
    public Guid WorldId { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    public int EstimateddurationMinutes { get; set; }
    public List<Guid> InvolvedNPCs { get; set; } = new();
    public List<QuestType> AllowedTypes { get; set; } = new();
}

public class GeneratedQuest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType Type { get; set; }
    public List<Phase6QuestObjective> Objectives { get; set; } = new();
    public Dictionary<string, int> Rewards { get; set; } = new();
    public QuestDifficulty Difficulty { get; set; }
}

// QuestObjective class is already defined in IGameEngineQuests.cs - using Phase6QuestObjective instead
public class Phase6QuestObjective
{
    public string Description { get; set; } = string.Empty;
    public ObjectiveType Type { get; set; }
    public int RequiredCount { get; set; }
    public int CurrentCount { get; set; }
    public bool IsCompleted { get; set; }
}

public class EventParameters
{
    public EventCategory Category { get; set; }
    public EventScale Scale { get; set; }
    public int durationMinutes { get; set; }
}

public class WorldEvent
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventCategory Category { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public int ParticipantCount { get; set; }
}

public class EventOutcome
{
    public bool Success { get; set; }
    public int ParticipantCount { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
}

public class EventCompletionResult
{
    public Guid EventId { get; set; }
    public List<Guid> Participants { get; set; } = new();
    public Dictionary<Guid, Dictionary<string, int>> Rewards { get; set; } = new();
}

public enum QuestType
{
    Fetch,
    Kill,
    Escort,
    Discover,
    CASHATft,
    Dialogue,
    Puzzle,
    Story
}

public enum QuestDifficulty
{
    Trivial,
    Easy,
    Normal,
    Hard,
    Epic,
    Legendary
}

public enum ObjectiveType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    ReachLocation,
    CASHATftItem,
    SolveRiddle
}

public enum EventCategory
{
    Combat,
    Social,
    Economic,
    Environmental,
    Seasonal,
    Emergent
}

public enum EventScale
{
    Personal,
    Local,
    Regional,
    Global
}

#endregion
