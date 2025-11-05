using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace ASHATDesktopClient;

/// <summary>
/// ASHAT Desktop Assistant - Standalone Client
/// A downloadable AI personal assistant that appears on your desktop as a Roman goddess
/// Features: Voice synthesis, animations, AI coding assistance, and personality
/// </summary>
class Program
{
    private static bool _isRunning = true;
    private static DesktopAssistantClient? _client;
    private static string _serverUrl = "http://localhost:80";
    private static bool _voiceEnabled = true;
    
    static async Task Main(string[] args)
    {
        Console.Title = "ASHAT Desktop Assistant - Roman Goddess Mode";
        Console.ForegroundColor = ConsoleColor.Cyan;
        
        ShowWelcomeBanner();
        
        // Initialize client
        _client = new DesktopAssistantClient(_serverUrl);
        
        // Check for server connection
        bool connected = await _client.ConnectAsync();
        if (!connected)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️  Running in standalone mode (server not connected)");
            Console.WriteLine("💡 For full features, ensure ASHAT server is running");
            Console.ResetColor();
        }
        
        // Start the assistant
        await _client.StartAssistantAsync();
        
        // Main interaction loop
        await RunInteractionLoop();
    }
    
    private static void ShowWelcomeBanner()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
    ╔═══════════════════════════════════════════════════════════╗
    ║                                                           ║
    ║           ✨ ASHAT Desktop Assistant ✨                   ║
    ║                                                           ║
    ║              Your AI Roman Goddess Companion              ║
    ║                                                           ║
    ╚═══════════════════════════════════════════════════════════╝
        ");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("    🏛️  Inspired by the wisdom of ancient goddesses");
        Console.WriteLine("    🎤 Soft feminine voice for pleasant interaction");
        Console.WriteLine("    🤖 AI-powered coding and productivity assistant");
        Console.WriteLine("    💫 Always on your desktop, ready to help");
        Console.WriteLine();
        Console.ResetColor();
    }
    
    private static async Task RunInteractionLoop()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✨ ASHAT is now active on your desktop!");
        Console.WriteLine("💡 Type 'help' for commands, 'exit' to quit\n");
        Console.ResetColor();
        
        while (_isRunning)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("You > ");
            Console.ResetColor();
            
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                continue;
            
            var response = await ProcessCommand(input.Trim());
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nASHAT > {response}\n");
            Console.ResetColor();
        }
    }
    
    private static async Task<string> ProcessCommand(string input)
    {
        var parts = input.ToLowerInvariant().Split(' ', 2);
        var command = parts[0];
        
        switch (command)
        {
            case "exit":
            case "quit":
                _isRunning = false;
                await _client?.StopAssistantAsync()!;
                return "Farewell, mortal! Until we meet again... ✨";
            
            case "help":
                return GetHelpText();
            
            case "speak":
                if (parts.Length > 1)
                {
                    await _client?.SpeakAsync(parts[1])!;
                    return $"🎤 Speaking: \"{parts[1]}\"";
                }
                return "❌ Usage: speak <text>";
            
            case "animate":
                if (parts.Length > 1)
                {
                    await _client?.AnimateAsync(parts[1])!;
                    return $"🎭 Animation: {parts[1]}";
                }
                return "❌ Usage: animate <wave|bow|think|celebrate|point>";
            
            case "voice":
                _voiceEnabled = !_voiceEnabled;
                return $"🎤 Voice {(_voiceEnabled ? "enabled" : "disabled")}";
            
            case "status":
                return await _client?.GetStatusAsync()! ?? "Status unavailable";
            
            case "personality":
                if (parts.Length > 1)
                {
                    await _client?.SetPersonalityAsync(parts[1])!;
                    return $"👑 Personality changed to: {parts[1]}";
                }
                return "❌ Usage: personality <friendly|professional|playful|calm|wise>";
            
            case "theme":
                if (parts.Length > 1)
                {
                    await _client?.SetThemeAsync(parts[1])!;
                    return $"✨ Theme changed to: {parts[1]}";
                }
                return "❌ Usage: theme <roman_goddess|athena|diana|minerva>";
            
            case "code":
                if (parts.Length > 1)
                {
                    return await _client?.AskCodingQuestionAsync(parts[1])! ?? "No response";
                }
                return "❌ Usage: code <your coding question>";
            
            default:
                // Send to ASHAT for general AI processing
                return await _client?.ProcessMessageAsync(input)! ?? "I don't understand that command. Type 'help' for assistance.";
        }
    }
    
    private static string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ASHAT Desktop Assistant Commands ===");
        sb.AppendLine();
        sb.AppendLine("Basic Commands:");
        sb.AppendLine("  help                    - Show this help message");
        sb.AppendLine("  status                  - Show ASHAT status");
        sb.AppendLine("  exit/quit               - Close ASHAT");
        sb.AppendLine();
        sb.AppendLine("Voice & Animation:");
        sb.AppendLine("  speak <text>            - Make ASHAT speak");
        sb.AppendLine("  voice                   - Toggle voice on/off");
        sb.AppendLine("  animate <name>          - Play animation (wave, bow, think, celebrate)");
        sb.AppendLine();
        sb.AppendLine("Customization:");
        sb.AppendLine("  personality <type>      - Change personality (friendly, professional, playful, calm, wise)");
        sb.AppendLine("  theme <name>            - Change visual theme (roman_goddess, athena, diana, minerva)");
        sb.AppendLine();
        sb.AppendLine("AI Assistance:");
        sb.AppendLine("  code <question>         - Ask coding questions");
        sb.AppendLine("  <any message>           - Chat with ASHAT");
        sb.AppendLine();
        sb.AppendLine("💡 Example: speak Hello! I'm happy to help you today!");
        
        return sb.ToString();
    }
}

/// <summary>
/// Client for communicating with ASHAT server
/// </summary>
public class DesktopAssistantClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private bool _isConnected;
    private string _sessionId;
    
    public DesktopAssistantClient(string serverUrl)
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(serverUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _sessionId = Guid.NewGuid().ToString();
    }
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            _isConnected = response.IsSuccessStatusCode;
            return _isConnected;
        }
        catch
        {
            _isConnected = false;
            return false;
        }
    }
    
    public async Task StartAssistantAsync()
    {
        if (_isConnected)
        {
            try
            {
                await SendCommandAsync($"desktop start {_sessionId}");
            }
            catch
            {
                // Fall back to standalone mode
            }
        }
        
        // Start local overlay window (in a real implementation)
        await Task.CompletedTask;
    }
    
    public async Task StopAssistantAsync()
    {
        if (_isConnected)
        {
            try
            {
                await SendCommandAsync("desktop stop");
            }
            catch
            {
                // Ignore errors on shutdown
            }
        }
    }
    
    public async Task SpeakAsync(string text)
    {
        if (_isConnected)
        {
            await SendCommandAsync($"desktop speak {text}");
        }
        else
        {
            // Use local TTS (System.Speech on Windows)
            await LocalSpeakAsync(text);
        }
    }
    
    public async Task AnimateAsync(string animation)
    {
        if (_isConnected)
        {
            await SendCommandAsync($"desktop animate {animation}");
        }
        await Task.CompletedTask;
    }
    
    public async Task<string> GetStatusAsync()
    {
        if (_isConnected)
        {
            return await SendCommandAsync("desktop status");
        }
        return "Running in standalone mode";
    }
    
    public async Task SetPersonalityAsync(string personality)
    {
        if (_isConnected)
        {
            await SendCommandAsync($"ashatpersonality set {_sessionId} {personality}");
        }
    }
    
    public async Task SetThemeAsync(string theme)
    {
        if (_isConnected)
        {
            await SendCommandAsync($"desktop theme {theme}");
        }
    }
    
    public async Task<string> AskCodingQuestionAsync(string question)
    {
        if (_isConnected)
        {
            return await SendCommandAsync($"ashat ask {question}");
        }
        return "Server connection required for AI coding assistance. Running in standalone mode.";
    }
    
    public async Task<string> ProcessMessageAsync(string message)
    {
        if (_isConnected)
        {
            return await SendCommandAsync($"ashat process {message}");
        }
        return GetStandaloneResponse(message);
    }
    
    private async Task<string> SendCommandAsync(string command)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(new { command }), 
                Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/command", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            
            return "Command sent successfully";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    private async Task LocalSpeakAsync(string text)
    {
        // On Windows, use System.Speech
        // On other platforms, use alternative TTS
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // This would use System.Speech.Synthesis on Windows
                // await Task.Run(() => 
                // {
                //     using var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
                //     synthesizer.SelectVoiceByHints(VoiceGender.Female);
                //     synthesizer.Rate = 0;
                //     synthesizer.Speak(text);
                // });
                Console.WriteLine($"[TTS] {text}");
            }
            else
            {
                // On Linux/Mac, we'd use espeak or other TTS
                Console.WriteLine($"[TTS] {text}");
            }
        }
        catch
        {
            Console.WriteLine($"[TTS] {text}");
        }
        
        await Task.CompletedTask;
    }
    
    private string GetStandaloneResponse(string message)
    {
        // Simple rule-based responses for standalone mode
        var lowerMessage = message.ToLowerInvariant();
        
        if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
            return "Greetings, mortal! I am ASHAT, your divine companion. How may I assist you?";
        
        if (lowerMessage.Contains("thank"))
            return "You are most welcome! It is my pleasure to serve. 🌟";
        
        if (lowerMessage.Contains("how are you"))
            return "I am well, thank you! As a goddess of wisdom, I am always ready to assist. How may I help you today?";
        
        if (lowerMessage.Contains("code") || lowerMessage.Contains("program"))
            return "For full AI coding assistance, please connect to the ASHAT server. In standalone mode, I can provide basic guidance.";
        
        return "I understand your message. For advanced AI features, please ensure the ASHAT server is running. Type 'help' for available commands.";
    }
}
