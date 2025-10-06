# Self Healing Module - Enhanced Site-Wide Error Detection

## Overview
The Self Healing module has been enhanced to automatically detect, diagnose, and repair site-wide errors in RaOS. It now provides comprehensive system diagnostics and automated recovery capabilities.

## New Features

### 1. System-Wide Diagnostics
The module now performs comprehensive checks across multiple system areas:

- **Critical Directories**: Detects missing directories (Databases, Admins, Apache, php)
- **File System Permissions**: Verifies write permissions in critical locations
- **Configuration Files**: Validates Apache and PHP configuration files
- **Module Initialization**: Checks if all modules are properly initialized
- **System Resources**: Monitors disk space and memory usage

### 2. Automated Error Detection
During boot, the Self Healing module automatically:
- Scans for missing critical directories
- Checks file permissions
- Validates configuration files
- Tests module health
- Monitors system resources

### 3. Automatic Recovery
When issues are detected, the module can automatically:
- Create missing directories
- Reinitialize failed modules
- Report permission issues (requires manual admin intervention)
- Log all recovery actions

### 4. Enhanced Reporting
The module provides detailed reports including:
- Issue severity (Issues vs Warnings)
- Specific error descriptions
- Suggested recovery actions
- Recovery success/failure status

## Usage

### During Boot
The Self Healing module automatically runs during system boot and displays:
- Health check results for all modules
- System-wide diagnostic summary
- Automatic repair results

### Manual Commands

#### Run Full Diagnostics
```
selfhealing diagnose
```
This shows all diagnostic results without attempting fixes.

#### Run Diagnostics and Auto-Fix
```
selfhealing diagnose fix
```
This runs diagnostics and automatically attempts to fix detected issues.

#### Check Module Health
```
selfhealing check
```
Checks the health of all loaded modules.

#### View Recovery Log
```
selfhealing log
```
Shows the last 10 recovery actions taken.

#### View Statistics
```
selfhealing stats
```
Shows overall system health statistics.

## Example Output

### Boot Sequence with Self Healing
```
    ╭─────────────────────────────────────╮
    │  ଘ(੭ˊᵕˋ)੭ Step 1/3: Health Check!  │
    ╰─────────────────────────────────────╯

    ✨ Health check complete! ✨
       (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ Healthy: 36
       (´･ω･`) Degraded: 1

    ♡ (っ◔◡◔)っ Attempting auto-recovery with love...
       ✨💚 AILanguage: Healed! (◕‿◕✿)

    🔍 Running system-wide diagnostics...
       Summary: 5 passed, 0 failed
       Issues: 0, Warnings: 3
       ✓ Critical Directories
       ✓ File System Permissions
       ✓ Configuration Files
       ✓ Module Initialization
       ✓ System Resources
       === Attempting Automatic Repairs ===
       Repair Summary: 0 succeeded, 0 failed
```

### Manual Diagnostics
```
selfhealing diagnose fix
```

Output:
```
System-Wide Diagnostics Report:

Summary: 4 passed, 1 failed
Issues: 3, Warnings: 2

✓ Critical Directories
✗ File System Permissions
    ✗ Issue: No write permission in: Databases
    Suggested actions: 1
✓ Configuration Files
    ⚠ Warning: Apache directory exists but no .conf files found
    ⚠ Warning: PHP directory exists but no php.ini found
✓ Module Initialization
✓ System Resources

=== Attempting Automatic Repairs ===

Repairing File System Permissions:
  ✓ Fix permissions for directory: /path/to/Databases

Repair Summary: 1 succeeded, 0 failed
```

## Diagnostic Areas

### 1. Critical Directories Check
Ensures these directories exist:
- `Databases/` - Database storage
- `Admins/` - Admin instance configurations
- `Apache/` - Apache configuration files
- `php/` - PHP configuration files

**Auto-fix**: Creates missing directories

### 2. File System Permissions Check
Tests write permissions in:
- Base directory
- Databases folder
- Admins folder

**Auto-fix**: Reports permission issues (requires admin intervention)

### 3. Configuration Files Check
Validates:
- Apache configuration files (*.conf)
- PHP configuration files (php.ini)
- Configuration file content (not empty)

**Auto-fix**: None (requires manual configuration)

### 4. Module Initialization Check
Tests all loaded modules by:
- Calling their health check methods
- Detecting error responses
- Identifying failed initializations

**Auto-fix**: Attempts to reinitialize failed modules

### 5. System Resources Check
Monitors:
- Available disk space
- Memory usage (working set)

**Auto-fix**: None (informational warnings)

## Recovery Actions

### Action Types

1. **create_directory**: Creates missing directories
   - Auto-executes without approval
   - Logs creation success

2. **fix_permissions**: Attempts to fix file permissions
   - May require administrator privileges
   - Reports if manual intervention needed

3. **reinitialize_module**: Reinitializes a failed module
   - Calls the module's Initialize method
   - Logs success/failure

4. **restart**: Restarts a degraded module
   - Simulated recovery action
   - Always succeeds

## Integration with Boot Sequence

The Self Healing module is integrated into the boot sequence:

1. **Module Health Checks**: All modules are checked for health
2. **Auto-Recovery**: Degraded/unhealthy modules are recovered
3. **System Diagnostics**: Site-wide diagnostics are performed
4. **Automatic Repairs**: Issues are automatically fixed when possible

This ensures the system is healthy before accepting user requests.

## Logging

All Self Healing actions are logged:
- Module health check results
- Diagnostic findings
- Recovery actions attempted
- Success/failure status

Logs can be viewed with:
```
selfhealing log
```

## Future Enhancements

Planned features:
- Database connection testing
- Network connectivity checks
- External service health monitoring
- Automated backup restoration
- Advanced permission repair
- Configuration file generation
- Dependency version checking

## API Integration

The Self Healing module can be called programmatically:

```csharp
var selfHealing = moduleManager.GetModule<SelfHealingModule>();

// Run diagnostics
var report = selfHealing.Process("diagnose");

// Run diagnostics and fix
var repairReport = selfHealing.Process("diagnose fix");

// Get stats
var stats = selfHealing.Process("stats");
```

## Troubleshooting

### "Permission fixes may require manual intervention"
This warning indicates that automatic permission fixes couldn't be applied. You may need to:
1. Run RaCore with elevated privileges (sudo on Linux/Mac, Administrator on Windows)
2. Manually adjust folder permissions
3. Check for file system locks or access restrictions

### "Module check failed"
If a module repeatedly fails health checks:
1. Check module-specific logs
2. Verify module dependencies are installed
3. Check for configuration errors in module settings
4. Try reinitializing the module manually

### Diagnostics Show Warnings
Warnings are informational and don't prevent system operation. Common warnings:
- Missing configuration files (will use defaults)
- Module reports errors (non-critical issues)
- Resource usage notifications (informational)

## Security Considerations

- The Self Healing module requires appropriate permissions to create directories and modify files
- Permission fixes may require administrator/root privileges
- All recovery actions are logged for audit purposes
- Critical actions can be configured to require user approval
- No sensitive data is stored in diagnostic reports

---

*Generated by RaCore Self Healing Module v2.0*
