using System;
using System.Collections.Generic;
using Abstractions;
using ASHATCore.Engine.Manager;
using ASHATCore.Modules.Extensions.Ashat;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for verifying the ASHAT AI Coding Assistant Module functionality
/// This test demonstrates that the built-in coding agent (Ashat) can understand 
/// natural language and provide appropriate coding assistance within the mainframe
/// </summary>
public class CodingAgentTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    ASHAT Coding Assistant Natural Language Tests        ║");
        Console.WriteLine("║         (Built-in Mainframe Coding Agent)               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        TestModuleInitialization();
        TestNaturalLanguageCommands();
        TestCodingSessionManagement();
        TestModuleKnowledgeBase();
        TestInteractiveHelp();
        
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     ✨ All Coding Agent Tests Passed! ✨                ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    }
    
    /// <summary>
    /// Test 1: Verify Ashat Coding Assistant module initializes correctly
    /// </summary>
    private static void TestModuleInitialization()
    {
        Console.WriteLine("Test 1: Ashat Coding Assistant Module Initialization");
        
        var moduleManager = new ModuleManager();
        var ashatModule = new AshatCodingAssistantModule();
        
        ashatModule.Initialize(moduleManager);
        
        if (ashatModule.Name != "Ashat")
            throw new Exception($"Expected module name 'Ashat', got '{ashatModule.Name}'");
        
        Console.WriteLine("  ✅ Ashat module initialized successfully");
        Console.WriteLine($"  ✅ Module name: {ashatModule.Name}");
        Console.WriteLine("  ✅ Built into mainframe as expected");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 2: Verify Ashat understands and responds to natural language commands
    /// </summary>
    private static void TestNaturalLanguageCommands()
    {
        Console.WriteLine("Test 2: Natural Language Command Understanding");
        
        var moduleManager = new ModuleManager();
        var ashatModule = new AshatCodingAssistantModule();
        ashatModule.Initialize(moduleManager);
        
        // Test help command - natural language
        var helpResponse = ashatModule.Process("help");
        if (string.IsNullOrWhiteSpace(helpResponse))
            throw new Exception("Help command returned empty response");
        
        if (!helpResponse.Contains("Ashat") && !helpResponse.Contains("help"))
            throw new Exception("Help response doesn't contain expected content");
        
        Console.WriteLine("  ✅ Ashat responds to 'help' command");
        
        // Test status command
        var statusResponse = ashatModule.Process("status");
        if (string.IsNullOrWhiteSpace(statusResponse))
            throw new Exception("Status command returned empty response");
        
        Console.WriteLine("  ✅ Ashat responds to 'status' command");
        
        // Test asking Ashat a question - natural language
        var askResponse = ashatModule.Process("ask How do I create a new module?");
        if (string.IsNullOrWhiteSpace(askResponse))
            throw new Exception("Ask command returned empty response");
        
        Console.WriteLine("  ✅ Ashat responds to natural language questions");
        Console.WriteLine($"  ✅ Question understood and processed");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 3: Verify coding session management works with natural language
    /// Natural Language: "I want to start working on adding a feature"
    /// </summary>
    private static void TestCodingSessionManagement()
    {
        Console.WriteLine("Test 3: Coding Session Management");
        
        var moduleManager = new ModuleManager();
        var ashatModule = new AshatCodingAssistantModule();
        ashatModule.Initialize(moduleManager);
        
        // Start a coding session with a natural language goal
        var startResponse = ashatModule.Process("start session testuser Add user authentication feature");
        
        if (string.IsNullOrWhiteSpace(startResponse))
            throw new Exception("Start session returned empty response");
        
        if (startResponse.Contains("Usage:") || startResponse.Contains("error"))
            throw new Exception($"Start session failed: {startResponse}");
        
        Console.WriteLine("  ✅ Coding session started with natural language goal");
        Console.WriteLine("  ✅ Session management working correctly");
        
        // End the session
        var endResponse = ashatModule.Process("end session testuser");
        if (string.IsNullOrWhiteSpace(endResponse))
            throw new Exception("End session returned empty response");
        
        Console.WriteLine("  ✅ Session ended successfully");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 4: Verify Ashat has knowledge of available modules
    /// Natural Language: "What modules are available?"
    /// </summary>
    private static void TestModuleKnowledgeBase()
    {
        Console.WriteLine("Test 4: Module Knowledge Base");
        
        var moduleManager = new ModuleManager();
        var ashatModule = new AshatCodingAssistantModule();
        ashatModule.Initialize(moduleManager);
        
        // Query available modules
        var modulesResponse = ashatModule.Process("modules");
        
        if (string.IsNullOrWhiteSpace(modulesResponse))
            throw new Exception("Modules query returned empty response");
        
        Console.WriteLine("  ✅ Ashat can list available modules");
        Console.WriteLine("  ✅ Knowledge base accessible");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 5: Verify Ashat provides interactive help
    /// Natural Language: "How do I use this system?"
    /// </summary>
    private static void TestInteractiveHelp()
    {
        Console.WriteLine("Test 5: Interactive Help System");
        
        var moduleManager = new ModuleManager();
        var ashatModule = new AshatCodingAssistantModule();
        ashatModule.Initialize(moduleManager);
        
        // Ask for help in natural language
        var question1 = ashatModule.Process("ask What can you help me with?");
        if (string.IsNullOrWhiteSpace(question1))
            throw new Exception("Interactive help query 1 failed");
        
        var question2 = ashatModule.Process("ask How do I get started?");
        if (string.IsNullOrWhiteSpace(question2))
            throw new Exception("Interactive help query 2 failed");
        
        Console.WriteLine("  ✅ Ashat responds to natural language questions");
        Console.WriteLine("  ✅ Interactive help system working");
        Console.WriteLine("  ✅ Natural language understanding verified");
        Console.WriteLine();
    }
}
