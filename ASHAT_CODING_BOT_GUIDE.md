# ASHAT AI Coding Bot - Complete Guide

## Overview

ASHAT (pronounced AH-SH-AHT) is now a **fully compatible AI coding bot** that can directly edit files, fix bugs, refactor code, and manage dependencies on the server - **without requiring GitHub or external version control**.

## 🌟 New Capabilities

### Direct File Operations
ASHAT can now directly manipulate files with automatic safety backups:

- ✅ Read, write, and append to files
- ✅ Find and replace text in files
- ✅ Automatic backups before any modification
- ✅ Restore files from backups
- ✅ List and explore directory structures

### Bug Detection & Fixing
ASHAT automatically detects and helps fix common bugs:

- ✅ Null reference detection
- ✅ SQL injection vulnerabilities
- ✅ Hard-coded credentials
- ✅ Missing async/await patterns
- ✅ Resource disposal issues
- ✅ Empty catch blocks
- ✅ String comparison issues
- ✅ Division by zero risks

### Code Refactoring
ASHAT improves code quality through refactoring:

- ✅ Rename symbols (classes, methods, variables)
- ✅ Format and standardize code style
- ✅ Simplify complex code patterns
- ✅ Organize using statements
- ✅ Remove code smells

### Dependency Management
ASHAT manages NuGet packages and dependencies:

- ✅ List project dependencies
- ✅ Add/remove NuGet packages
- ✅ Update packages to latest versions
- ✅ Restore package dependencies
- ✅ Check for available updates

### Git Operations (Optional)
For local version control tracking:

- ✅ Git status and diff
- ✅ Branch management
- ✅ Commit changes
- ✅ View history
- ✅ Working directory management

## 🚀 Quick Start

### 1. Edit a File Directly

```bash
# View and prepare to edit a file
ashat edit file /path/to/MyModule.cs

# Then use FileOperations module to make changes
fileops replace OldMethod with NewMethod in /path/to/MyModule.cs
```

### 2. Fix Bugs Automatically

```bash
# Scan a file for bugs
ashat fix bug in /path/to/BuggyFile.cs

# Get fix suggestions
bugdetect fix <issueId>

# Apply the fix
fileops replace <old_code> with <fixed_code> in /path/to/BuggyFile.cs
```

### 3. Refactor Code

```bash
# Analyze code for refactoring opportunities
ashat refactor /path/to/ComplexFile.cs

# Rename a symbol
refactor rename OldClassName to NewClassName in /path/to/file.cs

# Format code
refactor format /path/to/file.cs

# Organize code structure
refactor organize /path/to/file.cs
```

### 4. Analyze Code Comprehensively

```bash
# Complete code analysis (file info + bug detection)
ashat analyze code /path/to/file.cs
```

### 5. Manage Dependencies

```bash
# List current dependencies
deps list /path/to/project.csproj

# Add a new package
deps add Newtonsoft.Json to /path/to/project.csproj

# Remove a package
deps remove OldPackage from /path/to/project.csproj

# Update all packages
deps update /path/to/project.csproj

# Restore packages
deps restore /path/to/project.csproj

# Check for updates
deps check /path/to/project.csproj
```

## 📋 Complete Command Reference

### ASHAT Direct Operations

| Command | Description |
|---------|-------------|
| `ashat edit file <path>` | Initiate file editing with guidance |
| `ashat fix bug in <path>` | Scan and provide bug fixes |
| `ashat refactor <path>` | Get refactoring suggestions |
| `ashat analyze code <path>` | Comprehensive code analysis |
| `ashat start session <userId> <goal>` | Start guided coding session |
| `ashat status` | View ASHAT status |
| `ashat modules` | List all available modules |

### File Operations Module

| Command | Description |
|---------|-------------|
| `fileops read <path>` | Read file contents |
| `fileops write <path> >> <content>` | Write to file (overwrites) |
| `fileops append <path> >> <content>` | Append to file |
| `fileops replace <old> with <new> in <path>` | Replace text in file |
| `fileops backup <path>` | Create manual backup |
| `fileops restore <path>` | Restore from backup |
| `fileops exists <path>` | Check if file exists |
| `fileops list <directory>` | List files in directory |
| `fileops status` | Show backup status |

### Bug Detector Module

| Command | Description |
|---------|-------------|
| `bugdetect scan <file>` | Scan file for bugs |
| `bugdetect scandir <directory>` | Scan entire directory |
| `bugdetect list` | List all detected issues |
| `bugdetect fix <issueId>` | Get fix suggestion |
| `bugdetect clear` | Clear issues list |
| `bugdetect patterns` | Show detection patterns |

### Refactoring Module

| Command | Description |
|---------|-------------|
| `refactor rename <old> to <new> in <path>` | Rename symbol |
| `refactor format <path>` | Format code style |
| `refactor simplify <path>` | Get simplification suggestions |
| `refactor organize <path>` | Organize code structure |
| `refactor history` | View refactoring history |

### Dependency Manager Module

| Command | Description |
|---------|-------------|
| `deps list <project>` | List dependencies |
| `deps add <package> to <project>` | Add NuGet package |
| `deps remove <package> from <project>` | Remove package |
| `deps update <project>` | Update all packages |
| `deps restore <project>` | Restore packages |
| `deps check <project>` | Check for updates |
| `deps history` | View operation history |

### Git Operations Module (Optional)

| Command | Description |
|---------|-------------|
| `git status` | Show working tree status |
| `git branch [options]` | Manage branches |
| `git checkout <branch>` | Switch branches |
| `git add <files>` | Stage files |
| `git commit -m "message"` | Commit changes |
| `git push [remote] [branch]` | Push to remote |
| `git pull [remote] [branch]` | Pull from remote |
| `git diff [options]` | Show changes |
| `git log [options]` | Show commit history |
| `git setdir <path>` | Set working directory |
| `git info` | Show repository info |
| `git history` | Show operation history |

## 🔄 Typical Workflows

### Workflow 1: Fix a Bug

```bash
# 1. Analyze the file
ashat fix bug in /path/to/MyModule.cs

# 2. Review detected issues
bugdetect list

# 3. Get fix suggestion for specific issue
bugdetect fix abc123

# 4. Apply the fix
fileops replace "if (obj.ToString()" with "if (obj?.ToString()" in /path/to/MyModule.cs

# 5. Verify the change
fileops read /path/to/MyModule.cs
```

### Workflow 2: Refactor Code

```bash
# 1. Analyze for refactoring opportunities
ashat refactor /path/to/OldCode.cs

# 2. Rename a class
refactor rename OldClassName to NewClassName in /path/to/OldCode.cs

# 3. Format the code
refactor format /path/to/OldCode.cs

# 4. Organize imports
refactor organize /path/to/OldCode.cs
```

### Workflow 3: Add New Feature with Dependencies

```bash
# 1. Start a guided session
ashat start session dev001 Add JSON serialization feature

# 2. Add required package
deps add Newtonsoft.Json to /path/to/project.csproj

# 3. Restore packages
deps restore /path/to/project.csproj

# 4. Continue with implementation
ashat continue dev001 Added Newtonsoft.Json package

# 5. Approve and execute
ashat approve dev001
```

### Workflow 4: Complete File Edit

```bash
# 1. Read the file
fileops read /path/to/Module.cs

# 2. Make targeted change
fileops replace "public class OldName" with "public class NewName" in /path/to/Module.cs

# 3. Verify change
fileops read /path/to/Module.cs

# 4. If needed, restore from backup
fileops restore /path/to/Module.cs
```

## 🛡️ Safety Features

### Automatic Backups
- Every file modification creates an automatic backup
- Backups are timestamped and uniquely identified
- Easy restoration with `fileops restore`

### Approval Workflow
- ASHAT maintains approval-based workflow for guided sessions
- No automatic code changes without user approval
- Full transparency in action plans

### Operation History
- All modules track operation history
- Review what changes were made and when
- Audit trail for debugging

## 💡 Best Practices

### 1. Always Review Before Editing
```bash
# Read file first
fileops read /path/to/file.cs

# Or use analyze
ashat analyze code /path/to/file.cs
```

### 2. Use Bug Detection First
```bash
# Scan before manual changes
bugdetect scan /path/to/file.cs

# Get suggestions
bugdetect fix <issueId>
```

### 3. Test After Refactoring
```bash
# After refactoring, build to verify
dotnet build /path/to/project.csproj
```

### 4. Check Dependencies Regularly
```bash
# Check for updates periodically
deps check /path/to/project.csproj

# Update when safe
deps update /path/to/project.csproj
```

### 5. Use Guided Sessions for Complex Tasks
```bash
# For complex changes, use sessions
ashat start session myid Complex refactoring task

# ASHAT will guide you step by step
ashat continue myid [your input]
ashat approve myid
```

## 🔧 Integration with Existing Features

### Works Seamlessly With
- **ModuleSpawner** - Create new modules and edit them
- **AICodeGen** - Generate code and refine with direct edits
- **TestRunner** - Run tests after code changes
- **Knowledge** - Access documentation while editing
- **Chat Support** - Get coding help while working

### Module Coordination
All new modules are auto-discovered and available immediately:
```bash
# Check loaded modules
ashat modules

# You'll see:
# - FileOperations
# - BugDetector
# - Refactoring
# - DependencyManager
# - GitOperations
```

## 📊 Examples

### Example 1: Fix Null Reference Bug

```bash
$ ashat fix bug in /app/MyService.cs

🔍 Bug Fix Mode
===============

Scanning file for potential issues: /app/MyService.cs

📄 Scanned: /app/MyService.cs
   Lines: 150
   Issues found: 1

Detected Issues:
🟠 [a7b3] Line 42: Potential Null Reference
   Severity: High
   Code: var result = user.ToString();
   Issue: Method call on potentially null object

💡 Next steps:
  1. Review the detected issues above
  2. Use: bugdetect fix a7b3 - to get fix suggestions
  3. Use: fileops replace ... - to apply fixes

$ bugdetect fix a7b3

Fix Suggestion for Issue a7b3:

Pattern: Potential Null Reference
Severity: High
File: /app/MyService.cs
Line: 42

Issue: Method call on potentially null object

Code:
  var result = user.ToString();

💡 Suggested Fix:
  Add null check before method call (e.g., obj?.Method() or if (obj != null))

$ fileops replace "var result = user.ToString();" with "var result = user?.ToString() ?? \"Unknown\";" in /app/MyService.cs

✅ Replacement successful: /app/MyService.cs
   Occurrences replaced: 1
   Old text: 'var result = user.ToString();'
   New text: 'var result = user?.ToString() ?? "Unknown";'
```

### Example 2: Refactor Class Name

```bash
$ refactor rename UserService to UserManager in /app/Services/UserService.cs

✅ Rename refactoring completed
   File: /app/Services/UserService.cs
   UserService → UserManager
   Occurrences: 5
   Backup: /app/Services/UserService.cs.refactor.backup
```

### Example 3: Add Package and Update Code

```bash
$ deps add System.Text.Json to /app/MyApp.csproj

✅ Package added successfully
   Package: System.Text.Json
   Project: MyApp.csproj

Determining projects to restore...
  Restored MyApp.csproj (in 543 ms).

$ fileops read /app/Config.cs

$ fileops replace "using Newtonsoft.Json;" with "using System.Text.Json;" in /app/Config.cs

✅ Replacement successful: /app/Config.cs
   Occurrences replaced: 1
```

## 🎯 Key Advantages

1. **No GitHub Dependency** - Works entirely on the server with direct file access
2. **Automatic Backups** - Never lose work with automatic backup creation
3. **Comprehensive Analysis** - 8 bug patterns + refactoring suggestions
4. **Integrated Workflow** - All tools work together seamlessly
5. **Approval-Based Safety** - Maintains ASHAT's ethical "Harm None" principles
6. **Operation History** - Complete audit trail of all changes
7. **Easy to Use** - Simple, consistent command syntax

## 🌈 Future Enhancements

- [ ] Web UI for visual code editing
- [ ] Advanced code parsing with Roslyn
- [ ] Machine learning-based bug detection
- [ ] Automated test generation
- [ ] Code similarity detection
- [ ] Performance profiling integration
- [ ] Advanced refactoring patterns
- [ ] Multi-file refactoring support

## 📞 Support

For help with ASHAT's coding capabilities:

```bash
# Get general help
ashat help

# Get module-specific help
fileops help
bugdetect help
refactor help
deps help
git help
```

## 🎓 Learning Resources

- **ASHAT_QUICKSTART.md** - Getting started guide
- **Module documentation** - Each module has built-in help
- **Examples directory** - Sample workflows and code
- **Chat support** - Ask ASHAT questions anytime

---

**Ready to code with ASHAT! 🚀**

Transform your development workflow with AI-powered direct file editing, bug fixing, and code management - all without leaving the server!
