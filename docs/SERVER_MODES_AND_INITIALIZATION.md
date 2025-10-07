# RaOS Server Modes and First-Time Initialization

## Overview

RaOS supports multiple server modes to accommodate different deployment scenarios and operational requirements. The first-time initialization sequence guides administrators through a secure and comprehensive setup process.

## Server Modes

### Available Modes

1. **Dev Mode**
   - Purpose: Development and testing with bypass of external validations
   - Features: Skips license server validation during initial setup, enables development features in modules (e.g., LegendaryPay rewards)
   - Use Case: Local development and testing environments
   - **Note**: This mode may be renamed or removed in future versions
   
2. **Alpha Mode**
   - Purpose: Early development and testing
   - Features: Full logging, extended debugging, experimental features enabled
   - Use Case: Internal development teams
   
3. **Beta Mode**
   - Purpose: Pre-release testing with selected users
   - Features: Enhanced logging, beta feature access
   - Use Case: Early adopters and beta testers
   
4. **Omega Mode**
   - Purpose: Main server configuration (US-Omega)
   - Features: Central licensing validation, full feature set
   - Use Case: Primary RaOS deployment server
   
5. **Demo Mode**
   - Purpose: Demonstration and evaluation
   - Features: Limited features, sample data included
   - Use Case: Product demonstrations and trials
   
6. **Production Mode** (Default)
   - Purpose: Full production deployment
   - Features: Optimized performance, production-level logging
   - Use Case: Live production environments

### Dev Mode Behavior

When the server is in Dev mode:
- License validation is bypassed during initial Super Admin setup
- Super Admin License can be freely set on first password change without server verification
- Modules like LegendaryPay automatically enable development features (e.g., test rewards)
- External validation calls to license servers are skipped for faster development iteration

**Security Note**: Dev mode should NEVER be used in production environments as it bypasses critical security validations.

### Setting Server Mode

Server mode can be set during initial configuration or changed later through the server configuration file:

**Location**: `server-config.json` in the RaCore executable directory

**Example Configuration**:
```json
{
  "Mode": "Production",
  "IsFirstRun": false,
  "InitializationCompleted": true,
  "InitializedAt": "2024-01-01T00:00:00Z",
  "Version": "1.0",
  "LicenseKey": "RACORE-XXXXXXXX-XXXXXXXX-XXXXXXXX",
  "LicenseType": "Enterprise",
  "AdminPasswordChanged": true,
  "AdminUsernameChanged": true,
  "AshatEnabled": false,
  "SystemRequirementsMet": true,
  "SystemWarnings": [],
  "CmsPath": "/path/to/wwwroot",
  "MainServerUrl": "https://us-omega.raos.io",
  "SkipLicenseValidation": false
}
```

**Dev Mode Example**:
```json
{
  "Mode": "Dev",
  "IsFirstRun": false,
  "InitializationCompleted": true,
  "InitializedAt": "2024-01-15T00:00:00Z",
  "Version": "1.0",
  "LicenseKey": "RACORE-DEV-TEST-12345678",
  "LicenseType": "Development",
  "AdminPasswordChanged": true,
  "AdminUsernameChanged": true,
  "AshatEnabled": false,
  "SystemRequirementsMet": true,
  "SystemWarnings": [],
  "CmsPath": "/path/to/wwwroot",
  "MainServerUrl": "https://us-omega.raos.io",
  "SkipLicenseValidation": true
}
```

## First-Time Initialization Sequence

### Initialization Flow

When RaOS is started for the first time, it executes the following sequence:

#### Step 1: System Requirements Check
- Verifies .NET runtime version
- Checks for PHP installation
- Validates Nginx availability
- Checks disk space availability
- Monitors memory usage
- Reports warnings for any missing dependencies

#### Step 2: Generate wwwroot Control Panel
- Creates the wwwroot directory structure
- Generates control panel files
- Sets up static file serving

#### Step 3: Spawn CMS with Integrated Control Panel
- Deploys the integrated CMS
- Creates default admin account
- Configures role-based access

#### Step 4: Configure Nginx
- Generates Nginx configuration for CMS
- Sets up reverse proxy for RaCore
- Provides instructions for Nginx setup

#### Step 5: Initialization Guidance
Displays instructions for completing setup:
- Accessing the Control Panel
- Default credentials (admin/admin123)
- Required security steps

#### Step 6: License Validation
- Prompts for license key entry
- Validates license against main server (US-Omega)
- Configures features based on license type

#### Step 7: Ashat AI Assistant
- Introduces Ashat AI assistant
- Provides access instructions
- Enables collaborative development

### Post-Initialization Steps

After the automated initialization, administrators must complete these steps through the Control Panel:

1. **Change Default Password** (REQUIRED)
   - Navigate to: http://localhost/control-panel.html
   - Login with admin/admin123
   - Change password immediately
   
2. **Change Default Username** (RECOMMENDED)
   - Update the admin username for additional security
   - Choose a unique identifier
   
3. **Enter License Key** (REQUIRED)
   - Input your RaOS license key
   - System validates against US-Omega server
   - License determines available features
   
4. **Configure Server Features**
   - Based on license type (Forum/CMS/GameServer)
   - Enable/disable modules
   - Configure system settings
   
5. **Enable Ashat AI** (OPTIONAL)
   - Activate the AI assistant
   - Configure AI preferences
   - Begin collaborative development

## License Validation

### License Types

- **Forum License**: Forum functionality only
- **CMS License**: Content management system features
- **GameServer License**: Game server capabilities
- **Enterprise License**: All features enabled

### Validation Process

1. Admin enters license key in Control Panel
2. RaOS connects to main server (US-Omega) at `https://us-omega.raos.io`
3. License key is validated
4. Server receives license type and expiration date
5. Features are enabled based on license package
6. Configuration is persisted to disk

**Dev Mode Exception**: When server is in Dev mode, steps 2-4 are skipped for initial Super Admin setup, allowing license to be set freely without server validation. This enables faster development iteration without license server dependencies.

### Offline Mode

If the main server is unreachable:
- Warning is displayed to administrator
- Server operates with limited functionality
- Periodic retry attempts are made
- Manual validation can be triggered

**Dev Mode**: License validation failures are ignored to allow development without connectivity requirements.

## Configuration Files

### server-config.json

Main configuration file containing:
- Server mode setting
- Initialization state
- License information
- Security settings
- System warnings
- `SkipLicenseValidation`: Automatically set to `true` in Dev mode

### Module Synchronization

When the server mode is set to Dev:
- LegendaryPay module automatically enables development features (e.g., test rewards)
- License module bypasses external server validation
- Other modules may adapt behavior based on server mode

The FirstRunManager automatically synchronizes the Dev mode setting with modules that support it on startup and when the mode is changed.

### .racore_initialized

Legacy marker file for backwards compatibility:
- Created when initialization completes
- Contains basic initialization metadata
- Used by older RaCore versions

## API Access

### Getting Server Configuration

```csharp
var firstRunManager = new FirstRunManager(moduleManager);
var config = firstRunManager.GetServerConfiguration();
Console.WriteLine($"Server Mode: {config.Mode}");
Console.WriteLine($"License: {config.LicenseKey}");
```

### Setting Server Mode

```csharp
firstRunManager.SetServerMode(ServerMode.Beta);
```

### Checking First Run Status

```csharp
if (firstRunManager.IsFirstRun())
{
    await firstRunManager.InitializeAsync();
}
```

## Security Considerations

### Password Security
- Default password must be changed immediately
- Strong password requirements enforced
- Password history tracked
- Failsafe password available for recovery

### License Security
- License keys are encrypted
- Validation uses secure HTTPS
- Invalid licenses deny access
- License theft protection implemented

### Network Security
- Control Panel accessible only to admins
- SSL/TLS recommended for production
- Firewall configuration guidance provided
- Rate limiting on authentication attempts

## Troubleshooting

### Initialization Fails

**Problem**: Initialization does not complete

**Solutions**:
1. Check system requirements
2. Verify PHP installation
3. Ensure sufficient disk space
4. Check file permissions
5. Review console logs

### License Validation Fails

**Problem**: Cannot validate license key

**Solutions**:
1. Check internet connectivity
2. Verify license key accuracy
3. Contact support for key verification
4. Check firewall settings
5. Try manual validation

### Cannot Access Control Panel

**Problem**: Control Panel not accessible

**Solutions**:
1. Verify Nginx is running
2. Check port 80 availability
3. Review Nginx configuration
4. Check PHP-FPM status
5. Verify wwwroot directory exists

### Ashat AI Not Available

**Problem**: Ashat AI assistant not responding

**Solutions**:
1. Verify AshatEnabled in config
2. Check AI module loading
3. Review module dependencies
4. Check memory availability
5. Restart RaCore service

## Advanced Topics

### Custom Main Server URL

To use a custom license validation server:

```json
{
  "MainServerUrl": "https://custom-validation-server.com"
}
```

### Resetting Initialization

To re-run first-time initialization:

1. Stop RaCore
2. Delete `server-config.json`
3. Delete `.racore_initialized`
4. Restart RaCore
5. Follow initialization prompts

### Migration Between Modes

To change server modes after initialization:

1. Edit `server-config.json`
2. Change `"Mode"` value
3. Restart RaCore
4. Verify mode change in logs

## Best Practices

1. **Always change default credentials** before exposing to network
2. **Backup server-config.json** regularly
3. **Monitor system warnings** from requirements check
4. **Keep license key secure** and backed up
5. **Document custom configurations** for team reference
6. **Test in Demo mode** before deploying to Production
7. **Use appropriate mode** for environment (Alpha for dev, Production for live)
8. **Enable Ashat AI** for guided assistance during setup

## Future Enhancements

- Web-based initialization wizard
- Automated license renewal
- Multi-server synchronization
- Advanced telemetry options
- Integrated backup/restore functionality
- Role-based initialization workflows

## Support

For assistance with server modes or initialization:
- Email: support@raos.io
- Forum: https://forum.raos.io
- Documentation: https://docs.raos.io
- GitHub Issues: https://github.com/buffbot88/TheRaProject/issues

---

**Version**: 1.0  
**Last Updated**: 2024-01-01  
**Module**: RaCore
