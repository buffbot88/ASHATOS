# RaStudios: Unified Client for RaOS

RaStudios is an IDE, game player, and content browser for [RaOS](https://github.com/buffbot88/TheRaProject).
It enables admins to build games and sites, users to play games, and everyone to browse RaOS-powered content.

## 🎯 Project Status

**Current:** This repository contains two implementations:
1. **RaStudio.py** (Python/PyQt6) - Legacy implementation in the root directory
2. **RaStudios.WinForms** (C#/.NET 9.0) - **New** Windows-native implementation with AI integration

### RaStudios.WinForms (Recommended)

The new WinForms implementation provides:
- 🏠 **Integrated Homepage**: Modern web browser with news, updates, and quick links
- 🔐 **ASHATOS Authentication**: Secure login with session management
- 🎮 **Game Client**: Launch and play games with privilege-based access
- ⚡ **Auto Updates**: Built-in update checker and notification system
- 🤖 **AI Coding Bot**: AI-powered code generation with human-in-the-loop approval
- 💻 **DirectX 11 Terminal**: Hardware-accelerated terminal with GPU rendering
- 🔒 **Security First**: Role-based access control, no auto-execution, rate limiting
- 📊 **Logs & Diagnostics**: Centralized logging with color-coded output
- 🔍 **Code Preview**: Sandboxed preview area (never auto-executes)

See [RaStudios.WinForms/README.md](RaStudios.WinForms/README.md) for detailed documentation.

**Quick Start (Windows):**
```bash
cd RaStudios.WinForms
dotnet restore
dotnet build
dotnet run
```

## Python Implementation

### Features
- 🏠 **Integrated Homepage**: Web browser with login and update notifications
- 🎨 **Game Development Tools**: IDE and asset editor
- 🕹️ **Game Player**: For all users with authentication
- 🌐 **Website Browsing**: Browse RaOS-powered content
- 🔄 **Real-time Sync**: WebSocket connection with RaOS backend

### Quickstart
1. Install Python 3.10+ and dependencies: `pip install -r requirements.txt`
2. Configure your connection in `.env` (see `.env.example`)
3. Run: `python main.py`

### Connecting to RaOS
- By default, RaStudios connects to `http://localhost:8000` (see `config.py`).
- Uses WebSocket and REST for communication.
- See [docs/API.md](docs/API.md) for protocol details.

## Directory Structure

### WinForms Implementation
- `RaStudios.WinForms/` — .NET WinForms application
  - `Forms/` — UI panels and forms
  - `Modules/` — Core business logic (ServerConnector, AiAgent)
  - `Services/` — Supporting services
  - `Models/` — Data models
- `RaStudios.WinForms.Tests/` — Unit tests

### Python Implementation (Legacy)
- `main.py` — entry point
- `ui/` — PyQt6 UI components
- `services/` — Backend services
- `panels/` — UI panels
- `plugins/` — extension modules

## Security & Safety

The WinForms implementation emphasizes security:
- ✅ **Human approval required** for all AI-generated code
- ✅ **Rate limiting** on API calls (10 requests/minute default)
- ✅ **Input validation** and dangerous pattern filtering
- ✅ **Sandboxed preview** (no auto-execution)
- ✅ **Authentication required** for server operations
- ✅ **Audit trail** for all code approvals
- ❌ **No autonomous self-replication**
- ❌ **No auto-execution** of untrusted code

## Development

### Building
```bash
# WinForms
dotnet build RaStudios.WinForms.sln

# Python
pip install -r requirements.txt
```

### Testing
```bash
# WinForms (Windows only)
dotnet test RaStudios.WinForms.sln

# Python
pytest
```

## Documentation

- [WinForms Documentation](RaStudios.WinForms/README.md)
- [Testing Guide](RaStudios.WinForms.Tests/README.md)
- [Homepage Feature Guide](docs/HOMEPAGE_FEATURE.md) - **NEW!**
- [API Reference](docs/API.md) (Python implementation)
- [Plugin Development](docs/PLUGINS.md) (Python implementation)

## Migration Notes

The project is transitioning from Python to WinForms/.NET for:
- Better Windows integration
- DirectX 11 support for advanced graphics
- Improved security controls
- Native AI integration with safety controls
- Better performance for real-time operations

See [PHASES.md](PHASES.md) for migration roadmap.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md)

## License
See [LICENSE](LICENSE)
