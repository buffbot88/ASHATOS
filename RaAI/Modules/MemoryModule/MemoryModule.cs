using RaAI.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaAI.Modules.MemoryModule
{
    public class MemoryModule : ModuleBase, IMemory
    {
        private readonly Dictionary<Guid, MemoryItem> _memory = new Dictionary<Guid, MemoryItem>();
        private readonly Dictionary<Guid, Candidate> _candidates = new Dictionary<Guid, Candidate>();
        private readonly Dictionary<Guid, ConsciousEntry> _consciousIndex = new Dictionary<Guid, ConsciousEntry>();

        public override string Name => "Memory";

        public MemoryModule()
        {
            LogInfo("Memory module initialized.");
        }

        public Guid Remember(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var item = new MemoryItem
            {
                Id = Guid.NewGuid(),
                Key = key.Trim(),
                Value = value.Trim(),
                CreatedAt = DateTime.UtcNow,
            };
            _memory[item.Id] = item;
            LogInfo($"Remembered: {item.Key}='{item.Value}'");
            return item.Id;
        }

        public string Recall(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var item = _memory.Values.LastOrDefault(i => string.Equals(i.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                LogInfo($"Recalled: {key}='{item.Value}'");
                return item.Value;
            }
            else
            {
                LogInfo($"Key not found in memory: {key}");
                return string.Empty;
            }
        }

        public Guid AddCandidate(string text, Dictionary<string, string> metadata, bool requireConsent)
        {
            var candidate = new Candidate
            {
                CandidateId = Guid.NewGuid(),
                Text = text,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                RequireConsent = requireConsent
            };
            _candidates[candidate.CandidateId] = candidate;
            return candidate.CandidateId;
        }

        public void PromoteCandidateToMemory(Guid candidateId)
        {
            if (_candidates.TryGetValue(candidateId, out var candidate))
            {
                var memoryItem = new MemoryItem
                {
                    Id = candidate.CandidateId,
                    Key = candidate.Text,
                    Value = candidate.Text,
                    CreatedAt = DateTime.UtcNow
                };
                _memory[memoryItem.Id] = memoryItem;
                _candidates.Remove(candidateId);
            }
        }

        public void PromoteToConscious(Guid itemId)
        {
            if (_memory.TryGetValue(itemId, out var memoryItem))
            {
                var consciousEntry = new ConsciousEntry
                {
                    Id = memoryItem.Id,
                    PromotedAt = DateTime.UtcNow
                };
                _consciousIndex[consciousEntry.Id] = consciousEntry;
            }
        }
    }
}