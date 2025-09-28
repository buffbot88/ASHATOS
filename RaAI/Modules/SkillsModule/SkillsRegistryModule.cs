using RaAI.Handlers.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RaAI.Modules.SkillsModule
{
    // [RaModule("Skills")]
    public sealed class SkillRegistryModule : ModuleBase
    {
        public override string Name => "Skills";

        private readonly Dictionary<string, ISkill> _skills = new(StringComparer.OrdinalIgnoreCase);

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            // Discover all modules that also implement ISkill
            foreach (var w in manager.Modules)
                if (w.Instance is ISkill s)
                    _skills[s.Name] = s;

            LogInfo($"Skills registered: {string.Join(", ", _skills.Keys)}");
        }

        public IReadOnlyCollection<ISkill> List() => _skills.Values;
        public ISkill? Get(string name) => _skills.TryGetValue(name, out var s) ? s : null;

        public override string Process(string input)
        {
            var t = (input ?? "").Trim();
            if (t.Equals("help", StringComparison.OrdinalIgnoreCase))
                return "Skills commands:\n  skills list\n  skills describe <name>";

            if (t.Equals("skills list", StringComparison.OrdinalIgnoreCase))
                return string.Join("\n", _skills.Keys.OrderBy(k => k));

            if (t.StartsWith("skills describe ", StringComparison.OrdinalIgnoreCase))
            {
                var name = t["skills describe ".Length..].Trim();
                var s = Get(name);
                if (s == null) return $"Skill '{name}' not found.";
                var obj = new { s.Name, s.Description, s.ParametersSchema };
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            }

            return "Unknown command. Try: help";
        }
    }
}