# Phase 9.3.4 Quick Reference

**Version:** 9.3.4  
**Last Updated:** January 2025

---

## 📚 Documentation Index

### For Module Developers

| Document | Purpose | Use When |
|----------|---------|----------|
| **CONTROL_PANEL_MODULE_API.md** | Complete API reference | You need detailed technical specs |
| **CONTROL_PANEL_DEVELOPER_GUIDE.md** | Step-by-step integration guide | You're building a new module tab |
| **MODULE_DEVELOPMENT_GUIDE.md** | General module development | You're creating any RaCore module |
| **PHASE9_3_4_SUMMARY.md** | Implementation summary | You want an overview of Phase 9.3.4 |

### For End Users

| Document | Purpose | Use When |
|----------|---------|----------|
| **LEGENDARY_CLIENTBUILDER_WEB_INTERFACE.md** | Client builder web interface guide | Using the client builder dashboard |
| **PHASE9_3_3_SUMMARY.md** | Control panel features | Learning about the control panel |

---

## 🚀 Quick Start: Add a Custom Tab (5 Minutes)

### Step 1: Create Your Module

```csharp
[RaModule(Category = "extensions")]
public class MyModule : ModuleBase
{
    public override string Name => "MyModule";
    public override string Version => "1.0.0";
    
    public int GetCount() => 42; // Your module logic
}
```

### Step 2: Add API Endpoint

In `RaCore/Program.cs`:

```csharp
app.MapGet("/api/mymodule/status", async (HttpContext context) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
    var user = await authModule.GetUserByTokenAsync(token);
    
    if (user == null || user.Role < UserRole.Admin)
    {
        context.Response.StatusCode = 403;
        return;
    }
    
    var myModule = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<MyModule>()
        .FirstOrDefault();
    
    await context.Response.WriteAsJsonAsync(new
    {
        success = true,
        count = myModule?.GetCount() ?? 0
    });
});
```

### Step 3: Add Tab Definition

In `RaCore/Modules/Extensions/SiteBuilder/WwwrootGenerator.cs`, add to `MODULE_TABS`:

```csharp
'MyModule': { 
    category: 'extensions',
    icon: '🎮',
    requiredRole: 'Admin',
    render: renderMyModuleTab
}
```

### Step 4: Add Render Function

In the same file, add:

```csharp
async function renderMyModuleTab(container) {{
    container.innerHTML = '<p class=\""loading\"">Loading...</p>';
    
    try {{
        const data = await fetch('/api/mymodule/status', {{
            headers: {{ 'Authorization': `Bearer ${{localStorage.getItem('racore_token')}}` }}
        }}).then(r => r.json());
        
        container.innerHTML = `
            <h2 style=\""color: #667eea;\"">🎮 My Module</h2>
            <div class=\""stat-card\"">
                <h3>${{data.count}}</h3>
                <p>Items</p>
            </div>
        `;
    }} catch (error) {{
        container.innerHTML = `<p class=\""error\"">${{error.message}}</p>`;
    }}
}}
```

### Step 5: Build and Test

```bash
dotnet build
dotnet run
# Navigate to http://localhost:5000/control-panel.html
```

Done! Your module now has a custom tab in the control panel.

---

## 🎨 UI Component Quick Reference

### Stats Grid

```html
<div class="stats-grid">
    <div class="stat-card">
        <h3>123</h3>
        <p>Metric Name</p>
    </div>
</div>
```

### Module Grid

```html
<div class="modules-grid">
    <div class="module-card">
        <h3>Feature</h3>
        <p>Description</p>
        <button onclick="action()">Action</button>
    </div>
</div>
```

### Status Badge

```html
<span class="module-status active">Active</span>
<span class="module-status inactive">Inactive</span>
```

### Loading State

```html
<p class="loading">Loading data...</p>
```

### Error Message

```html
<div class="error-message">
    <h3>⚠️ Error</h3>
    <p>Error description</p>
</div>
```

---

## 🎨 Color Palette

```css
Primary:    #667eea  /* Purple - main theme color */
Success:    #10b981  /* Green - success states */
Warning:    #f59e0b  /* Orange - warnings */
Error:      #ef4444  /* Red - errors */
Text:       #1a202c  /* Dark - primary text */
Secondary:  #666     /* Gray - secondary text */
Background: #f7fafc  /* Light - background */
```

---

## 🔐 Permission Levels

| Role | Privilege | Use Case |
|------|-----------|----------|
| `User` | Basic access | Regular authenticated users |
| `Admin` | Administrative | Module administrators |
| `SuperAdmin` | Full access | System administrators |

---

## 📡 API Endpoint Patterns

### GET Endpoint

```csharp
app.MapGet("/api/{module}/status", async (HttpContext context) =>
{
    // 1. Check auth module
    if (authModule == null) { /* 503 */ }
    
    // 2. Validate token
    var token = /* extract from header */;
    var user = await authModule.GetUserByTokenAsync(token);
    
    // 3. Check permissions
    if (user == null || user.Role < UserRole.Admin) { /* 403 */ }
    
    // 4. Get module
    var module = moduleManager.Modules
        .Select(m => m.Instance)
        .OfType<YourModule>()
        .FirstOrDefault();
    
    // 5. Return data
    await context.Response.WriteAsJsonAsync(new { /* data */ });
});
```

### POST Endpoint

```csharp
app.MapPost("/api/{module}/action", async (HttpContext context) =>
{
    // Same auth/permission checks as GET
    
    // Get request body
    var body = await context.Request.ReadFromJsonAsync<Dictionary<string, object>>();
    
    // Process action
    var result = module.DoSomething(body);
    
    // Return result
    await context.Response.WriteAsJsonAsync(new { success = true, result });
});
```

---

## 📋 Common Tasks

### Add Real-Time Updates

```javascript
async function renderMyModuleTab(container) {
    let updateInterval;
    
    const update = async () => {
        const data = await fetch('/api/mymodule/status', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('racore_token')}` }
        }).then(r => r.json());
        
        document.getElementById('count').textContent = data.count;
    };
    
    container.innerHTML = `<h3 id="count">0</h3>`;
    
    await update();
    updateInterval = setInterval(update, 5000); // Update every 5s
    
    container.addEventListener('removed', () => clearInterval(updateInterval));
}
```

### Handle Errors Gracefully

```javascript
async function renderMyModuleTab(container) {
    try {
        const data = await fetch('/api/mymodule/status', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('racore_token')}` }
        }).then(r => r.json());
        
        if (!data.success) throw new Error(data.message);
        
        // Render content
    } catch (error) {
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

### Cache Data

```javascript
let cachedData = null;
let cacheExpiry = null;

async function renderMyModuleTab(container) {
    const now = Date.now();
    
    if (cachedData && cacheExpiry > now) {
        renderContent(container, cachedData);
        return;
    }
    
    const data = await fetchData();
    cachedData = data;
    cacheExpiry = now + (5 * 60 * 1000); // 5 min cache
    
    renderContent(container, data);
}
```

---

## 🐛 Troubleshooting

### Tab Not Appearing

1. ✅ Module has `[RaModule]` attribute
2. ✅ Module appears in `/api/control/modules`
3. ✅ Tab name matches module name exactly
4. ✅ User has required role
5. ✅ Category matches module category

### API Returns 403

1. ✅ Token is valid (check localStorage)
2. ✅ User is logged in
3. ✅ User has Admin role (or higher)
4. ✅ Permission check in endpoint is correct

### UI Not Rendering

1. ✅ HTML is valid (no unclosed tags)
2. ✅ CSS classes exist
3. ✅ JavaScript has no errors (check console)
4. ✅ API endpoint returns valid JSON

---

## 📦 File Locations

### Core Files

```
RaCore/
├── Program.cs                                    # Add API endpoints here
├── Modules/Extensions/SiteBuilder/
│   └── WwwrootGenerator.cs                      # Add tab definitions here
└── Modules/Extensions/{YourModule}/
    └── {YourModule}Module.cs                    # Your module implementation
```

### Documentation Files

```
TheRaProject/
├── CONTROL_PANEL_MODULE_API.md                  # API reference
├── CONTROL_PANEL_DEVELOPER_GUIDE.md             # Developer guide
├── LEGENDARY_CLIENTBUILDER_WEB_INTERFACE.md     # Web interface docs
├── MODULE_DEVELOPMENT_GUIDE.md                  # Module dev guide
├── PHASE9_3_4_SUMMARY.md                        # Implementation summary
└── PHASE9_3_4_QUICKREF.md                       # This file
```

---

## 🔗 Related Documentation

- **PHASE9_3_3_SUMMARY.md** - Control panel architecture
- **AUTHENTICATION_QUICKSTART.md** - Authentication guide
- **LegendaryClientBuilder/README.md** - Client builder module
- **ARCHITECTURE.md** - System architecture

---

## 💡 Best Practices

### DO ✅

- Use standard UI components
- Follow color palette
- Handle errors gracefully
- Show loading states
- Cache when appropriate
- Validate permissions
- Document your module

### DON'T ❌

- Modify core control panel code unnecessarily
- Use conflicting CSS
- Ignore error handling
- Block UI with sync operations
- Store secrets in frontend
- Skip permission checks

---

## 🎯 Success Checklist

Building a new control panel module tab:

- [ ] Module created with `[RaModule]` attribute
- [ ] API endpoint added with auth/permission checks
- [ ] Tab definition added to MODULE_TABS
- [ ] Render function implemented
- [ ] UI uses standard components
- [ ] Error handling implemented
- [ ] Loading states added
- [ ] Tested on mobile/tablet/desktop
- [ ] Documentation updated
- [ ] Code reviewed and tested

---

## 📞 Get Help

1. **Check Documentation:**
   - CONTROL_PANEL_MODULE_API.md
   - CONTROL_PANEL_DEVELOPER_GUIDE.md
   - MODULE_DEVELOPMENT_GUIDE.md

2. **Search Issues:**
   - GitHub Issues
   - GitHub Discussions

3. **Create Issue:**
   - Include error messages
   - Provide steps to reproduce
   - Share relevant code

---

**Version:** 9.3.4  
**Last Updated:** January 2025

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
