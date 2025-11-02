using System.Text.Json;
using Abstractions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace ASHATCore.Engine.Settings
{
    /// <summary>
    /// Persistent settings storage for control panel and modules using SQLite.
    /// Provides a centralized settings database for storing configuration across all modules.
    /// </summary>
    [RaModule]
    public class SettingsModule : ModuleBase
    {
        public override string Name => "Settings";
        
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SettingsModule(string? dbPath = null)
        {
            Batteries_V2.Init();
            _dbPath = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(AppContext.BaseDirectory, "ASHAT_settings.sqlite")
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
CREATE TABLE IF NOT EXISTS ModuleSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ModuleName TEXT NOT NULL,
    SettingKey TEXT NOT NULL,
    SettingValue TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    UNIQUE(ModuleName, SettingKey)
);

CREATE TABLE IF NOT EXISTS ControlPanelStats (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StatKey TEXT NOT NULL UNIQUE,
    StatValue TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get a setting value for a specific module
        /// </summary>
        public string? GetSetting(string moduleName, string settingKey)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT SettingValue FROM ModuleSettings 
WHERE ModuleName = $moduleName AND SettingKey = $settingKey;";
            cmd.Parameters.AddWithValue("$moduleName", moduleName);
            cmd.Parameters.AddWithValue("$settingKey", settingKey);
            
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        /// <summary>
        /// Set a setting value for a specific module
        /// </summary>
        public void SetSetting(string moduleName, string settingKey, string settingValue)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ModuleSettings (ModuleName, SettingKey, SettingValue, UpdatedAt)
VALUES ($moduleName, $settingKey, $settingValue, $updatedAt)
ON CONFLICT(ModuleName, SettingKey) 
DO UPDATE SET SettingValue = $settingValue, UpdatedAt = $updatedAt;";
            cmd.Parameters.AddWithValue("$moduleName", moduleName);
            cmd.Parameters.AddWithValue("$settingKey", settingKey);
            cmd.Parameters.AddWithValue("$settingValue", settingValue);
            cmd.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get all settings for a specific module as a dictionary
        /// </summary>
        public Dictionary<string, string> GetModuleSettings(string moduleName)
        {
            var settings = new Dictionary<string, string>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT SettingKey, SettingValue FROM ModuleSettings 
WHERE ModuleName = $moduleName;";
            cmd.Parameters.AddWithValue("$moduleName", moduleName);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                settings[reader.GetString(0)] = reader.GetString(1);
            }
            return settings;
        }

        /// <summary>
        /// Set multiple settings for a module at once
        /// </summary>
        public void SetModuleSettings(string moduleName, Dictionary<string, string> settings)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            
            try
            {
                foreach (var kvp in settings)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
INSERT INTO ModuleSettings (ModuleName, SettingKey, SettingValue, UpdatedAt)
VALUES ($moduleName, $settingKey, $settingValue, $updatedAt)
ON CONFLICT(ModuleName, SettingKey) 
DO UPDATE SET SettingValue = $settingValue, UpdatedAt = $updatedAt;";
                    cmd.Parameters.AddWithValue("$moduleName", moduleName);
                    cmd.Parameters.AddWithValue("$settingKey", kvp.Key);
                    cmd.Parameters.AddWithValue("$settingValue", kvp.Value);
                    cmd.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("o"));
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Update a control panel stat (e.g., total users, active sessions)
        /// </summary>
        public void UpdateStat(string statKey, string statValue)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ControlPanelStats (StatKey, StatValue, UpdatedAt)
VALUES ($statKey, $statValue, $updatedAt)
ON CONFLICT(StatKey) 
DO UPDATE SET StatValue = $statValue, UpdatedAt = $updatedAt;";
            cmd.Parameters.AddWithValue("$statKey", statKey);
            cmd.Parameters.AddWithValue("$statValue", statValue);
            cmd.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get a control panel stat value
        /// </summary>
        public string? GetStat(string statKey)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT StatValue FROM ControlPanelStats 
WHERE StatKey = $statKey;";
            cmd.Parameters.AddWithValue("$statKey", statKey);
            
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        /// <summary>
        /// Get all control panel stats
        /// </summary>
        public Dictionary<string, string> GetAllStats()
        {
            var stats = new Dictionary<string, string>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT StatKey, StatValue FROM ControlPanelStats;";
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                stats[reader.GetString(0)] = reader.GetString(1);
            }
            return stats;
        }

        public override string Process(string input)
        {
            return "Settings module: use API endpoints to manage settings";
        }
    }
}
