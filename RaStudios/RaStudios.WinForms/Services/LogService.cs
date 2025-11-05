using System;
using System.Collections.Concurrent;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Services
{
    /// <summary>
    /// Centralized logging service for the application.
    /// Thread-safe singleton implementation.
    /// </summary>
    public class LogService
    {
        private static readonly Lazy<LogService> instance = new Lazy<LogService>(() => new LogService());
        private readonly ConcurrentQueue<LogEntry> logQueue = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public static LogService Instance => instance.Value;

        public event EventHandler<LogEntry>? LogAdded;

        private LogService()
        {
        }

        public void LogDebug(string source, string message)
        {
            AddLog(LogLevel.Debug, source, message);
        }

        public void LogInfo(string source, string message)
        {
            AddLog(LogLevel.Info, source, message);
        }

        public void LogWarning(string source, string message)
        {
            AddLog(LogLevel.Warning, source, message);
        }

        public void LogError(string source, string message, Exception? exception = null)
        {
            AddLog(LogLevel.Error, source, message, exception);
        }

        public void LogCritical(string source, string message, Exception? exception = null)
        {
            AddLog(LogLevel.Critical, source, message, exception);
        }

        private void AddLog(LogLevel level, string source, string message, Exception? exception = null)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Source = source,
                Message = message,
                Exception = exception?.ToString()
            };

            logQueue.Enqueue(logEntry);

            // Trim old logs
            while (logQueue.Count > MaxLogEntries)
            {
                logQueue.TryDequeue(out _);
            }

            LogAdded?.Invoke(this, logEntry);

            // Also write to console for debugging
            Console.WriteLine($"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{level}] {source}: {message}");
            if (exception != null)
            {
                Console.WriteLine($"  Exception: {exception}");
            }
        }

        public LogEntry[] GetAllLogs()
        {
            return logQueue.ToArray();
        }

        public void ClearLogs()
        {
            while (logQueue.TryDequeue(out _)) { }
        }
    }
}
