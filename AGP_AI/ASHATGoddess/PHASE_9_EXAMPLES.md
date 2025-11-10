# Phase 9 Features - Usage Examples

This document provides practical examples of how to use the Phase 9.1 and 9.2 features in the ASHAT Goddess platform.

## Example 1: Creating a Simple Physics Simulation

```csharp
using ASHATGoddessClient.PhysicsEngine;
using System.Numerics;

public class PhysicsDemo
{
    public void Run()
    {
        // Create physics engine
        var physics = new PhysicsEngine(gravity: 9.81f);

        // Create ground (kinematic - doesn't move)
        var groundBody = new RigidBody
        {
            Position = new Vector2(0, 100),
            IsKinematic = true
        };
        
        var groundCollider = new BoxCollider
        {
            Position = new Vector2(0, 100),
            Size = new Vector2(200, 10),
            RigidBody = groundBody
        };

        // Create falling ball
        var ballBody = new RigidBody
        {
            Position = new Vector2(0, 0),
            Mass = 1.0f,
            Restitution = 0.8f // Bouncy!
        };
        
        var ballCollider = new CircleCollider
        {
            Position = new Vector2(0, 0),
            Radius = 5,
            RigidBody = ballBody
        };

        // Add to physics world
        physics.AddRigidBody(groundBody);
        physics.AddRigidBody(ballBody);
        physics.AddCollider(groundCollider);
        physics.AddCollider(ballCollider);

        // Simulation loop
        for (int i = 0; i < 100; i++)
        {
            float deltaTime = 0.016f; // 60 FPS
            physics.Update(deltaTime);
            
            // Update collider positions
            ballCollider.Position = ballBody.Position;
            
            Console.WriteLine($"Frame {i}: Ball at {ballBody.Position.Y:F2}");
        }
    }
}
```

## Example 2: AI Patrol with State Machine

```csharp
using ASHATGoddessClient.AI;
using System.Numerics;

public class AIAgent
{
    private StateMachine<AIState> stateMachine;
    private Vector2 position;
    private Vector2 enemyPosition;
    private List<Vector2> patrolPoints;
    private int currentPatrolIndex;

    public void Initialize()
    {
        stateMachine = new StateMachine<AIState>();
        patrolPoints = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(10, 0),
            new Vector2(10, 10),
            new Vector2(0, 10)
        };

        // Setup states
        stateMachine.AddState(AIState.Idle, new IdleState(
            shouldPatrol: () => true,
            detectEnemy: () => Vector2.Distance(position, enemyPosition) < 15.0f,
            maxIdleTime: 2.0f
        ));

        stateMachine.AddState(AIState.Patrol, new PatrolState(
            detectEnemy: () => Vector2.Distance(position, enemyPosition) < 15.0f,
            reachedWaypoint: () => Vector2.Distance(position, patrolPoints[currentPatrolIndex]) < 1.0f,
            moveToNextWaypoint: () =>
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                Console.WriteLine($"Moving to waypoint {currentPatrolIndex}");
            }
        ));

        stateMachine.AddState(AIState.Chase, new ChaseState(
            isEnemyInRange: () => Vector2.Distance(position, enemyPosition) < 3.0f,
            lostEnemy: () => Vector2.Distance(position, enemyPosition) > 20.0f,
            chaseEnemy: () =>
            {
                var direction = Vector2.Normalize(enemyPosition - position);
                position += direction * 0.5f; // Move towards enemy
            }
        ));

        stateMachine.SetInitialState(AIState.Idle);
    }

    public void Update(float deltaTime)
    {
        stateMachine.Update(deltaTime);
        
        // Move towards current patrol point when patrolling
        if (stateMachine.GetCurrentState() == AIState.Patrol)
        {
            var target = patrolPoints[currentPatrolIndex];
            var direction = Vector2.Normalize(target - position);
            position += direction * 0.2f * deltaTime * 60; // Adjust for frame time
        }
    }
}
```

## Example 3: Multiplayer Game Server

```csharp
using ASHATGoddessClient.Networking;
using System.Collections.Generic;

public class GameServer
{
    private NetworkManager network;
    private Dictionary<string, PlayerState> players = new();

    public async Task Start()
    {
        network = new NetworkManager(port: 7777);

        // Setup event handlers
        network.OnClientConnected += OnPlayerJoined;
        network.OnClientDisconnected += OnPlayerLeft;
        network.OnMessageReceived += OnMessageReceived;

        // Start server
        await network.StartServerAsync();
        Console.WriteLine("Game server started!");

        // Game loop
        while (true)
        {
            await UpdateGameState();
            await Task.Delay(16); // ~60 FPS
        }
    }

    private void OnPlayerJoined(string clientId)
    {
        players[clientId] = new PlayerState
        {
            Position = new Vector2(0, 0),
            Health = 100
        };

        Console.WriteLine($"Player {clientId} joined. Total players: {players.Count}");
        
        // Broadcast to all players
        _ = network.BroadcastMessage(new NetworkMessage
        {
            Type = MessageType.Custom,
            Data = new { action = "player_joined", playerId = clientId }
        });
    }

    private void OnPlayerLeft(string clientId)
    {
        players.Remove(clientId);
        Console.WriteLine($"Player {clientId} left. Total players: {players.Count}");
    }

    private void OnMessageReceived(string clientId, NetworkMessage message)
    {
        if (message.Type == MessageType.Custom && message.Data != null)
        {
            // Handle custom game messages
            Console.WriteLine($"Received from {clientId}: {message.Data}");
        }
    }

    private async Task UpdateGameState()
    {
        // Sync all player positions
        foreach (var kvp in players)
        {
            await network.SyncState($"player_{kvp.Key}_pos", kvp.Value.Position);
            await network.SyncState($"player_{kvp.Key}_health", kvp.Value.Health);
        }
    }

    class PlayerState
    {
        public Vector2 Position { get; set; }
        public int Health { get; set; }
    }
}
```

## Example 4: Visual Scripting for Game Logic

```csharp
using ASHATGoddessClient.VisualScripting;

public class ScriptingDemo
{
    public void CreateHealthSystem()
    {
        var engine = new VisualScriptingEngine();
        var graph = engine.CreateGraph("HealthSystem");

        // Create nodes
        var startNode = graph.AddNode("event.start", x: 0, y: 0);
        
        // Check if health is low
        var healthCheck = graph.AddNode("logic.if", x: 200, y: 0);
        
        // Print warning
        var printWarning = graph.AddNode("debug.log", x: 400, y: -50);
        
        // Print OK
        var printOK = graph.AddNode("debug.log", x: 400, y: 50);

        // Connect nodes
        graph.Connect(startNode.Id, "Flow", healthCheck.Id, "Condition");
        graph.Connect(healthCheck.Id, "True", printWarning.Id, "Flow");
        graph.Connect(healthCheck.Id, "False", printOK.Id, "Flow");

        // Execute
        graph.Execute();
    }

    public void CreateCustomNodes()
    {
        var engine = new VisualScriptingEngine();

        // Register damage calculation node
        engine.RegisterNodeType(new NodeType
        {
            TypeId = "game.calculate_damage",
            Name = "Calculate Damage",
            Category = "Game",
            Inputs = new[]
            {
                new NodePort { Name = "BaseDamage", DataType = "float" },
                new NodePort { Name = "Multiplier", DataType = "float" }
            },
            Outputs = new[]
            {
                new NodePort { Name = "TotalDamage", DataType = "float" }
            },
            Execute = (inputs) =>
            {
                var base = Convert.ToSingle(inputs[0]);
                var mult = Convert.ToSingle(inputs[1]);
                var total = base * mult;
                Console.WriteLine($"Damage calculated: {base} × {mult} = {total}");
                return new object[] { total };
            }
        });

        // Use the custom node
        var graph = engine.CreateGraph("DamageSystem");
        var damageNode = graph.AddNode("game.calculate_damage");
        // ... connect and execute
    }
}
```

## Example 5: Performance Profiling

```csharp
using ASHATGoddessClient.Profiling;

public class GameWithProfiling
{
    private PerformanceProfiler profiler = new();

    public void GameLoop()
    {
        for (int frame = 0; frame < 1000; frame++)
        {
            // Profile physics
            profiler.BeginSample("Physics");
            SimulatePhysics();
            profiler.EndSample("Physics");

            // Profile rendering using scope
            using (new ProfilerScope(profiler, "Rendering"))
            {
                RenderScene();
            }

            // Profile AI
            profiler.BeginSample("AI");
            UpdateAI();
            profiler.EndSample("AI");

            // End frame
            profiler.EndFrame();

            // Print report every 60 frames
            if (frame % 60 == 0)
            {
                profiler.PrintReport();
            }
        }
    }

    private void SimulatePhysics()
    {
        // Physics code here
        Thread.Sleep(2); // Simulate work
    }

    private void RenderScene()
    {
        // Rendering code here
        Thread.Sleep(5); // Simulate work
    }

    private void UpdateAI()
    {
        // AI code here
        Thread.Sleep(3); // Simulate work
    }
}
```

## Example 6: Plugin Development

### Creating a Plugin

```csharp
// MyGamePlugin.cs
using ASHATGoddessClient.PluginSystem;

public class MyGamePlugin : PluginBase, IGameLogicPlugin
{
    private int tickCount = 0;

    public override string Name => "My Awesome Plugin";
    public override string Version => "1.0.0";
    public override string Description => "Adds custom game mechanics";
    public override string Author => "Your Name";

    public override void OnLoad()
    {
        Console.WriteLine($"[{Name}] Plugin loaded successfully!");
        base.OnLoad();
    }

    public void ProcessGameLogic()
    {
        tickCount++;
        
        if (tickCount % 60 == 0)
        {
            Console.WriteLine($"[{Name}] Plugin has been running for {tickCount / 60} seconds");
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        ProcessGameLogic();
    }

    public override void OnUnload()
    {
        Console.WriteLine($"[{Name}] Plugin unloaded after {tickCount} ticks");
        base.OnUnload();
    }
}
```

### Using the Plugin

```csharp
using ASHATGoddessClient.PluginSystem;

public class PluginHostDemo
{
    public void Run()
    {
        var pluginManager = new PluginManager("Plugins");

        // Load all plugins
        pluginManager.LoadAllPlugins();

        // Get specific plugin
        var myPlugin = pluginManager.GetPlugin("My Awesome Plugin");
        if (myPlugin != null)
        {
            Console.WriteLine($"Found plugin: {myPlugin.Name} v{myPlugin.Version}");
        }

        // Update loop
        for (int i = 0; i < 300; i++)
        {
            pluginManager.Update(0.016f);
            Thread.Sleep(16);
        }

        // Cleanup
        pluginManager.UnloadAll();
    }
}
```

## Example 7: Asset Loading Pipeline

```csharp
using ASHATGoddessClient.AssetPipeline;

public class AssetDemo
{
    public async Task Run()
    {
        var assetPipeline = new AssetPipeline("Assets");

        // Load configuration
        var config = await assetPipeline.LoadAsync<JsonAsset>("config/game.json");
        if (config != null)
        {
            var levelCount = config.Data.GetProperty("levels").GetArrayLength();
            Console.WriteLine($"Game has {levelCount} levels");
        }

        // Load multiple assets
        var tasks = new[]
        {
            assetPipeline.LoadAsync<TextAsset>("data/story.txt"),
            assetPipeline.LoadAsync<JsonAsset>("data/items.json"),
            assetPipeline.LoadAsync<BinaryAsset>("models/player.bin")
        };

        await Task.WhenAll(tasks);

        // Access loaded data
        var story = tasks[0].Result;
        var items = tasks[1].Result;
        var model = tasks[2].Result;

        Console.WriteLine($"Story: {story?.Content.Substring(0, 50)}...");
        Console.WriteLine($"Items loaded: {items?.Data.GetArrayLength() ?? 0}");
        Console.WriteLine($"Model size: {model?.Size ?? 0} bytes");

        // Get cache statistics
        var stats = assetPipeline.GetCacheStats();
        Console.WriteLine($"Cache hit rate: {stats.HitRate:P2}");
        Console.WriteLine($"Total hits: {stats.TotalHits}");
        Console.WriteLine($"Total misses: {stats.TotalMisses}");

        // Unload when done
        assetPipeline.UnloadAll();
    }
}
```

## Example 8: Complete Game Loop Integration

```csharp
using ASHATGoddessClient.PhysicsEngine;
using ASHATGoddessClient.AI;
using ASHATGoddessClient.Networking;
using ASHATGoddessClient.Profiling;
using ASHATGoddessClient.PluginSystem;
using ASHATGoddessClient.AssetPipeline;

public class CompleteGame
{
    private PhysicsEngine physics;
    private PerformanceProfiler profiler;
    private PluginManager plugins;
    private AssetPipeline assets;
    private NetworkManager network;
    private bool isRunning = true;

    public async Task Start()
    {
        // Initialize all systems
        physics = new PhysicsEngine();
        profiler = new PerformanceProfiler();
        plugins = new PluginManager();
        assets = new AssetPipeline();
        network = new NetworkManager();

        // Load assets
        Console.WriteLine("Loading assets...");
        await assets.LoadAsync<JsonAsset>("config/game.json");

        // Load plugins
        Console.WriteLine("Loading plugins...");
        plugins.LoadAllPlugins();

        // Start network (optional)
        // await network.StartServerAsync();

        // Game loop
        Console.WriteLine("Starting game loop...");
        while (isRunning)
        {
            float deltaTime = 0.016f;

            // Update physics
            using (new ProfilerScope(profiler, "Physics"))
            {
                physics.Update(deltaTime);
            }

            // Update plugins
            using (new ProfilerScope(profiler, "Plugins"))
            {
                plugins.Update(deltaTime);
            }

            // End frame and print stats occasionally
            profiler.EndFrame();
            
            await Task.Delay(16); // ~60 FPS
        }

        // Cleanup
        plugins.UnloadAll();
        assets.UnloadAll();
        network.Stop();
    }
}
```

## Running the Examples

To run these examples:

1. Create a new C# project or add to existing ASHAT project
2. Copy the example code
3. Add necessary using statements
4. Call from your Program.cs:

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        // Run any example
        var demo = new CompleteGame();
        await demo.Start();
    }
}
```

## Next Steps

- Experiment with combining multiple systems
- Create custom plugins for your needs
- Profile your game to find bottlenecks
- Build networked multiplayer experiences
- Create visual scripts without coding

For more information, see [PHASE_9_FEATURES.md](PHASE_9_FEATURES.md).
