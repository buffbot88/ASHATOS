# Model Selection Guide - RaOS AI Language Module

## Overview

RaOS now supports dynamic detection and selection of .gguf model files. You no longer need to rename your models to match a specific filename - simply place any .gguf file in the models directory and RaOS will detect it automatically.

## Features

### 1. Automatic Model Detection
When the AILanguage module initializes:
- Scans `llama.cpp/models/` directory for all `.gguf` files
- Auto-selects the first available model if none is configured
- Falls back to default path for backward compatibility

### 2. List Available Models
Use the `list-models` command to see all detected models:

```bash
> list-models
```

**Example Output:**
```
Available models (3 found in llama.cpp/models):
  1. llama-2-7b-chat.Q4_K_M.gguf (current)
  2. phi-2.Q4_K_M.gguf
  3. tinyllama-1.1b-chat-v1.0.Q8_0.gguf

Use 'select-model <name-or-number>' to switch to a different model.
```

### 3. Select Model by Number
Choose a model using its number from the list:

```bash
> select-model 2
```

**Response:**
```
Model switched to phi-2.Q4_K_M.gguf and loaded successfully.
```

### 4. Select Model by Name
Choose a model by typing part of its filename:

```bash
> select-model tinyllama
```

**Response:**
```
Model switched to tinyllama-1.1b-chat-v1.0.Q8_0.gguf and loaded successfully.
```

### 5. Full Path Support (Backward Compatible)
The original `set-model` command still works for custom paths:

```bash
> set-model /path/to/custom/model.gguf
```

## Setup

1. **Place your models** in the `llama.cpp/models/` directory:
   ```
   llama.cpp/
   └── models/
       ├── phi-2.Q4_K_M.gguf
       ├── tinyllama-1.1b-chat-v1.0.Q8_0.gguf
       └── llama-2-7b-chat.Q4_K_M.gguf
   ```

2. **Start RaOS**:
   ```bash
   cd RaCore
   dotnet run
   ```

3. **Check initialization logs**:
   ```
   [Module:AILanguage] INFO: Scanned llama.cpp/models and found 3 .gguf model(s).
   [Module:AILanguage] INFO: Auto-detected model: phi-2.Q4_K_M.gguf
   [Module:AILanguage] INFO: AILanguage module initialized successfully and ready.
   ```

## Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| `help` | Show all available commands | `help` |
| `status` | Show current module status | `status` |
| `list-models` | List all available .gguf models | `list-models` |
| `select-model <n>` | Select model by number | `select-model 2` |
| `select-model <name>` | Select model by name (partial match) | `select-model phi` |
| `set-model <path>` | Set model to specific file path | `set-model /custom/path/model.gguf` |
| `reload` | Reload the current model | `reload` |
| `set-context <n>` | Set context size (1-8192) | `set-context 4096` |
| `set-tokens <n>` | Set max tokens (1-2048) | `set-tokens 256` |
| `set-exe <path>` | Set llama.cpp executable path | `set-exe /path/to/llama-cli` |

## Error Handling

### No Models Found
If no models are detected, you'll see:
```
[Module:AILanguage] WARN: Model file not found at: /path/to/default/model.gguf
[Module:AILanguage] WARN: AILanguage module will not be functional until a model is configured.
[Module:AILanguage] INFO: Use 'set-model <path>' to configure model path.
```

**Solution:** Place a .gguf model file in `llama.cpp/models/` or use `set-model` to specify a custom path.

### Model Not Found by Name
If you try to select a non-existent model:
```
> select-model nonexistent

Model not found: nonexistent

Available models (3 found in llama.cpp/models):
  1. llama-2-7b-chat.Q4_K_M.gguf (current)
  2. phi-2.Q4_K_M.gguf
  3. tinyllama-1.1b-chat-v1.0.Q8_0.gguf

Use 'select-model <name-or-number>' to switch to a different model.
```

## Technical Details

### Model Scanning
- Directory: `llama.cpp/models/`
- Pattern: `*.gguf`
- Scope: Top-level only (no subdirectories)
- Timing: On module initialization and when checking for models

### Selection Logic
1. **By Number**: Exact match (1-based index)
2. **By Name**: Case-insensitive partial match on filename
3. **First Match**: If multiple models match the name, selects the first one

### Initialization Sequence
1. Check if `_modelPath` is already set
2. If not set, scan for available models
3. If models found, auto-select the first one
4. If no models found, fall back to default path
5. Log results and display helpful messages

## Examples

### Example 1: Fresh Installation
```bash
# Start with no configured model
cd RaCore
dotnet run

# Check what models are available
> list-models
Available models (2 found in llama.cpp/models):
  1. phi-2.Q4_K_M.gguf (current)
  2. tinyllama-1.1b-chat-v1.0.Q8_0.gguf

# Model phi-2 is auto-selected - ready to use!
> status
AILanguageModule: ready
Model path: /full/path/to/llama.cpp/models/phi-2.Q4_K_M.gguf
```

### Example 2: Switch Between Models
```bash
# Currently using phi-2
> status
Model path: /path/to/phi-2.Q4_K_M.gguf

# Switch to tinyllama
> select-model tinyllama
Model switched to tinyllama-1.1b-chat-v1.0.Q8_0.gguf and loaded successfully.

# Verify the change
> status
Model path: /path/to/tinyllama-1.1b-chat-v1.0.Q8_0.gguf
```

### Example 3: Adding New Models
```bash
# Copy a new model to the directory
cp ~/Downloads/mistral-7b.Q5_K_M.gguf llama.cpp/models/

# Reload to detect the new model
> reload

# List models - now shows 3
> list-models
Available models (3 found in llama.cpp/models):
  1. phi-2.Q4_K_M.gguf (current)
  2. tinyllama-1.1b-chat-v1.0.Q8_0.gguf
  3. mistral-7b.Q5_K_M.gguf

# Select the new model
> select-model mistral
Model switched to mistral-7b.Q5_K_M.gguf and loaded successfully.
```

## Migration from Old System

### Before (Static Filename)
```bash
# Had to rename files to match expected name
mv phi-2.Q4_K_M.gguf llama-2-7b-chat.Q4_K_M.gguf
```

### After (Dynamic Detection)
```bash
# Just place the file - no renaming needed
cp phi-2.Q4_K_M.gguf llama.cpp/models/

# Select by name
> select-model phi-2
```

## Troubleshooting

### Models Not Detected
1. Verify directory exists: `ls llama.cpp/models/`
2. Check file extension: Must be `.gguf` (lowercase)
3. Ensure files are in the top level (not in subdirectories)
4. Restart RaOS to re-scan: `reload` command

### Permission Issues
If you see "Access Denied" errors:
```bash
# Linux/Mac: Fix permissions
chmod 755 llama.cpp/models/
chmod 644 llama.cpp/models/*.gguf

# Windows: Run as Administrator or check folder permissions
```

## Best Practices

1. **Organize by Quantization**: Use descriptive names that include quantization type
   - Good: `phi-2.Q4_K_M.gguf`, `llama-2-7b.Q8_0.gguf`
   - Avoid: `model.gguf`, `test.gguf`

2. **Keep Original Names**: No need to rename - the system handles any valid .gguf filename

3. **Use list-models**: Always check what's available before selecting

4. **Version Models**: Include version info in filenames
   - Example: `tinyllama-1.1b-chat-v1.0.Q8_0.gguf`

5. **Test After Selection**: Use `status` to verify the model loaded successfully

## Related Documentation
- [LLAMA_CPP_AUTO_DETECTION.md](LLAMA_CPP_AUTO_DETECTION.md) - Automatic llama.cpp executable detection
- Original Issue: Bug: RaOS Does Not Detect *.gguf Files and Expects a Static Filename

---

**Version:** 1.0.0  
**Last Updated:** 2025-01-05  
**Status:** Production Ready
