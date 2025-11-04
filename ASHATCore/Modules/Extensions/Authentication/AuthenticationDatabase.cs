using System.Text.Json;
using Abstractions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace ASHATCore.Modules.Extensions.Authentication;

/// <summary>
/// Database persistence layer for Authentication module.
/// Provides SQLite storage for users and sessions across server restarts.
/// </summary>
public class AuthenticationDatabase : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public AuthenticationDatabase(string? dbPath = null)
    {
        Batteries_V2.Init();
        _dbPath = string.IsNullOrWhiteSpace(dbPath)
            ? Path.Combine(AppContext.BaseDirectory, "Databases", "authentication.sqlite")
            : dbPath;
        
        // Ensure Databases directory exists
        var directory = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={_dbPath}";
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY,
    Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
    Email TEXT,
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    Role INTEGER NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    LastLoginUtc TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Sessions (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Token TEXT NOT NULL UNIQUE,
    CreatedAtUtc TEXT NOT NULL,
    ExpiresAtUtc TEXT NOT NULL,
    LastActivityUtc TEXT NOT NULL,
    IpAddress TEXT,
    UserAgent TEXT,
    IsValid INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_sessions_token ON Sessions(Token);
CREATE INDEX IF NOT EXISTS idx_sessions_userid ON Sessions(UserId);
CREATE INDEX IF NOT EXISTS idx_sessions_expiresAt ON Sessions(ExpiresAtUtc);
";
        cmd.ExecuteNonQuery();
        
        // Add LastActivityUtc column to existing Sessions table if it doesn't exist
        cmd.CommandText = @"
PRAGMA table_info(Sessions);
";
        using var reader = cmd.ExecuteReader();
        bool hasLastActivityUtc = false;
        while (reader.Read())
        {
            var columnName = reader.GetString(1);
            if (columnName == "LastActivityUtc")
            {
                hasLastActivityUtc = true;
                break;
            }
        }
        reader.Close();
        
        if (!hasLastActivityUtc)
        {
            cmd.CommandText = @"
ALTER TABLE Sessions ADD COLUMN LastActivityUtc TEXT NOT NULL DEFAULT '';
UPDATE Sessions SET LastActivityUtc = CreatedAtUtc WHERE LastActivityUtc = '';
";
            cmd.ExecuteNonQuery();
        }
    }

    #region User Operations

    public void SaveUser(User user)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = @"
INSERT OR REPLACE INTO Users (Id, Username, Email, PasswordHash, PasswordSalt, Role, CreatedAtUtc, LastLoginUtc, IsActive)
VALUES ($id, $username, $email, $passwordHash, $passwordSalt, $role, $createdAtUtc, $lastLoginUtc, $isActive);";
        
        cmd.Parameters.AddWithValue("$id", user.Id.ToString());
        cmd.Parameters.AddWithValue("$username", user.Username);
        cmd.Parameters.AddWithValue("$email", user.Email ?? "");
        cmd.Parameters.AddWithValue("$passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("$passwordSalt", user.PasswordSalt);
        cmd.Parameters.AddWithValue("$role", (int)user.Role);
        cmd.Parameters.AddWithValue("$createdAtUtc", user.CreatedAtUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$lastLoginUtc", user.LastLoginUtc?.ToString("o") ?? "");
        cmd.Parameters.AddWithValue("$isActive", user.IsActive ? 1 : 0);
        
        cmd.ExecuteNonQuery();
    }

    public User? GetUserById(Guid userId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "SELECT * FROM Users WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", userId.ToString());
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return ReadUser(reader);
        }
        return null;
    }

    public User? GetUserByUsername(string username)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "SELECT * FROM Users WHERE Username = $username COLLATE NOCASE;";
        cmd.Parameters.AddWithValue("$username", username);
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return ReadUser(reader);
        }
        return null;
    }

    public List<User> GetAllUsers()
    {
        var users = new List<User>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "SELECT * FROM Users ORDER BY CreatedAtUtc DESC;";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(ReadUser(reader));
        }
        return users;
    }

    private static User ReadUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = Guid.Parse(reader.GetString(0)),
            Username = reader.GetString(1),
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
            PasswordHash = reader.GetString(3),
            PasswordSalt = reader.GetString(4),
            Role = (UserRole)reader.GetInt32(5),
            CreatedAtUtc = DateTime.Parse(reader.GetString(6)),
            LastLoginUtc = reader.IsDBNull(7) || string.IsNullOrWhiteSpace(reader.GetString(7)) ? null : DateTime.Parse(reader.GetString(7)),
            IsActive = reader.GetInt32(8) == 1
        };
    }

    #endregion

    #region Session Operations

    public void SaveSession(Session session)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = @"
INSERT OR REPLACE INTO Sessions (Id, UserId, Token, CreatedAtUtc, ExpiresAtUtc, LastActivityUtc, IpAddress, UserAgent, IsValid)
VALUES ($id, $userId, $token, $createdAtUtc, $expiresAtUtc, $lastActivityUtc, $ipAddress, $userAgent, $isValid);";
        
        cmd.Parameters.AddWithValue("$id", session.Id.ToString());
        cmd.Parameters.AddWithValue("$userId", session.UserId.ToString());
        cmd.Parameters.AddWithValue("$token", session.Token);
        cmd.Parameters.AddWithValue("$createdAtUtc", session.CreatedAtUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$expiresAtUtc", session.ExpiresAtUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$lastActivityUtc", session.LastActivityUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$ipAddress", session.IpAddress ?? "");
        cmd.Parameters.AddWithValue("$userAgent", session.UserAgent ?? "");
        cmd.Parameters.AddWithValue("$isValid", session.IsValid ? 1 : 0);
        
        cmd.ExecuteNonQuery();
    }

    public Session? GetSessionByToken(string token)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "SELECT * FROM Sessions WHERE Token = $token;";
        cmd.Parameters.AddWithValue("$token", token);
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return ReadSession(reader);
        }
        return null;
    }

    public List<Session> GetActiveSessions()
    {
        var sessions = new List<Session>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "SELECT * FROM Sessions WHERE IsValid = 1 AND ExpiresAtUtc > $now;";
        cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            sessions.Add(ReadSession(reader));
        }
        return sessions;
    }

    public void InvalidateSession(string token)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "UPDATE Sessions SET IsValid = 0 WHERE Token = $token;";
        cmd.Parameters.AddWithValue("$token", token);
        
        cmd.ExecuteNonQuery();
    }

    public void CleanupExpiredSessions()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = "DELETE FROM Sessions WHERE ExpiresAtUtc < $now;";
        cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.AddDays(-7).ToString("o")); // Keep expired sessions for 7 days for audit
        
        cmd.ExecuteNonQuery();
    }

    private static Session ReadSession(SqliteDataReader reader)
    {
        return new Session
        {
            Id = Guid.Parse(reader.GetString(0)),
            UserId = Guid.Parse(reader.GetString(1)),
            Token = reader.GetString(2),
            CreatedAtUtc = DateTime.Parse(reader.GetString(3)),
            ExpiresAtUtc = DateTime.Parse(reader.GetString(4)),
            LastActivityUtc = reader.IsDBNull(5) || string.IsNullOrWhiteSpace(reader.GetString(5)) 
                ? DateTime.Parse(reader.GetString(3)) // Fall back to CreatedAtUtc if not set
                : DateTime.Parse(reader.GetString(5)),
            IpAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
            UserAgent = reader.IsDBNull(7) ? null : reader.GetString(7),
            IsValid = reader.GetInt32(8) == 1
        };
    }

    #endregion

    public void Dispose()
    {
        // Cleanup any resources if needed
    }
}
