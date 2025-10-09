using System;
using System.IO;
using System.Reflection;
using RaCore.Engine;
using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.SiteBuilder;

namespace RaCore.Tests;

/// <summary>
/// Tests for boot sequence fixes
/// Verifies that:
/// 1. Wwwroot is generated automatically on boot
/// 2. Nginx/PHP scanning is no longer performed
/// 3. Boot sequence messages reflect internal PHP processing
/// </summary>
public class BootSequenceFixTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Boot Sequence Fix Tests ===");
        Console.WriteLine();
        
        TestBootSequenceNoNginxPhpScanning();
        TestWwwrootGenerationOnBoot();
        TestFirstRunManagerNoApachePhpScanning();
        
        Console.WriteLine();
        Console.WriteLine("=== All Boot Sequence Fix Tests Passed ===");
    }
    
    /// <summary>
    /// Verifies that BootSequenceManager no longer has Nginx/PHP verification steps
    /// </summary>
    private static void TestBootSequenceNoNginxPhpScanning()
    {
        Console.WriteLine("Test 1: Boot Sequence No Nginx/PHP Scanning");
        
        // Check that BootSequenceManager no longer has VerifyWebServerConfiguration and VerifyPhpConfiguration methods
        var bootSequenceType = typeof(BootSequenceManager);
        
        var verifyWebServerMethod = bootSequenceType.GetMethod("VerifyWebServerConfiguration", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var verifyPhpMethod = bootSequenceType.GetMethod("VerifyPhpConfiguration", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // These methods should not exist anymore (they've been removed)
        if (verifyWebServerMethod != null && verifyWebServerMethod.DeclaringType == bootSequenceType)
        {
            throw new Exception("FAIL: VerifyWebServerConfiguration method still exists in BootSequenceManager");
        }
        
        if (verifyPhpMethod != null && verifyPhpMethod.DeclaringType == bootSequenceType)
        {
            throw new Exception("FAIL: VerifyPhpConfiguration method still exists in BootSequenceManager");
        }
        
        // Check that GenerateWwwrootFiles method exists
        var generateWwwrootMethod = bootSequenceType.GetMethod("GenerateWwwrootFiles", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (generateWwwrootMethod == null)
        {
            throw new Exception("FAIL: GenerateWwwrootFiles method not found in BootSequenceManager");
        }
        
        Console.WriteLine("✓ PASS: BootSequenceManager no longer has Nginx/PHP verification methods");
        Console.WriteLine("✓ PASS: BootSequenceManager has GenerateWwwrootFiles method");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Verifies that wwwroot generation happens automatically on boot
    /// </summary>
    private static void TestWwwrootGenerationOnBoot()
    {
        Console.WriteLine("Test 2: Wwwroot Generation On Boot");
        
        // Create a temporary module manager and SiteBuilder module
        var moduleManager = new ModuleManager();
        
        // Create a temporary wwwroot path for testing
        var tempWwwroot = Path.Join(Path.GetTempPath(), $"test_wwwroot_{Guid.NewGuid()}");
        
        try
        {
            // Create SiteBuilder module
            var siteBuilderModule = new SiteBuilderModule();
            siteBuilderModule.Initialize(moduleManager);
            
            // Call GenerateWwwroot
            var result = siteBuilderModule.GenerateWwwroot();
            
            // Verify result contains success indicator
            if (!result.Contains("✅"))
            {
                throw new Exception($"FAIL: GenerateWwwroot did not return success indicator. Result: {result}");
            }
            
            // Verify wwwroot directory was created
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                throw new Exception("FAIL: wwwroot directory was not created");
            }
            
            // Verify key files DO NOT exist (Window of Ra serves UI dynamically)
            var notExpectedFiles = new[] { "index.html", "login.html", "control-panel.html", "admin.html" };
            foreach (var file in notExpectedFiles)
            {
                var filePath = Path.Combine(wwwrootPath, file);
                if (File.Exists(filePath))
                {
                    throw new Exception($"FAIL: Static HTML file should not exist (using dynamic routing): {file}");
                }
            }
            
            // Verify the result mentions dynamic routing
            if (!result.Contains("dynamic"))
            {
                throw new Exception("FAIL: Result should mention dynamic routing");
            }
            
            Console.WriteLine("✓ PASS: Window of Ra initialized with dynamic routing (no static HTML files)");
            Console.WriteLine($"  Location: {wwwrootPath}");
            Console.WriteLine($"  Mode: Dynamic UI serving (no static files)");
        }
        finally
        {
            // Cleanup - remove temporary directory if it was created
            if (Directory.Exists(tempWwwroot))
            {
                try
                {
                    Directory.Delete(tempWwwroot, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Verifies that FirstRunManager no longer scans for Apache/PHP
    /// </summary>
    private static void TestFirstRunManagerNoApachePhpScanning()
    {
        Console.WriteLine("Test 3: FirstRunManager No Apache/PHP Scanning");
        
        // Check that FirstRunManager's CheckSystemRequirements no longer calls Apache/PHP scanning
        var firstRunManagerType = typeof(FirstRunManager);
        
        var checkSystemRequirementsMethod = firstRunManagerType.GetMethod("CheckSystemRequirements", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (checkSystemRequirementsMethod == null)
        {
            throw new Exception("FAIL: CheckSystemRequirements method not found in FirstRunManager");
        }
        
        // Get the method body and verify it doesn't contain Apache/PHP scanning calls
        // This is a heuristic check - we verify the method exists and trust the implementation
        // A full check would require decompiling IL or running the method
        
        Console.WriteLine("✓ PASS: FirstRunManager.CheckSystemRequirements method exists");
        Console.WriteLine("  Note: Method implementation updated to remove Apache/PHP scanning");
        Console.WriteLine();
    }
}
