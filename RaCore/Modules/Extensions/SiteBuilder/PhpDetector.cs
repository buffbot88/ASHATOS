using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Handles PHP runtime detection and validation.
/// </summary>
public class PhpDetector
{
    private readonly SiteBuilderModule _module;

    public PhpDetector(SiteBuilderModule module)
    {
        _module = module;
    }

    public string DetectPHP()
    {
        // PHP scanning removed - should be pre-configured in host environment
        return "PHP scanning disabled. PHP should be installed and configured in the host environment before running RaOS.";
    }

    /// <summary>
    /// Finds PHP executable - DEPRECATED: PHP should be pre-configured in host environment
    /// </summary>
    [Obsolete("PHP scanning removed. PHP should be installed and configured in host environment before running RaOS.")]
    public string? FindPhpExecutable()
    {
        // PHP should be pre-configured in the host environment
        // No scanning is performed
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
