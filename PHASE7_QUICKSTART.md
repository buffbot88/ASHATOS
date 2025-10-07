# Phase 7 Quickstart - Module Spawner

## Overview

The Module Spawner enables RaAI to self-build and spawn new RaCore modules using natural language commands. This guide shows you how to use it.

## Quick Start (5 minutes)

### 1. Start RaCore

```bash
cd RaCore
dotnet run
```

Wait for the boot sequence to complete. You should see:
```
[Module:ModuleSpawner] INFO: Module Spawner initialized
[Module:ModuleSpawner] INFO: Modules will be spawned to: /path/to/Modules
```

### 2. Access the Console

Connect to RaCore via WebSocket or HTTP console at:
- **WebSocket**: `ws://localhost/ws`
- **Web Console**: `http://localhost/console.html` (if available)

### 3. Check Spawner Status

```
spawn status
```

Expected output:
```
Module Spawner Status:

Modules Path: /path/to/TheRaProject/Modules
Available Templates: 5
Spawned Modules: 0
Modules Awaiting Review: 0
Approved Modules: 0
Active Modules: 0

✅ Authentication module: Connected
```

### 4. View Available Templates

```
spawn list templates
```

Expected output:
```
Available Module Templates:

📦 api
   Module for REST API interactions and data fetching
   Category: extensions
   Features: HTTP requests, JSON parsing, Error handling, Rate limiting

📦 basic
   A basic RaCore module with standard functionality
   Category: extensions
   Features: Process commands, Help text, Logging

📦 game_feature
   Module for game-specific features and mechanics
   Category: extensions
   Features: Game logic, State management, Event handling

📦 integration
   Module for integrating with external services
   Category: extensions
   Features: Service connector, Authentication, Data sync

📦 utility
   Utility module with helper functions and tools
   Category: extensions
   Features: Helper functions, Data transformation, Validation
```

### 5. Spawn Your First Module

Try this example:

```
spawn module Create a simple test module
```

Expected output:
```
✅ Module 'SimpleTestModule' spawned successfully!

Template: basic
Features: basic_functionality
Location: /path/to/Modules/SimpleTestModule
Files generated: 3

Generated files:
  📄 SimpleTestModule.cs
  📄 README.md
  📄 SimpleTestModuleTests.cs

⚠️  Review required: Use 'spawn review SimpleTestModule' to inspect the code
✅ To activate: Use 'spawn approve SimpleTestModule' after review

Note: Module will be loaded on next RaCore restart after approval.
```

### 6. Review the Module

```
spawn review SimpleTestModule
```

This shows:
- Module status and metadata
- File listing with sizes
- Code preview (first 30 lines)
- Approval/rollback commands

### 7. Approve the Module

```
spawn approve SimpleTestModule
```

Expected output:
```
✅ Module 'SimpleTestModule' has been approved!

Location: /path/to/Modules/SimpleTestModule
Version: 1.0.0
Files: 3

⚠️  Module will be loaded on next RaCore restart.

Next steps:
1. Restart RaCore to load the module
2. Verify module appears in module list
3. Test module functionality
4. Customize code as needed
```

### 8. Restart RaCore

Stop RaCore (Ctrl+C) and restart:

```bash
dotnet run
```

The new module should appear in the module list during boot.

### 9. Verify Module Loaded

Check that your module is active by looking for it in the boot logs or by listing modules.

## Common Examples

### Weather Module

```
spawn module Create a weather forecast module that fetches weather data
```

Generates:
- **WeatherForecastModule** (API template)
- With REST API structure
- Configuration support
- Error handling

### Database Module

```
spawn module Build a database utility module with caching
```

Generates:
- **DatabaseUtilityModule** (Utility template)
- With database support
- Caching layer
- Storage dictionary

### Task Manager

```
spawn module Generate a task management module with TODO lists
```

Generates:
- **TaskManagementModule** (Basic template)
- With basic CRUD structure
- Help commands
- Test template

### Email Notifier

```
spawn module Create an email notification module
```

Generates:
- **EmailNotificationModule** (API template)
- With email sending structure
- SMTP configuration support

### Game Inventory

```
spawn module Create a player inventory game module
```

Generates:
- **PlayerInventoryGameModule** (Game Feature template)
- With game logic structure
- State management
- Event handling

## Available Commands

| Command | Description |
|---------|-------------|
| `spawn module <prompt>` | Generate new module |
| `spawn list templates` | Show templates |
| `spawn list modules` | Show all modules |
| `spawn review <name>` | Review module |
| `spawn approve <name>` | Approve module |
| `spawn rollback <name>` | Remove module |
| `spawn history <name>` | View history |
| `spawn status` | Show status |
| `help` | Show help |

## Module Lifecycle

```
1. SPAWN     → spawn module <prompt>
2. REVIEW    → spawn review <name>
3. APPROVE   → spawn approve <name>
4. RESTART   → dotnet run (module loads)
5. ACTIVE    → module is now running
```

Or rollback at any time:
```
ROLLBACK → spawn rollback <name>
```

## Generated Module Structure

```
/Modules/<ModuleName>/
├── <ModuleName>.cs           # Main module class
├── README.md                 # Documentation
├── <ModuleName>Config.json   # Config (if applicable)
└── <ModuleName>Tests.cs      # Unit tests
```

## Customizing Generated Modules

After generation, you can edit the files:

### 1. Navigate to Module
```bash
cd /path/to/Modules/SimpleTestModule
```

### 2. Edit Module Code
```bash
nano SimpleTestModule.cs
```

### 3. Add Your Logic
```csharp
public override string Process(string input)
{
    var text = (input ?? string.Empty).Trim();
    
    if (string.IsNullOrEmpty(text) || text.Equals("help", ...))
    {
        return GetHelp();
    }
    
    // Add your custom commands here
    if (text.StartsWith("mycommand", ...))
    {
        return "Custom command result";
    }
    
    return $"Processed: {text}";
}
```

### 4. Update README
Document your changes in `README.md`

### 5. Write Tests
Add tests in `<ModuleName>Tests.cs`

### 6. Restart to Reload
```bash
dotnet run
```

## Feature Detection Keywords

The spawner detects features from your prompt:

| Keyword | Feature Added |
|---------|---------------|
| api, rest, endpoint | REST API support |
| game, player | Game logic |
| integration, service | External service |
| utility, helper | Utility functions |
| database, storage | Database support |
| cache, caching | Caching layer |
| authentication, auth | Authentication |
| logging, log | Logging support |
| notification, alert | Notifications |
| schedule, cron | Scheduling |
| email, mail | Email support |
| webhook, callback | Webhooks |

## Tips for Good Prompts

### ✅ Good Prompts
- "Create a weather forecast module that fetches weather data"
- "Build a cryptocurrency price tracker with caching"
- "Generate a task management module with TODO lists and database storage"

### ❌ Poor Prompts
- "module" (too vague)
- "make something" (no context)
- "a" (insufficient information)

### Key Elements of Good Prompts
1. **Action verb**: Create, Build, Generate
2. **Descriptive name**: weather forecast, cryptocurrency tracker
3. **Type hint**: module
4. **Features**: that fetches, with caching, with database

## Troubleshooting

### Module Not Found
```
Error: Module 'TestModule' not found.
```
**Solution**: Check name with `spawn list modules`

### Module Already Exists
```
Error: Module 'TestModule' already exists.
```
**Solution**: Choose different name or `spawn rollback TestModule`

### Module Not Loading After Restart
**Check**:
1. Was module approved? (`spawn list modules`)
2. Are files present? (`ls Modules/TestModule/`)
3. Any compilation errors? (check boot logs)
4. Correct namespace? (should be `RaCore.Modules.<Name>`)

### Generated Code Has Errors
**Solutions**:
1. Review code: `spawn review <name>`
2. Edit files manually in `/Modules/<name>/`
3. Or rollback and regenerate: `spawn rollback <name>`

## Advanced Usage

### Multiple Features
```
spawn module Create an API module with database caching and logging
```

This generates a module with:
- API template
- Database support
- Caching layer
- Logging enabled

### Custom Names
The spawner extracts names intelligently:
```
"Create weather forecast module" → WeatherForecastModule
"Build crypto price tracker" → CryptoPriceTrackerModule
```

### Rollback After Customization
⚠️ **Warning**: Rollback deletes all files, including customizations!

To preserve work:
1. Back up your code
2. Or commit to version control
3. Then rollback if needed

## Next Steps

After spawning your first module:

1. **Explore Templates** - Try each template type
2. **Customize Code** - Edit generated modules
3. **Add Dependencies** - Install NuGet packages if needed
4. **Write Tests** - Complete the test templates
5. **Document** - Update README with your changes
6. **Share** - Submit useful modules back to project

## Help

For more information:
- **Module Spawner README**: `/RaCore/Modules/Extensions/ModuleSpawner/README.md`
- **Phase 7 Summary**: `PHASE7_SUMMARY.md`
- **Command Help**: `spawn help` or `help`

## Example Session

Complete example session:

```bash
# Start RaCore
$ dotnet run

# In console:
> spawn status
> spawn list templates
> spawn module Create a simple calculator utility module
> spawn list modules
> spawn review CalculatorUtilityModule
> spawn approve CalculatorUtilityModule

# Restart
^C
$ dotnet run

# Module now loaded and active!
```

---

**Phase**: 7  
**Guide**: Module Spawner Quickstart  
**Version**: 1.0.0  
**Last Updated**: 2025-01-09
