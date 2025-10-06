using System.Collections.Concurrent;
using System.Text.Json;
using Abstractions;
using RaCore.Engine.Manager;

namespace RaCore.Modules.Extensions.Support;

/// <summary>
/// AI-driven support chat module for handling user appeals on suspended accounts.
/// Integrates with RaAI to conduct automated interviews and make judgments.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class SupportChatModule : ModuleBase, ISupportChatModule
{
    public override string Name => "SupportChat";

    private readonly ConcurrentDictionary<string, AppealRequest> _appeals = new();
    private readonly ConcurrentDictionary<string, AppealSession> _sessions = new();
    private readonly ConcurrentDictionary<string, AppealDecision> _decisions = new();
    
    private ModuleManager? _manager;
    private IContentModerationModule? _moderationModule;
    private ISpeechModule? _speechModule;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Standard interview questions for appeal process
    private static readonly List<string> _appealQuestions = new()
    {
        "Can you explain in your own words why your account was suspended?",
        "Do you understand which community rules were violated?",
        "What steps will you take to prevent this from happening again?",
        "Is there any additional context or information you'd like to share?",
        "Do you acknowledge that future violations may result in permanent suspension?"
    };

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;
        
        if (_manager != null)
        {
            _moderationModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<IContentModerationModule>()
                .FirstOrDefault();
                
            _speechModule = _manager.Modules
                .Select(m => m.Instance)
                .OfType<ISpeechModule>()
                .FirstOrDefault();
        }
        
        Console.WriteLine($"[{Name}] Support Chat Module initialized");
        Console.WriteLine($"[{Name}] AI-driven appeal system ready");
    }

    public override string Process(string input)
    {
        var text = (input ?? "").Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("appeal start ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: appeal start <userId> <initial statement>";
            }
            
            var userId = parts[2];
            var statement = parts[3];
            var task = StartAppealAsync(userId, statement);
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("appeal respond ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return "Usage: appeal respond <userId> <response>";
            }
            
            var userId = parts[2];
            var response = parts[3];
            var task = SubmitAppealResponseAsync(userId, response);
            task.Wait();
            return task.Result;
        }

        if (text.StartsWith("appeal status ", StringComparison.OrdinalIgnoreCase))
        {
            var userId = text["appeal status ".Length..].Trim();
            var task = GetAppealDecisionAsync(userId);
            task.Wait();
            return task.Result != null 
                ? JsonSerializer.Serialize(task.Result, _jsonOptions)
                : "No appeal decision found for this user";
        }

        if (text.Equals("appeal pending", StringComparison.OrdinalIgnoreCase))
        {
            var task = GetPendingHumanReviewsAsync();
            task.Wait();
            return JsonSerializer.Serialize(task.Result, _jsonOptions);
        }

        if (text.StartsWith("appeal review ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 5, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5)
            {
                return "Usage: appeal review <appealId> <outcome> <reviewerId> <notes>";
            }
            
            if (!Guid.TryParse(parts[2], out var appealId))
            {
                return "Invalid appeal ID";
            }
            
            if (!Enum.TryParse<AppealOutcome>(parts[3], true, out var outcome))
            {
                return "Invalid outcome. Options: Approved, PartiallyApproved, Denied";
            }
            
            var reviewerId = parts[4];
            var notes = parts.Length > 5 ? string.Join(" ", parts.Skip(5)) : "Reviewed";
            var task = ReviewEscalatedAppealAsync(appealId, outcome, notes, reviewerId);
            task.Wait();
            return task.Result ? "Appeal reviewed successfully" : "Failed to review appeal";
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    public async Task<AppealRequest> StartAppealAsync(string userId, string initialStatement)
    {
        // Check if user is actually suspended
        if (_moderationModule == null || !await _moderationModule.IsUserSuspendedAsync(userId))
        {
            throw new InvalidOperationException("User is not currently suspended");
        }

        // Check if user already has an active appeal
        var existingAppeal = _appeals.Values.FirstOrDefault(a => 
            a.UserId == userId && 
            (a.Status == AppealStatus.Pending || a.Status == AppealStatus.InProgress));
            
        if (existingAppeal != null)
        {
            throw new InvalidOperationException("User already has an active appeal");
        }

        var suspension = await _moderationModule.GetActiveSuspensionAsync(userId);
        if (suspension == null)
        {
            throw new InvalidOperationException("Cannot find active suspension");
        }

        // Create appeal request
        var appeal = new AppealRequest
        {
            UserId = userId,
            SuspensionId = suspension.Id,
            InitialStatement = initialStatement,
            Status = AppealStatus.InProgress
        };

        _appeals[appeal.Id.ToString()] = appeal;

        // Create appeal session
        var session = new AppealSession
        {
            AppealRequestId = appeal.Id,
            UserId = userId,
            Status = AppealSessionStatus.Active
        };

        _sessions[session.Id.ToString()] = session;

        // Start with first question
        var firstQuestion = _appealQuestions[0];
        session.Interactions.Add(new AppealInteraction
        {
            Question = firstQuestion,
            Response = "", // Awaiting user response
            AiAnalysis = "Initial question"
        });

        Console.WriteLine($"[{Name}] Appeal started for user {userId}");
        
        return appeal;
    }

    public async Task<AppealSession?> GetActiveSessionAsync(string userId)
    {
        await Task.CompletedTask;
        
        return _sessions.Values.FirstOrDefault(s => 
            s.UserId == userId && 
            s.Status == AppealSessionStatus.Active);
    }

    public async Task<string> SubmitAppealResponseAsync(string userId, string response)
    {
        var session = await GetActiveSessionAsync(userId);
        if (session == null)
        {
            return "No active appeal session found. Start an appeal first.";
        }

        // Get current question
        var currentInteraction = session.Interactions.LastOrDefault(i => string.IsNullOrEmpty(i.Response));
        if (currentInteraction == null)
        {
            return "No pending question found.";
        }

        // Record user response
        currentInteraction.Response = response;

        // Analyze response using AI
        var aiAnalysis = await AnalyzeResponseAsync(currentInteraction.Question, response, session);
        currentInteraction.AiAnalysis = aiAnalysis;

        // Check if we have more questions
        var currentQuestionIndex = session.Interactions.Count - 1;
        if (currentQuestionIndex < _appealQuestions.Count - 1)
        {
            // Ask next question
            var nextQuestion = _appealQuestions[currentQuestionIndex + 1];
            session.Interactions.Add(new AppealInteraction
            {
                Question = nextQuestion,
                Response = "",
                AiAnalysis = "Awaiting response"
            });
            
            return $"Thank you. Next question: {nextQuestion}";
        }
        else
        {
            // All questions answered, make decision
            session.Status = AppealSessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;

            var decision = await MakeAppealDecisionAsync(session);
            _decisions[decision.Id.ToString()] = decision;

            // Update appeal status
            var appeal = _appeals.Values.FirstOrDefault(a => a.Id == session.AppealRequestId);
            if (appeal != null)
            {
                appeal.Status = decision.RequiresHumanReview 
                    ? AppealStatus.Escalated 
                    : AppealStatus.Completed;
            }

            // Apply decision if not escalated
            if (!decision.RequiresHumanReview && decision.Outcome == AppealOutcome.Approved)
            {
                await ApplyDecisionAsync(userId, decision);
            }

            return FormatDecisionResponse(decision);
        }
    }

    public async Task<AppealDecision?> GetAppealDecisionAsync(string userId)
    {
        await Task.CompletedTask;
        
        return _decisions.Values
            .Where(d => _appeals.Values.Any(a => a.Id == d.AppealRequestId && a.UserId == userId))
            .OrderByDescending(d => d.DecidedAt)
            .FirstOrDefault();
    }

    public async Task<List<AppealDecision>> GetPendingHumanReviewsAsync()
    {
        await Task.CompletedTask;
        
        return _decisions.Values
            .Where(d => d.RequiresHumanReview)
            .OrderByDescending(d => d.DecidedAt)
            .ToList();
    }

    public async Task<bool> ReviewEscalatedAppealAsync(Guid appealId, AppealOutcome outcome, string reviewNotes, string reviewerId)
    {
        var appeal = _appeals.Values.FirstOrDefault(a => a.Id == appealId);
        if (appeal == null)
        {
            return false;
        }

        var decision = _decisions.Values.FirstOrDefault(d => d.AppealRequestId == appealId);
        if (decision == null)
        {
            return false;
        }

        // Update decision with human review
        decision.Outcome = outcome;
        decision.ReviewNotes = reviewNotes;
        decision.RequiresHumanReview = false;

        // Update appeal status
        appeal.Status = AppealStatus.Completed;

        // Apply decision
        if (outcome == AppealOutcome.Approved || outcome == AppealOutcome.PartiallyApproved)
        {
            await ApplyDecisionAsync(appeal.UserId, decision);
        }

        Console.WriteLine($"[{Name}] Appeal {appealId} reviewed by {reviewerId}: {outcome}");
        
        return true;
    }

    private async Task<string> AnalyzeResponseAsync(string question, string response, AppealSession session)
    {
        if (_speechModule == null)
        {
            return "Basic analysis: Response recorded";
        }

        // Use AI to analyze the response quality and sincerity
        var analysisPrompt = $@"Analyze this user's appeal response for sincerity and understanding:

Question: {question}
Response: {response}

Provide a brief analysis (1-2 sentences) on whether the response shows:
1. Understanding of the violation
2. Genuine remorse
3. Commitment to follow rules

Analysis:";

        try
        {
            var analysis = await _speechModule.GenerateResponseAsync(analysisPrompt);
            return analysis ?? "Analysis unavailable";
        }
        catch
        {
            return "Analysis unavailable";
        }
    }

    private async Task<AppealDecision> MakeAppealDecisionAsync(AppealSession session)
    {
        var decision = new AppealDecision
        {
            AppealRequestId = session.AppealRequestId,
            SessionId = session.Id
        };

        if (_speechModule == null)
        {
            // Fallback: basic decision logic
            decision.Outcome = AppealOutcome.EscalateToHuman;
            decision.Reasoning = "AI unavailable - requires human review";
            decision.ConfidenceScore = 0.0;
            decision.RequiresHumanReview = true;
            return decision;
        }

        // Build comprehensive summary for AI decision
        var summary = BuildSessionSummary(session);
        var decisionPrompt = $@"You are an impartial AI moderator reviewing a user appeal for account suspension.

{summary}

Based on the user's responses, make a decision:
1. APPROVED - If the user shows clear understanding, remorse, and commitment to follow rules
2. DENIED - If the user shows lack of understanding, no remorse, or unwillingness to change
3. ESCALATE - If the case is complex or requires human judgment

Provide your decision in this format:
DECISION: [APPROVED/DENIED/ESCALATE]
CONFIDENCE: [0.0-1.0]
REASONING: [Brief explanation]";

        try
        {
            var aiResponse = await _speechModule.GenerateResponseAsync(decisionPrompt);
            ParseAiDecision(aiResponse, decision);
        }
        catch
        {
            decision.Outcome = AppealOutcome.EscalateToHuman;
            decision.Reasoning = "Error in AI processing - requires human review";
            decision.ConfidenceScore = 0.0;
            decision.RequiresHumanReview = true;
        }

        Console.WriteLine($"[{Name}] Appeal decision made: {decision.Outcome} (Confidence: {decision.ConfidenceScore:F2})");
        
        return decision;
    }

    private string BuildSessionSummary(AppealSession session)
    {
        var lines = new List<string>();
        lines.Add("Appeal Interview Summary:");
        lines.Add("========================");
        
        for (int i = 0; i < session.Interactions.Count; i++)
        {
            var interaction = session.Interactions[i];
            lines.Add($"\nQ{i + 1}: {interaction.Question}");
            lines.Add($"A{i + 1}: {interaction.Response}");
            if (!string.IsNullOrWhiteSpace(interaction.AiAnalysis))
            {
                lines.Add($"Analysis: {interaction.AiAnalysis}");
            }
        }
        
        return string.Join("\n", lines);
    }

    private void ParseAiDecision(string aiResponse, AppealDecision decision)
    {
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var lineLower = line.ToLowerInvariant();
            
            if (lineLower.Contains("decision:"))
            {
                if (lineLower.Contains("approved"))
                {
                    decision.Outcome = AppealOutcome.Approved;
                }
                else if (lineLower.Contains("denied"))
                {
                    decision.Outcome = AppealOutcome.Denied;
                }
                else if (lineLower.Contains("escalate"))
                {
                    decision.Outcome = AppealOutcome.EscalateToHuman;
                }
            }
            else if (lineLower.Contains("confidence:"))
            {
                var confStr = line.Split(':', 2).Last().Trim();
                if (double.TryParse(confStr, out var confidence))
                {
                    decision.ConfidenceScore = confidence;
                }
            }
            else if (lineLower.Contains("reasoning:"))
            {
                decision.Reasoning = line.Split(':', 2).Last().Trim();
            }
        }

        // Set human review flag based on confidence and outcome
        decision.RequiresHumanReview = 
            decision.Outcome == AppealOutcome.EscalateToHuman ||
            decision.ConfidenceScore < 0.7;
    }

    private async Task ApplyDecisionAsync(string userId, AppealDecision decision)
    {
        if (_moderationModule == null)
        {
            Console.WriteLine($"[{Name}] Cannot apply decision - moderation module not available");
            return;
        }

        if (decision.Outcome == AppealOutcome.Approved)
        {
            // Lift suspension
            await _moderationModule.UnsuspendUserAsync(userId, "AI Appeal System");
            Console.WriteLine($"[{Name}] Suspension lifted for user {userId} based on successful appeal");
        }
        else if (decision.Outcome == AppealOutcome.PartiallyApproved)
        {
            // Could reduce suspension duration here if needed
            Console.WriteLine($"[{Name}] Partial approval for user {userId} - manual intervention may be required");
        }
    }

    private string FormatDecisionResponse(AppealDecision decision)
    {
        var response = new System.Text.StringBuilder();
        response.AppendLine("=== Appeal Decision ===");
        response.AppendLine($"Outcome: {decision.Outcome}");
        response.AppendLine($"Reasoning: {decision.Reasoning}");
        response.AppendLine($"Confidence: {decision.ConfidenceScore:F2}");
        
        if (decision.RequiresHumanReview)
        {
            response.AppendLine("\nThis appeal has been escalated to human moderators for review.");
            response.AppendLine("You will be notified when a final decision is made.");
        }
        else if (decision.Outcome == AppealOutcome.Approved)
        {
            response.AppendLine("\nYour appeal has been approved. Your suspension has been lifted.");
            response.AppendLine("Please remember to follow community guidelines going forward.");
        }
        else if (decision.Outcome == AppealOutcome.Denied)
        {
            response.AppendLine("\nYour appeal has been denied. The suspension remains in effect.");
            response.AppendLine("You may reapply after the suspension period or contact support.");
        }
        
        return response.ToString();
    }

    private string GetHelp()
    {
        return @"Support Chat Module (AI-Driven Appeals):
  - help                                    : Show this help
  - appeal start <userId> <statement>       : Start an appeal for a suspended user
  - appeal respond <userId> <response>      : Submit a response to appeal question
  - appeal status <userId>                  : Check appeal decision status
  - appeal pending                          : List appeals requiring human review
  - appeal review <appealId> <outcome> <reviewerId> <notes> : Review escalated appeal

Features:
  - AI-driven interview process for suspended users
  - Automated question/answer appeal workflow
  - AI judgment and decision-making on appeals
  - Automatic suspension lifting for approved appeals
  - Escalation to human moderators for complex cases
  - Comprehensive logging of all appeal interactions

Appeal Process:
  1. User starts appeal with initial statement
  2. AI conducts interview with standard questions
  3. User responds to each question in sequence
  4. AI analyzes responses for sincerity and understanding
  5. AI makes final decision (approve/deny/escalate)
  6. Decision applied automatically or escalated to humans

Outcomes:
  - Approved: Suspension lifted, user reinstated
  - Denied: Suspension remains in effect
  - Escalated: Requires human moderator review
  - PartiallyApproved: Suspension reduced (manual intervention)";
    }
}
