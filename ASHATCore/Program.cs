using Abstractions;
using ASHATCore.Endpoints;
using ASHATCore.Engine;
using ASHATCore.Engine.Manager;
using ASHATCore.Engine.Memory;
using ASHATCore.Models;
using ASHATCore.Modules.Extensions.UserProfiles;
using SQLitePCL;
using System.Text.Json;

// Ensure wwwroot directory exists (used for config files only, not for static HTML)
// All UI is served dynamically through (SiteBuilder module)
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"[ASHATCore] Created wwwroot directory: {wwwrootPath}");
}

// 1. Instantiate MemoryModule FIRST
var memoryModule = new MemoryModule();
memoryModule.Initialize(null); // Pass ModuleManager if needed

// 2. Instantiate ModuleManager and register MemoryModule as built-in
var moduleManager = new ModuleManager();
moduleManager.RegisterBuiltInModule(memoryModule);

// 3. Check for first run and auto-spawn CMS BEFORE loading other modules
// This ensures all required files and configs are created before any module initialization
var firstRunManager = new ASHATCore.Engine.FirstRunManager(moduleManager);

if (firstRunManager.IsFirstRun())
{
    Console.WriteLine("[ASHATCore] First run detected - initializing CMS homepage...");
    await firstRunManager.InitializeAsync();
}
else
{
    // Always ensure (SiteBuilder) is initialized on boot
    Console.WriteLine("[ASHATCore] Initializing (SiteBuilder)...");
    await firstRunManager.EnsureWwwrootAsync();
}

// 4. Load other modules (plugins, etc.) AFTER CMS setup is complete
// Force load external Legendary module assemblies to ensure they're available for discovery
// This is necessary because .NET doesn't auto-load referenced DLLs unless they're explicitly used
try
{
    System.Reflection.Assembly.Load("LegendaryChat");
    System.Reflection.Assembly.Load("LegendaryLearning");
    System.Reflection.Assembly.Load("LegendaryGameServer");
    System.Reflection.Assembly.Load("LegendaryGameClient");
    // LegendaryCMS, LegendaryGameEngine, and LegendaryClientBuilder are already loaded elsewhere
}
catch (Exception ex)
{
    Console.WriteLine($"[ASHATCore] Warning: Could not preload some external modules: {ex.Message}");
}

moduleManager.LoadModules();

var builder = WebApplication.CreateBuilder(args);

// 5. Run boot sequence with self-healing checks and Configuration verification
// This will detect the port from Nginx Configuration (MUST run before building app)
var bootSequence = new ASHATCore.Engine.BootSequenceManager(moduleManager);
await bootSequence.ExecuteBootSequenceAsync();

// 6. Configure port - use detected port from Nginx config or fallback to default
// Nginx Configuration is the source of truth for port management
// Assume 'config' is loaded from server-config.json

// 1. Try environment variable
string port = Environment.GetEnvironmentVariable("ASHATCore_DETECTED_PORT");

// 2. If not set, try config file
if (string.IsNullOrEmpty(port))
{
    var serverRoot = Directory.GetCurrentDirectory();
    var configPath = Path.Combine(serverRoot, "server-config.json");
    if (File.Exists(configPath))
    {
        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("Port", out var portProp))
        {
            port = portProp.ToString();
        }
    }
}

// 3. Default fallback
if (string.IsNullOrEmpty(port))
    port = "80";
var urls = $"http://0.0.0.0:{port}";

Console.WriteLine($"[ASHATCore] Configuring Kestrel to listen on: {urls}");
Console.WriteLine($"[ASHATCore] This will bind to ALL network interfaces (0.0.0.0)");

// Add CORS support for agpstudios.online domain and dynamic port
// Check if we should use permissive CORS (for development/debugging)
var allowPermissiveCors = Environment.GetEnvironmentVariable("ASHATCore_PERMISSIVE_CORS")?.ToLower() == "true";

var allowedOrigins = new List<string>
{
    // Localhost with dynamic port
    $"http://localhost:{port}",
    "http://localhost",
    $"http://127.0.0.1:{port}",
    "http://127.0.0.1",
    // AGP Studios domains
    "http://agpstudios.online", //for server updates cloud-wide
    "https://agpstudios.online" //SSL must be enabled for incoming requests
};

builder.Services.AddCors(options =>
{
    if (allowPermissiveCors)
    {
        Console.WriteLine("[ASHATCore] CORS: Using permissive mode (ASHATCore_PERMISSIVE_CORS=true)");
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        Console.WriteLine($"[ASHATCore] CORS: Allowing specific origins: {string.Join(", ", allowedOrigins)}");
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins.ToArray())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// Add Razor Pages support for CMS homepage
builder.Services.AddRazorPages();

// Configure URLs with dynamic port
builder.WebHost.UseUrls(urls);

var app = builder.Build();
app.UseCors(); // Enable CORS
app.UseWebSockets();

// Enable Razor Pages routing
app.UseRouting();

Console.WriteLine($"[ASHATCore] Kestrel webserver starting...");
Console.WriteLine($"[ASHATCore] Server will be accessible at:");
Console.WriteLine($"  - http://localhost:{port}");
Console.WriteLine($"  - http://127.0.0.1:{port}");
Console.WriteLine($"  - http://<your-server-ip>:{port}");
Console.WriteLine($"[ASHATCore] Ensure firewall allows inbound connections on port {port}");

// NOTE: ASHATCore uses Kestrel webserver internally to host both the CMS and API endpoints.
// On Windows 11, Kestrel is the only supported webserver.
// External Apache/PHP8 is optional on Linux for PHP file execution only.

// URL redirects removed - all UI access goes through module (SiteBuilder)
// The module will handle all UI routes internally

// 7. Wire up FirstRunManager to ServerConfig and License modules
var serverConfigModule = moduleManager.Modules
    .Select(m => m.Instance)
    .FirstOrDefault(m => m.Name == "ServerConfig");

if (serverConfigModule != null)
{
    var setFirstRunManagerMethod = serverConfigModule.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(serverConfigModule, new object[] { firstRunManager });
    Console.WriteLine("[ASHATCore] FirstRunManager wired to ServerConfig module");
}

var licenseModuleInstance = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ILicenseModule>()
    .FirstOrDefault();

if (licenseModuleInstance != null)
{
    var setFirstRunManagerMethod = licenseModuleInstance.GetType().GetMethod("SetFirstRunManager");
    setFirstRunManagerMethod?.Invoke(licenseModuleInstance, [firstRunManager]);
    Console.WriteLine("[ASHATCore] FirstRunManager wired to License module");
}

// Optionally pick up a SpeechModule if present
ISpeechModule? speechModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ISpeechModule>()
    .FirstOrDefault();


// Get authentication module if present
IAuthenticationModule? authModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IAuthenticationModule>()
    .FirstOrDefault();

var wsHandler = new ASHATCoreWebSocketHandler(moduleManager, speechModule);

// Map WebSocket endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await wsHandler.HandleAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Authentication API endpoints
app.MapAuthEndpoints(authModule);



// Get game engine module if present
IGameEngineModule? gameEngineModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameEngineModule>()
    .FirstOrDefault();

// Game Engine API endpoints
app.MapGameEngineEndpoints(gameEngineModule, authModule);


// ServerSetup API endpoints
IServerSetupModule? serverSetupModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IServerSetupModule>()
    .FirstOrDefault();

app.MapServerSetupEndpoints(serverSetupModule, authModule);

// GameServer API endpoints
IGameServerModule? gameServerModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameServerModule>()
    .FirstOrDefault();

app.MapGameServerEndpoints(gameServerModule, authModule);

// Note: Homepage is served via Razor Pages at /Index
// Login UI is served via Razor Pages at /Login
// All CMS functionality is integrated through Razor Pages, not inline HTML generation

// Generate Onboarding UI dynamically
static string GenerateOnboardingUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Masters Class Onboarding - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .onboarding-container {
            background: rgba(20, 0, 40, 0.9);
            border-radius: 15px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 10px 40px rgba(138, 43, 226, 0.4);
            max-width: 900px;
            width: 100%;
            padding: 40px;
        }
        h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
            font-size: 28px;
        }
        .subtitle {
            color: #c8b6ff;
            margin-bottom: 30px;
            font-size: 16px;
        }
        .progress-bar {
            width: 100%;
            height: 8px;
            background: rgba(0, 0, 0, 0.3);
            border-radius: 4px;
            margin-bottom: 30px;
            overflow: hidden;
        }
        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #8b2fc7 0%, #6a1b9a 100%);
            width: 0%;
            transition: width 0.3s ease;
        }
        .course-list {
            display: grid;
            gap: 15px;
            margin-bottom: 30px;
        }
        .course-card {
            background: rgba(138, 43, 226, 0.1);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 10px;
            padding: 20px;
            cursor: pointer;
            transition: all 0.3s;
        }
        .course-card:hover {
            border-color: rgba(138, 43, 226, 0.6);
            box-shadow: 0 4px 12px rgba(138, 43, 226, 0.3);
            transform: translateX(5px);
        }
        .course-card.completed {
            background: rgba(34, 197, 94, 0.1);
            border-color: #22c55e;
        }
        .course-card.active {
            border-color: #8b2fc7;
            background: rgba(138, 43, 226, 0.2);
        }
        .course-title {
            font-size: 18px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 8px;
        }
        .course-description {
            color: #c8b6ff;
            font-size: 14px;
            margin-bottom: 10px;
        }
        .course-meta {
            display: flex;
            gap: 20px;
            font-size: 13px;
            color: #b8a8d8;
        }
        .lesson-viewer {
            display: none;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 10px;
            padding: 30px;
            margin-bottom: 30px;
            background: rgba(138, 43, 226, 0.1);
        }
        .lesson-title {
            font-size: 22px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 15px;
        }
        .lesson-content {
            color: #d8c8ff;
            line-height: 1.8;
            margin-bottom: 25px;
            white-space: pre-wrap;
        }
        .lesson-navigation {
            display: flex;
            gap: 10px;
            justify-content: space-between;
            align-items: center;
        }
        button {
            padding: 12px 24px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        button:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        button:disabled {
            background: #444;
            border-color: #555;
            cursor: not-allowed;
            opacity: 0.5;
        }
        button.secondary {
            background: #333;
            border-color: #444;
        }
        button.secondary:hover {
            background: #444;
        }
        button.success {
            background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
            border-color: rgba(34, 197, 94, 0.5);
        }
        button.success:hover {
            background: linear-gradient(135deg, #4ade80 0%, #22c55e 100%);
        }
        .status-message {
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .status-message.success {
            background: rgba(34, 197, 94, 0.2);
            color: #4ade80;
            border: 1px solid rgba(34, 197, 94, 0.5);
        }
        .status-message.error {
            background: rgba(239, 68, 68, 0.2);
            color: #fca5a5;
            border: 1px solid rgba(239, 68, 68, 0.5);
        }
        .loading {
            text-align: center;
            padding: 40px;
            color: #c8b6ff;
        }
        .completion-badge {
            display: inline-block;
            background: #22c55e;
            color: white;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
        }
    </style>
</head>
<body>
    <div class=""onboarding-container"">
        <h1>🎓 Masters Class Onboarding</h1>
        <p class=""subtitle"">Welcome to AGP Studios! Complete these courses to access the Control Panel.</p>
        
        <div class=""progress-bar"">
            <div class=""progress-fill"" id=""progressBar""></div>
        </div>
        
        <div class=""status-message"" id=""statusMessage""></div>
        
        <div id=""loadingMessage"" class=""loading"">Loading your courses...</div>
        
        <div id=""courseList"" class=""course-list"" style=""display: none;""></div>
        
        <div id=""lessonViewer"" class=""lesson-viewer""></div>
        
        <div style=""text-align: center; margin-top: 20px;"">
            <button id=""finishButton"" style=""display: none;"" class=""success"" onclick=""completeOnboarding()"">
                🎉 Complete Onboarding & Proceed to Activation
            </button>
        </div>
    </div>
    
    <script>
        let currentCourse = null;
        let currentLesson = null;
        let courses = [];
        let lessons = [];
        let completedLessons = new Set();
        
        async function checkAuth() {
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return false;
            }
            return true;
        }
        
        async function loadCourses() {
            if (!await checkAuth()) return;
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/learning/courses/SuperAdmin', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                if (response.status === 403 || response.status === 401) {
                    window.location.href = '/login';
                    return;
                }
                
                const data = await response.json();
                if (data.success) {
                    courses = data.courses;
                    await loadProgress();
                    renderCourses();
                    document.getElementById('loadingMessage').style.display = 'none';
                    document.getElementById('courseList').style.display = 'grid';
                } else {
                    showError('Failed to load courses: ' + data.message);
                }
            } catch (error) {
                showError('Failed to load courses: ' + error.message);
            }
        }
        
        async function loadProgress() {
            const token = localStorage.getItem('ASHATCore_token');
            for (const course of courses) {
                try {
                    const response = await fetch(`/api/learning/courses/${course.id}/lessons`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    const data = await response.json();
                    if (data.success && data.lessons) {
                        course.lessons = data.lessons;
                        course.completed = data.lessons.every(l => l.completed);
                        data.lessons.forEach(l => {
                            if (l.completed) completedLessons.add(l.id);
                        });
                    }
                } catch (error) {
                    console.error('Failed to load progress for course:', course.id);
                }
            }
            updateProgress();
        }
        
        function renderCourses() {
            const container = document.getElementById('courseList');
            container.innerHTML = '';
            
            courses.forEach(course => {
                const card = document.createElement('div');
                card.className = 'course-card' + (course.completed ? ' completed' : '');
                card.innerHTML = `
                    <div class=""course-title"">
                        ${course.title}
                        ${course.completed ? '<span class=""completion-badge"">✓ Completed</span>' : ''}
                    </div>
                    <div class=""course-description"">${course.description}</div>
                    <div class=""course-meta"">
                        <span>📚 ${course.lessonCount} lessons</span>
                        <span>⏱️ ${course.estimatedMinutes} minutes</span>
                    </div>
                `;
                card.onclick = () => openCourse(course);
                container.appendChild(card);
            });
        }
        
        async function openCourse(course) {
            if (!course.lessons) {
                await loadProgress();
            }
            
            currentCourse = course;
            lessons = course.lessons || [];
            currentLesson = lessons.find(l => !l.completed) || lessons[0];
            
            if (currentLesson) {
                await loadLessonContent(currentLesson);
            }
        }
        
        async function loadLessonContent(lesson) {
            const viewer = document.getElementById('lessonViewer');
            currentLesson = lesson;
            
            viewer.innerHTML = `
                <div class=""lesson-title"">${lesson.title}</div>
                <div class=""lesson-content"">${lesson.content}</div>
                <div class=""lesson-navigation"">
                    <button class=""secondary"" onclick=""closeLessonViewer()"">← Back to Courses</button>
                    <div>
                        <button onclick=""previousLesson()"" ${lessons.indexOf(lesson) === 0 ? 'disabled' : ''}>
                            ← Previous
                        </button>
                        <button onclick=""completeLesson()"" class=""${lesson.completed ? 'success' : ''}"">
                            ${lesson.completed ? '✓ Completed' : 'Mark Complete'}
                        </button>
                        <button onclick=""nextLesson()"" ${lessons.indexOf(lesson) === lessons.length - 1 ? 'disabled' : ''}>
                            Next →
                        </button>
                    </div>
                </div>
            `;
            
            viewer.style.display = 'block';
            viewer.scrollIntoView({ behavior: 'smooth' });
        }
        
        function closeLessonViewer() {
            document.getElementById('lessonViewer').style.display = 'none';
        }
        
        function previousLesson() {
            const currentIndex = lessons.indexOf(currentLesson);
            if (currentIndex > 0) {
                loadLessonContent(lessons[currentIndex - 1]);
            }
        }
        
        function nextLesson() {
            const currentIndex = lessons.indexOf(currentLesson);
            if (currentIndex < lessons.length - 1) {
                loadLessonContent(lessons[currentIndex + 1]);
            }
        }
        
        async function completeLesson() {
            if (!currentLesson || currentLesson.completed) return;
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch(`/api/learning/lessons/${currentLesson.id}/complete`, {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                const data = await response.json();
                if (data.success) {
                    currentLesson.completed = true;
                    completedLessons.add(currentLesson.id);
                    
                    // Check if course is completed
                    if (lessons.every(l => l.completed)) {
                        currentCourse.completed = true;
                        showSuccess('🎉 Course completed!');
                    } else {
                        showSuccess('✓ Lesson completed!');
                    }
                    
                    updateProgress();
                    renderCourses();
                    
                    // Auto-advance to next lesson
                    const currentIndex = lessons.indexOf(currentLesson);
                    if (currentIndex < lessons.length - 1) {
                        setTimeout(() => loadLessonContent(lessons[currentIndex + 1]), 1000);
                    } else {
                        setTimeout(closeLessonViewer, 1500);
                    }
                } else {
                    showError('Failed to complete lesson: ' + data.message);
                }
            } catch (error) {
                showError('Failed to complete lesson: ' + error.message);
            }
        }
        
        function updateProgress() {
            const totalLessons = courses.reduce((sum, c) => sum + (c.lessons?.length || c.lessonCount), 0);
            const completed = completedLessons.size;
            const percentage = totalLessons > 0 ? (completed / totalLessons) * 100 : 0;
            
            document.getElementById('progressBar').style.width = percentage + '%';
            
            // Show finish button if all courses completed
            if (courses.every(c => c.completed)) {
                document.getElementById('finishButton').style.display = 'block';
            }
        }
        
        async function completeOnboarding() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/learning/SuperAdmin/complete', {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                
                const data = await response.json();
                if (data.success) {
                    showSuccess('🎉 Onboarding completed! Redirecting to server activation...');
                    setTimeout(() => {
                        window.location.href = '/activation';
                    }, 2000);
                } else {
                    showError('Failed to complete onboarding: ' + data.message);
                }
            } catch (error) {
                showError('Failed to complete onboarding: ' + error.message);
            }
        }
        
        function showSuccess(message) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message success';
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            setTimeout(() => statusMsg.style.display = 'none', 5000);
        }
        
        function showError(message) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message error';
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            document.getElementById('loadingMessage').style.display = 'none';
        }
        
        // Initialize
        loadCourses();
    </script>
</body>
</html>";
}

// Generate Server Activation UI dynamically
static string GenerateActivationUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Server Activation - ASHATOS</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .activation-container {
            background: rgba(20, 0, 40, 0.9);
            border-radius: 20px;
            border: 2px solid rgba(138, 43, 226, 0.3);
            box-shadow: 0 20px 60px rgba(138, 43, 226, 0.4);
            padding: 40px;
            max-width: 600px;
            width: 100%;
        }
        h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
            font-size: 32px;
            text-align: center;
        }
        .subtitle {
            color: #c8b6ff;
            margin-bottom: 30px;
            text-align: center;
            font-size: 16px;
        }
        .section {
            margin-bottom: 30px;
        }
        .section-title {
            font-size: 18px;
            font-weight: 600;
            color: #e0d0ff;
            margin-bottom: 15px;
        }
        .info-box {
            background: rgba(138, 43, 226, 0.1);
            border-left: 4px solid #8b2fc7;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }
        .info-box p {
            margin: 5px 0;
            color: #d8c8ff;
            line-height: 1.6;
        }
        .form-group {
            margin-bottom: 20px;
        }
        label {
            display: block;
            margin-bottom: 8px;
            color: #e0d0ff;
            font-weight: 600;
        }
        input[type=""text""] {
            width: 100%;
            padding: 12px;
            background: rgba(0, 0, 0, 0.3);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 8px;
            font-size: 16px;
            color: white;
            transition: border-color 0.3s;
            font-family: monospace;
        }
        input[type=""text""]:focus {
            outline: none;
            border-color: rgba(138, 43, 226, 0.8);
            background: rgba(0, 0, 0, 0.4);
        }
        .button {
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            color: white;
            padding: 14px 28px;
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            width: 100%;
            transition: all 0.3s;
        }
        .button:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        .button:disabled {
            background: #444;
            border-color: #555;
            cursor: not-allowed;
            opacity: 0.5;
        }
        .status-message {
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .status-message.success {
            background: rgba(34, 197, 94, 0.2);
            color: #4ade80;
            border: 1px solid rgba(34, 197, 94, 0.5);
        }
        .status-message.error {
            background: rgba(239, 68, 68, 0.2);
            color: #fca5a5;
            border: 1px solid rgba(239, 68, 68, 0.5);
        }
        .status-message.warning {
            background: rgba(234, 179, 8, 0.2);
            color: #fcd34d;
            border: 1px solid rgba(234, 179, 8, 0.5);
        }
        .license-types {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 10px;
            margin-top: 15px;
        }
        .license-type {
            background: rgba(138, 43, 226, 0.1);
            padding: 10px;
            border-radius: 6px;
            border: 1px solid rgba(138, 43, 226, 0.3);
            font-size: 14px;
            color: #c8b6ff;
        }
        .license-type strong {
            color: #c084fc;
        }
        .dev-mode-notice {
            background: rgba(234, 179, 8, 0.2);
            border-left: 4px solid #fcd34d;
            padding: 12px;
            margin-bottom: 20px;
            border-radius: 4px;
            font-size: 14px;
            color: #fcd34d;
        }
    </style>
</head>
<body>
    <div class=""activation-container"">
        <h1>🔑 Server Activation</h1>
        <p class=""subtitle"">You're almost there! Enter your license key to activate your ASHAT Os server.</p>
        
        <div id=""statusMessage"" class=""status-message""></div>
        <div id=""devModeNotice"" class=""dev-mode-notice"" style=""display: none;"">
            <strong>Development Mode:</strong> License validation is bypassed. Any valid format key will activate the server.
        </div>
        
        <div class=""section"">
            <div class=""info-box"">
                <p><strong>✅ Onboarding Completed</strong></p>
                <p>You've successfully completed the Masters Class Training.</p>
                <p><strong>Next Step:</strong> Activate your server with a license key to unlock all features.</p>
            </div>
        </div>
        
        <div class=""section"">
            <div class=""section-title"">License Key</div>
            <div class=""form-group"">
                <label for=""licenseKey"">Enter your RaCore license key:</label>
                <input type=""text"" id=""licenseKey"" placeholder=""RaOS-XXXX-XXXX-XXXX-XXXX"" />
            </div>
            <button class=""button"" onclick=""activateServer()"" id=""activateBtn"">
                🚀 Activate Server
            </button>
        </div>
        
        <div class=""section"">
            <div class=""section-title"">Available License Types</div>
            <div class=""license-types"">
                <div class=""license-type"">
                    <strong>Forum:</strong> Forum features
                </div>
                <div class=""license-type"">
                    <strong>CMS:</strong> Content management
                </div>
                <div class=""license-type"">
                    <strong>GameServer:</strong> Game hosting
                </div>
                <div class=""license-type"">
                    <strong>Enterprise:</strong> All features
                </div>
            </div>
        </div>
    </div>
    
    <script>
        async function checkAuth() {
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return;
            }
            
            // Check if already activated
            try {
                const response = await fetch('/api/control/activation-status', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    if (data.activated) {
                        window.location.href = '/control-panel';
                        return;
                    }
                    
                    // Show dev mode notice if applicable
                    if (data.devMode) {
                        document.getElementById('devModeNotice').style.display = 'block';
                    }
                }
            } catch (err) {
                console.log('Activation check error:', err);
            }
        }
        
        async function activateServer() {
            const licenseKey = document.getElementById('licenseKey').value.trim();
            
            if (!licenseKey) {
                showMessage('Please enter a license key', 'error');
                return;
            }
            
            const btn = document.getElementById('activateBtn');
            btn.disabled = true;
            btn.textContent = '🔄 Activating...';
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/control/activate', {
                    method: 'POST',
                    headers: {
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ licenseKey })
                });
                
                const data = await response.json();
                
                if (data.success) {
                    showMessage('✅ Server activated successfully! Redirecting to Control Panel...', 'success');
                    setTimeout(() => {
                        window.location.href = '/control-panel';
                    }, 2000);
                } else {
                    showMessage('❌ Activation failed: ' + data.message, 'error');
                    btn.disabled = false;
                    btn.textContent = '🚀 Activate Server';
                }
            } catch (error) {
                showMessage('❌ Activation error: ' + error.message, 'error');
                btn.disabled = false;
                btn.textContent = '🚀 Activate Server';
            }
        }
        
        function showMessage(message, type) {
            const statusMsg = document.getElementById('statusMessage');
            statusMsg.className = 'status-message ' + type;
            statusMsg.textContent = message;
            statusMsg.style.display = 'block';
            setTimeout(() => {
                if (type !== 'success') {
                    statusMsg.style.display = 'none';
                }
            }, 5000);
        }
        
        // Allow Enter key to submit
        document.getElementById('licenseKey').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                activateServer();
            }
        });
        
        checkAuth();
    </script>
</body>
</html>";
}

// Generate Control Panel UI dynamically
static string GenerateControlPanelUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Control Panel - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%);
            min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9);
            border-bottom: 2px solid rgba(138, 43, 226, 0.3);
            color: white;
            padding: 20px;
            text-align: center;
            position: relative;
        }
        .header h1 {
            background: linear-gradient(to right, #ffffff, #c084fc);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .header p {
            color: #c8b6ff;
            margin-top: 10px;
        }
        .nav-tabs {
            display: flex;
            gap: 10px;
            margin: 20px;
            flex-wrap: wrap;
            justify-content: center;
        }
        .tab-button {
            padding: 12px 24px;
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            border-radius: 8px;
            color: #c8b6ff;
            cursor: pointer;
            transition: all 0.3s;
            font-size: 14px;
        }
        .tab-button:hover {
            border-color: rgba(138, 43, 226, 0.6);
            background: rgba(40, 0, 60, 0.9);
        }
        .tab-button.active {
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            border-color: rgba(138, 43, 226, 0.8);
            color: white;
        }
        .container {
            max-width: 1200px;
            margin: 20px auto;
            padding: 20px;
        }
        .tab-content {
            display: none;
        }
        .tab-content.active {
            display: block;
        }
        .stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .stat-card {
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.3);
            transition: all 0.3s;
        }
        .stat-card:hover {
            border-color: rgba(138, 43, 226, 0.6);
            transform: translateY(-3px);
        }
        .stat-card h3 { color: #c084fc; margin-bottom: 10px; }
        .stat-card p { color: #e0d0ff; font-size: 1.2em; }
        .modules {
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.3);
        }
        .modules h2 { color: #c084fc; margin-bottom: 15px; }
        .module { 
            padding: 10px; 
            border-bottom: 1px solid rgba(138, 43, 226, 0.2);
            color: #d8c8ff;
        }
        .setting-group {
            background: rgba(20, 0, 40, 0.9);
            border: 2px solid rgba(138, 43, 226, 0.3);
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        .setting-group h3 { color: #c084fc; margin-bottom: 15px; }
        .form-group {
            margin-bottom: 15px;
        }
        .form-group label {
            display: block;
            color: #c8b6ff;
            margin-bottom: 5px;
        }
        .form-group select, .form-group input {
            width: 100%;
            padding: 10px;
            background: rgba(10, 0, 20, 0.9);
            border: 1px solid rgba(138, 43, 226, 0.3);
            border-radius: 5px;
            color: #e0d0ff;
            font-size: 14px;
        }
        .btn {
            padding: 10px 20px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            color: white;
            cursor: pointer;
            transition: all 0.3s;
        }
        .btn:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        .success-msg {
            padding: 10px;
            background: rgba(16, 185, 129, 0.2);
            border: 1px solid rgba(16, 185, 129, 0.5);
            border-radius: 5px;
            color: #6ee7b7;
            margin-top: 10px;
            display: none;
        }
        .logout { 
            position: absolute;
            top: 20px;
            right: 20px;
            color: white;
            text-decoration: none;
            padding: 10px 20px;
            background: linear-gradient(135deg, #8b2fc7 0%, #6a1b9a 100%);
            border: 2px solid rgba(138, 43, 226, 0.5);
            border-radius: 8px;
            transition: all 0.3s;
        }
        .logout:hover {
            background: linear-gradient(135deg, #a13dd6 0%, #7d22ab 100%);
            box-shadow: 0 4px 15px rgba(138, 43, 226, 0.6);
        }
        .learning-link {
            display: inline-block;
            margin-top: 10px;
            padding: 10px 20px;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            border: 2px solid rgba(59, 130, 246, 0.5);
            border-radius: 8px;
            color: white;
            text-decoration: none;
            transition: all 0.3s;
        }
        .learning-link:hover {
            background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
            box-shadow: 0 4px 15px rgba(59, 130, 246, 0.6);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎛️ AGP Studios, INC - Control Panel</h1>
        <p>SiteBuilder - Internal Module Interface</p>
        <a href=""#"" class=""logout"" onclick=""logout()"">Logout</a>
    </div>
    
    <div class=""nav-tabs"">
        <button class=""tab-button active"" onclick=""switchTab('dashboard')"">📊 Dashboard</button>
        <button class=""tab-button"" onclick=""switchTab('cms-settings')"">⚙️ CMS Settings</button>
        <button class=""tab-button"" onclick=""switchTab('learning')"">📚 Learning Module</button>
        <button class=""tab-button"" onclick=""switchTab('module-settings')"">🔧 Module Settings</button>
    </div>
    
    <div class=""container"">
        <!-- Dashboard Tab -->
        <div id=""dashboard"" class=""tab-content active"">
            <div class=""stats"">
                <div class=""stat-card"">
                    <h3>System Status</h3>
                    <p id=""status"">Loading...</p>
                </div>
                <div class=""stat-card"">
                    <h3>Loaded Modules</h3>
                    <p id=""modules"">Loading...</p>
                </div>
                <div class=""stat-card"">
                    <h3>Active Users</h3>
                    <p id=""users"">Loading...</p>
                </div>
            </div>
            <div class=""modules"">
                <h2>Available Modules</h2>
                <div id=""moduleList"">Loading modules...</div>
            </div>
        </div>

        <!-- CMS Settings Tab -->
        <div id=""cms-settings"" class=""tab-content"">
            <div class=""setting-group"">
                <h3>🎨 Theme Configuration</h3>
                <p style=""color: #c8b6ff; margin-bottom: 20px;"">Configure the theme for the entire website including CMS and Learning Module</p>
                <div class=""form-group"">
                    <label for=""theme-select"">Website Theme:</label>
                    <select id=""theme-select"" onchange=""previewTheme()"">
                        <option value=""classic"">Classic (Purple/Dark)</option>
                        <option value=""light"">Light Theme</option>
                        <option value=""dark"">Dark Theme</option>
                        <option value=""blue"">Blue Ocean</option>
                        <option value=""green"">Forest Green</option>
                    </select>
                </div>
                <div class=""form-group"">
                    <label>
                        <input type=""checkbox"" id=""custom-themes"" checked> 
                        <span style=""color: #c8b6ff;"">Allow users to override theme</span>
                    </label>
                </div>
                <button class=""btn"" onclick=""saveThemeSettings()"">Save Theme Settings</button>
                <div id=""theme-success"" class=""success-msg"">Theme settings saved successfully!</div>
            </div>
            
            <div class=""setting-group"">
                <h3>🌐 Site Configuration</h3>
                <div class=""form-group"">
                    <label for=""site-name"">Site Name:</label>
                    <input type=""text"" id=""site-name"" value=""Legendary CMS"">
                </div>
                <div class=""form-group"">
                    <label for=""admin-email"">Admin Email:</label>
                    <input type=""email"" id=""admin-email"" value=""admin@legendarycms.local"">
                </div>
                <div class=""form-group"">
                    <label>
                        <input type=""checkbox"" id=""under-construction""> 
                        <span style=""color: #c8b6ff;"">Enable Under Construction Mode</span>
                    </label>
                    <small style=""display: block; color: #999; margin-top: 5px;"">
                        When enabled, non-admin users will see an &quot;Under Construction&quot; page
                    </small>
                </div>
                <button class=""btn"" onclick=""saveSiteSettings()"">Save Site Settings</button>
                <div id=""site-success"" class=""success-msg"">Site settings saved successfully!</div>
            </div>
        </div>

        <!-- Learning Module Tab -->
        <div id=""learning"" class=""tab-content"">
            <div class=""modules"">
                <h2>📚 Learning Module (Class System)</h2>
                <p style=""color: #c8b6ff; margin-bottom: 20px;"">
                    Welcome to the Learning Module! Access courses, track your progress, and earn achievements.
                </p>
                <div class=""stat-card"">
                    <h3>🎓 Available Courses</h3>
                    <p id=""course-count"">Loading...</p>
                    <a href=""/cms/learning"" class=""learning-link"">📖 Browse All Courses</a>
                </div>
                <div class=""stat-card"" style=""margin-top: 20px;"">
                    <h3>📋 Your Progress</h3>
                    <p id=""user-progress"">Loading...</p>
                    <a href=""/cms/learning/progress"" class=""learning-link"">📊 View Progress</a>
                </div>
                <div class=""stat-card"" style=""margin-top: 20px;"">
                    <h3>🏆 Achievements</h3>
                    <p id=""achievements"">Loading...</p>
                    <a href=""/cms/learning/achievements"" class=""learning-link"">🎖️ View Achievements</a>
                </div>
            </div>
        </div>

        <!-- Module Settings Tab -->
        <div id=""module-settings"" class=""tab-content"">
            <div class=""modules"">
                <h2>🔧 Module Settings</h2>
                <p style=""color: #c8b6ff; margin-bottom: 20px;"">
                    Configure settings for each loaded module. Changes are saved to the database.
                </p>
                <div id=""module-settings-container"">
                    <p style=""color: #c8b6ff;"">Loading module settings...</p>
                </div>
            </div>
        </div>
    </div>
    
    <script>
        let currentTab = 'dashboard';
        let loadedModules = [];
        
        function switchTab(tabName) {
            // Hide all tabs
            document.querySelectorAll('.tab-content').forEach(tab => {
                tab.classList.remove('active');
            });
            document.querySelectorAll('.tab-button').forEach(btn => {
                btn.classList.remove('active');
            });
            
            // Show selected tab
            document.getElementById(tabName).classList.add('active');
            // Find and activate the clicked button
            const buttons = document.querySelectorAll('.tab-button');
            buttons.forEach(btn => {
                if (btn.textContent.toLowerCase().includes(tabName.replace('-', ' '))) {
                    btn.classList.add('active');
                }
            });
            currentTab = tabName;
            
            // Load tab-specific data
            if (tabName === 'cms-settings') {
                loadCmsSettings();
            } else if (tabName === 'learning') {
                loadLearningModuleData();
            } else if (tabName === 'module-settings') {
                loadModuleSettings();
            }
        }
        
        async function checkAuth() {
            const token = localStorage.getItem('ASHATCore_token');
            if (!token) {
                window.location.href = '/login';
                return;
            }
        }
        
        async function loadStats() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/control/stats', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                const data = await response.json();
                if (data.success) {
                    document.getElementById('status').textContent = 'Online';
                    document.getElementById('users').textContent = data.stats?.totalUsers || '0';
                    
                    // Load modules count and list
                    await loadModules();
                }
            } catch (err) {
                console.error('Error loading stats:', err);
            }
        }
        
        async function loadModules() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/control/modules', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                const data = await response.json();
                if (data.success && data.modules) {
                    loadedModules = data.modules;
                    document.getElementById('modules').textContent = data.modules.length;
                    
                    const moduleList = document.getElementById('moduleList');
                    if (data.modules.length === 0) {
                        moduleList.innerHTML = '<p style=""color: #d8c8ff;"">No modules loaded</p>';
                    } else {
                        moduleList.innerHTML = data.modules.map(mod => 
                            '<div class=""module""><strong>' + mod.name + '</strong> - ' + mod.description + 
                            ' <span style=""color: #c084fc; margin-left: 10px;"">[' + mod.category + ']</span></div>'
                        ).join('');
                    }
                }
            } catch (err) {
                console.error('Error loading modules:', err);
                document.getElementById('moduleList').innerHTML = '<p style=""color: #ef4444;"">Error loading modules</p>';
            }
        }
        
        async function loadModuleSettings() {
            const container = document.getElementById('module-settings-container');
            
            if (loadedModules.length === 0) {
                container.innerHTML = '<p style=""color: #d8c8ff;"">No modules available. Please load the dashboard first.</p>';
                return;
            }
            
            try {
                const token = localStorage.getItem('ASHATCore_token');
                container.innerHTML = '<p style=""color: #c8b6ff;"">Loading settings...</p>';
                
                // Group modules by category
                const categories = {};
                loadedModules.forEach(mod => {
                    if (!categories[mod.category]) {
                        categories[mod.category] = [];
                    }
                    categories[mod.category].push(mod);
                });
                
                let html = '';
                
                for (const [category, modules] of Object.entries(categories)) {
                    html += '<div class=""setting-group""><h3>Category: ' + category + '</h3>';
                    
                    for (const module of modules) {
                        html += '<div style=""margin-bottom: 30px; padding: 15px; background: rgba(40, 0, 60, 0.5); border-radius: 8px;"">';
                        html += '<h4 style=""color: #c084fc; margin-bottom: 15px;"">' + module.name + ' Module</h4>';
                        
                        // Fetch settings for this module
                        try {
                            const settingsResponse = await fetch('/api/control/modules/' + module.name + '/settings', {
                                headers: { 'Authorization': 'Bearer ' + token }
                            });
                            const settingsData = await settingsResponse.json();
                            
                            if (settingsData.success && settingsData.settings && Object.keys(settingsData.settings).length > 0) {
                                html += '<div id=""settings-' + module.name + '"">';
                                for (const [key, value] of Object.entries(settingsData.settings)) {
                                    html += '<div class=""form-group""><label>' + key + ':</label>';
                                    html += '<input type=""text"" id=""setting-' + module.name + '-' + key + '"" value=""' + value + '"" /></div>';
                                }
                                html += '</div>';
                            } else {
                                html += '<p style=""color: #999; font-size: 14px;"">No settings configured for this module.</p>';
                                html += '<div class=""form-group""><label>Sample Setting:</label>';
                                html += '<input type=""text"" id=""setting-' + module.name + '-enabled"" value=""true"" />';
                                html += '<small style=""color: #999; display: block; margin-top: 5px;"">This is an example. Module-specific settings can be added.</small>';
                                html += '</div>';
                            }
                            
                            html += '<button class=""btn"" onclick=""saveModuleSettings(\'' + module.name + '\')"">Save ' + module.name + ' Settings</button>';
                        } catch (err) {
                            console.error('Error loading settings for ' + module.name + ':', err);
                            html += '<p style=""color: #ef4444;"">Error loading settings</p>';
                        }
                        
                        html += '</div>';
                    }
                    
                    html += '</div>';
                }
                
                container.innerHTML = html;
            } catch (err) {
                console.error('Error loading module settings:', err);
                container.innerHTML = '<p style=""color: #ef4444;"">Error loading module settings</p>';
            }
        }
        
        async function saveModuleSettings(moduleName) {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const settings = {};
                
                // Collect all settings for this module
                const inputs = document.querySelectorAll('[id^=""setting-' + moduleName + '-""]');
                inputs.forEach(input => {
                    const key = input.id.replace('setting-' + moduleName + '-', '');
                    settings[key] = input.value;
                });
                
                const response = await fetch('/api/control/modules/' + moduleName + '/settings', {
                    method: 'POST',
                    headers: {
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(settings)
                });
                
                const data = await response.json();
                if (data.success) {
                    alert(moduleName + ' settings saved successfully!');
                } else {
                    alert('Error saving settings: ' + data.error);
                }
            } catch (err) {
                console.error('Error saving settings:', err);
                alert('Error saving settings');
            }
        }
        
        async function loadCmsSettings() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/control/cms/settings', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                const data = await response.json();
                if (data.success && data.settings) {
                    document.getElementById('theme-select').value = data.settings.theme || 'classic';
                    document.getElementById('custom-themes').checked = data.settings.allowCustomThemes !== false;
                    document.getElementById('site-name').value = data.settings.siteName || 'Legendary CMS';
                    document.getElementById('admin-email').value = data.settings.adminEmail || '';
                    document.getElementById('under-construction').checked = data.settings.underConstruction || false;
                }
            } catch (err) {
                console.error('Error loading CMS settings:', err);
            }
        }
        
        function previewTheme() {
            const theme = document.getElementById('theme-select').value;
            // Theme preview functionality can be implemented in future
            // For now, just log the selected theme
        }
        
        async function saveThemeSettings() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const settings = {
                    theme: document.getElementById('theme-select').value,
                    allowCustomThemes: document.getElementById('custom-themes').checked
                };
                
                const response = await fetch('/api/control/cms/settings/theme', {
                    method: 'POST',
                    headers: { 
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(settings)
                });
                
                const data = await response.json();
                if (data.success) {
                    const msg = document.getElementById('theme-success');
                    msg.style.display = 'block';
                    setTimeout(() => msg.style.display = 'none', 3000);
                }
            } catch (err) {
                console.error('Error saving theme settings:', err);
                alert('Error saving theme settings');
            }
        }
        
        async function saveSiteSettings() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                
                // Save site settings
                const siteSettings = {
                    siteName: document.getElementById('site-name').value,
                    adminEmail: document.getElementById('admin-email').value
                };
                
                const siteResponse = await fetch('/api/control/cms/settings/site', {
                    method: 'POST',
                    headers: { 
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(siteSettings)
                });
                
                // Save under construction setting separately
                const underConstruction = document.getElementById('under-construction').checked;
                const ucResponse = await fetch('/api/control/cms/settings/underconstruction', {
                    method: 'POST',
                    headers: { 
                        'Authorization': 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ enabled: underConstruction })
                });
                
                const data = await siteResponse.json();
                const ucData = await ucResponse.json();
                
                if (data.success && ucData.success) {
                    const msg = document.getElementById('site-success');
                    msg.style.display = 'block';
                    setTimeout(() => msg.style.display = 'none', 3000);
                }
            } catch (err) {
                console.error('Error saving site settings:', err);
                alert('Error saving site settings');
            }
        }
        
        async function loadLearningModuleData() {
            try {
                const token = localStorage.getItem('ASHATCore_token');
                const response = await fetch('/api/learning/courses/User', {
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                const data = await response.json();
                if (data.success && data.courses) {
                    document.getElementById('course-count').textContent = data.courses.length + ' courses available';
                } else {
                    document.getElementById('course-count').textContent = 'No courses available';
                }
                
                // Load user progress (placeholder)
                document.getElementById('user-progress').textContent = '0% complete';
                document.getElementById('achievements').textContent = '0 achievements unlocked';
            } catch (err) {
                console.error('Error loading learning module data:', err);
                document.getElementById('course-count').textContent = 'Error loading courses';
            }
        }
        
        function logout() {
            localStorage.removeItem('ASHATCore_token');
            window.location.href = '/login';
        }
        
        checkAuth();
        loadStats();
    </script>
</body>
</html>";
}

// Generate Admin UI dynamically
static string GenerateAdminUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Admin Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        h2 { color: #667eea; margin-bottom: 15px; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>⚙️ Admin Dashboard</h1>
        <p>SiteBuilder - Administrative Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Server Management</h2>
            <p>Advanced server Configuration and monitoring tools.</p>
        </div>
        <div class=""card"">
            <h2>User Management</h2>
            <p>Manage users, roles, and permissions.</p>
        </div>
        <div class=""card"">
            <h2>Module Management</h2>
            <p>Load, unload, and configure modules.</p>
        </div>
    </div>
</body>
</html>";
}

// Generate Game Engine Dashboard UI dynamically
static string GenerateGameEngineDashboardUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Game Engine Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎮 Game Engine Dashboard</h1>
        <p>SiteBuilder - Game Engine Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Active Scenes</h2>
            <p>Manage game scenes, entities, and physics.</p>
        </div>
        <div class=""card"">
            <h2>Multiplayer Sessions</h2>
            <p>Monitor active game sessions and players.</p>
        </div>
    </div>
</body>
</html>";
}

// Generate Client Builder Dashboard UI dynamically
static string GenerateClientBuilderDashboardUI()
{
    return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Client Builder Dashboard - ASHATCore</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0033 50%, #2d004d 100%); min-height: 100vh;
        }
        .header {
            background: rgba(20, 0, 40, 0.9); border-bottom: 2px solid rgba(138, 43, 226, 0.3);
            color: white;
            padding: 20px;
            text-align: center;
        }
        .container { max-width: 1200px; margin: 20px auto; padding: 20px; }
        .card {
            background: white;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🛠️ Client Builder Dashboard</h1>
        <p>SiteBuilder - Client Generation Interface</p>
    </div>
    <div class=""container"">
        <div class=""card"">
            <h2>Client Templates</h2>
            <p>Select and customize client templates.</p>
        </div>
        <div class=""card"">
            <h2>Build Queue</h2>
            <p>Monitor client build progress and downloads.</p>
        </div>
    </div>
</body>
</html>";
}

// Root endpoint - always register to handle Under Construction and CMS routing
// Phase 9.3.9: Unified homepage handler for all scenarios
app.MapGet("/", async (HttpContext context) =>
{
    try
    {
        var serverConfig = firstRunManager.GetServerConfiguration();
        
        // Calculate days remaining for activation
        var daysRemaining = 30;
        if (serverConfig.ServerFirstStarted.HasValue && !serverConfig.ServerActivated)
        {
            var daysSinceStart = (DateTime.UtcNow - serverConfig.ServerFirstStarted.Value).TotalDays;
            daysRemaining = Math.Max(0, 30 - (int)Math.Ceiling(daysSinceStart));
        }
        
        // Force under construction if not activated after 30 days
        var forcedUnderConstruction = !serverConfig.ServerActivated && daysRemaining <= 0;
        
        // Phase 9.3.8: Check for Under Construction mode FIRST
        // This check must happen before any response headers or body are written
        if (serverConfig.UnderConstruction || forcedUnderConstruction)
        {
            // Check if user is an admin - admins can still access during construction
            var authHeader = context.Request.Headers["Authorization"].ToString();
            var cookieToken = context.Request.Cookies["authToken"];
            var token = !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ") 
                ? authHeader[7..] 
                : cookieToken ?? "";
            
            User? user = null;
            if (!string.IsNullOrWhiteSpace(token) && authModule != null)
            {
                user = await authModule.GetUserByTokenAsync(token);
            }
            
            // Non-admins see Under Construction page
            if (user == null || user.Role < UserRole.Admin)
            {
                // Set ContentType only when we're about to write the response
                context.Response.ContentType = "text/html";
                var message = forcedUnderConstruction 
                    ? "This server has not been activated and the 30-day trial period has expired. Please contact the administrator to activate the server."
                    : serverConfig.UnderConstructionMessage;
                    
                var tempConfig = new ServerConfiguration
                {
                    UnderConstructionMessage = message,
                    UnderConstructionRobotImage = serverConfig.UnderConstructionRobotImage
                };
                
                await context.Response.WriteAsync(UnderConstructionHandler.GenerateUnderConstructionPage(tempConfig));
                return;
            }
            
            // Admins can access normally - continue to CMS or fallback
        }
        
        // Phase 9.3.9: Redirect to full CMS homepage (Razor Pages)
        // The CMS homepage is always available via Razor Pages
        // It integrates with LegendaryCMS module when available for dynamic content
        Console.WriteLine("[ASHATCore] Redirecting to full CMS homepage (Razor Pages)");
        context.Response.Redirect("/Index");
        return;
    }
    catch (Exception ex)
    {
        // Error handling to prevent server cASHATshes if header setting fails
        Console.WriteLine($"[ASHATCore] Error handling homepage request: {ex.Message}");
        // Only try to set status code if response hasn't started
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body><h1>500 Internal Server Error</h1><p>An error occurred while loading the homepage.</p></body></html>");
        }
    }
});

// ============================================================================
// - UI ROUTES (Dynamic, no static files)
// All UI features accessed through internal module routing
// ============================================================================

// Control Panel UI - served dynamically
app.MapGet("/control-panel", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateControlPanelUI());
});

// Note: /login is handled by Razor Pages (Pages/Login.cshtml)

// Onboarding UI - served dynamically (for Masters class onboarding)
app.MapGet("/onboarding", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateOnboardingUI());
});

// Activation UI - served dynamically (for license activation)
app.MapGet("/activation", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateActivationUI());
});

// Admin UI - served dynamically
app.MapGet("/admin", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateAdminUI());
});

// Game Engine Dashboard UI - served dynamically
app.MapGet("/gameengine-dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateGameEngineDashboardUI());
});

// Client Builder Dashboard UI - served dynamically
app.MapGet("/clientbuilder-dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(GenerateClientBuilderDashboardUI());
});

Console.WriteLine("[ASHATCore] UI routes registered (dynamic, no static files):");
Console.WriteLine("  GET  /control-panel - Control Panel UI (served dynamically)");
Console.WriteLine("  GET  /login - Login UI (served via Razor Pages)");
Console.WriteLine("  GET  /admin - Admin UI (served dynamically)");
Console.WriteLine("  GET  /gameengine-dashboard - Game Engine Dashboard UI (served dynamically)");
Console.WriteLine("  GET  /clientbuilder-dashboard - Client Builder Dashboard UI (served dynamically)");

// ============================================================================
// CONTROL PANEL API ENDPOINTS
// ============================================================================

// Get RaCoin module if present
IRaCoinModule? RaCoinModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IRaCoinModule>()
    .FirstOrDefault();

// Get License module if present
ILicenseModule? licenseModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<ILicenseModule>()
    .FirstOrDefault();

// Server Configuration API Endpoints
app.MapGet("/api/control/server/config", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Insufficient permissions" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        await context.Response.WriteAsJsonAsync(new { success = true, config });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapPost("/api/control/server/mode", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.SuperAdmin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "SuperAdmin role required" });
            return;
        }

        var request = await context.Request.ReadFromJsonAsync<ServerModeChangeRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
            return;
        }

        if (!Enum.TryParse<ServerMode>(request.Mode, true, out var mode))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid server mode" });
            return;
        }

        firstRunManager.SetServerMode(mode);
        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true, 
            message = $"Server mode changed to {mode}",
            mode = mode.ToString(),
            requiresRestart = true
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapGet("/api/control/server/modes", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Insufficient permissions" });
            return;
        }

        var modes = Enum.GetValues<ServerMode>().Select(m => new
        {
            value = m.ToString(),
            name = m.ToString(),
            description = m switch
            {
                ServerMode.Alpha => "Early development and testing with full logging",
                ServerMode.Beta => "Pre-release testing with selected users",
                ServerMode.Omega => "Main server Configuration (us-omega)",
                ServerMode.Demo => "Demonstration instance with limited features",
                ServerMode.Production => "Full production deployment",
                _ => ""
            }
        });

        await context.Response.WriteAsJsonAsync(new { success = true, modes });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

// Under Construction Mode API endpoints (Phase 9.3.8)
app.MapPost("/api/control/server/underconstruction", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
            return;
        }

        var request = await context.Request.ReadFromJsonAsync<UnderConstructionRequest>();
        if (request == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid request" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        config.UnderConstruction = request.Enabled;
        
        if (request.Message != null)
        {
            config.UnderConstructionMessage = request.Message;
        }

        // Save the Configuration
        firstRunManager.SaveConfiguration();

        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true, 
            message = request.Enabled ? "Under Construction mode enabled" : "Under Construction mode disabled",
            underConstruction = config.UnderConstruction,
            customMessage = config.UnderConstructionMessage,
            customRobotImage = config.UnderConstructionRobotImage
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.MapGet("/api/control/server/underconstruction", async (HttpContext context) =>
{
    try
    {
        if (authModule == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Authentication not available" });
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
        var user = await authModule.GetUserByTokenAsync(token);

        if (user == null || user.Role < UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Admin role required" });
            return;
        }

        var config = firstRunManager.GetServerConfiguration();
        await context.Response.WriteAsJsonAsync(new 
        { 
            success = true,
            underConstruction = config.UnderConstruction,
            message = config.UnderConstructionMessage,
            robotImage = config.UnderConstructionRobotImage
        });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

Console.WriteLine("[ASHATCore] Server Configuration API endpoints registered:");
Console.WriteLine("  GET  /api/control/server/config - Get server Configuration (Admin+)");
Console.WriteLine("  POST /api/control/server/mode - Change server mode (SuperAdmin only)");
Console.WriteLine("  GET  /api/control/server/modes - List available server modes (Admin+)");
Console.WriteLine("  POST /api/control/server/underconstruction - Toggle Under Construction mode (Admin+)");
Console.WriteLine("  GET  /api/control/server/underconstruction - Get Under Construction status (Admin+)");



// Control Panel API endpoints
app.MapControlPanelEndpoints(moduleManager, authModule, licenseModule, RaCoinModule, gameEngineModule, firstRunManager);

// ============================================================================
// Distribution & Update API Endpoints
// ============================================================================

var distributionModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IDistributionModule>()
    .FirstOrDefault();

var updateModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IUpdateModule>()
    .FirstOrDefault();

app.MapDistributionEndpoints(distributionModule, updateModule, authModule);

// ============================================================================
// GameClient API Endpoints
// ============================================================================

var gameClientModule = moduleManager.Modules
    .Select(m => m.Instance)
    .OfType<IGameClientModule>()
    .FirstOrDefault();

app.MapGameClientEndpoints(gameClientModule, authModule);

// ============================================================================
// Razor Pages for CMS
// ============================================================================
app.MapRazorPages();
Console.WriteLine("[ASHATCore] Razor Pages CMS enabled:");
Console.WriteLine("  GET  /Index - CMS Homepage (full content management system)");
Console.WriteLine("  GET  /cms/blogs - Blog system");
Console.WriteLine("  GET  /cms/forums - Forum platform");
Console.WriteLine("  GET  /cms/profiles - User profiles");

app.Run();
