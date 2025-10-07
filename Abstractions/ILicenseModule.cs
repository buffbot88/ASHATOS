namespace Abstractions;

/// <summary>
/// Interface for license management functionality.
/// </summary>
public interface ILicenseModule : IDisposable
{
    /// <summary>
    /// Check if a user has a valid license.
    /// </summary>
    bool HasValidLicense(User user);
    
    /// <summary>
    /// Get a user's license information.
    /// </summary>
    License? GetUserLicense(Guid userId);
    
    /// <summary>
    /// Create a license and assign it to a user.
    /// </summary>
    License CreateAndAssignLicense(Guid userId, string instanceName, LicenseType type, int durationYears = 1);
    
    /// <summary>
    /// Revoke a user's license.
    /// </summary>
    bool RevokeLicense(Guid userId);
    
    /// <summary>
    /// Log a license validation event.
    /// </summary>
    Task LogLicenseEventAsync(Guid userId, string action, string details, bool success);
    
    /// <summary>
    /// Get all licenses in the system (Admin+).
    /// </summary>
    IEnumerable<License> GetAllLicenses();
    
    /// <summary>
    /// Create a new license (synonym for CreateAndAssignLicense).
    /// </summary>
    License CreateLicense(Guid userId, string instanceName, LicenseType type, int durationYears = 1);
    
    /// <summary>
    /// Set failsafe password for a license (SuperAdmin only).
    /// </summary>
    bool SetFailsafePassword(Guid licenseId, string passwordHash);
    
    /// <summary>
    /// Validate failsafe password for a license.
    /// </summary>
    bool ValidateFailsafePassword(Guid licenseId, string passwordHash);
    
    /// <summary>
    /// Get the server license (first license with Lifetime type).
    /// </summary>
    License? GetServerLicense();
}
