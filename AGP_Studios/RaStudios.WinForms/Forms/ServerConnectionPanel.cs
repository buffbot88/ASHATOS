using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Panel for managing server connection with authentication controls.
    /// </summary>
    public class ServerConnectionPanel : UserControl
    {
        private readonly ServerConnector serverConnector;
        private GroupBox connectionGroupBox;
        private TextBox serverUrlTextBox;
        private Button connectButton;
        private Button disconnectButton;
        private GroupBox authGroupBox;
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button authenticateButton;
        private RichTextBox statusTextBox;

        public ServerConnectionPanel(ServerConnector connector)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            InitializeComponent();
            serverConnector.ConnectionStatusChanged += OnConnectionStatusChanged;
            serverConnector.MessageReceived += OnMessageReceived;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Connection group box
            connectionGroupBox = new GroupBox
            {
                Text = "Server Connection",
                Location = new Point(10, 10),
                Size = new Size(400, 120),
                Padding = new Padding(10)
            };

            var urlLabel = new Label
            {
                Text = "Server URL:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            serverUrlTextBox = new TextBox
            {
                Text = serverConnector.ServerUrl,
                Location = new Point(10, 45),
                Size = new Size(370, 23)
            };

            connectButton = new Button
            {
                Text = "Connect",
                Location = new Point(10, 80),
                Size = new Size(180, 30)
            };
            connectButton.Click += OnConnectClick;

            disconnectButton = new Button
            {
                Text = "Disconnect",
                Location = new Point(200, 80),
                Size = new Size(180, 30),
                Enabled = false
            };
            disconnectButton.Click += OnDisconnectClick;

            connectionGroupBox.Controls.AddRange(new Control[]
            {
                urlLabel, serverUrlTextBox, connectButton, disconnectButton
            });

            // Authentication group box
            authGroupBox = new GroupBox
            {
                Text = "Authentication",
                Location = new Point(10, 140),
                Size = new Size(400, 150),
                Padding = new Padding(10)
            };

            var usernameLabel = new Label
            {
                Text = "Username:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            usernameTextBox = new TextBox
            {
                Location = new Point(10, 45),
                Size = new Size(370, 23)
            };

            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(10, 75),
                AutoSize = true
            };

            passwordTextBox = new TextBox
            {
                Location = new Point(10, 95),
                Size = new Size(370, 23),
                UseSystemPasswordChar = true
            };

            authenticateButton = new Button
            {
                Text = "Authenticate",
                Location = new Point(10, 115),
                Size = new Size(370, 30),
                Enabled = false
            };
            authenticateButton.Click += OnAuthenticateClick;

            authGroupBox.Controls.AddRange(new Control[]
            {
                usernameLabel, usernameTextBox, passwordLabel, passwordTextBox, authenticateButton
            });

            // Status text box
            var statusLabel = new Label
            {
                Text = "Connection Status:",
                Location = new Point(10, 300),
                AutoSize = true
            };

            statusTextBox = new RichTextBox
            {
                Location = new Point(10, 320),
                Size = new Size(400, 200),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9F)
            };

            this.Controls.AddRange(new Control[]
            {
                connectionGroupBox, authGroupBox, statusLabel, statusTextBox
            });

            this.ResumeLayout(false);

            AppendStatus("Ready to connect to game server.");
        }

        private async void OnConnectClick(object? sender, EventArgs e)
        {
            try
            {
                serverConnector.ServerUrl = serverUrlTextBox.Text;
                await serverConnector.ConnectAsync();
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                authenticateButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnDisconnectClick(object? sender, EventArgs e)
        {
            await serverConnector.DisconnectAsync();
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
            authenticateButton.Enabled = false;
        }

        private async void OnAuthenticateClick(object? sender, EventArgs e)
        {
            var username = usernameTextBox.Text.Trim();
            var password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var success = await serverConnector.AuthenticateAsync(username, password);

            if (success)
            {
                MessageBox.Show("Authentication successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                passwordTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Authentication failed. Please check your credentials.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e)
        {
            AppendStatus($"[{DateTime.Now:HH:mm:ss}] {e.Status}: {e.Message}");
        }

        private void OnMessageReceived(object? sender, string message)
        {
            AppendStatus($"[{DateTime.Now:HH:mm:ss}] Received: {message}");
        }

        private void AppendStatus(string text)
        {
            if (statusTextBox.InvokeRequired)
            {
                statusTextBox.Invoke(new Action(() => AppendStatus(text)));
                return;
            }

            statusTextBox.AppendText(text + "\n");
            statusTextBox.ScrollToCaret();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                serverConnector.ConnectionStatusChanged -= OnConnectionStatusChanged;
                serverConnector.MessageReceived -= OnMessageReceived;
            }
            base.Dispose(disposing);
        }
    }
}
