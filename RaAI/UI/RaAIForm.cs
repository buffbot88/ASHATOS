using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RaAI.Handlers;
using RaAI.Modules;
using RaAI.Modules.DigitalFace;

namespace RaAI.UI
{
    public partial class RaAIForm : Form, IDisposable
    {
        // Fix: Mark fields as nullable to resolve CS8618 diagnostics.
        private DigitalFaceControl? faceControl;
        private RichTextBox? ioTextArea;
        private TextBox? inputField;
        private Button? submitButton;
        private readonly ListBox? historyList;
        private CheckedListBox? moduleList;
        private ModuleManager? moduleManager;

        public RaAIForm()
        {
            SetupTheme();
            CreateLayout();
            InitializeDigitalFace();
        }

        private void SetupTheme()
        {
            BackColor = ThemeManager.PanelDark;
            ForeColor = ThemeManager.Text;
        }

        // Update the call to CreateLeftPanel in CreateLayout to use the correct type.

        private void CreateLayout()
        {
            // Create main container
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Left panel setup
            var leftPanel = CreateLeftPanel(); // Now returns System.Windows.Forms.Panel
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Right panel setup
            var rightPanel = CreateRightPanel();
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // Column and row styles
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Controls.Add(mainPanel);
        }

        private Panel CreateLeftPanel()
        {
            var panel = UIComponents.CreatePanel(DockStyle.Fill, ThemeManager.PanelDark);

            // Digital face component
            faceControl = new DigitalFaceControl
            {
                Dock = DockStyle.Top
            };
            panel.Controls.Add(faceControl);

            // Module controls
            var moduleControls = CreateModuleControls();
            panel.Controls.Add(moduleControls);

            return panel;
        }

        private Panel CreateModuleControls()
        {
            var panel = UIComponents.CreatePanel(DockStyle.Fill, ThemeManager.PanelDark);

            // Module list header
            var header = UIComponents.CreateLabel("Modules", 300, ContentAlignment.MiddleCenter);
            ThemeManager.StyleLabel(header);
            panel.Controls.Add(header);

            // Module list
            moduleList = UIComponents.CreateCheckedListBox();
            ThemeManager.StyleCheckedListBox(moduleList);
            panel.Controls.Add(moduleList);

            // Module help text
            var helpText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Width = 300,
                Dock = DockStyle.Bottom,
                BackColor = ThemeManager.PanelDark,
                ForeColor = ThemeManager.Text,
                ScrollBars = ScrollBars.Vertical,
                Font = ThemeManager.Monospace(9)
            };
            ThemeManager.StyleTextBox(helpText);
            panel.Controls.Add(helpText);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = UIComponents.CreatePanel(DockStyle.Fill, ThemeManager.PanelMid);

            // IO area
            ioTextArea = UIComponents.CreateRichTextBox();
            ThemeManager.StyleRichTextBox(ioTextArea);
            panel.Controls.Add(ioTextArea);

            // Input row
            var inputRow = CreateInputRow();
            panel.Controls.Add(inputRow);

            return panel;
        }

        private Panel CreateInputRow()
        {
            var panel = UIComponents.CreatePanel(DockStyle.Bottom, ThemeManager.PanelDark);

            // Input field
            inputField = UIComponents.CreateTextBox();
            ThemeManager.StyleTextBox(inputField);
            inputField.KeyDown += TxtInput_KeyDown;
            panel.Controls.Add(inputField);

            // Submit button
            submitButton = UIComponents.CreateButton("Submit", 100);
            ThemeManager.StyleButton(submitButton);
            submitButton.Click += BtnSend_Click;
            panel.Controls.Add(submitButton);

            return panel;
        }

        // Fix for CA1861: Use static readonly array for candidateNames in SendCurrentInputAsync
        private static readonly string[] SpeechCandidateNames = ["Speech", "Language"];
        private static readonly string[] ConsciousCandidateNames = ["Conscious", "ConsciousModule", "Mind"];
        private static readonly string[] candidateNamesArray0 = ["Subconscious", "SubconsciousModule"];
        private static readonly string[] candidateNamesArray1 = ["Memory", "MemoryModule"];

        private TextBox? GetInputField()
        {
            return inputField;
        }

        // In SendCurrentInputAsync, fix CS8604 by checking for null before calling UIHelper.AppendLog
        private async Task SendCurrentInputAsync(TextBox? inputField)
        {
            if (inputField == null) return;
            var input = inputField.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            // Add to history
            if (historyList != null)
            {
                historyList.Items.Insert(0, input);
                while (historyList.Items.Count > 20) historyList.Items.RemoveAt(historyList.Items.Count - 1);
            }

            if (moduleManager != null)
            {
                try
                {
                    // Speech
                    var speechRes = await moduleManager.InvokeModuleProcessByNameFallbackAsync(
                        SpeechCandidateNames, input, 2000);
                    if (!string.IsNullOrEmpty(speechRes))
                        ioTextArea?.AppendText($"Speech: {speechRes}{Environment.NewLine}");

                    // Conscious
                    var consciousRes = await moduleManager.InvokeModuleProcessByNameFallbackAsync(
                        ConsciousCandidateNames, input, 2000);
                    if (!string.IsNullOrEmpty(consciousRes))
                        ioTextArea?.AppendText($"Conscious: {consciousRes}{Environment.NewLine}");

                    // Subconscious
                    var subconsciousRes = await moduleManager.InvokeModuleProcessByNameFallbackAsync(
                        new[] { "Subconscious", "SubconsciousModule" }, input, 2000);
                    if (!string.IsNullOrEmpty(subconsciousRes))
                        ioTextArea?.AppendText($"Subconscious: {subconsciousRes}{Environment.NewLine}");

                    // Memory
                    var memoryRes = await moduleManager.InvokeModuleProcessByNameFallbackAsync(
                        new[] { "Memory", "MemoryModule" }, input, 2000);
                    if (!string.IsNullOrEmpty(memoryRes))
                        ioTextArea?.AppendText($"Memory: {memoryRes}{Environment.NewLine}");

                    faceControl?.SetMood(DigitalFaceControl.Mood.Thinking);
                    inputField.Clear();
                }
                catch (Exception ex)
                {
                    ioTextArea?.AppendText($"System: (routing error: {ex.Message}){Environment.NewLine}");
                    faceControl?.SetMood(DigitalFaceControl.Mood.Confused);
                }
            }

            return;
        }

        private void InitializeDigitalFace()
        {
            // Fix CS8602: Check for null before dereferencing faceControl
            faceControl?.SetMood(DigitalFaceControl.Mood.Neutral);
        }

        public void SetModuleManager(ModuleManager mgr)
        {
            moduleManager = mgr ?? throw new ArgumentNullException(nameof(mgr));
            LoadModuleList();
        }

        private void LoadModuleList()
        {
            var txtHelp = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Width = 300,
                Dock = DockStyle.Bottom,
                BackColor = ThemeManager.PanelDark,
                ForeColor = ThemeManager.Text,
                ScrollBars = ScrollBars.Vertical,
                Font = ThemeManager.Monospace(9)
            };

            // Fix: Check for null before dereferencing moduleList
            moduleList?.Items.Clear();
            txtHelp.Clear();

            if (moduleManager == null) return;

            try
            {
                // Use the Modules property instead of GetAvailableModules
                var modules = moduleManager.Modules;
                if (moduleList != null)
                {
                    foreach (var module in modules)
                    {
                        moduleList.Items.Add(module.Name, true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fix: Check for null before passing ioTextArea to UIHelper.AppendLog
                if (ioTextArea != null)
                {
                    UIHelper.AppendLog(ioTextArea, "System", "LoadModuleList", $"error: {ex.Message}");
                }
            }
        }

                // Update the TxtInput_KeyDown method signature to explicitly allow 'sender' to be nullable.
        private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                _ = SendCurrentInputAsync(GetInputField());
            }
        }

        private async Task DispatchToCheckedModulesAsync(string input)
        {
            if (moduleManager == null) return;

            // Fix CS8604: Check for null before passing moduleList to UIHelper.GetSelectedModule
            var selectedModule = moduleList != null ? UIHelper.GetSelectedModule(moduleList) : string.Empty;
            if (string.IsNullOrEmpty(selectedModule)) return;

            try
            {
                string? res = null;
                try
                {
                    res = await moduleManager.SafeInvokeModuleByNameAsync(selectedModule, input, 2000);
                }
                catch (MissingMethodException)
                {
                    res = moduleManager.SafeInvokeModuleByName(selectedModule, input);
                }
                catch
                {
                    res = moduleManager.SafeInvokeModuleByName(selectedModule, input);
                }

                if (string.IsNullOrEmpty(res))
                {
                    res = "(no response)";
                }

                // Fix CS8604: Check for null before passing ioTextArea to UIHelper.AppendLog
                if (ioTextArea != null)
                {
                    UIHelper.AppendLog(ioTextArea, selectedModule, input, res);
                }
            }
            catch (Exception ex)
            {
                if (ioTextArea != null)
                {
                    UIHelper.AppendLog(ioTextArea, "System", input, $"(invoke error: {ex.Message})");
                }
            }
        }

        // Change the BtnSend_Click method signature to accept nullable sender parameter to match EventHandler delegate.
        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            await SendCurrentInputAsync(GetInputField());
        }

        public void Initialize(ModuleManager manager)
        {
            throw new NotImplementedException();
        }

        public string Process(string input)
        {
            throw new NotImplementedException();
        }

        // Fix for CS8602: Check for null before dereferencing ioTextArea in LogOutput
        private void LogOutput(string message)
        {
            ioTextArea?.AppendText(message + Environment.NewLine);
        }
    }
}