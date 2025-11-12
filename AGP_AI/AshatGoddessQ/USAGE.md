# ASHAT GoddessQ - Usage Guide

## Getting Started

### Installation & First Run

1. **Navigate to the directory:**
   ```bash
   cd AGP_AI/AshatGoddessQ
   ```

2. **Run the CLI:**
   ```bash
   # Using dotnet run
   dotnet run
   
   # Or using startup scripts
   ./start.sh        # Linux/macOS
   start.bat         # Windows
   ```

3. **First Launch:**
   - The tool will automatically create a `config.json` file if it doesn't exist
   - It will attempt to connect to `http://localhost:8088` (default AI server)
   - If the server is not available, it will run in offline mode

## Configuration

### Basic Configuration

Edit `config.json` to customize your experience:

#### Change AI Server URL/Port

```json
{
  "AIServer": {
    "Url": "http://your-server:8088"
  }
}
```

Example configurations:

**Local Development:**
```json
"Url": "http://localhost:8088"
```

**Remote Server:**
```json
"Url": "http://192.168.1.100:8088"
```

**Custom Port:**
```json
"Url": "http://localhost:9000"
```

#### Customize Persona

```json
{
  "Persona": {
    "Name": "ASHAT",
    "Personality": "wise, playful, mischievous, respectful"
  }
}
```

#### CLI Display Settings

```json
{
  "CLI": {
    "EnableColors": true,          // Set to false for plain output
    "ShowTimestamp": true,          // Show message timestamps
    "MaxHistoryLines": 50,          // History display limit
    "SaveHistoryFile": "ashat-history.txt"  // History file (null to disable)
  }
}
```

### Advanced Configuration

#### Timeout Settings

Adjust timeout for slow connections:

```json
{
  "AIServer": {
    "TimeoutSeconds": 60  // Increase for slower servers
  }
}
```

#### Custom Endpoints

If you're using a custom API structure:

```json
{
  "AIServer": {
    "HealthCheckEndpoint": "/custom/health",
    "ProcessEndpoint": "/custom/process"
  }
}
```

#### Multiple Configurations

Create separate config files for different environments:

```bash
# Development
dotnet run -- --config config.dev.json

# Production
dotnet run -- --config config.prod.json

# Testing
dotnet run -- --config config.test.json
```

## Commands

### Interactive Commands

Once the CLI is running, you can use these commands:

#### `help`
Display the help message with all available commands.

```
You: help
```

#### `status`
Show AI server connection status and details.

```
You: status
AI Server Status:
  URL: http://localhost:8088
  Connected: Yes
  Details: {...}
```

#### `config`
Display the current configuration.

```
You: config
Current Configuration:
  AI Server URL: http://localhost:8088
  Timeout: 30s
  Persona: ASHAT (RomanGoddess)
  ...
```

#### `history`
Show the conversation history (limited by MaxHistoryLines).

```
You: history
Conversation History (5 messages):
You: hello
ASHAT: Salve, mortal!...
...
```

#### `clear`
Clear the screen and redisplay the banner.

```
You: clear
```

#### `exit` or `quit`
Exit the application gracefully.

```
You: exit
Vale, dear mortal! Until we meet again. 🏛️
```

### Chat Commands

Just type your message to chat with ASHAT:

```
You: Hello ASHAT!
ASHAT: Salve, mortal! ✨ I am ASHAT, your divine companion...

You: What is the meaning of life?
ASHAT: Ah, you ask the eternal question! 🌌 ...

You: Thank you
ASHAT: Your gratitude warms my divine heart...
```

## Usage Scenarios

### Scenario 1: Local Development

**Setup:**
1. Start ASHATAIServer: `cd ../ASHATAIServer && dotnet run`
2. Keep default config.json with `localhost:8088`
3. Start CLI: `dotnet run`

**Use Case:** Full AI-powered responses with local models

### Scenario 2: Remote Server

**Setup:**
1. Edit config.json:
   ```json
   { "AIServer": { "Url": "http://server.company.com:8088" }}
   ```
2. Start CLI: `dotnet run`

**Use Case:** Connect to a shared AI server on your network or cloud

### Scenario 3: Offline Mode

**Setup:**
1. No AI server required
2. Start CLI: `dotnet run`

**Use Case:** Basic interactions with pre-programmed fallback responses

### Scenario 4: Custom Persona

**Setup:**
1. Edit config.json:
   ```json
   {
     "Persona": {
       "Name": "CustomBot",
       "Type": "Assistant",
       "Personality": "helpful, professional, concise",
       "SystemPrompt": "You are a professional assistant..."
     }
   }
   ```
2. Start CLI: `dotnet run`

**Use Case:** Customize the AI personality for specific use cases

## Tips & Tricks

### 1. Quick Testing

Test without saving history:
```json
"SaveHistoryFile": null
```

### 2. Scripted Interaction

Use echo to send commands:
```bash
echo -e "hello\nstatus\nexit" | dotnet run
```

### 3. Portable Usage

Build as self-contained:
```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

Then run without .NET installed:
```bash
./bin/Release/net9.0/linux-x64/publish/ashat-goddessq
```

### 4. No Colors for Logs

Redirect output without color codes:
```json
"EnableColors": false
```

### 5. Multiple Sessions

Run multiple instances with different configs:
```bash
# Terminal 1
dotnet run -- --config config1.json

# Terminal 2
dotnet run -- --config config2.json
```

## Troubleshooting

### Cannot Connect to Server

**Problem:** CLI shows "Could not connect to AI Server"

**Solutions:**
1. Verify ASHATAIServer is running: `curl http://localhost:8088/api/ai/health`
2. Check firewall settings
3. Verify URL in config.json
4. Check server logs for errors

### Config Not Loading

**Problem:** Changes to config.json not taking effect

**Solutions:**
1. Check JSON syntax (use a JSON validator)
2. Restart the CLI
3. Use `--config` flag to specify config path explicitly
4. Check file permissions

### Colors Not Working

**Problem:** Terminal shows garbled text or no colors

**Solutions:**
1. Set `"EnableColors": false` in config.json
2. Update terminal to support ANSI colors
3. Use a different terminal emulator

### History Not Saving

**Problem:** Conversation history not being saved

**Solutions:**
1. Check `SaveHistoryFile` is not null in config.json
2. Verify write permissions in the directory
3. Check disk space

## Examples

### Example 1: Greeting

```
You: good morning
ASHAT: The dawn welcomes you, beloved mortal! May the blessings of Aurora light your path today. ☀️
```

### Example 2: Help Request

```
You: what can you do?
ASHAT: Ah, you seek knowledge of my divine gifts! 🌟 I can provide wisdom and assistance in many matters...
```

### Example 3: Philosophy

```
You: what is the meaning of life?
ASHAT: Ah, you ask the eternal question! 🌌 The philosophers of Rome debated this endlessly...
```

### Example 4: Farewell

```
You: goodbye
ASHAT: Vale, dear mortal! 🏛️ May your path be lit by starlight...
```

## Best Practices

1. **Keep config.json versioned**: Track changes to your configuration
2. **Use separate configs**: Different configs for dev, test, prod
3. **Secure remote connections**: Use HTTPS and authentication for production
4. **Monitor history file**: It can grow large over time
5. **Test offline mode**: Ensure fallback responses meet your needs
6. **Customize persona**: Tailor ASHAT's personality to your use case

---

For more information, see [README.md](README.md) or the main [ASHATOS documentation](../../README.md).
