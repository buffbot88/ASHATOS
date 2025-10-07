namespace Abstractions;

/// <summary>
/// Phase 6: Universal API & Extensibility Interfaces
/// Provides enhanced plugin marketplace, API generation, and SDK tools
/// </summary>

#region Enhanced Plugin Marketplace Module

/// <summary>
/// Interface for enhanced plugin marketplace with discovery, ratings, and publishing
/// </summary>
public interface IPluginMarketplaceModule : IDisposable
{
    /// <summary>
    /// Search for plugins with filters
    /// </summary>
    Task<IEnumerable<Plugin>> SearchPluginsAsync(PluginSearchQuery query);
    
    /// <summary>
    /// Get detailed information about a plugin
    /// </summary>
    Task<Plugin> GetPluginDetailsAsync(Guid pluginId);
    
    /// <summary>
    /// Install a plugin for a user
    /// </summary>
    Task<InstallResult> InstallPluginAsync(Guid pluginId, Guid userId);
    
    /// <summary>
    /// Uninstall a plugin
    /// </summary>
    Task<bool> UninstallPluginAsync(Guid pluginId, Guid userId);
    
    /// <summary>
    /// Update a plugin to latest version
    /// </summary>
    Task<bool> UpdatePluginAsync(Guid pluginId);
    
    /// <summary>
    /// Publish a new plugin to the marketplace
    /// </summary>
    Task<PublishResult> PublishPluginAsync(PluginPackage package, Guid publisherId);
    
    /// <summary>
    /// Update an existing plugin
    /// </summary>
    Task<bool> UpdatePluginAsync(Guid pluginId, PluginUpdate update);
    
    /// <summary>
    /// Add a review for a plugin
    /// </summary>
    Task<bool> AddReviewAsync(Guid pluginId, PluginReview review);
    
    /// <summary>
    /// Get reviews for a plugin
    /// </summary>
    Task<IEnumerable<PluginReview>> GetReviewsAsync(Guid pluginId);
}

public class PluginSearchQuery
{
    public string? Keyword { get; set; }
    public PluginCategory? Category { get; set; }
    public decimal? MaxPrice { get; set; }
    public float? MinRating { get; set; }
    public bool FreeOnly { get; set; }
    public PluginSortBy SortBy { get; set; } = PluginSortBy.Popularity;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class Plugin
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Guid PublisherId { get; set; }
    public string PublisherName { get; set; } = string.Empty;
    public PluginCategory Category { get; set; }
    public decimal Price { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalInstalls { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> Screenshots { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public PluginStatus Status { get; set; }
}

public class PluginPackage
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public PluginCategory Category { get; set; }
    public decimal Price { get; set; }
    public byte[] Code { get; set; } = Array.Empty<byte>();
    public Dictionary<string, byte[]> Assets { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public PluginPermissions RequiredPermissions { get; set; } = new();
}

public class PluginUpdate
{
    public string? NewVersion { get; set; }
    public string? NewDescription { get; set; }
    public decimal? NewPrice { get; set; }
    public byte[]? NewCode { get; set; }
}

public class PluginReview
{
    public Guid Id { get; set; }
    public Guid PluginId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
    public int HelpfulCount { get; set; }
}

public class InstallResult
{
    public bool Success { get; set; }
    public Guid? InstallationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

public class PublishResult
{
    public bool Success { get; set; }
    public Guid? PluginId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
}

public class PluginPermissions
{
    public bool CanAccessFileSystem { get; set; }
    public bool CanAccessNetwork { get; set; }
    public bool CanAccessDatabase { get; set; }
    public bool CanModifyGameState { get; set; }
    public List<string> CustomPermissions { get; set; } = new();
}

public enum PluginCategory
{
    GameMechanics,
    UserInterface,
    Graphics,
    Audio,
    AI,
    Networking,
    Tools,
    Content,
    Utilities,
    Other
}

public enum PluginSortBy
{
    Popularity,
    Rating,
    Recent,
    PriceLowToHigh,
    PriceHighToLow,
    Name
}

public enum PluginStatus
{
    Active,
    Deprecated,
    Suspended,
    UnderReview
}

#endregion

#region API Generator Module

/// <summary>
/// Interface for automatic REST and GraphQL API generation
/// </summary>
public interface IAPIGeneratorModule : IDisposable
{
    /// <summary>
    /// Generate a REST API for a game
    /// </summary>
    Task<GeneratedAPI> GenerateRESTAPIAsync(Guid gameId);
    
    /// <summary>
    /// Generate a GraphQL API for a game
    /// </summary>
    Task<GeneratedAPI> GenerateGraphQLAPIAsync(Guid gameId);
    
    /// <summary>
    /// Get API schema
    /// </summary>
    Task<APISchema> GetAPISchemaAsync(Guid apiId);
    
    /// <summary>
    /// Update API schema
    /// </summary>
    Task<bool> UpdateSchemaAsync(Guid apiId, SchemaUpdate update);
    
    /// <summary>
    /// Generate API documentation
    /// </summary>
    Task<string> GenerateAPIDocsAsync(Guid apiId, DocumentationFormat format);
    
    /// <summary>
    /// Generate Swagger/OpenAPI documentation
    /// </summary>
    Task<SwaggerDocument> GenerateSwaggerAsync(Guid apiId);
}

public class GeneratedAPI
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public APIType Type { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public List<APIEndpoint> Endpoints { get; set; } = new();
    public RateLimitConfig RateLimit { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class APIEndpoint
{
    public string Path { get; set; } = string.Empty;
    public HttpMethod Method { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<APIParameter> Parameters { get; set; } = new();
    public APIResponse ResponseSchema { get; set; } = new();
    public bool RequiresAuthentication { get; set; }
}

public class APIParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Description { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
}

public class APIResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = "application/json";
    public Dictionary<string, string> Schema { get; set; } = new();
}

public class APISchema
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public List<TypeDefinition> Types { get; set; } = new();
    public List<QueryDefinition> Queries { get; set; } = new();
    public List<MutationDefinition> Mutations { get; set; } = new();
}

public class TypeDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = new();
}

public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Nullable { get; set; }
}

public class QueryDefinition
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<APIParameter> Parameters { get; set; } = new();
}

public class MutationDefinition
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<APIParameter> Parameters { get; set; } = new();
}

public class SchemaUpdate
{
    public List<TypeDefinition>? NewTypes { get; set; }
    public List<QueryDefinition>? NewQueries { get; set; }
    public List<MutationDefinition>? NewMutations { get; set; }
}

public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public int BurstSize { get; set; } = 10;
}

public class SwaggerDocument
{
    public string OpenApiVersion { get; set; } = "3.0.0";
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
}

public enum APIType
{
    REST,
    GraphQL,
    WebSocket,
    gRPC
}

public enum HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH
}

public enum DocumentationFormat
{
    Markdown,
    HTML,
    PDF,
    JSON
}

#endregion

#region Plugin SDK Module

/// <summary>
/// Interface for plugin development tools and SDK
/// </summary>
public interface IPluginSDKModule : IDisposable
{
    /// <summary>
    /// Initialize a new plugin project
    /// </summary>
    Task<PluginProject> InitializeProjectAsync(PluginProjectConfig config);
    
    /// <summary>
    /// Generate boilerplate code for plugin
    /// </summary>
    Task<string> GenerateBoilerplateAsync(PluginType type);
    
    /// <summary>
    /// Validate plugin code
    /// </summary>
    Task<ValidationReport> ValidatePluginAsync(byte[] pluginCode);
    
    /// <summary>
    /// Test plugin in sandbox
    /// </summary>
    Task<TestResult> TestPluginAsync(byte[] pluginCode, TestConfig testConfig);
    
    /// <summary>
    /// Package plugin for distribution
    /// </summary>
    Task<byte[]> PackagePluginAsync(PluginProject project);
}

public class PluginProjectConfig
{
    public string Name { get; set; } = string.Empty;
    public PluginType Type { get; set; }
    public string TargetVersion { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
}

public class PluginProject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public PluginType Type { get; set; }
    public Dictionary<string, string> Files { get; set; } = new();
}

public class ValidationReport
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public SecurityScanResult SecurityScan { get; set; } = new();
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public ErrorSeverity Severity { get; set; }
}

public class ValidationWarning
{
    public string Message { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class SecurityScanResult
{
    public bool IsSafe { get; set; }
    public List<SecurityIssue> Issues { get; set; } = new();
}

public class SecurityIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
}

public class TestConfig
{
    public Dictionary<string, object> InputData { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 30;
}

public class TestResult
{
    public bool Passed { get; set; }
    public string Output { get; set; } = string.Empty;
    public List<string> Logs { get; set; } = new();
    public long ExecutionTimeMs { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public enum PluginType
{
    GameMechanic,
    UIComponent,
    AIBehavior,
    ContentGenerator,
    NetworkHandler,
    DataProcessor
}

public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

#endregion

#region Sandbox Module

/// <summary>
/// Interface for secure plugin execution in isolated environments
/// </summary>
public interface ISandboxModule : IDisposable
{
    /// <summary>
    /// Create a sandbox environment
    /// </summary>
    Task<SandboxEnvironment> CreateSandboxAsync(SandboxConfig config);
    
    /// <summary>
    /// Execute code in sandbox
    /// </summary>
    Task<ExecutionResult> ExecuteInSandboxAsync(Guid sandboxId, byte[] code, ExecutionContext context);
    
    /// <summary>
    /// Get sandbox resource usage
    /// </summary>
    Task<ResourceUsage> GetResourceUsageAsync(Guid sandboxId);
    
    /// <summary>
    /// Terminate sandbox
    /// </summary>
    Task<bool> TerminateSandboxAsync(Guid sandboxId);
}

public class SandboxConfig
{
    public int MaxMemoryMB { get; set; } = 256;
    public int MaxCpuPercentage { get; set; } = 50;
    public int MaxExecutionTimeSeconds { get; set; } = 30;
    public bool AllowNetworkAccess { get; set; }
    public bool AllowFileSystemAccess { get; set; }
    public List<string> AllowedNamespaces { get; set; } = new();
}

public class SandboxEnvironment
{
    public Guid Id { get; set; }
    public SandboxConfig Config { get; set; } = new();
    public SandboxStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExecutionContext
{
    public Dictionary<string, object> Variables { get; set; } = new();
    public string EntryPoint { get; set; } = "Main";
    public List<string> Arguments { get; set; } = new();
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public object? ReturnValue { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
    public ResourceUsage ResourcesUsed { get; set; } = new();
}

public class ResourceUsage
{
    public long MemoryUsedBytes { get; set; }
    public float CpuTimeMs { get; set; }
    public int NetworkRequests { get; set; }
    public long NetworkBytesTransferred { get; set; }
    public int FileOperations { get; set; }
}

public enum SandboxStatus
{
    Created,
    Running,
    Idle,
    Terminated,
    Error
}

#endregion
