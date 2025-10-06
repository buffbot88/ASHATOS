# Self Healing Module - Quick Reference

## What's New?

The Self Healing module now provides **automatic site-wide error detection and repair** during boot and on-demand.

## Automatic Features (During Boot)

✅ **Detects and creates missing directories**
- Databases, Apache, php, Admins folders

✅ **Monitors module health**  
- Checks all 36+ modules
- Detects degraded/unhealthy modules
- Automatically attempts recovery

✅ **Runs system-wide diagnostics**
- Critical directories check
- File permissions check
- Configuration files validation
- Module initialization status
- System resources monitoring

✅ **Automatic repairs**
- Creates missing directories
- Reinitializes failed modules
- Reports permission issues

## Manual Commands

| Command | Description |
|---------|-------------|
| `selfhealing diagnose` | Show diagnostics report (no fixes) |
| `selfhealing diagnose fix` | Run diagnostics and auto-fix issues |
| `selfhealing check` | Check module health status |
| `selfhealing health` | View current health status |
| `selfhealing recover` | Attempt manual recovery |
| `selfhealing log` | View last 10 recovery actions |
| `selfhealing stats` | View system health statistics |

## Diagnostic Areas

1. **Critical Directories** - Ensures required folders exist
2. **File System Permissions** - Verifies write access
3. **Configuration Files** - Validates Apache/PHP configs
4. **Module Initialization** - Checks module health
5. **System Resources** - Monitors disk space & memory

## Status Indicators

- ✓ **Passed** - No issues found
- ⚠ **Warning** - Minor issues, system still functional
- ✗ **Failed** - Critical issues requiring attention

## Example Boot Output

```
    ✨ Health check complete! ✨
       (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ Healthy: 36

    🔍 Running system-wide diagnostics...
       Summary: 5 passed, 0 failed
       Issues: 0, Warnings: 3
       
       ✓ Critical Directories
       ✓ File System Permissions
       ✓ Configuration Files
       ✓ Module Initialization
       ✓ System Resources
```

## Recovery Actions

The module can automatically:
- ✅ Create missing directories
- ✅ Reinitialize failed modules  
- ⚠️ Report permission issues (may need admin rights)

## When to Use Manual Commands

Use `selfhealing diagnose fix` when:
- System seems unstable
- Modules are reporting errors
- After manual configuration changes
- To verify system health

## Troubleshooting

**Q: Seeing permission warnings?**  
A: May need to run with elevated privileges (sudo/Administrator)

**Q: Module keeps failing health checks?**  
A: Check module-specific logs and dependencies

**Q: Diagnostics show warnings?**  
A: Warnings are informational - system will use defaults

## Learn More

See `SELFHEALING_ENHANCED.md` for detailed documentation.

---

*Self Healing Module v2.0 - Auto-detect, Auto-diagnose, Auto-repair*
