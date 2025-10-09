# Memory Management Features

## Overview

RaOS Memory Module now includes comprehensive memory management features to prevent unbounded growth and ensure efficient resource utilization.

## Features

### 1. Automatic Data Retention Policy

- **Default Max Age**: 90 days
- **Behavior**: Items older than the threshold are automatically pruned during maintenance
- **Configuration**: Set via `_maxAge` field in MemoryModule

### 2. Item Limit Enforcement

- **Default Max Items**: 10,000
- **Behavior**: When the limit is reached, oldest items are removed first (FIFO)
- **Automatic Trigger**: Enforced when storage reaches 90% capacity
- **Configuration**: Set via `_maxItems` field in MemoryModule

### 3. Deduplication

- **Purpose**: Removes duplicate key-value pairs
- **Strategy**: Keeps the most recent entry when duplicates are found
- **Usage**: Manual via command or automatic during maintenance

### 4. Statistics & Monitoring

- **Command**: `stats`
- **Information Provided**:
  - Total item count
  - Oldest item date
  - Newest item date
  - Average age of items

## Commands

The Memory module supports the following commands via the `Process()` method:

```csharp
// Store a key-value pair
memory.Process("store mykey myvalue");

// Recall a value
memory.Process("recall mykey");

// Get item count
memory.Process("count");

// Get detailed statistics
memory.Process("stats");

// Manually prune old items
memory.Process("prune");

// Manually remove duplicates
memory.Process("deduplicate");

// Run full maintenance (prune + deduplicate + enforce limit)
memory.Process("maintenance");

// Show help
memory.Process("help");
```

## Programmatic API

### PruneOldItems

```csharp
// Use default max age (90 days)
memory.PruneOldItems();

// Custom max age
memory.PruneOldItems(TimeSpan.FromDays(30));
```

### DeduplicateItems

```csharp
// Remove all duplicate entries
memory.DeduplicateItems();
```

### EnforceItemLimit

```csharp
// Use default limit (10,000)
memory.EnforceItemLimit();

// Custom limit
memory.EnforceItemLimit(5000);
```

### PerformMaintenance

```csharp
// Run all cleanup tasks
memory.PerformMaintenance();
```

### GetStats

```csharp
// Get statistics string
string stats = memory.GetStats();
Console.WriteLine(stats);
// Output: "Memory stats: 1234 items, oldest: 2024-01-15, newest: 2024-12-20, avg age: 45.3 days"
```

## Automatic Maintenance

The MemoryModule performs automatic maintenance checks:

1. **During Storage**: Every 100th item stored triggers a capacity check
2. **Threshold**: When storage reaches 90% of max capacity
3. **Action**: Automatically enforces item limit by removing oldest items

### Scheduled Maintenance Service

For production environments, use the `MemoryMaintenanceService` background service to run periodic maintenance:

```csharp
// In your Program.cs or Startup.cs
services.AddSingleton<MemoryModule>();
services.AddHostedService<MemoryMaintenanceService>();

// Custom interval (default is 24 hours)
services.AddHostedService(sp => 
    new MemoryMaintenanceService(
        sp.GetRequiredService<ILogger<MemoryMaintenanceService>>(),
        sp.GetRequiredService<MemoryModule>(),
        TimeSpan.FromHours(12) // Run every 12 hours
    )
);
```

The service will:
- Run maintenance automatically on schedule
- Log before/after statistics
- Continue running even if errors occur
- Gracefully stop when application shuts down

## Diagnostics Events

All maintenance operations raise diagnostic events via `MemoryDiagnostics`:

```csharp
MemoryDiagnostics.OnMemoryEvent += (message) => {
    Console.WriteLine($"[Memory] {message}");
};

// Events:
// - "Pruned X old memory items (older than YYYY-MM-DD)"
// - "Deduplicated X memory items"
// - "Enforced item limit: removed X oldest items"
// - "Memory maintenance completed"
```

## Configuration

To customize the default behavior, modify the MemoryModule constructor:

```csharp
// Current defaults:
private readonly TimeSpan _maxAge = TimeSpan.FromDays(90);
private readonly int _maxItems = 10000;

// To change defaults, update these values in MemoryModule.cs
```

## Best Practices

1. **Regular Maintenance**: Run `PerformMaintenance()` periodically (e.g., daily) via scheduled task
2. **Monitor Stats**: Use `GetStats()` to track memory usage trends
3. **Adjust Limits**: Set appropriate limits based on your use case and available resources
4. **Event Logging**: Subscribe to `MemoryDiagnostics` events for operational visibility

## Testing

A comprehensive test suite is available in `RaCore/Tests/MemoryManagementTests.cs`:

```csharp
// Run all tests
MemoryManagementTests.RunAll();
```

Tests cover:
- Pruning old items
- Deduplication
- Item limit enforcement
- Automatic maintenance
- Stats command

## Performance Considerations

- **Pruning**: O(n) where n = number of items to delete
- **Deduplication**: O(n log n) due to grouping and sorting
- **Enforce Limit**: O(1) for count check + O(n) for deletion where n = items to remove
- **Get Stats**: O(n) where n = total items (requires full table scan)

For optimal performance:
- Run maintenance during low-traffic periods
- Consider adding database indexes on `CreatedAt` column for faster pruning
- Use `count` command (O(1)) instead of `stats` for quick checks

## Migration Notes

Existing MemoryModule users can adopt these features without breaking changes:
- All new methods are additive
- Existing API remains unchanged
- Automatic maintenance is opt-in (triggered only at 90% capacity)
- Manual commands are optional

## Future Enhancements

Potential improvements:
- Configurable maintenance schedule
- Importance-based retention (keep high-importance items longer)
- Compression for old items
- Archive to cold storage instead of deletion
- Configurable automatic maintenance triggers
