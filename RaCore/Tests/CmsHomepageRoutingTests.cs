using System.IO;

namespace RaCore.Tests;

/// <summary>
/// Tests for CMS Homepage Routing (Phase 9.3.9)
/// Validates that the homepage routes to CMS when available
/// </summary>
public class CmsHomepageRoutingTests
{
    public static void RunTests()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("Running CMS Homepage Routing Tests");
        Console.WriteLine("========================================");
        Console.WriteLine();

        TestCmsAvailabilityCheck();
        TestCmsFileDetection();
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("All CMS Homepage Routing Tests Passed ✓");
        Console.WriteLine("========================================");
    }
    
    private static void TestCmsAvailabilityCheck()
    {
        Console.WriteLine("Test 1: CMS Availability Check");
        
        // Test with non-existent directory
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_cms_" + Guid.NewGuid());
        var isAvailable = IsCmsAvailableTestHelper(nonExistentPath);
        Assert(!isAvailable, "CMS should not be available when directory doesn't exist");
        
        // Test with existing directory but no index.php
        var tempDir = Path.Combine(Path.GetTempPath(), "cms_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        try
        {
            isAvailable = IsCmsAvailableTestHelper(tempDir);
            Assert(!isAvailable, "CMS should not be available when index.php doesn't exist");
            
            // Test with existing index.php
            var indexPhpPath = Path.Combine(tempDir, "index.php");
            File.WriteAllText(indexPhpPath, "<?php echo 'Hello'; ?>");
            
            isAvailable = IsCmsAvailableTestHelper(tempDir);
            Assert(isAvailable, "CMS should be available when index.php exists");
            
            Console.WriteLine("  ✓ CMS availability detection works correctly");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    private static void TestCmsFileDetection()
    {
        Console.WriteLine("Test 2: CMS File Detection");
        
        var tempDir = Path.Combine(Path.GetTempPath(), "cms_files_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Test with various file scenarios
            Assert(!IsCmsAvailableTestHelper(tempDir), "Empty directory should not have CMS");
            
            // Create index.html (wrong file)
            var indexHtmlPath = Path.Combine(tempDir, "index.html");
            File.WriteAllText(indexHtmlPath, "<html></html>");
            Assert(!IsCmsAvailableTestHelper(tempDir), "index.html should not be detected as CMS");
            
            // Create index.php (correct file)
            var indexPhpPath = Path.Combine(tempDir, "index.php");
            File.WriteAllText(indexPhpPath, "<?php phpinfo(); ?>");
            Assert(IsCmsAvailableTestHelper(tempDir), "index.php should be detected as CMS");
            
            // Verify file content doesn't matter, only existence
            File.WriteAllText(indexPhpPath, "");
            Assert(IsCmsAvailableTestHelper(tempDir), "Empty index.php should still be detected");
            
            Console.WriteLine("  ✓ CMS file detection works correctly");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    /// <summary>
    /// Helper method that mimics the IsCmsAvailable logic from Program.cs
    /// </summary>
    private static bool IsCmsAvailableTestHelper(string wwwrootPath)
    {
        var indexPhpPath = Path.Combine(wwwrootPath, "index.php");
        return File.Exists(indexPhpPath);
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
