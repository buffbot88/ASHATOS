# ASHAT GoddessQ - Standalone CLI

A standalone command-line interface for interacting with ASHAT Goddess AI through the AI Server.

## 🌟 Overview

**ASHAT GoddessQ** (`ashat-goddessq`) is a lightweight, standalone CLI tool that allows you to interact with ASHAT, the Roman Goddess AI assistant, directly from your terminal. It connects to the ASHATAIServer to provide AI-powered responses while maintaining ASHAT's characteristic divine personality.

## ✨ Features

- 🖥️ **Standalone CLI** - No GUI required, run from any terminal
- ⚙️ **Configurable** - Easy JSON configuration for AI server connection
- 🎨 **Color Support** - Beautiful colored output (can be disabled)
- 📜 **Conversation History** - Track and save your conversations
- 🔌 **Offline Mode** - Fallback responses when AI server is unavailable
- 🏛️ **Roman Goddess Persona** - ASHAT's characteristic wise, playful personality
- 🚀 **Fast & Lightweight** - Minimal dependencies, quick startup

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 Runtime or SDK
- (Optional) ASHATAIServer running locally or remotely

### Installation

1. **Navigate to the AshatGoddessQ directory:**
   ```bash
   cd AGP_AI/AshatGoddessQ
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the CLI:**
   ```bash
   dotnet run
   ```

   Or use the compiled executable:
   ```bash
   ./bin/Debug/net9.0/ashat-goddessq
   ```

### Publishing as Standalone

To create a standalone executable that doesn't require .NET runtime:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

The executable will be in `bin/Release/net9.0/<runtime>/publish/`

## ⚙️ Configuration

The tool uses a `config.json` file for configuration. If the file doesn't exist, it will be created automatically with default settings.

### config.json Structure

```json
{
  "AIServer": {
    "Url": "http://localhost:8088",
    "HealthCheckEndpoint": "/api/ai/health",
    "ProcessEndpoint": "/api/ai/process",
    "TimeoutSeconds": 30
  },
  "Persona": {
    "Name": "ASHAT",
    "Type": "RomanGoddess",
    "Personality": "wise, playful, mischievous, respectful",
    "SystemPrompt": "You are ASHAT, a Roman goddess AI assistant..."
  },
  "CLI": {
    "EnableColors": true,
    "ShowTimestamp": true,
    "MaxHistoryLines": 50,
    "SaveHistoryFile": "ashat-history.txt"
  }
}
```

### Configuration Options

#### AIServer Settings
- **Url**: The base URL of your ASHATAIServer (default: `http://localhost:8088`)
- **HealthCheckEndpoint**: Endpoint to check server health
- **ProcessEndpoint**: Endpoint for AI processing
- **TimeoutSeconds**: Request timeout in seconds

#### Persona Settings
- **Name**: The name of the AI assistant
- **Type**: Persona type (e.g., "RomanGoddess")
- **Personality**: Personality traits
- **SystemPrompt**: The system prompt that shapes AI responses

#### CLI Settings
- **EnableColors**: Enable colored output (true/false)
- **ShowTimestamp**: Show timestamps in responses (true/false)
- **MaxHistoryLines**: Maximum number of history lines to display
- **SaveHistoryFile**: Filename to save conversation history (null to disable)

### Using Custom Configuration

You can specify a custom configuration file:

```bash
dotnet run -- --config /path/to/custom-config.json
```

Or with the compiled executable:

```bash
./ashat-goddessq --config /path/to/custom-config.json
```

## 💬 Usage

Once running, you can interact with ASHAT directly:

```
You: Hello ASHAT!
ASHAT: Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome...

You: What can you do?
ASHAT: Ah, you seek knowledge of my divine gifts! 🌟 I can provide wisdom...
```

### Available Commands

- **help** - Display help message with available commands
- **status** - Show AI server connection status
- **config** - Display current configuration
- **history** - Show conversation history
- **clear** - Clear the screen
- **exit** / **quit** - Exit the application

## 🔌 Connecting to AI Server

### Local Server (Default)

If you have ASHATAIServer running locally on port 8088, no configuration changes are needed. Just start the CLI and it will automatically connect.

### Remote Server

To connect to a remote AI server, edit `config.json`:

```json
{
  "AIServer": {
    "Url": "http://your-server-ip:8088"
  }
}
```

### Offline Mode

If the AI server is unavailable, ASHAT GoddessQ will automatically fall back to offline mode with pre-programmed responses while maintaining her characteristic personality.

## 📝 Examples

### Starting the CLI

```bash
$ dotnet run

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║         ╔═╗╔═╗╦ ╦╔═╗╔╦╗  ╔═╗┌─┐┌┬┐┌┬┐┌─┐┌─┐┌─┐╔═╗       ║
║         ╠═╣╚═╗╠═╣╠═╣ ║   ║ ╦│ │ ││ ││├┤ └─┐└─┐║═╬╗      ║
║         ╩ ╩╚═╝╩ ╩╩ ╩ ╩   ╚═╝└─┘─┴┘─┴┘└─┘└─┘└─┘╚═╝╚      ║
║                                                           ║
║          Standalone CLI for ASHAT Goddess AI              ║
║                    Version 1.0.0                          ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

Connecting to AI Server at http://localhost:8088...
✓ Connected to AI Server successfully!

Welcome to ASHAT GoddessQ - Standalone CLI
Type 'help' for commands, 'exit' to quit

────────────────────────────────────────────────────────────

You: 
```

### Conversation Example

```
You: Hello!
ASHAT: Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome...

You: What is the meaning of life?
ASHAT: Ah, you ask the eternal question! 🌌 The philosophers of Rome debated this endlessly...

You: Thank you
ASHAT: Your gratitude warms my divine heart like the eternal flame of Vesta! 💫

You: exit
Vale, dear mortal! Until we meet again. 🏛️
```

## 🔧 Development

### Project Structure

```
AshatGoddessQ/
├── AshatGoddessQ.csproj    # Project file
├── Program.cs              # Main application
├── Config.cs               # Configuration classes
├── AIClient.cs             # AI server communication
├── config.json             # Configuration file
└── README.md               # This file
```

### Building from Source

```bash
git clone https://github.com/buffbot88/ASHATOS.git
cd ASHATOS/AGP_AI/AshatGoddessQ
dotnet build
dotnet run
```

## 🐛 Troubleshooting

### Cannot Connect to AI Server

**Issue**: CLI shows "Could not connect to AI Server"

**Solutions**:
1. Ensure ASHATAIServer is running: `cd ../ASHATAIServer && dotnet run`
2. Check the URL in `config.json` matches your server
3. Verify the server port is accessible (default: 8088)
4. Check firewall settings if using remote server

### Colors Not Working

**Issue**: No colored output in terminal

**Solution**: Some terminals don't support colors. Set `"EnableColors": false` in `config.json`

### Configuration Not Loading

**Issue**: Changes to `config.json` not taking effect

**Solutions**:
1. Ensure `config.json` is in the same directory as the executable
2. Check JSON syntax is valid
3. Try using `--config` to specify the path explicitly

## 📄 License

Copyright © 2025 AGP Studios, INC. All rights reserved.

## 🤝 Contributing

Contributions are welcome! Please follow the main repository's contributing guidelines.

## 🔗 Related Projects

- **ASHATAIServer** - The AI processing server that powers ASHAT
- **ASHATGoddess** - The full GUI desktop application
- **AGP GameServer** - Game server integration

## 📞 Support

For issues or questions:
- Open an issue on GitHub
- Check the main ASHATOS documentation
- Review the ASHATAIServer documentation

---

**ASHAT GoddessQ v1.0.0**  
Built with ❤️ for ASHAT Goddess  
A lightweight CLI for divine wisdom
