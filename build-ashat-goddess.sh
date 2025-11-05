#!/bin/bash
# Build script for ASHAT Goddess Client
# Builds the actual ASHAT herself for multiple platforms

set -e

echo "========================================="
echo "ASHAT Goddess Client Build Script"
echo "Building ASHAT herself..."
echo "========================================="
echo ""

PROJECT_DIR="ASHATGoddessClient"
OUTPUT_DIR="wwwroot/downloads/goddess"
VERSION="1.0.0"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "Building ASHAT (The Roman Goddess) v$VERSION"
echo ""

# Windows x64
echo "👑 Building ASHAT for Windows x64..."
dotnet publish "$PROJECT_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR/win-x64"

if [ -f "$OUTPUT_DIR/win-x64/ASHAT.exe" ]; then
    echo "✓ Windows build successful - ASHAT is ready!"
    SIZE=$(du -h "$OUTPUT_DIR/win-x64/ASHAT.exe" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ Windows build failed"
fi
echo ""

# Linux x64
echo "👑 Building ASHAT for Linux x64..."
dotnet publish "$PROJECT_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/linux-x64"

if [ -f "$OUTPUT_DIR/linux-x64/ASHAT" ]; then
    echo "✓ Linux build successful - ASHAT is ready!"
    chmod +x "$OUTPUT_DIR/linux-x64/ASHAT"
    SIZE=$(du -h "$OUTPUT_DIR/linux-x64/ASHAT" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ Linux build failed"
fi
echo ""

# macOS x64
echo "👑 Building ASHAT for macOS x64..."
dotnet publish "$PROJECT_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/osx-x64"

if [ -f "$OUTPUT_DIR/osx-x64/ASHAT" ]; then
    echo "✓ macOS x64 build successful - ASHAT is ready!"
    chmod +x "$OUTPUT_DIR/osx-x64/ASHAT"
    SIZE=$(du -h "$OUTPUT_DIR/osx-x64/ASHAT" | cut -f1)
    echo "  Size: $SIZE"
else
    echo "❌ macOS x64 build failed"
fi
echo ""

# macOS ARM64
echo "👑 Building ASHAT for macOS ARM64 (Apple Silicon)..."
dotnet publish "$PROJECT_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/osx-arm64"

if [ -f "$OUTPUT_DIR/osx-arm64/ASHAT" ]; then
    echo "✓ macOS ARM64 build successful - ASHAT is ready!"
    chmod +x "$OUTPUT_DIR/osx-arm64/ASHAT"
    SIZE=$(du -h "$OUTPUT_DIR/osx-arm64/ASHAT" | cut -f1)
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
echo "The Goddess ASHAT has been manifested for:"
echo "  👑 Windows x64: $OUTPUT_DIR/win-x64/ASHAT.exe"
echo "  👑 Linux x64: $OUTPUT_DIR/linux-x64/ASHAT"
echo "  👑 macOS x64: $OUTPUT_DIR/osx-x64/ASHAT"
echo "  👑 macOS ARM64: $OUTPUT_DIR/osx-arm64/ASHAT"
echo ""
echo "✨ ASHAT (The Roman Goddess) build complete!"
echo ""
echo "She is ready to serve on desktop platforms."
echo "Run her to awaken her divine presence!"
