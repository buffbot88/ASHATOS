using System;
using System.Drawing;
using System.Windows.Forms;
using RaStudios.WinForms.Models;
using RaStudios.WinForms.Modules;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Panel for AI Coding Bot with human-in-the-loop controls.
    /// </summary>
    public class AiAgentPanel : UserControl
    {
        private readonly AiAgent aiAgent;
        private GroupBox promptGroupBox;
        private TextBox promptTextBox;
        private ComboBox languageComboBox;
        private Button generateButton;
        private GroupBox resultGroupBox;
        private RichTextBox codeResultTextBox;
        private Button approveButton;
        private Button rejectButton;
        private TextBox approverNameTextBox;
        private GroupBox statusGroupBox;
        private RichTextBox statusTextBox;
        private CodeGenerationResult? currentResult;

        public AiAgentPanel(AiAgent agent)
        {
            aiAgent = agent ?? throw new ArgumentNullException(nameof(agent));
            InitializeComponent();
            aiAgent.OnStatusUpdate += OnStatusUpdate;
            aiAgent.OnCodeGenerated += OnCodeGenerated;
            aiAgent.OnError += OnError;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Prompt group box
            promptGroupBox = new GroupBox
            {
                Text = "Code Generation Prompt",
                Location = new Point(10, 10),
                Size = new Size(500, 150),
                Padding = new Padding(10)
            };

            var promptLabel = new Label
            {
                Text = "Describe what code you need:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            promptTextBox = new TextBox
            {
                Location = new Point(10, 45),
                Size = new Size(470, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var languageLabel = new Label
            {
                Text = "Language:",
                Location = new Point(10, 110),
                AutoSize = true
            };

            languageComboBox = new ComboBox
            {
                Location = new Point(80, 107),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            languageComboBox.Items.AddRange(new object[] { "csharp", "python", "javascript", "sql", "html" });
            languageComboBox.SelectedIndex = 0;

            generateButton = new Button
            {
                Text = "Generate Code",
                Location = new Point(350, 107),
                Size = new Size(130, 30)
            };
            generateButton.Click += OnGenerateClick;

            promptGroupBox.Controls.AddRange(new Control[]
            {
                promptLabel, promptTextBox, languageLabel, languageComboBox, generateButton
            });

            // Result group box
            resultGroupBox = new GroupBox
            {
                Text = "Generated Code (Sandboxed - Not Executed)",
                Location = new Point(10, 170),
                Size = new Size(500, 300),
                Padding = new Padding(10)
            };

            codeResultTextBox = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(470, 200),
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                BackColor = Color.WhiteSmoke
            };

            var approverLabel = new Label
            {
                Text = "Approver Name:",
                Location = new Point(10, 235),
                AutoSize = true
            };

            approverNameTextBox = new TextBox
            {
                Location = new Point(110, 232),
                Size = new Size(150, 23)
            };

            approveButton = new Button
            {
                Text = "Approve & Deploy",
                Location = new Point(270, 230),
                Size = new Size(100, 30),
                Enabled = false,
                BackColor = Color.LightGreen
            };
            approveButton.Click += OnApproveClick;

            rejectButton = new Button
            {
                Text = "Reject",
                Location = new Point(380, 230),
                Size = new Size(100, 30),
                Enabled = false,
                BackColor = Color.LightCoral
            };
            rejectButton.Click += OnRejectClick;

            var warningLabel = new Label
            {
                Text = "⚠️ Code requires explicit human approval before deployment",
                Location = new Point(10, 265),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            resultGroupBox.Controls.AddRange(new Control[]
            {
                codeResultTextBox, approverLabel, approverNameTextBox, approveButton, rejectButton, warningLabel
            });

            // Status group box
            statusGroupBox = new GroupBox
            {
                Text = "Status & Logs",
                Location = new Point(10, 480),
                Size = new Size(500, 150),
                Padding = new Padding(10)
            };

            statusTextBox = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(470, 110),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 8F)
            };

            statusGroupBox.Controls.Add(statusTextBox);

            this.Controls.AddRange(new Control[]
            {
                promptGroupBox, resultGroupBox, statusGroupBox
            });

            this.ResumeLayout(false);

            AppendStatus("AI Coding Bot ready. Enter a prompt to generate code.");
            AppendStatus("⚠️ All generated code requires human approval before deployment.");
        }

        private async void OnGenerateClick(object? sender, EventArgs e)
        {
            var prompt = promptTextBox.Text.Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                MessageBox.Show("Please enter a prompt describing the code you need.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            generateButton.Enabled = false;
            codeResultTextBox.Clear();
            currentResult = null;

            try
            {
                var language = languageComboBox.SelectedItem?.ToString() ?? "csharp";
                var result = await aiAgent.GenerateCodeAsync(prompt, language);

                if (result != null)
                {
                    currentResult = result;
                    codeResultTextBox.Text = result.GeneratedCode;
                    approveButton.Enabled = true;
                    rejectButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Code generation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                generateButton.Enabled = true;
            }
        }

        private async void OnApproveClick(object? sender, EventArgs e)
        {
            if (currentResult == null)
                return;

            var approverName = approverNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(approverName))
            {
                MessageBox.Show("Please enter your name to approve this code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                "Are you sure you want to approve and deploy this code?\n\n" +
                "This action will send the code to the game server.\n" +
                "Please review the code carefully before confirming.",
                "Confirm Code Approval",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                var approved = aiAgent.ApproveCode(currentResult, approverName);

                if (approved)
                {
                    var deployed = await aiAgent.DeployCodeAsync(currentResult, confirmedByHuman: true);

                    if (deployed)
                    {
                        MessageBox.Show("Code approved and deployed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Code deployment failed. Check logs for details.", "Deployment Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Approval error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRejectClick(object? sender, EventArgs e)
        {
            ResetForm();
            AppendStatus("Code generation rejected by user.");
        }

        private void ResetForm()
        {
            codeResultTextBox.Clear();
            currentResult = null;
            approveButton.Enabled = false;
            rejectButton.Enabled = false;
            approverNameTextBox.Clear();
        }

        private void OnStatusUpdate(object? sender, string message)
        {
            AppendStatus($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private void OnCodeGenerated(object? sender, CodeGenerationResult result)
        {
            AppendStatus($"[{DateTime.Now:HH:mm:ss}] Code generated. ID: {result.Id}");
        }

        private void OnError(object? sender, string message)
        {
            AppendStatus($"[{DateTime.Now:HH:mm:ss}] ERROR: {message}");
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
                aiAgent.OnStatusUpdate -= OnStatusUpdate;
                aiAgent.OnCodeGenerated -= OnCodeGenerated;
                aiAgent.OnError -= OnError;
            }
            base.Dispose(disposing);
        }
    }
}
