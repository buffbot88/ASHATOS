# llama.cpp Auto-Detection Implementation

## Overview
This document describes the implementation of automatic detection for llama.cpp executables in RaOS, resolving the issue where llama.cpp files were not being detected despite their presence.

## Problem Statement
RaOS was unable to detect or locate llama.cpp executable files even when present in the correct directory (e.g., E:/llama.cpp). The system required manual configuration using the `set-exe` command, which was not user-friendly.

## Solution
Implemented an automatic detection system similar to the existing PhpDetector and ApacheManager patterns.

### Key Components

#### 1. LlamaCppDetector Class
**Location**: `RaCore/Modules/Extensions/Language/LlamaCppDetector.cs`

A dedicated detector class that:
- Searches multiple common installation paths
- Tests executables by running them with `--version` flag
- Caches the detected path for performance
- Provides detailed logging during detection

#### 2. Search Strategy

##### Local Paths (Priority 1)
- `./llama.cpp/llama-cli.exe` (or `.sh` on Linux)
- `./llama.cpp/llama-server.exe`
- `./llama.cpp/llama-run.exe`
- `./llama.cpp/main.exe`
- `./llama.cpp/bin/` variants of above

##### System Paths (Priority 2)
- Executables in PATH environment variable
- `/usr/bin/` and `/usr/local/bin/` on Linux/macOS

##### Multi-Drive Windows Paths (Priority 3)
For drives C:, D:, E:, and F:
- `{Drive}:\llama.cpp\llama-cli.exe`
- `{Drive}:\llama.cpp\llama-server.exe`
- `{Drive}:\llama.cpp\llama-run.exe`
- `{Drive}:\llama.cpp\main.exe`
- `{Drive}:\llama.cpp\bin\` variants
- `{Drive}:\Program Files\llama.cpp\` variants

**Total Search Locations**: 
- Windows: ~60+ locations
- Linux/macOS: ~15 locations

### Supported Executables
The detector recognizes multiple llama.cpp executable names:
- `llama-cli.exe` / `llama-cli` (primary, modern builds)
- `llama-server.exe` / `llama-server` (server mode)
- `llama-run.exe` / `llama-run` (run mode)
- `main.exe` / `main` (legacy builds)

### Integration with AILanguageModule

#### Initialization Flow
1. Module initializes with default path `llama.cpp\main.exe`
2. If default path doesn't exist, auto-detection triggers
3. Detector searches all configured paths
4. First valid executable is selected
5. Version is extracted and logged
6. Path is cached for future use

#### Manual Override
Users can still manually set the path using:
```
set-exe <path-to-executable>
```

This clears the detection cache and allows custom paths.

### Logging and Diagnostics

The implementation provides comprehensive logging:

#### Successful Detection
```
[Module:AILanguage] INFO: Attempting to auto-detect llama.cpp executable...
[Module:AILanguage] INFO: Starting llama.cpp executable auto-detection...
[Module:AILanguage] INFO: Checking 60 possible llama.cpp locations...
[Module:AILanguage] INFO: Testing: E:\llama.cpp\llama-cli.exe
[Module:AILanguage] INFO: ✓ Found llama.cpp executable: E:\llama.cpp\llama-cli.exe
[Module:AILanguage] INFO: Auto-detected llama.cpp: E:\llama.cpp\llama-cli.exe
[Module:AILanguage] INFO: Version: llama.cpp build 1234
```

#### Failed Detection
```
[Module:AILanguage] INFO: Attempting to auto-detect llama.cpp executable...
[Module:AILanguage] INFO: Starting llama.cpp executable auto-detection...
[Module:AILanguage] INFO: Checking 60 possible llama.cpp locations...
[Module:AILanguage] WARN: llama.cpp executable not found in any common location.
[Module:AILanguage] INFO: Searched locations include:
[Module:AILanguage] INFO:   - Local: ./llama.cpp/
[Module:AILanguage] INFO:   - C:/llama.cpp/, D:/llama.cpp/, E:/llama.cpp/, F:/llama.cpp/
[Module:AILanguage] INFO:   - C:/Program Files/llama.cpp/
[Module:AILanguage] INFO: Use 'set-exe <path>' to manually configure the llama.cpp executable path.
```

## Benefits

1. **Zero Configuration**: Works out-of-the-box when llama.cpp is in standard locations
2. **Multi-Drive Support**: Automatically checks C:, D:, E:, F: drives on Windows
3. **Flexible**: Supports multiple executable names and variants
4. **Cross-Platform**: Works on Windows, Linux, and macOS
5. **Diagnostic**: Clear logging helps troubleshoot issues
6. **Performance**: Caches detected path to avoid repeated searches
7. **Non-Breaking**: Existing manual configuration still works

## Testing

### Test Scenario 1: Local Installation
- Created test executable in `./llama.cpp/llama-cli`
- Result: ✅ Successfully detected and configured

### Test Scenario 2: No Installation
- Removed all llama.cpp executables
- Result: ✅ Clear warning messages with helpful guidance

### Test Scenario 3: Multiple Executables
- Detector prioritizes and selects first valid executable
- Result: ✅ Selects llama-cli.exe as primary choice

## Compatibility

This implementation is compatible with:
- llama.cpp official builds
- Custom builds with standard naming
- Multiple llama.cpp versions
- Both server and CLI modes

## Future Enhancements

Potential improvements:
1. Add more drive letters (G:, H:, etc.) if needed
2. Support for config file with custom paths
3. Detection preference ordering (e.g., prefer llama-server over llama-cli)
4. Async detection to avoid blocking startup

## Related Files
- `RaCore/Modules/Extensions/Language/LlamaCppDetector.cs` - Detection logic
- `RaCore/Modules/Extensions/Language/AILanguageModule.cs` - Integration
- `RaCore/Modules/Extensions/SiteBuilder/PhpDetector.cs` - Similar pattern for PHP
- `RaCore/Engine/ApacheManager.cs` - Similar pattern for Apache

## References
- Issue: Bug: RaOS Fails to Detect llama.cpp Executable Files Despite Their Presence
- Implementation Date: October 2024
- Pattern: Detection pattern based on PhpDetector and ApacheManager
