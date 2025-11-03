using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;

namespace LegendaryCMS.API;

/// <summary>
/// AI Chatbot Manager for CMS
/// Provides intelligent chat responses using AI language models
/// </summary>
public class AIChatbotManager
{
    private readonly ConcurrentDictionary<string, ChatbotConversation> _conversations = new();
    private readonly ConcurrentDictionary<string, List<ChatbotMessage>> _conversationHistory = new();
    private IAILanguageModule? _aiLanguageModule;
    private IChatModule? _chatModule;
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Chatbot configuration
    private readonly string _botName = "RaBot";
    private readonly int _maxHistoryLength = 50;
    private readonly string _systemPrompt = @"You are RaBot, a helpful AI assistant for the RaOS CMS platform. 
You help users with:
- Content management and publishing
- Forum and blog creation
- User authentication and permissions
- System configuration and setup
- General questions about the CMS features

Be concise, helpful, and friendly. If you don't know something, say so clearly.";

    public void Initialize(object? moduleManager)
    {
        if (moduleManager != null)
        {
            // Try to get AI Language Module
            var modulesProperty = moduleManager.GetType().GetProperty("Modules");
            if (modulesProperty != null)
            {
                var modules = modulesProperty.GetValue(moduleManager) as System.Collections.IEnumerable;
                if (modules != null)
                {
                    foreach (var moduleWrapper in modules)
                    {
                        var instanceProperty = moduleWrapper.GetType().GetProperty("Instance");
                        if (instanceProperty != null)
                        {
                            var instance = instanceProperty.GetValue(moduleWrapper);
                            
                            if (instance is IAILanguageModule aiLang)
                            {
                                _aiLanguageModule = aiLang;
                            }
                            else if (instance is IChatModule chat)
                            {
                                _chatModule = chat;
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine($"[AIChatbot] Initialized - AI Module: {(_aiLanguageModule != null ? "Available" : "Unavailable")}");
    }

    /// <summary>
    /// Start a new chatbot conversation
    /// </summary>
    public async Task<ChatbotConversation> StartConversationAsync(string userId, string username)
    {
        await Task.CompletedTask;

        var conversationId = Guid.NewGuid().ToString();
        var conversation = new ChatbotConversation
        {
            ConversationId = conversationId,
            UserId = userId,
            Username = username,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            MessageCount = 0,
            IsActive = true
        };

        _conversations[conversationId] = conversation;
        _conversationHistory[conversationId] = new List<ChatbotMessage>();

        Console.WriteLine($"[AIChatbot] Started conversation {conversationId} for user {username}");
        return conversation;
    }

    /// <summary>
    /// Send a message to the chatbot and get a response
    /// </summary>
    public async Task<ChatbotResponse> SendMessageAsync(string conversationId, string userId, string message)
    {
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            return new ChatbotResponse
            {
                Success = false,
                Error = "Conversation not found",
                Timestamp = DateTime.UtcNow
            };
        }

        if (conversation.UserId != userId)
        {
            return new ChatbotResponse
            {
                Success = false,
                Error = "Unauthorized: User ID mismatch",
                Timestamp = DateTime.UtcNow
            };
        }

        // Store user message
        var userMessage = new ChatbotMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Content = message,
            IsFromBot = false,
            Timestamp = DateTime.UtcNow
        };

        if (_conversationHistory.TryGetValue(conversationId, out var history))
        {
            history.Add(userMessage);
            
            // Trim history if too long
            if (history.Count > _maxHistoryLength)
            {
                history.RemoveRange(0, history.Count - _maxHistoryLength);
            }
        }

        // Generate AI response
        string botReply;
        if (_aiLanguageModule != null)
        {
            try
            {
                // Build context from conversation history
                var context = BuildConversationContext(conversationId);
                
                var aiResponse = await _aiLanguageModule.GenerateAsync(
                    intent: "chat_response",
                    context: context,
                    language: "en",
                    metadata: new Dictionary<string, object>
                    {
                        { "system_prompt", _systemPrompt },
                        { "user_message", message },
                        { "conversation_id", conversationId }
                    }
                );

                botReply = !string.IsNullOrWhiteSpace(aiResponse.Text) && string.IsNullOrEmpty(aiResponse.Error)
                    ? aiResponse.Text
                    : GetFallbackResponse(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AIChatbot] AI generation error: {ex.Message}");
                botReply = GetFallbackResponse(message);
            }
        }
        else
        {
            // Use fallback responses when AI module is unavailable
            botReply = GetFallbackResponse(message);
        }

        // Store bot message
        var botMessage = new ChatbotMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Content = botReply,
            IsFromBot = true,
            Timestamp = DateTime.UtcNow
        };

        if (_conversationHistory.TryGetValue(conversationId, out var hist))
        {
            hist.Add(botMessage);
        }

        // Update conversation
        conversation.MessageCount += 2;
        conversation.LastActivityAt = DateTime.UtcNow;

        Console.WriteLine($"[AIChatbot] Processed message in conversation {conversationId}");

        return new ChatbotResponse
        {
            Success = true,
            ConversationId = conversationId,
            BotReply = botReply,
            MessageId = botMessage.MessageId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get conversation history
    /// </summary>
    public async Task<List<ChatbotMessage>> GetConversationHistoryAsync(string conversationId, string userId, int limit = 50)
    {
        await Task.CompletedTask;

        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            return new List<ChatbotMessage>();
        }

        if (conversation.UserId != userId)
        {
            return new List<ChatbotMessage>();
        }

        if (!_conversationHistory.TryGetValue(conversationId, out var history))
        {
            return new List<ChatbotMessage>();
        }

        return history.TakeLast(limit).ToList();
    }

    /// <summary>
    /// Get all active conversations for a user
    /// </summary>
    public async Task<List<ChatbotConversation>> GetUserConversationsAsync(string userId)
    {
        await Task.CompletedTask;

        return _conversations.Values
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.LastActivityAt)
            .ToList();
    }

    /// <summary>
    /// End a conversation
    /// </summary>
    public async Task<bool> EndConversationAsync(string conversationId, string userId)
    {
        await Task.CompletedTask;

        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            return false;
        }

        if (conversation.UserId != userId)
        {
            return false;
        }

        conversation.IsActive = false;
        conversation.EndedAt = DateTime.UtcNow;

        Console.WriteLine($"[AIChatbot] Ended conversation {conversationId}");
        return true;
    }

    /// <summary>
    /// Get chatbot statistics
    /// </summary>
    public async Task<ChatbotStats> GetStatsAsync()
    {
        await Task.CompletedTask;

        var activeConversations = _conversations.Values.Count(c => c.IsActive);
        var totalConversations = _conversations.Count;
        var totalMessages = _conversationHistory.Values.Sum(h => h.Count);

        return new ChatbotStats
        {
            ActiveConversations = activeConversations,
            TotalConversations = totalConversations,
            TotalMessages = totalMessages,
            AIModuleAvailable = _aiLanguageModule != null,
            BotName = _botName
        };
    }

    private string BuildConversationContext(string conversationId)
    {
        if (!_conversationHistory.TryGetValue(conversationId, out var history))
        {
            return _systemPrompt;
        }

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine(_systemPrompt);
        contextBuilder.AppendLine();
        contextBuilder.AppendLine("Recent conversation:");

        // Include last 10 messages for context
        var recentMessages = history.TakeLast(10);
        foreach (var msg in recentMessages)
        {
            var speaker = msg.IsFromBot ? _botName : "User";
            contextBuilder.AppendLine($"{speaker}: {msg.Content}");
        }

        return contextBuilder.ToString();
    }

    private string GetFallbackResponse(string message)
    {
        var msgLower = message.ToLowerInvariant();

        // Simple pattern matching for common queries
        if (msgLower.Contains("hello") || msgLower.Contains("hi"))
        {
            return $"Hello! I'm {_botName}, your CMS assistant. How can I help you today?";
        }

        if (msgLower.Contains("help"))
        {
            return "I can help you with:\n- Content management\n- Forums and blogs\n- User permissions\n- System configuration\n\nWhat would you like to know more about?";
        }

        if (msgLower.Contains("forum"))
        {
            return "Forums allow users to create discussions and engage with your community. You can create forums using the API endpoint /api/forums/post with proper authentication.";
        }

        if (msgLower.Contains("blog"))
        {
            return "Blogs are a great way to share content. Use the /api/blogs/create endpoint to publish blog posts. You'll need appropriate permissions to create blogs.";
        }

        if (msgLower.Contains("permission") || msgLower.Contains("access"))
        {
            return "The CMS uses Role-Based Access Control (RBAC) with multiple permission levels. Contact your administrator to adjust your permissions.";
        }

        if (msgLower.Contains("api"))
        {
            return "The CMS provides a comprehensive REST API with endpoints for forums, blogs, content, and more. Check /api/endpoints for a full list of available APIs.";
        }

        // Default response
        return $"I'm {_botName}, an AI assistant for the RaOS CMS. I can help with content management, forums, blogs, and system configuration. What would you like to know?";
    }
}

/// <summary>
/// Represents a chatbot conversation
/// </summary>
public class ChatbotConversation
{
    public string ConversationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int MessageCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a chatbot message
/// </summary>
public class ChatbotMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsFromBot { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Chatbot response structure
/// </summary>
public class ChatbotResponse
{
    public bool Success { get; set; }
    public string? ConversationId { get; set; }
    public string? BotReply { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Chatbot statistics
/// </summary>
public class ChatbotStats
{
    public int ActiveConversations { get; set; }
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }
    public bool AIModuleAvailable { get; set; }
    public string BotName { get; set; } = string.Empty;
}
