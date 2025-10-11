using Abstractions;
using ASHATCore.Engine;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for Under Construction Mode and HTML Error Handling (Phase 9.3.8)
/// </summary>
public class UnderConstructionTests
{
    public static void RunTests()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("Running Under Construction Tests");
        Console.WriteLine("========================================");
        Console.WriteLine();

        TestServerConfigurationUnderConstruction();
        TestUnderConstructionHandler();
        TestErrorPageGeneration();
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("All Under Construction Tests Passed ✓");
        Console.WriteLine("========================================");
    }
    
    private static void TestServerConfigurationUnderConstruction()
    {
        Console.WriteLine("Test 1: ServerConfiguration - Under Construction Properties");
        
        var config = new ServerConfiguration();
        
        // Test defaults
        Assert(config.UnderConstruction == false, "UnderConstruction should default to false");
        Assert(config.UnderConstructionMessage == null, "UnderConstructionMessage should default to null");
        Assert(config.UnderConstructionRobotImage == null, "UnderConstructionRobotImage should default to null");
        
        // Test setting properties
        config.UnderConstruction = true;
        Assert(config.UnderConstruction == true, "UnderConstruction should be settable");
        
        config.UnderConstructionMessage = "Custom maintenance message";
        Assert(config.UnderConstructionMessage == "Custom maintenance message", "UnderConstructionMessage should be settable");
        
        config.UnderConstructionRobotImage = "https://example.com/robot.svg";
        Assert(config.UnderConstructionRobotImage == "https://example.com/robot.svg", "UnderConstructionRobotImage should be settable");
        
        Console.WriteLine("  ✓ ServerConfiguration Under Construction properties work correctly");
    }
    
    private static void TestUnderConstructionHandler()
    {
        Console.WriteLine("Test 2: UnderConstructionHandler - Page Generation");
        
        var config = new ServerConfiguration
        {
            UnderConstruction = true
        };
        
        // Test with default message and robot
        var html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
        
        Assert(!string.IsNullOrEmpty(html), "Generated HTML should not be empty");
        Assert(html.Contains("<!DOCTYPE html>"), "HTML should have DOCTYPE declaASHATtion");
        Assert(html.Contains("Under Construction"), "HTML should contain 'Under Construction' title");
        Assert(html.Contains("check back soon"), "HTML should contain default message");
        Assert(html.Contains("data:image/svg+xml"), "HTML should contain default robot SVG");
        Assert(html.Contains("/control-panel"), "HTML should contain admin link");
        
        // Test with custom message
        config.UnderConstructionMessage = "We'll be back in 2 hours!";
        html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
        
        Assert(html.Contains("We'll be back in 2 hours!"), "HTML should contain custom message");
        
        // Test with custom robot image
        config.UnderConstructionRobotImage = "https://example.com/custom-robot.png";
        html = UnderConstructionHandler.GenerateUnderConstructionPage(config);
        
        Assert(html.Contains("https://example.com/custom-robot.png"), "HTML should contain custom robot image URL");
        
        Console.WriteLine("  ✓ UnderConstructionHandler Generates valid HTML");
    }
    
    private static void TestErrorPageGeneration()
    {
        Console.WriteLine("Test 3: UnderConstructionHandler - Error Page Generation");
        
        // Test 404 error page
        var html404 = UnderConstructionHandler.GenerateErrorPage(404, "Page Not Found", "The page you're looking for doesn't exist.");
        
        Assert(!string.IsNullOrEmpty(html404), "Generated error HTML should not be empty");
        Assert(html404.Contains("<!DOCTYPE html>"), "Error HTML should have DOCTYPE declaASHATtion");
        Assert(html404.Contains("404"), "Error HTML should contain error code");
        Assert(html404.Contains("Page Not Found"), "Error HTML should contain error title");
        Assert(html404.Contains("doesn't exist"), "Error HTML should contain error message");
        Assert(html404.Contains("/control-panel"), "Error HTML should contain control panel link by default");
        
        // Test 500 error page without control panel link
        var html500 = UnderConstructionHandler.GenerateErrorPage(500, "Internal Server Error", "Something went wrong on our end.", false);
        
        Assert(html500.Contains("500"), "Error HTML should contain error code");
        Assert(html500.Contains("Internal Server Error"), "Error HTML should contain error title");
        Assert(!html500.Contains("/control-panel"), "Error HTML should not contain control panel link when disabled");
        
        // Test styling consistency
        Assert(html404.Contains("linear-Gradient"), "Error page should have styled background");
        Assert(html404.Contains(".container"), "Error page should have container class");
        
        Console.WriteLine("  ✓ UnderConstructionHandler Generates valid error pages");
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
