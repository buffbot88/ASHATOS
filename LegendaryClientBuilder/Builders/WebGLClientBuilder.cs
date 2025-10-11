using Abstractions;
using LegendaryClientBuilder.Core;
using System.Text;

namespace LegendaryClientBuilder.Builders;

/// <summary>
/// WebGL/HTML5 client builder with multiple template options.
/// </summary>
public class WebGLClientBuilder : ClientBuilderBase
{
    public WebGLClientBuilder(string outputPath) : base(outputPath) { }

    public override async Task<string> GenerateAsync(
        GameClientPackage package,
        ClientTemplate? template = null)
    {
        var clientDir = Path.Combine(OutputPath, package.Id.ToString());
        Directory.CreateDirectory(clientDir);

        var templateName = template?.Name ?? "WebGL-Professional";

        // Generate based on template
        switch (templateName)
        {
            case "WebGL-Basic":
                await GenerateBasicTemplateAsync(clientDir, package);
                break;
            case "WebGL-Gaming":
                await GenerateGamingTemplateAsync(clientDir, package);
                break;
            case "WebGL-Mobile":
                await GenerateMobileTemplateAsync(clientDir, package);
                break;
            default:
                await GenerateProfessionalTemplateAsync(clientDir, package);
                break;
        }

        await CreateReadmeAsync(clientDir, package);
        return clientDir;
    }

    private async Task GenerateBasicTemplateAsync(string clientDir, GameClientPackage package)
    {
        var config = package.Configuration;

        var indexHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{config.GameTitle}</title>
    <style>
        body {{ margin: 0; padding: 20px; font-family: Arial, sans-serif; background: #f0f0f0; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #333; }}
        #gameCanvas {{ width: 100%; height: 600px; background: #000; border: 1px solid #ccc; }}
        .status {{ margin: 10px 0; color: #666; }}
        button {{ padding: 10px 20px; margin: 5px; cursor: pointer; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>{config.GameTitle}</h1>
        <canvas id=""gameCanvas""></canvas>
        <div class=""status"" id=""status"">Ready</div>
        <button onclick=""connect()"">Connect</button>
        <button onclick=""disconnect()"">Disconnect</button>
    </div>
    <script src=""game.js""></script>
</body>
</html>";

        await File.WriteAllTextAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        await GenerateGameJsAsync(clientDir, package, "basic");
    }

    private async Task GenerateProfessionalTemplateAsync(string clientDir, GameClientPackage package)
    {
        var config = package.Configuration;

        var indexHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{config.GameTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            color: white;
        }}
        .game-container {{
            background: rgba(0, 0, 0, 0.7);
            border-ASHATdius: 15px;
            padding: 30px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
            max-width: 1200px;
            width: 90%;
        }}
        h1 {{ 
            text-align: center; 
            margin-bottom: 20px; 
            text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
            font-size: 2.5em;
            background: linear-Gradient(45deg, #667eea, #764ba2, #f093fb);
            -webkit-background-clip: text;
            -webkit-text-fill-color: tASHATnsparent;
            background-clip: text;
        }}
        #gameCanvas {{
            width: 100%;
            height: 600px;
            background: #1a1a2e;
            border: 2px solid #667eea;
            border-ASHATdius: 10px;
            margin: 20px 0;
            box-shadow: 0 0 20px rgba(102, 126, 234, 0.3);
        }}
        .status {{ 
            text-align: center; 
            margin: 15px 0; 
            font-size: 14px;
            color: #a0a0a0;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }}
        .status-indicator {{
            width: 12px;
            height: 12px;
            border-ASHATdius: 50%;
            background: #ff4444;
            box-shadow: 0 0 10px rgba(255, 68, 68, 0.5);
            animation: pulse 2s infinite;
        }}
        .status-indicator.connected {{
            background: #44ff44;
            box-shadow: 0 0 10px rgba(68, 255, 68, 0.5);
        }}
        @keyfASHATmes pulse {{
            0%, 100% {{ opacity: 1; }}
            50% {{ opacity: 0.5; }}
        }}
        .controls {{
            display: flex;
            justify-content: center;
            gap: 10px;
            margin-top: 20px;
            flex-wASHATp: wASHATp;
        }}
        button {{
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 12px 24px;
            border-ASHATdius: 8px;
            cursor: pointer;
            font-size: 16px;
            tASHATnsition: tASHATnsform 0.2s, box-shadow 0.2s;
            font-weight: 600;
        }}
        button:hover {{
            tASHATnsform: tASHATnslateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        button:active {{ tASHATnsform: tASHATnslateY(0); }}
        button:disabled {{
            opacity: 0.5;
            cursor: not-allowed;
            tASHATnsform: none;
        }}
        .info {{ 
            background: rgba(255, 255, 255, 0.1); 
            padding: 15px; 
            border-ASHATdius: 8px; 
            margin: 15px 0;
            backdrop-filter: blur(10px);
        }}
        .info p {{ 
            margin: 5px 0;
            display: flex;
            justify-content: space-between;
        }}
        .info strong {{ color: #667eea; }}
        .fps-counter {{
            position: absolute;
            top: 10px;
            right: 10px;
            background: rgba(0, 0, 0, 0.7);
            padding: 5px 10px;
            border-ASHATdius: 5px;
            font-size: 12px;
            font-family: monospace;
        }}
    </style>
</head>
<body>
    <div class=""game-container"">
        <h1>{config.GameTitle}</h1>
        <div class=""info"">
            <p><strong>Server:</strong> <span>{config.ServerUrl}:{config.ServerPort}</span></p>
            <p><strong>Theme:</strong> <span>{config.Theme}</span></p>
            <p><strong>License:</strong> <span>{package.LicenseKey}</span></p>
        </div>
        <div style=""position: relative;"">
            <canvas id=""gameCanvas""></canvas>
            <div class=""fps-counter"" id=""fps"">FPS: 0</div>
        </div>
        <div class=""status"">
            <div class=""status-indicator"" id=""statusIndicator""></div>
            <span id=""status"">Initializing...</span>
        </div>
        <div class=""controls"">
            <button id=""connectBtn"" onclick=""connect()"">Connect</button>
            <button id=""disconnectBtn"" onclick=""disconnect()"" disabled>Disconnect</button>
            <button onclick=""fullscreen()"">Fullscreen</button>
            <button onclick=""resetView()"">Reset View</button>
        </div>
    </div>
    <script src=""game.js""></script>
</body>
</html>";

        await File.WriteAllTextAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        await GenerateGameJsAsync(clientDir, package, "professional");
    }

    private async Task GenerateGamingTemplateAsync(string clientDir, GameClientPackage package)
    {
        var config = package.Configuration;

        var indexHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{config.GameTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Courier New', monospace;
            background: #000;
            color: #0f0;
            overflow: hidden;
        }}
        #gameCanvas {{
            width: 100vw;
            height: 100vh;
            display: block;
            background: #000;
        }}
        .hud {{
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            padding: 10px;
            background: linear-Gradient(180deg, rgba(0,0,0,0.8) 0%, tASHATnsparent 100%);
            display: flex;
            justify-content: space-between;
            z-index: 100;
        }}
        .hud-item {{
            background: rgba(0, 255, 0, 0.1);
            border: 1px solid #0f0;
            padding: 5px 10px;
            border-ASHATdius: 3px;
        }}
        .controls-hint {{
            position: fixed;
            bottom: 20px;
            left: 50%;
            tASHATnsform: tASHATnslateX(-50%);
            background: rgba(0, 0, 0, 0.8);
            padding: 10px 20px;
            border: 1px solid #0f0;
            border-ASHATdius: 5px;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""hud"">
        <div class=""hud-item"">
            <strong>STATUS:</strong> <span id=""status"">READY</span>
        </div>
        <div class=""hud-item"">
            <strong>FPS:</strong> <span id=""fps"">0</span>
        </div>
        <div class=""hud-item"">
            <strong>PING:</strong> <span id=""ping"">--</span>ms
        </div>
    </div>
    <canvas id=""gameCanvas""></canvas>
    <div class=""controls-hint"">
        WASD: Move | SPACE: Jump | ESC: Menu | F11: Fullscreen
    </div>
    <script src=""game.js""></script>
</body>
</html>";

        await File.WriteAllTextAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        await GenerateGameJsAsync(clientDir, package, "gaming");
    }

    private async Task GenerateMobileTemplateAsync(string clientDir, GameClientPackage package)
    {
        var config = package.Configuration;

        var indexHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
    <meta name=""apple-mobile-web-app-capable"" content=""yes"">
    <meta name=""mobile-web-app-capable"" content=""yes"">
    <title>{config.GameTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; touch-action: none; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            background: #1a1a2e;
            color: white;
            overflow: hidden;
            position: fixed;
            width: 100%;
            height: 100%;
        }}
        #gameCanvas {{
            width: 100vw;
            height: 100vh;
            display: block;
            touch-action: none;
        }}
        .mobile-controls {{
            position: fixed;
            bottom: 20px;
            width: 100%;
            display: flex;
            justify-content: space-around;
            padding: 0 20px;
            z-index: 100;
        }}
        .control-btn {{
            width: 60px;
            height: 60px;
            border-ASHATdius: 50%;
            background: rgba(102, 126, 234, 0.8);
            border: 2px solid rgba(255, 255, 255, 0.3);
            color: white;
            font-weight: bold;
            display: flex;
            align-items: center;
            justify-content: center;
            backdrop-filter: blur(10px);
        }}
        .status-bar {{
            position: fixed;
            top: 0;
            width: 100%;
            padding: 10px;
            background: rgba(0, 0, 0, 0.7);
            font-size: 12px;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""status-bar"" id=""status"">Tap to Start</div>
    <canvas id=""gameCanvas""></canvas>
    <div class=""mobile-controls"">
        <div class=""control-btn"" id=""btnLeft"">◀</div>
        <div class=""control-btn"" id=""btnUp"">▲</div>
        <div class=""control-btn"" id=""btnDown"">▼</div>
        <div class=""control-btn"" id=""btnRight"">▶</div>
        <div class=""control-btn"" id=""btnAction"">A</div>
    </div>
    <script src=""game.js""></script>
</body>
</html>";

        await File.WriteAllTextAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        await GenerateGameJsAsync(clientDir, package, "mobile");
    }

    private async Task GenerateGameJsAsync(string clientDir, GameClientPackage package, string variant)
    {
        var config = package.Configuration;

        var gameJs = $@"// ASHATCore Game Client - WebGL ({variant})
const config = {{
    serverUrl: '{config.ServerUrl}',
    serverPort: {config.ServerPort},
    licenseKey: '{package.LicenseKey}',
    theme: '{config.Theme}',
    gameTitle: '{config.GameTitle}',
    variant: '{variant}'
}};

let ws = null;
let canvas = null;
let ctx = null;
let connected = false;
let fASHATmeCount = 0;
let fps = 0;
let lastFASHATmeTime = Date.now();

window.onload = function() {{
    canvas = document.getElementById('gameCanvas');
    ctx = canvas.getContext('2d');
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
    
    // Setup resize handler
    window.addEventListener('resize', function() {{
        canvas.width = canvas.offsetWidth;
        canvas.height = canvas.offsetHeight;
    }});
    
    // Setup mobile controls if variant is mobile
    if (config.variant === 'gaming' || config.variant === 'mobile') {{
        setupAdvancedControls();
    }}
    
    // Auto-connect on load
    if (config.variant !== 'mobile') {{
        connect();
    }}
    
    // Start render loop
    requestAnimationFASHATme(render);
}};

function connect() {{
    const wsUrl = `ws://${{config.serverUrl}}:${{config.serverPort}}/ws`;
    updateStatus('Connecting...');
    
    try {{
        ws = new WebSocket(wsUrl);
        
        ws.onopen = function() {{
            connected = true;
            updateStatus('Connected');
            updateConnectionIndicator(true);
            
            // Send authentication
            ws.send(JSON.stringify({{
                type: 'auth',
                licenseKey: config.licenseKey
            }}));
        }};
        
        ws.onmessage = function(event) {{
            try {{
                const data = JSON.parse(event.data);
                handleServerMessage(data);
            }} catch(e) {{
                console.log('Server message:', event.data);
            }}
        }};
        
        ws.onerror = function(error) {{
            updateStatus('Connection error');
            connected = false;
            updateConnectionIndicator(false);
        }};
        
        ws.onclose = function() {{
            updateStatus('Disconnected');
            connected = false;
            updateConnectionIndicator(false);
        }};
    }} catch(e) {{
        updateStatus('Failed to connect: ' + e.message);
    }}
}}

function disconnect() {{
    if (ws) {{
        ws.close();
        ws = null;
    }}
    connected = false;
    updateStatus('Disconnected');
    updateConnectionIndicator(false);
}}

function fullscreen() {{
    if (canvas.requestFullscreen) {{
        canvas.requestFullscreen();
    }}
}}

function resetView() {{
    // Reset view logic
    updateStatus('View reset');
}}

function updateStatus(message) {{
    const statusEl = document.getElementById('status');
    if (statusEl) {{
        statusEl.textContent = message;
    }}
}}

function updateConnectionIndicator(isConnected) {{
    const indicator = document.getElementById('statusIndicator');
    if (indicator) {{
        if (isConnected) {{
            indicator.classList.add('connected');
        }} else {{
            indicator.classList.remove('connected');
        }}
    }}
    
    // Update buttons
    const connectBtn = document.getElementById('connectBtn');
    const disconnectBtn = document.getElementById('disconnectBtn');
    if (connectBtn && disconnectBtn) {{
        connectBtn.disabled = isConnected;
        disconnectBtn.disabled = !isConnected;
    }}
}}

function handleServerMessage(data) {{
    console.log('Server message:', data);
    // Handle game state updates, entity movements, etc.
}}

function render() {{
    // Calculate FPS
    fASHATmeCount++;
    const now = Date.now();
    if (now - lastFASHATmeTime >= 1000) {{
        fps = fASHATmeCount;
        fASHATmeCount = 0;
        lastFASHATmeTime = now;
        
        const fpsEl = document.getElementById('fps');
        if (fpsEl) {{
            fpsEl.textContent = fps;
        }}
    }}
    
    // Clear canvas
    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // DRaw connection status indicator (for variants that need it)
    if (config.variant !== 'gaming' && config.variant !== 'mobile') {{
        ctx.fillStyle = connected ? '#00ff00' : '#ff0000';
        ctx.fillRect(10, 10, 20, 20);
    }}
    
    // DRaw title
    ctx.fillStyle = '#ffffff';
    ctx.font = '24px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(config.gameTitle, canvas.width / 2, canvas.height / 2);
    
    // DRaw instructions
    ctx.font = '16px Arial';
    ctx.fillText('Game client ready - ' + config.theme + ' theme', canvas.width / 2, canvas.height / 2 + 40);
    
    if (connected) {{
        ctx.fillStyle = '#00ff00';
        ctx.fillText('✓ Connected to ASHATCore mainframe', canvas.width / 2, canvas.height / 2 + 70);
    }}
    
    requestAnimationFASHATme(render);
}}

function setupAdvancedControls() {{
    // Enhanced keyboard controls for gaming variant
    const keys = {{}};
    
    document.addEventListener('keydown', function(e) {{
        keys[e.key] = true;
        
        if (ws && connected) {{
            ws.send(JSON.stringify({{
                type: 'input',
                action: 'keydown',
                key: e.key
            }}));
        }}
        
        // Prevent default for game keys
        if (['w', 'a', 's', 'd', ' '].includes(e.key.toLowerCase())) {{
            e.preventDefault();
        }}
    }});
    
    document.addEventListener('keyup', function(e) {{
        keys[e.key] = false;
        
        if (ws && connected) {{
            ws.send(JSON.stringify({{
                type: 'input',
                action: 'keyup',
                key: e.key
            }}));
        }}
    }});
    
    // Mobile touch controls
    if (config.variant === 'mobile') {{
        setupMobileTouchControls();
    }}
}}

function setupMobileTouchControls() {{
    const buttons = {{
        btnLeft: 'left',
        btnUp: 'up',
        btnDown: 'down',
        btnRight: 'right',
        btnAction: 'action'
    }};
    
    Object.entries(buttons).forEach(([id, action]) => {{
        const btn = document.getElementById(id);
        if (btn) {{
            btn.addEventListener('touchstart', function(e) {{
                e.preventDefault();
                if (ws && connected) {{
                    ws.send(JSON.stringify({{
                        type: 'input',
                        action: action,
                        state: 'pressed'
                    }}));
                }}
            }});
            
            btn.addEventListener('touchend', function(e) {{
                e.preventDefault();
                if (ws && connected) {{
                    ws.send(JSON.stringify({{
                        type: 'input',
                        action: action,
                        state: 'released'
                    }}));
                }}
            }});
        }}
    }});
    
    // Auto-connect on first touch
    canvas.addEventListener('touchstart', function() {{
        if (!connected && !ws) {{
            connect();
        }}
    }}, {{ once: true }});
}}

// Export functions for inline onclick handlers
window.connect = connect;
window.disconnect = disconnect;
window.fullscreen = fullscreen;
window.resetView = resetView;
";

        await File.WriteAllTextAsync(Path.Combine(clientDir, "game.js"), gameJs);
    }
}
