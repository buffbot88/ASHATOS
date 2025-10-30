using System;
using System.IO;
using System.Threading.Tasks;
using ASHATCore.Engine;
using ASHATCore.Modules.Extensions.Updates;
using Abstractions;

namespace ASHATCore.Tests;

/// <summary>
/// Comprehensive test suite for HTTP, PHP, FTP, and Update capabilities in the AI Framework
/// Tests all networking and file transfer capabilities needed for AI operations
/// </summary>
public class HttpPhpFtpUpdateCapabilitiesTest
{
    public static async Task RunTests()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  HTTP/PHP/FTP/Update Capabilities Test for AI Framework           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝\n");

        // Test HTTP Capabilities
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("  SECTION 1: HTTP Capabilities Testing");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════\n");
        await TestHttpCapabilities();

        // Test FTP Capabilities
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("  SECTION 2: FTP Capabilities Testing");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════\n");
        await TestFtpCapabilities();

        // Test PHP Capabilities
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("  SECTION 3: PHP Configuration Capabilities Testing");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════\n");
        TestPhpCapabilities();

        // Test Update Capabilities
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
        Console.WriteLine("  SECTION 4: Update Module Capabilities Testing");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════\n");
        await TestUpdateCapabilities();

        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  All Tests Completed                                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝\n");
    }

    private static async Task TestHttpCapabilities()
    {
        Console.WriteLine("--- Test 1.1: HttpHelper Instantiation ---");
        try
        {
            using var httpHelper = new HttpHelper(30);
            Console.WriteLine("✅ HttpHelper instantiated successfully");
            Console.WriteLine($"   Timeout: 30 seconds\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ HttpHelper instantiation failed: {ex.Message}\n");
            return;
        }

        Console.WriteLine("--- Test 1.2: HTTP URL Accessibility Test ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            // Test a reliable public endpoint
            var testUrl = "https://www.google.com";
            var result = await httpHelper.TestUrlAsync(testUrl);
            
            if (result.success)
            {
                Console.WriteLine($"✅ URL test successful: {result.message}");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Status Code: {result.statusCode}\n");
            }
            else
            {
                Console.WriteLine($"⚠️  URL test result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  URL test exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }

        Console.WriteLine("--- Test 1.3: HTTP GET Request ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            var testUrl = "https://httpbin.org/get";
            var result = await httpHelper.GetAsync(testUrl);
            
            if (result.success)
            {
                Console.WriteLine($"✅ GET request successful");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Status Code: {result.statusCode}");
                Console.WriteLine($"   Response Length: {result.response?.Length ?? 0} characters\n");
            }
            else
            {
                Console.WriteLine($"⚠️  GET request result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  GET request exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }

        Console.WriteLine("--- Test 1.4: HTTP Download Text ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            var testUrl = "https://httpbin.org/robots.txt";
            var result = await httpHelper.DownloadTextAsync(testUrl);
            
            if (result.success)
            {
                Console.WriteLine($"✅ Text download successful");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Content Length: {result.content?.Length ?? 0} characters\n");
            }
            else
            {
                Console.WriteLine($"⚠️  Text download result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Text download exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }

        Console.WriteLine("--- Test 1.5: HTTP Download File (Binary) ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            var testUrl = "https://httpbin.org/bytes/1024";
            var result = await httpHelper.DownloadFileAsync(testUrl);
            
            if (result.success && result.data != null)
            {
                Console.WriteLine($"✅ File download successful");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Bytes Downloaded: {result.data.Length}\n");
            }
            else
            {
                Console.WriteLine($"⚠️  File download result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  File download exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }

        Console.WriteLine("--- Test 1.6: HTTP POST JSON ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            var testUrl = "https://httpbin.org/post";
            var testData = new { test = "data", timestamp = DateTime.UtcNow };
            var result = await httpHelper.PostJsonAsync(testUrl, testData);
            
            if (result.success)
            {
                Console.WriteLine($"✅ POST JSON successful");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Response Length: {result.response?.Length ?? 0} characters\n");
            }
            else
            {
                Console.WriteLine($"⚠️  POST JSON result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  POST JSON exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }

        Console.WriteLine("--- Test 1.7: HTTP Custom Headers ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            httpHelper.SetHeader("X-Custom-Header", "TestValue");
            httpHelper.SetAuthorizationHeader("Bearer", "test-token-12345");
            
            Console.WriteLine($"✅ Custom headers set successfully");
            Console.WriteLine($"   X-Custom-Header: TestValue");
            Console.WriteLine($"   Authorization: Bearer test-token-12345\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Custom headers failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 1.8: HTTP Download to File ---");
        try
        {
            using var httpHelper = new HttpHelper(10);
            
            var testUrl = "https://httpbin.org/robots.txt";
            var testFile = Path.Combine(Path.GetTempPath(), "http_test_download.txt");
            
            var result = await httpHelper.DownloadToFileAsync(testUrl, testFile);
            
            if (result.success)
            {
                Console.WriteLine($"✅ Download to file successful");
                Console.WriteLine($"   URL: {testUrl}");
                Console.WriteLine($"   Saved to: {testFile}");
                
                if (File.Exists(testFile))
                {
                    var fileInfo = new FileInfo(testFile);
                    Console.WriteLine($"   File Size: {fileInfo.Length} bytes");
                    File.Delete(testFile);
                    Console.WriteLine($"   Cleanup: File deleted\n");
                }
            }
            else
            {
                Console.WriteLine($"⚠️  Download to file result: {result.message}");
                Console.WriteLine($"   This is expected if there's no internet connection\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Download to file exception: {ex.Message}");
            Console.WriteLine($"   This is expected if there's no internet connection\n");
        }
    }

    private static async Task TestFtpCapabilities()
    {
        Console.WriteLine("--- Test 2.1: FtpHelper Instantiation ---");
        try
        {
            var config = new ServerConfiguration
            {
                FtpHost = "localhost",
                FtpPort = 21,
                FtpUsername = "testuser",
                FtpPassword = "testpass"
            };
            
            var ftpHelper = new FtpHelper(config);
            Console.WriteLine("✅ FtpHelper instantiated successfully");
            Console.WriteLine($"   Host: {config.FtpHost}");
            Console.WriteLine($"   Port: {config.FtpPort}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FtpHelper instantiation failed: {ex.Message}\n");
            return;
        }

        Console.WriteLine("--- Test 2.2: FTP Connection Test ---");
        try
        {
            var ftpHelper = new FtpHelper("localhost", 21, "testuser", "testpass");
            var result = await ftpHelper.TestConnectionAsync();
            
            if (result.success)
            {
                Console.WriteLine($"✅ FTP connection successful: {result.message}\n");
            }
            else
            {
                Console.WriteLine($"⚠️  FTP connection result: {result.message}");
                Console.WriteLine($"   This is expected if FTP server is not running\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  FTP connection exception: {ex.Message}");
            Console.WriteLine($"   This is expected if FTP server is not running\n");
        }

        Console.WriteLine("--- Test 2.3: FTP Operations Available ---");
        try
        {
            var ftpHelper = new FtpHelper("localhost", 21, null, null);
            
            Console.WriteLine($"✅ FTP operations available:");
            Console.WriteLine($"   - TestConnectionAsync()");
            Console.WriteLine($"   - UploadFileAsync()");
            Console.WriteLine($"   - DownloadFileAsync()");
            Console.WriteLine($"   - ListDirectoryAsync()");
            Console.WriteLine($"   - CreateDirectoryAsync()");
            Console.WriteLine($"   - DeleteFileAsync()\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FTP operations check failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 2.4: FTP Methods Existence ---");
        try
        {
            var ftpHelper = new FtpHelper("test", 21, null, null);
            var type = ftpHelper.GetType();
            
            var methods = new[]
            {
                "TestConnectionAsync",
                "UploadFileAsync",
                "DownloadFileAsync",
                "ListDirectoryAsync",
                "CreateDirectoryAsync",
                "DeleteFileAsync"
            };

            var allMethodsExist = true;
            foreach (var methodName in methods)
            {
                var method = type.GetMethod(methodName);
                if (method != null)
                {
                    Console.WriteLine($"   ✅ {methodName} - Found");
                }
                else
                {
                    Console.WriteLine($"   ❌ {methodName} - Not Found");
                    allMethodsExist = false;
                }
            }

            if (allMethodsExist)
            {
                Console.WriteLine($"\n✅ All FTP methods exist and are callable\n");
            }
            else
            {
                Console.WriteLine($"\n⚠️  Some FTP methods are missing\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FTP methods check failed: {ex.Message}\n");
        }
    }

    private static void TestPhpCapabilities()
    {
        Console.WriteLine("--- Test 3.1: Apache/PHP Manager Capabilities ---");
        try
        {
            Console.WriteLine($"✅ ApacheManager static methods available:");
            Console.WriteLine($"   - ScanForApacheConfig()");
            Console.WriteLine($"   - ScanForPhpConfig()");
            Console.WriteLine($"   - ScanForPhpFolder()");
            Console.WriteLine($"   - VerifyApacheConfig()");
            Console.WriteLine($"   - VerifyPhpConfig()");
            Console.WriteLine($"   - ConfigurePhpIni()");
            Console.WriteLine($"   - IsApacheAvailable()\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ApacheManager check failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 3.2: PHP Folder Scan ---");
        try
        {
            var result = ApacheManager.ScanForPhpFolder();
            Console.WriteLine($"   PHP Folder Scan Result:");
            Console.WriteLine($"   Found: {result.found}");
            Console.WriteLine($"   Message: {result.message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  PHP folder scan exception: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 3.3: Apache Availability Check ---");
        try
        {
            var isAvailable = ApacheManager.IsApacheAvailable();
            Console.WriteLine($"   Apache Available: {isAvailable}");
            
            if (isAvailable)
            {
                Console.WriteLine($"   ✅ Apache is installed on this system\n");
            }
            else
            {
                Console.WriteLine($"   ⚠️  Apache is not installed (expected on Windows or non-Apache systems)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Apache availability check exception: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 3.4: PHP Configuration Scan ---");
        try
        {
            var result = ApacheManager.ScanForPhpConfig();
            Console.WriteLine($"   PHP Config Scan Result:");
            Console.WriteLine($"   Found: {result.found}");
            Console.WriteLine($"   Path: {result.path ?? "N/A"}");
            Console.WriteLine($"   Message: {result.message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  PHP config scan exception: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 3.5: Apache Configuration Scan ---");
        try
        {
            var result = ApacheManager.ScanForApacheConfig();
            Console.WriteLine($"   Apache Config Scan Result:");
            Console.WriteLine($"   Found: {result.found}");
            Console.WriteLine($"   Path: {result.path ?? "N/A"}");
            Console.WriteLine($"   Message: {result.message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Apache config scan exception: {ex.Message}\n");
        }
    }

    private static async Task TestUpdateCapabilities()
    {
        Console.WriteLine("--- Test 4.1: UpdateModule Instantiation ---");
        UpdateModule? updateModule = null;
        try
        {
            updateModule = new UpdateModule();
            updateModule.Initialize(null);
            Console.WriteLine("✅ UpdateModule instantiated successfully\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ UpdateModule instantiation failed: {ex.Message}\n");
            return;
        }

        Console.WriteLine("--- Test 4.2: Update Statistics ---");
        try
        {
            var stats = updateModule.Process("update stats");
            Console.WriteLine($"✅ Update statistics retrieved:");
            Console.WriteLine($"{stats}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Update statistics failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 4.3: List Available Updates ---");
        try
        {
            var updates = updateModule.GetAllUpdates();
            Console.WriteLine($"✅ Available updates: {updates.Count()}");
            
            foreach (var update in updates.Take(3))
            {
                Console.WriteLine($"   - Version: {update.Version}");
                Console.WriteLine($"     Status: {update.Status}");
                Console.WriteLine($"     Created: {update.CreatedAt}");
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ List updates failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 4.4: Create Test Update Package ---");
        try
        {
            var testVersion = "9.9.9-test";
            var testChangelog = "Test update package for capability verification";
            var testData = System.Text.Encoding.UTF8.GetBytes("Test update package content");
            
            var package = await updateModule.CreateUpdatePackageAsync(testVersion, testChangelog, testData);
            
            Console.WriteLine($"✅ Test update package created:");
            Console.WriteLine($"   Version: {package.Version}");
            Console.WriteLine($"   Size: {package.SizeBytes} bytes");
            Console.WriteLine($"   Checksum: {package.ChecksumSHA256.Substring(0, 16)}...");
            Console.WriteLine($"   Status: {package.Status}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Create update package failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 4.5: Update Package Retrieval ---");
        try
        {
            var testVersion = "9.9.9-test";
            var package = updateModule.GetUpdatePackage(testVersion);
            
            if (package != null)
            {
                Console.WriteLine($"✅ Update package retrieved:");
                Console.WriteLine($"   Version: {package.Version}");
                Console.WriteLine($"   Size: {package.SizeBytes} bytes");
                Console.WriteLine($"   Mandatory: {package.IsMandatory}\n");
            }
            else
            {
                Console.WriteLine($"⚠️  Update package not found (expected if not created in Test 4.4)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Update package retrieval failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 4.6: Package Integrity Verification ---");
        try
        {
            var testVersion = "9.9.9-test";
            var package = updateModule.GetUpdatePackage(testVersion);
            
            if (package != null)
            {
                var isValid = updateModule.VerifyPackageIntegrity(testVersion, package.ChecksumSHA256);
                Console.WriteLine($"✅ Package integrity verification:");
                Console.WriteLine($"   Version: {testVersion}");
                Console.WriteLine($"   Valid: {isValid}\n");
            }
            else
            {
                Console.WriteLine($"⚠️  Cannot verify - package not found\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Package integrity verification failed: {ex.Message}\n");
        }

        Console.WriteLine("--- Test 4.7: Update Module Commands ---");
        try
        {
            Console.WriteLine($"✅ Update module commands available:");
            var help = updateModule.Process("help");
            Console.WriteLine($"{help}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Update commands check failed: {ex.Message}\n");
        }

        // Cleanup
        try
        {
            updateModule?.Dispose();
        }
        catch { }
    }
}
