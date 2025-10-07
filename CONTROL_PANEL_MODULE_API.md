# Control Panel Module API Reference

**Version:** 9.3.4  
**Last Updated:** January 2025  
**Target Audience:** Module Developers

---

## Overview

The RaCore Control Panel provides a modular, tabbed architecture that allows modules to register their own administrative interfaces. This document describes the API for integrating custom modules into the control panel.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Module Registration](#module-registration)
3. [Tab Configuration](#tab-configuration)
4. [Render Functions](#render-functions)
5. [API Endpoints](#api-endpoints)
6. [Permission System](#permission-system)
7. [UI/UX Guidelines](#uiux-guidelines)
8. [Complete Example](#complete-example)

---

## Architecture Overview

### Tab Registration System

The control panel uses a JavaScript-based tab registration system defined in `control-panel-ui.js`:

```javascript
const MODULE_TABS = {
    'ModuleName': {
        category: 'extensions',      // Module category
        icon: '🎮',                   // Tab icon (emoji)
        requiredRole: 'Admin',        // Required user role
        render: renderFunctionName    // Render function reference
    }
};
```

### Module Discovery Flow

1. **Backend Registration**: Module is loaded by ModuleManager with `[RaModule]` attribute
2. **API Discovery**: Frontend calls `/api/control/modules` to get available modules
3. **Tab Filtering**: Available modules are matched to tab definitions
4. **Tab Rendering**: Tab buttons and content containers are created
5. **Content Rendering**: When tab is selected, render function is called

---

## Module Registration

### Backend Registration (C#)

Your module must be decorated with the `[RaModule]` attribute:

```csharp
using RaCore.Engine.Manager;
using Abstractions;

[RaModule(Category = "extensions")]
public class MyCustomModule : ModuleBase
{
    public override string Name => "MyCustomModule";
    public override string Version => "1.0.0";
    
    // Module implementation...
}
```

**Available Categories:**
- `core` - Core system modules
- `extensions` - Extension modules
- `clientbuilder` - Client builder modules
- `custom` - Custom third-party modules

### Module Discovery Endpoint

The `/api/control/modules` endpoint returns registered modules:

```http
GET /api/control/modules
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "modules": [
    {
      "name": "MyCustomModule",
      "description": "MyCustomModule module",
      "category": "extensions"
    }
  ]
}
```

---

## Tab Configuration

### Adding a Tab Definition

To add a new tab to the control panel, you need to modify the `MODULE_TABS` object in `WwwrootGenerator.cs`:

```csharp
private void GenerateControlPanelUiJs(string jsPath)
{
    var jsContent = $@"
// ... existing code ...

const MODULE_TABS = {{
    // ... existing tabs ...
    
    'MyCustomModule': {{ 
        category: 'extensions',
        icon: '🎮',
        requiredRole: 'Admin',
        render: renderMyCustomModuleTab
    }}
}};

// ... rest of the code ...
";
}
```

### Tab Configuration Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `category` | string | Yes | Module category for filtering (`core`, `extensions`, `clientbuilder`, `custom`) |
| `icon` | string | Yes | Emoji icon displayed on tab button |
| `requiredRole` | string | Yes | Minimum role required to view tab (`User`, `Admin`, `SuperAdmin`) |
| `render` | function | Yes | Function to render tab content |

---

## Render Functions

### Function Signature

Each tab must have a render function that accepts a container element:

```javascript
function renderMyCustomModuleTab(container) {
    // container is the DOM element to render into
    container.innerHTML = `
        <h2 style="color: #667eea;">🎮 My Custom Module</h2>
        <p>Module content goes here...</p>
    `;
}
```

### Standard Render Pattern

```javascript
function renderMyCustomModuleTab(container) {
    // 1. Define HTML structure
    const content = `
        <div style="max-width: 1200px;">
            <h2 style="color: #667eea; margin-bottom: 20px;">
                🎮 My Custom Module
            </h2>
            
            <p style="color: #666; margin-bottom: 20px;">
                Module description and functionality
            </p>
            
            <div class="modules-grid">
                <div class="module-card">
                    <h3>Feature 1</h3>
                    <p>Feature description</p>
                    <button onclick="handleFeature1()">Action</button>
                </div>
                
                <div class="module-card">
                    <h3>Feature 2</h3>
                    <p>Feature description</p>
                    <button onclick="handleFeature2()">Action</button>
                </div>
            </div>
        </div>
    `;
    
    // 2. Set container content
    container.innerHTML = content;
    
    // 3. Initialize any interactive elements
    initializeMyCustomModule();
}
```

### Async Data Loading

For modules that need to load data from the API:

```javascript
async function renderMyCustomModuleTab(container) {
    // Show loading state
    container.innerHTML = '<p class="loading">Loading module data...</p>';
    
    try {
        // Fetch module data
        const response = await fetch('/api/mycustommodule/status', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('racore_token')}`
            }
        });
        
        const data = await response.json();
        
        // Render with data
        container.innerHTML = `
            <h2 style="color: #667eea;">🎮 My Custom Module</h2>
            <div class="stats-grid">
                <div class="stat-card">
                    <h3>${data.totalItems}</h3>
                    <p>Total Items</p>
                </div>
            </div>
        `;
    } catch (error) {
        container.innerHTML = `
            <p class="error">Failed to load module: ${error.message}</p>
        `;
    }
}
```

---

## API Endpoints

### Creating Module-Specific Endpoints

Add endpoints in `Program.cs` for your module:

```csharp
// MyCustomModule: Get Status (Admin+)
app.MapGet("/api/mycustommodule/status", async (HttpContext context) =>
{
    try
    {
        // 1. Authenticate
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Authentication not available" 
            });
            return;
        }

        // 2. Validate token
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Insufficient permissions" 
            });
            return;
        }

        // 3. Get module instance
        var myModule = moduleManager.Modules
            .Select(m => m.Instance)
            .OfType<MyCustomModule>()
            .FirstOrDefault();

        if (myModule == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Module not found" 
            });
            return;
        }

        // 4. Get module data
        var status = new
        {
            success = true,
            totalItems = myModule.GetTotalItems(),
            activeUsers = myModule.GetActiveUsers(),
            status = "Operational"
        };

        await context.Response.WriteAsJsonAsync(status);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { 
            success = false, 
            message = ex.Message 
        });
    }
});
```

### Standard Endpoint Patterns

#### GET Status Endpoint
```http
GET /api/{modulename}/status
Authorization: Bearer <token>

Response:
{
  "success": true,
  "status": "Operational",
  "statistics": { ... }
}
```

#### POST Action Endpoint
```http
POST /api/{modulename}/action
Authorization: Bearer <token>
Content-Type: application/json

{
  "parameter": "value"
}

Response:
{
  "success": true,
  "message": "Action completed"
}
```

---

## Permission System

### Role-Based Access Control

The control panel supports role-based permissions:

```javascript
const MODULE_TABS = {
    'MyCustomModule': {
        requiredRole: 'Admin',  // User, Admin, or SuperAdmin
        // ...
    }
};
```

**Available Roles (in order of privilege):**
1. `User` - Basic authenticated users
2. `Admin` - Administrative users
3. `SuperAdmin` - Super administrators (full access)

### Permission Checking in Backend

```csharp
// Check user role
if (user == null || user.Role < UserRole.Admin)
{
    context.Response.StatusCode = 403;
    await context.Response.WriteAsJsonAsync(new { 
        success = false, 
        message = "Insufficient permissions" 
    });
    return;
}
```

### Permission Checking in Frontend

```javascript
// Check if current user has permission
async function renderMyCustomModuleTab(container) {
    const currentUser = await getCurrentUser();
    
    if (currentUser.role < 'Admin') {
        container.innerHTML = `
            <p class="error">Insufficient permissions to view this module</p>
        `;
        return;
    }
    
    // Render normal content
    // ...
}
```

---

## UI/UX Guidelines

### Color Scheme

Use the standard RaCore color palette:

```css
/* Primary Colors */
--primary: #667eea;        /* Purple */
--primary-dark: #5568d3;   /* Dark Purple */
--secondary: #764ba2;      /* Accent Purple */

/* Status Colors */
--success: #10b981;        /* Green */
--warning: #f59e0b;        /* Orange */
--error: #ef4444;          /* Red */
--info: #3b82f6;           /* Blue */

/* Neutral Colors */
--text-primary: #1a202c;   /* Dark Text */
--text-secondary: #666;    /* Gray Text */
--background: #f7fafc;     /* Light Background */
--card-bg: #ffffff;        /* Card Background */
```

### Standard Component Styles

#### Module Card
```html
<div class="module-card">
    <h3>Feature Title</h3>
    <p>Feature description</p>
    <button onclick="handleAction()">Action Button</button>
</div>
```

#### Stats Grid
```html
<div class="stats-grid">
    <div class="stat-card">
        <h3>123</h3>
        <p>Metric Name</p>
    </div>
</div>
```

#### Status Badge
```html
<span class="module-status active">Active</span>
<span class="module-status inactive">Inactive</span>
```

#### Action Buttons
```html
<button class="btn-primary" onclick="handleAction()">Primary Action</button>
<button class="btn-secondary" onclick="handleAction()">Secondary Action</button>
```

### Responsive Design

Follow mobile-first responsive patterns:

```css
/* Mobile: Single column */
.modules-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 20px;
}

/* Tablet: 2 columns */
@media (min-width: 768px) {
    .modules-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

/* Desktop: 3 columns */
@media (min-width: 1024px) {
    .modules-grid {
        grid-template-columns: repeat(3, 1fr);
    }
}
```

### Animation Guidelines

Use consistent animations:

```css
/* Tab switching */
.tab-content.active {
    animation: fadeIn 0.3s ease-in;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

/* Button hover */
button:hover {
    transform: translateY(-2px);
    transition: transform 0.2s ease;
}

/* Card hover */
.module-card:hover {
    box-shadow: 0 8px 16px rgba(0,0,0,0.1);
    transition: box-shadow 0.3s ease;
}
```

---

## Complete Example

### Step 1: Create Module (C#)

```csharp
using RaCore.Engine.Manager;
using Abstractions;

[RaModule(Category = "extensions")]
public class NotificationModule : ModuleBase
{
    public override string Name => "Notifications";
    public override string Version => "1.0.0";
    
    private List<string> _notifications = new();
    
    public int GetNotificationCount() => _notifications.Count;
    
    public List<string> GetRecentNotifications(int count = 10) 
        => _notifications.TakeLast(count).ToList();
    
    public void AddNotification(string message)
    {
        _notifications.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }
}
```

### Step 2: Add API Endpoint (Program.cs)

```csharp
// Notifications: Get Status (Admin+)
app.MapGet("/api/notifications/status", async (HttpContext context) =>
{
    try
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            return;
        }

        var notificationModule = moduleManager.Modules
            .Select(m => m.Instance)
            .OfType<NotificationModule>()
            .FirstOrDefault();

        if (notificationModule == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            totalNotifications = notificationModule.GetNotificationCount(),
            recent = notificationModule.GetRecentNotifications(5)
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { 
            success = false, 
            message = ex.Message 
        });
    }
});
```

### Step 3: Add Tab Definition (WwwrootGenerator.cs)

```csharp
private void GenerateControlPanelUiJs(string jsPath)
{
    var jsContent = $@"
const MODULE_TABS = {{
    // ... existing tabs ...
    
    'Notifications': {{ 
        category: 'extensions',
        icon: '🔔',
        requiredRole: 'Admin',
        render: renderNotificationsTab
    }}
}};

// ... rest of code ...

async function renderNotificationsTab(container) {{
    container.innerHTML = '<p class=\""loading\"">Loading notifications...</p>';
    
    try {{
        const data = await api.get('/api/notifications/status');
        
        if (data.success) {{
            const notificationsList = data.recent
                .map(n => `<div class=\""notification-item\"">${{n}}</div>`)
                .join('');
            
            container.innerHTML = `
                <h2 style=\""color: #667eea; margin-bottom: 20px;\"">
                    🔔 Notifications
                </h2>
                
                <div class=\""stats-grid\"">
                    <div class=\""stat-card\"">
                        <h3>${{data.totalNotifications}}</h3>
                        <p>Total Notifications</p>
                    </div>
                </div>
                
                <div style=\""margin-top: 30px;\"">
                    <h3>Recent Notifications</h3>
                    <div class=\""notifications-list\"">
                        ${{notificationsList || '<p>No notifications</p>'}}
                    </div>
                </div>
            `;
        }}
    }} catch (error) {{
        container.innerHTML = `
            <p class=\""error\"">Failed to load notifications: ${{error.message}}</p>
        `;
    }}
}}

// ... rest of code ...
";
    
    File.WriteAllText(jsPath, jsContent);
}
```

### Step 4: Test Your Module

1. **Build and run RaCore:**
   ```bash
   dotnet build
   dotnet run
   ```

2. **Access control panel:**
   - Navigate to `http://localhost:5000/login.html`
   - Login with admin credentials
   - Click on "Notifications" tab

3. **Verify functionality:**
   - Tab appears with correct icon
   - Data loads from API
   - UI matches design guidelines

---

## Best Practices

### 1. Error Handling

Always handle errors gracefully in both backend and frontend:

```javascript
async function renderMyModuleTab(container) {
    try {
        const data = await api.get('/api/mymodule/status');
        // ... render content
    } catch (error) {
        console.error('Module error:', error);
        container.innerHTML = `
            <div class="error-message">
                <h3>⚠️ Error Loading Module</h3>
                <p>${error.message}</p>
                <button onclick="renderMyModuleTab(document.getElementById('tab-MyModule'))">
                    Retry
                </button>
            </div>
        `;
    }
}
```

### 2. Loading States

Show loading states for async operations:

```javascript
async function renderMyModuleTab(container) {
    // 1. Show loading
    container.innerHTML = '<div class="loading-spinner">Loading...</div>';
    
    // 2. Fetch data
    const data = await fetchModuleData();
    
    // 3. Render content
    container.innerHTML = renderContent(data);
}
```

### 3. Caching

Cache data when appropriate:

```javascript
let moduleDataCache = null;
let cacheExpiry = null;

async function renderMyModuleTab(container) {
    const now = Date.now();
    
    // Use cache if valid
    if (moduleDataCache && cacheExpiry > now) {
        renderContent(container, moduleDataCache);
        return;
    }
    
    // Fetch fresh data
    const data = await fetchModuleData();
    moduleDataCache = data;
    cacheExpiry = now + (5 * 60 * 1000); // 5 minutes
    
    renderContent(container, data);
}
```

### 4. Consistent Naming

Follow naming conventions:

- **Module name:** PascalCase (e.g., `MyCustomModule`)
- **Tab render function:** `render{ModuleName}Tab` (e.g., `renderMyCustomModuleTab`)
- **API endpoints:** `/api/{modulename}/...` (lowercase)
- **CSS classes:** kebab-case (e.g., `module-card`)

---

## Troubleshooting

### Tab Not Appearing

1. **Check module registration:**
   - Verify `[RaModule]` attribute is present
   - Check module appears in `/api/control/modules` response

2. **Check tab definition:**
   - Ensure module name matches tab key exactly
   - Verify category matches module category
   - Check render function is defined

3. **Check permissions:**
   - Verify user has required role
   - Check `requiredRole` is set correctly

### API Endpoint Not Working

1. **Check authentication:**
   - Verify token is passed in Authorization header
   - Check token is valid and not expired

2. **Check permissions:**
   - Ensure user has required role
   - Verify permission check in endpoint code

3. **Check module availability:**
   - Confirm module is loaded by ModuleManager
   - Verify module instance can be retrieved

### UI Not Rendering Correctly

1. **Check HTML structure:**
   - Verify HTML is valid
   - Check for unclosed tags

2. **Check CSS classes:**
   - Ensure classes exist in control panel styles
   - Verify class names are correct

3. **Check JavaScript errors:**
   - Open browser console
   - Look for JavaScript errors

---

## Resources

- **CONTROL_PANEL_DEVELOPER_GUIDE.md** - Step-by-step implementation guide
- **PHASE9_3_3_SUMMARY.md** - Phase 9.3.3 implementation details
- **MODULE_DEVELOPMENT_GUIDE.md** - General module development guide
- **AUTHENTICATION_QUICKSTART.md** - Authentication and permissions

---

**Version:** 9.3.4  
**Last Updated:** January 2025  
**Maintained By:** RaCore Development Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
