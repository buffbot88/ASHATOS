using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Services;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Panel for displaying logs and diagnostics.
    /// </summary>
    public class LogsPanel : UserControl
    {
        private RichTextBox logsTextBox;
        private ComboBox logLevelComboBox;
        private Button clearButton;
        private CheckBox autoScrollCheckBox;
        private readonly LogService logService;

        public LogsPanel()
        {
            logService = LogService.Instance;
            InitializeComponent();
            logService.LogAdded += OnLogAdded;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Toolbar panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            var logLevelLabel = new Label
            {
                Text = "Log Level:",
                Location = new Point(5, 10),
                AutoSize = true
            };

            logLevelComboBox = new ComboBox
            {
                Location = new Point(75, 7),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            logLevelComboBox.Items.AddRange(new object[] { "All", "Debug", "Info", "Warning", "Error", "Critical" });
            logLevelComboBox.SelectedIndex = 0;
            logLevelComboBox.SelectedIndexChanged += OnLogLevelChanged;

            clearButton = new Button
            {
                Text = "Clear Logs",
                Location = new Point(185, 5),
                Size = new Size(100, 30)
            };
            clearButton.Click += OnClearClick;

            autoScrollCheckBox = new CheckBox
            {
                Text = "Auto-scroll",
                Location = new Point(295, 10),
                AutoSize = true,
                Checked = true
            };

            toolbarPanel.Controls.AddRange(new Control[]
            {
                logLevelLabel, logLevelComboBox, clearButton, autoScrollCheckBox
            });

            // Logs text box
            logsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9F),
                WordWrap = false
            };

            this.Controls.Add(logsTextBox);
            this.Controls.Add(toolbarPanel);

            this.ResumeLayout(false);

            logService.LogInfo("Logs", "Logs panel initialized.");
        }

        private void OnLogLevelChanged(object? sender, EventArgs e)
        {
            // Filter implementation would go here
        }

        private void OnClearClick(object? sender, EventArgs e)
        {
            logsTextBox.Clear();
            logService.LogInfo("Logs", "Logs cleared by user.");
        }

        private void OnLogAdded(object? sender, Models.LogEntry log)
        {
            if (logsTextBox.InvokeRequired)
            {
                logsTextBox.Invoke(new Action(() => OnLogAdded(sender, log)));
                return;
            }

            var logColor = GetLogColor(log.Level);
            var logText = $"[{log.Timestamp:HH:mm:ss}] [{log.Level}] {log.Source}: {log.Message}";

            if (!string.IsNullOrEmpty(log.Exception))
            {
                logText += $"\n    Exception: {log.Exception}";
            }

            logsTextBox.SelectionStart = logsTextBox.TextLength;
            logsTextBox.SelectionLength = 0;
            logsTextBox.SelectionColor = logColor;
            logsTextBox.AppendText(logText + "\n");
            logsTextBox.SelectionColor = logsTextBox.ForeColor;

            if (autoScrollCheckBox.Checked)
            {
                logsTextBox.ScrollToCaret();
            }
        }

        private Color GetLogColor(Models.LogLevel level)
        {
            return level switch
            {
                Models.LogLevel.Debug => Color.Gray,
                Models.LogLevel.Info => Color.LightGreen,
                Models.LogLevel.Warning => Color.Yellow,
                Models.LogLevel.Error => Color.Orange,
                Models.LogLevel.Critical => Color.Red,
                _ => Color.LightGray
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                logService.LogAdded -= OnLogAdded;
            }
            base.Dispose(disposing);
        }
    }
}
