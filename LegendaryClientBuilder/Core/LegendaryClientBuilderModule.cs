using System.Text.Json;
using Abstractions;
using LegendaryClientBuilder.Builders;
using LegendaryClientBuilder.Configuration;
using LegendaryClientBuilder.Templates;

namespace LegendaryClientBuilder.Core;

/// <summary>
/// Legendary Client Builder Suite - Advanced multi-platform game client Generation
/// Phase 9.1 implementation with professional templates and modular architecture
/// </summary>
[RaModule(Category = "clientbuilder")]
public sealed class LegendaryClientBuilderModule : ModuleBase, ILegendaryClientBuilderModule
{
    public override string Name => "LegendaryClientBuilder";
    public string Version => "9.3.9"; // Managed by unified version system

    private readonly Dictionary<Guid, GameClientPackage> _clients = new();
    private readonly Dictionary<Guid, List<Guid>> _userClients = new();
    private readonly object _lock = new();
    private readonly string _clientsPath;

    private ILicenseModule? _licenseModule;
    private IClientBuilderConfiguration? _Configuration;
    private TemplateManager? _templateManager;
    private DateTime _startTime;
    private bool _isInitialized;

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public LegendaryClientBuilderModule()
    {
        _clientsPath = Path.Combine(Directory.GetCurrentDirectory(), "GameClients");
        Directory.CreateDirectory(_clientsPath);
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);

        try
        {
            LogInfo("Initializing Legendary Client Builder Suite v9.3.9...");

            // Setup Configuration
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "clientbuilder-config.json");
            _Configuration = new ClientBuilderConfiguration(configPath);

            // Initialize template manager
            _templateManager = new TemplateManager();
            LogInfo("Template system initialized with built-in templates");

            // Get reference to license module using reflection to avoid direct ASHATCore dependency
            try
            {
                var managerType = manager?.GetType();
                var getModuleMethod = managerType?.GetMethod("GetModuleByName");
                if (getModuleMethod != null && manager != null)
                {
                    _licenseModule = getModuleMethod.Invoke(manager, new object[] { "License" }) as ILicenseModule;
                }
            }
            catch
            {
                // License module is optional
            }

            _startTime = DateTime.UtcNow;
            _isInitialized = true;

            LogInfo("✅ Legendary Client Builder Suite initialized successfully");
            LogInfo($"   Version: {Version}");
            LogInfo($"   Output Path: {_clientsPath}");
            LogInfo($"   Templates: {_templateManager?.GetAvailableTemplates()?.Count() ?? 0} available");
            LogInfo($"   Configuration: {_Configuration?.Environment}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to initialize Legendary Client Builder: {ex.Message}");
            throw;
        }
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("clientbuilder stats", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("cb stats", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(GetStats(), _jsonOptions);
        }

        if (text.StartsWith("clientbuilder list", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("cb list", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: clientbuilder list <user-id> or cb list <user-id>";
            }
            if (Guid.TryParse(parts[2], out var userId))
            {
                return JsonSerializer.Serialize(GetUserClientPackages(userId), _jsonOptions);
            }
            return "Invalid user ID format";
        }

        if (text.StartsWith("clientbuilder templates", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("cb templates", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            ClientPlatform? platform = null;

            if (parts.Length > 2 && Enum.TryParse<ClientPlatform>(parts[2], true, out var p))
            {
                platform = p;
            }

            return JsonSerializer.Serialize(GetAvailableTemplates(platform), _jsonOptions);
        }

        if (text.Equals("clientbuilder status", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("cb status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatusInfo();
        }

        return "Unknown clientbuilder command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "Legendary Client Builder Suite v9.3.9 - Commands:",
            "",
            "  clientbuilder stats (or cb stats)              - Show client Generation statistics",
            "  clientbuilder list <user-id> (or cb list)      - List clients for a user",
            "  clientbuilder templates [platform] (or cb t)   - List available templates",
            "  clientbuilder status (or cb status)            - Show module status",
            "",
            "API Endpoints:",
            "  POST /api/clientbuilder/Generate               - Generate game client",
            "  POST /api/clientbuilder/Generate/template      - Generate with template",
            "  GET  /api/clientbuilder/list                   - List user's clients",
            "  GET  /api/clientbuilder/templates              - Get available templates",
            "  DELETE /api/clientbuilder/delete/{id}          - Delete a client",
            "",
            "The Legendary Client Builder Generates professional multi-platform game clients",
            "with advanced templates and customization options."
        );
    }

    private string GetStatusInfo()
    {
        var uptime = DateTime.UtcNow - _startTime;
        var stats = GetStats();

        return JsonSerializer.Serialize(new
        {
            Module = Name,
            Version,
            Status = _isInitialized ? "Active" : "Inactive",
            Uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m",
            Configuration = _Configuration?.Environment ?? "Unknown",
            OutputPath = _clientsPath,
            Statistics = stats,
            Templates = _templateManager?.GetAvailableTemplates()?.Count() ?? 0
        }, _jsonOptions);
    }

    public ClientBuilderStats GetStats()
    {
        lock (_lock)
        {
            var stats = new ClientBuilderStats
            {
                TotalClients = _clients.Count,
                TotalUsers = _userClients.Count,
                TotalSizeBytes = _clients.Values.Sum(c => c.SizeBytes),
                ModuleStartTime = _startTime
            };

            // Group by platform
            foreach (var client in _clients.Values)
            {
                if (stats.ClientsByPlatform.ContainsKey(client.Platform))
                {
                    stats.ClientsByPlatform[client.Platform]++;
                }
                else
                {
                    stats.ClientsByPlatform[client.Platform] = 1;
                }

                // TASHATck templates (stored in custom settings if available)
                if (client.Configuration.CustomSettings.TryGetValue("template", out var template))
                {
                    if (stats.ClientsByTemplate.ContainsKey(template))
                    {
                        stats.ClientsByTemplate[template]++;
                    }
                    else
                    {
                        stats.ClientsByTemplate[template] = 1;
                    }
                }
            }

            return stats;
        }
    }

    public async Task<GameClientPackage> GenerateClientAsync(
        Guid userId,
        string licenseKey,
        ClientPlatform platform,
        ClientConfiguration config)
    {
        return await GenerateClientFromTemplateAsync(userId, licenseKey, platform, "", config);
    }

    public async Task<GameClientPackage> GenerateClientFromTemplateAsync(
        Guid userId,
        string licenseKey,
        ClientPlatform platform,
        string templateName,
        ClientConfiguration config)
    {
        // Verify license is valid
        if (_licenseModule != null)
        {
            var license = _licenseModule.GetAllLicenses()
                .FirstOrDefault(l => l.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase));

            if (license == null || license.Status != LicenseStatus.Active)
            {
                throw new InvalidOperationException("Invalid or inactive license - client Generation requires active license");
            }
        }

        // Check user client limit
        lock (_lock)
        {
            if (_userClients.TryGetValue(userId, out var userClientIds))
            {
                var maxClients = _Configuration?.MaxClientsPerUser ?? 10;
                if (userClientIds.Count >= maxClients)
                {
                    throw new InvalidOperationException($"User has reached maximum client limit ({maxClients})");
                }
            }
        }

        var package = new GameClientPackage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LicenseKey = licenseKey,
            Platform = platform,
            Configuration = config,
            CreatedAt = DateTime.UtcNow
        };

        // Store template name in custom settings
        if (!string.IsNullOrEmpty(templateName))
        {
            package.Configuration.CustomSettings["template"] = templateName;
        }

        // Get template
        ClientTemplate? template = null;
        if (!string.IsNullOrEmpty(templateName) && _templateManager != null)
        {
            template = _templateManager.GetTemplate(platform, templateName);
        }

        // Generate client based on platform
        var clientPath = await GenerateClientFilesAsync(package, template);

        package.PackagePath = clientPath;
        package.ClientUrl = $"/clients/{package.Id}/index.html";
        package.SizeBytes = GetDirectorySize(clientPath);

        lock (_lock)
        {
            _clients[package.Id] = package;

            if (!_userClients.ContainsKey(userId))
            {
                _userClients[userId] = new List<Guid>();
            }
            _userClients[userId].Add(package.Id);
        }

        LogInfo($"Generated {platform} client for user {userId}, license {licenseKey}, template: {templateName ?? "default"}");
        return package;
    }

    private async Task<string> GenerateClientFilesAsync(GameClientPackage package, ClientTemplate? template)
    {
        ClientBuilderBase builder = package.Platform switch
        {
            ClientPlatform.WebGL => new WebGLClientBuilder(_clientsPath),
            ClientPlatform.Windows => new DesktopClientBuilder(_clientsPath),
            ClientPlatform.Linux => new DesktopClientBuilder(_clientsPath),
            ClientPlatform.MacOS => new DesktopClientBuilder(_clientsPath),
            _ => new WebGLClientBuilder(_clientsPath)
        };

        return await builder.GenerateAsync(package, template);
    }

    public GameClientPackage? GetClientPackage(Guid packageId)
    {
        lock (_lock)
        {
            return _clients.TryGetValue(packageId, out var package) ? package : null;
        }
    }

    public IEnumerable<GameClientPackage> GetUserClientPackages(Guid userId)
    {
        lock (_lock)
        {
            if (_userClients.TryGetValue(userId, out var clientIds))
            {
                return clientIds
                    .Select(id => _clients.TryGetValue(id, out var package) ? package : null)
                    .Where(p => p != null)
                    .Cast<GameClientPackage>()
                    .ToList();
            }
        }

        return Enumerable.Empty<GameClientPackage>();
    }

    public async Task<bool> UpdateClientConfigAsync(Guid packageId, ClientConfiguration config)
    {
        await Task.CompletedTask;
        lock (_lock)
        {
            if (_clients.TryGetValue(packageId, out var package))
            {
                package.Configuration = config;
                LogInfo($"Updated Configuration for client {packageId}");
                return true;
            }
        }

        return false;
    }

    public IEnumerable<ClientTemplate> GetAvailableTemplates(ClientPlatform? platform = null)
    {
        if (_templateManager == null)
        {
            return Enumerable.Empty<ClientTemplate>();
        }

        return platform.HasValue
            ? _templateManager.GetAvailableTemplates(platform.Value)
            : _templateManager.GetAvailableTemplates();
    }

    public void RegisterTemplate(ClientTemplate template)
    {
        _templateManager?.RegisterTemplate(template);
        LogInfo($"Registered custom template: {template.Name} for {template.Platform}");
    }

    public async Task<bool> DeleteClientAsync(Guid packageId)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            if (!_clients.TryGetValue(packageId, out var package))
            {
                return false;
            }

            // Remove from user's client list
            if (_userClients.TryGetValue(package.UserId, out var userClientIds))
            {
                userClientIds.Remove(packageId);
                if (userClientIds.Count == 0)
                {
                    _userClients.Remove(package.UserId);
                }
            }

            // Remove package data
            _clients.Remove(packageId);

            // Delete files
            try
            {
                if (Directory.Exists(package.PackagePath))
                {
                    Directory.Delete(package.PackagePath, true);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete client files for {packageId}: {ex.Message}");
            }

            LogInfo($"Deleted client {packageId}");
            return true;
        }
    }

    public async Task<GameClientPackage> ReGenerateClientAsync(Guid packageId, ClientConfiguration newConfig)
    {
        await Task.CompletedTask; // Suppress CS1998 warning - method reserved for future async Operations
        
        lock (_lock)
        {
            if (!_clients.TryGetValue(packageId, out var oldPackage))
            {
                throw new InvalidOperationException($"Client {packageId} not found");
            }

            // Create new package with same credentials but new config
            var newPackage = new GameClientPackage
            {
                Id = Guid.NewGuid(),
                UserId = oldPackage.UserId,
                LicenseKey = oldPackage.LicenseKey,
                Platform = oldPackage.Platform,
                Configuration = newConfig,
                CreatedAt = DateTime.UtcNow
            };

            // Copy template setting if it exists
            if (oldPackage.Configuration.CustomSettings.TryGetValue("template", out var template))
            {
                newPackage.Configuration.CustomSettings["template"] = template;
            }

            // Delete old client
            DeleteClientAsync(packageId).Wait();

            // Generate new client
            return GenerateClientAsync(
                newPackage.UserId,
                newPackage.LicenseKey,
                newPackage.Platform,
                newPackage.Configuration).Result;
        }
    }

    private long GetDirectorySize(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        catch
        {
            return 0;
        }
    }

    public override void Dispose()
    {
        _isInitialized = false;
        base.Dispose();
    }
}
