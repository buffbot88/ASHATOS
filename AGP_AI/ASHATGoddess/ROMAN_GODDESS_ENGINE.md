# RomanGoddessEngine

A lightweight C# game engine specifically designed for the ASHAT Goddess companion system.

## Features

- **Custom Renderer**: Integrates with LegendaryGameEngineModule for scene and entity rendering
- **AI Backend Connection**: Entity property system supports AI state management
- **Entity System**: Full CRUD operations for game entities
- **Scene System**: Automatic scene management with cleanup
- **Update/Render Cycle**: Properly executing 60 FPS game loops
- **Event System**: Subscribe to engine events (OnUpdate, OnRender, OnStateChanged)
- **Performance Monitoring**: Real-time FPS tracking and engine statistics

## Quick Start

```csharp
using ASHATGoddessClient;

// Create and initialize the engine
var engine = new RomanGoddessEngine();
await engine.InitializeAsync("My Scene");

// Subscribe to events
engine.OnUpdate += (sender, e) => {
    // Update game logic (called ~60 times per second)
    Console.WriteLine($"Update: deltaTime={e.DeltaTime}");
};

engine.OnRender += (sender, e) => {
    // Render logic (called ~60 times per second)
    Console.WriteLine($"Render: FPS={e.FPS:F2}");
};

// Create entities
var entityId = await engine.CreateEntityAsync(
    name: "Goddess Avatar",
    type: "Character",
    position: new Vector3 { X = 0, Y = 0, Z = 0 },
    properties: new Dictionary<string, object> {
        ["health"] = 100,
        ["animation_state"] = "idle"
    }
);

// Start the engine loops
engine.Start();

// ... do work ...

// Stop and cleanup
engine.Stop();
engine.Dispose();
```

## Entity Management

### Create Entity
```csharp
var entityId = await engine.CreateEntityAsync(
    "Entity Name", 
    "Entity Type",
    new Vector3 { X = 100, Y = 200, Z = 0 },
    new Dictionary<string, object> { ["key"] = "value" }
);
```

### Update Entity
```csharp
await engine.UpdateEntityAsync(entityId, new Dictionary<string, object> {
    ["health"] = 50,
    ["status"] = "damaged"
});
```

### Get Entity
```csharp
var entity = await engine.GetEntityAsync(entityId);
Console.WriteLine($"Entity: {entity.Name} at ({entity.Position.X}, {entity.Position.Y})");
```

### List All Entities
```csharp
var entities = await engine.ListEntitiesAsync();
foreach (var entity in entities) {
    Console.WriteLine($"- {entity.Name} ({entity.Type})");
}
```

### Delete Entity
```csharp
await engine.DeleteEntityAsync(entityId);
```

## Engine Statistics

```csharp
// Get detailed engine information
var info = await engine.GetEngineInfoAsync();
Console.WriteLine(info); // Prints: RomanGoddessEngine: Running=True, FPS=60.00/60, Entities=5, ...

// Get underlying game engine stats
var stats = await engine.GetStatsAsync();
Console.WriteLine($"Total Scenes: {stats.TotalScenes}");
Console.WriteLine($"Total Entities: {stats.TotalEntities}");
```

## Events

### OnUpdate
Fired every update cycle (~60 times per second). Use for game logic updates.

```csharp
engine.OnUpdate += (sender, e) => {
    // e.DeltaTime contains time since last update (typically ~0.016s for 60 FPS)
};
```

### OnRender
Fired every render cycle (~60 times per second). Use for rendering logic.

```csharp
engine.OnRender += (sender, e) => {
    // e.FPS contains current frames per second
    // e.DeltaTime contains time since last render
};
```

### OnStateChanged
Fired when engine state changes (Initialized, Running, Stopped).

```csharp
engine.OnStateChanged += (sender, e) => {
    Console.WriteLine($"Engine state changed to: {e.State}");
};
```

## Engine Lifecycle

1. **Create**: `var engine = new RomanGoddessEngine()`
2. **Initialize**: `await engine.InitializeAsync("Scene Name")`
3. **Start**: `engine.Start()` - Begins update and render loops
4. **Stop**: `engine.Stop()` - Stops loops (can be restarted)
5. **Dispose**: `engine.Dispose()` - Full cleanup and scene removal

## Integration with ASHAT

The RomanGoddessEngine is automatically used by the GameServerVisualProcessor to manage ASHAT's visual state. The engine handles:

- Scene creation for goddess visualization
- Entity management for goddess character state
- Animation state tracking
- Visual property updates (glow, sparkles, etc.)
- Render loop execution for smooth animations

## Performance

- **Target FPS**: 60
- **Update Interval**: 16ms (~60 updates/second)
- **Render Interval**: 16ms (~60 renders/second)
- **FPS Tracking**: Real-time calculation updated every second

## Architecture

```
RomanGoddessEngine
  ├── LegendaryGameEngineModule (underlying game system)
  ├── Update Timer (game logic loop)
  ├── Render Timer (rendering loop)
  ├── Scene Management
  └── Entity Management
```

## Issues Fixed

This implementation addresses the following issues from the original bug report:

1. ✅ The field '_idleBehaviorTimer' is never used - Now properly used in StartIdleBehavior()
2. ✅ The name 'StartIdleBehavior' does not exist - Method implemented
3. ✅ The name 'CenterOnScreen' does not exist - Method implemented  
4. ✅ RomanGoddessEngine implementation - Complete with all requested features
5. ✅ Rendering loop and update cycle execution - Fixed with proper Timer-based loops

## Thread Safety

The engine uses System.Threading.Timer for update and render loops. Event handlers should be thread-safe as they may be called from timer threads. For UI updates, marshal to the UI thread using appropriate mechanisms (e.g., Avalonia.Threading.Dispatcher.UIThread).

## Disposal

Always dispose of the engine when done to ensure proper cleanup:

```csharp
using (var engine = new RomanGoddessEngine()) {
    // Use engine
}
// Automatically disposed
```

Or explicitly:

```csharp
engine.Dispose();
```
