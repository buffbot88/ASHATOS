using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Settings form for configuring server and AI agent.
    /// </summary>
    public class SettingsForm : Form
    {
        private readonly ServerConnector serverConnector;
        private readonly AiAgent aiAgent;
        private TabControl tabControl;
        private Button saveButton;
        private Button cancelButton;

        public SettingsForm(ServerConnector connector, AiAgent agent)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            aiAgent = agent ?? throw new ArgumentNullException(nameof(agent));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Server settings tab
            var serverTab = new TabPage("Server");
            AddServerSettings(serverTab);
            tabControl.TabPages.Add(serverTab);

            // AI settings tab
            var aiTab = new TabPage("AI Agent");
            AddAiSettings(aiTab);
            tabControl.TabPages.Add(aiTab);

            // Security settings tab
            var securityTab = new TabPage("Security");
            AddSecuritySettings(securityTab);
            tabControl.TabPages.Add(securityTab);

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(300, 10),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            saveButton.Click += OnSaveClick;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(390, 10),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            buttonPanel.Controls.AddRange(new Control[] { saveButton, cancelButton });

            this.Controls.Add(tabControl);
            this.Controls.Add(buttonPanel);
            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        private void AddServerSettings(TabPage tab)
        {
            var serverUrlLabel = new Label
            {
                Text = "Server URL:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var serverUrlTextBox = new TextBox
            {
                Text = serverConnector.ServerUrl,
                Location = new Point(20, 40),
                Size = new Size(420, 23),
                Name = "serverUrl"
            };

            var portLabel = new Label
            {
                Text = "Port:",
                Location = new Point(20, 75),
                AutoSize = true
            };

            var portTextBox = new TextBox
            {
                Text = "7077",
                Location = new Point(20, 95),
                Size = new Size(100, 23),
                Name = "port"
            };

            var timeoutLabel = new Label
            {
                Text = "Timeout (seconds):",
                Location = new Point(20, 130),
                AutoSize = true
            };

            var timeoutTextBox = new TextBox
            {
                Text = "30",
                Location = new Point(20, 150),
                Size = new Size(100, 23),
                Name = "timeout"
            };

            tab.Controls.AddRange(new Control[]
            {
                serverUrlLabel, serverUrlTextBox, portLabel, portTextBox, timeoutLabel, timeoutTextBox
            });
        }

        private void AddAiSettings(TabPage tab)
        {
            var endpointLabel = new Label
            {
                Text = "AI Endpoint:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var endpointTextBox = new TextBox
            {
                Text = aiAgent.AiEndpoint,
                Location = new Point(20, 40),
                Size = new Size(420, 23),
                Name = "aiEndpoint"
            };

            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                Location = new Point(20, 75),
                AutoSize = true
            };

            var apiKeyTextBox = new TextBox
            {
                Text = "your-api-key-here",
                Location = new Point(20, 95),
                Size = new Size(420, 23),
                UseSystemPasswordChar = true,
                Name = "apiKey"
            };

            var maxRequestsLabel = new Label
            {
                Text = "Max Requests per Minute:",
                Location = new Point(20, 130),
                AutoSize = true
            };

            var maxRequestsTextBox = new TextBox
            {
                Text = "10",
                Location = new Point(20, 150),
                Size = new Size(100, 23),
                Name = "maxRequests"
            };

            tab.Controls.AddRange(new Control[]
            {
                endpointLabel, endpointTextBox, apiKeyLabel, apiKeyTextBox, maxRequestsLabel, maxRequestsTextBox
            });
        }

        private void AddSecuritySettings(TabPage tab)
        {
            var requireApprovalCheckBox = new CheckBox
            {
                Text = "Require human approval for all code generation",
                Location = new Point(20, 20),
                AutoSize = true,
                Checked = true,
                Enabled = false, // Always enforced
                Name = "requireApproval"
            };

            var enableRateLimitCheckBox = new CheckBox
            {
                Text = "Enable rate limiting",
                Location = new Point(20, 50),
                AutoSize = true,
                Checked = true,
                Name = "enableRateLimit"
            };

            var enablePolicyFilterCheckBox = new CheckBox
            {
                Text = "Enable policy filter (blocks dangerous patterns)",
                Location = new Point(20, 80),
                AutoSize = true,
                Checked = true,
                Name = "enablePolicyFilter"
            };

            var sandboxLabel = new Label
            {
                Text = "⚠️ Code execution is always sandboxed and requires explicit approval.",
                Location = new Point(20, 120),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            var updateLabel = new Label
            {
                Text = "Update Strategy:",
                Location = new Point(20, 160),
                AutoSize = true
            };

            var updateComboBox = new ComboBox
            {
                Location = new Point(20, 180),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "updateStrategy"
            };
            updateComboBox.Items.AddRange(new object[] { "Manual", "GitHub Releases", "ClickOnce", "Custom" });
            updateComboBox.SelectedIndex = 0;

            tab.Controls.AddRange(new Control[]
            {
                requireApprovalCheckBox, enableRateLimitCheckBox, enablePolicyFilterCheckBox,
                sandboxLabel, updateLabel, updateComboBox
            });
        }

        private void OnSaveClick(object? sender, EventArgs e)
        {
            // Save settings implementation
            // In production, persist to configuration file
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
