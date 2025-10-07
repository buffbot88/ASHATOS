# 🤝 Contributing to RaOS

Thank you for your interest in contributing to RaOS! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Setup](#development-setup)
4. [How to Contribute](#how-to-contribute)
5. [Coding Standards](#coding-standards)
6. [Testing Guidelines](#testing-guidelines)
7. [Pull Request Process](#pull-request-process)
8. [Issue Guidelines](#issue-guidelines)
9. [Community](#community)

---

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

**In Summary:**
- Be respectful and inclusive
- Welcome newcomers
- Focus on what's best for the community
- Show empathy towards others

---

## Getting Started

### Prerequisites

Before you begin, ensure you have:

- **Required:**
  - .NET 9.0 SDK or later
  - Git
  - A GitHub account
  - Code editor (Visual Studio, VS Code, or Rider)

- **Optional:**
  - PHP 8+ (for CMS features)
  - Nginx (for production testing)
  - Docker (for containerized development)

### Understanding the Project

1. **Read the Documentation:**
   - [README.md](README.md) - Project overview
   - [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
   - [ROADMAP.md](ROADMAP.md) - Future plans

2. **Explore the Codebase:**
   - Browse the `/RaCore` directory
   - Look at existing modules in `/RaCore/Modules/Extensions`
   - Review `/LegendaryCMS` and `/LegendaryGameEngine`

3. **Build and Run:**
   ```bash
   git clone https://github.com/buffbot88/TheRaProject.git
   cd TheRaProject/RaCore
   dotnet build
   dotnet run
   ```

---

## Development Setup

### 1. Fork the Repository

Click the "Fork" button on the GitHub repository page to create your own copy.

### 2. Clone Your Fork

```bash
git clone https://github.com/YOUR_USERNAME/TheRaProject.git
cd TheRaProject
```

### 3. Add Upstream Remote

```bash
git remote add upstream https://github.com/buffbot88/TheRaProject.git
```

### 4. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

**Branch Naming Conventions:**
- `feature/feature-name` - New features
- `bugfix/bug-description` - Bug fixes
- `docs/documentation-update` - Documentation changes
- `refactor/refactoring-description` - Code refactoring
- `test/test-description` - Test additions or changes

### 5. Verify Build

```bash
# Build entire solution
dotnet build TheRaProject.sln

# Run tests (if available)
dotnet test

# Run RaCore
cd RaCore
dotnet run
```

---

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

#### 1. Code Contributions
- New features
- Bug fixes
- Performance improvements
- Code refactoring

#### 2. Documentation
- Improve existing docs
- Write tutorials
- Create examples
- Fix typos or clarify content

#### 3. Testing
- Write unit tests
- Write integration tests
- Report bugs
- Verify bug fixes

#### 4. Community Support
- Answer questions in discussions
- Help newcomers
- Review pull requests
- Share your projects built with RaOS

---

## Coding Standards

### C# Style Guide

Follow the [.NET Runtime Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md).

**Key Points:**

#### Naming Conventions

```csharp
// PascalCase for classes, methods, properties
public class MyModule : ModuleBase
{
    public string MyProperty { get; set; }
    
    public void MyMethod()
    {
        // ...
    }
}

// camelCase for local variables and parameters
public void ProcessData(string inputData)
{
    var resultData = Transform(inputData);
}

// _camelCase for private fields
private readonly ILogger _logger;
private string _internalState;

// UPPER_CASE for constants
public const int MAX_RETRY_COUNT = 3;
```

#### File Organization

```csharp
// 1. Using statements (sorted)
using System;
using System.Collections.Generic;
using System.Linq;
using Abstractions;

// 2. Namespace
namespace RaCore.Modules.Extensions.MyModule;

// 3. Class documentation
/// <summary>
/// Provides functionality for X feature.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class MyModule : ModuleBase
{
    // 4. Constants
    private const int DEFAULT_TIMEOUT = 30;
    
    // 5. Fields
    private readonly ILogger _logger;
    private string _state;
    
    // 6. Constructors
    public MyModule()
    {
        // ...
    }
    
    // 7. Properties
    public override string Name => "MyModule";
    
    // 8. Public methods
    public override void Initialize(object? manager)
    {
        // ...
    }
    
    // 9. Private methods
    private void InternalMethod()
    {
        // ...
    }
}
```

#### Code Formatting

```csharp
// Use braces even for single-line statements
if (condition)
{
    DoSomething();
}

// Prefer expression-bodied members when simple
public string Name => "MyModule";

// Use string interpolation
var message = $"User {username} logged in at {timestamp}";

// Use var when type is obvious
var user = new User();
var count = GetCount();

// Use explicit types when not obvious
IEnumerable<User> users = GetAllUsers();
```

#### Async/Await

```csharp
// Async methods should end with "Async"
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetAsync(id);
}

// Always use ConfigureAwait(false) in libraries
var result = await SomeMethodAsync().ConfigureAwait(false);

// Avoid async void (except event handlers)
public async Task ProcessAsync() // Good
public async void ProcessAsync() // Bad
```

#### Error Handling

```csharp
// Use specific exceptions
throw new ArgumentNullException(nameof(parameter));
throw new InvalidOperationException("Invalid state");

// Log exceptions
try
{
    await RiskyOperationAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}
```

### Module Development Standards

#### Module Structure

```csharp
[RaModule(Category = "extensions")]
public sealed class MyModule : ModuleBase
{
    public override string Name => "MyModule";
    
    // Always override Initialize
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        // Setup logic
        Log("MyModule initialized");
    }
    
    // Always override Process
    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelp();
        
        var parts = input.Split(' ', 2);
        var command = parts[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1] : string.Empty;
        
        return command switch
        {
            "help" => GetHelp(),
            "status" => GetStatus(),
            "action" => HandleAction(args),
            _ => $"Unknown command: {command}\nType 'help' for available commands."
        };
    }
    
    private string GetHelp()
    {
        return @"
MyModule Commands:
  help   - Show this help
  status - Show module status
  action - Perform action
";
    }
}
```

#### Documentation Comments

```csharp
/// <summary>
/// Processes user requests and generates responses.
/// </summary>
/// <param name="input">The user input command.</param>
/// <returns>The response message.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="input"/> is null.
/// </exception>
public override string Process(string input)
{
    // ...
}
```

---

## Testing Guidelines

### Writing Tests

Create tests in the appropriate test project or directory.

**Example Unit Test:**

```csharp
using Xunit;

public class MyModuleTests
{
    [Fact]
    public void Initialize_ShouldSetupModule()
    {
        // Arrange
        var module = new MyModule();
        
        // Act
        module.Initialize(null);
        
        // Assert
        Assert.NotNull(module.Name);
        Assert.Equal("MyModule", module.Name);
    }
    
    [Theory]
    [InlineData("help", "MyModule Commands")]
    [InlineData("status", "Status:")]
    public void Process_ShouldReturnExpectedResponse(
        string input, 
        string expectedSubstring)
    {
        // Arrange
        var module = new MyModule();
        module.Initialize(null);
        
        // Act
        var result = module.Process(input);
        
        // Assert
        Assert.Contains(expectedSubstring, result);
    }
}
```

### Test Categories

- **Unit Tests:** Test individual components in isolation
- **Integration Tests:** Test component interactions
- **End-to-End Tests:** Test complete workflows

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RaCore.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## Pull Request Process

### Before Submitting

1. **Update from Upstream:**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run Tests:**
   ```bash
   dotnet test
   ```

3. **Build Successfully:**
   ```bash
   dotnet build TheRaProject.sln
   ```

4. **Update Documentation:**
   - Update README if needed
   - Add/update code comments
   - Update relevant documentation files

### Creating a Pull Request

1. **Push Your Branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Open PR on GitHub:**
   - Go to your fork on GitHub
   - Click "Pull Request"
   - Select your branch
   - Fill out the PR template

3. **PR Title Format:**
   ```
   [Category] Brief description
   
   Examples:
   [Feature] Add custom plugin system
   [Bugfix] Fix memory leak in ModuleManager
   [Docs] Update contributing guidelines
   ```

4. **PR Description Should Include:**
   - What changes were made
   - Why the changes were made
   - How to test the changes
   - Related issue numbers (if any)
   - Screenshots (if UI changes)

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Fixes #123

## Testing
Describe how to test your changes

## Checklist
- [ ] Code builds successfully
- [ ] Tests pass
- [ ] Documentation updated
- [ ] Code follows style guidelines
```

### Review Process

1. **Automated Checks:** CI/CD will run builds and tests
2. **Code Review:** Maintainers will review your code
3. **Feedback:** Address any requested changes
4. **Approval:** Once approved, maintainers will merge

**Timeline:**
- Initial review: 2-5 business days
- Follow-up reviews: 1-3 business days

---

## Issue Guidelines

### Before Creating an Issue

1. **Search Existing Issues:** Check if already reported
2. **Read Documentation:** Ensure it's not a usage question
3. **Reproduce the Bug:** Verify it's reproducible

### Bug Reports

Use the bug report template and include:

```markdown
**Description**
Clear description of the bug

**Steps to Reproduce**
1. Go to '...'
2. Click on '...'
3. See error

**Expected Behavior**
What should happen

**Actual Behavior**
What actually happens

**Environment**
- OS: [e.g., Windows 11, Ubuntu 22.04]
- .NET Version: [e.g., 9.0.0]
- RaOS Version: [e.g., 9.0.0]

**Logs**
```
Paste relevant logs
```

**Screenshots**
If applicable
```

### Feature Requests

Use the feature request template:

```markdown
**Is your feature request related to a problem?**
Clear description of the problem

**Describe the solution you'd like**
What you want to happen

**Describe alternatives you've considered**
Other solutions you've considered

**Additional context**
Any other context or screenshots
```

### Questions

For questions, use:
- GitHub Discussions (preferred)
- Discord community
- Stack Overflow (tag: raos)

---

## Community

### Communication Channels

- **GitHub Discussions:** General discussions, Q&A
- **GitHub Issues:** Bug reports, feature requests
- **Discord:** Real-time chat (invite link in README)
- **Email:** raos-team@example.com

### Getting Help

- Check the [documentation](README.md)
- Search [existing issues](https://github.com/buffbot88/TheRaProject/issues)
- Ask in [discussions](https://github.com/buffbot88/TheRaProject/discussions)
- Join our [Discord community](#)

### Recognition

Contributors are recognized in:
- [CONTRIBUTORS.md](CONTRIBUTORS.md)
- Release notes
- Project README
- Annual contributor highlights

---

## License

By contributing to RaOS, you agree that your contributions will be licensed under the same license as the project (see [LICENSE](LICENSE)).

---

## Questions?

If you have questions about contributing, feel free to:
- Open a discussion on GitHub
- Ask in our Discord community
- Email the maintainers

---

**Thank you for contributing to RaOS! 🎉**

---

**Last Updated:** January 13, 2025  
**Version:** 1.0
