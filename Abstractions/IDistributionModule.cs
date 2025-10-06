namespace Abstractions;

/// <summary>
/// Interface for distribution management functionality.
/// Handles authorized copy distribution and license-based downloads.
/// </summary>
public interface IDistributionModule : IDisposable
{
    /// <summary>
    /// Create a distribution package for a licensed user.
    /// </summary>
    Task<DistributionPackage> CreatePackageAsync(Guid userId, string licenseKey, string version);
    
    /// <summary>
    /// Get download URL for an authorized license.
    /// </summary>
    Task<string?> GetDownloadUrlAsync(string licenseKey);
    
    /// <summary>
    /// Verify if a license is authorized to download.
    /// </summary>
    bool IsAuthorizedForDownload(string licenseKey);
    
    /// <summary>
    /// Get all distribution packages (Admin+).
    /// </summary>
    IEnumerable<DistributionPackage> GetAllPackages();
    
    /// <summary>
    /// Revoke download access for a license.
    /// </summary>
    bool RevokeAccess(string licenseKey);
}

/// <summary>
/// Represents a distribution package.
/// </summary>
public class DistributionPackage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int DownloadCount { get; set; }
    public string ChecksumSHA256 { get; set; } = string.Empty;
    public PackageStatus Status { get; set; }
}

/// <summary>
/// Package status enumeration.
/// </summary>
public enum PackageStatus
{
    Active,
    Expired,
    Revoked,
    Downloaded
}
