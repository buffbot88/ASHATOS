using System;
using System.Collections.Generic;
using Abstractions;

namespace RaCore.Engine.Memory
{
    /// <summary>
    /// Diagnostics and event hooks for Memory subsystem; for UI/logging integration.
    /// </summary>
    public static class MemoryDiagnostics
    {
        public static event Action<string>? OnMemoryEvent;
        public static event Action<Exception>? OnMemoryError;
        public static event Action<MemoryItem>? OnMemoryStored;
        public static event Action<MemoryItem>? OnMemoryRemoved;
        public static event Action<BinEntry>? OnBinEntryAdded;

        public static void RaiseEvent(string message)
        {
            OnMemoryEvent?.Invoke(message);
        }

        public static void RaiseError(Exception ex)
        {
            OnMemoryError?.Invoke(ex);
        }

        public static void RaiseMemoryStored(MemoryItem item)
        {
            OnMemoryStored?.Invoke(item);
        }

        public static void RaiseMemoryRemoved(MemoryItem item)
        {
            OnMemoryRemoved?.Invoke(item);
        }

        public static void RaiseBinEntryAdded(BinEntry entry)
        {
            OnBinEntryAdded?.Invoke(entry);
        }
    }
}