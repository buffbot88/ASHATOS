namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates integrated site with CMS, Control Panel, Forum, and Profiles.
/// </summary>
public class IntegratedSiteGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;
    private readonly CmsGenerator _cmsGenerator;
    private readonly ControlPanelGenerator _controlPanelGenerator;
    private readonly ForumGenerator _forumGenerator;
    private readonly ProfileGenerator _profileGenerator;

    public IntegratedSiteGenerator(
        SiteBuilderModule module,
        string cmsRootPath,
        CmsGenerator cmsGenerator,
        ControlPanelGenerator controlPanelGenerator,
        ForumGenerator forumGenerator,
        ProfileGenerator profileGenerator)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
        _cmsGenerator = cmsGenerator;
        _controlPanelGenerator = controlPanelGenerator;
        _forumGenerator = forumGenerator;
        _profileGenerator = profileGenerator;
    }

    public string GenerateIntegratedSite(string phpPath)
    {
        try
        {
            _module.Log("Starting Integrated CMS + Control Panel generation...");
            
            // Create base CMS
            var cmsResult = _cmsGenerator.GenerateHomepage(phpPath);
            
            // Add Control Panel
            var controlResult = _controlPanelGenerator.GenerateControlPanel(phpPath);
            
            // Add Forum
            var forumResult = _forumGenerator.GenerateForum(phpPath);
            
            // Add Profiles
            var profileResult = _profileGenerator.GenerateProfiles(phpPath);
            
            _module.Log("✅ Integrated CMS + Control Panel + Community generated successfully!");
            
            return $@"✅ Integrated site generated successfully!

📁 CMS Location: {_cmsRootPath}
📁 Server Root: {Directory.GetCurrentDirectory()}

Generated structure:
  {_cmsRootPath}/
    /               - CMS Homepage
    /control/       - Control Panel
    /community/     - Forums
    /profile.php    - User Profiles";
        }
        catch (Exception ex)
        {
            _module.Log($"Integrated site generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }
}
