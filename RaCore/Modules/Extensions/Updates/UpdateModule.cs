using System.Security.Cryptography;
using System.Text.Json;
using System.IO.Compression;
using Abstractions;

namespace RaCore.Modules.Extensions.Updates;

/// <summary>
/// Update Module - Manages version checking and update delivery from mainframe.
/// Handles update package creation, distribution, and verification.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class UpdateModule : ModuleBase, IUpdateModule
{
    public override string Name => "Update";

    private readonly Dictionary<string, UpdatePackage> _updates = new();
    private readonly object _lock = new();
    private readonly string _updatesPath;
    
    private ILicenseModule? _licenseModule;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private const string CurrentVersion = "4.6.0"; // Phase 4.6 completed

    public UpdateModule()
    {
        _updatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Updates", "Packages");
        Directory.CreateDirectory(_updatesPath);
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to license module
        if (manager is RaCore.Engine.Manager.ModuleManager moduleManager)
        {
            _licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;
        }
        
        LogInfo("Update module initialized");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("update stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("update check", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: update check <current-version> <license-key>";
            }
            var task = CheckForUpdatesAsync(parts[2], parts[3]);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.Equals("update list", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(GetAllUpdates(), _jsonOptions);
        }

        return "Unknown update command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Update Management commands:",
            "  update stats                          - Show update statistics",
            "  update check <version> <license>      - Check for available updates",
            "  update list                           - List all available updates",
            "",
            "The Update module manages mainframe update delivery."
        );
    }

    private string GetStats()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(new
            {
                CurrentVersion,
                TotalUpdates = _updates.Count,
                ActiveUpdates = _updates.Values.Count(u => u.Status == UpdateStatus.Active),
                DeprecatedUpdates = _updates.Values.Count(u => u.Status == UpdateStatus.Deprecated),
                LatestVersion = _updates.Values
                    .Where(u => u.Status == UpdateStatus.Active)
                    .OrderByDescending(u => u.Version)
                    .FirstOrDefault()?.Version ?? CurrentVersion
            }, _jsonOptions);
        }
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(string currentVersion, string licenseKey)
    {
        // Verify license is valid
        if (_licenseModule != null)
        {
            var license = _licenseModule.GetAllLicenses()
                .FirstOrDefault(l => l.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase));
            
            if (license == null || license.Status != LicenseStatus.Active)
            {
                throw new InvalidOperationException("Invalid or inactive license - updates require active license");
            }
        }

        lock (_lock)
        {
            var latestUpdate = _updates.Values
                .Where(u => u.Status == UpdateStatus.Active)
                .OrderByDescending(u => u.Version)
                .FirstOrDefault();

            if (latestUpdate == null)
            {
                return new UpdateInfo
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = CurrentVersion,
                    UpdateAvailable = false,
                    Changelog = "No updates available",
                    DownloadUrl = string.Empty,
                    SizeBytes = 0,
                    ReleasedAt = DateTime.UtcNow
                };
            }

            var isNewer = CompareVersions(latestUpdate.Version, currentVersion) > 0;

            return new UpdateInfo
            {
                CurrentVersion = currentVersion,
                LatestVersion = latestUpdate.Version,
                UpdateAvailable = isNewer,
                Changelog = latestUpdate.Changelog,
                DownloadUrl = isNewer ? $"/api/updates/download/{latestUpdate.Version}" : string.Empty,
                SizeBytes = latestUpdate.SizeBytes,
                ReleasedAt = latestUpdate.CreatedAt
            };
        }
    }

    public async Task<UpdatePackage> CreateUpdatePackageAsync(string version, string changelog, byte[] packageData)
    {
        var package = new UpdatePackage
        {
            Id = Guid.NewGuid(),
            Version = version,
            Changelog = changelog,
            CreatedAt = DateTime.UtcNow,
            Status = UpdateStatus.Active,
            IsMandatory = false
        };

        // Save package file
        var packageFileName = $"RaCore_Update_{version}_{package.Id}.zip";
        var packagePath = Path.Combine(_updatesPath, packageFileName);
        
        await File.WriteAllBytesAsync(packagePath, packageData);
        
        package.PackagePath = packagePath;
        package.SizeBytes = packageData.Length;
        package.ChecksumSHA256 = await ComputeChecksumAsync(packageData);

        lock (_lock)
        {
            _updates[version] = package;
        }

        LogInfo($"Created update package for version {version}");
        return package;
    }

    public UpdatePackage? GetUpdatePackage(string version)
    {
        lock (_lock)
        {
            return _updates.TryGetValue(version, out var package) ? package : null;
        }
    }

    public IEnumerable<UpdatePackage> GetAllUpdates()
    {
        lock (_lock)
        {
            return _updates.Values.OrderByDescending(u => u.Version).ToList();
        }
    }

    public bool VerifyPackageIntegrity(string version, string checksum)
    {
        lock (_lock)
        {
            if (_updates.TryGetValue(version, out var package))
            {
                return package.ChecksumSHA256.Equals(checksum, StringComparison.OrdinalIgnoreCase);
            }
        }
        
        return false;
    }

    private async Task<string> ComputeChecksumAsync(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToHexString(hash);
    }

    private int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
        var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
        
        for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
        {
            var v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            var v2Part = i < v2Parts.Length ? v2Parts[i] : 0;
            
            if (v1Part > v2Part) return 1;
            if (v1Part < v2Part) return -1;
        }
        
        return 0;
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
