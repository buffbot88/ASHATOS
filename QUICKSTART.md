# 🚀 RaOS Quick Start Guide

This guide provides a simple, step-by-step process to get RaOS up and running quickly, perfect for tired developers or first-time users.

---

## Prerequisites

Before you begin, ensure you have:
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Git** - For cloning the repository
- **Optional: PHP 8.0+** - For CMS features (automatically detected on first run)

---

## 1. Clone the Repository

```bash
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject
```

---

## 2. Build the Project

```bash
cd RaCore
dotnet build
```

This will restore all NuGet dependencies and build the project.

---

## 3. Configure Environment (Optional)

RaOS works with sensible defaults out-of-the-box. If you need to customize:

**Default Configuration:**
- **RaCore Server Port:** 80 (standard HTTP port)
- **CMS API Port:** 8080 (auto-configured)
- **Default Admin:** username: `admin`, password: `admin123`

**Change Port (if needed):**

**Linux/Mac:**
```bash
export RACORE_PORT=8080
export CMS_ENVIRONMENT=Production
```

**Windows:**
```cmd
set RACORE_PORT=8080
set CMS_ENVIRONMENT=Production
```

**Note:** Port 80 requires administrator/root privileges. For non-privileged environments, use a higher port (e.g., 5000, 8080).

---

## 4. Start RaCore

```bash
dotnet run
```

**🎉 That's it!** On first run, RaCore will automatically:
- ✅ Detect PHP (if installed)
- ✅ Generate the CMS homepage
- ✅ Configure the web server
- ✅ Start all services

**Console Output:**
```
========================================
   RaCore Server Starting
========================================
Server URL: http://localhost:80
Control Panel: http://localhost/control
WebSocket: ws://localhost:80/ws
========================================
```

---

## 5. Access the Application

Open your browser to:
- **Homepage:** `http://localhost` (or `http://localhost:PORT` if you configured a custom port)
- **Control Panel:** `http://localhost/control`
- **API Documentation:** Available through the Control Panel

**Default Login:**
- Username: `admin`
- Password: `admin123`

**⚠️ Change the default password immediately in production!**

---

## 6. First-Time Setup

### SuperAdmin Access

On first run, you'll have access to the SuperAdmin Control Panel with three role-based access levels:

- **SuperAdmin:** Full control panel + user panel
- **Admin:** Admin control panel + user panel  
- **User:** User panel only

### Default Credentials

```
Username: admin
Password: admin123
Role: SuperAdmin
```

**🔒 Security Note:** Change these credentials immediately after first login!

---

## 7. Optional: Enable CMS Features

If you have PHP 8.0+ installed, RaCore will automatically:
- Generate a CMS homepage with SQLite database
- Start the PHP built-in server on port 8080
- Integrate with the Control Panel

Access the CMS at: `http://localhost:8080`

---

## Troubleshooting

### Port 80 Access Issues

If you get permission errors on port 80:

**Linux/Mac:**
```bash
# Use sudo (not recommended for development)
sudo dotnet run

# OR use a non-privileged port
export RACORE_PORT=5000
dotnet run
```

**Windows:**
- Run Command Prompt or PowerShell as Administrator
- Or set `RACORE_PORT` to a higher port number

### PHP Not Detected

CMS features require PHP 8.0+. Install it:

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install php8.2-cli php8.2-sqlite3
```

**macOS:**
```bash
brew install php
```

**Windows:**  
Download from https://windows.php.net/download/

---

## Next Steps

**🎓 Learn More:**
- 📖 **[README.md](README.md)** - Full documentation
- 🔧 **[Module Documentation](LegendaryCMS/README.md)** - CMS features
- 🐧 **[Linux Hosting Setup](LINUX_HOSTING_SETUP.md)** - Production deployment
- 🎮 **[Game Engine Guide](LegendaryGameEngine/README.md)** - Game development

**🛠️ Development:**
- **[Module Development Guide](MODULE_DEVELOPMENT_GUIDE.md)** - Build extensions
- **[API Documentation](docs/archive/phases/PHASE8_LEGENDARY_CMS.md)** - REST API reference
- **[Plugin Guide](LegendaryCMS/README.md#plugin-development)** - Create plugins

**🔐 Security:**
- Change default admin password
- Configure HTTPS for production
- Review [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md)

---

> **That's it! You're ready to develop or test RaOS.** 🎉  
> *Take a break — you've earned it!* 😴

---

## Quick Reference Commands

```bash
# Clone repository
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject/RaCore

# Build and run (development)
dotnet build
dotnet run

# Build for production (Linux)
cd ..
./build-linux-production.sh

# Configure custom port
export RACORE_PORT=8080    # Linux/Mac
set RACORE_PORT=8080       # Windows

# Access points
http://localhost           # Homepage
http://localhost/control   # Control Panel
http://localhost:8080      # CMS (if PHP installed)
```

---

**Need Help?** Check the [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md) for comprehensive guides.
