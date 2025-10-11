using Abstractions;

namespace LegendaryClientBuilder.Core;

/// <summary>
/// Interface for the Legendary Client Builder module.
/// Provides advanced multi-platform game client Generation capabilities.
/// </summary>
public interface ILegendaryClientBuilderModule : IGameClientModule
{
    /// <summary>
    /// Gets the module version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets statistics about client Generation.
    /// </summary>
    ClientBuilderStats GetStats();

    /// <summary>
    /// Generate a client with advanced template options.
    /// </summary>
    Task<GameClientPackage> GenerateClientFromTemplateAsync(
        Guid userId, 
        string licenseKey, 
        ClientPlatform platform, 
        string templateName,
        ClientConfiguration config);

    /// <summary>
    /// Get available templates for a platform. Pass null to get all templates.
    /// </summary>
    IEnumerable<ClientTemplate> GetAvailableTemplates(ClientPlatform? platform = null);

    /// <summary>
    /// Register a custom client template.
    /// </summary>
    void RegisterTemplate(ClientTemplate template);

    /// <summary>
    /// Delete a client package.
    /// </summary>
    Task<bool> DeleteClientAsync(Guid packageId);

    /// <summary>
    /// ReGenerate a client with updated Configuration.
    /// </summary>
    Task<GameClientPackage> ReGenerateClientAsync(Guid packageId, ClientConfiguration newConfig);
}

/// <summary>
/// Statistics for client builder Operations.
/// </summary>
public class ClientBuilderStats
{
    public int TotalClients { get; set; }
    public Dictionary<ClientPlatform, int> ClientsByPlatform { get; set; } = new();
    public Dictionary<string, int> ClientsByTemplate { get; set; } = new();
    public int TotalUsers { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime ModuleStartTime { get; set; }
}

/// <summary>
/// Represents a client template.
/// </summary>
public class ClientTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ClientPlatform Platform { get; set; }
    public string Category { get; set; } = "Default";
    public Dictionary<string, string> DefaultSettings { get; set; } = new();
    public bool IsBuiltIn { get; set; } = true;
}
