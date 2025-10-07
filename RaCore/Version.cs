namespace RaCore;

/// <summary>
/// Centralized version management for RaOS.
/// Update this file to change the version across all modules and documentation.
/// </summary>
public static class RaVersion
{
    /// <summary>
    /// Current version of RaOS
    /// </summary>
    public const string Current = "9.3.9";
    
    /// <summary>
    /// Full version string with phase prefix
    /// </summary>
    public const string FullVersion = "Phase 9.3.9";
    
    /// <summary>
    /// Major version number
    /// </summary>
    public const int Major = 9;
    
    /// <summary>
    /// Minor version number
    /// </summary>
    public const int Minor = 3;
    
    /// <summary>
    /// Patch version number
    /// </summary>
    public const int Patch = 9;
    
    /// <summary>
    /// Version status/label
    /// </summary>
    public const string Label = "Production Ready";
    
    /// <summary>
    /// Last update date
    /// </summary>
    public const string LastUpdated = "October 2025";
    
    /// <summary>
    /// Gets version as a comparable tuple
    /// </summary>
    public static (int major, int minor, int patch) GetVersionTuple() => (Major, Minor, Patch);
    
    /// <summary>
    /// Compares if a given version string is newer than current
    /// </summary>
    public static bool IsNewerThan(string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 3) return false;
        
        if (!int.TryParse(parts[0], out var major)) return false;
        if (!int.TryParse(parts[1], out var minor)) return false;
        if (!int.TryParse(parts[2], out var patch)) return false;
        
        if (major > Major) return true;
        if (major < Major) return false;
        
        if (minor > Minor) return true;
        if (minor < Minor) return false;
        
        return patch > Patch;
    }
}
