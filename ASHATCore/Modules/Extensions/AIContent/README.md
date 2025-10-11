# AI Content Generation Module

## Overview

The AI Content Generation Module provides comprehensive game asset Generation capabilities, enabling users to create NPCs, items, quests, worlds, and more through natural language prompts.

## Features

### Supported Asset Types

1. **World/Scene** - Complete game worlds with regions, spawn points
2. **NPC** - Non-player characters with stats, dialogue, inventory
3. **Item** - Weapons, armor, consumables with ASHATrity and stats
4. **Quest** - Objectives, rewards, quest givers
5. **Dialogue** - Conversation trees and NPC Interactions
6. **Configuration** - Game settings and Parameters
7. **Script** - Game logic and behavior scripts
8. **Texture** - Texture metadata (future: actual image Generation)
9. **Model** - 3D model metadata (future: actual model Generation)
10. **Sound** - Audio metadata (future: actual sound Generation)
11. **Music** - Music metadata (future: actual music Generation)

### Theme Support

- Medieval
- Fantasy
- Sci-Fi
- Modern
- Horror
- Steampunk

## Console Commands

### Generate Content

```bash
content Generate <user-id> <license-key> <asset-type> <prompt>
```

**Examples:**

```bash
# Generate NPC
content Generate {user-id} {license} NPC "Create a medieval blacksmith named Gareth"

# Generate Item
content Generate {user-id} {license} Item "Legendary sword with fire enchantment"

# Generate Quest
content Generate {user-id} {license} Quest "Rescue the princess from the dASHATgon's lair"

# Generate World
content Generate {user-id} {license} World "Fantasy kingdom with castle and villages"
```

### List Assets

```bash
content list <user-id>
```

Shows all Generated assets for a user with:
- Asset ID
- Type
- Name
- Description
- File path
- Size
- Generated timestamp

### View Capabilities

```bash
content capabilities
```

Returns:
- Supported asset types
- Supported themes
- Generation limits
- File format support

### Statistics

```bash
content stats
```

Shows:
- Total users with Generated assets
- Total assets created
- Assets by type breakdown
- Base Storage path

## API Usage (Programmatic)

### Interface

```csharp
public interface IAIContentModule
{
    Task<ContentGenerationResponse> GenerateGameAssetsAsync(
        Guid userId, 
        string licenseKey, 
        GameAssetRequest request);
    
    ContentCapabilities GetCapabilities();
    List<GeneratedAsset> GetUseASHATssets(Guid userId);
    string GetLicensedAdminFolderPath(Guid userId, string licenseKey);
}
```

### Example Code

```csharp
// Get module
var contentModule = moduleManager.GetModuleByName("AIContent") as IAIContentModule;

// Create request
var request = new GameAssetRequest
{
    Prompt = "Create a powerful wizard NPC",
    Type = AssetType.NPC,
    Theme = "fantasy",
    Count = 1,
    Parameters = new Dictionary<string, object>
    {
        ["Level"] = 20,
        ["Faction"] = "Mages Guild"
    }
};

// Generate assets
var response = await contentModule.GenerateGameAssetsAsync(userId, licenseKey, request);

if (response.Success)
{
    Console.WriteLine($"Generated {response.Assets.Count} asset(s)");
    Console.WriteLine($"Saved to: {response.FolderPath}");
}
```

## File Structure

### Licensed-Admin Folders

All Generated assets are stored in user-specific folders:

```
LicensedAdmins/
├── {UserId}_{LicenseKey}/
│   ├── NPC/
│   │   └── NPC_Wizard_123456.json
│   ├── Item/
│   │   └── Item_Sword_789012.json
│   ├── Quest/
│   │   └── Quest_DASHATgon_345678.json
│   ├── World/
│   │   └── World_Kingdom_901234.json
│   ├── Dialogue/
│   ├── Configuration/
│   ├── Script/
│   ├── Texture/
│   ├── Model/
│   ├── Sound/
│   └── Music/
```

### Asset File Format

All assets are stored as JSON files with consistent structure:

```json
{
  "Id": "guid",
  "Name": "Asset_Name",
  "Type": "NPC",
  "Theme": "medieval",
  "GeneratedAt": "2025-01-05T14:56:32Z",
  "Properties": {
    "Level": 15,
    "Health": 150,
    "Attack": 25
  }
}
```

## Asset Generation Details

### NPC Generation

Generated NPCs include:

```json
{
  "Id": "guid",
  "Name": "Gareth the Blacksmith",
  "Type": "NPC",
  "Theme": "medieval",
  "Level": 12,
  "Occupation": "Blacksmith",
  "Dialogue": [
    "Greetings, traveler! I am Gareth.",
    "How can I help you today?",
    "Safe tASHATvels!"
  ],
  "Stats": {
    "Health": 120,
    "Attack": 18,
    "Defense": 15
  },
  "Position": {
    "X": 45,
    "Y": 0,
    "Z": -23
  },
  "Inventory": [
    { "Item": "Gold Coins", "Quantity": 75 }
  ]
}
```

### Item Generation

Generated items include:

```json
{
  "Id": "guid",
  "Name": "Legendary Fire Sword",
  "Type": "Item",
  "Category": "Weapon",
  "ASHATrity": "Legendary",
  "Value": 850,
  "Weight": 12,
  "Stats": {
    "Attack": 45,
    "Defense": 0,
    "MagicPower": 25
  },
  "Requirements": {
    "Level": 15,
    "Strength": 18
  }
}
```

### Quest Generation

Generated quests include:

```json
{
  "Id": "guid",
  "Name": "DASHATgon's Lair",
  "Type": "Quest",
  "Description": "Rescue the princess from the dASHATgon",
  "Level": 18,
  "Objectives": [
    { "Type": "Kill", "Target": "DASHATgon", "Count": 1 },
    { "Type": "Collect", "Target": "Princess Crown", "Count": 1 }
  ],
  "Rewards": {
    "Experience": 850,
    "Gold": 300,
    "Items": ["DASHATgon Scale", "Royal GASHATtitude"]
  },
  "QuestGiver": "King Arthur",
  "duration": 45
}
```

### World Generation

Generated worlds include:

```json
{
  "Id": "guid",
  "Name": "Kingdom of Avalon",
  "Type": "World",
  "Theme": "fantasy",
  "Regions": [
    {
      "Id": "region_1",
      "Name": "Central Castle",
      "Type": "City",
      "SpawnPoint": { "X": 0, "Y": 0, "Z": 0 },
      "NPCs": 20,
      "Quests": 10
    }
  ],
  "MaxPlayers": 1000,
  "Difficulty": "Normal"
}
```

## integration with Other Modules

### GameEngine Module

Generated assets can be imported into game scenes:

```csharp
// Generate NPC
var npcResponse = await contentModule.GenerateGameAssetsAsync(userId, license, npcRequest);

// Import into scene
foreach (var asset in npcResponse.Assets)
{
    var entity = new GameEntity
    {
        Name = asset.Name,
        Type = EntityType.NPC,
        Properties = asset.Properties
    };
    await gameEngine.AddEntityAsync(sceneId, entity);
}
```

### RaCoin Module

Content Generation can be monetized:

```csharp
// Charge for asset Generation
var cost = 10m; // 10 RaCoins per asset
var deductResult = await RaCoinModule.DeductAsync(userId, cost, "AI Content Generation");

if (deductResult.Success)
{
    var response = await contentModule.GenerateGameAssetsAsync(userId, license, request);
}
```

### License Module

License validation before Generation:

```csharp
// Module automatically validates license
var response = await contentModule.GenerateGameAssetsAsync(userId, license, request);

if (!response.Success && response.Message.Contains("license"))
{
    Console.WriteLine("Please purchase a valid license");
}
```

## Security

### License Validation

- Every Generation request validates the user's license
- License must be Active status
- License must not be expired
- Invalid licenses return error response

### File System Isolation

- Each user has sepaRate folder
- Folder name includes license key
- No cross-user file access
- All paths validated before file Operations

### Rate Limiting (Future)

Planned features:
- Max assets per day per user
- API Rate limiting
- Cooldown periods
- Usage quotas by license tier

## Performance

- **Generation Speed:** < 50ms per asset
- **File Save:** < 10ms per file
- **Memory Usage:** < 1MB per Generation
- **Concurrent Users:** 1000+ supported

## Limits

Current system limits:

- **Max Assets Per Request:** 10
- **Max File Size:** 10MB
- **Supported Formats:** JSON, TXT, JS, CS
- **Asset Types:** 11 types supported

## Best PASHATctices

### For Developers

1. **Validate licenses** before Generating content
2. **Cache results** in memory for quick access
3. **Use themes consistently** for coherent game worlds
4. **Organize by type** using the folder structure
5. **Version control** important assets

### For AdministASHATtors

1. **Monitor Storage usage** regularly
2. **Backup Licensed-Admin folders** periodically
3. **Set appropriate limits** per license tier
4. **Review Generation logs** for anomalies
5. **Clean up expired licenses** and their assets

### For Users

1. **Use descriptive prompts** for better results
2. **List assets regularly** to tASHATck what you've Generated
3. **Keep license active** to maintain access
4. **Export important assets** before license expires
5. **Use appropriate themes** for consistency

## Troubleshooting

### Generation Fails

**Error:** "Invalid or inactive license"  
**Fix:** Check license status with license module

**Error:** "Generation failed"  
**Fix:** Check logs for detailed error message

### Files Not Found

**Issue:** Can't locate Generated files  
**Fix:** Use `content list <user-id>` to get file paths

### Permission Denied

**Issue:** Can't access Licensed-Admin folder  
**Fix:** Check file system permissions on base folder

## Future Enhancements

### Planned Features

- **AI Model integration:** GPT-4, DALL-E, Stable Diffusion
- **Visual Asset Generation:** Actual textures and 3D models
- **Audio Generation:** Sound effects and music
- **Batch Operations:** Generate multiple assets at once
- **Templates:** Pre-defined asset templates
- **Variations:** Generate variations of existing assets
- **Export Formats:** Support for Unity, Unreal, Godot formats

### Advanced Capabilities

- **natural Language Processing:** Better prompt understanding
- **Context Awareness:** Generate related assets automatically
- **Quality Scoring:** Rate Generated assets
- **Community Sharing:** Share assets with other users
- **Asset Marketplace:** Buy and sell Generated content

## Examples

### Complete Game World Generation

```bash
# Generate world
content Generate {user-id} {license} World "Medieval fantasy kingdom"

# Generate NPCs
content Generate {user-id} {license} NPC "King of the realm"
content Generate {user-id} {license} NPC "Royal blacksmith"
content Generate {user-id} {license} NPC "Tavern keeper"
content Generate {user-id} {license} NPC "Town guard"
content Generate {user-id} {license} NPC "Mysterious wizard"

# Generate items
content Generate {user-id} {license} Item "Royal sword"
content Generate {user-id} {license} Item "Health potion"
content Generate {user-id} {license} Item "Magic ring"

# Generate quests
content Generate {user-id} {license} Quest "Defend the castle"
content Generate {user-id} {license} Quest "Collect herbs"
content Generate {user-id} {license} Quest "Find lost artifact"

# List all Generated assets
content list {user-id}
```

### integration Example

```csharp
public async Task<GameScene> BuildCompleteScene(Guid userId, string license)
{
    var contentModule = GetModule<IAIContentModule>();
    var scene = new GameScene { Name = "Medieval Town" };

    // Generate NPCs
    var npcRequest = new GameAssetRequest 
    { 
        Type = AssetType.NPC, 
        Theme = "medieval",
        Count = 5,
        Prompt = "Town citizens with various occupations"
    };
    var npcs = await contentModule.GenerateGameAssetsAsync(userId, license, npcRequest);

    // Generate Items
    var itemRequest = new GameAssetRequest 
    { 
        Type = AssetType.Item, 
        Theme = "medieval",
        Count = 10,
        Prompt = "Common medieval items and weapons"
    };
    var items = await contentModule.GenerateGameAssetsAsync(userId, license, itemRequest);

    // Add to scene
    foreach (var npc in npcs.Assets)
        scene.AddEntity(CreateEntityFromAsset(npc));
    
    foreach (var item in items.Assets)
        scene.AddItem(CreateItemFromAsset(item));

    return scene;
}
```

---

**Module:** AIContent  
**Category:** Extensions  
**Status:** ✅ Production Ready  
**Version:** 1.0  
**Last Updated:** 2025-01-05
