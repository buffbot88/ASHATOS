using Abstractions;
using RaCore.Engine;
using System;

class DirectTest
{
    static void Main()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Under Construction Mode Test Suite (Phase 9.3.8)    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        
        try
        {
            Test1();
            Test2();
            Test3();
            Console.WriteLine("\n✓ All tests passed successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Test failed: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    static void Test1()
    {
        Console.WriteLine("Test 1: ServerConfiguration Properties");
        var config = new ServerConfiguration();
        if (config.UnderConstruction != false) throw new Exception("Default failed");
        config.UnderConstruction = true;
        config.UnderConstructionMessage = "Test";
        if (config.UnderConstructionMessage != "Test") throw new Exception("Message failed");
        Console.WriteLine("  ✓ Pass");
    }
    
    static void Test2()
    {
        Console.WriteLine("Test 2: Under Construction HTML Generation");
        var config = new ServerConfiguration { UnderConstruction = true };
        var html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
        if (!html.Contains("Under Construction")) throw new Exception("Title missing");
        if (!html.Contains("check back soon")) throw new Exception("Default message missing");
        Console.WriteLine("  ✓ Pass");
    }
    
    static void Test3()
    {
        Console.WriteLine("Test 3: Error Page Generation");
        var html = UnderConstructionHandler.GenerateErrorPage(404, "Not Found", "Missing");
        if (!html.Contains("404")) throw new Exception("Error code missing");
        if (!html.Contains("Not Found")) throw new Exception("Title missing");
        Console.WriteLine("  ✓ Pass");
    }
}
