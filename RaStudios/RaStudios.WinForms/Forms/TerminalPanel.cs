using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using RaStudios.WinForms.Services;
using RaStudios.WinForms.Models;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Viewport = SharpDX.Mathematics.Interop.RawViewportF;

namespace RaStudios.WinForms.Forms
{
    /// <summary>
    /// Terminal panel with DirectX 11 graphics processing for advanced rendering.
    /// Provides hardware-accelerated terminal with GPU-based text and graphics rendering.
    /// Includes test screen for code preview, build, and FTP upload functionality.
    /// </summary>
    public partial class TerminalPanel : UserControl
    {
        private Device? d3dDevice;
        private SwapChain? swapChain;
        private RenderTargetView? renderTargetView;
        private Texture2D? backBuffer;
        private DeviceContext? context;
        
        private Panel renderPanel;
        private TextBox inputTextBox;
        private RichTextBox outputTextBox;
        private Button sendButton;
        private Button clearButton;
        private CheckBox enableDx11CheckBox;
        
        // Test screen components
        private TabControl testTabControl;
        private RichTextBox codePreviewTextBox;
        private RichTextBox buildOutputTextBox;
        private Button saveCodeButton;
        private Button buildButton;
        private Button uploadButton;
        private TextBox projectNameTextBox;
        private CheckBox autoUploadCheckBox;
        
        private bool isDx11Enabled = false;
        private TerminalRenderer? terminalRenderer;
        private BuildService? buildService;
        private FTPService? ftpService;
        private string? lastGeneratedCode;

        public TerminalPanel()
        {
            InitializeComponent();
            InitializeDirectX11();
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                var config = ConfigurationService.Instance.GetConfiguration();
                buildService = new BuildService(config.Build);
                ftpService = new FTPService(config.FTP);

                buildService.OnStatusUpdate += (s, msg) => AppendOutput($"[Build] {msg}\n");
                buildService.OnError += (s, msg) => AppendOutput($"[Build Error] {msg}\n");
                
                ftpService.OnStatusUpdate += (s, msg) => AppendOutput($"[FTP] {msg}\n");
                ftpService.OnError += (s, msg) => AppendOutput($"[FTP Error] {msg}\n");
            }
            catch (Exception ex)
            {
                AppendOutput($"[Config] Failed to initialize services: {ex.Message}\n");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Create main split container for terminal and test screen
            var mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // ===== Top Panel: Terminal Output =====
            var terminalPanel = new Panel { Dock = DockStyle.Fill };

            // Output text box (shows terminal output)
            outputTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LimeGreen,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.None
            };

            // Render panel for DirectX 11 (overlays on output when enabled)
            renderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black,
                Visible = false
            };

            // Input panel
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(5)
            };

            // Input text box
            inputTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Consolas", 10F),
                Height = 25
            };
            inputTextBox.KeyPress += OnInputKeyPress;

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5, 5, 5, 0)
            };

            sendButton = new Button
            {
                Text = "Send",
                Dock = DockStyle.Right,
                Width = 80
            };
            sendButton.Click += OnSendClick;

            clearButton = new Button
            {
                Text = "Clear",
                Dock = DockStyle.Right,
                Width = 80
            };
            clearButton.Click += OnClearClick;

            enableDx11CheckBox = new CheckBox
            {
                Text = "Enable DirectX 11 Rendering",
                Dock = DockStyle.Left,
                AutoSize = true,
                Checked = false
            };
            enableDx11CheckBox.CheckedChanged += OnDx11CheckedChanged;

            buttonPanel.Controls.Add(sendButton);
            buttonPanel.Controls.Add(clearButton);
            buttonPanel.Controls.Add(enableDx11CheckBox);

            inputPanel.Controls.Add(buttonPanel);
            inputPanel.Controls.Add(inputTextBox);

            terminalPanel.Controls.Add(renderPanel);
            terminalPanel.Controls.Add(outputTextBox);
            terminalPanel.Controls.Add(inputPanel);

            mainSplitContainer.Panel1.Controls.Add(terminalPanel);

            // ===== Bottom Panel: Test Screen =====
            testTabControl = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Code Preview
            var codePreviewTab = new TabPage("Code Preview");
            codePreviewTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.None,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White
            };

            var codePreviewToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(5)
            };

            var projectNameLabel = new Label
            {
                Text = "Project Name:",
                Dock = DockStyle.Left,
                Width = 100,
                TextAlign = ContentAlignment.MiddleLeft
            };

            projectNameTextBox = new TextBox
            {
                Dock = DockStyle.Left,
                Width = 200,
                Text = "GeneratedCode"
            };

            saveCodeButton = new Button
            {
                Text = "Save Code",
                Dock = DockStyle.Right,
                Width = 100
            };
            saveCodeButton.Click += OnSaveCodeClick;

            buildButton = new Button
            {
                Text = "Build DLL",
                Dock = DockStyle.Right,
                Width = 100
            };
            buildButton.Click += OnBuildClick;

            uploadButton = new Button
            {
                Text = "Upload to FTP",
                Dock = DockStyle.Right,
                Width = 120,
                Enabled = false
            };
            uploadButton.Click += OnUploadClick;

            autoUploadCheckBox = new CheckBox
            {
                Text = "Auto-upload after build",
                Dock = DockStyle.Right,
                AutoSize = true,
                Checked = false
            };

            codePreviewToolbar.Controls.Add(autoUploadCheckBox);
            codePreviewToolbar.Controls.Add(uploadButton);
            codePreviewToolbar.Controls.Add(buildButton);
            codePreviewToolbar.Controls.Add(saveCodeButton);
            codePreviewToolbar.Controls.Add(projectNameTextBox);
            codePreviewToolbar.Controls.Add(projectNameLabel);

            codePreviewTab.Controls.Add(codePreviewTextBox);
            codePreviewTab.Controls.Add(codePreviewToolbar);

            // Tab 2: Build Output
            var buildOutputTab = new TabPage("Build & Upload Output");
            buildOutputTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LimeGreen
            };
            buildOutputTab.Controls.Add(buildOutputTextBox);

            testTabControl.TabPages.Add(codePreviewTab);
            testTabControl.TabPages.Add(buildOutputTab);

            mainSplitContainer.Panel2.Controls.Add(testTabControl);

            // Add main split container to form
            this.Controls.Add(mainSplitContainer);

            this.ResumeLayout(false);

            // Initial message
            AppendOutput("RaStudios Terminal - DirectX 11 Enabled\n");
            AppendOutput("Type commands and press Enter or click Send.\n");
            AppendOutput("Enable DirectX 11 rendering for GPU-accelerated graphics.\n");
            AppendOutput("Use the test screen below to preview, build, and upload code.\n\n");
            AppendOutput("> ");
        }

        private void InitializeDirectX11()
        {
            try
            {
                // Create D3D11 device and swap chain
                var swapChainDescription = new SwapChainDescription
                {
                    BufferCount = 1,
                    ModeDescription = new ModeDescription(
                        renderPanel.ClientSize.Width,
                        renderPanel.ClientSize.Height,
                        new Rational(60, 1),
                        Format.R8G8B8A8_UNorm),
                    IsWindowed = true,
                    OutputHandle = renderPanel.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };

                Device.CreateWithSwapChain(
                    DriverType.Hardware,
                    DeviceCreationFlags.None,
                    swapChainDescription,
                    out d3dDevice,
                    out swapChain);

                context = d3dDevice.ImmediateContext;

                // Get back buffer and create render target view
                backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
                renderTargetView = new RenderTargetView(d3dDevice, backBuffer);

                // Set viewport with validation
                var width = Math.Max(1, renderPanel.ClientSize.Width);
                var height = Math.Max(1, renderPanel.ClientSize.Height);
                
                var viewport = new Viewport 
                { 
                    X = 0, 
                    Y = 0, 
                    Width = width, 
                    Height = height, 
                    MinDepth = 0.0f, 
                    MaxDepth = 1.0f 
                };
                context.Rasterizer.SetViewport(viewport);

                // Initialize terminal renderer
                terminalRenderer = new TerminalRenderer(d3dDevice, context);

                AppendOutput("[DirectX 11] Graphics device initialized successfully.\n");
            }
            catch (Exception ex)
            {
                AppendOutput($"[DirectX 11] Initialization failed: {ex.Message}\n");
                AppendOutput("[DirectX 11] Falling back to software rendering.\n");
            }
        }

        private void OnDx11CheckedChanged(object? sender, EventArgs e)
        {
            isDx11Enabled = enableDx11CheckBox.Checked;
            
            if (isDx11Enabled)
            {
                renderPanel.Visible = true;
                outputTextBox.Visible = false;
                AppendOutput("[DirectX 11] Hardware acceleration enabled.\n");
                RenderFrame();
            }
            else
            {
                renderPanel.Visible = false;
                outputTextBox.Visible = true;
                AppendOutput("[DirectX 11] Hardware acceleration disabled.\n");
            }
        }

        private void OnInputKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                SendCommand();
            }
        }

        private void OnSendClick(object? sender, EventArgs e)
        {
            SendCommand();
        }

        private void OnClearClick(object? sender, EventArgs e)
        {
            outputTextBox.Clear();
            AppendOutput("RaStudios Terminal\n> ");
        }

        private void SendCommand()
        {
            var command = inputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(command))
                return;

            AppendOutput(command + "\n");
            ProcessCommand(command);
            AppendOutput("> ");
            
            inputTextBox.Clear();
        }

        private void ProcessCommand(string command)
        {
            try
            {
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return;

                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "help":
                        ShowHelp();
                        break;
                    case "clear":
                        outputTextBox.Clear();
                        break;
                    case "echo":
                        AppendOutput(string.Join(" ", parts.Skip(1)) + "\n");
                        break;
                    case "dx11":
                        ShowDx11Info();
                        break;
                    case "render":
                        if (isDx11Enabled)
                            RenderFrame();
                        else
                            AppendOutput("DirectX 11 is not enabled. Check the 'Enable DirectX 11 Rendering' option.\n");
                        break;
                    case "status":
                        ShowStatus();
                        break;
                    case "preview":
                        if (parts.Length > 1)
                            PreviewCode(string.Join(" ", parts.Skip(1)));
                        else
                            AppendOutput("Usage: preview <code>\n");
                        break;
                    case "loadconfig":
                        LoadConfigCommand();
                        break;
                    default:
                        AppendOutput($"Unknown command: {cmd}. Type 'help' for available commands.\n");
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Error executing command: {ex.Message}\n");
            }
        }

        private void ShowHelp()
        {
            AppendOutput("Available Commands:\n");
            AppendOutput("  help          - Show this help message\n");
            AppendOutput("  clear         - Clear the terminal\n");
            AppendOutput("  echo          - Echo text to terminal\n");
            AppendOutput("  dx11          - Show DirectX 11 information\n");
            AppendOutput("  render        - Trigger a DirectX 11 render (when enabled)\n");
            AppendOutput("  status        - Show terminal status\n");
            AppendOutput("  preview       - Preview code in test screen\n");
            AppendOutput("  loadconfig    - Reload configuration file\n");
        }

        private void LoadConfigCommand()
        {
            try
            {
                ConfigurationService.Instance.LoadConfiguration();
                InitializeServices();
                AppendOutput("Configuration reloaded successfully.\n");
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to reload configuration: {ex.Message}\n");
            }
        }

        private void PreviewCode(string code)
        {
            lastGeneratedCode = code;
            codePreviewTextBox.Text = code;
            testTabControl.SelectedIndex = 0; // Switch to preview tab
            AppendOutput("Code loaded in preview tab.\n");
        }

        private async void OnSaveCodeClick(object? sender, EventArgs e)
        {
            try
            {
                var code = codePreviewTextBox.Text;
                if (string.IsNullOrWhiteSpace(code))
                {
                    AppendBuildOutput("No code to save.\n");
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
                    DefaultExt = "cs",
                    FileName = $"{projectNameTextBox.Text}.cs"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllTextAsync(saveDialog.FileName, code);
                    AppendBuildOutput($"Code saved to: {saveDialog.FileName}\n");
                    AppendOutput($"Code saved to: {saveDialog.FileName}\n");
                }
            }
            catch (Exception ex)
            {
                AppendBuildOutput($"Error saving code: {ex.Message}\n");
            }
        }

        private async void OnBuildClick(object? sender, EventArgs e)
        {
            try
            {
                var code = codePreviewTextBox.Text;
                if (string.IsNullOrWhiteSpace(code))
                {
                    AppendBuildOutput("No code to build.\n");
                    return;
                }

                if (buildService == null)
                {
                    AppendBuildOutput("Build service not initialized.\n");
                    return;
                }

                buildButton.Enabled = false;
                testTabControl.SelectedIndex = 1; // Switch to build output tab
                buildOutputTextBox.Clear();
                AppendBuildOutput("Starting build...\n");

                var projectName = projectNameTextBox.Text;
                var result = await buildService.BuildCodeAsync(code, projectName);

                if (result.Success)
                {
                    AppendBuildOutput("Build completed successfully!\n");
                    AppendBuildOutput($"Output: {result.Output}\n");
                    
                    if (result.BuiltFiles.Length > 0)
                    {
                        AppendBuildOutput($"Built DLL: {result.BuiltFiles[0]}\n");
                        uploadButton.Enabled = true;
                        
                        // Auto-upload if enabled
                        if (autoUploadCheckBox.Checked)
                        {
                            await UploadBuiltFiles(result.BuiltFiles);
                        }
                    }
                }
                else
                {
                    AppendBuildOutput("Build failed.\n");
                    AppendBuildOutput($"Error: {result.ErrorOutput}\n");
                }
            }
            catch (Exception ex)
            {
                AppendBuildOutput($"Build error: {ex.Message}\n");
            }
            finally
            {
                buildButton.Enabled = true;
            }
        }

        private async void OnUploadClick(object? sender, EventArgs e)
        {
            try
            {
                var code = codePreviewTextBox.Text;
                if (string.IsNullOrWhiteSpace(code))
                {
                    AppendBuildOutput("No code to upload. Build first.\n");
                    return;
                }

                if (buildService == null || ftpService == null)
                {
                    AppendBuildOutput("Services not initialized.\n");
                    return;
                }

                uploadButton.Enabled = false;
                AppendBuildOutput("\nStarting upload to FTP server...\n");

                // First build if not already built
                var projectName = projectNameTextBox.Text;
                var buildResult = await buildService.BuildCodeAsync(code, projectName);

                if (!buildResult.Success)
                {
                    AppendBuildOutput("Build failed. Cannot upload.\n");
                    return;
                }

                await UploadBuiltFiles(buildResult.BuiltFiles);
            }
            catch (Exception ex)
            {
                AppendBuildOutput($"Upload error: {ex.Message}\n");
            }
            finally
            {
                uploadButton.Enabled = true;
            }
        }

        private async Task UploadBuiltFiles(string[] files)
        {
            if (ftpService == null || files.Length == 0)
                return;

            AppendBuildOutput("Uploading files to FTP server...\n");
            
            var uploadResult = await ftpService.UploadFilesAsync(files);
            
            if (uploadResult.Success)
            {
                AppendBuildOutput($"Upload completed successfully!\n");
                AppendBuildOutput($"Uploaded {uploadResult.UploadedFiles.Length} file(s).\n");
                foreach (var file in uploadResult.UploadedFiles)
                {
                    AppendBuildOutput($"  - {file}\n");
                }
            }
            else
            {
                AppendBuildOutput($"Upload failed: {uploadResult.Message}\n");
            }
        }

        private void AppendBuildOutput(string text)
        {
            if (buildOutputTextBox.InvokeRequired)
            {
                buildOutputTextBox.Invoke(new Action(() => AppendBuildOutput(text)));
                return;
            }

            buildOutputTextBox.AppendText(text);
            buildOutputTextBox.ScrollToCaret();
        }

        /// <summary>
        /// Public method to load code into the preview from external sources (e.g., AI Agent).
        /// </summary>
        public void LoadCodePreview(string code, string projectName = "GeneratedCode")
        {
            lastGeneratedCode = code;
            codePreviewTextBox.Text = code;
            projectNameTextBox.Text = projectName;
            testTabControl.SelectedIndex = 0;
        }

        private void ShowDx11Info()
        {
            if (d3dDevice == null)
            {
                AppendOutput("DirectX 11 is not available.\n");
                return;
            }

            AppendOutput("DirectX 11 Information:\n");
            AppendOutput($"  Feature Level: {d3dDevice.FeatureLevel}\n");
            AppendOutput($"  Device Type: {d3dDevice.CreationFlags}\n");
            AppendOutput($"  Enabled: {isDx11Enabled}\n");
        }

        private void ShowStatus()
        {
            AppendOutput("Terminal Status:\n");
            AppendOutput($"  DirectX 11: {(d3dDevice != null ? "Available" : "Not Available")}\n");
            AppendOutput($"  Hardware Acceleration: {(isDx11Enabled ? "Enabled" : "Disabled")}\n");
            AppendOutput($"  Render Size: {renderPanel.ClientSize.Width}x{renderPanel.ClientSize.Height}\n");
        }

        private void RenderFrame()
        {
            if (!isDx11Enabled || context == null || renderTargetView == null)
                return;

            try
            {
                // Clear render target
                context.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1));

                // Set render target
                context.OutputMerger.SetRenderTargets(renderTargetView);

                // Render terminal content using the renderer
                terminalRenderer?.Render(outputTextBox.Text);

                // Present
                swapChain?.Present(0, PresentFlags.None);
            }
            catch (Exception ex)
            {
                AppendOutput($"[DirectX 11] Render error: {ex.Message}\n");
            }
        }

        private void AppendOutput(string text)
        {
            if (outputTextBox.InvokeRequired)
            {
                outputTextBox.Invoke(new Action(() => AppendOutput(text)));
                return;
            }

            outputTextBox.AppendText(text);
            outputTextBox.ScrollToCaret();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (isDx11Enabled && swapChain != null && renderPanel.ClientSize.Width > 0 && renderPanel.ClientSize.Height > 0)
            {
                // Resize swap chain buffers
                try
                {
                    context?.ClearState();
                    renderTargetView?.Dispose();
                    backBuffer?.Dispose();

                    var width = Math.Max(1, renderPanel.ClientSize.Width);
                    var height = Math.Max(1, renderPanel.ClientSize.Height);

                    swapChain.ResizeBuffers(1, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

                    backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
                    renderTargetView = new RenderTargetView(d3dDevice, backBuffer);

                    var viewport = new Viewport 
                    { 
                        X = 0, 
                        Y = 0, 
                        Width = width, 
                        Height = height, 
                        MinDepth = 0.0f, 
                        MaxDepth = 1.0f 
                    };
                    context?.Rasterizer.SetViewport(viewport);
                }
                catch (Exception ex)
                {
                    AppendOutput($"[DirectX 11] Resize error: {ex.Message}\n");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                terminalRenderer?.Dispose();
                renderTargetView?.Dispose();
                backBuffer?.Dispose();
                swapChain?.Dispose();
                context?.Dispose();
                d3dDevice?.Dispose();
                ftpService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
