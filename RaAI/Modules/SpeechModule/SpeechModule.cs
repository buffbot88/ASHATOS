using RaAI.Handlers;
using RaAI.Modules.ConsciousModule;
using RaAI.Modules.SubconsciousModule;
using System;
using System.Text.RegularExpressions;

namespace RaAI.Modules.SpeechModule
{
    public class SpeechModule : ModuleBase, ISpeechModule
    {
        private readonly IMemory _memoryClient;
        private readonly SubconsciousClient _subconsciousClient;

        public SpeechModule(IMemory memoryClient, SubconsciousClient subconsciousClient)
        {
            _memoryClient = memoryClient ?? throw new ArgumentNullException(nameof(memoryClient));
            _subconsciousClient = subconsciousClient ?? throw new ArgumentNullException(nameof(subconsciousClient));
            LogInfo("Speech module initialized.");
        }

        public override string Name => "Speech";

        public async Task<string> GenerateResponseAsync(string input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "(no input)";

            var text = input.Trim();

            // Heuristics -> remember: "X is Y" or "remember X is Y" or "remember X=Y"
            var match = Regex.Match(text, @"^(?:remember\s+)?(.+?)\s*(?:is|=)\s*(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var key = NormalizeKey(match.Groups[1].Value);
                var value = match.Groups[2].Value.Trim();
                return await RememberAsync(key, value, cancellationToken);
            }

            // Recall: "what is X", "what's X", "recall X"
            match = Regex.Match(text, @"^(?:what(?:'s| is)|recall)\s+(.+)\??$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var key = NormalizeKey(match.Groups[1].Value);
                return await RecallAsync(key, cancellationToken);
            }

            // Direct thinking: "think about X" or "think X"
            match = Regex.Match(text, @"^(?:think(?: about)?|ponder)\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                return await ThinkAsync(content, cancellationToken);
            }

            // Probe subconscious
            match = Regex.Match(text, @"^(?:ask\s+subconscious|probe|ask)\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                return await ProbeSubconsciousAsync(content, cancellationToken);
            }

            // Default: Treat as think
            return await ThinkAsync(text, cancellationToken);
        }

        private async Task<string> RememberAsync(string key, string value, CancellationToken cancellationToken)
        {
            // Call the MemoryClient to remember the key-value pair
            await _memoryClient.RememberAsync(key, value, cancellationToken);
            return $"Remembered: {key} = {value}";
        }

        private async Task<string> RecallAsync(string key, CancellationToken cancellationToken)
        {
            // Call the MemoryClient to recall the value associated with the key
            var value = await _memoryClient.RecallAsync(key, cancellationToken);
            return string.IsNullOrEmpty(value) ? $"No memory found for: {key}" : value;
        }

        private static Task<string> ThinkAsync(string content, CancellationToken cancellationToken)
        {
            // Implement the logic to think about the given content
            // This can involve natural language processing, language understanding, and response generation techniques
            // For now, let's return a placeholder response
            return Task.FromResult($"Thought about: {content}");
        }

        private async Task<string> ProbeSubconsciousAsync(string content, CancellationToken cancellationToken)
        {
            // Call the SubconsciousClient to probe the subconscious with the given content
            var response = await _subconsciousClient.ProbeAsync(content, cancellationToken);
            return $"Subconscious response: {response}";
        }

        private static string NormalizeKey(string s) =>
            string.IsNullOrWhiteSpace(s) ? s : s.Trim().Replace(" ", "");
    }
}