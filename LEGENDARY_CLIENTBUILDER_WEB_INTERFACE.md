# Legendary Client Builder - Web Interface Documentation

**Version:** 9.3.4  
**Last Updated:** January 2025  
**Target Audience:** End Users & Administrators

---

## Overview

The Legendary Client Builder web interface provides a comprehensive dashboard for generating, managing, and monitoring multi-platform game clients. This document covers all features, workflows, and capabilities of the web interface.

## Table of Contents

1. [Accessing the Dashboard](#accessing-the-dashboard)
2. [Dashboard Overview](#dashboard-overview)
3. [Client Generation](#client-generation)
4. [Template Management](#template-management)
5. [Client Management](#client-management)
6. [Progress Tracking](#progress-tracking)
7. [Statistics & Analytics](#statistics--analytics)
8. [Advanced Features](#advanced-features)
9. [Troubleshooting](#troubleshooting)

---

## Accessing the Dashboard

### Prerequisites

- ✅ Valid RaCore account with Admin role
- ✅ Active license (if license system is enabled)
- ✅ Modern web browser (Chrome, Firefox, Edge, Safari)
- ✅ JavaScript enabled

### Access Methods

#### Method 1: Via Control Panel

1. Navigate to `http://localhost:5000/login.html`
2. Login with your credentials
3. Go to Control Panel (`http://localhost:5000/control-panel.html`)
4. Click on the **"LegendaryClientBuilder"** tab (🔨)
5. Click **"Open Full Dashboard"** button

#### Method 2: Direct Access

1. Navigate directly to `http://localhost:5000/clientbuilder-dashboard.html`
2. You'll be redirected to login if not authenticated
3. Dashboard loads after successful authentication

### Default Credentials

**⚠️ Security Warning:** Change default credentials immediately!

- **Username:** `admin`
- **Password:** `admin123`

---

## Dashboard Overview

### Main Components

The dashboard consists of several key sections:

```
┌─────────────────────────────────────────────────────┐
│  🔨 Legendary Client Builder Dashboard              │
│  [Logout]                                           │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐  │
│  │  Statistics │ │  Generation │ │   Progress  │  │
│  │  Overview   │ │   Controls  │ │   Tracking  │  │
│  └─────────────┘ └─────────────┘ └─────────────┘  │
│                                                     │
│  ┌─────────────────────────────────────────────┐  │
│  │           Build Log Console                  │  │
│  │  [INFO] Starting client generation...        │  │
│  └─────────────────────────────────────────────┘  │
│                                                     │
│  ┌──────────────────┐ ┌────────────────────────┐  │
│  │  Template Browser │ │  Recent Clients       │  │
│  └──────────────────┘ └────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

### Navigation

- **Header**: Dashboard title and logout button
- **Stats Section**: Key metrics at a glance
- **Controls Section**: Client generation actions
- **Progress Section**: Real-time build progress
- **Build Log**: Detailed build output
- **Templates**: Available client templates
- **Recent Clients**: Recently generated clients

---

## Client Generation

### Overview

The Client Builder supports multiple platforms and templates for game client generation.

### Supported Platforms

| Platform | Description | Output Format |
|----------|-------------|---------------|
| **WebGL** | Browser-based HTML5/WebGL client | HTML, CSS, JavaScript |
| **Windows** | Windows desktop application | .exe launcher + HTML files |
| **Linux** | Linux desktop application | .sh launcher + HTML files |
| **macOS** | macOS desktop application | .sh launcher + HTML files |

### Generation Workflow

#### Step 1: Start Generation

1. Click **"Start New Generation"** button
2. (Currently uses API - wizard coming in future update)

#### Step 2: Configure Settings (API Method)

Use the API to generate clients:

```bash
curl -X POST http://localhost:5000/api/clientbuilder/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "licenseKey": "YOUR-LICENSE-KEY",
    "platform": "WebGL",
    "templateName": "WebGL-Professional",
    "configuration": {
      "serverUrl": "localhost",
      "serverPort": 5000,
      "gameTitle": "My Epic Game",
      "theme": "fantasy"
    }
  }'
```

#### Step 3: Monitor Progress

- Watch the **Progress Tracking** section
- Progress bars show:
  - **Asset Import**: 0-100%
  - **World Generation**: 0-100%
  - **Client Build**: 0-100%

#### Step 4: View Results

- Check **Build Log** for detailed output
- Find generated client in **Recent Clients** section
- Click **"Open Client"** to preview

### Configuration Options

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `licenseKey` | string | Yes | Valid license key |
| `platform` | string | Yes | Target platform (WebGL, Windows, Linux, macOS) |
| `templateName` | string | No | Template to use (defaults to platform default) |
| `serverUrl` | string | Yes | Game server URL |
| `serverPort` | number | Yes | Game server port |
| `gameTitle` | string | Yes | Title displayed in client |
| `theme` | string | No | Visual theme (fantasy, sci-fi, cyberpunk, etc.) |

---

## Template Management

### Available Templates

#### WebGL Templates

##### 1. WebGL-Basic
- **Description:** Minimal styling, clean and simple
- **Use Case:** Quick prototypes, testing
- **Features:**
  - Basic connection UI
  - Simple canvas rendering
  - Minimal JavaScript

##### 2. WebGL-Professional
- **Description:** Professional gradient UI with advanced features
- **Use Case:** Production games, commercial projects
- **Features:**
  - Gradient purple/blue UI
  - Real-time FPS counter
  - Connection status indicator
  - Multiple control buttons (Connect, Disconnect, Fullscreen, Reset)
  - Server information display
  - Professional styling

##### 3. WebGL-Gaming
- **Description:** Gaming-focused with HUD overlay
- **Use Case:** Action games, MMOs
- **Features:**
  - Full-screen HUD overlay
  - Gaming-style monospace font
  - Performance metrics (FPS, Ping)
  - Keyboard shortcut hints
  - Immersive black background
  - Real-time status updates

##### 4. WebGL-Mobile
- **Description:** Mobile-optimized responsive design
- **Use Case:** Mobile games, touch devices
- **Features:**
  - Touch-optimized controls
  - Virtual D-pad and action buttons
  - Mobile-friendly responsive layout
  - No-zoom viewport settings
  - Touch event handling
  - Status bar at top

#### Desktop Templates

##### 1. Desktop-Standard
- **Description:** Standard launcher with browser detection
- **Use Case:** Basic desktop deployment
- **Features:**
  - Windows: `.bat` and `.ps1` launchers
  - Linux/macOS: `.sh` launcher
  - Browser detection (Chrome, Edge, Firefox)
  - App mode launching

##### 2. Desktop-Advanced
- **Description:** Advanced features with multiple launch options
- **Use Case:** Advanced desktop applications
- **Features:**
  - All Desktop-Standard features
  - Additional configuration options
  - Advanced browser detection
  - Custom launch parameters

### Template Browser

Access templates in the dashboard:

1. Scroll to **"Available Templates"** section
2. Browse templates by platform
3. View template details:
   - Template name
   - Platform badge
   - Category tag
   - Description
   - Features list

### Template Selection

Templates are automatically selected based on:
1. Platform choice (required)
2. Explicit template name (optional)
3. Default template for platform (fallback)

**Example:**
- Platform: `WebGL` → Default: `WebGL-Professional`
- Template: `WebGL-Gaming` → Override default

---

## Client Management

### Recent Clients

View recently generated clients in the **"Recent Clients"** section.

#### Client Information

Each client card shows:
- **Client ID:** Unique identifier (GUID)
- **Platform:** Target platform badge
- **Template:** Template used
- **Created:** Timestamp
- **Actions:** Open, Delete (Admin only)

#### Client Actions

##### Open Client
1. Click **"Open Client"** button
2. Opens in new browser tab
3. URL format: `http://localhost:5000/clients/{client-id}/index.html`

##### Delete Client (Admin Only)
1. Click **"Delete"** button
2. Confirm deletion
3. Client is permanently removed

**API Method:**
```bash
curl -X DELETE http://localhost:5000/api/clientbuilder/delete/{client-id} \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Client List

View all your clients via API:

```bash
curl -X GET http://localhost:5000/api/clientbuilder/list \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "success": true,
  "clients": [
    {
      "id": "abc-123-def",
      "platform": "WebGL",
      "template": "WebGL-Professional",
      "configuration": {
        "gameTitle": "My Game",
        "serverUrl": "localhost"
      },
      "createdAt": "2025-01-15T10:30:00Z",
      "accessUrl": "http://localhost:5000/clients/abc-123-def/index.html"
    }
  ]
}
```

---

## Progress Tracking

### Real-Time Progress

The dashboard displays real-time progress for client generation:

#### Progress Indicators

1. **Asset Import Progress**
   - Shows import completion (0-100%)
   - Visual progress bar with percentage
   - Updates every 5 seconds (simulated)

2. **World Generation Progress**
   - Shows world gen completion (0-100%)
   - Visual progress bar with percentage
   - Updates every 5 seconds (simulated)

3. **Client Build Progress**
   - Shows build completion (0-100%)
   - Visual progress bar with percentage
   - Updates every 5 seconds (simulated)

#### Status Indicators

- **🔄 Running:** Active generation in progress
- **✅ Idle:** No active generation
- **❌ Error:** Generation failed

### Build Log Console

Real-time build output with:

#### Log Entry Types

```
[INFO]    - Informational messages (blue)
[SUCCESS] - Success messages (green)
[ERROR]   - Error messages (red)
[WARNING] - Warning messages (orange)
```

#### Log Features

- **Auto-scroll:** Automatically scrolls to latest entry
- **Timestamps:** Each entry shows timestamp
- **Color coding:** Easy visual identification
- **Terminal style:** Monospace font, dark theme
- **Scrollable:** View entire build history

#### Example Log Output

```
[INFO]    2025-01-15 10:30:15 - Starting client generation...
[INFO]    2025-01-15 10:30:16 - Importing assets...
[SUCCESS] 2025-01-15 10:30:20 - Assets imported successfully
[INFO]    2025-01-15 10:30:21 - Generating world...
[SUCCESS] 2025-01-15 10:30:25 - World generation complete
[INFO]    2025-01-15 10:30:26 - Building client...
[SUCCESS] 2025-01-15 10:30:30 - Client built successfully
[SUCCESS] 2025-01-15 10:30:31 - Generation complete!
```

---

## Statistics & Analytics

### Dashboard Statistics

The dashboard displays key metrics:

#### Total Clients Generated
- **Metric:** Total number of clients created
- **Update:** Real-time
- **Display:** Large number card

#### Available Templates
- **Metric:** Total templates available
- **Update:** On page load
- **Display:** Number card

#### Builder Status
- **Metric:** Running or Idle
- **Update:** Real-time
- **Display:** Status badge with indicator

#### Last Generation
- **Metric:** Timestamp of last client generation
- **Update:** After each generation
- **Display:** Relative time (e.g., "5 minutes ago")

### Module Status

Access via API:

```bash
curl -X GET http://localhost:5000/api/clientbuilder/status \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "success": true,
  "totalClients": 42,
  "templatesCount": 6,
  "isRunning": true,
  "lastGenerated": "2025-01-15T10:30:00Z",
  "version": "9.3.3",
  "status": "Operational"
}
```

---

## Advanced Features

### Auto-Refresh

The dashboard auto-refreshes data every 5 seconds:

- Updates progress bars
- Refreshes statistics
- Adds new log entries
- Updates client list

**To disable auto-refresh:**
```javascript
// In browser console
clearInterval(window.updateInterval);
```

### Keyboard Shortcuts

*(Coming in future update)*

- `Ctrl+G` - Start new generation
- `Ctrl+R` - Refresh dashboard
- `Ctrl+L` - Clear build log
- `Escape` - Close modals

### Fullscreen Mode

*(Coming in future update)*

1. Click fullscreen button
2. Dashboard expands to full screen
3. Press `Escape` to exit

### Export Build Log

*(Coming in future update)*

1. Click "Export Log" button
2. Download as `.txt` or `.json`
3. Use for debugging or documentation

---

## Troubleshooting

### Common Issues

#### Dashboard Not Loading

**Symptoms:**
- Blank page
- "Loading..." stuck
- 404 error

**Solutions:**
1. Check authentication (logout and login again)
2. Verify RaCore is running (`http://localhost:5000`)
3. Check browser console for JavaScript errors
4. Clear browser cache and reload

#### "Authentication Required" Error

**Symptoms:**
- Redirected to login page
- API returns 401 or 403

**Solutions:**
1. Login again with valid credentials
2. Check user has Admin role
3. Verify license is active (if required)
4. Clear localStorage and re-login

#### Client Generation Fails

**Symptoms:**
- Error in build log
- Progress bars stop
- API returns error

**Solutions:**
1. Check license key is valid
2. Verify all required parameters
3. Check available disk space
4. Review error message in build log
5. Check RaCore console for errors

#### Progress Not Updating

**Symptoms:**
- Progress bars stuck at 0%
- No new log entries
- Statistics not refreshing

**Solutions:**
1. Refresh the page (F5)
2. Check browser console for errors
3. Verify API endpoint is accessible
4. Check network tab for failed requests

#### Templates Not Showing

**Symptoms:**
- Empty template browser
- "No templates available" message

**Solutions:**
1. Check module is loaded (`/api/control/modules`)
2. Verify templates endpoint (`/api/clientbuilder/templates`)
3. Restart RaCore
4. Check module initialization logs

### Debug Mode

Enable debug mode for detailed logging:

```javascript
// In browser console
localStorage.setItem('debug', 'true');
location.reload();
```

View detailed logs:
```javascript
// Check module status
fetch('/api/clientbuilder/status', {
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('racore_token') }
})
.then(r => r.json())
.then(console.log);

// Check templates
fetch('/api/clientbuilder/templates', {
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('racore_token') }
})
.then(r => r.json())
.then(console.log);
```

### Getting Help

If issues persist:

1. **Check Documentation:**
   - PHASE9_3_3_SUMMARY.md
   - LegendaryClientBuilder/README.md
   - CONTROL_PANEL_MODULE_API.md

2. **GitHub Issues:**
   - Search existing issues
   - Create new issue with:
     - Steps to reproduce
     - Error messages
     - Browser console output
     - RaCore console output

3. **Community Support:**
   - GitHub Discussions
   - Stack Overflow (tag: racore)

---

## API Reference

### Quick Reference

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/clientbuilder/generate` | POST | Generate new client |
| `/api/clientbuilder/templates` | GET | List available templates |
| `/api/clientbuilder/templates?platform=WebGL` | GET | List templates by platform |
| `/api/clientbuilder/list` | GET | List user's clients |
| `/api/clientbuilder/delete/{id}` | DELETE | Delete a client |
| `/api/clientbuilder/status` | GET | Get builder status |

### Detailed Documentation

See:
- **LegendaryClientBuilder/README.md** - Full API documentation
- **PHASE9_1_IMPLEMENTATION.md** - Phase 9.1 details
- **PHASE9_1_QUICKSTART.md** - Quick start guide

---

## Future Enhancements

### Planned Features

#### Phase 9.4 (Q1 2025)
- ✨ Client generation wizard (step-by-step UI)
- ✨ Template editor (visual customization)
- ✨ Real-time WebSocket progress updates
- ✨ Build queue (multiple concurrent builds)

#### Phase 9.5 (Q2 2025)
- ✨ One-click client deployment
- ✨ Client analytics dashboard
- ✨ A/B testing for templates
- ✨ Custom theme builder

#### Phase 10 (Q3 2025)
- ✨ Mobile app clients (iOS/Android)
- ✨ Progressive Web App (PWA) support
- ✨ Template marketplace
- ✨ Multi-language support

---

## Best Practices

### For End Users

1. **Always test clients** before deploying to production
2. **Use appropriate templates** for your use case
3. **Monitor build logs** for errors or warnings
4. **Keep track of client IDs** for easy management
5. **Delete unused clients** to save disk space

### For Administrators

1. **Set up proper authentication** (change default password!)
2. **Configure user quotas** (max clients per user)
3. **Monitor disk usage** (client storage)
4. **Back up client configurations** regularly
5. **Review build logs** for system health

### For Developers

1. **Use the API** for automation
2. **Implement error handling** for API calls
3. **Cache template data** when appropriate
4. **Follow rate limits** (if implemented)
5. **Test on multiple platforms** before release

---

## Conclusion

The Legendary Client Builder web interface provides a powerful, user-friendly platform for generating and managing multi-platform game clients. With its modern UI, real-time progress tracking, and comprehensive template library, it streamlines the client generation process for developers and administrators alike.

### Key Takeaways

- 🚀 **Easy Access:** Two ways to access dashboard
- 🎨 **Multiple Templates:** 6+ professional templates
- 🔄 **Real-Time Updates:** Live progress tracking
- 📊 **Statistics:** Comprehensive analytics
- 🛠️ **Full Control:** Complete client management
- 🔒 **Secure:** Role-based access control

### Next Steps

1. **Explore Templates:** Try different templates for your game
2. **Generate Clients:** Create clients for multiple platforms
3. **Monitor Progress:** Watch real-time build progress
4. **Manage Clients:** View, open, and delete clients
5. **Review Documentation:** Dive deeper with API docs

---

## Additional Resources

### Documentation
- **PHASE9_3_3_SUMMARY.md** - Control panel architecture
- **LegendaryClientBuilder/README.md** - Module documentation
- **CONTROL_PANEL_MODULE_API.md** - API reference
- **MODULE_DEVELOPMENT_GUIDE.md** - Module development

### Community
- **GitHub Repository:** https://github.com/buffbot88/TheRaProject
- **Issues & Bugs:** GitHub Issues
- **Discussions:** GitHub Discussions

### Support
- **Email:** support@agpstudios.com (planned)
- **Discord:** Coming soon
- **Documentation Site:** Coming soon

---

**Version:** 9.3.4  
**Last Updated:** January 2025  
**Maintained By:** RaCore Development Team

---

**Happy Building! 🔨🎮**

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
