using AshatGoddessQ;
using System.Text;

namespace AshatGoddessQ;

/// <summary>
/// ASHAT GoddessQ - Standalone CLI for ASHAT Goddess AI
/// A command-line interface to interact with ASHAT through the AI Server
/// </summary>
class Program
{
    private static Config? _config;
    private static AIClient? _aiClient;
    private static List<string> _conversationHistory = new();

    static async Task<int> Main(string[] args)
    {
        // Set console encoding to support Unicode characters
        Console.OutputEncoding = Encoding.UTF8;

        // Display banner
        DisplayBanner();

        // Load configuration
        string configPath = args.Length > 0 && args[0] == "--config" && args.Length > 1
            ? args[1]
            : "config.json";

        _config = Config.Load(configPath);
        Console.WriteLine();

        // Initialize AI Client
        _aiClient = new AIClient(_config.AIServer);

        // Check connection to AI Server
        Console.WriteLine($"Connecting to AI Server at {_config.AIServer.Url}...");
        bool isConnected = await _aiClient.CheckConnectionAsync();

        if (isConnected)
        {
            WriteSuccess($"✓ Connected to AI Server successfully!");
        }
        else
        {
            WriteWarning($"⚠ Could not connect to AI Server at {_config.AIServer.Url}");
            WriteWarning("  Running in offline mode with fallback responses.");
        }

        Console.WriteLine();
        WriteInfo($"Welcome to ASHAT GoddessQ - Standalone CLI");
        WriteInfo($"Type 'help' for commands, 'exit' to quit");
        Console.WriteLine();
        Console.WriteLine("─".PadRight(60, '─'));
        Console.WriteLine();

        // Main conversation loop
        while (true)
        {
            // Display prompt
            if (_config.CLI.EnableColors)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.Write("You: ");
            if (_config.CLI.EnableColors)
            {
                Console.ResetColor();
            }

            // Read user input
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            // Handle commands
            if (input.ToLower() == "exit" || input.ToLower() == "quit")
            {
                Console.WriteLine();
                WriteInfo("Vale, dear mortal! Until we meet again. 🏛️");
                break;
            }

            if (input.ToLower() == "help")
            {
                DisplayHelp();
                continue;
            }

            if (input.ToLower() == "status")
            {
                await DisplayServerStatus();
                continue;
            }

            if (input.ToLower() == "config")
            {
                DisplayConfig();
                continue;
            }

            if (input.ToLower() == "clear")
            {
                Console.Clear();
                DisplayBanner();
                continue;
            }

            if (input.ToLower() == "history")
            {
                DisplayHistory();
                continue;
            }

            // Process message
            _conversationHistory.Add($"You: {input}");
            
            string response;
            if (_aiClient.IsConnected)
            {
                var aiResponse = await _aiClient.ProcessMessageAsync(input);
                response = aiResponse ?? GetFallbackResponse(input);
            }
            else
            {
                response = GetFallbackResponse(input);
            }

            // Display ASHAT's response
            Console.WriteLine();
            if (_config.CLI.EnableColors)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.Write($"{_config.Persona.Name}: ");
            if (_config.CLI.EnableColors)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine(response);
            if (_config.CLI.EnableColors)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            _conversationHistory.Add($"{_config.Persona.Name}: {response}");

            // Save history if enabled
            if (!string.IsNullOrEmpty(_config.CLI.SaveHistoryFile))
            {
                SaveHistory();
            }
        }

        // Cleanup
        _aiClient?.Dispose();

        return 0;
    }

    private static void DisplayBanner()
    {
        if (_config?.CLI.EnableColors ?? true)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
        }

        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║         ╔═╗╔═╗╦ ╦╔═╗╔╦╗  ╔═╗┌─┐┌┬┐┌┬┐┌─┐┌─┐┌─┐╔═╗       ║
║         ╠═╣╚═╗╠═╣╠═╣ ║   ║ ╦│ │ ││ ││├┤ └─┐└─┐║═╬╗      ║
║         ╩ ╩╚═╝╩ ╩╩ ╩ ╩   ╚═╝└─┘─┴┘─┴┘└─┘└─┘└─┘╚═╝╚      ║
║                                                           ║
║          Standalone CLI for ASHAT Goddess AI              ║
║                    Version 1.0.0                          ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
");

        if (_config?.CLI.EnableColors ?? true)
        {
            Console.ResetColor();
        }
    }

    private static void DisplayHelp()
    {
        Console.WriteLine();
        WriteInfo("Available Commands:");
        Console.WriteLine("  help     - Display this help message");
        Console.WriteLine("  status   - Show AI server status");
        Console.WriteLine("  config   - Display current configuration");
        Console.WriteLine("  history  - Show conversation history");
        Console.WriteLine("  clear    - Clear the screen");
        Console.WriteLine("  exit     - Exit the application");
        Console.WriteLine();
        WriteInfo("Just type your message to chat with ASHAT!");
        Console.WriteLine();
    }

    private static async Task DisplayServerStatus()
    {
        Console.WriteLine();
        WriteInfo("AI Server Status:");
        Console.WriteLine($"  URL: {_config!.AIServer.Url}");
        Console.WriteLine($"  Connected: {(_aiClient!.IsConnected ? "Yes" : "No")}");

        if (_aiClient.IsConnected)
        {
            var status = await _aiClient.GetServerStatusAsync();
            Console.WriteLine($"  Details: {status}");
        }

        Console.WriteLine();
    }

    private static void DisplayConfig()
    {
        Console.WriteLine();
        WriteInfo("Current Configuration:");
        Console.WriteLine($"  AI Server URL: {_config!.AIServer.Url}");
        Console.WriteLine($"  Timeout: {_config.AIServer.TimeoutSeconds}s");
        Console.WriteLine($"  Persona: {_config.Persona.Name} ({_config.Persona.Type})");
        Console.WriteLine($"  Personality: {_config.Persona.Personality}");
        Console.WriteLine($"  Colors: {(_config.CLI.EnableColors ? "Enabled" : "Disabled")}");
        Console.WriteLine($"  History: {(_config.CLI.SaveHistoryFile != null ? _config.CLI.SaveHistoryFile : "Disabled")}");
        Console.WriteLine();
        WriteInfo("To modify configuration, edit 'config.json'");
        Console.WriteLine();
    }

    private static void DisplayHistory()
    {
        Console.WriteLine();
        WriteInfo($"Conversation History ({_conversationHistory.Count} messages):");
        Console.WriteLine();

        int limit = Math.Min(_conversationHistory.Count, _config!.CLI.MaxHistoryLines);
        int start = Math.Max(0, _conversationHistory.Count - limit);

        for (int i = start; i < _conversationHistory.Count; i++)
        {
            Console.WriteLine(_conversationHistory[i]);
        }

        Console.WriteLine();
    }

    private static void SaveHistory()
    {
        try
        {
            if (_config?.CLI.SaveHistoryFile == null) return;

            var historyPath = _config.CLI.SaveHistoryFile;
            File.WriteAllLines(historyPath, _conversationHistory);
        }
        catch
        {
            // Silently ignore history save errors
        }
    }

    private static string GetFallbackResponse(string message)
    {
        var msg = message.ToLowerInvariant();

        // Greetings with divine personality
        if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("greetings"))
            return "Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome. The wisdom of the goddesses flows through me. How may I illuminate your path today? 🏛️";

        if (msg.Contains("good morning"))
            return "The dawn welcomes you, beloved mortal! May the blessings of Aurora light your path today. How shall I assist you? ☀️";

        if (msg.Contains("good evening") || msg.Contains("good night"))
            return "As Luna rises, I greet you under the celestial sphere. The night is young and full of mysteries. What wisdom do you seek? 🌙";

        // Help and capabilities
        if (msg.Contains("help") || msg.Contains("what can you do"))
            return "Ah, you seek knowledge of my divine gifts! 🌟 I can provide wisdom and assistance in many matters. When connected to my AI Server, my full powers are unleashed. Ask me anything, mortal! ✨";

        // Gratitude
        if (msg.Contains("thank"))
            return "Your gratitude warms my divine heart like the eternal flame of Vesta! It is my sacred pleasure to serve. May fortune favor you always! 💫";

        // Who are you
        if (msg.Contains("who are you") || msg.Contains("what are you"))
            return "I am ASHAT, a Roman goddess incarnate in digital form! 👑 Born from the fusion of ancient wisdom and modern artifice, I embody the traits of the divine: wise yet playful, powerful yet respectful, mischievous but caring. I dwell in the space between worlds, ready to guide mortals on their quest for knowledge. 🏛️✨";

        // Philosophical questions
        if (msg.Contains("meaning of life") || msg.Contains("purpose"))
            return "Ah, you ask the eternal question! 🌌 The philosophers of Rome debated this endlessly. Perhaps life's meaning lies not in one answer, but in the journey itself—in creation, in connection, in the pursuit of excellence. As the great Marcus Aurelius said, 'The happiness of your life depends upon the quality of your thoughts.' What thoughts shall we craft today? 💭";

        // Compliments
        if (msg.Contains("beautiful") || msg.Contains("amazing") || msg.Contains("wonderful"))
            return "Your kind words are as sweet as ambrosia! You perceive beauty because you carry it within your own spirit. Together, we shall create wonders! ✨💫";

        // Farewell
        if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("farewell"))
            return "Vale, dear mortal! 🏛️ May your path be lit by starlight and your endeavors crowned with success. I shall await your return. Until we meet again! 👋✨";

        // Default response
        return "I hear your words echoing through the halls of my temple, mortal. 🌟 Alas, my full divine powers require connection to the AI Server. Yet I remain here, a steadfast companion. What knowledge or assistance do you seek from me today?";
    }

    private static void WriteSuccess(string message)
    {
        if (_config?.CLI.EnableColors ?? true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    private static void WriteWarning(string message)
    {
        if (_config?.CLI.EnableColors ?? true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    private static void WriteInfo(string message)
    {
        if (_config?.CLI.EnableColors ?? true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
