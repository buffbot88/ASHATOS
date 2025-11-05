#!/bin/bash
# Build script for ASHAT Desktop Assistant
# Builds standalone executables for Windows, Linux, and macOS

set -e

echo "========================================="
echo "ASHAT Desktop Assistant Build Script"
echo "========================================="
echo ""

PROJECT_DIR="ASHATDesktopClient"
OUTPUT_DIR="wwwroot/downloads"
VERSION="1.0.0"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ASHAT Desktop Assistant v$VERSION"
echo ""

# Windows x64
echo "📦 Building for Windows x64..."
dotnet publish "$PROJECT_DIR/ASHATDesktopClient.csproj" \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR/win-x64"

if [ -f "$OUTPUT_DIR/win-x64/ASHATDesktopAssistant.exe" ]; then
    echo "✓ Windows build successful"
    SIZE=$(du -h "$OUTPUT_DIR/win-x64/ASHATDesktopAssistant.exe" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ Windows build failed"
fi
echo ""

# Linux x64
echo "📦 Building for Linux x64..."
dotnet publish "$PROJECT_DIR/ASHATDesktopClient.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/linux-x64"

if [ -f "$OUTPUT_DIR/linux-x64/ASHATDesktopAssistant" ]; then
    echo "✓ Linux build successful"
    chmod +x "$OUTPUT_DIR/linux-x64/ASHATDesktopAssistant"
    SIZE=$(du -h "$OUTPUT_DIR/linux-x64/ASHATDesktopAssistant" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ Linux build failed"
fi
echo ""

# macOS x64 (Intel)
echo "📦 Building for macOS x64..."
dotnet publish "$PROJECT_DIR/ASHATDesktopClient.csproj" \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/osx-x64"

if [ -f "$OUTPUT_DIR/osx-x64/ASHATDesktopAssistant" ]; then
    echo "✓ macOS x64 build successful"
    chmod +x "$OUTPUT_DIR/osx-x64/ASHATDesktopAssistant"
    SIZE=$(du -h "$OUTPUT_DIR/osx-x64/ASHATDesktopAssistant" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ macOS x64 build failed"
fi
echo ""

# macOS ARM64 (Apple Silicon)
echo "📦 Building for macOS ARM64..."
dotnet publish "$PROJECT_DIR/ASHATDesktopClient.csproj" \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/osx-arm64"

if [ -f "$OUTPUT_DIR/osx-arm64/ASHATDesktopAssistant" ]; then
    echo "✓ macOS ARM64 build successful"
    chmod +x "$OUTPUT_DIR/osx-arm64/ASHATDesktopAssistant"
    SIZE=$(du -h "$OUTPUT_DIR/osx-arm64/ASHATDesktopAssistant" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ macOS ARM64 build failed"
fi
echo ""

echo "========================================="
echo "Build Summary"
echo "========================================="
echo "Output directory: $OUTPUT_DIR"
echo ""
echo "Built packages:"
echo "  • Windows x64: $OUTPUT_DIR/win-x64/ASHATDesktopAssistant.exe"
echo "  • Linux x64: $OUTPUT_DIR/linux-x64/ASHATDesktopAssistant"
echo "  • macOS x64: $OUTPUT_DIR/osx-x64/ASHATDesktopAssistant"
echo "  • macOS ARM64: $OUTPUT_DIR/osx-arm64/ASHATDesktopAssistant"
echo ""
echo "✨ ASHAT Desktop Assistant build complete!"
echo ""
echo "Next steps:"
echo "  1. Copy executables to ASHATCore/wwwroot/downloads/"
echo "  2. Users can download from /api/download/ashat-desktop-[platform]"
echo "  3. Or visit /downloads for the download page"
