using System;
using System.IO;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for Server Modes and First-Time Initialization
/// </summary>
public class ServerModesTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Server Modes Tests ===");
        Console.WriteLine();
        
        TestServerModeEnum();
        TestServerConfiguration();
        TestFirstRunManager();
        TestDevModeFeatures();
        
        Console.WriteLine();
        Console.WriteLine("=== All Server Modes Tests Passed ===");
    }
    
    private static void TestDevModeFeatures()
    {
        Console.WriteLine("Test 4: Dev Mode Features");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "ASHATCore_devmode_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            // Create module manager
            var moduleManager = new ModuleManager();
            
            // Create FirstRunManager
            var firstRunManager = new FirstRunManager(moduleManager);
            
            // Test Dev mode sets SkipLicenseValidation
            firstRunManager.SetServerMode(ServerMode.Dev);
            var config = firstRunManager.GetServerConfiguration();
            Assert(config.Mode == ServerMode.Dev, "Mode should be Dev");
            Assert(config.SkipLicenseValidation == true, "SkipLicenseValidation should be true in Dev mode");
            
            // Test non-Dev mode doesn't set SkipLicenseValidation
            firstRunManager.SetServerMode(ServerMode.Production);
            config = firstRunManager.GetServerConfiguration();
            Assert(config.Mode == ServerMode.Production, "Mode should be Production");
            Assert(config.SkipLicenseValidation == false, "SkipLicenseValidation should be false in Production mode");
            
            // Restore original directory
            Directory.SetCurrentDirectory(originalDir);
            
            Console.WriteLine("  ✓ Dev mode features work correctly");
        }
        finally
        {
            // Clean up test directory
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
    }
    
    private static void TestServerModeEnum()
    {
        Console.WriteLine("Test 1: ServerMode Enum");
        
        // Test enum values
        var modes = Enum.GetValues<ServerMode>();
        Assert(modes.Length == 6, "Should have 6 server modes");
        
        Assert(ServerMode.Dev.ToString() == "Dev", "Dev mode should be named Dev");
        Assert(ServerMode.Alpha.ToString() == "Alpha", "Alpha mode should be named Alpha");
        Assert(ServerMode.Beta.ToString() == "Beta", "Beta mode should be named Beta");
        Assert(ServerMode.Omega.ToString() == "Omega", "Omega mode should be named Omega");
        Assert(ServerMode.Demo.ToString() == "Demo", "Demo mode should be named Demo");
        Assert(ServerMode.Production.ToString() == "Production", "Production mode should be named Production");
        
        Console.WriteLine("  ✓ ServerMode enum has all expected values");
    }
    
    private static void TestServerConfiguration()
    {
        Console.WriteLine("Test 2: ServerConfiguration");
        
        var config = new ServerConfiguration();
        
        // Test defaults
        Assert(config.Mode == ServerMode.Production, "Default mode should be Production");
        Assert(config.IsFirstRun == true, "IsFirstRun should default to true");
        Assert(config.InitializationCompleted == false, "InitializationCompleted should default to false");
        Assert(config.Version == "1.0", "Version should default to 1.0");
        Assert(config.MainServerUrl == "https://us-omega.ASHATOS.io", "MainServerUrl should have default value");
        Assert(config.Port == 7077, "Port should default to 7077");
        Assert(config.SkipLicenseValidation == false, "SkipLicenseValidation should default to false");
        
        // Test property modifications
        config.Mode = ServerMode.Dev;
        Assert(config.Mode == ServerMode.Dev, "Mode should be changeable to Dev");
        
        config.SkipLicenseValidation = true;
        Assert(config.SkipLicenseValidation == true, "SkipLicenseValidation should be settable");
        
        config.Mode = ServerMode.Alpha;
        Assert(config.Mode == ServerMode.Alpha, "Mode should be changeable");
        
        config.LicenseKey = "TEST-KEY-123";
        Assert(config.LicenseKey == "TEST-KEY-123", "LicenseKey should be settable");
        
        config.InitializationCompleted = true;
        Assert(config.InitializationCompleted == true, "InitializationCompleted should be changeable");
        
        config.Port = 8080;
        Assert(config.Port == 8080, "Port should be settable");
        
        Console.WriteLine("  ✓ ServerConfiguration works correctly");
    }
    
    private static void TestFirstRunManager()
    {
        Console.WriteLine("Test 3: FirstRunManager");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "ASHATCore_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            // Create module manager
            var moduleManager = new ModuleManager();
            
            // Create FirstRunManager
            var firstRunManager = new FirstRunManager(moduleManager);
            
            // Test IsFirstRun
            var isFirstRun = firstRunManager.IsFirstRun();
            Assert(isFirstRun == true, "Should be first run initially");
            
            // Test GetServerConfiguration
            var config = firstRunManager.GetServerConfiguration();
            Assert(config != null, "ServerConfiguration should not be null");
            Assert(config!.Mode == ServerMode.Production, "Default mode should be Production");
            
            // Test SetServerMode
            firstRunManager.SetServerMode(ServerMode.Beta);
            config = firstRunManager.GetServerConfiguration();
            Assert(config!.Mode == ServerMode.Beta, "Mode should be changed to Beta");
            
            // Verify config file was created
            var configPath = Path.Combine(testDir, "server-config.json");
            Assert(File.Exists(configPath), "Config file should exist");
            
            // Verify config file content
            var json = File.ReadAllText(configPath);
            Assert(json.Contains("\"Mode\": 1"), "Config file should contain Beta mode (1)");
            
            // Test MarkAsInitialized
            firstRunManager.MarkAsInitialized();
            
            var markerPath = Path.Combine(testDir, ".ASHATCore_initialized");
            Assert(File.Exists(markerPath), "Initialization marker should exist");
            
            config = firstRunManager.GetServerConfiguration();
            Assert(config.IsFirstRun == false, "IsFirstRun should be false after initialization");
            Assert(config.InitializationCompleted == true, "InitializationCompleted should be true");
            
            // Test that IsFirstRun returns false after initialization
            isFirstRun = firstRunManager.IsFirstRun();
            Assert(isFirstRun == false, "IsFirstRun should return false after initialization");
            
            // Restore original directory
            Directory.SetCurrentDirectory(originalDir);
            
            Console.WriteLine("  ✓ FirstRunManager works correctly");
        }
        finally
        {
            // Clean up test directory
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
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
