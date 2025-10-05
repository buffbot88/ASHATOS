using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Core.Collaboration;

/// <summary>
/// Module coordinator for inter-module collaboration and messaging
/// </summary>
[RaModule(Category = "core")]
public sealed class ModuleCoordinatorModule : ModuleBase
{
    public override string Name => "ModuleCoordinator";

    private ModuleManager? _manager;
    private readonly ConcurrentQueue<ModuleMessage> _messageQueue = new();
    private readonly ConcurrentDictionary<string, List<ModuleMessage>> _messageHistory = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "ModuleCoordinator: Use 'messages', 'send <module> <message>', or 'broadcast <message>'";

        var parts = input.Trim().Split(' ', 2);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "messages" => GetMessageHistory(),
            "send" when parts.Length > 1 => SendMessage(parts[1]),
            "broadcast" when parts.Length > 1 => BroadcastMessage(parts[1]),
            "stats" => GetStats(),
            _ => "Unknown command. Use: messages, send <module> <message>, broadcast <message>, stats"
        };
    }

    public async Task<ModuleResponse> SendMessageAsync(ModuleMessage message)
    {
        _messageQueue.Enqueue(message);
        
        // Store in history
        var historyKey = $"{message.FromModule}->{message.ToModule ?? "broadcast"}";
        _messageHistory.AddOrUpdate(historyKey, 
            new List<ModuleMessage> { message },
            (_, list) => { list.Add(message); return list; });

        if (_manager != null && !string.IsNullOrEmpty(message.ToModule))
        {
            var targetModule = _manager.GetModuleInstanceByName(message.ToModule);
            if (targetModule is ICollaborativeModule collab)
            {
                var response = await collab.ReceiveMessageAsync(message);
                if (response != null)
                {
                    return response;
                }
            }
        }

        return new ModuleResponse
        {
            Text = $"Message sent from {message.FromModule} to {message.ToModule ?? "all modules"}",
            Type = "message.sent",
            Status = "ok"
        };
    }

    public async Task<List<ModuleResponse>> BroadcastMessageAsync(ModuleMessage message)
    {
        var responses = new List<ModuleResponse>();
        
        if (_manager != null)
        {
            foreach (var wrapper in _manager.Modules)
            {
                if (wrapper.Instance is ICollaborativeModule collab && 
                    wrapper.Instance.Name != message.FromModule)
                {
                    var response = await collab.ReceiveMessageAsync(message);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }
        }

        return responses;
    }

    private string SendMessage(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
            return "Usage: send <module> <message>";

        var message = new ModuleMessage
        {
            FromModule = "User",
            ToModule = parts[0],
            Content = parts[1],
            Priority = MessagePriority.Normal
        };

        var task = SendMessageAsync(message);
        task.Wait();
        return task.Result.Text;
    }

    private string BroadcastMessage(string content)
    {
        var message = new ModuleMessage
        {
            FromModule = "User",
            Content = content,
            Priority = MessagePriority.Normal
        };

        var task = BroadcastMessageAsync(message);
        task.Wait();
        return $"Broadcast sent. {task.Result.Count} responses received.";
    }

    private string GetMessageHistory()
    {
        if (_messageHistory.IsEmpty)
            return "No message history.";

        var result = new System.Text.StringBuilder("Message History:\n");
        foreach (var kvp in _messageHistory)
        {
            result.AppendLine($"\n{kvp.Key}:");
            foreach (var msg in kvp.Value.TakeLast(5))
            {
                result.AppendLine($"  [{msg.Timestamp:HH:mm:ss}] {msg.Content} (Priority: {msg.Priority})");
            }
        }
        return result.ToString();
    }

    private string GetStats()
    {
        return $"ModuleCoordinator Stats:\n" +
               $"  Messages in queue: {_messageQueue.Count}\n" +
               $"  Conversation threads: {_messageHistory.Count}\n" +
               $"  Total messages: {_messageHistory.Values.Sum(v => v.Count)}";
    }
}
