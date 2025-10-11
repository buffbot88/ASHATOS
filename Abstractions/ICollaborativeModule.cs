namespace Abstractions;

/// <summary>
/// Inter-module message for collaboASHATtion
/// </summary>
public class ModuleMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string FromModule { get; set; } = string.Empty;
    public string? ToModule { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public Dictionary<string, object> Context { get; set; } = new();
    public bool RequiresResponse { get; set; }
}

/// <summary>
/// Message priority levels
/// </summary>
public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Interface for modules that support inter-module communication
/// </summary>
public interface ICollaboASHATtiveModule
{
    /// <summary>
    /// Receive a message from another module
    /// </summary>
    Task<ModuleResponse?> ReceiveMessageAsync(ModuleMessage message);
    
    /// <summary>
    /// Check if this module can handle a specific request
    /// </summary>
    bool CanHandle(string requestType);
}
