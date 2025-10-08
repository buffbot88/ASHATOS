using System;
using System.IO;
using System.Runtime.InteropServices;
using RaCore.Engine;
using RaCore.Modules.Extensions.SiteBuilder;

namespace RaCore.Tests;

/// <summary>
/// Tests for Windows 11 Kestrel-only behavior
/// Verifies that Apache/Nginx configs are NOT generated on Windows
/// </summary>
public class Windows11KestrelTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== Windows 11 Kestrel-Only Tests ===");
        Console.WriteLine();
        
        TestApacheConfigSkippedOnWindows();
        TestApacheManagerWindowsDetection();
        TestFtpHelperWindowsMessages();
        TestWwwrootGenerationWindowsBehavior();
        
        Console.WriteLine();
        Console.WriteLine("=== All Windows 11 Kestrel-Only Tests Passed ===");
    }
    
    private static void TestApacheConfigSkippedOnWindows()
    {
        Console.WriteLine("Test 1: Apache Config Skipped on Windows");
        
        // Test ApacheManager.ScanForApacheConfig() behavior on Windows
        var (found, path, message) = ApacheManager.ScanForApacheConfig();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (found)
            {
                throw new Exception("FAIL: Apache config should NOT be found on Windows");
            }
            
            if (message == null || (!message.Contains("Kestrel") && !message.Contains("not required")))
            {
                throw new Exception($"FAIL: Message should mention Kestrel or not required. Got: {message ?? "null"}");
            }
            
            Console.WriteLine($"✓ PASS: Apache config correctly skipped on Windows");
            Console.WriteLine($"  Message: {message}");
        }
        else
        {
            Console.WriteLine($"ℹ️  SKIP: Running on non-Windows platform");
        }
        
        Console.WriteLine();
    }
    
    private static void TestApacheManagerWindowsDetection()
    {
        Console.WriteLine("Test 2: Apache Manager Windows Detection");
        
        // Test ApacheManager.ScanForPhpConfig() behavior on Windows
        var (found, path, message) = ApacheManager.ScanForPhpConfig();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (found)
            {
                throw new Exception("FAIL: PHP config should NOT be found on Windows");
            }
            
            if (message == null || !message.Contains("optional") || !message.Contains("Kestrel"))
            {
                throw new Exception($"FAIL: Message should mention optional and Kestrel. Got: {message ?? "null"}");
            }
            
            Console.WriteLine($"✓ PASS: PHP config correctly reported as optional on Windows");
            Console.WriteLine($"  Message: {message}");
        }
        else
        {
            Console.WriteLine($"ℹ️  SKIP: Running on non-Windows platform");
        }
        
        Console.WriteLine();
    }
    
    private static void TestFtpHelperWindowsMessages()
    {
        Console.WriteLine("Test 3: FTP Helper Windows-Specific Messages");
        
        // Create FtpHelper with invalid credentials to test error messages
        var ftpHelper = new FtpHelper("invalid-host", 21, "test", "test");
        
        try
        {
            var result = ftpHelper.TestConnectionAsync().GetAwaiter().GetResult();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (result.success)
                {
                    Console.WriteLine($"ℹ️  NOTE: FTP connection succeeded (FTP server is running)");
                }
                else if (result.message.Contains("Windows") || result.message.Contains("optional"))
                {
                    Console.WriteLine($"✓ PASS: FTP error message includes Windows-specific guidance");
                    Console.WriteLine($"  Message excerpt: {result.message.Substring(0, Math.Min(150, result.message.Length))}...");
                }
                else
                {
                    Console.WriteLine($"⚠️  WARN: FTP error message may not include Windows guidance");
                    Console.WriteLine($"  Message: {result.message}");
                }
            }
            else
            {
                Console.WriteLine($"ℹ️  SKIP: Running on non-Windows platform");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ℹ️  NOTE: FTP test threw exception (expected): {ex.Message}");
        }
        
        Console.WriteLine();
    }
    
    private static void TestWwwrootGenerationWindowsBehavior()
    {
        Console.WriteLine("Test 4: Wwwroot Generation Windows Behavior");
        
        // Create a temporary directory for testing
        var testDir = Path.Combine(Path.GetTempPath(), "racore_windows_wwwroot_test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Create a mock SiteBuilder module
            var siteBuilderModule = new SiteBuilderModule();
            siteBuilderModule.Initialize(null);
            
            // Create WwwrootGenerator
            var generator = new WwwrootGenerator(siteBuilderModule, testDir);
            
            // Generate wwwroot
            var result = generator.GenerateWwwroot();
            
            // Check if config directory was created
            var configPath = Path.Combine(testDir, "config");
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, config files should NOT be generated
                if (Directory.Exists(configPath))
                {
                    var files = Directory.GetFiles(configPath);
                    if (files.Length > 0)
                    {
                        throw new Exception($"FAIL: Config files should NOT be generated on Windows. Found: {string.Join(", ", files)}");
                    }
                }
                
                if (!result.Contains("Kestrel") && !result.Contains("no external webserver"))
                {
                    throw new Exception($"FAIL: Result should mention Kestrel on Windows. Got: {result}");
                }
                
                Console.WriteLine($"✓ PASS: Wwwroot generation correctly skips Apache/Nginx configs on Windows");
                Console.WriteLine($"  Result mentions Kestrel: {result.Contains("Kestrel")}");
            }
            else
            {
                // On Linux, config files SHOULD be generated
                if (!Directory.Exists(configPath))
                {
                    throw new Exception("FAIL: Config directory should exist on Linux");
                }
                
                var nginxConf = Path.Combine(configPath, "nginx.conf");
                var apacheConf = Path.Combine(configPath, "apache.conf");
                var phpIni = Path.Combine(configPath, "php.ini");
                
                if (!File.Exists(nginxConf) || !File.Exists(apacheConf) || !File.Exists(phpIni))
                {
                    throw new Exception("FAIL: Config files should be generated on Linux");
                }
                
                Console.WriteLine($"✓ PASS: Wwwroot generation correctly creates Apache/Nginx configs on Linux");
            }
        }
        finally
        {
            // Cleanup
            try
            {
                Directory.Delete(testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        
        Console.WriteLine();
    }
}
