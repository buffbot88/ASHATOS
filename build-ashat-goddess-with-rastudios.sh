#!/bin/bash
# Build script for ASHAT Goddess Client with RaStudios Integration
# Builds ASHAT with RaStudios bundled for a complete desktop experience

set -e

echo "========================================="
echo "ASHAT Goddess + RaStudios Build Script"
echo "Building the complete desktop suite..."
echo "========================================="
echo ""

GODDESS_DIR="ASHATGoddessClient"
RASTUDIOS_DIR="RaStudios/RaStudios.WinForms"
OUTPUT_DIR="wwwroot/downloads/goddess-with-rastudios"
VERSION="1.0.0"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "🏗️ Building ASHAT Goddess + RaStudios Suite v$VERSION"
echo ""

# ============================================
# Windows x64 Build
# ============================================
echo "👑 Building for Windows x64..."
echo "   Step 1: Building ASHAT Goddess..."

dotnet publish "$GODDESS_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR/win-x64/ASHAT"

if [ -f "$OUTPUT_DIR/win-x64/ASHAT/ASHAT.exe" ]; then
    echo "   ✓ ASHAT Goddess built successfully"
    ASHAT_SIZE=$(du -h "$OUTPUT_DIR/win-x64/ASHAT/ASHAT.exe" | cut -f1)
    echo "     Size: $ASHAT_SIZE"
else
    echo "   ❌ ASHAT Goddess build failed"
    exit 1
fi

echo "   Step 2: Building RaStudios..."

dotnet publish "$RASTUDIOS_DIR/RaStudios.WinForms.csproj" \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/win-x64/RaStudios"

if [ -f "$OUTPUT_DIR/win-x64/RaStudios/RaStudios.exe" ]; then
    echo "   ✓ RaStudios built successfully"
    RASTUDIOS_SIZE=$(du -h "$OUTPUT_DIR/win-x64/RaStudios/RaStudios.exe" | cut -f1)
    echo "     Size: $RASTUDIOS_SIZE"
else
    echo "   ❌ RaStudios build failed"
    exit 1
fi

echo "   Step 3: Creating package..."
cd "$OUTPUT_DIR/win-x64"
mkdir -p "ASHAT-RaStudios-Suite"
cp -r ASHAT/* "ASHAT-RaStudios-Suite/"
cp -r RaStudios "ASHAT-RaStudios-Suite/"

# Create README
cat > "ASHAT-RaStudios-Suite/README.txt" << 'EOF'
ASHAT Goddess + RaStudios Desktop Suite
========================================

Welcome to the complete ASHAT desktop experience!

This package includes:
- ASHAT Goddess Desktop Assistant (ASHAT.exe)
- RaStudios IDE and Game Development Platform (RaStudios/RaStudios.exe)

Quick Start:
1. Run ASHAT.exe to launch your AI Goddess companion
2. Tell ASHAT "open RaStudios" to launch the development studio
3. Or run RaStudios/RaStudios.exe directly

ASHAT Goddess can help you with:
- Coding and debugging assistance
- RaStudios control and automation
- General AI assistance

RaStudios provides:
- Game development IDE
- Asset management
- Server connection tools
- AI-powered code generation

System Requirements:
- Windows 10 or later
- .NET 9.0 runtime (included in this package)
- 4 GB RAM minimum
- 500 MB disk space

For more information, visit: https://github.com/buffbot88/ASHATOS

Copyright © 2025 AGP Studios, INC. All rights reserved.
EOF

# Create launcher script
cat > "ASHAT-RaStudios-Suite/Launch-ASHAT.bat" << 'EOF'
@echo off
echo Starting ASHAT Goddess...
start "" "%~dp0ASHAT.exe"
EOF

cat > "ASHAT-RaStudios-Suite/Launch-RaStudios.bat" << 'EOF'
@echo off
echo Starting RaStudios...
start "" "%~dp0RaStudios\RaStudios.exe"
EOF

zip -r "../ASHAT-RaStudios-Suite-Windows-x64.zip" "ASHAT-RaStudios-Suite"
cd ../../..

echo "   ✓ Windows package created"
PACKAGE_SIZE=$(du -h "$OUTPUT_DIR/win-x64/ASHAT-RaStudios-Suite-Windows-x64.zip" | cut -f1)
echo "     Package size: $PACKAGE_SIZE"
echo ""

# ============================================
# Linux x64 Build
# ============================================
echo "👑 Building for Linux x64..."
echo "   Step 1: Building ASHAT Goddess..."

dotnet publish "$GODDESS_DIR/ASHATGoddessClient.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_DIR/linux-x64/ASHAT"

if [ -f "$OUTPUT_DIR/linux-x64/ASHAT/ASHAT" ]; then
    echo "   ✓ ASHAT Goddess built successfully"
    chmod +x "$OUTPUT_DIR/linux-x64/ASHAT/ASHAT"
    ASHAT_SIZE=$(du -h "$OUTPUT_DIR/linux-x64/ASHAT/ASHAT" | cut -f1)
    echo "     Size: $ASHAT_SIZE"
else
    echo "   ❌ ASHAT Goddess build failed"
    exit 1
fi

echo "   Step 2: Building RaStudios (Python version for Linux)..."
# For Linux, we'll include the Python version of RaStudios
mkdir -p "$OUTPUT_DIR/linux-x64/RaStudios"
cp -r RaStudios/*.py "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true
cp -r RaStudios/ui "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true
cp -r RaStudios/services "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true
cp -r RaStudios/panels "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true
cp -r RaStudios/plugins "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true
cp RaStudios/requirements.txt "$OUTPUT_DIR/linux-x64/RaStudios/" 2>/dev/null || true

echo "   ✓ RaStudios (Python) copied"

echo "   Step 3: Creating package..."
cd "$OUTPUT_DIR/linux-x64"
mkdir -p "ASHAT-RaStudios-Suite"
cp -r ASHAT/* "ASHAT-RaStudios-Suite/"
cp -r RaStudios "ASHAT-RaStudios-Suite/"

# Create README
cat > "ASHAT-RaStudios-Suite/README.txt" << 'EOF'
ASHAT Goddess + RaStudios Desktop Suite (Linux)
================================================

Welcome to the complete ASHAT desktop experience for Linux!

This package includes:
- ASHAT Goddess Desktop Assistant (ASHAT)
- RaStudios IDE (Python version)

Quick Start:
1. Run ./ASHAT to launch your AI Goddess companion
2. Tell ASHAT "open RaStudios" to launch the development studio
3. Or install RaStudios dependencies and run it:
   cd RaStudios
   pip install -r requirements.txt
   python main.py

System Requirements:
- Linux (Ubuntu 20.04+ recommended)
- .NET 9.0 runtime (included)
- Python 3.10+ (for RaStudios)
- 4 GB RAM minimum
- 500 MB disk space

For more information, visit: https://github.com/buffbot88/ASHATOS

Copyright © 2025 AGP Studios, INC. All rights reserved.
EOF

# Create launcher scripts
cat > "ASHAT-RaStudios-Suite/launch-ashat.sh" << 'EOF'
#!/bin/bash
echo "Starting ASHAT Goddess..."
cd "$(dirname "$0")"
./ASHAT
EOF
chmod +x "ASHAT-RaStudios-Suite/launch-ashat.sh"

cat > "ASHAT-RaStudios-Suite/launch-rastudios.sh" << 'EOF'
#!/bin/bash
echo "Starting RaStudios..."
cd "$(dirname "$0")/RaStudios"
python3 main.py
EOF
chmod +x "ASHAT-RaStudios-Suite/launch-rastudios.sh"

tar -czf "../ASHAT-RaStudios-Suite-Linux-x64.tar.gz" "ASHAT-RaStudios-Suite"
cd ../../..

echo "   ✓ Linux package created"
PACKAGE_SIZE=$(du -h "$OUTPUT_DIR/linux-x64/ASHAT-RaStudios-Suite-Linux-x64.tar.gz" | cut -f1)
echo "     Package size: $PACKAGE_SIZE"
echo ""

echo "========================================="
echo "Build Summary"
echo "========================================="
echo "Output directory: $OUTPUT_DIR"
echo ""
echo "The ASHAT + RaStudios Suite has been built for:"
echo "  📦 Windows x64: $OUTPUT_DIR/win-x64/ASHAT-RaStudios-Suite-Windows-x64.zip"
echo "  📦 Linux x64: $OUTPUT_DIR/linux-x64/ASHAT-RaStudios-Suite-Linux-x64.tar.gz"
echo ""
echo "✨ ASHAT Goddess + RaStudios Suite build complete!"
echo ""
echo "The complete desktop experience is ready:"
echo "  👑 ASHAT - Your AI Goddess companion"
echo "  🎮 RaStudios - Powerful development platform"
echo ""
echo "Users can now download a single package with both applications!"
