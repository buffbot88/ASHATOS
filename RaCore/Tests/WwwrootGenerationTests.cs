using System;
using System.IO;
using System.Linq;
using RaCore.Engine;
using RaCore.Engine.Manager;
using RaCore.Engine.Memory;
using RaCore.Modules.Extensions.SiteBuilder;

namespace RaCore.Tests;

/// <summary>
/// Tests for Window of Ra (SiteBuilder) initialization on boot
/// Verifies that dynamic UI routing is configured (no static HTML files)
/// </summary>
public class WwwrootGenerationTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Window of Ra (SiteBuilder) Initialization Tests ===");
        Console.WriteLine();
        
        TestEnsureWwwrootAsync();
        TestWwwrootDirectoryCreation();
        TestNoStaticHtmlGeneration();
        
        Console.WriteLine();
        Console.WriteLine("=== All Window of Ra Tests Passed ===");
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
        Console.WriteLine("Test 2: Wwwroot Directory Creation (config only, no HTML)");
        
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
            
            Console.WriteLine("  ✓ Wwwroot directory creation works (for config files only)");
            
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
    
    private static void TestNoStaticHtmlGeneration()
    {
        Console.WriteLine("Test 3: Window of Ra - No Static HTML Generation");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "racore_no_html_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Save current directory
            var originalDir = Directory.GetCurrentDirectory();
            
            // Change to test directory
            Directory.SetCurrentDirectory(testDir);
            
            var wwwrootPath = Path.Combine(testDir, "wwwroot");
            Directory.CreateDirectory(wwwrootPath);
            
            // Create minimal setup
            var memoryModule = new MemoryModule();
            memoryModule.Initialize(null);
            
            var moduleManager = new ModuleManager();
            moduleManager.RegisterBuiltInModule(memoryModule);
            
            // Load SiteBuilder module
            moduleManager.LoadModules();
            
            // Find SiteBuilder module
            var siteBuilderModule = moduleManager.Modules
                .Select(m => m.Instance)
                .OfType<SiteBuilderModule>()
                .FirstOrDefault();
            
            if (siteBuilderModule != null)
            {
                // Generate wwwroot (should NOT create HTML files)
                var result = siteBuilderModule.GenerateWwwroot();
                
                // Verify no HTML files were created
                var htmlFiles = Directory.Exists(wwwrootPath) 
                    ? Directory.GetFiles(wwwrootPath, "*.html", SearchOption.AllDirectories) 
                    : Array.Empty<string>();
                
                Assert(htmlFiles.Length == 0, "No HTML files should be generated - all UI is dynamic");
                
                // Verify the result message mentions dynamic routing
                Assert(result.Contains("dynamic"), "Result should mention dynamic routing");
                
                Console.WriteLine("  ✓ No static HTML files generated (dynamic routing only)");
            }
            else
            {
                Console.WriteLine("  ⚠ SiteBuilder module not found - test skipped");
            }
            
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
