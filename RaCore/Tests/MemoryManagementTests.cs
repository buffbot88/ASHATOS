using RaCore.Engine.Memory;
using Abstractions;

namespace RaCore.Tests;

/// <summary>
/// Tests for memory management features including pruning, deduplication, and limits.
/// </summary>
public static class MemoryManagementTests
{
    public static void RunAll()
    {
        Console.WriteLine("=== Memory Management Tests ===\n");

        TestPruning();
        TestDeduplication();
        TestItemLimit();
        TestAutoMaintenance();
        TestStatsCommand();

        Console.WriteLine("\n=== All Memory Management Tests Completed ===");
    }

    private static void TestPruning()
    {
        Console.WriteLine("Test: Pruning old items");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_prune_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add some items with different ages
            memory.RememberAsync("recent", "value1").Wait();
            memory.RememberAsync("key2", "value2").Wait();
            memory.RememberAsync("key3", "value3").Wait();
            
            var initialCount = memory.Count();
            Console.WriteLine($"  Initial count: {initialCount}");
            
            // Prune items older than 1 second (should remove nothing since items are new)
            memory.PruneOldItems(TimeSpan.FromSeconds(1));
            System.Threading.Thread.Sleep(100);
            
            var afterPrune = memory.Count();
            Console.WriteLine($"  After prune (0 seconds): {afterPrune}");
            
            // Verify no items were removed (they're all recent)
            if (afterPrune == initialCount)
            {
                Console.WriteLine("  ✓ PASS: No recent items were pruned");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Unexpected items were pruned");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }

    private static void TestDeduplication()
    {
        Console.WriteLine("Test: Deduplication");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_dedup_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add duplicate items
            memory.RememberAsync("duplicate", "same_value").Wait();
            System.Threading.Thread.Sleep(10);
            memory.RememberAsync("duplicate", "same_value").Wait();
            System.Threading.Thread.Sleep(10);
            memory.RememberAsync("duplicate", "same_value").Wait();
            memory.RememberAsync("unique", "different_value").Wait();
            
            var beforeDedup = memory.Count();
            Console.WriteLine($"  Before deduplication: {beforeDedup} items");
            
            memory.DeduplicateItems();
            
            var afterDedup = memory.Count();
            Console.WriteLine($"  After deduplication: {afterDedup} items");
            
            // Should have removed 2 duplicates, keeping 1 + 1 unique = 2 total
            if (afterDedup == 2)
            {
                Console.WriteLine("  ✓ PASS: Duplicates removed correctly");
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: Expected 2 items, got {afterDedup}");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }

    private static void TestItemLimit()
    {
        Console.WriteLine("Test: Item limit enforcement");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_limit_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add items exceeding limit
            for (int i = 0; i < 25; i++)
            {
                memory.RememberAsync($"key{i}", $"value{i}").Wait();
                System.Threading.Thread.Sleep(5); // Small delay to ensure different timestamps
            }
            
            var beforeLimit = memory.Count();
            Console.WriteLine($"  Before limit enforcement: {beforeLimit} items");
            
            // Enforce a limit of 10 items
            memory.EnforceItemLimit(10);
            
            var afterLimit = memory.Count();
            Console.WriteLine($"  After limit enforcement (max=10): {afterLimit} items");
            
            if (afterLimit == 10)
            {
                Console.WriteLine("  ✓ PASS: Item limit enforced correctly");
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: Expected 10 items, got {afterLimit}");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }

    private static void TestAutoMaintenance()
    {
        Console.WriteLine("Test: Automatic maintenance");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_auto_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add some items
            for (int i = 0; i < 10; i++)
            {
                memory.RememberAsync($"item{i}", $"value{i}").Wait();
            }
            
            var beforeMaintenance = memory.Count();
            Console.WriteLine($"  Before maintenance: {beforeMaintenance} items");
            
            memory.PerformMaintenance();
            
            var afterMaintenance = memory.Count();
            Console.WriteLine($"  After maintenance: {afterMaintenance} items");
            Console.WriteLine("  ✓ PASS: Maintenance completed without errors");
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }

    private static void TestStatsCommand()
    {
        Console.WriteLine("Test: Stats command");
        
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_stats_{Guid.NewGuid()}.sqlite");
        try
        {
            using var memory = new MemoryModule(dbPath);
            
            // Add some items
            memory.RememberAsync("key1", "value1").Wait();
            memory.RememberAsync("key2", "value2").Wait();
            memory.RememberAsync("key3", "value3").Wait();
            
            var stats = memory.GetStats();
            Console.WriteLine($"  Stats: {stats}");
            
            if (stats.Contains("3 items"))
            {
                Console.WriteLine("  ✓ PASS: Stats display correct item count");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Stats do not show correct count");
            }
            
            // Test Process command
            var processStats = memory.Process("stats");
            Console.WriteLine($"  Process stats: {processStats}");
            
            if (processStats.Contains("3 items"))
            {
                Console.WriteLine("  ✓ PASS: Process command stats work correctly");
            }
            else
            {
                Console.WriteLine("  ✗ FAIL: Process command stats incorrect");
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
        
        Console.WriteLine();
    }
}
