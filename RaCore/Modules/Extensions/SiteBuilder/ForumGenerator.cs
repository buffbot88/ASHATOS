namespace RaCore.Modules.Extensions.SiteBuilder;

/// <summary>
/// Generates forum system (vBulletin-style).
/// </summary>
public class ForumGenerator
{
    private readonly SiteBuilderModule _module;
    private readonly string _cmsRootPath;
    private readonly string _cmsInternalPath;

    public ForumGenerator(SiteBuilderModule module, string cmsRootPath)
    {
        _module = module;
        _cmsRootPath = cmsRootPath;
        // PHP files go to internal directory, not wwwroot
        _cmsInternalPath = Path.Combine(Directory.GetCurrentDirectory(), "CMS");
    }

    public string GenerateForum(string phpPath)
    {
        try
        {
            _module.Log("Starting Forum generation...");
            
            var forumPath = Path.Combine(_cmsInternalPath, "community");
            if (!Directory.Exists(forumPath))
            {
                Directory.CreateDirectory(forumPath);
            }

            // Generate forum files
            GenerateForumIndexFile(forumPath);
            GenerateForumTopicFile(forumPath);
            GenerateForumPostFile(forumPath);
            GenerateForumStylesFile(forumPath);
            InitializeForumDatabase();
            
            _module.Log("Forum generated successfully");
            return $"✅ Forum generated at: {forumPath}";
        }
        catch (Exception ex)
        {
            _module.Log($"Forum generation failed: {ex.Message}", "ERROR");
            return $"❌ Error: {ex.Message}";
        }
    }

    private void InitializeForumDatabase()
    {
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Databases", "cms_database.sqlite");
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS forum_categories (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS forum_topics (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                category_id INTEGER NOT NULL,
                title TEXT NOT NULL,
                author TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                views INTEGER DEFAULT 0,
                FOREIGN KEY (category_id) REFERENCES forum_categories(id)
            );

            CREATE TABLE IF NOT EXISTS forum_posts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                topic_id INTEGER NOT NULL,
                author TEXT NOT NULL,
                content TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (topic_id) REFERENCES forum_topics(id)
            );

            INSERT OR IGNORE INTO forum_categories (id, name, description) VALUES (1, 'General Discussion', 'General topics and discussions');
            INSERT OR IGNORE INTO forum_categories (id, name, description) VALUES (2, 'Announcements', 'Important announcements and updates');
            INSERT OR IGNORE INTO forum_categories (id, name, description) VALUES (3, 'Support', 'Get help and support');
        ";
        
        command.ExecuteNonQuery();
        _module.Log("Forum database tables initialized");
    }

    private void GenerateForumIndexFile(string forumPath)
    {
        var content = @"<?php
require_once '../config.php';
require_once '../db.php';

session_start();
$currentUser = $_SESSION['username'] ?? 'guest';
$isLoggedIn = isset($_SESSION['logged_in']) && $_SESSION['logged_in'];

$db = new Database();

// Get forum categories with topic counts
$categories = $db->query('SELECT * FROM forum_categories ORDER BY id');
?>
<!DOCTYPE html>
<html>
<head>
    <title>Community Forums - <?php echo SITE_NAME; ?></title>
    <link rel='stylesheet' href='../styles.css'>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'><?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='../index.php'>Home</a>
                <a href='../blogs.php'>Blogs</a>
                <a href='../forums.php'>Forums</a>
                <a href='../chat.php'>Chat</a>
                <a href='../profile.php?user=<?php echo urlencode($currentUser); ?>'>Social</a>
                <a href='/control-panel.html' target='_blank'>Settings</a>
            </div>
        </nav>
        <div class='content'>
            <div class='forum-header'>
                <h1>💬 Community Forums</h1>
                <p>Engage in discussions with the community</p>
            </div>
            
            <div class='forum-categories'>
                <?php foreach ($categories as $category): ?>
                    <?php 
                    $topicCount = count($db->query('SELECT * FROM forum_topics WHERE category_id = ?', [$category['id']]));
                    $postCount = 0;
                    $topics = $db->query('SELECT * FROM forum_topics WHERE category_id = ?', [$category['id']]);
                    foreach ($topics as $topic) {
                        $postCount += count($db->query('SELECT * FROM forum_posts WHERE topic_id = ?', [$topic['id']]));
                    }
                    
                    // Get latest post
                    $latestTopic = $db->query('SELECT * FROM forum_topics WHERE category_id = ? ORDER BY created_at DESC LIMIT 1', [$category['id']]);
                    ?>
                    <div class='forum-category'>
                        <div class='category-icon'>📁</div>
                        <div class='category-info'>
                            <h3><a href='topic.php?category=<?php echo $category['id']; ?>'><?php echo htmlspecialchars($category['name']); ?></a></h3>
                            <p><?php echo htmlspecialchars($category['description']); ?></p>
                            <?php if (!empty($latestTopic)): ?>
                                <div class='latest-post'>
                                    Latest: <a href='topic.php?id=<?php echo $latestTopic[0]['id']; ?>'><?php echo htmlspecialchars($latestTopic[0]['title']); ?></a>
                                    by <?php echo htmlspecialchars($latestTopic[0]['author']); ?>
                                </div>
                            <?php endif; ?>
                        </div>
                        <div class='category-stats'>
                            <div class='stat-item'>
                                <strong><?php echo $topicCount; ?></strong>
                                <span>Topics</span>
                            </div>
                            <div class='stat-item'>
                                <strong><?php echo $postCount; ?></strong>
                                <span>Posts</span>
                            </div>
                        </div>
                    </div>
                <?php endforeach; ?>
            </div>
            
            <?php if (!$isLoggedIn): ?>
            <div class='forum-notice'>
                <p>👋 Please <a href='../admin.php'>log in</a> to create new topics and post replies.</p>
            </div>
            <?php endif; ?>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Manage Your Site</a></p>
        </footer>
    </div>
</body>
</html>";

        File.WriteAllText(Path.Combine(forumPath, "index.php"), content);
        _module.Log("Generated community/index.php");
    }

    private void GenerateForumTopicFile(string forumPath)
    {
        var content = @"<?php
require_once '../config.php';
require_once '../db.php';

session_start();
$currentUser = $_SESSION['username'] ?? 'guest';
$isLoggedIn = isset($_SESSION['logged_in']) && $_SESSION['logged_in'];

$db = new Database();
$message = '';

// Handle new topic creation
if ($_SERVER['REQUEST_METHOD'] === 'POST' && $isLoggedIn) {
    $action = $_POST['action'] ?? '';
    
    if ($action === 'new_topic') {
        $categoryId = $_POST['category_id'] ?? '';
        $title = $_POST['title'] ?? '';
        $content = $_POST['content'] ?? '';
        
        if ($categoryId && $title && $content) {
            $db->execute('INSERT INTO forum_topics (category_id, title, author) VALUES (?, ?, ?)', 
                        [$categoryId, $title, $currentUser]);
            $topicId = $db->query('SELECT last_insert_rowid() as id')[0]['id'];
            $db->execute('INSERT INTO forum_posts (topic_id, author, content) VALUES (?, ?, ?)', 
                        [$topicId, $currentUser, $content]);
            header('Location: topic.php?id=' . $topicId);
            exit;
        }
    } elseif ($action === 'reply') {
        $topicId = $_POST['topic_id'] ?? '';
        $content = $_POST['content'] ?? '';
        
        if ($topicId && $content) {
            $db->execute('INSERT INTO forum_posts (topic_id, author, content) VALUES (?, ?, ?)', 
                        [$topicId, $currentUser, $content]);
            header('Location: topic.php?id=' . $topicId);
            exit;
        }
    }
}

// Get topic or category
$topicId = $_GET['id'] ?? null;
$categoryId = $_GET['category'] ?? null;

if ($topicId) {
    // View single topic with posts
    $topic = $db->query('SELECT * FROM forum_topics WHERE id = ?', [$topicId]);
    if (empty($topic)) {
        die('Topic not found');
    }
    $topic = $topic[0];
    
    // Increment view count
    $db->execute('UPDATE forum_topics SET views = views + 1 WHERE id = ?', [$topicId]);
    
    $posts = $db->query('SELECT * FROM forum_posts WHERE topic_id = ? ORDER BY created_at ASC', [$topicId]);
} elseif ($categoryId) {
    // List topics in category
    $category = $db->query('SELECT * FROM forum_categories WHERE id = ?', [$categoryId]);
    if (empty($category)) {
        die('Category not found');
    }
    $category = $category[0];
    
    $topics = $db->query('SELECT * FROM forum_topics WHERE category_id = ? ORDER BY created_at DESC', [$categoryId]);
}
?>
<!DOCTYPE html>
<html>
<head>
    <title><?php echo $topicId ? htmlspecialchars($topic['title']) : htmlspecialchars($category['name']); ?> - Forums</title>
    <link rel='stylesheet' href='../styles.css'>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <nav class='main-nav'>
            <div class='nav-brand'><?php echo SITE_NAME; ?></div>
            <div class='nav-links'>
                <a href='../index.php'>Home</a>
                <a href='../blogs.php'>Blogs</a>
                <a href='../forums.php'>Forums</a>
                <a href='../chat.php'>Chat</a>
                <a href='../profile.php?user=<?php echo urlencode($currentUser); ?>'>Social</a>
                <a href='/control-panel.html' target='_blank'>Settings</a>
            </div>
        </nav>
        <div class='content'>
            <div class='breadcrumb'>
                <a href='index.php'>Forums</a>
                <?php if ($categoryId): ?>
                    &gt; <?php echo htmlspecialchars($category['name']); ?>
                <?php elseif ($topicId): ?>
                    &gt; <a href='topic.php?category=<?php echo $topic['category_id']; ?>'>Category</a>
                    &gt; <?php echo htmlspecialchars($topic['title']); ?>
                <?php endif; ?>
            </div>
            
            <?php if ($topicId): ?>
                <!-- Single topic view -->
                <div class='topic-header'>
                    <h1><?php echo htmlspecialchars($topic['title']); ?></h1>
                    <div class='topic-meta'>
                        Started by <strong><?php echo htmlspecialchars($topic['author']); ?></strong>
                        on <?php echo date('F j, Y g:i a', strtotime($topic['created_at'])); ?>
                        • <?php echo $topic['views']; ?> views
                    </div>
                </div>
                
                <div class='posts-list'>
                    <?php foreach ($posts as $index => $post): ?>
                    <div class='forum-post <?php echo $index === 0 ? 'first-post' : ''; ?>'>
                        <div class='post-author'>
                            <div class='author-avatar'><?php echo strtoupper(substr($post['author'], 0, 1)); ?></div>
                            <div class='author-name'><?php echo htmlspecialchars($post['author']); ?></div>
                            <div class='post-date'><?php echo date('M j, Y', strtotime($post['created_at'])); ?></div>
                        </div>
                        <div class='post-content'>
                            <?php echo nl2br(htmlspecialchars($post['content'])); ?>
                        </div>
                    </div>
                    <?php endforeach; ?>
                </div>
                
                <?php if ($isLoggedIn): ?>
                <div class='reply-form'>
                    <h3>Post Reply</h3>
                    <form method='post'>
                        <input type='hidden' name='action' value='reply'>
                        <input type='hidden' name='topic_id' value='<?php echo $topicId; ?>'>
                        <textarea name='content' class='form-control' rows='6' placeholder='Write your reply...' required></textarea>
                        <button type='submit' class='btn btn-primary' style='margin-top: 10px;'>📨 Post Reply</button>
                    </form>
                </div>
                <?php endif; ?>
                
            <?php elseif ($categoryId): ?>
                <!-- Category topics list -->
                <div class='category-header'>
                    <h1><?php echo htmlspecialchars($category['name']); ?></h1>
                    <p><?php echo htmlspecialchars($category['description']); ?></p>
                </div>
                
                <?php if ($isLoggedIn): ?>
                <button onclick='showNewTopicForm()' class='btn btn-primary'>➕ New Topic</button>
                
                <div id='newTopicForm' class='new-topic-form' style='display: none;'>
                    <h3>Create New Topic</h3>
                    <form method='post'>
                        <input type='hidden' name='action' value='new_topic'>
                        <input type='hidden' name='category_id' value='<?php echo $categoryId; ?>'>
                        <div class='form-group'>
                            <label>Topic Title</label>
                            <input type='text' name='title' class='form-control' required>
                        </div>
                        <div class='form-group'>
                            <label>Content</label>
                            <textarea name='content' class='form-control' rows='6' required></textarea>
                        </div>
                        <button type='submit' class='btn btn-primary'>📝 Create Topic</button>
                        <button type='button' onclick='hideNewTopicForm()' class='btn btn-secondary'>Cancel</button>
                    </form>
                </div>
                <?php endif; ?>
                
                <div class='topics-list'>
                    <?php if (empty($topics)): ?>
                    <p style='text-align: center; padding: 40px; color: #666;'>No topics yet. Be the first to start a discussion!</p>
                    <?php else: ?>
                        <?php foreach ($topics as $t): ?>
                            <?php $postCount = count($db->query('SELECT * FROM forum_posts WHERE topic_id = ?', [$t['id']])); ?>
                            <div class='topic-item'>
                                <div class='topic-icon'>💬</div>
                                <div class='topic-info'>
                                    <h3><a href='topic.php?id=<?php echo $t['id']; ?>'><?php echo htmlspecialchars($t['title']); ?></a></h3>
                                    <div class='topic-meta'>
                                        By <?php echo htmlspecialchars($t['author']); ?> 
                                        on <?php echo date('F j, Y', strtotime($t['created_at'])); ?>
                                    </div>
                                </div>
                                <div class='topic-stats'>
                                    <div class='stat'><?php echo $postCount; ?> replies</div>
                                    <div class='stat'><?php echo $t['views']; ?> views</div>
                                </div>
                            </div>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </div>
            <?php endif; ?>
        </div>
        <footer class='site-footer'>
            <p>Powered by RaCore | <a href='/control-panel.html'>Manage Your Site</a></p>
        </footer>
    </div>
    
    <script>
    function showNewTopicForm() {
        document.getElementById('newTopicForm').style.display = 'block';
    }
    function hideNewTopicForm() {
        document.getElementById('newTopicForm').style.display = 'none';
    }
    </script>
</body>
</html>";

        File.WriteAllText(Path.Combine(forumPath, "topic.php"), content);
        _module.Log("Generated community/topic.php");
    }

    private void GenerateForumPostFile(string forumPath)
    {
        var content = @"<?php
// Post handling is done in topic.php
// This file can be used for future AJAX post creation
header('Location: index.php');
exit;
?>";

        File.WriteAllText(Path.Combine(forumPath, "post.php"), content);
        _module.Log("Generated community/post.php");
    }

    private void GenerateForumStylesFile(string forumPath)
    {
        var content = @"/* Forum-specific styles */

.forum-header {
    text-align: center;
    margin-bottom: 30px;
}

.forum-categories {
    display: flex;
    flex-direction: column;
    gap: 15px;
}

.forum-category {
    display: flex;
    align-items: center;
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
    transition: box-shadow 0.3s;
}

.forum-category:hover {
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.category-icon {
    font-size: 48px;
    margin-right: 20px;
}

.category-info {
    flex: 1;
}

.category-info h3 {
    margin: 0 0 5px 0;
}

.category-info h3 a {
    color: #667eea;
    text-decoration: none;
}

.category-info h3 a:hover {
    color: #764ba2;
}

.category-info p {
    margin: 5px 0;
    color: #666;
}

.latest-post {
    font-size: 0.9em;
    color: #888;
    margin-top: 8px;
}

.category-stats {
    display: flex;
    gap: 30px;
    text-align: center;
}

.stat-item strong {
    display: block;
    font-size: 24px;
    color: #667eea;
}

.stat-item span {
    display: block;
    font-size: 12px;
    color: #666;
    text-transform: uppercase;
}

.forum-notice {
    background: #fff3cd;
    border: 1px solid #ffc107;
    border-radius: 5px;
    padding: 15px;
    margin-top: 20px;
    text-align: center;
}

.breadcrumb {
    padding: 10px 0;
    margin-bottom: 20px;
    color: #666;
}

.breadcrumb a {
    color: #667eea;
    text-decoration: none;
}

.topic-header {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
    margin-bottom: 20px;
}

.topic-header h1 {
    margin: 0 0 10px 0;
    color: #333;
    border: none;
}

.topic-meta {
    color: #666;
    font-size: 0.9em;
}

.posts-list {
    display: flex;
    flex-direction: column;
    gap: 15px;
    margin-bottom: 30px;
}

.forum-post {
    display: flex;
    gap: 20px;
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
}

.forum-post.first-post {
    border-left: 4px solid #667eea;
}

.post-author {
    flex: 0 0 150px;
    text-align: center;
}

.author-avatar {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 36px;
    font-weight: bold;
    margin: 0 auto 10px;
}

.author-name {
    font-weight: bold;
    color: #333;
    margin-bottom: 5px;
}

.post-date {
    font-size: 0.8em;
    color: #999;
}

.post-content {
    flex: 1;
    line-height: 1.6;
}

.reply-form {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
}

.reply-form h3 {
    margin-top: 0;
    color: #667eea;
}

.category-header {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
    margin-bottom: 20px;
}

.category-header h1 {
    margin: 0 0 10px 0;
    color: #333;
    border: none;
}

.new-topic-form {
    background: white;
    border: 1px solid #ddd;
    border-radius: 10px;
    padding: 20px;
    margin: 20px 0;
}

.new-topic-form h3 {
    margin-top: 0;
    color: #667eea;
}

.form-group {
    margin-bottom: 15px;
}

.form-group label {
    display: block;
    margin-bottom: 5px;
    font-weight: bold;
}

.form-control {
    width: 100%;
    padding: 10px;
    border: 2px solid #ddd;
    border-radius: 5px;
    font-size: 14px;
}

.form-control:focus {
    outline: none;
    border-color: #667eea;
}

.btn {
    padding: 10px 20px;
    border: none;
    border-radius: 5px;
    font-size: 14px;
    font-weight: bold;
    cursor: pointer;
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

.topics-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-top: 20px;
}

.topic-item {
    display: flex;
    align-items: center;
    gap: 15px;
    background: white;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 15px;
    transition: background 0.2s;
}

.topic-item:hover {
    background: #f8f9fa;
}

.topic-icon {
    font-size: 32px;
}

.topic-info {
    flex: 1;
}

.topic-info h3 {
    margin: 0 0 5px 0;
    font-size: 18px;
}

.topic-info h3 a {
    color: #333;
    text-decoration: none;
}

.topic-info h3 a:hover {
    color: #667eea;
}

.topic-stats {
    text-align: right;
    font-size: 0.9em;
    color: #666;
}

.topic-stats .stat {
    margin-bottom: 5px;
}";

        File.WriteAllText(Path.Combine(forumPath, "styles.css"), content);
        _module.Log("Generated community/styles.css");
    }
}
