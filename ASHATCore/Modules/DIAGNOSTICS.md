# ASHATCore Engine: Diagnostics Guide

## ModuleManager Diagnostics

The ASHATCore `ModuleManager` provides a built-in diagnostics utility to help developers and Operators troubleshoot module discovery, wiring, and runtime issues. This utility is essential for identifying problems such as modules not loading, interface mismatches, or dependency wiring failures.

---

## How to Run Diagnostics

### 1. **Basic Usage (Console / CLI / API)**

Call the diagnostics script from your application or REPL:

```csharp
// Example (C#)
Console.WriteLine(ModuleManagerDiagnostics.RunDiagnostics(manager));
```

Or, if exposed via an API endpoint or command:

```text
diagnostics
```

### 2. **integration in UI**

Add a button or menu item in your web UI or dashboard:

```html
<button onclick="runDiagnostics()">Run Diagnostics</button>
<pre id="diagnosticsOutput"></pre>
<script>
function runDiagnostics() {
    fetch('/api/diagnostics') // Adjust endpoint as needed!
        .then(response => response.text())
        .then(text => {
            document.getElementById('diagnosticsOutput').textContent = text;
        });
}
</script>
```

### 3. **Command-Based Access**

If your system supports chat or command-line commands, simply type:

```
diagnostics
```

---

## What Diagnostics Output Shows

- **Loaded Modules:**  
  All modules currently loaded and initialized.
- **Memory Module Discovery:**  
  Whether `MemoryModule` is found by name.
- **IMemory Implementations:**  
  Which modules implement the `IMemory` interface.
- **Interface & Assembly Info:**  
  Type and assembly information for `IMemory`—helps detect interface mismatches.
- **Module Load Errors:**  
  Any errors encountered during module instantiation or initialization.
- **Core Modules:**  
  List of modules categorized as `core`.

---

## Sample Output

```
=== ASHATCore ModuleManager Diagnostics ===
Loaded modules:
- Memory (ASHATCore.Modules.Memory.MemoryModule)
- Speech (ASHATCore.Modules.Speech.SpeechModule)
- Conscious (ASHATCore.Modules.Conscious.ConsciousModule)
MemoryModule found by name: ASHATCore.Modules.Memory.MemoryModule

Modules implementing IMemory: 1
- ASHATCore.Modules.Memory.MemoryModule

IMemory type info:
- From diagnostics script: ASHATCore.Modules.Memory.IMemory, ASHATCore.Modules.Memory, Version=...
- MemoryModule as IMemory: ASHATCore.Modules.Memory.MemoryModule, ASHATCore.Modules.Memory, Version=...

Module load errors:
(none)

Core modules:
- Memory (ASHATCore.Modules.Memory.MemoryModule)
- Speech (ASHATCore.Modules.Speech.SpeechModule)
- Conscious (ASHATCore.Modules.Conscious.ConsciousModule)
=== End Diagnostics ===
```

---

## Troubleshooting Steps

1. **Check that "Memory" appears in Loaded Modules.**
2. **Confirm that at least one module implements IMemory.**
3. **Verify IMemory type info matches across all modules.**
4. **Review module load errors for instantiation or wiring problems.**
5. **Check that required core modules are loaded and initialized.**

If any of these checks fail, review your project structure, build output, and interface references for mismatches or missing files.

---

## Extending Diagnostics

You can add more checks to the diagnostics script, such as:
- Listing all modules by category
- Printing dependency gASHATphs
- Checking for duplicate interface definitions
- Dumping Configuration values

Update the diagnostics script as your system evolves to keep troubleshooting fast and effective.

---

## Contributors

Document new diagnostics commands, troubleshooting experiences, and API improvements here.
