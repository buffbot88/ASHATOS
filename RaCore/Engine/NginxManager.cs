using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaCore.Engine;

/// <summary>
/// Manages Nginx web server configuration for RaCore CMS
/// Also manages PHP configuration independently of web server
/// 
/// NOTE: This manager only CONFIGURES Nginx and PHP, it does NOT manage their lifecycle.
/// Nginx and PHP must be installed and managed externally by the user/system administrator.
/// </summary>
public class NginxManager
{
    private readonly string _cmsPath;
    private readonly int _port;
    
    public NginxManager(string cmsPath, int port = 8080)
    {
        _cmsPath = cmsPath;
        _port = port;
    }
    
    /// <summary>
    /// Checks if Nginx is installed and available
    /// </summary>
    public static bool IsNginxAvailable()
    {
        return FindNginxExecutable() != null;
    }
    
    /// <summary>
    /// Finds the Nginx executable path
    /// </summary>
    public static string? FindNginxExecutable()
    {
        // Try common command-line commands first (Linux/Unix/Mac)
        var nginxCommands = new[] { "nginx" };
        
        foreach (var cmd in nginxCommands)
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
        
        // Try common install locations
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var paths = new[]
            {
                @"C:\nginx\nginx.exe",
                @"C:\nginx-1.24.0\nginx.exe",
                @"C:\nginx-1.25.0\nginx.exe",
                @"D:\nginx\nginx.exe",
                @"E:\nginx\nginx.exe",
                @"C:\Program Files\nginx\nginx.exe"
            };
            
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }
        else
        {
            // Linux/Unix specific paths
            var paths = new[]
            {
                "/usr/sbin/nginx",
                "/usr/local/sbin/nginx",
                "/usr/bin/nginx",
                "/usr/local/bin/nginx"
            };
            
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    try
                    {
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
        }
        
        return null;
    }
    
    /// <summary>
    /// Finds the Nginx configuration file path
    /// </summary>
    public static string? FindNginxConfigPath()
    {
        var nginxPath = FindNginxExecutable();
        if (nginxPath == null)
        {
            return null;
        }
        
        // Try to get config path from Nginx itself
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = nginxPath,
                Arguments = "-V",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // nginx -V outputs to stderr, not stdout
                var output = process.StandardError.ReadToEnd();
                process.WaitForExit(3000);
                
                // Look for --conf-path in output
                var match = System.Text.RegularExpressions.Regex.Match(output, @"--conf-path=([^\s]+)");
                if (match.Success)
                {
                    var configPath = match.Groups[1].Value;
                    if (File.Exists(configPath))
                    {
                        return configPath;
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
                @"C:\nginx\conf\nginx.conf",
                @"C:\nginx-1.24.0\conf\nginx.conf",
                @"C:\nginx-1.25.0\conf\nginx.conf",
                @"D:\nginx\conf\nginx.conf",
                @"E:\nginx\conf\nginx.conf"
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
            // Linux/Unix config locations
            var configPaths = new[]
            {
                "/etc/nginx/nginx.conf",
                "/usr/local/nginx/conf/nginx.conf",
                "/usr/local/etc/nginx/nginx.conf"
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
    /// Parses the Nginx configuration to extract configured RaCore port from existing proxy rules
    /// </summary>
    /// <returns>The configured RaCore port, or null if not found</returns>
    public static int? GetConfiguredRaCorePort()
    {
        var configPath = FindNginxConfigPath();
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

            // Extract port from proxy_pass directive
            // Looking for patterns like: proxy_pass http://localhost:5000;
            var proxyPassMatch = System.Text.RegularExpressions.Regex.Match(
                config,
                @"proxy_pass\s+http://localhost:(\d+);"
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
    /// Configures Nginx as a reverse proxy for RaCore
    /// </summary>
    public bool ConfigureReverseProxy(int racorePort = 80, string domain = "localhost")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("[NginxManager] Reverse proxy auto-configuration currently only supported on Windows");
            Console.WriteLine("[NginxManager] For Linux/Mac, please configure Nginx manually in /etc/nginx/sites-available/");
            Console.WriteLine($"[NginxManager]   proxy_pass http://localhost:{racorePort};");
            return false;
        }
        
        var configPath = FindNginxConfigPath();
        if (configPath == null)
        {
            Console.WriteLine("[NginxManager] ⚠️  Could not find Nginx configuration file");
            Console.WriteLine("[NginxManager] Please ensure Nginx is installed and accessible");
            Console.WriteLine("[NginxManager] Common locations:");
            Console.WriteLine("[NginxManager]   - C:\\nginx\\conf\\nginx.conf");
            return false;
        }
        
        try
        {
            Console.WriteLine($"[NginxManager] Found Nginx config: {configPath}");
            
            // Read existing config
            var config = File.ReadAllText(configPath);
            
            // Check if RaCore reverse proxy is already configured
            var racoreProxyMarker = "# RaCore Reverse Proxy Configuration";
            var hasRaCoreConfig = config.Contains(racoreProxyMarker);
            
            if (!hasRaCoreConfig)
            {
                // Create backup before modification
                var backupPath = configPath + $".backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
                File.Copy(configPath, backupPath);
                Console.WriteLine($"[NginxManager] 💾 Backup created: {backupPath}");
                
                // Find the http block and add server configuration
                var httpBlockMatch = System.Text.RegularExpressions.Regex.Match(config, @"http\s*\{", System.Text.RegularExpressions.RegexOptions.Singleline);
                
                if (httpBlockMatch.Success)
                {
                    // Find the closing brace of the http block
                    var insertPosition = config.LastIndexOf('}');
                    
                    // Add reverse proxy configuration before the closing brace
                    var proxyConfig = $@"
    # RaCore Reverse Proxy Configuration
    # Auto-generated by RaCore NginxManager on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
    # This configuration allows accessing RaCore at http://{domain} (port 80)
    # instead of http://localhost:{racorePort}
    server {{
        listen 80;
        server_name {domain} agpstudios.online www.agpstudios.online;
        
        location / {{
            proxy_pass http://localhost:{racorePort};
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection ""upgrade"";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;
        }}
        
        # WebSocket support
        location /ws {{
            proxy_pass http://localhost:{racorePort}/ws;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection ""upgrade"";
            proxy_set_header Host $host;
        }}
        
        access_log logs/racore_proxy_access.log;
        error_log logs/racore_proxy_error.log;
    }}

";
                    config = config.Insert(insertPosition, proxyConfig);
                    
                    File.WriteAllText(configPath, config);
                    Console.WriteLine($"[NginxManager] ✅ Added reverse proxy configuration for {domain}:{racorePort}");
                    Console.WriteLine($"[NginxManager] ✅ Server names configured for: {domain}, agpstudios.online, www.agpstudios.online");
                    Console.WriteLine();
                    Console.WriteLine("[NginxManager] ⚠️  IMPORTANT: Nginx configuration has been updated!");
                    Console.WriteLine("[NginxManager] You must manually start/restart Nginx for changes to take effect:");
                    Console.WriteLine();
                    Console.WriteLine("[NginxManager]   Windows:");
                    Console.WriteLine("[NginxManager]     - Start: cd C:\\nginx && start nginx");
                    Console.WriteLine("[NginxManager]     - Reload: nginx -s reload");
                    Console.WriteLine("[NginxManager]     - Stop: nginx -s stop");
                    Console.WriteLine();
                    Console.WriteLine("[NginxManager]   Linux/Mac:");
                    Console.WriteLine("[NginxManager]     - Start: sudo systemctl start nginx");
                    Console.WriteLine("[NginxManager]     - Reload: sudo systemctl reload nginx");
                    Console.WriteLine("[NginxManager]     - Restart: sudo systemctl restart nginx");
                    Console.WriteLine();
                    Console.WriteLine($"[NginxManager] 🌐 After starting Nginx, access RaCore at:");
                    Console.WriteLine($"[NginxManager]   - http://{domain}");
                    Console.WriteLine($"[NginxManager]   - http://agpstudios.online");
                    Console.WriteLine($"[NginxManager]   - http://www.agpstudios.online");
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("[NginxManager] ⚠️  Could not find http block in nginx.conf");
                    return false;
                }
            }
            else
            {
                // Verify existing RaCore configuration is complete and correct
                Console.WriteLine("[NginxManager] ℹ️  RaCore reverse proxy configuration found, verifying...");
                
                var needsUpdate = false;
                var updateReasons = new List<string>();
                
                // Check if server_name includes required domains
                if (!config.Contains("server_name") || 
                    !(config.Contains("agpstudios.online") && config.Contains("www.agpstudios.online")))
                {
                    needsUpdate = true;
                    updateReasons.Add("Missing or incomplete server_name configuration");
                }
                
                // Check if correct port is configured
                var currentPortPattern = @"proxy_pass\s+http://localhost:(\d+);";
                var portMatch = System.Text.RegularExpressions.Regex.Match(config, currentPortPattern);
                if (portMatch.Success)
                {
                    var currentPort = int.Parse(portMatch.Groups[1].Value);
                    if (currentPort != racorePort)
                    {
                        needsUpdate = true;
                        updateReasons.Add($"Port mismatch: configured={currentPort}, expected={racorePort}");
                    }
                }
                
                if (needsUpdate)
                {
                    Console.WriteLine("[NginxManager] ⚠️  Configuration needs updates:");
                    foreach (var reason in updateReasons)
                    {
                        Console.WriteLine($"[NginxManager]   - {reason}");
                    }
                    Console.WriteLine("[NginxManager] Please update nginx.conf manually or delete the RaCore configuration block to regenerate");
                }
                else
                {
                    Console.WriteLine("[NginxManager] ✅ Configuration looks good!");
                    Console.WriteLine($"[NginxManager] 🌐 Access RaCore at:");
                    Console.WriteLine($"[NginxManager]   - http://{domain}");
                    Console.WriteLine($"[NginxManager]   - http://agpstudios.online");
                    Console.WriteLine($"[NginxManager]   - http://www.agpstudios.online");
                }
            }
            
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[NginxManager] ❌ Error: Access denied to Nginx config file");
            Console.WriteLine($"[NginxManager] Please run RaCore as Administrator to configure Nginx");
            Console.WriteLine($"[NginxManager] Details: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NginxManager] ❌ Error configuring reverse proxy: {ex.Message}");
            Console.WriteLine($"[NginxManager] You can manually configure Nginx by adding the following to nginx.conf:");
            Console.WriteLine($"[NginxManager]   server {{");
            Console.WriteLine($"[NginxManager]     listen 80;");
            Console.WriteLine($"[NginxManager]     server_name {domain};");
            Console.WriteLine($"[NginxManager]     location / {{");
            Console.WriteLine($"[NginxManager]       proxy_pass http://localhost:{racorePort};");
            Console.WriteLine($"[NginxManager]     }}");
            Console.WriteLine($"[NginxManager]   }}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates Nginx configuration file for the CMS
    /// </summary>
    public void CreateNginxConfig()
    {
        var configDir = Path.Combine(_cmsPath, "nginx_conf");
        Directory.CreateDirectory(configDir);
        
        var configPath = Path.Combine(configDir, "racore.conf");
        
        var config = $@"# RaCore CMS Nginx Configuration
# Auto-generated by RaCore NginxManager

server {{
    listen {_port};
    server_name localhost;
    root ""{_cmsPath}"";
    index index.php index.html;
    
    location / {{
        try_files $uri $uri/ /index.php?$query_string;
    }}
    
    location ~ \.php$ {{
        fastcgi_pass 127.0.0.1:9000;
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
    }}
    
    location ~ /\.ht {{
        deny all;
    }}
    
    # Logging
    access_log logs/racore_access.log;
    error_log logs/racore_error.log;
}}
";
        
        File.WriteAllText(configPath, config);
        Console.WriteLine($"[NginxManager] Nginx config created: {configPath}");
    }
    
    /// <summary>
    /// [DEPRECATED] This method should not be used. PHP should be managed externally.
    /// Starts PHP built-in web server (for legacy compatibility only)
    /// </summary>
    [Obsolete("RaOS should not manage PHP lifecycle. Install and run PHP/PHP-FPM externally.")]
    public bool StartPhpServer(string phpPath)
    {
        Console.WriteLine("[NginxManager] ⚠️  WARNING: StartPhpServer is deprecated!");
        Console.WriteLine("[NginxManager] PHP should be installed and managed externally.");
        Console.WriteLine("[NginxManager] To run PHP manually:");
        Console.WriteLine($"[NginxManager]   cd {_cmsPath} && php -S localhost:{_port}");
        return false;
    }
    
    /// <summary>
    /// [DEPRECATED] This method should not be used. PHP should be managed externally.
    /// Stops the running PHP server (for legacy compatibility only)
    /// </summary>
    [Obsolete("RaOS should not manage PHP lifecycle. Install and run PHP/PHP-FPM externally.")]
    public void Stop()
    {
        Console.WriteLine("[NginxManager] ⚠️  WARNING: Stop is deprecated!");
        Console.WriteLine("[NginxManager] PHP should be installed and managed externally.");
    }
    
    /// <summary>
    /// Restarts the Nginx web server
    /// NOTE: This method is kept for the manual restart API endpoint only.
    /// RaOS does not automatically restart Nginx during configuration.
    /// </summary>
    /// <returns>Tuple with success status and message</returns>
    public static (bool success, string message) RestartNginx()
    {
        try
        {
            var nginxPath = FindNginxExecutable();
            if (nginxPath == null)
            {
                return (false, "Nginx not found. Please ensure Nginx is installed.");
            }
            
            Console.WriteLine($"[NginxManager] Attempting to restart Nginx using: {nginxPath}");
            
            // Windows: Use nginx -s reload
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Try to reload using nginx -s reload
                var startInfo = new ProcessStartInfo
                {
                    FileName = nginxPath,
                    Arguments = "-s reload",
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
                        Console.WriteLine("[NginxManager] ✅ Nginx reloaded successfully");
                        return (true, "Nginx reloaded successfully");
                    }
                    else
                    {
                        Console.WriteLine($"[NginxManager] ⚠️  Nginx reload returned code {process.ExitCode}: {error}");
                        return (false, $"Nginx reload failed: {error}");
                    }
                }
                
                return (false, "Failed to start Nginx reload process");
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
                        Arguments = "restart nginx",
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
                            Console.WriteLine("[NginxManager] ✅ Nginx restarted successfully via systemctl");
                            return (true, "Nginx restarted successfully");
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
                        Arguments = "nginx restart",
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
                            Console.WriteLine("[NginxManager] ✅ Nginx restarted successfully via service");
                            return (true, "Nginx restarted successfully");
                        }
                        else if (error.Contains("Permission denied") || error.Contains("access denied"))
                        {
                            return (false, "Permission denied. Please run RaCore with sudo or grant appropriate permissions.");
                        }
                    }
                }
                catch { }
                
                return (false, "Nginx restart requires sudo privileges. Please restart manually with: sudo systemctl restart nginx");
            }
            // macOS: Use nginx -s reload
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = nginxPath,
                    Arguments = "-s reload",
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
                        Console.WriteLine("[NginxManager] ✅ Nginx reloaded successfully");
                        return (true, "Nginx reloaded successfully");
                    }
                    else if (error.Contains("Permission denied") || error.Contains("access denied"))
                    {
                        return (false, "Permission denied. Please run RaCore with sudo or grant appropriate permissions.");
                    }
                    else
                    {
                        return (false, $"Nginx reload failed: {error}");
                    }
                }
                
                return (false, "Failed to start Nginx reload process");
            }
            
            return (false, "Unsupported platform for automatic Nginx restart");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NginxManager] ❌ Error restarting Nginx: {ex.Message}");
            return (false, $"Error restarting Nginx: {ex.Message}");
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
                @"\php81\php.exe",
                @"\php82\php.exe",
                @"\xampp\php\php.exe",
                @"\wamp\bin\php\php8.1.0\php.exe",
                @"\wamp64\bin\php\php8.1.0\php.exe"
            };
            
            foreach (var drive in driveLetters)
            {
                foreach (var path in phpPaths)
                {
                    possiblePaths.Add($"{drive}:{path}");
                }
            }
        }
        
        // Check each path
        foreach (var path in possiblePaths)
        {
            try
            {
                if (File.Exists(path))
                {
                    return path;
                }
                
                // Try to execute if it's in PATH
                if (path == "php")
                {
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
                        process.WaitForExit(3000);
                        if (process.ExitCode == 0)
                        {
                            return path;
                        }
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
        
        // Try to get php.ini path from PHP itself
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
                process.WaitForExit(3000);
                
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
    /// Gets the suggested path for generating a php.ini file based on PHP's expected location
    /// </summary>
    public static string? GetSuggestedPhpIniPath(string? phpPath = null)
    {
        phpPath ??= FindPhpExecutable();
        
        if (phpPath == null)
        {
            return null;
        }
        
        // Try to get php.ini path from PHP itself
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
                process.WaitForExit(3000);
                
                var lines = output.Split('\n');
                
                // First try to find "Configuration File (php.ini) Path:"
                foreach (var line in lines)
                {
                    if (line.Contains("Configuration File (php.ini) Path:"))
                    {
                        var colonIndex = line.IndexOf("Path:");
                        if (colonIndex >= 0)
                        {
                            var pathPart = line.Substring(colonIndex + 5).Trim();
                            if (!string.IsNullOrEmpty(pathPart) && pathPart != "(none)")
                            {
                                // Return the full path to php.ini in that directory
                                return Path.Combine(pathPart, "php.ini");
                            }
                        }
                    }
                }
                
                // Fallback: generate in the same directory as php.exe
                var phpDir = Path.GetDirectoryName(phpPath);
                if (!string.IsNullOrEmpty(phpDir))
                {
                    return Path.Combine(phpDir, "php.ini");
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
; Auto-generated by RaCore NginxManager

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
            Console.WriteLine($"[NginxManager] PHP configuration generated: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NginxManager] Error generating PHP config: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Configures PHP settings in an existing php.ini file
    /// </summary>
    public static bool ConfigurePhpIni(string? phpIniPath = null)
    {
        phpIniPath ??= FindPhpIniPath();
        
        if (phpIniPath == null || !File.Exists(phpIniPath))
        {
            Console.WriteLine("[NginxManager] Could not find php.ini file");
            return false;
        }
        
        try
        {
            Console.WriteLine($"[NginxManager] Configuring PHP settings in: {phpIniPath}");
            
            // Create backup before modification
            var backupPath = phpIniPath + $".backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
            File.Copy(phpIniPath, backupPath);
            Console.WriteLine($"[NginxManager] 💾 Backup created: {backupPath}");
            
            var content = File.ReadAllText(phpIniPath);
            
            // Configure recommended settings
            var settings = new Dictionary<string, string>
            {
                { "memory_limit", "256M" },
                { "max_execution_time", "60" },
                { "upload_max_filesize", "50M" },
                { "post_max_size", "50M" }
            };
            
            foreach (var setting in settings)
            {
                var pattern = $@"^\s*;?\s*{setting.Key}\s*=.*$";
                var replacement = $"{setting.Key} = {setting.Value}";
                
                if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern, System.Text.RegularExpressions.RegexOptions.Multiline))
                {
                    content = System.Text.RegularExpressions.Regex.Replace(
                        content,
                        pattern,
                        replacement,
                        System.Text.RegularExpressions.RegexOptions.Multiline
                    );
                }
                else
                {
                    // Add setting if it doesn't exist
                    content += $"\n{replacement}";
                }
            }
            
            File.WriteAllText(phpIniPath, content);
            Console.WriteLine("[NginxManager] ✅ PHP configuration updated");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NginxManager] Error configuring php.ini: {ex.Message}");
            return false;
        }
    }
}
