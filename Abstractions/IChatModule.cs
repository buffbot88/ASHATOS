namespace Abstractions;

/// <summary>
/// Chat module interface for real-time messaging.
/// </summary>
public interface IChatModule
{
    /// <summary>
    /// Get all chat rooms.
    /// </summary>
    Task<List<ChatRoom>> GetRoomsAsync();
    
    /// <summary>
    /// Get a specific chat room by ID.
    /// </summary>
    Task<ChatRoom?> GetRoomByIdAsync(string roomId);
    
    /// <summary>
    /// Create a new chat room.
    /// </summary>
    Task<(bool success, string message, string? roomId)> CreateRoomAsync(string name, string createdBy, bool isPrivate = false);
    
    /// <summary>
    /// Get messages for a chat room.
    /// </summary>
    Task<List<ChatMessage>> GetMessagesAsync(string roomId, int limit = 50);
    
    /// <summary>
    /// Send a message to a chat room.
    /// </summary>
    Task<(bool success, string message, string? messageId)> SendMessageAsync(string roomId, string userId, string username, string content);
    
    /// <summary>
    /// Delete a message.
    /// </summary>
    Task<bool> DeleteMessageAsync(string messageId, string userId);
    
    /// <summary>
    /// Get active users in a room.
    /// </summary>
    Task<List<ChatUser>> GetActiveUsersAsync(string roomId);
    
    /// <summary>
    /// Join a chat room.
    /// </summary>
    Task<bool> JoinRoomAsync(string roomId, string userId, string username);
    
    /// <summary>
    /// Leave a chat room.
    /// </summary>
    Task<bool> LeaveRoomAsync(string roomId, string userId);
}

public class ChatRoom
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPrivate { get; set; }
    public int MessageCount { get; set; }
    public int ActiveUserCount { get; set; }
}

public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ContentRating ContentRating { get; set; } = ContentRating.Everyone;
}

public class ChatUser
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
}
