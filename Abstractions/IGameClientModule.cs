namespace Abstractions;

/// <summary>
/// Interface for game client generation functionality.
/// Generates multi-platform game client screens for each game server.
/// </summary>
public interface IGameClientModule : IDisposable
{
    /// <summary>
    /// Generate a game client for a specific server instance.
    /// </summary>
    Task<GameClientPackage> GenerateClientAsync(Guid userId, string licenseKey, ClientPlatform platform, ClientConfiguration config);
    
    /// <summary>
    /// Get client package by ID.
    /// </summary>
    GameClientPackage? GetClientPackage(Guid packageId);
    
    /// <summary>
    /// Get all client packages for a user.
    /// </summary>
    IEnumerable<GameClientPackage> GetUserClientPackages(Guid userId);
    
    /// <summary>
    /// Update client configuration.
    /// </summary>
    Task<bool> UpdateClientConfigAsync(Guid packageId, ClientConfiguration config);
}

/// <summary>
/// Game client package containing the generated client.
/// </summary>
public class GameClientPackage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public ClientPlatform Platform { get; set; }
    public string PackagePath { get; set; } = string.Empty;
    public ClientConfiguration Configuration { get; set; } = new();
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ClientUrl { get; set; } = string.Empty;
}

/// <summary>
/// Client configuration for game server connection.
/// </summary>
public class ClientConfiguration
{
    public string ServerUrl { get; set; } = string.Empty;
    public int ServerPort { get; set; } = 5000;
    public string GameTitle { get; set; } = "RaCore Game";
    public string Theme { get; set; } = "fantasy";
    public Dictionary<string, string> CustomSettings { get; set; } = new();
}

/// <summary>
/// Supported client platforms.
/// </summary>
public enum ClientPlatform
{
    WebGL,
    Windows,
    Linux,
    MacOS,
    Android,
    iOS
}
