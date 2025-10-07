using Abstractions;
using LegendaryClientBuilder.Core;

namespace LegendaryClientBuilder.Templates;

/// <summary>
/// Manages client templates for different platforms and use cases.
/// </summary>
public class TemplateManager
{
    private readonly Dictionary<string, ClientTemplate> _templates = new();
    private readonly object _lock = new();

    public TemplateManager()
    {
        RegisterBuiltInTemplates();
    }

    private void RegisterBuiltInTemplates()
    {
        // WebGL Templates
        RegisterTemplate(new ClientTemplate
        {
            Name = "WebGL-Basic",
            Description = "Basic HTML5/WebGL client with minimal styling",
            Platform = ClientPlatform.WebGL,
            Category = "Basic",
            IsBuiltIn = true
        });

        RegisterTemplate(new ClientTemplate
        {
            Name = "WebGL-Professional",
            Description = "Professional-grade WebGL client with gradient UI and advanced features",
            Platform = ClientPlatform.WebGL,
            Category = "Professional",
            IsBuiltIn = true
        });

        RegisterTemplate(new ClientTemplate
        {
            Name = "WebGL-Gaming",
            Description = "Gaming-focused client with enhanced controls and performance",
            Platform = ClientPlatform.WebGL,
            Category = "Gaming",
            IsBuiltIn = true
        });

        RegisterTemplate(new ClientTemplate
        {
            Name = "WebGL-Mobile",
            Description = "Mobile-optimized responsive WebGL client",
            Platform = ClientPlatform.WebGL,
            Category = "Mobile",
            IsBuiltIn = true
        });

        // Desktop Templates
        RegisterTemplate(new ClientTemplate
        {
            Name = "Desktop-Standard",
            Description = "Standard desktop client with launcher script",
            Platform = ClientPlatform.Windows,
            Category = "Standard",
            IsBuiltIn = true
        });

        RegisterTemplate(new ClientTemplate
        {
            Name = "Desktop-Advanced",
            Description = "Advanced desktop client with native features",
            Platform = ClientPlatform.Windows,
            Category = "Advanced",
            IsBuiltIn = true
        });
    }

    public void RegisterTemplate(ClientTemplate template)
    {
        lock (_lock)
        {
            var key = $"{template.Platform}-{template.Name}";
            _templates[key] = template;
        }
    }

    public IEnumerable<ClientTemplate> GetAvailableTemplates(ClientPlatform? platform = null)
    {
        lock (_lock)
        {
            return platform.HasValue
                ? _templates.Values.Where(t => t.Platform == platform.Value).ToList()
                : _templates.Values.ToList();
        }
    }

    public ClientTemplate? GetTemplate(ClientPlatform platform, string templateName)
    {
        lock (_lock)
        {
            var key = $"{platform}-{templateName}";
            return _templates.TryGetValue(key, out var template) ? template : null;
        }
    }

    public bool RemoveTemplate(ClientPlatform platform, string templateName)
    {
        lock (_lock)
        {
            var key = $"{platform}-{templateName}";
            if (_templates.TryGetValue(key, out var template) && !template.IsBuiltIn)
            {
                _templates.Remove(key);
                return true;
            }
            return false;
        }
    }
}
