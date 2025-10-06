# 🌟 RaCore AI Mainframe Phase Roadmap

---

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Optional: PHP 8+ for CMS features
- Optional: Apache for production CMS deployment

### Running RaCore

```bash
# Clone the repository
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject

# Build and run
cd RaCore
dotnet build
dotnet run
```

### Default Configuration
- **RaCore Server Port:** 5000 (configurable via `RACORE_PORT` environment variable)
- **CMS PHP Server Port:** 8080 (auto-started on first run)
- **Default Admin:** username: `admin`, password: `admin123` ⚠️ **Change this immediately!**

### Custom Port Configuration

**Linux/Mac:**
```bash
export RACORE_PORT=8080
dotnet run
```

**Windows:**
```cmd
set RACORE_PORT=8080
dotnet run
```

### First Run
On first run, RaCore automatically:
1. Creates a CMS with integrated Control Panel
2. Initializes SQLite database
3. Starts PHP development server (port 8080)
4. Creates default admin user

**Default Entry Points:**
- **Main CMS Homepage:** `http://localhost:5000/` (redirects to CMS on port 8080)
- **CMS Direct Access:** `http://localhost:8080/`
- **Control Panel:** `http://localhost:5000/control-panel.html`

The CMS homepage provides navigation to:
- **Home** - Landing/welcome page
- **Blogs** - Share thoughts and stories
- **Forums** - Community discussions
- **Chat** - Real-time chat room
- **Social** - MySpace-like user profiles
- **Settings** - Control Panel for admin/user settings

For more details, see [FIRST_RUN_INITIALIZATION.md](FIRST_RUN_INITIALIZATION.md)

---

## 🧩 **Phase 2: Modular Expansion** ✅ **COMPLETED**
- ✅ Add dynamic plugin/module discovery
- ✅ Support extensions (skills, planners, executors, etc)
- ✅ SQLite-backed persistent module memory
- ✅ Robust diagnostics & error handling

---

## 🎨 **Phase 3: Advanced Features & Extension Development** ✅ **COMPLETED**
- ✅ WebSocket integration for real-time communication
- ✅ User authentication & authorization system (PBKDF2, session management, RBAC)
- ✅ **License Management System** - Subscription-based access control
- ✅ CMS generation & deployment (PHP 8+ with SQLite, Apache integration)
- ✅ Advanced routing & async module invocation
- ✅ Safety & ethics modules (consent registry, ethics guard, risk scoring)
- ✅ Skills, Planning, and Execution pipeline
- ✅ First-run auto-initialization system
- ✅ Comprehensive security architecture
- ✅ **AI Code Generation Module** - Natural language game creation (MMORPG, RPG, FPS, etc.)
- ✅ Sales page integration for license purchases
- ✅ Spreadsheet, image, asset intake (for game & content modules)
- ✅ Patch manager & continuous backend updates

---

## 🚀 **Phase 4: Public Release Preparation** 🔄 **IN PROGRESS**
- ✅ License validation & access enforcement
- ✅ Complete AI content generation system
- ✅ Distribution system for authorized copies
- ✅ Update delivery from mainframe
- ✅ Multi-platform game client generation
- 🔜 Multi-tenant support & mainframe access control

---

RaCore v3+ will set the tone for public release for $20 per requested copy (no download link will be supplied for RaCore) and agreement to not remove licenses coding (Ra will check and then ban).

You can turn any old computer into RaAI or use RaAI client to access RaAI's mainframe IF Ra permits you to enter, else if you don't pass the vibe check, you get banned from the MainFrame.

**Future Licenses:** $20 for 1 Year WITH updates from the Mainframe

---

**Last Updated:** 2025-10-06  
**Current Version:** Phase 4.4 (Completed)
