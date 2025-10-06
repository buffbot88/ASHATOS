using System.Threading.Tasks;

namespace Abstractions;

/// <summary>
/// Interface for server setup and configuration management.
/// Handles Nginx, PHP, Database folders, and per-admin instance management.
/// </summary>
public interface IServerSetupModule
{
    /// <summary>
    /// Setup Nginx web server configuration for an admin
    /// </summary>
    Task<SetupResult> SetupNginxConfigAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Setup PHP configuration for an admin
    /// </summary>
    Task<SetupResult> SetupPhpConfigAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Create admin folder structure
    /// </summary>
    Task<SetupResult> CreateAdminFolderStructureAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Discover and validate required folders (Databases, php, Nginx)
    /// </summary>
    Task<DiscoveryResult> DiscoverServerFoldersAsync();
    
    /// <summary>
    /// Get admin instance path
    /// </summary>
    string GetAdminInstancePath(string licenseNumber, string username);
}

/// <summary>
/// Result of a setup operation
/// </summary>
public class SetupResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Path { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// Result of folder discovery
/// </summary>
public class DiscoveryResult
{
    public bool DatabasesFolderExists { get; set; }
    public string? DatabasesFolderPath { get; set; }
    
    public bool PhpFolderExists { get; set; }
    public string? PhpFolderPath { get; set; }
    
    public bool NginxFolderExists { get; set; }
    public string? NginxFolderPath { get; set; }
    
    public bool AdminsFolderExists { get; set; }
    public string? AdminsFolderPath { get; set; }
    
    public List<string> MissingFolders { get; set; } = new();
    public List<string> CreatedFolders { get; set; } = new();
}
