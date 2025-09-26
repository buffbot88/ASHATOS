using RaAI.Handlers;
using RaAI.Modules.DigitalFace;
using RaAI.Modules.SubconsciousModule.Core;
using RaAI.UI;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using static RaAI.Modules.DigitalFace.DigitalFaceControl;

namespace RaAI.Modules.ConsciousModule
{
    public sealed class ConsciousModule : ModuleBase, IRaModule
    {
        public override string Name => "Conscious";

        private readonly ModuleManager _manager;
        private readonly MessageBus _bus;
        private readonly MemoryClient _memory;
        // Change the declaration of _sub to nullable to fix CS8618
        private readonly SubconsciousModule.SubconsciousClient? _sub;
        private readonly ThoughtProcessor _processor;
        private readonly ConcurrentQueue<Thought> _thoughtHistory;
        private readonly Lock _historyLock = new();
        private readonly RaAIForm? _uiForm;
        private readonly DigitalFaceControl? _faceControl;
        private readonly Mood _currentMood = Mood.Neutral;

        public ConsciousModule(ModuleManager manager, IContainerStorage containerStorage)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            // Initialize dependencies
            _bus = new MessageBus();

            // With this line:
            _memory = new MemoryClient();
            var subconsciousModule = new SubconsciousModule.SubconsciousModule(containerStorage); // Create an instance of SubconsciousModule directly
            _sub = new SubconsciousModule.SubconsciousClient(subconsciousModule); // assign to _sub
            _sub.SendMessage("Hello, subconscious!");
            _ = _sub.GetResponse();

            _processor = new ThoughtProcessor(
                ConsciousConfig.FeatureVectorSize,
                ConsciousConfig.LearningRate
            );

            // Initialize collections
            _thoughtHistory = new ConcurrentQueue<Thought>();

            // Initialize face
            _faceControl?.SetMood(DigitalFaceControl.Mood.Neutral);

            LogInfo("Conscious module initialized successfully.");
        }

        // Public API methods
        public string Think(string input) => ProcessInput("think", input);
        public string Remember(string key, string value) => ProcessInput("remember", $"{key}={value}");
        public string Recall(string key) => ProcessInput("recall", key);
        public string ProbeSubconscious(string cmd) => ProcessInput("probe", cmd);

        public override string Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "(empty input)";

            var trimmed = input.Trim();
            var firstSpace = trimmed.IndexOf(' ');
            var verb = firstSpace > 0
                ? trimmed[..firstSpace].ToLower()
                : trimmed.ToLower();
            var args = firstSpace > 0
                ? trimmed[(firstSpace + 1)..].Trim()
                : string.Empty;

            return verb switch
            {
                "help" => GetHelpText(),
                "think" => CommandThink(args),
                "remember" => CommandRemember(args),
                "recall" => CommandRecall(args),
                "status" => CommandStatus(),
                "history" => CommandHistory(args),
                "reset" => CommandReset(),
                "train" => CommandTrain(args),
                "probesub" => CommandProbeSub(args),
                "probe" => CommandProbeSub(args),
                _ when Regex.IsMatch(trimmed, @"\w+\s*=\s*.+")
                    => CommandRemember(trimmed),
                _ => CommandThink(trimmed)
            };
        }

        private string ProcessInput(string verb, string args)
        {
            try
            {
                return Process($"{verb} {args}");
            }
            catch (Exception ex)
            {
                LogError($"Input processing error: {ex.Message}");
                return $"(error) {ex.Message}";
            }
        }

        private static string GetHelpText() => @"
            Conscious Module Commands:

            think <input>          - Process input through consciousness
            remember key=value      - Store in memory
            recall key             - Retrieve from memory
            probesub <command>     - Query subconscious
            status                 - Show current status
            history [count]        - View recent thoughts
            reset                  - Clear internal state
            train <id> <reward>    - Reinforce learning
            help                   - This help text";

        private string CommandThink(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "Please provide content to think about.";

            var thought = new Thought
            {
                Id = _processor.GenerateThoughtId(),
                Content = content,
                Timestamp = DateTime.UtcNow,
                Source = "user"
            };

            try
            {
                // Get memory context
                var memoryContext = GetMemorySnapshot(thought.Id);

                // Get subconscious signals
                var subconsciousSignals = GetSubconsciousSignals(thought.Content);

                // Process thought
                var result = _processor.Process(
                    thought,
                    memoryContext,
                    subconsciousSignals,
                    ConsciousConfig.AssociationLimit
                );

                // Update history
                UpdateThoughtHistory(thought);

                // Update UI
                UpdateFacialExpression(result);

                // Log and publish
                LogInfo($"Processed thought #{thought.Id}: {thought.Content}");
                _bus.Publish("thought.processed", thought.ToString());

                return result;
            }
            catch (Exception ex)
            {
                LogError($"Thinking failed: {ex.Message}");
                return $"(thinking error) {ex.Message}";
            }
        }

        private string[] GetMemorySnapshot(int thoughtId)
        {
            if (_memory == null)
                return [];

            try
            {
                var key = $"context_{thoughtId}";
                _memory.Remember(key, DateTime.Now.ToString());
                return [.. _memory.Dump()
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Take(5)];
            }
            catch
            {
                return [];
            }
        }

        private string[] GetSubconsciousSignals(string content)
        {
            if (_sub == null)
                return [];

            try
            {
                return [_sub.Probe(content) ?? string.Empty];
            }
            catch
            {
                return [];
            }
        }

        private void UpdateThoughtHistory(Thought thought)
        {
            _thoughtHistory.Enqueue(thought);

            lock (_historyLock)
            {
                while (_thoughtHistory.Count > ConsciousConfig.ThoughtHistoryLimit)
                {
                    _thoughtHistory.TryDequeue(out _);
                }
            }
        }

        private void UpdateFacialExpression(string processingResult)
        {
            if (_faceControl == null)
                return;

            var mood = DetermineMood(processingResult);
            _faceControl.SetMood(mood);
        }

        private static DigitalFaceControl.Mood DetermineMood(string result)
        {
            if (result.ContainsIgnoreCase("thinking"))
                return DigitalFaceControl.Mood.Thinking;
            if (result.ContainsIgnoreCase("speaking"))
                return DigitalFaceControl.Mood.Speaking;
            if (result.ContainsIgnoreCase("confused"))
                return DigitalFaceControl.Mood.Confused;
            if (result.ContainsIgnoreCase("happy"))
                return DigitalFaceControl.Mood.Happy;
            return DigitalFaceControl.Mood.Neutral;
        }

        // Add the missing command methods
        private string CommandHistory(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                // Return all history
                return string.Join("\n", _thoughtHistory.Select(t => $"{t.Id}: {t.Content}"));
            }

            if (int.TryParse(args, out int count))
            {
                // Return last N thoughts
                return string.Join("\n", _thoughtHistory
                    .Take(count)
                    .Select(t => $"{t.Id}: {t.Content}"));
            }

            return "Invalid history count. Please provide a number.";
        }

        private string CommandTrain(string args)
        {
            var parts = args.Split(' ', 2);
            if (parts.Length < 2 || !int.TryParse(parts[0], out int id))
            {
                return "Invalid train command. Usage: train <id> <reward>";
            }

            if (!double.TryParse(parts[1], out double reward))
            {
                return "Invalid reward value. Please provide a number.";
            }

            // Find the thought by ID
            var thought = _thoughtHistory.FirstOrDefault(t => t.Id == id);
            if (thought == null)
            {
                return $"Thought with ID {id} not found.";
            }

            _processor.Train(thought, reward);
            return $"Trained thought #{id} with reward {reward}.";
        }

        private string CommandStatus()
        {
            var status = new StringBuilder();
            status.AppendLine($"Conscious Module Status:");
            status.AppendLine($"- Current Mood: {_currentMood}");
            status.AppendLine($"- Thoughts in History: {_thoughtHistory.Count}");
            status.AppendLine($"- Memory Entries: {_memory?.Count() ?? 0}");
            status.AppendLine($"- Subconscious Available: {_sub != null}");

            return status.ToString();
        }

        private string CommandReset()
        {
            lock (_historyLock)
            {
                _thoughtHistory.Clear();
            }

            _memory?.Clear();

            _faceControl?.SetMood(DigitalFaceControl.Mood.Neutral);

            return "Conscious module state has been reset.";
        }

        private string CommandRemember(string args)
        {
            var parts = args.Split('=', 2);
            if (parts.Length < 2)
            {
                return "Invalid remember command. Usage: remember key=value";
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            _memory?.Remember(key, value);
            return $"Remembered: {key} = {value}";
        }

        private string CommandRecall(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "Please provide a key to recall.";
            }

            var value = _memory?.Recall(key);
            return value ?? $"No value found for key: {key}";
        }

        private string CommandProbeSub(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return "Please provide a command to probe the subconscious.";
            }

            var result = _sub?.Probe(args) ?? "Subconscious not available.";
            return result;
        }
    }
}