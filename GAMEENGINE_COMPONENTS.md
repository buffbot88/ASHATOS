# Game Engine Entity Components - Phase 4.2

## Overview

The Entity Components system provides RPG-style components for game entities, including health, inventory, stats, and status effects. This enables rich gameplay mechanics with damage, healing, item management, character progression, and buff/debuff systems.

## Components

### 🩸 Health Component

Tracks entity health, damage, healing, and death state.

**Features:**
- Current HP and Max HP
- HP regeneration rate
- Invulnerability flag
- Death detection
- Damage and heal tracking

**Example:**
```csharp
var health = new HealthComponent
{
    MaxHP = 150,
    CurrentHP = 150,
    RegenRate = 5f // 5 HP per second
};

// Take damage
var damage = health.TakeDamage(30); // Returns 30

// Heal
var healed = health.Heal(20); // Returns 20

// Check status
bool isDead = health.IsDead;
float percentage = health.GetHealthPercentage(); // Returns 93.3%

// Revive
if (health.IsDead)
{
    health.Revive(0.5f); // Revive at 50% HP
}
```

### 🎒 Inventory Component

Manages items with weight, slots, stacking, and quantity.

**Features:**
- Slot-based inventory (configurable max slots)
- Weight system (max weight limit)
- Item stacking for stackable items
- Item types (Weapon, Armor, Consumable, Quest, etc.)
- Add/remove/query items

**Example:**
```csharp
var inventory = new InventoryComponent
{
    MaxSlots = 20,
    MaxWeight = 100f
};

// Add item
var sword = new InventoryItem
{
    Name = "Iron Sword",
    ItemType = "Weapon",
    Weight = 5f,
    Quantity = 1,
    IsStackable = false
};

var result = inventory.AddItem(sword);
if (result.Success)
{
    Console.WriteLine("Item added!");
}

// Add stackable item
var potion = new InventoryItem
{
    Name = "Health Potion",
    ItemType = "Consumable",
    Weight = 0.5f,
    Quantity = 10,
    IsStackable = true,
    MaxStackSize = 99
};

inventory.AddItem(potion);

// Check item
bool hasPotion = inventory.HasItem(potion.ItemId, 5); // Check for 5 potions

// Remove item
var removeResult = inventory.RemoveItem(potion.ItemId, 3); // Remove 3 potions

// Query
int usedSlots = inventory.UsedSlots;
float currentWeight = inventory.CurrentWeight;
List<InventoryItem> weapons = inventory.GetItemsByType("Weapon");
```

### 📊 Stats Component

Character attributes and level progression system.

**Features:**
- Primary stats (Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma)
- Level and experience system
- Derived stats (Attack Power, Defense, Magic Power, Critical Chance, etc.)
- Stat point allocation
- Auto-leveling with experience

**Example:**
```csharp
var stats = new StatsComponent
{
    Strength = 15,
    Dexterity = 12,
    Intelligence = 18,
    Level = 5
};

// Add experience
stats.AddExperience(1000);
// Auto-levels up if enough XP

// Allocate stat points
if (stats.StatPoints > 0)
{
    stats.AllocateStatPoint("strength");
    stats.AllocateStatPoint("intelligence");
}

// Check derived stats
int attackPower = stats.AttackPower; // Strength * 2 + Dexterity = 42
int magicPower = stats.MagicPower; // Intelligence * 2 + Wisdom = 46
float critChance = stats.CriticalChance; // Dexterity * 0.1 = 1.2%

// Get all stats
var allStats = stats.GetAllStats();
```

### ✨ Status Effects Component

Manages buffs, debuffs, and temporary effects.

**Features:**
- Buff and debuff system
- Duration-based effects
- Permanent effects
- Stackable effects
- Stat modifiers
- Auto-expiration

**Example:**
```csharp
var statusEffects = new StatusEffectsComponent();

// Add strength buff
var strengthBuff = new StatusEffect
{
    Name = "Strength Boost",
    Type = "Buff",
    Duration = 60f, // 60 seconds
    StatModifiers = new Dictionary<string, float>
    {
        { "Strength", 10 },
        { "AttackPower", 20 }
    }
};

statusEffects.AddEffect(strengthBuff);

// Add poison debuff
var poison = new StatusEffect
{
    Name = "Poison",
    Type = "Debuff",
    Duration = 30f,
    CanStack = true,
    MaxStacks = 3,
    StatModifiers = new Dictionary<string, float>
    {
        { "CurrentHP", -2 } // Damage over time
    }
};

statusEffects.AddEffect(poison);

// Update effects (remove expired)
statusEffects.UpdateEffects();

// Get total modifiers
var totalMods = statusEffects.GetTotalStatModifiers();
// Returns { "Strength": 10, "AttackPower": 20, "CurrentHP": -2 }

// Query effects
bool hasBuff = statusEffects.HasEffect("Strength Boost");
var buffs = statusEffects.GetBuffs();
var debuffs = statusEffects.GetDebuffs();
```

## Integration with GameEntity

Components are stored in the entity's Properties dictionary:

```csharp
var entity = new GameEntity
{
    Name = "Player Character",
    Type = "Player"
};

// Add components
var components = new EntityComponents
{
    Health = new HealthComponent { MaxHP = 200, CurrentHP = 200 },
    Inventory = new InventoryComponent { MaxSlots = 30, MaxWeight = 150 },
    Stats = new StatsComponent { Strength = 15, Intelligence = 12 },
    StatusEffects = new StatusEffectsComponent()
};

// Store in properties (will be serialized to JSON)
entity.Properties["components"] = components;

// Retrieve components
if (entity.Properties.TryGetValue("components", out var compsObj))
{
    var entityComponents = compsObj as EntityComponents;
    if (entityComponents?.HasHealth == true)
    {
        entityComponents.Health.TakeDamage(25);
    }
}
```

## API Endpoints

### Get Entity Components

```http
GET /api/gameengine/scene/{sceneId}/entity/{entityId}/components
```

Response:
```json
{
  "success": true,
  "components": {
    "health": {
      "currentHP": 180,
      "maxHP": 200,
      "regenRate": 5,
      "isDead": false,
      "healthPercentage": 90
    },
    "inventory": {
      "usedSlots": 5,
      "maxSlots": 30,
      "currentWeight": 25.5,
      "maxWeight": 150,
      "items": [...]
    },
    "stats": {
      "level": 12,
      "experience": 14400,
      "strength": 18,
      "dexterity": 15,
      "attackPower": 51,
      "defense": 34
    },
    "statusEffects": {
      "activeEffects": [...]
    }
  }
}
```

### Update Entity Health

```http
POST /api/gameengine/scene/{sceneId}/entity/{entityId}/health/damage
Content-Type: application/json

{
  "amount": 50
}
```

```http
POST /api/gameengine/scene/{sceneId}/entity/{entityId}/health/heal
Content-Type: application/json

{
  "amount": 30
}
```

### Add Item to Inventory

```http
POST /api/gameengine/scene/{sceneId}/entity/{entityId}/inventory/add
Content-Type: application/json

{
  "name": "Health Potion",
  "itemType": "Consumable",
  "weight": 0.5,
  "quantity": 5,
  "isStackable": true
}
```

### Allocate Stat Points

```http
POST /api/gameengine/scene/{sceneId}/entity/{entityId}/stats/allocate
Content-Type: application/json

{
  "statName": "strength"
}
```

### Apply Status Effect

```http
POST /api/gameengine/scene/{sceneId}/entity/{entityId}/effects/apply
Content-Type: application/json

{
  "name": "Haste",
  "type": "Buff",
  "duration": 60,
  "statModifiers": {
    "MovementSpeed": 2.0,
    "AttackSpeed": 0.5
  }
}
```

## Use Cases

### Combat System

```csharp
// Player attacks enemy
var player = GetEntity(playerId);
var enemy = GetEntity(enemyId);

var playerComponents = GetComponents(player);
var enemyComponents = GetComponents(enemy);

// Calculate damage
int damage = playerComponents.Stats.AttackPower - enemyComponents.Stats.Defense;
damage = Math.Max(1, damage); // Minimum 1 damage

// Apply damage
enemyComponents.Health.TakeDamage(damage);

// Check for death
if (enemyComponents.Health.IsDead)
{
    // Award experience
    int expReward = enemy.Level * 100;
    playerComponents.Stats.AddExperience(expReward);
    
    // Drop loot
    DropLoot(enemy, player);
}
```

### Healing System

```csharp
// Use health potion
var components = GetComponents(entity);

if (components.Inventory.HasItem("health_potion"))
{
    components.Inventory.RemoveItem("health_potion", 1);
    components.Health.Heal(50);
    
    Console.WriteLine($"Healed for 50 HP. Current HP: {components.Health.CurrentHP}");
}
```

### Buff System

```csharp
// Cast buff spell
var caster = GetEntity(casterId);
var target = GetEntity(targetId);

var targetComponents = GetComponents(target);

var buff = new StatusEffect
{
    Name = "Divine Shield",
    Type = "Buff",
    Duration = 30,
    StatModifiers = new Dictionary<string, float>
    {
        { "Defense", 50 },
        { "MagicDefense", 50 }
    }
};

targetComponents.StatusEffects.AddEffect(buff);
```

### Level Up System

```csharp
// Defeat enemy and gain XP
var playerComponents = GetComponents(player);

int currentLevel = playerComponents.Stats.Level;
playerComponents.Stats.AddExperience(500);

if (playerComponents.Stats.Level > currentLevel)
{
    Console.WriteLine($"Level up! Now level {playerComponents.Stats.Level}");
    Console.WriteLine($"Stat points to allocate: {playerComponents.Stats.StatPoints}");
    
    // Fully heal on level up
    playerComponents.Health.CurrentHP = playerComponents.Health.MaxHP;
}
```

## Performance

- Component access: O(1) via Properties dictionary
- Inventory search: O(n) where n = number of items
- Status effect updates: O(n) where n = number of active effects
- Stat calculations: O(1) computed properties

## Database Persistence

All components are automatically serialized to JSON and stored in the `Entities.Properties` column. No schema changes required.

## WebSocket Events

Component changes broadcast events:
- `entity.health.changed` - Health modified
- `entity.inventory.changed` - Inventory modified
- `entity.stats.changed` - Stats/level changed
- `entity.effects.changed` - Status effects changed

## Best Practices

1. **Always check for null**: Not all entities have all components
2. **Update status effects regularly**: Call `UpdateEffects()` to remove expired effects
3. **Cap values**: Health can't exceed MaxHP, inventory can't exceed MaxWeight
4. **Validate inputs**: Check item exists before removing, validate stat names
5. **Use derived stats**: Don't store computed values, calculate them on demand

## Future Enhancements

- Equipment system (equip slots for weapons/armor)
- Skill system (abilities, cooldowns, mana costs)
- Quest tracking per entity
- Achievement system
- Pet/companion system
- Crafting system

---

**Module**: GameEngine Entity Components  
**Version**: v4.8.9 (Phase 4.2)  
**Status**: ✅ Production Ready  
**Last Updated**: 2025-01-13
