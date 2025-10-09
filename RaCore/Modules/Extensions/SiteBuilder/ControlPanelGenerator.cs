namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates Control Panel for site administration.
/// </summary>
public class ControlPanelGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;
    private readonly string _cmsInternalPath;

    public ControlPanelGenerator(SiteBuilderModule module, string cmsRootPath)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
        // PHP files go to internal directory, not wwwroot
        _cmsInternalPath = Path.Combine(Directory.GetCurrentDirectory(), "CMS");
    }

    public string GenerateControlPanel(string phpPath)
    {
        try
        {
            _module.Log("Starting Control Panel generation...");
            
            var controlPath = Path.Combine(_cmsInternalPath, "control");
            if (!Directory.Exists(controlPath))
            {
                Directory.CreateDirectory(controlPath);
                _module.Log($"Created Control Panel directory: {controlPath}");
            }

            // Generate control panel files
            GenerateControlIndexFile(controlPath);
            GenerateControlSettingsFile(controlPath);
            GenerateControlUsersFile(controlPath);
            GenerateControlStylesFile(controlPath);
            
            _module.Log("Control Panel generated successfully");
            return $"✅ Control Panel generated at: {controlPath}";
        }
        catch (Exception ex)
        {
            _module.Log($"Control Panel generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }

    private void GenerateControlIndexFile(string controlPath)
    {
        var content = @"<?php
require_once '../config.php';
require_once '../db.php';

session_start();

// Simple admin authentication check
if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    header('Location: ../admin.php');
    exit;
}

$db = new Database();
$stats = [
    'totalUsers' => count($db->query('SELECT * FROM users')),
    'totalPages' => count($db->query('SELECT * FROM pages')),
];
?>
<!DOCTYPE html>
<html>
<head>
    <title>Control Panel - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='../styles.css'>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'>🎛️ Control Panel - <?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Dashboard</a>
                <a href='settings.php'>Settings</a>
                <a href='users.php'>Users</a>
                <a href='../index.php'>View Site</a>
                <a href='../admin.php?logout=1'>Logout</a>
            </div>
        </nav>
        <div class='content'>
            <h1>📊 Dashboard</h1>
            <p>Welcome to the Control Panel, <?php echo htmlspecialchars($_SESSION['username']); ?>!</p>
            
            <div class='stats-grid'>
                <div class='stat-card'>
                    <div class='stat-icon'>👥</div>
                    <div class='stat-value'><?php echo $stats['totalUsers']; ?></div>
                    <div class='stat-label'>Total Users</div>
                </div>
                <div class='stat-card'>
                    <div class='stat-icon'>📄</div>
                    <div class='stat-value'><?php echo $stats['totalPages']; ?></div>
                    <div class='stat-label'>Total Pages</div>
                </div>
                <div class='stat-card'>
                    <div class='stat-icon'>🌐</div>
                    <div class='stat-value'><?php echo SITE_NAME; ?></div>
                    <div class='stat-label'>Site Name</div>
                </div>
            </div>
            
            <div class='action-panel'>
                <h2>Quick Actions</h2>
                <div class='action-buttons'>
                    <a href='settings.php' class='action-btn'>⚙️ Site Settings</a>
                    <a href='users.php' class='action-btn'>👥 Manage Users</a>
                    <a href='/control-panel.html' target='_blank' class='action-btn'>🎛️ RaCore Control Panel</a>
                    <a href='../index.php' class='action-btn'>🌐 View Site</a>
                </div>
            </div>
            
            <div class='info-panel'>
                <h3>💡 About This Control Panel</h3>
                <p>This is a basic PHP-based control panel for managing your CMS site. For advanced features like:</p>
                <ul>
                    <li>Module Management</li>
                    <li>Game Engine Configuration</li>
                    <li>License Management</li>
                    <li>RaCoin System</li>
                </ul>
                <p>Use the <strong><a href='/control-panel.html' target='_blank'>RaCore Control Panel</a></strong> instead.</p>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Advanced Management</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(controlPath, "index.php"), content);
        _module.Log("Generated control/index.php");
    }

    private void GenerateControlSettingsFile(string controlPath)
    {
        var content = @"<?php
require_once '../config.php';
require_once '../db.php';

session_start();

if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    header('Location: ../admin.php');
    exit;
}

$db = new Database();
$message = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $siteName = $_POST['site_name'] ?? '';
    
    if ($siteName) {
        $db->execute('INSERT OR REPLACE INTO settings (key, value) VALUES (?, ?)', ['site_name', $siteName]);
        $message = '✅ Settings saved successfully!';
    }
}

$settings = $db->query('SELECT * FROM settings');
$settingsMap = [];
foreach ($settings as $setting) {
    $settingsMap[$setting['key']] = $setting['value'];
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>Site Settings - Control Panel</title>
    <link rel='stylesheet' href='../styles.css'>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'>🎛️ Control Panel - <?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Dashboard</a>
                <a href='settings.php'>Settings</a>
                <a href='users.php'>Users</a>
                <a href='../index.php'>View Site</a>
                <a href='../admin.php?logout=1'>Logout</a>
            </div>
        </nav>
        <div class='content'>
            <h1>⚙️ Site Settings</h1>
            
            <?php if ($message): ?>
            <div class='alert alert-success'><?php echo $message; ?></div>
            <?php endif; ?>
            
            <div class='settings-form'>
                <form method='post'>
                    <div class='form-group'>
                        <label>Site Name</label>
                        <input type='text' name='site_name' 
                               value='<?php echo htmlspecialchars($settingsMap['site_name'] ?? 'RaCore CMS'); ?>' 
                               class='form-control' required>
                    </div>
                    
                    <button type='submit' class='btn btn-primary'>💾 Save Settings</button>
                </form>
            </div>
            
            <div class='info-panel' style='margin-top: 30px;'>
                <h3>📝 Note</h3>
                <p>Changes to the site name will require a page refresh to take effect. 
                   For more advanced configuration options, use the 
                   <a href='/control-panel.html' target='_blank'>RaCore Control Panel</a>.</p>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Advanced Management</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(controlPath, "settings.php"), content);
        _module.Log("Generated control/settings.php");
    }

    private void GenerateControlUsersFile(string controlPath)
    {
        var content = @"<?php
require_once '../config.php';
require_once '../db.php';

session_start();

if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    header('Location: ../admin.php');
    exit;
}

$db = new Database();
$message = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $action = $_POST['action'] ?? '';
    
    if ($action === 'add_user') {
        $username = $_POST['username'] ?? '';
        $password = $_POST['password'] ?? '';
        
        if ($username && $password) {
            try {
                $db->execute('INSERT INTO users (username, password) VALUES (?, ?)', [$username, $password]);
                $message = '✅ User added successfully!';
            } catch (Exception $e) {
                $message = '❌ Error: Username already exists';
            }
        }
    } elseif ($action === 'delete_user') {
        $userId = $_POST['user_id'] ?? '';
        if ($userId && $userId != 1) { // Don't allow deleting admin user
            $db->execute('DELETE FROM users WHERE id = ?', [$userId]);
            $message = '✅ User deleted successfully!';
        }
    }
}

$users = $db->query('SELECT * FROM users ORDER BY created_at DESC');
?>
<!DOCTYPE html>
<html>
<head>
    <title>User Management - Control Panel</title>
    <link rel='stylesheet' href='../styles.css'>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'>🎛️ Control Panel - <?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Dashboard</a>
                <a href='settings.php'>Settings</a>
                <a href='users.php'>Users</a>
                <a href='../index.php'>View Site</a>
                <a href='../admin.php?logout=1'>Logout</a>
            </div>
        </nav>
        <div class='content'>
            <h1>👥 User Management</h1>
            
            <?php if ($message): ?>
            <div class='alert alert-success'><?php echo $message; ?></div>
            <?php endif; ?>
            
            <div class='user-form'>
                <h2>Add New User</h2>
                <form method='post'>
                    <input type='hidden' name='action' value='add_user'>
                    <div class='form-group'>
                        <label>Username</label>
                        <input type='text' name='username' class='form-control' required>
                    </div>
                    <div class='form-group'>
                        <label>Password</label>
                        <input type='password' name='password' class='form-control' required>
                    </div>
                    <button type='submit' class='btn btn-primary'>➕ Add User</button>
                </form>
            </div>
            
            <div class='users-list' style='margin-top: 30px;'>
                <h2>Existing Users</h2>
                <table class='data-table'>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Username</th>
                            <th>Created At</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($users as $user): ?>
                        <tr>
                            <td><?php echo $user['id']; ?></td>
                            <td><?php echo htmlspecialchars($user['username']); ?></td>
                            <td><?php echo $user['created_at']; ?></td>
                            <td>
                                <?php if ($user['id'] != 1): ?>
                                <form method='post' style='display: inline;' 
                                      onsubmit='return confirm(""Are you sure you want to delete this user?"");'>
                                    <input type='hidden' name='action' value='delete_user'>
                                    <input type='hidden' name='user_id' value='<?php echo $user['id']; ?>'>
                                    <button type='submit' class='btn btn-danger btn-sm'>🗑️ Delete</button>
                                </form>
                                <?php else: ?>
                                <span style='color: #999;'>Admin (Protected)</span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Advanced Management</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(controlPath, "users.php"), content);
        _module.Log("Generated control/users.php");
    }

    private void GenerateControlStylesFile(string controlPath)
    {
        var content = @"/* Control Panel Specific Styles */

.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
    margin: 30px 0;
}

.stat-card {
    background: white;
    border: 2px solid #667eea;
    border-radius: 10px;
    padding: 30px;
    text-align: center;
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    transition: transform 0.2s;
}

.stat-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 6px 12px rgba(0,0,0,0.15);
}

.stat-icon {
    font-size: 48px;
    margin-bottom: 10px;
}

.stat-value {
    font-size: 32px;
    font-weight: bold;
    color: #667eea;
    margin: 10px 0;
}

.stat-label {
    color: #666;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.action-panel {
    background: #f8f9fa;
    border-radius: 10px;
    padding: 25px;
    margin: 30px 0;
}

.action-panel h2 {
    margin-top: 0;
    color: #667eea;
}

.action-buttons {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
    margin-top: 20px;
}

.action-btn {
    display: block;
    padding: 15px 20px;
    background: #667eea;
    color: white !important;
    text-align: center;
    border-radius: 8px;
    text-decoration: none;
    font-weight: bold;
    transition: background 0.3s;
}

.action-btn:hover {
    background: #764ba2;
    transform: translateY(-2px);
}

.info-panel {
    background: #e8f4f8;
    border-left: 4px solid #667eea;
    border-radius: 5px;
    padding: 20px;
    margin: 30px 0;
}

.info-panel h3 {
    margin-top: 0;
    color: #667eea;
}

.alert {
    padding: 15px 20px;
    border-radius: 5px;
    margin: 20px 0;
}

.alert-success {
    background: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
}

.settings-form, .user-form {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 25px;
    margin: 20px 0;
}

.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    margin-bottom: 8px;
    font-weight: bold;
    color: #333;
}

.form-control {
    width: 100%;
    padding: 10px 15px;
    border: 2px solid #ddd;
    border-radius: 5px;
    font-size: 16px;
    transition: border-color 0.3s;
}

.form-control:focus {
    outline: none;
    border-color: #667eea;
}

.btn {
    padding: 12px 24px;
    border: none;
    border-radius: 5px;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    transition: background 0.3s;
}

.btn-primary {
    background: #667eea;
    color: white;
}

.btn-primary:hover {
    background: #764ba2;
}

.btn-danger {
    background: #dc3545;
    color: white;
}

.btn-danger:hover {
    background: #c82333;
}

.btn-sm {
    padding: 6px 12px;
    font-size: 14px;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    background: white;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.data-table thead {
    background: #667eea;
    color: white;
}

.data-table th, .data-table td {
    padding: 12px 15px;
    text-align: left;
}

.data-table tbody tr {
    border-bottom: 1px solid #ddd;
}

.data-table tbody tr:hover {
    background: #f8f9fa;
}

.users-list {
    margin-top: 30px;
}

.users-list h2 {
    color: #667eea;
    margin-bottom: 15px;
}";

        File.WriteAllText(Path.Combine(controlPath, "styles.css"), content);
        _module.Log("Generated control/styles.css");
    }
}
