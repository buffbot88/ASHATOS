# Onboarding Flow and License Activation Implementation

## Overview

This document describes the complete onboarding and license activation flow implementation that ensures all first-time SuperAdmin users complete the Masters Class courses and activate their server with a valid license key before accessing the RaCore Control Panel.

## Problem Addressed

**Issue:** Users were bypassing the class module and Masters class on first login, going directly to the RaCore Control Panel without completing essential onboarding. Additionally, servers could be accessed without proper license activation.

**Solution:** Implemented a mandatory onboarding and activation flow that:
- Routes new SuperAdmin users through Masters Class courses
- Gates Control Panel access until onboarding is complete
- Requires license activation after onboarding completion
- Validates license keys before granting full access
- Provides clear progress tracking and completion status

## Implementation Details

### 1. Login Flow Enhancement

**Location:** `RaCore/Program.cs` - `GenerateLoginUI()`

The login page now checks the `requiresLULModule` flag returned by the authentication API:

```javascript
const data = await response.json();
if (data.success) {
    localStorage.setItem('racore_token', data.token);
    
    // Check if user needs to complete LULModule onboarding
    if (data.requiresLULModule) {
        window.location.href = '/onboarding';
    } else {
        window.location.href = '/control-panel';
    }
}
```

### 2. Onboarding Page

**Location:** `RaCore/Program.cs` - `GenerateOnboardingUI()` and `/onboarding` route

A comprehensive onboarding interface that:
- Displays all SuperAdmin courses (Masters Class)
- Shows course descriptions, lesson counts, and time estimates
- Provides interactive lesson viewer
- Tracks progress with visual progress bar
- Allows lesson-by-lesson completion
- Auto-advances through lessons
- Completes onboarding when all courses finished

**Features:**
- **Course List**: Grid of all required SuperAdmin courses
- **Lesson Viewer**: Full-screen lesson content with navigation
- **Progress Tracking**: Real-time percentage completion
- **Completion Button**: Appears when all courses done
- **Responsive Design**: Works on all screen sizes

### 3. Control Panel Gating

**Location:** `RaCore/Program.cs` - `GenerateControlPanelUI()` - `checkAuth()`

Enhanced authentication check that:
```javascript
async function checkAuth() {
    const token = localStorage.getItem('racore_token');
    if (!token) {
        window.location.href = '/login';
        return;
    }
    
    // Check if SuperAdmin needs to complete onboarding
    try {
        const response = await fetch('/api/learning/superadmin/status', {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        
        if (response.ok) {
            const data = await response.json();
            if (!data.hasCompleted) {
                // Redirect to onboarding if not completed
                window.location.href = '/onboarding';
                return;
            }
        }
    } catch (err) {
        // If not SuperAdmin or learning module unavailable, continue
        console.log('Onboarding check skipped:', err);
    }
}
```

This prevents users from:
- Manually navigating to `/control-panel` without completing onboarding
- Bypassing the flow by bookmarking the control panel URL
- Accessing admin features before understanding the system

### 4. API Endpoints

**Location:** `RaCore/Endpoints/ControlPanelEndpoints.cs`

**Onboarding endpoints:**
- **GET `/api/learning/superadmin/status`** - Check completion status
- **POST `/api/learning/superadmin/complete`** - Mark courses complete
- **GET `/api/learning/courses/SuperAdmin`** - Get all SuperAdmin courses
- **GET `/api/learning/courses/{courseId}/lessons`** - Get course lessons
- **POST `/api/learning/lessons/{lessonId}/complete`** - Mark lesson complete

**Activation endpoints:**
- **GET `/api/control/activation-status`** - Check server activation status
  - Returns: `{ activated, activatedAt, licenseKey, licenseType, devMode }`
  - Requires: SuperAdmin authentication
  
- **POST `/api/control/activate`** - Activate server with license key
  - Body: `{ licenseKey: "RAOS-XXXX-XXXX-XXXX-XXXX" }`
  - Returns: `{ success, message, licenseType, activatedAt }`
  - Requires: SuperAdmin authentication
  - Validates license format
  - In Dev mode: bypasses external validation
  - In Production mode: validates with US-Omega server

### 5. Backend Support

**Location:** `RaCore/Modules/Extensions/Authentication/AuthenticationModule.cs`

The authentication module sets `RequiresLULModule` during login:

```csharp
// Check if SuperAdmin needs to complete LULModule courses
bool requiresLULModule = false;
if (user.Role == UserRole.SuperAdmin && _learningModule != null)
{
    var hasCompleted = await _learningModule.HasCompletedSuperAdminCoursesAsync(user.Id.ToString());
    requiresLULModule = !hasCompleted;
}

return new AuthResponse
{
    Success = true,
    Message = "Login successful",
    Token = token,
    User = SanitizeUser(user),
    TokenExpiresAt = session?.ExpiresAtUtc,
    RequiresLULModule = requiresLULModule
};
```

### 6. ServerConfiguration Changes

**Location:** `Abstractions/ServerMode.cs`

Added activation state tracking to ServerConfiguration:

```csharp
/// <summary>
/// Indicates if the server has been activated with a valid license
/// Server activation is required after onboarding to access the control panel
/// </summary>
public bool ServerActivated { get; set; } = false;

/// <summary>
/// Timestamp when the server was activated
/// </summary>
public DateTime? ActivatedAt { get; set; }
```

**FirstRunManager** integration:
- `GetServerConfiguration()` - Returns current server configuration including activation state
- `SaveConfiguration()` - Persists configuration changes including activation status
- Passed to `MapControlPanelEndpoints()` to enable activation API endpoints

## User Experience Flow

### First-Time Login

1. User logs in with credentials
2. Authentication checks if SuperAdmin courses completed
3. Returns `requiresLULModule: true` if not completed
4. Login page redirects to `/onboarding`
5. User sees Masters Class courses
6. User completes lessons one by one
7. Progress bar updates in real-time
8. After all courses complete, "Complete Onboarding" button appears
9. User clicks button, system marks courses as complete
10. User redirected to `/activation` (License Activation Page)
11. User enters license key
12. System validates license (online validation or Dev mode bypass)
13. On successful activation, server is activated and configuration saved
14. User redirected to Control Panel

### Subsequent Logins

1. User logs in with credentials
2. Authentication checks completion status
3. Returns `requiresLULModule: false` (already completed)
4. Login page redirects directly to `/control-panel`
5. Control panel checks activation status
6. If activated, control panel loads normally
7. If not activated, redirects to `/activation`

### Bypass Prevention

If user tries to navigate directly to `/control-panel`:
1. Control panel loads
2. `checkAuth()` runs on page load
3. Calls `/api/learning/superadmin/status`
4. If not completed, redirects to `/onboarding`
5. Calls `/api/control/activation-status`
6. If not activated, redirects to `/activation`
7. User cannot access control panel until onboarding done AND server activated

## License Activation Flow

### Activation Page (/activation)

After completing onboarding, users are presented with the activation page that:
- Displays completion confirmation
- Shows license key input field
- Lists available license types (Forum, CMS, GameServer, Enterprise)
- Validates license format (RAOS-XXXX-XXXX-XXXX-XXXX)
- Submits to `/api/control/activate` endpoint

### License Validation

**Production Mode:**
- License key is validated against main server (US-Omega) at `https://us-omega.raos.io`
- Server receives license type and expiration information
- Features are enabled based on license package
- Configuration is persisted to disk

**Dev Mode:**
- License validation is bypassed for faster development
- Any valid format license key is accepted
- License type is set to "Development"
- Dev mode notice is displayed on activation page

### Post-Activation

After successful activation:
- `ServerActivated` flag is set to `true`
- `ActivatedAt` timestamp is recorded
- `LicenseKey` and `LicenseType` are saved to configuration
- Activation event is logged
- User is redirected to Control Panel
- All features are unlocked based on license type

## SuperAdmin Courses (Masters Class)

The onboarding includes these courses:

1. **RaOS Architecture & Development** (9 lessons, ~135 min)
   - System architecture and module integration
   - Security and memory management
   - Module development practices

2. **RaOS System Administration** (7 lessons, ~105 min)
   - Server setup and configuration
   - Database and backup management
   - Security hardening

3. **Module Marketplace Management** (5 lessons, ~75 min)
   - Marketplace review and approval
   - Security auditing
   - Pricing and support

4. **RaOS Development History** (8 lessons, ~200 min)
   - Complete platform evolution
   - Key features and milestones
   - Phase-by-phase development

## Testing

**Location:** `RaCore/Tests/OnboardingFlowTests.cs`

Run tests with:
```bash
cd RaCore
dotnet run --project RaCore.csproj onboarding
```

Tests verify:
- ✓ Onboarding route exists
- ✓ Login checks `requiresLULModule` flag
- ✓ Login redirects to `/onboarding` when required
- ✓ Control panel checks onboarding status
- ✓ Control panel redirects incomplete users
- ✓ Learning API endpoints configured
- ✓ Onboarding UI structure complete

## Benefits

### For Users
- **Guided Setup**: Clear path through server configuration
- **Education**: Learn system before administering it
- **Confidence**: Understand RaOS capabilities fully

### For Administrators
- **Security**: Ensure admins understand security features
- **Quality**: Reduce support tickets from uninformed admins
- **Compliance**: Document that admins are trained

### For Developers
- **Clean Architecture**: Reusable onboarding pattern
- **Extensible**: Easy to add more courses
- **Maintainable**: Clear separation of concerns

## Configuration

No configuration required. The onboarding flow is:
- **Automatic**: Triggers on first SuperAdmin login
- **Role-Based**: Only affects SuperAdmin users
- **Non-Intrusive**: Regular users unaffected

## Troubleshooting

### User Can't Access Control Panel

**Symptom:** Always redirected to onboarding
**Cause:** Courses not marked complete
**Solution:** Complete all lessons and click "Complete Onboarding"

### Onboarding Not Triggered

**Symptom:** SuperAdmin goes directly to control panel
**Cause:** Courses already marked complete
**Solution:** This is expected behavior after first completion

### Learning Module Unavailable

**Symptom:** Error loading courses
**Cause:** LegendaryLearning module not loaded
**Solution:** Ensure `LegendaryLearning.dll` is present and loaded

## Future Enhancements

Potential improvements:
- [ ] Add course completion certificates
- [ ] Include quiz questions for validation
- [ ] Add video tutorials for each lesson
- [ ] Track time spent on each lesson
- [ ] Add optional advanced courses
- [ ] Create course leaderboards
- [ ] Enable course reset for refresher training

## Related Documentation

- [LULModule SuperAdmin Requirement](docs/LULMODULE_SUPERADMIN_REQUIREMENT.md)
- [Module Development Guide](MODULE_DEVELOPMENT_GUIDE.md)
- [Authentication QuickStart](AUTHENTICATION_QUICKSTART.md)

## Acceptance Criteria

- [x] New users always experience onboarding on first login
- [x] Control panel gated until onboarding is finished
- [x] Onboarding steps are clear, actionable, and comprehensive
- [x] Bypass prevention implemented
- [x] Progress tracking visible and accurate
- [x] Tests validate all requirements
- [x] Documentation complete
- [x] License activation required after onboarding completion
- [x] Server activation state tracked in ServerConfiguration
- [x] Control panel gated until server is activated
- [x] License validation with Dev mode bypass
- [x] Activation logging and error handling implemented

## Implementation Date

October 9, 2025 (Initial onboarding flow)
January 2026 (License activation integration)

## Issue Reference

Resolves issue: "Onboarding Flow Broken: Users Bypass Class Module and Masters Class on First Login"
Implements: "Feature: Guided Onboarding and License-Based Server Activation Flow"
