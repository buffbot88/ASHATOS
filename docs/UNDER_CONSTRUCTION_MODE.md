# Under Construction Mode - Phase 9.3.8

## Overview

The Under Construction mode feature provides a friendly, professional way to display maintenance or "coming soon" pages to website visitors while allowing administrators full access to the system. This feature includes:

- Centralized HTML error handling
- Configurable "Under Construction" mode with cute robot face
- Admin bypass for maintenance work
- Customizable messages and images
- RESTful API for configuration
- Consistent, professional error pages

## Features

### 1. Under Construction Mode

When enabled, all non-admin users (including guests and regular members) will see a friendly "Under Construction" page featuring:

- Animated cute robot face (default SVG)
- Customizable message
- Professional gradient background
- Responsive design for mobile devices
- Link to admin control panel for administrators

### 2. HTML Error Handler

The `UnderConstructionHandler` class provides:

- Centralized error page generation
- Consistent styling across all error pages
- Support for common HTTP error codes (404, 500, etc.)
- Optional control panel links
- Mobile-responsive design

### 3. Admin Access

Administrators (UserRole.Admin and above) can:

- Access the website even when Under Construction mode is enabled
- Configure Under Construction settings via Control Panel
- Customize maintenance messages and robot images
- Toggle the feature on/off instantly

## Configuration

### Server Configuration Properties

The following properties are added to `ServerConfiguration` in `Abstractions/ServerMode.cs`:

```csharp
/// <summary>
/// Indicates if the site is in "Under Construction" mode
/// </summary>
public bool UnderConstruction { get; set; } = false;

/// <summary>
/// Custom message for the Under Construction page
/// </summary>
public string? UnderConstructionMessage { get; set; }

/// <summary>
/// Custom robot image URL for the Under Construction page
/// If null, uses default cute robot face
/// </summary>
public string? UnderConstructionRobotImage { get; set; }
```

### Configuration File

Settings are stored in `server-config.json`:

```json
{
  "Mode": "Production",
  "UnderConstruction": false,
  "UnderConstructionMessage": null,
  "UnderConstructionRobotImage": null,
  ...
}
```

## API Endpoints

### Get Under Construction Status

```http
GET /api/control/server/underconstruction
Authorization: Bearer <admin-token>
```

**Response:**
```json
{
  "success": true,
  "underConstruction": false,
  "message": "Custom message or null",
  "robotImage": "https://example.com/robot.svg or null"
}
```

### Set Under Construction Mode

```http
POST /api/control/server/underconstruction
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "enabled": true,
  "message": "We're working on something awesome! Check back soon.",
  "robotImage": "https://example.com/custom-robot.svg"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Under Construction mode enabled",
  "underConstruction": true,
  "customMessage": "We're working on something awesome! Check back soon.",
  "customRobotImage": "https://example.com/custom-robot.svg"
}
```

## Usage Examples

### Enable Under Construction Mode

```bash
curl -X POST http://localhost/api/control/server/underconstruction \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "enabled": true,
    "message": "Site maintenance in progress. We'"'"'ll be back soon!"
  }'
```

### Disable Under Construction Mode

```bash
curl -X POST http://localhost/api/control/server/underconstruction \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"enabled": false}'
```

### Check Status

```bash
curl -X GET http://localhost/api/control/server/underconstruction \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

## Use Cases

### 1. Planned Maintenance

Enable Under Construction mode before performing major updates or database migrations:

```csharp
var config = firstRunManager.GetServerConfiguration();
config.UnderConstruction = true;
config.UnderConstructionMessage = "Scheduled maintenance: 2:00 AM - 4:00 AM EST";
firstRunManager.SaveConfiguration();
```

### 2. New Website Launch

Use as a "Coming Soon" page for a website in development:

```csharp
var config = firstRunManager.GetServerConfiguration();
config.UnderConstruction = true;
config.UnderConstructionMessage = "Launching January 2025! Stay tuned for something amazing.";
config.UnderConstructionRobotImage = "https://example.com/launch-robot.svg";
firstRunManager.SaveConfiguration();
```

### 3. Emergency Downtime

Quickly enable maintenance mode during unexpected issues:

```csharp
var config = firstRunManager.GetServerConfiguration();
config.UnderConstruction = true;
config.UnderConstructionMessage = "We're experiencing technical difficulties. Our team is working to resolve the issue.";
firstRunManager.SaveConfiguration();
```

## Code Architecture

### UnderConstructionHandler Class

Located in `RaCore/Engine/UnderConstructionHandler.cs`, this static class provides:

#### Methods

**GenerateUnderConstructionPage(ServerConfiguration config)**
- Generates the complete HTML page for Under Construction mode
- Uses default robot SVG or custom image
- Includes default or custom message
- Returns responsive, animated HTML

**GenerateErrorPage(int errorCode, string errorTitle, string errorMessage, bool showControlPanelLink)**
- Generates consistent error pages for HTTP errors
- Supports all standard error codes
- Optional control panel link
- Professional, branded styling

### Homepage Integration

The homepage route in `Program.cs` checks Under Construction status:

```csharp
app.MapGet("/", async (HttpContext context) =>
{
    var serverConfig = firstRunManager.GetServerConfiguration();
    if (serverConfig.UnderConstruction)
    {
        // Check if user is admin
        var user = await GetUserFromToken(context);
        
        // Non-admins see Under Construction page
        if (user == null || user.Role < UserRole.Admin)
        {
            await context.Response.WriteAsync(
                UnderConstructionHandler.GenerateUnderConstructionPage(serverConfig)
            );
            return;
        }
    }
    
    // Normal homepage logic continues...
});
```

## Default Robot SVG

The default robot face is a cute, friendly SVG with:

- Gradient purple/blue color scheme matching RaCore branding
- Animated bouncing effect
- Inline data URI for fast loading
- No external dependencies

## Styling

Both Under Construction and error pages feature:

- Gradient background (purple to blue)
- White content container with shadow
- Responsive design (mobile-friendly)
- Smooth animations
- Consistent typography
- Professional appearance

## Testing

Tests are located in `RaCore/Tests/UnderConstructionTests.cs`:

1. **ServerConfiguration Tests**: Verify property defaults and setters
2. **HTML Generation Tests**: Validate Under Construction page HTML
3. **Error Page Tests**: Check error page generation
4. **Customization Tests**: Test custom messages and images

Run tests with:

```bash
cd /tmp/UnderConstructionTest
dotnet test
```

Or use the test runner:

```bash
dotnet run --project RaCore/RaCore.csproj underconstruction
```

## Security Considerations

1. **Admin-Only Configuration**: Only Admin+ users can toggle Under Construction mode
2. **Token Validation**: API endpoints require valid authentication tokens
3. **Role Checking**: Admin role verified before granting access during maintenance
4. **No Information Leakage**: Error pages don't expose sensitive system information

## Performance

- HTML generation is fast (inline SVG, no external resources)
- Minimal server overhead
- No database queries during Under Construction check
- Configuration cached in memory

## Browser Compatibility

Tested and working on:
- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

## Future Enhancements

Potential future improvements:

1. Countdown timer for scheduled maintenance
2. Email notifications when site comes back online
3. Multiple robot/mascot options
4. Dark mode support
5. Progress bar for maintenance tasks
6. Subscriber-only access during construction
7. Regional/localized messages
8. Custom CSS theme support

## Troubleshooting

### Issue: Under Construction page not showing

**Solution**: 
- Check `server-config.json` - ensure `UnderConstruction: true`
- Verify you're not logged in as Admin
- Clear browser cache

### Issue: Admins seeing Under Construction page

**Solution**:
- Verify user has Admin or higher role
- Check authentication token is valid
- Review server logs for auth errors

### Issue: Custom message not displaying

**Solution**:
- Ensure message is properly escaped in JSON
- Check configuration was saved (`server-config.json`)
- Restart RaCore to reload configuration

## Related Documentation

- [Server Modes and Initialization](SERVER_MODES_AND_INITIALIZATION.md)
- [Control Panel API](CONTROL_PANEL_MODULE_API.md)
- [Authentication Guide](AUTHENTICATION_QUICKSTART.md)
- [Error Handling Best Practices](ADVANCED_FEATURES.md)

---

**Version**: 1.0  
**Phase**: 9.3.8  
**Module**: RaCore + LegendaryCMS  
**Last Updated**: 2024-01-15
