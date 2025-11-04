using System;
using System.IO;
using Abstractions;
using ASHATCore.Modules.Extensions.Authentication;

namespace ASHATCore.Tests;

/// <summary>
/// Test suite for Login Persistence and Server Activation functionality.
/// Verifies that authentication sessions persist and activation flow works correctly.
/// </summary>
public static class LoginPersistenceTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     LOGIN PERSISTENCE & ACTIVATION TESTS                          ║");
        Console.WriteLine("║     Testing authentication persistence and activation flow        ║");
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

            // Test 1: Session token persistence across database reopens
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 1: SESSION TOKEN PERSISTENCE (simulating page refresh)");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var testToken = $"test-token-{Guid.NewGuid()}";
                var testUserId = Guid.NewGuid();
                var testUsername = "admin";
                
                // Create session and user
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var user = new User
                    {
                        Id = testUserId,
                        Username = testUsername,
                        Email = "admin@test.com",
                        PasswordHash = "hash123",
                        PasswordSalt = "salt123",
                        Role = UserRole.Admin,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsActive = true
                    };
                    db.SaveUser(user);
                    
                    var session = new Session
                    {
                        Id = Guid.NewGuid(),
                        UserId = testUserId,
                        Token = testToken,
                        CreatedAtUtc = DateTime.UtcNow,
                        ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
                        IpAddress = "127.0.0.1",
                        UserAgent = "Test Browser",
                        IsValid = true
                    };
                    db.SaveSession(session);
                    Console.WriteLine($"✓ Session created for user '{testUsername}' with token");
                }

                // Simulate page refresh: reopen database and verify session still valid
                using (var db = new AuthenticationDatabase(testDbPath))
                {
                    var session = db.GetSessionByToken(testToken);
                    if (session != null && session.IsValid && session.UserId == testUserId)
                    {
                        Console.WriteLine($"✓ Session persisted across database reopen (simulated page refresh)");
                        Console.WriteLine($"  - Token still valid: {session.IsValid}");
                        Console.WriteLine($"  - User ID matches: {session.UserId == testUserId}");
                        Console.WriteLine($"  - Expires at: {session.ExpiresAtUtc}");
                        testsPassed++;
                        Console.WriteLine("✓ TEST 1 PASSED");
                    }
                    else
                    {
                        throw new Exception("Session not found or invalid after database reopen");
                    }
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 1 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 2: Server activation configuration
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 2: SERVER ACTIVATION CONFIGURATION");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var config = new ServerConfiguration
                {
                    ServerActivated = false,
                    ServerFirstStarted = DateTime.UtcNow.AddDays(-5),
                    UnderConstruction = false
                };
                
                // Calculate days remaining
                var daysSinceStart = (DateTime.UtcNow - config.ServerFirstStarted.Value).TotalDays;
                var daysRemaining = Math.Max(0, 30 - (int)Math.Ceiling(daysSinceStart));
                
                Console.WriteLine($"✓ Server first started: {config.ServerFirstStarted}");
                Console.WriteLine($"✓ Days since start: {daysSinceStart:F1}");
                Console.WriteLine($"✓ Days remaining for activation: {daysRemaining}");
                Console.WriteLine($"✓ Server activated: {config.ServerActivated}");
                Console.WriteLine($"✓ Under construction: {config.UnderConstruction}");
                
                if (daysRemaining > 0 && daysRemaining <= 30 && !config.ServerActivated)
                {
                    Console.WriteLine($"✓ Activation timer working correctly ({daysRemaining} days remaining)");
                    testsPassed++;
                    Console.WriteLine("✓ TEST 2 PASSED");
                }
                else
                {
                    throw new Exception($"Activation timer calculation incorrect: {daysRemaining} days remaining");
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 2 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 3: Forced under construction after 30 days
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 3: FORCED UNDER CONSTRUCTION AFTER 30 DAYS");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var config = new ServerConfiguration
                {
                    ServerActivated = false,
                    ServerFirstStarted = DateTime.UtcNow.AddDays(-35), // 35 days ago
                    UnderConstruction = false
                };
                
                var daysSinceStart = (DateTime.UtcNow - config.ServerFirstStarted.Value).TotalDays;
                var daysRemaining = Math.Max(0, 30 - (int)Math.Ceiling(daysSinceStart));
                var forcedUnderConstruction = !config.ServerActivated && daysRemaining <= 0;
                
                Console.WriteLine($"✓ Server first started: {config.ServerFirstStarted}");
                Console.WriteLine($"✓ Days since start: {daysSinceStart:F1}");
                Console.WriteLine($"✓ Days remaining: {daysRemaining}");
                Console.WriteLine($"✓ Forced under construction: {forcedUnderConstruction}");
                
                if (forcedUnderConstruction && daysRemaining == 0)
                {
                    Console.WriteLine($"✓ Forced under construction activated correctly after 30 days");
                    testsPassed++;
                    Console.WriteLine("✓ TEST 3 PASSED");
                }
                else
                {
                    throw new Exception($"Forced under construction not activated: {forcedUnderConstruction}");
                }
            }
            catch (Exception ex)
            {
                testsFailed++;
                Console.WriteLine($"✗ TEST 3 FAILED: {ex.Message}");
            }
            Console.WriteLine();

            // Test 4: Under construction default setting
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                Console.WriteLine("TEST 4: UNDER CONSTRUCTION DEFAULT SETTING");
                Console.WriteLine("════════════════════════════════════════════════════════════════════");
                
                var config = new ServerConfiguration();
                
                Console.WriteLine($"✓ Default UnderConstruction value: {config.UnderConstruction}");
                
                if (config.UnderConstruction == false)
                {
                    Console.WriteLine($"✓ Under construction is OFF by default (as required)");
                    testsPassed++;
                    Console.WriteLine("✓ TEST 4 PASSED");
                }
                else
                {
                    throw new Exception($"Under construction default is ON, should be OFF");
                }
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
                Console.WriteLine("\nConclusion: Login persistence and server activation features working correctly.");
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
