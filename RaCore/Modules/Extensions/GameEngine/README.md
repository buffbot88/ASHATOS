# Game Engine Module

## Overview

The Game Engine module provides a robust, AI-controllable game engine integration for RaCore, enabling dynamic world generation, scene management, and real-time game operations.

## Quick Start

### Console Commands

```bash
# Create a scene
engine scene create Medieval Town

# List all scenes
engine scene list

# Get engine statistics
engine stats

# Create an entity
engine entity create <sceneId> Blacksmith NPC

# AI-generate world content
engine generate <sceneId> Generate a medieval town with 10 NPCs
```

### API Usage

See [GAMEENGINE_API.md](../../../GAMEENGINE_API.md) for complete API documentation.

**Example: Create a scene via API**
```bash
curl -X POST http://localhost:7077/api/gameengine/scene \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"name": "Fantasy World"}'
```

**Example: AI-generate content**
```bash
curl -X POST http://localhost:7077/api/gameengine/scene/<sceneId>/generate \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate a medieval town with NPCs",
    "theme": "medieval",
    "entityCount": 10,
    "generateNPCs": true,
    "generateTerrain": true
  }'
```

## Features

✅ **Scene Management** - Create, list, and delete game scenes  
✅ **Entity System** - Manage game objects with 3D transforms  
✅ **AI-Driven Generation** - Generate worlds from natural language  
✅ **Permission-based Access** - RBAC integration for security  
✅ **REST API** - Full HTTP API for remote control  
✅ **Real-time Stats** - Monitor engine health and performance  
✅ **SQLite Persistence** - All data persists across server restarts (Phase 4.2)  

## Architecture

### Data Flow
```
User/AI → API/Console → GameEngineModule → Scene/Entity Management → Response
                                         ↓
                                    AI Generation
                                         ↓
                                  AICodeGen Integration
```

### Components

- **GameEngineModule**: Main module implementation
- **IGameEngineModule**: Interface for external access
- **GameScene**: Scene data model
- **GameEntity**: Entity data model with 3D transforms
- **WorldGenerationRequest**: AI generation parameters

## Integration

### With Authentication Module
```csharp
// Automatically integrated via Program.cs
// All API endpoints require valid authentication tokens
// Admin role required for create/delete operations
```

### With AICodeGen Module
```csharp
// Game projects can include engine scenes
codegen generate Create an MMORPG with medieval city
```

### With Speech Module
```csharp
// Control engine via natural language
"Create a new fantasy scene with dragons and knights"
```

## Extension Points

### Custom Entity Types
```csharp
var entity = new GameEntity
{
    Name = "Dragon",
    Type = "Enemy",
    Properties = {
        ["health"] = 1000,
        ["damage"] = 50,
        ["element"] = "fire"
    }
};
```

### Custom Scene Metadata
```csharp
var scene = new GameScene
{
    Name = "Dungeon Level 1",
    Metadata = {
        ["difficulty"] = "hard",
        ["theme"] = "dark_fantasy",
        ["maxPlayers"] = 4
    }
};
```

## Performance

- **In-memory storage** for fast access
- **SQLite persistence** for data durability
- **Thread-safe operations** via ConcurrentDictionary
- **Async/await** for non-blocking I/O
- **Efficient lookups** via dictionary indexing

## Persistence

All scenes and entities are automatically saved to SQLite database:
- **Database:** `Databases/game_engine.sqlite`
- **Auto-load:** Scenes restored on server startup
- **Zero config:** Works automatically
- **See:** [GAMEENGINE_PERSISTENCE.md](../../../GAMEENGINE_PERSISTENCE.md) for details

## Future Enhancements

- [ ] WebSocket event broadcasting
- [ ] Physics simulation integration
- [ ] Multi-client synchronization
- [ ] Asset streaming from external sources
- [ ] Player dashboards (web UI)
- [ ] Quest system integration
- [ ] Inventory management
- [x] **Persistence layer (SQLite database)** - ✅ Completed in Phase 4.2

## Documentation

- **API Reference**: See [GAMEENGINE_API.md](../../../GAMEENGINE_API.md)
- **Authentication**: See [AUTHENTICATION_QUICKSTART.md](../../../AUTHENTICATION_QUICKSTART.md)
- **Phase Roadmap**: See [PHASES.md](../../../PHASES.md)

## Testing

```bash
# Via console
engine status
engine scene create Test Scene
engine scene list
engine stats

# Via API (requires authentication)
# See GAMEENGINE_API.md for curl examples
```

## Troubleshooting

**Q: "Authentication not available" error**  
A: Ensure the Authentication module is loaded. Check module list.

**Q: "Insufficient permissions" error**  
A: Create/delete operations require Admin role. Standard users can only view.

**Q: Scene not found**  
A: Verify scene ID with `engine scene list` command.

## Support

For issues, feature requests, or questions:
- Check [GAMEENGINE_API.md](../../../GAMEENGINE_API.md)
- Review module logs: `[Module:GameEngine]`
- Check engine status: `engine status`

---

**Module**: GameEngine  
**Category**: extensions  
**Phase**: 4  
**Status**: ✅ Production Ready
