#!/bin/bash

# Run Private Alpha Readiness Tests
echo "╔═══════════════════════════════════════════════════════════════════╗"
echo "║         BUILDING AND RUNNING PRIVATE ALPHA TESTS                  ║"
echo "╚═══════════════════════════════════════════════════════════════════╝"
echo ""

cd "$(dirname "$0")"

# Build the solution first
echo "Building solution..."
dotnet build TheRaProject.sln --configuration Release -v quiet

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✓ Build successful"
echo ""

# Create a temporary test runner project
TEST_DIR=$(mktemp -d)
cd "$TEST_DIR"

cat > AlphaTestRunner.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="$OLDPWD/RaCore/RaCore.csproj" />
  </ItemGroup>
</Project>
EOF

# Replace $OLDPWD with actual path
sed -i "s|\$OLDPWD|$OLDPWD|g" AlphaTestRunner.csproj

cat > Program.cs << 'EOF'
using RaCore.Tests;
using System;

Console.WriteLine("Launching Private Alpha Readiness Tests...");
Console.WriteLine();

try
{
    PrivateAlphaReadinessTests.RunTests();
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Test execution failed: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
EOF

echo "Building test runner..."
dotnet build -v quiet

if [ $? -ne 0 ]; then
    echo "❌ Test runner build failed!"
    cd "$OLDPWD"
    rm -rf "$TEST_DIR"
    exit 1
fi

echo "Running tests..."
echo ""
echo "════════════════════════════════════════════════════════════════════"
echo ""

dotnet run --no-build

TEST_RESULT=$?

# Cleanup
cd "$OLDPWD"
rm -rf "$TEST_DIR"

exit $TEST_RESULT
