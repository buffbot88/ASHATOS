# Apache Restart API

## Overview
RaOS now includes a REST API endpoint that allows administrators to restart Apache web server without having to restart the entire RaOS application. This enables seamless configuration updates and service maintenance while keeping RaOS running continuously.

## Feature Details

### API Endpoint
**POST** `/api/control/system/restart-apache`

### Authentication
- **Required:** Yes (Bearer token)
- **Required Role:** Admin or higher
- **Permission:** System management

### Response Format

#### Success Response
```json
{
  "success": true,
  "message": "Apache restarted successfully"
}
```

#### Error Response
```json
{
  "success": false,
  "error": "Error message describing the issue"
}
```

## Platform Support

| Platform | Method | Privileges Required |
|----------|--------|---------------------|
| Windows | `httpd.exe -k restart` | Administrator (if Apache service) |
| Linux | `systemctl restart apache2` or `service apache2 restart` | sudo/root |
| macOS | `apachectl restart` | sudo/root |

## Usage Examples

### Using curl (Linux/Mac)
```bash
# Get authentication token first (if needed)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Restart Apache using the token
curl -X POST http://localhost:5000/api/control/system/restart-apache \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Using PowerShell (Windows)
```powershell
# Get authentication token first (if needed)
$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"admin123"}'

$token = $loginResponse.token

# Restart Apache using the token
Invoke-RestMethod -Uri "http://localhost:5000/api/control/system/restart-apache" `
  -Method POST `
  -Headers @{ "Authorization" = "Bearer $token" }
```

### Using JavaScript/Fetch
```javascript
// Assuming you have the JWT token stored
const token = localStorage.getItem('authToken');

fetch('http://localhost:5000/api/control/system/restart-apache', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(response => response.json())
.then(data => {
  if (data.success) {
    console.log('✅ Apache restarted successfully');
  } else {
    console.error('❌ Failed to restart Apache:', data.error);
  }
})
.catch(error => console.error('Error:', error));
```

## Common Use Cases

### 1. After Configuration Changes
When Apache configuration is automatically updated by RaOS during boot sequence, restart Apache to apply changes:

```bash
# RaOS auto-configures Apache during boot
# Then restart Apache via API
curl -X POST http://localhost:5000/api/control/system/restart-apache \
  -H "Authorization: Bearer $TOKEN"
```

### 2. Manual Configuration Updates
After manually editing Apache configuration files:

```bash
# Edit httpd.conf or VirtualHost files
sudo nano /etc/apache2/apache2.conf

# Restart Apache via RaOS API
curl -X POST http://localhost:5000/api/control/system/restart-apache \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Scheduled Maintenance
Integrate with cron jobs or scheduled tasks for periodic Apache restarts:

```bash
# Crontab example - restart Apache daily at 3 AM
0 3 * * * curl -X POST http://localhost:5000/api/control/system/restart-apache \
  -H "Authorization: Bearer $(cat /path/to/token.txt)"
```

### 4. Integration with Control Panel UI
Add a button in the web-based control panel:

```html
<button onclick="restartApache()">Restart Apache</button>

<script>
async function restartApache() {
  const token = localStorage.getItem('authToken');
  
  if (!token) {
    alert('Please login first');
    return;
  }
  
  try {
    const response = await fetch('/api/control/system/restart-apache', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    const data = await response.json();
    
    if (data.success) {
      alert('✅ Apache restarted successfully!');
    } else {
      alert('❌ Failed to restart Apache: ' + data.error);
    }
  } catch (error) {
    alert('Error: ' + error.message);
  }
}
</script>
```

## Error Handling

### Permission Denied
**Error:** `Permission denied. Please run RaCore with sudo or grant appropriate permissions.`

**Solution:**
- **Linux/Mac:** Run RaOS with sudo privileges
  ```bash
  sudo dotnet run
  ```
- **Windows:** Run RaOS as Administrator (right-click → Run as Administrator)

### Apache Not Found
**Error:** `Apache not found. Please ensure Apache is installed.`

**Solution:**
- Install Apache web server
- Ensure Apache is in the system PATH
- Verify installation:
  - **Windows:** Check `C:\Apache24\bin\httpd.exe`
  - **Linux:** Run `which apache2` or `which httpd`
  - **macOS:** Run `which apachectl`

### Service Restart Failed
**Error:** `Apache restart failed: [specific error message]`

**Solution:**
- Check Apache configuration syntax:
  ```bash
  # Linux
  apachectl configtest
  
  # Windows
  httpd.exe -t
  ```
- Review Apache error logs:
  - **Linux:** `/var/log/apache2/error.log`
  - **Windows:** `C:\Apache24\logs\error.log`
- Ensure Apache service is running before attempting restart

## Security Considerations

### 1. Authentication Required
- Only authenticated users with Admin role or higher can restart Apache
- JWT tokens are validated on every request
- Failed authentication attempts are logged

### 2. Rate Limiting
Consider implementing rate limiting for this endpoint to prevent abuse:
```csharp
// Example: Limit to 5 restarts per minute per user
// Can be implemented using middleware or custom logic
```

### 3. Audit Logging
All Apache restart requests are logged with:
- Timestamp
- Requesting user
- Success/failure status
- Error details (if any)

Example log output:
```
[API] Apache restart requested by user: admin
[ApacheManager] Attempting to restart Apache using: apache2
[ApacheManager] ✅ Apache restarted successfully
[API] ✅ Apache restarted successfully by admin
```

## Benefits

1. **No Downtime for RaOS**: Restart Apache without interrupting RaOS service
2. **Remote Management**: Control Apache from web interface or API calls
3. **Automated Workflows**: Integrate with scripts and automation tools
4. **Better UX**: Apply configuration changes seamlessly
5. **Continuous Operation**: Keep RaOS running while managing Apache

## Workflow Example

```
User logs into Control Panel
       ↓
RaOS auto-configures Apache during boot
       ↓
User sees message: "Apache configured, restart required"
       ↓
User clicks "Restart Apache" button
       ↓
API call to /api/control/system/restart-apache
       ↓
RaOS executes platform-specific restart command
       ↓
Apache restarts with new configuration
       ↓
User can access RaCore via http://localhost
       ↓
RaOS continues running normally
```

## Future Enhancements

- [ ] Apache status check endpoint (running/stopped)
- [ ] Apache configuration validation before restart
- [ ] Graceful restart option (finish serving existing requests)
- [ ] Scheduled restart functionality
- [ ] Apache module management (enable/disable modules)
- [ ] Configuration rollback on restart failure
- [ ] Web UI integration with control panel
- [ ] Email notifications on restart events
- [ ] Support for Apache Tomcat and other web servers

## Troubleshooting

### Issue: API returns 403 Forbidden
**Cause:** User lacks Admin permissions

**Solution:** Ensure user has Admin role:
```bash
# Check user role in authentication system
# Promote user to Admin if needed (requires SuperAdmin)
```

### Issue: API returns 500 Internal Server Error
**Cause:** Apache restart command failed

**Solution:**
1. Check RaOS logs for detailed error message
2. Verify Apache is installed and accessible
3. Ensure proper permissions (sudo/Administrator)
4. Test manual Apache restart:
   ```bash
   # Linux
   sudo systemctl restart apache2
   
   # Windows
   httpd.exe -k restart
   ```

### Issue: "Command not found" error
**Cause:** Apache executable not in PATH

**Solution:**
- Add Apache to system PATH
- Or use full path in Apache installation
- Verify with: `which apache2` (Linux) or `where httpd.exe` (Windows)

## Related Documentation

- [BOOT_SEQUENCE.md](BOOT_SEQUENCE.md) - Apache auto-configuration during boot
- [APACHE_AUTO_CONFIG_FIX.md](APACHE_AUTO_CONFIG_FIX.md) - Automatic Apache configuration details
- [APACHE_AUTO_CONFIG_FLOW.md](APACHE_AUTO_CONFIG_FLOW.md) - Configuration flow diagrams

## Support

For issues or questions about the Apache Restart API:
1. Check the troubleshooting section above
2. Review RaOS console output for error details
3. Verify Apache installation and permissions
4. Check Apache error logs for service-specific issues
