# 🔧 RaOS Nginx Management on Ubuntu

## Overview

This guide explains how to configure Ubuntu Linux to allow RaOS (RaCore) to fully manage and control Nginx without requiring SuperAdmin terminal-level intervention. This enables RaOS to start, stop, restart, and reload Nginx automatically through its API without manual sudo password entry.

## Why This Is Safe

The configuration only grants specific, limited permissions:
- ✅ Start, stop, restart, reload Nginx service
- ✅ Check Nginx status
- ✅ Test Nginx configuration
- ❌ NO access to other system services
- ❌ NO root shell access
- ❌ NO ability to modify system files outside of normal permissions

This is accomplished using Linux sudoers policy, which is a standard security mechanism used by system administrators worldwide.

---

## Quick Setup (Recommended)

### Prerequisites
- Ubuntu 22.04 LTS or newer
- Nginx installed (`sudo apt install nginx`)
- RaOS user account (default: `racore`)

### One-Command Setup

```bash
# If your RaOS user is 'racore' (default):
sudo /home/racore/TheRaProject/setup/setup-nginx-permissions.sh

# If your RaOS user is different:
sudo /home/racore/TheRaProject/setup/setup-nginx-permissions.sh your-username
```

That's it! RaOS can now manage Nginx automatically.

### Verify Setup

Test that it works by running as your RaOS user:

```bash
# Switch to your RaOS user
su - racore

# Test Nginx management (should NOT ask for password)
sudo systemctl status nginx
sudo systemctl restart nginx
```

If these commands run without asking for a password, the setup is successful! ✅

---

## Manual Setup

If you prefer to configure manually or need to customize the setup:

### Step 1: Create Sudoers Configuration File

```bash
sudo nano /etc/sudoers.d/raos-nginx
```

Add the following content (replace `racore` with your RaOS username):

```sudoers
# Sudoers configuration for RaOS Nginx Management

# Allow racore user to manage Nginx service without password
racore ALL=(ALL) NOPASSWD: /usr/bin/systemctl start nginx
racore ALL=(ALL) NOPASSWD: /usr/bin/systemctl stop nginx
racore ALL=(ALL) NOPASSWD: /usr/bin/systemctl restart nginx
racore ALL=(ALL) NOPASSWD: /usr/bin/systemctl reload nginx
racore ALL=(ALL) NOPASSWD: /usr/bin/systemctl status nginx

# Allow racore user to test Nginx configuration
racore ALL=(ALL) NOPASSWD: /usr/sbin/nginx -t
racore ALL=(ALL) NOPASSWD: /usr/bin/nginx -t

# Legacy service command support (older systems)
racore ALL=(ALL) NOPASSWD: /usr/sbin/service nginx start
racore ALL=(ALL) NOPASSWD: /usr/sbin/service nginx stop
racore ALL=(ALL) NOPASSWD: /usr/sbin/service nginx restart
racore ALL=(ALL) NOPASSWD: /usr/sbin/service nginx reload
racore ALL=(ALL) NOPASSWD: /usr/sbin/service nginx status
```

### Step 2: Set Correct Permissions

```bash
sudo chmod 0440 /etc/sudoers.d/raos-nginx
```

**Important:** The file must be owned by root and have mode 0440 (read-only). This prevents unauthorized modification.

### Step 3: Validate Syntax

```bash
sudo visudo -c -f /etc/sudoers.d/raos-nginx
```

You should see: `parsed OK` ✅

If there are syntax errors, fix them before proceeding. An invalid sudoers file can lock you out of sudo!

### Step 4: Test Configuration

Switch to your RaOS user and test:

```bash
su - racore
sudo systemctl status nginx
```

If it works without asking for a password, you're done! ✅

---

## How RaOS Uses This Configuration

Once configured, RaOS can automatically manage Nginx through these API endpoints:

### Available API Endpoints

```http
POST /api/control/system/restart-apache
Authorization: Bearer <admin-token>
```

**Note:** The endpoint is named `restart-apache` for legacy reasons, but it actually manages Nginx.

This API allows administrators to:
- Restart Nginx after configuration changes
- Reload Nginx configuration without downtime
- Start/stop Nginx service
- Check Nginx status

### Authentication Required

All Nginx management endpoints require:
- Valid authentication token
- Admin or SuperAdmin role
- License validation (for non-SuperAdmin users)

### Example Usage

```javascript
// JavaScript example - calling from Control Panel
async function restartNginx() {
    const response = await fetch('/api/control/system/restart-apache', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authToken}`
        }
    });
    
    const result = await response.json();
    if (result.success) {
        console.log('Nginx restarted successfully!');
    } else {
        console.error('Failed to restart Nginx:', result.error);
    }
}
```

---

## Understanding the Security Model

### What Permissions Are Granted?

The sudoers configuration grants these **specific commands only**:

1. **Service Management (systemctl)**
   - `/usr/bin/systemctl start nginx`
   - `/usr/bin/systemctl stop nginx`
   - `/usr/bin/systemctl restart nginx`
   - `/usr/bin/systemctl reload nginx`
   - `/usr/bin/systemctl status nginx`

2. **Configuration Testing**
   - `/usr/sbin/nginx -t`
   - `/usr/bin/nginx -t`

3. **Legacy Service Command**
   - `/usr/sbin/service nginx {start|stop|restart|reload|status}`

### What Permissions Are NOT Granted?

- ❌ Cannot manage other services (Apache, MySQL, etc.)
- ❌ Cannot edit system configuration files
- ❌ Cannot install/remove packages
- ❌ Cannot modify user accounts
- ❌ Cannot access other users' files
- ❌ Cannot run arbitrary sudo commands

This is known as the **Principle of Least Privilege** - granting only the minimum permissions needed for the task.

---

## Troubleshooting

### "Permission denied" Error

**Problem:** RaOS still shows "Permission denied" when managing Nginx.

**Solutions:**

1. **Verify sudoers file exists and has correct permissions:**
   ```bash
   ls -l /etc/sudoers.d/raos-nginx
   # Should show: -r--r----- 1 root root
   ```

2. **Check username in sudoers file matches:**
   ```bash
   sudo cat /etc/sudoers.d/raos-nginx | grep "ALL=(ALL)"
   # Should show your RaOS username, not 'racore' if you use a different name
   ```

3. **Test manually:**
   ```bash
   su - racore
   sudo -n systemctl status nginx
   # Should work without password
   ```

4. **Check sudoers file syntax:**
   ```bash
   sudo visudo -c -f /etc/sudoers.d/raos-nginx
   ```

### "Nginx start requires sudo privileges"

**Problem:** RaOS shows this message even after setup.

**Solution:** Restart your RaOS service to pick up the new permissions:

```bash
sudo systemctl restart racore
```

### Commands Still Ask for Password

**Problem:** Commands still prompt for password after setup.

**Solutions:**

1. **Ensure you're testing as the correct user:**
   ```bash
   whoami
   # Should show: racore (or your RaOS username)
   ```

2. **Check if sudoers file is being read:**
   ```bash
   sudo -l -U racore
   # Should list the Nginx management commands
   ```

3. **Verify sudoers directory is included:**
   ```bash
   sudo grep "includedir" /etc/sudoers
   # Should show: @includedir /etc/sudoers.d
   ```

### Sudoers Syntax Error

**Problem:** You get a syntax error when validating the sudoers file.

**Solution:**

1. **Don't panic!** You haven't broken sudo yet (file isn't active if syntax is wrong)

2. **Fix the syntax:**
   ```bash
   sudo visudo -f /etc/sudoers.d/raos-nginx
   ```

3. **Common mistakes:**
   - Missing `NOPASSWD:` before command
   - Incorrect username
   - Wrong command path
   - Missing `ALL=(ALL)` before `NOPASSWD:`

---

## Advanced Configuration

### Multiple RaOS Users

If you have multiple users running RaOS instances:

```bash
# Run setup for each user
sudo ./setup/setup-nginx-permissions.sh user1
sudo ./setup/setup-nginx-permissions.sh user2
```

Or manually add to `/etc/sudoers.d/raos-nginx`:

```sudoers
# Allow multiple users to manage Nginx
user1 ALL=(ALL) NOPASSWD: /usr/bin/systemctl start nginx
user1 ALL=(ALL) NOPASSWD: /usr/bin/systemctl restart nginx
user2 ALL=(ALL) NOPASSWD: /usr/bin/systemctl start nginx
user2 ALL=(ALL) NOPASSWD: /usr/bin/systemctl restart nginx
```

### Using Polkit Instead of Sudoers

For more complex scenarios, you can use Polkit (PolicyKit):

```bash
# Create polkit rule
sudo nano /etc/polkit-1/rules.d/50-raos-nginx.rules
```

Add:

```javascript
// Allow racore user to manage Nginx without password
polkit.addRule(function(action, subject) {
    if (action.id == "org.freedesktop.systemd1.manage-units" &&
        action.lookup("unit") == "nginx.service" &&
        subject.user == "racore") {
        return polkit.Result.YES;
    }
});
```

**Note:** Polkit is more complex but offers finer-grained control.

### Logging Nginx Management Actions

To audit Nginx management actions:

```bash
# Add to sudoers file
Defaults log_output
Defaults!/usr/bin/systemctl log_output
```

View logs:

```bash
sudo cat /var/log/sudo.log
```

---

## Security Best Practices

1. **Never grant full sudo access** - Only grant specific commands needed
2. **Use service accounts** - Don't use your personal account for RaOS
3. **Enable audit logging** - Track when Nginx is managed
4. **Regular security reviews** - Periodically review `/etc/sudoers.d/`
5. **Principle of Least Privilege** - Grant minimum permissions necessary
6. **Monitor Nginx logs** - Watch for unusual restart patterns

---

## Uninstalling

To remove RaOS Nginx management permissions:

```bash
# Remove sudoers file
sudo rm /etc/sudoers.d/raos-nginx

# Verify removal
sudo -l -U racore
# Should no longer show Nginx management commands
```

---

## FAQ

### Q: Is this secure?

**A:** Yes! This uses standard Linux sudoers policy, which is designed for exactly this purpose. Major services like Docker, libvirt, and others use the same mechanism.

### Q: Can this be exploited?

**A:** The commands are very specific. An attacker with access to the RaOS user could only start/stop Nginx, not gain root access or modify system files.

### Q: Why not run RaOS as root?

**A:** **Never run applications as root!** This violates security best practices and creates a much larger attack surface. This approach is far safer.

### Q: Does this work on other Linux distributions?

**A:** Yes, mostly! The setup is the same for any distribution using systemd (Debian, Fedora, CentOS, Arch, etc.). Only the package manager and service names might differ slightly.

### Q: Can I grant permission for other services?

**A:** Yes! You can add similar rules for PHP-FPM, MySQL, etc. Just add them to the same sudoers file following the same pattern.

### Q: Will this persist after system updates?

**A:** Yes! Files in `/etc/sudoers.d/` are preserved during system updates.

---

## Additional Resources

- **RaOS Documentation:** [README.md](../README.md)
- **Linux Hosting Setup:** [LINUX_HOSTING_SETUP.md](../LINUX_HOSTING_SETUP.md)
- **Deployment Guide:** [DEPLOYMENT_GUIDE.md](../DEPLOYMENT_GUIDE.md)
- **Ubuntu Sudoers Manual:** `man sudoers`
- **Systemd Service Management:** `man systemctl`

---

## Getting Help

If you encounter issues:

1. Check the troubleshooting section above
2. Review RaOS logs: `sudo journalctl -u racore -n 100`
3. Check Nginx logs: `sudo tail -f /var/log/nginx/error.log`
4. Verify user permissions: `sudo -l -U racore`

---

**Last Updated:** 2025-01-09  
**RaOS Version:** v7.0.0+  
**Tested On:** Ubuntu 22.04 LTS, Ubuntu 24.04 LTS
