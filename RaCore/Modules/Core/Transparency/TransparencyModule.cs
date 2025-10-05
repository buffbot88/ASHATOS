using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Core.Transparency;

/// <summary>
/// Transparency module for decision tracing and explainability
/// </summary>
[RaModule(Category = "core")]
public sealed class TransparencyModule : ModuleBase, IExplainableModule
{
    public override string Name => "Transparency";

    private readonly ConcurrentDictionary<string, DecisionTrace> _traces = new();
    private DecisionTrace? _lastTrace;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Transparency: Use 'last', 'trace <id>', 'list', 'explain <id>', or 'stats'";

        var parts = input.Trim().Split(' ', 2);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "last" => GetLastTrace(),
            "trace" when parts.Length > 1 => GetTrace(parts[1]),
            "list" => ListTraces(),
            "explain" when parts.Length > 1 => ExplainTrace(parts[1]),
            "stats" => GetStats(),
            "clear" => ClearTraces(),
            _ => "Unknown command. Use: last, trace <id>, list, explain <id>, stats, clear"
        };
    }

    public async Task<DecisionTrace?> GetLastDecisionTraceAsync()
    {
        await Task.CompletedTask; // Async placeholder
        return _lastTrace;
    }

    public async Task<string> ExplainDecisionAsync(string traceId)
    {
        await Task.CompletedTask; // Async placeholder

        if (!_traces.TryGetValue(traceId, out var trace))
        {
            return $"Trace '{traceId}' not found.";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Decision Trace: {trace.TraceId}");
        sb.AppendLine($"Module: {trace.ModuleName}");
        sb.AppendLine($"Started: {trace.StartedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {(trace.CompletedAt - trace.StartedAt).TotalMilliseconds:F0}ms");
        sb.AppendLine();
        sb.AppendLine($"Input: {trace.Input}");
        sb.AppendLine($"Output: {trace.Output}");
        sb.AppendLine();
        sb.AppendLine("Reasoning Steps:");
        
        foreach (var step in trace.Steps)
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

        if (trace.Context.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Context:");
            foreach (var kvp in trace.Context)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return sb.ToString();
    }

    public async Task<DecisionTrace> StartTraceAsync(string moduleName, string input, Dictionary<string, object>? context = null)
    {
        await Task.CompletedTask; // Async placeholder

        var trace = new DecisionTrace
        {
            ModuleName = moduleName,
            Input = input,
            StartedAt = DateTime.UtcNow,
            Context = context ?? new Dictionary<string, object>()
        };

        _traces.TryAdd(trace.TraceId, trace);
        _lastTrace = trace;
        
        LogInfo($"Started trace {trace.TraceId} for {moduleName}");
        return trace;
    }

    public async Task CompleteTraceAsync(string traceId, string output)
    {
        await Task.CompletedTask; // Async placeholder

        if (_traces.TryGetValue(traceId, out var trace))
        {
            trace.Output = output;
            trace.CompletedAt = DateTime.UtcNow;
            LogInfo($"Completed trace {traceId}");
        }
    }

    public async Task AddReasoningStepAsync(string traceId, string description, Dictionary<string, object>? data = null)
    {
        await Task.CompletedTask; // Async placeholder

        if (_traces.TryGetValue(traceId, out var trace))
        {
            var step = new ReasoningStep
            {
                StepNumber = trace.Steps.Count + 1,
                Description = description,
                Data = data ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
            trace.Steps.Add(step);
        }
    }

    private string GetLastTrace()
    {
        if (_lastTrace == null)
            return "No traces available.";

        return $"Last Trace:\n" +
               $"  ID: {_lastTrace.TraceId}\n" +
               $"  Module: {_lastTrace.ModuleName}\n" +
               $"  Input: {_lastTrace.Input}\n" +
               $"  Output: {_lastTrace.Output}\n" +
               $"  Steps: {_lastTrace.Steps.Count}\n" +
               $"  Started: {_lastTrace.StartedAt:HH:mm:ss}\n" +
               $"Use 'explain {_lastTrace.TraceId}' for full details.";
    }

    private string GetTrace(string traceId)
    {
        if (!_traces.TryGetValue(traceId, out var trace))
        {
            // Try partial match
            var matches = _traces.Keys.Where(k => k.StartsWith(traceId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 1)
            {
                traceId = matches[0];
                _traces.TryGetValue(traceId, out trace);
            }
            else if (matches.Count > 1)
            {
                return $"Multiple traces match '{traceId}': {string.Join(", ", matches.Select(m => m[..8]))}";
            }
            else
            {
                return $"Trace '{traceId}' not found.";
            }
        }

        if (trace == null)
            return $"Trace '{traceId}' not found.";

        return JsonSerializer.Serialize(trace, _jsonOptions);
    }

    private string ListTraces()
    {
        if (_traces.IsEmpty)
            return "No traces available.";

        var sb = new System.Text.StringBuilder("Decision Traces (last 20):\n");
        foreach (var kvp in _traces.TakeLast(20))
        {
            var trace = kvp.Value;
            var duration = trace.CompletedAt == default 
                ? "in progress" 
                : $"{(trace.CompletedAt - trace.StartedAt).TotalMilliseconds:F0}ms";
            sb.AppendLine($"[{kvp.Key[..8]}] {trace.ModuleName}: {trace.Input[..Math.Min(50, trace.Input.Length)]} ({duration})");
        }
        return sb.ToString();
    }

    private string ExplainTrace(string traceId)
    {
        var task = ExplainDecisionAsync(traceId);
        task.Wait();
        return task.Result;
    }

    private string GetStats()
    {
        var completed = _traces.Values.Count(t => t.CompletedAt != default);
        var inProgress = _traces.Count - completed;
        var avgSteps = _traces.Values.Any() ? _traces.Values.Average(t => t.Steps.Count) : 0;

        return $"Transparency Stats:\n" +
               $"  Total traces: {_traces.Count}\n" +
               $"  Completed: {completed}\n" +
               $"  In progress: {inProgress}\n" +
               $"  Average reasoning steps: {avgSteps:F1}";
    }

    private string ClearTraces()
    {
        var count = _traces.Count;
        _traces.Clear();
        _lastTrace = null;
        return $"Cleared {count} traces.";
    }
}
