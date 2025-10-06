# SiteBuilder Module

## Overview

The SiteBuilder module (formerly CMSSpawner) is a unified website building system that generates and manages CMS, Control Panels, Forums, and Profile systems.

## Refactoring from CMSSpawner

This module has been refactored from the monolithic `CMSSpawner` (4747 lines) into a modular component-based architecture for better maintainability and easier future development.

## Architecture

### Component Structure

```
SiteBuilder/
├── SiteBuilderModule.cs           # Main module coordinator
├── PhpDetector.cs                 # PHP runtime detection
├── CmsGenerator.cs                # CMS homepage generation
├── ControlPanelGenerator.cs       # Control panel generation
├── ForumGenerator.cs              # Forum system generation
├── ProfileGenerator.cs            # Profile system generation
├── IntegratedSiteGenerator.cs     # Integrated site orchestrator
└── README.md                      # This file
```

### Components

1. **SiteBuilderModule** - Main entry point, handles commands and coordinates components
2. **PhpDetector** - Detects and validates PHP runtime
3. **CmsGenerator** - Generates basic CMS with SQLite database
4. **ControlPanelGenerator** - Creates admin control panel
5. **ForumGenerator** - Builds vBulletin-style forums
6. **ProfileGenerator** - Creates MySpace-style user profiles
7. **IntegratedSiteGenerator** - Orchestrates all components for complete site

## Commands

### New Commands (Preferred)

```bash
site spawn            # Generate CMS homepage
site spawn home       # Same as 'site spawn'
site spawn control    # Generate control panel
site spawn integrated # Generate complete integrated site
site status           # Show deployment status
site detect php       # Detect PHP runtime
```

### Legacy Commands (Backward Compatible)

All `cms` commands still work:

```bash
cms spawn            # Works, same as 'site spawn'
cms spawn control    # Works, same as 'site spawn control'
cms spawn integrated # Works, same as 'site spawn integrated'
cms status           # Works, same as 'site status'
cms detect php       # Works, same as 'site detect php'
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
- ✅ Clear separation of concerns
- ✅ Simple to add new generators or features
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

### Generate Complete Integrated Site

```bash
site spawn integrated
```

This creates:
- CMS Homepage
- Admin Control Panel
- Forum System
- User Profiles
- All integrated together

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

1. Create new generator class (e.g., `BlogGenerator.cs`)
2. Implement generation logic
3. Register in `SiteBuilderModule.cs` Initialize method
4. Add command handling in `ProcessInternal`
5. Update help text

Example:

```csharp
// BlogGenerator.cs
public class BlogGenerator
{
    private readonly SiteBuilderModule _module;
    
    public BlogGenerator(SiteBuilderModule module)
    {
        _module = module;
    }
    
    public string GenerateBlog(string phpPath)
    {
        // Implementation
    }
}

// In SiteBuilderModule.cs Initialize():
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
var module = new SiteBuilderModule();
var phpDetector = new PhpDetector(module);
var phpPath = phpDetector.FindPhpExecutable();
Assert.NotNull(phpPath);
```

## Migration from CMSSpawner

The old `CMSSpawner` module has been preserved for reference but is deprecated. All functionality is now in `SiteBuilder`.

### For Users

- No action needed - all `cms` commands still work
- Recommended: Start using `site` commands instead
- Behavior is identical

### For Developers

- Import `RaCore.Modules.Extensions.SiteBuilder` instead of `CMSSpawner`
- Use component classes for specific functionality
- Extend by adding new generator components

## Future Enhancements

Planned additions:
- [ ] E-commerce generator
- [ ] Blog system generator
- [ ] Wiki generator
- [ ] Gallery/media system
- [ ] API documentation generator
- [ ] Static site generator
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
- Separate database for each component (CMS, Control Panel, Forum)
- Protected with `.htaccess` files

### Security

- Session-based authentication
- Password hashing (production ready)
- CSRF protection (to be added)
- SQL injection protection (PDO prepared statements)

---

**Module**: SiteBuilder  
**Formerly**: CMSSpawner  
**Status**: ✅ Production Ready  
**Version**: 2.0 (Refactored)  
**Lines of Code**: ~1,500 (was 4,747)  
**Components**: 7  
**Last Updated**: 2025-01-05
