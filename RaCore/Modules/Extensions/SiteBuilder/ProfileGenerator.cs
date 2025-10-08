namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates profile system (MySpace-style).
/// </summary>
public class ProfileGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;

    public ProfileGenerator(SiteBuilderModule module, string cmsRootPath)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
    }

    public string GenerateProfiles(string phpPath)
    {
        try
        {
            _module.Log("Starting Profile system generation...");
            
            // Generate profile files in the CMS root
            GenerateProfileEditFile();
            GenerateProfileSettingsFile();
            GenerateProfileStylesFile();
            InitializeProfileDatabase();
            
            _module.Log("Profile system generated successfully");
            return $"✅ Profile system generated at: {_cmsRootPath}";
        }
        catch (Exception ex)
        {
            _module.Log($"Profile generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }

    private void InitializeProfileDatabase()
    {
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Databases", "cms_database.sqlite");
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS user_profiles (
                user_id INTEGER PRIMARY KEY,
                bio TEXT,
                location TEXT,
                website TEXT,
                interests TEXT,
                avatar_url TEXT,
                background_color TEXT DEFAULT '#667eea',
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id)
            );

            CREATE TABLE IF NOT EXISTS profile_posts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                content TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id)
            );

            CREATE TABLE IF NOT EXISTS profile_friends (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                friend_id INTEGER NOT NULL,
                status TEXT DEFAULT 'pending',
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id),
                FOREIGN KEY (friend_id) REFERENCES users(id)
            );

            CREATE TABLE IF NOT EXISTS profile_comments (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                profile_user_id INTEGER NOT NULL,
                author_id INTEGER NOT NULL,
                content TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (profile_user_id) REFERENCES users(id),
                FOREIGN KEY (author_id) REFERENCES users(id)
            );
        ";
        
        command.ExecuteNonQuery();
        _module.Log("Profile database tables initialized");
    }

    private void GenerateProfileEditFile()
    {
        var content = @"<?php
require_once 'config.php';
require_once 'db.php';

session_start();

if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    header('Location: admin.php');
    exit;
}

$db = new Database();
$userId = $_SESSION['user_id'] ?? null;
$message = '';

// Get current profile
$profile = $db->query('SELECT * FROM user_profiles WHERE user_id = ?', [$userId]);
if (empty($profile)) {
    // Create profile if it doesn't exist
    $db->execute('INSERT INTO user_profiles (user_id) VALUES (?)', [$userId]);
    $profile = $db->query('SELECT * FROM user_profiles WHERE user_id = ?', [$userId]);
}
$profile = $profile[0];

// Handle form submission
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $bio = $_POST['bio'] ?? '';
    $location = $_POST['location'] ?? '';
    $website = $_POST['website'] ?? '';
    $interests = $_POST['interests'] ?? '';
    $backgroundColor = $_POST['background_color'] ?? '#667eea';
    
    $db->execute(
        'UPDATE user_profiles SET bio = ?, location = ?, website = ?, interests = ?, background_color = ?, updated_at = CURRENT_TIMESTAMP WHERE user_id = ?',
        [$bio, $location, $website, $interests, $backgroundColor, $userId]
    );
    
    $message = '✅ Profile updated successfully!';
    $profile = $db->query('SELECT * FROM user_profiles WHERE user_id = ?', [$userId])[0];
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>Edit Profile - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='styles.css'>
    <link rel='stylesheet' href='profile-styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'><?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Home</a>
                <a href='blogs.php'>Blogs</a>
                <a href='forums.php'>Forums</a>
                <a href='chat.php'>Chat</a>
                <a href='profile.php?user=<?php echo urlencode($_SESSION['username']); ?>'>Social</a>
                <a href='/control-panel.html' target='_blank'>Settings</a>
            </div>
        </nav>
        <div class='content'>
            <div class='profile-edit-header'>
                <h1>✏️ Edit Your Profile</h1>
                <a href='profile.php?user=<?php echo urlencode($_SESSION['username']); ?>' class='btn btn-secondary'>View Profile</a>
            </div>
            
            <?php if ($message): ?>
            <div class='alert alert-success'><?php echo $message; ?></div>
            <?php endif; ?>
            
            <div class='profile-edit-form'>
                <form method='post'>
                    <div class='form-section'>
                        <h2>Basic Information</h2>
                        
                        <div class='form-group'>
                            <label>Bio</label>
                            <textarea name='bio' class='form-control' rows='4' placeholder='Tell us about yourself...'><?php echo htmlspecialchars($profile['bio'] ?? ''); ?></textarea>
                        </div>
                        
                        <div class='form-group'>
                            <label>Location</label>
                            <input type='text' name='location' class='form-control' value='<?php echo htmlspecialchars($profile['location'] ?? ''); ?>' placeholder='City, Country'>
                        </div>
                        
                        <div class='form-group'>
                            <label>Website</label>
                            <input type='url' name='website' class='form-control' value='<?php echo htmlspecialchars($profile['website'] ?? ''); ?>' placeholder='https://example.com'>
                        </div>
                        
                        <div class='form-group'>
                            <label>Interests (comma separated)</label>
                            <input type='text' name='interests' class='form-control' value='<?php echo htmlspecialchars($profile['interests'] ?? ''); ?>' placeholder='Gaming, Coding, Music'>
                        </div>
                    </div>
                    
                    <div class='form-section'>
                        <h2>Appearance</h2>
                        
                        <div class='form-group'>
                            <label>Background Color</label>
                            <div class='color-picker-wrapper'>
                                <input type='color' name='background_color' value='<?php echo htmlspecialchars($profile['background_color'] ?? '#667eea'); ?>' class='color-picker'>
                                <span class='color-preview' style='background: <?php echo htmlspecialchars($profile['background_color'] ?? '#667eea'); ?>;'></span>
                            </div>
                            <small>Choose a color for your profile theme</small>
                        </div>
                    </div>
                    
                    <button type='submit' class='btn btn-primary'>💾 Save Changes</button>
                    <a href='profile.php?user=<?php echo urlencode($_SESSION['username']); ?>' class='btn btn-secondary'>Cancel</a>
                </form>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Manage Your Site</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(_cmsRootPath, "profile-edit.php"), content);
        _module.Log("Generated profile-edit.php");
    }

    private void GenerateProfileSettingsFile()
    {
        var content = @"<?php
require_once 'config.php';
require_once 'db.php';

session_start();

if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    header('Location: admin.php');
    exit;
}

$db = new Database();
$userId = $_SESSION['user_id'] ?? null;
$message = '';

// Handle privacy settings
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $action = $_POST['action'] ?? '';
    
    if ($action === 'change_password') {
        $currentPassword = $_POST['current_password'] ?? '';
        $newPassword = $_POST['new_password'] ?? '';
        $confirmPassword = $_POST['confirm_password'] ?? '';
        
        if ($newPassword === $confirmPassword) {
            $user = $db->query('SELECT * FROM users WHERE id = ?', [$userId]);
            if (!empty($user) && $user[0]['password'] === $currentPassword) {
                $db->execute('UPDATE users SET password = ? WHERE id = ?', [$newPassword, $userId]);
                $message = '✅ Password changed successfully!';
            } else {
                $message = '❌ Current password is incorrect';
            }
        } else {
            $message = '❌ New passwords do not match';
        }
    }
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>Profile Settings - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='styles.css'>
    <link rel='stylesheet' href='profile-styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'><?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Home</a>
                <a href='blogs.php'>Blogs</a>
                <a href='forums.php'>Forums</a>
                <a href='chat.php'>Chat</a>
                <a href='profile.php?user=<?php echo urlencode($_SESSION['username']); ?>'>Social</a>
                <a href='/control-panel.html' target='_blank'>Settings</a>
            </div>
        </nav>
        <div class='content'>
            <div class='profile-settings-header'>
                <h1>⚙️ Profile Settings</h1>
                <div class='settings-tabs'>
                    <a href='profile-edit.php' class='tab-btn'>Edit Profile</a>
                    <a href='profile-settings.php' class='tab-btn active'>Settings</a>
                </div>
            </div>
            
            <?php if ($message): ?>
            <div class='alert <?php echo strpos($message, '✅') !== false ? 'alert-success' : 'alert-danger'; ?>'>
                <?php echo $message; ?>
            </div>
            <?php endif; ?>
            
            <div class='settings-section'>
                <h2>🔒 Security</h2>
                <div class='settings-form'>
                    <form method='post'>
                        <input type='hidden' name='action' value='change_password'>
                        
                        <div class='form-group'>
                            <label>Current Password</label>
                            <input type='password' name='current_password' class='form-control' required>
                        </div>
                        
                        <div class='form-group'>
                            <label>New Password</label>
                            <input type='password' name='new_password' class='form-control' required>
                        </div>
                        
                        <div class='form-group'>
                            <label>Confirm New Password</label>
                            <input type='password' name='confirm_password' class='form-control' required>
                        </div>
                        
                        <button type='submit' class='btn btn-primary'>🔐 Change Password</button>
                    </form>
                </div>
            </div>
            
            <div class='settings-section'>
                <h2>🔔 Notifications</h2>
                <div class='settings-form'>
                    <p style='color: #666;'>Notification preferences will be available in a future update.</p>
                </div>
            </div>
            
            <div class='settings-section'>
                <h2>🔐 Privacy</h2>
                <div class='settings-form'>
                    <p style='color: #666;'>Privacy settings will be available in a future update.</p>
                    <ul style='margin-top: 10px; color: #666;'>
                        <li>Profile visibility</li>
                        <li>Friend request settings</li>
                        <li>Post visibility</li>
                    </ul>
                </div>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Manage Your Site</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(_cmsRootPath, "profile-settings.php"), content);
        _module.Log("Generated profile-settings.php");
    }

    private void GenerateProfileStylesFile()
    {
        var content = @"/* Profile-specific styles */

.profile-edit-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.profile-edit-header h1 {
    margin: 0;
    border: none;
}

.profile-edit-form {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 25px;
}

.form-section {
    margin-bottom: 30px;
}

.form-section h2 {
    color: #667eea;
    margin-top: 0;
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 2px solid #667eea;
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

.form-group small {
    display: block;
    margin-top: 5px;
    color: #666;
    font-size: 0.9em;
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

textarea.form-control {
    resize: vertical;
    font-family: inherit;
}

.color-picker-wrapper {
    display: flex;
    align-items: center;
    gap: 15px;
}

.color-picker {
    width: 60px;
    height: 40px;
    border: 2px solid #ddd;
    border-radius: 5px;
    cursor: pointer;
}

.color-preview {
    width: 100px;
    height: 40px;
    border-radius: 5px;
    border: 1px solid #ddd;
}

.btn {
    padding: 12px 24px;
    border: none;
    border-radius: 5px;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    text-decoration: none;
    display: inline-block;
    transition: background 0.3s;
    margin-right: 10px;
}

.btn-primary {
    background: #667eea;
    color: white;
}

.btn-primary:hover {
    background: #764ba2;
}

.btn-secondary {
    background: #6c757d;
    color: white;
}

.btn-secondary:hover {
    background: #5a6268;
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

.alert-danger {
    background: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
}

.profile-settings-header {
    margin-bottom: 20px;
}

.profile-settings-header h1 {
    margin: 0 0 15px 0;
    border: none;
}

.settings-tabs {
    display: flex;
    gap: 10px;
    border-bottom: 2px solid #ddd;
    margin-top: 10px;
}

.tab-btn {
    padding: 10px 20px;
    background: transparent;
    color: #667eea;
    text-decoration: none;
    border: none;
    border-bottom: 3px solid transparent;
    font-weight: bold;
    transition: border-color 0.3s;
}

.tab-btn:hover {
    border-bottom-color: #667eea;
}

.tab-btn.active {
    border-bottom-color: #667eea;
    color: #333;
}

.settings-section {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 25px;
    margin-bottom: 20px;
}

.settings-section h2 {
    margin-top: 0;
    color: #667eea;
    border-bottom: 2px solid #667eea;
    padding-bottom: 10px;
}

.settings-form {
    margin-top: 15px;
}

/* Profile container styles */
.profile-container {
    display: grid;
    grid-template-columns: 300px 1fr;
    gap: 20px;
}

.profile-sidebar {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
}

.profile-avatar {
    width: 150px;
    height: 150px;
    border-radius: 50%;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 60px;
    font-weight: bold;
    margin: 0 auto 20px;
}

.profile-main {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
}

.profile-stat {
    display: inline-block;
    margin-right: 20px;
    padding: 10px;
    background: #f0f0f0;
    border-radius: 5px;
    text-align: center;
}

.profile-stat strong {
    display: block;
    font-size: 1.5em;
    color: #667eea;
}

.friends-list {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 10px;
}

.friend-badge {
    display: inline-block;
    padding: 5px 12px;
    background: #667eea;
    color: white;
    border-radius: 15px;
    font-size: 0.9em;
}

.add-friend-btn {
    display: inline-block;
    background: #28a745;
    color: white;
    padding: 10px 20px;
    border-radius: 5px;
    text-decoration: none;
    margin-top: 15px;
}

.add-friend-btn:hover {
    background: #218838;
}

/* Responsive design */
@media (max-width: 768px) {
    .profile-container {
        grid-template-columns: 1fr;
    }
    
    .color-picker-wrapper {
        flex-direction: column;
        align-items: flex-start;
    }
}";

        File.WriteAllText(Path.Combine(_cmsRootPath, "profile-styles.css"), content);
        _module.Log("Generated profile-styles.css");
    }
}
