# Ashat AI Coding Bot - Quick Start Guide

> **Ashat** (pronounced AH-SH-AHT) - The Face of RaOS, your intelligent AI coding bot

## 🎯 What is Ashat?

Ashat is now a **fully compatible AI coding bot** that can directly:
- **Edit source files** with automatic backups
- **Fix bugs** with automated detection and suggestions
- **Refactor code** for better quality and maintainability
- **Manage dependencies** (NuGet packages)
- **Create new modules** with guided workflows
- **Debug and analyze** code with intelligent analysis
- **Learn and assist** with RaOS architecture

**Key Principle:** Ashat works **WITH** you, not **FOR** you. She can now directly edit files but maintains approval-based workflow for safety.

## 🌟 NEW: Direct File Editing

Ashat can now directly edit files on the server - **no GitHub required!**

### Quick Examples

```bash
# Edit a file with guidance
ashat edit file /path/to/Module.cs

# Fix bugs automatically
ashat fix bug in /path/to/BuggyFile.cs

# Refactor code
ashat refactor /path/to/OldCode.cs

# Comprehensive analysis
ashat analyze code /path/to/file.cs
```

**👉 See [ASHAT_CODING_BOT_GUIDE.md](ASHAT_CODING_BOT_GUIDE.md) for complete documentation**

## ⚡ Getting Started

### 1. Access Ashat

Ashat is available immediately after starting RaCore:

```bash
cd RaCore
dotnet run
```

Once RaCore is running, use any of these commands to interact with Ashat.

### 2. Basic Commands

```bash
# Get help
ashat help

# List all available modules
ashat modules

# Get information about a specific module
ashat module info AICodeGen

# Ask a question
ashat ask How do I create a new module?

# Check status
ashat status
```

### 3. Start a Coding Session

The most powerful way to use Ashat is through guided sessions:

```bash
ashat start session yourname Create a weather forecast module
```

**What happens:**
1. Ashat analyzes your goal
2. Identifies relevant modules (e.g., AICodeGen, ModuleSpawner)
3. Asks clarifying questions
4. Creates an action plan
5. Waits for your approval

### 4. Continue the Conversation

```bash
ashat continue yourname It should fetch from OpenWeather API and cache results
```

**What happens:**
1. Ashat refines the action plan based on your input
2. Shows you exactly what will be done
3. Requests approval to proceed

### 5. Approve and Execute

```bash
ashat approve yourname
```

**What happens:**
1. Ashat executes the approved plan
2. Provides implementation guidance
3. Offers next steps

### 6. Complete the Session

```bash
ashat end session yourname
```

**What happens:**
1. Session summary is shown
2. Statistics are provided
3. Session is closed

## 🚀 NEW: Direct Coding Operations

### Fix Bugs Directly

```bash
# Scan for bugs
ashat fix bug in /path/to/file.cs

# View detected issues
bugdetect list

# Get fix suggestion
bugdetect fix <issueId>

# Apply fix
fileops replace "buggy code" with "fixed code" in /path/to/file.cs
```

### Edit Files Directly

```bash
# Read a file
fileops read /path/to/file.cs

# Replace text
fileops replace "OldClass" with "NewClass" in /path/to/file.cs

# Append content
fileops append /path/to/file.cs >> // New comment

# Restore from backup if needed
fileops restore /path/to/file.cs
```

### Refactor Code

```bash
# Rename symbols
refactor rename OldName to NewName in /path/to/file.cs

# Format code
refactor format /path/to/file.cs

# Organize code
refactor organize /path/to/file.cs
```

### Manage Dependencies

```bash
# List dependencies
deps list /path/to/project.csproj

# Add package
deps add Newtonsoft.Json to /path/to/project.csproj

# Update packages
deps update /path/to/project.csproj
```

## 📋 Common Use Cases

### Learn About the System

```bash
# Discover available modules
ashat modules

# Get details about a module
ashat module info Chat
ashat module info AICodeGen

# Ask questions
ashat ask What modules handle authentication?
ashat ask How does the Knowledge module work?
```

### Create a New Module

```bash
# Start the session
ashat start session dev001 Create a notification system module

# Provide details
ashat continue dev001 It should send email and SMS notifications

# Review the plan and approve
ashat approve dev001

# Complete when done
ashat end session dev001
```

### Debug an Issue

```bash
# Start debugging session
ashat start session debug001 Fix null reference in ChatModule

# Provide context
ashat continue debug001 Error occurs in JoinRoomAsync when room doesn't exist

# Review solution and approve
ashat approve debug001

# End session
ashat end session debug001
```

## 🔄 The Ashat Workflow

```
Start Session
    ↓
Requirements Gathering
    ↓
Create Action Plan
    ↓
Request Approval
    ↓
Approved? ───No───→ Revise Plan
    ↓ Yes
Execute Plan
    ↓
Review Results
    ↓
Complete Session
```

## 🎓 Integration Points

### Chat Support System
- Access Ashat through chat with `@ashat` or `ashat` prefix
- Real-time assistance during development
- Persistent sessions across chat rooms

### Dev Pages
- Context-sensitive help based on current page
- Module references and documentation
- Interactive guidance

## 💡 Pro Tips

1. **Be Specific**: Instead of "Create a module", try "Create a user notification module with email and SMS support"

2. **Iterate**: Don't worry about getting it perfect first time. Use `ashat continue` to refine.

3. **Ask Questions**: Use `ashat ask` before starting complex tasks to understand the system.

4. **Check Status**: Use `ashat status` to see active sessions and Ashat's state.

5. **Explore First**: Use `ashat modules` to discover what's already available.

## 🔧 Configuration

Ashat works out-of-the-box with no configuration required. However, she integrates with:

- **Speech Module**: For natural language AI responses
- **Chat Module**: For chat system integration
- **AICodeGen**: For code generation
- **Knowledge**: For documentation lookup

## 📚 Documentation

- **Full README**: [RaCore/Modules/Extensions/Ashat/README.md](RaCore/Modules/Extensions/Ashat/README.md)
- **Quickstart Guide**: [RaCore/Modules/Extensions/Ashat/QUICKSTART.md](RaCore/Modules/Extensions/Ashat/QUICKSTART.md)
- **Integration Guide**: [RaCore/Modules/Extensions/Ashat/INTEGRATION.md](RaCore/Modules/Extensions/Ashat/INTEGRATION.md)
- **Examples**: [RaCore/Modules/Extensions/Ashat/examples/](RaCore/Modules/Extensions/Ashat/examples/)

## 🎯 Example Session

Here's a complete example:

```bash
# Start
$ ashat start session alice Create a weather module

→ Ashat: Hello! I'm Ashat, your AI coding companion.
→ Ashat: I understand you want to: Create a weather module
→ Ashat: Relevant modules I found:
→   • AICodeGen - Generates code and project structures
→   • AIContent - Creates game assets and content
→ Ashat: What specific aspect would you like to focus on first?

# Continue
$ ashat continue alice Fetch from OpenWeather API every hour

→ Ashat: Based on your goal and input, here's my proposed action plan:
→ Steps:
→   1. Analyze requirements and identify module type
→   2. Generate module structure and boilerplate code
→   3. Review generated code with developer
→   4. Test and validate the module
→ Ashat: ⚠️ Before I proceed, I need your approval.

# Approve
$ ashat approve alice

→ Ashat: ✅ Action plan approved!
→ Ashat: Proceeding with execution...

# End
$ ashat end session alice

→ Ashat: === Session Summary ===
→ Goal: Create a weather module
→ Duration: 8.5 minutes
→ Status: Completed
→ Thank you for working with me! 🚀
```

## 🐛 Troubleshooting

### Ashat doesn't respond
- Verify RaCore is running
- Check command format: starts with `ashat`
- Try `ashat help`

### Session not found
- Ensure you started a session first: `ashat start session <userId> <goal>`
- Use correct userId in commands
- Check active sessions: `ashat status`

### Need to start over
```bash
ashat end session yourname
ashat start session yourname <new goal>
```

## 🎉 Next Steps

1. **Try it yourself**: Start with a simple session
2. **Explore modules**: Learn what's available with `ashat modules`
3. **Ask questions**: Use `ashat ask` to learn about RaOS
4. **Build something**: Create a real module with Ashat's guidance

## 🔗 Quick Command Reference

```bash
# Information
ashat help                          # Show all commands
ashat modules                       # List all modules
ashat module info <name>            # Module details
ashat ask <question>                # Ask anything
ashat status                        # Check status

# Sessions
ashat start session <user> <goal>   # Start session
ashat continue <user> <message>     # Continue conversation
ashat approve <user>                # Approve action plan
ashat reject <user> <reason>        # Reject and revise
ashat end session <user>            # End session
```

---

**Ready to start?**

```bash
ashat start session yourname Create something amazing!
```

*Welcome to AI-assisted development with Ashat, the Face of RaOS! 🚀*

---

## 📖 Related Documentation

- [Main README](README.md)
- [Module Development Guide](MODULE_DEVELOPMENT_GUIDE.md)
- [Architecture Guide](ARCHITECTURE.md)
- [Development Guide](DEVELOPMENT_GUIDE.md)

**Module Location**: `RaCore/Modules/Extensions/Ashat/`  
**Category**: Extensions  
**Status**: ✅ Production Ready  
**Version**: 1.0
