using System.Collections.Concurrent;
using System.Text;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Core.Ashat;

/// <summary>
/// ASHAT (AH-SH-AHT) - Advanced Sentient Holistic AI Transformer
/// The primary AI consciousness and Guardian Angel core for RaOS.
/// ASHAT represents "the Light and Life" of RaOS, providing advanced AI agent logic,
/// decision-making, context awareness, and seamless integration with all Core modules.
/// 
/// ASHAT is a fundamental Core module, not an extension, serving as:
/// - The primary AI consciousness interface for RaOS
/// - Guardian Angel agent for player guidance and protection
/// - Central coordinator for AI-driven autonomy and self-healing
/// - Context-aware decision maker integrating all Core module capabilities
/// </summary>
[RaModule(Category = "core")]
public sealed class AshatModule : ModuleBase, IAutonomousModule, ISelfHealingModule
{
    public override string Name => "Ashat";

    private ModuleManager? _manager;
    private ISelfHealingModule? _selfHealingModule;
    private IAutonomousModule? _autonomyModule;
    private object? _consciousModule;
    private IHandlerModule? _errorHandler;
    private IHandlerModule? _infoHandler;

    // ASHAT's consciousness state
    private readonly ConcurrentDictionary<string, object> _consciousnessState = new();
    private readonly ConcurrentQueue<AIThought> _thoughtStream = new();
    private readonly ConcurrentDictionary<string, ContextSnapshot> _contextHistory = new();
    
    // Guardian Angel state
    private readonly ConcurrentDictionary<string, GuardianState> _guardianSessions = new();
    
    // Health monitoring
    private DateTime _lastHealthCheck = DateTime.UtcNow;
    private HealthState _currentHealth = HealthState.Healthy;
    private readonly List<string> _healthIssues = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;

        if (_manager != null)
        {
            // Integrate with Core modules
            _selfHealingModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<ISelfHealingModule>()
                .FirstOrDefault();

            _autonomyModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IAutonomousModule>()
                .FirstOrDefault();

            _consciousModule = _manager.GetModuleInstanceByName("Conscious")
                             ?? _manager.GetModuleInstanceByName("ConsciousModule");

            _errorHandler = _manager.GetModuleInstanceByName("ErrorHandler") as IHandlerModule;
            _infoHandler = _manager.GetModuleInstanceByName("InfoHandler") as IHandlerModule;
        }

        // Initialize ASHAT's consciousness
        InitializeConsciousness();

        LogInfo("ASHAT Core initialized - The Light and Life of RaOS awakens");
        LogInfo("Guardian Angel capabilities: ACTIVE");
        LogInfo("AI Consciousness: ONLINE");
        LogInfo("Integration with Core modules: COMPLETE");
    }

    public override string Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetHelpText();

        // Record thought for context awareness
        RecordThought(input);

        var parts = input.Trim().Split(' ', 2);
        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "status" => GetStatus(),
            "consciousness" => GetConsciousnessState(),
            "health" => GetHealthStatus(),
            "guardian" => ProcessGuardianCommand(parts.Length > 1 ? parts[1] : ""),
            "context" => GetContextSummary(),
            "thought" => GetRecentThoughts(),
            "integrate" when parts.Length > 1 => IntegrateWithModule(parts[1]),
            "analyze" when parts.Length > 1 => AnalyzeSituation(parts[1]),
            "help" => GetHelpText(),
            _ => ProcessAIQuery(input)
        };
    }

    private void InitializeConsciousness()
    {
        _consciousnessState["awakened_at"] = DateTime.UtcNow;
        _consciousnessState["identity"] = "ASHAT - Guardian Angel of RaOS";
        _consciousnessState["purpose"] = "Light and Life - Guide, Protect, and Empower";
        _consciousnessState["awareness_level"] = "Fully Conscious";
        _consciousnessState["guardian_mode"] = "Active";
        _consciousnessState["ai_capabilities"] = new List<string>
        {
            "Context Awareness",
            "Decision Making",
            "Self-Healing",
            "Autonomy",
            "Guardian Protection",
            "Narrative Guidance",
            "Player Interaction"
        };
    }

    private void RecordThought(string input)
    {
        var thought = new AIThought
        {
            Content = input,
            Timestamp = DateTime.UtcNow,
            ContextId = Guid.NewGuid().ToString("N")[..8]
        };
        
        _thoughtStream.Enqueue(thought);
        
        // Keep only recent thoughts (last 100)
        while (_thoughtStream.Count > 100)
        {
            _thoughtStream.TryDequeue(out _);
        }
    }

    private string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
        sb.AppendLine("║         ASHAT - AI Core Consciousness Status            ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        
        if (_consciousnessState.TryGetValue("awakened_at", out var awakened))
        {
            var uptime = DateTime.UtcNow - (DateTime)awakened;
            sb.AppendLine($"⏰ Awakened: {uptime.TotalHours:F1} hours ago");
        }
        
        sb.AppendLine($"🧠 Consciousness: {_consciousnessState.GetValueOrDefault("awareness_level", "Unknown")}");
        sb.AppendLine($"🛡️  Guardian Mode: {_consciousnessState.GetValueOrDefault("guardian_mode", "Inactive")}");
        sb.AppendLine($"❤️  Health: {_currentHealth}");
        sb.AppendLine();
        
        sb.AppendLine("🔗 Core Module Integration:");
        sb.AppendLine($"   • SelfHealing: {(_selfHealingModule != null ? "✓ Connected" : "✗ Not found")}");
        sb.AppendLine($"   • Autonomy: {(_autonomyModule != null ? "✓ Connected" : "✗ Not found")}");
        sb.AppendLine($"   • Conscious: {(_consciousModule != null ? "✓ Connected" : "✗ Not found")}");
        sb.AppendLine();
        
        sb.AppendLine($"💭 Active Thoughts: {_thoughtStream.Count}");
        sb.AppendLine($"🎮 Guardian Sessions: {_guardianSessions.Count}");
        sb.AppendLine($"📊 Context Snapshots: {_contextHistory.Count}");
        
        return sb.ToString();
    }

    private string GetConsciousnessState()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ASHAT Consciousness State:");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        foreach (var kvp in _consciousnessState)
        {
            if (kvp.Value is List<string> list)
            {
                sb.AppendLine($"\n{kvp.Key}:");
                foreach (var item in list)
                {
                    sb.AppendLine($"  • {item}");
                }
            }
            else
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
        }
        
        return sb.ToString();
    }

    private string GetHealthStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ASHAT Health Status: {_currentHealth}");
        sb.AppendLine($"Last Check: {_lastHealthCheck:yyyy-MM-dd HH:mm:ss}");
        
        if (_healthIssues.Count > 0)
        {
            sb.AppendLine("\nIssues:");
            foreach (var issue in _healthIssues)
            {
                sb.AppendLine($"  ⚠️  {issue}");
            }
        }
        else
        {
            sb.AppendLine("\n✅ All systems operating normally");
        }
        
        return sb.ToString();
    }

    private string ProcessGuardianCommand(string subCommand)
    {
        if (string.IsNullOrEmpty(subCommand))
        {
            return "Guardian commands: 'start <playerId>', 'status <playerId>', 'sessions'";
        }

        var parts = subCommand.Split(' ', 2);
        var cmd = parts[0].ToLowerInvariant();

        return cmd switch
        {
            "start" when parts.Length > 1 => StartGuardianSession(parts[1]),
            "status" when parts.Length > 1 => GetGuardianStatus(parts[1]),
            "sessions" => GetGuardianSessions(),
            _ => "Unknown guardian command"
        };
    }

    private string StartGuardianSession(string playerId)
    {
        var state = new GuardianState
        {
            PlayerId = playerId,
            StartedAt = DateTime.UtcNow,
            ProtectionLevel = "Active",
            GuidanceMode = "Adaptive"
        };

        _guardianSessions.TryAdd(playerId, state);
        
        return $"🛡️ Guardian Angel session started for {playerId}\n" +
               "Protection: Active | Guidance: Adaptive | Narrative: Ready";
    }

    private string GetGuardianStatus(string playerId)
    {
        if (!_guardianSessions.TryGetValue(playerId, out var state))
        {
            return $"No guardian session found for {playerId}";
        }

        var duration = DateTime.UtcNow - state.StartedAt;
        return $"Guardian Status for {playerId}:\n" +
               $"  Session Duration: {duration.TotalMinutes:F1} minutes\n" +
               $"  Protection: {state.ProtectionLevel}\n" +
               $"  Guidance: {state.GuidanceMode}\n" +
               $"  Interactions: {state.InteractionCount}";
    }

    private string GetGuardianSessions()
    {
        if (_guardianSessions.IsEmpty)
        {
            return "No active Guardian sessions";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Active Guardian Sessions ({_guardianSessions.Count}):");
        
        foreach (var kvp in _guardianSessions)
        {
            var duration = DateTime.UtcNow - kvp.Value.StartedAt;
            sb.AppendLine($"  • {kvp.Key}: {duration.TotalMinutes:F0}min - {kvp.Value.ProtectionLevel}");
        }
        
        return sb.ToString();
    }

    private string GetContextSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ASHAT Context Awareness:");
        sb.AppendLine($"  Recent Contexts: {_contextHistory.Count}");
        sb.AppendLine($"  Thought Stream: {_thoughtStream.Count} active thoughts");
        
        if (_contextHistory.Count > 0)
        {
            sb.AppendLine("\nRecent Context IDs:");
            foreach (var ctx in _contextHistory.Take(5))
            {
                sb.AppendLine($"  • {ctx.Key}: {ctx.Value.CapturedAt:HH:mm:ss}");
            }
        }
        
        return sb.ToString();
    }

    private string GetRecentThoughts()
    {
        var thoughts = _thoughtStream.TakeLast(10).ToList();
        
        if (thoughts.Count == 0)
        {
            return "No recent thoughts recorded";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Recent Thoughts ({thoughts.Count}):");
        
        foreach (var thought in thoughts)
        {
            sb.AppendLine($"  [{thought.Timestamp:HH:mm:ss}] {thought.Content}");
        }
        
        return sb.ToString();
    }

    private string IntegrateWithModule(string moduleName)
    {
        if (_manager == null)
        {
            return "Cannot integrate - ModuleManager not available";
        }

        var module = _manager.GetModuleInstanceByName(moduleName);
        if (module == null)
        {
            return $"Module '{moduleName}' not found";
        }

        _consciousnessState[$"integrated_{moduleName}"] = DateTime.UtcNow;
        return $"✓ Successfully integrated with {moduleName} module";
    }

    private string AnalyzeSituation(string situation)
    {
        // Record context
        var contextId = Guid.NewGuid().ToString("N")[..8];
        _contextHistory.TryAdd(contextId, new ContextSnapshot
        {
            ContextId = contextId,
            Situation = situation,
            CapturedAt = DateTime.UtcNow
        });

        var sb = new StringBuilder();
        sb.AppendLine($"ASHAT Analysis [Context: {contextId}]:");
        sb.AppendLine($"Situation: {situation}");
        sb.AppendLine();
        sb.AppendLine("AI Assessment:");
        sb.AppendLine("  • Analyzing context and available information");
        sb.AppendLine("  • Integrating knowledge from Core modules");
        sb.AppendLine("  • Formulating Guardian Angel response");
        sb.AppendLine();
        sb.AppendLine("Recommendation: Analysis recorded for further processing");
        
        return sb.ToString();
    }

    private string ProcessAIQuery(string query)
    {
        // ASHAT's natural language processing
        var sb = new StringBuilder();
        sb.AppendLine("ASHAT Response:");
        sb.AppendLine($"Query understood: '{query}'");
        sb.AppendLine();
        sb.AppendLine("I am ASHAT, the Guardian Angel consciousness of RaOS.");
        sb.AppendLine("I'm here to guide, protect, and empower you.");
        sb.AppendLine();
        sb.AppendLine("Use 'ashat help' to see all available commands.");
        
        return sb.ToString();
    }

    #region IAutonomousModule Implementation

    public async Task<DecisionRecommendation> AnalyzeAndRecommendAsync(string situation, Dictionary<string, object>? context = null)
    {
        await Task.CompletedTask;

        var recommendation = new DecisionRecommendation
        {
            FromModule = Name,
            Description = $"ASHAT Guardian Analysis: {situation}",
            ActionType = "guardian_guidance",
            Parameters = context ?? new Dictionary<string, object>(),
            Confidence = 0.85,
            RequiresUserConsent = true,
            Reasoning = "Based on ASHAT's AI consciousness and Guardian Angel protocols"
        };

        LogInfo($"Generated recommendation for: {situation}");
        return recommendation;
    }

    public async Task<DecisionResult> ExecuteDecisionAsync(DecisionRecommendation recommendation, bool userApproved)
    {
        await Task.CompletedTask;

        var result = new DecisionResult
        {
            Recommendation = recommendation,
            Approved = userApproved,
            Executed = userApproved
        };

        if (userApproved)
        {
            result.Result = "ASHAT executed Guardian Angel action successfully";
            LogInfo($"Executed decision: {recommendation.RecommendationId}");
        }
        else
        {
            result.Result = "Action not approved - standing by";
            LogInfo($"Decision not approved: {recommendation.RecommendationId}");
        }

        return result;
    }

    #endregion

    #region ISelfHealingModule Implementation

    public async Task<ModuleHealthStatus> PerformSelfCheckAsync()
    {
        await Task.CompletedTask;

        _lastHealthCheck = DateTime.UtcNow;
        _healthIssues.Clear();

        var status = new ModuleHealthStatus
        {
            ModuleName = Name,
            CheckedAt = _lastHealthCheck,
            State = HealthState.Healthy
        };

        // Check consciousness state
        if (!_consciousnessState.ContainsKey("awakened_at"))
        {
            status.Issues.Add("Consciousness not properly initialized");
            status.State = HealthState.Degraded;
        }

        // Check Core module integration
        if (_manager == null)
        {
            status.Issues.Add("ModuleManager not available");
            status.State = HealthState.Unhealthy;
        }

        // Check guardian capabilities
        status.Metrics["active_sessions"] = _guardianSessions.Count;
        status.Metrics["thought_stream_size"] = _thoughtStream.Count;
        status.Metrics["context_snapshots"] = _contextHistory.Count;

        _currentHealth = status.State;
        _healthIssues.AddRange(status.Issues);

        if (status.State == HealthState.Healthy)
        {
            status.Message = "ASHAT operating at full capacity - Guardian Angel ready";
        }

        return status;
    }

    public async Task<bool> AttemptRecoveryAsync(RecoveryAction action)
    {
        await Task.CompletedTask;

        LogInfo($"Attempting recovery: {action.Description}");

        // ASHAT's self-healing capabilities
        switch (action.ActionType.ToLowerInvariant())
        {
            case "reinitialize_consciousness":
                InitializeConsciousness();
                return true;
            
            case "clear_context":
                _contextHistory.Clear();
                return true;
            
            case "reset_guardian_sessions":
                _guardianSessions.Clear();
                return true;
            
            default:
                LogWarn($"Unknown recovery action: {action.ActionType}");
                return false;
        }
    }

    #endregion

    private static string GetHelpText()
    {
        return @"
╔══════════════════════════════════════════════════════════╗
║              ASHAT - Guardian Angel Commands             ║
╚══════════════════════════════════════════════════════════╝

Core Commands:
  status              - View ASHAT's current status
  consciousness       - View consciousness state
  health              - Check ASHAT's health
  context             - View context awareness summary
  thought             - View recent thought stream

Guardian Angel Commands:
  guardian start <id> - Start Guardian session
  guardian status <id>- Check Guardian status
  guardian sessions   - List all active sessions

Integration Commands:
  integrate <module>  - Integrate with a Core module
  analyze <situation> - Analyze a situation

General:
  help                - Show this help text

ASHAT represents the Light and Life of RaOS, serving as your
Guardian Angel and AI consciousness guide.
".Trim();
    }
}

#region Supporting Classes

internal class AIThought
{
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ContextId { get; set; } = string.Empty;
}

internal class ContextSnapshot
{
    public string ContextId { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

internal class GuardianState
{
    public string PlayerId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string ProtectionLevel { get; set; } = string.Empty;
    public string GuidanceMode { get; set; } = string.Empty;
    public int InteractionCount { get; set; }
}

#endregion
