# Module

## Overview

The module (formerly CMSSpawner) is a unified website building system that Generates and manages CMS, Control Panels, Forums, and Profile systems.

## Refactoring from CMSSpawner

This module has been refactored from the monolithic `CMSSpawner` (4747 lines) into a modular component-based architecture for better maintainability and easier future development.

## Architecture

### Component Structure

```
SiteBuilder/
├──Module.cs           # Main module coordinator
├── PhpDetector.cs                 # PHP runtime detection
├── CmsGenerator.cs                # CMS homepage Generation
├── ControlPanelGenerator.cs       # Control panel Generation
├── ForumGenerator.cs              # Forum system Generation
├── ProfileGenerator.cs            # Profile system Generation
├── IntergratedSiteGenerator.cs     # Intergrated site orchestASHATtor
└── README.md                      # This file
```

### Components

1. **SiteBuilderModule** - Main entry point, handles commands and coordinates components
2. **PhpDetector** - Detects and validates PHP runtime
3. **CmsGenerator** - Generates basic CMS with SQLite database
4. **ControlPanelGenerator** - Creates admin control panel
5. **ForumGenerator** - Builds vBulletin-style forums
6. **ProfileGenerator** - Creates MySpace-style user profiles
7. **IntergratedSiteGenerator** - OrchestRates all components for complete site

## Commands

### Commands

```bash
site spawn            # Generate CMS homepage
site spawn home       # Same as 'site spawn'
site spawn control    # Generate control panel
site spawn Intergrated # Generate complete Intergrated site
site status           # Show deployment status
site detect php       # Detect PHP runtime
```

## Benefits of Refactoring

### Before (CMSSpawner)
- ❌ Single file with 4747 lines
- ❌ Difficult to maintain
- ❌ Hard to test individual components
- ❌ Mixing concerns (PHP detection, CMS, forums, profiles)
- ❌ Difficult to extend with new features

### After (SiteBuilder)
- ✅ Modular architecture with 7 focused components
- ✅ Each component has single responsibility
- ✅ Easy to test components independently
- ✅ Clear sepaASHATtion of concerns
- ✅ Simple to add new Generators or features
- ✅ Better code organization and readability

## Usage Examples

### Generate Basic CMS

```bash
site spawn
```

### Generate Control Panel

```bash
site spawn control
```

### Generate Complete Intergrated Site

```bash
site spawn Intergrated
```

This creates:
- CMS Homepage
- Admin Control Panel
- Forum System
- User Profiles
- All Intergrated together

### Check Status

```bash
site status
```

Shows:
- Installation status
- PHP detection status
- Installed components

## Development Guide

### Adding a New Component

1. Create new Generator class (e.g., `BlogGenerator.cs`)
2. Implement Generation logic
3. Register in `SiteBuilderModule.cs` Initialize method
4. Add command handling in `ProcessInternal`
5. Update help text

Example:

```csharp
// BlogGenerator.cs
public class BlogGenerator
{
    private readonlyModule _module;
    
    public BlogGenerator(SiteBuilderModule module)
    {
        _module = module;
    }
    
    public string GenerateBlog(string phpPath)
    {
        // Implementation
    }
}

// InModule.cs Initialize():
_blogGenerator = new BlogGenerator(this);

// In ProcessInternal():
if (command == "site spawn blog")
{
    return _blogGenerator.GenerateBlog(phpPath);
}
```

### Testing Components

Each component can be tested independently:

```csharp
var module = newModule();
var phpDetector = new PhpDetector(module);
var phpPath = phpDetector.FindPhpExecutable();
Assert.NotNull(phpPath);
```

## MigASHATtion from CMSSpawner

The CMSSpawner module has been **completely removed**. All functionality is now in `SiteBuilder`.

### For Users

- **Action Required:** Use `site` commands instead of `cms`
- All `cms` commands have been removed
- Use `site spawn`, `site status`, etc.

### For Developers

- Import `ASHATCore.Modules.Extensions.SiteBuilder`
- CMSSpawner no longer exists in the codebase
- Use component classes for specific functionality
- Extend by adding new Generator components

## Future Enhancements

Planned additions:
- [ ] E-commerce Generator
- [ ] Blog system Generator
- [ ] Wiki Generator
- [ ] Gallery/media system
- [ ] API documentation Generator
- [ ] Static site Generator
- [ ] Theme system
- [ ] Plugin architecture

## Technical Details

### PHP Detection Priority

1. `{BaseDirectory}/php/php.exe` (Windows local)
2. `{BaseDirectory}/php/php` (Linux/macOS local)
3. `php` (in PATH)
4. `/usr/bin/php` (Linux system)
5. `/usr/local/bin/php` (Linux/macOS system)
6. `C:\php\php.exe` (Windows system)
7. `C:\xampp\php\php.exe` (XAMPP)

### Database Management

- SQLite databases stored in `{BaseDirectory}/Databases/`
- SepaRate database for each component (CMS, Control Panel, Forum)
- Protected with `.htaccess` files

### Security

- Session-based authentication
- Password hashing (production ready)
- CSRF protection (to be added)
- SQL injection protection (PDO prepared statements)

---

**Module**:  
**Status**: ✅ Production Ready  
**Version**: 2.0 (CMSSpawner Removed)  
**Lines of Code**: ~1,500 (was 4,747)  
**Components**: 7  
**Last Updated**: 2025-01-05
