using ASHATCore.Tests;

namespace ASHATCore;

/// <summary>
/// Standalone test runner for Login Persistence tests
/// </summary>
public class TestRunnerLoginPersistence
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Login Persistence Tests...\n");
        LoginPersistenceTests.RunTests();
        Console.WriteLine("\nTests completed. Press any key to exit...");
        Console.ReadKey();
    }
}
