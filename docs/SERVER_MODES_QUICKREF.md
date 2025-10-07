# Server Modes and Initialization - Quick Reference

## Server Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| **Alpha** | Early development with full logging | Internal development |
| **Beta** | Pre-release testing | Early adopters, beta testers |
| **Omega** | Main server (US-Omega) | Central licensing validation |
| **Demo** | Limited features | Product demonstrations |
| **Production** | Full deployment (default) | Live production |

## Quick Commands

### Via Module (Terminal/WebSocket)
```
serverconfig status           # View current configuration
serverconfig modes            # List available modes
serverconfig mode Beta        # Change to Beta mode
```

### Via API (Control Panel/REST)
```bash
# Get server configuration
GET /api/control/server/config
Authorization: Bearer <token>

# Change server mode (SuperAdmin only)
POST /api/control/server/mode
Authorization: Bearer <token>
Content-Type: application/json
{"Mode": "Beta"}

# List available modes
GET /api/control/server/modes
Authorization: Bearer <token>
```

## First-Run Initialization Steps

When RaOS starts for the first time:

1. **System Requirements Check** - Verifies .NET, PHP, Nginx, disk space, memory
2. **Generate wwwroot** - Creates control panel directory structure
3. **Spawn CMS** - Deploys integrated CMS with default admin account
4. **Configure Nginx** - Generates Nginx configuration and reverse proxy
5. **Initialization Guidance** - Displays setup instructions
6. **License Validation** - Prompts for license key entry
7. **Ashat AI** - Introduces AI assistant capabilities

## Configuration Files

### server-config.json
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
  "MainServerUrl": "https://us-omega.raos.io"
}
```

## Post-Initialization Checklist

After first run, complete these steps in the Control Panel:

- [ ] Login to Control Panel (http://localhost/control-panel.html)
- [ ] Change default password (admin/admin123) - **REQUIRED**
- [ ] Change default username - **RECOMMENDED**
- [ ] Enter and validate license key - **REQUIRED**
- [ ] Configure server features based on license
- [ ] Enable Ashat AI assistant (optional)

## Troubleshooting

### Initialization Fails
```bash
# Check system requirements
# Verify PHP installation
# Ensure sufficient disk space
# Check file permissions
```

### License Validation Fails
```bash
# Check internet connectivity
# Verify license key accuracy
# Check firewall settings
# Contact support
```

### Cannot Access Control Panel
```bash
# Verify Nginx is running
# Check port 80 availability
# Review Nginx configuration
# Verify wwwroot directory exists
```

## Security Notes

⚠️ **IMPORTANT**: Always change default credentials immediately after first run
🔒 License keys are encrypted and validated via HTTPS
🛡️ SuperAdmin role required to change server mode

## Documentation

For complete documentation, see:
- [SERVER_MODES_AND_INITIALIZATION.md](./SERVER_MODES_AND_INITIALIZATION.md) - Full guide
- [DOCUMENTATION_INDEX.md](./DOCUMENTATION_INDEX.md) - All documentation

## Support

- Email: support@raos.io
- Forum: https://forum.raos.io
- GitHub: https://github.com/buffbot88/TheRaProject/issues
