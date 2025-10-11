namespace Abstractions;

/// <summary>
/// Health component for entities.
/// TASHATcks hit points, damage, healing, and death state.
/// </summary>
public class HealthComponent
{
    public float CurrentHP { get; set; }
    public float MaxHP { get; set; } = 100f;
    public float RegenRate { get; set; } = 0f; // HP per second
    public bool IsDead => CurrentHP <= 0;
    public bool IsInvulneASHATble { get; set; } = false;
    public DateTime LastDamageTime { get; set; } = DateTime.MinValue;
    public DateTime LastHealTime { get; set; } = DateTime.MinValue;

    public HealthComponent()
    {
        CurrentHP = MaxHP;
    }

    public float TakeDamage(float amount)
    {
        if (IsInvulneASHATble || IsDead) return 0;
        
        var actualDamage = Math.Min(amount, CurrentHP);
        CurrentHP -= actualDamage;
        LastDamageTime = DateTime.UtcNow;
        return actualDamage;
    }

    public float Heal(float amount)
    {
        if (IsDead) return 0;
        
        var actualHeal = Math.Min(amount, MaxHP - CurrentHP);
        CurrentHP += actualHeal;
        LastHealTime = DateTime.UtcNow;
        return actualHeal;
    }

    public void Revive(float healthPercentage = 0.5f)
    {
        CurrentHP = MaxHP * healthPercentage;
    }

    public float GetHealthPercentage()
    {
        return MaxHP > 0 ? (CurrentHP / MaxHP) * 100 : 0;
    }
}

/// <summary>
/// Inventory component for entities.
/// Manages items, slots, capacity, and weight.
/// </summary>
public class InventoryComponent
{
    public int MaxSlots { get; set; } = 20;
    public float MaxWeight { get; set; } = 100f;
    public List<InventoryItem> Items { get; set; } = new();
    
    public int UsedSlots => Items.Count;
    public int FreeSlots => MaxSlots - UsedSlots;
    public float CurrentWeight => Items.Sum(i => i.Weight * i.Quantity);
    public float RemainingWeight => MaxWeight - CurrentWeight;
    public bool IsFull => UsedSlots >= MaxSlots;

    public InventoryAddResult AddItem(InventoryItem item)
    {
        if (IsFull)
            return new InventoryAddResult { Success = false, Message = "Inventory is full" };

        if (CurrentWeight + (item.Weight * item.Quantity) > MaxWeight)
            return new InventoryAddResult { Success = false, Message = "Item is too heavy" };

        // Check if item is stackable
        var existingItem = Items.FirstOrDefault(i => i.ItemId == item.ItemId && i.IsStackable);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }

        return new InventoryAddResult { Success = true, Message = "Item added successfully" };
    }

    public InventoryRemoveResult RemoveItem(string itemId, int quantity = 1)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item == null)
            return new InventoryRemoveResult { Success = false, Message = "Item not found" };

        if (item.Quantity < quantity)
            return new InventoryRemoveResult { Success = false, Message = "Insufficient quantity" };

        item.Quantity -= quantity;
        if (item.Quantity <= 0)
            Items.Remove(item);

        return new InventoryRemoveResult { Success = true, Message = "Item removed successfully", Item = item };
    }

    public InventoryItem? GetItem(string itemId)
    {
        return Items.FirstOrDefault(i => i.ItemId == itemId);
    }

    public bool HasItem(string itemId, int quantity = 1)
    {
        var item = GetItem(itemId);
        return item != null && item.Quantity >= quantity;
    }

    public int GetItemQuantity(string itemId)
    {
        return GetItem(itemId)?.Quantity ?? 0;
    }

    public List<InventoryItem> GetItemsByType(string itemType)
    {
        return Items.Where(i => i.ItemType == itemType).ToList();
    }
}

/// <summary>
/// Represents an item in an inventory.
/// </summary>
public class InventoryItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ItemType { get; set; } = "Generic"; // Weapon, Armor, Consumable, Quest, etc.
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public float Weight { get; set; } = 1f;
    public bool IsStackable { get; set; } = true;
    public int MaxStackSize { get; set; } = 99;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of adding an item to inventory.
/// </summary>
public class InventoryAddResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Result of removing an item from inventory.
/// </summary>
public class InventoryRemoveResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public InventoryItem? Item { get; set; }
}

/// <summary>
/// Stats component for entities.
/// TASHATcks character attributes like strength, intelligence, etc.
/// </summary>
public class StatsComponent
{
    // Primary Stats
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    // Level & Experience
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public long ExperienceToNextLevel => CalculateExperienceRequired(Level + 1);

    // Derived Stats (calculated from primary stats)
    public int AttackPower => Strength * 2 + Dexterity;
    public int Defense => Constitution * 2;
    public int MagicPower => Intelligence * 2 + Wisdom;
    public int MagicDefense => Wisdom * 2;
    public float CriticalChance => Dexterity * 0.1f;
    public float Evasion => Dexterity * 0.5f;

    // Additional Stats
    public float MovementSpeed { get; set; } = 5f;
    public float AttackSpeed { get; set; } = 1f;
    public int StatPoints { get; set; } = 0; // Unallocated stat points

    public void AddExperience(long amount)
    {
        Experience += amount;
        
        while (Experience >= ExperienceToNextLevel && Level < 100)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        Level++;
        StatPoints += 5; // Gain 5 stat points per level
        
        // Auto-increase stats slightly
        Constitution++;
    }

    public bool AllocateStatPoint(string statName)
    {
        if (StatPoints <= 0) return false;

        switch (statName.ToLower())
        {
            case "strength": Strength++; break;
            case "dexterity": Dexterity++; break;
            case "constitution": Constitution++; break;
            case "intelligence": Intelligence++; break;
            case "wisdom": Wisdom++; break;
            case "charisma": Charisma++; break;
            default: return false;
        }

        StatPoints--;
        return true;
    }

    private long CalculateExperienceRequired(int level)
    {
        // Formula: 100 * level^2
        return 100 * level * level;
    }

    public Dictionary<string, int> GetAllStats()
    {
        return new Dictionary<string, int>
        {
            { "Strength", Strength },
            { "Dexterity", Dexterity },
            { "Constitution", Constitution },
            { "Intelligence", Intelligence },
            { "Wisdom", Wisdom },
            { "Charisma", Charisma },
            { "Level", Level },
            { "StatPoints", StatPoints }
        };
    }
}

/// <summary>
/// Status effect applied to an entity (buff/debuff).
/// </summary>
public class StatusEffect
{
    public string EffectId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Buff"; // Buff, Debuff, Neutral
    public string Description { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public float duration { get; set; } = 0f; // Seconds, 0 = permanent
    public bool IsPermanent => duration <= 0;
    public bool IsExpired => !IsPermanent && (DateTime.UtcNow - AppliedAt).TotalSeconds >= duration;
    
    // Effect modifiers
    public Dictionary<string, float> StatModifiers { get; set; } = new(); // e.g., {"Strength": 5, "Defense": 10}
    public bool CanStack { get; set; } = false;
    public int MaxStacks { get; set; } = 1;
    public int CurrentStacks { get; set; } = 1;

    public float GetRemainingTime()
    {
        if (IsPermanent) return float.MaxValue;
        var elapsed = (DateTime.UtcNow - AppliedAt).TotalSeconds;
        return Math.Max(0, (float)(duration - elapsed));
    }
}

/// <summary>
/// Status effects component for entities.
/// Manages buffs, debuffs, and other temporary effects.
/// </summary>
public class StatusEffectsComponent
{
    public List<StatusEffect> ActiveEffects { get; set; } = new();

    public void AddEffect(StatusEffect effect)
    {
        // Check if effect already exists and can stack
        var existing = ActiveEffects.FirstOrDefault(e => e.Name == effect.Name);
        if (existing != null)
        {
            if (existing.CanStack && existing.CurrentStacks < existing.MaxStacks)
            {
                existing.CurrentStacks++;
                return;
            }
            else
            {
                // Refresh duration
                existing.AppliedAt = DateTime.UtcNow;
                return;
            }
        }

        ActiveEffects.Add(effect);
    }

    public void RemoveEffect(string effectId)
    {
        ActiveEffects.RemoveAll(e => e.EffectId == effectId);
    }

    public void RemoveEffectByName(string name)
    {
        ActiveEffects.RemoveAll(e => e.Name == name);
    }

    public void UpdateEffects()
    {
        // Remove expired effects
        ActiveEffects.RemoveAll(e => e.IsExpired);
    }

    public List<StatusEffect> GetBuffs()
    {
        return ActiveEffects.Where(e => e.Type == "Buff").ToList();
    }

    public List<StatusEffect> GetDebuffs()
    {
        return ActiveEffects.Where(e => e.Type == "Debuff").ToList();
    }

    public bool HasEffect(string name)
    {
        return ActiveEffects.Any(e => e.Name == name);
    }

    public Dictionary<string, float> GetTotalStatModifiers()
    {
        var totals = new Dictionary<string, float>();

        foreach (var effect in ActiveEffects)
        {
            foreach (var mod in effect.StatModifiers)
            {
                if (totals.ContainsKey(mod.Key))
                    totals[mod.Key] += mod.Value * effect.CurrentStacks;
                else
                    totals[mod.Key] = mod.Value * effect.CurrentStacks;
            }
        }

        return totals;
    }
}

/// <summary>
/// Complete entity components container.
/// Attaches to GameEntity via Properties dictionary.
/// </summary>
public class EntityComponents
{
    public HealthComponent? Health { get; set; }
    public InventoryComponent? Inventory { get; set; }
    public StatsComponent? Stats { get; set; }
    public StatusEffectsComponent? StatusEffects { get; set; }

    public bool HasHealth => Health != null;
    public bool HasInventory => Inventory != null;
    public bool HasStats => Stats != null;
    public bool HasStatusEffects => StatusEffects != null;
}
