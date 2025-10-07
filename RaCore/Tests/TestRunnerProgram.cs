using RaCore.Tests;
using System;

namespace RaCore;

/// <summary>
/// Simple test runner for Phase 8 verification
/// </summary>
public class TestRunner
{
    public static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Legendary CMS Suite - Phase 8 Verification Test     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

        // Run LegendaryCMS tests
        LegendaryCMSTest.RunTests();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
