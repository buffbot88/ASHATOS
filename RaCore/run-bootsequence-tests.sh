#!/bin/bash
# Run Boot Sequence Fix Tests

echo "Running Boot Sequence Fix Tests..."
echo ""

dotnet run --project . --no-build bootsequence

echo ""
echo "Tests complete!"
