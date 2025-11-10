using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Modules;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Sandboxed code preview panel. Code is never auto-executed.
    /// </summary>
    public class CodePreviewPanel : UserControl
    {
        private readonly AiAgent aiAgent;
        private RichTextBox codePreviewTextBox;
        private Button refreshButton;
        private Label warningLabel;
        private CheckBox syntaxHighlightCheckBox;

        public CodePreviewPanel(AiAgent agent)
        {
            aiAgent = agent ?? throw new ArgumentNullException(nameof(agent));
            InitializeComponent();
            aiAgent.OnCodeGenerated += OnCodeGenerated;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Warning banner
            var bannerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Yellow,
                Padding = new Padding(10)
            };

            warningLabel = new Label
            {
                Text = "⚠️ SANDBOXED CODE PREVIEW - NOT EXECUTED ⚠️\n" +
                       "This area displays generated code for review only. Code will NOT be executed automatically.",
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkRed,
                Font = new Font(this.Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            bannerPanel.Controls.Add(warningLabel);

            // Toolbar
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            refreshButton = new Button
            {
                Text = "Refresh Preview",
                Location = new Point(5, 5),
                Size = new Size(120, 30)
            };
            refreshButton.Click += OnRefreshClick;

            syntaxHighlightCheckBox = new CheckBox
            {
                Text = "Enable Syntax Highlighting",
                Location = new Point(135, 10),
                AutoSize = true,
                Checked = false
            };
            syntaxHighlightCheckBox.CheckedChanged += OnSyntaxHighlightChanged;

            toolbarPanel.Controls.AddRange(new Control[]
            {
                refreshButton, syntaxHighlightCheckBox
            });

            // Code preview text box
            codePreviewTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BackColor = Color.WhiteSmoke,
                WordWrap = false
            };

            this.Controls.Add(codePreviewTextBox);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(bannerPanel);

            this.ResumeLayout(false);

            codePreviewTextBox.AppendText("// Sandboxed code preview area\n");
            codePreviewTextBox.AppendText("// Generated code will appear here for review\n");
            codePreviewTextBox.AppendText("// Code is NOT executed automatically for security\n");
        }

        private void OnCodeGenerated(object? sender, Models.CodeGenerationResult result)
        {
            if (codePreviewTextBox.InvokeRequired)
            {
                codePreviewTextBox.Invoke(new Action(() => OnCodeGenerated(sender, result)));
                return;
            }

            codePreviewTextBox.Clear();
            codePreviewTextBox.AppendText($"// Generated Code - ID: {result.Id}\n");
            codePreviewTextBox.AppendText($"// Language: {result.Language}\n");
            codePreviewTextBox.AppendText($"// Generated: {result.GeneratedAt}\n");
            codePreviewTextBox.AppendText($"// Status: {(result.IsApproved ? "APPROVED" : "PENDING APPROVAL")}\n");
            codePreviewTextBox.AppendText("// ==========================================\n\n");
            codePreviewTextBox.AppendText(result.GeneratedCode);
        }

        private void OnRefreshClick(object? sender, EventArgs e)
        {
            // Refresh implementation
        }

        private void OnSyntaxHighlightChanged(object? sender, EventArgs e)
        {
            // Basic syntax highlighting toggle
            // Full implementation would use a proper syntax highlighting library
            if (syntaxHighlightCheckBox.Checked)
            {
                codePreviewTextBox.BackColor = Color.FromArgb(30, 30, 30);
                codePreviewTextBox.ForeColor = Color.White;
            }
            else
            {
                codePreviewTextBox.BackColor = Color.WhiteSmoke;
                codePreviewTextBox.ForeColor = Color.Black;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                aiAgent.OnCodeGenerated -= OnCodeGenerated;
            }
            base.Dispose(disposing);
        }
    }
}
