<?php
require_once 'config.php';
require_once 'db.php';

session_start();

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $username = $_POST['username'] ?? '';
    $password = $_POST['password'] ?? '';
    
    $db = new Database();
    $users = $db->query('SELECT * FROM users WHERE username = ?', [$username]);
    
    if (!empty($users) && $users[0]['password'] === $password) {
        $_SESSION['logged_in'] = true;
        $_SESSION['username'] = $username;
    }
}

if (!isset($_SESSION['logged_in']) || !$_SESSION['logged_in']) {
    ?>
    <!DOCTYPE html>
    <html>
    <head>
        <title>Admin Login - <?php echo SITE_NAME; ?></title>
        <link rel='stylesheet' href='styles.css'>
        <style>
            .login-container {
                min-height: 100vh;
                display: flex;
                align-items: center;
                justify-content: center;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            }
            .login-box {
                background: white;
                padding: 40px;
                border-radius: 15px;
                box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
                max-width: 400px;
                width: 100%;
            }
            .login-box input {
                width: 100%;
                padding: 12px;
                margin: 10px 0;
                border: 2px solid #e0e0e0;
                border-radius: 8px;
            }
            .login-box button {
                width: 100%;
                padding: 12px;
                background: #667eea;
                color: white;
                border: none;
                border-radius: 8px;
                cursor: pointer;
                font-size: 16px;
                font-weight: bold;
            }
        </style>
    </head>
    <body>
        <div class='login-container'>
            <div class='login-box'>
                <h2>Admin Login</h2>
                <form method='post'>
                    <input name='username' placeholder='Username' required>
                    <input type='password' name='password' placeholder='Password' required>
                    <button>Login</button>
                </form>
                <p style='margin-top: 20px; text-align: center;'>
                    <a href='index.php'>Back to Homepage</a>
                </p>
            </div>
        </div>
    </body>
    </html>
    <?php
    exit;
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>Admin Panel - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'>Admin Panel - <?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Home</a>
                <a href='admin.php'>Admin</a>
                <a href='/control-panel.html' target='_blank'>RaCore Control Panel</a>
                <a href='?logout=1'>Logout</a>
            </div>
        </nav>
        <div class='content'>
            <h1>Welcome, <?php echo htmlspecialchars($_SESSION['username']); ?>!</h1>
            
            <div style='margin: 30px 0; padding: 20px; background: #f8f9fa; border-radius: 10px;'>
                <h2 style='color: #667eea; margin-bottom: 15px;'>Quick Actions</h2>
                <ul style='list-style: none; padding: 0;'>
                    <li style='margin: 10px 0;'>
                        <a href='/control-panel.html' target='_blank' style='display: inline-block; padding: 10px 20px; background: #667eea; color: white; border-radius: 5px; text-decoration: none;'>
                            🎛️ Open RaCore Control Panel
                        </a>
                    </li>
                    <li style='margin: 10px 0; color: #666;'>
                        📝 Manage Pages (Coming Soon)
                    </li>
                    <li style='margin: 10px 0; color: #666;'>
                        👥 Manage Users (Coming Soon)
                    </li>
                    <li style='margin: 10px 0; color: #666;'>
                        ⚙️ Site Settings (Coming Soon)
                    </li>
                </ul>
            </div>
            
            <div style='margin: 30px 0; padding: 20px; background: #e8f4f8; border-radius: 10px; border-left: 4px solid #667eea;'>
                <h3 style='color: #667eea; margin-bottom: 10px;'>💡 Pro Tip</h3>
                <p>Use the <strong>RaCore Control Panel</strong> for advanced management including:</p>
                <ul style='margin-top: 10px;'>
                    <li>User & Role Management</li>
                    <li>License Management</li>
                    <li>RaCoin Wallet Management</li>
                    <li>Game Server Configuration</li>
                    <li>Forum Moderation</li>
                </ul>
            </div>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Advanced Management</a></p>
        </footer>
    </div>
</body>
</html>