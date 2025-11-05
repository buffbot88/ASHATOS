using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASHATCore.Endpoints;

/// <summary>
/// Download endpoints for ASHAT Desktop Assistant distribution
/// </summary>
public static class DownloadEndpoints
{
    public static void MapDownloadEndpoints(this WebApplication app)
    {
        var downloads = app.MapGroup("/api/download")
            .WithTags("Downloads");

        // Get download info
        downloads.MapGet("/info", GetDownloadInfo)
            .WithName("GetDownloadInfo")
            .WithSummary("Get information about available downloads");

        // List all packages
        downloads.MapGet("/packages", ListPackages)
            .WithName("ListPackages")
            .WithSummary("List all available download packages");

        // Download specific package
        downloads.MapGet("/{packageId}", DownloadPackage)
            .WithName("DownloadPackage")
            .WithSummary("Download a specific package");

        // Record download statistics
        downloads.MapPost("/record", RecordDownload)
            .WithName("RecordDownload")
            .WithSummary("Record a download for statistics");
    }

    private static IResult GetDownloadInfo()
    {
        var info = new
        {
            title = "ASHAT Desktop Assistant + RaStudios",
            description = "Your AI Roman Goddess Companion with RaStudios Development Platform",
            version = "1.0.0",
            features = new[]
            {
                "✨ Animated Roman goddess character",
                "🎤 Soft female voice synthesis",
                "🤖 AI coding and productivity assistant",
                "👑 Multiple personality modes",
                "🎭 Various animations and expressions",
                "💫 Always-on-desktop companion",
                "🔌 Optional server connection for advanced features",
                "🎮 RaStudios IDE integrated - ASHAT can launch and control RaStudios",
                "🛠️ Complete game development platform included"
            },
            systemRequirements = new
            {
                runtime = ".NET 9.0 or included in standalone build",
                os = "Windows 10+, Linux (Ubuntu 20.04+), or macOS 11+",
                disk = "~500 MB (includes RaStudios)",
                internet = "Optional, for server features"
            },
            packages = new[]
            {
                new { 
                    id = "ashat-goddess-suite-windows", 
                    name = "ASHAT + RaStudios Suite (Windows x64)", 
                    size = "~100 MB",
                    description = "Complete package with ASHAT Goddess and RaStudios IDE"
                },
                new { 
                    id = "ashat-goddess-suite-linux", 
                    name = "ASHAT + RaStudios Suite (Linux x64)", 
                    size = "~100 MB",
                    description = "Complete package with ASHAT Goddess and RaStudios (Python version)"
                },
                new { 
                    id = "ashat-desktop-windows", 
                    name = "ASHAT Only (Windows x64)", 
                    size = "~15 MB",
                    description = "ASHAT Goddess only (without RaStudios)"
                },
                new { 
                    id = "ashat-desktop-linux", 
                    name = "ASHAT Only (Linux x64)", 
                    size = "~15 MB",
                    description = "ASHAT Goddess only (without RaStudios)"
                },
                new { 
                    id = "ashat-desktop-macos", 
                    name = "ASHAT Only (macOS Universal)", 
                    size = "~15 MB",
                    description = "ASHAT Goddess only (without RaStudios)"
                }
            }
        };

        return Results.Json(info);
    }

    private static IResult ListPackages()
    {
        var packagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "packages.json");
        
        if (!File.Exists(packagesPath))
        {
            // Return default packages if file doesn't exist
            var defaultPackages = new[]
            {
                new
                {
                    id = "ashat-goddess-suite-windows",
                    name = "ASHAT + RaStudios Suite (Windows)",
                    version = "1.0.0",
                    platform = "Windows",
                    architecture = "x64",
                    fileSize = "~100 MB",
                    fileName = "ASHAT-RaStudios-Suite-Windows-x64.zip",
                    description = "Complete ASHAT Goddess with RaStudios IDE for Windows",
                    recommended = true
                },
                new
                {
                    id = "ashat-goddess-suite-linux",
                    name = "ASHAT + RaStudios Suite (Linux)",
                    version = "1.0.0",
                    platform = "Linux",
                    architecture = "x64",
                    fileSize = "~100 MB",
                    fileName = "ASHAT-RaStudios-Suite-Linux-x64.tar.gz",
                    description = "Complete ASHAT Goddess with RaStudios (Python version) for Linux",
                    recommended = true
                },
                new
                {
                    id = "ashat-desktop-windows",
                    name = "ASHAT Desktop Assistant (Windows)",
                    version = "1.0.0",
                    platform = "Windows",
                    architecture = "x64",
                    fileSize = "~15 MB",
                    fileName = "ASHATDesktopAssistant.exe",
                    description = "Standalone ASHAT desktop assistant for Windows (without RaStudios)",
                    recommended = false
                },
                new
                {
                    id = "ashat-desktop-linux",
                    name = "ASHAT Desktop Assistant (Linux)",
                    version = "1.0.0",
                    platform = "Linux",
                    architecture = "x64",
                    fileSize = "~15 MB",
                    fileName = "ASHATDesktopAssistant",
                    description = "Standalone ASHAT desktop assistant for Linux (without RaStudios)",
                    recommended = false
                },
                new
                {
                    id = "ashat-desktop-macos",
                    name = "ASHAT Desktop Assistant (macOS)",
                    version = "1.0.0",
                    platform = "macOS",
                    architecture = "x64/arm64",
                    fileSize = "~15 MB",
                    fileName = "ASHATDesktopAssistant",
                    description = "Standalone ASHAT desktop assistant for macOS (without RaStudios)",
                    recommended = false
                }
            };

            return Results.Json(defaultPackages);
        }

        try
        {
            var json = File.ReadAllText(packagesPath);
            var packages = JsonSerializer.Deserialize<object>(json);
            return Results.Json(packages);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to load packages: {ex.Message}");
        }
    }

    private static IResult DownloadPackage(string packageId)
    {
        // Map package IDs to file paths
        var packageFiles = new Dictionary<string, (string fileName, string contentType)>
        {
            ["ashat-goddess-suite-windows"] = ("ASHAT-RaStudios-Suite-Windows-x64.zip", "application/zip"),
            ["ashat-goddess-suite-linux"] = ("ASHAT-RaStudios-Suite-Linux-x64.tar.gz", "application/gzip"),
            ["ashat-desktop-windows"] = ("ASHATDesktopAssistant.exe", "application/octet-stream"),
            ["ashat-desktop-linux"] = ("ASHATDesktopAssistant", "application/octet-stream"),
            ["ashat-desktop-macos"] = ("ASHATDesktopAssistant", "application/octet-stream")
        };

        if (!packageFiles.ContainsKey(packageId))
        {
            return Results.NotFound(new { error = "Package not found", packageId });
        }

        var (fileName, contentType) = packageFiles[packageId];
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", fileName);

        // Check if file exists
        if (!File.Exists(filePath))
        {
            // For now, return a message that the build is needed
            return Results.Ok(new
            {
                message = "Download package not yet built",
                packageId,
                fileName,
                instructions = new[]
                {
                    "The ASHAT Desktop Assistant package needs to be built.",
                    "Run: cd ASHATDesktopClient",
                    $"Run: dotnet publish -c Release -r {GetRuntimeIdentifier(packageId)} --self-contained",
                    "The executable will be in bin/Release/net9.0/[rid]/publish/"
                }
            });
        }

        // Serve the file for download
        var fileBytes = File.ReadAllBytes(filePath);
        return Results.File(fileBytes, contentType, fileName);
    }

    private static IResult RecordDownload([FromBody] DownloadRecordRequest request)
    {
        if (string.IsNullOrEmpty(request.PackageId))
        {
            return Results.BadRequest(new { error = "PackageId is required" });
        }

        var record = new
        {
            id = Guid.NewGuid().ToString(),
            packageId = request.PackageId,
            userId = request.UserId ?? "anonymous",
            timestamp = DateTime.UtcNow,
            platform = request.Platform ?? "unknown"
        };

        // Log the download
        Console.WriteLine($"[Download] {record.packageId} by {record.userId} ({record.platform})");

        // In a real implementation, store in database
        // For now, just return success
        return Results.Ok(new
        {
            message = "Download recorded",
            record
        });
    }

    private static string GetRuntimeIdentifier(string packageId)
    {
        return packageId switch
        {
            "ashat-desktop-windows" => "win-x64",
            "ashat-desktop-linux" => "linux-x64",
            "ashat-desktop-macos" => "osx-x64",
            _ => "win-x64"
        };
    }
}

public record DownloadRecordRequest(
    string PackageId,
    string? UserId,
    string? Platform
);
