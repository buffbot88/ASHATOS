# ASHATOS - AGP Studios AI & Game Platform

ASHATOS is a comprehensive platform combining AI services, game development tools, and authentication systems.

## 🌟 Components

### 🤖 AI Services

#### ASHATAIServer
- AI Language Model Processing using .gguf model files
- Game Server Integration for AI-enhanced game creation
- REST API for AI inference and game management
- Located in: `AGP_AI/ASHATAIServer/`
- [Documentation](AGP_AI/ASHATAIServer/README.md)

#### ASHAT Goddess
- Desktop AI assistant with Roman goddess persona
- GUI mode with animated visualization
- Headless mode for background operation
- Located in: `AGP_AI/ASHATGoddess/`
- [Documentation](AGP_AI/ASHATGoddess/README.md)

#### **NEW: ASHAT GoddessQ CLI** ⭐
- **Standalone command-line interface** for ASHAT
- Lightweight and portable
- Configurable AI server connection via JSON
- Works offline with fallback responses
- Located in: `AGP_AI/AshatGoddessQ/`
- [Documentation](AGP_AI/AshatGoddessQ/README.md)

### 🎮 Game Platform

#### AGP GameServer
- Complete game creation and deployment system
- AI-enhanced game generation
- Multiplayer support with state synchronization
- Located in: `AGP_AI/AGP_GameServer/`

#### RaStudios
- Game development IDE
- Visual editors and asset management
- Located in: `AGP_Studios/`

### 🔐 Authentication

#### phpBB3 Extension
- Forum-based authentication integration
- OAuth bridge for ASHATOS services
- Located in: `phpbb3_extension/`
- [Documentation](PHPBB3_INTEGRATION_GUIDE.md)

## 🚀 Quick Start

### Running ASHAT GoddessQ (Standalone CLI)

```bash
cd AGP_AI/AshatGoddessQ
dotnet build
dotnet run
```

Or use the startup scripts:
- Windows: `start.bat`
- Linux/macOS: `./start.sh`

Configuration: Edit `config.json` to set your AI server URL/port.

### Running ASHATAIServer (AI Backend)

```bash
cd AGP_AI/ASHATAIServer
dotnet build
dotnet run
```

Server will start on port 8088 by default.

### Running ASHAT Goddess (GUI)

```bash
cd AGP_AI/ASHATGoddess
dotnet run
```

For headless mode:
```bash
dotnet run -- --headless
```

## 📖 Documentation

- [ASHATAIServer Documentation](AGP_AI/ASHATAIServer/README.md)
- [ASHAT Goddess Documentation](AGP_AI/ASHATGoddess/README.md)
- [ASHAT GoddessQ CLI Documentation](AGP_AI/AshatGoddessQ/README.md)
- [phpBB3 Integration Guide](PHPBB3_INTEGRATION_GUIDE.md)

## 🔧 Requirements

- .NET 9.0 SDK or Runtime
- (Optional) .gguf language model files for AI features
- (Optional) phpBB3 forum for authentication

## 📄 License

Copyright © 2025 AGP Studios, INC. All rights reserved.

## 🤝 Contributing

Contributions welcome! Please open issues and pull requests on GitHub.

---

**ASHATOS Platform**  
Built with ❤️ by AGP Studios
