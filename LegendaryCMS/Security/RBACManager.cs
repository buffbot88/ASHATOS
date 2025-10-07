namespace LegendaryCMS.Security;

/// <summary>
/// Enhanced RBAC system for CMS
/// </summary>
public interface IRBACManager
{
    /// <summary>
    /// Check if user has permission
    /// </summary>
    bool HasPermission(string userId, string permission);

    /// <summary>
    /// Check if user has role
    /// </summary>
    bool HasRole(string userId, string role);

    /// <summary>
    /// Assign permission to user
    /// </summary>
    Task AssignPermissionAsync(string userId, string permission);

    /// <summary>
    /// Revoke permission from user
    /// </summary>
    Task RevokePermissionAsync(string userId, string permission);

    /// <summary>
    /// Assign role to user
    /// </summary>
    Task AssignRoleAsync(string userId, string role);

    /// <summary>
    /// Revoke role from user
    /// </summary>
    Task RevokeRoleAsync(string userId, string role);

    /// <summary>
    /// Get user permissions
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// Get user roles
    /// </summary>
    Task<List<string>> GetUserRolesAsync(string userId);
}

/// <summary>
/// CMS Permission definitions
/// </summary>
public static class CMSPermissions
{
    // Forum permissions
    public const string ForumView = "forum.view";
    public const string ForumPost = "forum.post";
    public const string ForumEdit = "forum.edit";
    public const string ForumDelete = "forum.delete";
    public const string ForumModerate = "forum.moderate";

    // Blog permissions
    public const string BlogView = "blog.view";
    public const string BlogCreate = "blog.create";
    public const string BlogEdit = "blog.edit";
    public const string BlogDelete = "blog.delete";
    public const string BlogPublish = "blog.publish";

    // Chat permissions
    public const string ChatJoin = "chat.join";
    public const string ChatSend = "chat.send";
    public const string ChatModerate = "chat.moderate";
    public const string ChatKick = "chat.kick";
    public const string ChatBan = "chat.ban";

    // Profile permissions
    public const string ProfileView = "profile.view";
    public const string ProfileEdit = "profile.edit";
    public const string ProfileDelete = "profile.delete";

    // Admin permissions
    public const string AdminAccess = "admin.access";
    public const string AdminUsers = "admin.users";
    public const string AdminSettings = "admin.settings";
    public const string AdminPlugins = "admin.plugins";
    public const string AdminThemes = "admin.themes";

    // System permissions
    public const string SystemConfig = "system.config";
    public const string SystemBackup = "system.backup";
    public const string SystemMigrate = "system.migrate";
}

/// <summary>
/// CMS Role definitions
/// </summary>
public static class CMSRoles
{
    public const string SuperAdmin = "superadmin";
    public const string Admin = "admin";
    public const string Moderator = "moderator";
    public const string User = "user";
    public const string Guest = "guest";
}

/// <summary>
/// RBAC Manager implementation
/// </summary>
public class RBACManager : IRBACManager
{
    private readonly Dictionary<string, List<string>> _userPermissions = new();
    private readonly Dictionary<string, List<string>> _userRoles = new();
    private readonly Dictionary<string, List<string>> _rolePermissions = new();
    private readonly object _lock = new();

    public RBACManager()
    {
        InitializeDefaultRoles();
    }

    private void InitializeDefaultRoles()
    {
        // SuperAdmin - all permissions
        _rolePermissions[CMSRoles.SuperAdmin] = new List<string>
        {
            CMSPermissions.ForumView, CMSPermissions.ForumPost, CMSPermissions.ForumEdit,
            CMSPermissions.ForumDelete, CMSPermissions.ForumModerate,
            CMSPermissions.BlogView, CMSPermissions.BlogCreate, CMSPermissions.BlogEdit,
            CMSPermissions.BlogDelete, CMSPermissions.BlogPublish,
            CMSPermissions.ChatJoin, CMSPermissions.ChatSend, CMSPermissions.ChatModerate,
            CMSPermissions.ChatKick, CMSPermissions.ChatBan,
            CMSPermissions.ProfileView, CMSPermissions.ProfileEdit, CMSPermissions.ProfileDelete,
            CMSPermissions.AdminAccess, CMSPermissions.AdminUsers, CMSPermissions.AdminSettings,
            CMSPermissions.AdminPlugins, CMSPermissions.AdminThemes,
            CMSPermissions.SystemConfig, CMSPermissions.SystemBackup, CMSPermissions.SystemMigrate
        };

        // Admin - most permissions except system-level
        _rolePermissions[CMSRoles.Admin] = new List<string>
        {
            CMSPermissions.ForumView, CMSPermissions.ForumPost, CMSPermissions.ForumEdit,
            CMSPermissions.ForumDelete, CMSPermissions.ForumModerate,
            CMSPermissions.BlogView, CMSPermissions.BlogCreate, CMSPermissions.BlogEdit,
            CMSPermissions.BlogDelete, CMSPermissions.BlogPublish,
            CMSPermissions.ChatJoin, CMSPermissions.ChatSend, CMSPermissions.ChatModerate,
            CMSPermissions.ChatKick, CMSPermissions.ChatBan,
            CMSPermissions.ProfileView, CMSPermissions.ProfileEdit,
            CMSPermissions.AdminAccess, CMSPermissions.AdminUsers, CMSPermissions.AdminSettings
        };

        // Moderator - content moderation permissions
        _rolePermissions[CMSRoles.Moderator] = new List<string>
        {
            CMSPermissions.ForumView, CMSPermissions.ForumPost, CMSPermissions.ForumEdit,
            CMSPermissions.ForumModerate,
            CMSPermissions.BlogView, CMSPermissions.BlogCreate, CMSPermissions.BlogEdit,
            CMSPermissions.ChatJoin, CMSPermissions.ChatSend, CMSPermissions.ChatModerate,
            CMSPermissions.ChatKick,
            CMSPermissions.ProfileView, CMSPermissions.ProfileEdit
        };

        // User - standard user permissions
        _rolePermissions[CMSRoles.User] = new List<string>
        {
            CMSPermissions.ForumView, CMSPermissions.ForumPost,
            CMSPermissions.BlogView, CMSPermissions.BlogCreate,
            CMSPermissions.ChatJoin, CMSPermissions.ChatSend,
            CMSPermissions.ProfileView, CMSPermissions.ProfileEdit
        };

        // Guest - minimal permissions
        _rolePermissions[CMSRoles.Guest] = new List<string>
        {
            CMSPermissions.ForumView,
            CMSPermissions.BlogView,
            CMSPermissions.ProfileView
        };
    }

    public bool HasPermission(string userId, string permission)
    {
        lock (_lock)
        {
            // Check direct permissions
            if (_userPermissions.TryGetValue(userId, out var permissions) &&
                permissions.Contains(permission))
            {
                return true;
            }

            // Check role-based permissions
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                foreach (var role in roles)
                {
                    if (_rolePermissions.TryGetValue(role, out var rolePerms) &&
                        rolePerms.Contains(permission))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool HasRole(string userId, string role)
    {
        lock (_lock)
        {
            return _userRoles.TryGetValue(userId, out var roles) && roles.Contains(role);
        }
    }

    public Task AssignPermissionAsync(string userId, string permission)
    {
        lock (_lock)
        {
            if (!_userPermissions.ContainsKey(userId))
            {
                _userPermissions[userId] = new List<string>();
            }
            if (!_userPermissions[userId].Contains(permission))
            {
                _userPermissions[userId].Add(permission);
            }
        }
        return Task.CompletedTask;
    }

    public Task RevokePermissionAsync(string userId, string permission)
    {
        lock (_lock)
        {
            if (_userPermissions.TryGetValue(userId, out var permissions))
            {
                permissions.Remove(permission);
            }
        }
        return Task.CompletedTask;
    }

    public Task AssignRoleAsync(string userId, string role)
    {
        lock (_lock)
        {
            if (!_userRoles.ContainsKey(userId))
            {
                _userRoles[userId] = new List<string>();
            }
            if (!_userRoles[userId].Contains(role))
            {
                _userRoles[userId].Add(role);
            }
        }
        return Task.CompletedTask;
    }

    public Task RevokeRoleAsync(string userId, string role)
    {
        lock (_lock)
        {
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                roles.Remove(role);
            }
        }
        return Task.CompletedTask;
    }

    public Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        lock (_lock)
        {
            var allPermissions = new HashSet<string>();

            // Add direct permissions
            if (_userPermissions.TryGetValue(userId, out var permissions))
            {
                foreach (var perm in permissions)
                {
                    allPermissions.Add(perm);
                }
            }

            // Add role-based permissions
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                foreach (var role in roles)
                {
                    if (_rolePermissions.TryGetValue(role, out var rolePerms))
                    {
                        foreach (var perm in rolePerms)
                        {
                            allPermissions.Add(perm);
                        }
                    }
                }
            }

            return Task.FromResult(allPermissions.ToList());
        }
    }

    public Task<List<string>> GetUserRolesAsync(string userId)
    {
        lock (_lock)
        {
            return Task.FromResult(_userRoles.TryGetValue(userId, out var roles)
                ? new List<string>(roles)
                : new List<string>());
        }
    }
}
