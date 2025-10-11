using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ASHATCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Handles PHP8 runtime detection and validation for executing Generated PHP files.
/// ASHATOS Generates PHP files (CMS, forums, profiles) that require external PHP8 with development ini defaults.
/// </summary>
public class PhpDetector(SiteBuilderModule module)
{
    private readonly SiteBuilderModule _module = module;

    public string DetectPHP()
    {
        var phpPath = FindPhpExecutable();
        if (phpPath == null)
        {
            return GetPhpNotFoundMessage();
        }
        
        var version = GetPhpVersion(phpPath);
        return $"✅ PHP detected\nPath: {phpPath}\nVersion: {version}";
    }

    /// <summary>
    /// Finds PHP executable by scanning common installation locations
    /// </summary>
    public string? FindPhpExecutable()
    {
        try
        {
            // First, check the local php folder in server root
            var serverRoot = Directory.GetCurrentDirectory();
            var localPhpFolder = Path.Combine(serverRoot, "php");
            var localPhpExe = Path.Combine(localPhpFolder, "php.exe");
            
            if (File.Exists(localPhpExe))
            {
                _module.Log($"Found PHP in local folder: {localPhpExe}");
                return localPhpExe;
            }
            
            // Try to find PHP in PATH
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Check common PHP installation paths
                var commonPaths = new[]
                {
                    @"C:\php\php.exe",
                    @"C:\PHP8\php.exe",
                    @"C:\PHP82\php.exe",
                    @"C:\Program Files\PHP\php.exe",
                    @"C:\xampp\php\php.exe",
                    @"C:\wamp64\bin\php\php8.2.0\php.exe"
                };
                
                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        _module.Log($"Found PHP: {path}");
                        return path;
                    }
                }
                
                // Try using 'where' command to find php in PATH
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "php",
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
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            var phpPath = output.Split('\n')[0].Trim();
                            if (File.Exists(phpPath))
                            {
                                _module.Log($"Found PHP in PATH: {phpPath}");
                                return phpPath;
                            }
                        }
                    }
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Linux/macOS: Use 'which' command
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = "php",
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
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            var phpPath = output.Trim();
                            if (File.Exists(phpPath))
                            {
                                _module.Log($"Found PHP: {phpPath}");
                                return phpPath;
                            }
                        }
                    }
                }
                catch { }
            }
            
            _module.Log("PHP not found in any common locations");
            return null;
        }
        catch (Exception ex)
        {
            _module.Log($"Error searching for PHP: {ex.Message}", "ERROR");
            return null;
        }
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
