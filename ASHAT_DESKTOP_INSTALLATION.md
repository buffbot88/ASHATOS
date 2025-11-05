# ASHAT Desktop Assistant - Installation Guide

Quick guide to get ASHAT on your desktop in under 5 minutes!

## 🚀 Quick Install

### For Windows Users

1. **Download**
   - Visit http://your-server/downloads OR
   - Direct download: http://your-server/api/download/ashat-desktop-windows
   - File: `ASHATDesktopAssistant.exe` (~15 MB)

2. **Run**
   - Double-click `ASHATDesktopAssistant.exe`
   - No installation needed!
   - ASHAT will start immediately

3. **First Time Setup**
   ```
   You > help
   ASHAT > [Shows all available commands]
   
   You > speak Hello! I'm ASHAT!
   ASHAT > 🎤 Speaking: "Hello! I'm ASHAT!"
   
   You > personality friendly
   ASHAT > 👑 Personality changed to: friendly
   ```

### For Linux Users

1. **Download**
   - Visit http://your-server/downloads OR
   - Direct download: http://your-server/api/download/ashat-desktop-linux
   - File: `ASHATDesktopAssistant` (~15 MB)

2. **Make Executable**
   ```bash
   chmod +x ASHATDesktopAssistant
   ```

3. **Run**
   ```bash
   ./ASHATDesktopAssistant
   ```

### For macOS Users

1. **Download**
   - Visit http://your-server/downloads OR
   - Direct download: http://your-server/api/download/ashat-desktop-macos
   - File: `ASHATDesktopAssistant` (~15 MB)

2. **Make Executable**
   ```bash
   chmod +x ASHATDesktopAssistant
   ```

3. **Run**
   ```bash
   ./ASHATDesktopAssistant
   ```

4. **Security Note**
   - If macOS blocks the app, go to System Preferences > Security & Privacy
   - Click "Allow" for ASHATDesktopAssistant

## 🎯 Basic Usage

### Starting ASHAT

When you run ASHAT, you'll see:

```
    ╔═══════════════════════════════════════════════════════════╗
    ║                                                           ║
    ║           ✨ ASHAT Desktop Assistant ✨                   ║
    ║                                                           ║
    ║              Your AI Roman Goddess Companion              ║
    ║                                                           ║
    ╚═══════════════════════════════════════════════════════════╝

    🏛️  Inspired by the wisdom of ancient goddesses
    🎤 Soft feminine voice for pleasant interaction
    🤖 AI-powered coding and productivity assistant
    💫 Always on your desktop, ready to help

✨ ASHAT is now active on your desktop!
💡 Type 'help' for commands, 'exit' to quit

You >
```

### Essential Commands

```bash
# Get help
help

# Make ASHAT speak
speak Hello, I'm here to help!

# Change personality
personality friendly    # Warm and encouraging
personality professional # Focused and educational
personality playful     # Fun and creative
personality calm       # Patient and reassuring
personality wise       # Thoughtful and strategic

# Animations
animate wave          # Wave hello
animate bow           # Respectful bow
animate think         # Thinking pose
animate celebrate     # Celebration!

# Change appearance
theme athena         # Goddess of wisdom theme
theme diana          # Goddess of hunt theme
theme minerva        # Goddess of arts theme

# Voice control
voice                # Toggle voice on/off

# AI assistance
code How do I sort an array in Python?
code What's the difference between let and const?

# Exit
exit
```

## 🎨 Customization

### Personalities

| Personality | Best For | Style |
|------------|----------|-------|
| **Friendly** | Daily use | Warm, encouraging |
| **Professional** | Learning | Clear, focused |
| **Playful** | Brainstorming | Fun, creative |
| **Calm** | Stressful work | Patient, reassuring |
| **Wise** | Decision making | Thoughtful, strategic |

### Themes

| Theme | Description | Color Palette |
|-------|-------------|---------------|
| **Roman Goddess** | Classic elegance | Purple & Gold |
| **Athena** | Wisdom & strategy | Blue & Silver |
| **Diana** | Nature & hunt | Green & Silver |
| **Minerva** | Arts & intellect | Gold & White |

## 🔌 Server Connection

### Standalone Mode (Default)

ASHAT works offline with basic features:
- Voice synthesis
- Animations
- Basic conversations
- Personality changes
- Theme switching

### Connected Mode (Full Features)

When connected to ASHAT server:
- Full AI coding assistance
- Knowledge base access
- Advanced personality system
- Session persistence
- Multi-device sync

### Connecting to Server

1. Ensure ASHAT server is running:
   ```bash
   cd ASHATCore
   dotnet run
   ```

2. ASHAT Desktop will auto-connect if server is on localhost:80

3. For remote servers, start with:
   ```bash
   ./ASHATDesktopAssistant --server http://your-server:port
   ```

## 💡 Tips & Tricks

1. **Morning Greeting**
   ```bash
   speak Good morning! Ready for a productive day!
   animate wave
   ```

2. **Motivation Boost**
   ```bash
   personality coach
   speak You've got this! Keep going!
   animate celebrate
   ```

3. **Focus Mode**
   ```bash
   personality calm
   speak Let's focus. Take it one step at a time.
   ```

4. **Code Review**
   ```bash
   code Review my React component for best practices
   animate think
   ```

## 🐛 Troubleshooting

### ASHAT Won't Start

**Problem**: Double-clicking does nothing or shows error

**Solutions**:
- **Windows**: Run from Command Prompt to see errors
  ```cmd
  ASHATDesktopAssistant.exe
  ```
- **Linux/Mac**: Run from terminal to see errors
  ```bash
  ./ASHATDesktopAssistant
  ```
- Verify .NET 9.0 is installed (or use standalone build)

### Voice Not Working

**Problem**: No sound when using speak command

**Solutions**:
- Check system volume
- Toggle voice: type `voice`
- Verify speakers/headphones connected
- On Linux, install espeak: `sudo apt install espeak`

### Cannot Connect to Server

**Problem**: Shows "Running in standalone mode"

**Solutions**:
- This is normal! ASHAT works in standalone mode
- To connect: Ensure server is running at http://localhost:80
- Check firewall isn't blocking connection
- Use `status` command to see connection state

### Commands Not Working

**Problem**: Commands show "I don't understand"

**Solutions**:
- Type `help` to see all commands
- Check command spelling
- Some commands need server connection (shown in help)

## 📚 Next Steps

1. **Explore Commands**: Try all commands from `help`
2. **Customize**: Set your favorite personality and theme
3. **Chat**: Have a conversation with ASHAT
4. **Code Help**: Ask coding questions
5. **Make it Yours**: Find your perfect ASHAT setup

## 🆘 Support

Need help?
- 📖 [Full Documentation](ASHAT_DESKTOP_ASSISTANT.md)
- 💬 [Quick Start Guide](ASHAT_QUICKSTART.md)
- 🌐 Visit: http://your-server/support
- 📧 Email: support@agpstudios.online

---

**Welcome to ASHAT!** 👑

*Your AI Roman Goddess companion is ready to assist you on your desktop!*

✨ Enjoy wisdom, grace, and intelligence - right at your fingertips! ✨
