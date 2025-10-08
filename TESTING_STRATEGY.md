# 🧪 Testing Strategy

**Version:** 9.4.0  
**Last Updated:** January 2025  
**Status:** Production Ready

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Testing Philosophy](#testing-philosophy)
3. [Testing Types](#testing-types)
4. [Unit Testing](#unit-testing)
5. [Integration Testing](#integration-testing)
6. [End-to-End Testing](#end-to-end-testing)
7. [Performance Testing](#performance-testing)
8. [Security Testing](#security-testing)
9. [Manual Testing](#manual-testing)
10. [Test Coverage](#test-coverage)
11. [Continuous Integration](#continuous-integration)
12. [Best Practices](#best-practices)

---

## Overview

This document outlines the comprehensive testing strategy for RaOS. Following these guidelines ensures code quality, reliability, and maintainability across all modules and components.

### Testing Goals

- 🎯 **Quality Assurance**: Catch bugs before they reach production
- 🔒 **Reliability**: Ensure system behaves as expected
- 📚 **Documentation**: Tests serve as living documentation
- 🚀 **Confidence**: Deploy with confidence knowing tests pass
- 🔄 **Regression Prevention**: Prevent old bugs from reappearing

---

## Testing Philosophy

### Guiding Principles

1. **Test Early, Test Often**: Write tests as you develop
2. **Test What Matters**: Focus on behavior, not implementation
3. **Maintainable Tests**: Tests should be easy to understand and maintain
4. **Fast Feedback**: Tests should run quickly
5. **Reliable Tests**: Tests should be deterministic (no flaky tests)

### Testing Pyramid

```
        /\
       /E2E\        Few - Slow - Expensive
      /------\
     /  Integ \     Some - Medium Speed
    /----------\
   /   Unit     \   Many - Fast - Cheap
  /--------------\
```

- **70%** Unit Tests - Fast, isolated, focused
- **20%** Integration Tests - Component interaction
- **10%** End-to-End Tests - Full system scenarios

---

## Testing Types

### 1. Unit Tests

**Purpose**: Test individual methods and classes in isolation

**Characteristics**:
- Fast execution (< 100ms per test)
- No external dependencies
- Focused on single unit of work
- Easy to maintain

**When to Use**:
- Testing business logic
- Validating algorithms
- Testing utility functions
- Verifying error handling

### 2. Integration Tests

**Purpose**: Test interaction between components

**Characteristics**:
- Slower than unit tests
- May use real dependencies (database, file system)
- Test component boundaries
- Verify data flows

**When to Use**:
- Testing module interactions
- Database operations
- API endpoint behavior
- File system operations

### 3. End-to-End Tests

**Purpose**: Test complete user scenarios

**Characteristics**:
- Slowest test type
- Full system testing
- User perspective
- Complex setup/teardown

**When to Use**:
- Critical user workflows
- Release validation
- Smoke tests for production

### 4. Performance Tests

**Purpose**: Verify performance requirements

**Characteristics**:
- Measure response times
- Load testing
- Stress testing
- Resource usage monitoring

**When to Use**:
- API performance validation
- Module initialization benchmarks
- Database query optimization
- Memory leak detection

### 5. Security Tests

**Purpose**: Identify security vulnerabilities

**Characteristics**:
- Input validation testing
- Authentication/authorization tests
- SQL injection prevention
- XSS/CSRF protection

**When to Use**:
- New security features
- Input handling code
- Authentication changes
- API endpoints

---

## Unit Testing

### Test Framework

We use **xUnit** for C# testing:

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq  # For mocking
```

### Unit Test Structure

Follow the **AAA Pattern** (Arrange, Act, Assert):

```csharp
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        var a = 5;
        var b = 3;
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.Equal(8, result);
    }
}
```

### Parameterized Tests

Use `[Theory]` for testing multiple inputs:

```csharp
[Theory]
[InlineData(0, 0, 0)]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(100, -50, 50)]
public void Add_VariousInputs_ReturnsCorrectSum(int a, int b, int expected)
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(a, b);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Testing Exceptions

```csharp
[Fact]
public void Divide_ByZero_ThrowsException()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act & Assert
    Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
}
```

### Async Testing

```csharp
[Fact]
public async Task InitializeAsync_ValidConfig_ReturnsTrue()
{
    // Arrange
    var module = new TestModule();
    
    // Act
    var result = await module.InitializeAsync();
    
    // Assert
    Assert.True(result);
}
```

### Mocking Dependencies

Use **Moq** for creating mock objects:

```csharp
using Moq;

[Fact]
public void ProcessCommand_CallsLogger()
{
    // Arrange
    var mockLogger = new Mock<ILogger>();
    var module = new TestModule(mockLogger.Object);
    
    // Act
    module.ProcessCommand("test");
    
    // Assert
    mockLogger.Verify(
        x => x.LogInfo(It.IsAny<string>()),
        Times.Once);
}
```

### Test Naming Convention

Use descriptive test names that indicate:
1. Method being tested
2. Scenario or input
3. Expected outcome

Format: `MethodName_Scenario_ExpectedResult`

Examples:
- `Add_TwoPositiveNumbers_ReturnsSum`
- `GetPlayer_InvalidId_ReturnsNull`
- `InitializeAsync_AlreadyInitialized_ReturnsFalse`

---

## Integration Testing

### Setting Up Integration Tests

```csharp
public class ModuleIntegrationTests : IClassFixture<TestServerFixture>
{
    private readonly TestServerFixture _fixture;
    
    public ModuleIntegrationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Module_LoadAndInitialize_Success()
    {
        // Arrange
        var moduleManager = _fixture.GetService<ModuleManager>();
        
        // Act
        var result = await moduleManager.LoadModuleAsync("TestModule");
        
        // Assert
        Assert.True(result);
    }
}
```

### Database Testing

```csharp
[Fact]
public async Task SavePlayer_ValidData_PersistsToDatabase()
{
    // Arrange
    using var context = CreateTestDbContext();
    var repository = new PlayerRepository(context);
    var player = new Player { Name = "TestPlayer", Level = 1 };
    
    // Act
    await repository.SaveAsync(player);
    
    // Assert
    var savedPlayer = await repository.GetByNameAsync("TestPlayer");
    Assert.NotNull(savedPlayer);
    Assert.Equal(1, savedPlayer.Level);
}

private TestDbContext CreateTestDbContext()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    return new TestDbContext(options);
}
```

### API Testing

```csharp
[Fact]
public async Task GetPlayer_ValidId_ReturnsPlayer()
{
    // Arrange
    var client = _fixture.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/players/1");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"id\":1", content);
}
```

---

## End-to-End Testing

### User Scenario Testing

```csharp
[Fact]
public async Task CompleteGameFlow_CreateAndPlay()
{
    // Arrange
    var client = _fixture.CreateClient();
    
    // Act 1: Create player
    var createResponse = await client.PostAsync(
        "/api/players",
        new StringContent(JsonSerializer.Serialize(new { name = "Hero" })));
    Assert.True(createResponse.IsSuccessStatusCode);
    
    // Act 2: Create scene
    var sceneResponse = await client.PostAsync(
        "/api/scenes",
        new StringContent(JsonSerializer.Serialize(new { name = "World1" })));
    Assert.True(sceneResponse.IsSuccessStatusCode);
    
    // Act 3: Join scene
    var joinResponse = await client.PostAsync("/api/scenes/1/join", null);
    Assert.True(joinResponse.IsSuccessStatusCode);
    
    // Assert: Verify complete flow
    var statusResponse = await client.GetAsync("/api/players/1/status");
    var status = await statusResponse.Content.ReadAsStringAsync();
    Assert.Contains("\"currentScene\":1", status);
}
```

---

## Performance Testing

### Benchmarking

```csharp
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class ModuleBenchmarks
{
    private TestModule _module;
    
    [GlobalSetup]
    public void Setup()
    {
        _module = new TestModule();
        _module.InitializeAsync().Wait();
    }
    
    [Benchmark]
    public async Task ProcessCommand_Performance()
    {
        await _module.Process("test command", new Dictionary<string, object>());
    }
}
```

### Load Testing

```csharp
[Fact]
public async Task API_HandlesHighLoad()
{
    // Arrange
    var client = _fixture.CreateClient();
    var tasks = new List<Task<HttpResponseMessage>>();
    
    // Act: Send 1000 concurrent requests
    for (int i = 0; i < 1000; i++)
    {
        tasks.Add(client.GetAsync("/api/status"));
    }
    
    var responses = await Task.WhenAll(tasks);
    
    // Assert
    Assert.All(responses, r => Assert.True(r.IsSuccessStatusCode));
}
```

---

## Security Testing

### Input Validation Tests

```csharp
[Theory]
[InlineData("'; DROP TABLE users; --")]
[InlineData("<script>alert('xss')</script>")]
[InlineData("../../../etc/passwd")]
public async Task ProcessInput_MaliciousInput_RejectsOrSanitizes(string maliciousInput)
{
    // Arrange
    var module = new TestModule();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        async () => await module.ProcessCommand(maliciousInput));
}
```

### Authentication Tests

```csharp
[Fact]
public async Task ProtectedEndpoint_NoToken_ReturnsUnauthorized()
{
    // Arrange
    var client = _fixture.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/admin/users");
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

## Manual Testing

### Manual Test Checklist

When manual testing is required (e.g., UI, user experience):

#### Module Loading
- [ ] Module initializes successfully
- [ ] No errors in console/logs
- [ ] Module appears in status output
- [ ] Module responds to commands

#### API Testing
- [ ] Endpoints return expected responses
- [ ] Error handling works correctly
- [ ] Rate limiting functions properly
- [ ] Authentication/authorization works

#### Performance
- [ ] Acceptable response times
- [ ] No memory leaks
- [ ] Handles expected load
- [ ] Graceful degradation under stress

#### Security
- [ ] Input validation working
- [ ] Authentication required where needed
- [ ] No sensitive data exposed
- [ ] Logs don't contain secrets

### Manual Testing Workflow

1. **Setup**: Clean environment, fresh build
2. **Execute**: Follow test cases step by step
3. **Observe**: Note any unexpected behavior
4. **Document**: Record results, screenshots
5. **Report**: File issues for any problems

---

## Test Coverage

### Coverage Goals

- **Minimum**: 70% overall code coverage
- **Target**: 80%+ for critical paths
- **Critical Components**: 90%+ coverage

### Measuring Coverage

```bash
# Install coverage tool
dotnet add package coverlet.collector

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# View coverage in browser (with ReportGenerator)
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

### Coverage Exclusions

Exclude from coverage:
- Generated code
- Simple DTOs/POCOs
- Main entry points
- Configuration classes
- Third-party code

---

## Continuous Integration

### CI Pipeline

```yaml
# Example GitHub Actions workflow
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

### Test Execution Strategy

1. **Fast Tests First**: Run unit tests immediately
2. **Integration Tests**: Run if unit tests pass
3. **E2E Tests**: Run only on main branch or release
4. **Parallel Execution**: Run tests in parallel when possible

---

## Best Practices

### Do's ✅

- **Write tests first** (TDD when appropriate)
- **Keep tests simple** and focused
- **Use descriptive names** for tests
- **Test edge cases** and error conditions
- **Mock external dependencies** in unit tests
- **Clean up resources** after tests
- **Run tests frequently** during development
- **Maintain tests** as code evolves

### Don'ts ❌

- **Don't skip tests** because they're hard to write
- **Don't test implementation details** - test behavior
- **Don't share state** between tests
- **Don't make tests dependent** on each other
- **Don't ignore failing tests** - fix or remove them
- **Don't test framework code** - test your code
- **Don't over-mock** - use real objects when practical
- **Don't write slow tests** without good reason

### Test Readability

```csharp
// ❌ BAD: Unclear test
[Fact]
public void Test1()
{
    var x = new Module();
    var y = x.Do("abc");
    Assert.True(y);
}

// ✅ GOOD: Clear, descriptive test
[Fact]
public void ProcessCommand_ValidInput_ReturnsSuccessResult()
{
    // Arrange
    var module = new GameModule();
    var command = "spawn npc warrior";
    
    // Act
    var result = module.ProcessCommand(command);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Equal("npc_warrior", result.EntityId);
}
```

### Test Independence

```csharp
// ❌ BAD: Tests share state
private static int _counter = 0;

[Fact]
public void Test1() { _counter++; Assert.Equal(1, _counter); }

[Fact]
public void Test2() { _counter++; Assert.Equal(2, _counter); }

// ✅ GOOD: Tests are independent
[Fact]
public void Test1()
{
    var counter = 0;
    counter++;
    Assert.Equal(1, counter);
}

[Fact]
public void Test2()
{
    var counter = 0;
    counter++;
    Assert.Equal(1, counter);
}
```

---

## Test Organization

### Directory Structure

```
TheRaProject.Tests/
├── Unit/
│   ├── Core/
│   │   └── ModuleManagerTests.cs
│   ├── Modules/
│   │   ├── CMSModuleTests.cs
│   │   └── GameEngineTests.cs
│   └── Utils/
│       └── HelperTests.cs
├── Integration/
│   ├── API/
│   │   └── EndpointTests.cs
│   └── Database/
│       └── RepositoryTests.cs
├── E2E/
│   └── Scenarios/
│       └── GameFlowTests.cs
├── Fixtures/
│   └── TestServerFixture.cs
└── TestData/
    └── sample_data.json
```

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RaCore.Tests/RaCore.Tests.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~GameEngineTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~GameEngineTests.InitializeAsync_Success"

# Run with verbosity
dotnet test --verbosity detailed

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### IDE Integration

- **Visual Studio**: Test Explorer window
- **VS Code**: .NET Test Explorer extension
- **Rider**: Built-in test runner

---

## Troubleshooting Tests

### Flaky Tests

If tests fail intermittently:
1. Check for race conditions
2. Verify test independence
3. Look for timing issues
4. Ensure proper cleanup
5. Check for external dependencies

### Slow Tests

To improve test speed:
1. Use in-memory databases
2. Mock external services
3. Minimize I/O operations
4. Run tests in parallel
5. Use test fixtures properly

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Manual testing guide
- [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) - Development standards

---

**Last Updated:** January 2025  
**Version:** 9.4.0  
**Maintained By:** RaOS Development Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
