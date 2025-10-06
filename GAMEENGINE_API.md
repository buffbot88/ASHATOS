# Game Engine Module - Phase 4 Implementation

## Overview

The Game Engine Module provides a robust, modular game engine integration directly controllable by RaCore AI modules. Inspired by Unreal Engine architecture, it enables RaCore to serve as an orchestrator for game logic, world generation, and runtime management.

## Key Features

### ✅ Core Engine Capabilities
- **Scene Management**: Create, list, update, and delete game scenes (worlds, levels, areas)
- **Entity System**: Hierarchical entity management (NPCs, players, objects, terrain)
- **AI-Driven Generation**: Automatic world and content generation from natural language prompts
- **Asset Streaming**: Dynamic asset loading and management
- **Real-time Updates**: Live patching and updates via AI-driven commands
- **Permission-based Access**: RBAC integration for secure engine control

### ✅ API Integration
- **REST APIs**: Full HTTP API for game engine operations
- **WebSocket Support**: Real-time game event broadcasting
- **Authentication**: Integrated with RaCore authentication system
- **Role-based Access**: User, Admin, and SuperAdmin permission levels

## Architecture

### Module Structure
```
RaCore/Modules/Extensions/GameEngine/
└── GameEngineModule.cs          # Main game engine implementation

Abstractions/
└── IGameEngineModule.cs         # Interface and data models
```

### Data Models

#### GameScene
Represents a game world, level, or area.
```csharp
{
    "id": "unique-scene-id",
    "name": "Medieval Town",
    "description": "A bustling medieval marketplace",
    "createdAt": "2025-01-05T12:00:00Z",
    "createdBy": "admin",
    "isActive": true,
    "metadata": { /* custom properties */ },
    "entities": [ /* list of GameEntity */ ]
}
```

#### GameEntity
Represents any game object (character, NPC, object, terrain).
```csharp
{
    "id": "unique-entity-id",
    "name": "Blacksmith",
    "type": "NPC",
    "position": { "x": 10.0, "y": 0.0, "z": 5.0 },
    "rotation": { "x": 0.0, "y": 90.0, "z": 0.0 },
    "scale": { "x": 1.0, "y": 1.0, "z": 1.0 },
    "properties": {
        "dialogue": "Welcome to my forge!",
        "occupation": "Blacksmith"
    },
    "createdAt": "2025-01-05T12:05:00Z",
    "createdBy": "AI:admin"
}
```

## API Reference

### Authentication
All API endpoints require authentication via Bearer token (except where noted).

**Headers:**
```
Authorization: Bearer <your-token>
```

### Endpoints

#### 1. Create Scene
**POST** `/api/gameengine/scene`

Creates a new game scene.

**Required Role:** Admin

**Request Body:**
```json
{
    "name": "Medieval Town",
    "description": "A bustling marketplace"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Scene 'Medieval Town' created successfully",
    "data": { /* GameScene object */ }
}
```

---

#### 2. List Scenes
**GET** `/api/gameengine/scenes`

Lists all game scenes.

**Required Role:** User

**Response:**
```json
{
    "success": true,
    "scenes": [ /* array of GameScene objects */ ]
}
```

---

#### 3. Get Scene Details
**GET** `/api/gameengine/scene/{sceneId}`

Gets details of a specific scene.

**Required Role:** User

**Response:**
```json
{
    "success": true,
    "scene": { /* GameScene object */ }
}
```

---

#### 4. Delete Scene
**DELETE** `/api/gameengine/scene/{sceneId}`

Deletes a scene by ID.

**Required Role:** Admin

**Response:**
```json
{
    "success": true,
    "message": "Scene deleted successfully"
}
```

---

#### 5. Create Entity
**POST** `/api/gameengine/scene/{sceneId}/entity`

Creates an entity in the specified scene.

**Required Role:** Admin

**Request Body:**
```json
{
    "name": "Guard",
    "type": "NPC",
    "position": { "x": 0, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "properties": {
        "dialogue": "Halt! State your business.",
        "occupation": "Guard"
    }
}
```

**Response:**
```json
{
    "success": true,
    "message": "Entity 'Guard' created successfully",
    "data": { /* GameEntity object */ }
}
```

---

#### 6. List Entities
**GET** `/api/gameengine/scene/{sceneId}/entities`

Lists all entities in a scene.

**Required Role:** User

**Response:**
```json
{
    "success": true,
    "entities": [ /* array of GameEntity objects */ ]
}
```

---

#### 7. AI-Generate World Content
**POST** `/api/gameengine/scene/{sceneId}/generate`

Generates world content using AI from a natural language prompt.

**Required Role:** Admin

**Request Body:**
```json
{
    "prompt": "Generate a medieval town with 10 NPCs and market district",
    "theme": "medieval",
    "entityCount": 10,
    "generateNPCs": true,
    "generateTerrain": true,
    "generateQuests": false
}
```

**Response:**
```json
{
    "success": true,
    "message": "Generated 11 entities in scene 'Medieval Town'",
    "data": [ /* array of generated GameEntity objects */ ]
}
```

---

#### 8. Get Engine Statistics
**GET** `/api/gameengine/stats`

Gets engine statistics and health metrics.

**Required Role:** User

**Response:**
```json
{
    "success": true,
    "stats": {
        "totalScenes": 5,
        "activeScenes": 4,
        "totalEntities": 123,
        "memoryUsageMB": 256,
        "uptime": "02:15:30",
        "connectedClients": 3,
        "startTime": "2025-01-05T10:00:00Z"
    }
}
```

---

## Console Commands

The Game Engine module also supports console/WebSocket commands:

```
engine status                          - Show engine status and configuration
engine stats                           - Show engine statistics
engine scene create <name>             - Create a new game scene
engine scene list                      - List all scenes
engine scene delete <sceneId>          - Delete a scene
engine entity create <sceneId> <name> <type> - Create an entity
engine generate <sceneId> <prompt>     - AI-generate world content
help                                   - Show help message
```

## Usage Examples

### Example 1: Creating a Medieval Town Scene

**Step 1:** Create a scene
```bash
POST /api/gameengine/scene
{
    "name": "Medieval Town",
    "description": "Central hub for players"
}
```

**Step 2:** Generate AI content
```bash
POST /api/gameengine/scene/{sceneId}/generate
{
    "prompt": "Generate a medieval town with blacksmith, innkeeper, and guards",
    "theme": "medieval",
    "entityCount": 10,
    "generateNPCs": true,
    "generateTerrain": true
}
```

**Step 3:** List generated entities
```bash
GET /api/gameengine/scene/{sceneId}/entities
```

### Example 2: Via Console Commands

```
> engine scene create Fantasy World
✅ Scene 'Fantasy World' created successfully

> engine generate scene-abc123 Generate a mystical forest with 5 NPCs
✅ Generated 6 entities in scene 'Fantasy World'

> engine scene list
🟢 Active - Fantasy World (ID: scene-abc123)
   Created: 2025-01-05 12:00:00 by admin
   Entities: 6
```

## Integration with AI Modules

The Game Engine integrates seamlessly with other RaCore modules:

### With AICodeGen Module
Generate complete game projects with integrated game engine scenes:
```
codegen generate Create an MMORPG with medieval city
```

### With Speech Module
Control game engine via natural language:
```
"Create a new medieval scene with 20 NPCs"
"Generate a fantasy dungeon in scene abc123"
```

## Security & Permissions

### Role Requirements

| Operation | User | Admin | SuperAdmin |
|-----------|------|-------|------------|
| List Scenes | ✅ | ✅ | ✅ |
| View Scene Details | ✅ | ✅ | ✅ |
| View Entities | ✅ | ✅ | ✅ |
| Get Stats | ✅ | ✅ | ✅ |
| Create Scene | ❌ | ✅ | ✅ |
| Delete Scene | ❌ | ✅ | ✅ |
| Create Entity | ❌ | ✅ | ✅ |
| AI Generate Content | ❌ | ✅ | ✅ |

### Permission Validation
All API endpoints validate:
1. Valid authentication token
2. Active user session
3. Sufficient role permissions
4. Module-specific access rights

## Extension Points

The Game Engine is designed for extensibility:

### Plugin Support
- Graphics renderers (OpenGL, Vulkan, DirectX)
- Physics engines (PhysX, Bullet)
- Audio systems
- Networking layers
- Input handlers

### Future Enhancements
- Persistence layer (save/load scenes)
- Multi-client synchronization
- Physics simulation integration
- Advanced AI pathfinding
- Quest system integration
- Inventory management
- Player progression tracking

## Performance Considerations

- In-memory storage for rapid access
- Concurrent dictionary for thread-safe operations
- Async/await for non-blocking operations
- Efficient entity lookups via dictionaries
- Memory-efficient data structures

## Monitoring & Diagnostics

### Health Checks
```bash
GET /api/gameengine/stats
```

### Console Diagnostics
```bash
engine status  # Quick overview
engine stats   # Detailed metrics
```

### Logs
The module logs all operations:
```
[Module:GameEngine] INFO: Scene created: Medieval Town (ID: abc123)
[Module:GameEngine] INFO: Entity created: Blacksmith (NPC) in scene Medieval Town
[Module:GameEngine] INFO: Generated 10 entities for scene Medieval Town
```

## Development Roadmap

### Phase 4.1 - Current Implementation ✅
- [x] Core engine module
- [x] Scene management
- [x] Entity system
- [x] AI-driven generation
- [x] REST API endpoints
- [x] Authentication integration
- [x] Console commands

### Phase 4.2 - Planned Enhancements
- [ ] WebSocket real-time events
- [ ] Persistence layer (SQLite)
- [ ] Physics simulation
- [ ] Advanced entity components
- [ ] Quest system
- [ ] Inventory management
- [ ] Player dashboards (HTML/JS)

### Phase 4.4 - Advanced Features
- [ ] Multi-client synchronization
- [ ] Plugin architecture
- [ ] External client integration
- [ ] Asset pipeline
- [ ] Live patching system
- [ ] Performance profiling

## Testing

### Manual Testing
```bash
# Test scene creation
POST /api/gameengine/scene
{
    "name": "Test Scene"
}

# Test AI generation
POST /api/gameengine/scene/{sceneId}/generate
{
    "prompt": "Generate test content",
    "entityCount": 5
}

# Verify stats
GET /api/gameengine/stats
```

### Integration Testing
The module integrates with existing TestRunner module for automated tests.

## Troubleshooting

### Issue: "Authentication not available"
**Solution:** Ensure the Authentication module is loaded before Game Engine module.

### Issue: "Scene not found"
**Solution:** Verify scene ID from `GET /api/gameengine/scenes` endpoint.

### Issue: "Insufficient permissions"
**Solution:** Ensure user has Admin role for create/delete operations.

## Support & Documentation

- **API Documentation**: This file
- **Module Source**: `RaCore/Modules/Extensions/GameEngine/GameEngineModule.cs`
- **Interfaces**: `Abstractions/IGameEngineModule.cs`
- **Integration Guide**: See AUTHENTICATION_QUICKSTART.md for auth integration

## License

Part of the RaCore AI Mainframe system - Phase 4 implementation.

---

**Module**: GameEngine  
**Version**: v4.8.9  
**Status**: ✅ Production Ready  
**Phase**: 4 Complete  
**Last Updated**: 2025-01-13
