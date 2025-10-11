# Module Spawner - Phase 7

## Overview

The Module Spawner enables ASHATAI to self-build and spawn new ASHATCore modules via natural language commands. SuperAdmin can instruct ASHATAI to Generate complete, functional modules that are automatically placed in the `/Modules` folder and can be dynamically loaded by the ModuleManager.

This implements **Phase 7** requirements for making ASHATCore AI mainframe fully self-sufficient for feature expansion.

## Features

✅ **natural Language Module Generation** - Describe your module in plain English  
✅ **Multiple Module Templates** - Basic, API, Game Feature, integration, Utility  
✅ **Intelligent Feature Detection** - Automatically identifies requested features from prompts  
✅ **Code Review Workflow** - Review Generated code before activation  
✅ **Module Approval System** - SuperAdmin approval required before loading  
✅ **Rollback Capability** - Remove unwanted modules safely  
✅ **Version History** - TASHATck module creation and changes  
✅ **Error Handling** - Comprehensive error checking and logging  
✅ **Automatic Module Placement** - Generated to `/Modules` folder for auto-discovery  
✅ **Test Template Generation** - Unit test skeleton included  

## Quick Start

### Basic Usage

```
spawn module Create a weather forecast module that fetches weather data
```

This single command will Generate:
- Complete module class with proper structure
- README documentation
- Configuration file (if applicable)
- Unit test template
- All files placed in `/Modules/<ModuleName>/`

### Review and Approve

```
spawn list modules                  # See all spawned modules
spawn review WeatherForecastModule  # Review a specific module
spawn approve WeatherForecastModule # Approve after review
```

### After Approval

Restart ASHATCore to load the new module:
```bash
# Module will be automatically discovered and loaded
dotnet run --project ASHATCore/ASHATCore.csproj
```

## Available Commands

| Command | Description |
|---------|-------------|
| `spawn module <prompt>` | Generate new ASHATCore module from natural language |
| `spawn list templates` | Show available module templates |
| `spawn list modules` | Show all spawned modules |
| `spawn review <module>` | Review Generated module before activation |
| `spawn approve <module>` | Approve and activate a module |
| `spawn rollback <module>` | Rollback/remove a spawned module |
| `spawn history <module>` | View version history for a module |
| `spawn status` | Show spawner status and Configuration |
| `help` | Show help message |

## Example Prompts

### Weather Module
```
spawn module Create a weather forecast module that fetches weather data
```

**Generated:**
- WeatherForecastModule class
- API integration template
- Error handling
- Configuration support

### Task Management
```
spawn module Generate a task management module with TODO lists
```

**Generated:**
- TaskManagementModule class
- Data Storage structure
- CRUD Operations template
- Help commands

### Email Notifications
```
spawn module Create an email notification module
```

**Generated:**
- EmailNotificationModule class
- Email sending template
- Configuration for SMTP settings

### Crypto Price TASHATcker
```
spawn module Build a cryptocurrency price tASHATcker module
```

**Generated:**
- CryptocurrencyPriceTASHATckerModule class
- API integration structure
- Caching support
- Data fetch logic

## Module Templates

### Basic Module
- **Category**: extensions
- **Features**: Process commands, Help text, Logging
- **Best for**: Simple utility modules

### API Module
- **Category**: extensions
- **Features**: HTTP requests, JSON parsing, Error handling, Rate limiting
- **Best for**: External API integrations

### Game Feature Module
- **Category**: extensions
- **Features**: Game logic, State management, Event handling
- **Best for**: Game-specific functionality

### integration Module
- **Category**: extensions
- **Features**: Service connector, Authentication, Data sync
- **Best for**: Third-party service integrations

### Utility Module
- **Category**: extensions
- **Features**: Helper functions, Data tASHATnsformation, Validation
- **Best for**: Utility and helper tools

## Feature Detection

The spawner intelligently detects features from your prompt:

| Keywords | Feature Added |
|----------|---------------|
| api, rest, endpoint | REST API support |
| game, player, character | Game logic |
| integration, connector, service | External service integration |
| utility, helper, tool | Utility functions |
| database, Storage | Database support |
| cache, caching | Caching layer |
| authentication, auth | Authentication |
| logging, log | Logging support |
| notification, alert | Notification system |
| schedule, cron, timer | Scheduling |
| email, mail | Email support |
| webhook, callback | Webhook handling |

## Generated Module Structure

### File Layout
```
/Modules/<ModuleName>/
├── <ModuleName>.cs           # Main module class
├── README.md                 # Module documentation
├── <ModuleName>Config.json   # Configuration (if applicable)
└── <ModuleName>Tests.cs      # Unit test template
```

### Module Class Structure
```csharp
using System;
using System.Text;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.<ModuleName>;

[RaModule(Category = "extensions")]
public sealed class <ModuleName> : ModuleBase
{
    public override string Name => "<ModuleName>";
    
    private ModuleManager? _manager;
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        LogInfo("<ModuleName> initialized");
    }
    
    public override string Process(string input)
    {
        // Command processing logic
        return "Result";
    }
    
    private string GetHelp()
    {
        return "Help text";
    }
}
```

## Code Review Workflow

### 1. Generate Module
```
spawn module Create a weather module
```

**Output:**
```
✅ Module 'WeatherModule' spawned successfully!

Template: api
Features: rest_api, basic_functionality
Location: /Modules/WeatherModule
Files Generated: 4

Generated files:
  📄 WeatherModule.cs
  📄 README.md
  📄 WeatherModuleConfig.json
  📄 WeatherModuleTests.cs

⚠️  Review required: Use 'spawn review WeatherModule' to inspect the code
✅ To activate: Use 'spawn approve WeatherModule' after review
```

### 2. Review Module
```
spawn review WeatherModule
```

**Output shows:**
- Module status
- Creation timestamp
- Template used
- Original prompt
- File listing
- Code preview (first 30 lines)
- Approval instructions

### 3. Approve Module
```
spawn approve WeatherModule
```

**Output:**
```
✅ Module 'WeatherModule' has been approved!

Location: /Modules/WeatherModule
Version: 1.0.0
Files: 4

⚠️  Module will be loaded on next ASHATCore restart.

Next steps:
1. Restart ASHATCore to load the module
2. Verify module appears in module list
3. Test module functionality
4. Customize code as needed
```

### 4. Restart ASHATCore
```bash
dotnet run --project ASHATCore/ASHATCore.csproj
```

Module will be automatically discovered and loaded by ModuleManager.

## Module Lifecycle

```
Prompt → Parse → Generate → Review → Approve → Restart → Active
                     ↓
                  Rollback (if needed)
```

### States

1. **Awaiting Review** (⏳) - Module Generated, needs review
2. **Approved** (✅) - Module reviewed and approved
3. **Active** (🟢) - Module loaded and running (after restart)

## Security

### SuperAdmin Only

⚠️ **Important**: Only SuperAdmin users can spawn modules. This prevents unauthorized code execution.

### Review Required

All Generated modules must be reviewed and explicitly approved before they can be loaded. This provides:
- Code inspection opportunity
- Security validation
- Quality control
- Rollback option

### Best PASHATctices

1. **Always review Generated code** before approval
2. **Check for security issues** in Generated logic
3. **Validate Configuration files** are properly formatted
4. **Test in safe environment** after loading
5. **Keep backups** of customized modules
6. **Use meaningful names** in prompts for better Generation
7. **Document customizations** in module README

## Module Path Resolution

The spawner automatically determines the correct `/Modules` folder:

```
If running from: /path/to/repo/ASHATCore/bin/Debug/net9.0/
Modules path:   /path/to/repo/Modules/
```

This ensures modules are placed at the repository root level for proper discovery.

## integration with ModuleManager

Generated modules follow the standard ASHATCore module pattern:

1. **Implements** `ModuleBase`
2. **DecoRated** with `[RaModule(Category = "extensions")]`
3. **Overrides** `Name`, `Initialize()`, and `Process()`
4. **Uses** standard logging via `LogInfo()`, `LogError()`

ModuleManager will automatically discover and load approved modules from the `/Modules` folder on startup.

## Customization After Generation

### Editing Generated Code

1. Navigate to `/Modules/<ModuleName>/`
2. Edit `<ModuleName>.cs` with your logic
3. Update `README.md` with documentation
4. Add tests to `<ModuleName>Tests.cs`
5. Restart ASHATCore to reload

### Adding Dependencies

If your module needs external packages:

1. Add package references to `ASHATCore.csproj`
2. Add `using` statements to module file
3. Rebuild project

Example:
```bash
dotnet add ASHATCore/ASHATCore.csproj package RestSharp
```

## Troubleshooting

### "Module already exists"
- Choose a different name in your prompt
- Or use `spawn rollback <module>` to remove existing module

### "Module not found" during review
- Check module name with `spawn list modules`
- Ensure module was successfully Generated
- Verify no errors during Generation

### Module not loading after restart
- Check ModuleManager logs for errors
- Verify module files exist in `/Modules/<ModuleName>/`
- Ensure module was approved
- Check for compilation errors in Generated code

### Generated code has errors
- Use `spawn review <module>` to inspect
- Edit the Generated .cs file manually
- Or use `spawn rollback <module>` and reGenerate

## Version History

The spawner tASHATcks version history for each module:

```
spawn history WeatherModule
```

Shows:
- Version number
- Creation timestamp
- Approval timestamp
- Template used
- Original prompt
- Status timeline

## Future Enhancements

Planned features:
- [ ] Module versioning and updates
- [ ] Dependency management
- [ ] Module marketplace integration
- [ ] AI-powered code optimization
- [ ] Automated testing on approval
- [ ] Hot reload without restart
- [ ] Module categories expansion
- [ ] Custom template support
- [ ] Module cloning from existing
- [ ] CollaboASHATtion features

## Architecture

### Generation Pipeline

```
natural Language Prompt
    ↓
Prompt Parser (type & features)
    ↓
Template Selection
    ↓
Module Class Generator
    ↓
README Generator
    ↓
Config Generator
    ↓
Tests Generator
    ↓
File System Write
    ↓
Module Record Creation
    ↓
Review Queue
    ↓
Approval & Activation
```

### Module Structure

```
ModuleSpawnerModule
├── Process() - Command dispatcher
├── SpawnModuleFromPrompt() - Main Generation logic
├── ParsePrompt() - NLP feature Extraction
├── GenerateModuleFiles() - File Generation orchestASHATtor
│   ├── GenerateModuleClass()
│   ├── GenerateReadme()
│   ├── GenerateConfig()
│   └── GenerateTestsTemplate()
├── Review/Approve/Rollback workflows
└── Template management
```

## Technology Stack

- **Language**: C# 9.0
- **Framework**: .NET 9.0
- **Format**: JSON for Configurations
- **Output**: C# modules with ASHATCore patterns

## Performance

- **Generation Time**: < 1 second per module
- **Output Size**: 5-20 KB per module
- **Memory Usage**: Minimal (templates cached)
- **Scalability**: Unlimited modules

## Examples

### Example 1: Weather Module

**Prompt:**
```
spawn module Create a weather forecast module that fetches weather data
```

**Generated Module** (WeatherForecastModule):
- API integration structure
- Configuration for weather service
- Error handling template
- Help commands

### Example 2: Database Module

**Prompt:**
```
spawn module Build a database utility module with caching
```

**Generated Module** (DatabaseUtilityModule):
- Database connection template
- Caching layer structure
- Data store dictionary
- Configuration support

### Example 3: Game Feature

**Prompt:**
```
spawn module Create a player inventory game module
```

**Generated Module** (PlayerInventoryGameModule):
- Game logic structure
- State management template
- Event handling skeleton
- Game-specific patterns

## Support

For issues or questions:
1. Check this README
2. Use `spawn status` to verify setup
3. Review Generated README.md in modules
4. Check ASHATCore console output for errors
5. Use `spawn review <module>` to inspect code

## License

Part of the ASHATCore project. See main repository for license information.

---

**Phase**: 7  
**Module**: ModuleSpawner  
**Version**: 1.0.0  
**Category**: extensions  
**Author**: ASHATCore Development Team  
**Last Updated**: 2025-01-09
