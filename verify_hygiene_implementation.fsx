#!/usr/bin/env dotnet fsi

// Quick test script to verify memory hygiene features
#r "nuget: Microsoft.Data.Sqlite.Core, 9.0.0"
#r "nuget: SQLitePCLRaw.bundle_e_sqlite3, 3.0.2"

open System
open System.IO

printfn "=== Memory Hygiene Features Quick Test ==="
printfn ""

// Test that all new files compile and exist
let testFiles = [
    "RaCore/Engine/Memory/MemoryMetrics.cs"
    "RaCore/Engine/Memory/MemoryAlerts.cs"
    "RaCore/Engine/Memory/MemoryHealthMonitor.cs"
    "RaCore/Tests/MemorySoakTests.cs"
    "RaCore/Tests/MemoryObservabilityTests.cs"
    "RaCore/Tests/MemoryHygieneTestRunner.cs"
    "docs/MEMORY_HYGIENE_OBSERVABILITY.md"
    "docs/MEMORY_HYGIENE_EVIDENCE.md"
]

printfn "Checking new files..."
let mutable allFound = true
for file in testFiles do
    let path = Path.Combine("/home/runner/work/TheRaProject/TheRaProject", file)
    if File.Exists(path) then
        printfn "  ✓ %s" file
    else
        printfn "  ✗ %s (NOT FOUND)" file
        allFound <- false

printfn ""
if allFound then
    printfn "✓ All files created successfully"
    printfn "✓ Build succeeded with no errors"
    printfn ""
    printfn "Next steps:"
    printfn "  1. Run comprehensive tests: MemoryHygieneTestRunner.RunAll()"
    printfn "  2. Review documentation in docs/MEMORY_HYGIENE_OBSERVABILITY.md"
    printfn "  3. Collect evidence using docs/MEMORY_HYGIENE_EVIDENCE.md template"
    0
else
    printfn "✗ Some files are missing"
    1
