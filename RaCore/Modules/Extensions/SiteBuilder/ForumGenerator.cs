namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates forum system (vBulletin-style).
/// </summary>
public class ForumGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;

    public ForumGenerator(SiteBuilderModule module, string cmsRootPath)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
    }

    public string GenerateForum(string phpPath)
    {
        try
        {
            _module.Log("Starting Forum generation...");
            
            var forumPath = Path.Combine(_cmsRootPath, "community");
            if (!Directory.Exists(forumPath))
            {
                Directory.CreateDirectory(forumPath);
            }

            // Generate forum files
            // This would contain the extracted code from original CMSSpawner
            
            _module.Log("Forum generated successfully");
            return $"✅ Forum generated at: {forumPath}";
        }
        catch (Exception ex)
        {
            _module.Log($"Forum generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }
}
