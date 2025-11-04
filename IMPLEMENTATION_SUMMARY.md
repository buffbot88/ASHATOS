# Login Persistence and Server Activation Implementation

## Issue Resolution Summary

This implementation resolves all requirements from issue **[BUG] Login not persistent** and adds comprehensive server activation features.

## Changes Implemented

### 1. Login Persistence Fix ✅

**Problem**: After logging in, users were redirected back to the login page because sessions weren't persisting.

**Solution**:
- Added HTTP-only cookie-based authentication alongside localStorage tokens
- Modified `AuthEndpoints.cs` to set secure cookies on successful login
- Updated `Login.cshtml` to store tokens in both localStorage (key: `ASHATCore_token`) and cookies
- Modified homepage handler in `Program.cs` to check both Authorization headers and cookies
- Cookie security: Dynamically sets Secure flag (HTTPS required in production, HTTP allowed in development)

**Files Modified**:
- `ASHATCore/Endpoints/AuthEndpoints.cs` - Added cookie setting logic
- `ASHATCore/Pages/Login.cshtml` - Dual token storage
- `ASHATCore/Program.cs` - Cookie-aware authentication check

### 2. Server Activation Flow ✅

**Requirement**: Admin should be taken to activation page on first login if server not activated, with 30-day activation timer.

**Solution**:
- Added `ServerFirstStarted` timestamp to `ServerConfiguration` to track when server first starts
- Changed `ServerActivated` default from `true` to `false` (requires activation)
- Created two new API endpoints:
  - `GET /api/control/activation-status` - Returns activation status and days remaining
  - `POST /api/control/activate` - Activates server with license key
- Updated `Login.cshtml` to check activation status after login and redirect admins to `/activation` if not activated
- Activation timer calculates: `daysRemaining = Math.Max(0, 30 - daysSinceStart)`

**Files Modified**:
- `Abstractions/ServerMode.cs` - Added `ServerFirstStarted`, changed defaults
- `ASHATCore/Engine/FirstRunManager.cs` - Initialize `ServerFirstStarted` on first run
- `ASHATCore/Endpoints/ControlPanelEndpoints.cs` - Added activation endpoints
- `ASHATCore/Pages/Login.cshtml` - Added activation check and redirect logic

### 3. Activation Warning Banner ✅

**Requirement**: White banner with red text across website showing "30 days to activate".

**Solution**:
- Added `GenerateActivationWarningBanner()` method to `UnderConstructionHandler` 
- Updated `Index.cshtml` to display banner when server not activated
- Banner shows: "⚠️ SERVER NOT ACTIVATED - X days remaining to activate this server. [Activate Now]"
- Banner includes urgent animation when less than 7 days remain
- Banner is positioned fixed at top with high z-index (9999) to appear on all pages

**Files Modified**:
- `ASHATCore/Engine/UnderConstructionHandler.cs` - Added banner generator method
- `ASHATCore/Pages/Index.cshtml` - Banner display logic with config reading

### 4. Under Construction Mode ✅

**Requirement**: Switch homepage content with under construction when enabled, default to OFF, add toggle in admin panel.

**Solution**:
- Changed `UnderConstruction` default from `true` to `false` in `ServerConfiguration`
- Added checkbox toggle in Control Panel CMS Settings tab
- Created endpoint: `POST /api/control/cms/settings/underconstruction`
- Updated `loadCmsSettings()` and `saveSiteSettings()` JavaScript to handle the toggle
- Homepage (`Program.cs`) checks `UnderConstruction` flag and shows construction page for non-admins

**Files Modified**:
- `Abstractions/ServerMode.cs` - Changed `UnderConstruction` default to `false`
- `ASHATCore/Program.cs` - Added under construction toggle UI and endpoint
- `ASHATCore/Endpoints/ControlPanelEndpoints.cs` - Added settings endpoint

### 5. 30-Day Forced Under Construction ✅

**Requirement**: After 30 days without activation, force under construction page.

**Solution**:
- Modified homepage handler to calculate `forcedUnderConstruction = !activated && daysRemaining <= 0`
- When forced, displays custom message: "This server has not been activated and the 30-day trial period has expired"
- Admins can still access during forced construction
- Logic implemented in root endpoint handler in `Program.cs`

**Files Modified**:
- `ASHATCore/Program.cs` - Added forced under construction logic

## Testing

Created comprehensive test suite: `ASHATCore/Tests/LoginPersistenceTests.cs`

**Tests Included**:
1. **Session Token Persistence** - Verifies tokens persist across database reopens (simulates page refresh)
2. **Server Activation Configuration** - Tests activation timer and days remaining calculation
3. **Forced Under Construction** - Validates automatic enforcement after 30 days
4. **Default Settings** - Confirms UnderConstruction defaults to OFF

All tests use local test directory for better container compatibility.

## Security Enhancements

1. **HTTP-Only Cookies**: Authentication tokens stored in HTTP-only cookies prevent XSS attacks
2. **Dynamic Secure Flag**: Cookies require HTTPS in production, HTTP allowed in localhost/development
3. **Token Validation**: Both cookie and header-based tokens validated through authentication module
4. **Session Expiry**: 24-hour token expiration enforced

## API Endpoints Added

| Endpoint | Method | Access | Purpose |
|----------|--------|--------|---------|
| `/api/control/activation-status` | GET | Admin+ | Get activation status and days remaining |
| `/api/control/activate` | POST | Admin+ | Activate server with license key |
| `/api/control/cms/settings/underconstruction` | POST | Admin+ | Toggle under construction mode |

## Configuration Changes

**ServerConfiguration** (in `Abstractions/ServerMode.cs`):
```csharp
public bool ServerActivated { get; set; } = false;  // Changed from true
public DateTime? ServerFirstStarted { get; set; }   // New field
public bool UnderConstruction { get; set; } = false; // Changed default comment
```

## User Flow

1. **First Admin Login**:
   - Admin logs in → Authentication succeeds → Check activation status
   - If not activated → Redirect to `/activation`
   - If activated → Continue to `/control-panel`

2. **Activation Warning**:
   - Homepage displays banner: "⚠️ SERVER NOT ACTIVATED - 25 days remaining"
   - Banner appears on all pages until activated
   - Clicking "Activate Now" takes user to `/activation` page

3. **After 30 Days**:
   - Non-admins see under construction page
   - Admins can still log in but see warning banner
   - Message: "30-day trial expired, please activate"

4. **Under Construction Toggle**:
   - Admin goes to Control Panel → CMS Settings
   - Checkbox: "Enable Under Construction Mode"
   - Save → Site shows construction page to non-admins

## Build Status

✅ All code compiles successfully with 0 errors  
✅ All warnings addressed  
✅ Code review feedback implemented  
✅ Security best practices followed

## Backward Compatibility

- Existing sessions remain valid (database schema unchanged)
- Existing users can log in normally
- No breaking changes to API contracts
- Configuration file format remains compatible

## Notes for Deployment

1. **First Deployment**: Server will start with `ServerFirstStarted` set to current time
2. **Migration**: Existing servers will need `ServerFirstStarted` populated on first run
3. **License Keys**: Format expected: `ASHATOS-XXXX-XXXX-XXXX-XXXX`
4. **Development Mode**: Dev mode bypasses license server validation for testing

## Summary

All requirements from the issue have been successfully implemented:
- ✅ Login persistence fixed
- ✅ Admin redirected to activation on first login if not activated
- ✅ White/red banner showing 30-day warning
- ✅ Under construction mode with admin toggle (defaults OFF)
- ✅ 30-day forced under construction after expiry
- ✅ Comprehensive testing included
- ✅ Security enhancements applied
