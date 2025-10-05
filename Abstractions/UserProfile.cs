namespace Abstractions;

/// <summary>
/// User profile for personalization
/// </summary>
public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public Dictionary<string, string> Preferences { get; set; } = new();
    public List<string> AllowedModules { get; set; } = new();
    public string? Role { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// User preference keys
/// </summary>
public static class UserPreferenceKeys
{
    public const string Language = "language";
    public const string ResponseStyle = "response_style";
    public const string VerbosityLevel = "verbosity";
    public const string Theme = "theme";
    public const string AutoSave = "auto_save";
}
