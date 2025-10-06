using System.Text;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// SiteBuilder Module - Unified website building system.
/// Generates and manages CMS, Control Panels, Forums, and Profile systems.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class SiteBuilderModule : ModuleBase
{
    public override string Name => "SiteBuilder";

    private ModuleManager? _manager;
    private string? _cmsRootPath;
    private readonly object _lock = new();
    
    private PhpDetector? _phpDetector;
    private CmsGenerator? _cmsGenerator;
    private ControlPanelGenerator? _controlPanelGenerator;
    private ForumGenerator? _forumGenerator;
    private ProfileGenerator? _profileGenerator;
    private IntegratedSiteGenerator? _integratedSiteGenerator;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _cmsRootPath = Path.Combine(AppContext.BaseDirectory, "racore_cms");
        
        // Initialize components
        _phpDetector = new PhpDetector(this);
        _cmsGenerator = new CmsGenerator(this, _cmsRootPath);
        _controlPanelGenerator = new ControlPanelGenerator(this, _cmsRootPath);
        _forumGenerator = new ForumGenerator(this, _cmsRootPath);
        _profileGenerator = new ProfileGenerator(this, _cmsRootPath);
        _integratedSiteGenerator = new IntegratedSiteGenerator(
            this, _cmsRootPath, _cmsGenerator, _controlPanelGenerator, _forumGenerator, _profileGenerator);
        
        LogInfo("SiteBuilder module initialized");
    }

    public override string Process(string input)
    {
        return ProcessInternal(input);
    }

    private string ProcessInternal(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        var command = text.ToLowerInvariant();
        
        if (command == "site spawn" || command == "site spawn home")
        {
            return SpawnHomepage();
        }

        if (command == "site spawn control")
        {
            return SpawnControlPanel();
        }

        if (command == "site spawn integrated")
        {
            return SpawnIntegratedSite();
        }

        if (command == "site status")
        {
            return GetSiteStatus();
        }

        if (command == "site detect php")
        {
            return _phpDetector?.DetectPHP() ?? "PHP detector not initialized";
        }

        return $"Unknown SiteBuilder command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "SiteBuilder commands:",
            "  site spawn           - Create PHP CMS homepage with SQLite database",
            "  site spawn home      - Same as 'site spawn'",
            "  site spawn control   - Create standalone Control Panel",
            "  site spawn integrated - Create CMS with integrated Control Panel (first-run)",
            "  site status          - Show site deployment status",
            "  site detect php      - Detect PHP runtime version",
            "  help                 - Show this help message"
        );
    }

    private string SpawnHomepage()
    {
        lock (_lock)
        {
            if (_cmsGenerator == null || _phpDetector == null)
            {
                return "Error: Components not initialized";
            }

            var phpPath = _phpDetector.FindPhpExecutable();
            if (phpPath == null)
            {
                return _phpDetector.GetPhpNotFoundMessage();
            }

            return _cmsGenerator.GenerateHomepage(phpPath);
        }
    }

    private string SpawnControlPanel()
    {
        lock (_lock)
        {
            if (_controlPanelGenerator == null || _phpDetector == null)
            {
                return "Error: Components not initialized";
            }

            var phpPath = _phpDetector.FindPhpExecutable();
            if (phpPath == null)
            {
                return _phpDetector.GetPhpNotFoundMessage();
            }

            return _controlPanelGenerator.GenerateControlPanel(phpPath);
        }
    }

    private string SpawnIntegratedSite()
    {
        lock (_lock)
        {
            if (_integratedSiteGenerator == null || _phpDetector == null)
            {
                return "Error: Components not initialized";
            }

            var phpPath = _phpDetector.FindPhpExecutable();
            if (phpPath == null)
            {
                return _phpDetector.GetPhpNotFoundMessage();
            }

            return _integratedSiteGenerator.GenerateIntegratedSite(phpPath);
        }
    }

    private string GetSiteStatus()
    {
        if (_cmsRootPath == null)
        {
            return "CMS root path not configured";
        }

        var sb = new StringBuilder();
        sb.AppendLine("SiteBuilder Status:");
        sb.AppendLine();
        sb.AppendLine($"CMS Root: {_cmsRootPath}");
        sb.AppendLine($"Exists: {Directory.Exists(_cmsRootPath)}");
        
        if (Directory.Exists(_cmsRootPath))
        {
            var files = Directory.GetFiles(_cmsRootPath, "*.php");
            sb.AppendLine($"PHP Files: {files.Length}");
            
            var controlPath = Path.Combine(_cmsRootPath, "control");
            sb.AppendLine($"Control Panel: {(Directory.Exists(controlPath) ? "Installed" : "Not installed")}");
            
            var forumPath = Path.Combine(_cmsRootPath, "community");
            sb.AppendLine($"Forum: {(Directory.Exists(forumPath) ? "Installed" : "Not installed")}");
        }

        if (_phpDetector != null)
        {
            var phpPath = _phpDetector.FindPhpExecutable();
            sb.AppendLine();
            sb.AppendLine($"PHP Status: {(phpPath != null ? "Found" : "Not found")}");
            if (phpPath != null)
            {
                sb.AppendLine($"PHP Path: {phpPath}");
            }
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
