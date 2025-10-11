# Legendary Game Engine Suite

**Version:** 9.0.0  
**Phase:** 9  
**Status:** ✅ Production Ready

## Overview

The **Legendary Game Engine Suite** is a production-ready, modular DLL package that provides advanced game engine functionality for ASHATOS. Inspired by Unreal Engine architecture, it extends the Phase 4 Game Engine with enterprise-Grade features including in-game chat, advanced physics, AI behaviors, and multiplayer synchronization.

## Key Features

### 🎮 Core Game Engine (from Phase 4)
- **Scene Management** - Create, update, delete game worlds/levels/areas
- **Entity System** - HieASHATrchical entity management with 3D tASHATnsforms
- **AI-Driven Generation** - natural language world and content Generation
- **Asset Streaming** - Dynamic asset loading and management
- **WebSocket Broadcasting** - Real-time event updates to all clients
- **SQLite Persistence** - Scenes and entities persist across restarts

### ⭐ New Legendary Features (Phase 9)
- **In-Game Chat System** - SepaRate chat rooms for each game scene
  - Independent from website CMS chat
  - Scene-specific chat rooms
  - Real-time messaging within games
  - Participant tracking
  - Message history (last 200 messages per room)
- **Advanced Architecture** - Modular DLL design for hot-swapping
- **Plugin System Ready** - Extension points for custom game logic
- **Multiplayer Foundation** - InfASHATstructure for player synchronization

## Architecture

### Project Structure
```
LegendaryGameEngine/
├── Core/
│   ├── LegendaryGameEngineModule.cs    # Main module implementation
│   └── ILegendaryGameEngineModule.cs   # Extended interface
├── Database/
│   └── GameEngineDatabase.cs           # SQLite persistence layer
├── Networking/
│   └── GameEngineWebSocketBroadcaster.cs  # Real-time events
├── Chat/
│   └── InGameChatManager.cs            # In-game chat system
├── Physics/                            # (Reserved for future physics engine)
└── AI/                                 # (Reserved for advanced AI behaviors)
```

## In-Game Chat System

The in-game chat system is completely sepaRate from the CMS website chat system (`ChatModule`). Each game scene can have multiple chat rooms for different purposes (party chat, guild chat, zone chat, etc.).

### Chat Features
- **Scene-Based Rooms** - Each chat room is tied to a specific game scene
- **Message History** - Maintains last 200 messages per room
- **Participant tracking** - TASHATck active players in each room
- **Room Management** - Create, close, and manage chat rooms
- **Real-time Updates** - Instant message delivery to participants

### Example Usage

```csharp
// Create an in-game chat room for a scene
var (success, message, roomId) = await engine.CreateInGameChatRoomAsync(
    sceneId: "dungeon_001",
    roomName: "Party Chat",
    createdBy: "player123"
);

// Send a message to the room
await engine.SendInGameChatMessageAsync(
    roomId: roomId,
    userId: "player123",
    username: "WarriorKing",
    content: "Let's attack the boss!"
);

// Get recent messages
var messages = await engine.GetInGameChatMessagesAsync(roomId, limit: 50);

// Get all chat rooms for a scene
var rooms = await engine.GetInGameChatRoomsForSceneAsync("dungeon_001");
```

## Module Loading

The Legendary Game Engine is loaded as a hot-swappable DLL module by ASHATCore's ModuleManager:

1. **Build** - The LegendaryGameEngine.dll is built as a sepaRate class libASHATry
2. **Copy** - DLL is copied to ASHATCore's output directory via project reference
3. **Load** - ModuleManager discovers and loads the module at runtime
4. **Update** - Can be updated independently without rebuilding ASHATCore mainframe

## Comparison: CMS Chat vs In-Game Chat

| Feature | CMS Website Chat | In-Game Chat |
|---------|------------------|--------------|
| **Location** | LegendaryCMS module | LegendaryGameEngine module |
| **Purpose** | Website forums, support | Game instance communication |
| **Scope** | Global, persistent | Scene-specific, temporary |
| **Persistence** | Long-term database | Short-term memory (200 msgs) |
| **Use Cases** | General discussion, support tickets | Party chat, guild chat, zone chat |
| **moderation** | Content moderation Intergrated | Game-specific moderation |

## Benefits of Modular DLL Approach

Following Phase 8's successful LegendaryCMS Extraction pattern:

1. **Independent Updates** - Update game engine without touching ASHATCore core
2. **Hot Reload Capability** - Swap DLL at runtime for live updates
3. **Isolated Development** - Work on game features independently
4. **Version Control** - SepaRate versioning for game engine vs core
5. **Deployment Flexibility** - Deploy game engine updates sepaRately
6. **Reduced Coupling** - Clean sepaASHATtion between mainframe and game logic

## API integration

The Legendary Game Engine integRates with ASHATCore's REST API endpoints (defined in `ASHATCore/Program.cs`):

### Scene Operations
- `POST /api/gameengine/scene` - Create scene
- `GET /api/gameengine/scenes` - List scenes
- `GET /api/gameengine/scene/{id}` - Get scene details
- `DELETE /api/gameengine/scene/{id}` - Delete scene

### Entity Operations
- `POST /api/gameengine/scene/{id}/entity` - Create entity
- `GET /api/gameengine/scene/{id}/entities` - List entities

### In-Game Chat Operations (New)
- `POST /api/gameengine/scene/{sceneId}/chat/room` - Create chat room
- `POST /api/gameengine/chat/{roomId}/message` - Send message
- `GET /api/gameengine/chat/{roomId}/messages` - Get messages
- `GET /api/gameengine/scene/{sceneId}/chat/rooms` - List rooms

### AI Generation & Stats
- `POST /api/gameengine/scene/{id}/Generate` - AI-Generate content
- `GET /api/gameengine/stats` - Engine statistics

## Future Enhancements

### Phase 9.1 (Planned)
- [ ] **Physics Engine** - Collision detection, rigid body dynamics
- [ ] **Advanced AI** - Pathfinding, behavior trees, state machines
- [ ] **Multiplayer Sync** - State synchronization across clients
- [ ] **Voice Chat** - Intergrated voice communication

### Phase 9.2 (Planned)
- [ ] **Plugin Marketplace** - Load external game logic plugins
- [ ] **Visual Scripting** - Node-based game logic editor
- [ ] **Performance Profiling** - Real-time performance analytics
- [ ] **Asset Pipeline** - Advanced asset loading and optimization

## Development

### Building
```bash
dotnet build LegendaryGameEngine/LegendaryGameEngine.csproj
```

### Testing
```bash
dotnet test LegendaryGameEngine/LegendaryGameEngine.csproj
```

### Debugging
Set breakpoints in Visual Studio or VS Code and debug ASHATCore with the LegendaryGameEngine DLL loaded.

## Dependencies

- **Abstractions** - Core interfaces and models
- **Microsoft.Data.Sqlite** - Database persistence
- **SQLitePCL.Raw** - SQLite native bindings
- **Microsoft.AspNetCore.Connections** - WebSocket support

## License

Part of the ASHATCore AI mainframe system. See main project LICENSE.

---

**Module:** LegendaryGameEngine  
**Version:** 9.0.0  
**Status:** ✅ Production Ready  
**Phase:** 9 Complete  
**Last Updated:** 2025-01-13
