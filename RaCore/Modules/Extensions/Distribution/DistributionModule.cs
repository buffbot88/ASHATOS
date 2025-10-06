using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Abstractions;

namespace RaCore.Modules.Extensions.Distribution;

/// <summary>
/// Distribution Module - Manages authorized copy distribution and license-based downloads.
/// Handles packaging, version tracking, and download authorization.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class DistributionModule : ModuleBase, IDistributionModule
{
    public override string Name => "Distribution";

    private readonly Dictionary<Guid, DistributionPackage> _packages = new();
    private readonly Dictionary<string, Guid> _licenseToPackage = new();
    private readonly object _lock = new();
    private readonly string _packagesPath;
    
    private ILicenseModule? _licenseModule;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public DistributionModule()
    {
        _packagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Distribution", "Packages");
        Directory.CreateDirectory(_packagesPath);
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to license module
        if (manager is RaCore.Engine.Manager.ModuleManager moduleManager)
        {
            _licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;
        }
        
        LogInfo("Distribution module initialized");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("distribution stats", StringComparison.OrdinalIgnoreCase))
        {
            return GetStats();
        }

        if (text.StartsWith("distribution create", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: distribution create <license-key> <version>";
            }
            var task = CreatePackageAsync(Guid.NewGuid(), parts[2], parts[3]);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("distribution authorize", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: distribution authorize <license-key>";
            }
            return IsAuthorizedForDownload(parts[2]).ToString();
        }

        return "Unknown distribution command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Distribution Management commands:",
            "  distribution stats                    - Show distribution statistics",
            "  distribution create <license> <ver>   - Create distribution package",
            "  distribution authorize <license>      - Check download authorization",
            "",
            "The Distribution module manages authorized copies for licensed users."
        );
    }

    private string GetStats()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(new
            {
                TotalPackages = _packages.Count,
                ActivePackages = _packages.Values.Count(p => p.Status == PackageStatus.Active),
                ExpiredPackages = _packages.Values.Count(p => p.Status == PackageStatus.Expired),
                RevokedPackages = _packages.Values.Count(p => p.Status == PackageStatus.Revoked),
                TotalDownloads = _packages.Values.Sum(p => p.DownloadCount),
                TotalSizeBytes = _packages.Values.Sum(p => p.SizeBytes)
            }, _jsonOptions);
        }
    }

    public async Task<DistributionPackage> CreatePackageAsync(Guid userId, string licenseKey, string version)
    {
        // Verify license is valid
        if (_licenseModule != null)
        {
            var license = _licenseModule.GetAllLicenses()
                .FirstOrDefault(l => l.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase));
            
            if (license == null || license.Status != LicenseStatus.Active)
            {
                throw new InvalidOperationException("Invalid or inactive license");
            }
        }

        var package = new DistributionPackage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LicenseKey = licenseKey,
            Version = version,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1), // 1 year expiration
            Status = PackageStatus.Active,
            DownloadCount = 0
        };

        // Create package file
        var packageFileName = $"RaCore_{licenseKey}_{version}_{package.Id}.zip";
        var packagePath = Path.Combine(_packagesPath, packageFileName);
        
        await CreatePackageFileAsync(packagePath, licenseKey, version);
        
        package.PackagePath = packagePath;
        package.SizeBytes = new FileInfo(packagePath).Length;
        package.ChecksumSHA256 = await ComputeChecksumAsync(packagePath);

        lock (_lock)
        {
            _packages[package.Id] = package;
            _licenseToPackage[licenseKey] = package.Id;
        }

        LogInfo($"Created distribution package for license {licenseKey}, version {version}");
        return package;
    }

    private async Task CreatePackageFileAsync(string packagePath, string licenseKey, string version)
    {
        // Create a temporary directory for package contents
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create package metadata
            var metadata = new
            {
                LicenseKey = licenseKey,
                Version = version,
                CreatedAt = DateTime.UtcNow,
                ProductName = "RaCore AI Mainframe",
                Description = "Authorized RaCore installation package"
            };

            var metadataPath = Path.Combine(tempDir, "package.json");
            await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, _jsonOptions));

            // Create README
            var readme = $@"# RaCore AI Mainframe - Licensed Copy

License Key: {licenseKey}
Version: {version}
Created: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

This is an authorized copy of RaCore. Do not share or redistribute.
License validation is required for all operations.

## Installation

1. Extract this package
2. Run: dotnet restore
3. Run: dotnet run
4. Access at: http://localhost:5000

Your license key will be validated on startup.

---
RaCore AI Mainframe - $20/year with updates from the Mainframe
";
            await File.WriteAllTextAsync(Path.Combine(tempDir, "README.md"), readme);

            // Create license file
            var licenseFile = Path.Combine(tempDir, "LICENSE.txt");
            await File.WriteAllTextAsync(licenseFile, $"Licensed to: {licenseKey}\nExpires: {DateTime.UtcNow.AddYears(1):yyyy-MM-dd}\n\nRaCore AI Mainframe License Agreement\n\nThis software is licensed, not sold. You may not:\n- Remove license validation code\n- Redistribute this software\n- Share your license key\n\nViolation will result in permanent ban from the Mainframe.");

            // Create the ZIP package
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
            
            ZipFile.CreateFromDirectory(tempDir, packagePath, CompressionLevel.Optimal, false);
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private async Task<string> ComputeChecksumAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }

    public async Task<string?> GetDownloadUrlAsync(string licenseKey)
    {
        lock (_lock)
        {
            if (_licenseToPackage.TryGetValue(licenseKey, out var packageId))
            {
                if (_packages.TryGetValue(packageId, out var package))
                {
                    if (package.Status == PackageStatus.Active && 
                        package.ExpiresAt > DateTime.UtcNow)
                    {
                        package.DownloadCount++;
                        return $"/api/distribution/download/{licenseKey}";
                    }
                }
            }
        }
        
        return null;
    }

    public bool IsAuthorizedForDownload(string licenseKey)
    {
        lock (_lock)
        {
            if (_licenseToPackage.TryGetValue(licenseKey, out var packageId))
            {
                if (_packages.TryGetValue(packageId, out var package))
                {
                    return package.Status == PackageStatus.Active && 
                           package.ExpiresAt > DateTime.UtcNow;
                }
            }
        }
        
        return false;
    }

    public IEnumerable<DistributionPackage> GetAllPackages()
    {
        lock (_lock)
        {
            return _packages.Values.ToList();
        }
    }

    public bool RevokeAccess(string licenseKey)
    {
        lock (_lock)
        {
            if (_licenseToPackage.TryGetValue(licenseKey, out var packageId))
            {
                if (_packages.TryGetValue(packageId, out var package))
                {
                    package.Status = PackageStatus.Revoked;
                    LogInfo($"Revoked distribution access for license {licenseKey}");
                    return true;
                }
            }
        }
        
        return false;
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
