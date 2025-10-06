using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.AIContent;

/// <summary>
/// AI Content Generation Module - Generates game assets, configurations, and content for licensed admins.
/// Provides unified content generation system integrated with GameEngine, RaCoin, and License systems.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AIContentModule : ModuleBase, IAIContentModule
{
    public override string Name => "AIContent";

    private ModuleManager? _manager;
    private IGameEngineModule? _gameEngineModule;
    private ILicenseModule? _licenseModule;
    private string _assetsBasePath = string.Empty;
    private readonly Dictionary<Guid, List<GeneratedAsset>> _userAssets = new();
    private readonly object _lock = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Asset generation templates
    private readonly Dictionary<AssetType, List<string>> _assetTemplates = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _assetsBasePath = Path.Combine(AppContext.BaseDirectory, "LicensedAdmins");
        
        if (!Directory.Exists(_assetsBasePath))
        {
            Directory.CreateDirectory(_assetsBasePath);
        }

        if (_manager != null)
        {
            _gameEngineModule = _manager.GetModuleByName("GameEngine") as IGameEngineModule;
            _licenseModule = _manager.GetModuleByName("License") as ILicenseModule;
        }

        InitializeAssetTemplates();
        LogInfo("AI Content Generation module initialized - Asset generation system active");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("content stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.Equals("content capabilities", StringComparison.OrdinalIgnoreCase))
        {
            var caps = GetCapabilities();
            return JsonSerializer.Serialize(caps, _jsonOptions);
        }

        if (text.StartsWith("content generate", StringComparison.OrdinalIgnoreCase))
        {
            // content generate <user-id> <license-key> <type> <prompt>
            var parts = text.Split(' ', 5, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: content generate <user-id> <license-key> <asset-type> <prompt>";
            }
            
            if (Guid.TryParse(parts[2], out var userId) && 
                Enum.TryParse<AssetType>(parts[3], true, out var assetType))
            {
                var prompt = parts[4];
                var request = new GameAssetRequest
                {
                    Prompt = prompt,
                    Type = assetType,
                    Theme = "medieval",
                    Count = 1
                };
                
                var task = GenerateGameAssetsAsync(userId, parts[2], request);
                task.Wait();
                return JsonSerializer.Serialize(task.Result, _jsonOptions);
            }
            return "Invalid parameters";
        }

        if (text.StartsWith("content list", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: content list <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                var assets = GetUserAssets(userId);
                return JsonSerializer.Serialize(new
                {
                    UserId = userId,
                    TotalAssets = assets.Count,
                    Assets = assets.Select(a => new
                    {
                        a.Id,
                        a.Type,
                        a.Name,
                        a.Description,
                        a.FilePath,
                        a.GeneratedAtUtc,
                        SizeKB = a.SizeBytes / 1024.0
                    })
                }, _jsonOptions);
            }
            return "Invalid user ID format";
        }

        return "Unknown content command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "AI Content Generation commands:",
            "  content generate <user-id> <license-key> <type> <prompt> - Generate game assets",
            "  content list <user-id>                                   - List user's generated assets",
            "  content capabilities                                     - Show generation capabilities",
            "  content stats                                            - Show system statistics",
            "  help                                                     - Show this help message",
            "",
            "Asset Types:",
            "  World, NPC, Item, Quest, Dialogue, Texture, Model, Sound, Music, Script, Configuration",
            "",
            "Example:",
            "  content generate {user-id} {license} NPC 'Create a medieval blacksmith'"
        );
    }

    private string GetStats()
    {
        lock (_lock)
        {
            var totalAssets = _userAssets.Values.Sum(list => list.Count);
            var assetsByType = _userAssets.Values
                .SelectMany(list => list)
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() });

            return JsonSerializer.Serialize(new
            {
                TotalUsers = _userAssets.Count,
                TotalAssets = totalAssets,
                AssetsByType = assetsByType,
                AssetsBasePath = _assetsBasePath
            }, _jsonOptions);
        }
    }

    public async Task<ContentGenerationResponse> GenerateGameAssetsAsync(Guid userId, string licenseKey, GameAssetRequest request)
    {
        // Validate license
        if (_licenseModule != null)
        {
            var license = _licenseModule.GetUserLicense(userId);
            if (license == null || license.Status != LicenseStatus.Active)
            {
                return new ContentGenerationResponse
                {
                    Success = false,
                    Message = "Invalid or inactive license"
                };
            }
        }

        // Get or create user folder
        var folderPath = GetLicensedAdminFolderPath(userId, licenseKey);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var assets = new List<GeneratedAsset>();

        try
        {
            // Generate assets based on type
            for (int i = 0; i < request.Count; i++)
            {
                var asset = await GenerateAssetAsync(userId, licenseKey, request, folderPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            // Store assets in memory
            lock (_lock)
            {
                if (!_userAssets.ContainsKey(userId))
                {
                    _userAssets[userId] = new List<GeneratedAsset>();
                }
                _userAssets[userId].AddRange(assets);
            }

            LogInfo($"Generated {assets.Count} {request.Type} asset(s) for user {userId}");

            return new ContentGenerationResponse
            {
                Success = true,
                Message = $"Successfully generated {assets.Count} asset(s)",
                Assets = assets,
                FolderPath = folderPath,
                Metadata = new Dictionary<string, object>
                {
                    ["GeneratedAt"] = DateTime.UtcNow,
                    ["AssetType"] = request.Type.ToString(),
                    ["Theme"] = request.Theme
                }
            };
        }
        catch (Exception ex)
        {
            LogError($"Failed to generate assets: {ex.Message}");
            return new ContentGenerationResponse
            {
                Success = false,
                Message = $"Generation failed: {ex.Message}"
            };
        }
    }

    private async Task<GeneratedAsset?> GenerateAssetAsync(Guid userId, string licenseKey, GameAssetRequest request, string folderPath)
    {
        var asset = new GeneratedAsset
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LicenseKey = licenseKey,
            Type = request.Type,
            GeneratedAtUtc = DateTime.UtcNow
        };

        // Generate content based on type
        switch (request.Type)
        {
            case AssetType.NPC:
                GenerateNPC(asset, request);
                break;
            case AssetType.Item:
                GenerateItem(asset, request);
                break;
            case AssetType.Quest:
                GenerateQuest(asset, request);
                break;
            case AssetType.Dialogue:
                GenerateDialogue(asset, request);
                break;
            case AssetType.World:
                GenerateWorld(asset, request);
                break;
            case AssetType.Configuration:
                GenerateConfiguration(asset, request);
                break;
            case AssetType.Script:
                GenerateScript(asset, request);
                break;
            default:
                asset.Name = $"{request.Type}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
                asset.Description = $"Generated {request.Type}";
                asset.Content = JsonSerializer.Serialize(new
                {
                    Type = request.Type.ToString(),
                    Prompt = request.Prompt,
                    Theme = request.Theme,
                    GeneratedAt = DateTime.UtcNow
                }, _jsonOptions);
                break;
        }

        // Save to file
        var fileName = $"{asset.Name}_{asset.Id}.json";
        var filePath = Path.Combine(folderPath, request.Type.ToString(), fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, asset.Content);
        asset.FilePath = filePath;
        asset.SizeBytes = new FileInfo(filePath).Length;

        return asset;
    }

    private void GenerateNPC(GeneratedAsset asset, GameAssetRequest request)
    {
        var npcName = ExtractNameFromPrompt(request.Prompt, "NPC");
        asset.Name = npcName;
        asset.Description = $"NPC generated from: {request.Prompt}";

        var npc = new
        {
            Id = Guid.NewGuid(),
            Name = npcName,
            Type = "NPC",
            Theme = request.Theme,
            Level = Random.Shared.Next(1, 20),
            Occupation = GetRandomOccupation(request.Theme),
            Dialogue = new[]
            {
                $"Greetings, traveler! I am {npcName}.",
                "How can I help you today?",
                "Safe travels!"
            },
            Stats = new
            {
                Health = Random.Shared.Next(50, 200),
                Attack = Random.Shared.Next(5, 25),
                Defense = Random.Shared.Next(5, 20)
            },
            Position = new
            {
                X = Random.Shared.Next(-100, 100),
                Y = 0,
                Z = Random.Shared.Next(-100, 100)
            },
            Inventory = new[]
            {
                new { Item = "Gold Coins", Quantity = Random.Shared.Next(10, 100) }
            },
            Prompt = request.Prompt,
            GeneratedAt = DateTime.UtcNow
        };

        asset.Content = JsonSerializer.Serialize(npc, _jsonOptions);
        asset.Properties["Occupation"] = npc.Occupation;
        asset.Properties["Level"] = npc.Level;
    }

    private void GenerateItem(GeneratedAsset asset, GameAssetRequest request)
    {
        var itemName = ExtractNameFromPrompt(request.Prompt, "Item");
        asset.Name = itemName;
        asset.Description = $"Item generated from: {request.Prompt}";

        var item = new
        {
            Id = Guid.NewGuid(),
            Name = itemName,
            Type = "Item",
            Category = GetRandomItemCategory(),
            Rarity = GetRandomRarity(),
            Value = Random.Shared.Next(10, 1000),
            Weight = Random.Shared.Next(1, 50),
            Description = request.Prompt,
            Stats = new
            {
                Attack = Random.Shared.Next(0, 50),
                Defense = Random.Shared.Next(0, 50),
                MagicPower = Random.Shared.Next(0, 30)
            },
            Requirements = new
            {
                Level = Random.Shared.Next(1, 20),
                Strength = Random.Shared.Next(5, 20)
            },
            Theme = request.Theme,
            GeneratedAt = DateTime.UtcNow
        };

        asset.Content = JsonSerializer.Serialize(item, _jsonOptions);
        asset.Properties["Category"] = item.Category;
        asset.Properties["Rarity"] = item.Rarity;
    }

    private void GenerateQuest(GeneratedAsset asset, GameAssetRequest request)
    {
        var questName = ExtractNameFromPrompt(request.Prompt, "Quest");
        asset.Name = questName;
        asset.Description = $"Quest generated from: {request.Prompt}";

        var quest = new
        {
            Id = Guid.NewGuid(),
            Name = questName,
            Type = "Quest",
            Description = request.Prompt,
            Theme = request.Theme,
            Level = Random.Shared.Next(1, 20),
            Objectives = new[]
            {
                new { Type = "Kill", Target = "Enemies", Count = Random.Shared.Next(5, 20) },
                new { Type = "Collect", Target = "Items", Count = Random.Shared.Next(3, 10) }
            },
            Rewards = new
            {
                Experience = Random.Shared.Next(100, 1000),
                Gold = Random.Shared.Next(50, 500),
                Items = new[] { "Reward Item 1", "Reward Item 2" }
            },
            QuestGiver = $"NPC_{Random.Shared.Next(1000, 9999)}",
            Duration = Random.Shared.Next(10, 60),
            GeneratedAt = DateTime.UtcNow
        };

        asset.Content = JsonSerializer.Serialize(quest, _jsonOptions);
        asset.Properties["Level"] = quest.Level;
        asset.Properties["QuestGiver"] = quest.QuestGiver;
    }

    private void GenerateDialogue(GeneratedAsset asset, GameAssetRequest request)
    {
        var dialogueName = ExtractNameFromPrompt(request.Prompt, "Dialogue");
        asset.Name = dialogueName;
        asset.Description = $"Dialogue generated from: {request.Prompt}";

        var dialogue = new
        {
            Id = Guid.NewGuid(),
            Name = dialogueName,
            Type = "Dialogue",
            Theme = request.Theme,
            Lines = new[]
            {
                new { Speaker = "NPC", Text = "Welcome to our realm!" },
                new { Speaker = "Player", Text = "Thank you. What brings you here?" },
                new { Speaker = "NPC", Text = "I have a quest for brave adventurers like you." },
                new { Speaker = "Player", Text = "Tell me more." },
                new { Speaker = "NPC", Text = request.Prompt }
            },
            Context = request.Prompt,
            GeneratedAt = DateTime.UtcNow
        };

        asset.Content = JsonSerializer.Serialize(dialogue, _jsonOptions);
        asset.Properties["LineCount"] = dialogue.Lines.Length;
    }

    private void GenerateWorld(GeneratedAsset asset, GameAssetRequest request)
    {
        var worldName = ExtractNameFromPrompt(request.Prompt, "World");
        asset.Name = worldName;
        asset.Description = $"World generated from: {request.Prompt}";

        var world = new
        {
            Id = Guid.NewGuid(),
            Name = worldName,
            Type = "World",
            Theme = request.Theme,
            Description = request.Prompt,
            Regions = new[]
            {
                new
                {
                    Id = "region_1",
                    Name = "Central Hub",
                    Type = "City",
                    SpawnPoint = new { X = 0, Y = 0, Z = 0 },
                    NPCs = 10,
                    Quests = 5
                },
                new
                {
                    Id = "region_2",
                    Name = "Wilderness",
                    Type = "Forest",
                    SpawnPoint = new { X = 100, Y = 0, Z = 100 },
                    NPCs = 5,
                    Quests = 3
                }
            },
            MaxPlayers = 1000,
            Difficulty = "Normal",
            GeneratedAt = DateTime.UtcNow
        };

        asset.Content = JsonSerializer.Serialize(world, _jsonOptions);
        asset.Properties["RegionCount"] = world.Regions.Length;
        asset.Properties["MaxPlayers"] = world.MaxPlayers;
    }

    private void GenerateConfiguration(GeneratedAsset asset, GameAssetRequest request)
    {
        asset.Name = "GameConfig";
        asset.Description = $"Game configuration generated from: {request.Prompt}";

        var config = new
        {
            GameName = ExtractNameFromPrompt(request.Prompt, "Game"),
            Version = "1.0.0",
            Theme = request.Theme,
            GeneratedBy = "RaCore AI Content Generation",
            GeneratedAt = DateTime.UtcNow,
            Settings = new
            {
                MaxPlayers = request.Parameters.GetValueOrDefault("MaxPlayers", 1000),
                Difficulty = request.Parameters.GetValueOrDefault("Difficulty", "Normal"),
                AutoSave = true,
                SaveIntervalSeconds = 300
            },
            Features = new[]
            {
                "Player Management",
                "NPC System",
                "Quest System",
                "Inventory System",
                "Combat System"
            }
        };

        asset.Content = JsonSerializer.Serialize(config, _jsonOptions);
        asset.Properties["GameName"] = config.GameName;
    }

    private void GenerateScript(GeneratedAsset asset, GameAssetRequest request)
    {
        var scriptName = ExtractNameFromPrompt(request.Prompt, "Script");
        asset.Name = scriptName;
        asset.Description = $"Script generated from: {request.Prompt}";

        var script = new StringBuilder();
        script.AppendLine("// Auto-generated game script");
        script.AppendLine($"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        script.AppendLine($"// Prompt: {request.Prompt}");
        script.AppendLine();
        script.AppendLine("function initialize() {");
        script.AppendLine("    console.log('Game initialized');");
        script.AppendLine("    // Add initialization code here");
        script.AppendLine("}");
        script.AppendLine();
        script.AppendLine("function update(deltaTime) {");
        script.AppendLine("    // Game update logic");
        script.AppendLine("}");
        script.AppendLine();
        script.AppendLine("function render() {");
        script.AppendLine("    // Rendering logic");
        script.AppendLine("}");

        asset.Content = script.ToString();
        asset.Properties["Language"] = "JavaScript";
    }

    public ContentCapabilities GetCapabilities()
    {
        return new ContentCapabilities
        {
            SupportedTypes = Enum.GetValues<AssetType>().ToList(),
            SupportedThemes = new List<string> { "medieval", "fantasy", "sci-fi", "modern", "horror", "steampunk" },
            Limits = new Dictionary<string, object>
            {
                ["MaxAssetsPerRequest"] = 10,
                ["MaxFileSize"] = 10_000_000,
                ["SupportedFormats"] = new[] { "json", "txt", "js", "cs" }
            }
        };
    }

    public List<GeneratedAsset> GetUserAssets(Guid userId)
    {
        lock (_lock)
        {
            return _userAssets.GetValueOrDefault(userId, new List<GeneratedAsset>());
        }
    }

    public string GetLicensedAdminFolderPath(Guid userId, string licenseKey)
    {
        var sanitizedLicense = string.IsNullOrEmpty(licenseKey) ? "default" : 
            string.Join("", licenseKey.Take(8));
        return Path.Combine(_assetsBasePath, $"{userId}_{sanitizedLicense}");
    }

    private void InitializeAssetTemplates()
    {
        // Initialize templates for different asset types
        _assetTemplates[AssetType.NPC] = new List<string> { "Merchant", "Guard", "Blacksmith", "Mage", "Warrior" };
        _assetTemplates[AssetType.Item] = new List<string> { "Sword", "Shield", "Potion", "Armor", "Ring" };
        _assetTemplates[AssetType.Quest] = new List<string> { "Rescue Mission", "Fetch Quest", "Kill Quest", "Escort Mission" };
    }

    private string ExtractNameFromPrompt(string prompt, string defaultPrefix)
    {
        // Simple extraction - in production, use AI/NLP
        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            var name = string.Join(" ", words.Take(Math.Min(3, words.Length)));
            return $"{defaultPrefix}_{name.Replace(" ", "_")}_{DateTime.UtcNow:HHmmss}";
        }
        return $"{defaultPrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
    }

    private string GetRandomOccupation(string theme)
    {
        var occupations = theme.ToLower() switch
        {
            "medieval" => new[] { "Blacksmith", "Merchant", "Guard", "Farmer", "Innkeeper" },
            "fantasy" => new[] { "Mage", "Alchemist", "Enchanter", "Bard", "Ranger" },
            "sci-fi" => new[] { "Engineer", "Scientist", "Pilot", "Trader", "Soldier" },
            _ => new[] { "Vendor", "Citizen", "Worker", "Guard", "Shopkeeper" }
        };
        return occupations[Random.Shared.Next(occupations.Length)];
    }

    private string GetRandomItemCategory()
    {
        var categories = new[] { "Weapon", "Armor", "Consumable", "Quest Item", "Material", "Accessory" };
        return categories[Random.Shared.Next(categories.Length)];
    }

    private string GetRandomRarity()
    {
        var rarities = new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        return rarities[Random.Shared.Next(rarities.Length)];
    }
}
