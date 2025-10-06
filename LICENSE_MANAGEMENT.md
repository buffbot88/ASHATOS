# RaCore License Management System

## Overview

The RaCore License Management System implements robust license-based access control to ensure only authorized users (SuperAdmin and users with valid purchased licenses) can access the Control Panel and advanced features.

---

## Architecture

### Components

1. **License Module** (`RaCore/Modules/Extensions/License/LicenseModule.cs`)
   - Central license management and validation
   - License creation and assignment
   - Expiration tracking and enforcement

2. **Authentication Integration** (`AuthenticationModule.cs`)
   - License validation on login
   - Permission checking with license awareness
   - Audit logging for license events

3. **Database Schema** (SQLite)
   - `licenses` table: Stores all licenses
   - `user_licenses` table: Links users to licenses
   - `audit_log` table: Records all license validation events

---

## License Types

```csharp
public enum LicenseType
{
    Trial,        // Limited trial access
    Standard,     // 10 users
    Professional, // 50 users
    Enterprise,   // 500 users
    Lifetime      // Unlimited users, no expiration
}
```

---

## License Status

```csharp
public enum LicenseStatus
{
    Active,     // Valid and usable
    Inactive,   // Temporarily disabled
    Expired,    // Past expiration date
    Revoked,    // Permanently disabled
    Suspended   // Temporarily suspended
}
```

---

## Access Control Rules

### SuperAdmin Access
- **Always has full access** without license requirement
- Bypasses all license checks
- Can manage licenses for other users
- Role = 2 in the system

### Regular User Access
- **Requires valid license** to access Control Panel
- License must be:
  - Status = `Active`
  - Not expired (if expiration date set)
  - Assigned to the user via `user_licenses` table
- Without valid license: redirected to purchase/license page

### Admin Access
- Has elevated permissions but still requires valid license
- Can view license management interface
- Role = 1 in the system

---

## Database Schema

### licenses Table
```sql
CREATE TABLE licenses (
    id TEXT PRIMARY KEY,
    license_key TEXT UNIQUE NOT NULL,
    instance_name TEXT,
    status TEXT NOT NULL,
    type TEXT DEFAULT 'Standard',
    created_at TEXT NOT NULL,
    expires_at TEXT,
    max_users INTEGER DEFAULT 1
);
```

### user_licenses Table
```sql
CREATE TABLE user_licenses (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL,
    license_id TEXT NOT NULL,
    assigned_at TEXT NOT NULL,
    is_active INTEGER DEFAULT 1,
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (license_id) REFERENCES licenses(id)
);
```

---

## API Reference

### LicenseModule Methods

#### `bool HasValidLicense(User user)`
Check if a user has a valid, active license. SuperAdmin always returns true.

```csharp
var isValid = licenseModule.HasValidLicense(user);
```

#### `License? GetUserLicense(Guid userId)`
Retrieve the active license for a specific user.

```csharp
var license = licenseModule.GetUserLicense(userId);
```

#### `License CreateAndAssignLicense(Guid userId, string instanceName, LicenseType type, int durationYears)`
Create a new license and assign it to a user in one operation.

```csharp
var license = licenseModule.CreateAndAssignLicense(
    userId, 
    "Production Instance", 
    LicenseType.Enterprise, 
    1  // 1 year
);
```

#### `bool RevokeLicense(Guid userId)`
Revoke a user's license, immediately disabling access.

```csharp
bool revoked = licenseModule.RevokeLicense(userId);
```

### AuthenticationModule Methods

#### `bool HasLicensePermission(User user, string moduleName, UserRole requiredRole)`
Check if user has both role permission AND valid license.

```csharp
if (authModule.HasLicensePermission(user, "AdvancedFeatures", UserRole.User))
{
    // Grant access
}
```

---

## Login Flow with License Validation

1. User submits credentials
2. Authentication validates username/password
3. **If user is not SuperAdmin:**
   - Check for active license via `LicenseModule.HasValidLicense()`
   - Verify license status is `Active`
   - Check expiration date
   - If no valid license: deny access with message
4. **If SuperAdmin:** skip license check
5. Create session token
6. Log success/failure events
7. Return authentication response

---

## Integration with Control Panel

### PHP License Checking

The generated PHP control panel includes license validation:

```php
// Check license for non-SuperAdmin users
if (!$isSuperAdmin) {
    $userLicense = $db->getUserLicense($currentUser['user_id']);
    $hasValidLicense = $userLicense !== null && $userLicense['status'] === 'Active';
    
    // Check expiration
    if ($userLicense && !empty($userLicense['expires_at'])) {
        $expiryDate = strtotime($userLicense['expires_at']);
        if ($expiryDate && $expiryDate < time()) {
            $hasValidLicense = false;
        }
    }
    
    if (!$hasValidLicense) {
        // Deny access
        $error = 'Access denied: Valid license required.';
    }
}
```

---

## Security Events

The system logs all license-related events:

- `LicenseValidationFailure`: User attempted access without valid license
- `LicenseExpired`: User's license expired
- `LicenseRevoked`: License was manually revoked
- `LoginSuccess` with license details: Successful licensed login

All events include:
- Timestamp
- User ID
- Action details
- IP address (when available)
- Success/failure status

---

## Usage Examples

### Creating a License for a New User

```csharp
// In your application code
var licenseModule = moduleManager.GetModuleByName("License") as ILicenseModule;
var authModule = moduleManager.GetModuleByName("Authentication") as IAuthenticationModule;

// Create and assign license
var license = licenseModule.CreateAndAssignLicense(
    userId: newUser.Id,
    instanceName: "Customer Production",
    type: LicenseType.Standard,
    durationYears: 1
);

Console.WriteLine($"License created: {license.LicenseKey}");
```

### Checking Access Before Granting Features

```csharp
var user = await authModule.GetUserByTokenAsync(token);

if (authModule.HasLicensePermission(user, "AIConsole", UserRole.Admin))
{
    // Allow access to AI Console
}
else
{
    // Deny access or show upgrade prompt
}
```

### Revoking a License

```csharp
bool success = licenseModule.RevokeLicense(userId);

if (success)
{
    // Notify user and log action
    await licenseModule.LogLicenseEventAsync(
        userId, 
        "Revoke", 
        "License revoked by admin", 
        true
    );
}
```

---

## Sales Integration (Placeholder)

The system is designed to integrate with a sales/payment system:

1. User browses to Sales page
2. Selects license tier and duration
3. Completes payment
4. Sales system calls `CreateAndAssignLicense()` API
5. User immediately gains access
6. License details emailed to user

**Note:** Sales page implementation is planned for Phase 4.

---

## Monitoring and Auditing

### View License Statistics

```bash
# Via RaCore CLI
> license stats

# Returns JSON with:
{
  "TotalLicenses": 5,
  "ActiveLicenses": 3,
  "AssignedLicenses": 3,
  "ExpiredLicenses": 1,
  "Licenses": [...]
}
```

### Audit Log

All license operations are logged to the `audit_log` table:
- License creation
- Assignment to users
- Validation attempts
- Expiration events
- Revocations

---

## Best Practices

1. **Always check licenses at entry points**: Control Panel, API endpoints, advanced features
2. **Use HasLicensePermission() for feature gates**: Ensures both role AND license are valid
3. **Log all license events**: Creates audit trail for compliance
4. **Set reasonable expiration dates**: Typically 1 year for standard licenses
5. **SuperAdmin for maintenance only**: Regular admins should have licenses like other users

---

## Troubleshooting

### "Access denied: Valid license required"
- Verify user has entry in `user_licenses` table
- Check license status is `Active` in `licenses` table  
- Confirm license hasn't expired
- SuperAdmin users never see this error

### License appears valid but access denied
- Check `is_active` field in `user_licenses` (should be 1)
- Verify foreign key relationships are correct
- Check audit log for specific failure reason

### How to manually grant license to user
```sql
-- Find user ID
SELECT id, username FROM users WHERE username = 'targetuser';

-- Find license ID
SELECT id, license_key FROM licenses WHERE status = 'Active';

-- Create license assignment
INSERT INTO user_licenses (id, user_id, license_id, assigned_at, is_active)
VALUES (hex(randomblob(16)), 'user-id-here', 'license-id-here', datetime('now'), 1);
```

---

## Future Enhancements

- Multi-tier subscription management
- Automatic license renewal
- Grace period for expired licenses
- License transfer between users
- Usage analytics per license
- API endpoints for external license management

---

## Related Documentation

- [Authentication Quickstart](AUTHENTICATION_QUICKSTART.md)
- [SuperAdmin Control Panel](SUPERADMIN_CONTROL_PANEL.md)
- [Security Architecture](SECURITY_ARCHITECTURE.md)
- [First Run Initialization](FIRST_RUN_INITIALIZATION.md)

---

**RaCore License Management System**  
**Version**: 1.0  
**Last Updated**: 2025-10-06  
**Status**: Phase 3 - In Progress
