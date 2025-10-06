using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Core.SelfHealing;

namespace RaCore.Engine;

/// <summary>
/// Manages the boot sequence with self-healing checks and configuration verification
/// </summary>
public class BootSequenceManager
{
    private readonly ModuleManager _moduleManager;
    
    public BootSequenceManager(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }
    
    /// <summary>
    /// Performs the boot sequence: self-healing checks, configuration verification, and recovery
    /// </summary>
    public async Task<bool> ExecuteBootSequenceAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   RaCore Boot Sequence");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        var success = true;
        
        // Step 1: Run self-healing checks
        success &= await RunSelfHealingChecksAsync();
        
        // Step 2: Verify Apache configuration
        success &= VerifyApacheConfiguration();
        
        // Step 3: Verify PHP configuration
        success &= VerifyPhpConfiguration();
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine($"   Boot Sequence Complete: {(success ? "✅ Success" : "⚠️  With Warnings")}");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        return success;
    }
    
    private async Task<bool> RunSelfHealingChecksAsync()
    {
        Console.WriteLine("[BootSequence] Step 1/3: Running self-healing health checks...");
        Console.WriteLine();
        
        try
        {
            // Find SelfHealingModule
            var selfHealingModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<SelfHealingModule>()
                .FirstOrDefault();
            
            if (selfHealingModule == null)
            {
                Console.WriteLine("ℹ️  SelfHealingModule not found - skipping health checks");
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            // Run health checks on all modules
            var results = await selfHealingModule.CheckAllModulesAsync();
            
            // Display results
            var healthy = results.Values.Count(s => s.State == HealthState.Healthy);
            var degraded = results.Values.Count(s => s.State == HealthState.Degraded);
            var unhealthy = results.Values.Count(s => s.State == HealthState.Unhealthy);
            
            Console.WriteLine($"✅ Health check complete:");
            Console.WriteLine($"   Healthy: {healthy}");
            if (degraded > 0) Console.WriteLine($"   ⚠️  Degraded: {degraded}");
            if (unhealthy > 0) Console.WriteLine($"   ❌ Unhealthy: {unhealthy}");
            Console.WriteLine();
            
            // Attempt auto-recovery if needed
            if (degraded > 0 || unhealthy > 0)
            {
                Console.WriteLine("[BootSequence] Attempting auto-recovery...");
                
                var unhealthyModules = results.Values
                    .Where(s => s.State == HealthState.Unhealthy || s.State == HealthState.Degraded)
                    .ToList();
                
                foreach (var status in unhealthyModules)
                {
                    var action = new RecoveryAction
                    {
                        ActionType = "restart",
                        Description = $"Restart {status.ModuleName} module",
                        RequiresUserApproval = false
                    };
                    
                    var recovered = await selfHealingModule.AttemptRecoveryAsync(action);
                    Console.WriteLine($"   {status.ModuleName}: {(recovered ? "✅ Recovered" : "❌ Failed")}");
                }
                
                Console.WriteLine();
            }
            
            return unhealthy == 0; // Return false only if there are unhealthy modules
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error during self-healing checks: {ex.Message}");
            Console.WriteLine();
            return true; // Don't fail boot on self-healing errors
        }
    }
    
    private bool VerifyApacheConfiguration()
    {
        Console.WriteLine("[BootSequence] Step 2/3: Verifying Apache configuration...");
        Console.WriteLine();
        
        try
        {
            var apachePath = ApacheManager.FindApacheExecutable();
            
            if (apachePath == null)
            {
                Console.WriteLine("ℹ️  Apache not found - skipping Apache configuration");
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            Console.WriteLine($"✅ Apache found: {apachePath}");
            
            var configPath = ApacheManager.FindApacheConfigPath();
            if (configPath != null)
            {
                Console.WriteLine($"✅ Apache config found: {configPath}");
                
                // Check if reverse proxy modules are available
                var config = File.ReadAllText(configPath);
                var hasProxyModule = config.Contains("LoadModule proxy_module");
                var hasProxyHttpModule = config.Contains("LoadModule proxy_http_module");
                
                if (hasProxyModule && hasProxyHttpModule)
                {
                    Console.WriteLine("✅ Reverse proxy modules available");
                }
                else
                {
                    Console.WriteLine("ℹ️  Reverse proxy modules not enabled (use ConfigureReverseProxy to enable)");
                }
            }
            else
            {
                Console.WriteLine("⚠️  Apache config file not found");
            }
            
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error verifying Apache: {ex.Message}");
            Console.WriteLine();
            return true; // Don't fail boot on Apache verification errors
        }
    }
    
    private bool VerifyPhpConfiguration()
    {
        Console.WriteLine("[BootSequence] Step 3/3: Verifying PHP configuration...");
        Console.WriteLine();
        
        try
        {
            var phpPath = ApacheManager.FindPhpExecutable();
            
            if (phpPath == null)
            {
                Console.WriteLine("⚠️  PHP not found in common locations");
                Console.WriteLine("   Install PHP 8+ to run CMS and web applications");
                Console.WriteLine();
                return true; // Not a fatal error for RaCore itself
            }
            
            Console.WriteLine($"✅ PHP found: {phpPath}");
            
            // Get PHP version
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = phpPath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var firstLine = output.Split('\n')[0];
                    Console.WriteLine($"   Version: {firstLine}");
                    process.WaitForExit(5000);
                }
            }
            catch { }
            
            // Check for php.ini
            var phpIniPath = ApacheManager.FindPhpIniPath(phpPath);
            if (phpIniPath != null)
            {
                Console.WriteLine($"✅ PHP config found: {phpIniPath}");
            }
            else
            {
                Console.WriteLine("ℹ️  PHP config (php.ini) not found - using defaults");
            }
            
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error verifying PHP: {ex.Message}");
            Console.WriteLine();
            return true; // Don't fail boot on PHP verification errors
        }
    }
}
