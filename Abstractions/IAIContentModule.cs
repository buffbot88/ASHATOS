namespace Abstractions;

/// <summary>
/// Interface for AI Content Generation Module - Generates game assets, configurations, and content
/// </summary>
public interface IAIContentModule
{
    /// <summary>
    /// Generate game assets for a licensed admin
    /// </summary>
    Task<ContentGenerationResponse> GenerateGameAssetsAsync(Guid userId, string licenseKey, GameAssetRequest request);
    
    /// <summary>
    /// Get content generation capabilities
    /// </summary>
    ContentCapabilities GetCapabilities();
    
    /// <summary>
    /// List generated assets for a user
    /// </summary>
    List<GeneratedAsset> GetUserAssets(Guid userId);
    
    /// <summary>
    /// Get Licensed-Admin folder path for a user
    /// </summary>
    string GetLicensedAdminFolderPath(Guid userId, string licenseKey);
}

/// <summary>
/// Request for generating game assets
/// </summary>
public class GameAssetRequest
{
    public string Prompt { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public string Theme { get; set; } = "medieval";
    public int Count { get; set; } = 1;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Types of assets that can be generated
/// </summary>
public enum AssetType
{
    World,
    NPC,
    Item,
    Quest,
    Dialogue,
    Texture,
    Model,
    Sound,
    Music,
    Script,
    Configuration
}

/// <summary>
/// Response from content generation
/// </summary>
public class ContentGenerationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<GeneratedAsset> Assets { get; set; } = new();
    public string FolderPath { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a generated asset
/// </summary>
public class GeneratedAsset
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public long SizeBytes { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Content generation capabilities
/// </summary>
public class ContentCapabilities
{
    public List<AssetType> SupportedTypes { get; set; } = new();
    public List<string> SupportedThemes { get; set; } = new();
    public Dictionary<string, object> Limits { get; set; } = new();
}
