# ASHAT Headless Host Architecture

## Overview

The ASHAT Headless Host service allows the AI assistant to run autonomously without a GUI, interfacing directly with ASHATOS endpoints for all heavy AI processing. This architecture supports:

- Standalone service operation (no GUI required)
- Configurable ASHATOS server endpoints
- Robust session management
- Consent-aware persistent memory (opt-in only)
- Roman goddess persona shaping
- Clear separation of UI and host logic for reusability

## Architecture Components

### 1. Configuration System (`AshatHostConfiguration.cs`)

The configuration system loads settings from `appsettings.json` and provides:

- **AshatHost**: Server URL, mode, and logging settings
- **AshatosEndpoints**: API endpoint paths for LLM, TTS, ASR, and Memory
- **Session**: Session management settings including consent requirements
- **Persona**: Roman goddess personality configuration
- **SearchEngine**: Search engine integration settings (DuckDuckGo, Google, Bing, or custom)

Configuration is automatically validated at startup to ensure all settings are correct.

### 2. ASHATOS API Client (`AshatosApiClient.cs`)

The API client handles all communication with ASHATOS endpoints:

- **LLM (Language Model)**: Send chat messages and receive responses
- **TTS (Text-to-Speech)**: Convert text to speech audio
- **ASR (Automatic Speech Recognition)**: Transcribe audio to text
- **Memory**: Store and retrieve conversation memories in vector database

### 3. Session Manager (`SessionManager.cs`)

Manages session state and conversation history:

- Create and track sessions
- Store conversation history with configurable limits
- Consent-based persistent memory control
- Automatic session cleanup and expiration

### 4. Host Service (`AshatHostService.cs`)

The main headless host service that coordinates all components:

- Initialize and start/stop the service
- Process user messages through ASHATOS LLM
- Handle TTS and ASR requests
- Manage session-based memory with consent
- Fallback to local responses when server is unavailable

## Running ASHAT in Headless Mode

### Basic Usage

```bash
# Run in headless mode with default configuration
./ASHAT --headless

# Run with custom configuration file
./ASHAT --headless --config /path/to/config.json
```

### Configuration File

Create or modify `appsettings.json`:

```json
{
  "AshatHost": {
    "ServerUrl": "http://agpstudios.online",
    "Mode": "headless",
    "EnableLogging": true
  },
  "AshatosEndpoints": {
    "LLM": "/api/llm/chat",
    "TTS": "/api/tts/speak",
    "ASR": "/api/asr/transcribe",
    "Memory": "/api/memory",
    "Health": "/health"
  },
  "Session": {
    "PersistentMemory": false,
    "ConsentRequired": true,
    "SessionTimeout": 3600,
    "MaxHistoryLength": 100
  },
  "Persona": {
    "Name": "ASHAT",
    "Type": "RomanGoddess",
    "Personality": "wise, playful, mischievous, respectful",
    "SystemPrompt": "You are ASHAT, a Roman goddess AI assistant..."
  },
  "SearchEngine": {
    "Enabled": false,
    "Provider": "duckduckgo",
    "ApiKey": "",
    "SearchEngineId": "",
    "ResultLimit": 10,
    "CustomEndpoint": "",
    "TimeoutSeconds": 30,
    "Region": "wt-wt",
    "SafeSearch": "moderate"
  }
}
```

For detailed configuration options and search engine setup, see `CONFIG_GUIDE.md`.

### Interactive Commands

When running in headless mode:

- Type your message and press Enter to chat with ASHAT
- Type `consent` to enable persistent memory for the session
- Type `exit` or `quit` to stop the service

## Integrating with Other Applications

The headless host can be easily integrated into other applications:

```csharp
using ASHATGoddessClient.Configuration;
using ASHATGoddessClient.Host;

// Load configuration
var config = AshatHostConfiguration.LoadFromFile("appsettings.json");

// Create and start the host service
var hostService = new AshatHostService(config);
await hostService.StartAsync();

// Create a session
var sessionId = hostService.CreateSession(consentGiven: false);

// Process messages
var response = await hostService.ProcessMessageAsync(sessionId, "Hello ASHAT!");
Console.WriteLine(response);

// Get TTS audio
var audioData = await hostService.RequestSpeechAsync("Hello, mortal!");

// Transcribe audio
var transcription = await hostService.TranscribeAudioAsync(audioBytes, "wav");

// Stop the service
hostService.Stop();
```

## Privacy and Consent

ASHAT respects user privacy through consent-based memory:

1. **By Default**: Sessions do NOT persist memories
2. **Opt-In**: Users must explicitly consent to enable persistent memory
3. **Transparent**: Users are always informed about memory status
4. **Session-Based**: Each session has independent consent settings

To enable persistent memory programmatically:

```csharp
hostService.UpdateSessionConsent(sessionId, consentGiven: true);
```

## Persona Shaping

The Roman goddess persona is defined in configuration:

- **Name**: ASHAT
- **Type**: RomanGoddess
- **Personality**: Wise, playful, mischievous, respectful
- **System Prompt**: Guides the AI's responses and behavior

Customize the persona in `appsettings.json` to adjust ASHAT's personality and behavior.

## Search Engine Integration

ASHAT can be configured to use external search engines for web search capabilities:

### Supported Search Providers

1. **DuckDuckGo** (default, no API key required)
2. **Google Custom Search** (requires API key and search engine ID)
3. **Bing Web Search** (requires Azure API key)
4. **Custom** (for custom search API endpoints)

### Configuration Examples

**DuckDuckGo (Easiest Setup):**
```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "duckduckgo",
    "ResultLimit": 10,
    "Region": "wt-wt",
    "SafeSearch": "moderate"
  }
}
```

**Google Custom Search:**
```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "google",
    "ApiKey": "YOUR_GOOGLE_API_KEY",
    "SearchEngineId": "YOUR_SEARCH_ENGINE_ID",
    "ResultLimit": 10,
    "Region": "en-US"
  }
}
```

**Bing Web Search:**
```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "bing",
    "ApiKey": "YOUR_BING_API_KEY",
    "ResultLimit": 10,
    "Region": "en-US"
  }
}
```

### Quick Start with Search Templates

The repository includes pre-configured templates for each search provider:

```bash
# Use DuckDuckGo
cp appsettings.search-duckduckgo.json appsettings.json

# Use Google (remember to add your API key!)
cp appsettings.search-google.json appsettings.json

# Use Bing (remember to add your API key!)
cp appsettings.search-bing.json appsettings.json
```

For detailed search engine setup instructions, see `CONFIG_GUIDE.md`.

## Fallback Mode

When ASHATOS server is unavailable, the service operates in fallback mode:

- Uses local response generation for basic interactions
- Maintains session management
- No TTS, ASR, or persistent memory features
- Gracefully degrades functionality

## Building and Deployment

### Build

```bash
dotnet build
```

### Run

```bash
# GUI mode (default)
dotnet run

# Headless mode
dotnet run -- --headless

# Headless with custom config
dotnet run -- --headless --config myconfig.json
```

### Publish

```bash
# Self-contained for Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Self-contained for Windows
dotnet publish -c Release -r win-x64 --self-contained
```

## API Endpoints Expected from ASHATOS

The headless host expects the following ASHATOS endpoints:

### Health Check
- **Endpoint**: `/health`
- **Method**: GET
- **Response**: 200 OK

### LLM Chat
- **Endpoint**: `/api/llm/chat`
- **Method**: POST
- **Request Body**:
  ```json
  {
    "message": "user message",
    "systemPrompt": "system prompt",
    "personality": "personality traits"
  }
  ```
- **Response**:
  ```json
  {
    "response": "AI response"
  }
  ```

### TTS (Text-to-Speech)
- **Endpoint**: `/api/tts/speak`
- **Method**: POST
- **Request Body**:
  ```json
  {
    "text": "text to speak",
    "voice": "female",
    "speed": 1.0
  }
  ```
- **Response**: Audio bytes (binary)

### ASR (Speech Recognition)
- **Endpoint**: `/api/asr/transcribe`
- **Method**: POST
- **Request**: Multipart form with audio file
- **Response**:
  ```json
  {
    "transcription": "transcribed text"
  }
  ```

### Memory Storage
- **Endpoint**: `/api/memory/store`
- **Method**: POST
- **Request Body**:
  ```json
  {
    "sessionId": "session-id",
    "content": "memory content",
    "metadata": {},
    "timestamp": "2025-01-01T00:00:00Z"
  }
  ```

### Memory Retrieval
- **Endpoint**: `/api/memory/retrieve`
- **Method**: POST
- **Request Body**:
  ```json
  {
    "sessionId": "session-id",
    "query": "search query",
    "limit": 5
  }
  ```
- **Response**:
  ```json
  {
    "memories": [
      {
        "content": "memory content"
      }
    ]
  }
  ```

## Future Enhancements

- WebSocket support for real-time communication
- REST API for external applications to interact with the host
- Metrics and monitoring integration
- Multi-user session support
- Advanced memory retrieval with semantic search
- Plugin system for extensibility
