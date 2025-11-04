using System;
using System.Threading.Tasks;
using Abstractions;
using ASHATCore.Modules.Extensions.Authentication;

namespace ASHATCore.Tests;

/// <summary>
/// Integration test for 3-hour inactivity timeout feature.
/// This test verifies that sessions properly track activity and extend expiration.
/// </summary>
public static class SessionInactivityTimeoutTest
{
    public static async Task<bool> RunTest()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     SESSION INACTIVITY TIMEOUT TEST                               ║");
        Console.WriteLine("║     Testing 3-hour sliding window expiration                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var testsPassed = 0;
        var testsFailed = 0;

        // Test 1: Verify session created with 3-hour expiry
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 1: SESSION CREATED WITH 3-HOUR EXPIRY");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");

            var authModule = new AuthenticationModule();
            authModule.Initialize(null);

            // Register a test user
            var registerRequest = new RegisterRequest
            {
                Username = $"testuser_{Guid.NewGuid()}",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var registerResponse = await authModule.RegisterAsync(registerRequest, "127.0.0.1");
            if (!registerResponse.Success)
            {
                throw new Exception($"Registration failed: {registerResponse.Message}");
            }

            // Login to create a session
            var loginRequest = new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password
            };

            var loginResponse = await authModule.LoginAsync(loginRequest, "127.0.0.1", "TestAgent");
            if (!loginResponse.Success || loginResponse.Token == null)
            {
                throw new Exception($"Login failed: {loginResponse.Message}");
            }

            var token = loginResponse.Token;
            var expiresAt = loginResponse.TokenExpiresAt;

            if (expiresAt.HasValue)
            {
                var hoursUntilExpiry = (expiresAt.Value - DateTime.UtcNow).TotalHours;
                Console.WriteLine($"✓ Session created successfully");
                Console.WriteLine($"  Token: {token.Substring(0, 16)}...");
                Console.WriteLine($"  Expires in: {hoursUntilExpiry:F2} hours");
                
                if (hoursUntilExpiry >= 2.9 && hoursUntilExpiry <= 3.1)
                {
                    Console.WriteLine($"✓ Expiry time is correct (~3 hours)");
                    testsPassed++;
                    Console.WriteLine("✓ TEST 1 PASSED");
                }
                else
                {
                    throw new Exception($"Expiry time incorrect: {hoursUntilExpiry:F2} hours (expected ~3)");
                }
            }
            else
            {
                throw new Exception("TokenExpiresAt not set");
            }

            authModule.Dispose();
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 1 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 2: Verify session extends on validation
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 2: SESSION EXTENDS ON ACTIVITY");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");

            var authModule = new AuthenticationModule();
            authModule.Initialize(null);

            // Register and login
            var registerRequest = new RegisterRequest
            {
                Username = $"testuser_{Guid.NewGuid()}",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            await authModule.RegisterAsync(registerRequest, "127.0.0.1");
            var loginResponse = await authModule.LoginAsync(new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password
            }, "127.0.0.1", "TestAgent");

            var token = loginResponse.Token!;
            var initialExpiry = loginResponse.TokenExpiresAt!.Value;

            Console.WriteLine($"✓ Initial session created");
            Console.WriteLine($"  Initial expiry: {initialExpiry}");

            // Wait 1 second, then validate to simulate activity
            await Task.Delay(1000);

            var session = await authModule.ValidateTokenAsync(token);
            if (session == null)
            {
                throw new Exception("Session validation failed");
            }

            var newExpiry = session.ExpiresAtUtc;
            Console.WriteLine($"  New expiry after activity: {newExpiry}");

            // New expiry should be later than initial expiry
            if (newExpiry > initialExpiry)
            {
                var extension = (newExpiry - initialExpiry).TotalSeconds;
                Console.WriteLine($"✓ Session extended by {extension:F1} seconds");
                Console.WriteLine($"✓ LastActivityUtc updated: {session.LastActivityUtc}");
                testsPassed++;
                Console.WriteLine("✓ TEST 2 PASSED");
            }
            else
            {
                throw new Exception($"Session not extended: {newExpiry} <= {initialExpiry}");
            }

            authModule.Dispose();
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 2 FAILED: {ex.Message}");
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
            Console.WriteLine("\nConclusion: 3-hour inactivity timeout is working correctly.");
            Console.WriteLine("Sessions stay alive as long as there is activity.");
            return true;
        }
        else
        {
            Console.WriteLine("✗ SOME TESTS FAILED");
            Console.WriteLine($"\nConclusion: {testsFailed} test(s) failed.");
            return false;
        }
    }
}
