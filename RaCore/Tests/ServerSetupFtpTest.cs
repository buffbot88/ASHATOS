using System;
using System.IO;
using System.Threading.Tasks;
using RaCore.Modules.Extensions.ServerSetup;

namespace RaCore.Tests
{
    /// <summary>
    /// Test for ServerSetup FTP management functionality
    /// </summary>
    public class ServerSetupFtpTest
    {
        public static async Task RunTests()
        {
            Console.WriteLine("=== ServerSetup FTP Management Test ===\n");

            var module = new ServerSetupModule();
            module.Initialize(null);

            // Test 0: Server Health Check
            Console.WriteLine("\n--- Test 0: Server Health Check ---");
            await TestServerHealth(module);

            // Test 1: FTP Status Check
            Console.WriteLine("\n--- Test 1: FTP Status Check ---");
            await TestFtpStatus(module);

            // Test 2: Admin Instance Creation with FTP
            Console.WriteLine("\n--- Test 2: Admin Instance Creation ---");
            await TestAdminInstanceCreation(module);

            // Test 3: FTP Setup for Admin
            Console.WriteLine("\n--- Test 3: FTP Setup for Admin ---");
            await TestFtpSetup(module);

            // Test 4: FTP Connection Info
            Console.WriteLine("\n--- Test 4: FTP Connection Info ---");
            await TestFtpConnectionInfo(module);

            // Test 5: Create Restricted FTP User (instructions only)
            Console.WriteLine("\n--- Test 5: Create Restricted FTP User ---");
            await TestCreateRestrictedFtpUser(module);

            // Cleanup
            Console.WriteLine("\n--- Cleanup ---");
            await CleanupTestData(module);

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        private static async Task TestServerHealth(ServerSetupModule module)
        {
            try
            {
                var health = await module.CheckLiveServerHealthAsync();
                
                Console.WriteLine($"✓ Server Health Check Completed");
                Console.WriteLine($"  Is Operational: {health.IsOperational}");
                Console.WriteLine($"  Databases Accessible: {health.DatabasesAccessible}");
                Console.WriteLine($"  PHP Folder Accessible: {health.PhpFolderAccessible}");
                Console.WriteLine($"  Admins Folder Accessible: {health.AdminsFolderAccessible}");
                Console.WriteLine($"  FTP Folder Accessible: {health.FtpFolderAccessible}");
                
                if (health.Issues.Count > 0)
                {
                    Console.WriteLine($"  Issues: {string.Join(", ", health.Issues)}");
                }

                // Test console command
                var commandResult = module.Process("serversetup health");
                Console.WriteLine($"\n  Console Command Output:");
                Console.WriteLine($"  {commandResult.Replace("\n", "\n  ")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Server Health Check Failed: {ex.Message}");
            }
        }

        private static async Task TestFtpStatus(ServerSetupModule module)
        {
            try
            {
                var status = await module.GetFtpStatusAsync();
                
                Console.WriteLine($"✓ FTP Status Check Successful");
                Console.WriteLine($"  Is Linux: {status.IsLinux}");
                Console.WriteLine($"  Is Installed: {status.IsInstalled}");
                Console.WriteLine($"  Is Running: {status.IsRunning}");
                
                if (!string.IsNullOrWhiteSpace(status.Version))
                {
                    Console.WriteLine($"  Version: {status.Version}");
                }
                
                if (!string.IsNullOrWhiteSpace(status.ConfigPath))
                {
                    Console.WriteLine($"  Config Path: {status.ConfigPath}");
                }
                
                Console.WriteLine($"  Message: {status.Message}");

                // Test console command
                var commandResult = module.Process("serversetup ftp status");
                Console.WriteLine($"\n  Console Command Output:");
                Console.WriteLine($"  {commandResult.Replace("\n", "\n  ")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FTP Status Check Failed: {ex.Message}");
            }
        }

        private static async Task TestAdminInstanceCreation(ServerSetupModule module)
        {
            try
            {
                var testLicense = "TEST123";
                var testUsername = "ftptest";

                var result = await module.CreateAdminFolderStructureAsync(testLicense, testUsername);
                
                if (result.Success)
                {
                    Console.WriteLine($"✓ Admin Instance Creation Successful");
                    Console.WriteLine($"  Path: {result.Path}");
                    Console.WriteLine($"  Message: {result.Message}");
                }
                else
                {
                    Console.WriteLine($"✗ Admin Instance Creation Failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Admin Instance Creation Failed: {ex.Message}");
            }
        }

        private static async Task TestFtpSetup(ServerSetupModule module)
        {
            try
            {
                var testLicense = "TEST123";
                var testUsername = "ftptest";

                var result = await module.SetupFtpAccessAsync(testLicense, testUsername);
                
                if (result.Success)
                {
                    Console.WriteLine($"✓ FTP Setup Successful");
                    Console.WriteLine($"  Path: {result.Path}");
                    Console.WriteLine($"  Message: {result.Message}");
                    
                    if (result.Details.ContainsKey("ftp_path"))
                    {
                        Console.WriteLine($"  FTP Path: {result.Details["ftp_path"]}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠ FTP Setup Result: {result.Message}");
                }

                // Test console command
                var commandResult = module.Process($"serversetup ftp setup license={testLicense} username={testUsername}");
                Console.WriteLine($"\n  Console Command Output:");
                Console.WriteLine($"  {commandResult.Replace("\n", "\n  ")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FTP Setup Failed: {ex.Message}");
            }
        }

        private static async Task TestFtpConnectionInfo(ServerSetupModule module)
        {
            try
            {
                var testLicense = "TEST123";
                var testUsername = "ftptest";

                var result = await module.GetFtpConnectionInfoAsync(testLicense, testUsername);
                
                if (result.Success)
                {
                    Console.WriteLine($"✓ FTP Connection Info Retrieved");
                    Console.WriteLine($"  Host: {result.Host}");
                    Console.WriteLine($"  Port: {result.Port}");
                    Console.WriteLine($"  Username: {result.Username}");
                    Console.WriteLine($"  FTP Path: {result.FtpPath}");
                }
                else
                {
                    Console.WriteLine($"⚠ FTP Connection Info Result: {result.Message}");
                }

                // Test console command
                var commandResult = module.Process($"serversetup ftp info license={testLicense} username={testUsername}");
                Console.WriteLine($"\n  Console Command Output:");
                Console.WriteLine($"  {commandResult.Replace("\n", "\n  ")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FTP Connection Info Failed: {ex.Message}");
            }
        }

        private static async Task CleanupTestData(ServerSetupModule module)
        {
            try
            {
                var testLicense = "TEST123";
                var testUsername = "ftptest";
                
                var adminPath = module.GetAdminInstancePath(testLicense, testUsername);
                if (Directory.Exists(adminPath))
                {
                    Directory.Delete(adminPath, true);
                    Console.WriteLine($"✓ Cleaned up admin instance: {adminPath}");
                }

                var ftpPath = Path.Combine(Directory.GetCurrentDirectory(), "ftp", $"{testLicense}.{testUsername}");
                if (Directory.Exists(ftpPath))
                {
                    Directory.Delete(ftpPath, true);
                    Console.WriteLine($"✓ Cleaned up FTP directory: {ftpPath}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Cleanup Warning: {ex.Message}");
            }
        }

        private static async Task TestCreateRestrictedFtpUser(ServerSetupModule module)
        {
            try
            {
                var testUsername = "raos_ftp_test";
                var testPath = "/home/racore/TheRaProject/RaCore";

                var result = await module.CreateRestrictedFtpUserAsync(testUsername, testPath);
                
                Console.WriteLine($"✓ FTP User Creation Instructions Retrieved");
                Console.WriteLine($"  Success: {result.Success}");
                Console.WriteLine($"  Message Preview: {result.Message.Substring(0, Math.Min(100, result.Message.Length))}...");
                
                // Test console command
                var commandResult = module.Process($"serversetup ftp createuser username={testUsername} path={testPath}");
                Console.WriteLine($"\n  Console Command Output Preview:");
                Console.WriteLine($"  {commandResult.Substring(0, Math.Min(150, commandResult.Length)).Replace("\n", "\n  ")}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Create Restricted FTP User Test Failed: {ex.Message}");
            }
        }
    }
}
