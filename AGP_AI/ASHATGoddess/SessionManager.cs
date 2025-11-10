namespace ASHATGoddessClient.Host
{
    /// <summary>
    /// Manages session state and conversation history
    /// </summary>
    public class SessionManager
    {
        private readonly Configuration.SessionSettings _settings;
        private readonly Dictionary<string, SessionData> _sessions;
        private readonly bool _enableLogging;

        public SessionManager(Configuration.SessionSettings settings, bool enableLogging = true)
        {
            _settings = settings;
            _sessions = new Dictionary<string, SessionData>();
            _enableLogging = enableLogging;
        }

        /// <summary>
        /// Create a new session
        /// </summary>
        public string CreateSession(bool consentGiven = false)
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new SessionData
            {
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ConsentGiven = consentGiven,
                PersistentMemoryEnabled = consentGiven && _settings.PersistentMemory
            };

            _sessions[sessionId] = session;

            if (_enableLogging)
            {
                Console.WriteLine($"[SessionManager] Created new session: {sessionId}");
                Console.WriteLine($"[SessionManager] Persistent memory: {session.PersistentMemoryEnabled}");
            }

            return sessionId;
        }

        /// <summary>
        /// Get session data by ID
        /// </summary>
        public SessionData? GetSession(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.LastAccessedAt = DateTime.UtcNow;
                return session;
            }
            return null;
        }

        /// <summary>
        /// Add a message to the session history
        /// </summary>
        public void AddMessage(string sessionId, string role, string content)
        {
            var session = GetSession(sessionId);
            if (session == null) return;

            session.History.Add(new MessageData
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });

            // Enforce max history length
            while (session.History.Count > _settings.MaxHistoryLength)
            {
                session.History.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get conversation history for a session
        /// </summary>
        public List<MessageData> GetHistory(string sessionId, int limit = 0)
        {
            var session = GetSession(sessionId);
            if (session == null) return new List<MessageData>();

            if (limit > 0 && session.History.Count > limit)
            {
                return session.History.Skip(session.History.Count - limit).ToList();
            }

            return new List<MessageData>(session.History);
        }

        /// <summary>
        /// Update consent status for a session
        /// </summary>
        public void UpdateConsent(string sessionId, bool consentGiven)
        {
            var session = GetSession(sessionId);
            if (session == null) return;

            session.ConsentGiven = consentGiven;
            session.PersistentMemoryEnabled = consentGiven && _settings.PersistentMemory;

            if (_enableLogging)
            {
                Console.WriteLine($"[SessionManager] Session {sessionId} consent updated: {consentGiven}");
                Console.WriteLine($"[SessionManager] Persistent memory: {session.PersistentMemoryEnabled}");
            }
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public void CleanupExpiredSessions()
        {
            var expiredSessions = _sessions
                .Where(kvp => (DateTime.UtcNow - kvp.Value.LastAccessedAt).TotalSeconds > _settings.SessionTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                _sessions.Remove(sessionId);
                if (_enableLogging)
                {
                    Console.WriteLine($"[SessionManager] Removed expired session: {sessionId}");
                }
            }
        }

        /// <summary>
        /// Get active session count
        /// </summary>
        public int GetActiveSessionCount()
        {
            CleanupExpiredSessions();
            return _sessions.Count;
        }
    }

    /// <summary>
    /// Session data storage
    /// </summary>
    public class SessionData
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public bool ConsentGiven { get; set; }
        public bool PersistentMemoryEnabled { get; set; }
        public List<MessageData> History { get; set; } = new();
    }

    /// <summary>
    /// Message data in conversation history
    /// </summary>
    public class MessageData
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
