# Onboarding Flow - Quick Reference

## Overview
First-time SuperAdmin users must complete Masters Class courses before accessing the Control Panel.

## User Flow

```
┌─────────────┐
│   Login     │
│   Page      │
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│  Authentication     │
│  Check Completion   │
└──────┬──────┬───────┘
       │      │
  Not  │      │ Already
  Done │      │ Complete
       │      │
       ▼      ▼
┌───────────┐ ┌───────────────┐
│ Onboarding│ │ Control Panel │
│   Page    │ │               │
└─────┬─────┘ └───────────────┘
      │
      │ Complete
      │ Courses
      ▼
┌───────────────┐
│ Mark Complete │
│   & Redirect  │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│ Control Panel │
└───────────────┘
```

## Routes

| Route | Purpose | Access |
|-------|---------|--------|
| `/login` | User authentication | Public |
| `/onboarding` | Masters Class courses | SuperAdmin (incomplete) |
| `/control-panel` | Admin interface | SuperAdmin (complete) |

## API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/login` | POST | Authenticate & check status |
| `/api/learning/superadmin/status` | GET | Check completion status |
| `/api/learning/courses/SuperAdmin` | GET | Get all courses |
| `/api/learning/courses/{id}/lessons` | GET | Get course lessons |
| `/api/learning/lessons/{id}/complete` | POST | Mark lesson complete |
| `/api/learning/superadmin/complete` | POST | Mark onboarding complete |

## Key Files

- **RaCore/Program.cs** - Login, onboarding, and control panel UI
- **RaCore/Endpoints/ControlPanelEndpoints.cs** - Learning API endpoints
- **RaCore/Modules/Extensions/Authentication/AuthenticationModule.cs** - Auth logic
- **LegendaryLearning/LegendaryUserLearningModule.cs** - Course content

## SuperAdmin Courses

1. **RaOS Architecture & Development** (9 lessons, 135 min)
2. **RaOS System Administration** (7 lessons, 105 min)
3. **Module Marketplace Management** (5 lessons, 75 min)
4. **RaOS Development History** (8 lessons, 200 min)

**Total:** 29 lessons, ~515 minutes (~8.5 hours)

## Testing

Run validation tests:
```bash
cd RaCore
dotnet run --project RaCore.csproj onboarding
```

## Bypass Prevention

- ✅ Login checks `requiresLULModule` flag
- ✅ Control panel checks status on load
- ✅ Direct URL navigation blocked
- ✅ Incomplete users always redirected

## Completion Criteria

All lessons in all SuperAdmin courses must be marked complete:
- Read all lesson content
- Click "Mark Complete" for each lesson
- Complete all 29 lessons across 4 courses
- Click "Complete Onboarding & Access Control Panel"

## Common Scenarios

### New SuperAdmin First Login
1. Login → Redirect to `/onboarding`
2. Complete courses → Click complete button
3. Redirect to `/control-panel`

### Existing SuperAdmin Login
1. Login → Redirect to `/control-panel` (already complete)

### Direct URL Navigation Attempt
1. Navigate to `/control-panel`
2. Page loads, `checkAuth()` runs
3. Status check: not complete → Redirect to `/onboarding`

### Non-SuperAdmin User
1. Login → Redirect to `/control-panel` (no onboarding required)
2. Access based on role permissions

## Troubleshooting

**Problem:** Can't access control panel  
**Solution:** Complete all lessons and click "Complete Onboarding"

**Problem:** Progress not saving  
**Solution:** Check authentication token is valid

**Problem:** Courses not loading  
**Solution:** Ensure LegendaryLearning module is loaded

## Configuration

No configuration needed - automatic based on:
- User role (SuperAdmin only)
- Completion status (tracked per user)
- Backend detection (via API)

## Future Enhancements

- Course completion certificates
- Quiz validation questions
- Video tutorials
- Time tracking analytics
- Advanced optional courses
- Course leaderboards
- Refresher training option

## Related Documentation

- [Full Implementation Guide](ONBOARDING_FLOW_IMPLEMENTATION.md)
- [LULModule SuperAdmin Requirement](LULMODULE_SUPERADMIN_REQUIREMENT.md)
- [Module Development Guide](../MODULE_DEVELOPMENT_GUIDE.md)

## Issue Reference

**Issue:** Onboarding Flow Broken - Users Bypass Class Module and Masters Class on First Login  
**Status:** ✅ Resolved  
**PR:** #[PR_NUMBER]  
**Date:** October 9, 2025
