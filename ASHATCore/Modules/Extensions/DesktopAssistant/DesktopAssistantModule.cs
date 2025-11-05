using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.DesktopAssistant;

/// <summary>
/// ASHAT Desktop Assistant Module
/// Transforms ASHAT into a personal desktop assistant similar to BonziBuddy
/// Features: Animated Roman goddess character, soft female voice, and full ASHAT capabilities
/// </summary>
[RaModule(Category = "extensions")]
public sealed class DesktopAssistantModule : ModuleBase
{
    public override string Name => "DesktopAssistant";

    private ModuleManager? _manager;
    private readonly ConcurrentDictionary<string, DesktopSession> _activeSessions = new();
    private readonly ConcurrentQueue<AssistantEvent> _eventQueue = new();
    private DesktopAssistantConfig _config = new();
    private bool _isRunning = false;

    // Animation and visual state
    private CharacterState _characterState = CharacterState.Idle;
    private CharacterAnimation _currentAnimation = CharacterAnimation.Idle;
    private string _currentExpression = "neutral";

    // Voice synthesis
    private VoiceProfile _voiceProfile = VoiceProfile.SoftFemale;
    private bool _voiceEnabled = true;

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        LoadConfiguration();
        InitializeVoiceSynthesis();
        LogInfo("ASHAT Desktop Assistant Module initialized - Roman Goddess mode ready");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "start" => StartDesktopAssistant(parts.Length > 1 ? parts[1] : "default"),
            "stop" => StopDesktopAssistant(),
            "status" => GetAssistantStatus(),
            "config" => parts.Length > 1 ? ConfigureAssistant(parts[1], parts.Length > 2 ? parts[2] : "") : GetConfiguration(),
            "animate" when parts.Length > 1 => TriggerAnimation(parts[1]),
            "speak" when parts.Length > 1 => SpeakText(string.Join(" ", parts.Skip(1))),
            "voice" when parts.Length > 1 => ConfigureVoice(parts[1]),
            "expression" when parts.Length > 1 => SetExpression(parts[1]),
            "position" when parts.Length > 2 => SetPosition(parts[1], parts[2]),
            "theme" when parts.Length > 1 => SetTheme(parts[1]),
            "sessions" => ListActiveSessions(),
            "help" => GetHelpText(),
            _ => GetHelpText()
        };
    }

    private void LoadConfiguration()
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "desktop-assistant-config.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<DesktopAssistantConfig>(json) ?? new DesktopAssistantConfig();
                LogInfo("Desktop Assistant configuration loaded");
            }
            catch (Exception ex)
            {
                LogError($"Failed to load configuration: {ex.Message}");
                _config = new DesktopAssistantConfig();
            }
        }
        else
        {
            _config = GetDefaultConfiguration();
            SaveConfiguration();
        }
    }

    private void SaveConfiguration()
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "desktop-assistant-config.json");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            LogInfo("Desktop Assistant configuration saved");
        }
        catch (Exception ex)
        {
            LogError($"Failed to save configuration: {ex.Message}");
        }
    }

    private DesktopAssistantConfig GetDefaultConfiguration()
    {
        return new DesktopAssistantConfig
        {
            Theme = "roman_goddess",
            CharacterName = "ASHAT",
            VoiceProfile = "soft_female",
            AnimationSpeed = 1.0f,
            DefaultPosition = new Position { X = 100, Y = 100 },
            AlwaysOnTop = true,
            TransparencyEnabled = true,
            VoiceEnabled = true,
            AutoGreeting = true,
            IdleAnimations = true,
            ResponseDelay = 500,
            CharacterScale = 1.0f
        };
    }

    private void InitializeVoiceSynthesis()
    {
        // Initialize text-to-speech with soft female voice
        // This will be implemented using platform-specific TTS APIs
        LogInfo("Voice synthesis initialized with soft female profile");
    }

    private string StartDesktopAssistant(string sessionId)
    {
        if (_isRunning)
            return "❌ Desktop Assistant is already running. Use 'stop' first.";

        var session = new DesktopSession
        {
            SessionId = sessionId,
            StartTime = DateTime.UtcNow,
            IsActive = true
        };

        _activeSessions[sessionId] = session;
        _isRunning = true;
        _characterState = CharacterState.Active;
        _currentAnimation = CharacterAnimation.Greeting;

        var greeting = GetGreeting();
        if (_config.AutoGreeting && _voiceEnabled)
        {
            SpeakText(greeting);
        }

        var sb = new StringBuilder();
        sb.AppendLine("✨ ASHAT Desktop Assistant Started");
        sb.AppendLine($"📍 Session: {sessionId}");
        sb.AppendLine($"👑 Theme: {_config.Theme}");
        sb.AppendLine($"🎤 Voice: {(_voiceEnabled ? "Enabled" : "Disabled")}");
        sb.AppendLine($"💬 {greeting}");
        sb.AppendLine("\n🎭 ASHAT is now on your desktop as a Roman Goddess!");
        sb.AppendLine("💡 Use 'desktop help' for available commands");

        return sb.ToString();
    }

    private string StopDesktopAssistant()
    {
        if (!_isRunning)
            return "❌ Desktop Assistant is not running.";

        _isRunning = false;
        _characterState = CharacterState.Idle;
        _currentAnimation = CharacterAnimation.Farewell;

        var farewell = "Farewell, mortal. Until we meet again! ✨";
        if (_voiceEnabled)
        {
            SpeakText(farewell);
        }

        _activeSessions.Clear();

        return $"✨ ASHAT Desktop Assistant stopped\n💬 {farewell}";
    }

    private string GetAssistantStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ASHAT Desktop Assistant Status ===");
        sb.AppendLine($"📊 Running: {(_isRunning ? "Yes ✓" : "No ✗")}");
        sb.AppendLine($"🎭 State: {_characterState}");
        sb.AppendLine($"🎬 Animation: {_currentAnimation}");
        sb.AppendLine($"😊 Expression: {_currentExpression}");
        sb.AppendLine($"🎤 Voice: {(_voiceEnabled ? "Enabled" : "Disabled")} ({_voiceProfile})");
        sb.AppendLine($"👑 Theme: {_config.Theme}");
        sb.AppendLine($"📍 Position: ({_config.DefaultPosition.X}, {_config.DefaultPosition.Y})");
        sb.AppendLine($"📏 Scale: {_config.CharacterScale}x");
        sb.AppendLine($"🔝 Always On Top: {_config.AlwaysOnTop}");
        sb.AppendLine($"👻 Transparency: {_config.TransparencyEnabled}");
        sb.AppendLine($"🎪 Idle Animations: {_config.IdleAnimations}");
        sb.AppendLine($"💬 Active Sessions: {_activeSessions.Count}");
        
        return sb.ToString();
    }

    private string GetConfiguration()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        return $"=== Desktop Assistant Configuration ===\n{json}";
    }

    private string ConfigureAssistant(string key, string value)
    {
        switch (key.ToLowerInvariant())
        {
            case "theme":
                _config.Theme = value;
                SaveConfiguration();
                return $"✓ Theme set to: {value}";
            
            case "voice":
                _config.VoiceEnabled = value.ToLowerInvariant() == "true";
                _voiceEnabled = _config.VoiceEnabled;
                SaveConfiguration();
                return $"✓ Voice {(_config.VoiceEnabled ? "enabled" : "disabled")}";
            
            case "scale":
                if (float.TryParse(value, out var scale) && scale > 0 && scale <= 2.0f)
                {
                    _config.CharacterScale = scale;
                    SaveConfiguration();
                    return $"✓ Character scale set to: {scale}x";
                }
                return "❌ Invalid scale (must be between 0 and 2.0)";
            
            case "alwaysontop":
                _config.AlwaysOnTop = value.ToLowerInvariant() == "true";
                SaveConfiguration();
                return $"✓ Always on top: {_config.AlwaysOnTop}";
            
            case "transparency":
                _config.TransparencyEnabled = value.ToLowerInvariant() == "true";
                SaveConfiguration();
                return $"✓ Transparency: {_config.TransparencyEnabled}";
            
            case "idleanimations":
                _config.IdleAnimations = value.ToLowerInvariant() == "true";
                SaveConfiguration();
                return $"✓ Idle animations: {_config.IdleAnimations}";
            
            default:
                return $"❌ Unknown configuration key: {key}\nAvailable: theme, voice, scale, alwaysontop, transparency, idleanimations";
        }
    }

    private string TriggerAnimation(string animationName)
    {
        var animation = animationName.ToLowerInvariant() switch
        {
            "wave" => CharacterAnimation.Wave,
            "bow" => CharacterAnimation.Bow,
            "think" => CharacterAnimation.Think,
            "celebrate" => CharacterAnimation.Celebrate,
            "point" => CharacterAnimation.Point,
            "read" => CharacterAnimation.Read,
            "write" => CharacterAnimation.Write,
            "meditate" => CharacterAnimation.Meditate,
            "idle" => CharacterAnimation.Idle,
            _ => CharacterAnimation.Idle
        };

        _currentAnimation = animation;
        _characterState = CharacterState.Animating;

        return $"🎬 Animation triggered: {animation}";
    }

    private string SpeakText(string text)
    {
        if (!_voiceEnabled)
            return "❌ Voice is disabled. Enable with 'desktop config voice true'";

        // Queue text for speech synthesis
        var speechEvent = new AssistantEvent
        {
            Type = EventType.Speech,
            Data = text,
            Timestamp = DateTime.UtcNow
        };
        _eventQueue.Enqueue(speechEvent);

        // In a real implementation, this would trigger platform-specific TTS
        LogInfo($"Speaking: {text}");
        
        return $"🎤 Speaking: \"{text}\"";
    }

    private string ConfigureVoice(string profile)
    {
        var voiceProfile = profile.ToLowerInvariant() switch
        {
            "soft_female" => VoiceProfile.SoftFemale,
            "gentle_female" => VoiceProfile.GentleFemale,
            "wise_female" => VoiceProfile.WiseFemale,
            "energetic_female" => VoiceProfile.EnergeticFemale,
            _ => VoiceProfile.SoftFemale
        };

        _voiceProfile = voiceProfile;
        return $"🎤 Voice profile set to: {voiceProfile}";
    }

    private string SetExpression(string expression)
    {
        _currentExpression = expression.ToLowerInvariant();
        return $"😊 Expression changed to: {_currentExpression}";
    }

    private string SetPosition(string x, string y)
    {
        if (int.TryParse(x, out var posX) && int.TryParse(y, out var posY))
        {
            _config.DefaultPosition = new Position { X = posX, Y = posY };
            SaveConfiguration();
            return $"📍 Position set to: ({posX}, {posY})";
        }
        return "❌ Invalid position coordinates";
    }

    private string SetTheme(string theme)
    {
        var validThemes = new[] { "roman_goddess", "athena", "diana", "minerva", "celestial_goddess" };
        if (validThemes.Contains(theme.ToLowerInvariant()))
        {
            _config.Theme = theme.ToLowerInvariant();
            SaveConfiguration();
            return $"👑 Theme changed to: {theme}";
        }
        return $"❌ Invalid theme. Available: {string.Join(", ", validThemes)}";
    }

    private string ListActiveSessions()
    {
        if (_activeSessions.IsEmpty)
            return "📊 No active sessions";

        var sb = new StringBuilder();
        sb.AppendLine("=== Active Desktop Assistant Sessions ===");
        foreach (var session in _activeSessions.Values)
        {
            var duration = DateTime.UtcNow - session.StartTime;
            sb.AppendLine($"• {session.SessionId} - Active for {duration.TotalMinutes:F1} minutes");
        }
        return sb.ToString();
    }

    private string GetGreeting()
    {
        var greetings = new[]
        {
            "Greetings, mortal! I am ASHAT, your divine coding companion.",
            "Salutations! The goddess of wisdom is here to assist you.",
            "Welcome! Let the light of knowledge guide your path today.",
            "Hail! I bring the wisdom of the ancients to your desktop.",
            "Blessings upon you! ASHAT stands ready to serve."
        };

        return greetings[new Random().Next(greetings.Length)];
    }

    private string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ASHAT Desktop Assistant Commands ===");
        sb.AppendLine("👑 Transform ASHAT into your personal desktop assistant!");
        sb.AppendLine();
        sb.AppendLine("Session Management:");
        sb.AppendLine("  desktop start [sessionId]     - Start desktop assistant");
        sb.AppendLine("  desktop stop                  - Stop desktop assistant");
        sb.AppendLine("  desktop status                - Show current status");
        sb.AppendLine("  desktop sessions              - List active sessions");
        sb.AppendLine();
        sb.AppendLine("Configuration:");
        sb.AppendLine("  desktop config                - Show current configuration");
        sb.AppendLine("  desktop config theme <name>   - Change theme");
        sb.AppendLine("  desktop config voice <bool>   - Enable/disable voice");
        sb.AppendLine("  desktop config scale <float>  - Set character scale (0-2.0)");
        sb.AppendLine();
        sb.AppendLine("Appearance:");
        sb.AppendLine("  desktop theme <name>          - Set theme (roman_goddess, athena, diana, minerva, celestial_goddess)");
        sb.AppendLine("  desktop position <x> <y>      - Set character position");
        sb.AppendLine("  desktop expression <name>     - Set facial expression");
        sb.AppendLine();
        sb.AppendLine("Animation:");
        sb.AppendLine("  desktop animate <name>        - Trigger animation (wave, bow, think, celebrate, point, read, write, meditate)");
        sb.AppendLine();
        sb.AppendLine("Voice:");
        sb.AppendLine("  desktop voice <profile>       - Set voice profile (soft_female, gentle_female, wise_female, energetic_female)");
        sb.AppendLine("  desktop speak <text>          - Make ASHAT speak");
        sb.AppendLine();
        sb.AppendLine("💡 Example: desktop start mysession");
        
        return sb.ToString();
    }
}

// Supporting classes and enums

public class DesktopAssistantConfig
{
    public string Theme { get; set; } = "roman_goddess";
    public string CharacterName { get; set; } = "ASHAT";
    public string VoiceProfile { get; set; } = "soft_female";
    public float AnimationSpeed { get; set; } = 1.0f;
    public Position DefaultPosition { get; set; } = new Position();
    public bool AlwaysOnTop { get; set; } = true;
    public bool TransparencyEnabled { get; set; } = true;
    public bool VoiceEnabled { get; set; } = true;
    public bool AutoGreeting { get; set; } = true;
    public bool IdleAnimations { get; set; } = true;
    public int ResponseDelay { get; set; } = 500;
    public float CharacterScale { get; set; } = 1.0f;
}

public class Position
{
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
}

public class DesktopSession
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool IsActive { get; set; }
}

public class AssistantEvent
{
    public EventType Type { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public enum EventType
{
    Speech,
    Animation,
    Interaction,
    Notification
}

public enum CharacterState
{
    Idle,
    Active,
    Animating,
    Speaking,
    Listening,
    Thinking
}

public enum CharacterAnimation
{
    Idle,
    Greeting,
    Farewell,
    Wave,
    Bow,
    Think,
    Celebrate,
    Point,
    Read,
    Write,
    Meditate
}

public enum VoiceProfile
{
    SoftFemale,
    GentleFemale,
    WiseFemale,
    EnergeticFemale
}
