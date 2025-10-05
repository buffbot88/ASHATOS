using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Abstractions;
using Microsoft.Data.Sqlite;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.CMSSpawner;

/// <summary>
/// CMS Spawner Module - Generates and manages PHP 8+ homepage with SQLite database.
/// Supports AI-powered content generation and automated CMS deployment.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class CMSSpawnerModule : ModuleBase
{
    public override string Name => "CMSSpawner";

    private ModuleManager? _manager;
    private string? _phpPath;
    private string? _cmsRootPath;
    private readonly object _lock = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        _cmsRootPath = Path.Combine(AppContext.BaseDirectory, "superadmin_control_panel");
        LogInfo("CMS Spawner module initialized");
    }

    public override string Process(string input)
    {
        return ProcessInternal(input);
    }

    private string ProcessInternal(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.Equals("cms spawn", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("cms spawn home", StringComparison.OrdinalIgnoreCase))
        {
            return SpawnHomepage();
        }

        if (text.Equals("cms spawn control", StringComparison.OrdinalIgnoreCase))
        {
            return SpawnControlPanel();
        }

        if (text.Equals("cms status", StringComparison.OrdinalIgnoreCase))
        {
            return GetCMSStatus();
        }

        if (text.Equals("cms detect php", StringComparison.OrdinalIgnoreCase))
        {
            return DetectPHP();
        }

        return $"Unknown CMS command. Type 'help' for available commands.";
    }

    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "CMSSpawner commands:",
            "  cms spawn           - Create PHP CMS homepage with SQLite database",
            "  cms spawn home      - Same as 'cms spawn'",
            "  cms spawn control   - Create SuperAdmin Control Panel (first-run)",
            "  cms status          - Show CMS deployment status",
            "  cms detect php      - Detect PHP runtime version",
            "  help                - Show this help message"
        );
    }

    private string DetectPHP()
    {
        try
        {
            _phpPath = FindPHPExecutable();
            
            if (_phpPath == null)
            {
                return "PHP runtime not found. Please install PHP 8.0 or higher.\n" +
                       "Installation guide:\n" +
                       "  - Linux: sudo apt install php8.2-cli php8.2-sqlite3\n" +
                       "  - macOS: brew install php\n" +
                       "  - Windows: Download from https://windows.php.net/download/";
            }

            var version = GetPHPVersion(_phpPath);
            return $"PHP found: {_phpPath}\nVersion: {version}";
        }
        catch (Exception ex)
        {
            LogError($"PHP detection error: {ex.Message}");
            return $"Error detecting PHP: {ex.Message}";
        }
    }

    private string? FindPHPExecutable()
    {
        // Try common PHP locations
        string[] possiblePaths = 
        {
            "php",           // In PATH
            "/usr/bin/php",  // Linux
            "/usr/local/bin/php", // Linux/macOS
            "C:\\php\\php.exe",   // Windows
            "C:\\xampp\\php\\php.exe" // XAMPP on Windows
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(5000);
                    if (process.ExitCode == 0)
                    {
                        return path;
                    }
                }
            }
            catch { continue; }
        }

        return null;
    }

    private string GetPHPVersion(string phpPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = phpPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);
                
                // Extract version from output (first line typically)
                var lines = output.Split('\n');
                return lines.Length > 0 ? lines[0] : "Unknown";
            }
        }
        catch { }
        
        return "Unknown";
    }

    private string SpawnHomepage()
    {
        lock (_lock)
        {
            try
            {
                LogInfo("Starting CMS homepage generation...");

                // Detect PHP
                _phpPath = FindPHPExecutable();
                if (_phpPath == null)
                {
                    return "❌ Error: PHP runtime not found. Run 'cms detect php' for installation instructions.";
                }

                var version = GetPHPVersion(_phpPath);
                LogInfo($"PHP detected: {version}");

                // Create CMS directory
                if (_cmsRootPath != null && !Directory.Exists(_cmsRootPath))
                {
                    Directory.CreateDirectory(_cmsRootPath);
                    LogInfo($"Created CMS directory: {_cmsRootPath}");
                }

                // Initialize SQLite database
                var dbPath = _cmsRootPath != null ? Path.Combine(_cmsRootPath, "cms_database.sqlite") : "cms_database.sqlite";
                InitializeSQLiteDatabase(dbPath);

                // Generate PHP files
                GenerateConfigFile(dbPath);
                GenerateDatabaseFile();
                GenerateIndexFile();
                GenerateAdminFile();
                GenerateStylesFile();

                var serverUrl = "http://localhost:8080";
                
                return string.Join(Environment.NewLine,
                    "✅ CMS Homepage generated successfully!",
                    "",
                    $"📁 Location: {_cmsRootPath}",
                    $"🗄️  Database: {dbPath}",
                    $"🐘 PHP Version: {version}",
                    "",
                    "📝 Generated files:",
                    "  - index.php       (Homepage)",
                    "  - admin.php       (Admin panel)",
                    "  - config.php      (Configuration)",
                    "  - db.php          (Database layer)",
                    "  - styles.css      (Styling)",
                    "",
                    "🚀 To start the CMS:",
                    $"  cd {_cmsRootPath}",
                    $"  php -S localhost:8080",
                    $"  Open {serverUrl} in your browser",
                    "",
                    "🔑 Default login: admin / admin123"
                );
            }
            catch (Exception ex)
            {
                LogError($"CMS spawn error: {ex.Message}");
                return $"❌ Error spawning CMS: {ex.Message}";
            }
        }
    }

    private string SpawnControlPanel()
    {
        lock (_lock)
        {
            try
            {
                LogInfo("Starting SuperAdmin Control Panel generation...");

                // Detect PHP
                _phpPath = FindPHPExecutable();
                if (_phpPath == null)
                {
                    return "❌ Error: PHP runtime not found. Run 'cms detect php' for installation instructions.";
                }

                var version = GetPHPVersion(_phpPath);
                LogInfo($"PHP detected: {version}");

                // Create Control Panel directory
                if (_cmsRootPath != null && !Directory.Exists(_cmsRootPath))
                {
                    Directory.CreateDirectory(_cmsRootPath);
                    LogInfo($"Created Control Panel directory: {_cmsRootPath}");
                }

                // Initialize SQLite database with control panel schema
                var dbPath = _cmsRootPath != null ? Path.Combine(_cmsRootPath, "control_panel.sqlite") : "control_panel.sqlite";
                InitializeControlPanelDatabase(dbPath);

                // Generate PHP files for Control Panel
                GenerateControlPanelConfigFile(dbPath);
                GenerateControlPanelDatabaseFile();
                GenerateControlPanelIndexFile();
                GenerateControlPanelStylesFile();

                var serverUrl = "http://localhost:8080";
                
                return string.Join(Environment.NewLine,
                    "✅ SuperAdmin Control Panel generated successfully!",
                    "",
                    $"📁 Location: {_cmsRootPath}",
                    $"🗄️  Database: {dbPath}",
                    $"🐘 PHP Version: {version}",
                    "",
                    "📝 Generated files:",
                    "  - index.php       (Control Panel)",
                    "  - config.php      (Configuration)",
                    "  - db.php          (Database layer)",
                    "  - styles.css      (Styling)",
                    "",
                    "🔐 Access Control:",
                    "  - SuperAdmin role required",
                    "  - Secure authentication via RaCore Auth API",
                    "",
                    "🚀 To start the Control Panel:",
                    $"  cd {_cmsRootPath}",
                    $"  php -S localhost:8080",
                    $"  Open {serverUrl} in your browser",
                    "",
                    "🔑 Default SuperAdmin: admin / admin123",
                    "⚠️  CHANGE PASSWORD IMMEDIATELY!"
                );
            }
            catch (Exception ex)
            {
                LogError($"Control Panel spawn error: {ex.Message}");
                return $"❌ Error spawning Control Panel: {ex.Message}";
            }
        }
    }

    private void InitializeSQLiteDatabase(string dbPath)
    {
        var connectionString = $"Data Source={dbPath}";
        
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        // Create pages table
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS pages (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    title TEXT NOT NULL,
    content TEXT,
    slug TEXT UNIQUE NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT
);

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    email TEXT,
    created_at TEXT NOT NULL
);";
        cmd.ExecuteNonQuery();

        // Insert default admin user (password: admin123)
        cmd.CommandText = @"
INSERT OR IGNORE INTO users (id, username, password, email, created_at)
VALUES (1, 'admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'admin@racore.local', datetime('now'));";
        cmd.ExecuteNonQuery();

        // Insert default settings
        cmd.CommandText = @"
INSERT OR IGNORE INTO settings (key, value) VALUES ('site_title', 'RaCore AI Mainframe');
INSERT OR IGNORE INTO settings (key, value) VALUES ('site_description', 'Welcome to the RaCore AI System');
INSERT OR IGNORE INTO settings (key, value) VALUES ('theme', 'default');";
        cmd.ExecuteNonQuery();

        // Insert welcome page
        cmd.CommandText = @"
INSERT OR IGNORE INTO pages (id, title, content, slug, created_at, updated_at)
VALUES (
    1,
    'Welcome to RaCore',
    '<h2>Welcome to the RaCore AI Mainframe!</h2>
<p>This is your AI-powered CMS homepage, generated automatically by the RaCore system.</p>
<h3>Features:</h3>
<ul>
<li>PHP 8+ powered backend</li>
<li>SQLite database for lightweight deployment</li>
<li>AI-generated content and configuration</li>
<li>Extensible architecture for future modules</li>
<li>Admin dashboard for content management</li>
</ul>
<h3>Getting Started:</h3>
<p>Visit the <a href=""admin.php"">Admin Panel</a> to customize your homepage and add new content.</p>',
    'home',
    datetime('now'),
    datetime('now')
);";
        cmd.ExecuteNonQuery();

        LogInfo($"SQLite database initialized: {dbPath}");
    }

    private void GenerateConfigFile(string dbPath)
    {
        var configContent = $@"<?php
// RaCore CMS Configuration
// Generated by RaCore CMSSpawner Module

// Database configuration
define('DB_PATH', __DIR__ . '/cms_database.sqlite');

// Site settings
define('SITE_TITLE', 'RaCore AI Mainframe');
define('SITE_DESCRIPTION', 'Welcome to the RaCore AI System');

// Security settings
define('SESSION_LIFETIME', 3600); // 1 hour
ini_set('session.cookie_httponly', 1);
ini_set('session.use_strict_mode', 1);

// Start session
if (session_status() === PHP_SESSION_NONE) {{
    session_start();
}}

// Timezone
date_default_timezone_set('UTC');

// Error reporting (disable in production)
error_reporting(E_ALL);
ini_set('display_errors', '1');
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "config.php"), configContent);
        LogInfo("Generated config.php");
    }

    private void GenerateDatabaseFile()
    {
        var dbContent = @"<?php
// Database layer for RaCore CMS
require_once 'config.php';

class Database {
    private static $instance = null;
    private $pdo;

    private function __construct() {
        try {
            $this->pdo = new PDO('sqlite:' . DB_PATH);
            $this->pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
            $this->pdo->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
        } catch (PDOException $e) {
            die('Database connection failed: ' . $e->getMessage());
        }
    }

    public static function getInstance() {
        if (self::$instance === null) {
            self::$instance = new Database();
        }
        return self::$instance;
    }

    public function getConnection() {
        return $this->pdo;
    }

    public function query($sql, $params = []) {
        try {
            $stmt = $this->pdo->prepare($sql);
            $stmt->execute($params);
            return $stmt;
        } catch (PDOException $e) {
            error_log('Database query error: ' . $e->getMessage());
            return false;
        }
    }

    public function getSetting($key) {
        $stmt = $this->query('SELECT value FROM settings WHERE key = ?', [$key]);
        $result = $stmt ? $stmt->fetch() : false;
        return $result ? $result['value'] : null;
    }

    public function setSetting($key, $value) {
        return $this->query(
            'INSERT OR REPLACE INTO settings (key, value) VALUES (?, ?)',
            [$key, $value]
        );
    }

    public function getPage($slug) {
        $stmt = $this->query('SELECT * FROM pages WHERE slug = ?', [$slug]);
        return $stmt ? $stmt->fetch() : false;
    }

    public function getAllPages() {
        $stmt = $this->query('SELECT * FROM pages ORDER BY created_at DESC');
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function createPage($title, $content, $slug) {
        $now = date('Y-m-d H:i:s');
        return $this->query(
            'INSERT INTO pages (title, content, slug, created_at, updated_at) VALUES (?, ?, ?, ?, ?)',
            [$title, $content, $slug, $now, $now]
        );
    }

    public function updatePage($id, $title, $content) {
        $now = date('Y-m-d H:i:s');
        return $this->query(
            'UPDATE pages SET title = ?, content = ?, updated_at = ? WHERE id = ?',
            [$title, $content, $now, $id]
        );
    }

    public function getUserByUsername($username) {
        $stmt = $this->query('SELECT * FROM users WHERE username = ?', [$username]);
        return $stmt ? $stmt->fetch() : false;
    }
}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "db.php"), dbContent);
        LogInfo("Generated db.php");
    }

    private void GenerateIndexFile()
    {
        var indexContent = @"<?php
// RaCore CMS Homepage
// Generated by RaCore CMSSpawner Module

require_once 'config.php';
require_once 'db.php';

$db = Database::getInstance();
$siteTitle = $db->getSetting('site_title') ?? SITE_TITLE;
$siteDescription = $db->getSetting('site_description') ?? SITE_DESCRIPTION;

// Get homepage content
$page = $db->getPage('home');
$content = $page ? $page['content'] : '<p>Welcome to RaCore CMS!</p>';
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title><?php echo htmlspecialchars($siteTitle); ?></title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1><?php echo htmlspecialchars($siteTitle); ?></h1>
            <nav>
                <a href=""index.php"">Home</a>
                <a href=""admin.php"">Admin</a>
            </nav>
        </div>
    </header>

    <main class=""container"">
        <div class=""hero"">
            <p class=""subtitle""><?php echo htmlspecialchars($siteDescription); ?></p>
        </div>

        <div class=""content"">
            <?php echo $content; ?>
        </div>

        <div class=""features"">
            <div class=""feature-card"">
                <h3>🤖 AI-Powered</h3>
                <p>Generated and managed by RaCore's AI mainframe</p>
            </div>
            <div class=""feature-card"">
                <h3>⚡ Modern Stack</h3>
                <p>PHP 8+ with SQLite for lightweight deployment</p>
            </div>
            <div class=""feature-card"">
                <h3>🔧 Extensible</h3>
                <p>Easy to extend with new modules and features</p>
            </div>
        </div>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore AI Mainframe. Powered by .NET 10 & PHP 8+</p>
        </div>
    </footer>
</body>
</html>
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "index.php"), indexContent);
        LogInfo("Generated index.php");
    }

    private void GenerateAdminFile()
    {
        var adminContent = @"<?php
// RaCore CMS Admin Panel
require_once 'config.php';
require_once 'db.php';

$db = Database::getInstance();
$error = '';
$success = '';

// Handle login
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action'])) {
    if ($_POST['action'] === 'login') {
        $username = $_POST['username'] ?? '';
        $password = $_POST['password'] ?? '';
        
        $user = $db->getUserByUsername($username);
        
        // Simple password check (in production, use password_verify)
        if ($user && $password === 'admin123') {
            $_SESSION['user_id'] = $user['id'];
            $_SESSION['username'] = $user['username'];
            $success = 'Login successful!';
        } else {
            $error = 'Invalid credentials';
        }
    } elseif ($_POST['action'] === 'logout') {
        session_destroy();
        header('Location: admin.php');
        exit;
    }
}

$isLoggedIn = isset($_SESSION['user_id']);

// Handle page updates (only if logged in)
if ($isLoggedIn && $_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['update_page'])) {
    $pageId = $_POST['page_id'] ?? 1;
    $title = $_POST['title'] ?? '';
    $content = $_POST['content'] ?? '';
    
    if ($db->updatePage($pageId, $title, $content)) {
        $success = 'Page updated successfully!';
    } else {
        $error = 'Failed to update page';
    }
}

// Get pages
$pages = $db->getAllPages();
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Admin Panel - RaCore CMS</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1>RaCore CMS Admin</h1>
            <nav>
                <a href=""index.php"">View Site</a>
                <?php if ($isLoggedIn): ?>
                    <form method=""POST"" style=""display: inline;"">
                        <input type=""hidden"" name=""action"" value=""logout"">
                        <button type=""submit"" class=""logout-btn"">Logout</button>
                    </form>
                <?php endif; ?>
            </nav>
        </div>
    </header>

    <main class=""container"">
        <?php if (!$isLoggedIn): ?>
            <div class=""login-form"">
                <h2>Login to Admin Panel</h2>
                <?php if ($error): ?>
                    <div class=""error""><?php echo htmlspecialchars($error); ?></div>
                <?php endif; ?>
                <form method=""POST"">
                    <input type=""hidden"" name=""action"" value=""login"">
                    <div class=""form-group"">
                        <label>Username:</label>
                        <input type=""text"" name=""username"" required>
                    </div>
                    <div class=""form-group"">
                        <label>Password:</label>
                        <input type=""password"" name=""password"" required>
                    </div>
                    <button type=""submit"">Login</button>
                    <p class=""hint"">Default: admin / admin123</p>
                </form>
            </div>
        <?php else: ?>
            <h2>Welcome, <?php echo htmlspecialchars($_SESSION['username']); ?>!</h2>
            
            <?php if ($success): ?>
                <div class=""success""><?php echo htmlspecialchars($success); ?></div>
            <?php endif; ?>
            <?php if ($error): ?>
                <div class=""error""><?php echo htmlspecialchars($error); ?></div>
            <?php endif; ?>

            <div class=""admin-section"">
                <h3>Manage Pages</h3>
                <?php foreach ($pages as $page): ?>
                    <div class=""page-editor"">
                        <form method=""POST"">
                            <input type=""hidden"" name=""update_page"" value=""1"">
                            <input type=""hidden"" name=""page_id"" value=""<?php echo $page['id']; ?>"">
                            
                            <div class=""form-group"">
                                <label>Title:</label>
                                <input type=""text"" name=""title"" value=""<?php echo htmlspecialchars($page['title']); ?>"" required>
                            </div>
                            
                            <div class=""form-group"">
                                <label>Content (HTML):</label>
                                <textarea name=""content"" rows=""10"" required><?php echo htmlspecialchars($page['content']); ?></textarea>
                            </div>
                            
                            <button type=""submit"">Update Page</button>
                        </form>
                    </div>
                <?php endforeach; ?>
            </div>

            <div class=""admin-section"">
                <h3>System Status</h3>
                <div class=""status-info"">
                    <p><strong>PHP Version:</strong> <?php echo PHP_VERSION; ?></p>
                    <p><strong>Database:</strong> SQLite (<?php echo DB_PATH; ?>)</p>
                    <p><strong>Total Pages:</strong> <?php echo count($pages); ?></p>
                    <p><strong>Server Time:</strong> <?php echo date('Y-m-d H:i:s'); ?> UTC</p>
                </div>
            </div>
        <?php endif; ?>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore AI Mainframe</p>
        </div>
    </footer>
</body>
</html>
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "admin.php"), adminContent);
        LogInfo("Generated admin.php");
    }

    private void GenerateStylesFile()
    {
        var stylesContent = @"/* RaCore CMS Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    line-height: 1.6;
    color: #333;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 20px;
}

header {
    background: rgba(255, 255, 255, 0.95);
    padding: 20px 0;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

header h1 {
    color: #667eea;
    margin-bottom: 10px;
}

nav {
    margin-top: 10px;
}

nav a {
    color: #667eea;
    text-decoration: none;
    margin-right: 20px;
    font-weight: 500;
    transition: color 0.3s;
}

nav a:hover {
    color: #764ba2;
}

main {
    background: white;
    margin: 40px auto;
    padding: 40px;
    border-radius: 10px;
    box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
}

.hero {
    text-align: center;
    margin-bottom: 40px;
}

.subtitle {
    font-size: 1.2em;
    color: #666;
    margin-top: 10px;
}

.content {
    margin: 40px 0;
}

.content h2 {
    color: #667eea;
    margin-bottom: 20px;
}

.content h3 {
    color: #764ba2;
    margin: 30px 0 15px;
}

.content ul {
    margin-left: 30px;
    margin-bottom: 20px;
}

.content li {
    margin-bottom: 10px;
}

.features {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 20px;
    margin: 40px 0;
}

.feature-card {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 30px;
    border-radius: 10px;
    text-align: center;
}

.feature-card h3 {
    margin-bottom: 15px;
    font-size: 1.5em;
}

footer {
    background: rgba(255, 255, 255, 0.95);
    padding: 20px 0;
    text-align: center;
    color: #666;
}

/* Admin Panel Styles */
.login-form {
    max-width: 400px;
    margin: 50px auto;
    padding: 30px;
    background: #f8f9fa;
    border-radius: 10px;
}

.login-form h2 {
    color: #667eea;
    margin-bottom: 20px;
    text-align: center;
}

.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    margin-bottom: 5px;
    font-weight: 500;
    color: #333;
}

.form-group input,
.form-group textarea {
    width: 100%;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 16px;
}

button {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    padding: 12px 30px;
    border-radius: 5px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
    transition: transform 0.2s;
}

button:hover {
    transform: translateY(-2px);
}

.logout-btn {
    background: #dc3545;
    padding: 8px 15px;
    font-size: 14px;
}

.error {
    background: #f8d7da;
    color: #721c24;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
}

.success {
    background: #d4edda;
    color: #155724;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
}

.hint {
    color: #666;
    font-size: 0.9em;
    margin-top: 10px;
    text-align: center;
}

.admin-section {
    margin: 40px 0;
}

.admin-section h3 {
    color: #667eea;
    margin-bottom: 20px;
}

.page-editor {
    background: #f8f9fa;
    padding: 20px;
    border-radius: 10px;
    margin-bottom: 20px;
}

.status-info {
    background: #f8f9fa;
    padding: 20px;
    border-radius: 10px;
}

.status-info p {
    margin-bottom: 10px;
}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "styles.css"), stylesContent);
        LogInfo("Generated styles.css");
    }

    private string GetCMSStatus()
    {
        try
        {
            var status = new StringBuilder();
            status.AppendLine("CMS Homepage Status:");
            status.AppendLine();

            if (_cmsRootPath == null || !Directory.Exists(_cmsRootPath))
            {
                status.AppendLine("❌ CMS not deployed. Run 'cms spawn' to create it.");
                return status.ToString();
            }

            status.AppendLine($"✅ CMS deployed at: {_cmsRootPath}");
            status.AppendLine();

            // Check files
            string[] requiredFiles = { "index.php", "admin.php", "config.php", "db.php", "styles.css", "cms_database.sqlite" };
            status.AppendLine("📁 Files:");
            foreach (var file in requiredFiles)
            {
                var filePath = Path.Combine(_cmsRootPath, file);
                var exists = File.Exists(filePath);
                var size = exists ? new FileInfo(filePath).Length : 0;
                status.AppendLine($"  {(exists ? "✅" : "❌")} {file} {(exists ? $"({size} bytes)" : "(missing)")}");
            }

            status.AppendLine();

            // Check PHP
            _phpPath = FindPHPExecutable();
            if (_phpPath != null)
            {
                status.AppendLine($"✅ PHP: {GetPHPVersion(_phpPath)}");
            }
            else
            {
                status.AppendLine("❌ PHP not detected");
            }

            status.AppendLine();
            status.AppendLine("🚀 To start CMS server:");
            status.AppendLine($"  cd {_cmsRootPath}");
            status.AppendLine("  php -S localhost:8080");

            return status.ToString();
        }
        catch (Exception ex)
        {
            LogError($"Status check error: {ex.Message}");
            return $"Error checking CMS status: {ex.Message}";
        }
    }

    #region Control Panel Generation Methods

    private void InitializeControlPanelDatabase(string dbPath)
    {
        var connectionString = $"Data Source={dbPath}";
        
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        // Create control panel schema
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS users (
    id TEXT PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    role TEXT NOT NULL,
    created_at TEXT NOT NULL,
    last_login TEXT
);

CREATE TABLE IF NOT EXISTS modules (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT UNIQUE NOT NULL,
    category TEXT,
    status TEXT NOT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS permissions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id TEXT NOT NULL,
    module_name TEXT NOT NULL,
    can_access INTEGER DEFAULT 0,
    can_configure INTEGER DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS server_health (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    cpu_usage REAL,
    memory_usage REAL,
    status TEXT
);

CREATE TABLE IF NOT EXISTS licenses (
    id TEXT PRIMARY KEY,
    license_key TEXT UNIQUE NOT NULL,
    instance_name TEXT,
    status TEXT NOT NULL,
    created_at TEXT NOT NULL,
    expires_at TEXT,
    max_users INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS audit_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    user_id TEXT,
    action TEXT NOT NULL,
    details TEXT,
    ip_address TEXT
);";
        cmd.ExecuteNonQuery();

        // Insert sample data
        cmd.CommandText = @"
INSERT OR IGNORE INTO users (id, username, role, created_at) 
VALUES ('admin-guid', 'admin', 'SuperAdmin', datetime('now'));

INSERT OR IGNORE INTO licenses (id, license_key, instance_name, status, created_at, max_users) 
VALUES ('default-license', 'RACORE-DEFAULT-' || substr(hex(randomblob(8)), 1, 16), 'RaCore Main Instance', 'active', datetime('now'), 10);";
        cmd.ExecuteNonQuery();

        LogInfo($"Control Panel database initialized: {dbPath}");
    }

    private void GenerateControlPanelConfigFile(string dbPath)
    {
        var configContent = $@"<?php
// RaCore SuperAdmin Control Panel Configuration
// Generated by RaCore CMSSpawner Module

// Database configuration
define('DB_PATH', __DIR__ . '/control_panel.sqlite');

// RaCore Authentication API
define('RACORE_AUTH_API', 'http://localhost:7077/api/auth');

// Security settings
define('SESSION_LIFETIME', 3600); // 1 hour
ini_set('session.cookie_httponly', 1);
ini_set('session.use_strict_mode', 1);

// Start session
if (session_status() === PHP_SESSION_NONE) {{
    session_start();
}}

// Timezone
date_default_timezone_set('UTC');

// Error reporting (disable in production)
error_reporting(E_ALL);
ini_set('display_errors', '1');

// Check SuperAdmin authentication
function checkSuperAdmin() {{
    if (!isset($_SESSION['token'])) {{
        return false;
    }}
    
    // Validate token with RaCore Auth API
    $ch = curl_init(RACORE_AUTH_API . '/validate');
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, [
        'Authorization: Bearer ' . $_SESSION['token'],
        'Content-Type: application/json'
    ]);
    
    $response = curl_exec($ch);
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    
    if ($httpCode !== 200) {{
        return false;
    }}
    
    $data = json_decode($response, true);
    return $data['valid'] && $data['user']['Role'] === 2; // SuperAdmin role = 2
}}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "config.php"), configContent);
        LogInfo("Generated control panel config.php");
    }

    private void GenerateControlPanelDatabaseFile()
    {
        var dbContent = @"<?php
// Database layer for RaCore SuperAdmin Control Panel
require_once 'config.php';

class Database {
    private static $instance = null;
    private $pdo;

    private function __construct() {
        try {
            $this->pdo = new PDO('sqlite:' . DB_PATH);
            $this->pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
            $this->pdo->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
        } catch (PDOException $e) {
            die('Database connection failed: ' . $e->getMessage());
        }
    }

    public static function getInstance() {
        if (self::$instance === null) {
            self::$instance = new Database();
        }
        return self::$instance;
    }

    public function getConnection() {
        return $this->pdo;
    }

    public function query($sql, $params = []) {
        try {
            $stmt = $this->pdo->prepare($sql);
            $stmt->execute($params);
            return $stmt;
        } catch (PDOException $e) {
            error_log('Database query error: ' . $e->getMessage());
            return false;
        }
    }

    public function getUsers() {
        $stmt = $this->query('SELECT * FROM users ORDER BY created_at DESC');
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function getModules() {
        $stmt = $this->query('SELECT * FROM modules ORDER BY name');
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function getLicenses() {
        $stmt = $this->query('SELECT * FROM licenses ORDER BY created_at DESC');
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function getServerHealth() {
        $stmt = $this->query('SELECT * FROM server_health ORDER BY timestamp DESC LIMIT 20');
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function getAuditLog($limit = 50) {
        $stmt = $this->query('SELECT * FROM audit_log ORDER BY timestamp DESC LIMIT ?', [$limit]);
        return $stmt ? $stmt->fetchAll() : [];
    }

    public function logAudit($userId, $action, $details, $ipAddress = '') {
        return $this->query(
            'INSERT INTO audit_log (timestamp, user_id, action, details, ip_address) VALUES (datetime(\'now\'), ?, ?, ?, ?)',
            [$userId, $action, $details, $ipAddress]
        );
    }
}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "db.php"), dbContent);
        LogInfo("Generated control panel db.php");
    }

    private void GenerateControlPanelIndexFile()
    {
        var indexContent = @"<?php
// RaCore SuperAdmin Control Panel
// Generated by RaCore CMSSpawner Module

require_once 'config.php';
require_once 'db.php';

$error = '';
$success = '';

// Handle login
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action'])) {
    if ($_POST['action'] === 'login') {
        $username = $_POST['username'] ?? '';
        $password = $_POST['password'] ?? '';
        
        // Authenticate with RaCore Auth API
        $ch = curl_init(RACORE_AUTH_API . '/login');
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HTTPHEADER, ['Content-Type: application/json']);
        curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode([
            'Username' => $username,
            'Password' => $password
        ]));
        
        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        curl_close($ch);
        
        if ($httpCode === 200) {
            $data = json_decode($response, true);
            if ($data['Success'] && $data['User']['Role'] === 2) { // SuperAdmin only
                $_SESSION['token'] = $data['Token'];
                $_SESSION['username'] = $data['User']['Username'];
                $_SESSION['user_id'] = $data['User']['Id'];
                $success = 'Login successful!';
            } else {
                $error = 'Access denied. SuperAdmin role required.';
            }
        } else {
            $error = 'Invalid credentials';
        }
    } elseif ($_POST['action'] === 'logout') {
        session_destroy();
        header('Location: index.php');
        exit;
    }
}

$isLoggedIn = checkSuperAdmin();

// Get data if logged in
$db = Database::getInstance();
$users = [];
$modules = [];
$licenses = [];
$auditLog = [];
$serverHealth = [];

if ($isLoggedIn) {
    $users = $db->getUsers();
    $licenses = $db->getLicenses();
    $auditLog = $db->getAuditLog(20);
}
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore SuperAdmin Control Panel</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1>🔐 RaCore SuperAdmin Control Panel</h1>
            <?php if ($isLoggedIn): ?>
                <nav>
                    <span>Welcome, <?php echo htmlspecialchars($_SESSION['username']); ?></span>
                    <form method=""POST"" style=""display: inline;"">
                        <input type=""hidden"" name=""action"" value=""logout"">
                        <button type=""submit"" class=""logout-btn"">Logout</button>
                    </form>
                </nav>
            <?php endif; ?>
        </div>
    </header>

    <main class=""container"">
        <?php if (!$isLoggedIn): ?>
            <div class=""login-form"">
                <h2>SuperAdmin Authentication</h2>
                <p class=""info"">Only SuperAdmin accounts can access this Control Panel</p>
                <?php if ($error): ?>
                    <div class=""error""><?php echo htmlspecialchars($error); ?></div>
                <?php endif; ?>
                <?php if ($success): ?>
                    <div class=""success""><?php echo htmlspecialchars($success); ?></div>
                <?php endif; ?>
                <form method=""POST"">
                    <input type=""hidden"" name=""action"" value=""login"">
                    <div class=""form-group"">
                        <label>Username:</label>
                        <input type=""text"" name=""username"" required autofocus>
                    </div>
                    <div class=""form-group"">
                        <label>Password:</label>
                        <input type=""password"" name=""password"" required>
                    </div>
                    <button type=""submit"">Login</button>
                    <p class=""hint"">Default: admin / admin123 (SuperAdmin)</p>
                </form>
            </div>
        <?php else: ?>
            <div class=""dashboard"">
                <div class=""section"">
                    <h2>📊 System Overview</h2>
                    <div class=""stats-grid"">
                        <div class=""stat-card"">
                            <div class=""stat-icon"">👥</div>
                            <div class=""stat-value""><?php echo count($users); ?></div>
                            <div class=""stat-label"">Total Users</div>
                        </div>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">🔑</div>
                            <div class=""stat-value""><?php echo count($licenses); ?></div>
                            <div class=""stat-label"">Active Licenses</div>
                        </div>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">🟢</div>
                            <div class=""stat-value"">Online</div>
                            <div class=""stat-label"">System Status</div>
                        </div>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">📝</div>
                            <div class=""stat-value""><?php echo count($auditLog); ?></div>
                            <div class=""stat-label"">Recent Events</div>
                        </div>
                    </div>
                </div>

                <div class=""section"">
                    <h2>🔐 License Management</h2>
                    <div class=""info-box"">
                        <p><strong>Subscription-Based Licensing:</strong> Control and manage licensed instances from this central mainframe.</p>
                        <?php if (!empty($licenses)): ?>
                            <table class=""data-table"">
                                <thead>
                                    <tr>
                                        <th>Instance</th>
                                        <th>License Key</th>
                                        <th>Status</th>
                                        <th>Max Users</th>
                                        <th>Created</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <?php foreach ($licenses as $license): ?>
                                        <tr>
                                            <td><?php echo htmlspecialchars($license['instance_name']); ?></td>
                                            <td><code><?php echo htmlspecialchars($license['license_key']); ?></code></td>
                                            <td><span class=""status-badge status-<?php echo $license['status']; ?>""><?php echo ucfirst($license['status']); ?></span></td>
                                            <td><?php echo $license['max_users']; ?></td>
                                            <td><?php echo htmlspecialchars($license['created_at']); ?></td>
                                        </tr>
                                    <?php endforeach; ?>
                                </tbody>
                            </table>
                        <?php else: ?>
                            <p>No licenses configured</p>
                        <?php endif; ?>
                    </div>
                </div>

                <div class=""section"">
                    <h2>👥 User Management</h2>
                    <div class=""info-box"">
                        <?php if (!empty($users)): ?>
                            <table class=""data-table"">
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th>Role</th>
                                        <th>Created</th>
                                        <th>Last Login</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <?php foreach ($users as $user): ?>
                                        <tr>
                                            <td><?php echo htmlspecialchars($user['username']); ?></td>
                                            <td><span class=""role-badge role-<?php echo strtolower($user['role']); ?>""><?php echo htmlspecialchars($user['role']); ?></span></td>
                                            <td><?php echo htmlspecialchars($user['created_at']); ?></td>
                                            <td><?php echo htmlspecialchars($user['last_login'] ?? 'Never'); ?></td>
                                        </tr>
                                    <?php endforeach; ?>
                                </tbody>
                            </table>
                        <?php else: ?>
                            <p>No users in local database. Users are managed via RaCore Auth API.</p>
                        <?php endif; ?>
                    </div>
                </div>

                <div class=""section"">
                    <h2>📝 Audit Log</h2>
                    <div class=""info-box"">
                        <?php if (!empty($auditLog)): ?>
                            <table class=""data-table"">
                                <thead>
                                    <tr>
                                        <th>Timestamp</th>
                                        <th>User</th>
                                        <th>Action</th>
                                        <th>Details</th>
                                        <th>IP Address</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <?php foreach ($auditLog as $log): ?>
                                        <tr>
                                            <td><?php echo htmlspecialchars($log['timestamp']); ?></td>
                                            <td><?php echo htmlspecialchars($log['user_id'] ?? 'System'); ?></td>
                                            <td><?php echo htmlspecialchars($log['action']); ?></td>
                                            <td><?php echo htmlspecialchars($log['details'] ?? '-'); ?></td>
                                            <td><?php echo htmlspecialchars($log['ip_address'] ?? '-'); ?></td>
                                        </tr>
                                    <?php endforeach; ?>
                                </tbody>
                            </table>
                        <?php else: ?>
                            <p>No audit events recorded</p>
                        <?php endif; ?>
                    </div>
                </div>

                <div class=""section"">
                    <h2>🔧 System Health & Diagnostics</h2>
                    <div class=""info-box"">
                        <div class=""health-grid"">
                            <div class=""health-item"">
                                <strong>RaCore Version:</strong> 1.0.0
                            </div>
                            <div class=""health-item"">
                                <strong>PHP Version:</strong> <?php echo PHP_VERSION; ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Database:</strong> SQLite
                            </div>
                            <div class=""health-item"">
                                <strong>Auth API:</strong> <?php echo RACORE_AUTH_API; ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Server Time:</strong> <?php echo date('Y-m-d H:i:s'); ?> UTC
                            </div>
                            <div class=""health-item"">
                                <strong>Multi-Tenant:</strong> Enabled
                            </div>
                        </div>
                    </div>
                </div>

                <div class=""section"">
                    <h2>🚀 Future Server Spawning</h2>
                    <div class=""info-box"">
                        <p><strong>Coming Soon:</strong> Spawn and manage additional subscription-based RaCore instances from this Control Panel.</p>
                        <ul>
                            <li>✅ Secure license key generation</li>
                            <li>✅ Multi-tenant architecture support</li>
                            <li>✅ Centralized user permission management</li>
                            <li>⏳ Automated instance deployment</li>
                            <li>⏳ Remote server monitoring</li>
                            <li>⏳ Update distribution from mainframe</li>
                        </ul>
                    </div>
                </div>
            </div>
        <?php endif; ?>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore AI Mainframe - SuperAdmin Control Panel</p>
            <p class=""security-notice"">⚠️ This panel is restricted to SuperAdmin accounts only</p>
        </div>
    </footer>
</body>
</html>
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "index.php"), indexContent);
        LogInfo("Generated control panel index.php");
    }

    private void GenerateControlPanelStylesFile()
    {
        var stylesContent = @"/* RaCore SuperAdmin Control Panel Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    line-height: 1.6;
    color: #333;
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    min-height: 100vh;
}

.container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 20px;
}

header {
    background: rgba(0, 0, 0, 0.3);
    backdrop-filter: blur(10px);
    padding: 20px 0;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
    color: white;
}

header h1 {
    margin-bottom: 10px;
}

nav {
    margin-top: 10px;
    display: flex;
    align-items: center;
    gap: 20px;
}

nav span {
    color: rgba(255, 255, 255, 0.9);
}

main {
    background: white;
    margin: 40px auto;
    padding: 40px;
    border-radius: 10px;
    box-shadow: 0 5px 20px rgba(0, 0, 0, 0.2);
}

.login-form {
    max-width: 450px;
    margin: 50px auto;
    padding: 40px;
    background: #f8f9fa;
    border-radius: 10px;
    box-shadow: 0 3px 15px rgba(0, 0, 0, 0.1);
}

.login-form h2 {
    color: #1e3c72;
    margin-bottom: 10px;
    text-align: center;
}

.info {
    text-align: center;
    color: #666;
    margin-bottom: 20px;
    font-size: 0.9em;
}

.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    margin-bottom: 5px;
    font-weight: 500;
    color: #333;
}

.form-group input,
.form-group textarea,
.form-group select {
    width: 100%;
    padding: 12px;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 16px;
}

button {
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    color: white;
    border: none;
    padding: 12px 30px;
    border-radius: 5px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
    transition: transform 0.2s;
}

button:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.logout-btn {
    background: #dc3545;
    padding: 8px 15px;
    font-size: 14px;
}

.error {
    background: #f8d7da;
    color: #721c24;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
    border-left: 4px solid #dc3545;
}

.success {
    background: #d4edda;
    color: #155724;
    padding: 15px;
    border-radius: 5px;
    margin-bottom: 20px;
    border-left: 4px solid #28a745;
}

.hint {
    color: #666;
    font-size: 0.9em;
    margin-top: 10px;
    text-align: center;
}

.dashboard {
    padding: 20px 0;
}

.section {
    margin-bottom: 40px;
}

.section h2 {
    color: #1e3c72;
    margin-bottom: 20px;
    padding-bottom: 10px;
    border-bottom: 2px solid #e9ecef;
}

.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
    margin-bottom: 30px;
}

.stat-card {
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    color: white;
    padding: 30px;
    border-radius: 10px;
    text-align: center;
    box-shadow: 0 3px 10px rgba(0, 0, 0, 0.1);
}

.stat-icon {
    font-size: 3em;
    margin-bottom: 10px;
}

.stat-value {
    font-size: 2.5em;
    font-weight: bold;
    margin-bottom: 5px;
}

.stat-label {
    font-size: 0.9em;
    opacity: 0.9;
}

.info-box {
    background: #f8f9fa;
    padding: 25px;
    border-radius: 10px;
    border-left: 4px solid #1e3c72;
}

.info-box p {
    margin-bottom: 15px;
}

.info-box ul {
    list-style-position: inside;
    margin-bottom: 15px;
}

.info-box li {
    margin-bottom: 8px;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
    background: white;
    border-radius: 8px;
    overflow: hidden;
}

.data-table th {
    background: #1e3c72;
    color: white;
    padding: 12px;
    text-align: left;
    font-weight: 600;
}

.data-table td {
    padding: 12px;
    border-bottom: 1px solid #e9ecef;
}

.data-table tr:last-child td {
    border-bottom: none;
}

.data-table tr:hover {
    background: #f8f9fa;
}

.data-table code {
    background: #e9ecef;
    padding: 2px 6px;
    border-radius: 3px;
    font-size: 0.85em;
}

.status-badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.85em;
    font-weight: 500;
}

.status-active {
    background: #d4edda;
    color: #155724;
}

.status-inactive {
    background: #f8d7da;
    color: #721c24;
}

.role-badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.85em;
    font-weight: 500;
}

.role-superadmin {
    background: #dc3545;
    color: white;
}

.role-admin {
    background: #ffc107;
    color: #333;
}

.role-user {
    background: #17a2b8;
    color: white;
}

.health-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 15px;
}

.health-item {
    padding: 15px;
    background: white;
    border-radius: 5px;
    border-left: 3px solid #1e3c72;
}

.health-item strong {
    display: block;
    margin-bottom: 5px;
    color: #1e3c72;
}

footer {
    background: rgba(0, 0, 0, 0.3);
    backdrop-filter: blur(10px);
    padding: 20px 0;
    text-align: center;
    color: rgba(255, 255, 255, 0.9);
}

.security-notice {
    margin-top: 10px;
    font-size: 0.9em;
    color: #ffc107;
}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "styles.css"), stylesContent);
        LogInfo("Generated control panel styles.css");
    }

    #endregion
}
