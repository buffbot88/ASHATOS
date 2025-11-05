using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.DesktopAssistant;

/// <summary>
/// Download Manager Module for ASHAT Desktop Assistant
/// Handles distribution and downloads of the desktop assistant application
/// </summary>
[RaModule(Category = "extensions")]
public sealed class DownloadManagerModule : ModuleBase
{
    public override string Name => "DownloadManager";

    private readonly ConcurrentDictionary<string, DownloadRecord> _downloadRecords = new();
    private readonly string _downloadPath;
    private DownloadManagerConfig _config = new();

    public DownloadManagerModule()
    {
        _downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
        Directory.CreateDirectory(_downloadPath);
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        LoadConfiguration();
        InitializeDownloadPackages();
        LogInfo("Download Manager Module initialized - ASHAT Desktop Assistant available for download");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "info" => GetDownloadInfo(),
            "stats" => GetDownloadStats(),
            "record" when parts.Length > 1 => RecordDownload(parts[1], parts.Length > 2 ? parts[2] : "unknown"),
            "prepare" => PrepareDownloadPackages(),
            "packages" => ListAvailablePackages(),
            "url" when parts.Length > 1 => GetDownloadUrl(parts[1]),
            "help" => GetHelpText(),
            _ => GetHelpText()
        };
    }

    private void LoadConfiguration()
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "download-manager-config.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<DownloadManagerConfig>(json) ?? new DownloadManagerConfig();
                LogInfo("Download Manager configuration loaded");
            }
            catch (Exception ex)
            {
                LogError($"Failed to load configuration: {ex.Message}");
                _config = new DownloadManagerConfig();
            }
        }
        else
        {
            _config = GetDefaultConfiguration();
            SaveConfiguration();
        }
    }

    private void SaveConfiguration()
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "download-manager-config.json");
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            LogInfo("Download Manager configuration saved");
        }
        catch (Exception ex)
        {
            LogError($"Failed to save configuration: {ex.Message}");
        }
    }

    private DownloadManagerConfig GetDefaultConfiguration()
    {
        return new DownloadManagerConfig
        {
            Version = "1.0.0",
            EnableDownloads = true,
            TrackDownloads = true,
            RequireAuthentication = false,
            MaxDownloadsPerDay = 1000
        };
    }

    private void InitializeDownloadPackages()
    {
        // Create download info JSON
        var packages = new[]
        {
            new DownloadPackage
            {
                Id = "ashat-desktop-windows",
                Name = "ASHAT Desktop Assistant (Windows)",
                Version = "1.0.0",
                Platform = "Windows",
                Architecture = "x64",
                FileSize = "~15 MB",
                FileName = "ASHATDesktopAssistant.exe",
                Description = "Standalone ASHAT desktop assistant for Windows. Features voice synthesis, animations, and AI coding assistance.",
                Requirements = new[] { ".NET 9.0 Runtime or included in standalone build" },
                Features = new[]
                {
                    "✨ Animated Roman goddess character",
                    "🎤 Soft female voice synthesis",
                    "🤖 AI coding and productivity assistant",
                    "👑 Multiple personality modes",
                    "🎭 Various animations and expressions",
                    "💫 Always-on-desktop companion",
                    "🔌 Optional server connection for advanced features"
                }
            },
            new DownloadPackage
            {
                Id = "ashat-desktop-linux",
                Name = "ASHAT Desktop Assistant (Linux)",
                Version = "1.0.0",
                Platform = "Linux",
                Architecture = "x64",
                FileSize = "~15 MB",
                FileName = "ASHATDesktopAssistant",
                Description = "Standalone ASHAT desktop assistant for Linux. Features voice synthesis, animations, and AI coding assistance.",
                Requirements = new[] { ".NET 9.0 Runtime or included in standalone build" },
                Features = new[]
                {
                    "✨ Animated Roman goddess character",
                    "🎤 Soft female voice synthesis",
                    "🤖 AI coding and productivity assistant",
                    "👑 Multiple personality modes",
                    "🎭 Various animations and expressions",
                    "💫 Always-on-desktop companion",
                    "🔌 Optional server connection for advanced features"
                }
            },
            new DownloadPackage
            {
                Id = "ashat-desktop-macos",
                Name = "ASHAT Desktop Assistant (macOS)",
                Version = "1.0.0",
                Platform = "macOS",
                Architecture = "x64/arm64",
                FileSize = "~15 MB",
                FileName = "ASHATDesktopAssistant",
                Description = "Standalone ASHAT desktop assistant for macOS. Features voice synthesis, animations, and AI coding assistance.",
                Requirements = new[] { ".NET 9.0 Runtime or included in standalone build" },
                Features = new[]
                {
                    "✨ Animated Roman goddess character",
                    "🎤 Soft female voice synthesis",
                    "🤖 AI coding and productivity assistant",
                    "👑 Multiple personality modes",
                    "🎭 Various animations and expressions",
                    "💫 Always-on-desktop companion",
                    "🔌 Optional server connection for advanced features"
                }
            }
        };

        var packagesPath = Path.Combine(_downloadPath, "packages.json");
        var json = JsonSerializer.Serialize(packages, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(packagesPath, json);
        LogInfo("Download packages initialized");
    }

    private string PrepareDownloadPackages()
    {
        try
        {
            InitializeDownloadPackages();
            return "✓ Download packages prepared successfully";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to prepare packages: {ex.Message}";
        }
    }

    private string GetDownloadInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ASHAT Desktop Assistant - Download Information ===");
        sb.AppendLine();
        sb.AppendLine("📥 Available for Download:");
        sb.AppendLine("  • Windows x64 (ASHATDesktopAssistant.exe)");
        sb.AppendLine("  • Linux x64 (ASHATDesktopAssistant)");
        sb.AppendLine("  • macOS x64/ARM64 (ASHATDesktopAssistant)");
        sb.AppendLine();
        sb.AppendLine("✨ Features:");
        sb.AppendLine("  • Animated Roman goddess character on your desktop");
        sb.AppendLine("  • Soft feminine voice for pleasant interaction");
        sb.AppendLine("  • AI-powered coding and productivity assistance");
        sb.AppendLine("  • Customizable personality and appearance");
        sb.AppendLine("  • Standalone mode or server-connected");
        sb.AppendLine();
        sb.AppendLine("📋 System Requirements:");
        sb.AppendLine("  • .NET 9.0 Runtime (or use standalone build)");
        sb.AppendLine("  • Windows 10+, Linux (Ubuntu 20.04+), or macOS 11+");
        sb.AppendLine("  • ~50 MB disk space");
        sb.AppendLine("  • Internet connection (optional, for server features)");
        sb.AppendLine();
        sb.AppendLine("🔗 Download URLs:");
        sb.AppendLine("  • Web: http://your-server/downloads/ashat-desktop");
        sb.AppendLine("  • Direct: http://your-server/api/download/ashat-desktop-windows");
        sb.AppendLine();
        sb.AppendLine($"📊 Version: {_config.Version}");
        sb.AppendLine($"✓ Downloads Enabled: {_config.EnableDownloads}");

        return sb.ToString();
    }

    private string GetDownloadStats()
    {
        var totalDownloads = _downloadRecords.Count;
        var todayDownloads = _downloadRecords.Values.Count(r => r.Timestamp.Date == DateTime.UtcNow.Date);
        var platformStats = _downloadRecords.Values
            .GroupBy(r => r.Platform)
            .Select(g => $"  • {g.Key}: {g.Count()} downloads")
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== Download Statistics ===");
        sb.AppendLine($"📊 Total Downloads: {totalDownloads}");
        sb.AppendLine($"📅 Today: {todayDownloads}");
        sb.AppendLine();
        sb.AppendLine("By Platform:");
        foreach (var stat in platformStats)
        {
            sb.AppendLine(stat);
        }

        return sb.ToString();
    }

    private string RecordDownload(string platform, string userId)
    {
        var record = new DownloadRecord
        {
            Id = Guid.NewGuid().ToString(),
            Platform = platform,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Version = _config.Version
        };

        _downloadRecords[record.Id] = record;
        LogInfo($"Download recorded: {platform} by {userId}");

        return $"✓ Download recorded: {record.Id}";
    }

    private string ListAvailablePackages()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Available ASHAT Desktop Packages ===");
        sb.AppendLine();
        sb.AppendLine("1. Windows x64");
        sb.AppendLine("   • File: ASHATDesktopAssistant.exe");
        sb.AppendLine("   • Size: ~15 MB");
        sb.AppendLine("   • ID: ashat-desktop-windows");
        sb.AppendLine();
        sb.AppendLine("2. Linux x64");
        sb.AppendLine("   • File: ASHATDesktopAssistant");
        sb.AppendLine("   • Size: ~15 MB");
        sb.AppendLine("   • ID: ashat-desktop-linux");
        sb.AppendLine();
        sb.AppendLine("3. macOS Universal");
        sb.AppendLine("   • File: ASHATDesktopAssistant");
        sb.AppendLine("   • Size: ~15 MB");
        sb.AppendLine("   • ID: ashat-desktop-macos");
        sb.AppendLine();
        sb.AppendLine("💡 Use 'download url <id>' to get download link");

        return sb.ToString();
    }

    private string GetDownloadUrl(string packageId)
    {
        var baseUrl = "http://localhost:80"; // This should be configurable
        return $"📥 Download URL: {baseUrl}/api/download/{packageId}";
    }

    private string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Download Manager Commands ===");
        sb.AppendLine();
        sb.AppendLine("Information:");
        sb.AppendLine("  download info                    - Show download information");
        sb.AppendLine("  download stats                   - Show download statistics");
        sb.AppendLine("  download packages                - List available packages");
        sb.AppendLine();
        sb.AppendLine("Management:");
        sb.AppendLine("  download prepare                 - Prepare download packages");
        sb.AppendLine("  download record <platform> <user> - Record a download");
        sb.AppendLine("  download url <packageId>         - Get download URL");
        sb.AppendLine();
        sb.AppendLine("💡 Example: download info");

        return sb.ToString();
    }
}

public class DownloadManagerConfig
{
    public string Version { get; set; } = "1.0.0";
    public bool EnableDownloads { get; set; } = true;
    public bool TrackDownloads { get; set; } = true;
    public bool RequireAuthentication { get; set; } = false;
    public int MaxDownloadsPerDay { get; set; } = 1000;
}

public class DownloadPackage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Requirements { get; set; } = Array.Empty<string>();
    public string[] Features { get; set; } = Array.Empty<string>();
}

public class DownloadRecord
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}
