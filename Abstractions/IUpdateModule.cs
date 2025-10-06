namespace Abstractions;

/// <summary>
/// Interface for update management functionality.
/// Handles version checking and update delivery from mainframe.
/// </summary>
public interface IUpdateModule : IDisposable
{
    /// <summary>
    /// Check for available updates.
    /// </summary>
    Task<UpdateInfo?> CheckForUpdatesAsync(string currentVersion, string licenseKey);
    
    /// <summary>
    /// Create an update package.
    /// </summary>
    Task<UpdatePackage> CreateUpdatePackageAsync(string version, string changelog, byte[] packageData);
    
    /// <summary>
    /// Get update package by version.
    /// </summary>
    UpdatePackage? GetUpdatePackage(string version);
    
    /// <summary>
    /// Get all available updates.
    /// </summary>
    IEnumerable<UpdatePackage> GetAllUpdates();
    
    /// <summary>
    /// Verify update package integrity.
    /// </summary>
    bool VerifyPackageIntegrity(string version, string checksum);
}

/// <summary>
/// Update information returned to clients.
/// </summary>
public class UpdateInfo
{
    public string LatestVersion { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public bool UpdateAvailable { get; set; }
    public string Changelog { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime ReleasedAt { get; set; }
}

/// <summary>
/// Represents an update package.
/// </summary>
public class UpdatePackage
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ChecksumSHA256 { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public UpdateStatus Status { get; set; }
}

/// <summary>
/// Update status enumeration.
/// </summary>
public enum UpdateStatus
{
    Active,
    Deprecated,
    Revoked
}
