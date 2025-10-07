using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.SiteBuilder;
using System.Text.Json;
using Abstractions;

namespace RaCore.Engine;

/// <summary>
/// Manages first-run initialization for RaCore, including CMS spawning, Nginx setup, and guided initialization sequence
/// </summary>
public class FirstRunManager
{
    private readonly string _firstRunMarkerPath;
    private readonly string _configPath;
    private readonly string _cmsPath;
    private readonly ModuleManager _moduleManager;
    private ServerConfiguration _serverConfig;
    
    public FirstRunManager(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
        // Use GetCurrentDirectory() to get the RaCore.exe server root directory (where the executable runs)
        var serverRoot = Directory.GetCurrentDirectory();
        _firstRunMarkerPath = Path.Combine(serverRoot, ".racore_initialized");
        _configPath = Path.Combine(serverRoot, "server-config.json");
        _cmsPath = Path.Combine(serverRoot, "wwwroot");
        
        // Load or create server configuration
        _serverConfig = LoadServerConfiguration();
        
        // Sync Dev mode with modules on startup
        SyncDevModeWithModules();
    }
    
    /// <summary>
    /// Load server configuration from disk or create a new one
    /// </summary>
    private ServerConfiguration LoadServerConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<ServerConfiguration>(json);
                if (config != null)
                {
                    Console.WriteLine($"[FirstRunManager] Loaded server configuration: Mode={config.Mode}");
                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirstRunManager] Warning: Could not load server configuration: {ex.Message}");
        }
        
        // Return default configuration
        return new ServerConfiguration
        {
            Mode = ServerMode.Production,
            IsFirstRun = true,
            CmsPath = _cmsPath,
            UnderConstruction = true  // Default to Under Construction until admin sets up the site
        };
    }
    
    /// <summary>
    /// Save server configuration to disk
    /// </summary>
    private void SaveServerConfiguration()
    {
        try
        {
            var json = JsonSerializer.Serialize(_serverConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
            Console.WriteLine($"[FirstRunManager] Server configuration saved: Mode={_serverConfig.Mode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirstRunManager] Warning: Could not save server configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Save server configuration to disk (public method for API access)
    /// </summary>
    public void SaveConfiguration()
    {
        SaveServerConfiguration();
    }
    
    /// <summary>
    /// Get the current server configuration
    /// </summary>
    public ServerConfiguration GetServerConfiguration()
    {
        return _serverConfig;
    }
    
    /// <summary>
    /// Set the server mode
    /// </summary>
    public void SetServerMode(ServerMode mode)
    {
        _serverConfig.Mode = mode;
        
        // Automatically enable license validation bypass for Dev mode
        _serverConfig.SkipLicenseValidation = (mode == ServerMode.Dev);
        
        SaveServerConfiguration();
        Console.WriteLine($"[FirstRunManager] Server mode set to: {mode}");
        
        if (mode == ServerMode.Dev)
        {
            Console.WriteLine($"[FirstRunManager] Dev mode enabled - License validation will be bypassed for initial setup");
        }
        
        // Sync Dev mode with LegendaryPay module if available
        SyncDevModeWithModules();
    }
    
    /// <summary>
    /// Sync Dev mode setting with modules that need it (e.g., LegendaryPay, LegendarySupermarket)
    /// </summary>
    private void SyncDevModeWithModules()
    {
        var serverMode = _serverConfig.Mode;
        
        // Find LegendaryPay module and sync Dev mode
        var legendaryPayModule = _moduleManager.Modules
            .Select(m => m.Instance)
            .FirstOrDefault(m => m.Name == "LegendaryPay");
        
        if (legendaryPayModule != null)
        {
            var setDevModeMethod = legendaryPayModule.GetType().GetMethod("SetDevModeFromServer");
            if (setDevModeMethod != null)
            {
                var isDevMode = serverMode == ServerMode.Dev;
                setDevModeMethod.Invoke(legendaryPayModule, new object[] { isDevMode });
                Console.WriteLine($"[FirstRunManager] Synced Dev mode ({isDevMode}) with LegendaryPay module");
            }
        }
        
        // Find LegendarySupermarket module and sync server mode (for Reseller feature control)
        var supermarketModule = _moduleManager.Modules
            .Select(m => m.Instance)
            .FirstOrDefault(m => m.Name == "LegendarySupermarket");
        
        if (supermarketModule != null)
        {
            var setServerModeMethod = supermarketModule.GetType().GetMethod("SetServerModeFromConfig");
            if (setServerModeMethod != null)
            {
                setServerModeMethod.Invoke(supermarketModule, new object[] { serverMode });
                Console.WriteLine($"[FirstRunManager] Synced server mode ({serverMode}) with LegendarySupermarket module");
            }
        }
    }
    
    /// <summary>
    /// Checks if this is the first run of RaCore
    /// </summary>
    public bool IsFirstRun()
    {
        return _serverConfig.IsFirstRun && !File.Exists(_firstRunMarkerPath);
    }
    
    /// <summary>
    /// Check system requirements and dependencies
    /// </summary>
    private InitializationStepResult CheckSystemRequirements()
    {
        var result = new InitializationStepResult { Success = true };
        var warnings = new List<string>();
        
        Console.WriteLine("[FirstRunManager] Checking system requirements...");
        
        // Check .NET version
        var dotnetVersion = Environment.Version;
        Console.WriteLine($"  ✓ .NET Runtime: {dotnetVersion}");
        
        // Check for PHP
        var phpPath = FindPhpExecutable();
        if (phpPath != null)
        {
            Console.WriteLine($"  ✓ PHP executable found: {phpPath}");
        }
        else
        {
            warnings.Add("PHP not found - CMS functionality will be limited");
            Console.WriteLine("  ⚠️  PHP not found - CMS functionality will be limited");
        }
        
        // Check for Nginx
        if (NginxManager.IsNginxAvailable())
        {
            Console.WriteLine("  ✓ Nginx is available");
        }
        else
        {
            warnings.Add("Nginx not available - will use PHP built-in server");
            Console.WriteLine("  ⚠️  Nginx not available - will use PHP built-in server");
        }
        
        // Check disk space
        try
        {
            var drive = new DriveInfo(Directory.GetCurrentDirectory());
            var freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            Console.WriteLine($"  ✓ Available disk space: {freeSpaceGB:F2} GB");
            
            if (freeSpaceGB < 1.0)
            {
                warnings.Add($"Low disk space: {freeSpaceGB:F2} GB available");
                Console.WriteLine($"  ⚠️  Low disk space: {freeSpaceGB:F2} GB available");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Could not check disk space: {ex.Message}");
        }
        
        // Check memory
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);
            Console.WriteLine($"  ✓ Current memory usage: {memoryMB:F2} MB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Could not check memory: {ex.Message}");
        }
        
        result.Warnings = warnings;
        result.Message = warnings.Count > 0 
            ? $"System requirements check completed with {warnings.Count} warning(s)"
            : "System requirements check completed successfully";
            
        _serverConfig.SystemRequirementsMet = true;
        _serverConfig.SystemWarnings = warnings;
        
        return result;
    }
    
    /// <summary>
    /// Marks the system as initialized
    /// </summary>
    public void MarkAsInitialized()
    {
        try
        {
            _serverConfig.IsFirstRun = false;
            _serverConfig.InitializationCompleted = true;
            _serverConfig.InitializedAt = DateTime.UtcNow;
            _serverConfig.CmsPath = _cmsPath;
            
            // Keep Under Construction mode enabled by default
            // Admin can disable it via Control Panel when ready to go live
            // _serverConfig.UnderConstruction remains true until admin changes it
            
            // Save configuration
            SaveServerConfiguration();
            
            // Create marker file for backwards compatibility
            var initInfo = new
            {
                InitializedAt = DateTime.UtcNow,
                Version = _serverConfig.Version,
                CmsPath = _cmsPath,
                ServerMode = _serverConfig.Mode.ToString(),
                LicenseKey = _serverConfig.LicenseKey
            };
            
            File.WriteAllText(_firstRunMarkerPath, JsonSerializer.Serialize(initInfo, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("[FirstRunManager] System marked as initialized");
            Console.WriteLine("[FirstRunManager] Site remains in Under Construction mode - disable via Control Panel when ready");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirstRunManager] Warning: Could not create initialization marker: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Performs first-run initialization: spawns CMS with integrated Control Panel and configures Nginx
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        await Task.CompletedTask;
        Console.WriteLine("========================================");
        Console.WriteLine("   RaCore First-Run Initialization");
        Console.WriteLine($"   Server Mode: {_serverConfig.Mode}");
        Console.WriteLine("   CMS + Integrated Control Panel");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        try
        {
            // Step 1: Check system requirements and dependencies
            Console.WriteLine("[FirstRunManager] Step 1/7: Checking system requirements...");
            Console.WriteLine();
            var reqResult = CheckSystemRequirements();
            Console.WriteLine($"[FirstRunManager] {reqResult.Message}");
            if (reqResult.Warnings.Count > 0)
            {
                Console.WriteLine("[FirstRunManager] Warnings:");
                foreach (var warning in reqResult.Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
            }
            Console.WriteLine();
            
            // Find SiteBuilder module
            var siteBuilderModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<SiteBuilderModule>()
                .FirstOrDefault();
            
            if (siteBuilderModule == null)
            {
                Console.WriteLine("[FirstRunManager] Warning: SiteBuilder module not found");
                Console.WriteLine("[FirstRunManager] Skipping site initialization");
                MarkAsInitialized();
                return false;
            }
            
            Console.WriteLine("[FirstRunManager] Step 2/7: Generating wwwroot control panel...");
            Console.WriteLine();
            
            // Generate wwwroot directory with control panel files
            var wwwrootResult = siteBuilderModule.GenerateWwwroot();
            Console.WriteLine(wwwrootResult);
            Console.WriteLine();
            
            Console.WriteLine("[FirstRunManager] Step 3/7: Spawning CMS with Integrated Control Panel...");
            Console.WriteLine();
            
            // Spawn the site with integrated control panel
            var result = siteBuilderModule.Process("site spawn integrated");
            Console.WriteLine(result);
            Console.WriteLine();
            
            // Check if CMS was created successfully
            if (!Directory.Exists(_cmsPath))
            {
                Console.WriteLine("[FirstRunManager] Error: CMS directory was not created");
                return false;
            }
            
            Console.WriteLine("[FirstRunManager] Step 4/7: Configuring Nginx...");
            Console.WriteLine();
            
            // Configure Nginx - use port 80 for the integrated CMS
            var nginxManager = new NginxManager(_cmsPath, 80);
            
            if (NginxManager.IsNginxAvailable())
            {
                // First, create the CMS-specific Nginx config
                nginxManager.CreateNginxConfig();
                Console.WriteLine();
                Console.WriteLine("[FirstRunManager] Nginx configuration files created");
                Console.WriteLine("[FirstRunManager] See instructions above to enable Nginx for CMS");
                
                // Also configure reverse proxy for RaCore if not already done
                var configPath = NginxManager.FindNginxConfigPath();
                if (configPath != null)
                {
                    var config = File.ReadAllText(configPath);
                    if (!config.Contains("# RaCore Reverse Proxy Configuration"))
                    {
                        Console.WriteLine();
                        Console.WriteLine("[FirstRunManager] Configuring Nginx reverse proxy for RaCore...");
                        
                        var port = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "80";
                        var domain = Environment.GetEnvironmentVariable("RACORE_PROXY_DOMAIN") ?? "localhost";
                        var racorePort = int.Parse(port);
                        
                        var proxyManager = new NginxManager("", 80);
                        if (proxyManager.ConfigureReverseProxy(racorePort, domain))
                        {
                            Console.WriteLine($"[FirstRunManager] ✅ Nginx reverse proxy configured for {domain}");
                            Console.WriteLine($"[FirstRunManager] After Nginx restart, access RaCore at: http://{domain}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("[FirstRunManager] Nginx not available, will use PHP built-in server");
            }
            
            Console.WriteLine();
            Console.WriteLine("[FirstRunManager] Step 5/7: Initialization Guidance");
            Console.WriteLine();
            
            // Display guidance for completing initialization
            Console.WriteLine("📋 TO COMPLETE INITIALIZATION:");
            Console.WriteLine();
            Console.WriteLine("1. Start RaCore and access the Control Panel:");
            Console.WriteLine("   http://localhost/control-panel.html");
            Console.WriteLine();
            Console.WriteLine("2. Login with default credentials:");
            Console.WriteLine("   Username: admin");
            Console.WriteLine("   Password: admin123");
            Console.WriteLine();
            Console.WriteLine("3. IMPORTANT - Complete these steps:");
            Console.WriteLine("   a) Change the default admin password (REQUIRED for security)");
            Console.WriteLine("   b) Change the default admin username (RECOMMENDED)");
            Console.WriteLine("   c) Enter your License Key for validation");
            Console.WriteLine("   d) Configure server based on your license type");
            Console.WriteLine();
            Console.WriteLine($"4. Server Mode: {_serverConfig.Mode}");
            if (_serverConfig.Mode == ServerMode.Alpha || _serverConfig.Mode == ServerMode.Beta)
            {
                Console.WriteLine("   Note: Extended logging and debugging features are enabled");
            }
            else if (_serverConfig.Mode == ServerMode.Demo)
            {
                Console.WriteLine("   Note: Some features are limited in Demo mode");
            }
            Console.WriteLine();
            
            Console.WriteLine("[FirstRunManager] Step 6/7: License Validation");
            Console.WriteLine();
            Console.WriteLine("License validation will be performed when you:");
            Console.WriteLine("  - Enter your license key in the Control Panel");
            Console.WriteLine($"  - Validation server: {_serverConfig.MainServerUrl}");
            Console.WriteLine("  - Supported license types: Forum, CMS, GameServer, Enterprise");
            Console.WriteLine();
            
            Console.WriteLine("[FirstRunManager] Step 7/7: Ashat AI Assistant");
            Console.WriteLine();
            Console.WriteLine("The Ashat AI assistant can be enabled after initialization:");
            Console.WriteLine("  - Helps with server configuration and management");
            Console.WriteLine("  - Assists with building websites and game systems");
            Console.WriteLine("  - Available through the Control Panel");
            Console.WriteLine();
            
            // Check if PHP is available but don't start it
            var phpPath = FindPhpExecutable();
            if (phpPath != null)
            {
                Console.WriteLine("✅ CMS files have been generated!");
                Console.WriteLine($"   Location: {_cmsPath}");
                Console.WriteLine();
                Console.WriteLine("📋 To run the integrated CMS website:");
                Console.WriteLine();
                Console.WriteLine("The CMS is configured to run through Nginx + PHP-FPM on port 80:");
                Console.WriteLine("   1. Ensure Nginx is running (see instructions above)");
                Console.WriteLine("   2. Configure Nginx to serve the CMS directory on port 80");
                Console.WriteLine("   3. Start PHP-FPM service");
                Console.WriteLine();
                Console.WriteLine("🎛️  Default CMS Access:");
                Console.WriteLine("   Homepage: http://localhost (port 80)");
                Console.WriteLine("   Control Panel: http://localhost/control");
                Console.WriteLine("   Default login: admin / admin123");
                Console.WriteLine();
                Console.WriteLine("🎛️  ROLE-BASED ACCESS:");
                Console.WriteLine("   - SuperAdmin: Full control panel + user panel");
                Console.WriteLine("   - Admin: Admin control panel + user panel");
                Console.WriteLine("   - User: User panel only");
                Console.WriteLine();
                Console.WriteLine("⚠️  Change the default password immediately!");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("✅ CMS files have been generated!");
                Console.WriteLine($"   Location: {_cmsPath}");
                Console.WriteLine();
                Console.WriteLine("⚠️  PHP not found. Please install PHP 8+ to run the CMS");
                Console.WriteLine("   - Windows: Download from https://windows.php.net/download/");
                Console.WriteLine("   - Linux: sudo apt install php8.1 (or your distro's package manager)");
                Console.WriteLine("   - macOS: brew install php");
                Console.WriteLine();
            }
            
            // Mark as initialized
            MarkAsInitialized();
            
            Console.WriteLine("========================================");
            Console.WriteLine("   First-Run Initialization Complete");
            Console.WriteLine($"   Server Mode: {_serverConfig.Mode}");
            Console.WriteLine("   Next: Complete setup in Control Panel");
            Console.WriteLine("========================================");
            Console.WriteLine();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirstRunManager] Error during initialization: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return false;
        }
    }
    
    private string? FindPhpExecutable()
    {
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
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
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

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"[FirstRunManager] Found PHP at: {path}");
                        return path;
                    }
                }
            }
            catch { continue; }
        }

        // Provide helpful diagnostic message
        Console.WriteLine("[FirstRunManager] PHP not found in standard locations:");
        Console.WriteLine($"  - Local folder: {localPhpFolder}");
        Console.WriteLine("  - System PATH");
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Console.WriteLine("  - Common Windows paths (C:\\php, D:\\php, E:\\php, etc.)");
        }
        else
        {
            Console.WriteLine("  - /usr/bin/php, /usr/local/bin/php");
        }

        return null;
    }
}
