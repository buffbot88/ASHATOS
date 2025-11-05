# RaStudios.WinForms - .NET WinForms Edition

RaStudios rebuilt as a WinForms application with AI Coding Bot integration, DirectX 11 terminal, and game server automation.

## Features

### Core Modules
- **Server Connection**: Secure WebSocket connection to game server with authentication and rate limiting
- **AI Coding Bot**: AI-powered code generation with human-in-the-loop approval system
- **Terminal (DirectX 11)**: Hardware-accelerated terminal with GPU-based rendering
- **Logs & Diagnostics**: Centralized logging with filtering and color-coding
- **Code Preview**: Sandboxed code preview area (never auto-executes)

### Security Features
- ✅ Human approval required for all AI-generated code
- ✅ Rate limiting on API calls (10 requests/minute default)
- ✅ Input validation and dangerous pattern filtering
- ✅ Sandboxed code preview (no auto-execution)
- ✅ Authentication required for server operations
- ✅ Audit trail for all code approvals
- ❌ No autonomous self-replication
- ❌ No auto-execution of untrusted code

## Requirements

- .NET 9.0 SDK or later
- Windows 10/11 (for WinForms and DirectX 11)
- Visual Studio 2022 or VS Code with C# extension (optional)

## Building

```bash
# Restore dependencies
dotnet restore RaStudios.WinForms/RaStudios.WinForms.csproj

# Build the project
dotnet build RaStudios.WinForms/RaStudios.WinForms.csproj

# Run the application
dotnet run --project RaStudios.WinForms/RaStudios.WinForms.csproj
```

## Configuration

RaStudios.WinForms uses a `config.json` file for comprehensive configuration of all services and connections.

### Configuration File Structure

The `config.json` file includes the following sections:

#### 1. ASHAT OS Connection
Configuration for connecting to the ASHAT OS backend server:
```json
{
  "AshatOS": {
    "Host": "localhost",
    "Port": 7077,
    "Protocol": "WebSocket",
    "WebSocketUrl": "ws://localhost:7077/ws",
    "ApiBaseUrl": "http://localhost:7077/api",
    "TimeoutSeconds": 30,
    "UseAuthentication": true,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 5
  }
}
```

#### 2. FTP Configuration
Configuration for uploading built DLLs to an FTP server:
```json
{
  "FTP": {
    "Host": "localhost",
    "Port": 21,
    "Username": "",
    "Password": "",
    "UseSSL": true,
    "UploadPath": "/uploads/dlls",
    "TimeoutSeconds": 60,
    "PassiveMode": true
  }
}
```

**Security Note**: Store FTP credentials securely. In production, use environment variables or Windows Credential Manager instead of storing passwords in plain text.

#### 3. Build Configuration
Configuration for .NET build service:
```json
{
  "Build": {
    "DotNetPath": "dotnet",
    "BuildConfiguration": "Release",
    "OutputPath": "./bin/Release",
    "TargetFramework": "net9.0",
    "EnableNuGetRestore": true
  }
}
```

#### 4. Server Configuration
Legacy server connection settings (maintained for compatibility):
- **Server URL**: Default `ws://localhost:7077/ws`
- **Protocol**: WebSocket with JSON message format
- **Authentication**: Username/password (should use secure credential storage in production)
- **Rate Limiting**: 10 messages per second (configurable)

#### 5. AI Agent Configuration
- **Endpoint**: Default `http://localhost:8080/api/ai/generate`
- **Max Requests**: 10 per minute (configurable)
- **Approval**: Always required (cannot be disabled)
- **Policy Filter**: Enabled by default

### Loading Configuration

Configuration is automatically loaded on application startup. You can reload it at runtime using:
- Terminal command: `loadconfig`
- Or programmatically: `ConfigurationService.Instance.LoadConfiguration()`

## Usage

### Connecting to Game Server
1. Launch RaStudios.WinForms
2. Navigate to "Server Connection" tab
3. Enter server URL (e.g., `ws://localhost:7077/ws`)
4. Click "Connect"
5. Enter credentials and click "Authenticate"

### Using AI Coding Bot
1. Navigate to "AI Coding Bot" tab
2. Enter a description of the code you need
3. Select programming language (C#, Python, JavaScript, SQL, HTML)
4. Click "Generate Code"
5. Review generated code in the preview
6. Enter your name as approver
7. Click "Approve & Deploy" (requires explicit confirmation)

### Using DirectX 11 Terminal
1. Navigate to "Terminal (DirectX 11)" tab
2. Check "Enable DirectX 11 Rendering" for hardware acceleration
3. Type commands in the input box
4. Available commands:
   - `help` - Show available commands
   - `clear` - Clear terminal
   - `echo [text]` - Echo text
   - `dx11` - Show DirectX 11 info
   - `render` - Trigger GPU render
   - `status` - Show terminal status
   - `preview [code]` - Load code into preview tab
   - `loadconfig` - Reload configuration file

### Using Terminal Test Screen
The terminal now includes an integrated test screen for code development, building, and deployment:

#### Code Preview Tab
1. Enter or paste your C# code in the preview area
2. Set a project name for the build output
3. Click **Save Code** to save the code to a file
4. Click **Build DLL** to compile the code into a .NET DLL
5. Click **Upload to FTP** to upload the built DLL to the configured FTP server
6. Enable **Auto-upload after build** to automatically upload after successful builds

#### Build & Upload Output Tab
This tab displays real-time output from:
- Build process (compilation, errors, warnings)
- FTP connection status
- Upload progress and results

**Workflow Example:**
```
1. Write/paste code in Code Preview tab
2. Click "Build DLL" → switches to Build Output tab
3. Review build results
4. If successful, click "Upload to FTP" or enable auto-upload
5. View upload progress and confirmation
```

## Architecture

```
RaStudios.WinForms/
├── Forms/              # UI panels and forms
│   ├── MainForm.cs     # Main application window
│   ├── ServerConnectionPanel.cs
│   ├── AiAgentPanel.cs
│   ├── TerminalPanel.cs
│   ├── LogsPanel.cs
│   ├── CodePreviewPanel.cs
│   └── SettingsForm.cs
├── Modules/            # Core business logic
│   ├── ServerConnector.cs # WebSocket server connection
│   └── AiAgent.cs         # AI code generation with approval
├── Services/           # Supporting services
│   ├── LogService.cs      # Centralized logging
│   ├── TerminalRenderer.cs # DirectX 11 rendering
│   ├── BuildService.cs    # .NET build automation
│   ├── FTPService.cs      # FTP upload for DLLs
│   └── ConfigurationService.cs # Config management
├── Models/             # Data models
│   └── Models.cs
├── Program.cs          # Entry point
├── config.json         # Application configuration
└── app.manifest        # Windows manifest

```

## Security Considerations

### Human-in-the-Loop Workflow
1. User requests code generation
2. AI generates code (validated for dangerous patterns)
3. Code displayed in sandboxed preview
4. **Human review required** - code is NOT executed automatically
5. Human explicitly approves with name for audit trail
6. Human confirms deployment via dialog
7. Code deployed to server (if connected)

### What is NOT Allowed
- ❌ Autonomous code execution
- ❌ Self-replication or worm-like behavior
- ❌ Auto-deployment without human confirmation
- ❌ Bypassing approval requirements
- ❌ Executing code from untrusted sources

### Safe Automation
- ✅ CI-driven artifact deployment (external to this app)
- ✅ Signed update packages (GitHub Releases, ClickOnce)
- ✅ Human-reviewed code changes
- ✅ Rate-limited API access
- ✅ Audit logging

## Dependencies

- **SharpDX** (4.2.0): DirectX 11 bindings for graphics rendering
- **Newtonsoft.Json** (13.0.3): JSON serialization
- **System.Net.WebSockets.Client** (4.3.2): WebSocket client

## Testing

Tests will be added in a separate test project:

```bash
# Create test project
dotnet new xunit -n RaStudios.WinForms.Tests

# Add reference
dotnet add RaStudios.WinForms.Tests reference RaStudios.WinForms

# Run tests
dotnet test
```

## Update Strategy

The application supports multiple update strategies:
- **Manual**: User downloads and installs new version
- **GitHub Releases**: Download from GitHub releases page
- **ClickOnce**: One-click updates (configure in project properties)
- **Custom**: Implement custom update mechanism

All updates must use signed packages for security.

## Contributing

1. Follow existing code style and patterns
2. Add security controls for any new features
3. Require human approval for any code execution
4. Add logging for all significant operations
5. Update documentation

## License

See LICENSE file in repository root.

## References

- [ASHATOS Repository](https://github.com/buffbot88/ASHATOS) - Related C# and automation concepts
- [RaOS Project](https://github.com/buffbot88/TheRaProject) - Game server backend
- [SharpDX Documentation](http://sharpdx.org/) - DirectX 11 bindings
