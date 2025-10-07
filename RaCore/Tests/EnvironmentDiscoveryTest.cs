using System;
using System.IO;
using System.Linq;
using RaCore.Engine.Manager;

namespace RaCore.Tests
{
    /// <summary>
    /// Simple test program to verify RaOS v7.5 environment discovery and scanning features
    /// </summary>
    public class EnvironmentDiscoveryTest
    {
        public static void RunTests()
        {
            Console.WriteLine("=== RaOS v7.5 Environment Discovery Test ===\n");

            var moduleManager = new ModuleManager();
            moduleManager.DebugLoggingEnabled = true;

            // Test 1: Environment Discovery
            Console.WriteLine("\n--- Test 1: Environment Discovery ---");
            TestEnvironmentDiscovery(moduleManager);

            // Test 2: Folder Update Scanning
            Console.WriteLine("\n--- Test 2: Folder Update Scanning ---");
            TestFolderUpdateScanning(moduleManager);

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        private static void TestEnvironmentDiscovery(ModuleManager manager)
        {
            try
            {
                var environment = manager.DiscoverEnvironment();

                Console.WriteLine($"✓ Environment Discovery Successful");
                Console.WriteLine($"  Root Directory: {environment.RootDirectory}");
                Console.WriteLine($"  App Base Directory: {environment.AppBaseDirectory}");
                Console.WriteLine($"  Discovery Time: {environment.DiscoveryTime}");
                Console.WriteLine($"  Module Folders: {environment.ModuleFolders.Count}");
                Console.WriteLine($"  External Resources: {environment.ExternalResources.Count}");
                Console.WriteLine($"  Configuration Files: {environment.ConfigurationFiles.Count}");
                Console.WriteLine($"  Admin Instances: {environment.AdminInstances.Count}");

                if (environment.ModuleFolders.Any())
                {
                    Console.WriteLine("\n  Discovered Module Folders:");
                    foreach (var folder in environment.ModuleFolders.Take(5))
                    {
                        Console.WriteLine($"    - {folder}");
                    }
                    if (environment.ModuleFolders.Count > 5)
                    {
                        Console.WriteLine($"    ... and {environment.ModuleFolders.Count - 5} more");
                    }
                }

                if (environment.ExternalResources.Any())
                {
                    Console.WriteLine("\n  Discovered External Resources:");
                    foreach (var resource in environment.ExternalResources)
                    {
                        Console.WriteLine($"    - {resource.Name} ({resource.Type}): {(resource.Exists ? "✓ Found" : "✗ Not Found")}");
                    }
                }

                if (environment.AdminInstances.Any())
                {
                    Console.WriteLine($"\n  Admin Instances Found: {environment.AdminInstances.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Environment Discovery Failed: {ex.Message}");
            }
        }

        private static void TestFolderUpdateScanning(ModuleManager manager)
        {
            try
            {
                var rootDir = Directory.GetCurrentDirectory();

                var foldersToScan = new[]
                {
                    Path.Combine(rootDir, "RaCore"),
                    Path.Combine(rootDir, "Abstractions"),
                    Path.Combine(rootDir, "Modules")
                }.Where(Directory.Exists).ToArray();

                if (!foldersToScan.Any())
                {
                    Console.WriteLine("⚠ No folders found to scan");
                    return;
                }

                var updateResult = manager.ScanForUpdates(foldersToScan);

                Console.WriteLine($"✓ Folder Update Scan Successful");
                Console.WriteLine($"  Scan Time: {updateResult.ScanTime}");
                Console.WriteLine($"  Scanned Folders: {updateResult.ScannedFolders.Count}");
                Console.WriteLine($"  Folder Details: {updateResult.FolderDetails.Count}");

                Console.WriteLine("\n  Folder Details:");
                foreach (var folder in updateResult.FolderDetails)
                {
                    Console.WriteLine($"\n    {folder.Name}:");
                    Console.WriteLine($"      Path: {folder.Path}");
                    Console.WriteLine($"      Last Modified: {folder.LastModified}");
                    Console.WriteLine($"      File Count: {folder.FileCount}");
                    Console.WriteLine($"      Subdirectories: {folder.Subdirectories.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Folder Update Scan Failed: {ex.Message}");
            }
        }
    }
}
