using System;
using ASHATCore.Tests;

/// <summary>
/// Standalone test runner for Coding Agent tests
/// </summary>
class CodingAgentTestRunner
{
    static int Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting Coding Agent Natural Language Tests...\n");
            CodingAgentTests.RunTests();
            Console.WriteLine("\n✅ All tests completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }
}
