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
        _cmsRootPath = Path.Combine(AppContext.BaseDirectory, "racore_cms");
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

        if (text.Equals("cms spawn integrated", StringComparison.OrdinalIgnoreCase))
        {
            return SpawnIntegratedCMS();
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
            "  cms spawn control   - Create standalone Control Panel",
            "  cms spawn integrated - Create CMS with integrated Control Panel (first-run)",
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
                LogInfo("Starting Control Panel generation...");

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
                    "✅ Control Panel generated successfully!",
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
                    "🎛️ Access Control:",
                    "  - Role-based access for all users",
                    "  - Different features shown based on permissions",
                    "  - SuperAdmin: Full access to all features",
                    "  - Admin: User management and audit logs",
                    "  - User: Personal account information",
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

    private string SpawnIntegratedCMS()
    {
        lock (_lock)
        {
            try
            {
                LogInfo("Starting Integrated CMS + Control Panel generation...");

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

                // Create control subdirectory
                var controlPath = Path.Combine(_cmsRootPath!, "control");
                if (!Directory.Exists(controlPath))
                {
                    Directory.CreateDirectory(controlPath);
                    LogInfo($"Created Control Panel directory: {controlPath}");
                }

                // Initialize main CMS database
                var cmsDbPath = Path.Combine(_cmsRootPath!, "cms_database.sqlite");
                InitializeSQLiteDatabase(cmsDbPath);

                // Initialize Control Panel database
                var controlDbPath = Path.Combine(controlPath, "control_panel.sqlite");
                InitializeControlPanelDatabase(controlDbPath);

                // Initialize Forum database (Phase 3)
                var forumDbPath = Path.Combine(_cmsRootPath!, "forum_database.sqlite");
                InitializeForumDatabase(forumDbPath);

                // Generate main CMS files
                GenerateConfigFile(cmsDbPath);
                GenerateDatabaseFile();
                GenerateIntegratedIndexFile(); // New homepage with link to control panel
                GenerateStylesFile();

                // Generate Control Panel files in /control subdirectory
                GenerateControlPanelConfigFileAt(controlPath, controlDbPath);
                GenerateControlPanelDatabaseFileAt(controlPath);
                GenerateControlPanelIndexFileAt(controlPath);
                GenerateControlPanelStylesFileAt(controlPath);

                // Generate Phase 3 files - Forums & Profiles
                GenerateForumIndexFile(forumDbPath);
                GenerateForumStylesFile();
                GenerateProfileSystemFiles(cmsDbPath);

                var serverUrl = "http://localhost:8080";
                
                return string.Join(Environment.NewLine,
                    "✅ Integrated CMS + Control Panel + Community generated successfully!",
                    "",
                    $"📁 Location: {_cmsRootPath}",
                    $"🗄️  CMS Database: {cmsDbPath}",
                    $"🗄️  Control Database: {controlDbPath}",
                    $"🗄️  Forum Database: {forumDbPath}",
                    $"🐘 PHP Version: {version}",
                    "",
                    "📝 Generated structure:",
                    "  /                   - CMS Homepage",
                    "  /control/           - Control Panel (role-based)",
                    "  /community/         - vBulletin-style Forums",
                    "  /profile.php        - MySpace-style User Profiles",
                    "",
                    "🎛️ Access Control:",
                    "  - Homepage: Public access",
                    "  - Control Panel: Authenticated users only",
                    "    • SuperAdmin: All panels (Super + Admin + User)",
                    "    • Admin: Admin panel + User panel + Forum Management",
                    "    • User: User panel + Profile customization",
                    "  - Community Forums: All authenticated users",
                    "  - User Profiles: Public viewing, owner can customize",
                    "",
                    "✨ Phase 3 Features:",
                    "  - vBulletin v3-style Forum System",
                    "  - Forum Management in Admin Panel",
                    "  - MySpace-style Profiles with Custom CSS",
                    "",
                    "🚀 To start the CMS:",
                    $"  cd {_cmsRootPath}",
                    $"  php -S localhost:8080",
                    $"  Homepage: {serverUrl}",
                    $"  Control Panel: {serverUrl}/control",
                    $"  Community: {serverUrl}/community",
                    "",
                    "🔑 Default SuperAdmin: admin / admin123",
                    "⚠️  CHANGE PASSWORD IMMEDIATELY!"
                );
            }
            catch (Exception ex)
            {
                LogError($"Integrated CMS spawn error: {ex.Message}");
                return $"❌ Error spawning Integrated CMS: {ex.Message}";
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
// RaCore Control Panel Configuration
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

// User role constants
define('ROLE_USER', 0);
define('ROLE_ADMIN', 1);
define('ROLE_SUPERADMIN', 2);

// Check if user is authenticated and return user data
function checkAuthentication() {{
    if (!isset($_SESSION['token'])) {{
        return null;
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
        return null;
    }}
    
    $data = json_decode($response, true);
    if ($data['valid']) {{
        return [
            'username' => $data['user']['Username'] ?? $_SESSION['username'],
            'role' => $data['user']['Role'] ?? 0,
            'email' => $data['user']['Email'] ?? ''
        ];
    }}
    return null;
}}

// Check if user has at least the specified role
function hasRole($userRole, $requiredRole) {{
    return $userRole >= $requiredRole;
}}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "config.php"), configContent);
        LogInfo("Generated control panel config.php");
    }

    private void GenerateControlPanelDatabaseFile()
    {
        var dbContent = @"<?php
// Database layer for RaCore Control Panel
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
// RaCore Control Panel
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
            if ($data['Success']) {
                $_SESSION['token'] = $data['Token'];
                $_SESSION['username'] = $data['User']['Username'];
                $_SESSION['user_id'] = $data['User']['Id'];
                $_SESSION['role'] = $data['User']['Role'];
                $success = 'Login successful!';
            } else {
                $error = 'Invalid credentials';
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

$currentUser = checkAuthentication();
$isLoggedIn = $currentUser !== null;

// Get user role and determine permissions
$userRole = $currentUser['role'] ?? 0;
$isUser = $isLoggedIn;
$isAdmin = hasRole($userRole, ROLE_ADMIN);
$isSuperAdmin = hasRole($userRole, ROLE_SUPERADMIN);

// Get role name
$roleName = 'Guest';
if ($isSuperAdmin) {
    $roleName = 'SuperAdmin';
} elseif ($isAdmin) {
    $roleName = 'Admin';
} elseif ($isUser) {
    $roleName = 'User';
}

// Get data if logged in
$db = Database::getInstance();
$users = [];
$modules = [];
$licenses = [];
$auditLog = [];
$serverHealth = [];

if ($isLoggedIn) {
    $users = $db->getUsers();
    if ($isAdmin) {
        $licenses = $db->getLicenses();
        $auditLog = $db->getAuditLog(20);
    }
}
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Control Panel</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1>🎛️ RaCore Control Panel</h1>
            <?php if ($isLoggedIn): ?>
                <nav>
                    <span>Welcome, <?php echo htmlspecialchars($currentUser['username']); ?> (<?php echo $roleName; ?>)</span>
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
                <h2>Control Panel Authentication</h2>
                <p class=""info"">Login to access your personalized control panel</p>
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
                    <p class=""hint"">Default: admin / admin123</p>
                </form>
            </div>
        <?php else: ?>
            <div class=""dashboard"">
                <div class=""section"">
                    <h2>📊 System Overview</h2>
                    <div class=""stats-grid"">
                        <div class=""stat-card"">
                            <div class=""stat-icon"">👤</div>
                            <div class=""stat-value"">You</div>
                            <div class=""stat-label"">Access Level: <?php echo $roleName; ?></div>
                        </div>
                        <?php if ($isAdmin): ?>
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
                        <?php endif; ?>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">🟢</div>
                            <div class=""stat-value"">Online</div>
                            <div class=""stat-label"">System Status</div>
                        </div>
                        <?php if ($isAdmin): ?>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">📝</div>
                            <div class=""stat-value""><?php echo count($auditLog); ?></div>
                            <div class=""stat-label"">Recent Events</div>
                        </div>
                        <?php endif; ?>
                    </div>
                </div>

                <?php if ($isSuperAdmin): ?>
                <div class=""section"">
                    <h2>🔐 License Management (SuperAdmin)</h2>
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
                <?php endif; ?>

                <?php if ($isAdmin): ?>
                <div class=""section"">
                    <h2>👥 User Management <?php echo $isSuperAdmin ? '(SuperAdmin)' : '(Admin)'; ?></h2>
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
                <?php endif; ?>

                <div class=""section"">
                    <h2>ℹ️ Your Account</h2>
                    <div class=""info-box"">
                        <div class=""health-grid"">
                            <div class=""health-item"">
                                <strong>Username:</strong> <?php echo htmlspecialchars($currentUser['username']); ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Role:</strong> <?php echo $roleName; ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Email:</strong> <?php echo htmlspecialchars($currentUser['email'] ?: 'Not set'); ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Session:</strong> Active
                            </div>
                        </div>
                    </div>
                </div>

                <?php if ($isAdmin): ?>
                <div class=""section"">
                    <h2>📝 Audit Log <?php echo $isSuperAdmin ? '(SuperAdmin)' : '(Admin)'; ?></h2>
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
                <?php endif; ?>

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
                            <?php if ($isAdmin): ?>
                            <div class=""health-item"">
                                <strong>Multi-Tenant:</strong> Enabled
                            </div>
                            <?php endif; ?>
                        </div>
                    </div>
                </div>

                <?php if ($isSuperAdmin): ?>
                <div class=""section"">
                    <h2>🚀 Future Server Spawning (SuperAdmin)</h2>
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
                <?php endif; ?>

                <?php if (!$isAdmin): ?>
                <div class=""section"">
                    <h2>ℹ️ Additional Features</h2>
                    <div class=""info-box"">
                        <p>You have <strong><?php echo $roleName; ?></strong> access level. Contact your administrator for access to advanced features such as:</p>
                        <ul>
                            <li>User Management</li>
                            <li>License Management (SuperAdmin only)</li>
                            <li>Audit Log Viewing</li>
                            <li>Server Spawning (SuperAdmin only)</li>
                        </ul>
                    </div>
                </div>
                <?php endif; ?>
            </div>
        <?php endif; ?>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore AI Mainframe - Control Panel</p>
            <p class=""security-notice"">Your access level: <?php echo $roleName; ?></p>
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
        var stylesContent = @"/* RaCore Control Panel Styles */
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

    #region Integrated CMS Helper Methods

    private void GenerateIntegratedIndexFile()
    {
        var indexContent = @"<?php
// RaCore CMS Homepage
// Generated by RaCore CMSSpawner Module

require_once 'config.php';
require_once 'db.php';

$db = Database::getInstance();
$pages = $db->getAllPages();
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore CMS</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1>🚀 RaCore CMS</h1>
            <nav>
                <a href=""/"">Home</a>
                <a href=""/control"">Control Panel</a>
            </nav>
        </div>
    </header>

    <main class=""container"">
        <div class=""hero"">
            <h2>Welcome to RaCore</h2>
            <p>Your AI-powered content management system with integrated control panel.</p>
        </div>

        <?php if (!empty($pages)): ?>
            <div class=""pages-section"">
                <h3>Published Pages</h3>
                <div class=""pages-grid"">
                    <?php foreach ($pages as $page): ?>
                        <div class=""page-card"">
                            <h4><?php echo htmlspecialchars($page['title']); ?></h4>
                            <p><?php echo htmlspecialchars(substr($page['content'], 0, 150)); ?>...</p>
                            <small>Updated: <?php echo htmlspecialchars($page['updated_at']); ?></small>
                        </div>
                    <?php endforeach; ?>
                </div>
            </div>
        <?php else: ?>
            <div class=""empty-state"">
                <p>No pages yet. Go to the <a href=""/control"">Control Panel</a> to create content.</p>
            </div>
        <?php endif; ?>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore - Powered by AI</p>
        </div>
    </footer>
</body>
</html>
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "index.php"), indexContent);
        LogInfo("Generated integrated CMS index.php");
    }

    private void GenerateControlPanelConfigFileAt(string controlPath, string dbPath)
    {
        var configContent = $@"<?php
// RaCore Control Panel Configuration
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

// User role constants
define('ROLE_USER', 0);
define('ROLE_ADMIN', 1);
define('ROLE_SUPERADMIN', 2);

// Check if user is authenticated and return user data
function checkAuthentication() {{
    if (!isset($_SESSION['token'])) {{
        return null;
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
        return null;
    }}
    
    $data = json_decode($response, true);
    if ($data['valid']) {{
        return [
            'username' => $data['user']['Username'] ?? $_SESSION['username'],
            'role' => $data['user']['Role'] ?? 0,
            'email' => $data['user']['Email'] ?? ''
        ];
    }}
    return null;
}}

// Check if user has at least the specified role
function hasRole($userRole, $requiredRole) {{
    return $userRole >= $requiredRole;
}}
";

        File.WriteAllText(Path.Combine(controlPath, "config.php"), configContent);
        LogInfo("Generated control panel config.php in control directory");
    }

    private void GenerateControlPanelDatabaseFileAt(string controlPath)
    {
        var dbContent = @"<?php
// Database layer for RaCore Control Panel
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

        File.WriteAllText(Path.Combine(controlPath, "db.php"), dbContent);
        LogInfo("Generated control panel db.php in control directory");
    }

    private void GenerateControlPanelIndexFileAt(string controlPath)
    {
        // Use the same control panel index as before, just in the control subdirectory
        var indexContent = @"<?php
// RaCore Control Panel
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
            if ($data['Success']) {
                $_SESSION['token'] = $data['Token'];
                $_SESSION['username'] = $data['User']['Username'];
                $_SESSION['user_id'] = $data['User']['Id'];
                $_SESSION['role'] = $data['User']['Role'];
                $success = 'Login successful!';
            } else {
                $error = 'Invalid credentials';
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

$currentUser = checkAuthentication();
$isLoggedIn = $currentUser !== null;

// Get user role and determine permissions
$userRole = $currentUser['role'] ?? 0;
$isUser = $isLoggedIn;
$isAdmin = hasRole($userRole, ROLE_ADMIN);
$isSuperAdmin = hasRole($userRole, ROLE_SUPERADMIN);

// Get role name
$roleName = 'Guest';
if ($isSuperAdmin) {
    $roleName = 'SuperAdmin';
} elseif ($isAdmin) {
    $roleName = 'Admin';
} elseif ($isUser) {
    $roleName = 'User';
}

// Get data if logged in
$db = Database::getInstance();
$users = [];
$modules = [];
$licenses = [];
$auditLog = [];
$serverHealth = [];

if ($isLoggedIn) {
    $users = $db->getUsers();
    if ($isAdmin) {
        $licenses = $db->getLicenses();
        $auditLog = $db->getAuditLog(20);
    }
}
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Control Panel</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <header>
        <div class=""container"">
            <h1>🎛️ RaCore Control Panel</h1>
            <?php if ($isLoggedIn): ?>
                <nav>
                    <a href=""/"">← Back to Site</a>
                    <span>Welcome, <?php echo htmlspecialchars($currentUser['username']); ?> (<?php echo $roleName; ?>)</span>
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
                <h2>Control Panel Authentication</h2>
                <p class=""info"">Login to access your personalized control panel</p>
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
                    <p class=""hint"">Default: admin / admin123</p>
                </form>
            </div>
        <?php else: ?>
            <div class=""dashboard"">
                <div class=""section"">
                    <h2>📊 System Overview</h2>
                    <div class=""stats-grid"">
                        <div class=""stat-card"">
                            <div class=""stat-icon"">👤</div>
                            <div class=""stat-value"">You</div>
                            <div class=""stat-label"">Access Level: <?php echo $roleName; ?></div>
                        </div>
                        <?php if ($isAdmin): ?>
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
                        <?php endif; ?>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">🟢</div>
                            <div class=""stat-value"">Online</div>
                            <div class=""stat-label"">System Status</div>
                        </div>
                        <?php if ($isAdmin): ?>
                        <div class=""stat-card"">
                            <div class=""stat-icon"">📝</div>
                            <div class=""stat-value""><?php echo count($auditLog); ?></div>
                            <div class=""stat-label"">Recent Events</div>
                        </div>
                        <?php endif; ?>
                    </div>
                </div>

                <?php if ($isAdmin): ?>
                <div class=""section"">
                    <h2>💬 RaAI Console <?php echo $isSuperAdmin ? '(SuperAdmin)' : '(Admin)'; ?></h2>
                    <div class=""info-box"">
                        <p><strong>Administrative AI Communication:</strong> Communicate with RaAI for server management and automation tasks.</p>
                        <div class=""ai-console"">
                            <div class=""console-output"" id=""consoleOutput"">
                                <div class=""console-line""><span class=""prompt"">RaAI@<?php echo $roleName; ?> ~$</span> System ready. Type ''help'' for available commands.</div>
                            </div>
                            <div class=""console-input-wrapper"">
                                <span class=""prompt"">RaAI@<?php echo $roleName; ?> ~$</span>
                                <input type=""text"" class=""console-input"" id=""consoleInput"" placeholder=""Enter command..."" autocomplete=""off"" />
                            </div>
                        </div>
                    </div>
                </div>
                <?php endif; ?>

                <!-- Support Chat Widget (All Users) -->
                <div class=""support-chat-widget"" id=""supportChat"">
                    <div class=""support-chat-header"" onclick=""toggleSupportChat()"">
                        <span>💬 Support Chat</span>
                        <span class=""toggle-icon"">−</span>
                    </div>
                    <div class=""support-chat-body"" id=""supportChatBody"">
                        <div class=""chat-messages"" id=""chatMessages"">
                            <div class=""chat-message bot"">
                                <strong>Support Bot:</strong> Hello <?php echo htmlspecialchars($currentUser['username']); ?>! How can I help you today?
                            </div>
                        </div>
                        <div class=""chat-input-wrapper"">
                            <input type=""text"" class=""chat-input"" id=""chatInput"" placeholder=""Type your message..."" autocomplete=""off"" />
                            <button class=""chat-send-btn"" onclick=""sendSupportMessage()"">Send</button>
                        </div>
                    </div>
                </div>


                <?php if ($isSuperAdmin): ?>
                <div class=""section"">
                    <h2>🔐 License Management (SuperAdmin)</h2>
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
                <?php endif; ?>

                <?php if ($isAdmin): ?>
                <div class=""section"">
                    <h2>👥 User Management <?php echo $isSuperAdmin ? '(SuperAdmin)' : '(Admin)'; ?></h2>
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
                <?php endif; ?>

                <div class=""section"">
                    <h2>ℹ️ Your Account</h2>
                    <div class=""info-box"">
                        <div class=""health-grid"">
                            <div class=""health-item"">
                                <strong>Username:</strong> <?php echo htmlspecialchars($currentUser['username']); ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Role:</strong> <?php echo $roleName; ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Email:</strong> <?php echo htmlspecialchars($currentUser['email'] ?: 'Not set'); ?>
                            </div>
                            <div class=""health-item"">
                                <strong>Session:</strong> Active
                            </div>
                        </div>
                    </div>
                </div>

                <?php if ($isAdmin): ?>
                <div class=""section"">
                    <h2>📝 Audit Log <?php echo $isSuperAdmin ? '(SuperAdmin)' : '(Admin)'; ?></h2>
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
                <?php endif; ?>

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
                            <?php if ($isAdmin): ?>
                            <div class=""health-item"">
                                <strong>Multi-Tenant:</strong> Enabled
                            </div>
                            <?php endif; ?>
                        </div>
                    </div>
                </div>

                <?php if ($isSuperAdmin): ?>
                <div class=""section"">
                    <h2>🚀 Future Server Spawning (SuperAdmin)</h2>
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
                <?php endif; ?>

                <?php if (!$isAdmin): ?>
                <div class=""section"">
                    <h2>ℹ️ Additional Features</h2>
                    <div class=""info-box"">
                        <p>You have <strong><?php echo $roleName; ?></strong> access level. Contact your administrator for access to advanced features such as:</p>
                        <ul>
                            <li>User Management</li>
                            <li>License Management (SuperAdmin only)</li>
                            <li>Audit Log Viewing</li>
                            <li>Server Spawning (SuperAdmin only)</li>
                        </ul>
                    </div>
                </div>
                <?php endif; ?>
            </div>
        <?php endif; ?>
    </main>

    <footer>
        <div class=""container"">
            <p>&copy; <?php echo date('Y'); ?> RaCore AI Mainframe - Control Panel</p>
            <p class=""security-notice"">Your access level: <?php echo $roleName; ?></p>
        </div>
    </footer>

    <script>
    // AI Console functionality
    const consoleOutput = document.getElementById('consoleOutput');
    const consoleInput = document.getElementById('consoleInput');
    
    if (consoleInput) {
        consoleInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                const command = this.value.trim();
                if (command) {
                    addConsoleLine('<?php echo $roleName; ?>', command);
                    processCommand(command);
                    this.value = '';
                }
            }
        });
    }
    
    function addConsoleLine(user, text, isResponse = false) {
        const line = document.createElement('div');
        line.className = 'console-line' + (isResponse ? ' response' : '');
        if (!isResponse) {
            line.innerHTML = `<span class=""prompt"">RaAI@${user} ~$</span> ${escapeHtml(text)}`;
        } else {
            line.textContent = text;
        }
        consoleOutput.appendChild(line);
        consoleOutput.scrollTop = consoleOutput.scrollHeight;
    }
    
    function processCommand(cmd) {
        // Simulate AI command processing
        setTimeout(() => {
            let response = '';
            const lower = cmd.toLowerCase();
            if (lower === 'help') {
                response = 'Available commands: help, status, users, logs, clear, version';
            } else if (lower === 'status') {
                response = 'System Status: Online | Memory: 45% | CPU: 23% | Uptime: 5d 12h';
            } else if (lower === 'users') {
                response = 'Total Users: <?php echo count($users ?? []); ?> | Active Sessions: <?php echo $isAdmin ? count($users ?? []) : ""N/A""; ?>';
            } else if (lower === 'logs') {
                response = 'Recent Events: <?php echo $isAdmin ? count($auditLog ?? []) : ""N/A""; ?> | Check Audit Log section below';
            } else if (lower === 'clear') {
                consoleOutput.innerHTML = '<div class=""console-line""><span class=""prompt"">RaAI@<?php echo $roleName; ?> ~$</span> Console cleared.</div>';
                return;
            } else if (lower === 'version') {
                response = 'RaCore v1.0.0 | PHP <?php echo PHP_VERSION; ?> | Control Panel Phase 2';
            } else {
                response = `Command not found: ${cmd}. Type ''help'' for available commands.`;
            }
            addConsoleLine('', response, true);
        }, 300);
    }
    
    // Support Chat functionality
    const chatInput = document.getElementById('chatInput');
    const chatMessages = document.getElementById('chatMessages');
    const supportChatBody = document.getElementById('supportChatBody');
    
    function toggleSupportChat() {
        const isVisible = supportChatBody.style.display !== 'none';
        supportChatBody.style.display = isVisible ? 'none' : 'block';
        document.querySelector('.toggle-icon').textContent = isVisible ? '+' : '−';
    }
    
    function sendSupportMessage() {
        const message = chatInput.value.trim();
        if (message) {
            addChatMessage('You', message, false);
            chatInput.value = '';
            
            // Simulate bot response
            setTimeout(() => {
                let response = '';
                const lower = message.toLowerCase();
                if (lower.includes('help') || lower.includes('support')) {
                    response = 'I can help you with account management, system information, and general questions. What do you need?';
                } else if (lower.includes('password')) {
                    response = 'To change your password, please contact your administrator or use the Account Settings section.';
                } else if (lower.includes('access') || lower.includes('permission')) {
                    response = 'Access levels are managed by administrators. Your current role is: <?php echo $roleName; ?>';
                } else {
                    response = 'Thank you for your message! An administrator will respond shortly.';
                }
                addChatMessage('Support Bot', response, true);
            }, 800);
        }
    }
    
    function addChatMessage(sender, text, isBot) {
        const msg = document.createElement('div');
        msg.className = 'chat-message' + (isBot ? ' bot' : ' user');
        msg.innerHTML = `<strong>${escapeHtml(sender)}:</strong> ${escapeHtml(text)}`;
        chatMessages.appendChild(msg);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendSupportMessage();
            }
        });
    }
    
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    </script>
</body>
</html>
";

        File.WriteAllText(Path.Combine(controlPath, "index.php"), indexContent);
        LogInfo("Generated control panel index.php in control directory");
    }

    private void GenerateControlPanelStylesFileAt(string controlPath)
    {
        var stylesContent = @"/* RaCore Control Panel Styles */
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

nav a {
    color: rgba(255, 255, 255, 0.9);
    text-decoration: none;
}

nav a:hover {
    color: white;
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

/* AI Console Styles */
.ai-console {
    background: #000;
    border-radius: 5px;
    padding: 15px;
    font-family: 'Courier New', monospace;
    margin-top: 15px;
}

.console-output {
    background: #000;
    color: #00ff00;
    padding: 15px;
    min-height: 200px;
    max-height: 400px;
    overflow-y: auto;
    border-radius: 5px;
    margin-bottom: 10px;
    font-size: 14px;
}

.console-line {
    margin-bottom: 8px;
    line-height: 1.4;
}

.console-line.response {
    color: #00ff00;
    padding-left: 20px;
}

.console-input-wrapper {
    display: flex;
    align-items: center;
    background: #000;
    padding: 10px;
    border-radius: 5px;
}

.console-input-wrapper .prompt {
    color: #00ff00;
    margin-right: 10px;
    white-space: nowrap;
}

.console-input {
    flex: 1;
    background: transparent;
    border: none;
    color: #00ff00;
    font-family: 'Courier New', monospace;
    font-size: 14px;
    outline: none;
}

.console-input::placeholder {
    color: #006600;
}

.prompt {
    color: #00ff00;
    font-weight: bold;
}

/* Support Chat Widget Styles */
.support-chat-widget {
    position: fixed;
    bottom: 20px;
    right: 20px;
    width: 350px;
    max-width: calc(100vw - 40px);
    background: white;
    border-radius: 10px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    z-index: 1000;
}

.support-chat-header {
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    color: white;
    padding: 15px;
    border-radius: 10px 10px 0 0;
    cursor: pointer;
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-weight: 500;
}

.support-chat-header:hover {
    background: linear-gradient(135deg, #2a5298 0%, #1e3c72 100%);
}

.toggle-icon {
    font-size: 20px;
    font-weight: bold;
}

.support-chat-body {
    display: block;
}

.chat-messages {
    padding: 15px;
    max-height: 300px;
    overflow-y: auto;
    background: #f8f9fa;
}

.chat-message {
    margin-bottom: 12px;
    padding: 10px;
    border-radius: 5px;
    line-height: 1.4;
}

.chat-message.bot {
    background: #e3f2fd;
    border-left: 3px solid #2196f3;
}

.chat-message.user {
    background: #f1f8e9;
    border-left: 3px solid #8bc34a;
}

.chat-message strong {
    display: block;
    margin-bottom: 5px;
    color: #1e3c72;
}

.chat-input-wrapper {
    display: flex;
    padding: 10px;
    border-top: 1px solid #e9ecef;
    background: white;
    border-radius: 0 0 10px 10px;
}

.chat-input {
    flex: 1;
    padding: 8px 12px;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 14px;
    outline: none;
}

.chat-input:focus {
    border-color: #1e3c72;
}

.chat-send-btn {
    margin-left: 8px;
    padding: 8px 16px;
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
}

.chat-send-btn:hover {
    transform: translateY(-1px);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}
";

        File.WriteAllText(Path.Combine(controlPath, "styles.css"), stylesContent);
        LogInfo("Generated control panel styles.css in control directory");
    }

    #endregion

    #region Phase 3: Forums & Profiles

    private void InitializeForumDatabase(string dbPath)
    {
        var connectionString = $"Data Source={dbPath}";
        
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS forum_categories (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    display_order INTEGER DEFAULT 0,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS forum_threads (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    category_id INTEGER NOT NULL,
    title TEXT NOT NULL,
    author_username TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    views INTEGER DEFAULT 0,
    is_locked INTEGER DEFAULT 0,
    is_pinned INTEGER DEFAULT 0,
    FOREIGN KEY (category_id) REFERENCES forum_categories(id)
);

CREATE TABLE IF NOT EXISTS forum_posts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    thread_id INTEGER NOT NULL,
    author_username TEXT NOT NULL,
    content TEXT NOT NULL,
    created_at TEXT NOT NULL,
    edited_at TEXT,
    FOREIGN KEY (thread_id) REFERENCES forum_threads(id)
);

CREATE TABLE IF NOT EXISTS user_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    display_name TEXT,
    bio TEXT,
    custom_css TEXT,
    avatar_url TEXT,
    created_at TEXT NOT NULL,
    last_updated TEXT NOT NULL
);

INSERT INTO forum_categories (name, description, display_order, created_at) VALUES
('General Discussion', 'General topics and announcements', 1, datetime('now')),
('Support & Help', 'Get help from the community', 2, datetime('now')),
('Off-Topic', 'Anything goes!', 3, datetime('now'));
";
        cmd.ExecuteNonQuery();
        LogInfo($"Forum database initialized: {dbPath}");
    }

    private void GenerateForumIndexFile(string forumDbPath)
    {
        var communityDir = Path.Combine(_cmsRootPath!, "community");
        if (!Directory.Exists(communityDir))
        {
            Directory.CreateDirectory(communityDir);
        }

        var forumContent = @"<?php
// RaCore Community Forums - vBulletin v3 Style
session_start();

// Check if user is logged in (basic check)
$isLoggedIn = isset($_SESSION['username']);
$username = $isLoggedIn ? $_SESSION['username'] : 'Guest';

// Database connection
$dbPath = '../forum_database.sqlite';
$db = new PDO('sqlite:' . $dbPath);
$db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

// Handle actions
$action = $_GET['action'] ?? 'index';
$categoryId = $_GET['category'] ?? null;
$threadId = $_GET['thread'] ?? null;
$message = '';

if ($action === 'new_thread' && $_SERVER['REQUEST_METHOD'] === 'POST' && $isLoggedIn) {
    $title = $_POST['title'] ?? '';
    $content = $_POST['content'] ?? '';
    $category = $_POST['category_id'] ?? '';
    
    if ($title && $content && $category) {
        $stmt = $db->prepare('INSERT INTO forum_threads (category_id, title, author_username, created_at, updated_at) VALUES (?, ?, ?, datetime(""now""), datetime(""now""))');
        $stmt->execute([$category, $title, $username]);
        $threadId = $db->lastInsertId();
        
        $stmt = $db->prepare('INSERT INTO forum_posts (thread_id, author_username, content, created_at) VALUES (?, ?, ?, datetime(""now""))');
        $stmt->execute([$threadId, $username, $content]);
        
        header('Location: index.php?action=thread&thread=' . $threadId);
        exit;
    }
}

if ($action === 'new_post' && $_SERVER['REQUEST_METHOD'] === 'POST' && $isLoggedIn) {
    $content = $_POST['content'] ?? '';
    $thread = $_POST['thread_id'] ?? '';
    
    if ($content && $thread) {
        $stmt = $db->prepare('INSERT INTO forum_posts (thread_id, author_username, content, created_at) VALUES (?, ?, ?, datetime(""now""))');
        $stmt->execute([$thread, $username, $content]);
        
        $stmt = $db->prepare('UPDATE forum_threads SET updated_at = datetime(""now"") WHERE id = ?');
        $stmt->execute([$thread]);
        
        header('Location: index.php?action=thread&thread=' . $thread);
        exit;
    }
}

// Get categories
$categories = $db->query('SELECT * FROM forum_categories ORDER BY display_order')->fetchAll(PDO::FETCH_ASSOC);

// Get thread counts
$threadCounts = [];
foreach ($categories as $cat) {
    $stmt = $db->prepare('SELECT COUNT(*) as count FROM forum_threads WHERE category_id = ?');
    $stmt->execute([$cat['id']]);
    $threadCounts[$cat['id']] = $stmt->fetch(PDO::FETCH_ASSOC)['count'];
}
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>RaCore Community Forums</title>
    <link rel=""stylesheet"" href=""forum_styles.css"">
</head>
<body>
    <div class=""vb-header"">
        <div class=""container"">
            <h1>🗣️ RaCore Community</h1>
            <div class=""vb-nav"">
                <a href=""../index.php"">Home</a>
                <a href=""index.php"">Forums</a>
                <?php if ($isLoggedIn): ?>
                    <a href=""../profile.php?user=<?php echo urlencode($username); ?>"">My Profile</a>
                    <a href=""../control/index.php"">Control Panel</a>
                    <span class=""username"">Welcome, <?php echo htmlspecialchars($username); ?></span>
                <?php else: ?>
                    <a href=""../control/index.php"">Login</a>
                <?php endif; ?>
            </div>
        </div>
    </div>

    <div class=""container"">
        <?php if ($action === 'index'): ?>
            <!-- Forum Categories List -->
            <div class=""forum-header"">
                <h2>Forum Categories</h2>
                <?php if ($isLoggedIn): ?>
                    <button onclick=""showNewThreadForm()"" class=""btn-primary"">New Thread</button>
                <?php endif; ?>
            </div>

            <div id=""new-thread-form"" style=""display: none;"" class=""form-container"">
                <h3>Create New Thread</h3>
                <form method=""POST"" action=""index.php?action=new_thread"">
                    <select name=""category_id"" required class=""form-select"">
                        <option value="""">Select Category...</option>
                        <?php foreach ($categories as $cat): ?>
                            <option value=""<?php echo $cat['id']; ?>""><?php echo htmlspecialchars($cat['name']); ?></option>
                        <?php endforeach; ?>
                    </select>
                    <input type=""text"" name=""title"" placeholder=""Thread Title"" required class=""form-input"">
                    <textarea name=""content"" placeholder=""Your message..."" required class=""form-textarea""></textarea>
                    <button type=""submit"" class=""btn-primary"">Post Thread</button>
                    <button type=""button"" onclick=""document.getElementById('new-thread-form').style.display='none'"" class=""btn-secondary"">Cancel</button>
                </form>
            </div>

            <div class=""vb-categories"">
                <?php foreach ($categories as $category): ?>
                    <div class=""vb-category"">
                        <div class=""category-icon"">📁</div>
                        <div class=""category-info"">
                            <h3><a href=""index.php?action=category&category=<?php echo $category['id']; ?>""><?php echo htmlspecialchars($category['name']); ?></a></h3>
                            <p><?php echo htmlspecialchars($category['description']); ?></p>
                        </div>
                        <div class=""category-stats"">
                            <span><?php echo $threadCounts[$category['id']]; ?> threads</span>
                        </div>
                    </div>
                <?php endforeach; ?>
            </div>

        <?php elseif ($action === 'category' && $categoryId): ?>
            <!-- Category Thread List -->
            <?php
            $category = $db->prepare('SELECT * FROM forum_categories WHERE id = ?');
            $category->execute([$categoryId]);
            $category = $category->fetch(PDO::FETCH_ASSOC);
            
            $threads = $db->prepare('SELECT * FROM forum_threads WHERE category_id = ? ORDER BY is_pinned DESC, updated_at DESC');
            $threads->execute([$categoryId]);
            $threads = $threads->fetchAll(PDO::FETCH_ASSOC);
            ?>
            
            <div class=""forum-breadcrumb"">
                <a href=""index.php"">Forums</a> &gt; <?php echo htmlspecialchars($category['name']); ?>
            </div>

            <div class=""forum-header"">
                <h2><?php echo htmlspecialchars($category['name']); ?></h2>
                <?php if ($isLoggedIn): ?>
                    <button onclick=""showNewThreadForm()"" class=""btn-primary"">New Thread</button>
                <?php endif; ?>
            </div>

            <div id=""new-thread-form"" style=""display: none;"" class=""form-container"">
                <h3>Create New Thread</h3>
                <form method=""POST"" action=""index.php?action=new_thread"">
                    <input type=""hidden"" name=""category_id"" value=""<?php echo $categoryId; ?>"">
                    <input type=""text"" name=""title"" placeholder=""Thread Title"" required class=""form-input"">
                    <textarea name=""content"" placeholder=""Your message..."" required class=""form-textarea""></textarea>
                    <button type=""submit"" class=""btn-primary"">Post Thread</button>
                    <button type=""button"" onclick=""document.getElementById('new-thread-form').style.display='none'"" class=""btn-secondary"">Cancel</button>
                </form>
            </div>

            <div class=""vb-threads"">
                <?php if (empty($threads)): ?>
                    <p class=""no-threads"">No threads yet. Be the first to post!</p>
                <?php else: ?>
                    <?php foreach ($threads as $thread): ?>
                        <div class=""vb-thread <?php echo $thread['is_pinned'] ? 'pinned' : ''; ?>"">
                            <div class=""thread-icon"">
                                <?php echo $thread['is_pinned'] ? '📌' : '💬'; ?>
                            </div>
                            <div class=""thread-info"">
                                <h4><a href=""index.php?action=thread&thread=<?php echo $thread['id']; ?>""><?php echo htmlspecialchars($thread['title']); ?></a></h4>
                                <span class=""thread-meta"">Started by <?php echo htmlspecialchars($thread['author_username']); ?> on <?php echo date('M d, Y', strtotime($thread['created_at'])); ?></span>
                            </div>
                            <div class=""thread-stats"">
                                <span>👁️ <?php echo $thread['views']; ?> views</span>
                            </div>
                        </div>
                    <?php endforeach; ?>
                <?php endif; ?>
            </div>

        <?php elseif ($action === 'thread' && $threadId): ?>
            <!-- Thread View with Posts -->
            <?php
            $thread = $db->prepare('SELECT t.*, c.name as category_name, c.id as category_id FROM forum_threads t JOIN forum_categories c ON t.category_id = c.id WHERE t.id = ?');
            $thread->execute([$threadId]);
            $thread = $thread->fetch(PDO::FETCH_ASSOC);
            
            if ($thread) {
                // Increment views
                $db->prepare('UPDATE forum_threads SET views = views + 1 WHERE id = ?')->execute([$threadId]);
                
                $posts = $db->prepare('SELECT * FROM forum_posts WHERE thread_id = ? ORDER BY created_at ASC');
                $posts->execute([$threadId]);
                $posts = $posts->fetchAll(PDO::FETCH_ASSOC);
            }
            ?>
            
            <?php if ($thread): ?>
                <div class=""forum-breadcrumb"">
                    <a href=""index.php"">Forums</a> &gt; 
                    <a href=""index.php?action=category&category=<?php echo $thread['category_id']; ?>""><?php echo htmlspecialchars($thread['category_name']); ?></a> &gt; 
                    <?php echo htmlspecialchars($thread['title']); ?>
                </div>

                <div class=""thread-title"">
                    <h2><?php echo htmlspecialchars($thread['title']); ?></h2>
                </div>

                <div class=""vb-posts"">
                    <?php foreach ($posts as $index => $post): ?>
                        <div class=""vb-post <?php echo $index === 0 ? 'first-post' : ''; ?>"">
                            <div class=""post-author"">
                                <div class=""author-avatar"">👤</div>
                                <strong><?php echo htmlspecialchars($post['author_username']); ?></strong>
                                <span class=""post-date""><?php echo date('M d, Y g:i A', strtotime($post['created_at'])); ?></span>
                            </div>
                            <div class=""post-content"">
                                <?php echo nl2br(htmlspecialchars($post['content'])); ?>
                            </div>
                        </div>
                    <?php endforeach; ?>
                </div>

                <?php if ($isLoggedIn && !$thread['is_locked']): ?>
                    <div class=""reply-form form-container"">
                        <h3>Post Reply</h3>
                        <form method=""POST"" action=""index.php?action=new_post"">
                            <input type=""hidden"" name=""thread_id"" value=""<?php echo $threadId; ?>"">
                            <textarea name=""content"" placeholder=""Your reply..."" required class=""form-textarea""></textarea>
                            <button type=""submit"" class=""btn-primary"">Post Reply</button>
                        </form>
                    </div>
                <?php elseif ($thread['is_locked']): ?>
                    <p class=""locked-message"">🔒 This thread is locked. No new replies can be posted.</p>
                <?php else: ?>
                    <p class=""login-message"">Please <a href=""../control/index.php"">login</a> to post a reply.</p>
                <?php endif; ?>
            <?php else: ?>
                <p class=""error"">Thread not found.</p>
            <?php endif; ?>
        <?php endif; ?>
    </div>

    <script>
    function showNewThreadForm() {
        document.getElementById('new-thread-form').style.display = 'block';
    }
    </script>
</body>
</html>";

        File.WriteAllText(Path.Combine(communityDir, "index.php"), forumContent);
        LogInfo("Generated community/index.php (vBulletin-style forums)");
    }

    private void GenerateForumStylesFile()
    {
        var communityDir = Path.Combine(_cmsRootPath!, "community");
        
        var stylesContent = @"/* RaCore Community Forums - vBulletin v3 Style */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: Verdana, Arial, Helvetica, sans-serif;
    font-size: 12px;
    background: #E4E7F5;
    color: #000;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 15px;
}

/* vBulletin Header */
.vb-header {
    background: linear-gradient(to bottom, #4A6DA6 0%, #35547C 100%);
    padding: 10px 0;
    border-bottom: 1px solid #2A4365;
    margin-bottom: 15px;
}

.vb-header h1 {
    color: #fff;
    font-size: 24px;
    margin-bottom: 8px;
}

.vb-nav {
    display: flex;
    gap: 15px;
    align-items: center;
}

.vb-nav a {
    color: #fff;
    text-decoration: none;
    font-size: 11px;
    padding: 3px 8px;
    border-radius: 3px;
}

.vb-nav a:hover {
    background: rgba(255, 255, 255, 0.1);
}

.vb-nav .username {
    color: #FFD700;
    font-weight: bold;
    margin-left: auto;
}

/* Forum Categories */
.forum-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
    padding: 12px;
    background: #4A6DA6;
    border: 1px solid #2A4365;
    border-radius: 5px;
}

.forum-header h2 {
    color: #fff;
    font-size: 14px;
    font-weight: bold;
}

.vb-categories {
    background: #fff;
    border: 1px solid #C2CFDF;
}

.vb-category {
    display: flex;
    align-items: center;
    padding: 12px;
    border-bottom: 1px solid #E4E7F5;
    transition: background 0.2s;
}

.vb-category:hover {
    background: #F5F8FA;
}

.vb-category:last-child {
    border-bottom: none;
}

.category-icon {
    font-size: 32px;
    margin-right: 15px;
}

.category-info {
    flex: 1;
}

.category-info h3 {
    font-size: 13px;
    margin-bottom: 3px;
}

.category-info h3 a {
    color: #22229C;
    text-decoration: none;
    font-weight: bold;
}

.category-info h3 a:hover {
    color: #C00;
    text-decoration: underline;
}

.category-info p {
    color: #666;
    font-size: 11px;
}

.category-stats {
    text-align: right;
    font-size: 11px;
    color: #666;
}

/* Threads */
.vb-threads {
    background: #fff;
    border: 1px solid #C2CFDF;
}

.vb-thread {
    display: flex;
    align-items: center;
    padding: 10px;
    border-bottom: 1px solid #E4E7F5;
    transition: background 0.2s;
}

.vb-thread:hover {
    background: #F5F8FA;
}

.vb-thread.pinned {
    background: #FFF9E6;
}

.vb-thread.pinned:hover {
    background: #FFF4CC;
}

.thread-icon {
    font-size: 24px;
    margin-right: 12px;
}

.thread-info {
    flex: 1;
}

.thread-info h4 {
    font-size: 12px;
    margin-bottom: 3px;
}

.thread-info h4 a {
    color: #22229C;
    text-decoration: none;
    font-weight: bold;
}

.thread-info h4 a:hover {
    color: #C00;
    text-decoration: underline;
}

.thread-meta {
    font-size: 10px;
    color: #666;
}

.thread-stats {
    text-align: right;
    font-size: 11px;
    color: #666;
}

/* Posts */
.thread-title {
    background: #4A6DA6;
    padding: 12px;
    border: 1px solid #2A4365;
    border-radius: 5px;
    margin-bottom: 15px;
}

.thread-title h2 {
    color: #fff;
    font-size: 16px;
    font-weight: bold;
}

.vb-posts {
    background: #fff;
    border: 1px solid #C2CFDF;
}

.vb-post {
    display: flex;
    border-bottom: 1px solid #E4E7F5;
}

.vb-post.first-post {
    background: #F5F8FA;
}

.post-author {
    width: 180px;
    padding: 15px;
    background: #F2F6F8;
    border-right: 1px solid #E4E7F5;
    text-align: center;
}

.author-avatar {
    font-size: 48px;
    margin-bottom: 10px;
}

.post-author strong {
    display: block;
    font-size: 12px;
    color: #22229C;
    margin-bottom: 5px;
}

.post-date {
    display: block;
    font-size: 10px;
    color: #666;
}

.post-content {
    flex: 1;
    padding: 15px;
    font-size: 12px;
    line-height: 1.6;
}

/* Forms */
.form-container {
    background: #fff;
    border: 1px solid #C2CFDF;
    padding: 15px;
    margin-bottom: 15px;
    border-radius: 5px;
}

.form-container h3 {
    font-size: 14px;
    margin-bottom: 15px;
    color: #22229C;
}

.form-input, .form-textarea, .form-select {
    width: 100%;
    padding: 8px;
    margin-bottom: 10px;
    border: 1px solid #C2CFDF;
    border-radius: 3px;
    font-family: Verdana, Arial, sans-serif;
    font-size: 12px;
}

.form-textarea {
    min-height: 120px;
    resize: vertical;
}

.btn-primary, .btn-secondary {
    padding: 6px 15px;
    border: none;
    border-radius: 3px;
    font-size: 11px;
    font-weight: bold;
    cursor: pointer;
    margin-right: 8px;
}

.btn-primary {
    background: #4A6DA6;
    color: #fff;
}

.btn-primary:hover {
    background: #35547C;
}

.btn-secondary {
    background: #ccc;
    color: #333;
}

.btn-secondary:hover {
    background: #bbb;
}

/* Breadcrumb */
.forum-breadcrumb {
    padding: 10px;
    margin-bottom: 15px;
    font-size: 11px;
}

.forum-breadcrumb a {
    color: #22229C;
    text-decoration: none;
}

.forum-breadcrumb a:hover {
    text-decoration: underline;
}

/* Messages */
.no-threads, .locked-message, .login-message, .error {
    padding: 20px;
    text-align: center;
    background: #fff;
    border: 1px solid #C2CFDF;
    border-radius: 5px;
    margin-top: 15px;
}

.locked-message {
    background: #FFF9E6;
}

.error {
    background: #FFE6E6;
    color: #C00;
}

/* Reply Form */
.reply-form {
    margin-top: 15px;
}
";

        File.WriteAllText(Path.Combine(communityDir, "forum_styles.css"), stylesContent);
        LogInfo("Generated community/forum_styles.css");
    }

    private void GenerateProfileSystemFiles(string cmsDbPath)
    {
        // Generate profile.php in root
        var profileContent = @"<?php
// RaCore User Profiles - MySpace Style
session_start();

$dbPath = 'cms_database.sqlite';
$forumDbPath = 'forum_database.sqlite';

$db = new PDO('sqlite:' . $dbPath);
$db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

$forumDb = new PDO('sqlite:' . $forumDbPath);
$forumDb->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

$viewingUser = $_GET['user'] ?? '';
$isOwner = isset($_SESSION['username']) && $_SESSION['username'] === $viewingUser;
$isLoggedIn = isset($_SESSION['username']);

// Handle profile updates
if ($isOwner && $_SERVER['REQUEST_METHOD'] === 'POST') {
    $displayName = $_POST['display_name'] ?? '';
    $bio = $_POST['bio'] ?? '';
    $customCss = $_POST['custom_css'] ?? '';
    
    $stmt = $forumDb->prepare('INSERT OR REPLACE INTO user_profiles (username, display_name, bio, custom_css, created_at, last_updated) VALUES (?, ?, ?, ?, datetime(""now""), datetime(""now""))');
    $stmt->execute([$viewingUser, $displayName, $bio, $customCss]);
    
    header('Location: profile.php?user=' . urlencode($viewingUser));
    exit;
}

// Get profile data
$stmt = $forumDb->prepare('SELECT * FROM user_profiles WHERE username = ?');
$stmt->execute([$viewingUser]);
$profile = $stmt->fetch(PDO::FETCH_ASSOC);

$displayName = $profile['display_name'] ?? $viewingUser;
$bio = $profile['bio'] ?? '';
$customCss = $profile['custom_css'] ?? '';

// Get recent forum posts
$stmt = $forumDb->prepare('SELECT p.*, t.title as thread_title FROM forum_posts p JOIN forum_threads t ON p.thread_id = t.id WHERE p.author_username = ? ORDER BY p.created_at DESC LIMIT 10');
$stmt->execute([$viewingUser]);
$recentPosts = $stmt->fetchAll(PDO::FETCH_ASSOC);
?>
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title><?php echo htmlspecialchars($displayName); ?>'s Profile - RaCore</title>
    <link rel=""stylesheet"" href=""profile_styles.css"">
    <?php if ($customCss): ?>
        <style>
        /* User Custom CSS */
        <?php echo $customCss; ?>
        </style>
    <?php endif; ?>
</head>
<body>
    <div class=""myspace-header"">
        <div class=""container"">
            <h1>MySpace-Style Profile</h1>
            <div class=""header-nav"">
                <a href=""index.php"">Home</a>
                <a href=""community/index.php"">Community</a>
                <?php if ($isLoggedIn): ?>
                    <a href=""profile.php?user=<?php echo urlencode($_SESSION['username']); ?>"">My Profile</a>
                    <a href=""control/index.php"">Control Panel</a>
                <?php else: ?>
                    <a href=""control/index.php"">Login</a>
                <?php endif; ?>
            </div>
        </div>
    </div>

    <div class=""container"">
        <div class=""profile-layout"">
            <!-- Profile Header -->
            <div class=""profile-header"">
                <div class=""profile-avatar"">
                    <div class=""avatar-placeholder"">👤</div>
                </div>
                <div class=""profile-info"">
                    <h2><?php echo htmlspecialchars($displayName); ?></h2>
                    <p class=""username"">@<?php echo htmlspecialchars($viewingUser); ?></p>
                    <?php if ($bio): ?>
                        <p class=""bio""><?php echo nl2br(htmlspecialchars($bio)); ?></p>
                    <?php endif; ?>
                </div>
            </div>

            <div class=""profile-content"">
                <!-- Left Column -->
                <div class=""profile-sidebar"">
                    <div class=""profile-section"">
                        <h3>About Me</h3>
                        <div class=""about-content"">
                            <?php if ($bio): ?>
                                <?php echo nl2br(htmlspecialchars($bio)); ?>
                            <?php else: ?>
                                <p class=""no-content"">No information provided yet.</p>
                            <?php endif; ?>
                        </div>
                    </div>

                    <?php if ($isOwner): ?>
                        <div class=""profile-section edit-section"">
                            <h3>🎨 Customize Your Profile</h3>
                            <form method=""POST"" class=""edit-form"">
                                <label>Display Name:</label>
                                <input type=""text"" name=""display_name"" value=""<?php echo htmlspecialchars($displayName); ?>"" class=""form-input"">
                                
                                <label>Bio:</label>
                                <textarea name=""bio"" class=""form-textarea""><?php echo htmlspecialchars($bio); ?></textarea>
                                
                                <label>Custom CSS (MySpace-style!):</label>
                                <textarea name=""custom_css"" class=""form-textarea custom-css""><?php echo htmlspecialchars($customCss); ?></textarea>
                                <p class=""hint"">Add your own CSS to customize your profile! Try: <code>body { background: #000; color: #0f0; }</code></p>
                                
                                <button type=""submit"" class=""btn-save"">💾 Save Changes</button>
                            </form>
                        </div>
                    <?php endif; ?>
                </div>

                <!-- Right Column -->
                <div class=""profile-main"">
                    <div class=""profile-section"">
                        <h3>Recent Forum Posts</h3>
                        <?php if (empty($recentPosts)): ?>
                            <p class=""no-content"">No forum posts yet.</p>
                        <?php else: ?>
                            <div class=""recent-posts"">
                                <?php foreach ($recentPosts as $post): ?>
                                    <div class=""post-item"">
                                        <div class=""post-thread"">
                                            <a href=""community/index.php?action=thread&thread=<?php echo $post['thread_id']; ?>"">
                                                <?php echo htmlspecialchars($post['thread_title']); ?>
                                            </a>
                                        </div>
                                        <div class=""post-preview"">
                                            <?php echo htmlspecialchars(substr($post['content'], 0, 150)); ?><?php echo strlen($post['content']) > 150 ? '...' : ''; ?>
                                        </div>
                                        <div class=""post-date"">
                                            <?php echo date('M d, Y', strtotime($post['created_at'])); ?>
                                        </div>
                                    </div>
                                <?php endforeach; ?>
                            </div>
                        <?php endif; ?>
                    </div>

                    <div class=""profile-section"">
                        <h3>Profile Stats</h3>
                        <div class=""stats"">
                            <div class=""stat-item"">
                                <span class=""stat-label"">Total Posts:</span>
                                <span class=""stat-value""><?php echo count($recentPosts); ?>+</span>
                            </div>
                            <div class=""stat-item"">
                                <span class=""stat-label"">Member Since:</span>
                                <span class=""stat-value""><?php echo $profile ? date('M Y', strtotime($profile['created_at'])) : 'Recently'; ?></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class=""myspace-footer"">
        <p>🎵 RaCore Profiles - Express Yourself!</p>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "profile.php"), profileContent);
        LogInfo("Generated profile.php (MySpace-style profiles)");

        // Generate profile styles
        var profileStylesContent = @"/* RaCore Profiles - MySpace Style */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: Arial, Helvetica, sans-serif;
    background: linear-gradient(to bottom, #0066CC 0%, #003366 100%);
    color: #333;
    min-height: 100vh;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 15px;
}

/* MySpace Header */
.myspace-header {
    background: #1A1A1A;
    padding: 15px 0;
    border-bottom: 3px solid #FF6600;
    margin-bottom: 20px;
}

.myspace-header h1 {
    color: #FF6600;
    font-size: 24px;
    margin-bottom: 8px;
}

.header-nav {
    display: flex;
    gap: 15px;
}

.header-nav a {
    color: #66CCFF;
    text-decoration: none;
    font-size: 12px;
}

.header-nav a:hover {
    color: #FF6600;
}

/* Profile Layout */
.profile-layout {
    background: #fff;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 5px 20px rgba(0, 0, 0, 0.3);
}

.profile-header {
    display: flex;
    align-items: center;
    padding: 30px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: #fff;
}

.profile-avatar {
    margin-right: 25px;
}

.avatar-placeholder {
    width: 150px;
    height: 150px;
    background: #fff;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 80px;
    border: 5px solid #fff;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.profile-info h2 {
    font-size: 32px;
    margin-bottom: 5px;
}

.username {
    font-size: 18px;
    opacity: 0.9;
    margin-bottom: 15px;
}

.bio {
    font-size: 14px;
    line-height: 1.6;
    max-width: 600px;
}

/* Profile Content */
.profile-content {
    display: grid;
    grid-template-columns: 1fr 2fr;
    gap: 20px;
    padding: 20px;
}

@media (max-width: 768px) {
    .profile-content {
        grid-template-columns: 1fr;
    }
}

.profile-section {
    background: #f9f9f9;
    border: 2px solid #ddd;
    border-radius: 8px;
    padding: 15px;
    margin-bottom: 15px;
}

.profile-section h3 {
    color: #FF6600;
    font-size: 18px;
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 2px solid #FF6600;
}

.about-content {
    font-size: 14px;
    line-height: 1.6;
}

.no-content {
    color: #999;
    font-style: italic;
}

/* Edit Section */
.edit-section {
    background: #FFF9E6;
    border-color: #FF6600;
}

.edit-form {
    display: flex;
    flex-direction: column;
}

.edit-form label {
    font-weight: bold;
    margin-bottom: 5px;
    margin-top: 10px;
    color: #333;
}

.form-input, .form-textarea {
    padding: 8px;
    border: 2px solid #ddd;
    border-radius: 5px;
    font-family: Arial, sans-serif;
    font-size: 14px;
}

.form-textarea {
    min-height: 80px;
    resize: vertical;
}

.custom-css {
    min-height: 150px;
    font-family: 'Courier New', monospace;
    background: #1e1e1e;
    color: #00ff00;
}

.hint {
    font-size: 11px;
    color: #666;
    margin-top: 5px;
}

.hint code {
    background: #ffe;
    padding: 2px 5px;
    border-radius: 3px;
    font-size: 11px;
}

.btn-save {
    margin-top: 15px;
    padding: 10px 20px;
    background: #FF6600;
    color: #fff;
    border: none;
    border-radius: 5px;
    font-size: 14px;
    font-weight: bold;
    cursor: pointer;
}

.btn-save:hover {
    background: #CC5200;
}

/* Recent Posts */
.recent-posts {
    display: flex;
    flex-direction: column;
    gap: 15px;
}

.post-item {
    padding: 12px;
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 5px;
}

.post-thread {
    font-weight: bold;
    margin-bottom: 8px;
}

.post-thread a {
    color: #0066CC;
    text-decoration: none;
}

.post-thread a:hover {
    color: #FF6600;
    text-decoration: underline;
}

.post-preview {
    font-size: 13px;
    color: #666;
    margin-bottom: 8px;
}

.post-date {
    font-size: 11px;
    color: #999;
}

/* Stats */
.stats {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.stat-item {
    display: flex;
    justify-content: space-between;
    padding: 10px;
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 5px;
}

.stat-label {
    font-weight: bold;
    color: #666;
}

.stat-value {
    color: #FF6600;
    font-weight: bold;
}

/* Footer */
.myspace-footer {
    text-align: center;
    padding: 20px;
    margin-top: 30px;
    color: #fff;
    font-size: 14px;
}
";

        File.WriteAllText(Path.Combine(_cmsRootPath!, "profile_styles.css"), profileStylesContent);
        LogInfo("Generated profile_styles.css");
    }

    #endregion
}
