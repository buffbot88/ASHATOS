# AI Code Generation Module

## Overview

The AI Code Generation Module enables users to create complex game systems and applications using natural human language instructions. Simply describe what you want to build, and RaCore will generate the code, configurations, and data structures for you.

## Features

✅ **Natural Language Code Generation** - Describe your game in plain English  
✅ **Multiple Game Templates** - MMORPG, RPG, FPS, Platformer, and more  
✅ **Intelligent Feature Detection** - Automatically identifies requested features from prompts  
✅ **Multi-Stack Support** - Generates C# .NET and Python projects  
✅ **Code Review Workflow** - Review generated code before deployment  
✅ **Structured Project Output** - Well-organized, production-ready code  
✅ **Game-Specific Configurations** - JSON configs for worlds, NPCs, spawn points  
✅ **Extensible Templates** - Easy to add new project types  

## Quick Start

### Basic Usage

```
codegen generate Create me an MMORPG Game, with a Medieval Centralized City as the Main Spawn Point entry
```

This single command will generate:
- Complete C# project structure
- Game configuration files
- World and city layouts
- NPC definitions
- Spawn point configurations
- README with setup instructions

### Review and Approve

```
codegen list projects              # See all generated projects
codegen review mmorpg_20250105_143022   # Review a specific project
codegen approve mmorpg_20250105_143022  # Approve after review
```

## Available Commands

| Command | Description |
|---------|-------------|
| `codegen generate <prompt>` | Generate code from natural language prompt |
| `codegen list templates` | Show available project templates |
| `codegen list projects` | Show all generated projects |
| `codegen review <project>` | Review generated code before deployment |
| `codegen approve <project>` | Approve and finalize a project |
| `codegen status` | Show module status and configuration |
| `help` | Show help message |

## Example Prompts

### MMORPG Example
```
codegen generate Create me an MMORPG Game, with a Medieval Centralized City as the Main Spawn Point entry
```

**Generated:**
- MMORPG project with medieval theme
- Central city hub with spawn point
- Multiple city locations (Town Square, Market, Castle, Inn, Guild Hall)
- NPC system with merchants, guards, and quest givers
- Configuration files for world, spawns, and NPCs

### RPG with Quest System
```
codegen generate Generate a fantasy RPG with quest system and NPC dialogue
```

**Generated:**
- RPG project structure
- Quest system implementation
- Dialogue system for NPCs
- Character and inventory systems

### Dungeon Crawler
```
codegen generate Build a dungeon crawler with procedural level generation
```

**Generated:**
- Dungeon crawler project
- Procedural generation systems
- Level and progression mechanics

## Project Templates

### MMORPG
- **Stack**: C# / .NET 9.0
- **Components**: Server, Client, Database, World System, NPC System, Quest System
- **Best for**: Large-scale multiplayer games with persistent worlds

### RPG
- **Stack**: C# / .NET 9.0
- **Components**: Game Engine, Character System, Inventory, Quest System, Combat
- **Best for**: Single-player or co-op role-playing games

### FPS
- **Stack**: C# / .NET 9.0
- **Components**: Game Engine, Weapon System, Player Controller, Network Layer
- **Best for**: Fast-paced action games

### Platformer
- **Stack**: C# / .NET 9.0
- **Components**: Game Engine, Physics, Level System, Player Controller
- **Best for**: 2D/3D platform games

### Generic Game
- **Stack**: C# / .NET 9.0
- **Components**: Game Engine, Configuration, Asset Management
- **Best for**: Custom game projects

## Feature Detection

The module intelligently detects features from your prompt:

| Keywords | Feature |
|----------|---------|
| medieval, fantasy | Medieval theme |
| city, town | City hub system |
| spawn, entry point | Spawn point system |
| quest | Quest system |
| npc, character | NPC system |
| dialogue, conversation | Dialogue system |
| inventory | Inventory system |
| combat, battle | Combat system |
| level, progression | Level/XP system |
| procedural, random | Procedural generation |

## Generated Project Structure

### C# Projects
```
<project_name>/
├── README.md                  # Project documentation
├── <project_name>.csproj      # .NET project file
├── Program.cs                 # Main entry point
├── game_config.json           # Game configuration
├── world_config.json          # World/map configuration
├── spawn_points.json          # Player spawn points
├── npcs.json                  # NPC definitions
└── .gitignore                 # Git ignore file
```

### Python Projects
```
<project_name>/
├── README.md                  # Project documentation
├── main.py                    # Main entry point
├── requirements.txt           # Python dependencies
├── game_config.json           # Game configuration
├── world_config.json          # World/map configuration
├── spawn_points.json          # Player spawn points
├── npcs.json                  # NPC definitions
└── .gitignore                 # Git ignore file
```

## Configuration Files

### game_config.json
Main game configuration with metadata and settings.

```json
{
  "game_name": "MMORPG Game",
  "version": "1.0.0",
  "tech_stack": "C# / .NET 9.0",
  "features": ["medieval_theme", "city_hub", "spawn_system"],
  "settings": {
    "max_players": 1000,
    "difficulty": "normal",
    "auto_save": true
  }
}
```

### world_config.json
Defines regions, locations, and world structure.

```json
{
  "name": "Medieval Kingdom",
  "regions": [
    {
      "id": "central_city",
      "name": "Central City",
      "type": "city",
      "locations": [
        { "name": "Town Square", "type": "spawn_point" },
        { "name": "Market District", "type": "commerce" }
      ]
    }
  ]
}
```

### spawn_points.json
Player spawn locations.

```json
{
  "spawn_points": [
    {
      "id": "main_spawn",
      "name": "Town Square",
      "location": { "x": 0, "y": 0, "z": 0 },
      "is_default": true
    }
  ]
}
```

### npcs.json
NPC definitions with dialogue and services.

```json
{
  "npcs": [
    {
      "id": "blacksmith_001",
      "name": "Garret the Blacksmith",
      "type": "merchant",
      "location": "Market District",
      "dialogue": ["Welcome to my forge!"],
      "services": ["buy", "sell", "repair"]
    }
  ]
}
```

## Code Review Workflow

### 1. Generate
```
codegen generate Create an MMORPG with medieval city
```

Output:
```
✅ Project 'mmorpg_20250105_143022' generated successfully!
⚠️  Review required: Use 'codegen review mmorpg_20250105_143022' to inspect the code
```

### 2. Review
```
codegen review mmorpg_20250105_143022
```

Output shows:
- Project details
- Generated files list
- File preview (README)
- Approval command

### 3. Approve
```
codegen approve mmorpg_20250105_143022
```

Output:
```
✅ Project 'mmorpg_20250105_143022' has been approved!
Next steps:
1. Navigate to the project directory
2. Build and run the project
...
```

## Integration with Other Modules

### AILanguageModule Integration
The Code Generation module can integrate with the AILanguageModule for enhanced natural language understanding:

```
# Module automatically detects and uses AILanguageModule if available
codegen status
```

Shows:
```
✅ AILanguageModule integration: Available
```

### MemoryModule Integration
Future versions will support context-aware code suggestions using the MemoryModule to remember user preferences and past projects.

## Building Generated Projects

### C# Projects
```bash
cd generated_projects/<project_name>
dotnet restore
dotnet build
dotnet run
```

### Python Projects
```bash
cd generated_projects/<project_name>
pip install -r requirements.txt
python main.py
```

## Customization

After generation, you can customize:

1. **Code Logic**: Modify Program.cs or main.py
2. **Configuration**: Edit JSON config files
3. **Features**: Add new game mechanics
4. **Assets**: Add models, textures, sounds
5. **Dependencies**: Update .csproj or requirements.txt

## Best Practices

### Writing Effective Prompts
✅ **Good**: "Create an MMORPG with medieval city, quest system, and NPC dialogue"  
❌ **Bad**: "Make a game"

✅ **Good**: "Build an FPS with multiplayer and weapon customization"  
❌ **Bad**: "FPS"

✅ **Good**: "Generate a platformer with procedural levels and collectibles"  
❌ **Bad**: "Platformer game"

### Key Points
- Be specific about game type and features
- Mention themes (medieval, sci-fi, fantasy)
- Include desired systems (quests, dialogue, combat)
- Specify multiplayer vs single-player if relevant

### Safety Guidelines
- Always review generated code before running
- Check configuration files for security issues
- Validate JSON files are properly formatted
- Test in a safe environment first
- Keep backups of customized code

## Output Directory

All generated projects are stored in:
```
<RaCore>/generated_projects/
```

Each project gets a unique timestamped name:
```
mmorpg_20250105_143022/
rpg_20250105_150312/
fps_20250105_162045/
```

## Troubleshooting

### "No template found for project type"
- Check available templates: `codegen list templates`
- Use more specific keywords in your prompt
- Try a different project type keyword

### "Project not found"
- List all projects: `codegen list projects`
- Check project name spelling
- Ensure the project was successfully generated

### "Error generating code"
- Check disk space in output directory
- Verify write permissions
- Review error message for details

## Extending the Module

### Adding New Templates

Edit `AICodeGenModule.cs` in the `InitializeTemplates()` method:

```csharp
_templates["new_type"] = new ProjectTemplate
{
    Name = "New Game Type",
    Description = "Description here",
    TechStack = "C# / .NET 9.0",
    Components = new List<string> { "Component1", "Component2" }
};
```

### Adding New Features

Update `ParsePrompt()` method to detect new keywords:

```csharp
if (promptLower.Contains("new_feature"))
    features.Add("new_feature_system");
```

Then add feature implementation in `GenerateProjectFiles()`.

## Future Enhancements

Planned features:
- [ ] Unity/Unreal Engine integration
- [ ] Visual scripting generation
- [ ] Database schema generation
- [ ] API endpoint generation
- [ ] Docker containerization
- [ ] Cloud deployment configs
- [ ] Asset pipeline generation
- [ ] Testing framework setup
- [ ] CI/CD pipeline templates
- [ ] Multi-language support (Go, Rust, JavaScript)

## Architecture

### Pipeline Flow
```
User Prompt
    ↓
Prompt Parser (detect type & features)
    ↓
Template Selection
    ↓
Code Generator (files, configs, docs)
    ↓
Project Record (review queue)
    ↓
Review & Approval
    ↓
Final Project (ready to use)
```

### Module Structure
```
AICodeGenModule
├── Process() - Command dispatcher
├── GenerateFromPrompt() - Main generation logic
├── ParsePrompt() - NLP feature extraction
├── GenerateProjectFiles() - File generation
├── Review/Approve workflows
└── Template management
```

## Technology Stack

- **Language**: C# 9.0
- **Framework**: .NET 9.0
- **Format**: JSON for configurations
- **Output**: Multi-language (C#, Python)

## Security Considerations

- Generated code is stored locally
- No external API calls during generation
- Review workflow prevents automatic deployment
- User approval required before finalization
- All configurations are human-readable JSON

## Performance

- **Generation Time**: < 1 second per project
- **Output Size**: 10-50 KB per project
- **Memory Usage**: Minimal (templates cached)
- **Scalability**: Unlimited projects

## Support

For issues or questions:
1. Check this README
2. Use `codegen status` to verify setup
3. Review generated README.md in projects
4. Check RaCore console output for errors

## License

Part of the RaCore project. See main repository for license information.

---

**Generated by**: RaCore Development Team  
**Module**: AI Code Generation (AICodeGen)  
**Version**: 1.0  
**Last Updated**: 2025-01-05
