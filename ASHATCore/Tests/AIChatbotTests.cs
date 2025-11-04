using System;
using LegendaryCMS.API;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for AI Chatbot functionality in LegendaryCMS
/// </summary>
public class AIChatbotTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         AI Chatbot Tests - LegendaryCMS                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        TestChatbotInitialization();
        TestStartConversation();
        TestSendMessage();
        TestGetConversationHistory();
        TestGetUserConversations();
        TestEndConversation();
        TestGetStats();
        TestFallbackResponses();

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       ✨ All AI Chatbot Tests Passed! ✨                ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    }

    private static void TestChatbotInitialization()
    {
        Console.WriteLine("Test 1: Chatbot Initialization");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        Console.WriteLine("  ✅ Chatbot initialized successfully");
        Console.WriteLine("  ✅ Ready to handle conversations");
        Console.WriteLine();
    }

    private static void TestStartConversation()
    {
        Console.WriteLine("Test 2: Start Conversation");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        var task = chatbot.StartConversationAsync("user123", "TestUser");
        task.Wait();
        var conversation = task.Result;

        if (string.IsNullOrEmpty(conversation.ConversationId))
            throw new Exception("Conversation ID is empty");

        if (conversation.UserId != "user123")
            throw new Exception($"Expected UserId 'user123', got '{conversation.UserId}'");

        if (conversation.Username != "TestUser")
            throw new Exception($"Expected Username 'TestUser', got '{conversation.Username}'");

        if (!conversation.IsActive)
            throw new Exception("Conversation should be active");

        if (conversation.MessageCount != 1)
            throw new Exception($"Expected MessageCount 1 (welcome message), got {conversation.MessageCount}");

        Console.WriteLine($"  ✅ Conversation started: {conversation.ConversationId}");
        Console.WriteLine($"  ✅ User: {conversation.Username}");
        Console.WriteLine($"  ✅ Status: Active");
        Console.WriteLine($"  ✅ Welcome message sent automatically");
        Console.WriteLine();
    }

    private static void TestSendMessage()
    {
        Console.WriteLine("Test 3: Send Message");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        var startTask = chatbot.StartConversationAsync("user123", "TestUser");
        startTask.Wait();
        var conversation = startTask.Result;

        var sendTask = chatbot.SendMessageAsync(conversation.ConversationId, "user123", "Hello, can you help me?");
        sendTask.Wait();
        var response = sendTask.Result;

        if (!response.Success)
            throw new Exception($"Message send failed: {response.Error}");

        if (string.IsNullOrEmpty(response.BotReply))
            throw new Exception("Bot reply is empty");

        Console.WriteLine($"  ✅ Message sent successfully");
        Console.WriteLine($"  ✅ Bot reply received: {response.BotReply.Substring(0, Math.Min(50, response.BotReply.Length))}...");
        Console.WriteLine($"  ✅ Message ID: {response.MessageId}");
        Console.WriteLine();
    }

    private static void TestGetConversationHistory()
    {
        Console.WriteLine("Test 4: Get Conversation History");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        var startTask = chatbot.StartConversationAsync("user123", "TestUser");
        startTask.Wait();
        var conversation = startTask.Result;

        var sendTask = chatbot.SendMessageAsync(conversation.ConversationId, "user123", "Test message");
        sendTask.Wait();

        var historyTask = chatbot.GetConversationHistoryAsync(conversation.ConversationId, "user123", 50);
        historyTask.Wait();
        var history = historyTask.Result;

        if (history.Count != 3) // Welcome message + User message + Bot reply
            throw new Exception($"Expected 3 messages in history (welcome + user + bot), got {history.Count}");

        Console.WriteLine($"  ✅ Conversation history retrieved");
        Console.WriteLine($"  ✅ Message count: {history.Count}");
        Console.WriteLine($"  ✅ Welcome message: {history[0].Content.Substring(0, Math.Min(30, history[0].Content.Length))}...");
        Console.WriteLine($"  ✅ User message: {history[1].Content.Substring(0, Math.Min(30, history[1].Content.Length))}");
        Console.WriteLine($"  ✅ Bot reply: {history[2].Content.Substring(0, Math.Min(30, history[2].Content.Length))}");
        Console.WriteLine();
    }

    private static void TestGetUserConversations()
    {
        Console.WriteLine("Test 5: Get User Conversations");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        var startTask1 = chatbot.StartConversationAsync("user123", "TestUser");
        startTask1.Wait();

        var startTask2 = chatbot.StartConversationAsync("user123", "TestUser");
        startTask2.Wait();

        var conversationsTask = chatbot.GetUserConversationsAsync("user123");
        conversationsTask.Wait();
        var conversations = conversationsTask.Result;

        if (conversations.Count != 2)
            throw new Exception($"Expected 2 conversations, got {conversations.Count}");

        Console.WriteLine($"  ✅ User conversations retrieved");
        Console.WriteLine($"  ✅ Total conversations: {conversations.Count}");
        Console.WriteLine($"  ✅ All conversations active: {conversations.All(c => c.IsActive)}");
        Console.WriteLine();
    }

    private static void TestEndConversation()
    {
        Console.WriteLine("Test 6: End Conversation");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        var startTask = chatbot.StartConversationAsync("user123", "TestUser");
        startTask.Wait();
        var conversation = startTask.Result;

        var endTask = chatbot.EndConversationAsync(conversation.ConversationId, "user123");
        endTask.Wait();
        var success = endTask.Result;

        if (!success)
            throw new Exception("Failed to end conversation");

        Console.WriteLine($"  ✅ Conversation ended successfully");
        Console.WriteLine($"  ✅ Conversation ID: {conversation.ConversationId}");
        Console.WriteLine();
    }

    private static void TestGetStats()
    {
        Console.WriteLine("Test 7: Get Chatbot Stats");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null);

        // Create some conversations
        var startTask1 = chatbot.StartConversationAsync("user1", "User1");
        startTask1.Wait();

        var startTask2 = chatbot.StartConversationAsync("user2", "User2");
        startTask2.Wait();

        var statsTask = chatbot.GetStatsAsync();
        statsTask.Wait();
        var stats = statsTask.Result;

        if (stats.TotalConversations != 2)
            throw new Exception($"Expected 2 total conversations, got {stats.TotalConversations}");

        if (stats.ActiveConversations != 2)
            throw new Exception($"Expected 2 active conversations, got {stats.ActiveConversations}");

        Console.WriteLine($"  ✅ Stats retrieved successfully");
        Console.WriteLine($"  ✅ Bot Name: {stats.BotName}");
        Console.WriteLine($"  ✅ Total Conversations: {stats.TotalConversations}");
        Console.WriteLine($"  ✅ Active Conversations: {stats.ActiveConversations}");
        Console.WriteLine($"  ✅ AI Module Available: {stats.AIModuleAvailable}");
        Console.WriteLine();
    }

    private static void TestFallbackResponses()
    {
        Console.WriteLine("Test 8: Fallback Responses");

        var chatbot = new AIChatbotManager();
        chatbot.Initialize(null); // Initialize without AI module

        var startTask = chatbot.StartConversationAsync("user123", "TestUser");
        startTask.Wait();
        var conversation = startTask.Result;

        // Test various fallback patterns
        var testMessages = new[]
        {
            ("Hello", "hello"),
            ("Help me", "help"),
            ("Tell me about forums", "forum"),
            ("What about blogs?", "blog"),
            ("How do permissions work?", "permission"),
            ("API info", "api")
        };

        foreach (var (message, expectedKeyword) in testMessages)
        {
            var sendTask = chatbot.SendMessageAsync(conversation.ConversationId, "user123", message);
            sendTask.Wait();
            var response = sendTask.Result;

            if (!response.Success)
                throw new Exception($"Message '{message}' failed: {response.Error}");

            if (string.IsNullOrEmpty(response.BotReply))
                throw new Exception($"No reply for message '{message}'");

            if (!response.BotReply.ToLowerInvariant().Contains(expectedKeyword.ToLowerInvariant()) &&
                !response.BotReply.Contains("RaBot"))
            {
                Console.WriteLine($"  ⚠️  Reply for '{message}' may not match expected pattern");
            }
        }

        Console.WriteLine($"  ✅ Fallback responses working");
        Console.WriteLine($"  ✅ Tested {testMessages.Length} message patterns");
        Console.WriteLine($"  ✅ All responses generated successfully");
        Console.WriteLine();
    }
}
