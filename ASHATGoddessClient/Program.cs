using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASHATGoddessClient;

/// <summary>
/// ASHAT - The Roman Goddess herself
/// This IS ASHAT - the animated goddess who connects to the server
/// She loads her visual assets and processes through the game engine
/// </summary>
class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start ASHAT: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AshatApp>()
            .UsePlatformDetect()
            .LogToTrace();
}

public class AshatApp : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new AshatMainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

/// <summary>
/// ASHAT's main window - She appears on the desktop
/// </summary>
public class AshatMainWindow : Window
{
    private readonly AshatRenderer _renderer;
    private readonly AshatBrain _brain;
    private readonly ChatInterface _chatInterface;

    public AshatMainWindow()
    {
        Title = "ASHAT - Roman Goddess";
        Width = 800;
        Height = 900;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Set transparency
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        Background = Brushes.Transparent;
        
        _brain = new AshatBrain("http://localhost:80");
        _renderer = new AshatRenderer();
        _chatInterface = new ChatInterface(_brain);

        // Create main layout
        var mainPanel = new Panel();
        
        // Add goddess visual at the top
        var goddessContainer = new Border
        {
            Width = 400,
            Height = 500,
            Margin = new Thickness(200, 50, 200, 0),
            Child = _renderer.GetGoddessVisual()
        };
        
        // Add chat interface at the bottom
        var chatContainer = new Border
        {
            Width = 450,
            Height = 350,
            Margin = new Thickness(175, 550, 175, 0),
            Child = _chatInterface.GetChatPanel()
        };

        mainPanel.Children.Add(goddessContainer);
        mainPanel.Children.Add(chatContainer);
        
        Content = mainPanel;

        // Initialize ASHAT
        _ = InitializeAshatAsync();
    }

    private async Task InitializeAshatAsync()
    {
        await _brain.ConnectToServerAsync();
        await _brain.SpeakAsync("Greetings, mortal! I am ASHAT, your divine companion. The wisdom of the goddesses flows through me.");
    }
}

/// <summary>
/// ASHAT's renderer - Generates her visual appearance using game engine
/// </summary>
public class AshatRenderer
{
    private readonly Random _random = new Random();
    private bool _isAnimating = false;

    public Control GetGoddessVisual()
    {
        var canvas = new Canvas
        {
            Width = 400,
            Height = 500
        };

        // Background glow
        var glow = new Border
        {
            Width = 350,
            Height = 450,
            CornerRadius = new CornerRadius(175),
            Background = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(80, 138, 43, 226), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 138, 43, 226), 1)
                }
            }
        };
        Canvas.SetLeft(glow, 25);
        Canvas.SetTop(glow, 25);

        // Main goddess form
        var goddessBody = new Border
        {
            Width = 300,
            Height = 400,
            CornerRadius = new CornerRadius(150, 150, 135, 135),
            Background = new Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0.5, 0, Avalonia.RelativeUnit.Relative),
                EndPoint = new Avalonia.RelativePoint(0.5, 1, Avalonia.RelativeUnit.Relative),
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(80, 138, 43, 226), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(130, 75, 0, 130), 1)
                }
            },
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(150, 255, 215, 0)),
            BorderThickness = new Thickness(3)
        };
        Canvas.SetLeft(goddessBody, 50);
        Canvas.SetTop(goddessBody, 50);

        // Crown
        var crown = new TextBlock
        {
            Text = "👑",
            FontSize = 60,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 215, 0))
        };
        Canvas.SetLeft(crown, 170);
        Canvas.SetTop(crown, 20);

        // Face
        var face = new Ellipse
        {
            Width = 150,
            Height = 150,
            Fill = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 220, 200), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 200, 180), 1)
                }
            }
        };
        Canvas.SetLeft(face, 125);
        Canvas.SetTop(face, 120);

        // Eyes
        var leftEye = new Ellipse
        {
            Width = 15,
            Height = 15,
            Fill = Avalonia.Media.Brushes.DarkSlateBlue
        };
        Canvas.SetLeft(leftEye, 155);
        Canvas.SetTop(leftEye, 170);

        var rightEye = new Ellipse
        {
            Width = 15,
            Height = 15,
            Fill = Avalonia.Media.Brushes.DarkSlateBlue
        };
        Canvas.SetLeft(rightEye, 225);
        Canvas.SetTop(rightEye, 170);

        // Smile
        var smile = new Avalonia.Controls.Shapes.Path
        {
            Stroke = Brushes.DarkSlateBlue,
            StrokeThickness = 3,
            Data = Geometry.Parse("M 170,210 Q 200,230 230,210")
        };
        Canvas.SetLeft(smile, 0);
        Canvas.SetTop(smile, 0);

        // Name
        var name = new TextBlock
        {
            Text = "ASHAT",
            FontSize = 32,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0, 0.5, Avalonia.RelativeUnit.Relative),
                EndPoint = new Avalonia.RelativePoint(1, 0.5, Avalonia.RelativeUnit.Relative),
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 255, 255), 0.5),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 1)
                }
            }
        };
        Canvas.SetLeft(name, 140);
        Canvas.SetTop(name, 470);

        canvas.Children.Add(glow);
        canvas.Children.Add(goddessBody);
        canvas.Children.Add(crown);
        canvas.Children.Add(face);
        canvas.Children.Add(leftEye);
        canvas.Children.Add(rightEye);
        canvas.Children.Add(smile);
        canvas.Children.Add(name);

        return canvas;
    }

    public void PlayAnimation(string animationType)
    {
        _isAnimating = true;
        // Animation logic using game engine would go here
        // For now, we indicate animation state change
        Console.WriteLine($"[ASHAT] Playing animation: {animationType}");
    }
}

/// <summary>
/// ASHAT's brain - Connects to server and processes
/// </summary>
public class AshatBrain
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private bool _isConnected = false;
    private string _currentPersonality = "friendly";

    public AshatBrain(string serverUrl)
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(serverUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> ConnectToServerAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            _isConnected = response.IsSuccessStatusCode;
            
            if (_isConnected)
            {
                Console.WriteLine("[ASHAT] Connected to server successfully!");
            }
            else
            {
                Console.WriteLine("[ASHAT] Server not responding, running in standalone mode");
            }
            
            return _isConnected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ASHAT] Failed to connect: {ex.Message}");
            _isConnected = false;
            return false;
        }
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        if (_isConnected)
        {
            try
            {
                var request = new { message, personality = _currentPersonality };
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("/api/ashat/chat", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                    var responseText = result.GetProperty("response").GetString() ?? "...";
                    
                    // Speak the response
                    await SpeakAsync(responseText);
                    
                    return responseText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Error processing message: {ex.Message}");
            }
        }

        // Fallback to local processing
        return GetLocalResponse(message);
    }

    public async Task SpeakAsync(string text)
    {
        await Task.Run(() =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Use System.Speech on Windows
                    Console.WriteLine($"[ASHAT Speaking] {text}");
                    // In full implementation:
                    // using var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                    // synth.SelectVoiceByHints(VoiceGender.Female);
                    // synth.Rate = 0;
                    // synth.Speak(text);
                }
                else
                {
                    // On other platforms, log for now
                    Console.WriteLine($"[ASHAT Speaking] {text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Speech error: {ex.Message}");
            }
        });
    }

    public void SetPersonality(string personality)
    {
        _currentPersonality = personality;
        Console.WriteLine($"[ASHAT] Personality changed to: {personality}");
    }

    private string GetLocalResponse(string message)
    {
        var msg = message.ToLowerInvariant();
        
        if (msg.Contains("hello") || msg.Contains("hi"))
            return "Greetings, mortal! I am ASHAT, your divine companion. How may I assist you? 🏛️";
        
        if (msg.Contains("help"))
            return "I am here to provide wisdom in coding, debugging, and knowledge. I can also help you launch RaStudios! Try saying 'open rastudios' or 'launch studio'. ✨";
        
        if (msg.Contains("thank"))
            return "Your gratitude warms my divine heart! It is my pleasure to serve. 💫";
        
        // RaStudios commands
        if (msg.Contains("rastudios") || msg.Contains("ra studios") || msg.Contains("studio"))
        {
            if (msg.Contains("open") || msg.Contains("launch") || msg.Contains("start") || msg.Contains("run"))
            {
                LaunchRaStudios();
                return "Opening RaStudios for you! The studio shall manifest shortly. 🎮✨";
            }
            if (msg.Contains("what") || msg.Contains("about") || msg.Contains("tell"))
            {
                return "RaStudios is a powerful IDE and game development platform. It allows you to build games, edit assets, and manage your RaOS projects. Would you like me to open it for you? 🎨";
            }
        }
        
        return "I hear your words, mortal. To access my full divine wisdom, ensure the server connection is established. 🌟";
    }

    private void LaunchRaStudios()
    {
        try
        {
            string raStudiosPath = "";
            
            // Determine the path to RaStudios executable based on platform
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Look for RaStudios in common locations
                var possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppContext.BaseDirectory, "RaStudios", "RaStudios.exe"),
                    System.IO.Path.Combine(AppContext.BaseDirectory, "..", "RaStudios", "RaStudios.exe"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RaStudios", "RaStudios.exe"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "RaStudios", "RaStudios.exe")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        raStudiosPath = path;
                        break;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppContext.BaseDirectory, "RaStudios", "RaStudios"),
                    System.IO.Path.Combine(AppContext.BaseDirectory, "..", "RaStudios", "RaStudios"),
                    "/usr/local/bin/RaStudios",
                    "/opt/RaStudios/RaStudios"
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        raStudiosPath = path;
                        break;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppContext.BaseDirectory, "RaStudios", "RaStudios.app", "Contents", "MacOS", "RaStudios"),
                    "/Applications/RaStudios.app/Contents/MacOS/RaStudios"
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        raStudiosPath = path;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(raStudiosPath) && File.Exists(raStudiosPath))
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = raStudiosPath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
                Console.WriteLine($"[ASHAT] Launched RaStudios from: {raStudiosPath}");
            }
            else
            {
                Console.WriteLine("[ASHAT] RaStudios executable not found. Please ensure RaStudios is installed in the expected location.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ASHAT] Error launching RaStudios: {ex.Message}");
        }
    }
}

/// <summary>
/// Chat interface for ASHAT
/// </summary>
public class ChatInterface
{
    private readonly AshatBrain _brain;
    private readonly StackPanel _messagesPanel;
    private readonly TextBox _inputBox;

    public ChatInterface(AshatBrain brain)
    {
        _brain = brain;
        _messagesPanel = new StackPanel { Spacing = 10 };
        _inputBox = new TextBox
        {
            Watermark = "Speak to ASHAT...",
            Height = 40
        };
        
        _inputBox.KeyDown += async (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                await SendMessageAsync();
            }
        };

        // Add initial greeting
        AddMessage("Greetings, mortal! I am ASHAT, your divine companion. How may I assist you today? ✨", true);
    }

    public Control GetChatPanel()
    {
        var mainPanel = new StackPanel { Spacing = 10 };

        // Header
        var header = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(200, 75, 0, 130)),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(10, 10, 0, 0),
            Child = new TextBlock
            {
                Text = "✨ Chat with ASHAT ✨",
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Gold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };

        // Messages scroll viewer
        var scrollViewer = new ScrollViewer
        {
            Height = 220,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(230, 20, 0, 40)),
            Content = _messagesPanel
        };

        // Input area
        var inputPanel = new DockPanel
        {
            LastChildFill = true,
            Margin = new Thickness(0, 5, 0, 0)
        };

        var sendButton = new Button
        {
            Content = "Send",
            Width = 70,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(200, 138, 43, 226)),
            Foreground = Avalonia.Media.Brushes.Gold,
            Margin = new Thickness(5, 0, 0, 0)
        };
        sendButton.Click += async (s, e) => await SendMessageAsync();

        DockPanel.SetDock(sendButton, Avalonia.Controls.Dock.Right);
        inputPanel.Children.Add(sendButton);
        inputPanel.Children.Add(_inputBox);

        // Container
        var container = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(240, 20, 0, 40)),
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(150, 138, 43, 226)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10)
        };

        mainPanel.Children.Add(header);
        mainPanel.Children.Add(scrollViewer);
        mainPanel.Children.Add(inputPanel);

        container.Child = mainPanel;
        return container;
    }

    private async Task SendMessageAsync()
    {
        var message = _inputBox.Text?.Trim();
        if (string.IsNullOrEmpty(message)) return;

        // Add user message
        AddMessage(message, false);
        _inputBox.Text = string.Empty;

        // Get ASHAT's response
        var response = await _brain.ProcessMessageAsync(message);
        AddMessage(response, true);
    }

    private void AddMessage(string text, bool isAshat)
    {
        var messageBlock = new TextBlock
        {
            Text = text,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = isAshat ? Avalonia.Media.Brushes.LightGoldenrodYellow : Avalonia.Media.Brushes.LightBlue,
            FontSize = 13,
            Padding = new Thickness(10)
        };

        var messageBorder = new Border
        {
            Background = isAshat 
                ? new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(150, 75, 0, 130))
                : new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(150, 100, 50, 200)),
            BorderBrush = isAshat
                ? new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 255, 215, 0))
                : new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 138, 43, 226)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = messageBlock,
            HorizontalAlignment = isAshat ? Avalonia.Layout.HorizontalAlignment.Left : Avalonia.Layout.HorizontalAlignment.Right,
            MaxWidth = 350,
            Margin = new Thickness(5)
        };

        _messagesPanel.Children.Add(messageBorder);
    }
}
