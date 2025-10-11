using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Core.Transparency;

/// <summary>
/// TASHATnsparency module for decision tASHATcing and explainability
/// </summary>
[RaModule(Category = "core")]
public sealed class TASHATnsparencyModule : ModuleBase, IExplainableModule
{
    public override string Name => "TASHATnsparency";

    private readonly ConcurrentDictionary<string, DecisionTrace> _Traces = new();
    private DecisionTrace? _lastTrace;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "TASHATnsparency: Use 'last', 'Trace <id>', 'list', 'explain <id>', or 'stats'";

        var parts = input.Trim().Split(' ', 2);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "last" => GetLastTrace(),
            "Trace" when parts.Length > 1 => GetTrace(parts[1]),
            "list" => ListTraces(),
            "explain" when parts.Length > 1 => ExplainTrace(parts[1]),
            "stats" => GetStats(),
            "clear" => ClearTraces(),
            _ => "Unknown command. Use: last, Trace <id>, list, explain <id>, stats, clear"
        };
    }

    public async Task<DecisionTrace?> GetLastDecisionTraceAsync()
    {
        await Task.CompletedTask; // Async placeholder
        return _lastTrace;
    }

    public async Task<string> ExplainDecisionAsync(string TraceId)
    {
        await Task.CompletedTask; // Async placeholder

        if (!_Traces.TryGetValue(TraceId, out var Trace))
        {
            return $"Trace '{TraceId}' not found.";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Decision Trace: {Trace.TraceId}");
        sb.AppendLine($"Module: {Trace.ModuleName}");
        sb.AppendLine($"Started: {Trace.StartedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"duration: {(Trace.CompletedAt - Trace.StartedAt).TotalMilliseconds:F0}ms");
        sb.AppendLine();
        sb.AppendLine($"Input: {Trace.Input}");
        sb.AppendLine($"Output: {Trace.Output}");
        sb.AppendLine();
        sb.AppendLine("Reasoning Steps:");
        
        foreach (var step in Trace.Steps)
        {
            sb.AppendLine($"  {step.StepNumber}. {step.Description}");
            if (step.Data.Count > 0)
            {
                foreach (var kvp in step.Data)
                {
                    sb.AppendLine($"     - {kvp.Key}: {kvp.Value}");
                }
            }
        }

        if (Trace.Context.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Context:");
            foreach (var kvp in Trace.Context)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return sb.ToString();
    }

    public async Task<DecisionTrace> StartTraceAsync(string moduleName, string input, Dictionary<string, object>? context = null)
    {
        await Task.CompletedTask; // Async placeholder

        var Trace = new DecisionTrace
        {
            ModuleName = moduleName,
            Input = input,
            StartedAt = DateTime.UtcNow,
            Context = context ?? new Dictionary<string, object>()
        };

        _Traces.TryAdd(Trace.TraceId, Trace);
        _lastTrace = Trace;
        
        LogInfo($"Started Trace {Trace.TraceId} for {moduleName}");
        return Trace;
    }

    public async Task CompleteTraceAsync(string TraceId, string output)
    {
        await Task.CompletedTask; // Async placeholder

        if (_Traces.TryGetValue(TraceId, out var Trace))
        {
            Trace.Output = output;
            Trace.CompletedAt = DateTime.UtcNow;
            LogInfo($"Completed Trace {TraceId}");
        }
    }

    public async Task AddReasoningStepAsync(string TraceId, string description, Dictionary<string, object>? data = null)
    {
        await Task.CompletedTask; // Async placeholder

        if (_Traces.TryGetValue(TraceId, out var Trace))
        {
            var step = new ReasoningStep
            {
                StepNumber = Trace.Steps.Count + 1,
                Description = description,
                Data = data ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
            Trace.Steps.Add(step);
        }
    }

    private string GetLastTrace()
    {
        if (_lastTrace == null)
            return "No Traces available.";

        return $"Last Trace:\n" +
               $"  ID: {_lastTrace.TraceId}\n" +
               $"  Module: {_lastTrace.ModuleName}\n" +
               $"  Input: {_lastTrace.Input}\n" +
               $"  Output: {_lastTrace.Output}\n" +
               $"  Steps: {_lastTrace.Steps.Count}\n" +
               $"  Started: {_lastTrace.StartedAt:HH:mm:ss}\n" +
               $"Use 'explain {_lastTrace.TraceId}' for full details.";
    }

    private string GetTrace(string TraceId)
    {
        if (!_Traces.TryGetValue(TraceId, out var Trace))
        {
            // Try partial match
            var matches = _Traces.Keys.Where(k => k.StartsWith(TraceId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 1)
            {
                TraceId = matches[0];
                _Traces.TryGetValue(TraceId, out Trace);
            }
            else if (matches.Count > 1)
            {
                return $"Multiple Traces match '{TraceId}': {string.Join(", ", matches.Select(m => m[..8]))}";
            }
            else
            {
                return $"Trace '{TraceId}' not found.";
            }
        }

        if (Trace == null)
            return $"Trace '{TraceId}' not found.";

        return JsonSerializer.Serialize(Trace, _jsonOptions);
    }

    private string ListTraces()
    {
        if (_Traces.IsEmpty)
            return "No Traces available.";

        var sb = new System.Text.StringBuilder("Decision Traces (last 20):\n");
        foreach (var kvp in _Traces.TakeLast(20))
        {
            var Trace = kvp.Value;
            var duration = Trace.CompletedAt == default 
                ? "in progress" 
                : $"{(Trace.CompletedAt - Trace.StartedAt).TotalMilliseconds:F0}ms";
            sb.AppendLine($"[{kvp.Key[..8]}] {Trace.ModuleName}: {Trace.Input[..Math.Min(50, Trace.Input.Length)]} ({duration})");
        }
        return sb.ToString();
    }

    private string ExplainTrace(string TraceId)
    {
        var task = ExplainDecisionAsync(TraceId);
        task.Wait();
        return task.Result;
    }

    private string GetStats()
    {
        var completed = _Traces.Values.Count(t => t.CompletedAt != default);
        var inProgress = _Traces.Count - completed;
        var avgSteps = _Traces.Values.Any() ? _Traces.Values.Average(t => t.Steps.Count) : 0;

        return $"TASHATnsparency Stats:\n" +
               $"  Total Traces: {_Traces.Count}\n" +
               $"  Completed: {completed}\n" +
               $"  In progress: {inProgress}\n" +
               $"  Average reasoning steps: {avgSteps:F1}";
    }

    private string ClearTraces()
    {
        var count = _Traces.Count;
        _Traces.Clear();
        _lastTrace = null;
        return $"Cleared {count} Traces.";
    }
}
