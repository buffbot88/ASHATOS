# 💻 Development Guide

**Version:** 9.3.2  
**Last Updated:** January 2025  
**Target Audience:** Contributors and Module Developers

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Development Environment Setup](#development-environment-setup)
3. [Coding Standards](#coding-standards)
4. [Architecture Guidelines](#architecture-guidelines)
5. [Module Development](#module-development)
6. [API Development](#api-development)
7. [Testing Standards](#testing-standards)
8. [Security Best Practices](#security-best-practices)
9. [Performance Guidelines](#performance-guidelines)
10. [Debugging Tips](#debugging-tips)
11. [Code Review Checklist](#code-review-checklist)
12. [Deployment Procedures](#deployment-procedures)

---

## Overview

This guide provides comprehensive development standards and best practices for contributing to RaOS. Whether you're fixing a bug, adding a feature, or creating a new module, following these guidelines ensures code quality and consistency.

**Key Principles:**
- 🎯 **Simplicity**: Write clear, maintainable code
- 🔒 **Security**: Follow security best practices
- ⚡ **Performance**: Optimize for speed and efficiency
- 📚 **Documentation**: Document your code thoroughly
- 🧪 **Testing**: Test all functionality

---

## Development Environment Setup

### Required Tools

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 9.0+ | Core framework |
| Git | Latest | Version control |
| Visual Studio / VS Code | Latest | IDE |
| Docker | Optional | Containerized testing |

### Initial Setup

```bash
# 1. Clone the repository
git clone https://github.com/buffbot88/TheRaProject.git
cd TheRaProject

# 2. Restore dependencies
dotnet restore

# 3. Build the solution
dotnet build TheRaProject.sln

# 4. Run RaCore
cd RaCore
dotnet run

# 5. Verify setup
cd ..
chmod +x verify-phase8.sh
./verify-phase8.sh
```

### IDE Configuration

#### Visual Studio Code

Install recommended extensions:
- C# Dev Kit
- C# Extensions
- GitLens
- EditorConfig for VS Code

Create `.vscode/settings.json`:
```json
{
  "editor.formatOnSave": true,
  "editor.tabSize": 4,
  "files.trimTrailingWhitespace": true,
  "csharp.format.enable": true,
  "omnisharp.enableEditorConfigSupport": true
}
```

#### Visual Studio

Configure formatting:
1. Tools → Options → Text Editor → C# → Code Style
2. Import RaOS code style settings (if provided)
3. Enable "Format document on save"

---

## Coding Standards

### File Organization

```
RaCore/
├── Modules/
│   ├── Core/           # Core functionality
│   ├── Extensions/     # Extension modules
│   └── Legendary/      # Legendary suite modules
├── Engine/
│   ├── Manager/        # Module management
│   ├── Memory/         # Memory management
│   └── Interfaces/     # Core interfaces
└── Utils/              # Utility classes
```

### Naming Conventions

#### Classes and Interfaces

```csharp
// Classes: PascalCase
public class GameEngineModule { }
public class PlayerInventoryManager { }

// Interfaces: I + PascalCase
public interface IModuleBase { }
public interface IPluginContext { }

// Abstract classes
public abstract class ModuleBase { }

// Sealed classes (prefer when not designed for inheritance)
public sealed class PlayerData { }
```

#### Methods and Properties

```csharp
// Methods: PascalCase, verb-based
public void ProcessCommand() { }
public async Task InitializeAsync() { }
public bool ValidateInput(string input) { }

// Properties: PascalCase, noun-based
public string PlayerName { get; set; }
public int MaxHealth { get; private set; }
public bool IsActive { get; }

// Events: PascalCase, past tense or present participle
public event EventHandler GameStarted;
public event EventHandler<PlayerEventArgs> PlayerJoining;
```

#### Fields and Variables

```csharp
// Private fields: _camelCase
private readonly ILogger _logger;
private string _cachedData;
private int _playerCount;

// Local variables: camelCase
var playerHealth = 100;
string userName = "Player1";

// Constants: UPPER_CASE
private const int MAX_PLAYERS = 100;
public const string DEFAULT_SCENE = "MainWorld";

// Static readonly: PascalCase
private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
```

### Code Style

#### Indentation and Spacing

```csharp
// Use 4 spaces for indentation (not tabs)
public class Example
{
    private int _value;
    
    public void Method()
    {
        if (condition)
        {
            // Code here
        }
    }
}

// Single line spacing between logical blocks
public void Process()
{
    var data = LoadData();
    
    ProcessData(data);
    
    SaveResults();
}
```

#### Braces and Blocks

```csharp
// Always use braces, even for single-line blocks
if (condition)
{
    DoSomething();
}

// Opening brace on new line (Allman style)
public void Method()
{
    // Method body
}

// Exception: Properties can be on one line
public string Name { get; set; }

// But prefer this for complex properties
public string FullName
{
    get => $"{FirstName} {LastName}";
    set
    {
        var parts = value.Split(' ');
        FirstName = parts[0];
        LastName = parts.Length > 1 ? parts[1] : string.Empty;
    }
}
```

#### Line Length and Wrapping

```csharp
// Keep lines under 120 characters
// Wrap long lines at logical points
var result = SomeVeryLongMethodName(
    parameter1,
    parameter2,
    parameter3);

// Chain LINQ methods on separate lines
var players = gameState
    .GetAllPlayers()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Score)
    .Take(10)
    .ToList();
```

### Comments and Documentation

#### XML Documentation

```csharp
/// <summary>
/// Processes a game command and executes the corresponding action.
/// </summary>
/// <param name="command">The command string to process.</param>
/// <param name="context">The execution context containing player state.</param>
/// <returns>
/// A task representing the asynchronous operation. The task result contains
/// the command execution result or null if the command failed.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="command"/> is null or empty.
/// </exception>
/// <exception cref="InvalidOperationException">
/// Thrown when the game engine is not initialized.
/// </exception>
/// <example>
/// <code>
/// var result = await engine.ProcessCommandAsync("spawn npc warrior", context);
/// </code>
/// </example>
public async Task<object?> ProcessCommandAsync(
    string command,
    Dictionary<string, object> context)
{
    // Implementation
}
```

#### Inline Comments

```csharp
// Use inline comments to explain "why", not "what"
// BAD: Increment the counter
counter++;

// GOOD: Skip cached entries to force refresh
counter++;

// Use TODO comments with issue tracking
// TODO(#123): Implement proper error handling for network failures

// Complex algorithms deserve explanation
// Boyer-Moore string search algorithm implementation
// Time complexity: O(n/m) best case, O(nm) worst case
public int Search(string text, string pattern)
{
    // Algorithm implementation
}
```

### Modern C# Features

#### Null Safety

```csharp
// Enable nullable reference types in .csproj
<Nullable>enable</Nullable>

// Use nullable annotations
public string? GetPlayerName(int playerId)
{
    var player = FindPlayer(playerId);
    return player?.Name;
}

// Use null-coalescing operator
var name = playerName ?? "Unknown";

// Use null-conditional operator
var length = text?.Length ?? 0;

// Use pattern matching with null check
if (obj is GameEntity entity)
{
    entity.Update();
}
```

#### Expression-Bodied Members

```csharp
// Simple getters
public string FullName => $"{FirstName} {LastName}";

// Methods
public int Add(int a, int b) => a + b;

// Properties with backing field
private string _name = string.Empty;
public string Name
{
    get => _name;
    set => _name = value ?? throw new ArgumentNullException(nameof(value));
}
```

#### Pattern Matching

```csharp
// Type patterns
if (obj is GameEntity entity)
{
    entity.Update();
}

// Switch expressions
var result = value switch
{
    0 => "Zero",
    > 0 and < 10 => "Small",
    >= 10 and < 100 => "Medium",
    >= 100 => "Large",
    _ => "Unknown"
};

// Property patterns
var discount = customer switch
{
    { IsVIP: true } => 0.20m,
    { Orders.Count: > 10 } => 0.10m,
    { TotalSpent: > 1000 } => 0.15m,
    _ => 0.0m
};
```

#### Records

```csharp
// Use records for immutable data
public record PlayerState(int Health, int Mana, string Location);

// With additional members
public record GameConfig
{
    public required string ServerName { get; init; }
    public required int MaxPlayers { get; init; }
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);
}
```

---

## Architecture Guidelines

### Separation of Concerns

```csharp
// Separate data, business logic, and presentation

// Data layer
public class PlayerRepository
{
    public Task<Player?> GetByIdAsync(int id) { }
}

// Business logic layer
public class PlayerService
{
    private readonly PlayerRepository _repository;
    
    public async Task<bool> UpdatePlayerAsync(int id, PlayerData data)
    {
        var player = await _repository.GetByIdAsync(id);
        if (player == null) return false;
        
        // Business logic
        player.Update(data);
        
        await _repository.SaveAsync(player);
        return true;
    }
}

// Presentation layer
public class PlayerModule : ModuleBase
{
    private readonly PlayerService _service;
    
    public override async Task<object?> Process(string input, Dictionary<string, object> context)
    {
        // Handle user input, call service
    }
}
```

### Dependency Injection

```csharp
// Define interfaces for dependencies
public interface IGameEngine
{
    Task<Scene> CreateSceneAsync(string name);
}

// Inject via constructor
public class GameModule : ModuleBase
{
    private readonly IGameEngine _engine;
    private readonly ILogger _logger;
    
    public GameModule(IGameEngine engine, ILogger logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### SOLID Principles

#### Single Responsibility Principle

```csharp
// BAD: Class does too much
public class Player
{
    public void Move() { }
    public void Attack() { }
    public void SaveToDatabase() { }  // Database concern
    public void RenderOnScreen() { }  // UI concern
}

// GOOD: Separate responsibilities
public class Player
{
    public void Move() { }
    public void Attack() { }
}

public class PlayerRepository
{
    public void Save(Player player) { }
}

public class PlayerRenderer
{
    public void Render(Player player) { }
}
```

#### Open/Closed Principle

```csharp
// Open for extension, closed for modification
public abstract class CommandHandler
{
    public abstract bool CanHandle(string command);
    public abstract Task<object> HandleAsync(string command);
}

// Extend by adding new handlers
public class SpawnCommandHandler : CommandHandler
{
    public override bool CanHandle(string command) => command.StartsWith("spawn");
    public override Task<object> HandleAsync(string command) { /* ... */ }
}
```

---

## Module Development

See [MODULE_DEVELOPMENT_GUIDE.md](MODULE_DEVELOPMENT_GUIDE.md) for comprehensive module development documentation.

### Module Template

```csharp
using RaCore.Engine.Interfaces;
using RaCore.Attributes;

namespace RaCore.Modules.Extensions
{
    /// <summary>
    /// Brief description of what this module does.
    /// </summary>
    [RaModule(Category = "extensions")]
    public sealed class MyModule : ModuleBase
    {
        private bool _isInitialized;
        
        public override string Name => "MyModule";
        public override string Description => "Detailed module description";
        public override string Version => "1.0.0";
        
        public MyModule()
        {
            _logger = Logger.Instance;
        }
        
        public override async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInfo($"Initializing {Name}...");
                
                // Initialization logic here
                
                _isInitialized = true;
                _logger.LogSuccess($"{Name} initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize {Name}: {ex.Message}");
                return false;
            }
        }
        
        public override async Task<object?> Process(
            string input,
            Dictionary<string, object> context)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning($"{Name} not initialized");
                return null;
            }
            
            try
            {
                // Command processing logic
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return null;
                
                var command = parts[0].ToLowerInvariant();
                
                return command switch
                {
                    "status" => GetStatus(),
                    "help" => GetHelp(),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing command: {ex.Message}");
                return null;
            }
        }
        
        private object GetStatus()
        {
            return new
            {
                Module = Name,
                Version = Version,
                Initialized = _isInitialized
            };
        }
        
        private string GetHelp()
        {
            return $@"
{Name} v{Version}

Commands:
  status  - Show module status
  help    - Show this help message
";
        }
        
        public override void Dispose()
        {
            _logger.LogInfo($"Disposing {Name}...");
            // Cleanup resources
            _isInitialized = false;
        }
    }
}
```

---

## API Development

### RESTful Design

```csharp
// Use standard HTTP methods
[HttpGet("api/players")]          // GET - Retrieve
[HttpPost("api/players")]         // POST - Create
[HttpPut("api/players/{id}")]     // PUT - Update (full)
[HttpPatch("api/players/{id}")]   // PATCH - Update (partial)
[HttpDelete("api/players/{id}")]  // DELETE - Remove

// Use nouns for resources, not verbs
// GOOD:
GET /api/players
POST /api/players
GET /api/players/123

// BAD:
GET /api/getPlayers
POST /api/createPlayer
GET /api/getPlayerById?id=123
```

### API Versioning

```csharp
// Version in URL
[HttpGet("api/v1/players")]
[HttpGet("api/v2/players")]

// Or use header versioning
[ApiVersion("1.0")]
[ApiVersion("2.0")]
```

### Response Format

```csharp
// Consistent response structure
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

// Usage
[HttpGet("api/players/{id}")]
public async Task<IActionResult> GetPlayer(int id)
{
    var player = await _service.GetPlayerAsync(id);
    
    if (player == null)
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = "Player not found"
        });
    }
    
    return Ok(new ApiResponse<Player>
    {
        Success = true,
        Data = player
    });
}
```

---

## Testing Standards

See [TESTING_STRATEGY.md](TESTING_STRATEGY.md) for complete testing guidelines.

### Unit Testing

```csharp
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(2, 3);
    
    // Assert
    Assert.Equal(5, result);
}

[Theory]
[InlineData(0, 0, 0)]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
public void Add_VariousInputs_ReturnsCorrectSum(int a, int b, int expected)
{
    var calculator = new Calculator();
    var result = calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### Integration Testing

```csharp
public class ModuleIntegrationTests : IClassFixture<TestServerFixture>
{
    private readonly TestServerFixture _fixture;
    
    public ModuleIntegrationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Module_Initialize_LoadsSuccessfully()
    {
        // Arrange
        var module = _fixture.GetModule<GameEngineModule>();
        
        // Act
        var result = await module.InitializeAsync();
        
        // Assert
        Assert.True(result);
    }
}
```

---

## Security Best Practices

### Input Validation

```csharp
// Always validate and sanitize input
public async Task<object> ProcessCommand(string command)
{
    // Validate
    if (string.IsNullOrWhiteSpace(command))
        throw new ArgumentException("Command cannot be empty", nameof(command));
    
    if (command.Length > 1000)
        throw new ArgumentException("Command too long", nameof(command));
    
    // Sanitize
    command = command.Trim();
    command = Regex.Replace(command, @"[^\w\s-]", "");
    
    // Process
    return await ExecuteCommandAsync(command);
}
```

### Authentication and Authorization

```csharp
// Check permissions
public async Task<bool> CanExecute(string userId, string action)
{
    var user = await _userService.GetUserAsync(userId);
    if (user == null) return false;
    
    var permissions = await _permissionService.GetPermissionsAsync(user.Role);
    return permissions.Contains(action);
}

// Use attributes for authorization
[RequirePermission("admin.system.restart")]
public async Task<IActionResult> RestartSystem()
{
    // Only users with permission can access
}
```

### Secure Data Storage

```csharp
// Never store passwords in plain text
public async Task<bool> RegisterUser(string username, string password)
{
    // Hash password before storage
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    
    var user = new User
    {
        Username = username,
        PasswordHash = hashedPassword
    };
    
    await _repository.SaveAsync(user);
    return true;
}

// Verify password
public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

---

## Performance Guidelines

### Async/Await Best Practices

```csharp
// Use async for I/O operations
public async Task<string> FetchDataAsync()
{
    using var client = new HttpClient();
    var response = await client.GetStringAsync(url);
    return response;
}

// Don't block async code
// BAD
var result = SomeAsyncMethod().Result;  // Can cause deadlock

// GOOD
var result = await SomeAsyncMethod();
```

### Caching

```csharp
// Cache expensive operations
private readonly ConcurrentDictionary<string, Player> _playerCache = new();

public async Task<Player?> GetPlayerAsync(string id)
{
    // Check cache first
    if (_playerCache.TryGetValue(id, out var cachedPlayer))
        return cachedPlayer;
    
    // Load from database
    var player = await _repository.GetByIdAsync(id);
    if (player != null)
    {
        _playerCache.TryAdd(id, player);
    }
    
    return player;
}
```

### Resource Management

```csharp
// Always dispose resources
public async Task ProcessFile(string path)
{
    using var stream = File.OpenRead(path);
    using var reader = new StreamReader(stream);
    
    var content = await reader.ReadToEndAsync();
    // Process content
}

// Or use async dispose
public async Task ProcessFileAsync(string path)
{
    await using var stream = File.OpenRead(path);
    // Process stream
}
```

---

## Debugging Tips

### Logging

```csharp
// Use appropriate log levels
_logger.LogDebug("Detailed diagnostic info");
_logger.LogInfo("General information");
_logger.LogWarning("Warning condition");
_logger.LogError("Error occurred");

// Include context in logs
_logger.LogInfo($"Processing command: {command} for user: {userId}");

// Log exceptions with full stack trace
try
{
    // Operation
}
catch (Exception ex)
{
    _logger.LogError($"Operation failed: {ex}");
    throw;
}
```

### Debugging Modules

```bash
# Run with debugging
dotnet run --configuration Debug

# Attach debugger in VS Code
# Set breakpoints in code
# Press F5 to start debugging
```

---

## Code Review Checklist

Use this checklist when reviewing pull requests:

### Functionality
- [ ] Code accomplishes stated goal
- [ ] Edge cases handled
- [ ] Error handling implemented
- [ ] No obvious bugs

### Code Quality
- [ ] Follows coding standards
- [ ] Well-organized and readable
- [ ] Appropriate use of design patterns
- [ ] No code duplication

### Testing
- [ ] Unit tests included
- [ ] Tests are meaningful
- [ ] All tests pass
- [ ] Coverage is adequate

### Documentation
- [ ] XML documentation for public APIs
- [ ] README updated if needed
- [ ] Complex logic explained
- [ ] Examples provided

### Security
- [ ] Input validated
- [ ] No security vulnerabilities
- [ ] Sensitive data protected
- [ ] Permissions checked

### Performance
- [ ] No obvious performance issues
- [ ] Async/await used properly
- [ ] Resources disposed properly
- [ ] Caching considered

---

## Deployment Procedures

See [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) for complete deployment documentation.

### Build for Production

```bash
# Build in Release mode
dotnet build -c Release

# Publish self-contained
dotnet publish -c Release --self-contained true -r linux-x64

# Or use build scripts
./build-linux-production.sh
```

### Pre-Deployment Checklist

- [ ] All tests pass
- [ ] Version number updated
- [ ] CHANGELOG updated
- [ ] Documentation current
- [ ] Security review completed
- [ ] Performance tested
- [ ] Backup created

---

## Additional Resources

- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [MODULE_DEVELOPMENT_GUIDE.md](MODULE_DEVELOPMENT_GUIDE.md) - Module creation
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
- [TESTING_STRATEGY.md](TESTING_STRATEGY.md) - Testing approach
- [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) - Security details

---

**Last Updated:** January 2025  
**Version:** 9.3.2  
**Maintained By:** RaOS Development Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
