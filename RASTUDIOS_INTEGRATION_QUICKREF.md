# RaStudios Integration Quick Reference

## For Users

### Download
Get the complete suite: `/api/download/ashat-goddess-suite-windows` or `-linux`

### Launch
1. Run ASHAT.exe or ./ASHAT
2. Say "open RaStudios"
3. RaStudios launches automatically

### Commands
- "open rastudios" - Launch RaStudios
- "what is rastudios" - Get information

## For Developers

### Integration Points

**ASHAT Goddess (ASHATGoddessClient/Program.cs):**
- `LaunchRaStudios()` method - Handles launching
- `GetLocalResponse()` - Processes commands
- Multi-platform path detection

**Build Script (build-ashat-goddess-with-rastudios.sh):**
- Packages ASHAT + RaStudios together
- Creates complete suite for Windows and Linux
- Outputs to `wwwroot/downloads/goddess-with-rastudios/`

**Download Endpoints (ASHATCore/Endpoints/DownloadEndpoints.cs):**
- `ashat-goddess-suite-windows` - Complete Windows package
- `ashat-goddess-suite-linux` - Complete Linux package
- Backward compatible with standalone downloads

### Adding New Commands

Edit `ASHATGoddessClient/Program.cs`, `GetLocalResponse()` method:

```csharp
if (msg.Contains("your_keyword"))
{
    // Your action here
    return "Your response";
}
```

### Adding New Launch Paths

Edit `ASHATGoddessClient/Program.cs`, `LaunchRaStudios()` method:

```csharp
var possiblePaths = new[]
{
    // Add your paths here
    System.IO.Path.Combine(yourBasePath, "RaStudios.exe"),
    // ...
};
```

### Building

```bash
# Build ASHAT Goddess
dotnet build ASHATGoddessClient/ASHATGoddessClient.csproj

# Build complete suite
./build-ashat-goddess-with-rastudios.sh

# Build just RaStudios
cd RaStudios/RaStudios.WinForms
dotnet build
```

### Testing

1. Build ASHAT Goddess
2. Build RaStudios
3. Place RaStudios in expected location
4. Run ASHAT
5. Test voice commands
6. Verify launch works

## Files Modified

- `ASHATGoddessClient/Program.cs` - Added RaStudios integration
- `ASHATCore/Endpoints/DownloadEndpoints.cs` - Added suite packages
- `ASHATGoddessClient/README.md` - Documentation update
- `README.md` - Highlighted integration
- `ASHAT_RASTUDIOS_INTEGRATION.md` - Complete guide (new)
- `build-ashat-goddess-with-rastudios.sh` - Build script (new)

## Key Features

✅ Voice command recognition
✅ Multi-platform path detection  
✅ Automatic executable search
✅ Graceful error handling
✅ Complete suite packaging
✅ Backward compatible
✅ Comprehensive documentation

## Support

See: [ASHAT_RASTUDIOS_INTEGRATION.md](ASHAT_RASTUDIOS_INTEGRATION.md)
