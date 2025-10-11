using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.CodeGeneration;

/// <summary>
/// AI Code Generation Module - Translates natural language prompts into code, Configurations, and data structures.
/// Supports game creation use cases (e.g., world Generation, city layouts, NPC creation) via text commands.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AICodeGenModule : ModuleBase
{
    public override string Name => "AICodeGen";

    private ModuleManager? _manager;
    private string _outputPath = string.Empty;
    private readonly List<GeneratedProject> _GeneratedProjects = new();
    private readonly Dictionary<string, ProjectTemplate> _templates = new();
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _outputPath = Path.Combine(AppContext.BaseDirectory, "Generated_projects");
        
        if (!Directory.Exists(_outputPath))
        {
            Directory.CreateDirectory(_outputPath);
        }

        InitializeTemplates();
        LogInfo("AI Code Generation module initialized");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("codegen status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatus();
        }

        if (text.Equals("codegen list templates", StringComparison.OrdinalIgnoreCase))
        {
            return ListTemplates();
        }

        if (text.Equals("codegen list projects", StringComparison.OrdinalIgnoreCase))
        {
            return ListProjects();
        }

        if (text.StartsWith("codegen Generate ", StringComparison.OrdinalIgnoreCase))
        {
            var prompt = text["codegen Generate ".Length..].Trim();
            return GenerateFromPrompt(prompt);
        }

        if (text.StartsWith("codegen review ", StringComparison.OrdinalIgnoreCase))
        {
            var projectName = text["codegen review ".Length..].Trim();
            return ReviewProject(projectName);
        }

        if (text.StartsWith("codegen approve ", StringComparison.OrdinalIgnoreCase))
        {
            var projectName = text["codegen approve ".Length..].Trim();
            return ApproveProject(projectName);
        }

        return "Unknown codegen command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "AI Code Generation commands:",
            "  codegen Generate <prompt>  - Generate code from natural language (e.g., 'Create an MMORPG with medieval city')",
            "  codegen list templates     - Show available project templates",
            "  codegen list projects      - Show all Generated projects",
            "  codegen review <project>   - Review Generated code before deployment",
            "  codegen approve <project>  - Approve and finalize a project",
            "  codegen status             - Show module status",
            "  help                       - Show this help message",
            "",
            "Example prompts:",
            "  - Create me an MMORPG Game, with a Medieval Centralized City as the Main Spawn Point entry",
            "  - Generate a fantasy RPG with quest system and NPC dialogue",
            "  - Build a dungeon cRawler with procedural level Generation"
        );
    }

    private string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("AI Code Generation Status:");
        sb.AppendLine();
        sb.AppendLine($"Output Path: {_outputPath}");
        sb.AppendLine($"Available Templates: {_templates.Count}");
        sb.AppendLine($"Generated Projects: {_GeneratedProjects.Count}");
        sb.AppendLine($"Projects Awaiting Review: {_GeneratedProjects.Count(p => !p.IsApproved)}");
        sb.AppendLine($"Approved Projects: {_GeneratedProjects.Count(p => p.IsApproved)}");

        if (_manager != null)
        {
            var aiLanguage = _manager.Modules.FirstOrDefault(m => m.Instance?.Name == "AILanguage");
            if (aiLanguage?.Instance != null)
            {
                sb.AppendLine();
                sb.AppendLine("✅ AILanguageModule integration: Available");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("⚠️  AILanguageModule integration: Not available");
            }
        }

        return sb.ToString();
    }

    private string ListTemplates()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Available Project Templates:");
        sb.AppendLine();

        foreach (var (name, template) in _templates.OrderBy(t => t.Key))
        {
            sb.AppendLine($"📦 {name}");
            sb.AppendLine($"   {template.Description}");
            sb.AppendLine($"   Stack: {template.TechStack}");
            sb.AppendLine($"   Components: {string.Join(", ", template.Components)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ListProjects()
    {
        if (_GeneratedProjects.Count == 0)
        {
            return "No projects Generated yet. Use 'codegen Generate <prompt>' to create one.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Generated Projects:");
        sb.AppendLine();

        foreach (var project in _GeneratedProjects.OrderByDescending(p => p.CreatedAt))
        {
            var status = project.IsApproved ? "✅ Approved" : "⏳ Awaiting Review";
            sb.AppendLine($"{status} - {project.Name}");
            sb.AppendLine($"   Created: {project.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"   Template: {project.TemplateName}");
            sb.AppendLine($"   Path: {project.Path}");
            sb.AppendLine($"   Files: {project.GeneratedFiles.Count}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateFromPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return "Error: Please provide a prompt. Example: codegen Generate Create an MMORPG with medieval city";
        }

        try
        {
            // Parse the prompt to determine the project type
            var (projectType, features) = ParsePrompt(prompt);
            
            if (!_templates.ContainsKey(projectType))
            {
                return $"Error: No template found for project type '{projectType}'. Use 'codegen list templates' to see available templates.";
            }

            // Generate a unique project name
            var projectName = GenerateProjectName(projectType);
            var projectPath = Path.Combine(_outputPath, projectName);
            Directory.CreateDirectory(projectPath);

            // Generate the project files
            var template = _templates[projectType];
            var GeneratedFiles = GenerateProjectFiles(template, projectPath, features, prompt);

            // Create project record
            var project = new GeneratedProject
            {
                Name = projectName,
                TemplateName = projectType,
                Path = projectPath,
                CreatedAt = DateTime.UtcNow,
                Prompt = prompt,
                GeneratedFiles = GeneratedFiles,
                IsApproved = false
            };

            _GeneratedProjects.Add(project);

            // Generate summary
            var sb = new StringBuilder();
            sb.AppendLine($"✅ Project '{projectName}' Generated successfully!");
            sb.AppendLine();
            sb.AppendLine($"Template: {projectType}");
            sb.AppendLine($"Features: {string.Join(", ", features)}");
            sb.AppendLine($"Location: {projectPath}");
            sb.AppendLine($"Files Generated: {GeneratedFiles.Count}");
            sb.AppendLine();
            sb.AppendLine("Generated files:");
            foreach (var file in GeneratedFiles)
            {
                sb.AppendLine($"  📄 {Path.GetFileName(file)}");
            }
            sb.AppendLine();
            sb.AppendLine($"⚠️  Review required: Use 'codegen review {projectName}' to inspect the code");
            sb.AppendLine($"✅ To finalize: Use 'codegen approve {projectName}' after review");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Code Generation error: {ex.Message}");
            return $"Error Generating code: {ex.Message}";
        }
    }

    private (string projectType, List<string> features) ParsePrompt(string prompt)
    {
        var promptLower = prompt.ToLowerInvariant();
        
        // Determine project type based on keywords
        string projectType = "generic_game";
        
        if (promptLower.Contains("mmorpg") || (promptLower.Contains("multiplayer") && promptLower.Contains("rpg")))
        {
            projectType = "mmorpg";
        }
        else if (promptLower.Contains("rpg") || promptLower.Contains("role playing"))
        {
            projectType = "rpg";
        }
        else if (promptLower.Contains("fps") || promptLower.Contains("shooter"))
        {
            projectType = "fps";
        }
        else if (promptLower.Contains("platformer"))
        {
            projectType = "platformer";
        }

        // Extract features
        var features = new List<string>();
        
        if (promptLower.Contains("medieval") || promptLower.Contains("fantasy"))
            features.Add("medieval_theme");
        if (promptLower.Contains("city") || promptLower.Contains("town"))
            features.Add("city_hub");
        if (promptLower.Contains("spawn") || promptLower.Contains("entry point"))
            features.Add("spawn_system");
        if (promptLower.Contains("quest"))
            features.Add("quest_system");
        if (promptLower.Contains("npc") || promptLower.Contains("character"))
            features.Add("npc_system");
        if (promptLower.Contains("dialogue") || promptLower.Contains("conversation"))
            features.Add("dialogue_system");
        if (promptLower.Contains("inventory"))
            features.Add("inventory_system");
        if (promptLower.Contains("combat") || promptLower.Contains("battle"))
            features.Add("combat_system");
        if (promptLower.Contains("level") || promptLower.Contains("progression"))
            features.Add("level_system");
        if (promptLower.Contains("procedural") || promptLower.Contains("Random"))
            features.Add("procedural_Generation");

        if (features.Count == 0)
        {
            features.Add("basic_setup");
        }

        return (projectType, features);
    }

    private string GenerateProjectName(string projectType)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{projectType}_{timestamp}";
    }

    private List<string> GenerateProjectFiles(ProjectTemplate template, string projectPath, List<string> features, string originalPrompt)
    {
        var GeneratedFiles = new List<string>();

        // Generate README
        var readmePath = Path.Combine(projectPath, "README.md");
        File.WriteAllText(readmePath, GenerateReadme(template, features, originalPrompt));
        GeneratedFiles.Add(readmePath);

        // Generate main project file based on stack
        if (template.TechStack.Contains("C#"))
        {
            var csprojPath = Path.Combine(projectPath, $"{Path.GetFileName(projectPath)}.csproj");
            File.WriteAllText(csprojPath, GenerateCSharpProject(template));
            GeneratedFiles.Add(csprojPath);

            var ProgramPath = Path.Combine(projectPath, "Program.cs");
            File.WriteAllText(ProgramPath, GenerateCSharpProgram(template, features));
            GeneratedFiles.Add(ProgramPath);
        }
        else if (template.TechStack.Contains("Python"))
        {
            var mainPath = Path.Combine(projectPath, "main.py");
            File.WriteAllText(mainPath, GeneratePythonMain(template, features));
            GeneratedFiles.Add(mainPath);

            var requirementsPath = Path.Combine(projectPath, "requirements.txt");
            File.WriteAllText(requirementsPath, GeneratePythonRequirements(template));
            GeneratedFiles.Add(requirementsPath);
        }

        // Generate game-specific files
        if (features.Contains("city_hub") || features.Contains("medieval_theme"))
        {
            var worldPath = Path.Combine(projectPath, "world_config.json");
            File.WriteAllText(worldPath, GenerateWorldConfig(features));
            GeneratedFiles.Add(worldPath);
        }

        if (features.Contains("spawn_system"))
        {
            var spawnPath = Path.Combine(projectPath, "spawn_points.json");
            File.WriteAllText(spawnPath, GenerateSpawnConfig());
            GeneratedFiles.Add(spawnPath);
        }

        if (features.Contains("npc_system"))
        {
            var npcPath = Path.Combine(projectPath, "npcs.json");
            File.WriteAllText(npcPath, GenerateNPCConfig());
            GeneratedFiles.Add(npcPath);
        }

        // Generate Configuration file
        var configPath = Path.Combine(projectPath, "game_config.json");
        File.WriteAllText(configPath, GenerateGameConfig(template, features));
        GeneratedFiles.Add(configPath);

        // Generate .gitignore
        var gitignorePath = Path.Combine(projectPath, ".gitignore");
        File.WriteAllText(gitignorePath, GenerateGitignore(template));
        GeneratedFiles.Add(gitignorePath);

        return GeneratedFiles;
    }

    private string GenerateReadme(ProjectTemplate template, List<string> features, string originalPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {template.Name}");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"This project was Generated by ASHATCore AI Code Generation based on the prompt:");
        sb.AppendLine($"> {originalPrompt}");
        sb.AppendLine();
        sb.AppendLine("## Description");
        sb.AppendLine();
        sb.AppendLine(template.Description);
        sb.AppendLine();
        sb.AppendLine("## Tech Stack");
        sb.AppendLine();
        sb.AppendLine($"- **Platform**: {template.TechStack}");
        sb.AppendLine($"- **Components**: {string.Join(", ", template.Components)}");
        sb.AppendLine();
        sb.AppendLine("## Features");
        sb.AppendLine();
        foreach (var feature in features)
        {
            sb.AppendLine($"- {feature.Replace("_", " ").ToUpper()}");
        }
        sb.AppendLine();
        sb.AppendLine("## Getting Started");
        sb.AppendLine();
        if (template.TechStack.Contains("C#"))
        {
            sb.AppendLine("### Prerequisites");
            sb.AppendLine("- .NET 9.0 SDK or higher");
            sb.AppendLine();
            sb.AppendLine("### Build and Run");
            sb.AppendLine("```bash");
            sb.AppendLine("dotnet restore");
            sb.AppendLine("dotnet build");
            sb.AppendLine("dotnet run");
            sb.AppendLine("```");
        }
        else if (template.TechStack.Contains("Python"))
        {
            sb.AppendLine("### Prerequisites");
            sb.AppendLine("- Python 3.8 or higher");
            sb.AppendLine();
            sb.AppendLine("### Setup and Run");
            sb.AppendLine("```bash");
            sb.AppendLine("pip install -r requirements.txt");
            sb.AppendLine("python main.py");
            sb.AppendLine("```");
        }
        sb.AppendLine();
        sb.AppendLine("## Configuration");
        sb.AppendLine();
        sb.AppendLine("Edit `game_config.json` to customize game settings.");
        sb.AppendLine();
        sb.AppendLine("## Generated Files");
        sb.AppendLine();
        sb.AppendLine("- `README.md` - This file");
        sb.AppendLine("- `game_config.json` - Main game Configuration");
        if (features.Contains("city_hub"))
            sb.AppendLine("- `world_config.json` - World and city layout");
        if (features.Contains("spawn_system"))
            sb.AppendLine("- `spawn_points.json` - Player spawn Configurations");
        if (features.Contains("npc_system"))
            sb.AppendLine("- `npcs.json` - NPC definitions and behaviors");
        sb.AppendLine();
        sb.AppendLine("## Next Steps");
        sb.AppendLine();
        sb.AppendLine("1. Review the Generated code and Configurations");
        sb.AppendLine("2. Customize game logic in the main Program file");
        sb.AppendLine("3. Add assets (models, textures, sounds)");
        sb.AppendLine("4. Implement additional features as needed");
        sb.AppendLine("5. Test and iteRate");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"**Generated by**: ASHATCore AI Code Generation Module");
        sb.AppendLine($"**Generated at**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Template**: {template.Name}");
        
        return sb.ToString();
    }

    private string GenerateCSharpProject(ProjectTemplate template)
    {
        return @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.3"" />
  </ItemGroup>
</Project>";
    }

    private string GenerateCSharpProgram(ProjectTemplate template, List<string> features)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.IO;");
        sb.AppendLine("using Newtonsoft.Json;");
        sb.AppendLine();
        sb.AppendLine($"namespace {template.Name.Replace(" ", "")}");
        sb.AppendLine("{");
        sb.AppendLine("    class Program");
        sb.AppendLine("    {");
        sb.AppendLine("        static void Main(string[] args)");
        sb.AppendLine("        {");
        sb.AppendLine($"            Console.WriteLine(\"Welcome to {template.Name}!\");");
        sb.AppendLine("            Console.WriteLine(\"Generated by ASHATCore AI Code Generation\");");
        sb.AppendLine("            Console.WriteLine();");
        sb.AppendLine();
        sb.AppendLine("            // Load game Configuration");
        sb.AppendLine("            var config = LoadConfiguration();");
        sb.AppendLine("            Console.WriteLine($\"Game Name: {config.GameName}\");");
        sb.AppendLine("            Console.WriteLine($\"Version: {config.Version}\");");
        sb.AppendLine("            Console.WriteLine();");
        sb.AppendLine();
        
        if (features.Contains("city_hub") || features.Contains("medieval_theme"))
        {
            sb.AppendLine("            // Initialize world");
            sb.AppendLine("            InitializeWorld();");
            sb.AppendLine();
        }
        
        if (features.Contains("spawn_system"))
        {
            sb.AppendLine("            // Setup spawn system");
            sb.AppendLine("            SetupSpawnPoints();");
            sb.AppendLine();
        }
        
        if (features.Contains("npc_system"))
        {
            sb.AppendLine("            // Load NPCs");
            sb.AppendLine("            LoadNPCs();");
            sb.AppendLine();
        }
        
        sb.AppendLine("            Console.WriteLine(\"Game systems initialized. Press any key to start...\");");
        sb.AppendLine("            Console.ReadKey();");
        sb.AppendLine();
        sb.AppendLine("            // Main game loop");
        sb.AppendLine("            RunGameLoop();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        static dynamic LoadConfiguration()");
        sb.AppendLine("        {");
        sb.AppendLine("            var json = File.ReadAllText(\"game_config.json\");");
        sb.AppendLine("            return JsonConvert.DeserializeObject<dynamic>(json)!;");
        sb.AppendLine("        }");
        sb.AppendLine();
        
        if (features.Contains("city_hub") || features.Contains("medieval_theme"))
        {
            sb.AppendLine("        static void InitializeWorld()");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"Initializing world...\");");
            sb.AppendLine("            if (File.Exists(\"world_config.json\"))");
            sb.AppendLine("            {");
            sb.AppendLine("                var worldJson = File.ReadAllText(\"world_config.json\");");
            sb.AppendLine("                var world = JsonConvert.DeserializeObject<dynamic>(worldJson);");
            sb.AppendLine("                Console.WriteLine($\"World: {world.name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        
        if (features.Contains("spawn_system"))
        {
            sb.AppendLine("        static void SetupSpawnPoints()");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"Setting up spawn points...\");");
            sb.AppendLine("            if (File.Exists(\"spawn_points.json\"))");
            sb.AppendLine("            {");
            sb.AppendLine("                var spawnJson = File.ReadAllText(\"spawn_points.json\");");
            sb.AppendLine("                var spawns = JsonConvert.DeserializeObject<dynamic>(spawnJson);");
            sb.AppendLine("                Console.WriteLine($\"Spawn points configured: {spawns.spawn_points.Count}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        
        if (features.Contains("npc_system"))
        {
            sb.AppendLine("        static void LoadNPCs()");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"Loading NPCs...\");");
            sb.AppendLine("            if (File.Exists(\"npcs.json\"))");
            sb.AppendLine("            {");
            sb.AppendLine("                var npcJson = File.ReadAllText(\"npcs.json\");");
            sb.AppendLine("                var npcs = JsonConvert.DeserializeObject<dynamic>(npcJson);");
            sb.AppendLine("                Console.WriteLine($\"NPCs loaded: {npcs.npcs.Count}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        
        sb.AppendLine("        static void RunGameLoop()");
        sb.AppendLine("        {");
        sb.AppendLine("            Console.WriteLine(\"Game is running. Type 'exit' to quit.\");");
        sb.AppendLine("            while (true)");
        sb.AppendLine("            {");
        sb.AppendLine("                Console.Write(\"> \");");
        sb.AppendLine("                var input = Console.ReadLine();");
        sb.AppendLine("                if (input?.ToLower() == \"exit\") break;");
        sb.AppendLine("                Console.WriteLine($\"Command: {input}\");");
        sb.AppendLine("                // Add game logic here");
        sb.AppendLine("            }");
        sb.AppendLine("            Console.WriteLine(\"Game ended.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private string GeneratePythonMain(ProjectTemplate template, List<string> features)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#!/usr/bin/env python3");
        sb.AppendLine("import json");
        sb.AppendLine("import os");
        sb.AppendLine();
        sb.AppendLine("def main():");
        sb.AppendLine($"    print(\"Welcome to {template.Name}!\")");
        sb.AppendLine("    print(\"Generated by ASHATCore AI Code Generation\")");
        sb.AppendLine("    print()");
        sb.AppendLine();
        sb.AppendLine("    # Load game Configuration");
        sb.AppendLine("    config = load_Configuration()");
        sb.AppendLine("    print(f\"Game Name: {config['game_name']}\")");
        sb.AppendLine("    print(f\"Version: {config['version']}\")");
        sb.AppendLine("    print()");
        sb.AppendLine();
        
        if (features.Contains("city_hub") || features.Contains("medieval_theme"))
        {
            sb.AppendLine("    # Initialize world");
            sb.AppendLine("    initialize_world()");
            sb.AppendLine();
        }
        
        if (features.Contains("spawn_system"))
        {
            sb.AppendLine("    # Setup spawn system");
            sb.AppendLine("    setup_spawn_points()");
            sb.AppendLine();
        }
        
        if (features.Contains("npc_system"))
        {
            sb.AppendLine("    # Load NPCs");
            sb.AppendLine("    load_npcs()");
            sb.AppendLine();
        }
        
        sb.AppendLine("    print(\"Game systems initialized. Starting game...\")");
        sb.AppendLine("    run_game_loop()");
        sb.AppendLine();
        sb.AppendLine("def load_Configuration():");
        sb.AppendLine("    with open('game_config.json', 'r') as f:");
        sb.AppendLine("        return json.load(f)");
        sb.AppendLine();
        
        if (features.Contains("city_hub") || features.Contains("medieval_theme"))
        {
            sb.AppendLine("def initialize_world():");
            sb.AppendLine("    print(\"Initializing world...\")");
            sb.AppendLine("    if os.path.exists('world_config.json'):");
            sb.AppendLine("        with open('world_config.json', 'r') as f:");
            sb.AppendLine("            world = json.load(f)");
            sb.AppendLine("            print(f\"World: {world['name']}\")");
            sb.AppendLine();
        }
        
        if (features.Contains("spawn_system"))
        {
            sb.AppendLine("def setup_spawn_points():");
            sb.AppendLine("    print(\"Setting up spawn points...\")");
            sb.AppendLine("    if os.path.exists('spawn_points.json'):");
            sb.AppendLine("        with open('spawn_points.json', 'r') as f:");
            sb.AppendLine("            spawns = json.load(f)");
            sb.AppendLine("            print(f\"Spawn points configured: {len(spawns['spawn_points'])}\")");
            sb.AppendLine();
        }
        
        if (features.Contains("npc_system"))
        {
            sb.AppendLine("def load_npcs():");
            sb.AppendLine("    print(\"Loading NPCs...\")");
            sb.AppendLine("    if os.path.exists('npcs.json'):");
            sb.AppendLine("        with open('npcs.json', 'r') as f:");
            sb.AppendLine("            npcs = json.load(f)");
            sb.AppendLine("            print(f\"NPCs loaded: {len(npcs['npcs'])}\")");
            sb.AppendLine();
        }
        
        sb.AppendLine("def run_game_loop():");
        sb.AppendLine("    print(\"Game is running. Type 'exit' to quit.\")");
        sb.AppendLine("    while True:");
        sb.AppendLine("        user_input = input(\"> \")");
        sb.AppendLine("        if user_input.lower() == 'exit':");
        sb.AppendLine("            break");
        sb.AppendLine("        print(f\"Command: {user_input}\")");
        sb.AppendLine("        # Add game logic here");
        sb.AppendLine("    print(\"Game ended.\")");
        sb.AppendLine();
        sb.AppendLine("if __name__ == '__main__':");
        sb.AppendLine("    main()");
        
        return sb.ToString();
    }

    private string GeneratePythonRequirements(ProjectTemplate template)
    {
        return @"# Python dependencies
# Add game-specific packages as needed
";
    }

    private string GenerateWorldConfig(List<string> features)
    {
        var worldConfig = new
        {
            name = features.Contains("medieval_theme") ? "Medieval Kingdom" : "Fantasy World",
            description = "A vast world with cities, towns, and wilderness",
            regions = new[]
            {
                new
                {
                    id = "Central_city",
                    name = "Central City",
                    type = "city",
                    theme = features.Contains("medieval_theme") ? "medieval" : "fantasy",
                    description = "The main hub where players spawn and gather",
                    locations = new[]
                    {
                        new { name = "Town Square", type = "spawn_point" },
                        new { name = "Market District", type = "commerce" },
                        new { name = "Castle", type = "landmark" },
                        new { name = "Inn", type = "rest_area" },
                        new { name = "Guild Hall", type = "quest_hub" }
                    }
                },
                new
                {
                    id = "wilderness",
                    name = "Surrounding Wilderness",
                    type = "outdoor",
                    theme = "forest",
                    description = "Forests, fields, and dungeons surrounding the city",
                    locations = new[]
                    {
                        new { name = "Forest Path", type = "exploration" },
                        new { name = "Ancient Ruins", type = "dungeon" },
                        new { name = "River Crossing", type = "landmark" }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(worldConfig, _jsonOptions);
    }

    private string GenerateSpawnConfig()
    {
        var spawnConfig = new
        {
            spawn_points = new[]
            {
                new
                {
                    id = "main_spawn",
                    name = "Town Square",
                    location = new { x = 0, y = 0, z = 0 },
                    is_default = true,
                    description = "Main spawn point in the Central city"
                },
                new
                {
                    id = "inn_spawn",
                    name = "Inn",
                    location = new { x = 50, y = 0, z = 30 },
                    is_default = false,
                    description = "Respawn point for players who rested at the inn"
                }
            }
        };

        return JsonSerializer.Serialize(spawnConfig, _jsonOptions);
    }

    private string GenerateNPCConfig()
    {
        var npcConfig = new
        {
            npcs = new[]
            {
                new
                {
                    id = "blacksmith_001",
                    name = "Garret the Blacksmith",
                    type = "merchant",
                    location = "Market District",
                    dialogue = new[]
                    {
                        "Welcome to my forge!",
                        "I have the finest weapons and armor.",
                        "Need something repaired?"
                    },
                    services = new[] { "buy", "sell", "repair" }
                },
                new
                {
                    id = "guard_001",
                    name = "Captain Marcus",
                    type = "guard",
                    location = "Town Square",
                    dialogue = new[]
                    {
                        "Stay safe, traveler.",
                        "The roads have been dangerous lately.",
                        "Report any trouble to me."
                    },
                    services = new[] { "quest_giver", "information" }
                },
                new
                {
                    id = "innkeeper_001",
                    name = "Elara the Innkeeper",
                    type = "innkeeper",
                    location = "Inn",
                    dialogue = new[]
                    {
                        "Welcome to the Silver Swan Inn!",
                        "Need a room for the night?",
                        "We have the best ale in town."
                    },
                    services = new[] { "rest", "save_point", "rumors" }
                }
            }
        };

        return JsonSerializer.Serialize(npcConfig, _jsonOptions);
    }

    private string GenerateGameConfig(ProjectTemplate template, List<string> features)
    {
        var gameConfig = new
        {
            game_name = template.Name,
            version = "1.0.0",
            Generated_by = "ASHATCore AI Code Generation",
            Generated_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            tech_stack = template.TechStack,
            features = features,
            settings = new
            {
                max_players = template.Name.Contains("MMORPG") ? 1000 : 1,
                difficulty = "normal",
                auto_save = true,
                save_interval_seconds = 300
            }
        };

        return JsonSerializer.Serialize(gameConfig, _jsonOptions);
    }

    private string GenerateGitignore(ProjectTemplate template)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Build outputs");
        sb.AppendLine("bin/");
        sb.AppendLine("obj/");
        sb.AppendLine("out/");
        sb.AppendLine("build/");
        sb.AppendLine();
        sb.AppendLine("# IDE files");
        sb.AppendLine(".vs/");
        sb.AppendLine(".vscode/");
        sb.AppendLine("*.suo");
        sb.AppendLine("*.user");
        sb.AppendLine(".idea/");
        sb.AppendLine();
        sb.AppendLine("# Python");
        sb.AppendLine("__pycache__/");
        sb.AppendLine("*.pyc");
        sb.AppendLine("*.pyo");
        sb.AppendLine("venv/");
        sb.AppendLine(".env");
        sb.AppendLine();
        sb.AppendLine("# Logs");
        sb.AppendLine("*.log");
        sb.AppendLine("logs/");
        sb.AppendLine();
        sb.AppendLine("# OS files");
        sb.AppendLine(".DS_Store");
        sb.AppendLine("Thumbs.db");
        
        return sb.ToString();
    }

    private string ReviewProject(string projectName)
    {
        var project = _GeneratedProjects.FirstOrDefault(p => 
            p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));

        if (project == null)
        {
            return $"Error: Project '{projectName}' not found. Use 'codegen list projects' to see available projects.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📋 Review: {project.Name}");
        sb.AppendLine();
        sb.AppendLine($"Status: {(project.IsApproved ? "✅ Approved" : "⏳ Awaiting Approval")}");
        sb.AppendLine($"Created: {project.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Template: {project.TemplateName}");
        sb.AppendLine($"Original Prompt: {project.Prompt}");
        sb.AppendLine();
        sb.AppendLine($"Location: {project.Path}");
        sb.AppendLine();
        sb.AppendLine("Generated Files:");
        foreach (var file in project.GeneratedFiles)
        {
            var fileName = Path.GetFileName(file);
            var fileSize = new FileInfo(file).Length;
            sb.AppendLine($"  📄 {fileName} ({fileSize} bytes)");
        }
        sb.AppendLine();
        sb.AppendLine("File Preview (README.md):");
        sb.AppendLine("----------------------------------------");
        
        var readmePath = project.GeneratedFiles.FirstOrDefault(f => f.EndsWith("README.md"));
        if (readmePath != null && File.Exists(readmePath))
        {
            var readme = File.ReadAllText(readmePath);
            var lines = readme.Split('\n').Take(20);
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
            if (readme.Split('\n').Length > 20)
            {
                sb.AppendLine("... (truncated)");
            }
        }
        
        sb.AppendLine("----------------------------------------");
        sb.AppendLine();
        sb.AppendLine($"To approve this project: codegen approve {projectName}");
        
        return sb.ToString();
    }

    private string ApproveProject(string projectName)
    {
        var project = _GeneratedProjects.FirstOrDefault(p => 
            p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));

        if (project == null)
        {
            return $"Error: Project '{projectName}' not found. Use 'codegen list projects' to see available projects.";
        }

        if (project.IsApproved)
        {
            return $"Project '{projectName}' is already approved.";
        }

        project.IsApproved = true;
        project.ApprovedAt = DateTime.UtcNow;

        var sb = new StringBuilder();
        sb.AppendLine($"✅ Project '{projectName}' has been approved!");
        sb.AppendLine();
        sb.AppendLine($"Location: {project.Path}");
        sb.AppendLine($"Files: {project.GeneratedFiles.Count}");
        sb.AppendLine();
        sb.AppendLine("Next steps:");
        sb.AppendLine("1. Navigate to the project directory");
        sb.AppendLine("2. Build and run the project");
        sb.AppendLine("3. Customize the code as needed");
        sb.AppendLine("4. Add assets and resources");
        sb.AppendLine("5. Test and iteRate");
        
        return sb.ToString();
    }

    private void InitializeTemplates()
    {
        _templates["mmorpg"] = new ProjectTemplate
        {
            Name = "MMORPG Game",
            Description = "Massively Multiplayer Online Role-Playing Game with persistent world, cities, NPCs, and player progression",
            TechStack = "C# / .NET 9.0",
            Components = new List<string> { "Server", "Client", "Database", "World System", "NPC System", "Quest System" }
        };

        _templates["rpg"] = new ProjectTemplate
        {
            Name = "RPG Game",
            Description = "Single-player or co-op Role-Playing Game with story, quests, and character progression",
            TechStack = "C# / .NET 9.0",
            Components = new List<string> { "Game Engine", "character System", "Inventory", "Quest System", "Combat" }
        };

        _templates["fps"] = new ProjectTemplate
        {
            Name = "FPS Game",
            Description = "First-Person Shooter with fast-paced action and multiplayer support",
            TechStack = "C# / .NET 9.0",
            Components = new List<string> { "Game Engine", "Weapon System", "Player Controller", "Network Layer" }
        };

        _templates["platformer"] = new ProjectTemplate
        {
            Name = "Platformer Game",
            Description = "2D or 3D platformer with level-based progression and collectibles",
            TechStack = "C# / .NET 9.0",
            Components = new List<string> { "Game Engine", "Physics", "Level System", "Player Controller" }
        };

        _templates["generic_game"] = new ProjectTemplate
        {
            Name = "Generic Game Project",
            Description = "Basic game project template with core systems and Configuration",
            TechStack = "C# / .NET 9.0",
            Components = new List<string> { "Game Engine", "Configuration", "Asset Management" }
        };
    }
}

// Supporting classes
public class GeneratedProject
{
    public string Name { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public List<string> GeneratedFiles { get; set; } = new();
    public bool IsApproved { get; set; }
}

public class ProjectTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
}
