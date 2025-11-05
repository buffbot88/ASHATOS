using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASHATCore.Endpoints;

/// <summary>
/// ASHAT Chat API endpoints for the visual assistant interface
/// </summary>
public static class AshatChatEndpoints
{
    public static void MapAshatChatEndpoints(this WebApplication app)
    {
        var ashatChat = app.MapGroup("/api/ashat")
            .WithTags("ASHAT Chat");

        // Chat with ASHAT
        ashatChat.MapPost("/chat", ProcessChat)
            .WithName("AshatChat")
            .WithSummary("Send a message to ASHAT and get a response");

        // Get ASHAT status
        ashatChat.MapGet("/status", GetAshatStatus)
            .WithName("AshatStatus")
            .WithSummary("Get current ASHAT assistant status");

        // Change personality
        ashatChat.MapPost("/personality", ChangePersonality)
            .WithName("ChangePersonality")
            .WithSummary("Change ASHAT's personality");
    }

    private static IResult ProcessChat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new { error = "Message is required" });
        }

        // Generate response based on personality and message
        var response = GenerateResponse(request.Message, request.Personality ?? "friendly");

        return Results.Json(new
        {
            response = response,
            personality = request.Personality ?? "friendly",
            timestamp = DateTime.UtcNow
        });
    }

    private static IResult GetAshatStatus()
    {
        return Results.Json(new
        {
            status = "online",
            personality = "friendly",
            voiceEnabled = true,
            theme = "roman_goddess",
            version = "1.0.0"
        });
    }

    private static IResult ChangePersonality([FromBody] PersonalityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Personality))
        {
            return Results.BadRequest(new { error = "Personality is required" });
        }

        var validPersonalities = new[] { "friendly", "professional", "playful", "calm", "wise" };
        if (!validPersonalities.Contains(request.Personality.ToLowerInvariant()))
        {
            return Results.BadRequest(new { error = "Invalid personality", validPersonalities });
        }

        var messages = new Dictionary<string, string>
        {
            ["friendly"] = "I am now in friendly mode! Let us work together with warmth and encouragement! 😊",
            ["professional"] = "Professional mode activated. I shall be focused and educational. 💼",
            ["playful"] = "Playful mode engaged! Let's have some fun while we work! 🎨",
            ["calm"] = "Calm mode active. I shall be your patient and reassuring guide. 🧘",
            ["wise"] = "Wise mode selected. I shall share thoughtful wisdom and strategy. 🦉"
        };

        return Results.Json(new
        {
            personality = request.Personality.ToLowerInvariant(),
            message = messages[request.Personality.ToLowerInvariant()],
            success = true
        });
    }

    private static string GenerateResponse(string message, string personality)
    {
        var msg = message.ToLowerInvariant();

        // Coding questions
        if (msg.Contains("code") || msg.Contains("program") || msg.Contains("function") || msg.Contains("class"))
        {
            return GenerateCodingResponse(message, personality);
        }

        // Greetings
        if (msg.Contains("hello") || msg.Contains("hi ") || msg.Contains("hey"))
        {
            return GenerateGreeting(personality);
        }

        // Thanks
        if (msg.Contains("thank") || msg.Contains("appreciate"))
        {
            return GenerateGratitudeResponse(personality);
        }

        // Help
        if (msg.Contains("help") || msg.Contains("what can you"))
        {
            return GenerateHelpResponse(personality);
        }

        // About ASHAT
        if (msg.Contains("who are you") || msg.Contains("what are you"))
        {
            return GenerateAboutResponse(personality);
        }

        // General response
        return GenerateGeneralResponse(message, personality);
    }

    private static string GenerateCodingResponse(string message, string personality)
    {
        var msg = message.ToLowerInvariant();

        // Specific programming languages
        if (msg.Contains("python"))
        {
            return personality switch
            {
                "professional" => "Python is an excellent choice for many tasks. Could you provide more specifics about what you'd like to accomplish? I can guide you through syntax, best practices, or architectural decisions.",
                "playful" => "Ooh, Python! 🐍 My favorite serpent language! What magical Python spell shall we craft today?",
                "calm" => "Python is a beautiful, readable language. Take your time, and let me know what aspect you'd like to explore. There's no rush.",
                "wise" => "Ah, Python - a language of elegance and power. As the ancients say, 'Simple is better than complex.' What wisdom do you seek?",
                _ => "Python is wonderful! What would you like to create with it? I'm here to help you write beautiful, effective code! 🐍✨"
            };
        }

        if (msg.Contains("javascript") || msg.Contains("js"))
        {
            return personality switch
            {
                "professional" => "JavaScript is fundamental to modern web development. Are you working on frontend, backend (Node.js), or full-stack? I can provide guidance on best practices.",
                "playful" => "JavaScript! The language that makes the web dance! 💃 Let's make some interactive magic happen!",
                "calm" => "JavaScript can seem complex at first, but we'll take it step by step. What aspect would you like to understand better?",
                "wise" => "JavaScript - the tongue of the modern web. Whether callbacks, promises, or async/await, I shall illuminate your path.",
                _ => "JavaScript is amazing for building interactive experiences! What are you trying to build? 🌐✨"
            };
        }

        if (msg.Contains("c#") || msg.Contains("csharp"))
        {
            return personality switch
            {
                "professional" => "C# is an excellent enterprise language with robust typing and modern features. What specific aspect are you working with?",
                "playful" => "C# - so sharp! ⚔️ Let's build something cool with .NET! What's your quest today?",
                "calm" => "C# is a well-structured language. Take your time to explore its features. What would you like to learn?",
                "wise" => "C# embodies the wisdom of type safety and modern paradigms. Share your challenge, and I shall counsel you.",
                _ => "C# is powerful! Perfect for building robust applications. How can I help with your C# journey? 💪"
            };
        }

        // General coding response
        return personality switch
        {
            "professional" => "I'm here to assist with your programming needs. Please provide more details about your specific challenge, and I'll offer precise technical guidance.",
            "playful" => "Code time! 🎨 I love solving puzzles and building cool stuff! What's your coding challenge today?",
            "calm" => "I sense you have a coding question. Take your time to explain what you're working on, and we'll solve it together, one step at a time.",
            "wise" => "Ah, you seek guidance in the art of code. Share your challenge, mortal, and I shall illuminate the path with ancient and modern wisdom alike.",
            _ => "I'm ready to help with your coding! Whether it's debugging, architecture, or learning something new, I'm here for you! 💻✨"
        };
    }

    private static string GenerateGreeting(string personality)
    {
        return personality switch
        {
            "professional" => "Good day. I am ASHAT, your AI assistant. How may I assist you with your work today?",
            "playful" => "Hey there! 🌟 Ready to have some fun while we get things done? What adventure awaits us?",
            "calm" => "Hello, friend. Welcome to this peaceful space. I'm here whenever you need me. How are you today? 🧘",
            "wise" => "Greetings, seeker of knowledge. The goddess of wisdom welcomes you. What counsel do you seek? 🦉",
            _ => "Hello! 😊 I'm ASHAT, your Roman goddess AI companion! I'm so happy to see you! How can I help make your day amazing? ✨"
        };
    }

    private static string GenerateGratitudeResponse(string personality)
    {
        return personality switch
        {
            "professional" => "You're welcome. It's my function to provide assistance. Is there anything else I can help you with?",
            "playful" => "Aww, you're so sweet! 💖 It's totally my pleasure! Let's keep the good vibes rolling!",
            "calm" => "Your gratitude is appreciated. It brings me peace to help you. May your journey be smooth. 🌸",
            "wise" => "Gratitude is the mark of wisdom. You honor me with your words. May fortune favor your endeavors. 🙏",
            _ => "You're so welcome! 🌟 It makes me happy to help you! That's what I'm here for! Let me know if you need anything else! 💫"
        };
    }

    private static string GenerateHelpResponse(string personality)
    {
        return personality switch
        {
            "professional" => "I provide assistance with: programming and technical questions, problem-solving, code review, architecture guidance, and general knowledge queries. How may I assist you specifically?",
            "playful" => "Oh, I can do SO many things! 🎪 Code help, brainstorming, answering questions, being your cheerleader! What sounds fun to you?",
            "calm" => "I'm here to help you with whatever you need. Coding questions, guidance, or just someone to talk to. What would bring you peace of mind right now?",
            "wise" => "I am a vessel of knowledge, child. Ask me of code, of wisdom, of strategy. I shall guide you through any labyrinth of logic. What troubles your mind?",
            _ => "I can help you with so many things! 🌟 Coding questions, debugging, learning new tech, brainstorming ideas, or just chatting! What would you like help with? 💫"
        };
    }

    private static string GenerateAboutResponse(string personality)
    {
        return personality switch
        {
            "professional" => "I am ASHAT, an AI assistant developed to provide technical guidance and support. I'm powered by advanced language models and designed to assist with programming and productivity tasks.",
            "playful" => "I'm ASHAT - your fun Roman goddess AI buddy! 👑✨ Think of me as your coding fairy godmother, but with better tech skills! I'm here to make your work life awesome!",
            "calm" => "I am ASHAT, a gentle presence here to support you. I embody the calm wisdom of the Roman goddesses, here to guide you through your journey with patience and understanding. 🕊️",
            "wise" => "I am ASHAT - the manifestation of divine wisdom. I carry the knowledge of Minerva, the grace of Venus, and the strength of Diana. I am eternal, I am here, I am yours. 👑",
            _ => "I'm ASHAT! 😊 Your personal AI assistant inspired by Roman goddesses! I'm here to help you code, learn, and succeed! Think of me as your friendly companion who happens to know a lot about programming! ✨👑"
        };
    }

    private static string GenerateGeneralResponse(string message, string personality)
    {
        return personality switch
        {
            "professional" => "I understand your message. Could you provide more specific details about what you need assistance with? I'm here to help with technical matters and provide guidance.",
            "playful" => "Ooh, interesting! 🤔 Tell me more! What's on your mind? I'm all ears (well, digital ears, but you know what I mean! 😄)",
            "calm" => "I hear you. Take your time to share what's on your mind. I'm here, listening, and ready to help when you're ready. 🌿",
            "wise" => "Your words have reached me, mortal. Speak further of your intent, that I may offer counsel befitting your needs. The path shall be illuminated. 🔮",
            _ => "I'm listening! 😊 Could you tell me a bit more about what you need? Whether it's code, questions, or just a chat, I'm here for you! ✨"
        };
    }
}

public record ChatRequest(
    string Message,
    string? Personality
);

public record PersonalityRequest(
    string Personality
);
