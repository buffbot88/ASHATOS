namespace Abstractions;

/// <summary>
/// Phase 6: Distributed Cloud & Edge Networking Interfaces
/// Provides mesh networking, edge computing, load balancing, and failover capabilities
/// </summary>

#region Distributed Network Module

/// <summary>
/// Interface for distributed mesh networking coordination
/// </summary>
public interface IDistributedNetworkModule : IDisposable
{
    /// <summary>
    /// Register a peer node in the mesh network
    /// </summary>
    Task<bool> RegisterPeerAsync(PeerNode peer);
    
    /// <summary>
    /// Unregister a peer node from the mesh network
    /// </summary>
    Task<bool> UnregisterPeerAsync(Guid peerId);
    
    /// <summary>
    /// Get all currently active peers
    /// </summary>
    Task<IEnumerable<PeerNode>> GetActivePeersAsync();
    
    /// <summary>
    /// Broadcast a message to all peers
    /// </summary>
    Task<bool> BroadcastMessageAsync(NetworkMessage message);
    
    /// <summary>
    /// Send a message to a specific peer
    /// </summary>
    Task<bool> SendMessageAsync(Guid peerId, NetworkMessage message);
    
    /// <summary>
    /// Check health status of a peer
    /// </summary>
    Task<PeerHealth> CheckPeerHealthAsync(Guid peerId);
    
    /// <summary>
    /// Get current network topology
    /// </summary>
    Task<NetworkTopology> GetNetworkTopologyAsync();
}

public class PeerNode
{
    public Guid Id { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public int Port { get; set; }
    public EdgeLocation Location { get; set; }
    public int Capacity { get; set; }
    public int CurrentLoad { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public PeerStatus Status { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class NetworkMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public DateTime Timestamp { get; set; }
    public MessageType Type { get; set; }
    public string Payload { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class PeerHealth
{
    public Guid PeerId { get; set; }
    public HealthStatus Status { get; set; }
    public float CpuUsage { get; set; }
    public float MemoryUsage { get; set; }
    public float NetworkLatency { get; set; }
    public int ActiveConnections { get; set; }
    public DateTime LastCheck { get; set; }
}

public class NetworkTopology
{
    public List<PeerNode> Nodes { get; set; } = new();
    public List<PeerConnection> Connections { get; set; } = new();
    public int TotalNodes { get; set; }
    public int HealthyNodes { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class PeerConnection
{
    public Guid FromPeerId { get; set; }
    public Guid ToPeerId { get; set; }
    public float Latency { get; set; }
    public long Bandwidth { get; set; }
    public ConnectionStatus Status { get; set; }
}

public enum EdgeLocation
{
    US_WEST,
    US_EAST,
    US_CENTRAL,
    EU_WEST,
    EU_CENTRAL,
    EU_EAST,
    ASIA_PACIFIC,
    ASIA_EAST,
    SOUTH_AMERICA,
    MIDDLE_EAST,
    AFRICA,
    AUSTRALIA
}

public enum PeerStatus
{
    Active,
    Idle,
    Degraded,
    Offline,
    Maintenance
}

public enum MessageType
{
    Heartbeat,
    StateSync,
    PlayerAction,
    ServerCommand,
    HealthCheck,
    Discovery
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Critical,
    Unknown
}

public enum ConnectionStatus
{
    Connected,
    Connecting,
    Disconnected,
    Failed
}

#endregion

#region Edge AI Module

/// <summary>
/// Interface for edge computing and local AI inference
/// </summary>
public interface IEdgeAIModule : IDisposable
{
    /// <summary>
    /// Deploy an AI model to an edge location
    /// </summary>
    Task<bool> DeployModelAsync(string modelId, EdgeLocation location);
    
    /// <summary>
    /// Update a deployed model to a new version
    /// </summary>
    Task<bool> UpdateModelAsync(string modelId, string newVersion);
    
    /// <summary>
    /// Get all models deployed to edge nodes
    /// </summary>
    Task<IEnumerable<DeployedModel>> GetDeployedModelsAsync();
    
    /// <summary>
    /// Perform inference using edge AI (with cloud fallback)
    /// </summary>
    Task<AIResponse> InferAsync(string modelId, AIRequest request);
    
    /// <summary>
    /// Perform inference locally at edge (no cloud fallback)
    /// </summary>
    Task<AIResponse> InferLocallyAsync(string modelId, AIRequest request);
    
    /// <summary>
    /// Get performance metrics for an edge location
    /// </summary>
    Task<EdgePerformanceMetrics> GetMetricsAsync(EdgeLocation location);
}

public class DeployedModel
{
    public string ModelId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public EdgeLocation Location { get; set; }
    public DateTime DeployedAt { get; set; }
    public long ModelSizeBytes { get; set; }
    public ModelStatus Status { get; set; }
    public int InferenceCount { get; set; }
    public float AverageInferenceTimeMs { get; set; }
}

public class AIRequest
{
    public Guid RequestId { get; set; }
    public string Input { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
}

public class AIResponse
{
    public Guid RequestId { get; set; }
    public string Output { get; set; } = string.Empty;
    public float ConfidenceScore { get; set; }
    public long InferenceTimeMs { get; set; }
    public bool UsedEdge { get; set; }
    public EdgeLocation? EdgeLocation { get; set; }
}

public class EdgePerformanceMetrics
{
    public EdgeLocation Location { get; set; }
    public int TotalInferences { get; set; }
    public float AverageLatencyMs { get; set; }
    public float P95LatencyMs { get; set; }
    public float P99LatencyMs { get; set; }
    public int ErrorCount { get; set; }
    public float ErrorRate { get; set; }
    public DateTime CollectedAt { get; set; }
}

public enum ModelStatus
{
    Deploying,
    Active,
    Updating,
    Failed,
    Deprecated
}

#endregion

#region Load Balancer Module

/// <summary>
/// Interface for intelligent traffic distribution and load balancing
/// </summary>
public interface ILoadBalancerModule : IDisposable
{
    /// <summary>
    /// Assign a server to a player request
    /// </summary>
    Task<ServerAssignment> AssignServerAsync(PlayerRequest request);
    
    /// <summary>
    /// Register a server instance with the load balancer
    /// </summary>
    Task<bool> RegisterServerAsync(ServerInstance server);
    
    /// <summary>
    /// Unregister a server instance
    /// </summary>
    Task<bool> UnregisterServerAsync(Guid serverId);
    
    /// <summary>
    /// Get current load for a specific server
    /// </summary>
    Task<ServerLoad> GetServerLoadAsync(Guid serverId);
    
    /// <summary>
    /// Get load balancing statistics
    /// </summary>
    Task<LoadBalancingStats> GetStatsAsync();
    
    /// <summary>
    /// Set the load balancing strategy
    /// </summary>
    Task<bool> SetBalancingStrategyAsync(BalancingStrategy strategy);
}

public class PlayerRequest
{
    public Guid PlayerId { get; set; }
    public Guid GameId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public EdgeLocation? PreferredLocation { get; set; }
    public Dictionary<string, string> Requirements { get; set; } = new();
}

public class ServerAssignment
{
    public Guid AssignmentId { get; set; }
    public Guid ServerId { get; set; }
    public string ServerAddress { get; set; } = string.Empty;
    public int ServerPort { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public float ExpectedLatency { get; set; }
}

public class ServerInstance
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public EdgeLocation Location { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentPlayers { get; set; }
    public Phase6ServerStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class ServerLoad
{
    public Guid ServerId { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxCapacity { get; set; }
    public float LoadPercentage { get; set; }
    public float CpuUsage { get; set; }
    public float MemoryUsage { get; set; }
    public float NetworkUsage { get; set; }
    public DateTime MeasuredAt { get; set; }
}

public class LoadBalancingStats
{
    public int TotalServers { get; set; }
    public int ActiveServers { get; set; }
    public int TotalPlayers { get; set; }
    public float AverageLoadPercentage { get; set; }
    public long TotalAssignments { get; set; }
    public Dictionary<EdgeLocation, int> PlayersByLocation { get; set; } = new();
}

public enum BalancingStrategy
{
    RoundRobin,
    LeastConnections,
    Geographic,
    CapacityBased,
    LatencyBased,
    Hybrid
}

// ServerStatus enum renamed to avoid conflict
public enum Phase6ServerStatus
{
    Online,
    Starting,
    Stopping,
    Offline,
    Maintenance,
    Overloaded
}

#endregion

#region Failover Module

/// <summary>
/// Interface for high availability and automatic failover
/// </summary>
public interface IFailoverModule : IDisposable
{
    /// <summary>
    /// Monitor health of a server instance
    /// </summary>
    Task<HealthStatus> MonitorHealthAsync(Guid serverId);
    
    /// <summary>
    /// Get all active health alerts
    /// </summary>
    Task<IEnumerable<HealthAlert>> GetActiveAlertsAsync();
    
    /// <summary>
    /// Trigger manual failover for a failed server
    /// </summary>
    Task<FailoverResult> TriggerFailoverAsync(Guid failedServerId);
    
    /// <summary>
    /// Configure replication between primary and secondary servers
    /// </summary>
    Task<bool> ConfigureReplicationAsync(Guid primaryId, Guid secondaryId);
    
    /// <summary>
    /// Get recovery status for a server
    /// </summary>
    Task<RecoveryStatus> GetRecoveryStatusAsync(Guid serverId);
    
    /// <summary>
    /// Initiate recovery process for a server
    /// </summary>
    Task<bool> InitiateRecoveryAsync(Guid serverId);
}

public class HealthAlert
{
    public Guid Id { get; set; }
    public Guid ServerId { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public AlertStatus Status { get; set; }
}

public class FailoverResult
{
    public Guid FailoverId { get; set; }
    public Guid FailedServerId { get; set; }
    public Guid? NewServerId { get; set; }
    public bool Success { get; set; }
    public int PlayersTransferred { get; set; }
    public long FailoverTimeMs { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RecoveryStatus
{
    public Guid ServerId { get; set; }
    public RecoveryState State { get; set; }
    public int StepsCompleted { get; set; }
    public int TotalSteps { get; set; }
    public float ProgressPercentage { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum AlertStatus
{
    Active,
    Acknowledged,
    Resolved,
    Escalated
}

public enum RecoveryState
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

#endregion
