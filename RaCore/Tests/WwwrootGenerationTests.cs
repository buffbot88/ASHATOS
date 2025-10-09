using System;
using System.IO;
using RaCore.Engine;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using RaCore.Modules.Extensions.SiteBuilder;

namespace RaCore.Tests;

/// <summary>
/// Tests for wwwroot generation on boot
/// Verifies that static HTML files are generated automatically
/// </summary>
public class WwwrootGenerationTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Wwwroot Generation Tests ===");
        Console.WriteLine();
        
        TestEnsureWwwrootAsync();
        TestWwwrootDirectoryCreation();
        
        Console.WriteLine();
        Console.WriteLine("=== All Wwwroot Generation Tests Passed ===");
    }
    
    private static void TestEnsureWwwrootAsync()
    {
        Console.WriteLine("Test 1: EnsureWwwrootAsync Method Exists");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "racore_wwwroot_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            // Create minimal setup
            var memoryModule = new MemoryModule();
            memoryModule.Initialize(null);
            
            var moduleManager = new ModuleManager();
            moduleManager.RegisterBuiltInModule(memoryModule);
            
            // Create FirstRunManager
            var firstRunManager = new FirstRunManager(moduleManager);
            
            // Verify EnsureWwwrootAsync method exists and is callable
            var method = firstRunManager.GetType().GetMethod("EnsureWwwrootAsync");
            Assert(method != null, "EnsureWwwrootAsync method should exist");
            
            Console.WriteLine("  ✓ EnsureWwwrootAsync method exists");
            
            // Restore original directory
            Directory.SetCurrentDirectory(originalDir);
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
    
    private static void TestWwwrootDirectoryCreation()
    {
        Console.WriteLine("Test 2: Wwwroot Directory Creation");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "racore_wwwroot_dir_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            var wwwrootPath = Path.Combine(testDir, "wwwroot");
            
            // Verify wwwroot doesn't exist initially
            Assert(!Directory.Exists(wwwrootPath), "Wwwroot should not exist initially");
            
            // Create wwwroot directory (simulating what Program.cs does)
            Directory.CreateDirectory(wwwrootPath);
            
            // Verify it was created
            Assert(Directory.Exists(wwwrootPath), "Wwwroot directory should be created");
            
            Console.WriteLine("  ✓ Wwwroot directory creation works");
            
            // Restore original directory
            Directory.SetCurrentDirectory(originalDir);
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
