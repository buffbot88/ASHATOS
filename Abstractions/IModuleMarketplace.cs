namespace Abstractions;

/// <summary>
/// Module package information for marketplace
/// </summary>
public class ModulePackage
{
    public string PackageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime PublishedAt { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ChecksumSha256 { get; set; }
    public bool IsTrusted { get; set; }
}

/// <summary>
/// Module discovery result
/// </summary>
public class ModuleDiscoveryResult
{
    public List<ModulePackage> AvailableModules { get; set; } = new();
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
}

/// <summary>
/// Interface for module marketplace/discovery
/// </summary>
public interface IModuleMarketplace
{
    /// <summary>
    /// Discover available modules from trusted sources
    /// </summary>
    Task<ModuleDiscoveryResult> DiscoverModulesAsync();
    
    /// <summary>
    /// Install a module from the marketplace
    /// </summary>
    Task<bool> InstallModuleAsync(string packageId, bool verifyChecksum = true);
    
    /// <summary>
    /// Update a module to the latest version
    /// </summary>
    Task<bool> UpdateModuleAsync(string moduleName);
}
