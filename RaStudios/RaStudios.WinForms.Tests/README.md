# Testing RaStudios.WinForms

## Running Tests

### On Windows
```bash
dotnet test RaStudios.WinForms.sln
```

### On Linux/macOS
The tests require the Windows Desktop framework and cannot be run on non-Windows platforms. 
The tests are designed to run in a Windows environment or Windows CI/CD pipeline.

For development on non-Windows platforms:
1. Build the project: `dotnet build RaStudios.WinForms.sln`
2. Run tests in a Windows VM or Windows CI environment
3. Use cross-platform testing where possible (for business logic)

## Test Coverage

The test suite includes:

### Unit Tests (`CoreTests.cs`)
- **ServerConnectorTests**: Tests for server connection logic
  - Initial state verification
  - URL configuration
  - Connection state management

- **AiAgentTests**: Tests for AI agent functionality
  - Constructor validation
  - Endpoint configuration
  - Code approval workflow
  - Security controls

- **CodeGenerationResultTests**: Tests for code generation models
  - Default security state
  - Unique ID generation
  - Approval tracking

- **ServerConfigurationTests**: Tests for server configuration
  - Default secure settings
  - Authentication requirements
  - Rate limiting

- **AiServiceConfigurationTests**: Tests for AI service configuration
  - Approval requirements
  - Policy filter defaults
  - Rate limiting

## Integration Tests

Integration tests should be added for:
- [ ] WebSocket connection to game server
- [ ] AI API integration
- [ ] DirectX 11 rendering
- [ ] End-to-end code generation and deployment workflow

## Security Testing

All tests verify that:
- ✅ Human approval is required for code generation
- ✅ Rate limiting is enabled by default
- ✅ Authentication is enabled by default
- ✅ Policy filters are active
- ✅ No auto-execution of generated code

## Adding New Tests

When adding new features, ensure tests cover:
1. Security controls (approval, validation, rate limiting)
2. Error handling
3. Configuration defaults are secure
4. Audit trail is maintained
