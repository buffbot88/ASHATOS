# CMSSpawner to SiteBuilder Migration Guide

## Overview

The CMSSpawner module has been **completely removed** and replaced with the SiteBuilder module with a modular architecture. This guide explains the changes and migration path.

## ⚠️ IMPORTANT: CMSSpawner Removed

**CMSSpawner has been completely removed from the codebase.** All functionality is now in SiteBuilder.

## Why the Refactoring?

**Before (CMSSpawner):**
- Single monolithic file: 4,747 lines
- All functionality mixed together
- Difficult to maintain and extend
- Hard to test individual features
- No clear separation of concerns

**After (SiteBuilder):**
- Modular architecture: 7 focused components
- Clear separation of concerns
- Easy to maintain and extend
- Testable components
- Better code organization
- Total: ~1,500 lines across multiple files

## Module Comparison

### Old Structure (CMSSpawner)
```
CMSSpawner/
├── CMSSpawnerModule.cs (4,747 lines)
└── README.md
```

### New Structure (SiteBuilder)
```
SiteBuilder/
├── SiteBuilderModule.cs           (250 lines) - Coordinator
├── PhpDetector.cs                 (150 lines) - PHP detection
├── CmsGenerator.cs                (300 lines) - CMS generation
├── ControlPanelGenerator.cs       (150 lines) - Control panel
├── ForumGenerator.cs              (150 lines) - Forum system
├── ProfileGenerator.cs            (100 lines) - Profile system
├── IntegratedSiteGenerator.cs     (200 lines) - Orchestrator
└── README.md
```

## Command Changes

### Backward Compatible

All old `cms` commands still work:

| Old Command | Status | Notes |
|-------------|--------|-------|
| `cms spawn` | ✅ Works | Redirects to SiteBuilder |
| `cms spawn home` | ✅ Works | Redirects to SiteBuilder |
| `cms spawn control` | ✅ Works | Redirects to SiteBuilder |
| `cms spawn integrated` | ✅ Works | Redirects to SiteBuilder |
| `cms status` | ✅ Works | Redirects to SiteBuilder |
| `cms detect php` | ✅ Works | Redirects to SiteBuilder |

### New Recommended Commands

| New Command | Old Equivalent | Description |
|-------------|----------------|-------------|
| `site spawn` | `cms spawn` | Generate CMS homepage |
| `site spawn home` | `cms spawn home` | Same as above |
| `site spawn control` | `cms spawn control` | Generate control panel |
| `site spawn integrated` | `cms spawn integrated` | Generate integrated site |
| `site status` | `cms status` | Show deployment status |
| `site detect php` | `cms detect php` | Detect PHP runtime |

## For Users

### Action Required

The old CMSSpawner module has been **completely removed**. You must now use `site` commands:

### Required Command Changes

| Old Command (Removed) | New Command (Use This) | Status |
|-------------|----------------|--------|
| `cms spawn` | `site spawn` | ✅ Use now |
| `cms spawn home` | `site spawn home` | ✅ Use now |
| `cms spawn control` | `site spawn control` | ✅ Use now |
| `cms spawn integrated` | `site spawn integrated` | ✅ Use now |
| `cms status` | `site status` | ✅ Use now |
| `cms detect php` | `site detect php` | ✅ Use now |

## For Developers

### Module Reference Changes

**Before:**
```csharp
using RaCore.Modules.Extensions.CMSSpawner;

var cmsModule = moduleManager.GetModuleByName("CMSSpawner") as CMSSpawnerModule;
var result = cmsModule.Process("cms spawn");
```

**After:**
```csharp
using RaCore.Modules.Extensions.SiteBuilder;

var siteBuilderModule = moduleManager.GetModuleByName("SiteBuilder") as SiteBuilderModule;
var result = siteBuilderModule.Process("site spawn");
```

### Using Components Directly

New modular architecture allows using components independently:

```csharp
using RaCore.Modules.Extensions.SiteBuilder;

// Use PHP detector independently
var phpDetector = new PhpDetector(siteBuilderModule);
var phpPath = phpDetector.FindPhpExecutable();

// Use CMS generator independently
var cmsGenerator = new CmsGenerator(siteBuilderModule, cmsRootPath);
var result = cmsGenerator.GenerateHomepage(phpPath);

// Use Control Panel generator
var controlPanelGenerator = new ControlPanelGenerator(siteBuilderModule, cmsRootPath);
var cpResult = controlPanelGenerator.GenerateControlPanel(phpPath);
```

### Testing Individual Components

Each component can now be tested independently:

```csharp
[Test]
public void TestPhpDetection()
{
    var module = new SiteBuilderModule();
    var phpDetector = new PhpDetector(module);
    
    var phpPath = phpDetector.FindPhpExecutable();
    Assert.NotNull(phpPath);
}

[Test]
public void TestCmsGeneration()
{
    var module = new SiteBuilderModule();
    var cmsGenerator = new CmsGenerator(module, "/tmp/test_cms");
    
    var result = cmsGenerator.GenerateHomepage("/usr/bin/php");
    Assert.Contains("✅", result);
}
```

### Adding New Features

Adding features is now easier with the modular structure:

```csharp
// Create new component
public class BlogGenerator
{
    private readonly SiteBuilderModule _module;
    
    public BlogGenerator(SiteBuilderModule module)
    {
        _module = module;
    }
    
    public string GenerateBlog(string phpPath)
    {
        _module.Log("Generating blog...");
        // Implementation
        return "✅ Blog generated";
    }
}

// Register in SiteBuilderModule.Initialize()
_blogGenerator = new BlogGenerator(this);

// Add command handler in ProcessInternal()
if (command == "site spawn blog")
{
    return _blogGenerator.GenerateBlog(phpPath);
}
```

## Deprecation Timeline

### Phase 1: Now - Soft Deprecation ✅
- CMSSpawner marked as `[Obsolete]`
- All functionality moved to SiteBuilder
- Backward compatibility maintained
- Warning messages in logs
- Documentation updated

### Phase 2: Future - Grace Period
- CMSSpawner still available
- Users encouraged to migrate
- Both modules work simultaneously
- Migration tools provided

### Phase 3: Future - Hard Deprecation
- CMSSpawner module removed
- Only SiteBuilder available
- All `cms` commands redirect to `site`
- Clean codebase

## Benefits of Migration

### Immediate Benefits
- ✅ Cleaner module loading (no obsolete warnings)
- ✅ Better performance (modular initialization)
- ✅ Improved logging (component-specific messages)
- ✅ Future-proof codebase

### Long-term Benefits
- ✅ Easier maintenance
- ✅ Faster feature development
- ✅ Better testability
- ✅ Improved code quality
- ✅ Simplified debugging
- ✅ Better documentation

## Support

### Questions?

- Check [SiteBuilder README](RaCore/Modules/Extensions/SiteBuilder/README.md)
- Submit issues on GitHub

### Issues?

CMSSpawner has been completely removed. Use `site` commands instead:
1. Use `site` commands instead of `cms`
2. Check PHP detection: `site detect php`
3. Review logs for detailed error messages
4. Verify file permissions and paths

## Summary

| Aspect | CMSSpawner | SiteBuilder | Migration Effort |
|--------|------------|-------------|------------------|
| **Commands** | `cms` | `site` | None (backward compatible) |
| **Module Name** | CMSSpawner | SiteBuilder | Update imports only |
| **Functionality** | ✅ Complete | ✅ Complete | None (identical) |
| **Code Size** | 4,747 lines | ~1,500 lines | N/A |
| **Architecture** | Monolithic | Modular | N/A |
| **Status** | Deprecated | Active | Ready to use |

---

**Migration Status**: ✅ Complete  
**Backward Compatibility**: ✅ 100%  
**Breaking Changes**: ❌ None  
**Action Required**: ⚠️ Optional (recommended)  
**Completion Date**: 2025-01-05
