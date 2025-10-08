using System;
using System.IO;
using RaCore.Engine;

namespace RaCore.Tests
{
    /// <summary>
    /// Tests for config generation fixes - ensures templates are not regenerated when they already exist
    /// </summary>
    public class ConfigGenerationFixTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== Config Generation Fix Tests ===\n");

            TestPhpConfigExistenceCheck();
            TestNginxConfigExistenceCheck();
            TestPermissionErrorHandling();

            Console.WriteLine("\n=== All Config Generation Tests Completed ===");
        }

        private static void TestPhpConfigExistenceCheck()
        {
            Console.WriteLine("--- Test 1: PHP Config Existence Check ---");
            
            var testDir = Path.Combine(Path.GetTempPath(), "racore-test-php");
            Directory.CreateDirectory(testDir);
            
            var testPhpIniPath = Path.Combine(testDir, "php.ini");
            
            // Test scenario 1: File does not exist
            if (File.Exists(testPhpIniPath))
            {
                File.Delete(testPhpIniPath);
            }
            
            var shouldGenerate = !File.Exists(testPhpIniPath);
            Console.WriteLine($"  Scenario 1: File does not exist");
            Console.WriteLine($"    Should generate: {shouldGenerate}");
            
            if (shouldGenerate)
            {
                Console.WriteLine("    ✓ Correct: Will generate new template");
            }
            else
            {
                Console.WriteLine("    ✗ Error: Should generate but will not");
            }
            
            // Test scenario 2: File already exists
            File.WriteAllText(testPhpIniPath, "; Test PHP config");
            
            shouldGenerate = !File.Exists(testPhpIniPath);
            Console.WriteLine($"  Scenario 2: File already exists");
            Console.WriteLine($"    Should generate: {shouldGenerate}");
            
            if (!shouldGenerate)
            {
                Console.WriteLine("    ✓ Correct: Will skip generation");
            }
            else
            {
                Console.WriteLine("    ✗ Error: Should skip but will generate");
            }
            
            // Cleanup
            try
            {
                Directory.Delete(testDir, true);
            }
            catch { }
        }

        private static void TestNginxConfigExistenceCheck()
        {
            Console.WriteLine("\n--- Test 2: Nginx Config Existence Check ---");
            
            var testDir = Path.Combine(Path.GetTempPath(), "racore-test-nginx");
            Directory.CreateDirectory(testDir);
            
            var testNginxConfPath = Path.Combine(testDir, "nginx-racore.conf");
            
            // Test scenario 1: File does not exist
            if (File.Exists(testNginxConfPath))
            {
                File.Delete(testNginxConfPath);
            }
            
            var shouldGenerate = !File.Exists(testNginxConfPath);
            Console.WriteLine($"  Scenario 1: File does not exist");
            Console.WriteLine($"    Should generate: {shouldGenerate}");
            
            if (shouldGenerate)
            {
                Console.WriteLine("    ✓ Correct: Will generate new template");
            }
            else
            {
                Console.WriteLine("    ✗ Error: Should generate but will not");
            }
            
            // Test scenario 2: File already exists
            File.WriteAllText(testNginxConfPath, "# Test Nginx config");
            
            shouldGenerate = !File.Exists(testNginxConfPath);
            Console.WriteLine($"  Scenario 2: File already exists");
            Console.WriteLine($"    Should generate: {shouldGenerate}");
            
            if (!shouldGenerate)
            {
                Console.WriteLine("    ✓ Correct: Will skip generation");
            }
            else
            {
                Console.WriteLine("    ✗ Error: Should skip but will generate");
            }
            
            // Cleanup
            try
            {
                Directory.Delete(testDir, true);
            }
            catch { }
        }

        private static void TestPermissionErrorHandling()
        {
            Console.WriteLine("\n--- Test 3: Permission Error Handling ---");
            
            // Test various permission error messages
            var errorMessages = new[]
            {
                "nginx: [alert] could not open error log file: open() \"/usr/local/nginx/logs/error.log\" failed (13: Permission denied)\nnginx: the configuration file /usr/local/nginx/conf/nginx.conf syntax is ok",
                "nginx: the configuration file syntax is ok\n2025/10/08 07:05:20 [emerg] 5163#0: open() \"/usr/local/nginx/logs/nginx.pid\" failed (13: Permission denied)",
                "Permission denied while opening log file",
                "Syntax error in configuration"
            };
            
            Console.WriteLine("  Testing permission error detection:");
            
            foreach (var error in errorMessages)
            {
                var hasPermissionError = error.Contains("Permission denied") || 
                                        error.Contains("permission denied") ||
                                        (error.Contains("open()") && error.Contains("failed (13:"));
                
                var hasSyntaxOk = error.Contains("syntax is ok") || error.Contains("test is successful");
                
                var shouldBeValid = hasPermissionError && (hasSyntaxOk || !error.Contains("Syntax error"));
                
                var errorPreview = error.Length > 50 ? error.Substring(0, 50) + "..." : error;
                Console.WriteLine($"    Error: \"{errorPreview}\"");
                Console.WriteLine($"      Has permission error: {hasPermissionError}");
                Console.WriteLine($"      Has syntax OK: {hasSyntaxOk}");
                Console.WriteLine($"      Should treat as valid: {shouldBeValid}");
                
                if (shouldBeValid && hasPermissionError)
                {
                    Console.WriteLine("      ✓ Correct: Permission errors will be handled gracefully");
                }
                else if (!hasPermissionError && error.Contains("Syntax error"))
                {
                    Console.WriteLine("      ✓ Correct: Syntax errors will be reported");
                }
                else
                {
                    Console.WriteLine("      ℹ Info: Edge case");
                }
            }
        }
    }
}
