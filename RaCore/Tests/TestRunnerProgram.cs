using RaCore.Tests;
using System;
using System.Threading.Tasks;

namespace RaCore;

/// <summary>
/// Simple test runner for Phase 8 verification
/// </summary>
#pragma warning disable CS7022 // Entry point warning - this class can be used as alternate entry point
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

        // Run Server Modes tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "servermodes")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Server Modes & Initialization Test Suite            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            ServerModesTests.RunTests();
        }

        // Run Under Construction tests if requested (Phase 9.3.8)
        if (args.Length > 0 && args[0].ToLowerInvariant() == "underconstruction")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Under Construction Mode Test Suite (Phase 9.3.8)    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            UnderConstructionTests.RunTests();
        }

        // Run CMS Homepage Routing tests if requested (Phase 9.3.9)
        if (args.Length > 0 && args[0].ToLowerInvariant() == "cmshomepage")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   CMS Homepage Routing Test Suite (Phase 9.3.9)       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            CmsHomepageRoutingTests.RunTests();
        }

        // Run ASHAT Personality & Emotion tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "ashatpersonality")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ASHAT Personality & Emotion Intelligence Tests      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            AshatPersonalityEmotionTests.RunTests();
        }

        // Run Windows 11 Kestrel-only tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "windows11")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Windows 11 Kestrel-Only Test Suite                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            Windows11KestrelTests.RunTests();
        }

        // Run ASHAT Deployment Workflow tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "deployment")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ASHAT Deployment Workflow Test Suite                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            AshatDeploymentWorkflowTests.RunTests();
        }

        // Run ASHAT Server Setup Helper tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "ashatsetup")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ASHAT Server Setup Helper Test Suite                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            await AshatServerSetupHelperTest.RunTests();
        }

        // Run Memory Management tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "memory")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Memory Management Test Suite                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            MemoryManagementTests.RunAll();
        }

        // Run Boot Sequence Fix tests if requested
        if (args.Length > 0 && args[0].ToLowerInvariant() == "bootsequence")
        {
            Console.WriteLine("\n\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Boot Sequence Fix Test Suite                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
            
            BootSequenceFixTests.RunTests();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
