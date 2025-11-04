using System.Text;
using Abstractions;
using ASHATCore.Engine.Manager;
using LegendaryCMS.Core;

namespace ASHATCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Module - Unified website building system.
/// Integrates with LegendaryCMS for CMS functionality, generates static HTML sites.
/// </summary>
[RaModule(Category = "extensions")]
public class SiteBuilderModule : ModuleBase
{
    public override string Name => "SiteBuilder";

    private ModuleManager? _manager;
    private string? _wwwrootPath;
    private readonly object _lock = new();

    private WwwrootGenerator? _wwwrootGenerator;
    private ILegendaryCMSModule? _legendaryCMS;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;

        // Use GetCurrentDirectory() to get the ASHATCore.exe server root directory
        var serverRoot = Directory.GetCurrentDirectory();
        _wwwrootPath = Path.Combine(serverRoot, "wwwroot");

        // Initialize wwwroot Generator for static HTML
        _wwwrootGenerator = new WwwrootGenerator(this, _wwwrootPath);

        // Find LegendaryCMS module if loaded
        if (_manager != null)
        {
            var cmsModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<ILegendaryCMSModule>()
                .FirstOrDefault();

            if (cmsModule != null)
            {
                _legendaryCMS = cmsModule;
                LogInfo("SiteBuilder integrated with LegendaryCMS");
            }
            else
            {
                LogInfo("LegendaryCMS not found - CMS features will be unavailable");
            }
        }

        LogInfo("SiteBuilder module initialized (static HTML generation)");
    }

    public override string Process(string input)
    {
        return ProcessInternal(input);
    }

    public string ProcessInternal(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        var command = text.ToLowerInvariant();

        if (command == "site spawn" || command == "site spawn wwwroot" || command == "site spawn static")
        {
            return GenerateWwwroot();
        }

        if (command == "site spawn cms" || command == "site spawn integrated")
        {
            return InitializeCMS();
        }

        if (command == "site status")
        {
            return GetSiteStatus();
        }

        return $"Unknown command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "SiteBuilder commands:",
            "  site spawn           - Generate static HTML site (wwwroot)",
            "  site spawn wwwroot   - Same as 'site spawn'",
            "  site spawn static    - Same as 'site spawn'",
            "  site spawn cms       - Initialize LegendaryCMS module (if available)",
            "  site spawn integrated - Generate static site + initialize CMS",
            "  site status          - Show site deployment status",
            "  help                 - Show this help message",
            "",
            "Note: CMS functionality is provided by LegendaryCMS module (Beta v1.2.0)",
            "      Pure .NET architecture - uses Razor Pages/Blazor components",
            "      No PHP required - all content served dynamically via Kestrel"
        );
    }

    private string InitializeCMS()
    {
        lock (_lock)
        {
            if (_legendaryCMS == null)
            {
                return "❌ LegendaryCMS module not loaded. CMS features unavailable.\n" +
                       "   LegendaryCMS runs as a C# module within ASHATOS using Razor/Blazor.";
            }

            var status = _legendaryCMS.GetStatus();
            if (status.IsInitialized && status.IsRunning)
            {
                return $"✅ LegendaryCMS is already running (v{status.Version})\n" +
                       $"   Started: {status.StartTime:yyyy-MM-dd HH:mm:ss UTC}\n" +
                       "   API endpoints available at /api/*\n" +
                       "   Razor Pages available at /cms/*\n" +
                       "   Use 'cms status' for detailed information";
            }

            return "✅ LegendaryCMS module is loaded and ready\n" +
                   "   Use 'cms' commands to interact with CMS features\n" +
                   "   API endpoints: /api/forums, /api/blogs, /api/chat, etc.\n" +
                   "   Razor Pages: /cms/forums, /cms/blogs, /cms/profiles";
        }
    }

    /// <summary>
    /// Generates wwwroot directory with control panel files
    /// </summary>
    public string GenerateWwwroot()
    {
        lock (_lock)
        {
            if (_wwwrootGenerator == null)
            {
                return "Error: Wwwroot Generator not initialized";
            }

            return _wwwrootGenerator.GenerateWwwroot();
        }
    }

    private string GetSiteStatus()
    {
        if (_wwwrootPath == null)
        {
            return "Wwwroot path not configured";
        }

        var sb = new StringBuilder();
        sb.AppendLine("SiteBuilder Status:");
        sb.AppendLine();

        // Check wwwroot (static HTML)
        sb.AppendLine($"Static Site (wwwroot): {_wwwrootPath}");
        sb.AppendLine($"  Exists: {Directory.Exists(_wwwrootPath)}");
        if (Directory.Exists(_wwwrootPath))
        {
            var htmlFiles = Directory.GetFiles(_wwwrootPath, "*.html");
            sb.AppendLine($"  HTML Files: {htmlFiles.Length}");
        }

        sb.AppendLine();

        // Check LegendaryCMS status
        if (_legendaryCMS != null)
        {
            var cmsStatus = _legendaryCMS.GetStatus();
            sb.AppendLine("LegendaryCMS Module:");
            sb.AppendLine($"  Version: {cmsStatus.Version}");
            sb.AppendLine($"  Initialized: {cmsStatus.IsInitialized}");
            sb.AppendLine($"  Running: {cmsStatus.IsRunning}");
            if (cmsStatus.IsRunning)
            {
                sb.AppendLine($"  Started: {cmsStatus.StartTime:yyyy-MM-dd HH:mm:ss UTC}");
                sb.AppendLine("  API Endpoints: /api/forums, /api/blogs, /api/chat, etc.");
            }
        }
        else
        {
            sb.AppendLine("LegendaryCMS Module: Not loaded");
            sb.AppendLine("  CMS functionality unavailable");
        }

        return sb.ToString();
    }

    // Public methods for components to use
    public void Log(string message, string level = "INFO")
    {
        if (level == "ERROR")
            LogError(message);
        else
            LogInfo(message);
    }
}