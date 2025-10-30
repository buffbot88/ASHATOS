using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ASHATCore.Engine;

/// <summary>
/// Manages Apache Configuration scanning for external webserver on Linux
/// ASHATOS uses internal Kestrel webserver on all platforms
/// On Windows 11, Apache is NOT required - Kestrel handles all web serving
/// On Linux, external Apache/Nginx can be used as reverse proxy to Kestrel
/// Note: PHP support is DEPRECATED - LegendaryCMS uses pure .NET (Razor/Blazor)
/// </summary>
public class ApacheManager
{
    private readonly string _cmsPath;
    private readonly int _port;
    private static readonly string StaticConfigPath = @"C:\ASHATOS\webserver\settings";
    
    public ApacheManager(string cmsPath, int port = 80)
    {
        _cmsPath = cmsPath;
        _port = port;
    }
    
    /// <summary>
    /// Scans for Apache httpd.conf in the static Configuration folder
    /// Note: On Windows 11, this is not required as Kestrel handles all web serving
    /// </summary>
    public static (bool found, string? path, string? message) ScanForApacheConfig()
    {
        // On Windows, Apache is not needed
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var message = "Apache is not required on Windows - ASHATOS uses Kestrel webserver";
            Console.WriteLine($"[ApacheManager] ℹ️  {message}");
            return (false, null, message);
        }
        
        try
        {
            Console.WriteLine($"[ApacheManager] Scanning for Apache Configuration...");
            Console.WriteLine($"[ApacheManager] Static config path: {StaticConfigPath}");
            
            // Check if static config directory exists
            if (!Directory.Exists(StaticConfigPath))
            {
                var message = $"Static Configuration directory not found: {StaticConfigPath}";
                Console.WriteLine($"[ApacheManager] ⚠️  {message}");
                return (false, null, message);
            }
            
            // Look for httpd.conf in the static folder
            var httpdConfPath = Path.Combine(StaticConfigPath, "httpd.conf");
            
            if (File.Exists(httpdConfPath))
            {
                Console.WriteLine($"[ApacheManager] ✅ Found Apache Configuration: {httpdConfPath}");
                return (true, httpdConfPath, $"Apache Configuration found at: {httpdConfPath}");
            }
            
            var notFoundMessage = $"httpd.conf not found in {StaticConfigPath}";
            Console.WriteLine($"[ApacheManager] ⚠️  {notFoundMessage}");
            return (false, null, notFoundMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error scanning for Apache Configuration: {ex.Message}";
            Console.WriteLine($"[ApacheManager] ❌ {errorMessage}");
            return (false, null, errorMessage);
        }
    }
    
    /// <summary>
    /// DEPRECATED: Scans for PHP php.ini in the static Configuration folder
    /// LegendaryCMS now uses pure .NET (Razor/Blazor) - PHP is no longer required
    /// This method is kept for backward compatibility with legacy deployments only
    /// </summary>
    [Obsolete("PHP support is deprecated. LegendaryCMS now uses pure .NET with Razor Pages/Blazor.")]
    public static (bool found, string? path, string? message) ScanForPhpConfig()
    {
        // On Windows, PHP is not needed
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var message = "PHP is not required - LegendaryCMS uses pure .NET (Razor/Blazor)";
            Console.WriteLine($"[ApacheManager] ℹ️  {message}");
            return (false, null, message);
        }
        
        try
        {
            Console.WriteLine($"[ApacheManager] Scanning for PHP Configuration...");
            Console.WriteLine($"[ApacheManager] Static config path: {StaticConfigPath}");
            
            // Check if static config directory exists
            if (!Directory.Exists(StaticConfigPath))
            {
                var message = $"Static Configuration directory not found: {StaticConfigPath}";
                Console.WriteLine($"[ApacheManager] ⚠️  {message}");
                return (false, null, message);
            }
            
            // Look for php.ini in the static folder
            var phpIniPath = Path.Combine(StaticConfigPath, "php.ini");
            
            if (File.Exists(phpIniPath))
            {
                Console.WriteLine($"[ApacheManager] ✅ Found PHP Configuration: {phpIniPath}");
                return (true, phpIniPath, $"PHP Configuration found at: {phpIniPath}");
            }
            
            var notFoundMessage = $"php.ini not found in {StaticConfigPath}";
            Console.WriteLine($"[ApacheManager] ⚠️  {notFoundMessage}");
            return (false, null, notFoundMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error scanning for PHP Configuration: {ex.Message}";
            Console.WriteLine($"[ApacheManager] ❌ {errorMessage}");
            return (false, null, errorMessage);
        }
    }
    
    /// <summary>
    /// DEPRECATED: Scans for PHP folder in the ASHATCore.exe directory
    /// LegendaryCMS now uses pure .NET (Razor/Blazor) - PHP is no longer required
    /// This method is kept for backward compatibility with legacy deployments only
    /// </summary>
    [Obsolete("PHP support is deprecated. LegendaryCMS now uses pure .NET with Razor Pages/Blazor.")]
    public static (bool found, string? path, string? message) ScanForPhpFolder()
    {
        try
        {
            // Get the directory where ASHATCore.exe is running
            var serverRoot = Directory.GetCurrentDirectory();
            var phpFolder = Path.Combine(serverRoot, "php");
            
            Console.WriteLine($"[ApacheManager] Scanning for PHP folder in ASHATCore directory...");
            Console.WriteLine($"[ApacheManager] Server root: {serverRoot}");
            Console.WriteLine($"[ApacheManager] PHP folder path: {phpFolder}");
            
            // Check if php folder exists
            if (!Directory.Exists(phpFolder))
            {
                var message = $"PHP folder not found: {phpFolder}";
                Console.WriteLine($"[ApacheManager] ⚠️  {message}");
                return (false, null, message);
            }
            
            Console.WriteLine($"[ApacheManager] ✅ Found PHP folder: {phpFolder}");
            
            // Check if php.exe exists in the folder (Windows)
            var phpExe = Path.Combine(phpFolder, "php.exe");
            if (File.Exists(phpExe))
            {
                Console.WriteLine($"[ApacheManager] ✅ Found PHP executable: {phpExe}");
                return (true, phpFolder, $"PHP folder found with executable at: {phpFolder}");
            }
            
            // Check for php binary (Linux/macOS)
            var phpBinary = Path.Combine(phpFolder, "php");
            if (File.Exists(phpBinary))
            {
                Console.WriteLine($"[ApacheManager] ✅ Found PHP binary: {phpBinary}");
                return (true, phpFolder, $"PHP folder found with binary at: {phpFolder}");
            }
            
            // PHP folder exists but no executable found
            var noExeMessage = $"PHP folder exists but no php executable found in: {phpFolder}";
            Console.WriteLine($"[ApacheManager] ⚠️  {noExeMessage}");
            return (false, phpFolder, noExeMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error scanning for PHP folder: {ex.Message}";
            Console.WriteLine($"[ApacheManager] ❌ {errorMessage}");
            return (false, null, errorMessage);
        }
    }
    
    /// <summary>
    /// Verifies Apache Configuration is valid and contains required settings
    /// </summary>
    public static (bool valid, List<string> issues, string? message) VerifyApacheConfig(string configPath)
    {
        var issues = new List<string>();
        
        try
        {
            if (!File.Exists(configPath))
            {
                return (false, new List<string> { "Configuration file not found" }, "Apache Configuration file not found");
            }
            
            var config = File.ReadAllText(configPath);
            
            Console.WriteLine("[ApacheManager] ℹ️  Verifying Apache Configuration...");
            
            // Check for required proxy modules
            var proxyModuleEnabled = Regex.IsMatch(
                config, @"^\s*LoadModule\s+proxy_module\s+modules/mod_proxy\.so",
                RegexOptions.Multiline);
            
            var proxyHttpModuleEnabled = Regex.IsMatch(
                config, @"^\s*LoadModule\s+proxy_http_module\s+modules/mod_proxy_http\.so",
                RegexOptions.Multiline);
            
            if (!proxyModuleEnabled)
            {
                issues.Add("Missing or commented proxy_module");
            }
            
            if (!proxyHttpModuleEnabled)
            {
                issues.Add("Missing or commented proxy_http_module");
            }
            
            // Check if ASHATCore Configuration marker exists
            if (!config.Contains("# ASHATCore Configuration"))
            {
                issues.Add("ASHATCore Configuration not found");
            }
            
            if (issues.Count > 0)
            {
                Console.WriteLine("[ApacheManager] ⚠️  Configuration issues found:");
                foreach (var issue in issues)
                {
                    Console.WriteLine($"[ApacheManager]    - {issue}");
                }
                return (false, issues, "Apache Configuration has issues");
            }
            
            Console.WriteLine("[ApacheManager] ✅ Apache Configuration is valid");
            return (true, new List<string>(), "Apache Configuration is valid");
        }
        catch (Exception ex)
        {
            return (false, new List<string> { ex.Message }, $"Error verifying Apache Configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// DEPRECATED: Verifies PHP Configuration is valid and contains required settings
    /// LegendaryCMS now uses pure .NET (Razor/Blazor) - PHP is no longer required
    /// This method is kept for backward compatibility with legacy deployments only
    /// </summary>
    [Obsolete("PHP support is deprecated. LegendaryCMS now uses pure .NET with Razor Pages/Blazor.")]
    public static (bool valid, List<string> issues, string? message) VerifyPhpConfig(string configPath)
    {
        var issues = new List<string>();
        
        try
        {
            if (!File.Exists(configPath))
            {
                return (false, new List<string> { "Configuration file not found" }, "PHP Configuration file not found");
            }
            
            var config = File.ReadAllText(configPath);
            
            Console.WriteLine("[ApacheManager] ℹ️  Verifying PHP Configuration...");
            
            // Check for recommended PHP settings
            var requiredSettings = new Dictionary<string, string>
            {
                { "memory_limit", "256M" },
                { "max_execution_time", "60" },
                { "post_max_size", "10M" },
                { "upload_max_filesize", "10M" }
            };
            
            foreach (var setting in requiredSettings)
            {
                var pattern = $@"^\s*{Regex.Escape(setting.Key)}\s*=";
                if (!Regex.IsMatch(config, pattern, RegexOptions.Multiline))
                {
                    issues.Add($"Missing or commented: {setting.Key}");
                }
            }
            
            if (issues.Count > 0)
            {
                Console.WriteLine("[ApacheManager] ⚠️  PHP Configuration issues found:");
                foreach (var issue in issues)
                {
                    Console.WriteLine($"[ApacheManager]    - {issue}");
                }
                return (false, issues, "PHP Configuration has issues");
            }
            
            Console.WriteLine("[ApacheManager] ✅ PHP Configuration is valid");
            return (true, new List<string>(), "PHP Configuration is valid");
        }
        catch (Exception ex)
        {
            return (false, new List<string> { ex.Message }, $"Error verifying PHP Configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// DEPRECATED: Configures PHP settings in php.ini file
    /// LegendaryCMS now uses pure .NET (Razor/Blazor) - PHP is no longer required
    /// This method is kept for backward compatibility with legacy deployments only
    /// </summary>
    [Obsolete("PHP support is deprecated. LegendaryCMS now uses pure .NET with Razor Pages/Blazor.")]
    public static bool ConfigurePhpIni(string phpIniPath)
    {
        try
        {
            if (!File.Exists(phpIniPath))
            {
                Console.WriteLine($"[ApacheManager] ⚠️  PHP Configuration file not found: {phpIniPath}");
                return false;
            }
            
            Console.WriteLine($"[ApacheManager] 🔧 Configuring PHP settings in: {phpIniPath}");
            
            // Create backup before modification
            var backupPath = phpIniPath + $".ASHATCore_backup_{DateTime.UtcNow:yyyyMMddHHmmss}";
            File.Copy(phpIniPath, backupPath);
            Console.WriteLine($"[ApacheManager] 💾 Backup created: {backupPath}");
            
            var config = File.ReadAllText(phpIniPath);
            var updatedSettings = new List<string>();
            
            // Recommended PHP settings for ASHATCore
            var settings = new Dictionary<string, string>
            {
                { "memory_limit", "256M" },
                { "max_execution_time", "60" },
                { "max_input_time", "60" },
                { "post_max_size", "10M" },
                { "upload_max_filesize", "10M" },
                { "display_errors", "On" },
                { "log_errors", "On" },
                { "date.timezone", "UTC" }
            };
            
            foreach (var setting in settings)
            {
                var pattern = $@"^\s*;?\s*{Regex.Escape(setting.Key)}\s*=.*$";
                var replacement = $"{setting.Key} = {setting.Value}";
                
                if (Regex.IsMatch(config, pattern, RegexOptions.Multiline))
                {
                    config = Regex.Replace(config, pattern, replacement, RegexOptions.Multiline);
                    updatedSettings.Add(setting.Key);
                }
                else
                {
                    // Add the setting if it doesn't exist
                    config += $"\n{replacement}";
                    updatedSettings.Add(setting.Key);
                }
            }
            
            File.WriteAllText(phpIniPath, config);
            
            Console.WriteLine($"[ApacheManager] ✅ PHP Configuration updated");
            if (updatedSettings.Count > 0)
            {
                Console.WriteLine("[ApacheManager] Updated settings:");
                foreach (var setting in updatedSettings)
                {
                    Console.WriteLine($"[ApacheManager]    - {setting} = {settings[setting]}");
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] ❌ Error configuring PHP: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Checks if Apache is installed and available
    /// </summary>
    public static bool IsApacheAvailable()
    {
        try
        {
            // Try to find Apache in common locations
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var commonPaths = new[]
                {
                    @"C:\Apache24\bin\httpd.exe",
                    @"C:\Apache\bin\httpd.exe",
                    @"C:\Program Files\Apache\bin\httpd.exe",
                    @"C:\Program Files (x86)\Apache\bin\httpd.exe"
                };
                
                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Try to run apache2 or httpd
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = "apache2",
                        RedirectStandardOutput = true,
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
                            return true;
                        }
                    }
                }
                catch { }
                
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = "httpd",
                        RedirectStandardOutput = true,
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
                            return true;
                        }
                    }
                }
                catch { }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}
