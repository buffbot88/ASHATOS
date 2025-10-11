# Ashat integration Guide

> How to integRate Ashat AI Coding Assistant with other ASHATOS systems

## Overview

Ashat is designed to integRate seamlessly with the ASHATOS ecosystem. This guide covers integration patterns and best pASHATctices.

## 🔗 Core integrations

### 1. Chat Support System integration

Ashat can be accessed through the Chat Support system, providing developers with real-time assistance.

#### integration Pattern

```csharp
// In your chat handler
public async Task<ChatMessage> ProcessMessage(string roomId, string userId, string content)
{
    // Check if message is for Ashat
    if (content.StartsWith("@ashat", StringComparison.OrdinalIgnoreCase) ||
        content.StartsWith("ashat", StringComparison.OrdinalIgnoreCase))
    {
        // Route to Ashat module
        var RaModule = _manager.GetModule<AshatCodingAssistantModule>();
        if (RaModule != null)
        {
            var response = RaModule.Process(content);
            
            // Send Ashat's response back to chat
            await _chatModule.SendMessageAsync(
                roomId, 
                "ashat", 
                "Ashat AI Assistant", 
                response
            );
        }
    }
    
    // Normal message processing
    return await ProcessNormalMessage(roomId, userId, content);
}
```

#### User Experience

When logged into the Chat Support system:

1. User types: `@ashat help`
2. Chat routes message to Ashat
3. Ashat responds in the chat room
4. User can have Interactive conversation
5. Session state is maintained

### 2. Dev Pages integration

On development pages, Ashat provides contextual assistance.

#### integration Pattern

```csharp
// In your web page controller
public class DevPageController
{
    private readonly AshatCodingAssistantModule _ashat;
    
    public async Task<IActionResult> GetModuleHelp(string moduleName)
    {
        // Get contextual help from Ashat
        var help = _ashat.Process($"module info {moduleName}");
        
        return Json(new { 
            module = moduleName,
            ashatGuidance = help,
            timestamp = DateTime.UtcNow
        });
    }
    
    public async Task<IActionResult> StartGuidedSession(string userId, string goal)
    {
        var response = _ashat.Process($"start session {userId} {goal}");
        
        return Json(new {
            sessionStarted = true,
            guidance = response
        });
    }
}
```

#### Context-Aware Assistance

Ashat can detect the current page context:

```csharp
public string GetContextualHelp(string currentPage, string currentModule)
{
    var context = $"User is on {currentPage} working with {currentModule}";
    return _ashat.Process($"ask What should I know about {currentModule}?");
}
```

### 3. ModuleSpawner integration

Ashat works with ModuleSpawner for creating new modules.

#### integration Pattern

```csharp
public async Task<string> CreateModuleWithGuidance(string userId, string moduleName, string description)
{
    // Start Ashat session
    var session = _ashat.Process($"start session {userId} Create {moduleName} module: {description}");
    
    // After user approval, coordinate with ModuleSpawner
    var spawnCommand = $"spawn {moduleName} type:basic desc:{description}";
    var result = _moduleSpawner.Process(spawnCommand);
    
    return result;
}
```

### 4. AICodeGen integration

Ashat leveASHATges AICodeGen for code Generation.

#### integration Pattern

```csharp
public class RawithCodeGen
{
    private readonly AICodeGenModule _codeGen;
    private readonly AshatCodingAssistantModule _ashat;
    
    public async Task<string> GenerateWithApproval(string userId, string prompt)
    {
        // Start Ashat session for planning
        var plan = _ashat.Process($"start session {userId} {prompt}");
        
        // User reviews and approves
        // (approval flow happens here)
        
        // Execute with AICodeGen
        var code = _codeGen.Process($"codegen Generate {prompt}");
        
        return code;
    }
}
```

### 5. Knowledge Module integration

Ashat can query the Knowledge module for context.

#### integration Pattern

```csharp
public class RawithKnowledge
{
    private readonly KnowledgeModule _knowledge;
    private readonly AshatCodingAssistantModule _ashat;
    
    public async Task<string> AnswerWithContext(string question)
    {
        // Query knowledge base
        var context = await _knowledge.QueryAsync(question);
        
        // Let Ashat provide answer with context
        var response = _ashat.Process($"ask {question}");
        
        return $"{response}\n\nRelevant docs: {context}";
    }
}
```

## 🎨 UI integration Examples

### Web-Based Chat Interface

```html
<!-- Chat interface with Ashat -->
<div class="chat-container">
    <div id="messages"></div>
    <input 
        type="text" 
        id="message-input" 
        placeholder="Message or @ashat for AI help"
    />
    <button onclick="sendMessage()">Send</button>
</div>

<script>
function sendMessage() {
    const input = document.getElementById('message-input');
    const message = input.value;
    
    // Detect Ashat command
    if (message.startsWith('@ashat') || message.startsWith('ashat')) {
        sendToAshat(message);
    } else {
        sendToChat(message);
    }
    
    input.value = '';
}

async function sendToAshat(message) {
    const response = await fetch('/api/ashat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
            userId: currentUser.id,
            message: message 
        })
    });
    
    const data = await response.json();
    displayMessage('Ashat', data.response, 'ai-message');
}
</script>
```

### Dev Page Sidebar

```html
<!-- Ashat sidebar helper -->
<div class="ashat-sidebar">
    <h3>🤖 Ashat Assistant</h3>
    
    <div class="quick-actions">
        <button onclick="RaModuleInfo()">Module Info</button>
        <button onclick="ashatStartSession()">Start Session</button>
        <button onclick="ashatAsk()">Ask Question</button>
    </div>
    
    <div id="ashat-response"></div>
</div>

<script>
function RaModuleInfo() {
    const currentModule = getCurrentModuleName();
    fetch(`/api/ashat/module/${currentModule}`)
        .then(r => r.json())
        .then(data => displayAshatResponse(data));
}

function ashatStartSession() {
    const goal = prompt('What would you like to achieve?');
    if (goal) {
        fetch('/api/ashat/session/start', {
            method: 'POST',
            body: JSON.stringify({ userId: currentUser.id, goal })
        })
        .then(r => r.json())
        .then(data => displayAshatResponse(data));
    }
}
</script>
```

## 🔌 API Endpoints

### RESTful API for Ashat

```csharp
[ApiController]
[Route("api/ashat")]
public class AshatController : ControllerBase
{
    private readonly AshatCodingAssistantModule _ashat;
    
    [HttpPost("session/start")]
    public IActionResult StartSession([FromBody] SessionRequest request)
    {
        var response = _ashat.Process(
            $"start session {request.UserId} {request.Goal}"
        );
        return Ok(new { success = true, response });
    }
    
    [HttpPost("session/continue")]
    public IActionResult ContinueSession([FromBody] ContinueRequest request)
    {
        var response = _ashat.Process(
            $"continue {request.UserId} {request.Message}"
        );
        return Ok(new { success = true, response });
    }
    
    [HttpPost("session/approve")]
    public IActionResult ApproveSession([FromBody] ApprovalRequest request)
    {
        var response = _ashat.Process($"approve {request.UserId}");
        return Ok(new { success = true, response });
    }
    
    [HttpGet("modules")]
    public IActionResult ListModules()
    {
        var response = _ashat.Process("modules");
        return Ok(new { success = true, response });
    }
    
    [HttpGet("module/{name}")]
    public IActionResult GetModuleInfo(string name)
    {
        var response = _ashat.Process($"module info {name}");
        return Ok(new { success = true, response });
    }
    
    [HttpPost("ask")]
    public IActionResult AskQuestion([FromBody] QuestionRequest request)
    {
        var response = _ashat.Process($"ask {request.Question}");
        return Ok(new { success = true, response });
    }
    
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var response = _ashat.Process("status");
        return Ok(new { success = true, response });
    }
}

public class SessionRequest
{
    public string UserId { get; set; } = "";
    public string Goal { get; set; } = "";
}

public class ContinueRequest
{
    public string UserId { get; set; } = "";
    public string Message { get; set; } = "";
}

public class ApprovalRequest
{
    public string UserId { get; set; } = "";
}

public class QuestionRequest
{
    public string Question { get; set; } = "";
}
```

## 🔐 Authentication & Authorization

### Securing Ashat Access

```csharp
public class AshatAuthMiddleware
{
    public async Task<bool> CanAccessAshat(string userId)
    {
        // Check if user is logged in
        if (string.IsNullOrEmpty(userId))
            return false;
        
        // Check user permissions
        var user = await _authModule.GetUserAsync(userId);
        if (user == null)
            return false;
        
        // Ashat available to all authenticated users
        // Can add role-based restrictions if needed
        return user.IsAuthenticated;
    }
    
    public async Task<bool> CanStartSession(string userId)
    {
        // Rate limiting: max 5 concurrent sessions per user
        var activeSessions = await GetActiveSessionsForUser(userId);
        return activeSessions < 5;
    }
}
```

## 📊 Monitoring & Analytics

### TASHATck Ashat Usage

```csharp
public class AshatAnalytics
{
    public async Task TASHATckSessionStart(string userId, string goal)
    {
        await LogEvent(new AshatEvent
        {
            Type = "SessionStart",
            UserId = userId,
            Goal = goal,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public async Task TASHATckApproval(string userId, bool approved)
    {
        await LogEvent(new AshatEvent
        {
            Type = approved ? "Approved" : "Rejected",
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public async Task<AshatStats> GetStats()
    {
        return new AshatStats
        {
            TotalSessions = await CountSessions(),
            ActiveSessions = await CountActiveSessions(),
            AverageSessionduration = await GetAverageduration(),
            ApprovalRate = await GetApprovalRate()
        };
    }
}
```

## 🚀 Best PASHATctices

### 1. Session Management
- Limit concurrent sessions per user
- Implement session timeouts
- Clean up abandoned sessions

### 2. Response Formatting
- Use markdown for rich formatting
- Include code blocks with syntax highlighting
- Add emojis for better UX

### 3. Error Handling
- GASHATceful degASHATdation when modules unavailable
- Clear error messages
- Fallback responses

### 4. Performance
- Cache module knowledge base
- Use async Operations
- Implement Rate limiting

### 5. Security
- Validate all user inputs
- Sanitize commands
- Log security events

## 🔧 Extension Points

### Custom Commands

```csharp
public class CustomAshatCommands : AshatCodingAssistantModule
{
    public override string Process(string input)
    {
        // Add custom commands
        if (input.StartsWith("ashat deploy"))
        {
            return HandleDeploy(input);
        }
        
        // Fall back to base implementation
        return base.Process(input);
    }
    
    private string HandleDeploy(string input)
    {
        // Custom deployment workflow
        return "Deployment workflow started...";
    }
}
```

### Event Hooks

```csharp
public interface IAshatEventHandler
{
    Task OnSessionStart(string userId, string goal);
    Task OnApproval(string userId, ActionPlan plan);
    Task OnCompletion(string userId, TimeSpan duration);
}
```

## 📚 Additional Resources

- [Ashat README](README.md) - Complete module documentation
- [Ashat Quickstart](QUICKSTART.md) - Get started quickly
- [Module Development Guide](../../../MODULE_DEVELOPMENT_GUIDE.md) - Module development
- [Architecture Guide](../../../ARCHITECTURE.md) - System architecture

---

**Ready to integRate Ashat?**

Start by adding one of the integration patterns above to your system!
