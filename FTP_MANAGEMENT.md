# 📁 FTP Server Management for RaOS

**Version:** 1.0  
**Module:** ServerSetup Extension  
**Platform:** Linux (vsftpd)

## Overview

The FTP Server Management Extension for RaOS provides an internal system for managing FTP server access and configuration. It allows Super Admins to connect directly to RaOS via FTP to manage directory files, with per-admin instance isolation and security.

## Features

- ✅ **Server Health Monitoring** - Check if live server is operational before FTP setup
- ✅ **FTP Server Status Monitoring** - Check if vsftpd is installed and running
- ✅ **Per-Admin Instance FTP Setup** - Isolated FTP access for each licensed admin
- ✅ **Automatic Directory Structure** - Creates FTP folders and symlinks to admin instances
- ✅ **Connection Info Retrieval** - Get FTP credentials and paths for any admin instance
- ✅ **Restricted FTP User Creation** - Step-by-step guide to create secure, restricted FTP users
- ✅ **Console and API Access** - Manage FTP through RaOS console commands
- ✅ **Linux System Integration** - Works with existing vsftpd installations
- ✅ **Security First Approach** - Live server check required before FTP setup

---

## Prerequisites

### System Requirements

- **Operating System:** Linux (Ubuntu, Debian, CentOS, etc.)
- **FTP Server:** vsftpd (Very Secure FTP Daemon)
- **Permissions:** Ability to create directories and symlinks

### Installation

If vsftpd is not installed, install it using:

```bash
# Ubuntu/Debian
sudo apt install vsftpd

# CentOS/RHEL
sudo yum install vsftpd

# Start and enable service
sudo systemctl start vsftpd
sudo systemctl enable vsftpd
```

For complete vsftpd configuration, see [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md).

---

## Console Commands

### Check Server Health

Check if the live server is operational and ready for FTP setup:

```
serversetup health
```

**Example Output:**
```
✅ Live server is operational and ready for FTP setup.
All essential folders are accessible and FTP server is running.
```

### Check FTP Status

Check if vsftpd is installed and running on the system:

```
serversetup ftp status
```

**Example Output:**
```
FTP Server Status:

✅ vsftpd is installed
   Version: vsftpd: version 3.0.5
   Status: ✅ Running
   Config: /etc/vsftpd.conf
```

### Setup FTP Access for Admin

Create FTP directory structure and symlinks for an admin instance:

```
serversetup ftp setup license=<license_number> username=<username>
```

**Example:**
```
serversetup ftp setup license=12345 username=johndoe
```

**What this does:**
- Creates `/path/to/RaCore/ftp/<license>.<username>/` directory
- Creates `files/` subdirectory within FTP folder
- Creates symlink from `files/admin/` to the admin instance folder
- Generates FTP configuration file with connection details

**Example Output:**
```
✅ FTP access configured for 12345.johndoe
FTP Path: /home/racore/TheRaProject/RaCore/ftp/12345.johndoe
Files Directory: /home/racore/TheRaProject/RaCore/ftp/12345.johndoe/files
Admin Instance: /home/racore/TheRaProject/RaCore/ftp/12345.johndoe/files/admin -> /home/racore/TheRaProject/RaCore/Admins/12345.johndoe
Use 'serversetup ftp info license=12345 username=johndoe' for connection details.
```

### Get FTP Connection Info

Retrieve connection information for an admin instance:

```
serversetup ftp info license=<license_number> username=<username>
```

**Example:**
```
serversetup ftp info license=12345 username=johndoe
```

**Example Output:**
```
FTP Connection Info for 12345.johndoe:

Host: 192.168.1.100
Port: 21
Username: racore (Linux system user)
Password: (use Linux system password)
Remote Path: /ftp/12345.johndoe/files

The 'admin' directory in FTP links to: /home/racore/TheRaProject/RaCore/Admins/12345.johndoe
Super Admins can directly manage files through FTP.
```

### Create Restricted FTP User (Enhanced Security)

Create a dedicated FTP user restricted to the RaOS folder for enhanced security:

```
serversetup ftp createuser username=<username> path=<restricted_path>
```

**Example:**
```
serversetup ftp createuser username=raos_ftp path=/home/racore/TheRaProject/RaCore
```

**What this does:**
- Provides step-by-step instructions to create a restricted FTP user
- User is restricted to the specified directory (chroot jail)
- User has no shell access (nologin shell)
- Enhances security by isolating FTP access to RaOS folder only

**Example Output:**
```
⚠️  Creating FTP user 'raos_ftp' restricted to: /home/racore/TheRaProject/RaCore

This requires root/sudo privileges. Please run the following commands:

1. Create the FTP user (no shell access):
   sudo useradd -m -d /home/racore/TheRaProject/RaCore -s /usr/sbin/nologin raos_ftp

2. Set password for the FTP user:
   sudo passwd raos_ftp

3. Set ownership of the restricted directory:
   sudo chown raos_ftp:raos_ftp /home/racore/TheRaProject/RaCore
   sudo chmod 755 /home/racore/TheRaProject/RaCore

4. Configure vsftpd to restrict user (edit /etc/vsftpd.conf):
   chroot_local_user=YES
   allow_writeable_chroot=YES
   user_sub_token=$USER
   local_root=/home/racore/TheRaProject/RaCore

5. Restart vsftpd:
   sudo systemctl restart vsftpd

⚠️  SECURITY NOTE: The FTP user will be restricted to the specified directory.
    This user will NOT have shell access and cannot navigate outside the restricted path.
```

---

## Workflow Example

### 0. Check Server Health (New - Recommended First Step)

Before setting up FTP, verify the server is operational:

```
serversetup health
```

### 1. Create Admin Instance

First, create an admin instance:

```
serversetup admin create license=12345 username=johndoe
```

### 2. Setup FTP Access

Configure FTP access for the admin:

```
serversetup ftp setup license=12345 username=johndoe
```

### 3. Get Connection Info

Retrieve FTP connection details:

```
serversetup ftp info license=12345 username=johndoe
```

### 4. (Optional) Create Restricted FTP User

For enhanced security, create a dedicated FTP user restricted to RaOS:

```
serversetup ftp createuser username=raos_ftp path=/home/racore/TheRaProject/RaCore
```

Follow the provided instructions to complete the setup.

### 5. Connect via FTP Client

Use an FTP client (FileZilla, WinSCP, etc.) to connect:

- **Host:** Server IP or hostname
- **Port:** 21
- **Username:** Linux system user (e.g., `racore`)
- **Password:** Linux system password
- **Path:** `/ftp/12345.johndoe/files`

### 5. Manage Files

Once connected:
- Navigate to the `admin/` directory (symlinked to admin instance)
- Manage files in:
  - `admin/Databases/` - SQLite databases
  - `admin/wwwroot/` - Web files
  - `admin/documents/` - Admin documents
  - `admin/php.ini` - PHP configuration
  - `admin/nginx.conf` - Nginx configuration

---

## Directory Structure

### FTP Root Structure

```
RaCore/
├── ftp/                              # FTP root directory
│   └── <license>.<username>/         # Per-admin FTP directory
│       ├── files/                    # FTP accessible files directory
│       │   └── admin/               # Symlink to admin instance
│       │       ├── Databases/       # Database files
│       │       ├── wwwroot/         # Web files
│       │       ├── documents/       # Documents
│       │       ├── php.ini          # PHP config
│       │       └── nginx.conf       # Nginx config
│       └── ftp-config.txt           # FTP configuration info
└── Admins/                          # Admin instances (real location)
    └── <license>.<username>/        # Admin instance folder
```

### Security Model

1. **Linux User Authentication** - FTP uses Linux system users for authentication
2. **Per-Admin Isolation** - Each admin has their own FTP directory
3. **Symlink Access** - Admin files accessible via symlinks (no duplication)
4. **Read/Write Control** - Permissions controlled by Linux file system
5. **Chroot Jail** - vsftpd can be configured to jail users to their home directory

---

## API Integration

The FTP management functionality is exposed through the `IServerSetupModule` interface:

### Interface Methods

```csharp
public interface IServerSetupModule
{
    /// <summary>
    /// Get FTP server status (checks if vsftpd is installed and running on Linux)
    /// </summary>
    Task<FtpStatusResult> GetFtpStatusAsync();
    
    /// <summary>
    /// Setup FTP access for an admin instance
    /// </summary>
    Task<SetupResult> SetupFtpAccessAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Get FTP connection info for an admin
    /// </summary>
    Task<FtpConnectionInfo> GetFtpConnectionInfoAsync(string licenseNumber, string username);
    
    /// <summary>
    /// Create a restricted FTP user for RaOS folder access
    /// </summary>
    Task<SetupResult> CreateRestrictedFtpUserAsync(string username, string restrictedPath);
    
    /// <summary>
    /// Check if the live server is operational and ready for FTP setup
    /// </summary>
    Task<ServerHealthResult> CheckLiveServerHealthAsync();
}
```

### Result Models

```csharp
public class FtpStatusResult
{
    public bool IsInstalled { get; set; }
    public bool IsRunning { get; set; }
    public bool IsLinux { get; set; }
    public string? Version { get; set; }
    public string Message { get; set; }
    public string? ConfigPath { get; set; }
    public Dictionary<string, string> Details { get; set; }
}

public class FtpConnectionInfo
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 21;
    public string? Username { get; set; }
    public string? FtpPath { get; set; }
    public Dictionary<string, string> Details { get; set; }
}

public class ServerHealthResult
{
    public bool IsOperational { get; set; }
    public string Message { get; set; }
    public bool DatabasesAccessible { get; set; }
    public bool PhpFolderAccessible { get; set; }
    public bool AdminsFolderAccessible { get; set; }
    public bool FtpFolderAccessible { get; set; }
    public List<string> Issues { get; set; }
    public Dictionary<string, string> Details { get; set; }
}
```

### Usage Example

```csharp
var module = new ServerSetupModule();
module.Initialize(null);

// Check server health before FTP setup
var health = await module.CheckLiveServerHealthAsync();
if (!health.IsOperational)
{
    Console.WriteLine($"Server issues detected: {health.Message}");
    return;
}

// Check FTP status
var status = await module.GetFtpStatusAsync();
if (status.IsInstalled && status.IsRunning)
{
    Console.WriteLine("FTP server is ready!");
}

// Setup FTP for an admin
var setupResult = await module.SetupFtpAccessAsync("12345", "johndoe");
if (setupResult.Success)
{
    Console.WriteLine($"FTP configured at: {setupResult.Path}");
}

// Get connection info
var connInfo = await module.GetFtpConnectionInfoAsync("12345", "johndoe");
if (connInfo.Success)
{
    Console.WriteLine($"Connect to: {connInfo.Host}:{connInfo.Port}");
    Console.WriteLine($"Path: {connInfo.FtpPath}");
}

// Create restricted FTP user (provides instructions)
var userResult = await module.CreateRestrictedFtpUserAsync("raos_ftp", "/path/to/raos");
if (userResult.Success)
{
    Console.WriteLine(userResult.Message); // Displays setup instructions
}
```

---

## Security Considerations

### Authentication

- FTP uses **Linux system user authentication**
- Super Admins must have valid Linux user accounts
- Passwords are managed by the Linux system (not RaOS)

### Access Control

1. **File System Permissions** - Uses Linux file permissions (owner, group, other)
2. **Chroot Configuration** - vsftpd can jail users to specific directories
3. **Symlink Security** - Symlinks provide access without file duplication
4. **Per-Admin Isolation** - Each admin has separate FTP directory

### Best Practices

- ✅ Use strong passwords for Linux system users
- ✅ Configure vsftpd with `chroot_local_user=YES` for isolation
- ✅ Enable SSL/TLS in vsftpd for encrypted connections
- ✅ Set appropriate file permissions (644 for files, 755 for directories)
- ✅ Monitor FTP logs for suspicious activity
- ✅ Regularly update vsftpd to latest version

### Recommended vsftpd Configuration

```conf
# Security settings
anonymous_enable=NO
local_enable=YES
write_enable=YES
chroot_local_user=YES
allow_writeable_chroot=YES

# SSL/TLS (recommended)
ssl_enable=YES
rsa_cert_file=/etc/ssl/certs/vsftpd.pem
rsa_private_key_file=/etc/ssl/private/vsftpd.key

# Restrict to home directory
user_sub_token=$USER
local_root=/home/$USER/ftp

# Passive mode ports
pasv_enable=YES
pasv_min_port=40000
pasv_max_port=50000
```

---

## Troubleshooting

### FTP Server Not Installed

**Issue:** `serversetup ftp status` shows "vsftpd is not installed"

**Solution:**
```bash
sudo apt install vsftpd
sudo systemctl start vsftpd
sudo systemctl enable vsftpd
```

### FTP Server Not Running

**Issue:** vsftpd is installed but not running

**Solution:**
```bash
# Start the service
sudo systemctl start vsftpd

# Check status
sudo systemctl status vsftpd

# Check logs
sudo journalctl -u vsftpd -n 50
```

### Cannot Connect via FTP

**Issue:** FTP client cannot connect to server

**Solutions:**

1. **Check Firewall:**
```bash
sudo ufw allow 21/tcp
sudo ufw allow 40000:50000/tcp
sudo ufw reload
```

2. **Check vsftpd is running:**
```bash
sudo systemctl status vsftpd
```

3. **Test local connection:**
```bash
ftp localhost
```

4. **Check vsftpd logs:**
```bash
sudo tail -f /var/log/vsftpd.log
```

### Permission Denied

**Issue:** Cannot write files via FTP

**Solution:**
```bash
# Fix permissions on FTP directory
sudo chown -R racore:racore /path/to/ftp/directory
sudo chmod -R 755 /path/to/ftp/directory
```

### Symlink Not Working

**Issue:** Symlink to admin instance not visible or accessible

**Solution:**
```bash
# Verify symlink exists
ls -la /path/to/ftp/<license>.<username>/files/

# Recreate symlink if needed
ln -sf /path/to/Admins/<license>.<username> /path/to/ftp/<license>.<username>/files/admin

# Check vsftpd allows symlinks
grep "allow_writeable_chroot" /etc/vsftpd.conf
# Should be: allow_writeable_chroot=YES
```

---

## Platform Support

| Platform | Support Level | Notes |
|----------|--------------|-------|
| Linux | ✅ Full Support | Requires vsftpd installation |
| Windows | ❌ Not Supported | Use Windows native file sharing instead |
| macOS | ⚠️ Partial | Would require macOS FTP server setup |

---

## Related Documentation

- [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md) - Complete Linux server setup including vsftpd
- [LINUX_QUICKREF.md](LINUX_QUICKREF.md) - Quick reference for Linux deployment
- [SERVERSETUP_IMPLEMENTATION.md](docs/archive/summaries/SERVERSETUP_IMPLEMENTATION.md) - ServerSetup module details

---

## Change Log

### Version 1.0 (Current)
- ✅ Initial FTP management implementation
- ✅ vsftpd status checking
- ✅ Per-admin instance FTP setup
- ✅ FTP connection info retrieval
- ✅ Console command interface
- ✅ Automatic directory structure creation
- ✅ Symlink support for admin instances

### Planned Features
- 🔄 FTP user management (create/delete FTP users)
- 🔄 FTP quota management per admin
- 🔄 FTP activity logging and monitoring
- 🔄 Automatic SSL/TLS certificate setup
- 🔄 Web-based FTP file browser
- 🔄 Integration with RaOS web control panel

---

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review vsftpd logs: `/var/log/vsftpd.log`
3. Test FTP locally: `ftp localhost`
4. Open an issue on GitHub

---

**Generated by RaOS ServerSetup Module**  
**Documentation Version:** 1.0  
**Last Updated:** 2024
