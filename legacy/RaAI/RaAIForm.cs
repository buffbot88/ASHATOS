using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RaAI.Handlers.Manager;
using RaAI.Modules.DigitalFace;

namespace RaAI
{
    public partial class RaAIForm : Form
    {
        // UI
        private readonly Label headerLabel;
        private readonly Button reloadButton;

        // Split: left = (top) left console + (fill) DigitalFaceControl, right = big console + command bar
        private readonly SplitContainer mainSplit;

        // Left-top: module load + error logs (smaller font)
        private readonly RichTextBox leftConsoleBox;

        // Left-bottom: DigitalFace
        private readonly DigitalFaceControl faceControl;

        // Right: console + command bar
        private readonly Panel rightPanel;
        private readonly RichTextBox consoleBox;     // big console for Info handler data (smaller font)
        private readonly Panel commandBar;
        private readonly ComboBox moduleSelect;
        private readonly TextBox inputBox;
        private readonly Button sendButton;

        // State
        private readonly List<string> _history = new();
        private int _historyIndex = -1;
        private ModuleManager? moduleManager;

        // Colors
        private static readonly Color BgColor = Color.Black;
        private static readonly Color FgColor = Color.LimeGreen;
        private static readonly Color InfoColor = Color.LimeGreen;
        private static readonly Color WarnColor = Color.Khaki;
        private static readonly Color ErrorColor = Color.OrangeRed;
        private static readonly Color AccentColor = Color.MediumSpringGreen;

        // Fonts (smaller)
        private static readonly Font ConsoleFontSmall = new Font("Consolas", 10, FontStyle.Regular);
        private static readonly Font ConsoleFontSmallBold = new Font("Consolas", 10, FontStyle.Bold);

        public RaAIForm()
        {
            // Window styling
            BackColor = BgColor;
            ForeColor = FgColor;
            Font = new Font("Consolas", 12, FontStyle.Regular);
            Text = "RaAI — Console";
            DoubleBuffered = true;
            MinimumSize = new Size(1000, 600);

            // Header
            headerLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "RaAI Console",
                ForeColor = FgColor,
                BackColor = BgColor,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Consolas", 16, FontStyle.Bold)
            };
            Controls.Add(headerLabel);

            // Reload button (top-right)
            reloadButton = new Button
            {
                Text = "Reload",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Width = 120,
                Height = 30,
                Top = 5,
                Left = 0, // repositioned on Resize
                FlatStyle = FlatStyle.Flat,
                ForeColor = FgColor,
                BackColor = BgColor
            };
            reloadButton.FlatAppearance.BorderColor = FgColor;
            reloadButton.FlatAppearance.BorderSize = 1;
            reloadButton.Click += async (s, e) => await ReloadAndShowModulesAsync();
            Controls.Add(reloadButton);

            // Main split: Left = (left console + face), Right = console + command bar
            mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 4,
                BackColor = BgColor
            };
            Controls.Add(mainSplit);
            mainSplit.SplitterDistance = 320; // initial left width
            mainSplit.Panel1MinSize = 280;

            // Left panel content
            mainSplit.Panel1.BackColor = BgColor;

            // Left-top: small console (about 70px high) for module load list + error logs
            leftConsoleBox = new RichTextBox
            {
                Dock = DockStyle.Top,
                Height = 70,
                ReadOnly = true,
                BackColor = BgColor,
                ForeColor = FgColor,
                BorderStyle = BorderStyle.None,
                Font = ConsoleFontSmall,
                DetectUrls = false,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.None
            };
            mainSplit.Panel1.Controls.Add(leftConsoleBox);

            // Left-bottom: DigitalFace (fills remaining)
            faceControl = new DigitalFaceControl
            {
                Dock = DockStyle.Fill
            };
            mainSplit.Panel1.Controls.Add(faceControl);

            // Right: Console + command bar
            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgColor
            };
            mainSplit.Panel2.Controls.Add(rightPanel);

            // Right big console (Info handler data)
            consoleBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = BgColor,
                ForeColor = FgColor,
                BorderStyle = BorderStyle.None,
                Font = ConsoleFontSmall, // smaller font
                DetectUrls = false,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            rightPanel.Controls.Add(consoleBox);

            // Bottom command bar
            commandBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = Color.FromArgb(12, 12, 12)
            };
            rightPanel.Controls.Add(commandBar);

            moduleSelect = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                ForeColor = FgColor,
                BackColor = BgColor,
                Width = 220,
                Left = 8,
                Top = 8
            };
            commandBar.Controls.Add(moduleSelect);

            inputBox = new TextBox
            {
                ForeColor = FgColor,
                BackColor = BgColor,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Left = moduleSelect.Right + 8,
                Top = 8,
                Width = 500,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            commandBar.Controls.Add(inputBox);

            sendButton = new Button
            {
                Text = "Send",
                Width = 100,
                Height = 26,
                Left = commandBar.Width - 108,
                Top = 8,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                ForeColor = FgColor,
                BackColor = BgColor
            };
            sendButton.FlatAppearance.BorderColor = FgColor;
            sendButton.FlatAppearance.BorderSize = 1;
            sendButton.Click += async (s, e) => await SendCommandAsync();
            commandBar.Controls.Add(sendButton);

            // Layout
            Resize += (s, e) =>
            {
                reloadButton.Left = ClientSize.Width - reloadButton.Width - 12;
                reloadButton.Top = 6;

                inputBox.Width = commandBar.ClientSize.Width - moduleSelect.Width - sendButton.Width - 8 - 8 - 8 - 8;
                inputBox.Left = moduleSelect.Right + 8;
                sendButton.Left = commandBar.ClientSize.Width - sendButton.Width - 8;
            };

            // Input events
            inputBox.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await SendCommandAsync();
                }
                else if (e.KeyCode == Keys.Up)
                {
                    e.SuppressKeyPress = true;
                    NavigateHistory(-1);
                }
                else if (e.KeyCode == Keys.Down)
                {
                    e.SuppressKeyPress = true;
                    NavigateHistory(1);
                }
            };

            Shown += async (s, e) =>
            {
                if (moduleManager != null)
                {
                    if (moduleManager.Modules.Count == 0)
                    {
                        // Nothing preloaded (fallback to existing reload behavior)
                        await ReloadAndShowModulesAsync();
                    }
                    else
                    {
                        // Already loaded at startup: just update UI and list modules
                        ClearConsoles();

                        // Booting message in right console
                        RouteModuleMessage("System", "Startup", "Booting up...");

                        // Time the warm-up for a nice confirmation message
                        var sw = Stopwatch.StartNew();

                        PopulateModuleSelector();

                        RouteLeft("Load", $"Loaded {moduleManager.Modules.Count} module(s) at startup.");
                        foreach (var w in moduleManager.Modules)
                        {
                            try
                            {
                                var inst = w.Instance;
                                var name = inst?.Name ?? inst?.GetType().Name ?? "UnknownModule";
                                var typeName = inst?.GetType().FullName ?? "(unknown type)";
                                var asm = inst?.GetType().Assembly.GetName();
                                var asmName = asm?.Name ?? "(unknown asm)";
                                var asmVer = asm?.Version?.ToString() ?? "?";

                                RouteLeft($"loaded {name}",
                                    $"type: {typeName}{Environment.NewLine}asm:  {asmName} v{asmVer}");
                            }
                            catch (Exception ex)
                            {
                                RouteLeft("describe module error", ex.Message, isError: true);
                            }
                        }

                        // Wake up Speech (and any other desired modules) so first user input isn't needed
                        await WarmupModulesAsync();

                        sw.Stop();
                        RouteModuleMessage("System", "Startup", $"AI Booted in {sw.ElapsedMilliseconds} ms.");
                    }
                }
                else
                {
                    // Info -> right console
                    RouteModuleMessage("System", "Init", "ModuleManager not set. Waiting...");
                }
            };
        }

        public DigitalFaceControl Face => faceControl;

        public void SetModuleManager(ModuleManager mgr)
        {
            moduleManager = mgr ?? throw new ArgumentNullException(nameof(mgr));
            moduleManager.ModuleNamespacePrefix = "RaAI.Modules";
            // Do not reload here; Program preloaded modules. Shown handler will refresh the UI and warm up.
        }

        private async Task ReloadAndShowModulesAsync()
        {
            if (moduleManager == null)
            {
                RouteLeft("Reload", "No ModuleManager set.");
                return;
            }

            ClearConsoles();

            // Booting message in right console at start of reload
            RouteModuleMessage("System", "Startup", "Booting up...");
            var sw = Stopwatch.StartNew();

            RouteLeft("Scan", "Scanning for modules...");
            try
            {
                var loadResult = moduleManager.ReloadModules(requireAttributeOrNamespace: true);

                // Populate selector
                PopulateModuleSelector();

                if (moduleManager.Modules.Count == 0)
                {
                    RouteLeft("Load", "No modules found.");
                }
                else
                {
                    RouteLeft("Load", $"Loaded {moduleManager.Modules.Count} module(s).");
                    foreach (var w in moduleManager.Modules)
                    {
                        try
                        {
                            var inst = w.Instance;
                            var name = inst?.Name ?? inst?.GetType().Name ?? "UnknownModule";
                            var typeName = inst?.GetType().FullName ?? "(unknown type)";
                            var asm = inst?.GetType().Assembly.GetName();
                            var asmName = asm?.Name ?? "(unknown asm)";
                            var asmVer = asm?.Version?.ToString() ?? "?";

                            // Send module list to left console (load list)
                            RouteLeft($"loaded {name}",
                                $"type: {typeName}{Environment.NewLine}asm:  {asmName} v{asmVer}");
                        }
                        catch (Exception ex)
                        {
                            // Errors -> left console
                            RouteLeft("describe module error", ex.Message, isError: true);
                        }
                    }
                }

                // Discovery errors (if exposed)
                try
                {
                    var errProp = loadResult?.GetType().GetProperty("Errors", BindingFlags.Instance | BindingFlags.Public);
                    if (errProp != null)
                    {
                        var errs = (errProp.GetValue(loadResult) as System.Collections.IEnumerable)?
                                   .Cast<object?>().ToList();
                        if (errs != null && errs.Count > 0)
                        {
                            RouteLeft("Discovery errors",
                                string.Join(Environment.NewLine, errs.Select(e => $"- {e}")),
                                isError: true);
                        }
                    }
                }
                catch
                {
                    // ignore reflective inspection issues
                }
            }
            catch (Exception ex)
            {
                RouteLeft("Reload failed", ex.Message, isError: true);
            }

            // After reload, warm up Speech so it's ready immediately
            await WarmupModulesAsync();

            sw.Stop();
            RouteModuleMessage("System", "Startup", $"AI Booted in {sw.ElapsedMilliseconds} ms.");
        }

        private void PopulateModuleSelector()
        {
            moduleSelect.Items.Clear();

            moduleSelect.Items.Add("(All)");
            moduleSelect.Items.Add("(System)");

            if (moduleManager?.Modules is { Count: > 0 })
            {
                foreach (var w in moduleManager.Modules)
                {
                    var name = w.Instance?.Name ?? w.Instance?.GetType().Name ?? w.Instance?.GetType().FullName ?? "(unknown)";
                    moduleSelect.Items.Add(name);
                }
            }

            var prefer = new[] { "Conscious", "Speech", "Memory", "Subconscious" };
            var selected = prefer.FirstOrDefault(p => moduleSelect.Items.Contains(p)) ?? "(System)";
            moduleSelect.SelectedItem = selected;
        }

        // Warm-up modules so they are ready before first user input
        private async Task WarmupModulesAsync()
        {
            if (moduleManager == null) return;

            // Speech: "status" is supported and exercises dependency resolution
            TryWarmup("Speech", "status");

            // Optionally warm other modules too
            // TryWarmup("Conscious", "think wake up");
            // TryWarmup("Memory", "memory help");

            await Task.CompletedTask;
        }

        private void TryWarmup(string moduleName, string command)
        {
            try
            {
                var response =
                    moduleManager!.SafeInvokeModuleByName(moduleName, command) ??
                    moduleManager!.SafeInvokeModuleByName(moduleName + "Module", command);

                // Show the warm-up interaction in the right console (optional)
                if (!string.IsNullOrWhiteSpace(response))
                    RouteModuleMessage(moduleName, command, response);
                else
                    RouteLeft($"warmup {moduleName}", "(no response)");
            }
            catch (Exception ex)
            {
                RouteLeft($"warmup {moduleName}", ex.Message, isError: true);
            }
        }

        // Right console: Info handler data from modules
        public void RouteModuleMessage(string moduleName, string input, string response)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            var header = $"[{ts}] [{moduleName}] > {input}";

            AppendStyledRight(header, AccentColor, bold: true);

            if (!string.IsNullOrWhiteSpace(response))
            {
                var color = InferColor(response);
                AppendStyledRight(response, color);
            }

            AppendStyledRight(string.Empty, FgColor); // blank line
        }

        // Left console: load list + error handle logs
        private void RouteLeft(string input, string response, bool isError = false)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            var header = $"[{ts}] [System] > {input}";
            AppendStyledLeft(header, AccentColor, bold: true);

            if (!string.IsNullOrWhiteSpace(response))
            {
                AppendStyledLeft(response, isError ? ErrorColor : InfoColor);
            }

            AppendStyledLeft(string.Empty, FgColor);
        }

        private Color InferColor(string text)
        {
            if (string.IsNullOrEmpty(text)) return InfoColor;

            var t = text.TrimStart();

            if (t.StartsWith("[error]", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("(error", StringComparison.OrdinalIgnoreCase) ||
                t.IndexOf("exception", StringComparison.OrdinalIgnoreCase) >= 0 ||
                t.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0)
                return ErrorColor;

            if (t.StartsWith("[warn", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("(warn", StringComparison.OrdinalIgnoreCase) ||
                t.IndexOf("warning", StringComparison.OrdinalIgnoreCase) >= 0)
                return WarnColor;

            if (t.StartsWith("[info]", StringComparison.OrdinalIgnoreCase))
                return InfoColor;

            return InfoColor;
        }

        // ------------- Command handling -------------

        private async Task SendCommandAsync()
        {
            var raw = inputBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
                return;

            // Save to history
            if (_history.Count == 0 || _history.Last() != raw)
                _history.Add(raw);
            _historyIndex = _history.Count;

            inputBox.Clear();

            // Local commands
            if (raw.Equals(":reload", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("/reload", StringComparison.OrdinalIgnoreCase))
            {
                await ReloadAndShowModulesAsync();
                return;
            }

            if (raw.Equals(":help", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                RouteModuleMessage("System", "help",
@"Local commands:
  :reload or /reload       Reload and rediscover modules
  :help or /help           Show this help

Targeting:
  - Choose a module in the dropdown then type its command.
  - Or prefix input like 'Memory: recall key' to target a specific module quickly.");
                return;
            }

            if (moduleManager == null)
            {
                RouteModuleMessage("System", raw, "(no ModuleManager)");
                return;
            }

            // Allow "ModuleName: command" inline target override
            string? targetOverride = null;
            string cmd = raw;
            var colon = raw.IndexOf(':');
            if (colon > 0)
            {
                var possibleName = raw[..colon].Trim();
                if (moduleSelect.Items.Contains(possibleName))
                {
                    targetOverride = possibleName;
                    cmd = raw[(colon + 1)..].TrimStart();
                }
            }

            var target = targetOverride ?? (moduleSelect.SelectedItem?.ToString() ?? "(System)");

            // Broadcast to all modules
            if (target == "(All)")
            {
                if (moduleManager.Modules.Count == 0)
                {
                    RouteModuleMessage("(All)", raw, "(no modules)");
                    return;
                }

                foreach (var w in moduleManager.Modules)
                {
                    var name = w.Instance?.Name ?? w.Instance?.GetType().Name ?? "(unknown)";
                    string resp = string.Empty;
                    try
                    {
                        resp = moduleManager.SafeInvokeModuleByName(name, cmd)
                               ?? moduleManager.SafeInvokeModuleByName(name + "Module", cmd)
                               ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        resp = $"(error) {ex.Message}";
                    }
                    RouteModuleMessage(name, cmd, resp);
                    await Task.Yield();
                }
                return;
            }

            // System target: just echo or handle simple actions
            if (target == "(System)")
            {
                RouteModuleMessage("System", raw, "(no-op)");
                return;
            }

            // Single module invocation
            try
            {
                var response =
                    moduleManager.SafeInvokeModuleByName(target, cmd) ??
                    moduleManager.SafeInvokeModuleByName(target + "Module", cmd) ??
                    string.Empty;

                RouteModuleMessage(target, cmd, response);
            }
            catch (Exception ex)
            {
                RouteModuleMessage(target, cmd, $"(error) {ex.Message}");
            }
        }

        private void NavigateHistory(int delta)
        {
            if (_history.Count == 0) return;

            _historyIndex = Math.Clamp(_historyIndex + delta, 0, _history.Count);
            if (_historyIndex >= 0 && _historyIndex < _history.Count)
                inputBox.Text = _history[_historyIndex];
            else
                inputBox.Clear();

            inputBox.SelectionStart = inputBox.TextLength;
        }

        // ------------- Console helpers -------------

        private void ClearConsoles()
        {
            if (!consoleBox.IsDisposed)
            {
                if (InvokeRequired) { try { BeginInvoke(new Action(() => consoleBox.Clear())); } catch { } }
                else { consoleBox.Clear(); }
            }

            if (!leftConsoleBox.IsDisposed)
            {
                if (InvokeRequired) { try { BeginInvoke(new Action(() => leftConsoleBox.Clear())); } catch { } }
                else { leftConsoleBox.Clear(); }
            }
        }

        private void AppendStyledRight(string text, Color color, bool bold = false)
        {
            if (consoleBox.IsDisposed) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<string, Color, bool>(AppendStyledRight), text, color, bold); } catch { }
                return;
            }

            consoleBox.SelectionStart = consoleBox.TextLength;
            consoleBox.SelectionLength = 0;
            consoleBox.SelectionColor = color;
            consoleBox.SelectionFont = bold ? ConsoleFontSmallBold : ConsoleFontSmall;
            consoleBox.AppendText(text + Environment.NewLine);
            consoleBox.SelectionStart = consoleBox.TextLength;
            consoleBox.ScrollToCaret();
        }

        private void AppendStyledLeft(string text, Color color, bool bold = false)
        {
            if (leftConsoleBox.IsDisposed) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<string, Color, bool>(AppendStyledLeft), text, color, bold); } catch { }
                return;
            }

            leftConsoleBox.SelectionStart = leftConsoleBox.TextLength;
            leftConsoleBox.SelectionLength = 0;
            leftConsoleBox.SelectionColor = color;
            leftConsoleBox.SelectionFont = bold ? ConsoleFontSmallBold : ConsoleFontSmall;
            leftConsoleBox.AppendText(text + Environment.NewLine);
            leftConsoleBox.SelectionStart = leftConsoleBox.TextLength;
            leftConsoleBox.ScrollToCaret();
        }
    }
}