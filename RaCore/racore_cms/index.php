<?php
require_once 'config.php';
require_once 'db.php';

$db = new Database();
$pages = $db->query('SELECT * FROM pages LIMIT 1');
$page = $pages[0] ?? ['title' => 'Welcome', 'content' => 'No content'];
?>
<!DOCTYPE html>
<html>
<head>
    <title><?php echo htmlspecialchars($page['title']); ?> - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'><?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='index.php'>Home</a>
                <a href='admin.php'>Admin Panel</a>
                <a href='/control-panel.html' target='_blank'>RaCore Control Panel</a>
            </div>
        </nav>
        <div class='content'>
            <?php echo $page['content']; ?>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Manage Your Site</a></p>
        </footer>
    </div>
</body>
</html>