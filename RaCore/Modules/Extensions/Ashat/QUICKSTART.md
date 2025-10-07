# Ashat AI Coding Assistant - Quickstart Guide

> Get started with Ashat, your AI coding companion, in under 5 minutes!

## 🚀 What is Ashat?

**Ashat** (pronounced AH-SH-AHT) is the "Face" of RaOS - an AI-powered coding assistant that helps you develop, debug, and learn about the RaOS system. Unlike traditional code generators, Ashat works **with** you through an approval-based workflow.

## ⚡ Quick Start

### 1. Access Ashat

Ashat is automatically available in RaOS. Simply use commands starting with `ashat`:

```bash
ashat help
```

### 2. Start Your First Session

Let's create a simple module:

```bash
ashat start session myuser Create a simple greeting module
```

**What happens:**
- Ashat analyzes your goal
- Identifies relevant modules
- Asks clarifying questions

### 3. Have a Conversation

Continue the conversation to refine your goal:

```bash
ashat continue myuser It should say hello to users by name
```

**What happens:**
- Ashat creates an action plan
- Shows you exactly what will be done
- Waits for your approval

### 4. Review and Approve

Once you're happy with the plan:

```bash
ashat approve myuser
```

**What happens:**
- Ashat executes the approved plan
- Provides progress updates
- Guides you through next steps

### 5. Complete the Session

When you're done:

```bash
ashat end session myuser
```

**What happens:**
- Session summary is generated
- Statistics are shown
- Session is closed

## 📚 Common Use Cases

### Learn About Modules

```bash
# List all available modules
ashat modules

# Get info about a specific module
ashat module info AICodeGen

# Ask questions
ashat ask What modules handle game logic?
```

### Get Coding Help

```bash
# Start a feature development session
ashat start session dev001 Add inventory system to game engine

# Debug an issue
ashat start session dev002 Fix null reference in ChatModule

# Learn a concept
ashat ask How does the Knowledge module work?
```

### Interactive Development

```bash
# Start
ashat start session dev003 Create a new authentication method

# Iterate
ashat continue dev003 Use JWT tokens for authentication
ashat continue dev003 Store tokens in secure cookies

# Approve when ready
ashat approve dev003

# Complete
ashat end session dev003
```

## 🎯 Key Features

### 1. No Automatic Changes
Ashat **never** modifies code without your explicit approval.

### 2. Educational Approach
Ashat explains **why**, not just **how**, helping you learn.

### 3. Module Knowledge
Ashat knows about all RaOS modules and their capabilities.

### 4. Context-Aware
Ashat remembers your conversation and adapts to your needs.

### 5. Flexible Workflow
Start, pause, continue, or abandon sessions at any time.

## 🔄 The Ashat Workflow

```
┌─────────────────┐
│  Start Session  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Understand    │
│  Requirements   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Create Action  │
│      Plan       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Request       │ ◄──────┐
│   Approval      │        │
└────────┬────────┘        │
         │                 │
         ▼                 │
    Approved?              │
    /       \              │
  Yes        No            │
   │          │            │
   ▼          ▼            │
Execute    Revise ─────────┘
   │
   ▼
Review & Complete
```

## 💡 Pro Tips

### Tip 1: Be Specific
Instead of: "Create a module"
Try: "Create a user notification module that sends emails and push notifications"

### Tip 2: Iterate
Don't worry about getting it perfect the first time. Ashat allows you to refine the plan.

### Tip 3: Ask Questions
Use `ashat ask` to learn about the system before starting complex tasks.

### Tip 4: Check Status
Use `ashat status` to see all active sessions and Ashat's state.

### Tip 5: Explore Modules
Use `ashat modules` to discover what's already available before building something new.

## 🔧 Integration

### Chat Support System
Ashat is integrated with the Chat Support system:
- Available when logged in
- Persistent sessions across chat rooms
- Real-time assistance

### Dev Pages
On Dev Pages, Ashat provides:
- Context-sensitive help
- Module references
- Code guidance

## 🎓 Learning Resources

### Within RaOS
```bash
# Get help
ashat help

# Learn about modules
ashat modules
ashat module info <name>

# Ask anything
ashat ask <question>
```

### Documentation
- Full README: `/RaCore/Modules/Extensions/Ashat/README.md`
- Module Development Guide: `/MODULE_DEVELOPMENT_GUIDE.md`
- Architecture: `/ARCHITECTURE.md`

## 🐛 Troubleshooting

### Ashat doesn't respond
- Check if Ashat module is loaded: `ashat status`
- Ensure command format is correct
- Try the help command: `ashat help`

### Session not found
- Make sure you started a session first
- Use the correct userId in commands
- Check active sessions: `ashat status`

### Need to start over
```bash
# End current session and start fresh
ashat end session myuser
ashat start session myuser <new goal>
```

## 📊 Example Session

Here's a complete example session:

```bash
# Start
$ ashat start session alice Create a weather module
→ Ashat: Identifies AICodeGen, AIContent modules as relevant
→ Ashat: Asks about data source and features

# Refine
$ ashat continue alice Fetch from OpenWeather API and cache for 1 hour
→ Ashat: Creates detailed action plan with 4 steps
→ Ashat: Requests approval

# Approve
$ ashat approve alice
→ Ashat: Executes plan
→ Ashat: Provides implementation guidance

# Complete
$ ashat end session alice
→ Ashat: Shows session summary
→ Ashat: Total duration: 15.3 minutes
```

## 🎉 Next Steps

Now that you know the basics:

1. **Try it yourself** - Start a simple session
2. **Explore modules** - Learn what's available
3. **Ask questions** - Use the interactive Q&A
4. **Build something** - Create a real module with guidance

## 🔗 Quick Reference Card

```
Session Management:
  ashat start session <userId> <goal>
  ashat continue <userId> <message>
  ashat approve <userId>
  ashat reject <userId> <reason>
  ashat end session <userId>

Information:
  ashat modules
  ashat module info <name>
  ashat ask <question>
  ashat status

Help:
  ashat help
```

---

**Ready to get started?**

```bash
ashat start session yourname Create something amazing!
```

Welcome to the future of AI-assisted development with Ashat! 🚀

---

*For complete documentation, see the [full README](README.md)*
