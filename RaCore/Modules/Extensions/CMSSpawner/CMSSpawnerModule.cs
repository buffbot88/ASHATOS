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
        _cmsRootPath = Path.Combine(AppContext.BaseDirectory, "cms_homepage");
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
            "  cms spawn         - Create PHP CMS homepage with SQLite database",
            "  cms spawn home    - Same as 'cms spawn'",
            "  cms status        - Show CMS deployment status",
            "  cms detect php    - Detect PHP runtime version",
            "  help              - Show this help message"
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
}
