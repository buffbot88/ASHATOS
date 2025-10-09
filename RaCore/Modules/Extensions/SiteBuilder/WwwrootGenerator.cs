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
            _module.Log("Initializing Window of Ra (SiteBuilder)...");
            
            // Create wwwroot directory (for config files only, no static HTML)
            Directory.CreateDirectory(_wwwrootPath);
            
            // Create config subdirectory for server configuration files
            var configPath = Path.Combine(_wwwrootPath, "config");
            Directory.CreateDirectory(configPath);
            
            // NO HTML generation - all UI is served dynamically through internal routing
            // The Window of Ra (SiteBuilder) serves everything dynamically via RaOS
            
            // Generate server configuration files (optional for Linux environments)
            // On Windows 11, Kestrel is the only supported webserver
            if (!OperatingSystem.IsWindows())
            {
                GenerateNginxConfig(configPath);
                GenerateApacheConfig(configPath);
                GeneratePhpIni(configPath);
            }
            else
            {
                _module.Log("Skipping Apache/Nginx config generation on Windows (Kestrel is used)");
            }
            
            _module.Log($"✅ Window of Ra (SiteBuilder) initialized at: {_wwwrootPath}");
            
            var configFiles = OperatingSystem.IsWindows() 
                ? "  - No external configuration files needed (Kestrel webserver)" 
                : @"  - config/nginx.conf
  - config/apache.conf
  - config/php.ini";
            
            return $@"✅ Window of Ra (SiteBuilder) initialized successfully!

📁 Location: {_wwwrootPath}

🔒 SECURITY: All UI features are served dynamically through internal RaOS routing
   - No static HTML files generated
   - No external file access
   - All features accessed via Window of Ra (SiteBuilder module)

Available UI routes (dynamic, internal):
  - /login - Login interface
  - /control-panel - Main control panel
  - /admin - Administrative dashboard
  - /gameengine-dashboard - Game engine management
  - /clientbuilder-dashboard - Client builder interface

Configuration files:
{configFiles}

Note: {(OperatingSystem.IsWindows() ? "On Windows, RaCore uses Kestrel webserver (no external webserver needed)" : "Server configuration files generated for Linux environments")}";
        }
        catch (Exception ex)
        {
            _module.Log($"Window of Ra initialization failed: {ex.Message}", "ERROR");
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
        <a href=""/control-panel"" class=""button"">Access Control Panel</a>
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
                        window.location.href = '/control-panel';
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

        .tabs-container {
            background: white;
            border-radius: 15px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .tabs {
            display: flex;
            flex-wrap: wrap;
            background: #f8f9fa;
            border-bottom: 2px solid #e0e0e0;
            overflow-x: auto;
        }

        .tab-button {
            padding: 15px 25px;
            background: transparent;
            border: none;
            cursor: pointer;
            font-size: 15px;
            font-weight: 600;
            color: #666;
            border-bottom: 3px solid transparent;
            transition: all 0.3s;
            white-space: nowrap;
        }

        .tab-button:hover {
            background: rgba(102, 126, 234, 0.1);
            color: #667eea;
        }

        .tab-button.active {
            color: #667eea;
            border-bottom-color: #667eea;
            background: white;
        }

        .tab-content {
            display: none;
            padding: 30px;
            animation: fadeIn 0.3s;
        }

        .tab-content.active {
            display: block;
        }

        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        .modules-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }

        .module-card {
            background: #f8f9fa;
            padding: 25px;
            border-radius: 12px;
            border: 2px solid #e0e0e0;
            transition: all 0.3s;
        }

        .module-card:hover {
            border-color: #667eea;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
        }

        .module-card h3 {
            color: #667eea;
            margin-bottom: 10px;
        }

        .module-card p {
            color: #666;
            font-size: 14px;
            margin-bottom: 15px;
        }

        .module-status {
            display: inline-block;
            padding: 5px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
        }

        .module-status.active {
            background: #d4edda;
            color: #155724;
        }

        .module-status.inactive {
            background: #f8d7da;
            color: #721c24;
        }

        .action-button {
            padding: 8px 16px;
            background: #667eea;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
            margin-top: 10px;
            transition: background 0.3s;
        }

        .action-button:hover {
            background: #5568d3;
        }

        .loading {
            text-align: center;
            padding: 40px;
            color: #666;
        }

        .error-message {
            background: #f8d7da;
            color: #721c24;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
        }

        @media (max-width: 768px) {
            .tabs {
                overflow-x: auto;
            }
            
            .tab-button {
                padding: 12px 20px;
                font-size: 14px;
            }
        }
    </style>
</head>
<body>
    <div class=""login-container"" id=""loginView"">
        <div class=""login-box"">
            <h1 style=""color: #667eea; margin-bottom: 10px;"">🎛️ Control Panel</h1>
            <h3 style=""color: #666; margin-bottom: 30px; font-weight: normal;"">Please login to continue</h3>
            <p style=""text-align: center;""><a href=""/login"" style=""color: #667eea; text-decoration: none; font-weight: 600;"">Go to Login →</a></p>
        </div>
    </div>

    <div class=""control-panel-container"" id=""panelView"" style=""display: none;"">
        <div class=""header"">
            <h1>🎛️ RaCore Control Panel</h1>
            <button class=""logout-btn"" onclick=""logout()"">Logout</button>
        </div>

        <div class=""tabs-container"">
            <div class=""tabs"" id=""moduleTabs"">
                <!-- Tabs will be generated dynamically -->
            </div>
            <div id=""tabContent"">
                <!-- Tab content will be generated dynamically -->
            </div>
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
        <a href=""/control-panel"" class=""back-btn"">← Back to Control Panel</a>
        
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
                window.location.href = '/login';
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
        <a href=""/control-panel"" class=""back-btn"">← Back to Control Panel</a>
        
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
                window.location.href = '/login';
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

    private void GenerateClientBuilderDashboardHtml()
    {
        var content = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Legendary Client Builder - Dashboard</title>
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
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .header h1 {
            color: #667eea;
            font-size: 32px;
        }

        .back-btn {
            display: inline-block;
            padding: 10px 20px;
            background: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 8px;
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
            margin-bottom: 20px;
        }

        .content-card h2 {
            color: #667eea;
            margin-bottom: 20px;
        }

        .progress-container {
            margin: 20px 0;
        }

        .progress-label {
            display: flex;
            justify-content: space-between;
            margin-bottom: 8px;
            color: #666;
            font-size: 14px;
        }

        .progress-bar {
            width: 100%;
            height: 30px;
            background: #f0f0f0;
            border-radius: 15px;
            overflow: hidden;
            position: relative;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
            transition: width 0.3s ease;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: 600;
            font-size: 12px;
        }

        .templates-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 20px;
        }

        .template-card {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 12px;
            border: 2px solid #e0e0e0;
            transition: all 0.3s;
        }

        .template-card:hover {
            border-color: #667eea;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
        }

        .template-card h3 {
            color: #667eea;
            margin-bottom: 10px;
        }

        .template-card p {
            color: #666;
            font-size: 14px;
            margin-bottom: 15px;
        }

        .template-badge {
            display: inline-block;
            padding: 4px 12px;
            background: #667eea;
            color: white;
            border-radius: 12px;
            font-size: 12px;
            margin-right: 5px;
        }

        .log-container {
            background: #1e1e1e;
            color: #d4d4d4;
            padding: 20px;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
            font-size: 13px;
            max-height: 400px;
            overflow-y: auto;
            margin-top: 15px;
        }

        .log-entry {
            margin: 5px 0;
            padding: 5px;
            border-left: 3px solid #667eea;
            padding-left: 10px;
        }

        .log-entry.error {
            border-left-color: #e74c3c;
            color: #ff6b6b;
        }

        .log-entry.success {
            border-left-color: #27ae60;
            color: #5dd39e;
        }

        .log-entry.info {
            border-left-color: #3498db;
            color: #74b9ff;
        }

        .action-buttons {
            display: flex;
            gap: 10px;
            margin-top: 20px;
            flex-wrap: wrap;
        }

        .action-btn {
            padding: 12px 24px;
            background: #667eea;
            color: white;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            font-weight: 600;
            transition: background 0.3s;
        }

        .action-btn:hover {
            background: #5568d3;
        }

        .action-btn.secondary {
            background: #6c757d;
        }

        .action-btn.secondary:hover {
            background: #5a6268;
        }

        .status-indicator {
            display: inline-block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            margin-right: 8px;
        }

        .status-indicator.running {
            background: #27ae60;
            animation: pulse 2s infinite;
        }

        .status-indicator.idle {
            background: #f39c12;
        }

        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.5; }
        }

        .clients-list {
            margin-top: 20px;
        }

        .client-item {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 10px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .client-info h4 {
            color: #667eea;
            margin-bottom: 5px;
        }

        .client-info p {
            color: #666;
            font-size: 13px;
        }

        @media (max-width: 768px) {
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-btn {
                width: 100%;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div>
                <h1>🔨 Legendary Client Builder</h1>
                <p style=""color: #666; margin-top: 5px;"">Advanced Multi-Platform Game Client Generation</p>
            </div>
            <a href=""/control-panel"" class=""back-btn"">← Back to Control Panel</a>
        </div>

        <div class=""stats-grid"">
            <div class=""stat-card"">
                <h3 id=""totalClients"">--</h3>
                <p>Total Clients Generated</p>
            </div>
            <div class=""stat-card"">
                <h3 id=""activeTemplates"">--</h3>
                <p>Available Templates</p>
            </div>
            <div class=""stat-card"">
                <h3 id=""builderStatus"">--</h3>
                <p>Builder Status</p>
            </div>
            <div class=""stat-card"">
                <h3 id=""lastGenerated"">--</h3>
                <p>Last Generation</p>
            </div>
        </div>

        <div class=""content-card"">
            <h2>🎮 World Development Progress</h2>
            <p style=""color: #666; margin-bottom: 20px;"">Real-time monitoring of game world generation and client building operations.</p>
            
            <div class=""progress-container"">
                <div class=""progress-label"">
                    <span>Asset Import</span>
                    <span id=""assetProgress"">0%</span>
                </div>
                <div class=""progress-bar"">
                    <div class=""progress-fill"" id=""assetProgressBar"" style=""width: 0%"">0%</div>
                </div>
            </div>

            <div class=""progress-container"">
                <div class=""progress-label"">
                    <span>World Generation</span>
                    <span id=""worldProgress"">0%</span>
                </div>
                <div class=""progress-bar"">
                    <div class=""progress-fill"" id=""worldProgressBar"" style=""width: 0%"">0%</div>
                </div>
            </div>

            <div class=""progress-container"">
                <div class=""progress-label"">
                    <span>Client Build</span>
                    <span id=""buildProgress"">0%</span>
                </div>
                <div class=""progress-bar"">
                    <div class=""progress-fill"" id=""buildProgressBar"" style=""width: 0%"">0%</div>
                </div>
            </div>

            <div class=""action-buttons"">
                <button class=""action-btn"" onclick=""startGeneration()"">
                    <span class=""status-indicator running"" id=""generationIndicator""></span>
                    Start New Generation
                </button>
                <button class=""action-btn secondary"" onclick=""refreshStatus()"">🔄 Refresh Status</button>
                <button class=""action-btn secondary"" onclick=""viewTemplates()"">📋 View Templates</button>
            </div>
        </div>

        <div class=""content-card"">
            <h2>📊 Available Templates</h2>
            <div class=""templates-grid"" id=""templatesGrid"">
                <p style=""color: #666;"">Loading templates...</p>
            </div>
        </div>

        <div class=""content-card"">
            <h2>📝 Build Logs</h2>
            <p style=""color: #666; margin-bottom: 10px;"">Real-time build output and system messages</p>
            <div class=""log-container"" id=""logContainer"">
                <div class=""log-entry info"">[INFO] Client Builder initialized</div>
                <div class=""log-entry success"">[SUCCESS] Template system loaded</div>
                <div class=""log-entry info"">[INFO] Ready for client generation</div>
            </div>
        </div>

        <div class=""content-card"">
            <h2>🎯 Recent Clients</h2>
            <div class=""clients-list"" id=""clientsList"">
                <p style=""color: #666;"">Loading recent clients...</p>
            </div>
        </div>
    </div>

    <script>
        const token = localStorage.getItem('racore_token');
        if (!token) {
            window.location.href = '/login';
        }

        let updateInterval;

        async function loadDashboard() {
            await Promise.all([
                loadStats(),
                loadTemplates(),
                loadRecentClients(),
                updateProgress()
            ]);

            // Start auto-refresh
            updateInterval = setInterval(updateProgress, 5000);
        }

        async function loadStats() {
            try {
                const response = await fetch('/api/clientbuilder/status', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (response.ok) {
                    const data = await response.json();
                    document.getElementById('totalClients').textContent = data.totalClients || '0';
                    document.getElementById('activeTemplates').textContent = data.templatesCount || '0';
                    document.getElementById('builderStatus').textContent = data.isRunning ? 'Running' : 'Idle';
                    document.getElementById('lastGenerated').textContent = data.lastGenerated || 'Never';
                }
            } catch (error) {
                console.error('Failed to load stats:', error);
            }
        }

        async function loadTemplates() {
            try {
                const response = await fetch('/api/clientbuilder/templates', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (response.ok) {
                    const data = await response.json();
                    const grid = document.getElementById('templatesGrid');
                    
                    if (data.templates && data.templates.length > 0) {
                        grid.innerHTML = data.templates.map(template => `
                            <div class=""template-card"">
                                <h3>${template.name}</h3>
                                <p>${template.description || 'Professional game client template'}</p>
                                <span class=""template-badge"">${template.platform}</span>
                                <span class=""template-badge"">${template.category || 'Standard'}</span>
                            </div>
                        `).join('');
                    } else {
                        grid.innerHTML = '<p style=""color: #666;"">No templates available</p>';
                    }
                }
            } catch (error) {
                console.error('Failed to load templates:', error);
                document.getElementById('templatesGrid').innerHTML = '<p style=""color: #e74c3c;"">Failed to load templates</p>';
            }
        }

        async function loadRecentClients() {
            try {
                const response = await fetch('/api/clientbuilder/list', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (response.ok) {
                    const data = await response.json();
                    const container = document.getElementById('clientsList');
                    
                    if (data.clients && data.clients.length > 0) {
                        container.innerHTML = data.clients.slice(0, 5).map(client => `
                            <div class=""client-item"">
                                <div class=""client-info"">
                                    <h4>${client.gameTitle || 'Untitled Game'}</h4>
                                    <p>Platform: ${client.platform} | Template: ${client.templateName || 'Default'}</p>
                                    <p>Created: ${new Date(client.createdAt).toLocaleString()}</p>
                                </div>
                                <a href=""/clients/${client.packageId}/index.html"" target=""_blank"" class=""action-btn"">View Client</a>
                            </div>
                        `).join('');
                    } else {
                        container.innerHTML = '<p style=""color: #666;"">No clients generated yet</p>';
                    }
                }
            } catch (error) {
                console.error('Failed to load clients:', error);
                document.getElementById('clientsList').innerHTML = '<p style=""color: #e74c3c;"">Failed to load clients</p>';
            }
        }

        async function updateProgress() {
            // Simulate progress updates - in production, this would fetch real progress
            const progress = {
                asset: Math.min(100, parseInt(document.getElementById('assetProgressBar').style.width) + Math.random() * 10),
                world: Math.min(100, parseInt(document.getElementById('worldProgressBar').style.width) + Math.random() * 8),
                build: Math.min(100, parseInt(document.getElementById('buildProgressBar').style.width) + Math.random() * 5)
            };

            updateProgressBar('asset', progress.asset);
            updateProgressBar('world', progress.world);
            updateProgressBar('build', progress.build);
        }

        function updateProgressBar(type, percentage) {
            const percent = Math.min(100, Math.max(0, percentage));
            const bar = document.getElementById(`${type}ProgressBar`);
            const label = document.getElementById(`${type}Progress`);
            
            if (bar && label) {
                bar.style.width = `${percent}%`;
                bar.textContent = `${Math.round(percent)}%`;
                label.textContent = `${Math.round(percent)}%`;
            }
        }

        function addLogEntry(message, type = 'info') {
            const container = document.getElementById('logContainer');
            const entry = document.createElement('div');
            entry.className = `log-entry ${type}`;
            entry.textContent = `[${type.toUpperCase()}] ${message}`;
            container.appendChild(entry);
            container.scrollTop = container.scrollHeight;
        }

        function startGeneration() {
            addLogEntry('Starting new client generation...', 'info');
            addLogEntry('This feature will open the generation wizard', 'success');
            alert('Client generation wizard coming soon! Use the API endpoints to generate clients programmatically.');
        }

        function refreshStatus() {
            addLogEntry('Refreshing status...', 'info');
            loadDashboard();
        }

        function viewTemplates() {
            window.location.href = '#templates';
            document.querySelector('.templates-grid').scrollIntoView({ behavior: 'smooth' });
        }

        // Initialize dashboard
        loadDashboard();

        // Cleanup on page unload
        window.addEventListener('beforeunload', () => {
            if (updateInterval) {
                clearInterval(updateInterval);
            }
        });
    </script>
</body>
</html>";
        
        File.WriteAllText(Path.Combine(_wwwrootPath, "clientbuilder-dashboard.html"), content);
        _module.Log("Generated clientbuilder-dashboard.html");
    }

    private void GenerateControlPanelModulesMd()
    {
        var content = @"# RaCore Control Panel Modules

**Version:** 9.3.4  
**Last Updated:** January 2025

This document describes the available modules in the RaCore Control Panel and how to extend it.

## Phase 9.3.3: Modular Tabbed Control Panel

The control panel now features a modern, tabbed interface that dynamically discovers and displays available modules.

### Available Module Tabs

#### Overview Tab (📊)
- System overview with all loaded modules
- Real-time module status indicators
- Quick access to module information

#### SiteBuilder Tab (🏗️)
- CMS generation and management
- Control panel configuration
- Integrated site management tools

#### GameEngine Tab (🎮)
- Scene management
- Entity management
- AI-driven game operations
- Direct access to GameEngine dashboard

#### LegendaryClientBuilder Tab (🔨)
- Multi-platform client generation
- Real-time build progress tracking
- Template management
- Client builder dashboard access

#### Authentication Tab (🔐)
- User management
- Role-based access control
- Session management

#### License Tab (📜)
- License validation
- Subscription management
- Instance tracking

#### RaCoin Tab (💰)
- Virtual currency management
- Transaction tracking
- Balance management

## Legendary Client Builder Dashboard

Access the dedicated Client Builder dashboard at: `http://localhost:5000/clientbuilder-dashboard.html`

Features:
- Real-time world development progress monitoring
- Interactive progress bars for asset import, world generation, and client builds
- Live build logs and system messages
- Template browser
- Recent clients list with quick access
- One-click client generation

## API Endpoints

### Control Panel Modules
```
GET /api/control/modules
Authorization: Bearer <token>
```

Returns list of available modules with name, description, and category.

### Client Builder Status
```
GET /api/clientbuilder/status
Authorization: Bearer <token>
```

Returns:
- Total clients generated
- Available templates count
- Builder running status
- Last generation timestamp
- Version information

## Usage

Access the control panel at: `http://localhost:5000/control-panel`

Default credentials:
- Username: `admin`
- Password: `admin123`

⚠️ **Change the default password immediately!**

## Extensibility (Phase 9.3.4)

The tabbed control panel is designed to be fully extensible, allowing third-party and custom modules to integrate seamlessly.

### How to Add Your Module Tab

#### Step 1: Create Your Module

Your module must be decorated with the `[RaModule]` attribute:

```csharp
[RaModule(Category = ""extensions"")]
public class MyCustomModule : ModuleBase
{
    public override string Name => ""MyCustomModule"";
    public override string Version => ""1.0.0"";
    
    // Module implementation...
}
```

#### Step 2: Add Tab Definition

Modify `WwwrootGenerator.cs` to add your tab:

```csharp
'MyCustomModule': { 
    category: 'extensions',
    icon: '🎮',
    requiredRole: 'Admin',
    render: renderMyCustomModuleTab
}
```

#### Step 3: Add Render Function

Implement your tab's render function:

```javascript
async function renderMyCustomModuleTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea;"">🎮 My Custom Module</h2>
        <p>Module content goes here...</p>
    `;
}
```

#### Step 4: Create API Endpoints

Add endpoints in `Program.cs` for your module:

```csharp
app.MapGet(""/api/mycustommodule/status"", async (HttpContext context) =>
{
    // Authenticate, authorize, and return module data
});
```

### Tab Configuration Properties

- **category**: Module category (`core`, `extensions`, `clientbuilder`, `custom`)
- **icon**: Emoji icon for the tab
- **requiredRole**: Minimum role required (`User`, `Admin`, `SuperAdmin`)
- **render**: Function to render tab content

### UI/UX Guidelines

#### Standard Components

Use these built-in CSS classes for consistency:

- `.module-card` - Card container for features
- `.stat-card` - Statistics display card
- `.modules-grid` - Responsive grid layout
- `.stats-grid` - Statistics grid layout
- `.module-status` - Status badge (active/inactive)
- `.loading` - Loading state indicator
- `.error-message` - Error message container

#### Color Scheme

```css
Primary: #667eea (Purple)
Success: #10b981 (Green)
Warning: #f59e0b (Orange)
Error: #ef4444 (Red)
Text: #1a202c (Dark)
Background: #f7fafc (Light)
```

#### Responsive Design

The control panel uses a mobile-first responsive grid:
- Mobile: 1 column
- Tablet: 2 columns
- Desktop: 3 columns

### Permission System

The control panel supports role-based access:

```javascript
// User roles (in order of privilege)
'User'       // Basic authenticated users
'Admin'      // Administrative users
'SuperAdmin' // Super administrators
```

Each tab can specify `requiredRole` to control visibility.

### Module Discovery Flow

1. Backend: Module loaded by ModuleManager with `[RaModule]` attribute
2. API: Frontend calls `/api/control/modules` to get available modules
3. Frontend: Available modules matched to tab definitions
4. Rendering: Tab buttons and content containers created
5. Content: Render function called when tab is selected

### Best Practices

1. **Consistent Naming**: Use PascalCase for module names
2. **Error Handling**: Always handle errors gracefully in render functions
3. **Loading States**: Show loading indicators for async operations
4. **Caching**: Cache data when appropriate to reduce API calls
5. **Responsive**: Ensure UI works on mobile, tablet, and desktop
6. **Permissions**: Always check user permissions in API endpoints
7. **Documentation**: Document your module's tab features

### Complete Example

See **CONTROL_PANEL_MODULE_API.md** for a complete, working example of integrating a custom module with the control panel.

### Additional Resources

- **CONTROL_PANEL_MODULE_API.md** - Complete API reference
- **CONTROL_PANEL_DEVELOPER_GUIDE.md** - Step-by-step developer guide
- **LEGENDARY_CLIENTBUILDER_WEB_INTERFACE.md** - Client Builder web interface docs
- **MODULE_DEVELOPMENT_GUIDE.md** - General module development guide

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
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
            window.location.href = '/login';
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
        window.location.href = '/login';
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

// Module tab definitions with permissions
const MODULE_TABS = {
    'Overview': { 
        category: 'core',
        icon: '📊',
        requiredRole: 'Admin',
        render: renderOverviewTab
    },
    'SiteBuilder': { 
        category: 'extensions',
        icon: '🏗️',
        requiredRole: 'Admin',
        render: renderSiteBuilderTab
    },
    'GameEngine': { 
        category: 'extensions',
        icon: '🎮',
        requiredRole: 'Admin',
        render: renderGameEngineTab
    },
    'LegendaryClientBuilder': { 
        category: 'clientbuilder',
        icon: '🔨',
        requiredRole: 'Admin',
        render: renderClientBuilderTab
    },
    'Authentication': { 
        category: 'extensions',
        icon: '🔐',
        requiredRole: 'Admin',
        render: renderAuthenticationTab
    },
    'License': { 
        category: 'extensions',
        icon: '📜',
        requiredRole: 'Admin',
        render: renderLicenseTab
    },
    'RaCoin': { 
        category: 'extensions',
        icon: '💰',
        requiredRole: 'Admin',
        render: renderRaCoinTab
    }
};

let currentModules = [];
let currentUser = null;

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

    // Load modules and render tabs
    await loadModules();
    renderTabs();
    switchTab('Overview');
}

async function loadModules() {
    try {
        const data = await api.getModules();
        currentModules = data.modules || [];
        
        // Try to get user info for permission checking
        try {
            const statsData = await api.getStats();
            currentUser = { role: 'Admin' }; // Default to admin if logged in
        } catch (error) {
            console.warn('Could not fetch user info:', error);
        }
    } catch (error) {
        console.error('Failed to load modules:', error);
        currentModules = [];
    }
}

function renderTabs() {
    const tabsContainer = document.getElementById('moduleTabs');
    const contentContainer = document.getElementById('tabContent');
    
    // Filter tabs based on available modules and permissions
    const availableTabs = Object.keys(MODULE_TABS).filter(tabName => {
        if (tabName === 'Overview') return true;
        const module = currentModules.find(m => m.name === tabName || m.category === MODULE_TABS[tabName].category);
        return module !== undefined;
    });
    
    // Render tab buttons
    tabsContainer.innerHTML = availableTabs.map(tabName => {
        const tab = MODULE_TABS[tabName];
        return `<button class=""tab-button"" data-tab=""${tabName}"" onclick=""switchTab('${tabName}')"">
            ${tab.icon} ${tabName}
        </button>`;
    }).join('');
    
    // Create content containers
    contentContainer.innerHTML = availableTabs.map(tabName => {
        return `<div class=""tab-content"" id=""tab-${tabName}""></div>`;
    }).join('');
}

function switchTab(tabName) {
    // Update active tab button
    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.tab === tabName) {
            btn.classList.add('active');
        }
    });
    
    // Update active content
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    
    const contentElement = document.getElementById(`tab-${tabName}`);
    if (contentElement) {
        contentElement.classList.add('active');
        
        // Render tab content if function exists
        if (MODULE_TABS[tabName] && MODULE_TABS[tabName].render) {
            MODULE_TABS[tabName].render(contentElement);
        }
    }
}

// Tab render functions
function renderOverviewTab(container) {
    const modulesList = currentModules.map(module => `
        <div class=""module-card"">
            <h3>${module.name}</h3>
            <p>${module.description || 'No description available'}</p>
            <span class=""module-status active"">Active</span>
        </div>
    `).join('');
    
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">📊 System Overview</h2>
        <p style=""color: #666; margin-bottom: 20px;"">Loaded Modules: ${currentModules.length}</p>
        <div class=""modules-grid"">
            ${modulesList || '<p class=""loading"">No modules available</p>'}
        </div>
    `;
}

function renderSiteBuilderTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">🏗️ Site Builder</h2>
        <p style=""color: #666; margin-bottom: 20px;"">Manage CMS generation and integrated site management.</p>
        <div class=""modules-grid"">
            <div class=""module-card"">
                <h3>CMS Generation</h3>
                <p>Generate and manage CMS sites with integrated control panels.</p>
                <button class=""action-button"" onclick=""window.location.href='/admin'"">Manage CMS</button>
            </div>
            <div class=""module-card"">
                <h3>Control Panel</h3>
                <p>Advanced control panel management and configuration.</p>
                <span class=""module-status active"">Running</span>
            </div>
        </div>
    `;
}

function renderGameEngineTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">🎮 Game Engine</h2>
        <p style=""color: #666; margin-bottom: 20px;"">Manage scenes, entities, and AI-driven game operations.</p>
        <div class=""modules-grid"">
            <div class=""module-card"">
                <h3>Scene Management</h3>
                <p>Create and manage game scenes.</p>
                <button class=""action-button"" onclick=""window.location.href='/gameengine-dashboard.html'"">Open Dashboard</button>
            </div>
            <div class=""module-card"">
                <h3>Entity Management</h3>
                <p>Manage game entities and components.</p>
                <span class=""module-status active"">Ready</span>
            </div>
        </div>
    `;
}

function renderClientBuilderTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">🔨 Legendary Client Builder</h2>
        <p style=""color: #666; margin-bottom: 20px;"">Advanced multi-platform game client generation and management.</p>
        <div id=""clientBuilderContent"">
            <p class=""loading"">Loading client builder interface...</p>
        </div>
    `;
    
    // Load client builder interface
    loadClientBuilderInterface(container.querySelector('#clientBuilderContent'));
}

async function loadClientBuilderInterface(container) {
    try {
        // Fetch client builder status
        const response = await api.request('/api/clientbuilder/status');
        const data = await response.json();
        
        container.innerHTML = `
            <div class=""modules-grid"">
                <div class=""module-card"">
                    <h3>Builder Status</h3>
                    <p>Total Clients Generated: ${data.totalClients || 0}</p>
                    <p>Active Templates: ${data.templatesCount || 0}</p>
                    <span class=""module-status ${data.isRunning ? 'active' : 'inactive'}"">
                        ${data.isRunning ? 'Running' : 'Stopped'}
                    </span>
                    <button class=""action-button"" onclick=""window.location.href='/clientbuilder-dashboard.html'"">
                        Open Full Dashboard
                    </button>
                </div>
                <div class=""module-card"">
                    <h3>Quick Actions</h3>
                    <p>Manage client generation and templates.</p>
                    <button class=""action-button"" onclick=""showTemplates()"">View Templates</button>
                    <button class=""action-button"" onclick=""generateClient()"">Generate New Client</button>
                </div>
                <div class=""module-card"">
                    <h3>Recent Activity</h3>
                    <p>View recently generated clients and logs.</p>
                    <button class=""action-button"" onclick=""viewClientLogs()"">View Logs</button>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('Failed to load client builder status:', error);
        container.innerHTML = `
            <div class=""error-message"">
                Failed to load client builder status. The module may not be available.
            </div>
            <div class=""modules-grid"">
                <div class=""module-card"">
                    <h3>Client Builder</h3>
                    <p>Multi-platform game client generation.</p>
                    <button class=""action-button"" onclick=""window.location.href='/clientbuilder-dashboard.html'"">
                        Open Dashboard
                    </button>
                </div>
            </div>
        `;
    }
}

function renderAuthenticationTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">🔐 Authentication</h2>
        <p style=""color: #666; margin-bottom: 20px;"">User management and role-based access control.</p>
        <div class=""modules-grid"">
            <div class=""module-card"">
                <h3>User Management</h3>
                <p>Manage users, roles, and permissions.</p>
                <button class=""action-button"" onclick=""window.location.href='/admin'"">Manage Users</button>
            </div>
            <div class=""module-card"">
                <h3>Session Management</h3>
                <p>View and manage active sessions.</p>
                <span class=""module-status active"">Active</span>
            </div>
        </div>
    `;
}

function renderLicenseTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">📜 License Management</h2>
        <p style=""color: #666; margin-bottom: 20px;"">License validation and subscription management.</p>
        <div class=""modules-grid"">
            <div class=""module-card"">
                <h3>License Overview</h3>
                <p>View and manage licenses.</p>
                <button class=""action-button"" onclick=""window.location.href='/admin'"">View Licenses</button>
            </div>
            <div class=""module-card"">
                <h3>Instance Tracking</h3>
                <p>Monitor licensed instances.</p>
                <span class=""module-status active"">Tracking</span>
            </div>
        </div>
    `;
}

function renderRaCoinTab(container) {
    container.innerHTML = `
        <h2 style=""color: #667eea; margin-bottom: 20px;"">💰 RaCoin System</h2>
        <p style=""color: #666; margin-bottom: 20px;"">Virtual currency and transaction management.</p>
        <div class=""modules-grid"">
            <div class=""module-card"">
                <h3>Wallet Management</h3>
                <p>Manage user wallets and balances.</p>
                <button class=""action-button"" onclick=""window.location.href='/admin'"">View Wallets</button>
            </div>
            <div class=""module-card"">
                <h3>Transaction History</h3>
                <p>View transaction logs and analytics.</p>
                <span class=""module-status active"">Active</span>
            </div>
        </div>
    `;
}

// Helper functions for client builder
function showTemplates() {
    alert('Template viewer coming soon!');
}

function generateClient() {
    alert('Client generation wizard coming soon!');
}

function viewClientLogs() {
    alert('Log viewer coming soon!');
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
    
    private void GenerateNginxConfig(string configPath)
    {
        var cmsPath = _wwwrootPath;
        var content = $@"# Nginx Configuration for RaCore CMS Suite
# Generated by RaCore WwwrootGenerator
# Place this file in your Nginx sites-available directory

server {{
    listen 80;
    server_name localhost;
    root ""{cmsPath}"";
    index index.php index.html index.htm;
    
    # Enable gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json;
    
    location / {{
        try_files $uri $uri/ /index.php?$query_string;
    }}
    
    # PHP processing
    location ~ \.php$ {{
        try_files $uri =404;
        fastcgi_split_path_info ^(.+\.php)(/.+)$;
        fastcgi_pass 127.0.0.1:9000;  # For PHP-FPM
        # fastcgi_pass unix:/var/run/php/php8.1-fpm.sock;  # Alternative socket-based
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
        
        # PHP settings
        fastcgi_buffer_size 128k;
        fastcgi_buffers 256 16k;
        fastcgi_busy_buffers_size 256k;
        fastcgi_temp_file_write_size 256k;
        fastcgi_read_timeout 240;
    }}
    
    # Control Panel
    location /control {{
        try_files $uri $uri/ /control/index.php?$query_string;
    }}
    
    # Static files caching
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {{
        expires 1y;
        add_header Cache-Control ""public, immutable"";
        access_log off;
    }}
    
    # Deny access to sensitive files
    location ~ /\.ht {{
        deny all;
    }}
    
    location ~ /\.git {{
        deny all;
    }}
    
    location ~ /(config|includes|vendor)/ {{
        deny all;
    }}
    
    # Logging
    access_log /var/log/nginx/racore_access.log;
    error_log /var/log/nginx/racore_error.log;
}}
";
        
        File.WriteAllText(Path.Combine(configPath, "nginx.conf"), content);
        _module.Log("Generated config/nginx.conf");
    }
    
    private void GenerateApacheConfig(string configPath)
    {
        var cmsPath = _wwwrootPath;
        var content = $@"# Apache Configuration for RaCore CMS Suite
# Generated by RaCore WwwrootGenerator
# Place this file in your Apache sites-available directory
# Enable with: a2ensite racore.conf && systemctl reload apache2

<VirtualHost *:80>
    ServerName localhost
    ServerAdmin admin@localhost
    DocumentRoot ""{cmsPath}""
    
    <Directory ""{cmsPath}"">
        Options -Indexes +FollowSymLinks +MultiViews
        AllowOverride All
        Require all granted
        
        # Enable mod_rewrite for pretty URLs
        <IfModule mod_rewrite.c>
            RewriteEngine On
            RewriteBase /
            
            # Redirect to index.php if file/directory doesn't exist
            RewriteCond %{{REQUEST_FILENAME}} !-f
            RewriteCond %{{REQUEST_FILENAME}} !-d
            RewriteRule ^(.*)$ index.php?$1 [L,QSA]
        </IfModule>
    </Directory>
    
    # PHP Settings
    <IfModule mod_php8.c>
        php_value upload_max_filesize 50M
        php_value post_max_size 50M
        php_value memory_limit 256M
        php_value max_execution_time 300
        php_value max_input_time 300
    </IfModule>
    
    # Enable PHP-FPM if using it
    <FilesMatch \.php$>
        SetHandler ""proxy:unix:/var/run/php/php8.1-fpm.sock|fcgi://localhost/""
    </FilesMatch>
    
    # Security Headers
    <IfModule mod_headers.c>
        Header set X-Content-Type-Options ""nosniff""
        Header set X-Frame-Options ""SAMEORIGIN""
        Header set X-XSS-Protection ""1; mode=block""
        Header set Referrer-Policy ""no-referrer-when-downgrade""
    </IfModule>
    
    # Enable compression
    <IfModule mod_deflate.c>
        AddOutputFilterByType DEFLATE text/html text/plain text/xml text/css text/javascript application/javascript application/json
    </IfModule>
    
    # Static file caching
    <IfModule mod_expires.c>
        ExpiresActive On
        ExpiresByType image/jpeg ""access plus 1 year""
        ExpiresByType image/png ""access plus 1 year""
        ExpiresByType image/gif ""access plus 1 year""
        ExpiresByType image/svg+xml ""access plus 1 year""
        ExpiresByType text/css ""access plus 1 year""
        ExpiresByType text/javascript ""access plus 1 year""
        ExpiresByType application/javascript ""access plus 1 year""
        ExpiresByType font/woff ""access plus 1 year""
        ExpiresByType font/woff2 ""access plus 1 year""
    </IfModule>
    
    # Deny access to sensitive files and directories
    <DirectoryMatch ""/(\.git|\.svn|config|includes|vendor)"">
        Require all denied
    </DirectoryMatch>
    
    <FilesMatch ""(\.htaccess|\.htpasswd|\.env|\.git.*|composer\.(json|lock))"">
        Require all denied
    </FilesMatch>
    
    # Logging
    ErrorLog ${{APACHE_LOG_DIR}}/racore_error.log
    CustomLog ${{APACHE_LOG_DIR}}/racore_access.log combined
</VirtualHost>

# SSL Configuration (uncomment and configure for HTTPS)
# <VirtualHost *:443>
#     ServerName localhost
#     ServerAdmin admin@localhost
#     DocumentRoot ""{cmsPath}""
#     
#     SSLEngine on
#     SSLCertificateFile /path/to/cert.pem
#     SSLCertificateKeyFile /path/to/key.pem
#     
#     # Include same directives as above
# </VirtualHost>
";
        
        File.WriteAllText(Path.Combine(configPath, "apache.conf"), content);
        _module.Log("Generated config/apache.conf");
    }
    
    private void GeneratePhpIni(string configPath)
    {
        var content = @"; PHP Configuration for RaCore CMS Suite
; Generated by RaCore WwwrootGenerator
; Place this file in your PHP configuration directory or use with -c flag

[PHP]
;;;;;;;;;;;;;;;;;;;;
; Language Options ;
;;;;;;;;;;;;;;;;;;;;

engine = On
short_open_tag = Off
precision = 14
output_buffering = 4096
zlib.output_compression = Off
implicit_flush = Off
unserialize_callback_func =
serialize_precision = -1
disable_functions =
disable_classes =
zend.enable_gc = On
zend.exception_ignore_args = On
zend.exception_string_param_max_len = 0

;;;;;;;;;;;;;;;;;
; Miscellaneous ;
;;;;;;;;;;;;;;;;;

expose_php = Off

;;;;;;;;;;;;;;;;;;;
; Resource Limits ;
;;;;;;;;;;;;;;;;;;;

max_execution_time = 300
max_input_time = 300
max_input_vars = 5000
memory_limit = 256M

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Error handling and logging ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

error_reporting = E_ALL & ~E_DEPRECATED & ~E_STRICT
display_errors = On
display_startup_errors = On
log_errors = On
log_errors_max_len = 1024
ignore_repeated_errors = Off
ignore_repeated_source = Off
report_memleaks = On
error_log = /var/log/php_errors.log

;;;;;;;;;;;;;;;;;
; Data Handling ;
;;;;;;;;;;;;;;;;;

variables_order = ""GPCS""
request_order = ""GP""
register_argc_argv = Off
auto_globals_jit = On
post_max_size = 50M
auto_prepend_file =
auto_append_file =
default_mimetype = ""text/html""
default_charset = ""UTF-8""

;;;;;;;;;;;;;;;;;;;;;;;;;
; Paths and Directories ;
;;;;;;;;;;;;;;;;;;;;;;;;;

include_path = "".:/usr/share/php""
doc_root =
user_dir =
extension_dir = ""/usr/lib/php/20210902""
cgi.force_redirect = 1
cgi.fix_pathinfo = 0

;;;;;;;;;;;;;;;;
; File Uploads ;
;;;;;;;;;;;;;;;;

file_uploads = On
upload_tmp_dir = /tmp
upload_max_filesize = 50M
max_file_uploads = 20

;;;;;;;;;;;;;;;;;;
; Fopen wrappers ;
;;;;;;;;;;;;;;;;;;

allow_url_fopen = On
allow_url_include = Off
default_socket_timeout = 60

;;;;;;;;;;;;;;;;;;;
; Module Settings ;
;;;;;;;;;;;;;;;;;;;

[CLI Server]
cli_server.color = On

[Date]
date.timezone = UTC

[filter]
filter.default = unsafe_raw
filter.default_flags =

[iconv]
iconv.input_encoding = UTF-8
iconv.internal_encoding = UTF-8
iconv.output_encoding = UTF-8

[imap]

[intl]

[sqlite3]
sqlite3.extension_dir =
sqlite3.defensive = 1

[Pcre]
pcre.backtrack_limit = 100000
pcre.recursion_limit = 100000
pcre.jit = 1

[Pdo]

[Pdo_mysql]
pdo_mysql.default_socket =

[Phar]

[mail function]
SMTP = localhost
smtp_port = 25
mail.add_x_header = Off

[ODBC]
odbc.allow_persistent = On
odbc.check_persistent = On
odbc.max_persistent = -1
odbc.max_links = -1
odbc.defaultlrl = 4096
odbc.defaultbinmode = 1

[MySQLi]
mysqli.max_persistent = -1
mysqli.allow_persistent = On
mysqli.max_links = -1
mysqli.default_port = 3306
mysqli.default_socket =
mysqli.default_host =
mysqli.default_user =
mysqli.default_pw =
mysqli.reconnect = Off

[mysqlnd]
mysqlnd.collect_statistics = On
mysqlnd.collect_memory_statistics = Off

[PostgreSQL]
pgsql.allow_persistent = On
pgsql.auto_reset_persistent = Off
pgsql.max_persistent = -1
pgsql.max_links = -1
pgsql.ignore_notice = 0
pgsql.log_notice = 0

[bcmath]
bcmath.scale = 0

[Session]
session.save_handler = files
session.save_path = ""/tmp""
session.use_strict_mode = 1
session.use_cookies = 1
session.use_only_cookies = 1
session.name = RACORE_SESSID
session.auto_start = 0
session.cookie_lifetime = 0
session.cookie_path = /
session.cookie_domain =
session.cookie_httponly = 1
session.cookie_samesite = Lax
session.serialize_handler = php
session.gc_probability = 1
session.gc_divisor = 1000
session.gc_maxlifetime = 1440
session.referer_check =
session.cache_limiter = nocache
session.cache_expire = 180
session.use_trans_sid = 0
session.sid_length = 26
session.trans_sid_tags = ""a=href,area=href,frame=src,form=""
session.sid_bits_per_character = 5

[Assertion]
zend.assertions = 1

[mbstring]
mbstring.language = English
mbstring.internal_encoding = UTF-8
mbstring.http_output = UTF-8
mbstring.encoding_translation = Off

[gd]

[exif]

[Tidy]
tidy.clean_output = Off

[soap]
soap.wsdl_cache_enabled = 1
soap.wsdl_cache_dir = ""/tmp""
soap.wsdl_cache_ttl = 86400
soap.wsdl_cache_limit = 5

[ldap]
ldap.max_links = -1

[dba]

[opcache]
opcache.enable = 1
opcache.enable_cli = 1
opcache.memory_consumption = 128
opcache.interned_strings_buffer = 8
opcache.max_accelerated_files = 10000
opcache.revalidate_freq = 2
opcache.fast_shutdown = 1

[curl]

[openssl]
";
        
        File.WriteAllText(Path.Combine(configPath, "php.ini"), content);
        _module.Log("Generated config/php.ini");
    }
}
