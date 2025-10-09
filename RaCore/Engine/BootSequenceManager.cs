using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using RaCore.Modules.Core.SelfHealing;
using RaCore.Modules.Core.LanguageModelProcessor;

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
    /// Stores a server configuration setting in Ra_Memory for persistence
    /// </summary>
    private void StoreConfig(string key, string value)
    {
        try
        {
            var memoryModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<MemoryModule>()
                .FirstOrDefault();
            
            if (memoryModule != null)
            {
                // Store in Memory database for persistence across restarts
                memoryModule.RememberAsync($"server.config.{key}", value, 
                    new Dictionary<string, string> 
                    { 
                        { "type", "server_config" },
                        { "updated", DateTime.UtcNow.ToString("o") }
                    }).Wait();
            }
        }
        catch (Exception ex)
        {
            // Don't fail boot if memory storage fails
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    (´･ω･`) Could not persist config to Ra_Memory: {ex.Message}");
            Console.ResetColor();
        }
    }
    
    /// <summary>
    /// Retrieves a server configuration setting from Ra_Memory
    /// </summary>
    private string? GetConfig(string key)
    {
        try
        {
            var memoryModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<MemoryModule>()
                .FirstOrDefault();
            
            if (memoryModule != null)
            {
                var items = memoryModule.GetAllItems();
                var item = items.FirstOrDefault(i => i.Key == $"server.config.{key}");
                return item?.Value;
            }
        }
        catch
        {
            // Silently fail and return null
        }
        return null;
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
        Console.WriteLine("      *%%◆  ○○○  ◆%%*");
        Console.WriteLine("      *%%◆ ○◉◉◉○ ◆%%*");
        Console.WriteLine("      .#%%◆  ○○○  ◆%%#.");
        Console.WriteLine("       *#%%%◆◆◆%%%#*");
        Console.WriteLine("        .*###%%%###*.");
        Console.WriteLine("          ':*~*:'");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    ✧･ﾟ: *✧･ﾟ:* Welcome to Ra OS v{RaVersion.Current} *:･ﾟ✧*:･ﾟ✧");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("    ♡ ～(つˆ0ˆ)つ｡☆  Booting up with love!  ☆｡(⊃｡•́‿•̀｡)⊃ ♡");
        Console.ResetColor();
        Console.WriteLine();
        
        var success = true;
        
        // Step 1: Run self-healing checks
        success &= await RunSelfHealingChecksAsync();
        
        // Step 1.5: Process .gguf language models
        success &= await ProcessLanguageModelsAsync();
        
        // Note: Nginx and PHP scanning removed - RaOS processes PHP internally
        // and uses Kestrel as web server. External web servers should be
        // configured by the host environment before running RaOS.
        
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
        Console.WriteLine("    │  ଘ(੭ˊᵕˋ)੭ Step 1/2: Health Check!  │");
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
    
    private async Task<bool> ProcessLanguageModelsAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭ˊᵕˋ)੭ Step 2/2: .gguf Processing! │");
        Console.WriteLine("    ╰─────────────────────────────────────────╯");
        Console.ResetColor();
        Console.WriteLine();
        
        try
        {
            // Find LanguageModelProcessorModule
            var lmProcessorModule = _moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<LanguageModelProcessorModule>()
                .FirstOrDefault();
            
            if (lmProcessorModule == null)
            {
                Console.WriteLine("    (｡•́︿•̀｡) LanguageModelProcessor not found - skipping...");
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("    ♡ (っ◔◡◔)っ Processing language models...");
            Console.ResetColor();
            
            // Process all .gguf models
            var result = await lmProcessorModule.ProcessModelsAsync();
            
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    ✨ Language model processing complete! ✨");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    (´･ω･`) Model processing had issues but continuing...");
                Console.ResetColor();
            }
            
            // Get status summary
            var status = lmProcessorModule.Process("status");
            var statusLines = status.Split('\n');
            
            // Display summary
            foreach (var line in statusLines.Take(4)) // Show first 4 lines (header + counts)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine($"       {line.Trim()}");
                }
            }
            
            Console.WriteLine();
            return true; // Always continue boot
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    (╥﹏╥) Oopsie! Error processing models: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on model processing errors
        }
    }
}
