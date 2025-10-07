#!/bin/bash
#
# RaCore Linux Build Script
# Builds RaCore for Ubuntu 22.04 LTS x64 with .NET 9.0
# Optimized for systems with 8GB RAM and 80GB NVMe storage
#

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BUILD_CONFIG="Release"
TARGET_FRAMEWORK="net9.0"
RUNTIME_ID="linux-x64"
OUTPUT_DIR="./publish"
PROJECT_DIR="./RaCore"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   RaCore Linux Build Script${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Check if .NET SDK is installed
echo -e "${YELLOW}[1/6] Checking .NET SDK...${NC}"
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found!${NC}"
    echo "Please install .NET 9.0 SDK:"
    echo "  sudo apt install dotnet-sdk-9.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK found: $DOTNET_VERSION${NC}"
echo ""

# Check if project exists
echo -e "${YELLOW}[2/6] Checking project directory...${NC}"
if [ ! -d "$PROJECT_DIR" ]; then
    echo -e "${RED}ERROR: Project directory '$PROJECT_DIR' not found!${NC}"
    echo "Please run this script from the TheRaProject root directory."
    exit 1
fi
echo -e "${GREEN}✓ Project directory found${NC}"
echo ""

# Clean previous build
echo -e "${YELLOW}[3/6] Cleaning previous build...${NC}"
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
    echo -e "${GREEN}✓ Previous build cleaned${NC}"
else
    echo -e "${GREEN}✓ No previous build to clean${NC}"
fi
echo ""

# Restore dependencies
echo -e "${YELLOW}[4/6] Restoring dependencies...${NC}"
cd "$PROJECT_DIR"
dotnet restore
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# Build project
echo -e "${YELLOW}[5/6] Building RaCore...${NC}"
dotnet build \
    --configuration $BUILD_CONFIG \
    --no-restore \
    --verbosity minimal
echo -e "${GREEN}✓ Build completed successfully${NC}"
echo ""

# Publish for Linux x64
echo -e "${YELLOW}[6/6] Publishing for Linux x64...${NC}"
dotnet publish \
    --configuration $BUILD_CONFIG \
    --runtime $RUNTIME_ID \
    --self-contained false \
    --output "../$OUTPUT_DIR" \
    --verbosity minimal \
    -p:PublishSingleFile=false \
    -p:PublishReadyToRun=false

cd ..
echo -e "${GREEN}✓ Published successfully${NC}"
echo ""

# Build summary
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   Build Summary${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${GREEN}Configuration:${NC} $BUILD_CONFIG"
echo -e "${GREEN}Runtime:${NC} $RUNTIME_ID"
echo -e "${GREEN}Output:${NC} $OUTPUT_DIR"
echo ""

# List output files
echo -e "${YELLOW}Generated Files:${NC}"
ls -lh "$OUTPUT_DIR"/*.dll "$OUTPUT_DIR"/RaCore 2>/dev/null || echo "  (checking...)"
echo ""

# Calculate size
OUTPUT_SIZE=$(du -sh "$OUTPUT_DIR" | cut -f1)
echo -e "${GREEN}Total Size:${NC} $OUTPUT_SIZE"
echo ""

# Usage instructions
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   Next Steps${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${YELLOW}To run RaCore:${NC}"
echo "  cd $OUTPUT_DIR"
echo "  dotnet RaCore.dll"
echo ""
echo -e "${YELLOW}Or set up as a system service:${NC}"
echo "  See LINUX_HOSTING_SETUP.md for detailed instructions"
echo ""
echo -e "${YELLOW}To deploy to production:${NC}"
echo "  1. Copy $OUTPUT_DIR to your server"
echo "  2. Install .NET 9.0 Runtime: sudo apt install aspnetcore-runtime-9.0"
echo "  3. Follow LINUX_HOSTING_SETUP.md for service setup"
echo ""
echo -e "${GREEN}✓ Build completed successfully!${NC}"
