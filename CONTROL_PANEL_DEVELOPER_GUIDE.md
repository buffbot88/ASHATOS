# Control Panel Developer Guide

**Version:** 9.4.0  
**Last Updated:** October 2025
**Target Audience:** Third-Party Developers

---

## Introduction

This guide provides step-by-step instructions for integrating your custom module into the RaCore Admin Control Panel. By following this guide, you'll learn how to:

- Register your module with the control panel
- Create a custom tab with your own UI
- Implement API endpoints for data exchange
- Maintain consistent UI/UX with other modules
- Handle permissions and security

## Prerequisites

Before you begin, ensure you have:

- ✅ .NET 9.0 SDK installed
- ✅ Basic knowledge of C# and JavaScript
- ✅ Familiarity with RaOS module development ([MODULE_DEVELOPMENT_GUIDE.md](MODULE_DEVELOPMENT_GUIDE.md))
- ✅ A working RaCore development environment

## Table of Contents

1. [Quick Start](#quick-start)
2. [Creating Your Module](#creating-your-module)
3. [Adding Control Panel Integration](#adding-control-panel-integration)
4. [Creating API Endpoints](#creating-api-endpoints)
5. [Designing Your Tab UI](#designing-your-tab-ui)
6. [Testing Your Integration](#testing-your-integration)
7. [Advanced Features](#advanced-features)
8. [Deployment](#deployment)

---

## Quick Start

### 5-Minute Integration

Here's a minimal example to get your module tab working:

**Step 1:** Create your module with `[RaModule]` attribute

```csharp
[RaModule(Category = "extensions")]
public class QuickStartModule : ModuleBase
{
    public override string Name => "QuickStart";
    public override string Version => "1.0.0";
}
```

**Step 2:** Add API endpoint in `Program.cs`

```csharp
app.MapGet("/api/quickstart/status", async (HttpContext context) =>
{
    await context.Response.WriteAsJsonAsync(new { 
        success = true, 
        message = "QuickStart module is working!" 
    });
});
```

**Step 3:** Add tab definition in `WwwrootGenerator.cs`

```csharp
// In GenerateControlPanelUiJs method
'QuickStart': { 
    category: 'extensions',
    icon: '⚡',
    requiredRole: 'Admin',
    render: renderQuickStartTab
}

// Add render function
function renderQuickStartTab(container) {
    container.innerHTML = '<h2>⚡ QuickStart Module</h2><p>Hello from QuickStart!</p>';
}
```

**Step 4:** Build and test

```bash
dotnet build
dotnet run
# Navigate to http://localhost:5000/control-panel.html
```

That's it! Your module now has a tab in the control panel.

---

## Creating Your Module

### Module Structure

Create a new module following the standard RaOS structure:

```csharp
using RaCore.Engine.Manager;
using Abstractions;
using System.Threading.Tasks;

namespace MyCompany.RaCore.Modules
{
    /// <summary>
    /// Analytics module for tracking and reporting system metrics.
    /// </summary>
    [RaModule(Category = "extensions")]
    public class AnalyticsModule : ModuleBase
    {
        public override string Name => "Analytics";
        public override string Version => "1.0.0";
        
        private Dictionary<string, int> _metrics = new();
        
        public override async Task<bool> InitializeAsync()
        {
            Log("Initializing Analytics module...");
            
            // Initialize metrics
            _metrics["pageViews"] = 0;
            _metrics["apiCalls"] = 0;
            _metrics["activeUsers"] = 0;
            
            Log("Analytics module initialized successfully");
            return await Task.FromResult(true);
        }
        
        public void TrackMetric(string name, int value)
        {
            if (_metrics.ContainsKey(name))
                _metrics[name] += value;
            else
                _metrics[name] = value;
        }
        
        public int GetMetric(string name)
        {
            return _metrics.ContainsKey(name) ? _metrics[name] : 0;
        }
        
        public Dictionary<string, int> GetAllMetrics()
        {
            return new Dictionary<string, int>(_metrics);
        }
        
        public override async Task<ModuleResponse> ProcessAsync(string input)
        {
            if (input == "analytics stats")
            {
                return await Task.FromResult(new ModuleResponse
                {
                    Text = System.Text.Json.JsonSerializer.Serialize(GetAllMetrics()),
                    Status = "success"
                });
            }
            
            return await Task.FromResult(new ModuleResponse
            {
                Text = "Unknown command. Try 'analytics stats'",
                Status = "error"
            });
        }
    }
}
```

### Module Categories

Choose the appropriate category for your module:

| Category | Description | Examples |
|----------|-------------|----------|
| `core` | Core system functionality | Memory, Boot, System |
| `extensions` | Extension modules | Authentication, RaCoin, License |
| `clientbuilder` | Client generation related | LegendaryClientBuilder |
| `custom` | Third-party modules | Your custom modules |

---

## Adding Control Panel Integration

### Step 1: Locate WwwrootGenerator.cs

The file is located at:
```
RaCore/Modules/Extensions/SiteBuilder/WwwrootGenerator.cs
```

### Step 2: Add Tab Definition

Find the `GenerateControlPanelUiJs` method and locate the `MODULE_TABS` object:

```csharp
private void GenerateControlPanelUiJs(string jsPath)
{
    var jsContent = $@"
// ... existing code ...

const MODULE_TABS = {{
    'Overview': {{ /* ... */ }},
    'SiteBuilder': {{ /* ... */ }},
    // ... other existing tabs ...
    
    // ADD YOUR TAB HERE
    'Analytics': {{ 
        category: 'extensions',
        icon: '📊',
        requiredRole: 'Admin',
        render: renderAnalyticsTab
    }}
}};

// ... rest of code ...
```

### Step 3: Add Render Function

Add your render function after the MODULE_TABS definition:

```csharp
private void GenerateControlPanelUiJs(string jsPath)
{
    var jsContent = $@"
// ... MODULE_TABS definition ...

// ... existing render functions ...

async function renderAnalyticsTab(container) {{
    // Show loading state
    container.innerHTML = '<p class=\""loading\"">Loading analytics...</p>';
    
    try {{
        // Fetch data from your API endpoint
        const response = await fetch('/api/analytics/status', {{
            headers: {{
                'Authorization': `Bearer ${{localStorage.getItem('racore_token')}}`
            }}
        }});
        
        const data = await response.json();
        
        if (data.success) {{
            // Render your UI
            container.innerHTML = `
                <div style=\""max-width: 1200px;\"">
                    <h2 style=\""color: #667eea; margin-bottom: 20px;\"">
                        📊 Analytics Dashboard
                    </h2>
                    
                    <p style=\""color: #666; margin-bottom: 30px;\"">
                        Track and monitor system metrics in real-time
                    </p>
                    
                    <div class=\""stats-grid\"">
                        <div class=\""stat-card\"">
                            <h3>${{data.metrics.pageViews || 0}}</h3>
                            <p>Page Views</p>
                        </div>
                        
                        <div class=\""stat-card\"">
                            <h3>${{data.metrics.apiCalls || 0}}</h3>
                            <p>API Calls</p>
                        </div>
                        
                        <div class=\""stat-card\"">
                            <h3>${{data.metrics.activeUsers || 0}}</h3>
                            <p>Active Users</p>
                        </div>
                    </div>
                    
                    <div class=\""modules-grid\"" style=\""margin-top: 30px;\"">
                        <div class=\""module-card\"">
                            <h3>📈 View Reports</h3>
                            <p>Generate detailed analytics reports</p>
                            <button onclick=\""handleViewReports()\"">View Reports</button>
                        </div>
                        
                        <div class=\""module-card\"">
                            <h3>🔄 Refresh Data</h3>
                            <p>Update analytics with latest data</p>
                            <button onclick=\""handleRefreshAnalytics()\"">Refresh</button>
                        </div>
                    </div>
                </div>
            `;
        }} else {{
            throw new Error(data.message || 'Failed to load analytics');
        }}
    }} catch (error) {{
        container.innerHTML = `
            <div class=\""error-message\"">
                <h3>⚠️ Error Loading Analytics</h3>
                <p>${{error.message}}</p>
                <button onclick=\""renderAnalyticsTab(document.getElementById('tab-Analytics'))\"">
                    Retry
                </button>
            </div>
        `;
    }}
}}

// Handler functions for your module
function handleViewReports() {{
    alert('Reports feature coming soon!');
}}

function handleRefreshAnalytics() {{
    const container = document.getElementById('tab-Analytics');
    if (container) {{
        renderAnalyticsTab(container);
    }}
}}

// ... rest of code ...
";
    
    File.WriteAllText(jsPath, jsContent);
}
```

---

## Creating API Endpoints

### Step 1: Locate Program.cs

The file is located at:
```
RaCore/Program.cs
```

### Step 2: Add Your Endpoint

Add your API endpoint after the existing control panel endpoints:

```csharp
// ============================================================================
// ANALYTICS MODULE API ENDPOINTS
// ============================================================================

// Analytics: Get Status (Admin+)
app.MapGet("/api/analytics/status", async (HttpContext context) =>
{
    try
    {
        // 1. Check authentication module availability
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Authentication not available" 
            });
            return;
        }

        // 2. Extract and validate token
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        // 3. Check permissions
        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Insufficient permissions - Admin role required" 
            });
            return;
        }

        // 4. Get analytics module instance
        var analyticsModule = moduleManager.Modules
            .Select(m => m.Instance)
            .OfType<AnalyticsModule>()
            .FirstOrDefault();

        if (analyticsModule == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Analytics module not found" 
            });
            return;
        }

        // 5. Get metrics data
        var metrics = analyticsModule.GetAllMetrics();

        // 6. Return response
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            metrics = metrics,
            timestamp = DateTime.UtcNow,
            version = analyticsModule.Version
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

// Analytics: Track Metric (Admin+)
app.MapPost("/api/analytics/track", async (HttpContext context) =>
{
    try
    {
        // Authentication and permission checks (same as above)
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Authentication not available" 
            });
            return;
        }

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

        // Get request body
        var requestBody = await context.Request.ReadFromJsonAsync<Dictionary<string, object>>();
        if (requestBody == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Invalid request body" 
            });
            return;
        }

        var metricName = requestBody.ContainsKey("name") 
            ? requestBody["name"].ToString() 
            : null;
        var metricValue = requestBody.ContainsKey("value") 
            ? int.Parse(requestBody["value"].ToString() ?? "0") 
            : 0;

        if (string.IsNullOrEmpty(metricName))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Metric name is required" 
            });
            return;
        }

        // Get module and track metric
        var analyticsModule = moduleManager.Modules
            .Select(m => m.Instance)
            .OfType<AnalyticsModule>()
            .FirstOrDefault();

        if (analyticsModule == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { 
                success = false, 
                message = "Analytics module not found" 
            });
            return;
        }

        analyticsModule.TrackMetric(metricName, metricValue);

        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            message = $"Metric '{metricName}' tracked successfully"
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

### Endpoint Checklist

Ensure your endpoints include:

- ✅ Authentication check (`authModule` availability)
- ✅ Token extraction and validation
- ✅ Permission check (role-based)
- ✅ Module instance retrieval
- ✅ Error handling with appropriate status codes
- ✅ JSON response format

---

## Designing Your Tab UI

### UI Component Library

The control panel provides these built-in components:

#### 1. Stats Grid

```html
<div class="stats-grid">
    <div class="stat-card">
        <h3>123</h3>
        <p>Metric Name</p>
    </div>
</div>
```

#### 2. Module Grid

```html
<div class="modules-grid">
    <div class="module-card">
        <h3>Feature Title</h3>
        <p>Feature description</p>
        <button onclick="handleAction()">Action</button>
    </div>
</div>
```

#### 3. Status Badges

```html
<span class="module-status active">Active</span>
<span class="module-status inactive">Inactive</span>
```

#### 4. Loading State

```html
<p class="loading">Loading data...</p>
```

#### 5. Error Message

```html
<div class="error-message">
    <h3>⚠️ Error Title</h3>
    <p>Error description</p>
</div>
```

### Color Palette

Use these colors to maintain consistency:

```javascript
const colors = {
    primary: '#667eea',        // Purple
    primaryDark: '#5568d3',    // Dark Purple
    secondary: '#764ba2',      // Accent Purple
    success: '#10b981',        // Green
    warning: '#f59e0b',        // Orange
    error: '#ef4444',          // Red
    text: '#1a202c',           // Dark Text
    textSecondary: '#666',     // Gray Text
    background: '#f7fafc',     // Light Background
};
```

### Responsive Design

The control panel uses a responsive grid system:

```javascript
// Your tab should adapt to screen size:
// Mobile: 1 column
// Tablet: 2 columns  
// Desktop: 3 columns

const renderResponsive = () => `
    <div class="modules-grid">
        ${items.map(item => `
            <div class="module-card">
                <h3>${item.title}</h3>
                <p>${item.description}</p>
            </div>
        `).join('')}
    </div>
`;
```

---

## Testing Your Integration

### Local Testing

1. **Build your module:**
   ```bash
   dotnet build
   ```

2. **Run RaCore:**
   ```bash
   cd RaCore
   dotnet run
   ```

3. **Access control panel:**
   - Navigate to `http://localhost:5000/login.html`
   - Login with admin credentials (default: admin/admin123)
   - Click on your module tab

### Verification Checklist

- ✅ Module appears in module list (`/api/control/modules`)
- ✅ Tab button appears in control panel
- ✅ Tab icon displays correctly
- ✅ Tab content loads without errors
- ✅ API endpoints respond correctly
- ✅ UI matches design guidelines
- ✅ Responsive design works on mobile/tablet/desktop
- ✅ Permissions are enforced correctly

### Debugging Tips

#### Check Browser Console

Open browser DevTools (F12) and check:

```javascript
// 1. Check if module is loaded
fetch('/api/control/modules', {
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('racore_token') }
})
.then(r => r.json())
.then(d => console.log(d.modules));

// 2. Check if your API endpoint works
fetch('/api/analytics/status', {
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('racore_token') }
})
.then(r => r.json())
.then(d => console.log(d));
```

#### Check RaCore Logs

Monitor RaCore console output for:

```
[Analytics] Initializing Analytics module...
[Analytics] Analytics module initialized successfully
```

#### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Tab not appearing | Module category mismatch | Check category in `[RaModule]` matches tab definition |
| "Module not found" error | Module not loaded | Verify module initializes successfully |
| Permission denied | User role insufficient | Check `requiredRole` in tab definition |
| API returns 404 | Endpoint not registered | Verify endpoint is added in Program.cs |

---

## Advanced Features

### Real-Time Updates

Implement auto-refresh for live data:

```javascript
async function renderAnalyticsTab(container) {
    let updateInterval;
    
    const update = async () => {
        const data = await fetch('/api/analytics/status', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('racore_token')}` }
        }).then(r => r.json());
        
        // Update only the stats, not the entire UI
        document.getElementById('pageViews').textContent = data.metrics.pageViews;
        document.getElementById('apiCalls').textContent = data.metrics.apiCalls;
    };
    
    // Initial render
    container.innerHTML = `
        <div class="stats-grid">
            <div class="stat-card">
                <h3 id="pageViews">0</h3>
                <p>Page Views</p>
            </div>
            <div class="stat-card">
                <h3 id="apiCalls">0</h3>
                <p>API Calls</p>
            </div>
        </div>
    `;
    
    // Update every 5 seconds
    await update();
    updateInterval = setInterval(update, 5000);
    
    // Cleanup when tab is switched
    container.addEventListener('removed', () => {
        clearInterval(updateInterval);
    });
}
```

### Charts and Visualizations

Add chart libraries for data visualization:

```javascript
async function renderAnalyticsTab(container) {
    // Include Chart.js (add to control panel HTML)
    container.innerHTML = `
        <div style="max-width: 800px;">
            <h2>📊 Analytics Dashboard</h2>
            <canvas id="analyticsChart"></canvas>
        </div>
    `;
    
    // Fetch data
    const data = await fetch('/api/analytics/history', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('racore_token')}` }
    }).then(r => r.json());
    
    // Create chart
    new Chart(document.getElementById('analyticsChart'), {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Page Views',
                data: data.values,
                borderColor: '#667eea',
                backgroundColor: 'rgba(102, 126, 234, 0.1)'
            }]
        }
    });
}
```

### Modal Dialogs

Implement modal dialogs for complex interactions:

```javascript
function showConfigDialog() {
    const modal = document.createElement('div');
    modal.innerHTML = `
        <div style="position: fixed; top: 0; left: 0; right: 0; bottom: 0; 
                    background: rgba(0,0,0,0.5); display: flex; 
                    align-items: center; justify-content: center; z-index: 1000;">
            <div style="background: white; padding: 30px; border-radius: 8px; 
                        max-width: 500px; width: 100%;">
                <h3>Configure Analytics</h3>
                <form id="configForm">
                    <label>Update Interval (seconds):</label>
                    <input type="number" name="interval" value="5" min="1" max="60">
                    
                    <div style="margin-top: 20px;">
                        <button type="submit">Save</button>
                        <button type="button" onclick="this.closest('[style*=fixed]').remove()">
                            Cancel
                        </button>
                    </div>
                </form>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    modal.querySelector('form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        
        await fetch('/api/analytics/config', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('racore_token')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                interval: formData.get('interval')
            })
        });
        
        modal.remove();
    });
}
```

---

## Deployment

### Production Build

1. **Build in Release mode:**
   ```bash
   dotnet build -c Release
   ```

2. **Test in production mode:**
   ```bash
   cd RaCore/bin/Release/net9.0
   ./RaCore
   ```

3. **Verify your module:**
   - Check module loads correctly
   - Test all API endpoints
   - Verify UI renders properly
   - Test permissions

### Distribution

#### Option 1: Include in RaCore

Add your module to the main RaCore project:

```bash
# Copy module files to RaCore/Modules/Extensions/
cp -r MyModule/ RaCore/Modules/Extensions/MyModule/
```

#### Option 2: External DLL

Build as separate DLL and distribute:

```bash
# Build your module
dotnet build -c Release

# Package DLL
cp bin/Release/net9.0/MyModule.dll release/
zip MyModule-1.0.0.zip release/MyModule.dll README.md
```

#### Option 3: NuGet Package (Future)

```bash
# Create NuGet package
dotnet pack -c Release

# Publish to NuGet
dotnet nuget push bin/Release/MyModule.1.0.0.nupkg
```

### Version Management

Follow semantic versioning:

```csharp
public override string Version => "1.0.0";
// MAJOR.MINOR.PATCH
// 1 = Breaking changes
// 0 = New features (backward compatible)
// 0 = Bug fixes
```

---

## Best Practices Summary

### DO ✅

- Use consistent naming conventions
- Follow the standard UI component library
- Handle errors gracefully
- Show loading states for async operations
- Cache data when appropriate
- Document your API endpoints
- Test on multiple screen sizes
- Validate user permissions
- Use semantic versioning

### DON'T ❌

- Modify core control panel files unnecessarily
- Use custom CSS that conflicts with existing styles
- Ignore error handling
- Block the UI with synchronous operations
- Store sensitive data in frontend code
- Skip permission checks
- Use inline styles excessively
- Forget to cleanup resources (intervals, listeners)

---

## Example: Complete Implementation

See the full example in **CONTROL_PANEL_MODULE_API.md** for a complete, working implementation of the Analytics module.

---

## Support & Resources

### Documentation

- **CONTROL_PANEL_MODULE_API.md** - Complete API reference
- **MODULE_DEVELOPMENT_GUIDE.md** - General module development
- **PHASE9_3_3_SUMMARY.md** - Control panel architecture details
- **AUTHENTICATION_QUICKSTART.md** - Authentication guide

### Community

- **GitHub Issues** - Report bugs and request features
- **GitHub Discussions** - Ask questions and share ideas
- **Documentation** - Browse all available guides

### Getting Help

If you encounter issues:

1. Check this guide and the API reference
2. Search existing GitHub issues
3. Ask in GitHub Discussions
4. Create a new issue with:
   - Your module code
   - Error messages
   - Steps to reproduce

---

## What's Next?

After integrating your module:

1. **Add advanced features** (charts, real-time updates, etc.)
2. **Optimize performance** (caching, lazy loading)
3. **Enhance UX** (animations, transitions)
4. **Add tests** (unit tests, integration tests)
5. **Document your module** (README, API docs)
6. **Share with community** (GitHub, marketplace)

---

**Version:** 9.4.0  
**Last Updated:** October 2025  
**Maintained By:** RaCore Development Team

---

**Happy Coding! 🚀**

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
