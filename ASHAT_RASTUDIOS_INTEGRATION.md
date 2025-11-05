# ASHAT Goddess + RaStudios Integration Guide

## 🎯 Overview

This guide explains how ASHAT Goddess (your AI desktop assistant) integrates with RaStudios (the game development IDE) to provide a seamless desktop development experience.

## ✨ What's New

**ASHAT Goddess now includes RaStudios integration!**

With this update, ASHAT can:
- 🎮 Launch RaStudios with voice commands
- 📝 Provide information about RaStudios features
- 🔍 Automatically locate RaStudios on your system
- 💬 Help you with RaStudios-related questions

## 🚀 Quick Start

### Option 1: Download the Complete Suite (Recommended)

The easiest way to get started is to download the **ASHAT + RaStudios Suite**:

1. Visit your ASHAT server downloads page: `/api/download/info`
2. Download the suite for your platform:
   - Windows: `ASHAT-RaStudios-Suite-Windows-x64.zip`
   - Linux: `ASHAT-RaStudios-Suite-Linux-x64.tar.gz`
3. Extract the package
4. Run ASHAT.exe (Windows) or ./ASHAT (Linux)
5. Tell ASHAT: "open RaStudios"

### Option 2: Install Separately

If you already have ASHAT or RaStudios installed:

1. Install ASHAT Goddess if you haven't already
2. Install RaStudios in one of the standard locations
3. Run ASHAT and use voice commands to launch RaStudios

## 🎤 Voice Commands

### Launching RaStudios

Tell ASHAT any of these commands:
- "Open RaStudios"
- "Launch RaStudios"
- "Start RaStudios"
- "Launch the studio"
- "Open the studio"
- "Run RaStudios"

ASHAT will automatically find and launch RaStudios for you.

### Getting Information

Ask ASHAT about RaStudios:
- "What is RaStudios?"
- "Tell me about RaStudios"
- "What can RaStudios do?"

ASHAT will explain RaStudios features and capabilities.

## 📂 Installation Paths

ASHAT automatically searches for RaStudios in these locations:

### Windows
1. Same directory as ASHAT: `.\RaStudios\RaStudios.exe`
2. Parent directory: `..\RaStudios\RaStudios.exe`
3. Program Files: `C:\Program Files\RaStudios\RaStudios.exe`
4. Program Files (x86): `C:\Program Files (x86)\RaStudios\RaStudios.exe`

### Linux
1. Same directory as ASHAT: `./RaStudios/RaStudios`
2. Parent directory: `../RaStudios/RaStudios`
3. System binary: `/usr/local/bin/RaStudios`
4. Optional software: `/opt/RaStudios/RaStudios`

### macOS
1. Same directory as ASHAT: `./RaStudios/RaStudios.app/Contents/MacOS/RaStudios`
2. Applications folder: `/Applications/RaStudios.app/Contents/MacOS/RaStudios`

## 🛠️ Manual Installation

If you want to install ASHAT and RaStudios separately:

### Installing ASHAT Goddess

```bash
# Download ASHAT
# From your ASHAT server: /api/download/ashat-desktop-[platform]

# Windows
./ASHAT.exe

# Linux
chmod +x ./ASHAT
./ASHAT

# macOS
chmod +x ./ASHAT
./ASHAT
```

### Installing RaStudios

#### Windows (WinForms Version)
```bash
cd RaStudios/RaStudios.WinForms
dotnet build
dotnet run
```

Or use the pre-built executable:
```bash
RaStudios\RaStudios.exe
```

#### Linux (Python Version)
```bash
cd RaStudios
pip install -r requirements.txt
python main.py
```

### Linking Them Together

Place RaStudios in one of the locations ASHAT searches (see Installation Paths above), or:

1. Create a directory structure:
   ```
   ASHAT-Suite/
   ├── ASHAT.exe (or ASHAT)
   └── RaStudios/
       └── RaStudios.exe (or RaStudios)
   ```

2. Run ASHAT from the ASHAT-Suite directory
3. ASHAT will find RaStudios in the subdirectory

## 📦 Building the Suite Yourself

To build both ASHAT and RaStudios together:

```bash
# Clone the repository
git clone https://github.com/buffbot88/ASHATOS.git
cd ASHATOS

# Build the complete suite
./build-ashat-goddess-with-rastudios.sh

# Find the packages in:
# wwwroot/downloads/goddess-with-rastudios/
```

This script:
- Builds ASHAT Goddess for Windows and Linux
- Builds RaStudios (WinForms for Windows, Python for Linux)
- Packages them together with launch scripts and documentation
- Creates ready-to-distribute ZIP/TAR.GZ files

## 🎮 Using the Suite

### First Time Setup

1. **Extract the package** to a location of your choice
2. **Run ASHAT** using the provided launcher:
   - Windows: Double-click `Launch-ASHAT.bat`
   - Linux: Run `./launch-ashat.sh`

3. **Talk to ASHAT**: 
   ```
   You: Hello ASHAT!
   ASHAT: Greetings, mortal! I am ASHAT, your divine companion. How may I assist you?
   
   You: Open RaStudios
   ASHAT: Opening RaStudios for you! The studio shall manifest shortly. 🎮✨
   ```

### Daily Workflow

```
1. Start ASHAT (your AI companion is always ready)
2. Ask ASHAT to launch RaStudios when needed
3. Use RaStudios for development
4. Ask ASHAT for coding help or questions
5. Switch between ASHAT and RaStudios seamlessly
```

## 💡 Tips and Tricks

### Multiple ASHAT Personalities

ASHAT supports different personality modes for different tasks:

- **Professional Mode**: For focused development work
- **Friendly Mode**: For casual interaction and help
- **Wise Mode**: For strategic decisions about your project

Tell ASHAT: "Switch to professional mode" when working on complex code.

### ASHAT + RaStudios Workflow

1. **Planning**: Ask ASHAT about best practices or architecture
2. **Launch**: Tell ASHAT to open RaStudios
3. **Development**: Use RaStudios IDE features
4. **Questions**: Ask ASHAT for help while developing
5. **Debug**: Get ASHAT's assistance with debugging

### Customizing Installation

You can customize where ASHAT looks for RaStudios by:
1. Installing RaStudios in a standard location
2. Creating a symbolic link to RaStudios
3. Ensuring the directory structure matches ASHAT's search paths

## 🐛 Troubleshooting

### ASHAT can't find RaStudios

**Problem**: ASHAT says "RaStudios executable not found"

**Solutions**:
1. Check that RaStudios is installed
2. Verify it's in one of the expected locations (see Installation Paths)
3. On Linux, ensure the executable has execute permissions: `chmod +x RaStudios`
4. Try installing the complete suite package instead

### RaStudios doesn't launch

**Problem**: ASHAT tries to launch but RaStudios doesn't open

**Solutions**:
1. Try launching RaStudios manually to see any error messages
2. On Windows, ensure .NET 9.0 runtime is installed
3. On Linux, ensure Python 3.10+ and dependencies are installed
4. Check console output for error messages
5. Verify antivirus isn't blocking the launch

### Voice commands not working

**Problem**: ASHAT doesn't respond to RaStudios commands

**Solutions**:
1. Try typing the command in the chat interface instead
2. Ensure you're using one of the supported command phrases
3. Check if ASHAT is in standalone mode (may need server connection)
4. Restart ASHAT and try again

## 🔧 Advanced Configuration

### Custom RaStudios Location

If you want to use a custom RaStudios location, you can:

1. Create a symbolic link from a standard location to your custom path
2. Modify the ASHAT source code to add your custom path
3. Use the suite package and keep the standard directory structure

### Server Connection

ASHAT works better when connected to the ASHAT server:

1. Start your ASHAT server (ASHATCore)
2. Configure ASHAT to connect to your server URL
3. Enjoy enhanced AI capabilities and RaStudios integration

## 📚 Features Breakdown

### ASHAT Goddess Features

- 👑 Animated Roman goddess character
- 🎤 Natural voice synthesis
- 🤖 AI coding assistance
- 💬 Multiple personality modes
- 🎮 RaStudios integration (NEW!)
- 🔌 Optional server connection

### RaStudios Features

- 🎨 Game development IDE
- 🖼️ Asset management
- 🔧 Server connection tools
- 🤖 AI code generation
- 🎮 Game client launcher
- 📊 Diagnostics and logs

### Integration Features

- ✅ Voice-activated RaStudios launch
- ✅ Automatic path detection
- ✅ Cross-platform support
- ✅ Seamless switching
- ✅ Integrated help system
- ✅ Complete suite packaging

## 🌐 Platform Support

| Feature | Windows | Linux | macOS |
|---------|---------|-------|-------|
| ASHAT Goddess | ✅ | ✅ | ✅ |
| RaStudios WinForms | ✅ | ❌ | ❌ |
| RaStudios Python | ✅ | ✅ | ✅ |
| Voice Commands | ✅ | ✅ | ✅ |
| Auto-Launch | ✅ | ✅ | ✅ |
| Suite Package | ✅ | ✅ | 🔄 |

Legend:
- ✅ Fully supported
- 🔄 In development
- ❌ Not available

## 📄 License

Both ASHAT Goddess and RaStudios are part of the ASHATOS project.

Copyright © 2025 AGP Studios, INC. All rights reserved.

## 🤝 Contributing

Found a bug or want to improve the integration? Contributions are welcome!

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📞 Support

Need help?

- Check this guide for common solutions
- Review the [ASHAT Goddess README](ASHATGoddessClient/README.md)
- Review the [RaStudios README](RaStudios/README.md)
- Report issues on GitHub

## 🎉 What's Next?

Future enhancements planned:
- 🔄 Bidirectional communication (RaStudios ↔ ASHAT)
- 🤖 ASHAT controlling RaStudios features directly
- 📊 Status updates from RaStudios to ASHAT
- 🎮 Launch specific RaStudios projects by name
- 💾 Session management and project switching
- 🔧 RaStudios configuration through ASHAT

---

**Welcome to the future of AI-assisted game development!** 🚀

With ASHAT Goddess and RaStudios working together, you have a powerful AI companion ready to help you build amazing games and applications. ASHAT is always by your side, and with a simple voice command, your development studio is at your fingertips.

*"The goddess awaits your command."* ✨
