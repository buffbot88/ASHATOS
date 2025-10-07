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

        // Run ServerSetup FTP tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "ftp")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ServerSetup FTP Management Test Suite               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            await ServerSetupFtpTest.RunTests();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
