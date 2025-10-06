namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates wwwroot directory and control panel files for RaCore server.
/// </summary>
public class WwwrootGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _wwwrootPath;

    public WwwrootGenerator(SiteBuilderModule module, string wwwrootPath)
    {
        _module = module;
        _wwwrootPath = wwwrootPath;
    }

    public string GenerateWwwroot()
    {
        try
        {
            _module.Log("Starting wwwroot generation...");
            
            // Create wwwroot directory
            Directory.CreateDirectory(_wwwrootPath);
            
            // Create js subdirectory
            var jsPath = Path.Combine(_wwwrootPath, "js");
            Directory.CreateDirectory(jsPath);
            
            // Generate files
            GenerateIndexHtml();
            GenerateLoginHtml();
            GenerateControlPanelHtml();
            GenerateAdminHtml();
            GenerateGameEngineDashboardHtml();
            GenerateControlPanelModulesMd();
            GenerateControlPanelApiJs(jsPath);
            GenerateControlPanelUiJs(jsPath);
            
            _module.Log($"✅ wwwroot generated successfully at: {_wwwrootPath}");
            
            return $@"✅ wwwroot directory generated successfully!

📁 Location: {_wwwrootPath}

Generated files:
  - index.html
  - login.html
  - control-panel.html
  - admin.html
  - gameengine-dashboard.html
  - js/control-panel-api.js
  - js/control-panel-ui.js
  - CONTROL_PANEL_MODULES.md";
        }
        catch (Exception ex)
        {
            _module.Log($"wwwroot generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }

    private void GenerateIndexHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore - AI Mainframe</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .container {
            background: white;
            padding: 60px;
            border-radius: 15px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
            max-width: 600px;
            text-align: center;
        }

        h1 {
            color: #667eea;
            margin-bottom: 20px;
            font-size: 42px;
        }

        p {
            color: #666;
            font-size: 18px;
            margin-bottom: 30px;
            line-height: 1.6;
        }

        .button {
            display: inline-block;
            padding: 15px 40px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            transition: transform 0.2s;
        }

        .button:hover {
            transform: translateY(-2px);
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>🌟 RaCore AI Mainframe</h1>
        <p>Welcome to RaCore - Your AI-powered modular framework</p>
        <a href=""/control-panel.html"" class=""button"">Access Control Panel</a>
    </div>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "index.html"), content);
        _module.Log("Generated index.html");
    }

    private void GenerateLoginHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore - Login</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .login-container {
            background: white;
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
            max-width: 400px;
            width: 100%;
        }

        h1 {
            color: #667eea;
            margin-bottom: 10px;
            font-size: 28px;
        }

        h3 {
            color: #666;
            margin-bottom: 30px;
            font-weight: normal;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            display: block;
            color: #333;
            font-weight: 600;
            margin-bottom: 5px;
        }

        input {
            width: 100%;
            padding: 12px;
            border: 2px solid #ddd;
            border-radius: 8px;
            font-size: 14px;
            transition: border-color 0.3s;
        }

        input:focus {
            outline: none;
            border-color: #667eea;
        }

        button {
            width: 100%;
            padding: 12px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s;
        }

        button:hover {
            transform: translateY(-2px);
        }

        .error {
            color: #e74c3c;
            margin-top: 15px;
            font-size: 14px;
            display: none;
        }

        .success {
            color: #27ae60;
            margin-top: 15px;
            font-size: 14px;
            display: none;
        }
    </style>
</head>
<body>
    <div class=""login-container"">
        <h1>🔐 Login</h1>
        <h3>RaCore Control Panel</h3>
        
        <form id=""loginForm"">
            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""username"" required>
            </div>
            
            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""password"" id=""password"" name=""password"" required>
            </div>
            
            <button type=""submit"">Login</button>
            
            <div class=""error"" id=""error""></div>
            <div class=""success"" id=""success""></div>
        </form>
    </div>

    <script>
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const errorDiv = document.getElementById('error');
            const successDiv = document.getElementById('success');
            
            errorDiv.style.display = 'none';
            successDiv.style.display = 'none';
            
            try {
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ username, password })
                });
                
                const data = await response.json();
                
                if (data.success && data.token) {
                    localStorage.setItem('racore_token', data.token);
                    successDiv.textContent = 'Login successful! Redirecting...';
                    successDiv.style.display = 'block';
                    setTimeout(() => {
                        window.location.href = '/control-panel.html';
                    }, 1000);
                } else {
                    errorDiv.textContent = data.message || 'Login failed';
                    errorDiv.style.display = 'block';
                }
            } catch (error) {
                errorDiv.textContent = 'Connection error: ' + error.message;
                errorDiv.style.display = 'block';
            }
        });
    </script>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "login.html"), content);
        _module.Log("Generated login.html");
    }

    private void GenerateControlPanelHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Control Panel - Modular</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
        }

        .login-container {
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            padding: 20px;
        }

        .login-box {
            background: white;
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
            max-width: 400px;
            width: 100%;
        }

        .control-panel-container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 20px;
        }

        .header {
            background: white;
            padding: 20px 30px;
            border-radius: 15px;
            margin-bottom: 20px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .header h1 {
            color: #667eea;
            font-size: 28px;
        }

        .logout-btn {
            padding: 10px 20px;
            background: #e74c3c;
            color: white;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            font-weight: 600;
        }

        .modules-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 20px;
        }

        .module-card {
            background: white;
            padding: 25px;
            border-radius: 15px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
            cursor: pointer;
            transition: transform 0.2s;
        }

        .module-card:hover {
            transform: translateY(-5px);
        }

        .module-card h3 {
            color: #667eea;
            margin-bottom: 10px;
        }

        .module-card p {
            color: #666;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class=""login-container"" id=""loginView"">
        <div class=""login-box"">
            <h1 style=""color: #667eea; margin-bottom: 10px;"">🎛️ Control Panel</h1>
            <h3 style=""color: #666; margin-bottom: 30px; font-weight: normal;"">Please login to continue</h3>
            <p style=""text-align: center;""><a href=""/login.html"" style=""color: #667eea; text-decoration: none; font-weight: 600;"">Go to Login →</a></p>
        </div>
    </div>

    <div class=""control-panel-container"" id=""panelView"" style=""display: none;"">
        <div class=""header"">
            <h1>🎛️ RaCore Control Panel</h1>
            <button class=""logout-btn"" onclick=""logout()"">Logout</button>
        </div>

        <div class=""modules-grid"" id=""modulesGrid"">
            <!-- Modules will be loaded dynamically -->
        </div>
    </div>

    <script src=""/js/control-panel-api.js""></script>
    <script src=""/js/control-panel-ui.js""></script>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "control-panel.html"), content);
        _module.Log("Generated control-panel.html");
    }

    private void GenerateAdminHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Admin Panel</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        .header {
            background: white;
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 20px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        }

        .header h1 {
            color: #667eea;
            font-size: 32px;
            margin-bottom: 10px;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 20px;
        }

        .stat-card {
            background: white;
            padding: 25px;
            border-radius: 15px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        }

        .stat-card h3 {
            color: #667eea;
            font-size: 36px;
            margin-bottom: 10px;
        }

        .stat-card p {
            color: #666;
            font-size: 14px;
        }

        .content-card {
            background: white;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        }

        .content-card h2 {
            color: #667eea;
            margin-bottom: 20px;
        }

        .back-btn {
            display: inline-block;
            padding: 10px 20px;
            background: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            margin-bottom: 20px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <a href=""/control-panel.html"" class=""back-btn"">← Back to Control Panel</a>
        
        <div class=""header"">
            <h1>⚙️ Admin Panel</h1>
            <p style=""color: #666;"">System administration and management</p>
        </div>

        <div class=""stats-grid"">
            <div class=""stat-card"">
                <h3 id=""totalUsers"">--</h3>
                <p>Total Users</p>
            </div>
            <div class=""stat-card"">
                <h3 id=""totalModules"">--</h3>
                <p>Loaded Modules</p>
            </div>
            <div class=""stat-card"">
                <h3 id=""systemStatus"">--</h3>
                <p>System Status</p>
            </div>
        </div>

        <div class=""content-card"">
            <h2>System Information</h2>
            <p>Admin controls and monitoring features will be available here.</p>
        </div>
    </div>

    <script>
        // Load admin stats
        async function loadStats() {
            const token = localStorage.getItem('racore_token');
            if (!token) {
                window.location.href = '/login.html';
                return;
            }

            try {
                const response = await fetch('/api/control/stats', {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    document.getElementById('totalUsers').textContent = data.totalUsers || '0';
                    document.getElementById('totalModules').textContent = data.modulesLoaded || '0';
                    document.getElementById('systemStatus').textContent = 'Online';
                }
            } catch (error) {
                console.error('Failed to load stats:', error);
            }
        }

        loadStats();
    </script>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "admin.html"), content);
        _module.Log("Generated admin.html");
    }

    private void GenerateGameEngineDashboardHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Game Engine Dashboard</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }

        .container {
            max-width: 1400px;
            margin: 0 auto;
        }

        .header {
            background: white;
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 20px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        }

        .header h1 {
            color: #667eea;
            font-size: 32px;
            margin-bottom: 10px;
        }

        .content-card {
            background: white;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
            margin-bottom: 20px;
        }

        .content-card h2 {
            color: #667eea;
            margin-bottom: 20px;
        }

        .back-btn {
            display: inline-block;
            padding: 10px 20px;
            background: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            margin-bottom: 20px;
        }

        .scenes-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }

        .scene-card {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            border-left: 4px solid #667eea;
        }

        .scene-card h3 {
            color: #333;
            margin-bottom: 10px;
        }

        .scene-card p {
            color: #666;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <a href=""/control-panel.html"" class=""back-btn"">← Back to Control Panel</a>
        
        <div class=""header"">
            <h1>🎮 Game Engine Dashboard</h1>
            <p style=""color: #666;"">Manage game scenes and entities</p>
        </div>

        <div class=""content-card"">
            <h2>Scenes</h2>
            <div class=""scenes-grid"" id=""scenesGrid"">
                <p style=""color: #666;"">Loading scenes...</p>
            </div>
        </div>
    </div>

    <script>
        async function loadScenes() {
            const token = localStorage.getItem('racore_token');
            if (!token) {
                window.location.href = '/login.html';
                return;
            }

            try {
                const response = await fetch('/api/gameengine/scenes', {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    const grid = document.getElementById('scenesGrid');
                    
                    if (data.scenes && data.scenes.length > 0) {
                        grid.innerHTML = data.scenes.map(scene => `
                            <div class=""scene-card"">
                                <h3>${scene.name}</h3>
                                <p>${scene.description || 'No description'}</p>
                            </div>
                        `).join('');
                    } else {
                        grid.innerHTML = '<p style=""color: #666;"">No scenes created yet.</p>';
                    }
                }
            } catch (error) {
                console.error('Failed to load scenes:', error);
                document.getElementById('scenesGrid').innerHTML = '<p style=""color: #e74c3c;"">Failed to load scenes</p>';
            }
        }

        loadScenes();
    </script>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "gameengine-dashboard.html"), content);
        _module.Log("Generated gameengine-dashboard.html");
    }

    private void GenerateControlPanelModulesMd()
    {
        var content = @"# RaCore Control Panel Modules

This document describes the available modules in the RaCore Control Panel.

## Available Modules

### Authentication
- User management
- Role-based access control
- Session management

### Game Engine
- Scene management
- Entity management
- AI-driven game operations

### License Management
- License validation
- Subscription management
- Instance tracking

### RaCoin System
- Virtual currency management
- Transaction tracking
- Balance management

### Server Setup
- Apache configuration
- PHP configuration
- Database management

### Site Builder
- CMS generation
- Control panel generation
- Integrated site management

## Usage

Access the control panel at: `http://localhost:5000/control-panel.html`

Default credentials:
- Username: `admin`
- Password: `admin123`

⚠️ **Change the default password immediately!**
";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "CONTROL_PANEL_MODULES.md"), content);
        _module.Log("Generated CONTROL_PANEL_MODULES.md");
    }

    private void GenerateControlPanelApiJs(string jsPath)
    {
        var content = @"// RaCore Control Panel API Client

class RaCoreAPI {
    constructor() {
        this.baseUrl = '';
        this.token = localStorage.getItem('racore_token');
    }

    async request(endpoint, options = {}) {
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        const response = await fetch(`${this.baseUrl}${endpoint}`, {
            ...options,
            headers
        });

        if (response.status === 401) {
            localStorage.removeItem('racore_token');
            window.location.href = '/login.html';
            throw new Error('Unauthorized');
        }

        return response;
    }

    async getStats() {
        const response = await this.request('/api/control/stats');
        return response.json();
    }

    async getModules() {
        const response = await this.request('/api/control/modules');
        return response.json();
    }

    async login(username, password) {
        const response = await this.request('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        return response.json();
    }

    async logout() {
        await this.request('/api/auth/logout', { method: 'POST' });
        localStorage.removeItem('racore_token');
        window.location.href = '/login.html';
    }

    isAuthenticated() {
        return !!this.token;
    }
}

const api = new RaCoreAPI();
";
        
        File.WriteAllText(Path.Combine(jsPath, "control-panel-api.js"), content);
        _module.Log("Generated control-panel-api.js");
    }

    private void GenerateControlPanelUiJs(string jsPath)
    {
        var content = @"// RaCore Control Panel UI Logic

async function initializeControlPanel() {
    // Check authentication
    if (!api.isAuthenticated()) {
        document.getElementById('loginView').style.display = 'flex';
        document.getElementById('panelView').style.display = 'none';
        return;
    }

    // Show panel view
    document.getElementById('loginView').style.display = 'none';
    document.getElementById('panelView').style.display = 'block';

    // Load modules
    await loadModules();
}

async function loadModules() {
    const modulesGrid = document.getElementById('modulesGrid');
    
    try {
        const data = await api.getModules();
        
        if (data.modules && data.modules.length > 0) {
            modulesGrid.innerHTML = data.modules.map(module => `
                <div class=""module-card"" onclick=""openModule('${module.name}')"">
                    <h3>${module.name}</h3>
                    <p>${module.description || 'No description available'}</p>
                </div>
            `).join('');
        } else {
            modulesGrid.innerHTML = '<p style=""color: #666; text-align: center;"">No modules available</p>';
        }
    } catch (error) {
        console.error('Failed to load modules:', error);
        modulesGrid.innerHTML = '<p style=""color: #e74c3c; text-align: center;"">Failed to load modules</p>';
    }
}

function openModule(moduleName) {
    // Map module names to dashboard pages
    const modulePages = {
        'GameEngine': '/gameengine-dashboard.html',
        'Authentication': '/admin.html'
    };

    const page = modulePages[moduleName] || '/admin.html';
    window.location.href = page;
}

function logout() {
    api.logout();
}

// Initialize on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeControlPanel);
} else {
    initializeControlPanel();
}
";
        
        File.WriteAllText(Path.Combine(jsPath, "control-panel-ui.js"), content);
        _module.Log("Generated control-panel-ui.js");
    }
}
