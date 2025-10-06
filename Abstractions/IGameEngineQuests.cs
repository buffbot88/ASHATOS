namespace Abstractions;

/// <summary>
/// Quest objective that must be completed.
/// </summary>
public class QuestObjective
{
    public string ObjectiveId { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Generic"; // Kill, Collect, Talk, Explore, Escort, etc.
    public string TargetId { get; set; } = string.Empty; // Entity ID or item ID
    public int RequiredCount { get; set; } = 1;
    public int CurrentCount { get; set; } = 0;
    public bool IsCompleted => CurrentCount >= RequiredCount;
    public bool IsOptional { get; set; } = false;
    public Dictionary<string, object> Metadata { get; set; } = new();

    public void IncrementProgress(int amount = 1)
    {
        CurrentCount = Math.Min(CurrentCount + amount, RequiredCount);
    }

    public float GetProgressPercentage()
    {
        return RequiredCount > 0 ? ((float)CurrentCount / RequiredCount) * 100 : 0;
    }
}

/// <summary>
/// Quest reward given upon completion.
/// </summary>
public class QuestReward
{
    public string RewardType { get; set; } = "Experience"; // Experience, Gold, Item, Skill, etc.
    public string RewardId { get; set; } = string.Empty; // Item ID or skill ID
    public int Quantity { get; set; } = 1;
    public long Value { get; set; } = 0; // XP amount, gold amount, etc.
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Quest state for tracking progress.
/// </summary>
public enum QuestState
{
    NotStarted,
    Available,
    InProgress,
    Completed,
    Failed,
    Abandoned
}

/// <summary>
/// Represents a quest that can be completed by entities.
/// </summary>
public class Quest
{
    public string QuestId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QuestGiverId { get; set; } = string.Empty; // NPC who gives the quest
    public string Category { get; set; } = "Main"; // Main, Side, Daily, Event, etc.
    public int RecommendedLevel { get; set; } = 1;
    public List<QuestObjective> Objectives { get; set; } = new();
    public List<QuestReward> Rewards { get; set; } = new();
    public List<string> PrerequisiteQuests { get; set; } = new(); // Quest IDs that must be completed first
    public bool IsRepeatable { get; set; } = false;
    public DateTime? ExpiresAt { get; set; } // For time-limited quests
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    public Dictionary<string, object> Metadata { get; set; } = new();

    public bool AreAllObjectivesCompleted()
    {
        return Objectives.Where(o => !o.IsOptional).All(o => o.IsCompleted);
    }

    public float GetOverallProgress()
    {
        if (Objectives.Count == 0) return 0;
        
        var requiredObjectives = Objectives.Where(o => !o.IsOptional).ToList();
        if (requiredObjectives.Count == 0) return 100;

        float totalProgress = requiredObjectives.Sum(o => o.GetProgressPercentage());
        return totalProgress / requiredObjectives.Count;
    }

    public List<QuestObjective> GetIncompleteObjectives()
    {
        return Objectives.Where(o => !o.IsCompleted).ToList();
    }
}

/// <summary>
/// Tracks quest state for an entity.
/// </summary>
public class EntityQuestProgress
{
    public string QuestId { get; set; } = string.Empty;
    public QuestState State { get; set; } = QuestState.NotStarted;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<QuestObjective> Objectives { get; set; } = new(); // Copy of objectives with progress
    public int TimesCompleted { get; set; } = 0; // For repeatable quests
    public bool TurnedIn { get; set; } = false; // Whether rewards have been claimed

    public bool CanComplete()
    {
        return State == QuestState.InProgress && 
               Objectives.Where(o => !o.IsOptional).All(o => o.IsCompleted);
    }

    public void UpdateObjectiveProgress(string objectiveId, int amount)
    {
        var objective = Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
        objective?.IncrementProgress(amount);
    }
}

/// <summary>
/// Quest log component for entities.
/// Manages active, completed, and available quests.
/// </summary>
public class QuestLogComponent
{
    public List<EntityQuestProgress> ActiveQuests { get; set; } = new();
    public List<EntityQuestProgress> CompletedQuests { get; set; } = new();
    public List<string> AvailableQuestIds { get; set; } = new(); // Quests available to start
    public int MaxActiveQuests { get; set; } = 25;

    public bool HasQuest(string questId)
    {
        return ActiveQuests.Any(q => q.QuestId == questId) || 
               CompletedQuests.Any(q => q.QuestId == questId);
    }

    public bool HasActiveQuest(string questId)
    {
        return ActiveQuests.Any(q => q.QuestId == questId);
    }

    public bool HasCompletedQuest(string questId)
    {
        return CompletedQuests.Any(q => q.QuestId == questId);
    }

    public EntityQuestProgress? GetActiveQuest(string questId)
    {
        return ActiveQuests.FirstOrDefault(q => q.QuestId == questId);
    }

    public EntityQuestProgress? GetCompletedQuest(string questId)
    {
        return CompletedQuests.FirstOrDefault(q => q.QuestId == questId);
    }

    public QuestStartResult StartQuest(Quest quest)
    {
        if (ActiveQuests.Count >= MaxActiveQuests)
            return new QuestStartResult { Success = false, Message = "Quest log is full" };

        if (HasActiveQuest(quest.QuestId))
            return new QuestStartResult { Success = false, Message = "Quest already active" };

        if (!quest.IsRepeatable && HasCompletedQuest(quest.QuestId))
            return new QuestStartResult { Success = false, Message = "Quest already completed" };

        // Check prerequisites
        foreach (var prereq in quest.PrerequisiteQuests)
        {
            if (!HasCompletedQuest(prereq))
                return new QuestStartResult { Success = false, Message = "Prerequisites not met" };
        }

        // Create quest progress
        var progress = new EntityQuestProgress
        {
            QuestId = quest.QuestId,
            State = QuestState.InProgress,
            StartedAt = DateTime.UtcNow,
            Objectives = quest.Objectives.Select(o => new QuestObjective
            {
                ObjectiveId = o.ObjectiveId,
                Description = o.Description,
                Type = o.Type,
                TargetId = o.TargetId,
                RequiredCount = o.RequiredCount,
                CurrentCount = 0,
                IsOptional = o.IsOptional,
                Metadata = new Dictionary<string, object>(o.Metadata)
            }).ToList()
        };

        ActiveQuests.Add(progress);
        AvailableQuestIds.Remove(quest.QuestId);

        return new QuestStartResult { Success = true, Message = "Quest started", QuestProgress = progress };
    }

    public QuestCompleteResult CompleteQuest(string questId)
    {
        var progress = GetActiveQuest(questId);
        if (progress == null)
            return new QuestCompleteResult { Success = false, Message = "Quest not found" };

        if (!progress.CanComplete())
            return new QuestCompleteResult { Success = false, Message = "Quest objectives not completed" };

        progress.State = QuestState.Completed;
        progress.CompletedAt = DateTime.UtcNow;
        progress.TimesCompleted++;

        ActiveQuests.Remove(progress);
        
        // Check if quest already exists in completed (for repeatable quests)
        var existing = CompletedQuests.FirstOrDefault(q => q.QuestId == questId);
        if (existing != null)
        {
            existing.TimesCompleted = progress.TimesCompleted;
            existing.CompletedAt = progress.CompletedAt;
        }
        else
        {
            CompletedQuests.Add(progress);
        }

        return new QuestCompleteResult { Success = true, Message = "Quest completed", QuestProgress = progress };
    }

    public bool AbandonQuest(string questId)
    {
        var progress = GetActiveQuest(questId);
        if (progress == null) return false;

        progress.State = QuestState.Abandoned;
        ActiveQuests.Remove(progress);
        return true;
    }

    public void UpdateQuestObjective(string questId, string objectiveId, int amount = 1)
    {
        var progress = GetActiveQuest(questId);
        progress?.UpdateObjectiveProgress(objectiveId, amount);
    }

    public List<EntityQuestProgress> GetQuestsReadyToComplete()
    {
        return ActiveQuests.Where(q => q.CanComplete()).ToList();
    }

    public int GetActiveQuestCount()
    {
        return ActiveQuests.Count;
    }

    public int GetCompletedQuestCount()
    {
        return CompletedQuests.Count;
    }
}

/// <summary>
/// Result of starting a quest.
/// </summary>
public class QuestStartResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public EntityQuestProgress? QuestProgress { get; set; }
}

/// <summary>
/// Result of completing a quest.
/// </summary>
public class QuestCompleteResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public EntityQuestProgress? QuestProgress { get; set; }
}

/// <summary>
/// Quest manager for the game engine.
/// Manages all quests in the game world.
/// </summary>
public class QuestManager
{
    private readonly Dictionary<string, Quest> _quests = new();
    private readonly Dictionary<string, List<string>> _questsByGiver = new(); // NPC ID -> Quest IDs

    public void RegisterQuest(Quest quest)
    {
        _quests[quest.QuestId] = quest;

        if (!string.IsNullOrEmpty(quest.QuestGiverId))
        {
            if (!_questsByGiver.ContainsKey(quest.QuestGiverId))
                _questsByGiver[quest.QuestGiverId] = new List<string>();

            if (!_questsByGiver[quest.QuestGiverId].Contains(quest.QuestId))
                _questsByGiver[quest.QuestGiverId].Add(quest.QuestId);
        }
    }

    public Quest? GetQuest(string questId)
    {
        _quests.TryGetValue(questId, out var quest);
        return quest;
    }

    public List<Quest> GetQuestsByGiver(string giverId)
    {
        if (!_questsByGiver.TryGetValue(giverId, out var questIds))
            return new List<Quest>();

        return questIds.Select(id => _quests.GetValueOrDefault(id))
                      .Where(q => q != null)
                      .Cast<Quest>()
                      .ToList();
    }

    public List<Quest> GetQuestsByCategory(string category)
    {
        return _quests.Values.Where(q => q.Category == category).ToList();
    }

    public List<Quest> GetAvailableQuests(QuestLogComponent questLog, int entityLevel)
    {
        return _quests.Values.Where(q =>
        {
            // Quest not already active or completed (unless repeatable)
            if (questLog.HasActiveQuest(q.QuestId)) return false;
            if (!q.IsRepeatable && questLog.HasCompletedQuest(q.QuestId)) return false;

            // Quest not expired
            if (q.IsExpired) return false;

            // Entity meets level requirement
            if (entityLevel < q.RecommendedLevel) return false;

            // Prerequisites met
            foreach (var prereq in q.PrerequisiteQuests)
            {
                if (!questLog.HasCompletedQuest(prereq)) return false;
            }

            return true;
        }).ToList();
    }

    public int GetTotalQuestCount()
    {
        return _quests.Count;
    }

    public Dictionary<string, int> GetQuestStatistics()
    {
        var stats = new Dictionary<string, int>();
        
        foreach (var category in _quests.Values.Select(q => q.Category).Distinct())
        {
            stats[category] = _quests.Values.Count(q => q.Category == category);
        }

        return stats;
    }
}
