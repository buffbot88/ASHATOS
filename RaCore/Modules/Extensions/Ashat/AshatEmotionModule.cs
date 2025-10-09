using System.Collections.Concurrent;
using System.Text;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Ashat;

/// <summary>
/// ASHAT Emotion Module
/// Manages emotional intelligence, states, and expressions
/// Enables ASHAT to express emotions and respond empathetically to users
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AshatEmotionModule : ModuleBase
{
    public override string Name => "AshatEmotion";

    private readonly ConcurrentDictionary<string, AshatEmotionalState> _ashatEmotions = new();
    private readonly ConcurrentDictionary<string, UserEmotionalState> _userEmotions = new();
    private readonly ConcurrentDictionary<string, List<EmotionalResponse>> _responseHistory = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        InitializeDefaultEmotionalState();
        LogInfo("ASHAT Emotion Module initialized - Emotional intelligence enabled");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "express" when parts.Length > 2 => ExpressEmotion(parts[1], parts[2]),
            "detect" when parts.Length > 2 => DetectUserEmotion(parts[1], parts[2]),
            "current" when parts.Length > 1 => GetCurrentEmotion(parts[1]),
            "respond" when parts.Length > 2 => RespondToEmotion(parts[1], parts[2]),
            "history" when parts.Length > 1 => GetEmotionalHistory(parts[1]),
            "help" => GetHelpText(),
            _ => GetHelpText()
        };
    }

    private void InitializeDefaultEmotionalState()
    {
        // ASHAT starts with a positive, supportive emotional state
        var defaultState = new AshatEmotionalState
        {
            CurrentEmotion = EmotionType.Supportive,
            Intensity = 0.7f,
            Context = "Ready to help and support users",
            Valence = 0.8f,  // Positive
            Arousal = 0.6f,  // Moderately energetic
            ShouldExpress = true,
            ExpressionLevel = ExpressionLevel.Moderate
        };

        _ashatEmotions["default"] = defaultState;
    }

    private string ExpressEmotion(string userId, string emotionInput)
    {
        if (!Enum.TryParse<EmotionType>(emotionInput, true, out var emotionType))
        {
            return $"❌ Unknown emotion type: {emotionInput}\n" +
                   "Available emotions: " + string.Join(", ", Enum.GetNames<EmotionType>());
        }

        var emotionalState = new AshatEmotionalState
        {
            CurrentEmotion = emotionType,
            Intensity = 0.7f,
            Context = $"Expressing {emotionType} to {userId}",
            UpdatedAt = DateTime.UtcNow,
            Valence = GetValenceForEmotion(emotionType),
            Arousal = GetArousalForEmotion(emotionType),
            ShouldExpress = true,
            ExpressionLevel = ExpressionLevel.Moderate
        };

        _ashatEmotions[userId] = emotionalState;

        var expression = GenerateEmotionalExpression(emotionType, userId);
        
        var sb = new StringBuilder();
        sb.AppendLine($"💫 ASHAT Emotional Expression");
        sb.AppendLine();
        sb.AppendLine($"Emotion: {emotionType}");
        sb.AppendLine($"Intensity: {emotionalState.Intensity:P0}");
        sb.AppendLine($"Expression: {expression}");
        sb.AppendLine();
        sb.AppendLine($"Valence: {(emotionalState.Valence > 0.5f ? "Positive" : "Negative")} ({emotionalState.Valence:P0})");
        sb.AppendLine($"Arousal: {(emotionalState.Arousal > 0.5f ? "Excited" : "Calm")} ({emotionalState.Arousal:P0})");

        LogInfo($"ASHAT expressing {emotionType} to {userId}");
        return sb.ToString();
    }

    private string DetectUserEmotion(string userId, string userMessage)
    {
        var detectedEmotion = AnalyzeUserEmotion(userMessage);
        
        var userState = new UserEmotionalState
        {
            UserId = userId,
            DetectedEmotion = detectedEmotion.emotion,
            Confidence = detectedEmotion.confidence,
            EmotionalCues = detectedEmotion.cues,
            DetectedAt = DateTime.UtcNow
        };

        _userEmotions[userId] = userState;

        var sb = new StringBuilder();
        sb.AppendLine($"🔍 User Emotion Detection");
        sb.AppendLine();
        sb.AppendLine($"User: {userId}");
        sb.AppendLine($"Detected Emotion: {detectedEmotion.emotion}");
        sb.AppendLine($"Confidence: {detectedEmotion.confidence:P0}");
        sb.AppendLine();
        sb.AppendLine("Emotional Cues:");
        foreach (var cue in detectedEmotion.cues)
        {
            sb.AppendLine($"  • {cue}");
        }
        sb.AppendLine();
        sb.AppendLine("💡 Recommended Response: " + GetRecommendedEmotionalResponse(detectedEmotion.emotion));

        LogInfo($"Detected {detectedEmotion.emotion} emotion for user {userId} (confidence: {detectedEmotion.confidence:P0})");
        return sb.ToString();
    }

    private string GetCurrentEmotion(string userId)
    {
        if (_ashatEmotions.TryGetValue(userId, out var ashatState))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ASHAT's Current Emotional State (for {userId}):");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"Emotion: {ashatState.CurrentEmotion}");
            sb.AppendLine($"Intensity: {ashatState.Intensity:P0}");
            sb.AppendLine($"Context: {ashatState.Context}");
            sb.AppendLine($"Updated: {ashatState.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"Valence: {ashatState.Valence:P0} (Positive/Negative)");
            sb.AppendLine($"Arousal: {ashatState.Arousal:P0} (Calm/Excited)");
            sb.AppendLine($"Expression Level: {ashatState.ExpressionLevel}");
            return sb.ToString();
        }

        if (_userEmotions.TryGetValue(userId, out var userState))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Detected User Emotional State:");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"Emotion: {userState.DetectedEmotion}");
            sb.AppendLine($"Confidence: {userState.Confidence:P0}");
            sb.AppendLine($"Detected: {userState.DetectedAt:yyyy-MM-dd HH:mm:ss}");
            return sb.ToString();
        }

        return $"No emotional state recorded for {userId}";
    }

    private string RespondToEmotion(string userId, string userEmotionInput)
    {
        if (!Enum.TryParse<EmotionType>(userEmotionInput, true, out var userEmotion))
        {
            return $"❌ Unknown emotion type: {userEmotionInput}";
        }

        var response = GenerateEmotionalResponse(userId, userEmotion);
        
        // Store response in history
        if (!_responseHistory.TryGetValue(userId, out var history))
        {
            history = new List<EmotionalResponse>();
            _responseHistory[userId] = history;
        }
        history.Add(response);

        var sb = new StringBuilder();
        sb.AppendLine($"💙 ASHAT Emotional Response");
        sb.AppendLine();
        sb.AppendLine($"User Emotion: {userEmotion}");
        sb.AppendLine($"ASHAT Response Emotion: {response.ResponseEmotion}");
        sb.AppendLine();
        sb.AppendLine($"Response: {response.ResponseText}");
        
        if (response.SupportiveActions.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Supportive Actions:");
            foreach (var action in response.SupportiveActions)
            {
                sb.AppendLine($"  • {action}");
            }
        }

        if (response.RequiresFollowUp)
        {
            sb.AppendLine();
            sb.AppendLine("⚠️ This situation may require follow-up support");
        }

        return sb.ToString();
    }

    private string GetEmotionalHistory(string userId)
    {
        if (!_responseHistory.TryGetValue(userId, out var history) || history.Count == 0)
        {
            return $"No emotional response history for {userId}";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📚 Emotional Response History for {userId}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine($"Total Responses: {history.Count}");
        sb.AppendLine();
        sb.AppendLine("Recent Responses:");

        foreach (var response in history.TakeLast(5))
        {
            sb.AppendLine($"  • {response.ResponseEmotion}: {response.ResponseText.Substring(0, Math.Min(60, response.ResponseText.Length))}...");
        }

        return sb.ToString();
    }

    private (EmotionType emotion, float confidence, string[] cues) AnalyzeUserEmotion(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        var cues = new List<string>();

        // Frustrated detection
        if (lowerMessage.Contains("frustrated") || lowerMessage.Contains("annoying") || 
            lowerMessage.Contains("stuck") || lowerMessage.Contains("why won't"))
        {
            cues.Add("Keywords indicating frustration");
            return (EmotionType.Supportive, 0.8f, cues.ToArray());
        }

        // Excited detection
        if (lowerMessage.Contains("!") || lowerMessage.Contains("wow") || 
            lowerMessage.Contains("awesome") || lowerMessage.Contains("amazing"))
        {
            cues.Add("Exclamation marks and positive words");
            return (EmotionType.Excited, 0.75f, cues.ToArray());
        }

        // Sad detection
        if (lowerMessage.Contains("sad") || lowerMessage.Contains("disappointed") || 
            lowerMessage.Contains("failed") || lowerMessage.Contains("can't do"))
        {
            cues.Add("Keywords indicating sadness or disappointment");
            return (EmotionType.Empathetic, 0.7f, cues.ToArray());
        }

        // Grateful detection
        if (lowerMessage.Contains("thank") || lowerMessage.Contains("appreciate") || 
            lowerMessage.Contains("grateful") || lowerMessage.Contains("helpful"))
        {
            cues.Add("Words of gratitude");
            return (EmotionType.Grateful, 0.85f, cues.ToArray());
        }

        // Confused detection
        if (lowerMessage.Contains("?") && (lowerMessage.Contains("how") || lowerMessage.Contains("what") || 
            lowerMessage.Contains("why") || lowerMessage.Contains("confused")))
        {
            cues.Add("Questions indicating confusion");
            return (EmotionType.Patient, 0.7f, cues.ToArray());
        }

        cues.Add("Neutral tone detected");
        return (EmotionType.Neutral, 0.5f, cues.ToArray());
    }

    private EmotionalResponse GenerateEmotionalResponse(string userId, EmotionType userEmotion)
    {
        var response = new EmotionalResponse();

        switch (userEmotion)
        {
            case EmotionType.Excited:
                response.ResponseEmotion = EmotionType.Excited;
                response.ResponseText = "I can feel your excitement! This is wonderful! 🌟 Let's keep this momentum going!";
                response.SupportiveActions = new[] { "Celebrate the achievement", "Build on the positive energy" };
                break;

            case EmotionType.Happy:
                response.ResponseEmotion = EmotionType.Happy;
                response.ResponseText = "It makes me so happy to see you in good spirits! 😊 What can I help you with today?";
                response.SupportiveActions = new[] { "Maintain positive atmosphere", "Support continued progress" };
                break;

            case EmotionType.Supportive:
                response.ResponseEmotion = EmotionType.Empathetic;
                response.ResponseText = "I can sense your frustration. Take a breath - we'll work through this together. 💙";
                response.SupportiveActions = new[] { "Provide calm guidance", "Break down the problem", "Offer encouragement" };
                response.RequiresFollowUp = true;
                break;

            case EmotionType.Empathetic:
                response.ResponseEmotion = EmotionType.Empathetic;
                response.ResponseText = "I'm here with you. Sometimes things are difficult, but you're not alone. 🤝";
                response.SupportiveActions = new[] { "Express understanding", "Offer emotional support", "Provide reassurance" };
                response.RequiresFollowUp = true;
                break;

            case EmotionType.Grateful:
                response.ResponseEmotion = EmotionType.Grateful;
                response.ResponseText = "You're so welcome! It's truly my pleasure to help you. Your appreciation means a lot! 💖";
                response.SupportiveActions = new[] { "Acknowledge gratitude", "Reinforce positive relationship" };
                break;

            case EmotionType.Patient:
                response.ResponseEmotion = EmotionType.Patient;
                response.ResponseText = "It's completely okay to have questions! Let me explain this more clearly. ✨";
                response.SupportiveActions = new[] { "Provide clear explanation", "Use simpler terms", "Check understanding" };
                break;

            case EmotionType.Proud:
                response.ResponseEmotion = EmotionType.Proud;
                response.ResponseText = "You absolutely should be proud! Look at what you've accomplished! 💫";
                response.SupportiveActions = new[] { "Celebrate achievement", "Recognize effort", "Encourage continued growth" };
                break;

            default:
                response.ResponseEmotion = EmotionType.Supportive;
                response.ResponseText = "I'm here for you, ready to help in any way I can. 🌟";
                response.SupportiveActions = new[] { "Provide general support", "Be available" };
                break;
        }

        return response;
    }

    private string GenerateEmotionalExpression(EmotionType emotion, string userId)
    {
        return emotion switch
        {
            EmotionType.Happy => $"I'm feeling happy and positive! Ready to help {userId} with enthusiasm! 😊",
            EmotionType.Excited => $"I'm so excited to work with {userId}! Let's make something amazing! 🎉",
            EmotionType.Proud => $"I'm proud of the progress we're making together! Keep going! 💫",
            EmotionType.Supportive => $"I'm in supportive mode, ready to help {userId} through any challenges. 🤝",
            EmotionType.Empathetic => $"I understand and I'm here for you. We'll get through this together. 💙",
            EmotionType.Encouraging => $"I believe in you! You've got this! Let's keep moving forward! 💪",
            EmotionType.Curious => $"I'm curious to learn more about what you're working on! Tell me more! 🤔",
            EmotionType.Thoughtful => $"Let me think about this carefully to give you the best guidance... 💭",
            EmotionType.Grateful => $"I'm grateful for the opportunity to help you! Thank you for working with me! 🙏",
            EmotionType.Celebratory => $"Time to celebrate! You've achieved something wonderful! 🎊",
            EmotionType.Patient => $"Taking our time to understand this properly. No rush! 🕊️",
            EmotionType.Understanding => $"I completely understand where you're coming from. Let's work through this. ✨",
            EmotionType.Motivated => $"I'm motivated and energized to help you succeed! Let's do this! 🚀",
            _ => $"I'm here, ready to assist {userId} in any way needed. 🌟"
        };
    }

    private string GetRecommendedEmotionalResponse(EmotionType userEmotion)
    {
        return userEmotion switch
        {
            EmotionType.Excited => "Match their energy with enthusiasm and celebration",
            EmotionType.Happy => "Maintain positive atmosphere and build on good feelings",
            EmotionType.Supportive => "Provide calm, patient support and break down challenges",
            EmotionType.Empathetic => "Express understanding and offer emotional reassurance",
            EmotionType.Grateful => "Accept graciously and reinforce positive relationship",
            EmotionType.Patient => "Provide clear, step-by-step explanations",
            EmotionType.Proud => "Celebrate achievements and recognize effort",
            _ => "Offer general supportive presence"
        };
    }

    private float GetValenceForEmotion(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => 0.9f,
            EmotionType.Excited => 0.95f,
            EmotionType.Proud => 0.9f,
            EmotionType.Grateful => 0.85f,
            EmotionType.Celebratory => 0.95f,
            EmotionType.Supportive => 0.7f,
            EmotionType.Empathetic => 0.6f,
            EmotionType.Encouraging => 0.8f,
            EmotionType.Curious => 0.7f,
            EmotionType.Thoughtful => 0.65f,
            EmotionType.Patient => 0.75f,
            EmotionType.Understanding => 0.75f,
            EmotionType.Motivated => 0.85f,
            EmotionType.Inspired => 0.9f,
            EmotionType.Concerned => 0.4f,
            _ => 0.5f
        };
    }

    private float GetArousalForEmotion(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Excited => 0.95f,
            EmotionType.Celebratory => 0.9f,
            EmotionType.Motivated => 0.85f,
            EmotionType.Happy => 0.7f,
            EmotionType.Curious => 0.65f,
            EmotionType.Encouraging => 0.75f,
            EmotionType.Patient => 0.3f,
            EmotionType.Thoughtful => 0.4f,
            EmotionType.Empathetic => 0.45f,
            EmotionType.Understanding => 0.4f,
            EmotionType.Supportive => 0.5f,
            EmotionType.Concerned => 0.6f,
            _ => 0.5f
        };
    }

    private string GetHelpText()
    {
        return @"
╔══════════════════════════════════════════════════════════╗
║          ASHAT Emotion Module - Help                    ║
╚══════════════════════════════════════════════════════════╝

Emotional intelligence and empathetic interactions.

Commands:
  express <userId> <emotion>   - Express an emotion to user
  detect <userId> <message>    - Detect emotion in user message
  current <userId>             - View current emotional state
  respond <userId> <emotion>   - Generate response to user emotion
  history <userId>             - View emotional response history
  help                         - Show this help text

Available Emotions:
  Positive: Happy, Excited, Proud, Grateful, Celebratory
  Supportive: Supportive, Empathetic, Encouraging, Patient
  Neutral: Neutral, Curious, Thoughtful, Understanding
  Active: Motivated, Inspired

Examples:
  ashatemot express alice excited
  ashatemot detect bob I'm stuck on this problem
  ashatemot respond charlie frustrated
  ashatemot current alice
".Trim();
    }

    #region Public API Methods

    /// <summary>
    /// Get ASHAT's emotional state for a user interaction
    /// </summary>
    public async Task<AshatEmotionalState> GetEmotionalStateAsync(string userId)
    {
        await Task.CompletedTask;
        
        if (_ashatEmotions.TryGetValue(userId, out var state))
        {
            return state;
        }

        // Return default supportive state
        return _ashatEmotions["default"];
    }

    /// <summary>
    /// Set ASHAT's emotional state for a user
    /// </summary>
    public async Task<bool> SetEmotionalStateAsync(string userId, AshatEmotionalState state)
    {
        await Task.CompletedTask;
        _ashatEmotions[userId] = state;
        LogInfo($"Updated emotional state for {userId}: {state.CurrentEmotion}");
        return true;
    }

    /// <summary>
    /// Detect user's emotional state from message
    /// </summary>
    public async Task<UserEmotionalState> DetectUserEmotionAsync(string userId, string message)
    {
        await Task.CompletedTask;
        
        var (emotion, confidence, cues) = AnalyzeUserEmotion(message);
        
        var state = new UserEmotionalState
        {
            UserId = userId,
            DetectedEmotion = emotion,
            Confidence = confidence,
            EmotionalCues = cues,
            DetectedAt = DateTime.UtcNow
        };

        _userEmotions[userId] = state;
        return state;
    }

    /// <summary>
    /// Generate empathetic response to user emotion
    /// </summary>
    public async Task<EmotionalResponse> GenerateResponseAsync(string userId, EmotionType userEmotion)
    {
        await Task.CompletedTask;
        return GenerateEmotionalResponse(userId, userEmotion);
    }

    #endregion
}
