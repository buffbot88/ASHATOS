# Game Client Homepage Feature Documentation

## Overview

This document describes the newly implemented Game Client Homepage feature for RaStudios, which provides an integrated web browser with login capabilities, update notifications, and seamless connection to ASHATOS.

## Features

### 1. Homepage with Integrated Web Browser

#### WinForms Implementation (`HomePagePanel.cs`)
- **Location**: `RaStudios.WinForms/Forms/HomePagePanel.cs`
- **Features**:
  - Embedded WebBrowser control displaying a welcoming homepage
  - News and updates section with latest features
  - Quick links for documentation, community, bug reports, etc.
  - Modern gradient design with card-based layout
  - Responsive design optimized for desktop

#### Python Implementation (`web_browser_panel.py`)
- **Location**: `panels/web_browser_panel.py`
- **Features**:
  - PyQt6 WebEngineView for browsing RaOS-powered content
  - Enhanced homepage with feature highlights
  - Browser navigation controls (back, forward, refresh)
  - URL bar for navigation
  - Fallback display when WebEngine is not installed

### 2. Login Panel for ASHATOS Authentication

#### WinForms Implementation (`LoginForm.cs`)
- **Location**: `RaStudios.WinForms/Forms/LoginForm.cs`
- **Features**:
  - Secure login dialog with username and password fields
  - Password field with masked input
  - Remember Me checkbox for convenience
  - Progress indicator during authentication
  - Status messages for user feedback
  - Async authentication with ServerConnector
  - Clean, modern UI with proper styling

#### Python Implementation
- **Location**: `panels/web_browser_panel.py` (integrated)
- **Features**:
  - Login dialog integrated into web browser panel
  - Secure authentication with auth_service
  - Session management and logout functionality
  - Visual indicators for authentication status

### 3. Update Notification Feature

#### WinForms Implementation
- **Location**: `RaStudios.WinForms/Forms/HomePagePanel.cs`
- **Features**:
  - "Check for Updates" button in homepage
  - Progress bar showing update check status
  - Simulated update check with server communication
  - Version information display
  - Future-ready for real update mechanism

#### Python Implementation
- **Location**: `panels/web_browser_panel.py`
- **Features**:
  - Update check button with progress indicator
  - Version and build date display
  - Simulated 2-second update check
  - Ready for integration with real update server

### 4. Game Client Panel

#### WinForms Implementation (`GameClientPanel.cs`)
- **Location**: `RaStudios.WinForms/Forms/GameClientPanel.cs`
- **Features**:
  - Game library list with sample games
  - Launch and stop game controls
  - Game view panel (ready for game rendering)
  - Console output for game messages
  - Connection status monitoring
  - Graceful error handling

### 5. Privilege-Based Tab Visibility

#### MainForm Enhancement
- **Location**: `RaStudios.WinForms/MainForm.cs`
- **Features**:
  - Role-based access control (guest, player, developer, admin)
  - Role hierarchy enforcement
  - Dynamic tab visibility based on user privileges
  - Method `SetUserRole(string role)` to update user permissions
  - Tabs marked with 🔒 when locked

**Role Hierarchy**:
- **guest** (level 0): Can access Home tab
- **player** (level 1): Can access Home and Game Client tabs
- **developer** (level 2): Can access all above plus AI Coding Bot
- **admin** (level 3): Can access all tabs

### 6. Connection Status Indicators

#### ASHATOS Connection Status
- Visual indicators showing connection state
- Color-coded status (green=connected, red=disconnected)
- Real-time updates when connection state changes
- Displayed in:
  - Status bar at bottom of main window
  - Homepage control panel
  - Game client panel

#### Progress Indicators
- Download/update progress bars
- Marquee style for indeterminate operations
- Percentage-based for determinate operations
- Hidden when not in use

## Architecture

### Data Flow

```
User Action → UI Component → Service/Module → Server
                ↓
           Status Update
                ↓
        Update UI Elements
```

### Key Classes

1. **HomePagePanel**: Main homepage with browser and controls
2. **LoginForm**: Authentication dialog
3. **GameClientPanel**: Game launching and management
4. **ServerConnector**: WebSocket communication with server
5. **AuthService**: Authentication and session management (Python)
6. **LogService**: Centralized logging

### Security Features

- Password hashing before transmission
- Token-based authentication
- Session management
- Rate limiting on API calls
- Human-in-the-loop for sensitive operations
- Audit trail for authentication events

## Usage

### Launching the Application

**WinForms**:
```bash
cd RaStudios.WinForms
dotnet run
```

**Python**:
```bash
python main.py
```

### Logging In

1. Click "Login to ASHATOS" button
2. Enter username and password
3. Click "Login" or press Enter
4. Upon success, the status changes to show logged-in user

### Checking for Updates

1. Click "Check for Updates" button
2. Progress bar appears showing check in progress
3. Dialog shows current version status
4. Future versions will download and apply updates

### Playing Games

1. Navigate to "Game Client" tab (requires player role or higher)
2. Select a game from the list
3. Click "Launch Game"
4. Monitor console for game messages
5. Click "Stop" to exit the game

### Setting User Roles (Programmatic)

```csharp
// WinForms
var mainForm = new MainForm();
mainForm.SetUserRole("developer");
```

## Testing

### Unit Tests

Tests are located in `RaStudios.WinForms.Tests/NewFeatureTests.cs`:

- `HomePagePanelTests`: Tests for homepage panel initialization
- `LoginFormTests`: Tests for login form behavior
- `GameClientPanelTests`: Tests for game client functionality
- `MainFormTests`: Tests for main form and role management
- `PrivilegeBasedAccessTests`: Tests for role hierarchy and permissions

### Running Tests

```bash
dotnet test RaStudios.WinForms.sln
```

**Note**: Tests require Windows environment for WinForms components.

## Configuration

### Server Connection

Default server URL: `ws://localhost:7077/ws`

To change, update `ServerConnector.ServerUrl` property.

### Authentication

Authentication is integrated with the existing `ServerConnector` for WinForms and `AuthService` for Python.

## Future Enhancements

1. **Real Update Mechanism**
   - Connect to GitHub Releases API
   - Download and apply updates automatically
   - Digital signature verification

2. **Enhanced Game Client**
   - Actual game rendering in game view panel
   - Save game state persistence
   - Achievements and leaderboards
   - Multiplayer support

3. **Social Features**
   - Friends list
   - Chat integration
   - Game invites
   - Community feed

4. **Marketplace Integration**
   - Browse and install mods
   - Plugin marketplace
   - User-generated content

5. **Advanced Browser Features**
   - Bookmarks
   - History
   - Download manager
   - Developer tools

## Troubleshooting

### Common Issues

**Issue**: WebEngine not available (Python)
- **Solution**: Install PyQt6-WebEngine: `pip install PyQt6-WebEngine`

**Issue**: Cannot connect to server
- **Solution**: Ensure ASHATOS server is running on configured URL

**Issue**: Authentication fails
- **Solution**: Verify credentials and server connectivity

**Issue**: Tabs not appearing
- **Solution**: Check user role is set correctly with appropriate privileges

## API Reference

### HomePagePanel

```csharp
public class HomePagePanel : UserControl
{
    public HomePagePanel(ServerConnector connector);
    public void NavigateToUrl(string url);
}
```

### LoginForm

```csharp
public class LoginForm : Form
{
    public LoginForm(ServerConnector connector);
    public string Username { get; private set; }
}
```

### GameClientPanel

```csharp
public class GameClientPanel : UserControl
{
    public GameClientPanel(ServerConnector connector);
    public void UpdateConnectionStatus(bool connected);
}
```

### MainForm

```csharp
public class MainForm : Form
{
    public void SetUserRole(string role);
}
```

## Contributing

When contributing to these features:

1. Follow existing code style
2. Add appropriate error handling
3. Update documentation
4. Add unit tests
5. Test with various user roles
6. Ensure security best practices

## License

See repository LICENSE file.

## References

- [ASHATOS Repository](https://github.com/buffbot88/ASHATOS)
- [RaOS Project](https://github.com/buffbot88/TheRaProject)
- [PyQt6 Documentation](https://www.riverbankcomputing.com/static/Docs/PyQt6/)
- [.NET WinForms Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
