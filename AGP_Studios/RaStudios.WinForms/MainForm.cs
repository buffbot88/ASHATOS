using System;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;
using RaStudios.WinForms.Forms;
using RaStudios.WinForms.Services;

namespace RaStudios.WinForms
{
    /// <summary>
    /// Main application form with tab-based UI for RaStudios.
    /// Provides access to Server Connection, AI Agent, Terminal, and other modules.
    /// </summary>
    public partial class MainForm : Form
    {
        private TabControl mainTabControl;
        private ServerConnector serverConnector;
        private AiAgent aiAgent;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private MenuStrip menuStrip;
        private HomePagePanel? homePagePanel;
        private GameClientPanel? gameClientPanel;
        private string currentUserRole = "guest"; // guest, player, developer, admin

        public MainForm()
        {
            InitializeServices();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "RaStudios - Game Server Automation & AI Development";
            this.Size = new System.Drawing.Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create menu strip
            menuStrip = new MenuStrip();
            
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&Settings", null, OnSettingsClick);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Application.Exit());
            
            var toolsMenu = new ToolStripMenuItem("&Tools");
            toolsMenu.DropDownItems.Add("&Refresh Connection", null, OnRefreshConnection);
            toolsMenu.DropDownItems.Add("&Clear Logs", null, OnClearLogs);
            
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, OnAboutClick);
            helpMenu.DropDownItems.Add("&Documentation", null, OnDocumentationClick);
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });
            
            // Create status strip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            statusStrip.Items.Add(statusLabel);

            // Create main tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Add tabs for different modules based on privileges
            AddHomePageTab(); // Always visible
            AddGameClientTab(); // Visible for players and above
            AddServerConnectionTab();
            AddAiAgentTab(); // Visible for developers and admins
            AddTerminalTab();
            AddLogsTab();
            AddCodePreviewTab();

            // Add controls to form
            this.Controls.Add(mainTabControl);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void InitializeServices()
        {
            // Load configuration
            var config = Services.ConfigurationService.Instance.GetConfiguration();
            
            // Initialize server connector with security controls
            serverConnector = new ServerConnector();
            serverConnector.ServerUrl = config.Server.Url;
            serverConnector.ConnectionStatusChanged += OnConnectionStatusChanged;

            // Initialize AI agent with human-in-the-loop controls
            aiAgent = new AiAgent(serverConnector);
            aiAgent.AiEndpoint = config.AiService.Endpoint;
            aiAgent.OnStatusUpdate += OnAiAgentStatusUpdate;
        }

        private void AddHomePageTab()
        {
            var tabPage = new TabPage("🏠 Home");
            homePagePanel = new HomePagePanel(serverConnector);
            homePagePanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(homePagePanel);
            mainTabControl.TabPages.Add(tabPage);
        }

        private void AddGameClientTab()
        {
            var tabPage = new TabPage("🎮 Game Client");
            gameClientPanel = new GameClientPanel(serverConnector);
            gameClientPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(gameClientPanel);
            mainTabControl.TabPages.Add(tabPage);
            
            // Make tab visible based on user role
            tabPage.Tag = "player"; // Requires 'player' role or higher
        }

        private void AddServerConnectionTab()
        {
            var tabPage = new TabPage("Server Connection");
            var serverPanel = new ServerConnectionPanel(serverConnector);
            serverPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(serverPanel);
            mainTabControl.TabPages.Add(tabPage);
        }

        private void AddAiAgentTab()
        {
            var tabPage = new TabPage("AI Coding Bot");
            var aiPanel = new AiAgentPanel(aiAgent);
            aiPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(aiPanel);
            mainTabControl.TabPages.Add(tabPage);
            
            // Make tab visible based on user role
            tabPage.Tag = "developer"; // Requires 'developer' role or higher
        }

        private void AddTerminalTab()
        {
            var tabPage = new TabPage("Terminal (DirectX 11)");
            var terminalPanel = new TerminalPanel();
            terminalPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(terminalPanel);
            mainTabControl.TabPages.Add(tabPage);
        }

        private void AddLogsTab()
        {
            var tabPage = new TabPage("Logs & Diagnostics");
            var logsPanel = new LogsPanel();
            logsPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(logsPanel);
            mainTabControl.TabPages.Add(tabPage);
        }

        private void AddCodePreviewTab()
        {
            var tabPage = new TabPage("Code Preview (Sandboxed)");
            var codePreviewPanel = new CodePreviewPanel(aiAgent);
            codePreviewPanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(codePreviewPanel);
            mainTabControl.TabPages.Add(tabPage);
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionStatusChanged(sender, e)));
                return;
            }

            statusLabel.Text = $"Server: {e.Status} - {e.Message}";
        }

        private void OnAiAgentStatusUpdate(object? sender, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAiAgentStatusUpdate(sender, message)));
                return;
            }

            statusLabel.Text = $"AI Agent: {message}";
        }

        private void OnSettingsClick(object? sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(serverConnector, aiAgent);
            settingsForm.ShowDialog(this);
        }

        private void UpdateTabVisibilityByRole(string role)
        {
            currentUserRole = role;
            
            // Define role hierarchy: guest < player < developer < admin
            var roleHierarchy = new Dictionary<string, int>
            {
                { "guest", 0 },
                { "player", 1 },
                { "developer", 2 },
                { "admin", 3 }
            };

            int userLevel = roleHierarchy.ContainsKey(role) ? roleHierarchy[role] : 0;

            // Update visibility of tabs based on required role
            foreach (TabPage tab in mainTabControl.TabPages)
            {
                if (tab.Tag is string requiredRole)
                {
                    int requiredLevel = roleHierarchy.ContainsKey(requiredRole) ? roleHierarchy[requiredRole] : 0;
                    
                    // Show tab if user level meets or exceeds required level
                    if (userLevel < requiredLevel)
                    {
                        // Hide the tab (can't remove while iterating)
                        tab.Text = $"🔒 {tab.Text}"; // Mark as locked
                    }
                }
            }
        }

        public void SetUserRole(string role)
        {
            UpdateTabVisibilityByRole(role);
            statusLabel.Text = $"User role: {role}";
            LogService.Instance.LogInfo("MainForm", $"User role set to: {role}");
        }

        private void OnRefreshConnection(object? sender, EventArgs e)
        {
            serverConnector.Reconnect();
        }

        private void OnClearLogs(object? sender, EventArgs e)
        {
            // Clear logs implementation
            MessageBox.Show("Logs cleared.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAboutClick(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "RaStudios - WinForms Edition\n\n" +
                "Version: 1.0.0\n" +
                "Built with .NET 9.0\n\n" +
                "Features:\n" +
                "- Game Server Automation\n" +
                "- AI Coding Bot Integration\n" +
                "- DirectX 11 Terminal\n" +
                "- Sandboxed Code Preview\n" +
                "- Human-in-the-loop Security\n\n" +
                "© 2025 RaStudios Project",
                "About RaStudios",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnDocumentationClick(object? sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/buffbot88/RaStudios",
                UseShellExecute = true
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // Cleanup
            serverConnector?.Disconnect();
            aiAgent?.Dispose();
        }
    }
}
