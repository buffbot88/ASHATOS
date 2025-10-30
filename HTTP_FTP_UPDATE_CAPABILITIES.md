# HTTP/FTP/PHP/Update Capabilities for ASHATOS AI Framework

## Overview

This document details the comprehensive networking and file transfer capabilities available in the ASHATOS AI Framework. These capabilities enable the AI system to interact with external services, manage file transfers, configure web servers, and handle system updates.

**Status**: ✅ All capabilities verified and tested (October 2025, year corrected from future timestamp)

---

## 1. HTTP Capabilities

### HttpHelper Class
**Location**: `ASHATCore/Engine/HttpHelper.cs`  
**Status**: ✅ Fully implemented and tested

The `HttpHelper` class provides comprehensive HTTP client functionality for the AI Framework.

### Features

#### 1.1 File Downloads
```csharp
// Download binary file
var result = await httpHelper.DownloadFileAsync("https://example.com/file.bin");
if (result.success)
{
    byte[] fileData = result.data;
    // Process file data
}

// Download text file
var textResult = await httpHelper.DownloadTextAsync("https://example.com/data.txt");
if (textResult.success)
{
    string content = textResult.content;
    // Process text content
}

// Download directly to disk
var diskResult = await httpHelper.DownloadToFileAsync(
    "https://example.com/file.zip", 
    "/path/to/local/file.zip"
);
```

#### 1.2 File Uploads
```csharp
// Upload file with multipart/form-data
byte[] fileData = File.ReadAllBytes("document.pdf");
var result = await httpHelper.UploadFileAsync(
    "https://api.example.com/upload",
    fileData,
    "document.pdf",
    "file" // form field name
);
```

#### 1.3 JSON API Interactions
```csharp
// POST JSON data
var data = new { 
    name = "ASHAT AI", 
    version = "9.9.9",
    timestamp = DateTime.UtcNow 
};
var result = await httpHelper.PostJsonAsync("https://api.example.com/data", data);
if (result.success)
{
    string response = result.response;
    // Parse response
}

// GET request
var getResult = await httpHelper.GetAsync("https://api.example.com/status");
if (getResult.success)
{
    Console.WriteLine($"Status Code: {getResult.statusCode}");
    Console.WriteLine($"Response: {getResult.response}");
}
```

#### 1.4 URL Testing
```csharp
// Test if URL is accessible
var testResult = await httpHelper.TestUrlAsync("https://example.com");
if (testResult.success)
{
    Console.WriteLine($"URL is accessible - Status: {testResult.statusCode}");
}
```

#### 1.5 Custom Headers & Authentication
```csharp
// Set custom headers
httpHelper.SetHeader("X-API-Key", "your-api-key");
httpHelper.SetHeader("X-Custom-Header", "value");

// Set authorization
httpHelper.SetAuthorizationHeader("Bearer", "your-token-here");

// All subsequent requests will include these headers
var result = await httpHelper.GetAsync("https://api.example.com/protected");
```

### Configuration

```csharp
// Create with custom timeout (default: 30 seconds)
using var httpHelper = new HttpHelper(timeoutSeconds: 60);

// HttpHelper implements IDisposable - use 'using' statement
```

### Use Cases for AI Framework

1. **Model Downloads**: Download AI models from remote repositories
2. **Data Collection**: Fetch training data from external sources
3. **API Integration**: Interact with external AI services and APIs
4. **Update Delivery**: Download system and model updates
5. **Telemetry**: Send usage statistics and performance metrics
6. **Remote Configuration**: Fetch configuration from remote sources

---

## 2. FTP Capabilities

### FtpHelper Class
**Location**: `ASHATCore/Engine/FtpHelper.cs`  
**Status**: ✅ Fully implemented and tested

The `FtpHelper` class provides FTP operations for file management on FTP servers.

### Features

#### 2.1 Connection Management
```csharp
// Create FTP helper with configuration
var config = new ServerConfiguration
{
    FtpHost = "ftp.example.com",
    FtpPort = 21,
    FtpUsername = "username",
    FtpPassword = "password"
};
var ftpHelper = new FtpHelper(config);

// Or with direct parameters
var ftpHelper = new FtpHelper("ftp.example.com", 21, "user", "pass");

// Test connection
var result = await ftpHelper.TestConnectionAsync();
if (result.success)
{
    Console.WriteLine("FTP server is accessible");
}
```

#### 2.2 File Operations
```csharp
// Upload file
var uploadResult = await ftpHelper.UploadFileAsync(
    "/local/path/file.txt",
    "/remote/path/file.txt"
);

// Download file
var downloadResult = await ftpHelper.DownloadFileAsync("/remote/path/file.txt");
if (downloadResult.success)
{
    byte[] fileData = downloadResult.data;
    File.WriteAllBytes("/local/path/downloaded.txt", fileData);
}

// Delete file
var deleteResult = await ftpHelper.DeleteFileAsync("/remote/path/file.txt");
```

#### 2.3 Directory Operations
```csharp
// List directory contents
var listResult = await ftpHelper.ListDirectoryAsync("/remote/directory");
if (listResult.success)
{
    foreach (var file in listResult.files)
    {
        Console.WriteLine($"File: {file}");
    }
}

// Create directory
var createResult = await ftpHelper.CreateDirectoryAsync("/remote/newdir");
```

### Use Cases for AI Framework

1. **Model Storage**: Upload trained models to FTP servers
2. **Backup Management**: Backup system data to remote FTP servers
3. **File Distribution**: Distribute generated content via FTP
4. **Legacy System Integration**: Interface with legacy systems using FTP
5. **Bulk Data Transfer**: Transfer large datasets efficiently

---

## 3. PHP Configuration Capabilities

### ApacheManager Class
**Location**: `ASHATCore/Engine/ApacheManager.cs`  
**Status**: ✅ Fully implemented and tested

The `ApacheManager` class manages Apache and PHP configuration for the AI Framework's web interface.

### Features

#### 3.1 PHP Detection
```csharp
// Scan for PHP configuration
var phpResult = ApacheManager.ScanForPhpConfig();
if (phpResult.found)
{
    Console.WriteLine($"PHP config found at: {phpResult.path}");
}

// Scan for PHP folder
var phpFolderResult = ApacheManager.ScanForPhpFolder();
if (phpFolderResult.found)
{
    Console.WriteLine($"PHP folder found at: {phpFolderResult.path}");
}
```

#### 3.2 Apache Detection
```csharp
// Scan for Apache configuration
var apacheResult = ApacheManager.ScanForApacheConfig();
if (apacheResult.found)
{
    Console.WriteLine($"Apache config found at: {apacheResult.path}");
}

// Check if Apache is available
bool isAvailable = ApacheManager.IsApacheAvailable();
Console.WriteLine($"Apache available: {isAvailable}");
```

#### 3.3 Configuration Verification
```csharp
// Verify Apache configuration
var verifyResult = ApacheManager.VerifyApacheConfig("/path/to/httpd.conf");
if (verifyResult.valid)
{
    Console.WriteLine("Apache configuration is valid");
}
else
{
    Console.WriteLine("Issues found:");
    foreach (var issue in verifyResult.issues)
    {
        Console.WriteLine($"  - {issue}");
    }
}

// Verify PHP configuration
var phpVerifyResult = ApacheManager.VerifyPhpConfig("/path/to/php.ini");
```

#### 3.4 Automatic Configuration
```csharp
// Automatically configure PHP settings
bool success = ApacheManager.ConfigurePhpIni("/path/to/php.ini");
if (success)
{
    Console.WriteLine("PHP configured with recommended settings");
    // Creates backup: php.ini.ASHATOS_backup_20251030123456
}
```

### Recommended PHP Settings

The `ConfigurePhpIni` method automatically sets:

- `memory_limit = 256M`
- `max_execution_time = 60`
- `max_input_time = 60`
- `post_max_size = 10M`
- `upload_max_filesize = 10M`
- `display_errors = On`
- `log_errors = On`
- `date.timezone = UTC`

### Platform Support

- **Windows**: Apache/PHP optional (Kestrel handles web serving)
- **Linux**: External Apache/PHP8 optional for PHP file execution
- **Cross-platform**: All scanning methods work on all platforms

### Use Cases for AI Framework

1. **Web Interface Configuration**: Ensure proper PHP settings for web UI
2. **Deployment Automation**: Auto-configure PHP during first run
3. **Environment Validation**: Verify correct server setup
4. **Troubleshooting**: Detect configuration issues automatically
5. **Documentation Generation**: Generate PHP content dynamically

---

## 4. Update Management Capabilities

### UpdateModule Class
**Location**: `ASHATCore/Modules/Extensions/Updates/UpdateModule.cs`  
**Status**: ✅ Fully implemented and tested

The `UpdateModule` manages version checking and update delivery from the mainframe.

### Features

#### 4.1 Version Checking
```csharp
var updateModule = new UpdateModule();
updateModule.Initialize(moduleManager);

// Check for updates
var updateInfo = await updateModule.CheckForUpdatesAsync(
    currentVersion: "9.9.9",
    licenseKey: "YOUR-LICENSE-KEY"
);

if (updateInfo.UpdateAvailable)
{
    Console.WriteLine($"Update available: {updateInfo.LatestVersion}");
    Console.WriteLine($"Current version: {updateInfo.CurrentVersion}");
    Console.WriteLine($"Changelog: {updateInfo.Changelog}");
    Console.WriteLine($"Download URL: {updateInfo.DownloadUrl}");
    Console.WriteLine($"Size: {updateInfo.SizeBytes} bytes");
}
```

#### 4.2 Update Package Creation
```csharp
// Create update package
byte[] packageData = File.ReadAllBytes("ASHATOS_v9.10.0.zip");
var package = await updateModule.CreateUpdatePackageAsync(
    version: "9.10.0",
    changelog: "Added new AI capabilities, fixed bugs, improved performance",
    packageData: packageData
);

Console.WriteLine($"Package created: {package.Id}");
Console.WriteLine($"Version: {package.Version}");
Console.WriteLine($"Size: {package.SizeBytes} bytes");
Console.WriteLine($"Checksum: {package.ChecksumSHA256}");
```

#### 4.3 Package Management
```csharp
// Get specific update package
var package = updateModule.GetUpdatePackage("9.10.0");
if (package != null)
{
    Console.WriteLine($"Package found: {package.Version}");
    Console.WriteLine($"Status: {package.Status}");
    Console.WriteLine($"Mandatory: {package.IsMandatory}");
}

// List all updates
var allUpdates = updateModule.GetAllUpdates();
foreach (var update in allUpdates)
{
    Console.WriteLine($"{update.Version} - {update.Status} - {update.CreatedAt}");
}
```

#### 4.4 Package Integrity Verification
```csharp
// Verify package integrity using SHA256 checksum
bool isValid = updateModule.VerifyPackageIntegrity(
    version: "9.10.0",
    checksum: "ABC123DEF456..." // SHA256 hash
);

if (isValid)
{
    Console.WriteLine("Package integrity verified");
}
else
{
    Console.WriteLine("WARNING: Package integrity check failed!");
}
```

#### 4.5 Update Statistics
```csharp
// Console command
string stats = updateModule.Process("update stats");
Console.WriteLine(stats);

// Returns JSON:
// {
//   "CurrentVersion": "9.9.9",
//   "TotalUpdates": 5,
//   "ActiveUpdates": 3,
//   "DeprecatedUpdates": 2,
//   "LatestVersion": "9.10.0"
// }
```

### Console Commands

The UpdateModule supports console commands:

```bash
# Check for updates
update check <current-version> <license-key>

# List all available updates
update list

# Show update statistics
update stats

# Get help
help
```

### Update Package Structure

Each update package includes:

- **Id**: Unique identifier (GUID)
- **Version**: Semantic version string (e.g., "9.10.0")
- **Changelog**: Human-readable change description
- **PackagePath**: Local file system path to .zip package
- **SizeBytes**: Package size in bytes
- **CreatedAt**: Creation timestamp (UTC)
- **ChecksumSHA256**: SHA-256 hash for integrity verification
- **IsMandatory**: Whether update is required
- **Status**: Active, Deprecated, or Revoked

### Security Features

1. **License Validation**: Updates require valid, active license
2. **Checksum Verification**: SHA-256 integrity checks
3. **Version Comparison**: Semantic version parsing
4. **Access Control**: License-based update access
5. **Audit Trail**: All update operations logged

### Use Cases for AI Framework

1. **Model Updates**: Deliver new AI model versions
2. **System Updates**: Push system upgrades to deployed instances
3. **Hot Fixes**: Rapidly deploy critical security patches
4. **Feature Rollout**: Gradual feature deployment
5. **Rollback Support**: Version management for rollback scenarios

---

## 5. Integration Examples

### Example 1: Download AI Model via HTTP
```csharp
using var httpHelper = new HttpHelper(timeoutSeconds: 300); // 5 minutes for large files

// Download model file
var result = await httpHelper.DownloadToFileAsync(
    "https://models.ashatos.ai/latest/model.gguf",
    "/models/downloaded_model.gguf"
);

if (result.success)
{
    Console.WriteLine("AI model downloaded successfully");
    // Load model into AI system
}
```

### Example 2: Upload Training Results to FTP
```csharp
var ftpHelper = new FtpHelper("ftp.backup.com", 21, "ai_backup", "secure_pass");

// Upload training results
var result = await ftpHelper.UploadFileAsync(
    "/local/training_results.json",
    "/backups/training/results_2025_10_30.json"
);

if (result.success)
{
    Console.WriteLine("Training results backed up");
}
```

### Example 3: Check and Apply Updates
```csharp
var updateModule = new UpdateModule();
updateModule.Initialize(moduleManager);

// Check for updates
var updateInfo = await updateModule.CheckForUpdatesAsync(
    ASHATVersion.Current,
    licenseKey
);

if (updateInfo.UpdateAvailable)
{
    // Download update via HTTP
    using var httpHelper = new HttpHelper();
    var downloadResult = await httpHelper.DownloadFileAsync(updateInfo.DownloadUrl);
    
    if (downloadResult.success)
    {
        // Verify integrity
        var package = updateModule.GetUpdatePackage(updateInfo.LatestVersion);
        var isValid = updateModule.VerifyPackageIntegrity(
            updateInfo.LatestVersion,
            ComputeSHA256(downloadResult.data)
        );
        
        if (isValid)
        {
            // Apply update
            ApplyUpdate(downloadResult.data);
        }
    }
}
```

### Example 4: Configure PHP for AI Web Interface
```csharp
// Scan for PHP
var phpResult = ApacheManager.ScanForPhpConfig();

if (phpResult.found && phpResult.path != null)
{
    // Verify current configuration
    var verifyResult = ApacheManager.VerifyPhpConfig(phpResult.path);
    
    if (!verifyResult.valid)
    {
        Console.WriteLine("PHP configuration issues detected:");
        foreach (var issue in verifyResult.issues)
        {
            Console.WriteLine($"  - {issue}");
        }
        
        // Auto-fix
        bool fixed = ApacheManager.ConfigurePhpIni(phpResult.path);
        if (fixed)
        {
            Console.WriteLine("PHP configuration updated");
        }
    }
}
```

---

## 6. Testing

### Test Suite
**Location**: `ASHATCore/Tests/HttpPhpFtpUpdateCapabilitiesTest.cs`

Run comprehensive capability tests:

```bash
cd ASHATCore
dotnet run --configuration Release -- httpftpupdate
```

### Test Coverage

✅ HTTP Operations
- File downloads (binary and text)
- File uploads (multipart)
- JSON POST requests
- GET requests
- Custom headers
- Authorization headers
- URL accessibility testing
- Download to disk

✅ FTP Operations
- Connection testing
- File upload/download
- Directory operations
- All methods verified

✅ PHP/Apache Configuration
- Configuration scanning
- Folder detection
- Verification methods
- Apache availability

✅ Update Management
- Version checking
- Package creation
- Integrity verification
- Statistics retrieval

---

## 7. Best Practices

### HTTP Operations
1. **Always use `using` statements** with HttpHelper (implements IDisposable)
2. **Set appropriate timeouts** for large file downloads
3. **Validate responses** before processing
4. **Use HTTPS** for sensitive data
5. **Implement retry logic** for critical operations

### FTP Operations
1. **Test connection** before file operations
2. **Use secure credentials** storage
3. **Validate remote paths** before operations
4. **Implement error handling** for network issues
5. **Consider FTPS** for secure transfers

### PHP Configuration
1. **Always create backups** before modifying php.ini (automatic)
2. **Verify changes** after configuration updates
3. **Test PHP functionality** after updates
4. **Keep Apache/PHP versions** current
5. **Monitor PHP error logs** for issues

### Update Management
1. **Verify checksums** before applying updates
2. **Require valid licenses** for update access
3. **Test updates** in staging before production
4. **Maintain version history** for rollback capability
5. **Log all update operations** for audit trail

---

## 8. Security Considerations

### HTTP Security
- Use HTTPS for all sensitive operations
- Validate SSL certificates
- Implement timeout protection against hangs
- Sanitize URLs to prevent injection attacks
- Use authentication tokens for API access

### FTP Security
- Use FTPS (FTP over SSL/TLS) when possible
- Store credentials securely (encrypted)
- Restrict FTP user permissions
- Monitor FTP access logs
- Use strong passwords

### Update Security
- SHA-256 checksum verification mandatory
- License validation before update access
- Digitally sign update packages (future enhancement)
- Secure update download channels (HTTPS)
- Audit all update operations

---

## 9. Troubleshooting

### HTTP Issues
**Problem**: Timeout errors  
**Solution**: Increase timeout for large files, check network connectivity

**Problem**: SSL/TLS errors  
**Solution**: Verify certificate validity, update system certificates

**Problem**: 403/401 errors  
**Solution**: Check authentication headers, verify API keys

### FTP Issues
**Problem**: Connection refused  
**Solution**: Verify FTP server is running, check firewall rules

**Problem**: Permission denied  
**Solution**: Verify user credentials, check file/directory permissions

**Problem**: Passive mode issues  
**Solution**: Configure FTP client for active/passive mode as needed

### PHP Issues
**Problem**: PHP not detected  
**Solution**: Ensure PHP is installed, check PATH environment variable

**Problem**: Configuration verification fails  
**Solution**: Run ConfigurePhpIni to auto-fix common issues

**Problem**: Apache not found  
**Solution**: Install Apache or use Kestrel (Windows default)

### Update Issues
**Problem**: Update check fails  
**Solution**: Verify license is active, check network connectivity

**Problem**: Checksum mismatch  
**Solution**: Re-download package, verify download integrity

**Problem**: Package not found  
**Solution**: Ensure package was created, check Updates/Packages directory

---

## 10. Summary

The ASHATOS AI Framework provides comprehensive networking and file transfer capabilities:

| Capability | Status | Lines of Code | Methods |
|-----------|--------|---------------|---------|
| HTTP Operations | ✅ Complete | 266 | 7 async methods |
| FTP Operations | ✅ Complete | 255 | 6 async methods |
| PHP/Apache Config | ✅ Complete | 453 | 7 static methods |
| Update Management | ✅ Complete | 249 | 5 methods |
| **Total** | **✅ Verified** | **1,223** | **25 methods** |

All capabilities have been:
- ✅ Implemented and tested
- ✅ Documented comprehensively
- ✅ Integrated into AI Framework
- ✅ Verified through automated testing
- ✅ Ready for production use

---

## 11. Future Enhancements

Potential future improvements:

1. **HTTP/2 Support**: Upgrade to HTTP/2 for improved performance
2. **SFTP Support**: Add SFTP alongside FTP for secure transfers
3. **Caching Layer**: Implement HTTP caching for repeated requests
4. **Rate Limiting**: Add built-in rate limiting for API calls
5. **Retry Logic**: Automatic retry with exponential backoff
6. **Progress Callbacks**: Real-time progress for large transfers
7. **Digital Signatures**: Sign update packages for enhanced security
8. **Delta Updates**: Support incremental updates for efficiency
9. **Auto-Update**: Automatic update application without restart
10. **Telemetry**: Usage analytics for all network operations

---

**Document Version**: 1.0  
**Last Updated**: October 30, 2025  
**Status**: Production Ready ✅
