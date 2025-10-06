using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaCore.Engine;

/// <summary>
/// Manages Apache web server configuration and lifecycle for RaCore CMS
/// </summary>
public class ApacheManager
{
    private readonly string _cmsPath;
    private readonly int _port;
    private Process? _apacheProcess;
    
    public ApacheManager(string cmsPath, int port = 8080)
    {
        _cmsPath = cmsPath;
        _port = port;
    }
    
    /// <summary>
    /// Checks if Apache is installed and available
    /// </summary>
    public static bool IsApacheAvailable()
    {
        return FindApacheExecutable() != null;
    }
    
    /// <summary>
    /// Finds the Apache executable path
    /// </summary>
    public static string? FindApacheExecutable()
    {
        // Try common command-line commands first (Linux/Unix/Mac)
        var apacheCommands = new[] { "apache2", "httpd", "apachectl" };
        
        foreach (var cmd in apacheCommands)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = "-v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(3000);
                    if (process.ExitCode == 0)
                    {
                        return cmd;
                    }
                }
            }
            catch { continue; }
        }
        
        // Try common Windows Apache installation paths
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsPaths = new[]
            {
                @"C:\Apache\bin\httpd.exe",
                @"C:\Apache24\bin\httpd.exe",
                @"C:\Apache2\bin\httpd.exe",
                @"D:\Apache\bin\httpd.exe",
                @"D:\Apache24\bin\httpd.exe",
                @"E:\Apache\bin\httpd.exe",
                @"E:\Apache24\bin\httpd.exe",
                @"C:\Program Files\Apache Software Foundation\Apache2.4\bin\httpd.exe",
                @"C:\xampp\apache\bin\httpd.exe"
            };
            
            foreach (var path in windowsPaths)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }
                    
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "-v",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        process.WaitForExit(3000);
                        if (process.ExitCode == 0)
                        {
                            return path;
                        }
                    }
                }
                catch { continue; }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Finds the Apache httpd.conf configuration file path
    /// </summary>
    public static string? FindApacheConfigPath()
    {
        var apachePath = FindApacheExecutable();
        if (apachePath == null)
        {
            return null;
        }
        
        // Try to get config path from Apache itself
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = apachePath,
                Arguments = "-V",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);
                
                // Look for SERVER_CONFIG_FILE in output
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("SERVER_CONFIG_FILE"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(line, "\"(.+?)\"");
                        if (match.Success)
                        {
                            var configPath = match.Groups[1].Value;
                            
                            // If relative path, combine with HTTPD_ROOT
                            if (!Path.IsPathRooted(configPath))
                            {
                                foreach (var rootLine in lines)
                                {
                                    if (rootLine.Contains("HTTPD_ROOT"))
                                    {
                                        var rootMatch = System.Text.RegularExpressions.Regex.Match(rootLine, "\"(.+?)\"");
                                        if (rootMatch.Success)
                                        {
                                            var root = rootMatch.Groups[1].Value;
                                            configPath = Path.Combine(root, configPath);
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            if (File.Exists(configPath))
                            {
                                return configPath;
                            }
                        }
                    }
                }
            }
        }
        catch { }
        
        // Fallback: try common config locations
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var configPaths = new[]
            {
                @"C:\Apache\conf\httpd.conf",
                @"C:\Apache24\conf\httpd.conf",
                @"C:\Apache2\conf\httpd.conf",
                @"D:\Apache\conf\httpd.conf",
                @"D:\Apache24\conf\httpd.conf",
                @"E:\Apache\conf\httpd.conf",
                @"E:\Apache24\conf\httpd.conf",
                @"C:\Program Files\Apache Software Foundation\Apache2.4\conf\httpd.conf",
                @"C:\xampp\apache\conf\httpd.conf"
            };
            
            foreach (var path in configPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }
        else
        {
            var configPaths = new[]
            {
                "/etc/apache2/httpd.conf",
                "/etc/httpd/conf/httpd.conf",
                "/usr/local/apache2/conf/httpd.conf",
                "/opt/apache2/conf/httpd.conf"
            };
            
            foreach (var path in configPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Parses the Apache httpd.conf to extract configured RaCore port from existing proxy rules
    /// </summary>
    /// <returns>The configured RaCore port, or null if not found</returns>
    public static int? GetConfiguredRaCorePort()
    {
        var configPath = FindApacheConfigPath();
        if (configPath == null || !File.Exists(configPath))
        {
            return null;
        }

        try
        {
            var config = File.ReadAllText(configPath);
            
            // Look for RaCore proxy configuration marker
            if (!config.Contains("# RaCore Reverse Proxy Configuration"))
            {
                return null;
            }

            // Extract port from ProxyPass directive
            // Looking for patterns like: ProxyPass / http://localhost:5000/
            var proxyPassMatch = System.Text.RegularExpressions.Regex.Match(
                config,
                @"ProxyPass\s+/\s+http://localhost:(\d+)/"
            );

            if (proxyPassMatch.Success && int.TryParse(proxyPassMatch.Groups[1].Value, out var port))
            {
                return port;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Configures Apache as a reverse proxy for RaCore on Windows
    /// </summary>
    public bool ConfigureReverseProxy(int racorePort = 5000, string domain = "localhost")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("[ApacheManager] Reverse proxy auto-configuration currently only supported on Windows");
            Console.WriteLine("[ApacheManager] For Linux/Mac, please configure Apache manually:");
            Console.WriteLine($"[ApacheManager]   ProxyPass / http://localhost:{racorePort}/");
            Console.WriteLine($"[ApacheManager]   ProxyPassReverse / http://localhost:{racorePort}/");
            return false;
        }
        
        var configPath = FindApacheConfigPath();
        if (configPath == null)
        {
            Console.WriteLine("[ApacheManager] ⚠️  Could not find Apache httpd.conf");
            Console.WriteLine("[ApacheManager] Please ensure Apache is installed and accessible");
            Console.WriteLine("[ApacheManager] Common locations:");
            Console.WriteLine("[ApacheManager]   - C:\\Apache24\\conf\\httpd.conf");
            Console.WriteLine("[ApacheManager]   - C:\\xampp\\apache\\conf\\httpd.conf");
            return false;
        }
        
        try
        {
            Console.WriteLine($"[ApacheManager] Found Apache config: {configPath}");
            
            // Read existing config
            var config = File.ReadAllText(configPath);
            
            // Check if proxy modules are already enabled
            var proxyModuleEnabled = config.Contains("LoadModule proxy_module") && 
                                     !config.Contains("#LoadModule proxy_module");
            var proxyHttpModuleEnabled = config.Contains("LoadModule proxy_http_module") && 
                                         !config.Contains("#LoadModule proxy_http_module");
            
            var modified = false;
            
            // Enable proxy modules if not already enabled
            if (!proxyModuleEnabled)
            {
                config = System.Text.RegularExpressions.Regex.Replace(
                    config,
                    @"#\s*LoadModule\s+proxy_module\s+modules/mod_proxy\.so",
                    "LoadModule proxy_module modules/mod_proxy.so"
                );
                Console.WriteLine("[ApacheManager] ✅ Enabled mod_proxy");
                modified = true;
            }
            
            if (!proxyHttpModuleEnabled)
            {
                config = System.Text.RegularExpressions.Regex.Replace(
                    config,
                    @"#\s*LoadModule\s+proxy_http_module\s+modules/mod_proxy_http\.so",
                    "LoadModule proxy_http_module modules/mod_proxy_http.so"
                );
                Console.WriteLine("[ApacheManager] ✅ Enabled mod_proxy_http");
                modified = true;
            }
            
            // Check if RaCore reverse proxy is already configured
            var racoreProxyMarker = "# RaCore Reverse Proxy Configuration";
            if (!config.Contains(racoreProxyMarker))
            {
                // Add reverse proxy configuration at the end
                var proxyConfig = $@"

# RaCore Reverse Proxy Configuration
# Auto-generated by RaCore ApacheManager on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
# This configuration allows accessing RaCore at http://{domain} (port 80)
# instead of http://localhost:{racorePort}
<VirtualHost *:80>
    ServerName {domain}
    
    ProxyPreserveHost On
    ProxyPass / http://localhost:{racorePort}/
    ProxyPassReverse / http://localhost:{racorePort}/
    
    # WebSocket support
    ProxyPass /ws ws://localhost:{racorePort}/ws
    ProxyPassReverse /ws ws://localhost:{racorePort}/ws
    
    ErrorLog ""logs/racore_proxy_error.log""
    CustomLog ""logs/racore_proxy_access.log"" combined
</VirtualHost>
";
                config += proxyConfig;
                Console.WriteLine($"[ApacheManager] ✅ Added reverse proxy configuration for {domain}:{racorePort}");
                modified = true;
            }
            else
            {
                Console.WriteLine("[ApacheManager] ℹ️  RaCore reverse proxy already configured");
            }
            
            if (modified)
            {
                // Create backup
                var backupPath = configPath + ".racore_backup_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(configPath, backupPath);
                Console.WriteLine($"[ApacheManager] 💾 Backup created: {backupPath}");
                
                // Write modified config
                File.WriteAllText(configPath, config);
                Console.WriteLine($"[ApacheManager] ✅ Apache configuration updated: {configPath}");
                Console.WriteLine();
                Console.WriteLine("[ApacheManager] ⚠️  IMPORTANT: Please restart Apache for changes to take effect!");
                Console.WriteLine("[ApacheManager] Windows: Open Services and restart 'Apache2.4' service");
                Console.WriteLine("[ApacheManager] Or run: httpd.exe -k restart");
                Console.WriteLine();
                Console.WriteLine($"[ApacheManager] After restart, access RaCore at: http://{domain}");
                
                return true;
            }
            
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ApacheManager] ❌ Error: Access denied to Apache config file");
            Console.WriteLine($"[ApacheManager] Please run RaCore as Administrator to configure Apache");
            Console.WriteLine($"[ApacheManager] Details: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] ❌ Error configuring reverse proxy: {ex.Message}");
            Console.WriteLine($"[ApacheManager] You can manually configure Apache by adding the following to httpd.conf:");
            Console.WriteLine($"[ApacheManager]   LoadModule proxy_module modules/mod_proxy.so");
            Console.WriteLine($"[ApacheManager]   LoadModule proxy_http_module modules/mod_proxy_http.so");
            Console.WriteLine($"[ApacheManager]   <VirtualHost *:80>");
            Console.WriteLine($"[ApacheManager]     ProxyPass / http://localhost:{racorePort}/");
            Console.WriteLine($"[ApacheManager]     ProxyPassReverse / http://localhost:{racorePort}/");
            Console.WriteLine($"[ApacheManager]   </VirtualHost>");
            return false;
        }
    }
    
    /// <summary>
    /// Creates Apache configuration file for the CMS
    /// </summary>
    public void CreateApacheConfig()
    {
        var configDir = Path.Combine(_cmsPath, "apache_conf");
        Directory.CreateDirectory(configDir);
        
        var configPath = Path.Combine(configDir, "racore.conf");
        
        var config = $@"# RaCore CMS Apache Configuration
# Auto-generated by RaCore ApacheManager

<VirtualHost *:{_port}>
    ServerName localhost
    DocumentRoot ""{_cmsPath}""
    
    <Directory ""{_cmsPath}"">
        Options Indexes FollowSymLinks
        AllowOverride All
        Require all granted
        
        # Enable PHP processing
        <FilesMatch \.php$>
            SetHandler application/x-httpd-php
        </FilesMatch>
        
        DirectoryIndex index.php index.html
    </Directory>
    
    # Logging
    ErrorLog ""{{APACHE_LOG_DIR}}/racore_error.log""
    CustomLog ""{{APACHE_LOG_DIR}}/racore_access.log"" combined
    
    # PHP settings
    php_value upload_max_filesize 10M
    php_value post_max_size 10M
    php_value max_execution_time 60
</VirtualHost>
";
        
        File.WriteAllText(configPath, config);
        Console.WriteLine($"[ApacheManager] Apache config created: {configPath}");
    }
    
    /// <summary>
    /// Creates .htaccess file for the CMS directory
    /// </summary>
    public void CreateHtaccess()
    {
        var htaccessPath = Path.Combine(_cmsPath, ".htaccess");
        
        var htaccess = @"# RaCore CMS .htaccess
# Auto-generated by RaCore ApacheManager

# Enable PHP processing
<FilesMatch \.php$>
    SetHandler application/x-httpd-php
</FilesMatch>

# Default document
DirectoryIndex index.php index.html

# Security headers
<IfModule mod_headers.c>
    Header set X-Content-Type-Options ""nosniff""
    Header set X-Frame-Options ""SAMEORIGIN""
    Header set X-XSS-Protection ""1; mode=block""
</IfModule>

# Disable directory listing
Options -Indexes

# Protect sensitive files
<FilesMatch ""(\.sqlite|\.db|\.log)$"">
    Require all denied
</FilesMatch>
";
        
        File.WriteAllText(htaccessPath, htaccess);
        Console.WriteLine($"[ApacheManager] .htaccess created: {htaccessPath}");
    }
    
    /// <summary>
    /// Starts PHP built-in server as fallback if Apache is not available
    /// </summary>
    public bool StartPhpServer(string phpPath)
    {
        try
        {
            Console.WriteLine($"[ApacheManager] Starting PHP built-in server on port {_port}...");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = phpPath,
                Arguments = $"-S localhost:{_port} -t \"{_cmsPath}\"",
                WorkingDirectory = _cmsPath,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            _apacheProcess = Process.Start(startInfo);
            
            if (_apacheProcess != null)
            {
                // Give it a moment to start
                Thread.Sleep(1000);
                
                if (!_apacheProcess.HasExited)
                {
                    Console.WriteLine($"[ApacheManager] PHP server started successfully on http://localhost:{_port}");
                    
                    // Log output asynchronously
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            while (!_apacheProcess.HasExited)
                            {
                                var output = await _apacheProcess.StandardOutput.ReadLineAsync();
                                if (!string.IsNullOrEmpty(output))
                                {
                                    Console.WriteLine($"[PHP Server] {output}");
                                }
                            }
                        }
                        catch { }
                    });
                    
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] Error starting PHP server: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Configures Apache to use the CMS directory (Linux/Ubuntu)
    /// </summary>
    public bool ConfigureApache()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("[ApacheManager] Apache auto-configuration only supported on Linux");
            return false;
        }
        
        try
        {
            CreateApacheConfig();
            CreateHtaccess();
            
            var configPath = Path.Combine(_cmsPath, "apache_conf", "racore.conf");
            
            Console.WriteLine("[ApacheManager] Apache configuration files created");
            Console.WriteLine("[ApacheManager] To enable Apache:");
            Console.WriteLine($"  sudo cp {configPath} /etc/apache2/sites-available/");
            Console.WriteLine($"  sudo a2ensite racore.conf");
            Console.WriteLine($"  sudo systemctl reload apache2");
            Console.WriteLine($"[ApacheManager] Then access CMS at: http://localhost:{_port}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] Error configuring Apache: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Stops the running PHP server
    /// </summary>
    public void Stop()
    {
        if (_apacheProcess != null && !_apacheProcess.HasExited)
        {
            try
            {
                _apacheProcess.Kill();
                _apacheProcess.WaitForExit(5000);
                Console.WriteLine("[ApacheManager] PHP server stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApacheManager] Error stopping PHP server: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Restarts the Apache web server
    /// </summary>
    /// <returns>Tuple with success status and message</returns>
    public static (bool success, string message) RestartApache()
    {
        try
        {
            var apachePath = FindApacheExecutable();
            if (apachePath == null)
            {
                return (false, "Apache not found. Please ensure Apache is installed.");
            }
            
            Console.WriteLine($"[ApacheManager] Attempting to restart Apache using: {apachePath}");
            
            // Windows: Use httpd.exe -k restart
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Try to restart using httpd.exe -k restart
                var startInfo = new ProcessStartInfo
                {
                    FileName = apachePath,
                    Arguments = "-k restart",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit(10000);
                    
                    if (process.ExitCode == 0 || string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("[ApacheManager] ✅ Apache restarted successfully");
                        return (true, "Apache restarted successfully");
                    }
                    else
                    {
                        Console.WriteLine($"[ApacheManager] ⚠️  Apache restart returned code {process.ExitCode}: {error}");
                        return (false, $"Apache restart failed: {error}");
                    }
                }
                
                return (false, "Failed to start Apache restart process");
            }
            // Linux/Unix: Use systemctl or service command
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Try systemctl first (most modern Linux distributions)
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "systemctl",
                        Arguments = "restart apache2",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit(10000);
                        
                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine("[ApacheManager] ✅ Apache restarted successfully via systemctl");
                            return (true, "Apache restarted successfully");
                        }
                        else if (error.Contains("Permission denied") || error.Contains("access denied"))
                        {
                            return (false, "Permission denied. Please run RaCore with sudo or grant appropriate permissions.");
                        }
                    }
                }
                catch { }
                
                // Fallback to service command
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "service",
                        Arguments = "apache2 restart",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit(10000);
                        
                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine("[ApacheManager] ✅ Apache restarted successfully via service");
                            return (true, "Apache restarted successfully");
                        }
                        else if (error.Contains("Permission denied") || error.Contains("access denied"))
                        {
                            return (false, "Permission denied. Please run RaCore with sudo or grant appropriate permissions.");
                        }
                    }
                }
                catch { }
                
                return (false, "Apache restart requires sudo privileges. Please restart manually with: sudo systemctl restart apache2");
            }
            // macOS: Use apachectl
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "apachectl",
                    Arguments = "restart",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit(10000);
                    
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("[ApacheManager] ✅ Apache restarted successfully via apachectl");
                        return (true, "Apache restarted successfully");
                    }
                    else if (error.Contains("Permission denied") || error.Contains("access denied"))
                    {
                        return (false, "Permission denied. Please run RaCore with sudo or grant appropriate permissions.");
                    }
                    else
                    {
                        return (false, $"Apache restart failed: {error}");
                    }
                }
                
                return (false, "Failed to start Apache restart process");
            }
            
            return (false, "Unsupported platform for automatic Apache restart");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] ❌ Error restarting Apache: {ex.Message}");
            return (false, $"Error restarting Apache: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Finds PHP executable in common locations
    /// </summary>
    public static string? FindPhpExecutable()
    {
        // Try local php folder first
        var serverRoot = Directory.GetCurrentDirectory();
        var localPhpFolder = Path.Combine(serverRoot, "php");
        
        var possiblePaths = new List<string>
        {
            Path.Combine(localPhpFolder, "php.exe"),     // Local Windows
            Path.Combine(localPhpFolder, "php"),         // Local Linux/macOS
            "php",                                        // In PATH
            "/usr/bin/php",                               // Linux
            "/usr/local/bin/php",                         // Linux/macOS
        };
        
        // Add Windows-specific paths
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
                        return path;
                    }
                }
            }
            catch { continue; }
        }

        return null;
    }
    
    /// <summary>
    /// Finds PHP configuration file (php.ini)
    /// </summary>
    public static string? FindPhpIniPath(string? phpPath = null)
    {
        phpPath ??= FindPhpExecutable();
        if (phpPath == null)
        {
            return null;
        }
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = phpPath,
                Arguments = "--ini",
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
                
                // Look for "Loaded Configuration File" in output
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Loaded Configuration File:"))
                    {
                        var path = line.Split(':')[1].Trim();
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
        }
        catch { }
        
        return null;
    }
    
    /// <summary>
    /// Generates a basic PHP configuration file
    /// </summary>
    public static bool GeneratePhpIni(string outputPath)
    {
        try
        {
            var phpIni = @"; PHP Configuration
; Auto-generated by RaCore ApacheManager

[PHP]
engine = On
short_open_tag = Off
precision = 14
output_buffering = 4096
zlib.output_compression = Off
implicit_flush = Off
serialize_precision = -1
disable_functions =
disable_classes =
max_execution_time = 60
max_input_time = 60
memory_limit = 256M
error_reporting = E_ALL & ~E_DEPRECATED & ~E_STRICT
display_errors = On
display_startup_errors = On
log_errors = On
post_max_size = 10M
file_uploads = On
upload_max_filesize = 10M
max_file_uploads = 20
default_socket_timeout = 60

[Date]
date.timezone = UTC

[SQLite3]
sqlite3.extension_dir =
";
            
            File.WriteAllText(outputPath, phpIni);
            Console.WriteLine($"[ApacheManager] PHP configuration generated: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApacheManager] Error generating PHP config: {ex.Message}");
            return false;
        }
    }
}
