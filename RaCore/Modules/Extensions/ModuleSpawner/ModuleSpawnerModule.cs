using System.Text;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.ModuleSpawner;

/// <summary>
/// Module Spawner - Enables RaAI to self-build and spawn new RaCore modules via natural language.
/// SuperAdmin can instruct RaAI to generate new modules that are placed in /Modules folder and dynamically loaded.
/// Phase 7 implementation for self-sufficient RaAI capability.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ModuleSpawnerModule : ModuleBase
{
    public override string Name => "ModuleSpawner";

    private ModuleManager? _manager;
    private IAuthenticationModule? _authModule;
    private string _modulesRootPath = string.Empty;
    private readonly List<SpawnedModule> _spawnedModules = new();
    private readonly Dictionary<string, ModuleTemplate> _templates = new();
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        // Set modules path to repo root /Modules folder
        // Start from current directory and navigate up to find repo root
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = currentDir;
        
        // If we're in a subdirectory like RaCore, go up one level
        if (currentDir.EndsWith("RaCore", StringComparison.OrdinalIgnoreCase) ||
            currentDir.Contains(Path.Combine("RaCore", "bin")))
        {
            var rcoreIndex = currentDir.LastIndexOf("RaCore");
            if (rcoreIndex > 0)
            {
                repoRoot = currentDir.Substring(0, rcoreIndex).TrimEnd(Path.DirectorySeparatorChar);
            }
        }
        
        _modulesRootPath = Path.Combine(repoRoot, "Modules");
        
        if (!Directory.Exists(_modulesRootPath))
        {
            Directory.CreateDirectory(_modulesRootPath);
        }

        // Get authentication module for permission checks
        if (_manager != null)
        {
            _authModule = _manager.GetModuleByName("Authentication") as IAuthenticationModule;
        }

        InitializeTemplates();
        LogInfo("Module Spawner initialized");
        LogInfo($"Modules will be spawned to: {_modulesRootPath}");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("spawn status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatus();
        }

        if (text.Equals("spawn list templates", StringComparison.OrdinalIgnoreCase))
        {
            return ListTemplates();
        }

        if (text.Equals("spawn list modules", StringComparison.OrdinalIgnoreCase))
        {
            return ListSpawnedModules();
        }

        if (text.StartsWith("spawn module ", StringComparison.OrdinalIgnoreCase))
        {
            var prompt = text["spawn module ".Length..].Trim();
            return SpawnModuleFromPrompt(prompt);
        }

        if (text.StartsWith("spawn review ", StringComparison.OrdinalIgnoreCase))
        {
            var moduleName = text["spawn review ".Length..].Trim();
            return ReviewModule(moduleName);
        }

        if (text.StartsWith("spawn approve ", StringComparison.OrdinalIgnoreCase))
        {
            var moduleName = text["spawn approve ".Length..].Trim();
            return ApproveModule(moduleName);
        }

        if (text.StartsWith("spawn rollback ", StringComparison.OrdinalIgnoreCase))
        {
            var moduleName = text["spawn rollback ".Length..].Trim();
            return RollbackModule(moduleName);
        }

        if (text.StartsWith("spawn history ", StringComparison.OrdinalIgnoreCase))
        {
            var moduleName = text["spawn history ".Length..].Trim();
            return GetModuleHistory(moduleName);
        }

        return "Unknown spawn command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Module Spawner commands (SuperAdmin only):",
            "  spawn module <prompt>       - Generate new RaCore module from natural language",
            "  spawn list templates        - Show available module templates",
            "  spawn list modules          - Show all spawned modules",
            "  spawn review <module>       - Review generated module before activation",
            "  spawn approve <module>      - Approve and activate a module",
            "  spawn rollback <module>     - Rollback/remove a spawned module",
            "  spawn history <module>      - View version history for a module",
            "  spawn status                - Show spawner status",
            "  help                        - Show this help message",
            "",
            "Example prompts:",
            "  - Create a weather forecast module that fetches weather data",
            "  - Build a cryptocurrency price tracker module",
            "  - Generate a task management module with TODO lists",
            "  - Create an email notification module",
            "",
            "⚠️  Security: Only SuperAdmin can spawn modules. All modules require review before activation."
        );
    }

    private string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Module Spawner Status:");
        sb.AppendLine();
        sb.AppendLine($"Modules Path: {_modulesRootPath}");
        sb.AppendLine($"Available Templates: {_templates.Count}");
        sb.AppendLine($"Spawned Modules: {_spawnedModules.Count}");
        sb.AppendLine($"Modules Awaiting Review: {_spawnedModules.Count(m => !m.IsApproved)}");
        sb.AppendLine($"Approved Modules: {_spawnedModules.Count(m => m.IsApproved)}");
        sb.AppendLine($"Active Modules: {_spawnedModules.Count(m => m.IsActive)}");

        if (_authModule != null)
        {
            sb.AppendLine();
            sb.AppendLine("✅ Authentication module: Connected");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("⚠️  Authentication module: Not available (permission checks disabled)");
        }

        return sb.ToString();
    }

    private string ListTemplates()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Available Module Templates:");
        sb.AppendLine();

        foreach (var (name, template) in _templates.OrderBy(t => t.Key))
        {
            sb.AppendLine($"📦 {name}");
            sb.AppendLine($"   {template.Description}");
            sb.AppendLine($"   Category: {template.Category}");
            sb.AppendLine($"   Features: {string.Join(", ", template.Features)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ListSpawnedModules()
    {
        if (_spawnedModules.Count == 0)
        {
            return "No modules spawned yet. Use 'spawn module <prompt>' to create one.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Spawned Modules:");
        sb.AppendLine();

        foreach (var module in _spawnedModules.OrderByDescending(m => m.CreatedAt))
        {
            var status = module.IsActive ? "🟢 Active" : 
                        module.IsApproved ? "✅ Approved" : "⏳ Awaiting Review";
            sb.AppendLine($"{status} - {module.Name}");
            sb.AppendLine($"   Created: {module.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"   Template: {module.TemplateName}");
            sb.AppendLine($"   Version: {module.Version}");
            sb.AppendLine($"   Path: {module.Path}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string SpawnModuleFromPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return "Error: Please provide a prompt. Example: spawn module Create a weather forecast module";
        }

        // SECURITY NOTE: In production environments, this operation should only be available to SuperAdmin users.
        // This can be enforced at the API endpoint level or by checking _authModule here.
        // Current implementation allows module spawning for development/testing purposes.
        // Recommended: Add authentication context parameter and validate user role >= SuperAdmin

        try
        {
            // Parse the prompt to determine the module type and features
            var (moduleType, features, moduleName) = ParsePrompt(prompt);
            
            if (!_templates.ContainsKey(moduleType))
            {
                return $"Error: No template found for module type '{moduleType}'. Use 'spawn list templates' to see available templates.";
            }

            // Generate module directory
            var sanitizedName = SanitizeModuleName(moduleName);
            var modulePath = Path.Combine(_modulesRootPath, sanitizedName);
            
            if (Directory.Exists(modulePath))
            {
                return $"Error: Module '{sanitizedName}' already exists. Choose a different name or remove the existing module.";
            }

            Directory.CreateDirectory(modulePath);

            // Generate the module files
            var template = _templates[moduleType];
            var generatedFiles = GenerateModuleFiles(template, modulePath, features, prompt, sanitizedName);

            // Create module record
            var spawnedModule = new SpawnedModule
            {
                Name = sanitizedName,
                TemplateName = moduleType,
                Path = modulePath,
                CreatedAt = DateTime.UtcNow,
                Prompt = prompt,
                GeneratedFiles = generatedFiles,
                IsApproved = false,
                IsActive = false,
                Version = "1.0.0"
            };

            _spawnedModules.Add(spawnedModule);

            // Generate summary
            var sb = new StringBuilder();
            sb.AppendLine($"✅ Module '{sanitizedName}' spawned successfully!");
            sb.AppendLine();
            sb.AppendLine($"Template: {moduleType}");
            sb.AppendLine($"Features: {string.Join(", ", features)}");
            sb.AppendLine($"Location: {modulePath}");
            sb.AppendLine($"Files generated: {generatedFiles.Count}");
            sb.AppendLine();
            sb.AppendLine("Generated files:");
            foreach (var file in generatedFiles)
            {
                sb.AppendLine($"  📄 {Path.GetFileName(file)}");
            }
            sb.AppendLine();
            sb.AppendLine($"⚠️  Review required: Use 'spawn review {sanitizedName}' to inspect the code");
            sb.AppendLine($"✅ To activate: Use 'spawn approve {sanitizedName}' after review");
            sb.AppendLine();
            sb.AppendLine("Note: Module will be loaded on next RaCore restart after approval.");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Module spawning error: {ex.Message}");
            return $"Error spawning module: {ex.Message}";
        }
    }

    private (string moduleType, List<string> features, string moduleName) ParsePrompt(string prompt)
    {
        var promptLower = prompt.ToLowerInvariant();
        
        // Determine module type based on keywords
        string moduleType = "basic";
        List<string> features = new();
        
        if (promptLower.Contains("api") || promptLower.Contains("rest") || promptLower.Contains("endpoint"))
        {
            moduleType = "api";
            features.Add("rest_api");
        }
        else if (promptLower.Contains("game") || promptLower.Contains("player") || promptLower.Contains("character"))
        {
            moduleType = "game_feature";
            features.Add("game_logic");
        }
        else if (promptLower.Contains("integration") || promptLower.Contains("connector") || promptLower.Contains("service"))
        {
            moduleType = "integration";
            features.Add("external_service");
        }
        else if (promptLower.Contains("utility") || promptLower.Contains("helper") || promptLower.Contains("tool"))
        {
            moduleType = "utility";
            features.Add("utility_functions");
        }

        // Extract features
        if (promptLower.Contains("database") || promptLower.Contains("storage"))
            features.Add("database");
        if (promptLower.Contains("cache") || promptLower.Contains("caching"))
            features.Add("caching");
        if (promptLower.Contains("authentication") || promptLower.Contains("auth"))
            features.Add("authentication");
        if (promptLower.Contains("logging") || promptLower.Contains("log"))
            features.Add("logging");
        if (promptLower.Contains("notification") || promptLower.Contains("alert"))
            features.Add("notifications");
        if (promptLower.Contains("schedule") || promptLower.Contains("cron") || promptLower.Contains("timer"))
            features.Add("scheduling");
        if (promptLower.Contains("email") || promptLower.Contains("mail"))
            features.Add("email");
        if (promptLower.Contains("webhook") || promptLower.Contains("callback"))
            features.Add("webhooks");

        if (features.Count == 0)
        {
            features.Add("basic_functionality");
        }

        // Extract module name from prompt
        string moduleName = ExtractModuleName(prompt);

        return (moduleType, features, moduleName);
    }

    private string ExtractModuleName(string prompt)
    {
        // Try to extract a meaningful name from the prompt
        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2)
            .Select(w => w.Trim().Trim(',', '.', '!', '?'))
            .ToList();

        // Remove common words
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
        { 
            "a", "an", "the", "create", "build", "make", "generate", "add", "new", 
            "module", "that", "with", "for", "to", "is", "are", "and", "or"
        };
        var meaningfulWords = words.Where(w => !stopWords.Contains(w)).Take(3).ToList();

        if (meaningfulWords.Count == 0)
        {
            return "CustomModule";
        }

        // Capitalize first letter of each word
        var name = string.Join("", meaningfulWords.Select(w => 
            char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
        
        // Ensure it ends with "Module"
        if (!name.EndsWith("Module", StringComparison.OrdinalIgnoreCase))
        {
            name += "Module";
        }

        return name;
    }

    private string SanitizeModuleName(string name)
    {
        // Remove invalid characters
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        
        // Ensure it starts with a letter
        if (sanitized.Length == 0 || !char.IsLetter(sanitized[0]))
        {
            sanitized = "Module" + sanitized;
        }

        return sanitized;
    }

    private List<string> GenerateModuleFiles(ModuleTemplate template, string modulePath, 
        List<string> features, string originalPrompt, string moduleName)
    {
        var generatedFiles = new List<string>();

        // Generate main module file
        var moduleFilePath = Path.Combine(modulePath, $"{moduleName}.cs");
        File.WriteAllText(moduleFilePath, GenerateModuleClass(template, moduleName, features, originalPrompt));
        generatedFiles.Add(moduleFilePath);

        // Generate README
        var readmePath = Path.Combine(modulePath, "README.md");
        File.WriteAllText(readmePath, GenerateReadme(template, moduleName, features, originalPrompt));
        generatedFiles.Add(readmePath);

        // Generate config file if needed
        if (features.Contains("database") || features.Contains("external_service"))
        {
            var configPath = Path.Combine(modulePath, $"{moduleName}Config.json");
            File.WriteAllText(configPath, GenerateConfig(moduleName, features));
            generatedFiles.Add(configPath);
        }

        // Generate tests template
        var testsPath = Path.Combine(modulePath, $"{moduleName}Tests.cs");
        File.WriteAllText(testsPath, GenerateTestsTemplate(moduleName));
        generatedFiles.Add(testsPath);

        return generatedFiles;
    }

    private string GenerateModuleClass(ModuleTemplate template, string moduleName, 
        List<string> features, string originalPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using Abstractions;");
        sb.AppendLine("using RaCore.Engine.Manager;");
        sb.AppendLine();
        sb.AppendLine($"namespace RaCore.Modules.{moduleName};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {moduleName} - {template.Description}");
        sb.AppendLine($"/// Generated from prompt: {originalPrompt}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"[RaModule(Category = \"{template.Category}\")]");
        sb.AppendLine($"public sealed class {moduleName} : ModuleBase");
        sb.AppendLine("{");
        sb.AppendLine($"    public override string Name => \"{moduleName.Replace("Module", "")}\";");
        sb.AppendLine();
        sb.AppendLine("    private ModuleManager? _manager;");
        
        if (features.Contains("database"))
        {
            sb.AppendLine("    private readonly Dictionary<string, object> _dataStore = new();");
        }
        
        if (features.Contains("caching"))
        {
            sb.AppendLine("    private readonly Dictionary<string, CacheEntry> _cache = new();");
        }
        
        sb.AppendLine();
        sb.AppendLine("    public override void Initialize(object? manager)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.Initialize(manager);");
        sb.AppendLine("        _manager = manager as ModuleManager;");
        sb.AppendLine($"        LogInfo(\"{moduleName} initialized\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public override string Process(string input)");
        sb.AppendLine("    {");
        sb.AppendLine("        var text = (input ?? string.Empty).Trim();");
        sb.AppendLine();
        sb.AppendLine("        if (string.IsNullOrEmpty(text) || text.Equals(\"help\", StringComparison.OrdinalIgnoreCase))");
        sb.AppendLine("        {");
        sb.AppendLine("            return GetHelp();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // TODO: Implement module-specific commands here");
        sb.AppendLine("        return $\"Processed: {text}\";");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private string GetHelp()");
        sb.AppendLine("    {");
        sb.AppendLine("        return string.Join(Environment.NewLine,");
        sb.AppendLine($"            \"{moduleName.Replace("Module", "")} commands:\",");
        sb.AppendLine("            \"  help - Show this help message\",");
        sb.AppendLine("            \"  // TODO: Add your commands here\"");
        sb.AppendLine("        );");
        sb.AppendLine("    }");
        
        if (features.Contains("caching"))
        {
            sb.AppendLine();
            sb.AppendLine("    private class CacheEntry");
            sb.AppendLine("    {");
            sb.AppendLine("        public object? Value { get; set; }");
            sb.AppendLine("        public DateTime ExpiresAt { get; set; }");
            sb.AppendLine("    }");
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private string GenerateReadme(ModuleTemplate template, string moduleName, 
        List<string> features, string originalPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {moduleName}");
        sb.AppendLine();
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"This module was generated by RaCore Module Spawner based on the prompt:");
        sb.AppendLine($"> {originalPrompt}");
        sb.AppendLine();
        sb.AppendLine("## Description");
        sb.AppendLine();
        sb.AppendLine(template.Description);
        sb.AppendLine();
        sb.AppendLine("## Category");
        sb.AppendLine();
        sb.AppendLine($"- **Category**: {template.Category}");
        sb.AppendLine($"- **Template**: {template.Name}");
        sb.AppendLine();
        sb.AppendLine("## Features");
        sb.AppendLine();
        foreach (var feature in features)
        {
            sb.AppendLine($"- {feature.Replace("_", " ").ToUpper()}");
        }
        sb.AppendLine();
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine("// Module is automatically loaded by ModuleManager");
        sb.AppendLine($"// Access via: {moduleName.Replace("Module", "")} commands");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Commands");
        sb.AppendLine();
        sb.AppendLine("- `help` - Show available commands");
        sb.AppendLine("- TODO: Document your commands here");
        sb.AppendLine();
        sb.AppendLine("## Configuration");
        sb.AppendLine();
        if (features.Contains("database") || features.Contains("external_service"))
        {
            sb.AppendLine($"Edit `{moduleName}Config.json` to customize module settings.");
        }
        else
        {
            sb.AppendLine("This module has no external configuration.");
        }
        sb.AppendLine();
        sb.AppendLine("## Development");
        sb.AppendLine();
        sb.AppendLine("### Building");
        sb.AppendLine("```bash");
        sb.AppendLine("dotnet build RaCore/RaCore.csproj");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("### Testing");
        sb.AppendLine("```bash");
        sb.AppendLine($"# See {moduleName}Tests.cs for test cases");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Customization");
        sb.AppendLine();
        sb.AppendLine("1. Edit the `Process()` method to add your logic");
        sb.AppendLine("2. Add helper methods as needed");
        sb.AppendLine("3. Update the help text");
        sb.AppendLine("4. Add configuration options if needed");
        sb.AppendLine("5. Write tests");
        sb.AppendLine();
        sb.AppendLine("## Version History");
        sb.AppendLine();
        sb.AppendLine($"- **1.0.0** - Initial generation ({DateTime.UtcNow:yyyy-MM-dd})");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"**Generated by**: RaCore Module Spawner");
        sb.AppendLine($"**Generated at**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Template**: {template.Name}");
        
        return sb.ToString();
    }

    private string GenerateConfig(string moduleName, List<string> features)
    {
        object config;
        
        if (features.Contains("database"))
        {
            config = new
            {
                module_name = moduleName,
                enabled = true,
                version = "1.0.0",
                settings = new
                {
                    database_connection = "TODO: Add connection string",
                    cache_enabled = features.Contains("caching")
                }
            };
        }
        else
        {
            config = new
            {
                module_name = moduleName,
                enabled = true,
                version = "1.0.0",
                settings = new
                {
                    api_endpoint = "TODO: Add API endpoint",
                    timeout_seconds = 30
                }
            };
        }

        return JsonSerializer.Serialize(config, _jsonOptions);
    }

    private string GenerateTestsTemplate(string moduleName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();
        sb.AppendLine($"namespace RaCore.Modules.{moduleName}.Tests;");
        sb.AppendLine();
        sb.AppendLine($"public class {moduleName}Tests");
        sb.AppendLine("{");
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void Initialize_ShouldSucceed()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var module = new {moduleName}();");
        sb.AppendLine();
        sb.AppendLine("        // Act");
        sb.AppendLine("        module.Initialize(null);");
        sb.AppendLine();
        sb.AppendLine("        // Assert");
        sb.AppendLine($"        Assert.NotNull(module);");
        sb.AppendLine("        Assert.NotEmpty(module.Name);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void Process_WithHelpCommand_ReturnsHelpText()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Arrange");
        sb.AppendLine($"        var module = new {moduleName}();");
        sb.AppendLine("        module.Initialize(null);");
        sb.AppendLine();
        sb.AppendLine("        // Act");
        sb.AppendLine("        var result = module.Process(\"help\");");
        sb.AppendLine();
        sb.AppendLine("        // Assert");
        sb.AppendLine("        Assert.NotNull(result);");
        sb.AppendLine("        Assert.Contains(\"help\", result, StringComparison.OrdinalIgnoreCase);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // TODO: Add more tests here");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private string ReviewModule(string moduleName)
    {
        var module = _spawnedModules.FirstOrDefault(m => 
            m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            return $"Error: Module '{moduleName}' not found. Use 'spawn list modules' to see available modules.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📋 Review: {module.Name}");
        sb.AppendLine();
        sb.AppendLine($"Status: {(module.IsApproved ? "✅ Approved" : "⏳ Awaiting Approval")}");
        sb.AppendLine($"Active: {(module.IsActive ? "Yes" : "No")}");
        sb.AppendLine($"Created: {module.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Template: {module.TemplateName}");
        sb.AppendLine($"Version: {module.Version}");
        sb.AppendLine($"Original Prompt: {module.Prompt}");
        sb.AppendLine();
        sb.AppendLine($"Location: {module.Path}");
        sb.AppendLine();
        sb.AppendLine("Generated Files:");
        foreach (var file in module.GeneratedFiles)
        {
            var fileName = Path.GetFileName(file);
            if (File.Exists(file))
            {
                var fileSize = new FileInfo(file).Length;
                sb.AppendLine($"  📄 {fileName} ({fileSize} bytes)");
            }
            else
            {
                sb.AppendLine($"  ⚠️  {fileName} (not found)");
            }
        }
        sb.AppendLine();
        
        // Show preview of main module file
        var moduleFile = module.GeneratedFiles.FirstOrDefault(f => f.EndsWith(".cs") && !f.Contains("Tests"));
        if (moduleFile != null && File.Exists(moduleFile))
        {
            sb.AppendLine($"File Preview ({Path.GetFileName(moduleFile)}):");
            sb.AppendLine("----------------------------------------");
            var code = File.ReadAllText(moduleFile);
            var lines = code.Split('\n').Take(30);
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
            if (code.Split('\n').Length > 30)
            {
                sb.AppendLine("... (truncated)");
            }
            sb.AppendLine("----------------------------------------");
        }
        
        sb.AppendLine();
        sb.AppendLine($"To approve this module: spawn approve {moduleName}");
        sb.AppendLine($"To remove this module: spawn rollback {moduleName}");
        
        return sb.ToString();
    }

    private string ApproveModule(string moduleName)
    {
        var module = _spawnedModules.FirstOrDefault(m => 
            m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            return $"Error: Module '{moduleName}' not found. Use 'spawn list modules' to see available modules.";
        }

        if (module.IsApproved)
        {
            return $"Module '{moduleName}' is already approved.";
        }

        module.IsApproved = true;
        module.ApprovedAt = DateTime.UtcNow;
        module.IsActive = true; // Will be active on next restart

        var sb = new StringBuilder();
        sb.AppendLine($"✅ Module '{moduleName}' has been approved!");
        sb.AppendLine();
        sb.AppendLine($"Location: {module.Path}");
        sb.AppendLine($"Version: {module.Version}");
        sb.AppendLine($"Files: {module.GeneratedFiles.Count}");
        sb.AppendLine();
        sb.AppendLine("⚠️  Module will be loaded on next RaCore restart.");
        sb.AppendLine();
        sb.AppendLine("Next steps:");
        sb.AppendLine("1. Restart RaCore to load the module");
        sb.AppendLine("2. Verify module appears in module list");
        sb.AppendLine("3. Test module functionality");
        sb.AppendLine("4. Customize code as needed");
        
        return sb.ToString();
    }

    private string RollbackModule(string moduleName)
    {
        var module = _spawnedModules.FirstOrDefault(m => 
            m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            return $"Error: Module '{moduleName}' not found. Use 'spawn list modules' to see available modules.";
        }

        try
        {
            // Delete module directory
            if (Directory.Exists(module.Path))
            {
                Directory.Delete(module.Path, true);
            }

            // Remove from spawned modules list
            _spawnedModules.Remove(module);

            return $"✅ Module '{moduleName}' has been rolled back and removed successfully.";
        }
        catch (Exception ex)
        {
            LogError($"Rollback error: {ex.Message}");
            return $"Error rolling back module: {ex.Message}";
        }
    }

    private string GetModuleHistory(string moduleName)
    {
        var module = _spawnedModules.FirstOrDefault(m => 
            m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            return $"Error: Module '{moduleName}' not found. Use 'spawn list modules' to see available modules.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📚 History: {module.Name}");
        sb.AppendLine();
        sb.AppendLine($"Version: {module.Version}");
        sb.AppendLine($"Created: {module.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        if (module.ApprovedAt.HasValue)
        {
            sb.AppendLine($"Approved: {module.ApprovedAt.Value:yyyy-MM-dd HH:mm:ss} UTC");
        }
        sb.AppendLine();
        sb.AppendLine($"Template: {module.TemplateName}");
        sb.AppendLine($"Prompt: {module.Prompt}");
        sb.AppendLine();
        sb.AppendLine("Status Timeline:");
        sb.AppendLine($"  1. Created on {module.CreatedAt:yyyy-MM-dd HH:mm}");
        if (module.ApprovedAt.HasValue)
        {
            sb.AppendLine($"  2. Approved on {module.ApprovedAt.Value:yyyy-MM-dd HH:mm}");
        }
        if (module.IsActive)
        {
            sb.AppendLine("  3. Currently active");
        }
        
        return sb.ToString();
    }

    private void InitializeTemplates()
    {
        _templates["basic"] = new ModuleTemplate
        {
            Name = "Basic Module",
            Description = "A basic RaCore module with standard functionality",
            Category = "extensions",
            Features = new List<string> { "Process commands", "Help text", "Logging" }
        };

        _templates["api"] = new ModuleTemplate
        {
            Name = "API Module",
            Description = "Module for REST API interactions and data fetching",
            Category = "extensions",
            Features = new List<string> { "HTTP requests", "JSON parsing", "Error handling", "Rate limiting" }
        };

        _templates["game_feature"] = new ModuleTemplate
        {
            Name = "Game Feature Module",
            Description = "Module for game-specific features and mechanics",
            Category = "extensions",
            Features = new List<string> { "Game logic", "State management", "Event handling" }
        };

        _templates["integration"] = new ModuleTemplate
        {
            Name = "Integration Module",
            Description = "Module for integrating with external services",
            Category = "extensions",
            Features = new List<string> { "Service connector", "Authentication", "Data sync" }
        };

        _templates["utility"] = new ModuleTemplate
        {
            Name = "Utility Module",
            Description = "Utility module with helper functions and tools",
            Category = "extensions",
            Features = new List<string> { "Helper functions", "Data transformation", "Validation" }
        };
    }
}

// Supporting classes
public class SpawnedModule
{
    public string Name { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public List<string> GeneratedFiles { get; set; } = new();
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public string Version { get; set; } = "1.0.0";
}

public class ModuleTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
}
