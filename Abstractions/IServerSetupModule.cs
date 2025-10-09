using System.Threading.Tasks;

namespace Abstractions;

/// <summary>
/// Interface for server setup and configuration management.
/// Handles PHP, Database folders, FTP, and per-admin instance management.
/// </summary>
public interface IServerSetupModule
{
    /// <summary>
    /// Setup PHP configuration for an admin
    /// </summary>
    Task<SetupResult> SetupPhpConfigAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Create admin folder structure
    /// </summary>
    Task<SetupResult> CreateAdminFolderStructureAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Discover and validate required folders (Databases, php)
    /// </summary>
    Task<DiscoveryResult> DiscoverServerFoldersAsync();
    
    /// <summary>
    /// Get admin instance path
    /// </summary>
    string GetAdminInstancePath(string licenseNumber, string username);
    
    /// <summary>
    /// Get FTP server status (checks if vsftpd is installed and running on Linux)
    /// </summary>
    Task<FtpStatusResult> GetFtpStatusAsync();
    
    /// <summary>
    /// Setup FTP access for an admin instance
    /// </summary>
    Task<SetupResult> SetupFtpAccessAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Get FTP connection info for an admin
    /// </summary>
    Task<FtpConnectionInfo> GetFtpConnectionInfoAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Create a restricted FTP user for RaOS folder access
    /// </summary>
    Task<SetupResult> CreateRestrictedFtpUserAsync(string username, string restrictedPath);
    
    /// <summary>
    /// Check if the live server is operational and ready for FTP setup
    /// </summary>
    Task<ServerHealthResult> CheckLiveServerHealthAsync();
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
    
    public bool AdminsFolderExists { get; set; }
    public string? AdminsFolderPath { get; set; }
    
    public List<string> MissingFolders { get; set; } = new();
    public List<string> CreatedFolders { get; set; } = new();
}

/// <summary>
/// Result of FTP status check
/// </summary>
public class FtpStatusResult
{
    public bool IsInstalled { get; set; }
    public bool IsRunning { get; set; }
    public bool IsLinux { get; set; }
    public string? Version { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ConfigPath { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// FTP connection information for an admin
/// </summary>
public class FtpConnectionInfo
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int Port { get; set; } = 21;
    public string? Username { get; set; }
    public string? FtpPath { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// Server health check result
/// </summary>
public class ServerHealthResult
{
    public bool IsOperational { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool DatabasesAccessible { get; set; }
    public bool PhpFolderAccessible { get; set; }
    public bool AdminsFolderAccessible { get; set; }
    public bool FtpFolderAccessible { get; set; }
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, string> Details { get; set; } = new();
}
