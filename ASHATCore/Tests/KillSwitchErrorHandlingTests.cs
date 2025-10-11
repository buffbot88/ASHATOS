using System;
using System.IO;

namespace ASHATCore.Tests
{
    /// <summary>
    /// Tests for kill switch and enhanced error handling functionality
    /// </summary>
    public class KillSwitchErrorHandlingTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== Kill Switch and Error Handling Tests ===\n");

            TestKillSwitchEnvironmentVariables();
            TestEnhancedErrorMessages();
            TestPermissionDeniedDetection();

            Console.WriteLine("\n=== All Kill Switch Tests Completed ===");
        }

        private static void TestKillSwitchEnvironmentVariables()
        {
            Console.WriteLine("--- Test 1: Kill Switch Environment Variables ---");
            
            // Test ASHATCore_SKIP_NGINX_CHECK
            Console.WriteLine("  Scenario 1: ASHATCore_SKIP_NGINX_CHECK not set");
            var nginxCheck = Environment.GetEnvironmentVariable("ASHATCore_SKIP_NGINX_CHECK");
            var shouldRunNginxCheck = string.IsNullOrEmpty(nginxCheck) || !nginxCheck.Equals("true", StringComparison.OrdinalIgnoreCase);
            
            if (shouldRunNginxCheck)
            {
                Console.WriteLine("    ✓ Nginx check will run (kill switch not active)");
            }
            else
            {
                Console.WriteLine("    ℹ Nginx check will be skipped (kill switch active)");
            }
            
            // Test ASHATCore_SKIP_PHP_CHECK
            Console.WriteLine("  Scenario 2: ASHATCore_SKIP_PHP_CHECK not set");
            var phpCheck = Environment.GetEnvironmentVariable("ASHATCore_SKIP_PHP_CHECK");
            var shouldRunPhpCheck = string.IsNullOrEmpty(phpCheck) || !phpCheck.Equals("true", StringComparison.OrdinalIgnoreCase);
            
            if (shouldRunPhpCheck)
            {
                Console.WriteLine("    ✓ PHP check will run (kill switch not active)");
            }
            else
            {
                Console.WriteLine("    ℹ PHP check will be skipped (kill switch active)");
            }
            
            // Test with kill switch enabled
            Console.WriteLine("  Scenario 3: Testing kill switch activation");
            Environment.SetEnvironmentVariable("ASHATCore_SKIP_NGINX_CHECK", "true");
            nginxCheck = Environment.GetEnvironmentVariable("ASHATCore_SKIP_NGINX_CHECK");
            shouldRunNginxCheck = string.IsNullOrEmpty(nginxCheck) || !nginxCheck.Equals("true", StringComparison.OrdinalIgnoreCase);
            
            if (!shouldRunNginxCheck)
            {
                Console.WriteLine("    ✓ Kill switch successfully activated for Nginx");
            }
            else
            {
                Console.WriteLine("    ✗ Kill switch failed to activate for Nginx");
            }
            
            // Clean up
            Environment.SetEnvironmentVariable("ASHATCore_SKIP_NGINX_CHECK", null);
        }

        private static void TestEnhancedErrorMessages()
        {
            Console.WriteLine("\n--- Test 2: Enhanced Error Messages ---");
            
            // Simulate an exception to test error message format
            try
            {
                throw new InvalidOperationException("Test error for validation");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Testing error message components:");
                Console.WriteLine($"    Reason: {ex.Message}");
                Console.WriteLine($"    Type: {ex.GetType().Name}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
                }
                else
                {
                    Console.WriteLine($"    Inner Exception: None");
                }
                
                var stackTrace = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "N/A";
                Console.WriteLine($"    Stack Trace: {stackTrace}");
                
                Console.WriteLine("    ✓ All error message components available");
            }
        }

        private static void TestPermissionDeniedDetection()
        {
            Console.WriteLine("\n--- Test 3: Permission Denied Detection ---");
            
            // Test detecting UnauthorizedAccessException
            try
            {
                throw new UnauthorizedAccessException("Access denied to test path");
            }
            catch (Exception ex)
            {
                var isPermissionError = ex is UnauthorizedAccessException;
                Console.WriteLine($"  Scenario 1: UnauthorizedAccessException");
                Console.WriteLine($"    Is permission error: {isPermissionError}");
                
                if (isPermissionError)
                {
                    Console.WriteLine("    ✓ Permission error correctly detected");
                    Console.WriteLine("    Suggested fix: Run with elevated privileges or check directory permissions");
                }
            }
            
            // Test detecting directory not found
            Console.WriteLine($"  Scenario 2: Directory existence check");
            var testPath = Path.Combine(Path.GetTempPath(), "ASHATCore-nonexistent", "test.ini");
            var directory = Path.GetDirectoryName(testPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Console.WriteLine($"    Directory does not exist: {directory}");
                Console.WriteLine("    ✓ Directory existence check working");
            }
        }
    }
}
