# Phase 9.1 & 9.2 Features Documentation

## Overview

This document describes the implementation of Phase 9.1 and Phase 9.2 features for the ASHAT Goddess platform. These features extend the platform with advanced game engine capabilities, AI systems, networking, and plugin support.

## Phase 9.1 Features

### 1. Physics Engine

**Location**: `/PhysicsEngine/PhysicsEngine.cs`

A complete 2D physics engine with collision detection and rigid body dynamics.

#### Features:
- **Rigid Body Dynamics**: Mass, velocity, acceleration, drag, and restitution
- **Collision Detection**: 
  - Box-Box (AABB)
  - Circle-Circle
  - Box-Circle hybrid
- **Physics Simulation**: Gravity, force application, impulse-based collision resolution
- **Raycasting**: Cast rays through the physics world
- **Collider Types**:
  - `BoxCollider`: Axis-aligned bounding box collisions
  - `CircleCollider`: Circular collisions

#### Usage Example:
```csharp
using ASHATGoddessClient.PhysicsEngine;

var physics = new PhysicsEngine(gravity: 9.81f);

// Create a rigid body
var body = new RigidBody
{
    Position = new Vector2(0, 0),
    Mass = 1.0f,
    Restitution = 0.5f
};

// Add a collider
var collider = new BoxCollider
{
    Position = new Vector2(0, 0),
    Size = new Vector2(10, 10),
    RigidBody = body
};

physics.AddRigidBody(body);
physics.AddCollider(collider);

// Update physics
physics.Update(deltaTime);

// Raycast
if (physics.Raycast(origin, direction, maxDistance, out var hit))
{
    Console.WriteLine($"Hit at {hit.Point}");
}
```

### 2. Advanced AI

**Location**: `/AI/`

Three comprehensive AI systems for intelligent behavior:

#### A. Pathfinding System (`PathfindingSystem.cs`)

A* pathfinding algorithm for navigation on a grid.

**Features**:
- Grid-based navigation
- A* algorithm with heuristics
- Walkable/blocked cell configuration
- Path reconstruction

**Usage**:
```csharp
var pathfinding = new PathfindingSystem(width: 100, height: 100, cellSize: 1.0f);

// Set obstacles
pathfinding.SetWalkable(50, 50, false);

// Find path
var path = pathfinding.FindPath(
    start: new Vector2(0, 0),
    end: new Vector2(99, 99)
);
```

#### B. Behavior Trees (`BehaviorTree.cs`)

Hierarchical decision-making system for AI.

**Node Types**:
- **Composite Nodes**:
  - `SequenceNode`: Execute children in order
  - `SelectorNode`: Try children until one succeeds
  - `ParallelNode`: Execute multiple children simultaneously
- **Decorator Nodes**:
  - `InverterNode`: Invert child result
  - `RepeaterNode`: Repeat child N times
- **Leaf Nodes**:
  - `ActionNode`: Perform an action
  - `ConditionNode`: Check a condition

**Usage**:
```csharp
var root = new SelectorNode();

var patrolSequence = new SequenceNode();
patrolSequence.AddChild(new ConditionNode(() => !enemyDetected));
patrolSequence.AddChild(new ActionNode(() => { Patrol(); return NodeStatus.Success; }));

var attackSequence = new SequenceNode();
attackSequence.AddChild(new ConditionNode(() => enemyDetected));
attackSequence.AddChild(new ActionNode(() => { Attack(); return NodeStatus.Success; }));

root.AddChild(patrolSequence);
root.AddChild(attackSequence);

var behaviorTree = new BehaviorTree(root);
behaviorTree.Tick();
```

#### C. State Machines (`StateMachine.cs`)

Finite state machines for managing AI states.

**Features**:
- State transitions
- OnEnter, OnUpdate, OnExit callbacks
- Predefined AI states (Idle, Patrol, Chase, Attack, Flee, Investigate)

**Usage**:
```csharp
var stateMachine = new StateMachine<AIState>();

stateMachine.AddState(AIState.Idle, new IdleState(
    shouldPatrol: () => true,
    detectEnemy: () => enemyInRange,
    maxIdleTime: 3.0f
));

stateMachine.AddState(AIState.Patrol, new PatrolState(
    detectEnemy: () => enemyInRange,
    reachedWaypoint: () => atWaypoint,
    moveToNextWaypoint: () => MoveToNext()
));

stateMachine.SetInitialState(AIState.Idle);
stateMachine.Update(deltaTime);
```

### 3. Multiplayer Sync

**Location**: `/Networking/NetworkManager.cs`

TCP-based networking system for state synchronization across clients.

#### Features:
- **Server/Client Architecture**: Host as server or connect as client
- **State Synchronization**: Automatic state sync across network
- **Message Types**: StateUpdate, Ping/Pong, VoiceData, Custom
- **Event System**: OnMessageReceived, OnClientConnected, OnClientDisconnected

#### Usage:
```csharp
var networkManager = new NetworkManager(port: 7777);

// As Server
await networkManager.StartServerAsync();

// As Client
await networkManager.ConnectAsync("127.0.0.1", 7777);

// Sync state
await networkManager.SyncState("playerPosition", new { x = 10, y = 20 });

// Get synced state
var position = networkManager.GetState<dynamic>("playerPosition");

// Broadcast message
await networkManager.BroadcastMessage(new NetworkMessage
{
    Type = MessageType.Custom,
    Data = new { message = "Hello World" }
});
```

### 4. Voice Chat

**Location**: `/VoiceChat/VoiceChatSystem.cs`

Integrated voice communication using NAudio.

#### Features:
- **Audio Capture**: Record voice input
- **Audio Playback**: Play received audio
- **Device Management**: Select input/output devices
- **Volume Control**: Adjust input/output volumes
- **Configurable**: Sample rate, channels, bit depth

#### Usage:
```csharp
var voiceChat = new VoiceChatSystem();
voiceChat.Initialize();

// Handle captured audio
voiceChat.OnAudioCaptured += (audioData) =>
{
    // Send over network
    await networkManager.BroadcastMessage(new NetworkMessage
    {
        Type = MessageType.VoiceData,
        Data = audioData
    });
};

// Start recording
voiceChat.StartRecording();

// Play received audio
voiceChat.PlayAudio(audioData);

// Stop recording
voiceChat.StopRecording();

// Get available devices
var inputDevices = VoiceChatSystem.GetInputDevices();
var outputDevices = VoiceChatSystem.GetOutputDevices();
```

## Phase 9.2 Features

### 5. Plugin Marketplace

**Location**: `/PluginSystem/PluginManager.cs`

Dynamic plugin loading system with marketplace support.

#### Features:
- **Plugin Loading**: Load external DLL plugins at runtime
- **Plugin Isolation**: Separate AssemblyLoadContext for each plugin
- **Plugin Lifecycle**: OnLoad, OnUpdate, OnUnload hooks
- **Type System**: IGameLogicPlugin, IRenderingPlugin, IAIPlugin interfaces
- **Marketplace**: Browse, download, and install plugins (placeholder)

#### Creating a Plugin:
```csharp
public class MyGamePlugin : PluginBase, IGameLogicPlugin
{
    public override string Name => "My Game Plugin";
    public override string Version => "1.0.0";
    public override string Description => "A custom game logic plugin";
    public override string Author => "Your Name";

    public override void OnLoad()
    {
        Console.WriteLine($"{Name} loaded!");
    }

    public void ProcessGameLogic()
    {
        // Custom game logic here
    }

    public override void OnUpdate(float deltaTime)
    {
        ProcessGameLogic();
    }
}
```

#### Using the Plugin Manager:
```csharp
var pluginManager = new PluginManager("Plugins");

// Load all plugins
pluginManager.LoadAllPlugins();

// Load specific plugin
pluginManager.LoadPlugin("Plugins/MyPlugin.dll");

// Get plugin
var plugin = pluginManager.GetPlugin("My Game Plugin");

// Update all plugins
pluginManager.Update(deltaTime);

// Unload plugin
pluginManager.UnloadPlugin("My Game Plugin");
```

### 6. Visual Scripting

**Location**: `/VisualScripting/VisualScriptingEngine.cs`

Node-based visual scripting system for game logic.

#### Features:
- **Node Types**: Math, Logic, Events, Debug, Custom
- **Graph Execution**: Flow-based execution
- **Built-in Nodes**:
  - Math: Add, Subtract, Multiply, Divide
  - Logic: If, And, Or, Not
  - Events: OnStart, OnUpdate
  - Debug: Print, Log
- **Custom Nodes**: Register your own node types

#### Usage:
```csharp
var scriptEngine = new VisualScriptingEngine();

// Create a graph
var graph = scriptEngine.CreateGraph("MyScript");

// Add nodes
var startNode = graph.AddNode("event.start", x: 0, y: 0);
var printNode = graph.AddNode("debug.log", x: 200, y: 0);

// Connect nodes
graph.Connect(
    sourceNodeId: startNode.Id,
    sourcePortName: "Flow",
    targetNodeId: printNode.Id,
    targetPortName: "Flow"
);

// Execute
graph.Execute();

// Register custom node
scriptEngine.RegisterNodeType(new NodeType
{
    TypeId = "custom.mynode",
    Name = "My Custom Node",
    Category = "Custom",
    Inputs = new[] { new NodePort { Name = "Input", DataType = "float" } },
    Outputs = new[] { new NodePort { Name = "Output", DataType = "float" } },
    Execute = (inputs) => new object[] { Convert.ToSingle(inputs[0]) * 2 }
});
```

### 7. Performance Profiling

**Location**: `/Profiling/PerformanceProfiler.cs`

Real-time performance analytics and monitoring.

#### Features:
- **Sample Tracking**: Track execution time of code blocks
- **Frame Timing**: FPS and frame time monitoring
- **Statistics**: Average, Min, Max, Current values
- **History**: Keep track of last N samples
- **Memory Profiling**: GC and memory usage tracking
- **Profiler Scope**: Convenient RAII-style profiling

#### Usage:
```csharp
var profiler = new PerformanceProfiler();

// Manual profiling
profiler.BeginSample("Physics");
physics.Update(deltaTime);
profiler.EndSample("Physics");

// Using scope (automatic End on dispose)
using (new ProfilerScope(profiler, "Rendering"))
{
    Render();
}

// End frame
profiler.EndFrame();

// Get statistics
var stats = profiler.GetStats("Physics");
Console.WriteLine($"Physics: {stats.Average:F2}ms avg, {stats.Max:F2}ms max");

// Print full report
profiler.PrintReport();

// Memory profiling
var memoryProfiler = new MemoryProfiler();
Console.WriteLine(memoryProfiler.GetMemoryInfo());
```

### 8. Asset Pipeline

**Location**: `/AssetPipeline/AssetPipeline.cs`

Advanced asset loading and optimization system.

#### Features:
- **Asset Types**: Text, JSON, Binary, Custom
- **Async Loading**: Non-blocking asset loading
- **Caching**: Automatic asset caching with statistics
- **Custom Loaders**: Implement IAssetLoader for custom types
- **Lifecycle Management**: Load, unload, track assets

#### Built-in Asset Types:
- **TextAsset**: Plain text files
- **JsonAsset**: JSON configuration files
- **BinaryAsset**: Binary data files

#### Usage:
```csharp
var assetPipeline = new AssetPipeline("Assets");

// Load assets
var textAsset = await assetPipeline.LoadAsync<TextAsset>("config.txt");
var jsonAsset = await assetPipeline.LoadAsync<JsonAsset>("data.json");
var binaryAsset = await assetPipeline.LoadAsync<BinaryAsset>("model.bin");

// Access asset data
Console.WriteLine(textAsset.Content);
var value = jsonAsset.Data.GetProperty("key").GetString();
var bytes = binaryAsset.Data;

// Unload asset
assetPipeline.Unload("config.txt");

// Get cache statistics
var stats = assetPipeline.GetCacheStats();
Console.WriteLine($"Cache hit rate: {stats.HitRate:P2}");

// Register custom loader
assetPipeline.RegisterLoader<MyCustomAsset>(new MyCustomAssetLoader());
```

## Integration Examples

### Example 1: AI with Physics

```csharp
var physics = new PhysicsEngine();
var pathfinding = new PathfindingSystem(100, 100);
var stateMachine = new StateMachine<AIState>();

// Setup states that use pathfinding
stateMachine.AddState(AIState.Chase, new ChaseState(
    isEnemyInRange: () => Vector2.Distance(aiPos, enemyPos) < 5.0f,
    lostEnemy: () => Vector2.Distance(aiPos, enemyPos) > 20.0f,
    chaseEnemy: () =>
    {
        var path = pathfinding.FindPath(aiPos, enemyPos);
        if (path.Count > 1)
        {
            var direction = Vector2.Normalize(path[1] - aiPos);
            aiBody.ApplyForce(direction * speed);
        }
    }
));

// Update in game loop
physics.Update(deltaTime);
stateMachine.Update(deltaTime);
```

### Example 2: Multiplayer with Voice Chat

```csharp
var network = new NetworkManager(7777);
var voiceChat = new VoiceChatSystem();

// Server
await network.StartServerAsync();

// Client
await network.ConnectAsync("server.ip", 7777);

// Send voice data
voiceChat.OnAudioCaptured += async (audioData) =>
{
    await network.BroadcastMessage(new NetworkMessage
    {
        Type = MessageType.VoiceData,
        Data = audioData
    });
};

// Receive voice data
network.OnMessageReceived += (clientId, message) =>
{
    if (message.Type == MessageType.VoiceData && message.Data is byte[] audio)
    {
        voiceChat.PlayAudio(audio);
    }
};
```

### Example 3: Plugin with Profiling

```csharp
var pluginManager = new PluginManager();
var profiler = new PerformanceProfiler();

pluginManager.LoadAllPlugins();

// Game loop with profiling
while (running)
{
    using (new ProfilerScope(profiler, "PluginUpdate"))
    {
        pluginManager.Update(deltaTime);
    }
    
    profiler.EndFrame();
}

// Print performance report
profiler.PrintReport();
```

### Example 4: Visual Scripting with Assets

```csharp
var scriptEngine = new VisualScriptingEngine();
var assetPipeline = new AssetPipeline();

// Load script definition from JSON
var scriptAsset = await assetPipeline.LoadAsync<JsonAsset>("Scripts/myScript.json");

// Create graph from asset
var graph = scriptEngine.CreateGraph("LoadedScript");
// ... parse JSON and create nodes ...

// Execute
graph.Execute();
```

## Configuration

All features can be configured through code or configuration files. Example configuration structure:

```json
{
  "Physics": {
    "Gravity": 9.81,
    "FixedTimeStep": 0.016
  },
  "Networking": {
    "Port": 7777,
    "MaxClients": 10
  },
  "VoiceChat": {
    "SampleRate": 16000,
    "Channels": 1,
    "BitsPerSample": 16
  },
  "PluginSystem": {
    "PluginDirectory": "Plugins",
    "MarketplaceUrl": "https://plugins.ashat.example.com"
  },
  "Profiling": {
    "Enabled": true,
    "HistorySize": 100
  },
  "AssetPipeline": {
    "AssetRoot": "Assets",
    "CachingEnabled": true
  }
}
```

## Performance Considerations

- **Physics**: Use spatial partitioning for large numbers of colliders
- **Networking**: Consider using UDP for real-time data like voice
- **Voice Chat**: Compress audio data before transmission
- **Plugins**: Unload unused plugins to save memory
- **Assets**: Unload unused assets periodically
- **Profiling**: Disable in production for best performance

## Future Enhancements

- WebSocket support for web-based clients
- Physics: Continuous collision detection
- AI: Navigation mesh support
- Networking: UDP support for low-latency
- Voice Chat: Opus codec integration
- Plugins: Hot-reload support
- Visual Scripting: Debugger and breakpoints
- Asset Pipeline: Streaming and progressive loading

## Troubleshooting

### Physics Issues
- **Objects falling through**: Increase physics update rate
- **Jittery movement**: Adjust drag and restitution values

### Networking Issues
- **Connection timeouts**: Check firewall settings
- **State desync**: Ensure consistent tick rates

### Voice Chat Issues
- **No audio**: Check device permissions and drivers
- **Echo**: Enable echo cancellation
- **Latency**: Reduce buffer size

### Plugin Issues
- **Load failures**: Check .NET version compatibility
- **Missing dependencies**: Ensure all DLLs are in plugin folder

## License

All features are part of the ASHAT Goddess project and follow the project's license terms.

## Support

For issues, questions, or contributions, please refer to the main project repository.
