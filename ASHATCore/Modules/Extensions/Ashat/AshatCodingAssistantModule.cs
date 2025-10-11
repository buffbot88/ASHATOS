using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Modules.Extensions.Ashat;

/// <summary>
/// Ashat (AH-SH-AHT) - AI Coding Assistant Helper for ASHATOS Dev Team
/// The "Face" of ASHATOS, providing intelligent coding assistance and guidance.
/// Accessible through the Chat Support system when users are logged in.
/// On Dev Pages, helps guide and reference Class modules, working Interactively with developers.
/// 
/// ETHICAL COMMITMENT: "Harm None, Do What Ye Will"
/// - Ashat NEVER executes code without explicit user approval
/// - All actions follow approval-based workflow: Plan → Request Approval → Execute
/// - Users maintain full control and can reject/revise plans at any time
/// - Respects user autonomy, privacy, and follows UN Human Rights principles
/// - Educational approach: empowers developers ASHATther than replacing them
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AshatCodingAssistantModule : ModuleBase
{
    public override string Name => "Ashat";

    private ModuleManager? _manager;
    private ISpeechModule? _speechModule;
    private IChatModule? _chatModule;
    private readonly ConcurrentDictionary<string, CodingSession> _activeSessions = new();
    private readonly ConcurrentDictionary<string, List<string>> _userInteractions = new();
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Module knowledge base - tASHATcks available modules and their capabilities
    private readonly Dictionary<string, ModuleInfo> _moduleKnowledge = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;

        if (_manager != null)
        {
            _speechModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<ISpeechModule>()
                .FirstOrDefault();

            _chatModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IChatModule>()
                .FirstOrDefault();

            // Build module knowledge base from loaded modules
            BuildModuleKnowledge();
        }

        LogInfo("Ashat AI Coding Assistant initialized - Ready to help!");
        LogInfo("Ashat is the Face of ASHATOS, your intelligent development companion");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("ashat ", StringComparison.OrdinalIgnoreCase))
        {
            text = text["ashat ".Length..].Trim();
        }

        // Status and info commands
        if (text.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            return GetStatus();
        }

        if (text.Equals("modules", StringComparison.OrdinalIgnoreCase) || 
            text.Equals("list modules", StringComparison.OrdinalIgnoreCase))
        {
            return ListAvailableModules();
        }

        if (text.StartsWith("module info ", StringComparison.OrdinalIgnoreCase))
        {
            var moduleName = text["module info ".Length..].Trim();
            return GetModuleInfo(moduleName);
        }

        // Session management
        if (text.StartsWith("start session ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: ashat start session <userId> <goal>";
            }
            var userId = parts[2];
            var goal = parts[3];
            return StartCodingSession(userId, goal);
        }

        if (text.StartsWith("continue ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: ashat continue <userId> <message>";
            }
            var userId = parts[1];
            var message = parts[2];
            return ContinueSession(userId, message);
        }

        if (text.StartsWith("approve ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return "Usage: ashat approve <userId>";
            }
            var userId = parts[1];
            return ApproveAction(userId);
        }

        if (text.StartsWith("reject ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return "Usage: ashat reject <userId> <reason>";
            }
            var userId = parts[1];
            var reason = parts[2];
            return RejectAction(userId, reason);
        }

        if (text.StartsWith("end session ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["end session ".Length..].Trim();
            return EndSession(userId);
        }

        // Interactive help
        if (text.StartsWith("ask ", StringComparison.OrdinalIgnoreCase))
        {
            var question = text["ask ".Length..].Trim();
            return AskAshat(question, "anonymous");
        }

        return "Unknown command. Type 'help' or 'ashat help' for available commands.";
    }

    private void BuildModuleKnowledge()
    {
        if (_manager == null) return;

        LogInfo("Building module knowledge base...");

        foreach (var moduleInfo in _manager.Modules)
        {
            var instance = moduleInfo.Instance;
            if (instance == null) continue;

            var name = instance.Name;
            var category = GetModuleCategory(instance.GetType());

            _moduleKnowledge[name] = new ModuleInfo
            {
                Name = name,
                Category = category,
                Type = instance.GetType().Name,
                Capabilities = InferCapabilities(instance, name),
                Description = GetModuleDescription(name, category)
            };
        }

        LogInfo($"Module knowledge base built: {_moduleKnowledge.Count} modules indexed");
    }

    private string GetModuleCategory(Type moduleType)
    {
        var attribute = moduleType.GetCustomAttributes(typeof(RaModuleAttribute), false)
            .FirstOrDefault() as RaModuleAttribute;
        return attribute?.Category ?? "unknown";
    }

    private List<string> InferCapabilities(object module, string name)
    {
        var capabilities = new List<string>();

        // Infer from name and interfaces
        if (name.Contains("AI") || name.Contains("Language") || name.Contains("Speech"))
            capabilities.Add("AI Processing");
        
        if (name.Contains("Code") || name.Contains("Gen"))
            capabilities.Add("Code Generation");
        
        if (name.Contains("Chat") || name.Contains("Support"))
            capabilities.Add("Communication");
        
        if (name.Contains("Auth") || name.Contains("Security"))
            capabilities.Add("Security");
        
        if (name.Contains("Game") || name.Contains("Engine"))
            capabilities.Add("Game Development");
        
        if (name.Contains("Knowledge") || name.Contains("Learning"))
            capabilities.Add("Knowledge Management");

        if (capabilities.Count == 0)
            capabilities.Add("General Module");

        return capabilities;
    }

    private string GetModuleDescription(string name, string category)
    {
        // Provide context-aware descriptions
        return name switch
        {
            "AICodeGen" => "Generates code and project structures from natural language prompts",
            "AIContent" => "Creates game assets, NPCs, items, and world content",
            "Chat" => "Real-time messaging and communication system",
            "SupportChat" => "AI-driven support and appeal system",
            "Knowledge" => "Knowledge base and information retrieval system",
            "Speech" => "AI language model processing and Generation",
            "Authentication" => "User authentication and authorization",
            "FeatureExplorer" => "Explores and reports on system features",
            _ => $"{category} module providing {name} functionality"
        };
    }

    private string StartCodingSession(string userId, string goal)
    {
        // Check if user already has an active session
        if (_activeSessions.ContainsKey(userId))
        {
            return "You already have an active session. Use 'ashat end session' to close it first.";
        }

        var session = new CodingSession
        {
            UserId = userId,
            Goal = goal,
            StartedAt = DateTime.UtcNow,
            Status = SessionStatus.Active,
            Phase = SessionPhase.Planning
        };

        _activeSessions[userId] = session;

        if (!_userInteractions.ContainsKey(userId))
        {
            _userInteractions[userId] = new List<string>();
        }

        _userInteractions[userId].Add($"[{DateTime.UtcNow:HH:mm:ss}] Session started - Goal: {goal}");

        var response = new StringBuilder();
        response.AppendLine("=== Ashat AI Coding Assistant ===");
        response.AppendLine($"Hello! I'm Ashat, your AI coding companion.");
        response.AppendLine();
        response.AppendLine($"I understand you want to: {goal}");
        response.AppendLine();
        response.AppendLine("⚠️  IMPORTANT: I follow 'Harm None, Do What Ye Will' principles.");
        response.AppendLine("    I NEVER make changes without your explicit approval.");
        response.AppendLine();
        response.AppendLine("Let me help you plan this out. I'll work with you to:");
        response.AppendLine("1. Understand the requirements");
        response.AppendLine("2. Identify relevant modules and resources");
        response.AppendLine("3. Create an action plan");
        response.AppendLine("4. Get your approval before making changes");
        response.AppendLine();
        response.AppendLine("First, let me analyze what modules might help...");

        // Analyze which modules are relevant
        var relevantModules = FindRelevantModules(goal);
        if (relevantModules.Any())
        {
            response.AppendLine();
            response.AppendLine("📚 Relevant modules I found:");
            foreach (var mod in relevantModules.Take(5))
            {
                response.AppendLine($"  • {mod.Name} - {mod.Description}");
            }
        }

        response.AppendLine();
        response.AppendLine("What specific aspect would you like to focus on first?");
        response.AppendLine("Use 'ashat continue <userId> <your response>' to continue our conversation.");

        return response.ToString();
    }

    private List<ModuleInfo> FindRelevantModules(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        var relevant = new List<(ModuleInfo module, int score)>();

        foreach (var module in _moduleKnowledge.Values)
        {
            int score = 0;

            // Name matching
            if (goalLower.Contains(module.Name.ToLowerInvariant()))
                score += 10;

            // Category matching
            if (goalLower.Contains(module.Category.ToLowerInvariant()))
                score += 5;

            // Capability matching
            foreach (var capability in module.Capabilities)
            {
                if (goalLower.Contains(capability.ToLowerInvariant()))
                    score += 3;
            }

            // Keyword matching
            if (goalLower.Contains("code") && module.Capabilities.Contains("Code Generation"))
                score += 5;
            if (goalLower.Contains("game") && module.Capabilities.Contains("Game Development"))
                score += 5;
            if (goalLower.Contains("chat") && module.Capabilities.Contains("Communication"))
                score += 5;

            if (score > 0)
                relevant.Add((module, score));
        }

        return relevant.OrderByDescending(r => r.score)
            .Select(r => r.module)
            .ToList();
    }

    private string ContinueSession(string userId, string message)
    {
        if (!_activeSessions.TryGetValue(userId, out var session))
        {
            return "No active session found. Start a session with 'ashat start session <userId> <goal>'";
        }

        _userInteractions[userId].Add($"[{DateTime.UtcNow:HH:mm:ss}] User: {message}");

        var response = new StringBuilder();

        // Process based on current phase
        switch (session.Phase)
        {
            case SessionPhase.Planning:
                response.AppendLine("📋 Planning Phase");
                response.AppendLine();
                response.AppendLine($"Got it: {message}");
                response.AppendLine();
                response.AppendLine("Based on your goal and input, here's my proposed action plan:");
                response.AppendLine();

                var plan = GenerateActionPlan(session.Goal, message);
                session.ActionPlan = plan;
                session.Phase = SessionPhase.AwaitingApproval;

                response.AppendLine(FormatActionPlan(plan));
                response.AppendLine();
                response.AppendLine("⚠️  Before I proceed, I need your approval.");
                response.AppendLine("Review the plan above and:");
                response.AppendLine("  • Use 'ashat approve <userId>' to proceed");
                response.AppendLine("  • Use 'ashat reject <userId> <reason>' to revise the plan");
                break;

            case SessionPhase.AwaitingApproval:
                response.AppendLine("⏳ Awaiting Approval");
                response.AppendLine();
                response.AppendLine("I'm waiting for your approval on the action plan.");
                response.AppendLine("Please use 'ashat approve' or 'ashat reject' commands.");
                break;

            case SessionPhase.Executing:
                response.AppendLine("⚙️  Execution Phase");
                response.AppendLine();
                response.AppendLine($"Processing: {message}");
                response.AppendLine("(In a full implementation, this would execute the approved actions)");
                break;

            case SessionPhase.Completed:
                response.AppendLine("✅ Session Completed");
                response.AppendLine("This session has been completed. Start a new one if needed.");
                break;
        }

        _userInteractions[userId].Add($"[{DateTime.UtcNow:HH:mm:ss}] Ashat: {response}");

        return response.ToString();
    }

    private ActionPlan GenerateActionPlan(string goal, string additionalContext)
    {
        var plan = new ActionPlan
        {
            Goal = goal,
            Steps = new List<ActionStep>()
        };

        // Generate contextual steps based on goal
        if (goal.ToLowerInvariant().Contains("module") || goal.ToLowerInvariant().Contains("create"))
        {
            plan.Steps.Add(new ActionStep
            {
                Order = 1,
                Description = "Analyze requirements and identify module type",
                RequiredModules = new List<string> { "ModuleSpawner", "AICodeGen" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 2,
                Description = "Generate module structure and boilerplate code",
                RequiredModules = new List<string> { "AICodeGen" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 3,
                Description = "Review Generated code with developer",
                RequiredModules = new List<string> { "Ashat" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 4,
                Description = "Test and validate the module",
                RequiredModules = new List<string> { "TestRunner" }
            });
        }
        else if (goal.ToLowerInvariant().Contains("fix") || goal.ToLowerInvariant().Contains("debug"))
        {
            plan.Steps.Add(new ActionStep
            {
                Order = 1,
                Description = "Identify the issue and relevant code sections",
                RequiredModules = new List<string> { "Knowledge", "FeatureExplorer" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 2,
                Description = "Analyze the problem and propose solutions",
                RequiredModules = new List<string> { "Ashat", "Speech" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 3,
                Description = "Implement fix after approval",
                RequiredModules = new List<string> { "AICodeGen" }
            });
        }
        else
        {
            // Generic plan
            plan.Steps.Add(new ActionStep
            {
                Order = 1,
                Description = "Understand the requirements",
                RequiredModules = new List<string> { "Ashat" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 2,
                Description = "Research and identify relevant modules",
                RequiredModules = new List<string> { "Knowledge", "FeatureExplorer" }
            });

            plan.Steps.Add(new ActionStep
            {
                Order = 3,
                Description = "Propose implementation approach",
                RequiredModules = new List<string> { "Ashat" }
            });
        }

        plan.EstimatedComplexity = EstimateComplexity(goal);

        return plan;
    }

    private string FormatActionPlan(ActionPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📝 Action Plan:");
        sb.AppendLine($"Goal: {plan.Goal}");
        sb.AppendLine($"Complexity: {plan.EstimatedComplexity}");
        sb.AppendLine();
        sb.AppendLine("Steps:");

        foreach (var step in plan.Steps.OrderBy(s => s.Order))
        {
            sb.AppendLine($"  {step.Order}. {step.Description}");
            if (step.RequiredModules.Any())
            {
                sb.AppendLine($"     Required: {string.Join(", ", step.RequiredModules)}");
            }
        }

        return sb.ToString();
    }

    private string EstimateComplexity(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        // Simple heuristics for complexity estimation
        if (goalLower.Contains("simple") || goalLower.Contains("basic") || goalLower.Contains("quick"))
            return "Low";
        
        if (goalLower.Contains("complex") || goalLower.Contains("advanced") || goalLower.Contains("system"))
            return "High";
        
        if (goalLower.Contains("module") || goalLower.Contains("feature"))
            return "Medium";

        return "Medium";
    }

    private string ApproveAction(string userId)
    {
        if (!_activeSessions.TryGetValue(userId, out var session))
        {
            return "No active session found.";
        }

        if (session.Phase != SessionPhase.AwaitingApproval)
        {
            return "No action plan is awaiting approval.";
        }

        session.Phase = SessionPhase.Executing;
        session.ApprovedAt = DateTime.UtcNow;

        _userInteractions[userId].Add($"[{DateTime.UtcNow:HH:mm:ss}] User approved action plan");

        var response = new StringBuilder();
        response.AppendLine("✅ Action plan approved!");
        response.AppendLine();
        response.AppendLine("Proceeding with execution...");
        response.AppendLine();
        response.AppendLine("📝 Implementation Note:");
        response.AppendLine("In the full implementation, I would now:");
        response.AppendLine("  • Execute each step of the action plan");
        response.AppendLine("  • Coordinate with relevant modules");
        response.AppendLine("  • Generate/modify code as needed");
        response.AppendLine("  • Provide progress updates");
        response.AppendLine("  • Request review before finalizing");
        response.AppendLine();
        response.AppendLine("For now, this demonstRates the approval workflow.");
        response.AppendLine("Use 'ashat end session <userId>' to complete the session.");

        return response.ToString();
    }

    private string RejectAction(string userId, string reason)
    {
        if (!_activeSessions.TryGetValue(userId, out var session))
        {
            return "No active session found.";
        }

        if (session.Phase != SessionPhase.AwaitingApproval)
        {
            return "No action plan is awaiting approval.";
        }

        session.Phase = SessionPhase.Planning;
        session.ActionPlan = null;

        _userInteractions[userId].Add($"[{DateTime.UtcNow:HH:mm:ss}] User rejected: {reason}");

        var response = new StringBuilder();
        response.AppendLine("🔄 Action plan rejected");
        response.AppendLine();
        response.AppendLine($"Reason: {reason}");
        response.AppendLine();
        response.AppendLine("No problem! Let's revise the approach.");
        response.AppendLine("Can you help me understand what direction you'd prefer?");
        response.AppendLine();
        response.AppendLine("Use 'ashat continue <userId> <your guidance>' to help me create a better plan.");

        return response.ToString();
    }

    private string EndSession(string userId)
    {
        if (!_activeSessions.TryGetValue(userId, out var session))
        {
            return "No active session found.";
        }

        session.Phase = SessionPhase.Completed;
        session.CompletedAt = DateTime.UtcNow;

        _activeSessions.TryRemove(userId, out _);

        var duration = session.CompletedAt.HasValue 
            ? (session.CompletedAt.Value - session.StartedAt).TotalMinutes 
            : 0;

        var response = new StringBuilder();
        response.AppendLine("=== Session Summary ===");
        response.AppendLine($"Goal: {session.Goal}");
        response.AppendLine($"duration: {duration:F1} minutes");
        response.AppendLine($"Status: {session.Phase}");
        
        if (_userInteractions.TryGetValue(userId, out var Interactions))
        {
            response.AppendLine($"Interactions: {Interactions.Count}");
        }

        response.AppendLine();
        response.AppendLine("Thank you for working with me! Feel free to start a new session anytime.");
        response.AppendLine("I'm here to help whenever you need coding assistance! 🚀");

        return response.ToString();
    }

    private string AskAshat(string question, string userId)
    {
        var response = new StringBuilder();
        response.AppendLine("🤖 Ashat's Response:");
        response.AppendLine();

        // Provide contextual responses based on question
        var questionLower = question.ToLowerInvariant();

        if (questionLower.Contains("what") && questionLower.Contains("do"))
        {
            response.AppendLine("I'm Ashat, your AI coding assistant! I can help you:");
            response.AppendLine("  • Understand and reference ASHATOS modules");
            response.AppendLine("  • Plan and guide coding tasks");
            response.AppendLine("  • Generate code with approval-based workflow");
            response.AppendLine("  • Debug and fix issues");
            response.AppendLine("  • Learn about the ASHATOS architecture");
            response.AppendLine();
            response.AppendLine("Start a session to get personalized assistance!");
        }
        else if (questionLower.Contains("module"))
        {
            response.AppendLine($"I have knowledge of {_moduleKnowledge.Count} modules in the system.");
            response.AppendLine("Use 'ashat modules' to see the full list.");
            response.AppendLine("Use 'ashat module info <name>' for details about a specific module.");
        }
        else if (questionLower.Contains("help") || questionLower.Contains("how"))
        {
            response.AppendLine("To work with me effectively:");
            response.AppendLine("1. Start a session: 'ashat start session <userId> <your goal>'");
            response.AppendLine("2. Have a conversation about what you want to achieve");
            response.AppendLine("3. Review and approve my action plans");
            response.AppendLine("4. I'll guide you through the implementation");
            response.AppendLine();
            response.AppendLine("I always ask for approval before making changes!");
        }
        else
        {
            // Use AI module if available for more natural responses
            if (_speechModule != null)
            {
                try
                {
                    var aiPrompt = $"As Ashat, the ASHATOS AI coding assistant, answer this question: {question}";
                    var task = _speechModule.GenerateResponseAsync(aiPrompt);
                    task.Wait(TimeSpan.FromSeconds(10));
                    
                    if (task.IsCompleted)
                    {
                        response.AppendLine(task.Result);
                    }
                    else
                    {
                        response.AppendLine("I'm thinking about your question...");
                        response.AppendLine("(AI response timeout - try asking in a different way)");
                    }
                }
                catch
                {
                    response.AppendLine("I understand your question, but I need more context.");
                    response.AppendLine("Try starting a session for detailed assistance!");
                }
            }
            else
            {
                response.AppendLine("That's an interesting question!");
                response.AppendLine("Start a session with me to dive deeper into this topic.");
            }
        }

        return response.ToString();
    }

    private string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Ashat Status ===");
        sb.AppendLine($"Active Sessions: {_activeSessions.Count}");
        sb.AppendLine($"Known Modules: {_moduleKnowledge.Count}");
        sb.AppendLine($"AI Module: {(_speechModule != null ? "Connected" : "Not Available")}");
        sb.AppendLine($"Chat Module: {(_chatModule != null ? "Connected" : "Not Available")}");
        
        if (_activeSessions.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Active Sessions:");
            foreach (var session in _activeSessions.Values)
            {
                sb.AppendLine($"  • User {session.UserId}: {session.Goal} ({session.Phase})");
            }
        }

        return sb.ToString();
    }

    private string ListAvailableModules()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Available Modules ===");
        sb.AppendLine($"Total: {_moduleKnowledge.Count} modules");
        sb.AppendLine();

        var byCategory = _moduleKnowledge.Values
            .GroupBy(m => m.Category)
            .OrderBy(g => g.Key);

        foreach (var category in byCategory)
        {
            sb.AppendLine($"📂 {category.Key.ToUpper()}:");
            foreach (var module in category.OrderBy(m => m.Name))
            {
                sb.AppendLine($"  • {module.Name} - {module.Description}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Use 'ashat module info <name>' for detailed information about a module.");

        return sb.ToString();
    }

    private string GetModuleInfo(string moduleName)
    {
        var module = _moduleKnowledge.Values
            .FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            return $"Module '{moduleName}' not found. Use 'ashat modules' to see available modules.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"=== Module: {module.Name} ===");
        sb.AppendLine($"Category: {module.Category}");
        sb.AppendLine($"Type: {module.Type}");
        sb.AppendLine($"Description: {module.Description}");
        sb.AppendLine();
        sb.AppendLine("Capabilities:");
        foreach (var capability in module.Capabilities)
        {
            sb.AppendLine($"  • {capability}");
        }

        return sb.ToString();
    }

    private string GetHelp()
    {
        return @"=== Ashat AI Coding Assistant ===

Hi! I'm Ashat (AH-SH-AHT), the Face of ASHATOS and your AI coding companion.
I'm here to help you develop, debug, and understand the ASHATOS system.

⚠️  ETHICAL COMMITMENT: ""Harm None, Do What Ye Will""
  • I NEVER execute code without your explicit approval
  • You maintain full control - reject/revise plans anytime
  • I respect your autonomy, privacy, and human rights
  • I empower you through education, not automation

🎯 Interactive Workflow:
  1. I work WITH you, not FOR you
  2. I form action plans and get your approval
  3. I guide you through implementation
  4. I help you understand modules and code

📋 Commands:

  Session Management:
    ashat start session <userId> <goal>    - Start a coding assistance session
    ashat continue <userId> <message>      - Continue the conversation
    ashat approve <userId>                 - Approve the proposed action plan
    ashat reject <userId> <reason>         - Reject and revise the plan
    ashat end session <userId>             - End the current session

  Information & Learning:
    ashat modules                          - List all available modules
    ashat module info <name>               - Get detailed info about a module
    ashat ask <question>                   - Ask me anything about ASHATOS
    ashat status                           - View my current status

  General:
    help / ashat help                      - Show this help message

💡 Example Usage:
    ashat start session dev001 Create a new inventory module
    ashat continue dev001 It should tASHATck items and quantities
    ashat approve dev001
    ashat end session dev001

🌟 Features:
  • Interactive coding guidance
  • Module knowledge and reference
  • Approval-based workflow (Form Plan → Request Approval → Execute)
  • integration with Chat Support system
  • Context-aware assistance
  • Learning and adaptation

🔗 integration:
  • Accessible through Chat Support when logged in
  • Available on Dev Pages for guidance
  • References class modules and documentation
  • Works with existing ASHATOS modules

I'm excited to work with you! Start a session and let's build something great! 🚀";
    }
}

// Supporting classes for Ashat's functionality

public class CodingSession
{
    public string UserId { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public SessionStatus Status { get; set; }
    public SessionPhase Phase { get; set; }
    public ActionPlan? ActionPlan { get; set; }
}

public enum SessionStatus
{
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum SessionPhase
{
    Planning,           // Understanding requirements
    AwaitingApproval,   // Waiting for user approval
    Executing,          // Implementing the plan
    Reviewing,          // Reviewing results
    Completed           // Finished
}

public class ActionPlan
{
    public string Goal { get; set; } = string.Empty;
    public List<ActionStep> Steps { get; set; } = new();
    public string EstimatedComplexity { get; set; } = "Medium";
}

public class ActionStep
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredModules { get; set; } = new();
    public bool IsCompleted { get; set; }
}

public class ModuleInfo
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
}
