using System.Text.Json;
using Abstractions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace RaCore.Engine.Memory
{
    [RaModule]
    public class MemoryModule : ModuleBase, IMemory
    {
        public override string Name => "Memory";
        public static string Description => "Agentic persistent memory store with async, natural language interface.";
        public static IReadOnlyList<string> Capabilities => [
            "store", "recall", "list", "remove", "clear", "agentic", "async", "multi-language", "metadata"
        ];

        private readonly string _dbPath;
        private readonly string _connectionString;
        
        // Memory management configuration
        private readonly TimeSpan _maxAge = TimeSpan.FromDays(90); // Default: keep items for 90 days
        private readonly int _maxItems = 10000; // Default: maximum 10000 items
        
        // Configuration accessors for monitoring
        public TimeSpan MaxAge => _maxAge;
        public int MaxItems => _maxItems;
        public string DatabasePath => _dbPath;

        public MemoryModule(string? dbPath = null)
        {
            Batteries_V2.Init();
            _dbPath = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(AppContext.BaseDirectory, "ra_memory.sqlite")
                : dbPath;
            _connectionString = $"Data Source={_dbPath}";
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS MemoryItems (
    Id TEXT PRIMARY KEY,
    Key TEXT,
    Value TEXT,
    CreatedAt TEXT,
    Metadata TEXT
);";
            cmd.ExecuteNonQuery();
        }

        public Task<ModuleResponse> RememberAsync(string key, string value, Dictionary<string, string>? metadata = null)
        {
            var item = new MemoryItem
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                Metadata = metadata ?? []
            };

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO MemoryItems (Id, Key, Value, CreatedAt, Metadata)
VALUES ($id, $key, $value, $createdAt, $metadata);";
            cmd.Parameters.AddWithValue("$id", item.Id.ToString());
            cmd.Parameters.AddWithValue("$key", item.Key ?? "");
            cmd.Parameters.AddWithValue("$value", item.Value ?? "");
            cmd.Parameters.AddWithValue("$createdAt", item.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$metadata", JsonSerializer.Serialize(item.Metadata));

            cmd.ExecuteNonQuery();

            MemoryDiagnostics.RaiseMemoryStored(item);

            // Periodic maintenance: check every 100 items if we need cleanup
            if (item.Id.GetHashCode() % 100 == 0)
            {
                var count = Count();
                if (count > _maxItems * 0.9) // If approaching limit, enforce it
                {
                    EnforceItemLimit();
                }
            }

            string summary = $"Memory stored: \"{key}\" = \"{value}\".";
            return Task.FromResult(new ModuleResponse { Text = summary, Type = "memory.store", Status = "ok" });
        }

        public async Task<ModuleResponse> RememberAsync(string key, string value) => await RememberAsync(key, value, null);

        public async Task<ModuleResponse> RecallAsync(string key)
        {
            await Task.CompletedTask;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT Id, Value, CreatedAt, Metadata FROM MemoryItems
WHERE Key = $key
ORDER BY CreatedAt DESC
LIMIT 1;";
            cmd.Parameters.AddWithValue("$key", key);

            using var reader = cmd.ExecuteReader();
            string? result = null;
            Dictionary<string, string>? metadata = null;
            Guid? id = null;
            DateTime? createdAt = null;
            if (reader.Read())
            {
                id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id")));
                result = reader.GetString(reader.GetOrdinal("Value"));
                createdAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")));
                var metadataJson = reader.GetString(reader.GetOrdinal("Metadata"));
                if (!string.IsNullOrWhiteSpace(metadataJson))
                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
            }

            string summary = result != null
                ? $"Memory recall: \"{key}\" = \"{result}\"."
                : $"No memory found for \"{key}\".";

            if (result != null)
                MemoryDiagnostics.RaiseEvent($"Recalled memory: {key}={result}");

            return new ModuleResponse { Text = summary, Type = "memory.recall", Status = result != null ? "ok" : "error" };
        }

        public async Task<ModuleResponse> GetAllItemsAsync()
        {
            await Task.CompletedTask;
            var items = GetAllItems().ToList();
            string summary = items.Count == 0
                ? "Memory is empty."
                : $"Memory contains {items.Count} items. " + string.Join("; ", items.Select(i => $"{i.Key}={i.Value}"));

            return new ModuleResponse { Text = summary, Type = "memory.list", Status = "ok" };
        }

        public IEnumerable<MemoryItem> GetAllItems()
        {
            var items = new List<MemoryItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Key, Value, CreatedAt, Metadata FROM MemoryItems;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var metadataJson = reader.GetString(reader.GetOrdinal("Metadata"));
                Dictionary<string, string>? metadata = null;
                if (!string.IsNullOrWhiteSpace(metadataJson))
                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);

                items.Add(new MemoryItem
                {
                    Id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id"))),
                    Key = reader.GetString(reader.GetOrdinal("Key")),
                    Value = reader.GetString(reader.GetOrdinal("Value")),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                    Metadata = metadata ?? []
                });
            }
            return items;
        }

        public MemoryItem? GetItemById(Guid id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Key, Value, CreatedAt, Metadata FROM MemoryItems WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var metadataJson = reader.GetString(reader.GetOrdinal("Metadata"));
                Dictionary<string, string>? metadata = null;
                if (!string.IsNullOrWhiteSpace(metadataJson))
                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);

                return new MemoryItem
                {
                    Id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id"))),
                    Key = reader.GetString(reader.GetOrdinal("Key")),
                    Value = reader.GetString(reader.GetOrdinal("Value")),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                    Metadata = metadata ?? []
                };
            }
            return null;
        }

        public bool Remove(Guid id)
        {
            var item = GetItemById(id);
            if (item == null) return false;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MemoryItems WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());
            var affected = cmd.ExecuteNonQuery();
            if (affected > 0) MemoryDiagnostics.RaiseMemoryRemoved(item);
            return affected > 0;
        }

        public bool Remove(string key)
        {
            var item = GetAllItems().FirstOrDefault(i => i.Key == key);
            if (item == null) return false;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MemoryItems WHERE Key = $key;";
            cmd.Parameters.AddWithValue("$key", key);
            var affected = cmd.ExecuteNonQuery();
            if (affected > 0) MemoryDiagnostics.RaiseMemoryRemoved(item);
            return affected > 0;
        }

        public void Clear()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MemoryItems;";
            cmd.ExecuteNonQuery();
        }

        public int Count()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM MemoryItems;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int PruneOldItems(TimeSpan? maxAge = null)
        {
            var cutoff = DateTime.UtcNow - (maxAge ?? _maxAge);
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MemoryItems WHERE CreatedAt < $cutoff;";
            cmd.Parameters.AddWithValue("$cutoff", cutoff.ToString("o"));
            var deleted = cmd.ExecuteNonQuery();
            MemoryDiagnostics.RaiseEvent($"Pruned {deleted} old memory items (older than {cutoff:yyyy-MM-dd})");
            return deleted;
        }

        public int DeduplicateItems()
        {
            var items = GetAllItems().ToList();
            var groups = items.GroupBy(i => new { i.Key, i.Value }).Where(g => g.Count() > 1);
            int removed = 0;

            foreach (var group in groups)
            {
                var orderedItems = group.OrderByDescending(i => i.CreatedAt).ToList();
                // Keep the most recent, remove the rest
                foreach (var item in orderedItems.Skip(1))
                {
                    if (Remove(item.Id))
                        removed++;
                }
            }

            MemoryDiagnostics.RaiseEvent($"Deduplicated {removed} memory items");
            return removed;
        }

        public int EnforceItemLimit(int? maxItems = null)
        {
            var limit = maxItems ?? _maxItems;
            var count = Count();
            
            if (count <= limit) return 0;

            var toRemove = count - limit;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Remove oldest items first
            cmd.CommandText = @"
DELETE FROM MemoryItems 
WHERE Id IN (
    SELECT Id FROM MemoryItems 
    ORDER BY CreatedAt ASC 
    LIMIT $limit
);";
            cmd.Parameters.AddWithValue("$limit", toRemove);
            var deleted = cmd.ExecuteNonQuery();
            MemoryDiagnostics.RaiseEvent($"Enforced item limit: removed {deleted} oldest items");
            return deleted;
        }

        public (int pruned, int deduplicated, int limited) PerformMaintenance()
        {
            var pruned = PruneOldItems();
            var deduplicated = DeduplicateItems();
            var limited = EnforceItemLimit();
            MemoryDiagnostics.RaiseEvent("Memory maintenance completed");
            return (pruned, deduplicated, limited);
        }

        public string GetStats()
        {
            var count = Count();
            var items = GetAllItems().ToList();
            
            if (count == 0)
                return "Memory is empty.";

            var oldest = items.Min(i => i.CreatedAt);
            var newest = items.Max(i => i.CreatedAt);
            var avgAge = items.Average(i => (DateTime.UtcNow - i.CreatedAt).TotalDays);
            
            return $"Memory stats: {count} items, oldest: {oldest:yyyy-MM-dd}, newest: {newest:yyyy-MM-dd}, avg age: {avgAge:F1} days";
        }

        public override string Process(string input)
        {
            // Parse the input for commands, e.g. "store key value"
            var args = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 0)
                return "No command provided.";

            switch (args[0].ToLowerInvariant())
            {
                case "store":
                    if (args.Length == 3)
                    {
                        var result = RememberAsync(args[1], args[2]).Result;
                        return result.Text;
                    }
                    return "Usage: store <key> <value>";

                case "recall":
                    if (args.Length == 2)
                    {
                        var result = RecallAsync(args[1]).Result;
                        return result.Text;
                    }
                    return "Usage: recall <key>";

                case "count":
                    return $"Memory count: {Count()}";

                case "stats":
                    return GetStats();

                case "prune":
                    PruneOldItems();
                    return "Old items pruned successfully.";

                case "deduplicate":
                    DeduplicateItems();
                    return "Duplicate items removed successfully.";

                case "maintenance":
                    PerformMaintenance();
                    return "Memory maintenance completed successfully.";

                case "help":
                    return "Commands: store <key> <value>, recall <key>, count, stats, prune, deduplicate, maintenance, help";

                default:
                    return "Unknown command. Type 'help' for options.";
            }
        }
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}