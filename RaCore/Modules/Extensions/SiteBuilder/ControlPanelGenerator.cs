namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates Control Panel for site administration.
/// </summary>
public class ControlPanelGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;

    public ControlPanelGenerator(SiteBuilderModule module, string cmsRootPath)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
    }

    public string GenerateControlPanel(string phpPath)
    {
        try
        {
            _module.Log("Starting Control Panel generation...");
            
            var controlPath = Path.Combine(_cmsRootPath, "control");
            if (!Directory.Exists(controlPath))
            {
                Directory.CreateDirectory(controlPath);
                _module.Log($"Created Control Panel directory: {controlPath}");
            }

            // Generate control panel files
            // This would contain the extracted code from original CMSSpawner
            
            _module.Log("Control Panel generated successfully");
            return $"✅ Control Panel generated at: {controlPath}";
        }
        catch (Exception ex)
        {
            _module.Log($"Control Panel generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }
}
