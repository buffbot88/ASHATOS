namespace Abstractions;

/// <summary>
/// Phase 6: Advanced Security & Moderation, Cross-Platform, and Economy Interfaces
/// Provides threat detection, auto-patching, behavior analytics, native apps, AR/VR, voice, device sync, and universal economy
/// </summary>

#region Threat Detection Module

/// <summary>
/// Interface for real-time threat and anomaly detection
/// </summary>
public interface IThreatDetectionModule : IDisposable
{
    /// <summary>
    /// Analyze player behavior for threats
    /// </summary>
    Task<ThreatAnalysis> AnalyzePlayerBehaviorAsync(Guid playerId, PlayerActivity activity);
    
    /// <summary>
    /// Get all active threat alerts
    /// </summary>
    Task<IEnumerable<ThreatAlert>> GetActiveThreatAlertsAsync();
    
    /// <summary>
    /// Mitigate a detected threat
    /// </summary>
    Task<bool> MitigateThreatAsync(Guid threatId, MitigationAction action);
    
    /// <summary>
    /// Block a player due to security concerns
    /// </summary>
    Task<bool> BlockPlayerAsync(Guid playerId, BlockReason reason);
    
    /// <summary>
    /// Get security metrics
    /// </summary>
    Task<SecurityMetrics> GetSecurityMetricsAsync();
    
    /// <summary>
    /// Get security incident history
    /// </summary>
    Task<IEnumerable<SecurityIncident>> GetIncidentHistoryAsync();
}

public class PlayerActivity
{
    public Guid PlayerId { get; set; }
    public Phase6ActivityType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class ThreatAnalysis
{
    public Guid AnalysisId { get; set; }
    public Guid PlayerId { get; set; }
    public ThreatLevel Level { get; set; }
    public float ConfidenceScore { get; set; }
    public List<ThreatIndicator> Indicators { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}

public class ThreatIndicator
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float Severity { get; set; }
}

public class ThreatAlert
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public ThreatType Type { get; set; }
    public ThreatLevel Level { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public Phase6AlertStatus Status { get; set; }
}

public class MitigationAction
{
    public MitigationType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class SecurityMetrics
{
    public int TotalThreatsDetected { get; set; }
    public int ThreatsBlocked { get; set; }
    public int PlayersBlocked { get; set; }
    public float AverageResponseTimeMs { get; set; }
    public Dictionary<ThreatType, int> ThreatsByType { get; set; } = new();
}

public class SecurityIncident
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public ThreatType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public bool Resolved { get; set; }
}

// Phase6ActivityType to avoid conflict with existing ActivityType
public enum Phase6ActivityType
{
    Login,
    GameAction,
    Chat,
    Trade,
    ResourceGathering,
    Combat
}

public enum ThreatLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum ThreatType
{
    Cheating,
    Botting,
    Exploitation,
    Harassment,
    Hacking,
    DDoS,
    AccountCompromise
}

// AlertStatus enum renamed to avoid conflict
public enum Phase6AlertStatus
{
    Active,
    Investigating,
    Mitigated,
    FalsePositive
}

public enum MitigationType
{
    Warn,
    TemporaryBlock,
    PermanentBan,
    RequireVerification,
    RateLimitIncrease
}

public enum BlockReason
{
    Cheating,
    Hacking,
    Abuse,
    SuspiciousActivity,
    TermsViolation
}

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}

#endregion

#region Auto Patch Module

/// <summary>
/// Interface for automatic vulnerability detection and patching
/// </summary>
public interface IAutoPatchModule : IDisposable
{
    /// <summary>
    /// Scan for vulnerabilities
    /// </summary>
    Task<IEnumerable<Vulnerability>> ScanForVulnerabilitiesAsync();
    
    /// <summary>
    /// Get vulnerability report
    /// </summary>
    Task<VulnerabilityReport> GetVulnerabilityReportAsync();
    
    /// <summary>
    /// Generate a patch for a vulnerability
    /// </summary>
    Task<Patch> GeneratePatchAsync(Guid vulnerabilityId);
    
    /// <summary>
    /// Apply a patch
    /// </summary>
    Task<PatchResult> ApplyPatchAsync(Guid patchId);
    
    /// <summary>
    /// Rollback a patch
    /// </summary>
    Task<bool> RollbackPatchAsync(Guid patchId);
    
    /// <summary>
    /// Get patch status
    /// </summary>
    Task<PatchStatus> GetPatchStatusAsync(Guid patchId);
    
    /// <summary>
    /// Get patch history
    /// </summary>
    Task<IEnumerable<PatchHistory>> GetPatchHistoryAsync();
}

public class Vulnerability
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public VulnerabilitySeverity Severity { get; set; }
    public string AffectedComponent { get; set; } = string.Empty;
    public DateTime DiscoveredAt { get; set; }
    public bool HasPatch { get; set; }
}

public class VulnerabilityReport
{
    public int TotalVulnerabilities { get; set; }
    public Dictionary<VulnerabilitySeverity, int> BySeverity { get; set; } = new();
    public List<Vulnerability> CriticalVulnerabilities { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class Patch
{
    public Guid Id { get; set; }
    public Guid VulnerabilityId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[] PatchData { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
}

public class PatchResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public List<string> Changes { get; set; } = new();
}

public class PatchStatus
{
    public Guid PatchId { get; set; }
    public PatchState State { get; set; }
    public float ProgressPercentage { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
}

public class PatchHistory
{
    public Guid PatchId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public bool WasRolledBack { get; set; }
    public string Outcome { get; set; } = string.Empty;
}

public enum VulnerabilitySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum PatchState
{
    Generated,
    Testing,
    Applying,
    Applied,
    Failed,
    RolledBack
}

#endregion

#region Behavior Analytics Module

/// <summary>
/// Interface for player behavior analysis and anomaly detection
/// </summary>
public interface IBehaviorAnalyticsModule : IDisposable
{
    /// <summary>
    /// Analyze player behavior patterns
    /// </summary>
    Task<BehaviorProfile> AnalyzeBehaviorAsync(Guid playerId);
    
    /// <summary>
    /// Detect anomalies in behavior
    /// </summary>
    Task<IEnumerable<BehaviorAnomaly>> DetectAnomaliesAsync(Guid playerId);
    
    /// <summary>
    /// Get behavioral baseline for a player
    /// </summary>
    Task<BehaviorBaseline> GetBehaviorBaselineAsync(Guid playerId);
    
    /// <summary>
    /// Update behavior model with new data
    /// </summary>
    Task<bool> UpdateBehaviorModelAsync(Guid playerId, PlayerActivity activity);
}

public class BehaviorProfile
{
    public Guid PlayerId { get; set; }
    public Dictionary<string, float> BehaviorMetrics { get; set; } = new();
    public List<string> TypicalPatterns { get; set; } = new();
    public float AnomalyScore { get; set; }
    public DateTime LastAnalyzed { get; set; }
}

public class BehaviorAnomaly
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public AnomalyType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public float Severity { get; set; }
    public DateTime DetectedAt { get; set; }
}

public class BehaviorBaseline
{
    public Guid PlayerId { get; set; }
    public Dictionary<string, float> AverageMetrics { get; set; } = new();
    public Dictionary<string, float> StandardDeviations { get; set; } = new();
    public int SampleSize { get; set; }
}

public enum AnomalyType
{
    UnusualPlayTime,
    AbnormalProgress,
    SuspiciousTrading,
    ResourceExploitation,
    BehaviorChange
}

#endregion

#region Native App Generator Module

/// <summary>
/// Interface for generating native applications for multiple platforms
/// </summary>
public interface INativeAppGeneratorModule : IDisposable
{
    /// <summary>
    /// Generate Android app
    /// </summary>
    Task<AppPackage> GenerateAndroidAppAsync(Guid gameId, AndroidConfig config);
    
    /// <summary>
    /// Generate iOS app
    /// </summary>
    Task<AppPackage> GenerateiOSAppAsync(Guid gameId, iOSConfig config);
    
    /// <summary>
    /// Generate desktop app
    /// </summary>
    Task<AppPackage> GenerateDesktopAppAsync(Guid gameId, DesktopConfig config);
    
    /// <summary>
    /// Set app configuration
    /// </summary>
    Task<bool> SetAppConfigAsync(Guid gameId, AppPlatform platform, AppConfig config);
    
    /// <summary>
    /// Prepare app for store submission
    /// </summary>
    Task<PublishResult> PrepareForStoreAsync(AppPackage package, AppStore store);
}

public class AndroidConfig : AppConfig
{
    public string PackageName { get; set; } = string.Empty;
    public int MinSDKVersion { get; set; } = 21;
    public int TargetSDKVersion { get; set; } = 33;
}

public class iOSConfig : AppConfig
{
    public string BundleIdentifier { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public int MinimumOSVersion { get; set; } = 13;
}

public class DesktopConfig : AppConfig
{
    public DesktopPlatform Platform { get; set; }
    public bool Include32Bit { get; set; }
}

public class AppConfig
{
    public string AppName { get; set; } = string.Empty;
    public byte[] Icon { get; set; } = Array.Empty<byte>();
    public byte[] SplashScreen { get; set; } = Array.Empty<byte>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class AppPackage
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public AppPlatform Platform { get; set; }
    public string PackagePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public enum AppPlatform
{
    Android,
    iOS,
    Windows,
    MacOS,
    Linux
}

public enum DesktopPlatform
{
    Windows,
    MacOS,
    Linux
}

public enum AppStore
{
    GooglePlay,
    AppleAppStore,
    MicrosoftStore,
    Steam
}

#endregion

#region Voice Interface Module

/// <summary>
/// Interface for speech recognition and synthesis
/// </summary>
public interface IVoiceInterfaceModule : IDisposable
{
    /// <summary>
    /// Recognize voice command from audio
    /// </summary>
    Task<VoiceCommand> RecognizeCommandAsync(AudioStream audio);
    
    /// <summary>
    /// Transcribe audio to text
    /// </summary>
    Task<string> TranscribeAudioAsync(AudioStream audio, string language);
    
    /// <summary>
    /// Synthesize speech from text
    /// </summary>
    Task<AudioStream> SynthesizeSpeechAsync(string text, VoiceProfile profile);
    
    /// <summary>
    /// Get available voice profiles
    /// </summary>
    Task<IEnumerable<VoiceProfile>> GetAvailableVoicesAsync();
    
    /// <summary>
    /// Set user's voice profile preference
    /// </summary>
    Task<bool> SetVoiceProfileAsync(Guid userId, VoiceProfile profile);
    
    /// <summary>
    /// Train voice model for user
    /// </summary>
    Task<bool> TrainVoiceModelAsync(Guid userId, AudioSamples samples);
}

public class AudioStream
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public AudioFormat Format { get; set; }
    public int SampleRate { get; set; } = 44100;
    public int Channels { get; set; } = 1;
}

public class VoiceCommand
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public float Confidence { get; set; }
}

public class VoiceProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = "en-US";
    public VoiceGender Gender { get; set; }
    public float Pitch { get; set; } = 1.0f;
    public float Speed { get; set; } = 1.0f;
}

public class AudioSamples
{
    public List<AudioStream> Samples { get; set; } = new();
}

public enum AudioFormat
{
    WAV,
    MP3,
    OGG,
    FLAC
}

public enum VoiceGender
{
    Male,
    Female,
    Neutral
}

#endregion

#region AR/VR Module

/// <summary>
/// Interface for augmented and virtual reality integration
/// </summary>
public interface IARVRModule : IDisposable
{
    /// <summary>
    /// Create AR overlay for a game
    /// </summary>
    Task<AROverlay> CreateAROverlayAsync(Guid gameId, ARConfig config);
    
    /// <summary>
    /// Render AR object at spatial anchor
    /// </summary>
    Task<bool> RenderARObjectAsync(ARObject obj, SpatialAnchor anchor);
    
    /// <summary>
    /// Convert scene to VR format
    /// </summary>
    Task<VRScene> ConvertToVRAsync(Guid sceneId);
    
    /// <summary>
    /// Enable VR mode for a game
    /// </summary>
    Task<bool> EnableVRModeAsync(Guid gameId, VRPlatform platform);
    
    /// <summary>
    /// Get tracking data from AR/VR devices
    /// </summary>
    Task<TrackingData> GetTrackingDataAsync();
    
    /// <summary>
    /// Calibrate tracking system
    /// </summary>
    Task<bool> CalibrateTrackingAsync();
}

public class ARConfig
{
    public bool EnablePlaneDetection { get; set; } = true;
    public bool EnableImageTracking { get; set; }
    public bool EnableFaceTracking { get; set; }
    public float RenderDistance { get; set; } = 10f;
}

public class AROverlay
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public List<ARObject> Objects { get; set; } = new();
}

public class ARObject
{
    public Guid Id { get; set; }
    public string ModelPath { get; set; } = string.Empty;
    public Vector3 Position { get; set; } = new();
    public Vector3 Rotation { get; set; } = new();
    public Vector3 Scale { get; set; } = new() { X = 1, Y = 1, Z = 1 };
}

public class SpatialAnchor
{
    public Guid Id { get; set; }
    public Vector3 Position { get; set; } = new();
    public Vector3 Rotation { get; set; } = new();
    public AnchorType Type { get; set; }
}

public class VRScene
{
    public Guid Id { get; set; }
    public Guid SourceSceneId { get; set; }
    public List<VRObject> Objects { get; set; } = new();
    public VREnvironment Environment { get; set; } = new();
}

public class VRObject
{
    public Guid Id { get; set; }
    public string ModelPath { get; set; } = string.Empty;
    public Vector3 Position { get; set; } = new();
    public Vector3 Rotation { get; set; } = new();
    public bool IsInteractive { get; set; }
}

public class VREnvironment
{
    public string SkyboxPath { get; set; } = string.Empty;
    public float Brightness { get; set; } = 1.0f;
    public Vector3 AmbientColor { get; set; } = new();
}

public class TrackingData
{
    public Vector3 HeadPosition { get; set; } = new();
    public Vector3 HeadRotation { get; set; } = new();
    public Vector3? LeftHandPosition { get; set; }
    public Vector3? RightHandPosition { get; set; }
    public Dictionary<string, float> ButtonStates { get; set; } = new();
}

// Vector3 class is already defined in IGameEngineModule.cs

public enum VRPlatform
{
    Oculus,
    SteamVR,
    PSVR,
    WindowsMR,
    Vive
}

public enum AnchorType
{
    Plane,
    Image,
    Face,
    Object
}

#endregion

#region Device Sync Module

/// <summary>
/// Interface for cross-device state synchronization
/// </summary>
public interface IDeviceSyncModule : IDisposable
{
    /// <summary>
    /// Register a device for a user
    /// </summary>
    Task<DeviceRegistration> RegisterDeviceAsync(Guid userId, DeviceInfo device);
    
    /// <summary>
    /// Sync game state to all user devices
    /// </summary>
    Task<bool> SyncStateAsync(Guid userId, GameState state);
    
    /// <summary>
    /// Get synced state for a user
    /// </summary>
    Task<GameState> GetSyncedStateAsync(Guid userId);
    
    /// <summary>
    /// Get all registered devices for a user
    /// </summary>
    Task<IEnumerable<DeviceRegistration>> GetUserDevicesAsync(Guid userId);
    
    /// <summary>
    /// Unregister a device
    /// </summary>
    Task<bool> UnregisterDeviceAsync(Guid deviceId);
}

public class DeviceInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public DeviceType Type { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
}

public class DeviceRegistration
{
    public Guid DeviceId { get; set; }
    public Guid UserId { get; set; }
    public DeviceInfo Info { get; set; } = new();
    public DateTime RegisteredAt { get; set; }
    public DateTime LastSyncAt { get; set; }
}

public class GameState
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public Dictionary<string, object> StateData { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; }
}

public enum DeviceType
{
    Desktop,
    Mobile,
    Tablet,
    Console,
    VR
}

#endregion

#region Universal Economy Enhancements

/// <summary>
/// Interface for cross-game asset registry
/// </summary>
public interface IAssetRegistryModule : IDisposable
{
    /// <summary>
    /// Register an asset with ownership
    /// </summary>
    Task<AssetToken> RegisterAssetAsync(AssetDefinition asset, Guid ownerId);
    
    /// <summary>
    /// Transfer asset ownership
    /// </summary>
    Task<bool> TransferAssetAsync(Guid assetId, Guid fromId, Guid toId);
    
    /// <summary>
    /// Get all assets owned by a user
    /// </summary>
    Task<IEnumerable<AssetToken>> GetUserAssetsAsync(Guid userId);
    
    /// <summary>
    /// Get asset ownership information
    /// </summary>
    Task<AssetOwnership> GetAssetOwnershipAsync(Guid assetId);
    
    /// <summary>
    /// Verify ownership
    /// </summary>
    Task<bool> VerifyOwnershipAsync(Guid assetId, Guid userId);
    
    /// <summary>
    /// Get asset transfer history
    /// </summary>
    Task<IEnumerable<TransferHistory>> GetAssetHistoryAsync(Guid assetId);
}

public class AssetDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Phase6AssetType Type { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public byte[]? AssetData { get; set; }
}

public class AssetToken
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Phase6AssetType Type { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BlockchainHash { get; set; } = string.Empty;
}

public class AssetOwnership
{
    public Guid AssetId { get; set; }
    public Guid CurrentOwnerId { get; set; }
    public Guid OriginalOwnerId { get; set; }
    public int TransferCount { get; set; }
    public DateTime LastTransferAt { get; set; }
}

public class TransferHistory
{
    public Guid TransferId { get; set; }
    public Guid AssetId { get; set; }
    public Guid FromOwnerId { get; set; }
    public Guid ToOwnerId { get; set; }
    public DateTime TransferredAt { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
}

// AssetType enum renamed to avoid conflict  
public enum Phase6AssetType
{
    Character,
    Item,
    Skin,
    Pet,
    Vehicle,
    Property,
    Currency
}

/// <summary>
/// Interface for universal cross-game inventory
/// </summary>
public interface IUniversalInventoryModule : IDisposable
{
    /// <summary>
    /// Get user's universal inventory
    /// </summary>
    Task<Inventory> GetUniversalInventoryAsync(Guid userId);
    
    /// <summary>
    /// Add item to universal inventory
    /// </summary>
    Task<bool> AddItemAsync(Guid userId, UniversalItem item);
    
    /// <summary>
    /// Remove item from inventory
    /// </summary>
    Task<bool> RemoveItemAsync(Guid userId, Guid itemId);
    
    /// <summary>
    /// Transfer item to a specific game
    /// </summary>
    Task<bool> TransferItemToGameAsync(Guid itemId, Guid userId, Guid targetGameId);
    
    /// <summary>
    /// Get games compatible with an item
    /// </summary>
    Task<IEnumerable<CompatibleGame>> GetCompatibleGamesAsync(Guid itemId);
    
    /// <summary>
    /// Convert item for use in a specific game
    /// </summary>
    Task<ConversionResult> ConvertItemAsync(Guid itemId, Guid targetGameId);
}

public class Inventory
{
    public Guid UserId { get; set; }
    public List<UniversalItem> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int MaxCapacity { get; set; } = 1000;
}

public class UniversalItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Phase6AssetType Type { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<Guid> CompatibleGames { get; set; } = new();
}

public class CompatibleGame
{
    public Guid GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public bool RequiresConversion { get; set; }
}

public class ConversionResult
{
    public bool Success { get; set; }
    public Guid? ConvertedItemId { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Asset Security Module

/// <summary>
/// Interface for asset security with watermark detection and ownership verification
/// </summary>
public interface IAssetSecurityModule : IDisposable
{
    /// <summary>
    /// Scan asset for watermarks, copyright, or embedded identifiers
    /// </summary>
    Task<WatermarkScanResult> ScanForWatermarksAsync(AssetDefinition asset);
    
    /// <summary>
    /// Verify ownership or rights to an asset
    /// </summary>
    Task<OwnershipVerificationResult> VerifyOwnershipAsync(AssetDefinition asset, Guid userId);
    
    /// <summary>
    /// Import asset with security checks
    /// </summary>
    Task<AssetImportResult> ImportAssetAsync(AssetDefinition asset, Guid ownerId, bool requireOwnershipProof = true);
    
    /// <summary>
    /// Block asset import if verification fails
    /// </summary>
    Task<bool> BlockAssetImportAsync(Guid assetId, string reason);
}

public class WatermarkScanResult
{
    public bool HasWatermark { get; set; }
    public List<WatermarkInfo> DetectedWatermarks { get; set; } = new();
    public string ScanDetails { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
}

public class WatermarkInfo
{
    public string Type { get; set; } = string.Empty; // "copyright", "trademark", "identifier"
    public string Content { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // "metadata", "embedded", "steganographic"
    public float Confidence { get; set; } // 0.0 to 1.0
}

public class OwnershipVerificationResult
{
    public bool IsVerified { get; set; }
    public OwnershipStatus Status { get; set; }
    public string VerificationMethod { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
}

public class AssetImportResult
{
    public bool Success { get; set; }
    public Guid? AssetId { get; set; }
    public string Message { get; set; } = string.Empty;
    public WatermarkScanResult? WatermarkScan { get; set; }
    public OwnershipVerificationResult? OwnershipVerification { get; set; }
    public string LocalStoragePath { get; set; } = string.Empty;
}

public enum OwnershipStatus
{
    Verified,
    Unverified,
    RequiresProof,
    Blocked,
    Custom
}

#endregion
