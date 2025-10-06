using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.UserProfiles;

/// <summary>
/// User profile management for personalization
/// </summary>
[RaModule(Category = "extensions")]
public sealed class UserProfileModule : ModuleBase
{
    public override string Name => "UserProfile";

    private readonly ConcurrentDictionary<string, UserProfile> _profiles = new();
    private string? _currentUserId;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "UserProfile: Use 'create <userId>', 'switch <userId>', 'current', 'list', 'set <key> <value>', 'get <key>', or 'delete <userId>'";

        var parts = input.Trim().Split(' ', 3);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "create" when parts.Length > 1 => CreateProfile(parts[1]),
            "switch" when parts.Length > 1 => SwitchProfile(parts[1]),
            "current" => GetCurrentProfile(),
            "list" => ListProfiles(),
            "set" when parts.Length > 2 => SetPreference(parts[1], parts[2]),
            "get" when parts.Length > 1 => GetPreference(parts[1]),
            "delete" when parts.Length > 1 => DeleteProfile(parts[1]),
            "export" when parts.Length > 1 => ExportProfile(parts[1]),
            "stats" => GetStats(),
            _ => "Unknown command. Use: create, switch, current, list, set, get, delete, export, stats"
        };
    }

    public async Task<UserProfile> CreateProfileAsync(string userId, string? displayName = null, string? role = null)
    {
        await Task.CompletedTask; // Async placeholder

        var profile = new UserProfile
        {
            UserId = userId,
            DisplayName = displayName ?? userId,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow,
            Role = role
        };

        _profiles.AddOrUpdate(userId, profile, (_, existing) => 
        {
            existing.LastActiveAt = DateTime.UtcNow;
            return existing;
        });

        LogInfo($"Created profile for user: {userId}");
        return profile;
    }

    public async Task<UserProfile?> GetProfileAsync(string userId)
    {
        await Task.CompletedTask; // Async placeholder
        return _profiles.TryGetValue(userId, out var profile) ? profile : null;
    }

    public async Task<bool> UpdatePreferenceAsync(string userId, string key, string value)
    {
        await Task.CompletedTask; // Async placeholder

        if (!_profiles.TryGetValue(userId, out var profile))
            return false;

        profile.Preferences[key] = value;
        profile.LastActiveAt = DateTime.UtcNow;
        LogInfo($"Updated preference for {userId}: {key} = {value}");
        return true;
    }

    public async Task<string?> GetPreferenceAsync(string userId, string key)
    {
        await Task.CompletedTask; // Async placeholder

        if (!_profiles.TryGetValue(userId, out var profile))
            return null;

        profile.LastActiveAt = DateTime.UtcNow;
        return profile.Preferences.TryGetValue(key, out var value) ? value : null;
    }

    public async Task<bool> SetAllowedModulesAsync(string userId, List<string> modules)
    {
        await Task.CompletedTask; // Async placeholder

        if (!_profiles.TryGetValue(userId, out var profile))
            return false;

        profile.AllowedModules = modules;
        profile.LastActiveAt = DateTime.UtcNow;
        return true;
    }

    public bool CanAccessModule(string userId, string moduleName)
    {
        if (!_profiles.TryGetValue(userId, out var profile))
            return true; // Default: allow access if no profile

        if (profile.AllowedModules.Count == 0)
            return true; // No restrictions

        return profile.AllowedModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
    }

    private string CreateProfile(string userId)
    {
        var task = CreateProfileAsync(userId);
        task.Wait();
        return $"Created profile for user: {userId}";
    }

    private string SwitchProfile(string userId)
    {
        if (!_profiles.ContainsKey(userId))
        {
            return $"Profile '{userId}' not found. Use 'create {userId}' first.";
        }

        _currentUserId = userId;
        var task = GetProfileAsync(userId);
        task.Wait();
        
        if (task.Result != null)
            task.Result.LastActiveAt = DateTime.UtcNow;

        return $"Switched to profile: {userId}";
    }

    private string GetCurrentProfile()
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return "No active profile. Use 'switch <userId>' or 'create <userId>'.";

        var task = GetProfileAsync(_currentUserId);
        task.Wait();
        var profile = task.Result;

        if (profile == null)
            return $"Current profile '{_currentUserId}' not found.";

        return $"Current Profile:\n" +
               $"  User ID: {profile.UserId}\n" +
               $"  Display Name: {profile.DisplayName}\n" +
               $"  Role: {profile.Role ?? "none"}\n" +
               $"  Created: {profile.CreatedAt:yyyy-MM-dd}\n" +
               $"  Last Active: {profile.LastActiveAt:yyyy-MM-dd HH:mm:ss}\n" +
               $"  Preferences: {profile.Preferences.Count}\n" +
               $"  Allowed Modules: {(profile.AllowedModules.Count == 0 ? "all" : string.Join(", ", profile.AllowedModules))}";
    }

    private string ListProfiles()
    {
        if (_profiles.IsEmpty)
            return "No profiles found.";

        var sb = new System.Text.StringBuilder("User Profiles:\n");
        foreach (var kvp in _profiles)
        {
            var isCurrent = kvp.Key == _currentUserId ? " (current)" : "";
            sb.AppendLine($"  {kvp.Value.UserId}: {kvp.Value.DisplayName} - {kvp.Value.Role ?? "no role"}{isCurrent}");
        }
        return sb.ToString();
    }

    private string SetPreference(string key, string value)
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return "No active profile. Use 'switch <userId>' first.";

        var task = UpdatePreferenceAsync(_currentUserId, key, value);
        task.Wait();
        
        return task.Result 
            ? $"Set {key} = {value}" 
            : "Failed to set preference.";
    }

    private string GetPreference(string key)
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return "No active profile. Use 'switch <userId>' first.";

        var task = GetPreferenceAsync(_currentUserId, key);
        task.Wait();
        
        return task.Result != null 
            ? $"{key} = {task.Result}" 
            : $"Preference '{key}' not found.";
    }

    private string DeleteProfile(string userId)
    {
        if (_profiles.TryRemove(userId, out _))
        {
            if (_currentUserId == userId)
                _currentUserId = null;
            
            LogInfo($"Deleted profile: {userId}");
            return $"Deleted profile: {userId}";
        }
        return $"Profile '{userId}' not found.";
    }

    private string ExportProfile(string userId)
    {
        if (!_profiles.TryGetValue(userId, out var profile))
            return $"Profile '{userId}' not found.";

        return JsonSerializer.Serialize(profile, _jsonOptions);
    }

    private string GetStats()
    {
        var avgPreferences = _profiles.Values.Any() ? _profiles.Values.Average(p => p.Preferences.Count) : 0;
        var withRestrictions = _profiles.Values.Count(p => p.AllowedModules.Count > 0);

        return $"UserProfile Stats:\n" +
               $"  Total profiles: {_profiles.Count}\n" +
               $"  Current user: {_currentUserId ?? "none"}\n" +
               $"  Average preferences: {avgPreferences:F1}\n" +
               $"  Profiles with module restrictions: {withRestrictions}";
    }
    
    // Social features
    
    public async Task<bool> AddFriendAsync(string userId, string friendId)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return false;
        
        if (!profile.Friends.Contains(friendId))
        {
            profile.Friends.Add(friendId);
            
            // Add activity
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Username = profile.DisplayName ?? userId,
                Type = ActivityType.FriendAdded,
                Description = $"Added {friendId} as a friend",
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, string> { { "friendId", friendId } }
            };
            profile.ActivityFeed.Insert(0, activity);
            
            LogInfo($"User {userId} added friend {friendId}");
            return true;
        }
        
        return false;
    }
    
    public async Task<bool> RemoveFriendAsync(string userId, string friendId)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return false;
        
        return profile.Friends.Remove(friendId);
    }
    
    public async Task<List<string>> GetFriendsAsync(string userId)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return new List<string>();
        
        return profile.Friends;
    }
    
    public async Task<(bool success, string? postId)> CreateSocialPostAsync(string userId, string content)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return (false, null);
        
        var postId = Guid.NewGuid().ToString();
        var post = new SocialPost
        {
            Id = postId,
            UserId = userId,
            Username = profile.DisplayName ?? userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        
        profile.Posts.Insert(0, post);
        
        // Add activity
        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Username = profile.DisplayName ?? userId,
            Type = ActivityType.PostCreated,
            Description = content.Length > 50 ? content.Substring(0, 50) + "..." : content,
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, string> { { "postId", postId } }
        };
        profile.ActivityFeed.Insert(0, activity);
        
        LogInfo($"User {userId} created social post");
        return (true, postId);
    }
    
    public async Task<bool> LikeSocialPostAsync(string postId, string userId)
    {
        await Task.CompletedTask;
        
        foreach (var profile in _profiles.Values)
        {
            var post = profile.Posts.FirstOrDefault(p => p.Id == postId);
            if (post != null)
            {
                if (!post.Likes.Contains(userId))
                {
                    post.Likes.Add(userId);
                    return true;
                }
                return false;
            }
        }
        
        return false;
    }
    
    public async Task<bool> AddSocialCommentAsync(string postId, string userId, string username, string content)
    {
        await Task.CompletedTask;
        
        foreach (var profile in _profiles.Values)
        {
            var post = profile.Posts.FirstOrDefault(p => p.Id == postId);
            if (post != null)
            {
                var comment = new SocialComment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Username = username,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };
                post.Comments.Add(comment);
                
                // Add activity
                if (_profiles.TryGetValue(userId, out var userProfile))
                {
                    var activity = new Activity
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Username = username,
                        Type = ActivityType.CommentAdded,
                        Description = $"Commented on a post",
                        Timestamp = DateTime.UtcNow,
                        Data = new Dictionary<string, string> { { "postId", postId } }
                    };
                    userProfile.ActivityFeed.Insert(0, activity);
                }
                
                return true;
            }
        }
        
        return false;
    }
    
    public async Task<List<SocialPost>> GetSocialPostsAsync(string userId)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return new List<SocialPost>();
        
        return profile.Posts;
    }
    
    public async Task<List<Activity>> GetActivityFeedAsync(string userId, int limit = 20)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return new List<Activity>();
        
        return profile.ActivityFeed.Take(limit).ToList();
    }
    
    public async Task<bool> UpdateProfileBioAsync(string userId, string bio)
    {
        await Task.CompletedTask;
        
        if (!_profiles.TryGetValue(userId, out var profile))
            return false;
        
        profile.Bio = bio;
        
        // Add activity
        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Username = profile.DisplayName ?? userId,
            Type = ActivityType.ProfileUpdated,
            Description = "Updated profile bio",
            Timestamp = DateTime.UtcNow
        };
        profile.ActivityFeed.Insert(0, activity);
        
        return true;
    }
}
