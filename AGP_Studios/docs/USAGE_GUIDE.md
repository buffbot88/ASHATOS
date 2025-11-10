# RaStudios Usage Guide

This guide provides detailed instructions for using RaStudios as a unified Python client/IDE for RaOS.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Authentication](#authentication)
3. [Game Development IDE](#game-development-ide)
4. [Game Player](#game-player)
5. [Web Browser](#web-browser)
6. [Content Editor](#content-editor)
7. [Troubleshooting](#troubleshooting)

## Getting Started

### First Launch

1. Start RaStudios:
   ```bash
   python main.py
   ```

2. On first launch, you'll be prompted to authenticate with RaOS server
   - Click **Yes** to authenticate now
   - Click **No** to use offline mode (limited functionality)

3. If authenticating, enter your RaOS credentials:
   - **Username**: Your RaOS username or email
   - **Password**: Your RaOS password

### Interface Overview

RaStudios uses a tabbed interface with the following sections:

- **Dashboard**: Command interface and logs
- **Game Dev (IDE)**: Game development tools
- **Game Player**: Game launcher and player
- **Web Browser**: Integrated web browser
- **Content Editor**: Content management
- **Modules**: Plugin management
- **Monitor**: System diagnostics

## Authentication

### Logging In

1. Navigate to any tab that requires authentication
2. If not authenticated, you'll see an authentication prompt
3. Enter your credentials to access protected features

### Authentication Roles

RaStudios supports role-based access:

- **Player**: Can play games, view profiles, achievements, and leaderboards
- **Developer**: Can create/edit games, manage projects, and assets
- **Admin**: Full access to all features

### Session Management

- Access tokens expire after 1 hour
- RaStudios automatically refreshes tokens in the background
- If refresh fails, you'll be prompted to re-authenticate

## Game Development IDE

The Game Dev tab provides full IDE functionality for creating games with the LegendaryGameEngine.

### Creating a New Project

1. Switch to the **Game Dev (IDE)** tab
2. Enter project details:
   - **Project Name**: Unique name for your game
   - **Description**: Brief description of your game
3. Click **New Project**
4. Your project is created and loaded

### Loading an Existing Project

1. Click **Load Project**
2. Select from available projects (first project loaded by default in demo)
3. Project assets and content are loaded

### Managing Assets

#### Adding Assets

1. Click **Add Asset**
2. Select a file from your computer:
   - **Images**: PNG, JPG, JPEG, GIF, BMP
   - **Audio**: WAV, MP3, OGG, FLAC
   - **Models**: OBJ, FBX, GLTF, GLB
   - **Scripts**: PY, CS, JS, LUA
3. Asset is uploaded to RaOS server

#### Removing Assets

1. Select an asset from the assets list
2. Click **Remove Asset**
3. Asset is removed from project (not deleted from server)

### Synchronizing Assets

1. Click **Sync Assets** to synchronize with RaOS server
2. Latest assets are downloaded and displayed
3. Useful when collaborating with other developers

### Saving Projects

1. Make changes to your project
2. Click **Save Project**
3. Changes are uploaded to RaOS server

## Game Player

The Game Player tab allows you to discover and play games from RaOS.

### Browsing Games

1. Switch to the **Game Player** tab
2. Click **Refresh Games** to load available games
3. Select a game from the list to view details

### Launching a Game

1. Select a game from the list
2. Choose launch mode:
   - **Launch (Stream)**: Stream game from server (requires stable connection)
   - **Launch (Download)**: Download game for offline play
3. Click the appropriate launch button
4. Game starts in selected mode

### Stopping a Game

1. While game is running, click **Stop Game**
2. Game session ends and resources are released

### Viewing Player Profile

1. Click **Refresh Profile** to load your player data
2. Profile shows:
   - Username
   - Level
   - Experience
   - Stats

### Achievements

1. Select a game from the list
2. Achievements for that game are displayed automatically
3. ✓ indicates unlocked achievements
4. ✗ indicates locked achievements

### Leaderboards

1. Select a game from the list
2. Change view selector to **Leaderboard**
3. Select category:
   - **Global**: Worldwide rankings
   - **Friends**: Rankings among friends
   - **Regional**: Rankings in your region
4. Top players and scores are displayed

## Web Browser

The Web Browser tab provides integrated browsing for RaOS-powered websites.

### Navigating to a URL

1. Switch to the **Web Browser** tab
2. Enter URL in the address bar
3. Press Enter or click **Go**
4. Page loads in the embedded browser

### Navigation Controls

- **◀ (Back)**: Navigate to previous page
- **▶ (Forward)**: Navigate to next page
- **⟳ (Refresh)**: Reload current page

### Authenticated Browsing

When navigating to RaOS URLs:
- Authentication is automatically handled
- Your session tokens are included
- Secure communication via HTTPS/WSS

### Note on WebEngine

The browser requires **PyQt6-WebEngine** to be installed:

```bash
pip install PyQt6-WebEngine
```

If not installed, you'll see a warning message with installation instructions.

## Content Editor

The Content Editor tab provides tools for managing RaOS content assets.

### Creating New Content

1. Switch to the **Content Editor** tab
2. Click **New Content**
3. Select content type from dropdown:
   - Blog
   - Post
   - Image
   - Video
   - Code
   - Document
4. Enter title and content
5. Click **Save Content**

### Editing Existing Content

1. Select content type filter (or "All")
2. Click **Refresh List** to load content
3. Click on content item in the list
4. Edit title and content in the editor
5. Click **Save Content** to update

### Deleting Content

1. Select content from the list
2. Click **Delete Content**
3. Confirm deletion
4. Content is removed from RaOS server

### Uploading Binary Assets

The Content Editor includes an asset pipeline for binary uploads:

1. Click **Upload Binary Asset**
2. Select file from your computer
3. Configure pipeline options:
   - **Compress on upload**: Automatically compress file (recommended)
   - **Convert to**: Optional format conversion (PNG, JPEG, WebP, MP4, WebM)
4. Click **Open** to upload
5. Asset is processed and uploaded to server

### Asset Pipeline Features

The asset pipeline automatically:
- Compresses files to reduce size
- Converts formats (e.g., PNG to WebP for web optimization)
- Analyzes assets for quality and metadata

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to RaOS server

**Solutions**:
- Check that RaOS server is running
- Verify server URL in `main.py` (default: `ws://localhost:7077/ws`)
- Check firewall settings
- Ensure network connectivity

### Authentication Failures

**Problem**: Login fails or token expires

**Solutions**:
- Verify credentials are correct
- Check that user account is active on RaOS server
- Try logging out and back in
- Contact RaOS administrator

### WebEngine Not Available

**Problem**: Browser tab shows warning about missing WebEngine

**Solution**:
```bash
pip install PyQt6-WebEngine
```

Restart RaStudios after installation.

### Assets Not Syncing

**Problem**: Assets don't synchronize between RaStudios and server

**Solutions**:
- Click **Sync Assets** to manually synchronize
- Check authentication status (developer role required)
- Verify network connection
- Check RaOS server logs for errors

### UI Not Responding

**Problem**: RaStudios UI freezes or becomes unresponsive

**Solutions**:
- Close and restart RaStudios
- Check system resources (CPU, memory)
- Review logs in Dashboard tab for errors
- Report issue with logs to support

## Best Practices

### For Developers

1. **Save frequently**: Save your game projects regularly
2. **Use version control**: Keep backups of your work
3. **Sync before editing**: Synchronize assets before making changes
4. **Test thoroughly**: Use the game player to test your games

### For Players

1. **Keep profile updated**: Refresh profile to see latest stats
2. **Check achievements**: Track your progress regularly
3. **Compete on leaderboards**: Challenge other players

### For Content Creators

1. **Use asset pipeline**: Let the pipeline optimize your uploads
2. **Organize content**: Use consistent naming and categorization
3. **Preview before publishing**: Check content before making it public
4. **Compress large files**: Enable compression for better performance

## Getting Help

For additional support:

- **Documentation**: See [README.md](../README.md) and [PROTOCOL.md](PROTOCOL.md)
- **Issues**: Report bugs on [GitHub Issues](https://github.com/buffbot88/RaStudios/issues)
- **Community**: Join the RaOS community (coming soon)

---

**Happy creating with RaStudios!** 🚀
