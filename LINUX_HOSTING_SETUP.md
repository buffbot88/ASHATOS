# 🐧 Linux Hosting Setup Guide for RaCore

## Should You Switch to Linux?

### **Yes! Here's Why:**

Linux is **strongly recommended** for hosting RaCore in production environments. Here are the key benefits:

#### Performance & Efficiency
- **Lower Resource Usage**: Linux uses ~500MB RAM vs Windows ~2GB for same workload
- **Better for 8GB RAM Systems**: More memory available for RaCore and modules
- **Faster I/O**: Better disk and network performance for database operations
- **No Bloat**: Minimal background processes mean more resources for your app

#### Cost & Licensing
- **Free**: No Windows Server licensing costs ($1000+ per server)
- **Open Source**: Complete transparency and community support
- **Long-Term Support**: Ubuntu LTS versions supported for 5+ years

#### Developer Experience
- **Industry Standard**: 96% of web servers run Linux
- **Better Package Management**: apt/yum for easy software installation
- **Native .NET Support**: .NET was designed with Linux in mind
- **SSH Access**: Secure remote management built-in

#### Security
- **Fewer Attack Vectors**: Smaller attack surface than Windows
- **Better Security Updates**: Faster patches and updates
- **Fine-Grained Permissions**: Better file and process isolation

### Recommended for Your Setup
**For a server with 8GB RAM and 80GB NVMe storage, Linux is perfect:**
- Ubuntu Server 22.04 LTS (most popular, best documented)
- Lightweight and efficient
- Excellent .NET 9 support
- Perfect for RaCore + Nginx + PHP + databases

---

## Quick Start: Ubuntu 22.04 LTS Setup

### System Requirements
- **OS**: Ubuntu 22.04 LTS (Jammy Jellyfish) - Server or Desktop
- **RAM**: 8GB (RaCore will use ~1-2GB, leaving plenty for system)
- **Storage**: 80GB NVMe (fast storage recommended for SQLite databases)
- **CPU**: x64 architecture (AMD64 or Intel 64-bit)

---

## Part 1: Initial System Setup

### Step 1: Update System
```bash
sudo apt update && sudo apt upgrade -y
```

### Step 2: Install Essential Tools
```bash
sudo apt install -y curl wget git unzip build-essential
```

### Step 3: Create RaCore User (Optional but Recommended)
```bash
# Create dedicated user for RaCore
sudo adduser racore
sudo usermod -aG sudo racore

# Switch to racore user
su - racore
```

---

## Part 2: Install .NET 9.0 Runtime

### Option A: Install .NET SDK (for development)
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Verify installation
dotnet --version
# Should show: 9.0.xxx
```

### Option B: Install .NET Runtime (for production - lighter)
```bash
# Add Microsoft package repository (if not done above)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET Runtime
sudo apt update
sudo apt install -y aspnetcore-runtime-9.0

# Verify installation
dotnet --list-runtimes
```

---

## Part 3: Install and Configure PHP 8+

### Step 1: Install PHP and Extensions
```bash
# Install PHP 8.2 with required extensions
sudo apt install -y php8.2-cli php8.2-fpm php8.2-sqlite3 php8.2-mbstring php8.2-xml php8.2-curl php8.2-zip

# Verify installation
php --version
# Should show: PHP 8.2.x
```

### Step 2: Configure PHP-FPM
```bash
# Edit PHP-FPM pool configuration
sudo nano /etc/php/8.2/fpm/pool.d/www.conf

# Key settings to verify/adjust:
# user = www-data
# group = www-data
# listen = /run/php/php8.2-fpm.sock
# listen.owner = www-data
# listen.group = www-data
```

### Step 3: Optimize PHP for RaCore
```bash
# Edit php.ini
sudo nano /etc/php/8.2/fpm/php.ini

# Recommended settings:
# memory_limit = 256M
# max_execution_time = 60
# upload_max_filesize = 50M
# post_max_size = 50M
# date.timezone = UTC
```

### Step 4: Start PHP-FPM
```bash
# Start and enable PHP-FPM
sudo systemctl start php8.2-fpm
sudo systemctl enable php8.2-fpm

# Check status
sudo systemctl status php8.2-fpm
```

---

## Part 4: Install and Configure Nginx

### Step 1: Install Nginx
```bash
# Install Nginx
sudo apt install -y nginx

# Verify installation
nginx -v
# Should show: nginx version: nginx/1.18.0 or higher
```

### Step 2: Configure Nginx for RaCore

Create RaCore site configuration:
```bash
sudo nano /etc/nginx/sites-available/racore
```

Add the following configuration:
```nginx
# RaCore Reverse Proxy Configuration
server {
    listen 80;
    listen [::]:80;
    server_name localhost your-domain.com;

    # Increase timeouts for WebSocket connections
    proxy_connect_timeout 7d;
    proxy_send_timeout 7d;
    proxy_read_timeout 7d;

    # Proxy to RaCore backend
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# CMS PHP Server Block
server {
    listen 8080;
    listen [::]:8080;
    server_name localhost;
    
    root /home/racore/TheRaProject/wwwroot;
    index index.php index.html;
    
    location / {
        try_files $uri $uri/ /index.php?$query_string;
    }
    
    location ~ \.php$ {
        fastcgi_pass unix:/run/php/php8.2-fpm.sock;
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
    }
    
    # Deny access to hidden files
    location ~ /\. {
        deny all;
    }
}
```

### Step 3: Enable Site and Start Nginx
```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/racore /etc/nginx/sites-enabled/

# Remove default site (optional)
sudo rm /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
sudo systemctl enable nginx

# Check status
sudo systemctl status nginx
```

---

## Part 5: Install and Configure FTP Server

### Step 1: Install vsftpd (Very Secure FTP Daemon)
```bash
sudo apt install -y vsftpd
```

### Step 2: Configure vsftpd
```bash
# Backup original config
sudo cp /etc/vsftpd.conf /etc/vsftpd.conf.backup

# Edit configuration
sudo nano /etc/vsftpd.conf
```

**Recommended vsftpd configuration:**
```conf
# Enable standalone mode
listen=YES
listen_ipv6=NO

# Allow anonymous FTP? (NO for security)
anonymous_enable=NO

# Allow local users to log in
local_enable=YES

# Enable write permissions
write_enable=YES

# Set local umask
local_umask=022

# Log file transfers
xferlog_enable=YES

# Activate directory messages
dirmessage_enable=YES

# Use local time
use_localtime=YES

# Enable passive mode
pasv_enable=YES
pasv_min_port=40000
pasv_max_port=50000

# Chroot users to their home directory
chroot_local_user=YES
allow_writeable_chroot=YES

# Restrict users to their home directories
user_sub_token=$USER
local_root=/home/$USER/ftp

# Security options
ssl_enable=NO  # Set to YES if you have SSL certificates
```

### Step 3: Create FTP Directory Structure
```bash
# Create FTP directory for racore user
sudo mkdir -p /home/racore/ftp/files
sudo chown nobody:nogroup /home/racore/ftp
sudo chmod a-w /home/racore/ftp
sudo chown racore:racore /home/racore/ftp/files

# Create symlink to RaCore project
ln -s /home/racore/TheRaProject /home/racore/ftp/files/racore
```

### Step 4: Start FTP Server
```bash
# Restart vsftpd
sudo systemctl restart vsftpd
sudo systemctl enable vsftpd

# Check status
sudo systemctl status vsftpd
```

### Step 5: Configure Firewall for FTP
```bash
# Allow FTP through firewall
sudo ufw allow 20/tcp
sudo ufw allow 21/tcp
sudo ufw allow 40000:50000/tcp
sudo ufw allow 80/tcp
sudo ufw allow 8080/tcp
sudo ufw reload
```

---

## Part 6: Deploy RaCore

### Step 1: Clone Repository
```bash
# Navigate to home directory
cd ~

# Clone RaCore
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject
```

### Step 2: Build RaCore
```bash
# Build the project
cd RaCore
dotnet build -c Release
```

### Step 3: Configure RaCore for Linux
```bash
# Set environment variables
export RACORE_PORT=5000
export ASPNETCORE_ENVIRONMENT=Production

# Optional: Add to ~/.bashrc for persistence
echo 'export RACORE_PORT=5000' >> ~/.bashrc
echo 'export ASPNETCORE_ENVIRONMENT=Production' >> ~/.bashrc
```

### Step 4: Run RaCore Manually (Testing)
```bash
# Run in development mode
cd ~/TheRaProject/RaCore
dotnet run

# Or run the built binary
cd ~/TheRaProject/RaCore/bin/Release/net9.0
dotnet RaCore.dll
```

You should see:
```
========================================
   RaCore First-Run Initialization
========================================

[FirstRunManager] Step 1/3: Spawning CMS Homepage...
✅ CMS Homepage generated successfully!
...
🚀 RaCore is running on http://localhost:5000
```

### Step 5: Set Up RaCore as a Systemd Service

Create a systemd service file:
```bash
sudo nano /etc/systemd/system/racore.service
```

Add the following configuration:
```ini
[Unit]
Description=RaCore AI Mainframe
After=network.target

[Service]
Type=notify
User=racore
Group=racore
WorkingDirectory=/home/racore/TheRaProject/RaCore
ExecStart=/usr/bin/dotnet /home/racore/TheRaProject/RaCore/bin/Release/net9.0/RaCore.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=racore
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=RACORE_PORT=5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### Step 6: Enable and Start RaCore Service
```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable RaCore to start on boot
sudo systemctl enable racore

# Start RaCore service
sudo systemctl start racore

# Check status
sudo systemctl status racore

# View logs
sudo journalctl -u racore -f
```

### Service Management Commands
```bash
# Start RaCore
sudo systemctl start racore

# Stop RaCore
sudo systemctl stop racore

# Restart RaCore
sudo systemctl restart racore

# View status
sudo systemctl status racore

# View real-time logs
sudo journalctl -u racore -f

# View last 100 log lines
sudo journalctl -u racore -n 100
```

---

## Part 7: Firewall Configuration

### Configure UFW (Uncomplicated Firewall)
```bash
# Install UFW if not already installed
sudo apt install -y ufw

# Allow SSH (IMPORTANT - do this first!)
sudo ufw allow ssh

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 8080/tcp

# Allow FTP
sudo ufw allow 20/tcp
sudo ufw allow 21/tcp
sudo ufw allow 40000:50000/tcp

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status verbose
```

---

## Part 8: SSL/TLS Configuration (Production)

### Using Let's Encrypt (Free SSL)

```bash
# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Get SSL certificate
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

# Test automatic renewal
sudo certbot renew --dry-run
```

Update Nginx configuration for HTTPS:
```nginx
server {
    listen 80;
    server_name your-domain.com www.your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name your-domain.com www.your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Part 9: Monitoring and Maintenance

### System Monitoring
```bash
# Check system resources
htop  # Install with: sudo apt install htop

# Check disk usage
df -h

# Check memory usage
free -h

# Check RaCore process
ps aux | grep dotnet
```

### Log Management
```bash
# RaCore logs
sudo journalctl -u racore -n 100

# Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# PHP-FPM logs
sudo tail -f /var/log/php8.2-fpm.log

# System logs
sudo tail -f /var/log/syslog
```

### Backup Strategy
```bash
# Create backup script
nano ~/backup-racore.sh
```

Add backup script:
```bash
#!/bin/bash
BACKUP_DIR="/home/racore/backups"
DATE=$(date +%Y%m%d_%H%M%S)

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup RaCore database
cp -r /home/racore/TheRaProject/RaCore/Data $BACKUP_DIR/data_$DATE

# Backup CMS database
cp /home/racore/TheRaProject/Databases/cms_database.sqlite $BACKUP_DIR/cms_$DATE.sqlite

# Backup configuration
cp -r /etc/nginx/sites-available $BACKUP_DIR/nginx_$DATE

# Remove backups older than 30 days
find $BACKUP_DIR -type f -mtime +30 -delete

echo "Backup completed: $DATE"
```

Make it executable and schedule:
```bash
chmod +x ~/backup-racore.sh

# Add to crontab (daily at 2 AM)
crontab -e
# Add: 0 2 * * * /home/racore/backup-racore.sh >> /home/racore/backup.log 2>&1
```

---

## Part 10: Performance Optimization

### Optimize for 8GB RAM System

#### 1. Adjust PHP-FPM Pool Settings
```bash
sudo nano /etc/php/8.2/fpm/pool.d/www.conf
```

Recommended settings for 8GB RAM:
```conf
pm = dynamic
pm.max_children = 20
pm.start_servers = 4
pm.min_spare_servers = 2
pm.max_spare_servers = 6
pm.max_requests = 500
```

#### 2. Configure Nginx Worker Processes
```bash
sudo nano /etc/nginx/nginx.conf
```

```nginx
# Set to number of CPU cores (check with: nproc)
worker_processes auto;
worker_connections 1024;
```

#### 3. Optimize RaCore Settings
Add to systemd service file:
```ini
Environment=DOTNET_gcServer=1
Environment=DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=6
```

---

## Troubleshooting

### RaCore Won't Start
```bash
# Check service status
sudo systemctl status racore

# View detailed logs
sudo journalctl -u racore -n 50 --no-pager

# Check if port is available
sudo netstat -tlnp | grep :5000

# Check .NET installation
dotnet --info
```

### Nginx Issues
```bash
# Test configuration
sudo nginx -t

# Check error logs
sudo tail -f /var/log/nginx/error.log

# Restart Nginx
sudo systemctl restart nginx
```

### PHP Not Working
```bash
# Check PHP-FPM status
sudo systemctl status php8.2-fpm

# Test PHP
php -v

# Check socket exists
ls -la /run/php/php8.2-fpm.sock
```

### FTP Connection Issues
```bash
# Check vsftpd status
sudo systemctl status vsftpd

# Check firewall
sudo ufw status

# Test FTP connection locally
ftp localhost
```

### Database Permission Issues
```bash
# Fix SQLite permissions
sudo chown racore:racore /home/racore/TheRaProject/Databases/cms_database.sqlite
sudo chmod 664 /home/racore/TheRaProject/Databases/cms_database.sqlite
```

---

## Performance Benchmarks

### Expected Performance on 8GB RAM / 80GB NVMe

**RaCore:**
- Memory Usage: 500MB - 1GB
- Startup Time: < 5 seconds
- Response Time: < 50ms (average)
- Concurrent Users: 100+ (with Nginx proxy)

**System Resources:**
- OS + Services: ~1.5GB RAM
- PHP-FPM: ~200MB RAM
- Nginx: ~50MB RAM
- Available for RaCore: ~6GB RAM
- NVMe I/O: 3000+ MB/s (excellent for SQLite)

---

## Security Checklist

- [ ] Change default admin password (`admin/admin123`)
- [ ] Configure firewall (UFW)
- [ ] Install SSL certificates (Let's Encrypt)
- [ ] Disable root SSH login
- [ ] Set up automatic security updates
- [ ] Configure fail2ban for brute force protection
- [ ] Regular backups configured
- [ ] Monitor logs regularly
- [ ] Keep system updated

---

## Quick Command Reference

### Service Management
```bash
# RaCore
sudo systemctl {start|stop|restart|status} racore

# Nginx
sudo systemctl {start|stop|restart|status} nginx

# PHP-FPM
sudo systemctl {start|stop|restart|status} php8.2-fpm

# FTP
sudo systemctl {start|stop|restart|status} vsftpd
```

### Logs
```bash
# RaCore
sudo journalctl -u racore -f

# Nginx
sudo tail -f /var/log/nginx/{access,error}.log

# System
sudo tail -f /var/log/syslog
```

### Updates
```bash
# System updates
sudo apt update && sudo apt upgrade -y

# RaCore updates
cd ~/TheRaProject && git pull && cd RaCore && dotnet build -c Release
sudo systemctl restart racore
```

---

## Support and Resources

- **RaCore Documentation**: `/home/racore/TheRaProject/README.md`
- **CMS Guide**: `/home/racore/TheRaProject/CMS_QUICKSTART.md`
- **Ubuntu Documentation**: https://help.ubuntu.com/
- **Nginx Documentation**: https://nginx.org/en/docs/
- **PHP Documentation**: https://www.php.net/docs.php
- **.NET on Linux**: https://learn.microsoft.com/en-us/dotnet/core/install/linux

---

**Last Updated**: 2025-01-09  
**RaCore Version**: v7.0.0  
**Target Platform**: Ubuntu 22.04 LTS x64 (8GB RAM / 80GB NVMe)
