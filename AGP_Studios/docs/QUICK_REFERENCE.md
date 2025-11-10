# RaOS Integration - Quick Reference

## Overview

RaStudios now serves as the **unified Python client and IDE for RaOS**, integrating game development, game playing, web browsing, and content processing in a single application.

## What's New

### 🎮 Game Development IDE
- Create and manage game projects for LegendaryGameEngine
- Upload and sync assets (images, audio, models, scripts)
- Edit scenes and scripts
- Real-time collaboration support

### 🕹️ Game Player
- Browse and discover RaOS games
- Launch games via streaming or download
- View player profiles with stats
- Track achievements and compete on leaderboards

### 🌐 Web Browser
- Integrated browser for RaOS websites
- Automatic authentication and session management
- Secure browsing with TLS/WSS

### 📝 Content Editor
- Create and edit blogs, posts, images, videos, code
- Upload binary assets with processing pipeline
- Compress and convert assets automatically
- Analyze content metadata

### 🔒 Security
- Token-based authentication (JWT)
- TLS 1.2+ encryption for all communications
- Role-based access control (player, developer, admin)
- Automatic token refresh

## Quick Start

### Installation
```bash
# Clone repository
git clone https://github.com/buffbot88/RaStudios.git
cd RaStudios

# Install dependencies
pip install -r requirements.txt

# Run RaStudios
python main.py
```

### First Launch
1. Application starts and prompts for authentication
2. Enter RaOS credentials (or skip for offline mode)
3. Access all features through tabbed interface

## File Structure

```
RaStudios/
├── main.py                          # Entry point
├── requirements.txt                  # Dependencies
├── .gitignore                       # Git ignore rules
├── README.md                        # Main documentation
├── PHASES.md                        # Development roadmap
│
├── ui/
│   └── main_window.py               # Main UI window with tabs
│
├── services/                        # Business logic layer
│   ├── auth_service.py              # Authentication & authorization
│   ├── game_project_manager.py      # Game project management
│   ├── game_launcher.py             # Game playing functionality
│   ├── content_manager.py           # Content management
│   ├── rapi_client.py               # RaOS server communication
│   └── speech_pipeline_service.py   # Command pipeline
│
├── panels/                          # UI panels
│   ├── game_dev_panel.py            # Game development IDE
│   ├── game_player_panel.py         # Game player UI
│   ├── web_browser_panel.py         # Web browser UI
│   ├── content_editor_panel.py      # Content editor UI
│   ├── dashboard_panel.py           # Dashboard (existing)
│   └── logs_panel.py                # Logs viewer (existing)
│
├── core/
│   └── module_manager.py            # Module management
│
├── models/
│   ├── plugin_manifest.py           # Plugin metadata
│   └── diagnostics_entry.py         # Diagnostics data
│
├── docs/                            # Documentation
│   ├── PROTOCOL.md                  # RaOS protocol specification
│   ├── ARCHITECTURE.md              # System architecture
│   ├── USAGE_GUIDE.md               # Usage instructions
│   └── QUICK_REFERENCE.md           # This file
│
└── examples/
    └── service_usage.py             # Usage examples
```

## Key Services

### AuthService
```python
from services.auth_service import AuthService

auth = AuthService(rcore_client)
auth.authenticate(username, password)
if auth.is_authenticated():
    print(f"Roles: {auth.user_roles}")
```

### GameProjectManager
```python
from services.game_project_manager import GameProjectManager

project_mgr = GameProjectManager(rcore_client, auth_service)
project = project_mgr.create_project("My Game", "Description")
project_mgr.add_asset("sprite.png", "image", image_data)
project_mgr.save_project()
```

### GameLauncher
```python
from services.game_launcher import GameLauncher

launcher = GameLauncher(rcore_client, auth_service)
games = launcher.get_available_games()
launcher.launch_game(game_id, mode="stream")
achievements = launcher.get_achievements(game_id)
```

### ContentManager
```python
from services.content_manager import ContentManager

content_mgr = ContentManager(rcore_client, auth_service)
content = content_mgr.create_content("blog", "Title", "Content")
content_mgr.upload_binary_asset("image", "photo.jpg", data, compress=True)
```

## Protocol Summary

### Message Format
```json
{
  "action": "action_name",
  "auth_token": "jwt_token",
  "...": "action-specific fields"
}
```

### Key Actions
- **authenticate**: Login with credentials
- **create_game_project**: Create new game project
- **load_game_project**: Load existing project
- **list_games**: Get available games
- **launch_game**: Start game session
- **fetch_content**: Retrieve content
- **upload_binary_asset**: Upload files

See [PROTOCOL.md](PROTOCOL.md) for complete specification.

## UI Tabs

1. **Dashboard** - Command interface and logs
2. **Game Dev (IDE)** - Full game development environment
3. **Game Player** - Play games and track achievements
4. **Web Browser** - Browse RaOS websites
5. **Content Editor** - Manage and edit content
6. **Modules** - Plugin management
7. **Monitor** - System diagnostics

## Configuration

### Server Connection
Edit `main.py` to change server URL:
```python
rcore = RaCoreClient("ws://localhost:7077/ws")  # Local
rcore = RaCoreClient("wss://your-server.com/ws")  # Remote
```

### Dependencies
- **PyQt6** >= 6.4.0 - UI framework
- **PyQt6-WebEngine** >= 6.4.0 - Browser (optional)
- **websocket-client** >= 1.5.0 - WebSocket communication

## Security Checklist

- ✅ TLS 1.2+ for all connections
- ✅ JWT token-based authentication
- ✅ Password hashing (SHA256) before transmission
- ✅ Automatic token refresh
- ✅ Role-based access control
- ✅ Secure session management

## Common Tasks

### Create a Game Project
1. Switch to "Game Dev (IDE)" tab
2. Enter project name and description
3. Click "New Project"
4. Add assets with "Add Asset" button
5. Click "Save Project"

### Play a Game
1. Switch to "Game Player" tab
2. Click "Refresh Games"
3. Select a game from list
4. Click "Launch (Stream)" or "Launch (Download)"
5. Click "Stop Game" when done

### Edit Content
1. Switch to "Content Editor" tab
2. Click "New Content" or select existing
3. Edit title and content
4. Click "Save Content"

### Upload Assets
1. In "Content Editor" tab
2. Click "Upload Binary Asset"
3. Select file
4. Configure compression/conversion
5. Upload

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't connect to server | Check server URL and ensure RaOS is running |
| Authentication fails | Verify credentials and server availability |
| WebEngine not found | Install: `pip install PyQt6-WebEngine` |
| Assets not syncing | Click "Sync Assets" and check network |
| UI freezes | Restart application and check logs |

## Testing

### Run Example Script
```bash
cd RaStudios
python examples/service_usage.py
```

### Manual Testing
1. Start RaOS server
2. Launch RaStudios
3. Authenticate with test credentials
4. Test each tab's functionality
5. Check logs in Dashboard tab

## Development

### Adding New Features
1. Create service in `services/`
2. Create panel in `panels/`
3. Register in `ui/main_window.py`
4. Update protocol documentation
5. Add usage examples

### Code Quality
- All services have docstrings
- Type hints for parameters
- Error handling with try/catch
- User-friendly error messages

## Documentation

- **[README.md](../README.md)** - Overview and features
- **[PROTOCOL.md](PROTOCOL.md)** - Complete protocol spec
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Detailed usage instructions
- **[PHASES.md](../PHASES.md)** - Development roadmap

## Support

- **GitHub Issues**: https://github.com/buffbot88/RaStudios/issues
- **Documentation**: All docs in `/docs` folder
- **Examples**: Check `/examples` folder

## Version

- **Current Version**: 1.0.0 (RaOS Integration)
- **Protocol Version**: 1.0
- **Last Updated**: 2025-01-15

## Status

✅ Authentication & Security  
✅ Game Development IDE  
✅ Game Player  
✅ Web Browser  
✅ Content Editor  
✅ Protocol Documentation  
✅ Usage Examples  
🚧 Advanced features (in progress)  

---

**RaStudios - Your unified RaOS client for creating, playing, and managing content!**
