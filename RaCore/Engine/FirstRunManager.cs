using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.SiteBuilder;
using System.Text.Json;

namespace RaCore.Engine;

/// <summary>
/// Manages first-run initialization for RaCore, including CMS spawning and Apache setup
/// </summary>
public class FirstRunManager
{
    private readonly string _firstRunMarkerPath;
    private readonly string _cmsPath;
    private readonly ModuleManager _moduleManager;
    
    public FirstRunManager(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
        // Use GetCurrentDirectory() to get the RaCore.exe server root directory (where the executable runs)
        var serverRoot = Directory.GetCurrentDirectory();
        _firstRunMarkerPath = Path.Combine(serverRoot, ".racore_initialized");
        _cmsPath = Path.Combine(serverRoot, "racore_cms");
    }
    
    /// <summary>
    /// Checks if this is the first run of RaCore
    /// </summary>
    public bool IsFirstRun()
    {
        return !File.Exists(_firstRunMarkerPath);
    }
    
    /// <summary>
    /// Marks the system as initialized
    /// </summary>
    public void MarkAsInitialized()
    {
        try
        {
            var initInfo = new
            {
                InitializedAt = DateTime.UtcNow,
                Version = "1.0",
                CmsPath = _cmsPath
            };
            
            File.WriteAllText(_firstRunMarkerPath, JsonSerializer.Serialize(initInfo, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("[FirstRunManager] System marked as initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FirstRunManager] Warning: Could not create initialization marker: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Performs first-run initialization: spawns CMS with integrated Control Panel and configures Apache
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   RaCore First-Run Initialization");
        Console.WriteLine("   CMS + Integrated Control Panel");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        try
        {
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
            
            Console.WriteLine("[FirstRunManager] Step 1/4: Generating wwwroot control panel...");
            Console.WriteLine();
            
            // Generate wwwroot directory with control panel files
            var wwwrootResult = siteBuilderModule.GenerateWwwroot();
            Console.WriteLine(wwwrootResult);
            Console.WriteLine();
            
            Console.WriteLine("[FirstRunManager] Step 2/4: Spawning CMS with Integrated Control Panel...");
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
            
            Console.WriteLine("[FirstRunManager] Step 3/4: Configuring Apache...");
            Console.WriteLine();
            
            // Configure Apache
            var apacheManager = new ApacheManager(_cmsPath, 8080);
            
            if (ApacheManager.IsApacheAvailable())
            {
                // First, configure the CMS-specific Apache config (for Linux)
                apacheManager.ConfigureApache();
                Console.WriteLine();
                Console.WriteLine("[FirstRunManager] Apache configuration files created");
                Console.WriteLine("[FirstRunManager] See instructions above to enable Apache for CMS");
                
                // Also configure reverse proxy for RaCore if not already done
                var configPath = ApacheManager.FindApacheConfigPath();
                if (configPath != null)
                {
                    var config = File.ReadAllText(configPath);
                    if (!config.Contains("# RaCore Reverse Proxy Configuration"))
                    {
                        Console.WriteLine();
                        Console.WriteLine("[FirstRunManager] Configuring Apache reverse proxy for RaCore...");
                        
                        var port = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "5000";
                        var domain = Environment.GetEnvironmentVariable("RACORE_PROXY_DOMAIN") ?? "localhost";
                        var racorePort = int.Parse(port);
                        
                        var proxyManager = new ApacheManager("", 8080);
                        if (proxyManager.ConfigureReverseProxy(racorePort, domain))
                        {
                            Console.WriteLine($"[FirstRunManager] ✅ Apache reverse proxy configured for {domain}");
                            Console.WriteLine($"[FirstRunManager] After Apache restart, access RaCore at: http://{domain}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("[FirstRunManager] Apache not available, will use PHP built-in server");
            }
            
            Console.WriteLine();
            Console.WriteLine("[FirstRunManager] Step 4/4: Starting web server...");
            Console.WriteLine();
            
            // Try to start PHP server
            var phpPath = FindPhpExecutable();
            if (phpPath != null)
            {
                var started = apacheManager.StartPhpServer(phpPath);
                if (started)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ RaCore CMS is now running!");
                    Console.WriteLine($"   Homepage: http://localhost:8080");
                    Console.WriteLine($"   Control Panel: http://localhost:8080/control");
                    Console.WriteLine($"   Default login: admin / admin123");
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
                    Console.WriteLine("[FirstRunManager] Warning: Could not start PHP server automatically");
                    Console.WriteLine($"[FirstRunManager] You can start it manually: cd {_cmsPath} && php -S localhost:8080");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("[FirstRunManager] PHP not found. Install PHP 8+ to run the CMS");
                Console.WriteLine();
            }
            
            // Mark as initialized
            MarkAsInitialized();
            
            Console.WriteLine("========================================");
            Console.WriteLine("   First-Run Initialization Complete");
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
