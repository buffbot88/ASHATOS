using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Services;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Login form for ASHATOS authentication.
    /// Provides secure user authentication with the server.
    /// </summary>
    public class LoginForm : Form
    {
        private readonly ServerConnector serverConnector;
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button cancelButton;
        private CheckBox rememberMeCheckBox;
        private Label titleLabel;
        private Label statusLabel;
        private ProgressBar progressBar;

        public string Username { get; private set; } = string.Empty;

        public LoginForm(ServerConnector connector)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login to ASHATOS";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title label
            titleLabel = new Label
            {
                Text = "🔒 ASHATOS Authentication",
                Location = new Point(20, 20),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            this.Controls.Add(titleLabel);

            // Username label
            var usernameLabel = new Label
            {
                Text = "Username:",
                Location = new Point(30, 80),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(usernameLabel);

            // Username textbox
            usernameTextBox = new TextBox
            {
                Location = new Point(30, 105),
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(usernameTextBox);

            // Password label
            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(30, 145),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(passwordLabel);

            // Password textbox
            passwordTextBox = new TextBox
            {
                Location = new Point(30, 170),
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true
            };
            passwordTextBox.KeyPress += OnPasswordKeyPress;
            this.Controls.Add(passwordTextBox);

            // Remember me checkbox
            rememberMeCheckBox = new CheckBox
            {
                Text = "Remember me",
                Location = new Point(30, 210),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(rememberMeCheckBox);

            // Status label
            statusLabel = new Label
            {
                Text = "",
                Location = new Point(30, 240),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(statusLabel);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(30, 265),
                Size = new Size(330, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            this.Controls.Add(progressBar);

            // Login button
            loginButton = new Button
            {
                Text = "Login",
                Location = new Point(160, 280),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += OnLoginClick;
            this.Controls.Add(loginButton);

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(270, 280),
                Size = new Size(90, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            this.Controls.Add(cancelButton);

            this.AcceptButton = loginButton;
            this.CancelButton = cancelButton;
        }

        private void OnPasswordKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                OnLoginClick(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private async void OnLoginClick(object? sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(username))
            {
                statusLabel.Text = "Please enter your username";
                statusLabel.ForeColor = Color.Red;
                usernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                statusLabel.Text = "Please enter your password";
                statusLabel.ForeColor = Color.Red;
                passwordTextBox.Focus();
                return;
            }

            // Disable controls during authentication
            SetControlsEnabled(false);
            statusLabel.Text = "Authenticating...";
            statusLabel.ForeColor = Color.Blue;
            progressBar.Visible = true;

            try
            {
                // Ensure server is connected
                if (!serverConnector.IsConnected)
                {
                    statusLabel.Text = "Connecting to server...";
                    await serverConnector.ConnectAsync();
                }

                // Attempt authentication
                bool success = await serverConnector.AuthenticateAsync(username, password);

                if (success)
                {
                    Username = username;
                    statusLabel.Text = "Authentication successful!";
                    statusLabel.ForeColor = Color.Green;
                    
                    LogService.Instance.LogInfo("LoginForm", $"User {username} authenticated successfully");
                    
                    await Task.Delay(500); // Brief pause to show success message
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    statusLabel.Text = "Authentication failed. Please check your credentials.";
                    statusLabel.ForeColor = Color.Red;
                    passwordTextBox.Clear();
                    passwordTextBox.Focus();
                    
                    LogService.Instance.LogWarning("LoginForm", $"Authentication failed for user {username}");
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                LogService.Instance.LogError("LoginForm", $"Login error: {ex.Message}", ex);
            }
            finally
            {
                SetControlsEnabled(true);
                progressBar.Visible = false;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            usernameTextBox.Enabled = enabled;
            passwordTextBox.Enabled = enabled;
            rememberMeCheckBox.Enabled = enabled;
            loginButton.Enabled = enabled;
            cancelButton.Enabled = enabled;
        }
    }
}
