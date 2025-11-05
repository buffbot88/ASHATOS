# Configuration Guide for RaStudios.WinForms

This document provides detailed information about the `config.json` configuration file used by RaStudios.WinForms.

## Overview

The `config.json` file is the central configuration file for RaStudios.WinForms. It controls connections to ASHAT OS, FTP servers, build settings, AI services, and security policies.

## File Location

The `config.json` file should be placed in the same directory as the RaStudios.WinForms executable. It is automatically copied to the output directory during build.

## Configuration Sections

### 1. ASHAT OS Connection

Configures the connection to the ASHAT OS backend server.

```json
{
  "AshatOS": {
    "Host": "localhost",
    "Port": 7077,
    "Protocol": "WebSocket",
    "WebSocketUrl": "ws://localhost:7077/ws",
    "ApiBaseUrl": "http://localhost:7077/api",
    "TimeoutSeconds": 30,
    "UseAuthentication": true,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 5
  }
}
```

**Fields:**
- `Host`: Hostname or IP address of the ASHAT OS server
- `Port`: Port number for the connection
- `Protocol`: Communication protocol (WebSocket, HTTP, etc.)
- `WebSocketUrl`: Full WebSocket URL for real-time communication
- `ApiBaseUrl`: Base URL for REST API calls
- `TimeoutSeconds`: Connection timeout in seconds
- `UseAuthentication`: Whether authentication is required
- `RetryAttempts`: Number of connection retry attempts
- `RetryDelaySeconds`: Delay between retry attempts

### 2. FTP Configuration

Configures FTP server settings for uploading built DLLs.

```json
{
  "FTP": {
    "Host": "localhost",
    "Port": 21,
    "Username": "",
    "Password": "",
    "UseSSL": true,
    "UploadPath": "/uploads/dlls",
    "TimeoutSeconds": 60,
    "PassiveMode": true
  }
}
```

**Fields:**
- `Host`: FTP server hostname or IP address
- `Port`: FTP server port (default: 21)
- `Username`: FTP username for authentication
- `Password`: FTP password for authentication
- `UseSSL`: Enable SSL/TLS encryption (FTPS)
- `UploadPath`: Remote directory path for uploads
- `TimeoutSeconds`: FTP operation timeout
- `PassiveMode`: Use passive mode for data connections

**Security Notes:**
- ⚠️ **Never commit passwords to source control**
- Use environment variables for credentials in production
- Consider using Windows Credential Manager or Azure Key Vault
- Enable `UseSSL` in production environments

### 3. Build Configuration

Configures the .NET build service for compiling code.

```json
{
  "Build": {
    "DotNetPath": "dotnet",
    "BuildConfiguration": "Release",
    "OutputPath": "./bin/Release",
    "TargetFramework": "net9.0",
    "EnableNuGetRestore": true
  }
}
```

**Fields:**
- `DotNetPath`: Path to the dotnet CLI executable
- `BuildConfiguration`: Build configuration (Debug or Release)
- `OutputPath`: Output directory for built assemblies
- `TargetFramework`: Target .NET framework version
- `EnableNuGetRestore`: Restore NuGet packages before building

### 4. Server Configuration

Legacy server connection settings (maintained for compatibility).

```json
{
  "Server": {
    "Url": "ws://localhost:7077/ws",
    "Port": 7077,
    "Protocol": "WebSocket",
    "HandshakeType": "JSON",
    "TimeoutSeconds": 30,
    "UseAuthentication": true,
    "EnableRateLimiting": true,
    "MaxMessagesPerSecond": 10
  }
}
```

**Fields:**
- `Url`: WebSocket server URL
- `Port`: Server port number
- `Protocol`: Communication protocol
- `HandshakeType`: Message format (JSON, XML, etc.)
- `TimeoutSeconds`: Connection timeout
- `UseAuthentication`: Enable authentication
- `EnableRateLimiting`: Enable rate limiting
- `MaxMessagesPerSecond`: Maximum messages per second

### 5. AI Service Configuration

Configures the AI code generation service.

```json
{
  "AiService": {
    "Endpoint": "http://localhost:8080/api/ai/generate",
    "ApiKey": "",
    "MaxRequestsPerMinute": 10,
    "RequireApproval": true,
    "EnablePolicyFilter": true,
    "BlockedPatterns": [
      "rm -rf",
      "del /f",
      "format c:",
      "drop database",
      "delete from",
      "__import__('os').system",
      "eval(",
      "exec(",
      "Process.Start"
    ]
  }
}
```

**Fields:**
- `Endpoint`: AI service API endpoint URL
- `ApiKey`: API key for authentication (optional)
- `MaxRequestsPerMinute`: Rate limit for AI requests
- `RequireApproval`: Always require human approval (recommended: true)
- `EnablePolicyFilter`: Enable dangerous pattern filtering
- `BlockedPatterns`: List of patterns to block in prompts/code

### 6. Security Configuration

Global security settings for the application.

```json
{
  "Security": {
    "RequireHumanApproval": true,
    "EnableAuditLogging": true,
    "AllowAutoExecution": false,
    "AllowSelfReplication": false
  }
}
```

**Fields:**
- `RequireHumanApproval`: Require human approval for all AI code (recommended: true)
- `EnableAuditLogging`: Log all code approvals and deployments
- `AllowAutoExecution`: Allow automatic code execution (recommended: false)
- `AllowSelfReplication`: Allow self-modifying code (recommended: false)

### 7. Updates Configuration

Configures application update settings.

```json
{
  "Updates": {
    "Strategy": "Manual",
    "CheckForUpdates": true,
    "AutoDownload": false,
    "RequireSignedPackages": true
  }
}
```

**Fields:**
- `Strategy`: Update strategy (Manual, Automatic)
- `CheckForUpdates`: Check for updates on startup
- `AutoDownload`: Automatically download updates
- `RequireSignedPackages`: Only install signed updates

## Loading Configuration

### At Startup
The configuration is automatically loaded when the application starts.

### Runtime Reload
To reload configuration at runtime:

**Via Terminal:**
```
> loadconfig
```

**Via Code:**
```csharp
ConfigurationService.Instance.LoadConfiguration();
```

## Best Practices

1. **Never commit sensitive data**: Keep credentials out of source control
2. **Use environment variables**: For production deployments
3. **Enable SSL/TLS**: Always use secure connections in production
4. **Regular backups**: Keep backups of your configuration
5. **Validate changes**: Test configuration changes in a safe environment
6. **Document customizations**: Comment any non-standard settings

## Example: Production Configuration

```json
{
  "AshatOS": {
    "Host": "ashat-os.example.com",
    "Port": 443,
    "Protocol": "WebSocket",
    "WebSocketUrl": "wss://ashat-os.example.com/ws",
    "ApiBaseUrl": "https://ashat-os.example.com/api",
    "TimeoutSeconds": 30,
    "UseAuthentication": true,
    "RetryAttempts": 5,
    "RetryDelaySeconds": 10
  },
  "FTP": {
    "Host": "ftp.example.com",
    "Port": 990,
    "Username": "deploy_user",
    "Password": "",  // Use environment variable: FTP_PASSWORD
    "UseSSL": true,
    "UploadPath": "/production/dlls",
    "TimeoutSeconds": 120,
    "PassiveMode": true
  }
}
```

## Troubleshooting

### Configuration Not Loading
1. Check that `config.json` exists in the application directory
2. Validate JSON syntax using a JSON validator
3. Check application logs for detailed error messages

### Connection Failures
1. Verify host and port settings
2. Check firewall rules
3. Ensure SSL/TLS certificates are valid
4. Test connectivity with `ping` or `telnet`

### Build Failures
1. Verify `dotnet` is in PATH
2. Check TargetFramework matches installed SDK
3. Ensure write permissions in output directory

### FTP Upload Failures
1. Test FTP credentials manually
2. Check network connectivity to FTP server
3. Verify SSL/TLS settings
4. Ensure upload directory exists and has write permissions

## Support

For additional help, refer to:
- [Main README](../README.md)
- [WinForms README](README.md)
- [Testing Guide](../RaStudios.WinForms.Tests/README.md)
