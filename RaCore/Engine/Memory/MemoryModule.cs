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

                case "help":
                    return "Commands: store <key> <value>, recall <key>, count, help";

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