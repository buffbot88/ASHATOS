#!/bin/bash
# Startup script for ASHAT GoddessQ CLI

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Change to the AshatGoddessQ directory
cd "$SCRIPT_DIR"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET runtime not found. Please install .NET 9.0 or later."
    exit 1
fi

# Run the CLI
dotnet run -- "$@"
