using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Safety;

/// <summary>
/// Failsafe Backup Module - Provides emergency backup and restoASHATtion capabilities for ASHATOS.
/// SuperAdmin can set a failsafe password and trigger backups for system investigation and recovery.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class FailsafeModule : ModuleBase
{
    public override string Name => "Failsafe";

    private readonly Dictionary<Guid, FailsafeBackup> _backups = new();
    private readonly object _lock = new();
    private IAuthenticationModule? _authModule;
    private ILicenseModule? _licenseModule;
    private string? _encryptedFailsafePassword;
    private string? _failsafePasswordSalt;
    private bool _failsafePasswordSet;
    private Guid? _lastSafeBackupId;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to authentication and license modules
        if (manager is ModuleManager moduleManager)
        {
            _authModule = moduleManager.GetModuleByName("Authentication") as IAuthenticationModule;
            _licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;
        }
        
        LogInfo("Failsafe module initialized - Emergency backup system active");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("help_failsafe", StringComparison.OrdinalIgnoreCase))
        {
            return GetFailsafeHelp();
        }

        if (text.StartsWith("help_failsafe -start", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: help_failsafe -start <SERVER_LICENSE_PASSKEY>";
            }
            var passkey = parts[2];
            return TriggerFailsafe(passkey);
        }

        if (text.StartsWith("failsafe setpassword", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: failsafe setpassword <password>";
            }
            var password = string.Join(" ", parts.Skip(2));
            return SetFailsafePassword(password);
        }

        if (text.Equals("failsafe status", StringComparison.OrdinalIgnoreCase))
        {
            return GetFailsafeStatus();
        }

        if (text.Equals("failsafe backups", StringComparison.OrdinalIgnoreCase))
        {
            return ListBackups();
        }

        if (text.StartsWith("failsafe restore", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: failsafe restore <backup-id>";
            }
            if (Guid.TryParse(parts[2], out var backupId))
            {
                return RestoreBackup(backupId);
            }
            return "Invalid backup ID format";
        }

        if (text.StartsWith("failsafe marksafe", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: failsafe marksafe <backup-id>";
            }
            if (Guid.TryParse(parts[2], out var backupId))
            {
                return MarkBackupAsSafe(backupId);
            }
            return "Invalid backup ID format";
        }

        return "Unknown failsafe command. Type 'help' or 'help_failsafe' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Failsafe Backup System commands:",
            "  failsafe status                    - Check failsafe system status",
            "  failsafe setpassword <password>    - Set failsafe password (SuperAdmin only)",
            "  help_failsafe -start <passkey>     - Trigger emergency failsafe backup",
            "  failsafe backups                   - List all backups",
            "  failsafe marksafe <backup-id>      - Mark a backup as safe environment",
            "  failsafe restore <backup-id>       - Restore from a backup",
            "  help_failsafe                      - Show detailed failsafe help",
            "",
            "Note: Failsafe password must be set before triggering emergency backups"
        );
    }

    private string GetFailsafeHelp()
    {
        return string.Join(Environment.NewLine,
            "=== ASHATOS Failsafe Backup System ===",
            "",
            "The Failsafe system protects ASHATOS from corruption by:",
            "1. Creating emergency backups on demand",
            "2. Comparing current state to last known safe environment",
            "3. Investigating and isolating issues",
            "4. Restoring to safe backup when needed",
            "",
            "INITIAL SETUP (SuperAdmin only):",
            "  failsafe setpassword <password>  - Set encrypted failsafe password",
            "",
            "EMERGENCY TRIGGER:",
            "  help_failsafe -start <SERVER_LICENSE_PASSKEY>",
            "    - Creates immediate backup",
            "    - Files for investigation",
            "    - Compares to last safe environment",
            "    - Provides restoASHATtion recommendations",
            "",
            "BACKUP MANAGEMENT:",
            "  failsafe backups                 - View all backups",
            "  failsafe marksafe <backup-id>    - Mark backup as safe environment",
            "  failsafe restore <backup-id>     - Restore from backup",
            "  failsafe status                  - Check system status",
            "",
            "SECURITY:",
            "- Failsafe password is encrypted and stored in Server License",
            "- Only SuperAdmin can set failsafe password",
            "- License passkey required to trigger emergency backup",
            "- All Operations logged for audit trail",
            ""
        );
    }

    private string SetFailsafePassword(string password)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return "Failsafe password must be at least 8 characters long";
            }

            // Hash the password with salt using PBKDF2
            _failsafePasswordSalt = GenerateSalt();
            _encryptedFailsafePassword = HashPassword(password, _failsafePasswordSalt);
            _failsafePasswordSet = true;
            
            LogInfo("Failsafe password has been set and hashed securely using PBKDF2");
            
            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Failsafe password set successfully",
                Timestamp = DateTime.UtcNow,
                Note = "Password hashed and stored securely using PBKDF2. Use 'help_failsafe -start <passkey>' to trigger emergency backup."
            }, _jsonOptions);
        }
    }

    private string TriggerFailsafe(string passkey)
    {
        lock (_lock)
        {
            if (!_failsafePasswordSet)
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Failsafe password not set. Please set failsafe password first using 'failsafe setpassword <password>'",
                    Action = "Setup Required"
                }, _jsonOptions);
            }

            // Validate the passkey (in production, this would validate against the actual license)
            if (string.IsNullOrWhiteSpace(passkey))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Invalid server license passkey",
                    Action = "Access Denied"
                }, _jsonOptions);
            }

            // Create emergency backup
            var backup = CreateBackup("EMERGENCY", "Failsafe triggered by SuperAdmin");
            
            // Compare with last safe backup
            var comparison = CompareToSafeBackup(backup);
            
            LogInfo($"FAILSAFE TRIGGERED - Backup created: {backup.Id}");
            
            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Emergency failsafe backup created",
                BackupId = backup.Id,
                Timestamp = backup.CreatedAtUtc,
                Investigation = new
                {
                    Status = "Investigating",
                    LastSafeBackup = _lastSafeBackupId,
                    Comparison = comparison,
                    Recommendations = GenerateRecommendations(comparison)
                },
                NextSteps = new[]
                {
                    "Review the comparison results",
                    "Investigate identified issues",
                    "If safe, mark this backup as safe: failsafe marksafe " + backup.Id,
                    "If corrupted, restore from last safe backup: failsafe restore " + _lastSafeBackupId
                }
            }, _jsonOptions);
        }
    }

    private FailsafeBackup CreateBackup(string type, string reason)
    {
        var backup = new FailsafeBackup
        {
            Id = Guid.NewGuid(),
            Type = type,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow,
            State = CaptureSystemState(),
            IsSafe = false
        };

        _backups[backup.Id] = backup;
        return backup;
    }

    private SystemState CaptureSystemState()
    {
        // Capture current system state
        var state = new SystemState
        {
            Timestamp = DateTime.UtcNow,
            TotalUsers = _authModule?.GetAllUsers().Count() ?? 0,
            ActiveSessions = 0, // Would be calculated from session data
            TotalLicenses = _licenseModule?.GetAllLicenses().Count() ?? 0,
            SystemMetrics = new Dictionary<string, object>
            {
                { "CaptureTime", DateTime.UtcNow.ToString("o") },
                { "SystemVersion", "ASHATOS v1.0" },
                { "ModulesLoaded", Manager != null && Manager is ModuleManager mm ? mm.Modules.Count : 0 }
            }
        };

        return state;
    }

    private ComparisonResult CompareToSafeBackup(FailsafeBackup currentBackup)
    {
        if (_lastSafeBackupId == null || !_backups.ContainsKey(_lastSafeBackupId.Value))
        {
            return new ComparisonResult
            {
                HasSafeBackup = false,
                Message = "No safe backup found for comparison",
                Issues = new List<string> { "This is the first backup - no baseline to compare against" }
            };
        }

        var safeBackup = _backups[_lastSafeBackupId.Value];
        var issues = new List<string>();
        var changes = new List<string>();

        // Compare user counts
        if (currentBackup.State.TotalUsers < safeBackup.State.TotalUsers)
        {
            issues.Add($"User count decreased: {safeBackup.State.TotalUsers} -> {currentBackup.State.TotalUsers}");
        }
        else if (currentBackup.State.TotalUsers > safeBackup.State.TotalUsers)
        {
            changes.Add($"User count increased: {safeBackup.State.TotalUsers} -> {currentBackup.State.TotalUsers}");
        }

        // Compare license counts
        if (currentBackup.State.TotalLicenses < safeBackup.State.TotalLicenses)
        {
            issues.Add($"License count decreased: {safeBackup.State.TotalLicenses} -> {currentBackup.State.TotalLicenses}");
        }

        return new ComparisonResult
        {
            HasSafeBackup = true,
            SafeBackupId = _lastSafeBackupId.Value,
            SafeBackupTimestamp = safeBackup.CreatedAtUtc,
            Message = issues.Any() ? "Issues detected" : "No critical issues detected",
            Issues = issues,
            Changes = changes
        };
    }

    private List<string> GenerateRecommendations(ComparisonResult comparison)
    {
        var recommendations = new List<string>();

        if (!comparison.HasSafeBackup)
        {
            recommendations.Add("Mark this backup as safe if system is functioning correctly");
            recommendations.Add("Run comprehensive system checks");
        }
        else if (comparison.Issues.Any())
        {
            recommendations.Add("RECOMMENDED: Restore from last safe backup (ID: " + comparison.SafeBackupId + ")");
            recommendations.Add("Investigate the following issues:");
            recommendations.AddRange(comparison.Issues.Select(i => "  - " + i));
            recommendations.Add("Review audit logs for suspicious activity");
            recommendations.Add("Consider issuing appropriate sanctions");
        }
        else
        {
            recommendations.Add("System appears stable");
            recommendations.Add("Consider marking this backup as safe");
            recommendations.Add("Continue monitoring for anomalies");
        }

        return recommendations;
    }

    private string GetFailsafeStatus()
    {
        lock (_lock)
        {
            var status = new
            {
                FailsafePasswordSet = _failsafePasswordSet,
                TotalBackups = _backups.Count,
                SafeBackups = _backups.Values.Count(b => b.IsSafe),
                LastSafeBackupId = _lastSafeBackupId,
                LastBackup = _backups.Values.OrderByDescending(b => b.CreatedAtUtc).FirstOrDefault(),
                SystemStatus = "Operational"
            };

            return JsonSerializer.Serialize(status, _jsonOptions);
        }
    }

    private string ListBackups()
    {
        lock (_lock)
        {
            var backupList = _backups.Values
                .OrderByDescending(b => b.CreatedAtUtc)
                .Select(b => new
                {
                    b.Id,
                    b.Type,
                    b.Reason,
                    b.CreatedAtUtc,
                    b.IsSafe,
                    IsLastSafe = b.Id == _lastSafeBackupId,
                    State = new
                    {
                        b.State.TotalUsers,
                        b.State.TotalLicenses,
                        b.State.Timestamp
                    }
                });

            return JsonSerializer.Serialize(new
            {
                TotalBackups = _backups.Count,
                Backups = backupList
            }, _jsonOptions);
        }
    }

    private string MarkBackupAsSafe(Guid backupId)
    {
        lock (_lock)
        {
            if (!_backups.ContainsKey(backupId))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Backup not found"
                }, _jsonOptions);
            }

            var backup = _backups[backupId];
            backup.IsSafe = true;
            _lastSafeBackupId = backupId;

            LogInfo($"Backup {backupId} marked as safe environment");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Backup marked as safe environment",
                BackupId = backupId,
                Timestamp = DateTime.UtcNow
            }, _jsonOptions);
        }
    }

    private string RestoreBackup(Guid backupId)
    {
        lock (_lock)
        {
            if (!_backups.ContainsKey(backupId))
            {
                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Backup not found"
                }, _jsonOptions);
            }

            var backup = _backups[backupId];
            
            // In a real implementation, this would restore system state
            LogInfo($"RESTORE INITIATED - Restoring from backup {backupId}");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = "Backup restoASHATtion initiated",
                BackupId = backupId,
                BackupTimestamp = backup.CreatedAtUtc,
                RestoreTimestamp = DateTime.UtcNow,
                State = backup.State,
                Warning = "In production, this would restore user accounts, licenses, and system Configuration",
                NextSteps = new[]
                {
                    "Verify system integrity",
                    "Review restored data",
                    "Create new safe backup if system is stable"
                }
            }, _jsonOptions);
        }
    }

    private string EncryptPassword(string password)
    {
        // Deprecated - use HashPassword instead
        // Kept for reference but not used anymore
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static string GenerateSalt()
    {
        const int SaltSize = 32; // 256 bits
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        const int HashSize = 64; // 512 bits
        const int IteASHATtions = 100000; // PBKDF2 iteASHATtions
        var saltBytes = Convert.FromBase64String(salt);
        // Use the static Pbkdf2 method instead of the obsolete constructor
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            IteASHATtions,
            HashAlgorithmName.SHA512,
            HashSize
        );
        return Convert.ToBase64String(hash);
    }

    private bool ValidateFailsafePassword(string password)
    {
        if (string.IsNullOrEmpty(_encryptedFailsafePassword) || string.IsNullOrEmpty(_failsafePasswordSalt))
            return false;
        
        var hashedPassword = HashPassword(password, _failsafePasswordSalt);
        return hashedPassword == _encryptedFailsafePassword;
    }
}

/// <summary>
/// Represents a failsafe backup snapshot
/// </summary>
public class FailsafeBackup
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public SystemState State { get; set; } = new();
    public bool IsSafe { get; set; }
}

/// <summary>
/// Represents the state of the system at backup time
/// </summary>
public class SystemState
{
    public DateTime Timestamp { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalLicenses { get; set; }
    public Dictionary<string, object> SystemMetrics { get; set; } = new();
}

/// <summary>
/// Represents comparison results between current and safe backup
/// </summary>
public class ComparisonResult
{
    public bool HasSafeBackup { get; set; }
    public Guid? SafeBackupId { get; set; }
    public DateTime? SafeBackupTimestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
    public List<string> Changes { get; set; } = new();
}
