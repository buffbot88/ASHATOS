using Abstractions;
using RaCore.Engine;
using System;

namespace RaCore.Tests;

/// <summary>
/// Standalone test runner for Under Construction Tests (no server startup)
/// </summary>
#pragma warning disable CS7022 // Entry point warning - this class can be used as alternate entry point
class StandaloneUnderConstructionTestRunner
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Under Construction Mode Test Suite (Phase 9.3.8)    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            UnderConstructionTests.RunTests();
            
            Console.WriteLine();
            Console.WriteLine("✓ All tests passed successfully!");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"✗ Test failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
