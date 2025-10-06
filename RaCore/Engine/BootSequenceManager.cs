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
        // Display kawaii Ra's All Seeing Eye logo
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine("          .:*~*:.");
        Console.WriteLine("        .*###%%%###*.");
        Console.WriteLine("       *#%%%◆◆◆%%%#*");
        Console.WriteLine("      .#%%◆     ◆%%#.");
        Console.WriteLine("      *%%◆  ●●●  ◆%%*");
        Console.WriteLine("      *%%◆ ●███● ◆%%*");
        Console.WriteLine("      .#%%◆ ●●●  ◆%%#.");
        Console.WriteLine("       *#%%%◆◆◆%%%#*");
        Console.WriteLine("        .*###%%%###*.");
        Console.WriteLine("          ':*~*:'");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ✧･ﾟ: *✧･ﾟ:* Welcome to Ra OS v.4.7 *:･ﾟ✧*:･ﾟ✧");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("    ♡ ～(つˆ0ˆ)つ｡☆  Booting up with love!  ☆｡(⊃｡•́‿•̀｡)⊃ ♡");
        Console.ResetColor();
        Console.WriteLine();
        
        var success = true;
        
        // Step 1: Run self-healing checks
        success &= await RunSelfHealingChecksAsync();
        
        // Step 2: Verify Apache configuration
        success &= VerifyApacheConfiguration();
        
        // Step 3: Verify PHP configuration
        success &= VerifyPhpConfiguration();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"      Boot Complete: {(success ? "✨ Success! ✨ (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧" : "⚠️  With Warnings (´･ω･`)")}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿✿");
        Console.ResetColor();
        Console.WriteLine();
        
        return success;
    }
    
    private async Task<bool> RunSelfHealingChecksAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭ˊᵕˋ)੭ Step 1/3: Health Check!  │");
        Console.WriteLine("    ╰─────────────────────────────────────╯");
        Console.ResetColor();
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
                Console.WriteLine("    (｡•́︿•̀｡) SelfHealingModule not found - skipping...");
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            // Run health checks on all modules
            var results = await selfHealingModule.CheckAllModulesAsync();
            
            // Display results
            var healthy = results.Values.Count(s => s.State == HealthState.Healthy);
            var degraded = results.Values.Count(s => s.State == HealthState.Degraded);
            var unhealthy = results.Values.Count(s => s.State == HealthState.Unhealthy);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    ✨ Health check complete! ✨");
            Console.ResetColor();
            Console.WriteLine($"       (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ Healthy: {healthy}");
            if (degraded > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"       (´･ω･`) Degraded: {degraded}");
                Console.ResetColor();
            }
            if (unhealthy > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"       (╥﹏╥) Unhealthy: {unhealthy}");
                Console.ResetColor();
            }
            Console.WriteLine();
            
            // Attempt auto-recovery if needed
            if (degraded > 0 || unhealthy > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("    ♡ (っ◔◡◔)っ Attempting auto-recovery with love...");
                Console.ResetColor();
                
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
                    var emoji = recovered ? "✨💚" : "💔";
                    Console.WriteLine($"       {emoji} {status.ModuleName}: {(recovered ? "Healed! (◕‿◕✿)" : "Needs help (╥﹏╥)")}");
                }
                
                Console.WriteLine();
            }
            
            // Run system-wide diagnostics
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    🔍 Running system-wide diagnostics...");
            Console.ResetColor();
            
            var diagnosticReport = selfHealingModule.Process("diagnose fix");
            var diagnosticLines = diagnosticReport.Split('\n');
            
            // Show only summary and critical info during boot
            var showLine = false;
            foreach (var line in diagnosticLines)
            {
                if (line.Contains("Summary:") || line.Contains("Repair Summary:"))
                {
                    showLine = true;
                }
                
                if (showLine && !string.IsNullOrWhiteSpace(line))
                {
                    if (line.Contains("✓") || line.Contains("✗"))
                    {
                        Console.ForegroundColor = line.Contains("✓") ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine($"       {line.Trim()}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"       {line.Trim()}");
                    }
                }
                
                if (line.Contains("Repair Summary:"))
                {
                    break;
                }
            }
            Console.WriteLine();
            
            return unhealthy == 0; // Return false only if there are unhealthy modules
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    (╥﹏╥) Oopsie! Error during health checks: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on self-healing errors
        }
    }
    
    private bool VerifyApacheConfiguration()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭*ˊᵕˋ)੭* Step 2/3: Apache Check! │");
        Console.WriteLine("    ╰─────────────────────────────────────╯");
        Console.ResetColor();
        Console.WriteLine();
        
        try
        {
            var apachePath = ApacheManager.FindApacheExecutable();
            
            if (apachePath == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    (｡•́︿•̀｡) Apache not found - that's okay!");
                Console.ResetColor();
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    ✨ Apache found: {apachePath}");
            Console.ResetColor();
            
            var configPath = ApacheManager.FindApacheConfigPath();
            if (configPath != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ♡ Config found: {configPath}");
                Console.ResetColor();
                
                // Check if reverse proxy modules are available
                var config = File.ReadAllText(configPath);
                var hasProxyModule = config.Contains("LoadModule proxy_module");
                var hasProxyHttpModule = config.Contains("LoadModule proxy_http_module");
                var hasRaCoreProxy = config.Contains("# RaCore Reverse Proxy Configuration");
                
                if (hasProxyModule && hasProxyHttpModule)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("    (◕‿◕✿) Reverse proxy modules ready!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("    (•ᴗ•) Proxy modules not enabled yet - no worries!");
                    Console.ResetColor();
                }
                
                var domain = Environment.GetEnvironmentVariable("RACORE_PROXY_DOMAIN") ?? "localhost";
                
                // Check if RaCore reverse proxy is already configured
                if (!hasRaCoreProxy)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("    ♡ (っ◔◡◔)っ Auto-configuring Apache reverse proxy...");
                    Console.ResetColor();
                    
                    // Use environment variable only for initial setup, fallback to 5000
                    var portEnv = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "5000";
                    var initialPort = int.Parse(portEnv);
                    
                    var apacheManager = new ApacheManager("", 8080);
                    var success = apacheManager.ConfigureReverseProxy(initialPort, domain);
                    
                    if (success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"    ✨ Apache reverse proxy configured for {domain}!");
                        Console.WriteLine($"    ♡ Access RaCore at:");
                        Console.WriteLine($"       - http://{domain}");
                        Console.WriteLine($"       - http://agpstudios.online");
                        Console.WriteLine($"       - http://www.agpstudios.online");
                        Console.WriteLine($"    (after Apache restart)");
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("    ⚠️  Please restart Apache for changes to take effect");
                        Console.ResetColor();
                        
                        // Store detected port for RaCore to use
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", initialPort.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("    (´･ω･`) Could not auto-configure Apache");
                        Console.WriteLine("    No worries! You can configure it manually later.");
                        Console.ResetColor();
                    }
                }
                else
                {
                    // RaCore proxy is already configured - detect port from Apache
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("    ♡ (◕‿◕✿) RaCore reverse proxy already configured!");
                    Console.ResetColor();
                    
                    // Detect configured port in Apache - this is the source of truth
                    var configuredPort = ApacheManager.GetConfiguredRaCorePort();
                    if (configuredPort.HasValue)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"    📋 Apache is configured to proxy to port {configuredPort.Value}");
                        Console.WriteLine($"    ♡ RaCore will use port {configuredPort.Value} from Apache configuration");
                        Console.ResetColor();
                        
                        // Store detected port for RaCore to use
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", configuredPort.Value.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("    (´･ω･`) Could not detect port from Apache config");
                        Console.WriteLine("    Using default port 5000");
                        Console.ResetColor();
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", "5000");
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    (´･ω･`) Config file not found");
                Console.ResetColor();
            }
            
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    (╥﹏╥) Oopsie! Error with Apache: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on Apache verification errors
        }
    }
    
    private bool VerifyPhpConfiguration()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭ˊ꒳ˋ)੭✧ Step 3/3: PHP Check!   │");
        Console.WriteLine("    ╰─────────────────────────────────────╯");
        Console.ResetColor();
        Console.WriteLine();
        
        try
        {
            var phpPath = ApacheManager.FindPhpExecutable();
            
            if (phpPath == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    (｡•́︿•̀｡) PHP not found - install PHP 8+ for CMS!");
                Console.ResetColor();
                Console.WriteLine();
                return true; // Not a fatal error for RaCore itself
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    ✨ PHP found: {phpPath}");
            Console.ResetColor();
            
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
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"       ♡ Version: {firstLine}");
                    Console.ResetColor();
                    process.WaitForExit(5000);
                }
            }
            catch { }
            
            // Check for php.ini
            var phpIniPath = ApacheManager.FindPhpIniPath(phpPath);
            if (phpIniPath != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ♡ Config found: {phpIniPath}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("    (•ᴗ•) No php.ini found - using defaults!");
                Console.ResetColor();
            }
            
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    (╥﹏╥) Oopsie! Error with PHP: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on PHP verification errors
        }
    }
}
