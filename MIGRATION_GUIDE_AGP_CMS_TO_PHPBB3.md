# Migration Guide: AGP_CMS to phpBB3 Authentication

This guide helps you migrate from the previous AGP_CMS authentication system to the new phpBB3-based authentication.

## Overview

The migration involves:
1. Installing phpBB3 and the ASHATOS Authentication Bridge extension
2. Updating ASHATOS configuration files
3. Optionally migrating existing users
4. Testing the new authentication system

## Prerequisites

- Access to your server
- Existing ASHATOS installation using AGP_CMS
- Database backup (recommended)
- phpBB3 installation or ability to install it

## Step-by-Step Migration

### Phase 1: Preparation

#### 1.1 Backup Current System

```bash
# Backup AGP_CMS database
mysqldump -u username -p agp_cms_db > agp_cms_backup.sql

# Backup ASHATOS configuration
cp AGP_AI/ASHATAIServer/appsettings.json appsettings.json.backup
cp AGP_AI/ASHATGoddess/appsettings.json appsettings.goddess.backup
```

#### 1.2 Document Current Configuration

Note your current settings:
```json
{
  "Authentication": {
    "CmsBaseUrl": "http://localhost:5000",  // Note this URL
    "RequireAuthentication": true           // Note this setting
  }
}
```

### Phase 2: Install phpBB3

#### 2.1 Download and Install phpBB3

```bash
# Download phpBB3 (latest version)
wget https://www.phpbb.com/files/release/phpBB-3.3.x.zip

# Extract to web directory
unzip phpBB-3.3.x.zip -d /var/www/html/
mv /var/www/html/phpBB3 /var/www/html/phpbb

# Set permissions
chown -R www-data:www-data /var/www/html/phpbb
chmod -R 755 /var/www/html/phpbb
```

#### 2.2 Run phpBB Installation

1. Navigate to `http://your-domain.com/phpbb/install/`
2. Complete the installation wizard:
   - Choose database type (MySQL/MariaDB recommended)
   - Create or use existing database
   - Set admin account credentials
   - Configure board settings

3. Delete the install directory:
```bash
rm -rf /var/www/html/phpbb/install
```

### Phase 3: Install ASHATOS Authentication Bridge Extension

#### 3.1 Copy Extension Files

```bash
# Navigate to your ASHATOS repository
cd /path/to/ASHATOS

# Copy extension to phpBB
cp -r phpbb3_extension/ashatos /var/www/html/phpbb/ext/

# Set proper permissions
chown -R www-data:www-data /var/www/html/phpbb/ext/ashatos
chmod -R 755 /var/www/html/phpbb/ext/ashatos
```

#### 3.2 Enable Extension

1. Login to phpBB Admin Control Panel (ACP)
2. Navigate to "Customise" > "Manage extensions"
3. Find "ASHATOS Authentication Bridge"
4. Click "Enable"

#### 3.3 Clear Cache

```bash
cd /var/www/html/phpbb
php bin/phpbbcli.php cache:purge
```

Or manually:
```bash
rm -rf /var/www/html/phpbb/cache/*
```

### Phase 4: Update ASHATOS Configuration

#### 4.1 Update ASHATAIServer

Edit `AGP_AI/ASHATAIServer/appsettings.json`:

**Before:**
```json
{
  "Authentication": {
    "CmsBaseUrl": "http://localhost:5000",
    "RequireAuthentication": true
  }
}
```

**After:**
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-domain.com/phpbb",
    "RequireAuthentication": true
  }
}
```

For local development:
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://localhost/phpbb",
    "RequireAuthentication": true
  }
}
```

#### 4.2 Update ASHATGoddess (if used)

Edit `AGP_AI/ASHATGoddess/appsettings.json`:

**Before:**
```json
{
  "Authentication": {
    "CmsBaseUrl": "http://localhost:5000",
    "RequireAuthentication": false,
    "AutoLogin": false
  }
}
```

**After:**
```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-domain.com/phpbb",
    "RequireAuthentication": false,
    "AutoLogin": false
  }
}
```

#### 4.3 Rebuild ASHATOS

```bash
cd AGP_AI/ASHATAIServer
dotnet build
```

### Phase 5: User Migration (Optional)

You have three options for user migration:

#### Option 1: Let Users Re-register

**Pros:** Simplest approach
**Cons:** Users must create new accounts

1. Notify users that they need to re-register
2. Provide clear instructions
3. Users create accounts through phpBB3 or the game server

#### Option 2: Manual User Import

If you have a small number of users, manually create accounts in phpBB:

1. Login to phpBB ACP
2. Navigate to "Users and Groups" > "Manage users"
3. Click "Create user"
4. Fill in user details
5. Repeat for each user

#### Option 3: Automated User Migration Script

For larger user bases, create a migration script:

```php
<?php
// migration_script.php - Run from phpBB directory

define('IN_PHPBB', true);
$phpbb_root_path = './';
$phpEx = 'php';
include($phpbb_root_path . 'common.' . $phpEx);

// Start session
$user->session_begin();
$auth->acl($user->data);

// Connect to AGP_CMS database
$agp_cms_db = new mysqli('localhost', 'username', 'password', 'agp_cms_db');

// Get users from AGP_CMS
$result = $agp_cms_db->query("SELECT username, email, password_hash FROM users");

while ($row = $result->fetch_assoc()) {
    // Check if user already exists in phpBB
    $sql = 'SELECT user_id FROM ' . USERS_TABLE . "
            WHERE username_clean = '" . $db->sql_escape(utf8_clean_string($row['username'])) . "'";
    $result_check = $db->sql_query($sql);
    
    if (!$db->sql_fetchrow($result_check)) {
        // Create user in phpBB
        $user_row = array(
            'username' => $row['username'],
            'username_clean' => utf8_clean_string($row['username']),
            'user_email' => $row['email'],
            'user_password' => $row['password_hash'], // Note: May need conversion
            'user_type' => USER_NORMAL,
            'user_regdate' => time(),
            'user_lang' => $config['default_lang'],
            'user_timezone' => $config['board_timezone'],
            'user_dateformat' => $config['default_dateformat'],
            'user_style' => (int) $config['default_style'],
        );
        
        $sql = 'INSERT INTO ' . USERS_TABLE . ' ' . $db->sql_build_array('INSERT', $user_row);
        $db->sql_query($sql);
        
        echo "Migrated user: " . $row['username'] . "\n";
    }
    $db->sql_freeresult($result_check);
}

echo "Migration complete!\n";
?>
```

**Important Notes:**
- Password hashes from AGP_CMS may not be compatible with phpBB
- You may need to reset passwords after migration
- Test thoroughly with a few users first

### Phase 6: Testing

#### 6.1 Test Extension Endpoints

```bash
# Test registration
curl -X POST http://your-domain.com/phpbb/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"testpass123"}'

# Test login
curl -X POST http://your-domain.com/phpbb/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"testpass123"}'

# Test validation (use sessionId from login response)
curl -X POST http://your-domain.com/phpbb/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"your-session-id-here"}'
```

#### 6.2 Test ASHATAIServer Integration

```bash
# Start ASHATAIServer
cd AGP_AI/ASHATAIServer
dotnet run

# In another terminal, test authentication
curl -X POST http://localhost:8088/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"testpass123"}'
```

#### 6.3 Test ASHATGoddess (if used)

1. Start ASHATGoddess application
2. Attempt to login with test credentials
3. Verify authentication works
4. Check session persistence

### Phase 7: Cleanup

#### 7.1 Remove AGP_CMS (Optional)

Only after confirming phpBB3 authentication works:

```bash
# Stop AGP_CMS service
systemctl stop agp_cms

# Backup AGP_CMS data (just in case)
tar -czf agp_cms_backup.tar.gz /path/to/agp_cms

# Remove AGP_CMS (optional - keep backup)
# rm -rf /path/to/agp_cms
```

#### 7.2 Update Documentation

Update any internal documentation referencing AGP_CMS to point to phpBB3.

### Rollback Procedure

If you need to rollback to AGP_CMS:

```bash
# Restore configuration
cp appsettings.json.backup AGP_AI/ASHATAIServer/appsettings.json
cp appsettings.goddess.backup AGP_AI/ASHATGoddess/appsettings.json

# Rebuild
cd AGP_AI/ASHATAIServer
dotnet build

# Start AGP_CMS
systemctl start agp_cms

# Restart ASHATOS services
systemctl restart ashataiserver
```

## Common Migration Issues

### Issue 1: Extension Not Appearing

**Symptoms:** Extension doesn't show in ACP
**Solution:**
```bash
cd /var/www/html/phpbb
php bin/phpbbcli.php cache:purge
# Check file permissions
ls -la ext/ashatos/authbridge/
# Should be owned by www-data with 755 permissions
```

### Issue 2: API Returns 404

**Symptoms:** API endpoints return 404 errors
**Solution:**
- Verify URL rewriting is enabled
- Try using full URL: `http://domain.com/app.php/api/auth/login`
- Check Apache/Nginx configuration for mod_rewrite

### Issue 3: Authentication Fails

**Symptoms:** Login requests return 401
**Solution:**
- Verify user exists in phpBB database
- Check phpBB error logs: `store/errors.log`
- Ensure phpBB database connection is working
- Test logging in through phpBB forum directly

### Issue 4: CORS Errors

**Symptoms:** Browser shows CORS errors
**Solution:**
- Update CORS headers in `api_controller.php`
- Add your domain to allowed origins
- May need to add OPTIONS request handler

### Issue 5: Session Expires Too Quickly

**Symptoms:** Users logged out frequently
**Solution:**
- Adjust phpBB session settings in ACP
- Navigate to "Server configuration" > "Cookie settings"
- Increase "Session length" value

## Post-Migration Checklist

- [ ] phpBB3 installed and accessible
- [ ] ASHATOS Authentication Bridge extension enabled
- [ ] ASHATAIServer configuration updated
- [ ] ASHATGoddess configuration updated (if used)
- [ ] Test registration endpoint works
- [ ] Test login endpoint works
- [ ] Test session validation works
- [ ] Test logout endpoint works
- [ ] ASHATAIServer can authenticate users
- [ ] Users can register new accounts
- [ ] Users can login to existing accounts
- [ ] Sessions persist correctly
- [ ] HTTPS configured (production)
- [ ] Rate limiting configured (production)
- [ ] Monitoring setup (production)
- [ ] AGP_CMS backed up
- [ ] Documentation updated

## Support

If you encounter issues during migration:

1. Check the troubleshooting section in `PHPBB3_INTEGRATION_GUIDE.md`
2. Review phpBB error logs
3. Check web server error logs
4. Open an issue on GitHub with:
   - Error messages
   - Configuration files (sensitive data removed)
   - Steps to reproduce
   - phpBB and PHP versions

## Additional Resources

- [phpBB3 Installation Guide](https://www.phpbb.com/support/docs/en/3.3/ug/quickstart/installing/)
- [phpBB3 Admin Guide](https://www.phpbb.com/support/docs/en/3.3/ug/)
- [ASHATOS phpBB3 Integration Guide](PHPBB3_INTEGRATION_GUIDE.md)
- [phpBB Community Forums](https://www.phpbb.com/community/)

---

**Last Updated:** 2024-11-12  
**Version:** 1.0.0
