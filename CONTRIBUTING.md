# 🤝 Contributing to RaOS

First off, thank you for considering contributing to RaOS! It's people like you that make RaOS such a great platform.

---

## 📋 Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [How Can I Contribute?](#how-can-i-contribute)
4. [Development Workflow](#development-workflow)
5. [Coding Standards](#coding-standards)
6. [Commit Message Guidelines](#commit-message-guidelines)
7. [Pull Request Process](#pull-request-process)
8. [Testing Guidelines](#testing-guidelines)
9. [Documentation Guidelines](#documentation-guidelines)
10. [Community](#community)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inspiring community for all. Please be respectful and constructive in your interactions.

### Our Standards

**Examples of behavior that contributes to a positive environment:**
- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

**Examples of unacceptable behavior:**
- The use of sexualized language or imagery
- Trolling, insulting/derogatory comments, and personal or political attacks
- Public or private harassment
- Publishing others' private information without explicit permission
- Other conduct which could reasonably be considered inappropriate

---

## Getting Started

### Prerequisites

Before you begin, ensure you have:
- **.NET 9.0 SDK** installed
- **Git** for version control
- A **GitHub account**
- Basic understanding of **C# and .NET**
- Familiarity with the [Architecture](ARCHITECTURE.md)

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/TheRaProject.git
   cd TheRaProject
   ```

3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/buffbot88/TheRaProject.git
   ```

4. **Build the project**:
   ```bash
   cd RaCore
   dotnet build
   dotnet run
   ```

5. **Verify the setup**:
   ```bash
   cd ..
   chmod +x verify-phase8.sh
   ./verify-phase8.sh
   ```

---

## How Can I Contribute?

### 🐛 Reporting Bugs

Before creating bug reports, please check existing issues. When creating a bug report, include:

- **Clear and descriptive title**
- **Steps to reproduce** the problem
- **Expected behavior** vs actual behavior
- **Screenshots** if applicable
- **Environment details** (OS, .NET version, etc.)
- **Log files** or error messages

Use the [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md)

### 💡 Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **List any alternative solutions** you've considered

Use the [Feature Request Template](.github/ISSUE_TEMPLATE/feature_request.md)

### 📝 Improving Documentation

Documentation improvements are always welcome:
- Fix typos or clarify existing documentation
- Add examples or use cases
- Create new guides for complex features
- Translate documentation to other languages

### 🔧 Contributing Code

Code contributions can include:
- Bug fixes
- New features
- Performance improvements
- New modules or plugins
- Test coverage improvements

---

## Development Workflow

### 1. Create a Branch

Always create a new branch for your work:

```bash
# Update your fork
git fetch upstream
git checkout main
git merge upstream/main

# Create a feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/bug-description
```

**Branch Naming Convention:**
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation changes
- `refactor/` - Code refactoring
- `test/` - Test additions or changes
- `chore/` - Maintenance tasks

### 2. Make Your Changes

- Follow the [Coding Standards](#coding-standards)
- Write or update tests as needed
- Update documentation as needed
- Keep changes focused and atomic

### 3. Test Your Changes

```bash
# Build the solution
dotnet build TheRaProject.sln

# Run specific module tests
cd RaCore
dotnet test

# Manual testing
dotnet run

# Verify Phase 8 (if applicable)
cd ..
./verify-phase8.sh
```

### 4. Commit Your Changes

Follow the [Commit Message Guidelines](#commit-message-guidelines):

```bash
git add .
git commit -m "feat: add new game engine feature"
```

### 5. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 6. Create a Pull Request

- Go to your fork on GitHub
- Click "New Pull Request"
- Fill out the PR template
- Link any relevant issues

---

## Coding Standards

### General Principles

1. **Keep it simple** - Prefer clarity over cleverness
2. **Follow SOLID principles** - Especially Single Responsibility
3. **Write self-documenting code** - Clear names over comments
4. **Don't Repeat Yourself (DRY)** - Extract common functionality
5. **Fail fast** - Validate inputs early

### C# Style Guide

#### Naming Conventions

```csharp
// PascalCase for classes, methods, properties
public class GameEngineModule { }
public void ProcessCommand() { }
public string PlayerName { get; set; }

// camelCase for local variables and parameters
var playerHealth = 100;
public void SetHealth(int healthValue) { }

// _camelCase for private fields
private readonly ILogger _logger;
private string _cachedData;

// UPPER_CASE for constants
private const int MAX_PLAYERS = 100;
public const string DEFAULT_SCENE = "MainWorld";

// Interfaces start with 'I'
public interface IModuleBase { }
```

#### Code Organization

```csharp
// Order: fields, constructors, properties, methods
public class ExampleModule
{
    // 1. Private fields
    private readonly ILogger _logger;
    private string _state;
    
    // 2. Constructor(s)
    public ExampleModule(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 3. Public properties
    public string Name { get; set; }
    
    // 4. Public methods
    public void DoSomething() { }
    
    // 5. Private methods
    private void HelperMethod() { }
}
```

#### Comments and Documentation

```csharp
// Use XML documentation for public APIs
/// <summary>
/// Processes a game command and returns the result.
/// </summary>
/// <param name="command">The command to process.</param>
/// <returns>The result of the command execution.</returns>
/// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
public string ProcessCommand(string command)
{
    // Use inline comments only when necessary to explain "why", not "what"
    // The code should be self-explanatory for "what"
    
    if (string.IsNullOrEmpty(command))
        throw new ArgumentNullException(nameof(command));
    
    return ExecuteCommand(command);
}
```

#### Error Handling

```csharp
// Use specific exception types
public void LoadModule(string path)
{
    if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(nameof(path));
    
    if (!File.Exists(path))
        throw new FileNotFoundException($"Module not found: {path}");
    
    try
    {
        // Load module
    }
    catch (IOException ex)
    {
        _logger.LogError($"Failed to load module: {ex.Message}");
        throw;
    }
}
```

#### Async/Await

```csharp
// Always use async/await for I/O operations
public async Task<string> FetchDataAsync()
{
    // Use ConfigureAwait(false) in library code
    var result = await httpClient.GetStringAsync(url).ConfigureAwait(false);
    return result;
}

// Name async methods with 'Async' suffix
public async Task InitializeAsync() { }
```

#### LINQ and Modern C# Features

```csharp
// Use LINQ for collection operations
var activePlayers = players
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Select(p => p.Id)
    .ToList();

// Use null-conditional operator
var name = player?.Name ?? "Unknown";

// Use pattern matching
if (obj is GameEntity entity && entity.IsAlive)
{
    // Process entity
}

// Use expression-bodied members for simple methods
public string GetFullName() => $"{FirstName} {LastName}";
```

### Module Development Standards

When creating new modules:

1. **Implement `IModuleBase`** or inherit from `ModuleBase`
2. **Use `[RaModule]` attribute** with appropriate category
3. **Initialize in `InitializeAsync()`** method
4. **Handle commands in `Process()`** method
5. **Dispose resources properly** in `Dispose()`
6. **Log appropriately** using provided logger

Example:

```csharp
[RaModule(Category = "extensions")]
public sealed class MyCustomModule : ModuleBase
{
    public override string Name => "MyCustom";
    public override string Description => "My custom module description";
    
    public override async Task<bool> InitializeAsync()
    {
        _logger.LogInfo("Initializing MyCustomModule...");
        // Initialization logic
        return await Task.FromResult(true);
    }
    
    public override async Task<object?> Process(string input, Dictionary<string, object> context)
    {
        // Command processing logic
        return await Task.FromResult<object?>(null);
    }
}
```

See [MODULE_DEVELOPMENT_GUIDE.md](MODULE_DEVELOPMENT_GUIDE.md) for complete details.

---

## Commit Message Guidelines

### Format

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Maintenance tasks, dependency updates, etc.
- **build**: Changes to build system or dependencies
- **ci**: Changes to CI configuration

### Scope (Optional)

The scope should be the name of the module or component affected:
- `cms`
- `gameengine`
- `core`
- `api`
- `docs`

### Examples

```bash
# Simple commit
git commit -m "feat: add player inventory system"

# With scope
git commit -m "fix(gameengine): resolve scene loading issue"

# Breaking change
git commit -m "feat(api)!: change endpoint response format

BREAKING CHANGE: API response now includes metadata object"

# With body
git commit -m "refactor(cms): improve plugin loading performance

- Implement lazy loading for plugins
- Add caching mechanism
- Reduce initialization time by 40%"
```

### Guidelines

1. **Use imperative mood** - "add feature" not "added feature"
2. **Don't capitalize first letter** of subject
3. **No period at the end** of subject
4. **Limit subject to 50 characters**
5. **Wrap body at 72 characters**
6. **Separate subject from body** with a blank line
7. **Explain what and why**, not how

---

## Pull Request Process

### Before Submitting

- [ ] Code follows the style guidelines
- [ ] Self-review of code completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] Tests added or updated
- [ ] All tests pass
- [ ] No merge conflicts
- [ ] Commits follow commit message guidelines

### PR Title

Use the same format as commit messages:

```
feat(gameengine): add multiplayer support
fix(cms): resolve plugin loading issue
docs: update installation guide
```

### PR Description

Use the [Pull Request Template](.github/PULL_REQUEST_TEMPLATE.md):

1. **Description** - What changes does this PR introduce?
2. **Motivation** - Why is this change needed?
3. **Related Issues** - Link to relevant issues
4. **Type of Change** - Bug fix, feature, docs, etc.
5. **Testing** - How was this tested?
6. **Screenshots** - If applicable
7. **Checklist** - Verify all items are completed

### Review Process

1. **Automated checks** must pass (build, tests)
2. **At least one approval** from a maintainer
3. **All review comments** must be addressed
4. **Conflicts must be resolved**
5. **Documentation** must be updated if needed

### After Approval

- Maintainers will merge your PR
- Your contribution will be included in the next release
- You'll be added to the contributors list

---

## Testing Guidelines

### Test Requirements

- **Unit tests** for new functionality
- **Integration tests** for module interactions
- **Manual tests** for UI or complex features
- **Performance tests** for critical paths

### Writing Tests

```csharp
[Fact]
public void ProcessCommand_ValidInput_ReturnsSuccess()
{
    // Arrange
    var module = new TestModule();
    var command = "test command";
    
    // Act
    var result = module.ProcessCommand(command);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
}

[Theory]
[InlineData("", false)]
[InlineData(null, false)]
[InlineData("valid", true)]
public void ValidateInput_VariousInputs_ReturnsExpected(string input, bool expected)
{
    // Arrange & Act
    var result = Validator.IsValid(input);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RaCore.Tests/RaCore.Tests.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true
```

See [TESTING_STRATEGY.md](TESTING_STRATEGY.md) for complete testing approach.

---

## Documentation Guidelines

### When to Update Documentation

Update documentation when you:
- Add new features or modules
- Change existing functionality
- Fix bugs that affect user behavior
- Change APIs or interfaces
- Add configuration options

### Documentation Standards

1. **Use clear, concise language**
2. **Include code examples** where appropriate
3. **Add diagrams** for complex concepts
4. **Keep formatting consistent** with existing docs
5. **Update the [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)**

### Documentation Structure

```markdown
# Module Name

Brief description (1-2 sentences)

## Overview

Detailed description

## Features

- Feature 1
- Feature 2

## Quick Start

Basic usage example

## API Reference

Detailed API documentation

## Examples

Real-world examples

## Troubleshooting

Common issues and solutions

## See Also

Links to related documentation
```

---

## Community

### Getting Help

- **Documentation**: Start with [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)
- **GitHub Issues**: Search existing issues or create a new one
- **Discussions**: Use GitHub Discussions for questions

### Staying Updated

- **Watch the repository** for notifications
- **Read [PHASES.md](PHASES.md)** for development roadmap
- **Check [ROADMAP.md](ROADMAP.md)** for future plans

### Recognition

Contributors are recognized in:
- **README.md** - Contributors section
- **Release notes** - Credits for contributions
- **Commit history** - Your commits are preserved

---

## License

By contributing to RaOS, you agree that your contributions will be licensed under the same license as the project.

---

## Questions?

If you have questions about contributing:

1. Check the [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)
2. Review [MODULE_DEVELOPMENT_GUIDE.md](MODULE_DEVELOPMENT_GUIDE.md)
3. Open a GitHub Discussion
4. Create an issue with the "question" label

---

**Thank you for contributing to RaOS! Your efforts help make this project better for everyone.**

---

**Last Updated:** January 2025  
**Version:** 9.4.0  
**Maintained By:** RaOS Development Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
