using RaCore.Tests;
using System;
using System.Threading.Tasks;

namespace RaCore;

/// <summary>
/// Simple test runner for Phase 8 verification
/// </summary>
public class TestRunner
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Legendary CMS Suite - Phase 8 Verification Test     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

        // Run LegendaryCMS tests
        LegendaryCMSTest.RunTests();

        // Run Failsafe tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "failsafe")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Failsafe Backup System Test Suite                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            FailsafeTest.RunTests();
        }

        // Run ServerSetup FTP tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "ftp")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ServerSetup FTP Management Test Suite               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            await ServerSetupFtpTest.RunTests();
        }

        // Run Phase 9.3.7 tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "phase937")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Phase 9.3.7: Bot Detection & CloudFlare Tests       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            Phase937Tests.RunTests();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
