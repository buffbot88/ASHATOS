# CMS Architecture Separation

## Overview

This document describes the architectural change made to separate PHP CMS files from public static HTML files for enhanced security.

## Problem

Previously, both static HTML files and PHP CMS files were generated in the same `wwwroot` directory, which had several issues:

1. **Security Risk**: PHP files were publicly accessible in wwwroot
2. **Overwriting**: CMS generation could overwrite static HTML files
3. **Exposure**: Internal logic and file structure visible in public directory

## Solution

PHP files are now generated in an **internal directory** separate from public static files.

## Architecture

### Directory Structure

```
RaCore/
├── wwwroot/              # PUBLIC - Static HTML files only
│   ├── index.html
│   ├── login.html
│   ├── control-panel.html
│   └── js/
│       ├── control-panel-api.js
│       └── control-panel-ui.js
│
├── CMS/                  # INTERNAL - PHP files (not publicly accessible)
│   ├── index.php
│   ├── admin.php
│   ├── config.php
│   ├── db.php
│   ├── control/          # Control Panel PHP
│   ├── community/        # Forum PHP
│   └── profile.php       # Profile PHP
│
└── Databases/
    └── cms_database.sqlite
```

### Key Changes

1. **CmsGenerator**: Writes PHP files to `{ServerRoot}/CMS/` instead of `{ServerRoot}/wwwroot/`
2. **WwwrootGenerator**: Continues to write static HTML to `{ServerRoot}/wwwroot/`
3. **Program.cs**: Checks for CMS in internal directory, serves static HTML from wwwroot
4. **FirstRunManager**: Backs up/regenerates internal CMS directory, not wwwroot

### Benefits

✅ **Security**: PHP files not accessible via web URL  
✅ **Separation**: Static HTML and CMS logic completely separated  
✅ **No Overwriting**: CMS generation doesn't affect static HTML  
✅ **Clean Public Directory**: wwwroot contains only intended public files  

## Implementation Details

### Modified Files

1. **CmsGenerator.cs**
   - Added `_cmsInternalPath` field
   - All PHP file writes go to internal directory
   
2. **ControlPanelGenerator.cs**, **ForumGenerator.cs**, **ProfileGenerator.cs**
   - Added `_cmsInternalPath` field
   - Subdirectories created under internal CMS directory

3. **FirstRunManager.cs**
   - Checks/backs up internal CMS directory instead of wwwroot
   - Static HTML in wwwroot preserved during CMS regeneration

4. **Program.cs**
   - `IsCmsAvailable()` checks internal CMS directory
   - Serves static HTML from wwwroot when CMS exists

5. **SiteBuilderModule.cs**
   - Status command shows both wwwroot (static) and CMS (internal)

### .gitignore

Added `CMS/` to prevent committing generated PHP files.

## Usage

### Generate Static Site Only

```bash
site spawn wwwroot
# or
site spawn static
```

This creates static HTML files in `wwwroot/`.

### Generate CMS Only

```bash
site spawn
# or
site spawn home
```

This creates PHP files in internal `CMS/` directory.

### Generate Integrated Site

```bash
site spawn integrated
```

This creates:
- Static HTML in `wwwroot/`
- PHP files in internal `CMS/`
- Both coexist without conflicts

### Check Status

```bash
site status
```

Shows:
- Static Site (wwwroot): location and HTML file count
- CMS Internal (PHP): location and PHP file count

## Future Work

To complete the architecture:

1. **PHP Executor**: Implement internal PHP processor in RaOS
2. **API Endpoints**: Create `/api/cms/*` endpoints to execute internal PHP
3. **Static HTML Updates**: Update wwwroot HTML to call API endpoints
4. **Security**: Ensure internal PHP files never exposed via direct URL

## Migration

Existing installations:
- PHP files in `wwwroot` remain until regenerated
- New generations use internal `CMS/` directory
- Use `site spawn integrated` to regenerate with new architecture

## Security Notes

⚠️ **Important**: The `CMS/` directory should NEVER be publicly accessible  
⚠️ **Important**: Static HTML should call API endpoints, not direct PHP URLs  
⚠️ **Important**: Internal structure must not be exposed in HTML source  

---

**Version**: 1.0  
**Date**: 2025-10-08  
**Author**: RaOS Team
