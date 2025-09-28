using System;
using System.Collections.Generic;
using System.Text.Json;
using RaAI.Handlers.Manager;

namespace RaAI.Modules.PlanningModule
{
    // [RaModule("Planner")]
    public sealed class PlannerModule : ModuleBase
    {
        public override string Name => "Planner";
        private ModuleManager? _manager;

        public override void Initialize(ModuleManager manager)
        {
            base.Initialize(manager);
            _manager = manager;
        }

        public override string Process(string input)
        {
            // Input = intent json from NLU
            var intent = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
            if (intent == null || !intent.TryGetValue("intent", out var it)) return "(invalid intent)";

            var type = it?.ToString() ?? "";
            var plan = type switch
            {
                "device.control" => new Plan
                {
                    Goal = "Control device",
                    Steps = new()
                    {
                        new Step { Skill = "Device.Control", ArgumentsJson = input }
                    }
                },
                "system.open" => new Plan
                {
                    Goal = "Open target",
                    Steps = new()
                    {
                        new Step { Skill = "System.Open", ArgumentsJson = input }
                    }
                },
                _ => new Plan
                {
                    Goal = "Answer query",
                    Steps = new()
                    {
                        new Step { Skill = "Chat.Answer", ArgumentsJson = input }
                    }
                }
            };

            return JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true });
        }

        public sealed class Plan
        {
            public string Goal { get; set; } = "";
            public List<Step> Steps { get; set; } = new();
        }

        public sealed class Step
        {
            public string Skill { get; set; } = "";
            public string ArgumentsJson { get; set; } = "{}";
        }
    }
}