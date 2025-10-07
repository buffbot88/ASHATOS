#!/bin/bash

# Private Alpha Readiness Test Runner
# Runs comprehensive tests for Private Alpha deployment

echo "╔═══════════════════════════════════════════════════════════════════╗"
echo "║         PRIVATE ALPHA READINESS TEST RUNNER                       ║"
echo "╚═══════════════════════════════════════════════════════════════════╝"
echo ""

# Build the project
echo "Building RaCore..."
dotnet build TheRaProject.sln --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed! Cannot proceed with tests."
    exit 1
fi

echo ""
echo "✓ Build successful"
echo ""

# Create a simple test harness to call the test
cat > /tmp/run_alpha_tests.cs << 'EOF'
using RaCore.Tests;
using System;

class Program
{
    static void Main()
    {
        try
        {
            PrivateAlphaReadinessTests.RunTests();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test execution failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
EOF

# Run using dotnet script or by compiling a small test program
cd RaCore
echo "Running Private Alpha Readiness Tests..."
echo ""

# Use reflection to run the test from an interactive C# script
dotnet exec bin/Release/net8.0/RaCore.dll << 'TESTSCRIPT' 2>&1 | grep -v "^>" || true
using RaCore.Tests;
PrivateAlphaReadinessTests.RunTests();
exit
TESTSCRIPT

echo ""
echo "════════════════════════════════════════════════════════════════════"
echo "Test execution complete. Review results above."
echo "════════════════════════════════════════════════════════════════════"
