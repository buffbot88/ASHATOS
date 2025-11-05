# ASHAT Desktop Assistant

> Transform your desktop with ASHAT - Your AI Roman Goddess Companion

## 🌟 Overview

ASHAT Desktop Assistant is a downloadable personal AI assistant that appears on your desktop as an animated Roman goddess character. Inspired by classic desktop companions like BonziBuddy, ASHAT brings modern AI capabilities with a divine aesthetic.

## ✨ Features

### 👑 Roman Goddess Aesthetic
- **Animated Character**: Beautiful, animated Roman goddess that sits on your desktop
- **Multiple Themes**: Choose from Roman Goddess, Athena, Diana, Minerva, or Celestial Goddess
- **Rich Animations**: Wave, bow, think, celebrate, point, read, write, and meditate
- **Customizable Appearance**: Adjust size, position, and transparency

### 🎤 Soft Female Voice
- **Natural TTS**: Soft, feminine voice for all interactions
- **Multiple Voice Profiles**: Soft Female, Gentle Female, Wise Female, Energetic Female
- **Context-Aware Speech**: ASHAT speaks greetings, responses, and notifications
- **Voice Control**: Enable/disable voice as needed

### 🤖 AI Capabilities
- **Coding Assistant**: Get help with programming questions and debugging
- **Productivity Helper**: Task management, reminders, and workflow assistance
- **Knowledge Base**: Access to vast information and documentation
- **Context Awareness**: ASHAT remembers your conversations and preferences

### 💫 Personality System
- **Multiple Personalities**: Friendly, Professional, Playful, Calm, Wise
- **Emotional Intelligence**: Detects and responds to your emotions
- **Relationship Building**: Builds rapport over time based on interactions
- **Customizable Behavior**: Tailor ASHAT's responses to your preferences

### 🔌 Flexible Connectivity
- **Standalone Mode**: Works offline with core features
- **Server-Connected Mode**: Full AI capabilities when connected to ASHAT server
- **Automatic Fallback**: Seamlessly switches between modes

## 📥 Download

### Windows
- **File**: `ASHATDesktopAssistant.exe`
- **Size**: ~15 MB (self-contained)
- **Requirements**: Windows 10 or later, x64
- **Download**: [Get for Windows](http://localhost:80/api/download/ashat-desktop-windows)

### Linux
- **File**: `ASHATDesktopAssistant`
- **Size**: ~15 MB (self-contained)
- **Requirements**: Ubuntu 20.04+ or compatible, x64
- **Download**: [Get for Linux](http://localhost:80/api/download/ashat-desktop-linux)

### macOS
- **File**: `ASHATDesktopAssistant`
- **Size**: ~15 MB (self-contained)
- **Requirements**: macOS 11+ (Big Sur or later), Universal Binary
- **Download**: [Get for macOS](http://localhost:80/api/download/ashat-desktop-macos)

## 🚀 Quick Start

### Installation

**Windows:**
```bash
# Simply double-click ASHATDesktopAssistant.exe
# No installation required!
```

**Linux/macOS:**
```bash
# Make executable
chmod +x ASHATDesktopAssistant

# Run
./ASHATDesktopAssistant
```

### First Launch

When you first run ASHAT Desktop Assistant:

1. **Welcome Screen**: ASHAT greets you with a divine welcome
2. **Server Connection**: Attempts to connect to ASHAT server (optional)
3. **Standalone Mode**: Falls back to offline mode if server unavailable
4. **Ready to Help**: ASHAT appears and is ready for interaction!

### Basic Commands

```bash
# Get help
help

# Make ASHAT speak
speak Hello! I'm happy to help you today!

# Play animations
animate wave
animate bow
animate think

# Change personality
personality friendly
personality professional
personality playful

# Change theme
theme athena
theme diana

# Toggle voice
voice

# Ask coding questions
code How do I implement a binary search tree?

# Check status
status

# Exit
exit
```

## 🎮 Usage Examples

### Daily Productivity

```bash
You > speak Good morning! Let's have a productive day!
ASHAT > 🎤 Speaking: "Good morning! Let's have a productive day!"

You > personality coach
ASHAT > 👑 Personality changed to: coach

You > animate celebrate
ASHAT > 🎭 Animation: celebrate
```

### Coding Assistance

```bash
You > code What's the difference between == and === in JavaScript?
ASHAT > In JavaScript, == performs type coercion before comparison, 
        while === performs strict equality without type coercion...

You > animate think
ASHAT > 🎭 Animation: think
```

### Customization

```bash
You > theme minerva
ASHAT > ✨ Theme changed to: minerva

You > personality wise
ASHAT > 👑 Personality changed to: wise

You > speak Wisdom comes from experience and patience
ASHAT > 🎤 Speaking: "Wisdom comes from experience and patience"
```

## ⚙️ Configuration

### Server Connection

To connect to ASHAT server for advanced AI features:

1. Ensure ASHAT server is running (default: http://localhost:80)
2. ASHAT Desktop Assistant will auto-connect on startup
3. Green status indicator shows successful connection
4. Yellow indicator shows standalone mode

### Customization Options

Access via server commands (when connected):

```bash
# Through server module
desktop config theme roman_goddess
desktop config voice true
desktop config scale 1.5
desktop config alwaysontop true
desktop config transparency true
```

## 🎨 Themes

### Available Themes

1. **Roman Goddess** (Default)
   - Classic Roman deity aesthetic
   - Purple and gold colors
   - Elegant and timeless

2. **Athena**
   - Goddess of wisdom and warfare
   - Wise and strategic personality
   - Blue and silver colors

3. **Diana**
   - Goddess of the hunt and moon
   - Nature-focused aesthetic
   - Green and silver colors

4. **Minerva**
   - Goddess of wisdom and arts
   - Intellectual and creative
   - Gold and white colors

5. **Celestial Goddess**
   - Cosmic and mystical
   - Stars and nebulae
   - Purple and blue gradients

## 💡 Tips & Tricks

1. **Voice Profiles**: Experiment with different voice profiles to find your favorite
2. **Animations**: Use animations to add life to your workspace
3. **Personalities**: Match personality to your current task (calm for focus, coach for motivation)
4. **Server Mode**: Connect to server for full AI coding assistance
5. **Standalone Mode**: Works offline for basic features and companionship

## 🔧 Troubleshooting

### ASHAT Won't Start

1. Verify system requirements
2. Check .NET 9.0 is installed (or use standalone build)
3. Run from terminal to see error messages

### Voice Not Working

1. Toggle voice: `voice` command
2. Check system audio settings
3. Verify TTS is available on your system

### Cannot Connect to Server

1. Verify ASHAT server is running
2. Check server URL (default: http://localhost:80)
3. ASHAT will work in standalone mode if server unavailable

### Animations Not Showing

1. Character animations are console-based in current version
2. Future versions will include graphical overlays
3. Visual indicators show animation states

## 📚 Advanced Usage

### Server Integration

When connected to ASHAT server, you get access to:

- Full AI coding assistance
- Knowledge base queries
- Advanced personality system
- Relationship tracking
- Session persistence
- Multi-device sync

### Command Line Arguments

```bash
# Specify server URL
./ASHATDesktopAssistant --server http://your-server:port

# Disable voice on startup
./ASHATDesktopAssistant --no-voice

# Set theme
./ASHATDesktopAssistant --theme athena
```

## 🛠️ Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/buffbot88/ASHATOS.git
cd ASHATOS/ASHATDesktopClient

# Build
dotnet build

# Run
dotnet run

# Publish self-contained
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

### Contributing

Contributions are welcome! See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

## 📄 License

Copyright © 2025 AGP Studios, INC. All rights reserved.

See [LICENSE](../LICENSE) for details.

## 🌐 Links

- **Website**: http://agpstudios.online
- **Documentation**: [Full Documentation](../README.md)
- **Support**: support@agpstudios.online
- **Forum**: Coming soon
- **Discord**: Coming soon

## 🙏 Acknowledgments

Inspired by:
- BonziBuddy - Classic desktop companion
- Microsoft Clippy - Pioneering AI assistant
- Ancient Roman mythology - Aesthetic and personality
- Modern AI assistants - Capabilities and intelligence

## 📊 Version History

### Version 1.0.0 (2025-11-05)
- Initial release
- Roman goddess theme
- Multiple personalities
- Voice synthesis
- Animation system
- Server connectivity
- Standalone mode
- Cross-platform support

---

**Ready to transform your desktop?** Download ASHAT Desktop Assistant today and bring the wisdom of the goddesses to your workstation! ✨

🏛️ *"Wisdom, grace, and intelligence - right on your desktop"*
