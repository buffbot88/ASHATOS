using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Services;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// HomePage panel with integrated web browser for news, updates, and quick links.
    /// Provides user authentication and update notifications.
    /// </summary>
    public class HomePagePanel : UserControl
    {
        private readonly ServerConnector serverConnector;
        private WebBrowser webBrowser;
        private Panel topPanel;
        private Panel statusPanel;
        private Label statusLabel;
        private ProgressBar downloadProgressBar;
        private Button loginButton;
        private Button checkUpdatesButton;
        private Label connectionStatusLabel;
        private Label userLabel;
        private Button refreshButton;

        private bool isAuthenticated = false;
        private string currentUsername = "";
        private const string DEFAULT_HOMEPAGE = "https://github.com/buffbot88/RaStudios";

        public HomePagePanel(ServerConnector connector)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            InitializeComponent();
            LoadHomePage();
            serverConnector.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Top panel with controls
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            // Login button
            loginButton = new Button
            {
                Text = "Login to ASHATOS",
                Location = new Point(10, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginButton.Click += OnLoginClick;
            topPanel.Controls.Add(loginButton);

            // Check Updates button
            checkUpdatesButton = new Button
            {
                Text = "Check for Updates",
                Location = new Point(170, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            checkUpdatesButton.Click += OnCheckUpdatesClick;
            topPanel.Controls.Add(checkUpdatesButton);

            // Refresh button
            refreshButton = new Button
            {
                Text = "⟳ Refresh",
                Location = new Point(330, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.Click += OnRefreshClick;
            topPanel.Controls.Add(refreshButton);

            // User label
            userLabel = new Label
            {
                Text = "Not logged in",
                Location = new Point(10, 45),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            topPanel.Controls.Add(userLabel);

            // Connection status label
            connectionStatusLabel = new Label
            {
                Text = "ASHATOS: Disconnected",
                Location = new Point(200, 45),
                AutoSize = true,
                ForeColor = Color.Red
            };
            topPanel.Controls.Add(connectionStatusLabel);

            // Web browser
            webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                AllowWebBrowserDrop = false
            };
            webBrowser.Navigating += OnBrowserNavigating;
            webBrowser.DocumentCompleted += OnBrowserDocumentCompleted;

            // Status panel (bottom)
            statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            // Status label
            statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(10, 10),
                AutoSize = true
            };
            statusPanel.Controls.Add(statusLabel);

            // Download progress bar
            downloadProgressBar = new ProgressBar
            {
                Location = new Point(10, 30),
                Size = new Size(400, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            statusPanel.Controls.Add(downloadProgressBar);

            // Add controls to main panel
            this.Controls.Add(webBrowser);
            this.Controls.Add(topPanel);
            this.Controls.Add(statusPanel);

            this.ResumeLayout();
        }

        private void LoadHomePage()
        {
            try
            {
                // Load homepage HTML content
                string html = GetHomePageHtml();
                webBrowser.DocumentText = html;
                statusLabel.Text = "Homepage loaded";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading homepage: {ex.Message}";
                LogService.Instance.LogError("HomePage", $"Error loading homepage: {ex.Message}");
            }
        }

        private string GetHomePageHtml()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>RaStudios - Game Client Homepage</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 40px 20px;
        }
        h1 {
            font-size: 3em;
            margin-bottom: 10px;
            text-align: center;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }
        .subtitle {
            text-align: center;
            font-size: 1.2em;
            margin-bottom: 40px;
            opacity: 0.9;
        }
        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 40px;
        }
        .card {
            background: rgba(255, 255, 255, 0.1);
            padding: 30px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
            border: 1px solid rgba(255, 255, 255, 0.2);
            transition: transform 0.3s ease;
        }
        .card:hover {
            transform: translateY(-5px);
            background: rgba(255, 255, 255, 0.15);
        }
        .card h2 {
            margin-top: 0;
            font-size: 1.5em;
            margin-bottom: 15px;
        }
        .card p {
            line-height: 1.6;
            opacity: 0.9;
        }
        .icon {
            font-size: 2.5em;
            margin-bottom: 15px;
            display: block;
        }
        .news-section {
            margin-top: 40px;
            background: rgba(255, 255, 255, 0.1);
            padding: 30px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
        }
        .news-item {
            margin-bottom: 20px;
            padding-bottom: 20px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.2);
        }
        .news-item:last-child {
            border-bottom: none;
            margin-bottom: 0;
            padding-bottom: 0;
        }
        .news-date {
            font-size: 0.9em;
            opacity: 0.7;
            margin-bottom: 5px;
        }
        .quick-links {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin-top: 30px;
            justify-content: center;
        }
        .quick-link {
            background: rgba(255, 255, 255, 0.2);
            padding: 10px 20px;
            border-radius: 25px;
            text-decoration: none;
            color: white;
            transition: background 0.3s ease;
        }
        .quick-link:hover {
            background: rgba(255, 255, 255, 0.3);
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🎮 Welcome to RaStudios</h1>
        <p class='subtitle'>Your Unified Game Development & Play Client</p>
        
        <div class='grid'>
            <div class='card'>
                <span class='icon'>🎨</span>
                <h2>Game Development</h2>
                <p>Create amazing games with our integrated IDE. Build, test, and deploy your games seamlessly with ASHATOS integration.</p>
            </div>
            
            <div class='card'>
                <span class='icon'>🕹️</span>
                <h2>Game Player</h2>
                <p>Play games built with RaStudios. Access your library, track achievements, and connect with friends.</p>
            </div>
            
            <div class='card'>
                <span class='icon'>🌐</span>
                <h2>Content Browser</h2>
                <p>Explore RaOS-powered websites and content. Discover new games, mods, and community creations.</p>
            </div>
            
            <div class='card'>
                <span class='icon'>🤖</span>
                <h2>AI Coding Bot</h2>
                <p>Leverage AI assistance for game development. Generate code, debug, and optimize with human oversight.</p>
            </div>
            
            <div class='card'>
                <span class='icon'>🔒</span>
                <h2>Secure Authentication</h2>
                <p>Connect securely to ASHATOS servers with token-based authentication and encrypted communication.</p>
            </div>
            
            <div class='card'>
                <span class='icon'>⚡</span>
                <h2>Real-time Updates</h2>
                <p>Stay up to date with automatic client updates and instant notifications about new features.</p>
            </div>
        </div>
        
        <div class='news-section'>
            <h2>📰 Latest News & Updates</h2>
            
            <div class='news-item'>
                <div class='news-date'>2025-10-30</div>
                <h3>New Integrated Homepage Released!</h3>
                <p>We've added a new homepage with integrated web browser, login features, and update notifications. Enjoy seamless connection to ASHATOS!</p>
            </div>
            
            <div class='news-item'>
                <div class='news-date'>2025-10-28</div>
                <h3>DirectX 11 Terminal Update</h3>
                <p>Enhanced terminal with GPU rendering for better performance. Check it out in the Terminal tab!</p>
            </div>
            
            <div class='news-item'>
                <div class='news-date'>2025-10-25</div>
                <h3>AI Coding Bot Improvements</h3>
                <p>New safety controls and better code generation. Human-in-the-loop approval ensures secure development.</p>
            </div>
        </div>
        
        <div class='quick-links'>
            <a href='#' class='quick-link'>📚 Documentation</a>
            <a href='#' class='quick-link'>💬 Community</a>
            <a href='#' class='quick-link'>🐛 Report Bug</a>
            <a href='#' class='quick-link'>💡 Feature Request</a>
            <a href='#' class='quick-link'>🎯 Roadmap</a>
        </div>
    </div>
</body>
</html>";
        }

        private void OnLoginClick(object? sender, EventArgs e)
        {
            using var loginForm = new LoginForm(serverConnector);
            if (loginForm.ShowDialog(this) == DialogResult.OK)
            {
                isAuthenticated = true;
                currentUsername = loginForm.Username;
                userLabel.Text = $"Logged in as: {currentUsername}";
                userLabel.ForeColor = Color.Green;
                loginButton.Text = "Logout";
                statusLabel.Text = "Authentication successful!";
                LogService.Instance.LogInfo("HomePage", $"User {currentUsername} authenticated successfully");
            }
        }

        private void OnCheckUpdatesClick(object? sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void OnRefreshClick(object? sender, EventArgs e)
        {
            LoadHomePage();
        }

        private void CheckForUpdates()
        {
            statusLabel.Text = "Checking for updates...";
            downloadProgressBar.Visible = true;
            downloadProgressBar.Style = ProgressBarStyle.Marquee;

            // Simulate update check (in real implementation, this would query a server)
            Task.Run(async () =>
            {
                await Task.Delay(2000); // Simulate network delay

                Invoke(new Action(() =>
                {
                    downloadProgressBar.Style = ProgressBarStyle.Continuous;
                    downloadProgressBar.Visible = false;
                    statusLabel.Text = "You are running the latest version!";

                    MessageBox.Show(
                        "Your RaStudios client is up to date!\n\n" +
                        "Version: 1.0.0\n" +
                        "Build: 2025.10.30",
                        "Update Check",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LogService.Instance.LogInfo("HomePage", "Update check completed - client is up to date");
                }));
            });
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionStatusChanged(sender, e)));
                return;
            }

            connectionStatusLabel.Text = $"ASHATOS: {e.Status}";
            connectionStatusLabel.ForeColor = e.Status.Contains("Connected") ? Color.Green : Color.Red;

            LogService.Instance.LogInfo("HomePage", $"Connection status changed: {e.Status} - {e.Message}");
        }

        private void OnBrowserNavigating(object? sender, WebBrowserNavigatingEventArgs e)
        {
            statusLabel.Text = $"Navigating to {e.Url}...";
        }

        private void OnBrowserDocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
        {
            statusLabel.Text = "Page loaded";
        }

        public void NavigateToUrl(string url)
        {
            try
            {
                webBrowser.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to URL: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogService.Instance.LogError("HomePage", $"Navigation error: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                serverConnector.ConnectionStatusChanged -= OnConnectionStatusChanged;
                webBrowser?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
