using System.Collections.Concurrent;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Core.Autonomy;

/// <summary>
/// Decision arbitrator for autonomous module conflict resolution
/// </summary>
[RaModule(Category = "core")]
public sealed class DecisionArbitratorModule : ModuleBase, IAutonomousModule
{
    public override string Name => "DecisionArbitrator";

    private ModuleManager? _manager;
    private readonly ConcurrentDictionary<string, DecisionResult> _decisionHistory = new();
    private readonly ConcurrentQueue<DecisionRecommendation> _pendingDecisions = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "DecisionArbitrator: Use 'pending', 'history', 'approve <id>', or 'reject <id>'";

        var parts = input.Trim().Split(' ', 2);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "pending" => GetPendingDecisions(),
            "history" => GetDecisionHistory(),
            "approve" when parts.Length > 1 => ApproveDecision(parts[1]),
            "reject" when parts.Length > 1 => RejectDecision(parts[1]),
            "stats" => GetStats(),
            _ => "Unknown command. Use: pending, history, approve <id>, reject <id>, stats"
        };
    }

    public async Task<DecisionRecommendation> AnalyzeAndRecommendAsync(string situation, Dictionary<string, object>? context = null)
    {
        await Task.CompletedTask; // Async placeholder

        var recommendation = new DecisionRecommendation
        {
            FromModule = Name,
            Description = $"Analyzed situation: {situation}",
            ActionType = "analyze",
            Parameters = context ?? new Dictionary<string, object>(),
            Confidence = 0.75,
            RequiresUserConsent = true,
            Reasoning = "Based on current context and available data"
        };

        _pendingDecisions.Enqueue(recommendation);
        return recommendation;
    }

    public async Task<DecisionResult> ExecuteDecisionAsync(DecisionRecommendation recommendation, bool userApproved)
    {
        await Task.CompletedTask; // Async placeholder

        var result = new DecisionResult
        {
            Recommendation = recommendation,
            Approved = userApproved,
            Executed = false
        };

        if (userApproved)
        {
            // Execute the decision
            result.Executed = true;
            result.Result = $"Decision '{recommendation.Description}' executed successfully";
            LogInfo($"Executed decision: {recommendation.RecommendationId}");
        }
        else
        {
            result.Result = "Decision rejected by user";
            LogInfo($"Decision rejected: {recommendation.RecommendationId}");
        }

        _decisionHistory.TryAdd(result.DecisionId, result);
        return result;
    }

    public async Task<DecisionResult> ArbitrateConflictAsync(List<DecisionRecommendation> conflictingRecommendations)
    {
        await Task.CompletedTask; // Async placeholder

        if (conflictingRecommendations.Count == 0)
        {
            return new DecisionResult
            {
                Approved = false,
                Executed = false,
                Result = "No recommendations to arbitrate"
            };
        }

        // Simple arbitration: choose the one with highest confidence
        var best = conflictingRecommendations.OrderByDescending(r => r.Confidence).First();
        
        LogInfo($"Arbitrated between {conflictingRecommendations.Count} recommendations. Selected: {best.RecommendationId}");

        return await ExecuteDecisionAsync(best, best.RequiresUserConsent ? false : true);
    }

    private string GetPendingDecisions()
    {
        if (_pendingDecisions.IsEmpty)
            return "No pending decisions.";

        var result = new System.Text.StringBuilder("Pending Decisions:\n");
        var decisions = _pendingDecisions.ToArray();
        for (int i = 0; i < Math.Min(10, decisions.Length); i++)
        {
            var d = decisions[i];
            result.AppendLine($"{i + 1}. [{d.RecommendationId[..8]}] {d.Description}");
            result.AppendLine($"   From: {d.FromModule}, Confidence: {d.Confidence:F2}, Requires consent: {d.RequiresUserConsent}");
        }
        return result.ToString();
    }

    private string GetDecisionHistory()
    {
        if (_decisionHistory.IsEmpty)
            return "No decision history.";

        var result = new System.Text.StringBuilder("Decision History (last 10):\n");
        foreach (var kvp in _decisionHistory.TakeLast(10))
        {
            var d = kvp.Value;
            result.AppendLine($"[{kvp.Key[..8]}] Approved: {d.Approved}, Executed: {d.Executed}");
            if (d.Recommendation != null)
                result.AppendLine($"  {d.Recommendation.Description}");
        }
        return result.ToString();
    }

    private string ApproveDecision(string idPrefix)
    {
        var decisions = _pendingDecisions.ToArray();
        var match = decisions.FirstOrDefault(d => d.RecommendationId.StartsWith(idPrefix, StringComparison.OrdinalIgnoreCase));
        
        if (match == null)
            return $"Decision with ID starting with '{idPrefix}' not found.";

        var task = ExecuteDecisionAsync(match, true);
        task.Wait();
        return task.Result.Result ?? "Decision approved and executed.";
    }

    private string RejectDecision(string idPrefix)
    {
        var decisions = _pendingDecisions.ToArray();
        var match = decisions.FirstOrDefault(d => d.RecommendationId.StartsWith(idPrefix, StringComparison.OrdinalIgnoreCase));
        
        if (match == null)
            return $"Decision with ID starting with '{idPrefix}' not found.";

        var task = ExecuteDecisionAsync(match, false);
        task.Wait();
        return "Decision rejected.";
    }

    private string GetStats()
    {
        return $"DecisionArbitrator Stats:\n" +
               $"  Pending decisions: {_pendingDecisions.Count}\n" +
               $"  Total decisions made: {_decisionHistory.Count}\n" +
               $"  Approved: {_decisionHistory.Values.Count(d => d.Approved)}\n" +
               $"  Executed: {_decisionHistory.Values.Count(d => d.Executed)}";
    }
}
