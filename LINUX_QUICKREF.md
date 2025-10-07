# 🐧 Linux Deployment Quick Reference

**One-Page Cheat Sheet for RaCore on Ubuntu 22.04 LTS**

---

## 🚀 Quick Install (5 Minutes)

```bash
# 1. Install .NET 9.0
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y dotnet-sdk-9.0

# 2. Install PHP 8.2
sudo apt install -y php8.2-cli php8.2-fpm php8.2-sqlite3

# 3. Install Nginx
sudo apt install -y nginx

# 4. Clone and Build RaCore
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject
./build-linux-production.sh

# 5. Deploy
sudo mv publish-production /opt/racore
sudo cp /opt/racore/racore.service /etc/systemd/system/
sudo systemctl enable racore && sudo systemctl start racore
```

---

## 📦 Essential Commands

### Service Management
```bash
sudo systemctl start racore       # Start RaCore
sudo systemctl stop racore        # Stop RaCore
sudo systemctl restart racore     # Restart RaCore
sudo systemctl status racore      # Check status
```

### Logs
```bash
sudo journalctl -u racore -f     # Real-time RaCore logs
sudo tail -f /var/log/nginx/error.log  # Nginx errors
```

### Nginx
```bash
sudo systemctl restart nginx      # Restart Nginx
sudo nginx -t                     # Test config
```

### PHP-FPM
```bash
sudo systemctl restart php8.2-fpm # Restart PHP-FPM
sudo systemctl status php8.2-fpm  # Check status
```

---

## 🔧 Configuration Files

| Component | Config Location |
|-----------|----------------|
| RaCore Service | `/etc/systemd/system/racore.service` |
| Nginx | `/etc/nginx/sites-available/racore` |
| PHP-FPM | `/etc/php/8.2/fpm/pool.d/www.conf` |
| PHP.ini | `/etc/php/8.2/fpm/php.ini` |
| FTP | `/etc/vsftpd.conf` |

---

## 🌐 Default Ports

| Service | Port | URL |
|---------|------|-----|
| RaCore | 5000 | http://localhost:5000 |
| Nginx Proxy | 80 | http://your-domain.com |
| CMS | 8080 | http://localhost:8080 |
| FTP | 21 | ftp://your-server |

---

## 🔥 Firewall Setup

```bash
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 8080/tcp
sudo ufw allow 21/tcp
sudo ufw enable
```

---

## 🔐 SSL Setup (Let's Encrypt)

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com
sudo certbot renew --dry-run
```

---

## 📊 System Monitoring

```bash
htop                              # Resource usage
df -h                             # Disk space
free -h                           # Memory usage
ps aux | grep dotnet              # RaCore process
```

---

## 🔄 Update RaCore

```bash
cd /home/racore/TheRaProject
git pull
./build-linux-production.sh
sudo systemctl stop racore
sudo rm -rf /opt/racore/*
sudo cp -r publish-production/* /opt/racore/
sudo systemctl start racore
```

---

## 🆘 Troubleshooting

**RaCore won't start:**
```bash
sudo journalctl -u racore -n 50 --no-pager
sudo netstat -tlnp | grep :5000
```

**Port already in use:**
```bash
# Edit /etc/systemd/system/racore.service
# Change: Environment=RACORE_PORT=5001
sudo systemctl daemon-reload
sudo systemctl restart racore
```

**PHP not working:**
```bash
sudo systemctl restart php8.2-fpm
php -v
ls -la /run/php/php8.2-fpm.sock
```

**Database permissions:**
```bash
sudo chown -R racore:racore /opt/racore
sudo chmod 644 /opt/racore/*.sqlite
```

---

## 📚 Full Documentation

- **Complete Setup**: [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md)
- **CMS Guide**: [CMS_QUICKSTART.md](CMS_QUICKSTART.md)
- **General Docs**: [README.md](README.md)

---

**System Requirements:** Ubuntu 22.04 LTS | 8GB RAM | 80GB Storage | x64 CPU
