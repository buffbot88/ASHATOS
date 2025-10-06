namespace Abstractions;

/// <summary>
/// Interface for the Game Server Module - Provides unified AI-driven game creation and deployment system.
/// Orchestrates AICodeGen, GameEngine, AIContent, and ServerSetup modules for complete game development.
/// </summary>
public interface IGameServerModule
{
    /// <summary>
    /// Creates a complete game from natural language description.
    /// Generates front-end, back-end, assets, and deployment configuration.
    /// </summary>
    Task<GameCreationResponse> CreateGameFromDescriptionAsync(GameCreationRequest request);
    
    /// <summary>
    /// Deploys a game server instance.
    /// </summary>
    Task<ServerDeploymentResponse> DeployGameServerAsync(string gameId, DeploymentOptions options);
    
    /// <summary>
    /// Gets preview information for a created game.
    /// </summary>
    Task<GamePreview> GetGamePreviewAsync(string gameId);
    
    /// <summary>
    /// Updates an existing game based on natural language modifications.
    /// </summary>
    Task<GameUpdateResponse> UpdateGameAsync(string gameId, string updateDescription);
    
    /// <summary>
    /// Lists all games created by a user.
    /// </summary>
    Task<List<GameProject>> ListUserGamesAsync(Guid userId);
    
    /// <summary>
    /// Gets detailed information about a game project.
    /// </summary>
    Task<GameProject?> GetGameProjectAsync(string gameId);
    
    /// <summary>
    /// Deletes a game project and its resources.
    /// </summary>
    Task<GameServerResponse> DeleteGameProjectAsync(string gameId);
    
    /// <summary>
    /// Exports a game project with source code and documentation.
    /// </summary>
    Task<GameExportResponse> ExportGameProjectAsync(string gameId, ExportFormat format);
    
    /// <summary>
    /// Gets server management capabilities and statistics.
    /// </summary>
    Task<ServerCapabilities> GetCapabilitiesAsync();
}

/// <summary>
/// Request for creating a game from natural language.
/// </summary>
public class GameCreationRequest
{
    public Guid UserId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GameType GameType { get; set; } = GameType.Multiplayer;
    public string Theme { get; set; } = "fantasy";
    public List<string> Features { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
    public bool AutoDeploy { get; set; } = false;
    public bool GenerateAssets { get; set; } = true;
}

/// <summary>
/// Type of game to create.
/// </summary>
public enum GameType
{
    SinglePlayer,
    Multiplayer,
    MMO,
    Cooperative,
    PvP,
    Sandbox
}

/// <summary>
/// Response from game creation operation.
/// </summary>
public class GameCreationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public GameProject? Project { get; set; }
    public List<string> GeneratedFiles { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Options for deploying a game server.
/// </summary>
public class DeploymentOptions
{
    public string InstanceName { get; set; } = string.Empty;
    public int MaxPlayers { get; set; } = 100;
    public int Port { get; set; } = 8080;
    public bool EnableWebSocket { get; set; } = true;
    public bool EnableDatabase { get; set; } = true;
    public Dictionary<string, object> CustomConfig { get; set; } = new();
}

/// <summary>
/// Response from server deployment operation.
/// </summary>
public class ServerDeploymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public ServerStatus Status { get; set; }
    public Dictionary<string, object> ConnectionInfo { get; set; } = new();
}

/// <summary>
/// Status of a deployed server.
/// </summary>
public enum ServerStatus
{
    Stopped,
    Starting,
    Running,
    Paused,
    Error
}

/// <summary>
/// Preview information for a game.
/// </summary>
public class GamePreview
{
    public string GameId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public List<string> Screenshots { get; set; } = new();
    public Dictionary<string, object> GameInfo { get; set; } = new();
}

/// <summary>
/// Response from game update operation.
/// </summary>
public class GameUpdateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> ModifiedFiles { get; set; } = new();
    public Dictionary<string, object> Changes { get; set; } = new();
}

/// <summary>
/// Represents a complete game project.
/// </summary>
public class GameProject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid UserId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GameType Type { get; set; }
    public string Theme { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ProjectPath { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public Dictionary<string, string> GeneratedFiles { get; set; } = new();
    public ServerDeploymentInfo? DeploymentInfo { get; set; }
    public GameMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Server deployment information for a game.
/// </summary>
public class ServerDeploymentInfo
{
    public string InstanceId { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public ServerStatus Status { get; set; }
    public DateTime? DeployedAt { get; set; }
    public int ActivePlayers { get; set; }
    public Dictionary<string, object> Config { get; set; } = new();
}

/// <summary>
/// Metrics for a game project.
/// </summary>
public class GameMetrics
{
    public int TotalAssets { get; set; }
    public int TotalScenes { get; set; }
    public int TotalEntities { get; set; }
    public int LinesOfCode { get; set; }
    public long TotalSizeBytes { get; set; }
}

/// <summary>
/// Response from generic game server operations.
/// </summary>
public class GameServerResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Response from game export operation.
/// </summary>
public class GameExportResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ExportPath { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public ExportFormat Format { get; set; }
}

/// <summary>
/// Format for game export.
/// </summary>
public enum ExportFormat
{
    FullProject,
    SourceCodeOnly,
    BinaryOnly,
    Documentation,
    Complete
}

/// <summary>
/// Server management capabilities and statistics.
/// </summary>
public class ServerCapabilities
{
    public int MaxConcurrentServers { get; set; }
    public int ActiveServers { get; set; }
    public List<GameType> SupportedGameTypes { get; set; } = new();
    public List<string> AvailableFeatures { get; set; } = new();
    public Dictionary<string, object> SystemInfo { get; set; } = new();
}
