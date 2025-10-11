using ASHATCore.Tests;
using System;
using System.Threading.Tasks;

namespace ASHATCore.Tests;

/// <summary>
/// Comprehensive test runner for memory hygiene, observability, and soak tests.
/// Runs all tests required for Security Gate #235 validation.
/// </summary>
public static class MemoryHygieneTestRunner
{
    public static async Task RunAll()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Memory/Data Hygiene Comprehensive Test Suite                ║");
        Console.WriteLine("║  Security Gate #235 Validation                                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        var startTime = DateTime.UtcNow;
        var passed = 0;
        var failed = 0;
        
        try
        {
            // Run basic memory management tests
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ PHASE 1: Basic Memory Management Tests                     │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            MemoryManagementTests.RunAll();
            passed++;
            Console.WriteLine("✓ Phase 1 Complete\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Phase 1 Failed: {ex.Message}\n");
            failed++;
        }
        
        try
        {
            // Run observability tests
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ PHASE 2: Observability & Monitoring Tests                  │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            await MemoryObservabilityTests.RunAll();
            passed++;
            Console.WriteLine("✓ Phase 2 Complete\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Phase 2 Failed: {ex.Message}\n");
            failed++;
        }
        
        try
        {
            // Run soak tests
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ PHASE 3: Soak Tests (Long-Running Validation)              │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
            Console.WriteLine("⚠ Note: This phase will take 30+ seconds to complete");
            await MemorySoakTests.RunAll();
            passed++;
            Console.WriteLine("✓ Phase 3 Complete\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Phase 3 Failed: {ex.Message}\n");
            failed++;
        }
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        
        // Print summary
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Test Summary                                                 ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Total Phases: {passed + failed}");
        Console.WriteLine($"Passed: {passed}");
        Console.WriteLine($"Failed: {failed}");
        Console.WriteLine($"duration: {duration.TotalSeconds:F1} seconds");
        Console.WriteLine();
        
        if (failed == 0)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ✓ ALL TESTS PASSED                                          ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║  Memory hygiene controls are working correctly.               ║");
            Console.WriteLine("║  System is ready for Security Gate #235 validation.          ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        }
        else
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ✗ SOME TESTS FAILED                                         ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║  Please review failures and fix issues before proceeding.    ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        }
        
        Console.WriteLine();
        Console.WriteLine("Evidence Collection:");
        Console.WriteLine("  - Run this test suite and save output to evidence/test_results.txt");
        Console.WriteLine("  - Review evidence collection checklist in docs/MEMORY_HYGIENE_EVIDENCE.md");
        Console.WriteLine("  - Collect metrics from production environment");
        Console.WriteLine("  - Document Configuration and thresholds");
        Console.WriteLine();
    }
}
