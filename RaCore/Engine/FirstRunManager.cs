using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.CMSSpawner;
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
        _firstRunMarkerPath = Path.Combine(AppContext.BaseDirectory, ".racore_initialized");
        _cmsPath = Path.Combine(AppContext.BaseDirectory, "superadmin_control_panel");
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
    /// Performs first-run initialization: spawns Unified Control Panel and configures Apache
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   RaCore First-Run Initialization");
        Console.WriteLine("   Unified Control Panel Setup");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        try
        {
            // Find CMSSpawner module
            var cmsModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<CMSSpawnerModule>()
                .FirstOrDefault();
            
            if (cmsModule == null)
            {
                Console.WriteLine("[FirstRunManager] Warning: CMSSpawner module not found");
                Console.WriteLine("[FirstRunManager] Skipping CMS initialization");
                MarkAsInitialized();
                return false;
            }
            
            Console.WriteLine("[FirstRunManager] Step 1/3: Spawning Unified Control Panel...");
            Console.WriteLine();
            
            // Spawn the Unified Control Panel
            var result = cmsModule.Process("cms spawn control");
            Console.WriteLine(result);
            Console.WriteLine();
            
            // Check if Control Panel was created successfully
            if (!Directory.Exists(_cmsPath))
            {
                Console.WriteLine("[FirstRunManager] Error: Control Panel directory was not created");
                return false;
            }
            
            Console.WriteLine("[FirstRunManager] Step 2/3: Configuring Apache...");
            Console.WriteLine();
            
            // Configure Apache
            var apacheManager = new ApacheManager(_cmsPath, 8080);
            
            if (ApacheManager.IsApacheAvailable())
            {
                apacheManager.ConfigureApache();
                Console.WriteLine();
                Console.WriteLine("[FirstRunManager] Apache configuration files created");
                Console.WriteLine("[FirstRunManager] See instructions above to enable Apache");
            }
            else
            {
                Console.WriteLine("[FirstRunManager] Apache not available, will use PHP built-in server");
            }
            
            Console.WriteLine();
            Console.WriteLine("[FirstRunManager] Step 3/3: Starting web server...");
            Console.WriteLine();
            
            // Try to start PHP server
            var phpPath = FindPhpExecutable();
            if (phpPath != null)
            {
                var started = apacheManager.StartPhpServer(phpPath);
                if (started)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ Unified Control Panel is now running!");
                    Console.WriteLine($"   Access it at: http://localhost:8080");
                    Console.WriteLine($"   Default login: admin / admin123");
                    Console.WriteLine();
                    Console.WriteLine("🎛️  ROLE-BASED ACCESS:");
                    Console.WriteLine("   - SuperAdmin: Full control (license mgmt, server spawning)");
                    Console.WriteLine("   - Admin: User management and audit logs");
                    Console.WriteLine("   - User: Personal account information");
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
                Console.WriteLine("[FirstRunManager] PHP not found. Install PHP 8+ to run the Control Panel");
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
        string[] possiblePaths = 
        {
            "php",
            "/usr/bin/php",
            "/usr/local/bin/php",
            "C:\\php\\php.exe",
            "C:\\xampp\\php\\php.exe"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
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
                        return path;
                    }
                }
            }
            catch { continue; }
        }

        return null;
    }
}
