# Session Manager Verification Guide

## Overview
The sitewide session manager has been implemented to ensure login sessions work across all pages, not just the control panel.

## Implementation Details

### Files Modified/Created
1. **`/ASHATCore/wwwroot/js/session-manager.js`** (NEW)
   - Centralized session management JavaScript module
   - Automatically checks authentication on page load
   - Updates UI based on login status

2. **`/ASHATCore/Program.cs`**
   - Added `app.UseStaticFiles()` to serve static JavaScript files

3. **All Page Templates** (18 files updated)
   - Added `<script src="/js/session-manager.js"></script>` before `</body>` tag

## How It Works

### Automatic Session Checking
When any page loads, the session manager:
1. Checks `localStorage` for authentication tokens:
   - `ASHATCore_token` (primary)
   - `authToken` (fallback for compatibility)
2. Retrieves stored username and userRole
3. Updates the navigation UI automatically

### UI Updates
**When Logged In:**
- Hides "Login" and "Register" links
- Shows username with user icon (👤)
- Shows "Logout" button
- Maintains state across all pages

**When Not Logged In:**
- Shows "Login" and "Register" links
- Hides user info and logout button

### API Available
The session manager exposes a global `window.ASHATSession` object with:

```javascript
// Check session and update UI
ASHATSession.check()

// Logout user (clears tokens and redirects to home)
ASHATSession.logout()

// Validate token with server (async)
await ASHATSession.validate()

// Check if user is logged in (boolean)
ASHATSession.isLoggedIn()

// Get current username
ASHATSession.getUsername()

// Get user role
ASHATSession.getUserRole()

// Get authentication token
ASHATSession.getToken()
```

## Testing Instructions

### Manual Testing Steps

1. **Start the Server**
   ```bash
   cd ASHATCore
   dotnet run
   ```

2. **Test Login Flow**
   - Open browser to `http://localhost:8080`
   - Click "Login" in navigation
   - Enter credentials and login
   - **Expected:** After login, you should see:
     - Username displayed in navigation (e.g., "👤 admin")
     - "Logout" button appears
     - "Login" and "Register" links are hidden

3. **Test Session Persistence**
   - While logged in, navigate to different pages:
     - Home page (`/`)
     - Blogs (`/cms/blogs`)
     - Forums (`/cms/forums`)
     - Profiles (`/cms/profiles`)
     - Control Panel (`/control-panel`)
   - **Expected:** On ALL pages, you should see:
     - Your username in the navigation
     - "Logout" button present
     - Login state persists without re-authentication

4. **Test Logout**
   - Click "Logout" button from any page
   - **Expected:**
     - Redirects to home page
     - "Login" and "Register" links reappear
     - Username and "Logout" button are gone
     - Session is cleared across all pages

5. **Test Page Refresh**
   - Login to the site
   - Refresh the page (F5 or Ctrl+R)
   - **Expected:**
     - Session persists after refresh
     - Still shows logged-in state

6. **Test Multiple Tabs**
   - Login in one tab
   - Open a new tab to the same site
   - Navigate to different pages in both tabs
   - **Expected:**
     - Both tabs show logged-in state
     - Logout in one tab should clear session (refresh other tab to see)

## Verification Checklist

- [ ] Session manager JavaScript file loads without errors (check browser console)
- [ ] Login/Register links visible when not logged in
- [ ] After login, username appears in navigation
- [ ] After login, Login/Register links are hidden
- [ ] Logout button appears when logged in
- [ ] Session persists across page navigation
- [ ] Session persists after page refresh
- [ ] Logout clears session and redirects to home
- [ ] Control panel still works as before
- [ ] All pages (home, blogs, forums, profiles) show correct login state

## Browser Console Testing

Open browser developer tools (F12) and check the console for:

```javascript
// Should show session info when logged in
console.log(ASHATSession.check());

// Should return true when logged in
console.log(ASHATSession.isLoggedIn());

// Should show your username
console.log(ASHATSession.getUsername());

// Should validate token (returns boolean)
ASHATSession.validate().then(valid => console.log('Token valid:', valid));
```

## Troubleshooting

### Session Not Persisting
- Check browser console for JavaScript errors
- Verify `/js/session-manager.js` loads successfully (Network tab)
- Check localStorage in browser dev tools for tokens

### UI Not Updating
- Verify the page has `.nav` class on navigation element
- Check that links have `href="/login"` and `href="/register"` exactly
- Ensure JavaScript is enabled in browser

### Token Validation Failing
- Verify server is running and `/api/control/stats` endpoint works
- Check network tab for 401/403 errors
- Verify token format in localStorage

## Success Criteria

✅ **All pages show consistent login state**  
✅ **Session persists across navigation**  
✅ **Login/Logout works from any page**  
✅ **No need to re-login when changing pages**  
✅ **Control panel authentication still works**

## Notes

- The session manager uses localStorage for client-side state
- Token validation is done against `/api/control/stats` endpoint
- The implementation is backward compatible with existing control panel code
- Session manager is automatically initialized on page load
