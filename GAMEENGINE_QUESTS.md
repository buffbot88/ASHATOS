# Game Engine Quest System - Phase 4.2

## Overview

The Quest System provides a complete quest management framework for RPG-style games, including quest creation, objective tracking, rewards, prerequisites, and NPC quest givers. It integrates seamlessly with the entity components system for character progression.

## Core Concepts

### Quest
A task or mission that players can complete for rewards. Quests have objectives, rewards, prerequisites, and can be repeatable or one-time.

### Quest Objective
A specific goal within a quest (e.g., "Kill 10 goblins", "Collect 5 herbs", "Talk to the mayor").

### Quest State
Current status of a quest: NotStarted, Available, InProgress, Completed, Failed, or Abandoned.

### Quest Log
Player's collection of active and completed quests.

## Components

### 🎯 Quest

Main quest definition with objectives and rewards.

**Properties:**
- `QuestId` - Unique identifier
- `Name` - Quest name
- `Description` - Quest story/instructions
- `QuestGiverId` - NPC who gives the quest
- `Category` - Main, Side, Daily, Event, etc.
- `RecommendedLevel` - Suggested player level
- `Objectives` - List of objectives to complete
- `Rewards` - Rewards given on completion
- `PrerequisiteQuests` - Required completed quests
- `IsRepeatable` - Can be done multiple times
- `ExpiresAt` - Time limit (optional)

**Example:**
```csharp
var quest = new Quest
{
    Name = "The Missing Artifact",
    Description = "Find the ancient artifact stolen by bandits",
    QuestGiverId = "npc_mayor_001",
    Category = "Main",
    RecommendedLevel = 5,
    Objectives = new List<QuestObjective>
    {
        new() {
            Description = "Defeat bandit leader",
            Type = "Kill",
            TargetId = "enemy_bandit_leader",
            RequiredCount = 1
        },
        new() {
            Description = "Retrieve the artifact",
            Type = "Collect",
            TargetId = "item_ancient_artifact",
            RequiredCount = 1
        }
    },
    Rewards = new List<QuestReward>
    {
        new() { RewardType = "Experience", Value = 500 },
        new() { RewardType = "Gold", Value = 100 },
        new() { RewardType = "Item", RewardId = "item_magic_sword", Quantity = 1 }
    }
};
```

### 📋 Quest Objective

Individual tasks within a quest.

**Objective Types:**
- `Kill` - Defeat enemies
- `Collect` - Gather items
- `Talk` - Speak to NPCs
- `Explore` - Discover locations
- `Escort` - Protect an NPC
- `Deliver` - Take item to location
- `Craft` - Create items
- `Use` - Use specific items

**Example:**
```csharp
var objective = new QuestObjective
{
    Description = "Collect wolf pelts",
    Type = "Collect",
    TargetId = "item_wolf_pelt",
    RequiredCount = 5,
    CurrentCount = 0,
    IsOptional = false
};

// Update progress
objective.IncrementProgress(1); // CurrentCount = 1
float progress = objective.GetProgressPercentage(); // Returns 20%
bool complete = objective.IsCompleted; // Returns false
```

### 🎁 Quest Reward

Rewards given upon quest completion.

**Reward Types:**
- `Experience` - XP points
- `Gold` - Currency
- `Item` - Inventory items
- `Skill` - Unlock abilities
- `Reputation` - Faction standing
- `Title` - Achievement titles

**Example:**
```csharp
var rewards = new List<QuestReward>
{
    new() { RewardType = "Experience", Value = 1000 },
    new() { RewardType = "Gold", Value = 250 },
    new() { RewardType = "Item", RewardId = "potion_health", Quantity = 5 },
    new() { RewardType = "Title", RewardId = "title_hero" }
};
```

### 📖 Quest Log Component

Player's quest journal tracking active and completed quests.

**Features:**
- Active quests (in progress)
- Completed quests (finished)
- Available quests (can start)
- Max active quest limit
- Quest progress tracking

**Example:**
```csharp
var questLog = new QuestLogComponent
{
    MaxActiveQuests = 25
};

// Start a quest
var startResult = questLog.StartQuest(quest);
if (startResult.Success)
{
    Console.WriteLine($"Started quest: {quest.Name}");
}

// Update objective progress
questLog.UpdateQuestObjective(quest.QuestId, objectiveId, 1);

// Check if ready to complete
var progress = questLog.GetActiveQuest(quest.QuestId);
if (progress.CanComplete())
{
    var completeResult = questLog.CompleteQuest(quest.QuestId);
    Console.WriteLine("Quest completed!");
}

// Abandon quest
questLog.AbandonQuest(quest.QuestId);
```

### 🗂️ Quest Manager

Global quest registry for the game world.

**Features:**
- Register quests
- Query quests by ID, giver, category
- Find available quests for player
- Quest statistics

**Example:**
```csharp
var questManager = new QuestManager();

// Register quest
questManager.RegisterQuest(quest);

// Get quest by ID
var retrievedQuest = questManager.GetQuest(quest.QuestId);

// Get quests from NPC
var npcQuests = questManager.GetQuestsByGiver("npc_mayor_001");

// Get available quests for player
var playerQuestLog = GetPlayerQuestLog(playerId);
var playerLevel = GetPlayerLevel(playerId);
var availableQuests = questManager.GetAvailableQuests(playerQuestLog, playerLevel);

// Get quests by category
var mainQuests = questManager.GetQuestsByCategory("Main");
var sideQuests = questManager.GetQuestsByCategory("Side");

// Statistics
int totalQuests = questManager.GetTotalQuestCount();
var stats = questManager.GetQuestStatistics(); // {"Main": 10, "Side": 25, "Daily": 5}
```

## Integration with Entity Components

Quests integrate with the component system:

```csharp
// Add quest log to player entity
var components = new EntityComponents
{
    Health = new HealthComponent(),
    Stats = new StatsComponent(),
    Inventory = new InventoryComponent()
};

// Create quest log
var questLog = new QuestLogComponent();

// Store quest log separately or in components
entity.Properties["questLog"] = questLog;

// Or add to components
// (would require updating EntityComponents class)
```

## Quest Flow

### 1. Quest Discovery
```csharp
// Player talks to NPC
var npcQuests = questManager.GetQuestsByGiver(npcId);

// Check which quests player can accept
var playerQuestLog = GetQuestLog(player);
var playerLevel = GetPlayerStats(player).Level;

var availableQuests = npcQuests.Where(q => 
    !playerQuestLog.HasActiveQuest(q.QuestId) &&
    playerLevel >= q.RecommendedLevel &&
    q.PrerequisiteQuests.All(p => playerQuestLog.HasCompletedQuest(p))
).ToList();
```

### 2. Quest Accept
```csharp
var startResult = playerQuestLog.StartQuest(selectedQuest);

if (startResult.Success)
{
    // Broadcast quest started event
    await BroadcastEvent(new GameEngineEvent
    {
        EventType = "quest.started",
        Data = new { questId = selectedQuest.QuestId, questName = selectedQuest.Name }
    });
}
```

### 3. Quest Progress
```csharp
// Player kills enemy
void OnEnemyKilled(string enemyId)
{
    var questLog = GetQuestLog(player);
    
    // Check all active quests for kill objectives
    foreach (var activeQuest in questLog.ActiveQuests)
    {
        var killObjectives = activeQuest.Objectives.Where(o => 
            o.Type == "Kill" && o.TargetId == enemyId && !o.IsCompleted
        );
        
        foreach (var objective in killObjectives)
        {
            questLog.UpdateQuestObjective(activeQuest.QuestId, objective.ObjectiveId, 1);
            
            // Broadcast progress update
            await BroadcastEvent(new GameEngineEvent
            {
                EventType = "quest.progress",
                Data = new { 
                    questId = activeQuest.QuestId,
                    objectiveId = objective.ObjectiveId,
                    progress = objective.GetProgressPercentage()
                }
            });
        }
    }
}

// Player collects item
void OnItemCollected(string itemId)
{
    var questLog = GetQuestLog(player);
    
    foreach (var activeQuest in questLog.ActiveQuests)
    {
        var collectObjectives = activeQuest.Objectives.Where(o => 
            o.Type == "Collect" && o.TargetId == itemId && !o.IsCompleted
        );
        
        foreach (var objective in collectObjectives)
        {
            questLog.UpdateQuestObjective(activeQuest.QuestId, objective.ObjectiveId, 1);
        }
    }
}
```

### 4. Quest Completion
```csharp
// Player returns to quest giver
var questLog = GetQuestLog(player);
var readyQuests = questLog.GetQuestsReadyToComplete();

foreach (var readyQuest in readyQuests)
{
    var completeResult = questLog.CompleteQuest(readyQuest.QuestId);
    
    if (completeResult.Success)
    {
        // Award rewards
        var quest = questManager.GetQuest(readyQuest.QuestId);
        AwardQuestRewards(player, quest.Rewards);
        
        // Broadcast completion
        await BroadcastEvent(new GameEngineEvent
        {
            EventType = "quest.completed",
            Data = new { questId = quest.QuestId, questName = quest.Name }
        });
    }
}
```

### 5. Reward Distribution
```csharp
void AwardQuestRewards(GameEntity player, List<QuestReward> rewards)
{
    var components = GetComponents(player);
    
    foreach (var reward in rewards)
    {
        switch (reward.RewardType)
        {
            case "Experience":
                components.Stats.AddExperience(reward.Value);
                break;
                
            case "Gold":
                // Add gold to inventory or separate currency
                var gold = new InventoryItem
                {
                    Name = "Gold",
                    ItemType = "Currency",
                    Quantity = (int)reward.Value
                };
                components.Inventory.AddItem(gold);
                break;
                
            case "Item":
                var item = CreateItem(reward.RewardId);
                item.Quantity = reward.Quantity;
                components.Inventory.AddItem(item);
                break;
                
            case "Skill":
                // Unlock skill (would need skill system)
                break;
        }
    }
}
```

## Example Quests

### Main Quest
```csharp
var mainQuest = new Quest
{
    Name = "Save the Kingdom",
    Description = "Defeat the evil dragon threatening the kingdom",
    Category = "Main",
    RecommendedLevel = 20,
    QuestGiverId = "npc_king",
    Objectives = new List<QuestObjective>
    {
        new() { Description = "Find the dragon's lair", Type = "Explore", TargetId = "location_dragon_lair", RequiredCount = 1 },
        new() { Description = "Defeat the dragon", Type = "Kill", TargetId = "boss_dragon", RequiredCount = 1 }
    },
    Rewards = new List<QuestReward>
    {
        new() { RewardType = "Experience", Value = 10000 },
        new() { RewardType = "Gold", Value = 5000 },
        new() { RewardType = "Item", RewardId = "item_dragon_sword", Quantity = 1 },
        new() { RewardType = "Title", RewardId = "title_dragonslayer" }
    }
};
```

### Side Quest
```csharp
var sideQuest = new Quest
{
    Name = "Lost Cat",
    Description = "Find the innkeeper's missing cat",
    Category = "Side",
    RecommendedLevel = 1,
    QuestGiverId = "npc_innkeeper",
    Objectives = new List<QuestObjective>
    {
        new() { Description = "Search for clues", Type = "Explore", TargetId = "location_alley", RequiredCount = 1 },
        new() { Description = "Find the cat", Type = "Talk", TargetId = "npc_cat", RequiredCount = 1 }
    },
    Rewards = new List<QuestReward>
    {
        new() { RewardType = "Experience", Value = 50 },
        new() { RewardType = "Gold", Value = 10 }
    }
};
```

### Daily Quest
```csharp
var dailyQuest = new Quest
{
    Name = "Daily Bounty: Wolves",
    Description = "Defeat wolves threatening the village",
    Category = "Daily",
    RecommendedLevel = 5,
    QuestGiverId = "npc_guard_captain",
    IsRepeatable = true,
    ExpiresAt = DateTime.UtcNow.AddDays(1),
    Objectives = new List<QuestObjective>
    {
        new() { Description = "Kill wolves", Type = "Kill", TargetId = "enemy_wolf", RequiredCount = 10 }
    },
    Rewards = new List<QuestReward>
    {
        new() { RewardType = "Experience", Value = 200 },
        new() { RewardType = "Gold", Value = 50 }
    }
};
```

### Chain Quest
```csharp
// Quest 1
var quest1 = new Quest
{
    QuestId = "quest_chain_1",
    Name = "Investigation Begins",
    Category = "Main",
    Objectives = new List<QuestObjective>
    {
        new() { Description = "Question witnesses", Type = "Talk", TargetId = "npc_witness", RequiredCount = 3 }
    }
};

// Quest 2 (requires Quest 1)
var quest2 = new Quest
{
    QuestId = "quest_chain_2",
    Name = "Following the Trail",
    Category = "Main",
    PrerequisiteQuests = new List<string> { "quest_chain_1" },
    Objectives = new List<QuestObjective>
    {
        new() { Description = "Search the forest", Type = "Explore", TargetId = "location_forest", RequiredCount = 1 }
    }
};
```

## API Endpoints

### Get Available Quests from NPC

```http
GET /api/gameengine/scene/{sceneId}/entity/{npcId}/quests
```

Response:
```json
{
  "success": true,
  "quests": [
    {
      "questId": "quest_001",
      "name": "The Missing Artifact",
      "description": "Find the stolen artifact",
      "category": "Main",
      "recommendedLevel": 5,
      "objectives": [...],
      "rewards": [...]
    }
  ]
}
```

### Start Quest

```http
POST /api/gameengine/scene/{sceneId}/entity/{playerId}/quest/start
Content-Type: application/json

{
  "questId": "quest_001"
}
```

### Update Quest Progress

```http
POST /api/gameengine/scene/{sceneId}/entity/{playerId}/quest/{questId}/progress
Content-Type: application/json

{
  "objectiveId": "obj_001",
  "amount": 1
}
```

### Complete Quest

```http
POST /api/gameengine/scene/{sceneId}/entity/{playerId}/quest/{questId}/complete
```

### Get Player Quest Log

```http
GET /api/gameengine/scene/{sceneId}/entity/{playerId}/quests
```

Response:
```json
{
  "success": true,
  "questLog": {
    "activeQuests": [...],
    "completedQuests": [...],
    "availableQuestIds": [...]
  }
}
```

## Best Practices

1. **Quest Design**: Create meaningful objectives with clear descriptions
2. **Level Gating**: Set appropriate recommended levels
3. **Rewards**: Balance XP, gold, and items
4. **Prerequisites**: Use quest chains to tell stories
5. **Optional Objectives**: Add bonus objectives for extra rewards
6. **Time Limits**: Use ExpiresAt for time-sensitive quests
7. **Repeatability**: Daily/weekly quests for ongoing engagement

## WebSocket Events

Quest system broadcasts events:
- `quest.available` - New quest available
- `quest.started` - Player started quest
- `quest.progress` - Objective updated
- `quest.completed` - Quest finished
- `quest.abandoned` - Quest abandoned
- `quest.failed` - Quest failed (time limit, etc.)

## Performance

- Quest lookup: O(1) via dictionary
- Available quests: O(n) filtered by criteria
- Quest progress update: O(n) where n = number of objectives
- Completed quest check: O(n) where n = number of completed quests

## Future Enhancements

- Quest branching (multiple outcomes)
- Dynamic quest generation by AI
- Quest sharing in parties
- Quest markers and waypoints
- Voice acted quest dialogues
- Cutscenes for major quests

---

**Module**: GameEngine Quest System  
**Version**: v4.8.9 (Phase 4.2)  
**Status**: ✅ Production Ready  
**Last Updated**: 2025-01-13
