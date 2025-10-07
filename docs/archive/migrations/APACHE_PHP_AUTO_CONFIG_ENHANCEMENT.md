# Apache and PHP Auto-Configuration Enhancement

## Summary
This update resolves the issue where RaOS does not automatically modify pre-existing Apache or PHP configuration files during OS boot. The system now intelligently detects, verifies, and updates existing configuration files to ensure proper operation.

## Issue Description
**Problem**: RaOS did not automatically modify pre-existing Apache or PHP configuration files during OS boot, leaving required settings unchanged and causing deployment issues.

**Impact**: Users had to manually edit configuration files after installation or updates, leading to:
- Broken domain routing
- Inaccessible services
- Manual intervention required

## Solution Implemented

### 1. Enhanced Apache Module Detection
**Previous Behavior**: The system used a flawed logic to detect if Apache proxy modules were enabled:
```csharp
// Old logic - could incorrectly identify commented modules as enabled
var proxyModuleEnabled = config.Contains("LoadModule proxy_module") && 
                         !config.Contains("#LoadModule proxy_module");
```

**New Behavior**: Uses proper regex pattern matching:
```csharp
// New logic - correctly identifies only uncommented, active modules
var proxyModuleEnabled = Regex.IsMatch(
    config, @"^\s*LoadModule\s+proxy_module\s+modules/mod_proxy\.so", 
    RegexOptions.Multiline);
```

### 2. Apache Configuration Verification and Update
**Previous Behavior**: If the RaCore configuration marker was found, the system assumed the configuration was complete and skipped updates.

**New Behavior**: The system now:
1. Detects if RaCore configuration exists
2. **Verifies the configuration is complete and correct**:
   - Checks if ServerAlias includes required domains (agpstudios.online, www.agpstudios.online)
   - Verifies the correct port is configured
   - Ensures WebSocket support is present
3. **Automatically updates** if any issues are found:
   - Reports specific issues detected
   - Removes outdated configuration
   - Adds updated configuration with all required settings
   - Creates timestamped backup before modifications

**Example Output**:
```
[ApacheManager] ℹ️  RaCore reverse proxy configuration found, verifying...
[ApacheManager] ⚠️  Configuration is incomplete or outdated:
[ApacheManager]    - Missing or incomplete ServerAlias configuration
[ApacheManager]    - Port mismatch: configured=3000, expected=5000
[ApacheManager]    - Missing WebSocket support
[ApacheManager] 🔄 Updating RaCore configuration...
[ApacheManager] 💾 Backup created: C:\Apache24\conf\httpd.conf.racore_backup_20240115120000
[ApacheManager] ✅ Updated reverse proxy configuration for localhost:5000
[ApacheManager] ✅ ServerAlias configured for: agpstudios.online, www.agpstudios.online
```

### 3. PHP Configuration Auto-Update (NEW)
**Previous Behavior**: The system only detected php.ini but never modified it, leaving PHP with potentially incorrect or suboptimal settings.

**New Behavior**: Added `ConfigurePhpIni()` method that:
1. Finds the active php.ini file
2. Checks critical settings against recommended values:
   - `memory_limit` (256M)
   - `max_execution_time` (60)
   - `max_input_time` (60)
   - `post_max_size` (10M)
   - `upload_max_filesize` (10M)
   - `display_errors` (On)
   - `log_errors` (On)
   - `date.timezone` (UTC)
3. Updates settings that are:
   - Commented out
   - Set to incorrect values
   - Missing entirely
4. Creates timestamped backup before modifications
5. Reports all changes made

**Example Output**:
```
    ♡ (っ◔◡◔)っ Auto-configuring PHP settings...
[ApacheManager] Found PHP config: C:\php\php.ini
[ApacheManager] 💾 Backup created: C:\php\php.ini.racore_backup_20240115120000
[ApacheManager] ✅ PHP configuration updated: C:\php\php.ini
[ApacheManager] Updated settings:
[ApacheManager]    - memory_limit = 256M
[ApacheManager]    - max_execution_time = 60
[ApacheManager]    - date.timezone = UTC
[ApacheManager]    - display_errors = On
    ✨ PHP configuration updated successfully!
```

### 4. Boot Sequence Integration
Updated `BootSequenceManager.VerifyPhpConfiguration()` to automatically call the new PHP configuration update method during boot.

## Technical Details

### Files Modified
1. **RaCore/Engine/ApacheManager.cs** (+223 lines)
   - Fixed module detection regex
   - Added configuration verification logic
   - Added configuration update logic
   - Added `ConfigurePhpIni()` method
   
2. **RaCore/Engine/BootSequenceManager.cs** (+20 lines)
   - Integrated PHP auto-configuration into boot sequence

### Safety Features
1. **Automatic Backups**: All configuration files are backed up with timestamps before modification
2. **Idempotent**: Operations can be run multiple times safely
3. **Verification Before Update**: Checks existing configuration before making changes
4. **Detailed Logging**: All operations are logged with clear, helpful messages
5. **Non-Destructive**: Only updates specific settings, preserves existing configuration

### Backward Compatibility
- ✅ Fully backward compatible
- ✅ Works with existing installations
- ✅ Handles both fresh installs and updates
- ✅ No breaking changes

## Testing
- ✅ Project builds successfully with 0 errors, 0 warnings
- ✅ Module detection logic tested and verified
- ✅ Configuration verification logic tested and verified
- ✅ PHP setting update logic tested and verified

## Benefits
1. **Zero Manual Configuration**: Apache and PHP are automatically configured on every boot
2. **Self-Healing**: Outdated or incomplete configurations are automatically fixed
3. **Production Ready**: Ensures optimal settings for running RaCore
4. **User Friendly**: Clear, helpful error messages and status updates
5. **Safe**: Automatic backups prevent configuration loss

## Impact
This enhancement resolves the reported issue completely:
- ✅ Apache configurations are automatically verified and updated on boot
- ✅ PHP configurations are automatically verified and updated on boot
- ✅ All necessary directives are applied automatically
- ✅ No manual intervention required
- ✅ Broken configurations are automatically fixed
