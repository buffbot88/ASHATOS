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
        
        // Step 2: Verify Nginx configuration
        success &= VerifyWebServerConfiguration();
        
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
        Console.WriteLine("    │  ଘ(੭ˊᵕˋ)੭ Step 1/4: Health Check!  │");
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
        Console.WriteLine("    │  ଘ(੭ˊᵕˋ)੭ Step 1.5: .gguf Processing! │");
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
    
    private bool VerifyWebServerConfiguration()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭*ˊᵕˋ)੭* Step 2/4: Nginx Check!  │");
        Console.WriteLine("    ╰─────────────────────────────────────╯");
        Console.ResetColor();
        Console.WriteLine();
        
        // Kill switch: Skip Nginx verification if environment variable is set
        var skipNginx = Environment.GetEnvironmentVariable("RACORE_SKIP_NGINX_CHECK");
        if (!string.IsNullOrEmpty(skipNginx) && skipNginx.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ⚠️  Nginx verification skipped (RACORE_SKIP_NGINX_CHECK=true)");
            Console.ResetColor();
            Console.WriteLine();
            return true;
        }
        
        // Display server IP address
        var serverIp = NginxManager.GetServerIpAddress();
        if (serverIp != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    🌐 Server IP Address: {serverIp}");
            Console.ResetColor();
            StoreConfig("server.ip_address", serverIp);
        }
        
        try
        {
            var nginxPath = NginxManager.FindNginxExecutable();
            
            if (nginxPath == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    (｡•́︿•̀｡) Nginx not found - that's okay!");
                Console.ResetColor();
                
                // When Nginx is not available, use RACORE_PORT environment variable if set
                var portEnv = Environment.GetEnvironmentVariable("RACORE_PORT");
                if (!string.IsNullOrEmpty(portEnv))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"    📋 Using RACORE_PORT environment variable: {portEnv}");
                    Console.ResetColor();
                    Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", portEnv);
                    StoreConfig("nginx.port", portEnv);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("    📋 Using default port: 80");
                    Console.ResetColor();
                    Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", "80");
                    StoreConfig("nginx.port", "80");
                }
                
                Console.WriteLine();
                return true; // Not a fatal error
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    ✨ Nginx found: {nginxPath}");
            Console.ResetColor();
            
            // Verify Nginx configuration
            var (configExists, configValid, configMessage) = NginxManager.VerifyNginxConfig();
            
            if (!configExists)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    ⚠️  {configMessage}");
                Console.ResetColor();
            }
            else if (!configValid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    ⚠️  {configMessage}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ✅ {configMessage}");
                Console.ResetColor();
            }
            
            var configPath = NginxManager.FindNginxConfigPath();
            if (configPath != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ♡ Config found: {configPath}");
                Console.ResetColor();
                
                // Generate Nginx configuration template in local config folder
                var serverRoot = Directory.GetCurrentDirectory();
                var configFolder = Path.Combine(serverRoot, "config");
                Directory.CreateDirectory(configFolder);
                
                // Check if RaCore reverse proxy is already configured in system
                var config = File.ReadAllText(configPath);
                var hasRaCoreProxy = config.Contains("# RaCore Reverse Proxy Configuration");
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    (◕‿◕✿) Nginx is available for configuration!");
                Console.ResetColor();
                
                var domain = Environment.GetEnvironmentVariable("RACORE_PROXY_DOMAIN") ?? serverIp ?? "localhost";
                
                // Check if RaCore reverse proxy is already configured in system
                if (!hasRaCoreProxy)
                {
                    // Generate config template in local folder
                    var localNginxConfigPath = Path.Combine(configFolder, "nginx-racore.conf");
                    
                    // Check if template already exists
                    if (File.Exists(localNginxConfigPath))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"    ✨ Nginx configuration template already exists: {localNginxConfigPath}");
                        Console.ResetColor();
                        
                        // Use environment variable or default port
                        var portEnv = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "80";
                        var existingPort = int.Parse(portEnv);
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", existingPort.ToString());
                        StoreConfig("nginx.port", existingPort.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("    ♡ (っ◔◡◔)っ Generating Nginx reverse proxy configuration template...");
                        Console.ResetColor();
                        
                        // Use environment variable only for initial setup, fallback to 80
                        var portEnv = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "80";
                        var initialPort = int.Parse(portEnv);
                        
                        var serverNames = new List<string> { domain };
                        if (serverIp != null && serverIp != domain)
                        {
                            serverNames.Add(serverIp);
                        }
                        serverNames.AddRange(new[] { "agpstudios.online", "www.agpstudios.online" });
                        var serverNameList = string.Join(" ", serverNames);
                        
                        var nginxConfigTemplate = $@"# RaCore Reverse Proxy Configuration
# Auto-generated by RaCore NginxManager on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
# This configuration allows accessing RaCore at http://{domain} (port 80)
# Server IP: {serverIp ?? "not detected"}

server {{
    listen 80;
    server_name {serverNameList};

    # Logging
    access_log logs/racore-access.log;
    error_log logs/racore-error.log;

    location / {{
        proxy_pass http://localhost:{initialPort};
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection ""upgrade"";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support
        proxy_read_timeout 86400;
        proxy_send_timeout 86400;
        
        # Buffering
        proxy_buffering off;
    }}
}}
";
                        
                        try
                        {
                            File.WriteAllText(localNginxConfigPath, nginxConfigTemplate);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"    ✨ Nginx configuration template generated: {localNginxConfigPath}");
                            Console.ResetColor();
                            
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine();
                            Console.WriteLine("    📋 To enable reverse proxy, add this config to your Nginx:");
                            Console.WriteLine("       Windows: Add 'include' directive in C:\\nginx\\conf\\nginx.conf http block:");
                            Console.WriteLine($"                include {localNginxConfigPath.Replace("\\", "/")};");
                            Console.WriteLine("       Linux/Mac: Copy to /etc/nginx/sites-available/ and symlink to sites-enabled/");
                            Console.WriteLine();
                            Console.WriteLine("    ⚠️  Then manually start/restart Nginx:");
                            Console.WriteLine("       Windows: cd C:\\nginx && start nginx  (or: nginx -s reload)");
                            Console.WriteLine("       Linux/Mac: sudo systemctl start nginx  (or: sudo systemctl reload nginx)");
                            Console.ResetColor();
                            
                            // Store detected port for RaCore to use
                            Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", initialPort.ToString());
                            
                            // Persist configuration to Ra_Memory database
                            StoreConfig("nginx.port", initialPort.ToString());
                            StoreConfig("nginx.domain", domain);
                            StoreConfig("nginx.config_template", localNginxConfigPath);
                            StoreConfig("nginx.template_generated", "true");
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"    (´･ω･`) Could not generate Nginx config template: {ex.Message}");
                            Console.ResetColor();
                            
                            // If config generation failed, use RACORE_PORT environment variable if set
                            var fallbackPortEnv = Environment.GetEnvironmentVariable("RACORE_PORT") ?? "80";
                            var fallbackPort = int.Parse(fallbackPortEnv);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"    📋 Using port {fallbackPort} from RACORE_PORT");
                            Console.ResetColor();
                            Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", fallbackPort.ToString());
                            StoreConfig("nginx.port", fallbackPort.ToString());
                        }
                    }
                }
                else
                {
                    // RaCore proxy is already configured - detect port from Nginx
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("    ✨ RaCore reverse proxy already configured!");
                    Console.ResetColor();
                    
                    // Detect configured port in Nginx - this is the source of truth
                    var configuredPort = NginxManager.GetConfiguredRaCorePort();
                    
                    if (configuredPort.HasValue)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"    📋 Nginx is configured to proxy to port {configuredPort.Value}");
                        Console.WriteLine($"    ♡ RaCore will use port {configuredPort.Value} from Nginx configuration");
                        Console.ResetColor();
                        
                        // Store the detected port from Nginx as the source of truth
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", configuredPort.Value.ToString());
                        
                        // Persist configuration to Ra_Memory database
                        StoreConfig("nginx.port", configuredPort.Value.ToString());
                        StoreConfig("nginx.configured", "true");
                    }
                    else
                    {
                        // If we can't detect port from Nginx, fall back to default 80
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("    (´･ω･`) Could not detect port from Nginx config");
                        Console.WriteLine("    🔧 Using default port 80");
                        Console.ResetColor();
                        
                        Environment.SetEnvironmentVariable("RACORE_DETECTED_PORT", "80");
                        
                        // Store default configuration to Ra_Memory database
                        StoreConfig("nginx.port", "80");
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
            Console.WriteLine($"    ❌ NGINX VERIFICATION ERROR");
            Console.WriteLine($"    Reason: {ex.Message}");
            Console.WriteLine($"    Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"    Stack Trace: {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "N/A"}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    💡 To skip Nginx verification, set environment variable:");
            Console.WriteLine($"       RACORE_SKIP_NGINX_CHECK=true");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on Nginx verification errors
        }
    }
    
    private bool VerifyPhpConfiguration()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    ╭─────────────────────────────────────╮");
        Console.WriteLine("    │  ଘ(੭ˊ꒳ˋ)੭✧ Step 3/4: PHP Check!   │");
        Console.WriteLine("    ╰─────────────────────────────────────╯");
        Console.ResetColor();
        Console.WriteLine();
        
        // Kill switch: Skip PHP verification if environment variable is set
        var skipPhp = Environment.GetEnvironmentVariable("RACORE_SKIP_PHP_CHECK");
        if (!string.IsNullOrEmpty(skipPhp) && skipPhp.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ⚠️  PHP verification skipped (RACORE_SKIP_PHP_CHECK=true)");
            Console.ResetColor();
            Console.WriteLine();
            return true;
        }
        
        try
        {
            var phpPath = NginxManager.FindPhpExecutable();
            
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
            
            // Check for PHP configuration template in local config folder
            var serverRoot = Directory.GetCurrentDirectory();
            var configFolder = Path.Combine(serverRoot, "config");
            Directory.CreateDirectory(configFolder);
            
            var localPhpIniPath = Path.Combine(configFolder, "php.ini");
            
            // Check if template already exists
            if (File.Exists(localPhpIniPath))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ✨ PHP configuration template already exists: {localPhpIniPath}");
                Console.ResetColor();
                
                // Check if PHP can find its config
                var phpIniPath = NginxManager.FindPhpIniPath(phpPath);
                if (phpIniPath != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"    📋 PHP is currently using: {phpIniPath}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    (っ◔◡◔)っ Generating PHP configuration template...");
                Console.ResetColor();
                
                var generateSuccess = NginxManager.GeneratePhpIni(localPhpIniPath);
                
                if (generateSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"    ✨ PHP configuration template generated: {localPhpIniPath}");
                    Console.ResetColor();
                    
                    // Check if PHP can find its config
                    var phpIniPath = NginxManager.FindPhpIniPath(phpPath);
                    if (phpIniPath != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"    📋 PHP is currently using: {phpIniPath}");
                        Console.WriteLine($"    💡 To use the generated config, copy:");
                        Console.WriteLine($"       FROM: {localPhpIniPath}");
                        Console.WriteLine($"       TO:   {phpIniPath}");
                        Console.ResetColor();
                    }
                    else
                    {
                        var suggestedSystemPath = NginxManager.GetSuggestedPhpIniPath(phpPath);
                        if (suggestedSystemPath != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"    💡 To use the generated config, copy:");
                            Console.WriteLine($"       FROM: {localPhpIniPath}");
                            Console.WriteLine($"       TO:   {suggestedSystemPath}");
                            Console.ResetColor();
                        }
                    }
                    
                    // Persist PHP configuration to Ra_Memory database
                    StoreConfig("php.config_template", localPhpIniPath);
                    StoreConfig("php.template_generated", "true");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("    ❌ PHP TEMPLATE GENERATION FAILED");
                    Console.WriteLine("    Reason: Unknown error during template generation");
                    Console.WriteLine($"    Target Path: {localPhpIniPath}");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"    💡 To skip PHP verification, set environment variable:");
                    Console.WriteLine($"       RACORE_SKIP_PHP_CHECK=true");
                    Console.ResetColor();
                }
            }
            
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    ❌ PHP VERIFICATION ERROR");
            Console.WriteLine($"    Reason: {ex.Message}");
            Console.WriteLine($"    Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"    Stack Trace: {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "N/A"}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    💡 To skip PHP verification, set environment variable:");
            Console.WriteLine($"       RACORE_SKIP_PHP_CHECK=true");
            Console.ResetColor();
            Console.WriteLine();
            return true; // Don't fail boot on PHP verification errors
        }
    }
}
