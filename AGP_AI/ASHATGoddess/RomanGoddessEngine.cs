using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using LegendaryGameSystem.Core;

namespace ASHATGoddessClient;

/// <summary>
/// RomanGoddessEngine - A lightweight C# game engine with:
/// - Custom renderer
/// - AI backend connection
/// - Entity and Scene systems
/// - Goddess AI companion interface
/// </summary>
public class RomanGoddessEngine : IDisposable
{
    private readonly LegendaryGameEngineModule _gameEngine;
    private readonly Timer _updateTimer;
    private readonly Timer _renderTimer;
    private bool _isRunning;
    private bool _isInitialized;
    private string? _mainSceneId;
    private readonly List<string> _entityIds;
    private int _frameCount;
    private DateTime _lastFpsUpdate;
    private double _currentFps;
    private CancellationTokenSource? _cancellationTokenSource;

    // Engine configuration
    private const int TargetFPS = 60;
    private const int UpdateIntervalMs = 16; // ~60 FPS
    private const int RenderIntervalMs = 16; // ~60 FPS

    public event EventHandler<EngineStateEventArgs>? OnStateChanged;
    public event EventHandler<RenderEventArgs>? OnRender;
    public event EventHandler<UpdateEventArgs>? OnUpdate;

    public bool IsRunning => _isRunning;
    public bool IsInitialized => _isInitialized;
    public double CurrentFPS => _currentFps;
    public string? MainSceneId => _mainSceneId;

    public RomanGoddessEngine()
    {
        _gameEngine = new LegendaryGameEngineModule();
        _entityIds = new List<string>();
        _frameCount = 0;
        _lastFpsUpdate = DateTime.UtcNow;
        _currentFps = 0;

        // Initialize timers (but don't start them yet)
        _updateTimer = new Timer(UpdateLoop, null, Timeout.Infinite, Timeout.Infinite);
        _renderTimer = new Timer(RenderLoop, null, Timeout.Infinite, Timeout.Infinite);

        Console.WriteLine("[RomanGoddessEngine] Engine created");
    }

    /// <summary>
    /// Initialize the engine with optional scene name
    /// </summary>
    public async Task<bool> InitializeAsync(string sceneName = "Roman Goddess Scene")
    {
        if (_isInitialized)
        {
            Console.WriteLine("[RomanGoddessEngine] Engine already initialized");
            return true;
        }

        try
        {
            Console.WriteLine("[RomanGoddessEngine] Initializing engine...");

            // Initialize the underlying game engine module
            _gameEngine.Initialize(null);

            // Create the main scene
            var sceneResponse = await _gameEngine.CreateSceneAsync(sceneName, "RomanGoddessEngine");

            if (sceneResponse.Success && sceneResponse.Data is GameScene scene)
            {
                _mainSceneId = scene.Id;
                Console.WriteLine($"[RomanGoddessEngine] Main scene created: {_mainSceneId}");

                _isInitialized = true;
                OnStateChanged?.Invoke(this, new EngineStateEventArgs { State = EngineState.Initialized });

                Console.WriteLine("[RomanGoddessEngine] ✓ Engine initialized successfully");
                return true;
            }
            else
            {
                Console.WriteLine($"[RomanGoddessEngine] Failed to create main scene: {sceneResponse.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error during initialization: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Start the engine update and render loops
    /// </summary>
    public void Start()
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[RomanGoddessEngine] Cannot start: Engine not initialized");
            return;
        }

        if (_isRunning)
        {
            Console.WriteLine("[RomanGoddessEngine] Engine already running");
            return;
        }

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _lastFpsUpdate = DateTime.UtcNow;

        // Start the update and render loops
        _updateTimer.Change(0, UpdateIntervalMs);
        _renderTimer.Change(0, RenderIntervalMs);

        OnStateChanged?.Invoke(this, new EngineStateEventArgs { State = EngineState.Running });
        Console.WriteLine("[RomanGoddessEngine] ✓ Engine started - Update and render loops active");
    }

    /// <summary>
    /// Stop the engine update and render loops
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        // Stop the timers
        _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _renderTimer.Change(Timeout.Infinite, Timeout.Infinite);

        OnStateChanged?.Invoke(this, new EngineStateEventArgs { State = EngineState.Stopped });
        Console.WriteLine("[RomanGoddessEngine] Engine stopped");
    }

    /// <summary>
    /// Update loop - Called at regular intervals to update game state
    /// </summary>
    private void UpdateLoop(object? state)
    {
        if (!_isRunning || !_isInitialized)
        {
            return;
        }

        try
        {
            var deltaTime = (float)UpdateIntervalMs / 1000f;
            OnUpdate?.Invoke(this, new UpdateEventArgs { DeltaTime = deltaTime });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error in update loop: {ex.Message}");
        }
    }

    /// <summary>
    /// Render loop - Called at regular intervals to render the scene
    /// </summary>
    private void RenderLoop(object? state)
    {
        if (!_isRunning || !_isInitialized)
        {
            return;
        }

        try
        {
            _frameCount++;

            // Calculate FPS every second
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastFpsUpdate).TotalSeconds;
            if (elapsed >= 1.0)
            {
                _currentFps = _frameCount / elapsed;
                _frameCount = 0;
                _lastFpsUpdate = now;
            }

            var deltaTime = (float)RenderIntervalMs / 1000f;
            OnRender?.Invoke(this, new RenderEventArgs { DeltaTime = deltaTime, FPS = _currentFps });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error in render loop: {ex.Message}");
        }
    }

    /// <summary>
    /// Create an entity in the main scene
    /// </summary>
    public async Task<string?> CreateEntityAsync(string name, string type, Vector3? position = null, Dictionary<string, object>? properties = null)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_mainSceneId))
        {
            Console.WriteLine("[RomanGoddessEngine] Cannot create entity: Engine not initialized or no scene");
            return null;
        }

        try
        {
            var entity = new GameEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Type = type,
                Position = position ?? new Vector3 { X = 0, Y = 0, Z = 0 },
                Scale = new Vector3 { X = 1, Y = 1, Z = 1 },
                Rotation = new Vector3 { X = 0, Y = 0, Z = 0 },
                Properties = properties ?? new Dictionary<string, object>()
            };

            var response = await _gameEngine.CreateEntityAsync(_mainSceneId, entity, "RomanGoddessEngine");

            if (response.Success)
            {
                _entityIds.Add(entity.Id);
                Console.WriteLine($"[RomanGoddessEngine] Created entity '{name}' with ID: {entity.Id}");
                return entity.Id;
            }
            else
            {
                Console.WriteLine($"[RomanGoddessEngine] Failed to create entity: {response.Message}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error creating entity: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an entity in the main scene
    /// </summary>
    public async Task<bool> UpdateEntityAsync(string entityId, Dictionary<string, object> properties)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_mainSceneId))
        {
            return false;
        }

        try
        {
            var entities = await _gameEngine.ListEntitiesAsync(_mainSceneId);
            var entity = entities.FirstOrDefault(e => e.Id == entityId);

            if (entity == null)
            {
                Console.WriteLine($"[RomanGoddessEngine] Entity not found: {entityId}");
                return false;
            }

            // Update properties
            foreach (var prop in properties)
            {
                entity.Properties[prop.Key] = prop.Value;
            }

            var response = await _gameEngine.UpdateEntityAsync(_mainSceneId, entityId, entity, "RomanGoddessEngine");

            if (response.Success)
            {
                return true;
            }
            else
            {
                Console.WriteLine($"[RomanGoddessEngine] Failed to update entity: {response.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error updating entity: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get an entity by ID
    /// </summary>
    public async Task<GameEntity?> GetEntityAsync(string entityId)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_mainSceneId))
        {
            return null;
        }

        try
        {
            var entities = await _gameEngine.ListEntitiesAsync(_mainSceneId);
            return entities.FirstOrDefault(e => e.Id == entityId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error getting entity: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// List all entities in the main scene
    /// </summary>
    public async Task<List<GameEntity>> ListEntitiesAsync()
    {
        if (!_isInitialized || string.IsNullOrEmpty(_mainSceneId))
        {
            return new List<GameEntity>();
        }

        try
        {
            return await _gameEngine.ListEntitiesAsync(_mainSceneId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error listing entities: {ex.Message}");
            return new List<GameEntity>();
        }
    }

    /// <summary>
    /// Delete an entity from the main scene
    /// </summary>
    public async Task<bool> DeleteEntityAsync(string entityId)
    {
        if (!_isInitialized || string.IsNullOrEmpty(_mainSceneId))
        {
            return false;
        }

        try
        {
            var response = await _gameEngine.DeleteEntityAsync(_mainSceneId, entityId, "RomanGoddessEngine");

            if (response.Success)
            {
                _entityIds.Remove(entityId);
                Console.WriteLine($"[RomanGoddessEngine] Deleted entity: {entityId}");
                return true;
            }
            else
            {
                Console.WriteLine($"[RomanGoddessEngine] Failed to delete entity: {response.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error deleting entity: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get engine statistics
    /// </summary>
    public async Task<EngineStats?> GetStatsAsync()
    {
        try
        {
            return await _gameEngine.GetStatsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error getting stats: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get engine information including FPS and entity count
    /// </summary>
    public async Task<EngineInfo> GetEngineInfoAsync()
    {
        var entityCount = _entityIds.Count;
        var stats = await GetStatsAsync();

        return new EngineInfo
        {
            IsRunning = _isRunning,
            IsInitialized = _isInitialized,
            CurrentFPS = _currentFps,
            TargetFPS = TargetFPS,
            EntityCount = entityCount,
            SceneId = _mainSceneId,
            TotalScenes = stats?.TotalScenes ?? 0,
            TotalEntities = stats?.TotalEntities ?? 0
        };
    }

    public void Dispose()
    {
        try
        {
            Stop();

            _updateTimer?.Dispose();
            _renderTimer?.Dispose();
            _cancellationTokenSource?.Dispose();

            // Clean up the scene
            if (_isInitialized && !string.IsNullOrEmpty(_mainSceneId))
            {
                _gameEngine.DeleteSceneAsync(_mainSceneId, "RomanGoddessEngine").Wait();
                Console.WriteLine("[RomanGoddessEngine] Cleaned up main scene");
            }

            _gameEngine?.Dispose();
            Console.WriteLine("[RomanGoddessEngine] Engine disposed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RomanGoddessEngine] Error during disposal: {ex.Message}");
        }
    }
}

/// <summary>
/// Engine state enumeration
/// </summary>
public enum EngineState
{
    Uninitialized,
    Initialized,
    Running,
    Stopped
}

/// <summary>
/// Engine state change event args
/// </summary>
public class EngineStateEventArgs : EventArgs
{
    public EngineState State { get; set; }
}

/// <summary>
/// Render event args
/// </summary>
public class RenderEventArgs : EventArgs
{
    public float DeltaTime { get; set; }
    public double FPS { get; set; }
}

/// <summary>
/// Update event args
/// </summary>
public class UpdateEventArgs : EventArgs
{
    public float DeltaTime { get; set; }
}

/// <summary>
/// Engine information
/// </summary>
public class EngineInfo
{
    public bool IsRunning { get; set; }
    public bool IsInitialized { get; set; }
    public double CurrentFPS { get; set; }
    public int TargetFPS { get; set; }
    public int EntityCount { get; set; }
    public string? SceneId { get; set; }
    public int TotalScenes { get; set; }
    public int TotalEntities { get; set; }

    public override string ToString()
    {
        return $"RomanGoddessEngine: Running={IsRunning}, FPS={CurrentFPS:F2}/{TargetFPS}, " +
               $"Entities={EntityCount}, Scenes={TotalScenes}, TotalEntities={TotalEntities}";
    }
}
