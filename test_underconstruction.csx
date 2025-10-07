#r "RaCore/bin/Debug/net9.0/RaCore.dll"
#r "Abstractions/bin/Debug/net9.0/Abstractions.dll"

using RaCore.Tests;
using System;

Console.WriteLine("╔════════════════════════════════════════════════════════╗");
Console.WriteLine("║   Under Construction Mode Test Suite (Phase 9.3.8)    ║");
Console.WriteLine("╚════════════════════════════════════════════════════════╝");
Console.WriteLine();

try
{
    UnderConstructionTests.RunTests();
    
    Console.WriteLine();
    Console.WriteLine("✓ All tests passed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine($"✗ Test failed: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
