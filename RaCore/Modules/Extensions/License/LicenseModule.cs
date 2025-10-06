using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.License;

/// <summary>
/// License Management Module - Handles license validation, assignment, and access control.
/// Ensures only SuperAdmin and licensed users can access privileged features.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class LicenseModule : ModuleBase, ILicenseModule
{
    public override string Name => "License";

    private readonly Dictionary<Guid, Abstractions.License> _licenses = new();
    private readonly Dictionary<Guid, UserLicense> _userLicenses = new();
    private readonly object _lock = new();
    
    private IAuthenticationModule? _authModule;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to authentication module
        if (manager is ModuleManager moduleManager)
        {
            _authModule = moduleManager.GetModuleByName("Authentication") as IAuthenticationModule;
        }
        
        LogInfo("License module initialized");
    }

    public override string Process(string input)
    {
        return ProcessInternal(input);
    }

    private string ProcessInternal(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("license stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetLicenseStats();
        }

        if (text.StartsWith("license create", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var instanceName = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "Default Instance";
            return CreateLicense(instanceName);
        }

        if (text.StartsWith("license assign", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: license assign <username> <license-key>";
            }
            return AssignLicenseToUser(parts[2], parts[3]);
        }

        if (text.StartsWith("license validate", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: license validate <username>";
            }
            return ValidateUserLicense(parts[2]);
        }

        if (text.Equals("license prices", StringComparison.OrdinalIgnoreCase))
        {
            return GetLicensePrices();
        }

        return "Unknown license command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "License Management commands:",
            "  license stats              - Show license statistics",
            "  license create [name]      - Create new license",
            "  license assign <user> <key> - Assign license to user",
            "  license validate <user>    - Check user license status",
            "  license prices             - Show license prices in RaCoins",
            "  help                       - Show this help message",
            "",
            "Note: Licenses can be purchased via the SuperMarket module using RaCoins"
        );
    }

    private string GetLicenseStats()
    {
        lock (_lock)
        {
            var stats = new
            {
                TotalLicenses = _licenses.Count,
                ActiveLicenses = _licenses.Values.Count(l => l.Status == LicenseStatus.Active),
                AssignedLicenses = _userLicenses.Count(ul => ul.Value.IsActive),
                ExpiredLicenses = _licenses.Values.Count(l => l.Status == LicenseStatus.Expired),
                Licenses = _licenses.Values.Select(l => new
                {
                    l.Id,
                    l.LicenseKey,
                    l.InstanceName,
                    l.Status,
                    l.Type,
                    l.CreatedAtUtc,
                    l.ExpiresAtUtc
                })
            };
            return JsonSerializer.Serialize(stats, _jsonOptions);
        }
    }

    private string CreateLicense(string instanceName)
    {
        lock (_lock)
        {
            var license = new Abstractions.License
            {
                Id = Guid.NewGuid(),
                LicenseKey = GenerateLicenseKey(),
                InstanceName = instanceName,
                Status = LicenseStatus.Active,
                Type = LicenseType.Standard,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddYears(1), // 1 year default
                MaxUsers = 10
            };

            _licenses[license.Id] = license;
            LogInfo($"License created: {license.LicenseKey} for {instanceName}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "License created successfully",
                License = new
                {
                    license.Id,
                    license.LicenseKey,
                    license.InstanceName,
                    license.Status,
                    license.Type,
                    license.ExpiresAtUtc
                }
            }, _jsonOptions);
        }
    }

    private string AssignLicenseToUser(string username, string licenseKey)
    {
        lock (_lock)
        {
            // Find license
            var license = _licenses.Values.FirstOrDefault(l => 
                l.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase));

            if (license == null)
            {
                return JsonSerializer.Serialize(new { Success = false, Message = "License not found" }, _jsonOptions);
            }

            if (license.Status != LicenseStatus.Active)
            {
                return JsonSerializer.Serialize(new { Success = false, Message = "License is not active" }, _jsonOptions);
            }

            // Create user license assignment (in real implementation, would get user from auth module)
            var userId = Guid.NewGuid(); // Placeholder - would get from auth module
            var userLicense = new UserLicense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LicenseId = license.Id,
                AssignedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            _userLicenses[userLicense.Id] = userLicense;
            LogInfo($"License {licenseKey} assigned to user {username}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = $"License assigned to {username}",
                Assignment = new
                {
                    userLicense.Id,
                    Username = username,
                    LicenseKey = license.LicenseKey,
                    userLicense.AssignedAtUtc
                }
            }, _jsonOptions);
        }
    }

    private string ValidateUserLicense(string username)
    {
        lock (_lock)
        {
            // In real implementation, would lookup user through auth module
            var hasValidLicense = _userLicenses.Values.Any(ul => ul.IsActive);
            
            return JsonSerializer.Serialize(new
            {
                Username = username,
                HasValidLicense = hasValidLicense,
                Message = hasValidLicense ? "User has valid license" : "No valid license found"
            }, _jsonOptions);
        }
    }

    /// <summary>
    /// Check if a user has a valid license for access.
    /// SuperAdmin always returns true.
    /// </summary>
    public bool HasValidLicense(User user)
    {
        if (user == null || !user.IsActive)
            return false;

        // SuperAdmin always has access
        if (user.Role == UserRole.SuperAdmin)
            return true;

        lock (_lock)
        {
            // Check if user has an active license assignment
            var userLicense = _userLicenses.Values.FirstOrDefault(ul => 
                ul.UserId == user.Id && ul.IsActive);

            if (userLicense == null)
                return false;

            // Validate the license itself
            if (_licenses.TryGetValue(userLicense.LicenseId, out var license))
            {
                // Check license status
                if (license.Status != LicenseStatus.Active)
                    return false;

                // Check expiration
                if (license.ExpiresAtUtc.HasValue && license.ExpiresAtUtc.Value <= DateTime.UtcNow)
                {
                    license.Status = LicenseStatus.Expired;
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Get user's license information.
    /// </summary>
    public Abstractions.License? GetUserLicense(Guid userId)
    {
        lock (_lock)
        {
            var userLicense = _userLicenses.Values.FirstOrDefault(ul => 
                ul.UserId == userId && ul.IsActive);

            if (userLicense != null && _licenses.TryGetValue(userLicense.LicenseId, out var license))
            {
                return license;
            }

            return null;
        }
    }

    /// <summary>
    /// Create a license and assign it to a user.
    /// </summary>
    public Abstractions.License CreateAndAssignLicense(Guid userId, string instanceName, LicenseType type, int durationYears = 1)
    {
        lock (_lock)
        {
            var license = new Abstractions.License
            {
                Id = Guid.NewGuid(),
                LicenseKey = GenerateLicenseKey(),
                InstanceName = instanceName,
                Status = LicenseStatus.Active,
                Type = type,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddYears(durationYears),
                MaxUsers = type switch
                {
                    LicenseType.Trial => 1,
                    LicenseType.Standard => 10,
                    LicenseType.Professional => 50,
                    LicenseType.Enterprise => 500,
                    LicenseType.Lifetime => int.MaxValue,
                    _ => 10
                }
            };

            _licenses[license.Id] = license;

            var userLicense = new UserLicense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LicenseId = license.Id,
                AssignedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            _userLicenses[userLicense.Id] = userLicense;
            
            LogInfo($"License created and assigned: {license.LicenseKey} to user {userId}");
            
            return license;
        }
    }

    /// <summary>
    /// Revoke a user's license.
    /// </summary>
    public bool RevokeLicense(Guid userId)
    {
        lock (_lock)
        {
            var userLicense = _userLicenses.Values.FirstOrDefault(ul => 
                ul.UserId == userId && ul.IsActive);

            if (userLicense != null)
            {
                userLicense.IsActive = false;
                
                if (_licenses.TryGetValue(userLicense.LicenseId, out var license))
                {
                    license.Status = LicenseStatus.Revoked;
                }

                LogInfo($"License revoked for user {userId}");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Generate a secure license key.
    /// </summary>
    private string GenerateLicenseKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        var hex = BitConverter.ToString(bytes).Replace("-", "");
        return $"RACORE-{hex.Substring(0, 8)}-{hex.Substring(8, 8)}-{hex.Substring(16, 8)}";
    }

    /// <summary>
    /// Get license pricing information.
    /// </summary>
    private string GetLicensePrices()
    {
        var prices = new
        {
            Message = "License Pricing (in RaCoins)",
            Note = "Purchase licenses through the SuperMarket module",
            Licenses = new[]
            {
                new { Type = "Standard", Price = 100, Duration = "1 year", MaxUsers = 10, Features = "Basic features" },
                new { Type = "Professional", Price = 500, Duration = "1 year", MaxUsers = 50, Features = "Advanced features" },
                new { Type = "Enterprise", Price = 2000, Duration = "1 year", MaxUsers = 500, Features = "Unlimited features" }
            },
            PurchaseInstructions = new[]
            {
                "1. Ensure you have sufficient RaCoins (use 'racoin balance <user-id>')",
                "2. Top up if needed (use 'racoin topup <user-id> <amount>')",
                "3. Browse SuperMarket catalog (use 'market catalog')",
                "4. Purchase license (use 'market buy <user-id> <product-id>')"
            }
        };
        return JsonSerializer.Serialize(prices, _jsonOptions);
    }

    /// <summary>
    /// Log a license validation event.
    /// </summary>
    public async Task LogLicenseEventAsync(Guid userId, string action, string details, bool success)
    {
        if (_authModule != null)
        {
            var eventType = success ? SecurityEventType.LoginSuccess : SecurityEventType.LicenseValidationFailure;
            await _authModule.LogSecurityEventAsync(new SecurityEvent
            {
                Id = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                Type = eventType,
                UserId = userId,
                Details = $"License: {action} - {details}",
                Success = success
            });
        }
    }
    
    /// <summary>
    /// Get all licenses in the system (Admin+).
    /// </summary>
    public IEnumerable<Abstractions.License> GetAllLicenses()
    {
        lock (_lock)
        {
            return _licenses.Values.ToList();
        }
    }
    
    /// <summary>
    /// Create a new license (synonym for CreateAndAssignLicense).
    /// </summary>
    public Abstractions.License CreateLicense(Guid userId, string instanceName, LicenseType type, int durationYears = 1)
    {
        return CreateAndAssignLicense(userId, instanceName, type, durationYears);
    }
}
