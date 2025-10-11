using LegendaryCMS.Core;
using System;

namespace ASHATCore.Tests;

/// <summary>
/// Simple test to verify LegendaryCMS module can be instantiated and initialized
/// </summary>
public class LegendaryCMSTest
{
    public static void RunTests()
    {
        Console.WriteLine("=== LegendaryCMS Module Test ===\n");

        try
        {
            // Test 1: Module instantiation
            Console.WriteLine("Test 1: Module Instantiation");
            var module = new LegendaryCMSModule();
            Console.WriteLine($"✓ Module created: {module.Name} v{module.Version}");

            // Test 2: Module initialization
            Console.WriteLine("\nTest 2: Module Initialization");
            module.Initialize(null);
            Console.WriteLine("✓ Module initialized successfully");

            // Test 3: Get status
            Console.WriteLine("\nTest 3: Status Check");
            var status = module.GetStatus();
            Console.WriteLine($"✓ Status retrieved:");
            Console.WriteLine($"  - Initialized: {status.IsInitialized}");
            Console.WriteLine($"  - Running: {status.IsRunning}");
            Console.WriteLine($"  - Version: {status.Version}");
            Console.WriteLine($"  - Components: {status.ComponentStatus.Count}");

            // Test 4: Execute commands
            Console.WriteLine("\nTest 4: Command Execution");
            
            var helpResult = module.Process("help");
            Console.WriteLine("✓ Help command executed");
            Console.WriteLine(helpResult);

            Console.WriteLine("\nTest 5: Status Command");
            var statusResult = module.Process("cms status");
            Console.WriteLine(statusResult);

            Console.WriteLine("\nTest 6: Config Command");
            var configResult = module.Process("cms config");
            Console.WriteLine(configResult);

            Console.WriteLine("\nTest 7: API Command");
            var apiResult = module.Process("cms api");
            Console.WriteLine(apiResult);

            Console.WriteLine("\nTest 8: RBAC Command");
            var rbacResult = module.Process("cms rbac");
            Console.WriteLine(rbacResult);

            // Test 9: Configuration access
            Console.WriteLine("\nTest 9: Configuration Access");
            var config = module.GetConfiguration();
            var siteName = config.GetValue<string>("Site:Name");
            Console.WriteLine($"✓ Configuration accessible: Site Name = {siteName}");

            // Cleanup
            module.Dispose();
            Console.WriteLine("\n✓ Module disposed successfully");

            Console.WriteLine("\n=== All Tests Passed ✓ ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Test Failed: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
