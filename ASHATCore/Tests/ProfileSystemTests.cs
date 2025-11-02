using System;
using ASHATCore.Modules.Extensions.UserProfiles;
using Abstractions;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for the Profile System - verifying MySpace-style functionality
/// </summary>
public class ProfileSystemTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Profile System Tests                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        TestAdminProfileCreation();
        TestUserProfileCreation();
        TestProfileRetrieval();
        TestFriendSystem();
        TestSocialPosts();
        TestProfileBio();
        TestActivityFeed();
        
        Console.WriteLine();
        Console.WriteLine("✓ All Profile System tests completed successfully!");
    }
    
    private static void TestAdminProfileCreation()
    {
        Console.Write("Testing Admin profile creation on initialization... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        
        var adminProfile = module.GetProfileAsync("admin").Result;
        
        if (adminProfile == null)
            throw new Exception("Admin profile not created");
        if (adminProfile.UserId != "admin")
            throw new Exception("Admin profile has wrong UserId");
        if (adminProfile.DisplayName != "Admin")
            throw new Exception("Admin profile has wrong DisplayName");
        if (adminProfile.Role != "Administrator")
            throw new Exception("Admin profile has wrong Role");
        if (string.IsNullOrEmpty(adminProfile.Bio))
            throw new Exception("Admin profile missing Bio");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestUserProfileCreation()
    {
        Console.Write("Testing user profile creation... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        
        var profile = module.CreateProfileAsync("testuser", "Test User", "Member").Result;
        
        if (profile == null)
            throw new Exception("Profile not created");
        if (profile.UserId != "testuser")
            throw new Exception("Profile has wrong UserId");
        if (profile.DisplayName != "Test User")
            throw new Exception("Profile has wrong DisplayName");
        if (profile.Role != "Member")
            throw new Exception("Profile has wrong Role");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestProfileRetrieval()
    {
        Console.Write("Testing profile retrieval... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        module.CreateProfileAsync("testuser2", "Test User 2").Wait();
        
        var profile = module.GetProfileAsync("testuser2").Result;
        
        if (profile == null)
            throw new Exception("Profile not retrieved");
        if (profile.UserId != "testuser2")
            throw new Exception("Wrong profile retrieved");
        if (profile.DisplayName != "Test User 2")
            throw new Exception("Profile has wrong DisplayName");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestFriendSystem()
    {
        Console.Write("Testing friend system... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        module.CreateProfileAsync("user1", "User One").Wait();
        module.CreateProfileAsync("user2", "User Two").Wait();
        
        var result = module.AddFriendAsync("user1", "user2").Result;
        var friends = module.GetFriendsAsync("user1").Result;
        
        if (!result)
            throw new Exception("Failed to add friend");
        if (!friends.Contains("user2"))
            throw new Exception("Friend not in friends list");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestSocialPosts()
    {
        Console.Write("Testing social posts... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        module.CreateProfileAsync("poster", "Poster User").Wait();
        
        var (success, postId) = module.CreateSocialPostAsync("poster", "Hello, MySpace!").Result;
        var posts = module.GetSocialPostsAsync("poster").Result;
        
        if (!success)
            throw new Exception("Failed to create post");
        if (string.IsNullOrEmpty(postId))
            throw new Exception("Post ID is empty");
        if (posts.Count != 1)
            throw new Exception("Wrong number of posts");
        if (posts[0].Content != "Hello, MySpace!")
            throw new Exception("Post has wrong content");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestProfileBio()
    {
        Console.Write("Testing profile bio update... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        module.CreateProfileAsync("biouser", "Bio User").Wait();
        
        var result = module.UpdateProfileBioAsync("biouser", "This is my awesome bio!").Result;
        var profile = module.GetProfileAsync("biouser").Result;
        
        if (!result)
            throw new Exception("Failed to update bio");
        if (profile == null)
            throw new Exception("Profile not found");
        if (profile.Bio != "This is my awesome bio!")
            throw new Exception("Bio not updated correctly");
        
        Console.WriteLine("✓ Pass");
    }
    
    private static void TestActivityFeed()
    {
        Console.Write("Testing activity feed... ");
        
        var module = new UserProfileModule();
        module.Initialize(null);
        module.CreateProfileAsync("activeuser", "Active User").Wait();
        module.CreateProfileAsync("friend1", "Friend One").Wait(); // Create friend profile first
        module.AddFriendAsync("activeuser", "friend1").Wait();
        module.CreateSocialPostAsync("activeuser", "My first post!").Wait();
        
        var activities = module.GetActivityFeedAsync("activeuser", 10).Result;
        
        if (activities.Count < 2)
            throw new Exception("Activity feed missing activities");
        
        Console.WriteLine("✓ Pass");
    }
}
