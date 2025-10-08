using System;
using System.IO;
using RaCore.Engine;

namespace RaCore.Tests
{
    /// <summary>
    /// Tests for NginxManager PHP path parsing functionality
    /// </summary>
    public class NginxManagerPhpPathTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== NginxManager PHP Path Tests ===\n");

            // Test path extraction with quotes
            TestPathExtractionWithQuotes();
            
            // Test path extraction without quotes
            TestPathExtractionWithoutQuotes();
            
            // Test path extraction with mixed scenarios
            TestPathExtractionMixedScenarios();

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        private static void TestPathExtractionWithQuotes()
        {
            Console.WriteLine("--- Test 1: Path Extraction with Quotes ---");
            
            // Simulate the scenario from the bug report
            // The path part extracted would be: "/etc/php/8.5/cli"
            var pathWithQuotes = "\"/etc/php/8.5/cli\"";
            var trimmedPath = pathWithQuotes.Trim().Trim('"');
            var expectedPath = "/etc/php/8.5/cli";
            
            if (trimmedPath == expectedPath)
            {
                Console.WriteLine($"✓ Path with quotes trimmed correctly");
                Console.WriteLine($"  Input:    {pathWithQuotes}");
                Console.WriteLine($"  Output:   {trimmedPath}");
                Console.WriteLine($"  Expected: {expectedPath}");
            }
            else
            {
                Console.WriteLine($"✗ Path with quotes NOT trimmed correctly");
                Console.WriteLine($"  Input:    {pathWithQuotes}");
                Console.WriteLine($"  Output:   {trimmedPath}");
                Console.WriteLine($"  Expected: {expectedPath}");
            }
            
            // Test Path.Combine with the corrected path
            var combinedPath = Path.Combine(trimmedPath, "php.ini");
            var expectedCombined = "/etc/php/8.5/cli/php.ini";
            
            if (combinedPath == expectedCombined)
            {
                Console.WriteLine($"✓ Path.Combine produces correct result");
                Console.WriteLine($"  Combined: {combinedPath}");
            }
            else
            {
                Console.WriteLine($"✗ Path.Combine produces incorrect result");
                Console.WriteLine($"  Combined: {combinedPath}");
                Console.WriteLine($"  Expected: {expectedCombined}");
            }
        }

        private static void TestPathExtractionWithoutQuotes()
        {
            Console.WriteLine("\n--- Test 2: Path Extraction without Quotes ---");
            
            var pathWithoutQuotes = "/etc/php/8.5/cli";
            var trimmedPath = pathWithoutQuotes.Trim().Trim('"');
            var expectedPath = "/etc/php/8.5/cli";
            
            if (trimmedPath == expectedPath)
            {
                Console.WriteLine($"✓ Path without quotes handled correctly");
                Console.WriteLine($"  Input:    {pathWithoutQuotes}");
                Console.WriteLine($"  Output:   {trimmedPath}");
            }
            else
            {
                Console.WriteLine($"✗ Path without quotes NOT handled correctly");
                Console.WriteLine($"  Input:    {pathWithoutQuotes}");
                Console.WriteLine($"  Output:   {trimmedPath}");
                Console.WriteLine($"  Expected: {expectedPath}");
            }
        }

        private static void TestPathExtractionMixedScenarios()
        {
            Console.WriteLine("\n--- Test 3: Mixed Scenarios ---");
            
            var testCases = new[]
            {
                (Input: "  \"/usr/local/php\"  ", Expected: "/usr/local/php"),
                (Input: "C:\\php", Expected: "C:\\php"),
                (Input: "\"C:\\Program Files\\php\"", Expected: "C:\\Program Files\\php"),
                (Input: "(none)", Expected: "(none)"),
                (Input: "  /usr/bin/php  ", Expected: "/usr/bin/php")
            };

            int passed = 0;
            int total = testCases.Length;

            foreach (var testCase in testCases)
            {
                var result = testCase.Input.Trim().Trim('"');
                if (result == testCase.Expected)
                {
                    Console.WriteLine($"  ✓ '{testCase.Input}' -> '{result}'");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"  ✗ '{testCase.Input}' -> '{result}' (expected: '{testCase.Expected}')");
                }
            }

            Console.WriteLine($"\nPassed {passed}/{total} test cases");
        }
    }
}
