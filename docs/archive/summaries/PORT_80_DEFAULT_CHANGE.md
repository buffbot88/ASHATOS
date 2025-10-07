# RaCore Default Port Change: 5000 → 80

## Summary
Changed RaCore's default port from 5000 to 80 to allow seamless access without specifying a port in URLs.

## Issue Reference
**Issue:** Bug: RaOS isn't configuring webserver
- User reported inability to access the website despite ports 5000, 8080, and 80 being open
- User requested: "Please make the server's default port 80, so I don't have to point my domain at a port."

## Changes Made

### Code Changes
1. **Program.cs**
   - Changed default port fallback from `"5000"` to `"80"`
   - Updated startup messages to reference Nginx instead of Apache
   - Updated API documentation messages

2. **BootSequenceManager.cs**
   - Changed default port fallback from `"5000"` to `"80"` when Nginx config cannot be detected
   - Updated environment variable initialization: `RACORE_DETECTED_PORT = "80"`
   - Updated console messages to reflect new default

3. **NginxManager.cs**
   - Changed default parameter in `ConfigureReverseProxy()` from `5000` to `80`

4. **FirstRunManager.cs**
   - Changed default port fallback from `"5000"` to `"80"` when reading `RACORE_PORT` environment variable

5. **IGameClientModule.cs** (Abstractions)
   - Changed default `ServerPort` property from `5000` to `80`

### Documentation Updates
1. **README.md**
   - Updated default port from 5000 to 80
   - Added note about administrator/root privileges required for port 80
   - Updated all URL examples to use port 80

2. **FIRST_RUN_INITIALIZATION.md**
   - Updated default port documentation from 5000 to 80
   - Revised privilege note to explain port 80 requirements
   - Updated startup message examples

3. **BOOT_SEQUENCE.md**
   - Updated all port references and examples from 5000 to 80
   - Updated Nginx proxy configuration examples
   - Updated environment variable examples

## Impact

### Benefits
- ✅ Users can now access RaCore at `http://localhost` or `http://yourdomain.com` without specifying a port
- ✅ Standard HTTP port makes the application feel more professional and production-ready
- ✅ Nginx reverse proxy configuration now uses port 80 consistently

### Important Considerations
⚠️ **Port 80 requires administrator/root privileges on most operating systems:**
- **Windows:** Run as Administrator
- **Linux/macOS:** Run with `sudo` or configure capabilities

If users don't have these privileges or port 80 is already in use, they can still use custom ports:
```bash
# Use a non-privileged port instead
export RACORE_PORT=5000
dotnet run
```

## Nginx Configuration
When Nginx is used as a reverse proxy, it will now:
- Listen on port 80 (standard HTTP)
- Proxy to RaCore on port 80 (or custom port if configured)

Previously: Nginx (port 80) → RaCore (port 5000)
Now: Nginx (port 80) → RaCore (port 80, or custom via RACORE_PORT)

## Testing
- ✅ Project builds successfully without errors
- ✅ All code changes are syntactically correct
- ✅ Documentation is consistent with code changes

## Backward Compatibility
Users who have set custom ports via the `RACORE_PORT` environment variable will continue to work as before. The change only affects the default behavior when no custom port is specified.

## Deployment Notes
When deploying RaCore:
1. Ensure the process has privileges to bind to port 80, OR
2. Set `RACORE_PORT` to a non-privileged port (>1024), OR
3. Use Nginx/Apache as a reverse proxy (running as root) to forward port 80 to RaCore on a higher port

## Files Modified
- `Abstractions/IGameClientModule.cs`
- `BOOT_SEQUENCE.md`
- `FIRST_RUN_INITIALIZATION.md`
- `README.md`
- `RaCore/Engine/BootSequenceManager.cs`
- `RaCore/Engine/FirstRunManager.cs`
- `RaCore/Engine/NginxManager.cs`
- `RaCore/Program.cs`

**Total:** 8 files changed, 42 insertions(+), 38 deletions(-)
