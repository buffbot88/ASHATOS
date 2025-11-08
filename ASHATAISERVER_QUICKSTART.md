# ASHAT AI Server Quick Start Guide

## 🌟 Overview

**ASHATAIServer** is a standalone AI processing server designed for the ASHAT Goddess desktop client. It provides language model processing capabilities using .gguf (GGUF format) model files and exposes a REST API for AI inference.

## 🚀 Quick Start

### 1. Navigate to Server Directory
```bash
cd ASHATAIServer
```

### 2. Start the Server

**Linux/Mac:**
```bash
./start.sh
```

**Windows:**
```cmd
start.bat
```

**Manual Start:**
```bash
dotnet run
```

### 3. Server Information
- **Port:** 8088
- **Base URL:** `http://localhost:8088`
- **Status:** Check at `http://localhost:8088/api/ai/health`

## 📁 Adding Language Models

1. Place your .gguf model files in the `models` directory:
```bash
cp /path/to/your/model.gguf ASHATAIServer/models/
```

2. The server automatically scans on startup
3. Or trigger a manual scan: `POST http://localhost:8088/api/ai/models/scan`

### Where to Get Models

Download .gguf models from:
- [Hugging Face](https://huggingface.co/models?search=gguf) - Search for GGUF models
- [TheBloke on Hugging Face](https://huggingface.co/TheBloke) - Popular quantized models
- [llama.cpp examples](https://github.com/ggerganov/llama.cpp#models)

Popular models:
- Llama 2 7B (Q4_K_M) - ~4GB
- Mistral 7B (Q4_K_M) - ~4GB
- CodeLlama 7B (Q4_K_M) - ~4GB

## 🔌 API Endpoints

### Health Check
```bash
curl http://localhost:8088/api/ai/health
```

### Get Model Status
```bash
curl http://localhost:8088/api/ai/status
```

### Process AI Prompt
```bash
curl -X POST http://localhost:8088/api/ai/process \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hello, ASHAT!"}'
```

### Scan for New Models
```bash
curl -X POST http://localhost:8088/api/ai/models/scan
```

## 🤝 ASHAT Goddess Client Integration

Configure your ASHAT Goddess client to connect to this server:

```csharp
// In ASHATGoddessClient configuration
var serverUrl = "http://localhost:8088";
var brain = new AshatBrain(serverUrl);
```

Or if running on a different machine:
```csharp
var serverUrl = "http://192.168.1.100:8088";  // Replace with server IP
```

## 📊 Testing the Server

### Test 1: Health Check
```bash
curl http://localhost:8088/api/ai/health
```
Expected response:
```json
{
  "status": "healthy",
  "server": "ASHATAIServer",
  "timestamp": "2025-11-08T20:00:00Z"
}
```

### Test 2: Check Loaded Models
```bash
curl http://localhost:8088/api/ai/status
```
Expected response:
```json
{
  "modelsDirectory": "models",
  "loadedModels": [...],
  "failedModels": []
}
```

### Test 3: Process a Prompt (requires a loaded model)
```bash
curl -X POST http://localhost:8088/api/ai/process \
  -H "Content-Type: application/json" \
  -d '{"prompt": "What is AI?"}'
```

## 🛠️ Configuration

### Port Configuration
Edit `Program.cs` to change the port:
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8088);  // Change to desired port
});
```

### Models Directory
Edit `appsettings.json`:
```json
{
  "ModelsDirectory": "models"  // Change to desired path
}
```

## 🔒 Security for Production

Before deploying to production:

1. **Enable HTTPS:**
   - Configure SSL certificates
   - Update Kestrel configuration

2. **Add Authentication:**
   - Implement API key validation
   - Add JWT token authentication

3. **Restrict CORS:**
   - Limit to specific origins
   - Remove `AllowAnyOrigin()`

4. **Rate Limiting:**
   - Add rate limiting middleware
   - Prevent abuse

See `README.md` in ASHATAIServer for detailed security guidelines.

## 🐛 Troubleshooting

### Server Won't Start
**Issue:** Port 8088 already in use

**Solution:**
```bash
# Find process using port 8088
netstat -ano | findstr :8088    # Windows
lsof -i :8088                   # Linux/Mac

# Kill the process or change the port in Program.cs
```

### No Models Loading
**Issue:** Models directory empty

**Solution:**
```bash
# Check if models directory exists
ls ASHATAIServer/models/

# Add .gguf files
cp your-model.gguf ASHATAIServer/models/
```

### Model Validation Fails
**Issue:** Invalid GGUF format

**Solution:**
- Verify the file is a proper .gguf file
- Re-download the model if corrupted
- Check file permissions

### Connection Refused
**Issue:** Cannot connect from ASHAT Goddess client

**Solution:**
- Check server is running: `curl http://localhost:8088/api/ai/health`
- Verify port 8088 is not blocked by firewall
- If on different machines, use server IP instead of localhost

## 📚 Additional Documentation

- **Full Documentation:** [ASHATAIServer/README.md](ASHATAIServer/README.md)
- **Client Examples:** [ASHATAIServer/Examples.md](ASHATAIServer/Examples.md)
- **API Reference:** See README.md for detailed endpoint documentation
- **ASHAT Goddess Client:** [ASHATGoddessClient/README.md](ASHATGoddessClient/README.md)

## 📝 Quick Reference

| Task | Command |
|------|---------|
| Start server | `cd ASHATAIServer && ./start.sh` |
| Check health | `curl http://localhost:8088/api/ai/health` |
| Get status | `curl http://localhost:8088/api/ai/status` |
| Process prompt | `curl -X POST http://localhost:8088/api/ai/process -H "Content-Type: application/json" -d '{"prompt":"Hello"}'` |
| Add model | `cp model.gguf ASHATAIServer/models/` |
| Rescan models | `curl -X POST http://localhost:8088/api/ai/models/scan` |

## 🎯 Next Steps

1. **Start the server:** `cd ASHATAIServer && ./start.sh`
2. **Add models:** Place .gguf files in the models directory
3. **Test endpoints:** Use curl or Postman to test the API
4. **Integrate with ASHAT Goddess:** Configure client to connect to this server
5. **Deploy to production:** Follow security guidelines before deploying

---

**ASHATAIServer v1.0.0**  
Port: 8088  
Status: Production Ready ✅

For support, see the main repository documentation or open an issue on GitHub.
