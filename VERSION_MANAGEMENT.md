# RaOS Version Management Guide

**Current Version:** 9.3.9  
**Last Updated:** January 2025

---

## Overview

RaOS uses a unified version management system to ensure consistency across all modules, documentation, and releases. This guide explains how version numbers are managed and updated.

## Unified Version System

### RaCore/Version.cs

All version information is centralized in the `RaCore/Version.cs` file:

```csharp
namespace RaCore;

public static class RaVersion
{
    public const string Current = "9.3.9";
    public const string FullVersion = "Phase 9.3.9";
    public const int Major = 9;
    public const int Minor = 3;
    public const int Patch = 9;
    public const string Label = "Production Ready";
    public const string LastUpdated = "January 2025";
}
```

### How to Update the Version

To update RaOS to a new version:

1. **Edit `RaCore/Version.cs`**:
   - Update `Current`, `FullVersion`, `Major`, `Minor`, and `Patch`
   - Update `Label` if status changes (e.g., "Beta", "Production Ready")
   - Update `LastUpdated` to current month/year

2. **Update DOCUMENTATION_INDEX.md**:
   - Update the version number at the top of the file
   - Update lesson counts if LULModule courses changed

3. **Update LULModule History Course**:
   - Add new phase information to `LegendaryUserLearningModule.cs`
   - Update course descriptions to reflect new version

4. **Build and Test**:
   ```bash
   cd /path/to/TheRaProject
   dotnet build TheRaProject.sln
   ```

## Version Numbering Scheme

RaOS follows a semantic versioning pattern: `MAJOR.MINOR.PATCH`

### Major Version (9)
- Significant architectural changes
- Major feature additions
- Breaking changes to APIs

### Minor Version (3)
- New features and enhancements
- Non-breaking API additions
- Significant bug fixes

### Patch Version (9)
- Bug fixes
- Documentation updates
- Small enhancements
- Performance improvements

## Current Phase History

### Phase 9.3.x Releases

- **9.3.0-9.3.4**: Control Panel Integration & Documentation
- **9.3.5**: LegendaryPay & Currency Exchange
- **9.3.6**: Asset Security & Watermarking
- **9.3.7**: CloudFlare Integration & Bot Detection
- **9.3.8**: Ashat AI Coding Assistant
- **9.3.9**: Documentation Audit & Unified Version System

## Modules Using Unified Version

The following modules reference `RaVersion`:

- **UpdateModule** - Uses `RaVersion.Current` for version checking
- **LULModule** - Updated with current phase information
- **LegendaryClientBuilder** - Uses version constant (9.3.9)

## Documentation Updates

When updating versions, ensure these files are updated:

### Core Documentation
- `DOCUMENTATION_INDEX.md` - Main version number
- `README.md` - Version references
- `ROADMAP.md` - Current phase information

### Module Documentation
- `RaCore/Modules/Extensions/Learning/README.md` - Course information
- Module-specific READMEs as needed

### Developer Guides
- `CONTROL_PANEL_DEVELOPER_GUIDE.md`
- `CONTROL_PANEL_MODULE_API.md`
- `LEGENDARY_CLIENTBUILDER_WEB_INTERFACE.md`

## Best Practices

1. **Single Source of Truth**: Always use `RaVersion.cs` as the authoritative version source
2. **Consistent Naming**: Use "Phase X.Y.Z" format in documentation
3. **Document Changes**: Update LULModule history course with each significant release
4. **Test After Updates**: Always build and test after version changes
5. **Update All References**: Search for old version numbers in documentation

## Version Comparison

The `RaVersion` class provides a helper method for version comparison:

```csharp
bool isNewer = RaVersion.IsNewerThan("9.3.8"); // Returns true if 9.3.8 > current
```

## Automated Version Updates

Future enhancements may include:

- Build script to automatically update version in all files
- Version validation tests
- Automated changelog generation
- Release tagging automation

---

**For Questions**: Contact the RaOS development team or create an issue on GitHub.
