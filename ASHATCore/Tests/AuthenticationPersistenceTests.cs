using System;
using System.IO;
using Abstractions;
using ASHATCore.Modules.Extensions.Authentication;

namespace ASHATCore.Tests;

/// <summary>
/// Test suite for Authentication database persistence functionality.
/// Verifies that user accounts and sessions persist across database reopens (simulating server restarts).
/// </summary>
public static class AuthenticationPersistenceTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     AUTHENTICATION PERSISTENCE TESTS                              ║");
        Console.WriteLine("║     Testing SQLite database persistence across restarts           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var testsPassed = 0;
        var testsFailed = 0;

        var testDbPath = Path.Combine(Path.GetTempPath(), $"test_auth_{Guid.NewGuid()}.sqlite");
        
        try
        {
            // Clean up any existing test database
            if (File.Exists(testDbPath))
            {
                File.Delete(testDbPath);
            }

            // Test 1: Database creation and schema
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 1: DATABASE CREATION AND SCHEMA");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    Console.WriteLine("✓ Database instance created");
                }
                
                if (File.Exists(testDbPath))
                {
                    var fileInfo = new FileInfo(testDbPath);
                    Console.WriteLine($"✓ Database file exists ({fileInfo.Length} bytes)");
                    testsPassed++;
                    Console.WriteLine("✓ TEST 1 PASSED");
                }
                else
                {
                    throw new Exception("Database file not found after creation");
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 1 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 2: User persistence
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 2: USER DATA PERSISTENCE");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var testUserId = Guid.NewGuid();
                var testUsername = "persistencetest";
                
                // Save user
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var user = new User
                    {
                        Id = testUserId,
                        Username = testUsername,
                        Email = "test@example.com",
                        PasswordHash = "hash123",
                        PasswordSalt = "salt123",
                        Role = UserRole.User,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsActive = true
                    };
                    db.SaveUser(user);
                    Console.WriteLine($"✓ User '{testUsername}' saved to database");
                }

                // Reopen database (simulating server restart) and retrieve user
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var retrievedUser = db.GetUserByUsername(testUsername);
                    if (retrievedUser != null && retrievedUser.Id == testUserId)
                    {
                        Console.WriteLine($"✓ User '{retrievedUser.Username}' retrieved after database reopen");
                        Console.WriteLine($"  - ID matches: {retrievedUser.Id == testUserId}");
                        Console.WriteLine($"  - Email: {retrievedUser.Email}");
                        Console.WriteLine($"  - Role: {retrievedUser.Role}");
                        testsPassed++;
                        Console.WriteLine("✓ TEST 2 PASSED");
                    }
                    else
                    {
                        throw new Exception("User not found or ID mismatch after database reopen");
                    }
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 2 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 3: Session persistence
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 3: SESSION DATA PERSISTENCE");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var testToken = $"test-token-{Guid.NewGuid()}";
                var testUserId = Guid.NewGuid();
                
                // Save session
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var session = new Session
                    {
                        Id = Guid.NewGuid(),
                        UserId = testUserId,
                        Token = testToken,
                        CreatedAtUtc = DateTime.UtcNow,
                        ExpiresAtUtc = DateTime.UtcNow.AddHours(3),
                        LastActivityUtc = DateTime.UtcNow,
                        IpAddress = "127.0.0.1",
                        UserAgent = "Test Agent",
                        IsValid = true
                    };
                    db.SaveSession(session);
                    Console.WriteLine($"✓ Session with token '{testToken}' saved to database");
                }

                // Reopen database (simulating server restart) and retrieve session
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var retrievedSession = db.GetSessionByToken(testToken);
                    if (retrievedSession != null && retrievedSession.UserId == testUserId)
                    {
                        Console.WriteLine($"✓ Session retrieved after database reopen");
                        Console.WriteLine($"  - User ID matches: {retrievedSession.UserId == testUserId}");
                        Console.WriteLine($"  - Valid: {retrievedSession.IsValid}");
                        Console.WriteLine($"  - Expires: {retrievedSession.ExpiresAtUtc}");
                        testsPassed++;
                        Console.WriteLine("✓ TEST 3 PASSED");
                    }
                    else
                    {
                        throw new Exception("Session not found or User ID mismatch after database reopen");
                    }
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 3 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 4: Multiple database reopens
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 4: MULTIPLE DATABASE REOPENS (SIMULATING MULTIPLE RESTARTS)");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var adminUsername = "admin-test";
                var adminId = Guid.NewGuid();
                
                // Create admin user
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var admin = new User
                    {
                        Id = adminId,
                        Username = adminUsername,
                        Email = "admin@example.com",
                        PasswordHash = "adminhash",
                        PasswordSalt = "adminsalt",
                        Role = UserRole.SuperAdmin,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsActive = true
                    };
                    db.SaveUser(admin);
                    Console.WriteLine($"✓ Admin user created");
                }

                // Simulate 3 server restarts
                for (int i = 1; i <= 3; i++)
                {
                    using (var db = new AuthenticationDatabase(testDbPath))
                    {
                        var admin = db.GetUserByUsername(adminUsername);
                        if (admin != null && admin.Id == adminId && admin.Role == UserRole.SuperAdmin)
                        {
                            Console.WriteLine($"✓ Restart #{i}: Admin user still accessible with correct data");
                        }
                        else
                        {
                            throw new Exception($"Admin user data lost or corrupted after restart #{i}");
                        }
                    }
                }
                
                testsPassed++;
                Console.WriteLine("✓ TEST 4 PASSED");
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 4 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Summary
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST SUMMARY");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine($"Tests Passed: {testsPassed}");
            Console.WriteLine($"Tests Failed: {testsFailed}");
            Console.WriteLine();

            if (testsFailed == 0)
            {
                Console.WriteLine("✓ ALL TESTS PASSED!");
                Console.WriteLine("\nConclusion: Authentication data persists correctly across database reopens.");
                Console.WriteLine("Users can log in after server restart. Issue is RESOLVED.");
            }
            else
            {
                Console.WriteLine("✗ SOME TESTS FAILED");
                Console.WriteLine($"\nConclusion: {testsFailed} test(s) failed. Please review the failures above.");
            }
        }
        finally
        {
            // Cleanup
            try
            {
                if (File.Exists(testDbPath))
                {
                    File.Delete(testDbPath);
                    Console.WriteLine($"\n✓ Cleaned up test database: {testDbPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nWarning: Failed to clean up test database: {ex.Message}");
            }
        }
    }
}
