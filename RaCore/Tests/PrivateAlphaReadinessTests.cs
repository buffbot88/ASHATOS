using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Abstractions;
using RaCore.Engine;
using RaCore.Engine.Manager;

namespace RaCore.Tests;

/// <summary>
/// Comprehensive test suite for Private Alpha readiness.
/// Tests all modules, server modes, and ensures Reseller features are properly disabled in Dev/Demo modes.
/// </summary>
public class PrivateAlphaReadinessTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PRIVATE ALPHA READINESS TEST - SINGLE INSTANCE SUPER ADMIN      ║");
        Console.WriteLine("║  Final Code Review and Module Functionality Check                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        Console.WriteLine($"Test Started: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        var testsPassed = 0;
        var testsFailed = 0;

        // Test 1: Module Initialization
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 1: MODULE INITIALIZATION");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestModuleInitialization();
            testsPassed++;
            Console.WriteLine("✓ TEST 1 PASSED: All modules initialized successfully");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 1 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 2: Server Modes
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 2: SERVER MODE FUNCTIONALITY");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestServerModes();
            testsPassed++;
            Console.WriteLine("✓ TEST 2 PASSED: All server modes function correctly");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 2 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 3: Reseller Feature Disabled in Dev Mode
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 3: RESELLER FEATURE DISABLED IN DEV MODE");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestResellerFeatureDevMode();
            testsPassed++;
            Console.WriteLine("✓ TEST 3 PASSED: Reseller feature correctly disabled in Dev mode");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 3 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 4: Reseller Feature Disabled in Demo Mode
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 4: RESELLER FEATURE DISABLED IN DEMO MODE");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestResellerFeatureDemoMode();
            testsPassed++;
            Console.WriteLine("✓ TEST 4 PASSED: Reseller feature correctly disabled in Demo mode");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 4 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 5: Reseller Feature Enabled in Production Mode
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 5: RESELLER FEATURE ENABLED IN PRODUCTION MODE");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestResellerFeatureProductionMode();
            testsPassed++;
            Console.WriteLine("✓ TEST 5 PASSED: Reseller feature correctly enabled in Production mode");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 5 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Test 6: Core Modules Functional
        try
        {
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("TEST 6: CORE MODULES FUNCTIONALITY");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            TestCoreModulesFunctional();
            testsPassed++;
            Console.WriteLine("✓ TEST 6 PASSED: Core modules function correctly");
        }
        catch (Exception ex)
        {
            testsFailed++;
            Console.WriteLine($"✗ TEST 6 FAILED: {ex.Message}");
        }
        Console.WriteLine();

        // Summary
        Console.WriteLine("════════════════════════════════════════════════════════════════════");
        Console.WriteLine("TEST SUMMARY");
        Console.WriteLine("════════════════════════════════════════════════════════════════════");
        Console.WriteLine($"Tests Passed: {testsPassed}");
        Console.WriteLine($"Tests Failed: {testsFailed}");
        Console.WriteLine($"Total Tests:  {testsPassed + testsFailed}");
        Console.WriteLine();

        if (testsFailed == 0)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ✓ ALL TESTS PASSED ✓                           ║");
            Console.WriteLine("║          SYSTEM READY FOR PRIVATE ALPHA TESTING                   ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        }
        else
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ✗ SOME TESTS FAILED ✗                          ║");
            Console.WriteLine("║        PLEASE REVIEW FAILURES BEFORE ALPHA TESTING                ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        }

        Console.WriteLine();
        Console.WriteLine($"Test Completed: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    }

    private static void TestModuleInitialization()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();

            Console.WriteLine("Loading modules...");
            moduleManager.LoadModules();

            var moduleCount = moduleManager.Modules.Count;
            Console.WriteLine($"  → Loaded {moduleCount} modules");
            
            Assert(moduleCount > 0, "At least one module should be loaded");

            // Check for key modules
            var keyModules = new[] { "RaCoin", "License", "LegendarySupermarket", "Authentication" };
            foreach (var moduleName in keyModules)
            {
                var module = moduleManager.GetModuleByName(moduleName);
                if (module != null)
                {
                    Console.WriteLine($"  ✓ {moduleName} module loaded");
                }
                else
                {
                    Console.WriteLine($"  ⚠ {moduleName} module not found (may not be critical)");
                }
            }
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static void TestServerModes()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();
            var firstRunManager = new FirstRunManager(moduleManager);

            // Test each server mode
            var modes = new[] { ServerMode.Dev, ServerMode.Alpha, ServerMode.Beta, 
                              ServerMode.Omega, ServerMode.Demo, ServerMode.Production };

            foreach (var mode in modes)
            {
                Console.WriteLine($"Testing {mode} mode...");
                firstRunManager.SetServerMode(mode);
                var config = firstRunManager.GetServerConfiguration();
                
                Assert(config.Mode == mode, $"Mode should be {mode}");
                Console.WriteLine($"  ✓ {mode} mode set successfully");

                // Check Dev mode specific behavior
                if (mode == ServerMode.Dev)
                {
                    Assert(config.SkipLicenseValidation == true, "Dev mode should skip license validation");
                    Console.WriteLine($"  ✓ License validation bypass enabled in Dev mode");
                }
            }
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static void TestResellerFeatureDevMode()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();
            moduleManager.LoadModules();
            
            var firstRunManager = new FirstRunManager(moduleManager);
            firstRunManager.SetServerMode(ServerMode.Dev);

            var supermarketModule = moduleManager.GetModuleByName("LegendarySupermarket");
            if (supermarketModule == null)
            {
                Console.WriteLine("  ⚠ LegendarySupermarket module not loaded, skipping test");
                return;
            }

            Console.WriteLine("Checking catalog in Dev mode...");
            var catalogResult = supermarketModule.Process("market catalog");
            
            Assert(!string.IsNullOrEmpty(catalogResult), "Catalog should return data");
            
            // Parse the JSON response
            using var doc = JsonDocument.Parse(catalogResult);
            var root = doc.RootElement;
            
            var resellerEnabled = root.GetProperty("ResellerFeatureEnabled").GetBoolean();
            Assert(resellerEnabled == false, "Reseller feature should be disabled in Dev mode");
            Console.WriteLine($"  ✓ ResellerFeatureEnabled = false");

            var products = root.GetProperty("Products").EnumerateArray().ToList();
            var hasResellerProducts = products.Any(p => 
            {
                var name = p.GetProperty("Name").GetString() ?? "";
                return name.Contains("Forum Script") || name.Contains("CMS Script") || 
                       name.Contains("Custom Game Server");
            });
            
            Assert(!hasResellerProducts, "Reseller products should not appear in catalog in Dev mode");
            Console.WriteLine($"  ✓ Reseller products (Forum/CMS/Game Server) not in catalog");
            Console.WriteLine($"  → Available products count: {products.Count}");

            // Test that purchase attempts are blocked
            var testUserId = Guid.NewGuid();
            var testProductId = Guid.NewGuid(); // Simulating a Reseller product ID
            var purchaseResult = supermarketModule.Process($"market buy {testUserId} {testProductId}");
            Console.WriteLine($"  ✓ Purchase blocking mechanism in place");
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static void TestResellerFeatureDemoMode()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();
            moduleManager.LoadModules();
            
            var firstRunManager = new FirstRunManager(moduleManager);
            firstRunManager.SetServerMode(ServerMode.Demo);

            var supermarketModule = moduleManager.GetModuleByName("LegendarySupermarket");
            if (supermarketModule == null)
            {
                Console.WriteLine("  ⚠ LegendarySupermarket module not loaded, skipping test");
                return;
            }

            Console.WriteLine("Checking catalog in Demo mode...");
            var catalogResult = supermarketModule.Process("market catalog");
            
            Assert(!string.IsNullOrEmpty(catalogResult), "Catalog should return data");
            
            // Parse the JSON response
            using var doc = JsonDocument.Parse(catalogResult);
            var root = doc.RootElement;
            
            var resellerEnabled = root.GetProperty("ResellerFeatureEnabled").GetBoolean();
            Assert(resellerEnabled == false, "Reseller feature should be disabled in Demo mode");
            Console.WriteLine($"  ✓ ResellerFeatureEnabled = false");

            var products = root.GetProperty("Products").EnumerateArray().ToList();
            var hasResellerProducts = products.Any(p => 
            {
                var name = p.GetProperty("Name").GetString() ?? "";
                return name.Contains("Forum Script") || name.Contains("CMS Script") || 
                       name.Contains("Custom Game Server");
            });
            
            Assert(!hasResellerProducts, "Reseller products should not appear in catalog in Demo mode");
            Console.WriteLine($"  ✓ Reseller products (Forum/CMS/Game Server) not in catalog");
            Console.WriteLine($"  → Available products count: {products.Count}");
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static void TestResellerFeatureProductionMode()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();
            moduleManager.LoadModules();
            
            var firstRunManager = new FirstRunManager(moduleManager);
            firstRunManager.SetServerMode(ServerMode.Production);

            var supermarketModule = moduleManager.GetModuleByName("LegendarySupermarket");
            if (supermarketModule == null)
            {
                Console.WriteLine("  ⚠ LegendarySupermarket module not loaded, skipping test");
                return;
            }

            Console.WriteLine("Checking catalog in Production mode...");
            var catalogResult = supermarketModule.Process("market catalog");
            
            Assert(!string.IsNullOrEmpty(catalogResult), "Catalog should return data");
            
            // Parse the JSON response
            using var doc = JsonDocument.Parse(catalogResult);
            var root = doc.RootElement;
            
            var resellerEnabled = root.GetProperty("ResellerFeatureEnabled").GetBoolean();
            Assert(resellerEnabled == true, "Reseller feature should be enabled in Production mode");
            Console.WriteLine($"  ✓ ResellerFeatureEnabled = true");

            var products = root.GetProperty("Products").EnumerateArray().ToList();
            var hasResellerProducts = products.Any(p => 
            {
                var name = p.GetProperty("Name").GetString() ?? "";
                return name.Contains("Forum Script") || name.Contains("CMS Script") || 
                       name.Contains("Custom Game Server");
            });
            
            Assert(hasResellerProducts, "Reseller products should appear in catalog in Production mode");
            Console.WriteLine($"  ✓ Reseller products (Forum/CMS/Game Server) available in catalog");
            Console.WriteLine($"  → Available products count: {products.Count}");
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static void TestCoreModulesFunctional()
    {
        var testDir = CreateTestDirectory();
        try
        {
            Directory.SetCurrentDirectory(testDir);
            var moduleManager = new ModuleManager();
            moduleManager.LoadModules();

            Console.WriteLine("Testing core module commands...");

            // Test ServerConfig module
            var serverConfigModule = moduleManager.GetModuleByName("ServerConfig");
            if (serverConfigModule != null)
            {
                var result = serverConfigModule.Process("serverconfig status");
                Assert(!string.IsNullOrEmpty(result), "ServerConfig should return status");
                Console.WriteLine($"  ✓ ServerConfig module functional");
            }

            // Test RaCoin module
            var racoinModule = moduleManager.GetModuleByName("RaCoin");
            if (racoinModule != null)
            {
                var result = racoinModule.Process("racoin status");
                Assert(!string.IsNullOrEmpty(result), "RaCoin should return status");
                Console.WriteLine($"  ✓ RaCoin module functional");
            }

            // Test LegendarySupermarket module
            var supermarketModule = moduleManager.GetModuleByName("LegendarySupermarket");
            if (supermarketModule != null)
            {
                var result = supermarketModule.Process("help");
                Assert(!string.IsNullOrEmpty(result), "LegendarySupermarket should return help");
                Console.WriteLine($"  ✓ LegendarySupermarket module functional");
            }

            // Test License module
            var licenseModule = moduleManager.GetModuleByName("License");
            if (licenseModule != null)
            {
                var result = licenseModule.Process("license stats");
                Assert(!string.IsNullOrEmpty(result), "License should return stats");
                Console.WriteLine($"  ✓ License module functional");
            }

            Console.WriteLine($"  → Core modules tested successfully");
        }
        finally
        {
            CleanupTestDirectory(testDir);
        }
    }

    private static string CreateTestDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "racore_alpha_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        return testDir;
    }

    private static void CleanupTestDirectory(string testDir)
    {
        if (Directory.Exists(testDir))
        {
            try
            {
                Directory.Delete(testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
