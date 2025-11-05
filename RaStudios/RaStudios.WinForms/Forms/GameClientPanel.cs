using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Services;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Game Client panel for playing games built with RaStudios.
    /// Provides embedded game view and game controls.
    /// </summary>
    public class GameClientPanel : UserControl
    {
        private readonly ServerConnector serverConnector;
        private Panel gameViewPanel;
        private Panel controlPanel;
        private ListBox gamesListBox;
        private Button launchGameButton;
        private Button stopGameButton;
        private Label gameStatusLabel;
        private TextBox gameConsoleTextBox;
        private Label userInfoLabel;

        private string currentGame = "";
        private bool isGameRunning = false;

        public GameClientPanel(ServerConnector connector)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            InitializeComponent();
            LoadGamesList();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Control panel (left side)
            controlPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(44, 62, 80)
            };

            // User info label
            userInfoLabel = new Label
            {
                Text = "Select a game to play",
                Location = new Point(10, 10),
                Size = new Size(280, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            controlPanel.Controls.Add(userInfoLabel);

            // Games list label
            var gamesLabel = new Label
            {
                Text = "Available Games:",
                Location = new Point(10, 50),
                Size = new Size(280, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            controlPanel.Controls.Add(gamesLabel);

            // Games list box
            gamesListBox = new ListBox
            {
                Location = new Point(10, 80),
                Size = new Size(280, 400),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gamesListBox.SelectedIndexChanged += OnGameSelected;
            controlPanel.Controls.Add(gamesListBox);

            // Launch game button
            launchGameButton = new Button
            {
                Text = "Launch Game",
                Location = new Point(10, 490),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            launchGameButton.FlatAppearance.BorderSize = 0;
            launchGameButton.Click += OnLaunchGameClick;
            controlPanel.Controls.Add(launchGameButton);

            // Stop game button
            stopGameButton = new Button
            {
                Text = "Stop",
                Location = new Point(150, 490),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            stopGameButton.FlatAppearance.BorderSize = 0;
            stopGameButton.Click += OnStopGameClick;
            controlPanel.Controls.Add(stopGameButton);

            // Game status label
            gameStatusLabel = new Label
            {
                Text = "Status: Ready",
                Location = new Point(10, 535),
                Size = new Size(280, 25),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9)
            };
            controlPanel.Controls.Add(gameStatusLabel);

            // Game view panel (center)
            gameViewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            // Welcome label in game view
            var welcomeLabel = new Label
            {
                Text = "🎮 Game View\n\nSelect and launch a game to start playing!",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16),
                TextAlign = ContentAlignment.MiddleCenter
            };
            gameViewPanel.Controls.Add(welcomeLabel);

            // Game console (bottom)
            var consolePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var consoleLabel = new Label
            {
                Text = "Game Console:",
                Location = new Point(10, 5),
                Size = new Size(200, 20),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9)
            };
            consolePanel.Controls.Add(consoleLabel);

            gameConsoleTextBox = new TextBox
            {
                Location = new Point(10, 30),
                Size = new Size(consolePanel.Width - 20, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical
            };
            consolePanel.Controls.Add(gameConsoleTextBox);

            gameViewPanel.Controls.Add(consolePanel);

            // Add panels to main control
            this.Controls.Add(gameViewPanel);
            this.Controls.Add(controlPanel);

            this.ResumeLayout();
        }

        private void LoadGamesList()
        {
            // In a real implementation, this would query the server for available games
            gamesListBox.Items.Clear();
            gamesListBox.Items.AddRange(new object[]
            {
                "🎯 Space Shooter Demo",
                "🏃 Platformer Adventure",
                "🧩 Puzzle Quest",
                "🏎️ Racing Challenge",
                "⚔️ RPG Quest",
                "🏀 Sports Arena",
                "🎲 Board Game Collection",
                "🎨 Creative Sandbox"
            });

            LogToConsole("Game list loaded. Select a game to play.");
        }

        private void OnGameSelected(object? sender, EventArgs e)
        {
            if (gamesListBox.SelectedItem != null)
            {
                currentGame = gamesListBox.SelectedItem.ToString() ?? "";
                launchGameButton.Enabled = !isGameRunning;
                userInfoLabel.Text = $"Selected: {currentGame}";
                LogToConsole($"Selected game: {currentGame}");
            }
        }

        private async void OnLaunchGameClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentGame))
                return;

            try
            {
                // Check if connected to server
                if (!serverConnector.IsConnected)
                {
                    var result = MessageBox.Show(
                        "Not connected to game server. Would you like to connect?",
                        "Connection Required",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        LogToConsole("Connecting to game server...");
                        await serverConnector.ConnectAsync();
                    }
                    else
                    {
                        return;
                    }
                }

                // Launch the game
                isGameRunning = true;
                launchGameButton.Enabled = false;
                stopGameButton.Enabled = true;
                gameStatusLabel.Text = "Status: Running";
                gameStatusLabel.ForeColor = Color.LimeGreen;

                LogToConsole($"Launching game: {currentGame}");
                LogToConsole("Game engine initializing...");

                // Simulate game launch (in real implementation, this would start the actual game)
                await Task.Delay(1000);
                LogToConsole("Game started successfully!");
                LogToConsole("Press 'Stop' to exit the game.");

                // Send launch command to server
                var launchMessage = new
                {
                    type = "launch_game",
                    game = currentGame,
                    timestamp = DateTime.UtcNow
                };

                await serverConnector.SendMessageAsync(
                    Newtonsoft.Json.JsonConvert.SerializeObject(launchMessage));

                LogService.Instance.LogInfo("GameClient", $"Launched game: {currentGame}");
            }
            catch (Exception ex)
            {
                isGameRunning = false;
                launchGameButton.Enabled = true;
                stopGameButton.Enabled = false;
                gameStatusLabel.Text = "Status: Error";
                gameStatusLabel.ForeColor = Color.Red;

                LogToConsole($"Error launching game: {ex.Message}");
                LogService.Instance.LogError("GameClient", $"Game launch error: {ex.Message}", ex);

                MessageBox.Show(
                    $"Failed to launch game: {ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnStopGameClick(object? sender, EventArgs e)
        {
            try
            {
                isGameRunning = false;
                launchGameButton.Enabled = true;
                stopGameButton.Enabled = false;
                gameStatusLabel.Text = "Status: Stopped";
                gameStatusLabel.ForeColor = Color.Gray;

                LogToConsole($"Stopping game: {currentGame}");
                LogToConsole("Game stopped.");

                LogService.Instance.LogInfo("GameClient", $"Stopped game: {currentGame}");
            }
            catch (Exception ex)
            {
                LogToConsole($"Error stopping game: {ex.Message}");
                LogService.Instance.LogError("GameClient", $"Game stop error: {ex.Message}", ex);
            }
        }

        private void LogToConsole(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogToConsole(message)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            gameConsoleTextBox.AppendText($"[{timestamp}] {message}\r\n");
        }

        public void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                userInfoLabel.ForeColor = Color.LimeGreen;
            }
            else
            {
                userInfoLabel.ForeColor = Color.White;
                if (isGameRunning)
                {
                    OnStopGameClick(null, EventArgs.Empty);
                    MessageBox.Show(
                        "Connection to server lost. Game stopped.",
                        "Connection Lost",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }
    }
}
