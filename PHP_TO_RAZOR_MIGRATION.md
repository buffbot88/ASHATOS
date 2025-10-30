# PHP to Razor/Blazor Migration Guide

## Overview

This document describes the migration from PHP-based CMS templates to pure .NET using Razor Pages and Blazor components in ASHATOS/LegendaryCMS.

## Status

**✅ COMPLETED** - LegendaryCMS now uses pure .NET architecture
- PHP file generation removed
- Razor Pages implemented for Forums, Blogs, Profiles
- Blazor components created for reusable UI elements
- Legacy PHP methods marked as obsolete

## Architecture Changes

### Before (Legacy PHP)
```
User Request → Apache/Nginx → PHP-FPM → PHP Template → Response
```

### After (Pure .NET)
```
User Request → Kestrel → Razor Page/Blazor → Response
```

## What Changed

### 1. SiteBuilderModule
- ✅ Updated help text to reflect pure .NET architecture
- ✅ Removed PHP references from initialization messages
- ✅ Added Razor Pages routes to status display

### 2. WwwrootGenerator
- ✅ Removed `GeneratePhpIni()` method
- ✅ Updated Nginx config to proxy to Kestrel (no PHP-FPM)
- ✅ Updated Apache config to proxy to Kestrel (no mod_php)
- ✅ Updated success messages to mention Razor/Blazor

### 3. ApacheManager
- ✅ Marked PHP methods as obsolete with clear deprecation messages:
  - `ScanForPhpConfig()` - PHP config scanning deprecated
  - `ScanForPhpFolder()` - PHP folder detection deprecated
  - `VerifyPhpConfig()` - PHP verification deprecated
  - `ConfigurePhpIni()` - PHP configuration deprecated
- ✅ Updated class documentation to reflect pure .NET architecture

### 4. PhpDetector
- ✅ Marked entire class as obsolete
- ✅ Updated documentation to indicate deprecation
- ✅ Modified error messages to recommend .NET migration

### 5. New Razor Pages Created
- ✅ `/Pages/CMS/Forums/Index.cshtml[.cs]` - Forum categories and forums list
- ✅ `/Pages/CMS/Blogs/Index.cshtml[.cs]` - Blog posts list
- ✅ `/Pages/CMS/Profiles/Index.cshtml[.cs]` - User profiles

### 6. New Blazor Components Created
- ✅ `/Components/CMS/ForumPost.razor` - Reusable forum post component
- ✅ `/Components/CMS/BlogPostCard.razor` - Reusable blog card component

## Migration Benefits

### Performance
- ✅ **No external PHP-FPM process** - Everything runs in Kestrel
- ✅ **Single runtime** - .NET handles all processing
- ✅ **Async/await** - Better resource utilization
- ✅ **Compiled code** - Faster execution than interpreted PHP

### Security
- ✅ **Built-in XSS protection** - Razor auto-encodes output
- ✅ **CSRF tokens** - Automatic token validation
- ✅ **Type safety** - Compile-time checking prevents injection attacks
- ✅ **No PHP vulnerabilities** - Eliminated entire PHP attack surface

### Maintainability
- ✅ **Single language** - C# for everything
- ✅ **Strong typing** - IntelliSense and compile-time errors
- ✅ **Modern tooling** - Visual Studio, Rider, VS Code support
- ✅ **Unit testable** - PageModels are easily testable

### Developer Experience
- ✅ **Better IDE support** - Full IntelliSense for Razor
- ✅ **Debugging** - Step through C# code easily
- ✅ **NuGet packages** - Rich ecosystem
- ✅ **Modern C# features** - LINQ, pattern matching, records, etc.

## Backward Compatibility

### Legacy PHP Support
For systems that still require PHP (not recommended):
- ✅ PHP methods marked with `[Obsolete]` attribute
- ✅ Compile-time warnings guide developers to migrate
- ✅ Methods still functional for transition period
- ⚠️ Will be removed in future major version

### Migration Timeline
- **Phase 1 (Current)**: PHP methods deprecated but functional
- **Phase 2 (Next Release)**: PHP methods generate runtime warnings
- **Phase 3 (Future)**: PHP methods removed entirely

## Testing

### Tests Updated
The following tests were intentionally **not** updated to maintain awareness of PHP deprecation:
- `HttpPhpFtpUpdateCapabilitiesTest.cs` - Shows obsolete warnings as expected
- Other tests continue to work with new architecture

### New Test Recommendations
When adding tests for Razor Pages:
```csharp
[Fact]
public void ForumIndexPage_LoadsCategories()
{
    var pageModel = new ForumIndexModel();
    pageModel.OnGet();
    Assert.NotEmpty(pageModel.Categories);
}
```

## Configuration Changes

### Nginx (Linux)
Before:
```nginx
location ~ \.php$ {
    fastcgi_pass 127.0.0.1:9000;
    # ... PHP-FPM config
}
```

After:
```nginx
location / {
    proxy_pass http://localhost:7077;
    # ... Kestrel proxy config
}
```

### Apache (Linux)
Before:
```apache
<FilesMatch \.php$>
    SetHandler "proxy:unix:/var/run/php/php8.1-fpm.sock|fcgi://localhost/"
</FilesMatch>
```

After:
```apache
ProxyPreserveHost On
ProxyPass / http://localhost:7077/
ProxyPassReverse / http://localhost:7077/
```

### Windows
No changes needed - Windows already used pure Kestrel.

## Deployment

### New Deployments
1. Deploy ASHATCore with .NET 9 runtime
2. No PHP installation required
3. Configure Nginx/Apache as reverse proxy (Linux only)
4. Kestrel serves all content

### Existing Deployments
1. Update ASHATCore to latest version
2. PHP-FPM can be removed (optional)
3. Update Nginx/Apache configs to proxy to Kestrel
4. Restart services

## API Integration

Razor Pages integrate with LegendaryCMS module API:

```csharp
public class IndexModel : PageModel
{
    private readonly ILegendaryCMSModule _cms;
    
    public IndexModel(ILegendaryCMSModule cms)
    {
        _cms = cms;
    }
    
    public async Task OnGetAsync()
    {
        // Call LegendaryCMS API
        var forums = await _cms.GetForumsAsync();
        // Render with Razor
    }
}
```

## Troubleshooting

### Issue: "PHP methods are obsolete"
**Solution**: This is expected. Migrate to Razor Pages as shown in this guide.

### Issue: "Old PHP templates not working"
**Solution**: Convert to Razor Pages using examples in `/Pages/CMS/` directory.

### Issue: "Need dynamic JavaScript features"
**Solution**: Use Blazor components for interactive UI elements.

## Resources

- [ASP.NET Core Razor Pages](https://docs.microsoft.com/aspnet/core/razor-pages)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [LegendaryCMS Module](../../../LegendaryCMS/README.md)
- [Razor Pages Examples](../Pages/CMS/README.md)

## Support

For questions or issues with migration:
1. Review example Razor Pages in `/Pages/CMS/`
2. Check LegendaryCMS module documentation
3. File issue on GitHub

## Summary

✅ **Migration Complete** - LegendaryCMS is now pure .NET
✅ **No PHP Required** - All features use Razor/Blazor
✅ **Better Performance** - Single runtime, compiled code
✅ **Improved Security** - Built-in protections, no PHP vulnerabilities
✅ **Modern Architecture** - Future-proof .NET ecosystem

---

**Last Updated**: October 30, 2025  
**Version**: 1.0  
**Status**: Production Ready
