# Manual Testing Guide for Server IP & Nginx Updates

## Quick Test Commands

### 1. Test Server IP Detection
Run RaCore and observe the boot sequence:
```bash
cd RaCore
dotnet run
```

Expected output during boot:
```
    ╭─────────────────────────────────────╮
    │  ଘ(੭*ˊᵕˋ)੭* Step 2/4: Nginx Check!  │
    ╰─────────────────────────────────────╯

    🌐 Server IP Address: [Your IP Address]
```

### 2. Test Local Nginx Folder Detection

Create a local Nginx folder structure:
```bash
# In the RaCore root directory
mkdir -p Nginx/conf
cd Nginx

# On Windows, if you have Nginx installed:
# copy C:\nginx\nginx.exe .
# copy C:\nginx\conf\*.* conf\

# On Linux:
# cp /usr/sbin/nginx .
# cp /etc/nginx/nginx.conf conf/
```

Then run RaCore:
```bash
cd ..
dotnet run
```

Expected output:
```
[NginxManager] ✨ Found local Nginx: /path/to/RaCore/Nginx/nginx.exe
[NginxManager] ✨ Found local Nginx config: /path/to/RaCore/Nginx/conf/nginx.conf
```

### 3. Test Configuration Verification

#### PHP Config Verification
If you don't have a php.ini file, RaCore will:
1. Detect that it's missing
2. Generate a default one
3. Verify the newly created file

Expected output:
```
    ⚠️  PHP configuration file not found
    ⚠️  Generating default configuration...
    (っ◔◡◔)っ Creating php.ini at: [path]
    ✨ PHP configuration generated successfully!
    ✨ PHP configuration settings applied!
    ✅ PHP configuration is valid
```

#### Nginx Config Verification
If you don't have an nginx.conf file:
1. RaCore will create a basic one in `RaCore/Nginx/conf/`
2. Add RaCore reverse proxy configuration
3. Verify the configuration

Expected output:
```
[NginxManager] ✨ Creating Nginx configuration in local folder: [path]
[NginxManager] ✨ Created basic Nginx config: [path]
[NginxManager] ✅ Added reverse proxy configuration for [server names]
    ✅ Nginx configuration is valid (tested with nginx -t)
```

### 4. Test API Endpoint for Nginx Restart

First, you need to:
1. Start RaCore
2. Register an admin user
3. Login to get a token
4. Use the token to call the restart endpoint

```bash
# Register an admin user (first time only)
curl -X POST http://localhost:80/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@example.com",
    "password": "Admin123!",
    "role": 4
  }'

# Login
curl -X POST http://localhost:80/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }'

# Save the token from the response, then:
curl -X POST http://localhost:80/api/control/system/restart-apache \
  -H "Authorization: Bearer [YOUR_TOKEN]"
```

Expected response:
```json
{
  "success": true,
  "message": "Nginx reloaded successfully"
}
```

### 5. Test Configuration File Creation

Check that files are created in the right locations:

```bash
# List local Nginx config
ls -la Nginx/conf/

# Expected: nginx.conf and backup files

# List PHP config (if PHP is installed)
ls -la php/

# Expected: php.ini

# List admin-specific configs
ls -la Admins/*/

# Expected: Each admin folder should have:
# - php.ini
# - nginx.conf
# - admin.json
# - README.md
```

### 6. Verify Server Names in Nginx Config

Check that the nginx.conf includes your server IP:

```bash
cat Nginx/conf/nginx.conf | grep "server_name"
```

Expected output should include:
```
server_name localhost [YOUR_IP] agpstudios.online www.agpstudios.online;
```

## Troubleshooting

### Issue: Server IP not detected
**Solution**: Check network interfaces are up and have valid IP addresses:
```bash
# Windows
ipconfig

# Linux/Mac
ifconfig
ip addr
```

### Issue: Nginx not found
**Solution**: 
1. Install Nginx locally in `RaCore/Nginx/`
2. Or install system-wide Nginx
3. Check PATH environment variable

### Issue: Permission denied errors
**Solution**:
```bash
# Windows: Run as Administrator
# Linux/Mac: Use sudo or grant appropriate permissions
sudo chown -R $USER:$USER Nginx/
```

### Issue: Config files not being written
**Solution**:
1. Check folder permissions
2. Verify disk space
3. Check console output for specific error messages

## Success Criteria

All of the following should be true:

- ✅ Server IP is detected and displayed during boot
- ✅ Local Nginx folder is checked before system folders
- ✅ PHP configuration is verified and created if missing
- ✅ Nginx configuration is verified and created if missing
- ✅ Configuration files are written to `RaCore/Nginx/conf/`
- ✅ Server IP is included in Nginx server_name directive
- ✅ Nginx can be started via API endpoint (with admin token)
- ✅ All builds succeed without errors

## Notes

- Nginx is NOT automatically started for safety reasons
- Manual start is required after configuration changes
- Use the API endpoint for convenient Nginx management
- All configuration changes are backed up before modification
