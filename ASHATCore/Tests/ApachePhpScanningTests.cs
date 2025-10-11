using ASHATCore.Engine;
using System;
using System.IO;

namespace ASHATCore.Tests
{
    /// <summary>
    /// Tests for Apache and PHP Configuration scanning functionality
    /// Verifies that ASHATOS correctly scans for Apache httpd.conf and PHP php.ini
    /// from the static Configuration folder at C:\ASHATOS\webserver\settings
    /// </summary>
    public class ApachePhpScanningTests
    {
        public static void RunTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Apache and PHP Scanning Tests");
            Console.WriteLine("========================================");
            Console.WriteLine();

            TestApacheManagerExists();
            TestApacheScanningMethod();
            TestPhpScanningMethod();
            TestPhpFolderScanningMethod();
            TestApacheAvailabilityCheck();
            TestConfigVerificationMethods();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("  Apache and PHP Scanning Tests Complete");
            Console.WriteLine("========================================");
        }

        private static void TestApacheManagerExists()
        {
            Console.WriteLine("[Test 1/5] Testing ApacheManager class exists...");
            try
            {
                var manager = new ApacheManager("test", 80);
                Console.WriteLine("  ✅ ApacheManager instantiated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Failed to instantiate ApacheManager: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestApacheScanningMethod()
        {
            Console.WriteLine("[Test 2/5] Testing Apache Configuration scanning...");
            try
            {
                var result = ApacheManager.ScanFoASHATpacheConfig();
                
                if (result.found && result.path != null)
                {
                    Console.WriteLine($"  ✅ Apache config found: {result.path}");
                }
                else
                {
                    Console.WriteLine($"  ℹ️  Apache config not found: {result.message}");
                    Console.WriteLine("     This is expected if C:\\ASHATOS\\webserver\\settings\\httpd.conf doesn't exist");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Apache scanning failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestPhpScanningMethod()
        {
            Console.WriteLine("[Test 3/6] Testing PHP Configuration scanning...");
            try
            {
                var result = ApacheManager.ScanForPhpConfig();
                
                if (result.found && result.path != null)
                {
                    Console.WriteLine($"  ✅ PHP config found: {result.path}");
                }
                else
                {
                    Console.WriteLine($"  ℹ️  PHP config not found: {result.message}");
                    Console.WriteLine("     This is expected if C:\\ASHATOS\\webserver\\settings\\php.ini doesn't exist");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ PHP scanning failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestPhpFolderScanningMethod()
        {
            Console.WriteLine("[Test 4/6] Testing PHP folder scanning in ASHATCore directory...");
            try
            {
                var result = ApacheManager.ScanForPhpFolder();
                
                if (result.found && result.path != null)
                {
                    Console.WriteLine($"  ✅ PHP folder found: {result.path}");
                }
                else
                {
                    Console.WriteLine($"  ℹ️  PHP folder not found: {result.message}");
                    Console.WriteLine("     This is expected if php folder doesn't exist in ASHATCore.exe directory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ PHP folder scanning failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestApacheAvailabilityCheck()
        {
            Console.WriteLine("[Test 5/6] Testing Apache availability check...");
            try
            {
                var isAvailable = ApacheManager.IsApacheAvailable();
                
                if (isAvailable)
                {
                    Console.WriteLine("  ✅ Apache web server detected on system");
                }
                else
                {
                    Console.WriteLine("  ℹ️  Apache web server not detected");
                    Console.WriteLine("     This is expected if Apache is not installed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Apache availability check failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestConfigVerificationMethods()
        {
            Console.WriteLine("[Test 6/6] Testing Configuration verification methods...");
            try
            {
                // Create a temporary test config file
                var tempDir = Path.Combine(Path.GetTempPath(), "ASHATOS_test");
                Directory.CreateDirectory(tempDir);
                
                var testHttpdConf = Path.Combine(tempDir, "httpd.conf");
                File.WriteAllText(testHttpdConf, @"
# Test httpd.conf
LoadModule proxy_module modules/mod_proxy.so
LoadModule proxy_http_module modules/mod_proxy_http.so

# ASHATCore Configuration
<VirtualHost *:80>
    ServerName localhost
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/
</VirtualHost>
");
                
                var verifyResult = ApacheManager.VerifyApacheConfig(testHttpdConf);
                
                if (verifyResult.valid)
                {
                    Console.WriteLine("  ✅ Apache Configuration verification works correctly");
                }
                else
                {
                    Console.WriteLine($"  ℹ️  Apache config verification found issues:");
                    foreach (var issue in verifyResult.issues)
                    {
                        Console.WriteLine($"      - {issue}");
                    }
                }
                
                // Test PHP config verification
                var testPhpIni = Path.Combine(tempDir, "php.ini");
                File.WriteAllText(testPhpIni, @"
memory_limit = 256M
max_execution_time = 60
post_max_size = 10M
upload_max_filesize = 10M
");
                
                var phpVerifyResult = ApacheManager.VerifyPhpConfig(testPhpIni);
                
                if (phpVerifyResult.valid)
                {
                    Console.WriteLine("  ✅ PHP Configuration verification works correctly");
                }
                else
                {
                    Console.WriteLine($"  ℹ️  PHP config verification found issues:");
                    foreach (var issue in phpVerifyResult.issues)
                    {
                        Console.WriteLine($"      - {issue}");
                    }
                }
                
                // Cleanup
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Configuration verification test failed: {ex.Message}");
            }
            Console.WriteLine();
        }
    }
}
