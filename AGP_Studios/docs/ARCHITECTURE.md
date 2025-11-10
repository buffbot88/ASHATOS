# RaStudios Architecture

## System Overview

RaStudios is a unified Python client/IDE for RaOS that integrates game development, game playing, web browsing, and content processing capabilities in a single application.

```
┌─────────────────────────────────────────────────────────────────┐
│                        RaStudios Client                          │
│                     (Python/PyQt6 Desktop)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌───────────┐  ┌──────────┐  ┌─────────┐  ┌────────────────┐ │
│  │ Game Dev  │  │  Game    │  │   Web   │  │    Content     │ │
│  │   IDE     │  │  Player  │  │ Browser │  │    Editor      │ │
│  └─────┬─────┘  └────┬─────┘  └────┬────┘  └────────┬───────┘ │
│        │             │              │                 │          │
│  ┌─────┴─────────────┴──────────────┴─────────────────┴──────┐ │
│  │              Service Layer (Business Logic)                │ │
│  │  • AuthService      • GameProjectManager                   │ │
│  │  • GameLauncher     • ContentManager                       │ │
│  └────────────────────────────────┬───────────────────────────┘ │
│                                   │                              │
│  ┌────────────────────────────────┴───────────────────────────┐ │
│  │           Communication Layer (WebSocket/REST)              │ │
│  │               • RaCoreClient                                │ │
│  │               • SpeechPipelineService                       │ │
│  └────────────────────────────────┬───────────────────────────┘ │
└────────────────────────────────────┼───────────────────────────┘
                                     │
                        TLS/WSS Encrypted Connection
                                     │
┌────────────────────────────────────┼───────────────────────────┐
│                                    │                             │
│  ┌────────────────────────────────┴───────────────────────────┐ │
│  │                    RaOS Server                              │ │
│  │                   (C#/.NET Backend)                         │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  • Authentication & Authorization (JWT)                          │
│  • Game Project Storage & Management                             │
│  • Content Asset Storage & Processing                            │
│  • Player Profile & Achievement System                           │
│  • Leaderboard Management                                        │
│  • Real-time Event Broadcasting                                  │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

## Component Architecture

### 1. UI Layer (Panels)

#### Game Development Panel (`panels/game_dev_panel.py`)
- **Purpose**: Full IDE for game development
- **Features**:
  - Project creation and management
  - Asset upload and synchronization
  - Scene/script editing
  - Live collaboration support
- **Dependencies**: GameProjectManager

#### Game Player Panel (`panels/game_player_panel.py`)
- **Purpose**: Game discovery and playing
- **Features**:
  - Browse available games
  - Launch games (stream/download)
  - View player profile
  - Track achievements
  - View leaderboards
- **Dependencies**: GameLauncher

#### Web Browser Panel (`panels/web_browser_panel.py`)
- **Purpose**: Integrated web browsing
- **Features**:
  - Navigate RaOS websites
  - Automatic authentication
  - Secure session management
- **Dependencies**: AuthService, PyQt6-WebEngine

#### Content Editor Panel (`panels/content_editor_panel.py`)
- **Purpose**: Content management and processing
- **Features**:
  - Create/edit/delete content
  - Upload binary assets
  - Asset pipeline (compress/convert)
  - Content type filtering
- **Dependencies**: ContentManager

### 2. Service Layer

#### AuthService (`services/auth_service.py`)
```python
┌────────────────────────┐
│     AuthService        │
├────────────────────────┤
│ • authenticate()       │
│ • refresh_token()      │
│ • logout()             │
│ • is_authenticated()   │
│ • has_role()           │
│ • get_auth_header()    │
└────────────────────────┘
```
**Responsibilities**:
- User authentication with username/password
- Token management (access + refresh)
- Role-based access control
- Automatic token refresh
- Secure session handling

#### GameProjectManager (`services/game_project_manager.py`)
```python
┌────────────────────────┐
│  GameProjectManager    │
├────────────────────────┤
│ • create_project()     │
│ • load_project()       │
│ • save_project()       │
│ • list_projects()      │
│ • sync_assets()        │
│ • add_asset()          │
└────────────────────────┘
```
**Responsibilities**:
- Game project CRUD operations
- Asset management (upload/sync)
- Project versioning
- Collaboration support

#### GameLauncher (`services/game_launcher.py`)
```python
┌────────────────────────┐
│     GameLauncher       │
├────────────────────────┤
│ • get_available_games()│
│ • launch_game()        │
│ • stop_game()          │
│ • get_player_profile() │
│ • get_achievements()   │
│ • get_leaderboard()    │
│ • download_game()      │
└────────────────────────┘
```
**Responsibilities**:
- Game discovery
- Game launching (stream/download)
- Player profile management
- Achievement tracking
- Leaderboard integration

#### ContentManager (`services/content_manager.py`)
```python
┌────────────────────────┐
│    ContentManager      │
├────────────────────────┤
│ • fetch_content()      │
│ • list_content()       │
│ • create_content()     │
│ • update_content()     │
│ • delete_content()     │
│ • upload_binary_asset()│
│ • analyze_asset()      │
└────────────────────────┘
```
**Responsibilities**:
- Content CRUD operations
- Binary asset upload
- Asset pipeline (compression/conversion)
- Content type management
- Asset analysis

### 3. Communication Layer

#### RaCoreClient (`services/rapi_client.py`)
```python
┌────────────────────────┐
│     RaCoreClient       │
├────────────────────────┤
│ • send()               │
│ • recv()               │
│ • connect()            │
│ • disconnect()         │
└────────────────────────┘
```
**Responsibilities**:
- WebSocket connection management
- Message sending/receiving
- Connection health monitoring
- Automatic reconnection

## Data Flow

### Authentication Flow
```
User Input → AuthService → RaCoreClient → RaOS Server
                ↓                              ↓
         Store Tokens ←────────────── JWT Tokens
                ↓
         Update UI Status
```

### Game Development Flow
```
Developer Action → GameDevPanel → GameProjectManager → RaCoreClient
                                         ↓
                                  Project/Asset Data → RaOS Server
                                         ↓
                                  Confirmation ← RaOS Server
                                         ↓
                                  Update UI
```

### Game Playing Flow
```
Player Selection → GamePlayerPanel → GameLauncher → RaCoreClient
                                         ↓
                                  Game Launch Request → RaOS Server
                                         ↓
                                  Stream/Download URL ← RaOS Server
                                         ↓
                                  Start Game Session
```

### Content Management Flow
```
Content Edit → ContentEditorPanel → ContentManager → RaCoreClient
                                         ↓
                              Content + Pipeline Options → RaOS Server
                                         ↓
                              Process & Store → RaOS Server
                                         ↓
                              Confirmation ← RaOS Server
                                         ↓
                              Update UI
```

## Security Architecture

### Transport Security
- **Protocol**: TLS 1.2+ for all connections
- **WebSocket**: WSS (WebSocket Secure)
- **REST API**: HTTPS

### Authentication Security
```
┌─────────────────────────────────────────────────────┐
│ Client                      Server                   │
├─────────────────────────────────────────────────────┤
│ 1. Hash password (SHA256)                           │
│ 2. Send credentials ──────────→                     │
│                                ← Check credentials   │
│                                ← Generate JWT tokens │
│ 3. Store tokens ←───────────── Return tokens         │
│ 4. Use in requests ──────────→ Validate token       │
│                                ← Check expiry        │
│ 5. Refresh before expiry ────→                      │
│                                ← New access token    │
└─────────────────────────────────────────────────────┘
```

### Token Structure
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs...",
  "expires_in": 3600,
  "token_type": "Bearer"
}
```

## Protocol Specifications

See [PROTOCOL.md](PROTOCOL.md) for detailed message format and protocol specification.

### Message Format
All messages use JSON with this structure:
```json
{
  "action": "action_name",
  "auth_token": "optional_token",
  "...": "action-specific fields"
}
```

### Response Format
```json
{
  "success": true/false,
  "data": { ... },
  "error": "optional error message",
  "timestamp": "ISO 8601 timestamp"
}
```

## Error Handling

### Client-Side Error Handling
1. **Connection Errors**: Automatic reconnection with exponential backoff
2. **Authentication Errors**: Prompt user to re-authenticate
3. **Validation Errors**: Display user-friendly error messages
4. **Network Errors**: Queue requests and retry when connection restored

### Server-Side Error Codes
| Code | Type | Handling |
|------|------|----------|
| 1000 | Invalid Request | Validate input before sending |
| 1001 | Unauthorized | Refresh token or re-authenticate |
| 1002 | Forbidden | Check user permissions |
| 1003 | Not Found | Handle gracefully, show message |
| 1004 | Rate Limited | Implement client-side rate limiting |
| 1005 | Server Error | Retry with backoff |
| 1006 | Service Unavailable | Display status, retry later |

## Extensibility

### Plugin System
RaStudios supports plugins through:
- `models/plugin_manifest.py` - Plugin metadata
- `panels/pluginpanels/` - Plugin UI panels
- `plugins/` - Plugin implementations

### Adding New Features
1. Create service class in `services/`
2. Create UI panel in `panels/`
3. Register in `ui/main_window.py`
4. Update protocol documentation
5. Add usage examples

## Performance Considerations

### Connection Management
- **Heartbeat**: Ping every 30 seconds
- **Timeout**: 30 seconds for requests
- **Reconnection**: Exponential backoff (1s, 2s, 4s, 8s, max 60s)

### Caching Strategy
- **Authentication tokens**: In-memory cache
- **User profile**: Cache for 5 minutes
- **Game list**: Cache for 1 minute
- **Content list**: Cache for 30 seconds

### Resource Usage
- **WebSocket**: Single persistent connection
- **UI**: Lazy loading of panels
- **Assets**: Stream large files, don't load into memory

## Future Enhancements

### Planned Features
1. **Offline Mode**: Local caching and sync when online
2. **Multi-language**: Internationalization support
3. **Themes**: Dark/light/custom themes
4. **Collaborative Editing**: Real-time multi-user editing
5. **VR Support**: Virtual reality integration
6. **AI Tools**: AI-powered content generation
7. **Plugin Marketplace**: Community-driven plugin ecosystem

### Scalability
- **Load Balancing**: Multiple RaOS server instances
- **CDN Integration**: Asset delivery via CDN
- **Distributed Storage**: Cloud-based asset storage
- **Microservices**: Split services for better scaling

---

For implementation details, see:
- [Protocol Documentation](PROTOCOL.md)
- [Usage Guide](USAGE_GUIDE.md)
- [Phase Roadmap](../PHASES.md)
