namespace ASHATCore;

/// <summary>
/// Centralized version management for ASHATOS.
/// Update this file to change the version across all modules and documentation.
/// </summary>
public static class ASHATVersion
{
    /// <summary>
    /// Current version of ASHATOS
    /// </summary>
    public const string Current = "1.1.0";
    
    /// <summary>
    /// Full version string with phase prefix
    /// </summary>
    public const string FullVersion = "Beta v1.1.0";
    
    /// <summary>
    /// Major version number
    /// </summary>
    public const int Major = 1;
    
    /// <summary>
    /// Minor version number
    /// </summary>
    public const int Minor = 1;
    
    /// <summary>
    /// Patch version number
    /// </summary>
    public const int Patch = 0;
    
    /// <summary>
    /// Version status/label
    /// </summary>
    public const string Label = "Beta";
    
    /// <summary>
    /// Last update date
    /// </summary>
    public const string LastUpdated = "November 4 2025";
    
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
