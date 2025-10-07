using Abstractions;
using RaCore.Engine;
using System;

namespace TestRunner
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Under Construction Mode Test Suite (Phase 9.3.8)    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            // Test 1: ServerConfiguration
            TestServerConfiguration();
            
            // Test 2: HTML Generation
            TestUnderConstructionHandler();
            
            // Test 3: Error Page Generation
            TestErrorPages();
            
            Console.WriteLine();
            Console.WriteLine("✓ All tests passed successfully!");
        }
        
        static void TestServerConfiguration()
        {
            Console.WriteLine("Test 1: ServerConfiguration - Under Construction Properties");
            
            var config = new ServerConfiguration();
            Assert(config.UnderConstruction == false, "Default should be false");
            
            config.UnderConstruction = true;
            Assert(config.UnderConstruction == true, "Should be settable");
            
            config.UnderConstructionMessage = "Custom message";
            Assert(config.UnderConstructionMessage == "Custom message", "Message should be settable");
            
            Console.WriteLine("  ✓ Pass");
        }
        
        static void TestUnderConstructionHandler()
        {
            Console.WriteLine("Test 2: UnderConstructionHandler - Page Generation");
            
            var config = new ServerConfiguration { UnderConstruction = true };
            var html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
            
            Assert(html.Contains("<!DOCTYPE html>"), "Should have DOCTYPE");
            Assert(html.Contains("Under Construction"), "Should have title");
            Assert(html.Contains("check back soon"), "Should have default message");
            
            config.UnderConstructionMessage = "Custom!";
            html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
            Assert(html.Contains("Custom!"), "Should have custom message");
            
            Console.WriteLine("  ✓ Pass");
        }
        
        static void TestErrorPages()
        {
            Console.WriteLine("Test 3: Error Page Generation");
            
            var html = UnderConstructionHandler.GenerateErrorPage(404, "Not Found", "Missing");
            Assert(html.Contains("404"), "Should have error code");
            Assert(html.Contains("Not Found"), "Should have title");
            Assert(html.Contains("Missing"), "Should have message");
            
            Console.WriteLine("  ✓ Pass");
        }
        
        static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception($"Failed: {message}");
        }
    }
}
