# RaOS v7.5 Quick Start Guide

## Environment Discovery

### Basic Usage

```csharp
// In your module or application code
var moduleManager = new ModuleManager();
moduleManager.DebugLoggingEnabled = true; // Optional: enable detailed logging

// Discover the environment
var environment = moduleManager.DiscoverEnvironment();

// Access discovered information
Console.WriteLine($"Root Directory: {environment.RootDirectory}");
Console.WriteLine($"App Base Directory: {environment.AppBaseDirectory}");
Console.WriteLine($"Discovery Time: {environment.DiscoveryTime}");
```

### Exploring Module Folders

```csharp
Console.WriteLine($"\nDiscovered {environment.ModuleFolders.Count} module folders:");
foreach (var folder in environment.ModuleFolders)
{
    Console.WriteLine($"  - {folder}");
}
```

### Checking External Resources

```csharp
Console.WriteLine($"\nDiscovered {environment.ExternalResources.Count} external resources:");
foreach (var resource in environment.ExternalResources)
{
    Console.WriteLine($"  - {resource.Name} ({resource.Type})");
    Console.WriteLine($"    Path: {resource.Path}");
    Console.WriteLine($"    Exists: {resource.Exists}");
}
```

### Finding Configuration Files

```csharp
Console.WriteLine($"\nDiscovered {environment.ConfigurationFiles.Count} configuration files:");
foreach (var configFile in environment.ConfigurationFiles.Take(10))
{
    Console.WriteLine($"  - {configFile}");
}
```

### Listing Admin Instances

```csharp
Console.WriteLine($"\nDiscovered {environment.AdminInstances.Count} admin instances:");
foreach (var instance in environment.AdminInstances)
{
    Console.WriteLine($"  - {Path.GetFileName(instance)}");
}
```

## Folder Update Scanning

### Monitor Critical Folders

```csharp
var rootDir = Directory.GetCurrentDirectory();

// Define folders to monitor
var foldersToMonitor = new[]
{
    Path.Combine(rootDir, "Nginx"),
    Path.Combine(rootDir, "php"),
    Path.Combine(rootDir, "Admins"),
    Path.Combine(rootDir, "Modules"),
    Path.Combine(rootDir, "wwwroot")
};

// Scan for updates
var updateResult = moduleManager.ScanForUpdates(foldersToMonitor);

Console.WriteLine($"\nFolder Scan Results (scanned at {updateResult.ScanTime}):");
Console.WriteLine($"Scanned {updateResult.ScannedFolders.Count} folders:");
```

### Analyze Folder Details

```csharp
foreach (var folder in updateResult.FolderDetails)
{
    Console.WriteLine($"\n{folder.Name}:");
    Console.WriteLine($"  Path: {folder.Path}");
    Console.WriteLine($"  Last Modified: {folder.LastModified}");
    Console.WriteLine($"  File Count: {folder.FileCount}");
    Console.WriteLine($"  Subdirectories: {folder.Subdirectories.Count}");
    
    if (folder.Subdirectories.Any())
    {
        Console.WriteLine($"  Subdirectories:");
        foreach (var subdir in folder.Subdirectories)
        {
            Console.WriteLine($"    - {subdir}");
        }
    }
}
```

### Periodic Monitoring

```csharp
// Monitor for changes every 60 seconds
var timer = new System.Timers.Timer(60000);
var lastScanResult = moduleManager.ScanForUpdates(foldersToMonitor);

timer.Elapsed += (sender, e) =>
{
    var currentScanResult = moduleManager.ScanForUpdates(foldersToMonitor);
    
    // Compare with last scan
    for (int i = 0; i < lastScanResult.FolderDetails.Count; i++)
    {
        var lastFolder = lastScanResult.FolderDetails[i];
        var currentFolder = currentScanResult.FolderDetails[i];
        
        if (currentFolder.LastModified > lastFolder.LastModified)
        {
            Console.WriteLine($"[UPDATE DETECTED] {currentFolder.Name} was modified");
            Console.WriteLine($"  Old: {lastFolder.LastModified}");
            Console.WriteLine($"  New: {currentFolder.LastModified}");
            Console.WriteLine($"  File count changed: {lastFolder.FileCount} -> {currentFolder.FileCount}");
        }
    }
    
    lastScanResult = currentScanResult;
};

timer.Start();
```

## Integration with Boot Sequence

### Add to BootSequenceManager

```csharp
public class BootSequenceManager
{
    private readonly ModuleManager _moduleManager;

    public async Task ExecuteBootSequenceAsync()
    {
        // ... existing boot sequence code ...

        // Add environment discovery
        Console.WriteLine("[BootSequence] Discovering environment...");
        var environment = _moduleManager.DiscoverEnvironment();
        
        LogEnvironmentInfo(environment);
        
        // Check for required resources
        ValidateRequiredResources(environment);
        
        // Scan for updates
        var foldersToCheck = new[] 
        { 
            Path.Combine(environment.RootDirectory, "Nginx"),
            Path.Combine(environment.RootDirectory, "php") 
        };
        
        var updateResult = _moduleManager.ScanForUpdates(foldersToCheck);
        LogUpdateInfo(updateResult);
        
        // ... continue boot sequence ...
    }

    private void LogEnvironmentInfo(EnvironmentDiscoveryResult environment)
    {
        Console.WriteLine($"  Root Directory: {environment.RootDirectory}");
        Console.WriteLine($"  Modules: {environment.ModuleFolders.Count}");
        Console.WriteLine($"  External Resources: {environment.ExternalResources.Count}");
        Console.WriteLine($"  Config Files: {environment.ConfigurationFiles.Count}");
        Console.WriteLine($"  Admin Instances: {environment.AdminInstances.Count}");
    }

    private void ValidateRequiredResources(EnvironmentDiscoveryResult environment)
    {
        var required = new[] { "Nginx", "php", "Databases", "wwwroot" };
        
        foreach (var resourceName in required)
        {
            var resource = environment.ExternalResources
                .FirstOrDefault(r => r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
            
            if (resource == null || !resource.Exists)
            {
                Console.WriteLine($"  WARNING: Required resource '{resourceName}' not found");
            }
            else
            {
                Console.WriteLine($"  ✓ Found: {resourceName}");
            }
        }
    }

    private void LogUpdateInfo(FolderUpdateResult updateResult)
    {
        Console.WriteLine($"[BootSequence] Folder scan completed:");
        foreach (var folder in updateResult.FolderDetails)
        {
            Console.WriteLine($"  {folder.Name}: {folder.FileCount} files");
        }
    }
}
```

## API Endpoints

### Add REST API Endpoints

```csharp
// In Program.cs

app.MapGet("/api/environment/discover", (ModuleManager manager) =>
{
    var result = manager.DiscoverEnvironment();
    return Results.Json(result);
});

app.MapPost("/api/environment/scan-updates", (ModuleManager manager, string[] folders) =>
{
    var result = manager.ScanForUpdates(folders);
    return Results.Json(result);
});

app.MapGet("/api/environment/modules", (ModuleManager manager) =>
{
    var environment = manager.DiscoverEnvironment();
    return Results.Json(environment.ModuleFolders);
});

app.MapGet("/api/environment/resources", (ModuleManager manager) =>
{
    var environment = manager.DiscoverEnvironment();
    return Results.Json(environment.ExternalResources);
});
```

## Console Commands

### Add Module Commands

```csharp
// In a module's Process method

if (input.StartsWith("env discover", StringComparison.OrdinalIgnoreCase))
{
    if (_manager == null) return "Module manager not available";
    
    var environment = _manager.DiscoverEnvironment();
    var sb = new StringBuilder();
    
    sb.AppendLine("=== Environment Discovery ===");
    sb.AppendLine($"Root: {environment.RootDirectory}");
    sb.AppendLine($"Modules: {environment.ModuleFolders.Count}");
    sb.AppendLine($"Resources: {environment.ExternalResources.Count}");
    sb.AppendLine($"Configs: {environment.ConfigurationFiles.Count}");
    sb.AppendLine($"Admin Instances: {environment.AdminInstances.Count}");
    
    return sb.ToString();
}

if (input.StartsWith("env scan", StringComparison.OrdinalIgnoreCase))
{
    if (_manager == null) return "Module manager not available";
    
    var rootDir = Directory.GetCurrentDirectory();
    var folders = new[]
    {
        Path.Combine(rootDir, "Nginx"),
        Path.Combine(rootDir, "php"),
        Path.Combine(rootDir, "Admins")
    };
    
    var result = _manager.ScanForUpdates(folders);
    var sb = new StringBuilder();
    
    sb.AppendLine("=== Folder Scan Results ===");
    sb.AppendLine($"Scan Time: {result.ScanTime}");
    
    foreach (var folder in result.FolderDetails)
    {
        sb.AppendLine($"\n{folder.Name}:");
        sb.AppendLine($"  Files: {folder.FileCount}");
        sb.AppendLine($"  Modified: {folder.LastModified}");
    }
    
    return sb.ToString();
}
```

## Best Practices

### 1. Cache Discovery Results

```csharp
private EnvironmentDiscoveryResult? _cachedEnvironment;
private DateTime _lastDiscovery = DateTime.MinValue;
private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(5);

public EnvironmentDiscoveryResult GetEnvironment()
{
    if (_cachedEnvironment == null || 
        DateTime.UtcNow - _lastDiscovery > CacheTimeout)
    {
        _cachedEnvironment = moduleManager.DiscoverEnvironment();
        _lastDiscovery = DateTime.UtcNow;
    }
    
    return _cachedEnvironment;
}
```

### 2. Handle Missing Resources Gracefully

```csharp
var environment = moduleManager.DiscoverEnvironment();

var nginxResource = environment.ExternalResources
    .FirstOrDefault(r => r.Name.Equals("Nginx", StringComparison.OrdinalIgnoreCase));

if (nginxResource?.Exists == true)
{
    // Proceed with Nginx configuration
}
else
{
    Console.WriteLine("Nginx not found - using fallback configuration");
}
```

### 3. Enable Debug Logging During Development

```csharp
#if DEBUG
    moduleManager.DebugLoggingEnabled = true;
#endif
```

## Troubleshooting

### Issue: No modules discovered

**Solution**: Ensure modules are in folders named "Modules" or subdirectories thereof.

### Issue: External resources not found

**Solution**: Resources must be in the application root directory with standard names (Nginx, php, Apache, etc.).

### Issue: Performance with large directories

**Solution**: Configuration file discovery is limited to 100 files. Use specific paths when scanning for updates.

---

**Version**: 7.5.0  
**Last Updated**: 2025-01-XX
