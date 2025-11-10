# ASHAT Usage Examples

## Running ASHAT

### GUI Mode (Default)
```bash
dotnet run
```

### Headless Mode
```bash
dotnet run -- --headless
```

### Headless Mode with Custom Config
```bash
dotnet run -- --headless --config my_custom_config.json
```

## Integrating ASHAT Host Service in Your Application

### Basic Integration

```csharp
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration
        var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
        
        // Create and start the host service
        var hostService = new AshatHostService(config);
        await hostService.StartAsync();
        
        // Create a session
        var sessionId = hostService.CreateSession(consentGiven: false);
        
        // Process messages
        var response = await hostService.ProcessMessageAsync(
            sessionId, 
            "Hello ASHAT!"
        );
        Console.WriteLine($"ASHAT: {response}");
        
        // Stop the service
        hostService.Stop();
    }
}
```

### Advanced Usage with Consent and Memory

```csharp
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;

class Program
{
    static async Task Main(string[] args)
    {
        var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
        var hostService = new AshatHostService(config);
        await hostService.StartAsync();
        
        // Create a session with consent
        var sessionId = hostService.CreateSession(consentGiven: true);
        
        // Process multiple messages
        var messages = new[] {
            "Hello ASHAT, my name is John",
            "What is my name?",
            "Tell me about Roman goddesses"
        };
        
        foreach (var message in messages)
        {
            var response = await hostService.ProcessMessageAsync(sessionId, message);
            Console.WriteLine($"User: {message}");
            Console.WriteLine($"ASHAT: {response}");
            Console.WriteLine();
        }
        
        // Retrieve conversation history
        var history = hostService.GetSessionHistory(sessionId, limit: 10);
        Console.WriteLine($"Total messages in history: {history.Count}");
        
        hostService.Stop();
    }
}
```

### Using TTS (Text-to-Speech)

```csharp
var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
var hostService = new AshatHostService(config);
await hostService.StartAsync();

// Request speech synthesis
var audioData = await hostService.RequestSpeechAsync(
    "Greetings, mortal! I am ASHAT, your divine companion."
);

if (audioData != null)
{
    // Save audio to file
    await File.WriteAllBytesAsync("ashat_greeting.wav", audioData);
    Console.WriteLine("Audio saved!");
}

hostService.Stop();
```

### Using ASR (Speech Recognition)

```csharp
var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
var hostService = new AshatHostService(config);
await hostService.StartAsync();

// Load audio file
var audioBytes = await File.ReadAllBytesAsync("user_audio.wav");

// Transcribe audio
var transcription = await hostService.TranscribeAudioAsync(audioBytes, "wav");

if (transcription != null)
{
    Console.WriteLine($"User said: {transcription}");
    
    // Process the transcribed message
    var sessionId = hostService.CreateSession();
    var response = await hostService.ProcessMessageAsync(sessionId, transcription);
    Console.WriteLine($"ASHAT: {response}");
}

hostService.Stop();
```

### Custom Configuration

```csharp
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;

class Program
{
    static async Task Main(string[] args)
    {
        // Create custom configuration
        var config = new AshatHostConfiguration
        {
            AshatHost = new AshatHostSettings
            {
                ServerUrl = "http://my-custom-server.com",
                Mode = "headless",
                EnableLogging = true
            },
            AshatosEndpoints = new AshatosEndpointsSettings
            {
                LLM = "/api/v2/chat",
                TTS = "/api/v2/tts",
                ASR = "/api/v2/asr",
                Memory = "/api/v2/memory",
                Health = "/api/v2/health"
            },
            Session = new SessionSettings
            {
                PersistentMemory = true,
                ConsentRequired = false, // Auto-consent for testing
                SessionTimeout = 7200,
                MaxHistoryLength = 200
            },
            Persona = new PersonaSettings
            {
                Name = "Custom ASHAT",
                Type = "RomanGoddess",
                Personality = "wise, powerful, mysterious",
                SystemPrompt = "You are a powerful Roman goddess..."
            }
        };
        
        var hostService = new AshatHostService(config);
        await hostService.StartAsync();
        
        // Use the service...
        
        hostService.Stop();
    }
}
```

### Web API Integration Example

```csharp
using Microsoft.AspNetCore.Mvc;
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;

[ApiController]
[Route("api/ashat")]
public class AshatController : ControllerBase
{
    private static AshatHostService? _hostService;
    
    static AshatController()
    {
        var config = AshatHostConfiguration.LoadFromFile("appsettings.json");
        _hostService = new AshatHostService(config);
        _hostService.StartAsync().Wait();
    }
    
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (_hostService == null)
            return StatusCode(500, "Service not initialized");
            
        var response = await _hostService.ProcessMessageAsync(
            request.SessionId, 
            request.Message
        );
        
        return Ok(new { response });
    }
    
    [HttpPost("session")]
    public IActionResult CreateSession([FromBody] SessionRequest request)
    {
        if (_hostService == null)
            return StatusCode(500, "Service not initialized");
            
        var sessionId = _hostService.CreateSession(request.ConsentGiven);
        return Ok(new { sessionId });
    }
    
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        if (_hostService == null)
            return StatusCode(500, "Service not initialized");
            
        return Ok(new 
        { 
            isRunning = _hostService.IsRunning,
            activeSessions = _hostService.GetActiveSessionCount()
        });
    }
}

public class ChatRequest
{
    public string SessionId { get; set; } = "";
    public string Message { get; set; } = "";
}

public class SessionRequest
{
    public bool ConsentGiven { get; set; }
}
```

## Configuration Examples

### Minimal Configuration
```json
{
  "AshatHost": {
    "ServerUrl": "http://localhost:80"
  }
}
```

### Production Configuration
```json
{
  "AshatHost": {
    "ServerUrl": "https://ashatos.production.com",
    "Mode": "headless",
    "EnableLogging": false
  },
  "Session": {
    "PersistentMemory": true,
    "ConsentRequired": true,
    "SessionTimeout": 7200,
    "MaxHistoryLength": 200
  }
}
```

### Development Configuration
```json
{
  "AshatHost": {
    "ServerUrl": "http://localhost:5000",
    "Mode": "headless",
    "EnableLogging": true
  },
  "Session": {
    "PersistentMemory": false,
    "ConsentRequired": false,
    "SessionTimeout": 600,
    "MaxHistoryLength": 50
  }
}
```

## Testing

Run the test suite:
```bash
./test_headless.sh
```

## Building and Publishing

### Debug Build
```bash
dotnet build
```

### Release Build
```bash
dotnet build -c Release
```

### Self-Contained Executable (Linux)
```bash
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish/linux
```

### Self-Contained Executable (Windows)
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/windows
```

### Self-Contained Executable (macOS)
```bash
dotnet publish -c Release -r osx-x64 --self-contained -o ./publish/macos
```

## Troubleshooting

### Server Connection Issues
If you see "ASHATOS server is not responding":
1. Check that the server URL in `appsettings.json` is correct
2. Verify the server is running and accessible
3. Check firewall settings
4. The service will fall back to local responses automatically

### Configuration Not Loading
If configuration isn't loading:
1. Ensure `appsettings.json` exists in the same directory as the executable
2. Check JSON syntax is valid
3. Verify file permissions allow reading
4. Use `--config` flag to specify a custom path

### Memory/History Issues
If memory isn't working:
1. Verify `PersistentMemory` is set to `true` in configuration
2. Check that consent was given: `hostService.UpdateSessionConsent(sessionId, true)`
3. Ensure ASHATOS server is connected (memory requires server)
4. Check server logs for memory endpoint errors
