using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Handles PHP runtime detection and validation.
/// </summary>
public class PhpDetector
{
    private readonly SiteBuilderModule _module;
    private string? _cachedPhpPath;

    public PhpDetector(SiteBuilderModule module)
    {
        _module = module;
    }

    public string DetectPHP()
    {
        try
        {
            _cachedPhpPath = FindPhpExecutable();
            
            if (_cachedPhpPath == null)
            {
                return GetPhpNotFoundMessage();
            }

            var version = GetPhpVersion(_cachedPhpPath);
            return $"PHP found: {_cachedPhpPath}\nVersion: {version}";
        }
        catch (Exception ex)
        {
            _module.Log($"PHP detection error: {ex.Message}", "ERROR");
            return $"Error detecting PHP: {ex.Message}";
        }
    }

    public string? FindPhpExecutable()
    {
        if (_cachedPhpPath != null)
        {
            return _cachedPhpPath;
        }

        // Try local php folder first (same directory as RaCore.exe server root)
        var serverRoot = Directory.GetCurrentDirectory();
        var localPhpFolder = Path.Combine(serverRoot, "php");
        
        // Build list of possible paths
        var possiblePaths = new List<string>
        {
            Path.Combine(localPhpFolder, "php.exe"),     // Local Windows
            Path.Combine(localPhpFolder, "php"),         // Local Linux/macOS
            "php",                                        // In PATH
            "/usr/bin/php",                               // Linux
            "/usr/local/bin/php",                         // Linux/macOS
        };
        
        // Add Windows-specific paths with multiple drive letters
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var driveLetters = new[] { "C", "D", "E", "F" };
            var phpPaths = new[]
            {
                @"\php\php.exe",
                @"\php8\php.exe",
                @"\xampp\php\php.exe",
                @"\Program Files\php\php.exe"
            };
            
            foreach (var drive in driveLetters)
            {
                foreach (var phpPath in phpPaths)
                {
                    possiblePaths.Add($"{drive}:{phpPath}");
                }
            }
        }

        foreach (var path in possiblePaths)
        {
            try
            {
                // Check if file exists for absolute paths
                if (Path.IsPathRooted(path) && !File.Exists(path))
                {
                    continue;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    if (process.ExitCode == 0)
                    {
                        _cachedPhpPath = path;
                        return path;
                    }
                }
            }
            catch { continue; }
        }

        return null;
    }

    public string GetPhpVersion(string phpPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = phpPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);
                
                // Extract version from output (first line typically)
                var lines = output.Split('\n');
                return lines.Length > 0 ? lines[0] : "Unknown";
            }
        }
        catch { }
        
        return "Unknown";
    }

    public string GetPhpNotFoundMessage()
    {
        var serverRoot = Directory.GetCurrentDirectory();
        var localPhpFolder = Path.Combine(serverRoot, "php");
        
        return "PHP runtime not found. Please install PHP 8.0 or higher.\n" +
               "Recommended: Place PHP in the local 'php' folder:\n" +
               $"  {localPhpFolder}\n" +
               "\nAlternative installation:\n" +
               "  - Linux: sudo apt install php8.2-cli php8.2-sqlite3\n" +
               "  - macOS: brew install php\n" +
               "  - Windows: Download from https://windows.php.net/download/";
    }
}
