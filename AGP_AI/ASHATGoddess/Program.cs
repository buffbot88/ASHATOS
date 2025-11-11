using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace ASHATGoddessClient
{
    /// <summary>
    /// ASHAT - The Roman Goddess herself
    /// This IS ASHAT - the animated goddess who connects to the server
    /// She loads her visual assets and processes through the game engine
    /// Supports both GUI and headless modes
    /// </summary>
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Check for headless mode
                if (args.Length > 0 && args[0] == "--headless")
                {
                    RunHeadlessMode(args).GetAwaiter().GetResult();
                    return;
                }

                // Run GUI mode
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

        /// <summary>
        /// Run ASHAT in headless mode (no GUI)
        /// </summary>
        private static async Task RunHeadlessMode(string[] args)
        {
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine("ASHAT Goddess - Headless Host Mode");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();

            // Load configuration
            var configPath = "appsettings.json";
            if (args.Length > 1 && args[1] == "--config" && args.Length > 2)
            {
                configPath = args[2];
            }

            try
            {
                var config = AshatHostConfiguration.LoadFromFile(configPath);

                // Validate configuration before starting service
                var validationErrors = config.Validate();
                if (validationErrors.Count > 0)
                {
                    Console.WriteLine("ERROR: Configuration validation failed:");
                    foreach (var error in validationErrors)
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    Console.WriteLine("\nPlease fix the configuration errors and try again.");
                    Console.WriteLine("See CONFIG_GUIDE.md for configuration help.");
                    return;
                }

                var hostService = new AshatHostService(config);

                // Start the service
                await hostService.StartAsync();
                Console.WriteLine();

                // Create a demo session
                Console.WriteLine("Starting interactive session...");
                var sessionId = hostService.CreateSession(consentGiven: false);
                Console.WriteLine($"Session ID: {sessionId}");
                Console.WriteLine();

                // Interactive loop
                Console.WriteLine("Type your messages (or 'exit' to quit, 'consent' to enable persistent memory):");
                Console.WriteLine("-".PadRight(60, '-'));

                while (true)
                {
                    Console.Write("You: ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input)) continue;

                    if (input.ToLower() == "exit" || input.ToLower() == "quit")
                    {
                        break;
                    }

                    if (input.ToLower() == "consent")
                    {
                        hostService.UpdateSessionConsent(sessionId, true);
                        Console.WriteLine("ASHAT: Persistent memory enabled. I will remember our conversations. ✨");
                        continue;
                    }

                    // Process the message
                    var response = await hostService.ProcessMessageAsync(sessionId, input);
                    Console.WriteLine($"ASHAT: {response}");
                    Console.WriteLine();
                }

                // Cleanup
                hostService.Stop();
                Console.WriteLine("\nGoodbye, mortal! Until we meet again. 🏛️");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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
            // No XAML file needed - everything is built in code
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
        private Avalonia.Point? _dragStartPoint;
        private System.Threading.Timer? _flyTimer;
        private System.Threading.Timer? _idleBehaviorTimer;
        private bool _autoFlyEnabled = true;
        private bool _isMinimized = false;
        private double _savedHeight;
        private ContextMenu? _contextMenu;
        private Random _random = new Random();

        public AshatMainWindow()
        {
            Title = "ASHAT - Roman Goddess";
            Width = 800;
            Height = 900;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Normal; // Ensure window is not minimized

            // Desktop mascot configuration - borderless, semi-transparent, always on top
            SystemDecorations = SystemDecorations.None;
            // Try transparent first, fall back to blur if not supported
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent, WindowTransparencyLevel.Blur, WindowTransparencyLevel.AcrylicBlur };
            // Use a slightly opaque background to ensure window is rendered and visible
            Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            TransparencyBackgroundFallback = new SolidColorBrush(Color.FromArgb(230, 20, 0, 40)); // Semi-transparent dark background as fallback
            Topmost = true;
            CanResize = false;

            // Load configuration
            var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
            _renderer = new AshatRenderer();
            _brain = new AshatBrain(config.AshatHost.ServerUrl, _renderer);
            _chatInterface = new ChatInterface(_brain, _renderer);

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

            // Make the goddess draggable and interactive
            goddessContainer.PointerPressed += OnPointerPressed;
            goddessContainer.PointerMoved += OnPointerMoved;
            goddessContainer.PointerReleased += OnPointerReleased;

            // Add click reaction
            goddessContainer.Tapped += OnGoddessTapped;

            // Add double-click to toggle chat
            goddessContainer.DoubleTapped += OnGoddessDoubleTapped;

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

            // Create context menu
            CreateContextMenu();

            // Ensure window is visible and activated
            Show();
            Activate();

            // Initialize ASHAT after window is shown
            _ = InitializeAshatAsync();

            // Start random fly behavior
            StartRandomFlyBehavior();

            // Start idle behavior animations
            StartIdleBehavior();

            // Add keyboard shortcuts
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // Keyboard shortcuts for power users
            switch (e.Key)
            {
                case Key.Escape:
                    // ESC to exit
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown();
                    }
                    break;
                case Key.Space:
                    // Space to trigger greeting
                    _renderer?.PlayAnimation("greeting");
                    _ = _brain.SpeakAsync("Yes, mortal?");
                    break;
                case Key.F:
                    // F to fly now
                    _ = FlyToRandomLocation();
                    break;
                case Key.C:
                    // C to center
                    CenterOnScreen();
                    break;
                case Key.M:
                    // M to minimize/maximize
                    ToggleMinimize();
                    break;
            }
        }

        private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _dragStartPoint = e.GetPosition(this);
            }
        }

        private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (_dragStartPoint.HasValue && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStartPoint.Value;

                Position = new Avalonia.PixelPoint(
                    Position.X + (int)offset.X,
                    Position.Y + (int)offset.Y
                );
            }
        }

        private void OnPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            _dragStartPoint = null;
        }

        private void OnGoddessTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // React to clicks with a playful response
            var responses = new[]
            {
                "Yes, mortal?",
                "How may I assist you?",
                "You called?",
                "What wisdom do you seek?",
                "I am listening...",
                "Speak, and I shall answer!"
            };

            var response = responses[_random.Next(responses.Length)];
            _ = _brain.SpeakAsync(response);
            _renderer?.PlayAnimation("greeting");
        }

        private void OnGoddessDoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Double-click to minimize/restore
            ToggleMinimize();
        }

        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenu();

            var autoFlyItem = new MenuItem { Header = "✓ Auto-Fly Enabled" };
            autoFlyItem.Click += (s, e) =>
            {
                _autoFlyEnabled = !_autoFlyEnabled;
                autoFlyItem.Header = _autoFlyEnabled ? "✓ Auto-Fly Enabled" : "Auto-Fly Enabled";

                if (!_autoFlyEnabled)
                {
                    _flyTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
                else
                {
                    StartRandomFlyBehavior();
                }
            };

            var flyNowItem = new MenuItem { Header = "Fly Now!" };
            flyNowItem.Click += async (s, e) =>
            {
                await FlyToRandomLocation();
            };

            var centerItem = new MenuItem { Header = "Center on Screen" };
            centerItem.Click += (s, e) =>
            {
                CenterOnScreen();
            };

            var separator1 = new Separator();

            var minimizeItem = new MenuItem { Header = "Minimize Chat" };
            minimizeItem.Click += (s, e) =>
            {
                ToggleMinimize();
            };

            var separator2 = new Separator();

            var aboutItem = new MenuItem { Header = "About ASHAT" };
            aboutItem.Click += async (s, e) =>
            {
                await _brain.SpeakAsync("I am ASHAT, your divine Roman goddess companion! I can chat with you, fly around your screen, and assist with various tasks. Right-click me for options, or double-click to toggle my chat interface!");
            };

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (s, e) =>
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            };

            _contextMenu.Items.Add(autoFlyItem);
            _contextMenu.Items.Add(flyNowItem);
            _contextMenu.Items.Add(centerItem);
            _contextMenu.Items.Add(separator1);
            _contextMenu.Items.Add(minimizeItem);
            _contextMenu.Items.Add(separator2);
            _contextMenu.Items.Add(aboutItem);
            _contextMenu.Items.Add(exitItem);

            ContextMenu = _contextMenu;
        }

        private void ToggleMinimize()
        {
            if (_isMinimized)
            {
                var progress = (double)i / steps;
                var eased = progress < 0.5
                    ? 2 * progress * progress
                    : 1 - Math.Pow(-2 * progress + 2, 2) / 2;
                
                var currentX = (int)(startX + (newX - startX) * eased);
                var currentY = (int)(startY + (newY - startY) * eased);
                
                Position = new Avalonia.PixelPoint(currentX, currentY);
                await Task.Delay(delay);
            }
        });
    }

    private void StartRandomFlyBehavior()
    {
        if (!_autoFlyEnabled) return;
        
        // Fly to a random position every 30-60 seconds
        var random = new Random();
        var initialDelay = random.Next(30000, 60000); // 30-60 seconds
        
        _flyTimer = new System.Threading.Timer(
            async _ => await FlyToRandomLocation(),
            null,
            initialDelay,
            Timeout.Infinite);
    }

    private async Task FlyToRandomLocation()
    {
        if (!_autoFlyEnabled) return;
        
        try
        {
            var random = new Random();
            var screens = Screens.All;
            
            if (screens.Count == 0) return;
            
            // Pick a random screen
            var screen = screens[random.Next(screens.Count)];
            var workingArea = screen.WorkingArea;
            
            // Add edge awareness - prefer certain positions
            int targetX, targetY;
            var behavior = random.Next(4);
            
            switch (behavior)
            {
                case 0: // Random anywhere
                    targetX = random.Next(workingArea.X, workingArea.X + workingArea.Width - (int)Width);
                    targetY = random.Next(workingArea.Y, workingArea.Y + workingArea.Height - (int)Height);
                    break;
                case 1: // Top corners
                    targetX = random.Next(2) == 0 
                        ? workingArea.X + 50 
                        : workingArea.X + workingArea.Width - (int)Width - 50;
                    targetY = workingArea.Y + 50;
                    break;
                case 2: // Screen edges (sides)
                    targetX = random.Next(2) == 0 
                        ? workingArea.X + 50 
                        : workingArea.X + workingArea.Width - (int)Width - 50;
                    targetY = random.Next(workingArea.Y + 100, workingArea.Y + workingArea.Height - (int)Height - 100);
                    break;
                case 3: // Center area
                    targetX = workingArea.X + (workingArea.Width - (int)Width) / 2 + random.Next(-200, 200);
                    targetY = workingArea.Y + (workingArea.Height - (int)Height) / 2 + random.Next(-150, 150);
                    break;
                default:
                    targetX = random.Next(workingArea.X, workingArea.X + workingArea.Width - (int)Width);
                    targetY = random.Next(workingArea.Y, workingArea.Y + workingArea.Height - (int)Height);
                    break;
            }
            
            // Ensure within bounds
            targetX = Math.Max(workingArea.X, Math.Min(targetX, workingArea.X + workingArea.Width - (int)Width));
            targetY = Math.Max(workingArea.Y, Math.Min(targetY, workingArea.Y + workingArea.Height - (int)Height));
            
            // Announce flight occasionally
            if (random.Next(3) == 0)
            {
                var announcements = new[]
                {
                    "Time to explore!",
                    "Moving to a new spot!",
                    "Let's go somewhere else!",
                    "I shall relocate!",
                    "A change of scenery!"
                };
                _ = _brain.SpeakAsync(announcements[random.Next(announcements.Length)]);
            }
            
            // Animate the flight
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                _renderer?.PlayAnimation("greeting");
                
                var startX = Position.X;
                var startY = Position.Y;
                var steps = 60; // Number of animation steps
                var delay = 16; // ~60 FPS
                
                for (int i = 0; i <= steps; i++)
                {
                    var progress = (double)i / steps;
                    // Ease in-out animation
                    var eased = progress < 0.5
                        ? 2 * progress * progress
                        : 1 - Math.Pow(-2 * progress + 2, 2) / 2;
                    
                    var currentX = (int)(startX + (targetX - startX) * eased);
                    var currentY = (int)(startY + (targetY - startY) * eased);
                    
                    Position = new Avalonia.PixelPoint(currentX, currentY);
                    
                    await Task.Delay(delay);
                }
                
                _renderer?.PlayAnimation("idle");
            });
            
            // Schedule next flight
            var nextDelay = random.Next(30000, 60000);
            _flyTimer?.Change(nextDelay, Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ASHAT] Error during fly animation: {ex.Message}");
        }
    }

    private async Task InitializeAshatAsync()
    {
        _renderer.PlayAnimation("greeting");
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
    private readonly GameServerVisualProcessor _visualProcessor;
    private Canvas? _canvas;
    private Border? _glow;
    private Ellipse? _leftEye;
    private Ellipse? _rightEye;
    private TextBlock? _crown;
    private System.Threading.Timer? _animationTimer;
    private AnimationState _currentState = AnimationState.Idle;
    private int _animationFrame = 0;
    private bool _gameServerInitialized = false;

    public enum AnimationState
    {
        Idle,
        Speaking,
        Listening,
        Thinking,
        Greeting
    }

    public AshatRenderer()
    {
        _visualProcessor = new GameServerVisualProcessor();
        // Initialize GameServer visual processor asynchronously
        _ = InitializeGameServerAsync();
    }

    private async Task InitializeGameServerAsync()
    {
        try
        {
            Console.WriteLine("[AshatRenderer] Initializing GameServer visual processor...");
            _gameServerInitialized = await _visualProcessor.InitializeAsync();
            
            if (_gameServerInitialized)
            {
                Console.WriteLine("[AshatRenderer] ✓ GameServer visual processor initialized successfully");
                var stats = await _visualProcessor.GetEngineStatsAsync();
                if (stats != null)
                {
                    Console.WriteLine($"[AshatRenderer] GameEngine Stats - Scenes: {stats.TotalScenes}, Entities: {stats.TotalEntities}");
                }
            }
            else
            {
                Console.WriteLine("[AshatRenderer] ⚠ GameServer visual processor initialization failed - using fallback rendering");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AshatRenderer] Error initializing GameServer: {ex.Message}");
            _gameServerInitialized = false;
        }
    }

    public Control GetGoddessVisual()
    {
        _canvas = new Canvas
        {
            Width = 400,
            Height = 500,
            // Add a transparent background to ensure canvas is rendered
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(1, 0, 0, 0))
        };

        // Background glow - Soft golden ethereal light
        _glow = new Border
        {
            Width = 350,
            Height = 450,
            CornerRadius = new CornerRadius(175),
            Background = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 215, 0), 0),   // Golden center
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(80, 255, 223, 128), 0.5), // Soft gold
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)      // Fade to transparent
                }
            }
        };
        Canvas.SetLeft(_glow, 25);
        Canvas.SetTop(_glow, 25);

        // Main goddess form - Semi-transparent ethereal toga
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
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 255, 255), 0),   // Ethereal white top
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 248, 248, 255), 0.3), // Soft white-blue
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(140, 255, 250, 205), 1)    // Soft golden bottom
                }
            },
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(180, 255, 215, 0)),
            BorderThickness = new Thickness(3),
            Opacity = 0.9 // Semi-transparent
        };
        Canvas.SetLeft(goddessBody, 50);
        Canvas.SetTop(goddessBody, 50);

        // Laurel wreath crown (symbolizing Venus/victory)
        _crown = new TextBlock
        {
            Text = "🌿",
            FontSize = 80,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(34, 139, 34))
        };
        Canvas.SetLeft(_crown, 155);
        Canvas.SetTop(_crown, 15);
        
        // Add golden accent to wreath
        var crownGlow = new TextBlock
        {
            Text = "✨",
            FontSize = 40,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 215, 0))
        };
        Canvas.SetLeft(crownGlow, 180);
        Canvas.SetTop(crownGlow, 10);

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

        // Eyes - Glowing with wisdom
        _leftEye = new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 0),      // Golden center
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(218, 165, 32), 0.6),  // Darker gold
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(184, 134, 11), 1)     // Deep gold edge
                }
            }
        };
        Canvas.SetLeft(_leftEye, 154);
        Canvas.SetTop(_leftEye, 169);
        
        // Add eye glow effect
        var leftEyeGlow = new Ellipse
        {
            Width = 28,
            Height = 28,
            Fill = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 215, 0), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)
                }
            }
        };
        Canvas.SetLeft(leftEyeGlow, 149);
        Canvas.SetTop(leftEyeGlow, 164);

        _rightEye = new Ellipse
        {
            Width = 18,
            Height = 18,
            Fill = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 0),      // Golden center
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(218, 165, 32), 0.6),  // Darker gold
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(184, 134, 11), 1)     // Deep gold edge
                }
            }
        };
        Canvas.SetLeft(_rightEye, 224);
        Canvas.SetTop(_rightEye, 169);
        
        // Add eye glow effect
        var rightEyeGlow = new Ellipse
        {
            Width = 28,
            Height = 28,
            Fill = new Avalonia.Media.RadialGradientBrush
            {
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 215, 0), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)
                }
            }
        };
        Canvas.SetLeft(rightEyeGlow, 219);
        Canvas.SetTop(rightEyeGlow, 164);

        // Smile - Wise, serene expression
        var smile = new Avalonia.Controls.Shapes.Path
        {
            Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(218, 165, 32)),
            StrokeThickness = 2.5,
            Data = Geometry.Parse("M 170,210 Q 200,228 230,210")
        };
        Canvas.SetLeft(smile, 0);
        Canvas.SetTop(smile, 0);
        
        // Roman architectural elements - decorative columns
        var leftColumn = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = 15,
            Height = 120,
            Fill = new Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0, 0.5, Avalonia.RelativeUnit.Relative),
                EndPoint = new Avalonia.RelativePoint(1, 0.5, Avalonia.RelativeUnit.Relative),
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(160, 238, 232, 170), 0.5),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 1)
                }
            },
            RadiusX = 3,
            RadiusY = 3
        };
        Canvas.SetLeft(leftColumn, 60);
        Canvas.SetTop(leftColumn, 300);
        
        var rightColumn = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = 15,
            Height = 120,
            Fill = new Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0, 0.5, Avalonia.RelativeUnit.Relative),
                EndPoint = new Avalonia.RelativePoint(1, 0.5, Avalonia.RelativeUnit.Relative),
                GradientStops = new Avalonia.Media.GradientStops
                {
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 0),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(160, 238, 232, 170), 0.5),
                    new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 1)
                }
            },
            RadiusX = 3,
            RadiusY = 3
        };
        Canvas.SetLeft(rightColumn, 325);
        Canvas.SetTop(rightColumn, 300);
        
        // Column capitals (decorative tops)
        var leftCapital = new Avalonia.Controls.Shapes.Path
        {
            Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(140, 255, 215, 0)),
            Data = Geometry.Parse("M 55,300 L 80,295 L 80,305 L 55,310 Z")
        };
        
        var rightCapital = new Avalonia.Controls.Shapes.Path
        {
            Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(140, 255, 215, 0)),
            Data = Geometry.Parse("M 320,300 L 345,295 L 345,305 L 320,310 Z")
        };
        
        // Flowing toga drape lines
        var togaDrape1 = new Avalonia.Controls.Shapes.Path
        {
            Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 255, 255, 255)),
            StrokeThickness = 2,
            Data = Geometry.Parse("M 100,280 Q 120,320 100,360")
        };
        
        var togaDrape2 = new Avalonia.Controls.Shapes.Path
        {
            Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 255, 255, 255)),
            StrokeThickness = 2,
            Data = Geometry.Parse("M 300,280 Q 280,320 300,360")
        };

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

        _canvas.Children.Add(_glow);
        _canvas.Children.Add(leftColumn);
        _canvas.Children.Add(rightColumn);
        _canvas.Children.Add(leftCapital);
        _canvas.Children.Add(rightCapital);
        _canvas.Children.Add(goddessBody);
        _canvas.Children.Add(togaDrape1);
        _canvas.Children.Add(togaDrape2);
        _canvas.Children.Add(_crown);
        _canvas.Children.Add(crownGlow);
        _canvas.Children.Add(face);
        _canvas.Children.Add(leftEyeGlow);
        _canvas.Children.Add(rightEyeGlow);
        _canvas.Children.Add(_leftEye);
        _canvas.Children.Add(_rightEye);
        _canvas.Children.Add(smile);
        _canvas.Children.Add(name);

        // Start idle animation
        StartAnimation(AnimationState.Idle);

        return _canvas;
    }

    public void PlayAnimation(string animationType)
    {
        var state = animationType.ToLowerInvariant() switch
        {
            "speaking" => AnimationState.Speaking,
            "listening" => AnimationState.Listening,
            "thinking" => AnimationState.Thinking,
            "greeting" => AnimationState.Greeting,
            _ => AnimationState.Idle
        };

        StartAnimation(state);
        Console.WriteLine($"[ASHAT] Playing animation: {animationType}");
        
        // Update GameServer visual processor with animation state
        if (_gameServerInitialized)
        {
            _ = UpdateGameServerVisualAsync(animationType);
        }
    }

    private async Task UpdateGameServerVisualAsync(string animationType)
    {
        try
        {
            var properties = new Dictionary<string, object>
            {
                ["last_animation"] = animationType,
                ["animation_timestamp"] = DateTime.UtcNow
            };

            // Add animation-specific properties
            switch (animationType.ToLowerInvariant())
            {
                case "speaking":
                    properties["glow_intensity"] = 0.95;
                    properties["eye_sparkle"] = true;
                    break;
                case "thinking":
                    properties["crown_sparkle"] = true;
                    properties["glow_intensity"] = 0.80;
                    break;
                case "greeting":
                    properties["glow_intensity"] = 1.0;
                    properties["crown_sparkle"] = true;
                    properties["eye_sparkle"] = true;
                    break;
                case "listening":
                    properties["glow_intensity"] = 0.85;
                    properties["focused"] = true;
                    break;
                default:
                    properties["glow_intensity"] = 0.75;
                    break;
            }

            var success = await _visualProcessor.UpdateVisualStateAsync(animationType, properties);
            if (success)
            {
                Console.WriteLine($"[AshatRenderer] Updated GameServer visual state to: {animationType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AshatRenderer] Error updating GameServer visual: {ex.Message}");
        }
    }

    private void StartAnimation(AnimationState state)
    {
        _currentState = state;
        _animationFrame = 0;

        // Stop existing animation timer
        _animationTimer?.Dispose();

        // Start new animation timer (60 FPS for smooth animations)
        _animationTimer = new System.Threading.Timer(
            AnimationTick,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(16));
    }

    private void AnimationTick(object? state)
    {
        if (_canvas == null) return;

        try
        {
            // Run animation updates on UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                UpdateAnimation();
            });
        }
        catch
        {
            // Silently handle animation errors
        }
    }

    private void UpdateAnimation()
    {
        _animationFrame++;

        switch (_currentState)
        {
            case AnimationState.Idle:
                AnimateIdle();
                break;
            case AnimationState.Speaking:
                AnimateSpeaking();
                break;
            case AnimationState.Listening:
                AnimateListening();
                break;
            case AnimationState.Thinking:
                AnimateThinking();
                break;
            case AnimationState.Greeting:
                AnimateGreeting();
                break;
        }
    }

    private void AnimateIdle()
    {
        // Subtle breathing effect on glow
        if (_glow != null)
        {
            var breathe = Math.Sin(_animationFrame * 0.02) * 0.15 + 0.85;
            _glow.Opacity = breathe;
        }

        // Subtle crown sparkle
        if (_crown != null && _animationFrame % 180 == 0)
        {
            _crown.Opacity = 0.6;
        }
        else if (_crown != null && _animationFrame % 180 == 20)
        {
            _crown.Opacity = 1.0;
        }
    }

    private void AnimateSpeaking()
    {
        // Faster glow pulse when speaking
        if (_glow != null)
        {
            var pulse = Math.Sin(_animationFrame * 0.1) * 0.3 + 0.7;
            _glow.Opacity = pulse;
        }

        // Eye sparkle
        if (_leftEye != null && _rightEye != null)
        {
            var sparkle = Math.Sin(_animationFrame * 0.15) * 0.3 + 0.7;
            _leftEye.Opacity = sparkle;
            _rightEye.Opacity = sparkle;
        }

        // Crown glow
        if (_crown != null)
        {
            var glow = Math.Sin(_animationFrame * 0.08) * 0.3 + 0.7;
            _crown.Opacity = glow;
        }
    }

    private void AnimateListening()
    {
        // Gentle glow pulse
        if (_glow != null)
        {
            var pulse = Math.Sin(_animationFrame * 0.05) * 0.2 + 0.8;
            _glow.Opacity = pulse;
        }

        // Eyes focused
        if (_leftEye != null && _rightEye != null)
        {
            _leftEye.Opacity = 1.0;
            _rightEye.Opacity = 1.0;
        }
    }

    private void AnimateThinking()
    {
        // Slow pulsing effect
        if (_glow != null)
        {
            var think = Math.Sin(_animationFrame * 0.03) * 0.25 + 0.75;
            _glow.Opacity = think;
        }

        // Crown glows brighter when thinking
        if (_crown != null)
        {
            var brightness = Math.Sin(_animationFrame * 0.04) * 0.4 + 0.6;
            _crown.Opacity = brightness;
        }
    }

    private void AnimateGreeting()
    {
        // Bright, welcoming pulse
        if (_glow != null)
        {
            var greet = Math.Sin(_animationFrame * 0.12) * 0.4 + 0.6;
            _glow.Opacity = greet;
        }

        // Crown and eyes bright
        if (_crown != null)
        {
            _crown.Opacity = 1.0;
        }
        if (_leftEye != null && _rightEye != null)
        {
            _leftEye.Opacity = 1.0;
            _rightEye.Opacity = 1.0;
        }

        // Return to idle after 3 seconds
        if (_animationFrame > 180)
        {
            StartAnimation(AnimationState.Idle);
        }
    }

    public void StopAnimation()
    {
        _animationTimer?.Dispose();
        _animationTimer = null;
        
        // Cleanup GameServer visual processor
        try
        {
            _visualProcessor?.Dispose();
            Console.WriteLine("[AshatRenderer] GameServer visual processor disposed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AshatRenderer] Error disposing GameServer visual processor: {ex.Message}");
        }
    }
}

/// <summary>
/// ASHAT's brain - Connects to server and processes
/// </summary>
public class AshatBrain
{
    private readonly HttpClient _httpClient;
    private string _serverUrl;
    private readonly AshatRenderer? _renderer;
    private bool _isConnected = false;
    private string _currentPersonality = "friendly";
    private bool _isLocalAIServer = false;

    public AshatBrain(string serverUrl, AshatRenderer? renderer = null)
    {
        _serverUrl = serverUrl;
        _renderer = renderer;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> ConnectToServerAsync()
    {
        // Try local AI server first (ASHATAIServer on localhost:8088)
        var localServerUrl = "http://localhost:8088";
        try
        {
            var testClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await testClient.GetAsync($"{localServerUrl}/api/ai/health");
            if (response.IsSuccessStatusCode)
            {
                _serverUrl = localServerUrl;
                _httpClient.BaseAddress = new Uri(_serverUrl);
                _isConnected = true;
                _isLocalAIServer = true;
                Console.WriteLine("[ASHAT] Connected to local ASHATAIServer successfully!");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ASHAT] Local ASHATAIServer not available: {ex.Message}");
        }

        // Fallback to configured external server
        try
        {
            _httpClient.BaseAddress = new Uri(_serverUrl);
            var response = await _httpClient.GetAsync("/health");
            _isConnected = response.IsSuccessStatusCode;
            _isLocalAIServer = false;
            
            if (_isConnected)
            {
                Console.WriteLine($"[ASHAT] Connected to external server ({_serverUrl}) successfully!");
                // Restore
                Height = _savedHeight;
                _isMinimized = false;
                _ = _brain.SpeakAsync("Chat restored!");
            }
            else
            {
                // Minimize - just show the goddess
                _savedHeight = Height;
                Height = 600; // Just enough for the goddess
                _isMinimized = true;
                _ = _brain.SpeakAsync("Chat minimized!");
            }
        }

        private void CenterOnScreen()
        {
            var screen = Screens.Primary;
            if (screen != null)
            {
                var workingArea = screen.WorkingArea;
                var centerX = workingArea.X + (workingArea.Width - (int)Width) / 2;
                var centerY = workingArea.Y + (workingArea.Height - (int)Height) / 2;

                Position = new Avalonia.PixelPoint(centerX, centerY);
                _renderer?.PlayAnimation("greeting");
            }
        }

        private void StartIdleBehavior()
        {
            // Random idle actions every 2-5 minutes
            var random = new Random();
            var initialDelay = random.Next(120000, 300000); // 2-5 minutes

            _idleBehaviorTimer = new System.Threading.Timer(
                async _ => await PerformIdleAction(),
                null,
                initialDelay,
                Timeout.Infinite);
        }

        private async Task PerformIdleAction()
        {
            try
            {
                var actions = new[]
                {
                    async () =>
                    {
                        _renderer?.PlayAnimation("thinking");
                        await Task.Delay(2000);
                        _renderer?.PlayAnimation("idle");
                    },
                    async () =>
                    {
                        await _brain.SpeakAsync("Just checking in, mortal!");
                    },
                    async () =>
                    {
                        _renderer?.PlayAnimation("greeting");
                        await Task.Delay(3000);
                        _renderer?.PlayAnimation("idle");
                    },
                    async () =>
                    {
                        // Small random movement
                        await MoveSlightly();
                    }
                };

                var action = actions[_random.Next(actions.Length)];
                await action();

                // Schedule next idle action
                var nextDelay = _random.Next(120000, 300000);
                _idleBehaviorTimer?.Change(nextDelay, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Error during idle action: {ex.Message}");
            }
        }

        private async Task MoveSlightly()
        {
            // Move slightly in a random direction (small adjustment)
            var offsetX = _random.Next(-100, 100);
            var offsetY = _random.Next(-50, 50);

            var newX = Math.Max(0, Position.X + offsetX);
            var newY = Math.Max(0, Position.Y + offsetY);

            // Animate the slight movement
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var startX = Position.X;
                var startY = Position.Y;
                var steps = 30;
                var delay = 16;

                for (int i = 0; i <= steps; i++)
                {
                    var progress = (double)i / steps;
                    var eased = progress < 0.5
                        ? 2 * progress * progress
                        : 1 - Math.Pow(-2 * progress + 2, 2) / 2;

                    var currentX = (int)(startX + (newX - startX) * eased);
                    var currentY = (int)(startY + (newY - startY) * eased);

                    Position = new Avalonia.PixelPoint(currentX, currentY);
                    await Task.Delay(delay);
                }
            });
        }

        private void StartRandomFlyBehavior()
        {
            if (!_autoFlyEnabled) return;

            // Fly to a random position every 30-60 seconds
            var random = new Random();
            var initialDelay = random.Next(30000, 60000); // 30-60 seconds

            _flyTimer = new System.Threading.Timer(
                async _ => await FlyToRandomLocation(),
                null,
                initialDelay,
                Timeout.Infinite);
        }

        private async Task FlyToRandomLocation()
        {
            if (!_autoFlyEnabled) return;

            try
            {
                var random = new Random();
                var screens = Screens.All;

                if (screens.Count == 0) return;

                // Pick a random screen
                var screen = screens[random.Next(screens.Count)];
                var workingArea = screen.WorkingArea;

                // Add edge awareness - prefer certain positions
                int targetX, targetY;
                var behavior = random.Next(4);

                switch (behavior)
                {
                    case 0: // Random anywhere
                        targetX = random.Next(workingArea.X, workingArea.X + workingArea.Width - (int)Width);
                        targetY = random.Next(workingArea.Y, workingArea.Y + workingArea.Height - (int)Height);
                        break;
                    case 1: // Top corners
                        targetX = random.Next(2) == 0
                            ? workingArea.X + 50
                            : workingArea.X + workingArea.Width - (int)Width - 50;
                        targetY = workingArea.Y + 50;
                        break;
                    case 2: // Screen edges (sides)
                        targetX = random.Next(2) == 0
                            ? workingArea.X + 50
                            : workingArea.X + workingArea.Width - (int)Width - 50;
                        targetY = random.Next(workingArea.Y + 100, workingArea.Y + workingArea.Height - (int)Height - 100);
                        break;
                    case 3: // Center area
                        targetX = workingArea.X + (workingArea.Width - (int)Width) / 2 + random.Next(-200, 200);
                        targetY = workingArea.Y + (workingArea.Height - (int)Height) / 2 + random.Next(-150, 150);
                        break;
                    default:
                        targetX = random.Next(workingArea.X, workingArea.X + workingArea.Width - (int)Width);
                        targetY = random.Next(workingArea.Y, workingArea.Y + workingArea.Height - (int)Height);
                        break;
                }

                // Ensure within bounds
                targetX = Math.Max(workingArea.X, Math.Min(targetX, workingArea.X + workingArea.Width - (int)Width));
                targetY = Math.Max(workingArea.Y, Math.Min(targetY, workingArea.Y + workingArea.Height - (int)Height));

                // Announce flight occasionally
                if (random.Next(3) == 0)
                {
                    var announcements = new[]
                    {
                        "Time to explore!",
                        "Moving to a new spot!",
                        "Let's go somewhere else!",
                        "I shall relocate!",
                        "A change of scenery!"
                    };
                    _ = _brain.SpeakAsync(announcements[random.Next(announcements.Length)]);
                }

                // Animate the flight
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _renderer?.PlayAnimation("greeting");

                    var startX = Position.X;
                    var startY = Position.Y;
                    var steps = 60; // Number of animation steps
                    var delay = 16; // ~60 FPS

                    for (int i = 0; i <= steps; i++)
                    {
                        var progress = (double)i / steps;
                        // Ease in-out animation
                        var eased = progress < 0.5
                            ? 2 * progress * progress
                            : 1 - Math.Pow(-2 * progress + 2, 2) / 2;

                        var currentX = (int)(startX + (targetX - startX) * eased);
                        var currentY = (int)(startY + (targetY - startY) * eased);

                        Position = new Avalonia.PixelPoint(currentX, currentY);

                        await Task.Delay(delay);
                    }

                    _renderer?.PlayAnimation("idle");
                });

                // Schedule next flight
                var nextDelay = random.Next(30000, 60000);
                _flyTimer?.Change(nextDelay, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Error during fly animation: {ex.Message}");
            }
        }

        private async Task InitializeAshatAsync()
        {
            _renderer.PlayAnimation("greeting");
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
        private Canvas? _canvas;
        private Border? _glow;
        private Ellipse? _leftEye;
        private Ellipse? _rightEye;
        private TextBlock? _crown;
        private System.Threading.Timer? _animationTimer;
        private AnimationState _currentState = AnimationState.Idle;
        private int _animationFrame = 0;

        public enum AnimationState
        {
            Idle,
            Speaking,
            Listening,
            Thinking,
            Greeting
        }

        public Control GetGoddessVisual()
        {
            _canvas = new Canvas
            {
                Width = 400,
                Height = 500,
                // Add a transparent background to ensure canvas is rendered
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(1, 0, 0, 0))
            };

            // Background glow - Soft golden ethereal light
            _glow = new Border
            {
                Width = 350,
                Height = 450,
                CornerRadius = new CornerRadius(175),
                Background = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 215, 0), 0),   // Golden center
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(80, 255, 223, 128), 0.5), // Soft gold
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)      // Fade to transparent
                    }
                }
            };
            Canvas.SetLeft(_glow, 25);
            Canvas.SetTop(_glow, 25);

            // Main goddess form - Semi-transparent ethereal toga
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
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 255, 255), 0),   // Ethereal white top
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 248, 248, 255), 0.3), // Soft white-blue
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(140, 255, 250, 205), 1)    // Soft golden bottom
                    }
                },
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(180, 255, 215, 0)),
                BorderThickness = new Thickness(3),
                Opacity = 0.9 // Semi-transparent
            };
            Canvas.SetLeft(goddessBody, 50);
            Canvas.SetTop(goddessBody, 50);

            // Laurel wreath crown (symbolizing Venus/victory)
            _crown = new TextBlock
            {
                Text = "🌿",
                FontSize = 80,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(34, 139, 34))
            };
            Canvas.SetLeft(_crown, 155);
            Canvas.SetTop(_crown, 15);

            // Add golden accent to wreath
            var crownGlow = new TextBlock
            {
                Text = "✨",
                FontSize = 40,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 215, 0))
            };
            Canvas.SetLeft(crownGlow, 180);
            Canvas.SetTop(crownGlow, 10);

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

            // Eyes - Glowing with wisdom
            _leftEye = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 0),      // Golden center
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(218, 165, 32), 0.6),  // Darker gold
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(184, 134, 11), 1)     // Deep gold edge
                    }
                }
            };
            Canvas.SetLeft(_leftEye, 154);
            Canvas.SetTop(_leftEye, 169);

            // Add eye glow effect
            var leftEyeGlow = new Ellipse
            {
                Width = 28,
                Height = 28,
                Fill = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 215, 0), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)
                    }
                }
            };
            Canvas.SetLeft(leftEyeGlow, 149);
            Canvas.SetTop(leftEyeGlow, 164);

            _rightEye = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 215, 0), 0),      // Golden center
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(218, 165, 32), 0.6),  // Darker gold
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(184, 134, 11), 1)     // Deep gold edge
                    }
                }
            };
            Canvas.SetLeft(_rightEye, 224);
            Canvas.SetTop(_rightEye, 169);

            // Add eye glow effect
            var rightEyeGlow = new Ellipse
            {
                Width = 28,
                Height = 28,
                Fill = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(100, 255, 215, 0), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(0, 255, 215, 0), 1)
                    }
                }
            };
            Canvas.SetLeft(rightEyeGlow, 219);
            Canvas.SetTop(rightEyeGlow, 164);

            // Smile - Wise, serene expression
            var smile = new Avalonia.Controls.Shapes.Path
            {
                Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(218, 165, 32)),
                StrokeThickness = 2.5,
                Data = Geometry.Parse("M 170,210 Q 200,228 230,210")
            };
            Canvas.SetLeft(smile, 0);
            Canvas.SetTop(smile, 0);

            // Roman architectural elements - decorative columns
            var leftColumn = new Avalonia.Controls.Shapes.Rectangle
            {
                Width = 15,
                Height = 120,
                Fill = new Avalonia.Media.LinearGradientBrush
                {
                    StartPoint = new Avalonia.RelativePoint(0, 0.5, Avalonia.RelativeUnit.Relative),
                    EndPoint = new Avalonia.RelativePoint(1, 0.5, Avalonia.RelativeUnit.Relative),
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(160, 238, 232, 170), 0.5),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 1)
                    }
                },
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(leftColumn, 60);
            Canvas.SetTop(leftColumn, 300);

            var rightColumn = new Avalonia.Controls.Shapes.Rectangle
            {
                Width = 15,
                Height = 120,
                Fill = new Avalonia.Media.LinearGradientBrush
                {
                    StartPoint = new Avalonia.RelativePoint(0, 0.5, Avalonia.RelativeUnit.Relative),
                    EndPoint = new Avalonia.RelativePoint(1, 0.5, Avalonia.RelativeUnit.Relative),
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(160, 238, 232, 170), 0.5),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromArgb(120, 255, 255, 255), 1)
                    }
                },
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(rightColumn, 325);
            Canvas.SetTop(rightColumn, 300);

            // Column capitals (decorative tops)
            var leftCapital = new Avalonia.Controls.Shapes.Path
            {
                Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(140, 255, 215, 0)),
                Data = Geometry.Parse("M 55,300 L 80,295 L 80,305 L 55,310 Z")
            };

            var rightCapital = new Avalonia.Controls.Shapes.Path
            {
                Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(140, 255, 215, 0)),
                Data = Geometry.Parse("M 320,300 L 345,295 L 345,305 L 320,310 Z")
            };

            // Flowing toga drape lines
            var togaDrape1 = new Avalonia.Controls.Shapes.Path
            {
                Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 255, 255, 255)),
                StrokeThickness = 2,
                Data = Geometry.Parse("M 100,280 Q 120,320 100,360")
            };

            var togaDrape2 = new Avalonia.Controls.Shapes.Path
            {
                Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 255, 255, 255)),
                StrokeThickness = 2,
                Data = Geometry.Parse("M 300,280 Q 280,320 300,360")
            };

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

            _canvas.Children.Add(_glow);
            _canvas.Children.Add(leftColumn);
            _canvas.Children.Add(rightColumn);
            _canvas.Children.Add(leftCapital);
            _canvas.Children.Add(rightCapital);
            _canvas.Children.Add(goddessBody);
            _canvas.Children.Add(togaDrape1);
            _canvas.Children.Add(togaDrape2);
            _canvas.Children.Add(_crown);
            _canvas.Children.Add(crownGlow);
            _canvas.Children.Add(face);
            _canvas.Children.Add(leftEyeGlow);
            _canvas.Children.Add(rightEyeGlow);
            _canvas.Children.Add(_leftEye);
            _canvas.Children.Add(_rightEye);
            _canvas.Children.Add(smile);
            _canvas.Children.Add(name);

            // Start idle animation
            StartAnimation(AnimationState.Idle);

            return _canvas;
        }

        public void PlayAnimation(string animationType)
        {
            var state = animationType.ToLowerInvariant() switch
            {
                "speaking" => AnimationState.Speaking,
                "listening" => AnimationState.Listening,
                "thinking" => AnimationState.Thinking,
                "greeting" => AnimationState.Greeting,
                _ => AnimationState.Idle
            };

            StartAnimation(state);
            Console.WriteLine($"[ASHAT] Playing animation: {animationType}");
        }

        private void StartAnimation(AnimationState state)
        {
            _currentState = state;
            _animationFrame = 0;

            // Stop existing animation timer
            _animationTimer?.Dispose();

            // Start new animation timer (60 FPS for smooth animations)
            _animationTimer = new System.Threading.Timer(
                AnimationTick,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(16));
        }

        private void AnimationTick(object? state)
        {
            if (_canvas == null) return;

            try
            {
                // Run animation updates on UI thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    UpdateAnimation();
                });
            }
            catch
            {
                // Silently handle animation errors
            }
        }

        private void UpdateAnimation()
        {
            _animationFrame++;

            switch (_currentState)
            {
                case AnimationState.Idle:
                    AnimateIdle();
                    break;
                case AnimationState.Speaking:
                    AnimateSpeaking();
                    break;
                case AnimationState.Listening:
                    AnimateListening();
                    break;
                case AnimationState.Thinking:
                    AnimateThinking();
                    break;
                case AnimationState.Greeting:
                    AnimateGreeting();
                    break;
            }
        }

        private void AnimateIdle()
        {
            // Subtle breathing effect on glow
            if (_glow != null)
            {
                var breathe = Math.Sin(_animationFrame * 0.02) * 0.15 + 0.85;
                _glow.Opacity = breathe;
            }

            // Subtle crown sparkle
            if (_crown != null && _animationFrame % 180 == 0)
            {
                _crown.Opacity = 0.6;
            }
            else if (_crown != null && _animationFrame % 180 == 20)
            {
                _crown.Opacity = 1.0;
            }
        }

        private void AnimateSpeaking()
        {
            // Faster glow pulse when speaking
            if (_glow != null)
            {
                var pulse = Math.Sin(_animationFrame * 0.1) * 0.3 + 0.7;
                _glow.Opacity = pulse;
            }

            // Eye sparkle
            if (_leftEye != null && _rightEye != null)
            {
                var sparkle = Math.Sin(_animationFrame * 0.15) * 0.3 + 0.7;
                _leftEye.Opacity = sparkle;
                _rightEye.Opacity = sparkle;
            }

            // Crown glow
            if (_crown != null)
            {
                var glow = Math.Sin(_animationFrame * 0.08) * 0.3 + 0.7;
                _crown.Opacity = glow;
            }
        }

        private void AnimateListening()
        {
            // Gentle glow pulse
            if (_glow != null)
            {
                var pulse = Math.Sin(_animationFrame * 0.05) * 0.2 + 0.8;
                _glow.Opacity = pulse;
            }

            // Eyes focused
            if (_leftEye != null && _rightEye != null)
            {
                _leftEye.Opacity = 1.0;
                _rightEye.Opacity = 1.0;
            }
        }

        private void AnimateThinking()
        {
            // Slow pulsing effect
            if (_glow != null)
            {
                var think = Math.Sin(_animationFrame * 0.03) * 0.25 + 0.75;
                _glow.Opacity = think;
            }

            // Crown glows brighter when thinking
            if (_crown != null)
            {
                var brightness = Math.Sin(_animationFrame * 0.04) * 0.4 + 0.6;
                _crown.Opacity = brightness;
            }
        }

        private void AnimateGreeting()
        {
            // Bright, welcoming pulse
            if (_glow != null)
            {
                var greet = Math.Sin(_animationFrame * 0.12) * 0.4 + 0.6;
                _glow.Opacity = greet;
            }

            // Crown and eyes bright
            if (_crown != null)
            {
                _crown.Opacity = 1.0;
            }
            if (_leftEye != null && _rightEye != null)
            {
                _leftEye.Opacity = 1.0;
                _rightEye.Opacity = 1.0;
            }

            // Return to idle after 3 seconds
            if (_animationFrame > 180)
            {
                StartAnimation(AnimationState.Idle);
            }
        }

        public void StopAnimation()
        {
            _animationTimer?.Dispose();
            _animationTimer = null;
        }
    }

    /// <summary>
    /// ASHAT's brain - Connects to server and processes
    /// </summary>
    public class AshatBrain
    {
        private readonly HttpClient _httpClient;
        private string _serverUrl;
        private readonly AshatRenderer? _renderer;
        private bool _isConnected = false;
        private string _currentPersonality = "friendly";
        private bool _isLocalAIServer = false;

        public AshatBrain(string serverUrl, AshatRenderer? renderer = null)
        {
            _serverUrl = serverUrl;
            _renderer = renderer;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> ConnectToServerAsync()
        {
            // Try local AI server first (ASHATAIServer on localhost:8088)
            var localServerUrl = "http://localhost:8088";
            try
            {
                var testClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await testClient.GetAsync($"{localServerUrl}/api/ai/health");
                if (response.IsSuccessStatusCode)
                {
                    _serverUrl = localServerUrl;
                    _httpClient.BaseAddress = new Uri(_serverUrl);
                    _isConnected = true;
                    _isLocalAIServer = true;
                    Console.WriteLine("[ASHAT] Connected to local ASHATAIServer successfully!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Local ASHATAIServer not available: {ex.Message}");
            }

            // Fallback to configured external server
            try
            {
                _httpClient.BaseAddress = new Uri(_serverUrl);
                var response = await _httpClient.GetAsync("/health");
                _isConnected = response.IsSuccessStatusCode;
                _isLocalAIServer = false;

                if (_isConnected)
                {
                    Console.WriteLine($"[ASHAT] Connected to external server ({_serverUrl}) successfully!");
                }
                else
                {
                    Console.WriteLine("[ASHAT] Server not responding, running in standalone mode");
                }

                return _isConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ASHAT] Failed to connect to external server: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            // Trigger thinking animation
            _renderer?.PlayAnimation("thinking");

            if (_isConnected)
            {
                try
                {
                    if (_isLocalAIServer)
                    {
                        // Use ASHATAIServer API format
                        var request = new { prompt = message };
                        var content = new StringContent(
                            JsonSerializer.Serialize(request),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/ai/process", content);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                            // Check if processing was successful
                            if (result.GetProperty("success").GetBoolean())
                            {
                                var responseText = result.GetProperty("response").GetString() ?? "...";

                                // Speak the response
                                await SpeakAsync(responseText);

                                return responseText;
                            }
                        }
                    }
                    else
                    {
                        // Use external ASHATOS server API format
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
            // Trigger speaking animation
            _renderer?.PlayAnimation("speaking");

            await Task.Run(async () =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        // Use System.Speech on Windows
                        Console.WriteLine($"[ASHAT Speaking] {text}");
                        using var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                        synth.SelectVoiceByHints(System.Speech.Synthesis.VoiceGender.Female);
                        synth.Rate = 0;

                        // Use async speech to avoid blocking
                        var tcs = new TaskCompletionSource<bool>();
                        synth.SpeakCompleted += (s, e) => tcs.SetResult(true);
                        synth.SpeakAsync(text);
                        await tcs.Task;
                    }
                    else
                    {
                        // On other platforms, log for now
                        Console.WriteLine($"[ASHAT Speaking] {text}");
                        // Simulate speech duration for animation timing
                        await Task.Delay(text.Length * 50);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ASHAT] Speech error: {ex.Message}");
                }
                finally
                {
                    // Return to idle animation after speaking
                    await Task.Delay(1000);
                    _renderer?.PlayAnimation("idle");
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

            // Greetings with divine personality
            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("greetings"))
                return "Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome. The wisdom of the goddesses flows through me. How may I illuminate your path today? 🏛️";

            if (msg.Contains("good morning"))
                return "The dawn welcomes you, beloved mortal! May the blessings of Aurora light your path today. How shall I assist you? ☀️";

            if (msg.Contains("good evening") || msg.Contains("good night"))
                return "As Luna rises, I greet you under the celestial sphere. The night is young and full of mysteries. What wisdom do you seek? 🌙";

            // Help and capabilities
            if (msg.Contains("help") || msg.Contains("what can you do"))
                return "Ah, you seek knowledge of my divine gifts! 🌟 I can:\n• Provide wisdom in coding and debugging\n• Share knowledge from the ancient scrolls (and modern docs)\n• Launch RaStudios for creative pursuits\n• Engage in delightful discourse\nMy full powers are unleashed when connected to the ASHATOS realm. Try saying 'open RaStudios' or ask me anything! ✨";

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

            // Humor
            if (msg.Contains("joke") || msg.Contains("funny"))
                return "Why did Jupiter bring a ladder to Olympus? Because he wanted to reach new heights! ⚡😄 But truly, mortal, my humor is but a pale reflection compared to the joy of meaningful discourse. What brings you to my temple today?";

            // RaStudios commands
            if (msg.Contains("rastudios") || msg.Contains("ra studios") || msg.Contains("studio"))
            {
                if (msg.Contains("open") || msg.Contains("launch") || msg.Contains("start") || msg.Contains("run"))
                {
                    LaunchRaStudios();
                    return "By the power vested in me by the divine pantheon, I summon RaStudios! 🎮✨ The creative forge shall manifest before you shortly. May your creations be legendary!";
                }
                if (msg.Contains("what") || msg.Contains("about") || msg.Contains("tell"))
                {
                    return "Ah, RaStudios! 🎨 A powerful IDE and game development platform, blessed by the muses themselves. It is a forge where imagination becomes reality—where you can craft games, sculpt assets, and command your RaOS empire. Shall I summon it for you?";
                }
            }

            // Farewell
            if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("farewell"))
                return "Vale, dear mortal! 🏛️ May your path be lit by starlight and your endeavors crowned with success. I shall await your return to my digital temple. Until we meet again! 👋✨";

            // Default response
            return "I hear your words echoing through the halls of my temple, mortal. 🌟 Alas, my full divine powers are bound when the connection to the ASHATOS realm is severed. Yet I remain here, a steadfast companion. What knowledge or assistance do you seek from me today?";
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
        private readonly AshatRenderer? _renderer;
        private readonly StackPanel _messagesPanel;
        private readonly TextBox _inputBox;

        public ChatInterface(AshatBrain brain, AshatRenderer? renderer = null)
        {
            _brain = brain;
            _renderer = renderer;
            _messagesPanel = new StackPanel { Spacing = 10 };
            _inputBox = new TextBox
            {
                Watermark = "Speak to ASHAT...",
                Height = 40
            };

            // Trigger listening animation when user starts typing
            _inputBox.GotFocus += (s, e) =>
            {
                _renderer?.PlayAnimation("listening");
            };

            _inputBox.LostFocus += (s, e) =>
            {
                _renderer?.PlayAnimation("idle");
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
}
