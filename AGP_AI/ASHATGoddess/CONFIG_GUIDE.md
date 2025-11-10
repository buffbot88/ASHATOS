# ASHAT Configuration Guide

This guide provides detailed information about configuring the ASHAT Headless Host service.

## Configuration File

ASHAT uses a JSON configuration file (`appsettings.json`) to manage all settings. The configuration file is loaded at startup and validated automatically.

## Configuration Sections

### AshatHost

Controls the core host service settings.

```json
{
  "AshatHost": {
    "ServerUrl": "http://agpstudios.online",
    "Mode": "headless",
    "EnableLogging": true
  }
}
```

**Settings:**
- `ServerUrl` (string, required): The URL of the ASHATOS backend server. Default: `http://agpstudios.online`
- `Mode` (string): Operating mode. Use `"headless"` for server operation. Default: `"headless"`
- `EnableLogging` (boolean): Enable/disable console logging. Default: `true`

### AshatosEndpoints

Defines the API endpoint paths on the ASHATOS server.

```json
{
  "AshatosEndpoints": {
    "LLM": "/api/llm/chat",
    "TTS": "/api/tts/speak",
    "ASR": "/api/asr/transcribe",
    "Memory": "/api/memory",
    "Health": "/health"
  }
}
```

**Settings:**
- `LLM` (string): Language model chat endpoint. Default: `"/api/llm/chat"`
- `TTS` (string): Text-to-speech endpoint. Default: `"/api/tts/speak"`
- `ASR` (string): Automatic speech recognition endpoint. Default: `"/api/asr/transcribe"`
- `Memory` (string): Memory storage/retrieval endpoint. Default: `"/api/memory"`
- `Health` (string): Health check endpoint. Default: `"/health"`

### Session

Manages session behavior and conversation history.

```json
{
  "Session": {
    "PersistentMemory": false,
    "ConsentRequired": true,
    "SessionTimeout": 3600,
    "MaxHistoryLength": 100
  }
}
```

**Settings:**
- `PersistentMemory` (boolean): Enable persistent memory by default. Default: `false`
- `ConsentRequired` (boolean): Require explicit consent for memory. Default: `true`
- `SessionTimeout` (integer): Session timeout in seconds. Default: `3600` (1 hour)
- `MaxHistoryLength` (integer): Maximum conversation history length. Default: `100`

### Persona

Configures ASHAT's personality and behavior.

```json
{
  "Persona": {
    "Name": "ASHAT",
    "Type": "RomanGoddess",
    "Personality": "wise, playful, mischievous, respectful",
    "Description": "A charismatic Roman goddess personality",
    "SystemPrompt": "You are ASHAT, a Roman goddess AI assistant..."
  }
}
```

**Settings:**
- `Name` (string): The persona's name. Default: `"ASHAT"`
- `Type` (string): Persona type identifier. Default: `"RomanGoddess"`
- `Personality` (string): Comma-separated personality traits. Default: `"wise, playful, mischievous, respectful"`
- `Description` (string): Brief persona description
- `SystemPrompt` (string): System prompt sent to the LLM to guide behavior

### SearchEngine

Configures search engine integration for web searches.

```json
{
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

**Settings:**
- `Enabled` (boolean): Enable/disable search functionality. Default: `false`
- `Provider` (string): Search provider. Options: `"duckduckgo"`, `"google"`, `"bing"`, `"custom"`. Default: `"duckduckgo"`
- `ApiKey` (string): API key or token (required for Google and Bing). Default: `""`
- `SearchEngineId` (string): Custom search engine ID (required for Google Custom Search). Default: `""`
- `ResultLimit` (integer): Max search results to return (1-100). Default: `10`
- `CustomEndpoint` (string): Custom API endpoint URL (used when Provider is `"custom"`). Default: `""`
- `TimeoutSeconds` (integer): Request timeout for search API calls. Default: `30`
- `Region` (string): Region/locale for results (e.g., `"en-US"`, `"wt-wt"` for no region). Default: `"wt-wt"`
- `SafeSearch` (string): Safe search setting. Options: `"off"`, `"moderate"`, `"strict"`. Default: `"moderate"`

## Search Engine Configuration Examples

### DuckDuckGo (No API Key Required)

DuckDuckGo is the easiest to set up as it doesn't require an API key.

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

**Template:** Use `appsettings.search-duckduckgo.json` as a starting point.

### Google Custom Search

Requires a Google Cloud API key and Custom Search Engine ID.

**Setup:**
1. Create a Google Cloud project at https://console.cloud.google.com/
2. Enable the Custom Search API
3. Create API credentials (API Key)
4. Create a Custom Search Engine at https://programmablesearchengine.google.com/
5. Copy your API key and Search Engine ID

```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "google",
    "ApiKey": "YOUR_GOOGLE_API_KEY",
    "SearchEngineId": "YOUR_SEARCH_ENGINE_ID",
    "ResultLimit": 10,
    "Region": "en-US",
    "SafeSearch": "moderate"
  }
}
```

**Template:** Use `appsettings.search-google.json` as a starting point.

### Bing Web Search API

Requires a Microsoft Azure subscription and Bing Search API key.

**Setup:**
1. Create an Azure account at https://portal.azure.com/
2. Create a Bing Search resource
3. Copy your API key from the resource

```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "bing",
    "ApiKey": "YOUR_BING_API_KEY",
    "ResultLimit": 10,
    "Region": "en-US",
    "SafeSearch": "moderate"
  }
}
```

**Template:** Use `appsettings.search-bing.json` as a starting point.

### Custom Search API

For integration with custom or alternative search APIs.

```json
{
  "SearchEngine": {
    "Enabled": true,
    "Provider": "custom",
    "CustomEndpoint": "https://your-search-api.com/search",
    "ApiKey": "YOUR_API_KEY_IF_NEEDED",
    "ResultLimit": 10,
    "TimeoutSeconds": 30
  }
}
```

## Configuration Validation

ASHAT automatically validates configuration settings at startup. If validation fails, the service will display error messages and exit.

**Validation Rules:**
- `AshatHost.ServerUrl` must be a valid HTTP/HTTPS URL
- `Session.SessionTimeout` must be greater than 0
- `Session.MaxHistoryLength` must be greater than 0
- When `SearchEngine.Enabled` is true:
  - `Provider` must be one of: `duckduckgo`, `google`, `bing`, `custom`
  - `ApiKey` is required for `google` and `bing` providers
  - `SearchEngineId` is required for `google` provider
  - `CustomEndpoint` must be a valid URL when provider is `custom`
  - `ResultLimit` must be between 1 and 100

## Using Configuration Files

### Default Configuration

By default, ASHAT loads `appsettings.json`:

```bash
dotnet run -- --headless
```

### Custom Configuration File

Specify a custom configuration file:

```bash
dotnet run -- --headless --config /path/to/myconfig.json
```

### Switching Between Configurations

You can maintain multiple configuration files for different scenarios:

```bash
# Use DuckDuckGo search
cp appsettings.search-duckduckgo.json appsettings.json

# Use Google search
cp appsettings.search-google.json appsettings.json

# Use Bing search
cp appsettings.search-bing.json appsettings.json
```

Or run directly with a specific file:

```bash
dotnet run -- --headless --config appsettings.search-google.json
```

## Best Practices

1. **Security**: Never commit API keys to version control. Use environment variables or secure configuration management.

2. **Templates**: Keep `appsettings.example.json` as a clean template without sensitive data.

3. **Backup**: Keep a backup of your working configuration before making changes.

4. **Testing**: Test configuration changes in a development environment first.

5. **Validation**: Always check startup logs for validation warnings or errors.

6. **Documentation**: Document any custom configurations for your team.

## Environment Variables

While not currently supported, future versions may allow overriding configuration values with environment variables:

```bash
ASHAT_ServerUrl="http://localhost:8080" dotnet run -- --headless
```

## Troubleshooting

### Configuration File Not Found

**Error:** `Configuration file not found: appsettings.json`

**Solution:** Ensure `appsettings.json` exists in the same directory as the executable, or specify the full path with `--config`.

### Invalid Server URL

**Error:** `AshatHost.ServerUrl must be a valid HTTP/HTTPS URL`

**Solution:** Check that the URL starts with `http://` or `https://` and is properly formatted.

### Search API Key Missing

**Error:** `SearchEngine.ApiKey is required for provider: google`

**Solution:** Add your API key to the configuration file. Never use placeholder text like "YOUR_API_KEY".

### Invalid Result Limit

**Error:** `SearchEngine.ResultLimit must be between 1 and 100`

**Solution:** Set `ResultLimit` to a value between 1 and 100.

## Support

For more information, see:
- `HEADLESS_README.md` - Headless host architecture
- `USAGE_EXAMPLES.md` - Usage examples
- Issue tracker: https://github.com/buffbot88/ASHATGoddess/issues
