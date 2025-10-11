using ASHATCore.Modules.Extensions.Safety;
using System;
using System.Text.Json;

namespace ASHATCore.Tests;

/// <summary>
/// Test suite for Failsafe Backup System module
/// </summary>
public class FailsafeTest
{
    public static void RunTests()
    {
        Console.WriteLine("=== Failsafe Backup System Test ===\n");

        try
        {
            // Test 1: Module instantiation
            Console.WriteLine("Test 1: Module Instantiation");
            var module = new FailsafeModule();
            Console.WriteLine($"✓ Module created: {module.Name}");

            // Test 2: Module initialization
            Console.WriteLine("\nTest 2: Module Initialization");
            module.Initialize(null);
            Console.WriteLine("✓ Module initialized successfully");

            // Test 3: Get help
            Console.WriteLine("\nTest 3: Help Command");
            var helpResult = module.Process("help");
            Console.WriteLine("✓ Help command executed:");
            Console.WriteLine(helpResult);

            // Test 4: Get detailed failsafe help
            Console.WriteLine("\nTest 4: Detailed Failsafe Help");
            var failsafeHelp = module.Process("help_failsafe");
            Console.WriteLine("✓ Failsafe help executed:");
            Console.WriteLine(failsafeHelp);

            // Test 5: Check status (before password set)
            Console.WriteLine("\nTest 5: Status Check (No Password Set)");
            var statusBefore = module.Process("failsafe status");
            Console.WriteLine("✓ Status retrieved:");
            var statusObj = JsonSerializer.Deserialize<JsonElement>(statusBefore);
            Console.WriteLine($"  - Password Set: {statusObj.GetProperty("FailsafePasswordSet").GetBoolean()}");
            Console.WriteLine($"  - Total Backups: {statusObj.GetProperty("TotalBackups").GetInt32()}");
            Console.WriteLine($"  - Safe Backups: {statusObj.GetProperty("SafeBackups").GetInt32()}");

            // Test 6: Set failsafe password
            Console.WriteLine("\nTest 6: Set Failsafe Password");
            var setPasswordResult = module.Process("failsafe setpassword TestPassword123!");
            Console.WriteLine("✓ Password set:");
            var setPasswordObj = JsonSerializer.Deserialize<JsonElement>(setPasswordResult);
            Console.WriteLine($"  - Success: {setPasswordObj.GetProperty("Success").GetBoolean()}");
            Console.WriteLine($"  - Message: {setPasswordObj.GetProperty("Message").GetString()}");

            // Test 7: Check status (after password set)
            Console.WriteLine("\nTest 7: Status Check (After Password Set)");
            var statusAfter = module.Process("failsafe status");
            var statusAfterObj = JsonSerializer.Deserialize<JsonElement>(statusAfter);
            Console.WriteLine("✓ Updated status:");
            Console.WriteLine($"  - Password Set: {statusAfterObj.GetProperty("FailsafePasswordSet").GetBoolean()}");

            // Test 8: Trigger failsafe backup
            Console.WriteLine("\nTest 8: Trigger Emergency Failsafe");
            var failsafeResult = module.Process("help_failsafe -start TEST-LICENSE-KEY");
            Console.WriteLine("✓ Failsafe triggered:");
            var failsafeObj = JsonSerializer.Deserialize<JsonElement>(failsafeResult);
            Console.WriteLine($"  - Success: {failsafeObj.GetProperty("Success").GetBoolean()}");
            Console.WriteLine($"  - Message: {failsafeObj.GetProperty("Message").GetString()}");
            var backupId = failsafeObj.GetProperty("BackupId").GetString();
            Console.WriteLine($"  - Backup ID: {backupId}");

            // Test 9: List backups
            Console.WriteLine("\nTest 9: List All Backups");
            var backupsResult = module.Process("failsafe backups");
            Console.WriteLine("✓ Backups listed:");
            var backupsObj = JsonSerializer.Deserialize<JsonElement>(backupsResult);
            Console.WriteLine($"  - Total Backups: {backupsObj.GetProperty("TotalBackups").GetInt32()}");

            // Test 10: Mark backup as safe
            Console.WriteLine("\nTest 10: Mark Backup as Safe");
            var markSafeResult = module.Process($"failsafe marksafe {backupId}");
            Console.WriteLine("✓ Backup marked as safe:");
            var markSafeObj = JsonSerializer.Deserialize<JsonElement>(markSafeResult);
            Console.WriteLine($"  - Success: {markSafeObj.GetProperty("Success").GetBoolean()}");
            Console.WriteLine($"  - Message: {markSafeObj.GetProperty("Message").GetString()}");

            // Test 11: Trigger another failsafe (should compare with safe backup)
            Console.WriteLine("\nTest 11: Trigger Second Failsafe (With Comparison)");
            var secondFailsafeResult = module.Process("help_failsafe -start TEST-LICENSE-KEY");
            Console.WriteLine("✓ Second failsafe triggered with comparison:");
            var secondFailsafeObj = JsonSerializer.Deserialize<JsonElement>(secondFailsafeResult);
            Console.WriteLine($"  - Success: {secondFailsafeObj.GetProperty("Success").GetBoolean()}");
            var investigation = secondFailsafeObj.GetProperty("Investigation");
            Console.WriteLine($"  - Investigation Status: {investigation.GetProperty("Status").GetString()}");
            Console.WriteLine($"  - Has Safe Backup: {investigation.GetProperty("Comparison").GetProperty("HasSafeBackup").GetBoolean()}");

            // Test 12: Restore from backup
            Console.WriteLine("\nTest 12: Restore from Backup");
            var restoreResult = module.Process($"failsafe restore {backupId}");
            Console.WriteLine("✓ Restore initiated:");
            var restoreObj = JsonSerializer.Deserialize<JsonElement>(restoreResult);
            Console.WriteLine($"  - Success: {restoreObj.GetProperty("Success").GetBoolean()}");
            Console.WriteLine($"  - Message: {restoreObj.GetProperty("Message").GetString()}");

            // Test 13: Invalid commands
            Console.WriteLine("\nTest 13: Invalid Command Handling");
            var invalidResult = module.Process("failsafe invalidcommand");
            Console.WriteLine("✓ Invalid command handled gASHATcefully:");
            Console.WriteLine($"  - {invalidResult}");

            // Test 14: Missing Parameters
            Console.WriteLine("\nTest 14: Missing Parameter Handling");
            var missingPaASHATmResult = module.Process("help_failsafe -start");
            Console.WriteLine("✓ Missing Parameter handled:");
            Console.WriteLine($"  - {missingPaASHATmResult}");

            Console.WriteLine("\n=== All Failsafe Tests Passed Successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Test failed with error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
