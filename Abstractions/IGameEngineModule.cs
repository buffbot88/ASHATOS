namespace Abstractions;

/// <summary>
/// Interface for the Game Engine Module - Provides core game engine functionality
/// controllable by RaCore AI modules.
/// </summary>
public interface IGameEngineModule
{
    /// <summary>
    /// Creates a new game scene with the specified name.
    /// </summary>
    Task<GameEngineResponse> CreateSceneAsync(string sceneName, string createdBy);

    /// <summary>
    /// Lists all active scenes in the game engine.
    /// </summary>
    Task<List<GameScene>> ListScenesAsync();

    /// <summary>
    /// Gets details of a specific scene.
    /// </summary>
    Task<GameScene?> GetSceneAsync(string sceneId);

    /// <summary>
    /// Deletes a scene by ID.
    /// </summary>
    Task<GameEngineResponse> DeleteSceneAsync(string sceneId, string deletedBy);

    /// <summary>
    /// Creates an entity in the specified scene.
    /// </summary>
    Task<GameEngineResponse> CreateEntityAsync(string sceneId, GameEntity entity, string createdBy);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<GameEngineResponse> UpdateEntityAsync(string sceneId, string entityId, GameEntity entity, string updatedBy);

    /// <summary>
    /// Deletes an entity from a scene.
    /// </summary>
    Task<GameEngineResponse> DeleteEntityAsync(string sceneId, string entityId, string deletedBy);

    /// <summary>
    /// Lists all entities in a scene.
    /// </summary>
    Task<List<GameEntity>> ListEntitiesAsync(string sceneId);

    /// <summary>
    /// Generates world content using AI (NPCs, quests, terrain, etc.).
    /// </summary>
    Task<GameEngineResponse> GenerateWorldContentAsync(string sceneId, WorldGenerationRequest request, string requestedBy);

    /// <summary>
    /// Streams an asset into the game engine.
    /// </summary>
    Task<GameEngineResponse> StreamAssetAsync(AssetStreamRequest request);

    /// <summary>
    /// Gets engine statistics and health.
    /// </summary>
    Task<EngineStats> GetStatsAsync();

    /// <summary>
    /// Broadcasts a game event to connected clients.
    /// </summary>
    Task BroadcastEventAsync(GameEvent gameEvent);
    
    /// <summary>
    /// Get all scenes (Admin+).
    /// </summary>
    List<GameScene> GetAllScenes();
}

/// <summary>
/// Response from game engine operations.
/// </summary>
public class GameEngineResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// Represents a game scene (world, level, area).
/// </summary>
public class GameScene
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<GameEntity> Entities { get; set; } = new();
}

/// <summary>
/// Represents a game entity (character, object, NPC, etc.).
/// </summary>
public class GameEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Generic"; // NPC, Player, Object, Terrain, etc.
    public Vector3 Position { get; set; } = new();
    public Vector3 Rotation { get; set; } = new();
    public Vector3 Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// 3D Vector for position, rotation, scale.
/// </summary>
public class Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

/// <summary>
/// Request for AI-driven world generation.
/// </summary>
public class WorldGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string Theme { get; set; } = "fantasy";
    public int EntityCount { get; set; } = 10;
    public bool GenerateNPCs { get; set; } = true;
    public bool GenerateTerrain { get; set; } = true;
    public bool GenerateQuests { get; set; } = false;
}

/// <summary>
/// Request for asset streaming.
/// </summary>
public class AssetStreamRequest
{
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = "Model"; // Model, Texture, Audio, etc.
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Engine statistics and health metrics.
/// </summary>
public class EngineStats
{
    public int TotalScenes { get; set; }
    public int ActiveScenes { get; set; }
    public int TotalEntities { get; set; }
    public long MemoryUsageMB { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ConnectedClients { get; set; }
    public DateTime StartTime { get; set; }
}

/// <summary>
/// Game event for broadcasting to clients.
/// </summary>
public class GameEvent
{
    public string EventType { get; set; } = string.Empty;
    public string SceneId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
