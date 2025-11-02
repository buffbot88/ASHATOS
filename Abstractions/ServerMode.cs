namespace Abstractions;

/// <summary>
/// Defines the Operating mode for a ASHATOS server instance
/// </summary>
public enum ServerMode
{
    /// <summary>
    /// Dev mode - Development mode with testing features enabled, bypasses external validations.
    /// Note: This mode may be renamed or removed in future versions.
    /// </summary>
    Dev,
    
    /// <summary>
    /// Alpha mode - Early development and testing with full logging
    /// </summary>
    Alpha,
    
    /// <summary>
    /// Beta mode - Pre-release testing with selected users
    /// </summary>
    Beta,
    
    /// <summary>
    /// Omega mode - Main server Configuration (US-Omega)
    /// </summary>
    Omega,
    
    /// <summary>
    /// Demo mode - Demonstration instance with limited features
    /// </summary>
    Demo,
    
    /// <summary>
    /// Production mode - Full production deployment
    /// </summary>
    Production
}

/// <summary>
/// Configuration for ASHATOS server initialization and Operation
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Current server mode
    /// </summary>
    public ServerMode Mode { get; set; } = ServerMode.Production;
    
    /// <summary>
    /// Indicates if this is the first run of the server
    /// </summary>
    public bool IsFirstRun { get; set; } = true;
    
    /// <summary>
    /// Indicates if initialization has been completed
    /// </summary>
    public bool InitializationCompleted { get; set; } = false;
    
    /// <summary>
    /// Timestamp when the server was initialized
    /// </summary>
    public DateTime? InitializedAt { get; set; }
    
    /// <summary>
    /// Server version
    /// </summary>
    public string Version { get; set; } = "1.0";
    
    /// <summary>
    /// License key assigned to this server
    /// </summary>
    public string? LicenseKey { get; set; }
    
    /// <summary>
    /// License type (Forum, CMS, GameServer, etc.)
    /// </summary>
    public string? LicenseType { get; set; }
    
    /// <summary>
    /// Indicates if the admin password has been changed from default
    /// </summary>
    public bool AdminPasswordChanged { get; set; } = false;
    
    /// <summary>
    /// Indicates if the admin username has been changed from default
    /// </summary>
    public bool AdminUsernameChanged { get; set; } = false;
    
    /// <summary>
    /// Indicates if Ashat AI assistant is enabled
    /// </summary>
    public bool AshatEnabled { get; set; } = false;
    
    /// <summary>
    /// System requirements check passed
    /// </summary>
    public bool SystemRequirementsMet { get; set; } = false;
    
    /// <summary>
    /// List of warnings from system requirements check
    /// </summary>
    public List<string> SystemWarnings { get; set; } = new();
    
    /// <summary>
    /// CMS path
    /// </summary>
    public string? CmsPath { get; set; }
    
    /// <summary>
    /// Main server URL for license validation (default: US-Omega)
    /// </summary>
    public string MainServerUrl { get; set; } = "https://us-omega.ASHATOS.io";
    
    /// <summary>
    /// Server port for Kestrel web server (default: 7077)
    /// </summary>
    public int Port { get; set; } = 7077;
    
    /// <summary>
    /// Skip license server validation in Dev mode for Super Admin setup
    /// </summary>
    public bool SkipLicenseValidation { get; set; } = false;
    
    /// <summary>
    /// Indicates if the site is in "Under Construction" mode
    /// When enabled, non-admin users see a friendly Under Construction page
    /// Defaults to false; set to true during first-run initialization until admin completes initial setup
    /// </summary>
    public bool UnderConstruction { get; set; } = false;
    
    /// <summary>
    /// Custom message for the Under Construction page
    /// </summary>
    public string? UnderConstructionMessage { get; set; }
    
    /// <summary>
    /// Custom robot image URL for the Under Construction page
    /// If null, uses default cute robot face
    /// </summary>
    public string? UnderConstructionRobotImage { get; set; }
    
    /// <summary>
    /// FTP server username for local FTP server access
    /// Used by ASHATOS to connect to the local FTP server at localhost:21
    /// </summary>
    public string? FtpUsername { get; set; }
    
    /// <summary>
    /// FTP server password for local FTP server access
    /// Used by ASHATOS to connect to the local FTP server at localhost:21
    /// </summary>
    public string? FtpPassword { get; set; }
    
    /// <summary>
    /// FTP server host (defaults to localhost)
    /// </summary>
    public string FtpHost { get; set; } = "localhost";
    
    /// <summary>
    /// FTP server port (defaults to 21)
    /// </summary>
    public int FtpPort { get; set; } = 21;
    
    /// <summary>
    /// Indicates if the server has been activated with a valid license
    /// Server activation is required after onboarding to access the control panel
    /// </summary>
    public bool ServerActivated { get; set; } = false;
    
    /// <summary>
    /// Timestamp when the server was activated
    /// </summary>
    public DateTime? ActivatedAt { get; set; }
}

/// <summary>
/// Result of initialization step
/// </summary>
public class InitializationStepResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
}
