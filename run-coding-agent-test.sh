#!/bin/bash
# Simple script to run the Coding Agent Natural Language Tests

echo "=================================="
echo "Running Coding Agent Tests"
echo "=================================="
echo ""

cd "$(dirname "$0")"

# Compile a simple test runner that calls CodingAgentTests.RunTests()
cat > /tmp/run_coding_agent_test.cs << 'EOF'
using System;
using ASHATCore.Tests;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            CodingAgentTests.RunTests();
            Console.WriteLine("\n✅ All tests completed successfully!");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
EOF

# Compile and run the test
dotnet run --project ASHATCore/ASHATCore.csproj --no-build -c Release -- /tmp/run_coding_agent_test.cs

exit_code=$?

echo ""
echo "=================================="
if [ $exit_code -eq 0 ]; then
    echo "✅ Test run completed successfully"
else
    echo "❌ Test run failed with exit code: $exit_code"
fi
echo "=================================="

exit $exit_code
