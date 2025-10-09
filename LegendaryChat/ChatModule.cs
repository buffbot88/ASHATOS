using System.Collections.Concurrent;
using Abstractions;

namespace LegendaryChat;

[RaModule(Category = "extensions")]
public sealed class ChatModule : ModuleBase, IChatModule
{
    public override string Name => "Chat";
    
    private readonly ConcurrentDictionary<string, ChatRoom> _rooms = new();
    private readonly ConcurrentDictionary<string, ChatMessage> _messages = new();
    private readonly ConcurrentDictionary<string, List<string>> _roomMessages = new();
    private readonly ConcurrentDictionary<string, List<ChatUser>> _roomUsers = new();
    private IContentModerationModule? _moderationModule;
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Try to get moderation module through reflection to avoid tight coupling
        if (manager != null)
        {
            var modulesProperty = manager.GetType().GetProperty("Modules");
            if (modulesProperty != null)
            {
                var modules = modulesProperty.GetValue(manager) as System.Collections.IEnumerable;
                if (modules != null)
                {
                    foreach (var moduleWrapper in modules)
                    {
                        var instanceProperty = moduleWrapper.GetType().GetProperty("Instance");
                        if (instanceProperty != null)
                        {
                            var instance = instanceProperty.GetValue(moduleWrapper);
                            if (instance is IContentModerationModule mod)
                            {
                                _moderationModule = mod;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        Console.WriteLine($"[{Name}] Initializing Chat Module...");
        Console.WriteLine($"[{Name}] Content moderation: {(_moderationModule != null ? "enabled" : "disabled")}");
        
        SeedExampleData();
        
        Console.WriteLine($"[{Name}] Chat Module initialized with {_rooms.Count} rooms");
    }
    
    public override string Process(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return GetHelp();
        
        var command = parts[0].ToLowerInvariant();
        
        return command switch
        {
            "rooms" => ListRooms(),
            "create" when parts.Length >= 3 => CreateRoom(parts[1], parts[2]),
            "messages" when parts.Length >= 2 => GetMessages(parts[1]),
            "send" when parts.Length >= 4 => SendMessage(parts[1], parts[2], parts[3]),
            "help" => GetHelp(),
            _ => GetHelp()
        };
    }
    
    private string GetHelp()
    {
        return @"Chat Module Commands:
  rooms - List all chat rooms
  create <name> <createdBy> - Create a new chat room
  messages <roomId> - Get messages from a room
  send <roomId> <userId> <username> - Send a message
  help - Show this help message";
    }
    
    private string ListRooms()
    {
        var task = GetRoomsAsync();
        task.Wait();
        var rooms = task.Result;
        
        if (rooms.Count == 0)
        {
            return "No chat rooms found.";
        }
        
        var lines = new List<string> { $"Found {rooms.Count} chat rooms:" };
        foreach (var room in rooms)
        {
            lines.Add($"  [{room.Id}] {room.Name} - {room.MessageCount} messages, {room.ActiveUserCount} users");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string CreateRoom(string name, string createdBy)
    {
        var task = CreateRoomAsync(name, createdBy);
        task.Wait();
        var result = task.Result;
        
        return result.success 
            ? $"Chat room created: {result.roomId}" 
            : $"Failed to create room: {result.message}";
    }
    
    private string GetMessages(string roomId)
    {
        var task = GetMessagesAsync(roomId);
        task.Wait();
        var messages = task.Result;
        
        if (messages.Count == 0)
        {
            return $"No messages in room {roomId}.";
        }
        
        var lines = new List<string> { $"Messages in room {roomId}:" };
        foreach (var msg in messages.TakeLast(10))
        {
            lines.Add($"  [{msg.Timestamp:HH:mm:ss}] {msg.Username}: {msg.Content}");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string SendMessage(string roomId, string userId, string username)
    {
        var task = SendMessageAsync(roomId, userId, username, "Test message");
        task.Wait();
        var result = task.Result;
        
        return result.success 
            ? $"Message sent: {result.messageId}" 
            : $"Failed to send message: {result.message}";
    }
    
    public async Task<List<ChatRoom>> GetRoomsAsync()
    {
        await Task.CompletedTask;
        return _rooms.Values.OrderBy(r => r.Name).ToList();
    }
    
    public async Task<ChatRoom?> GetRoomByIdAsync(string roomId)
    {
        await Task.CompletedTask;
        return _rooms.TryGetValue(roomId, out var room) ? room : null;
    }
    
    public async Task<(bool success, string message, string? roomId)> CreateRoomAsync(
        string name, string createdBy, bool isPrivate = false)
    {
        await Task.CompletedTask;
        
        var roomId = Guid.NewGuid().ToString();
        var room = new ChatRoom
        {
            Id = roomId,
            Name = name,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = isPrivate,
            MessageCount = 0,
            ActiveUserCount = 0
        };
        
        _rooms[roomId] = room;
        _roomMessages[roomId] = new List<string>();
        _roomUsers[roomId] = new List<ChatUser>();
        
        Console.WriteLine($"[{Name}] Created chat room: {name} by {createdBy}");
        return (true, "Room created successfully", roomId);
    }
    
    public async Task<List<ChatMessage>> GetMessagesAsync(string roomId, int limit = 50)
    {
        await Task.CompletedTask;
        
        if (!_roomMessages.TryGetValue(roomId, out var messageIds))
        {
            return new List<ChatMessage>();
        }
        
        return messageIds
            .Select(id => _messages.TryGetValue(id, out var msg) ? msg : null)
            .Where(m => m != null)
            .Cast<ChatMessage>()
            .OrderBy(m => m.Timestamp)
            .TakeLast(limit)
            .ToList();
    }
    
    public async Task<(bool success, string message, string? messageId)> SendMessageAsync(
        string roomId, string userId, string username, string content)
    {
        await Task.CompletedTask;
        
        if (!_rooms.TryGetValue(roomId, out var room))
        {
            return (false, "Room not found", null);
        }
        
        // Content moderation check
        if (_moderationModule != null)
        {
            var result = await _moderationModule.ScanTextAsync(content, userId, Name, roomId);
            if (result.Action == ModerationAction.Blocked || result.Action == ModerationAction.RequiresReview)
            {
                var reason = result.Violations.Any() ? result.Violations[0].Description : "Content policy violation";
                return (false, $"Message rejected: {reason}", null);
            }
        }
        
        var messageId = Guid.NewGuid().ToString();
        var message = new ChatMessage
        {
            Id = messageId,
            RoomId = roomId,
            UserId = userId,
            Username = username,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        
        _messages[messageId] = message;
        
        if (!_roomMessages.ContainsKey(roomId))
        {
            _roomMessages[roomId] = new List<string>();
        }
        _roomMessages[roomId].Add(messageId);
        
        room.MessageCount++;
        
        Console.WriteLine($"[{Name}] Message sent to room {roomId} by {username}");
        return (true, "Message sent successfully", messageId);
    }
    
    public async Task<bool> DeleteMessageAsync(string messageId, string userId)
    {
        await Task.CompletedTask;
        
        if (!_messages.TryGetValue(messageId, out var message))
        {
            return false;
        }
        
        if (message.UserId != userId)
        {
            return false; // Only the author can delete
        }
        
        _messages.TryRemove(messageId, out _);
        
        if (_roomMessages.TryGetValue(message.RoomId, out var messageIds))
        {
            messageIds.Remove(messageId);
            
            if (_rooms.TryGetValue(message.RoomId, out var room))
            {
                room.MessageCount--;
            }
        }
        
        Console.WriteLine($"[{Name}] Deleted message: {messageId}");
        return true;
    }
    
    public async Task<List<ChatUser>> GetActiveUsersAsync(string roomId)
    {
        await Task.CompletedTask;
        
        if (!_roomUsers.TryGetValue(roomId, out var users))
        {
            return new List<ChatUser>();
        }
        
        // Filter users active in the last 5 minutes
        var cutoff = DateTime.UtcNow.AddMinutes(-5);
        return users.Where(u => u.LastActiveAt >= cutoff).ToList();
    }
    
    public async Task<bool> JoinRoomAsync(string roomId, string userId, string username)
    {
        await Task.CompletedTask;
        
        if (!_rooms.TryGetValue(roomId, out var room))
        {
            return false;
        }
        
        if (!_roomUsers.ContainsKey(roomId))
        {
            _roomUsers[roomId] = new List<ChatUser>();
        }
        
        var existingUser = _roomUsers[roomId].FirstOrDefault(u => u.UserId == userId);
        if (existingUser != null)
        {
            existingUser.LastActiveAt = DateTime.UtcNow;
        }
        else
        {
            var chatUser = new ChatUser
            {
                UserId = userId,
                Username = username,
                JoinedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            _roomUsers[roomId].Add(chatUser);
            room.ActiveUserCount++;
        }
        
        Console.WriteLine($"[{Name}] User {username} joined room {roomId}");
        return true;
    }
    
    public async Task<bool> LeaveRoomAsync(string roomId, string userId)
    {
        await Task.CompletedTask;
        
        if (!_roomUsers.TryGetValue(roomId, out var users))
        {
            return false;
        }
        
        var user = users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            users.Remove(user);
            
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.ActiveUserCount--;
            }
            
            Console.WriteLine($"[{Name}] User {userId} left room {roomId}");
            return true;
        }
        
        return false;
    }
    
    private void SeedExampleData()
    {
        var generalRoomId = Guid.NewGuid().ToString();
        var generalRoom = new ChatRoom
        {
            Id = generalRoomId,
            Name = "General",
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsPrivate = false,
            MessageCount = 15,
            ActiveUserCount = 3
        };
        _rooms[generalRoomId] = generalRoom;
        _roomMessages[generalRoomId] = new List<string>();
        _roomUsers[generalRoomId] = new List<ChatUser>();
        
        var techRoomId = Guid.NewGuid().ToString();
        var techRoom = new ChatRoom
        {
            Id = techRoomId,
            Name = "Tech Talk",
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            IsPrivate = false,
            MessageCount = 8,
            ActiveUserCount = 2
        };
        _rooms[techRoomId] = techRoom;
        _roomMessages[techRoomId] = new List<string>();
        _roomUsers[techRoomId] = new List<ChatUser>();
        
        Console.WriteLine($"[{Name}] Seeded {_rooms.Count} example chat rooms");
    }
}
