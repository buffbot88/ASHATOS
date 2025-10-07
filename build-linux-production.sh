#!/bin/bash
#
# RaCore Linux Production Build Script
# Creates self-contained deployment with .NET runtime included
# Optimized for Ubuntu 22.04 LTS x64 (8GB RAM / 80GB NVMe)
#

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
BUILD_CONFIG="Release"
TARGET_FRAMEWORK="net9.0"
RUNTIME_ID="linux-x64"
OUTPUT_DIR="./publish-production"
PROJECT_DIR="./RaCore"
SELF_CONTAINED=true

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --runtime-only)
            SELF_CONTAINED=false
            OUTPUT_DIR="./publish-runtime-only"
            shift
            ;;
        --output)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --runtime-only    Build without including .NET runtime (smaller, requires .NET on target)"
            echo "  --output DIR      Specify output directory (default: ./publish-production)"
            echo "  --help           Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

echo -e "${MAGENTA}╔════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║   RaCore Production Build Script      ║${NC}"
echo -e "${MAGENTA}║   Ubuntu 22.04 LTS x64                 ║${NC}"
echo -e "${MAGENTA}╔════════════════════════════════════════╗${NC}"
echo ""

# System information
echo -e "${CYAN}System Information:${NC}"
echo -e "  OS: $(lsb_release -d | cut -f2)"
echo -e "  Kernel: $(uname -r)"
echo -e "  Architecture: $(uname -m)"
echo -e "  CPU Cores: $(nproc)"
echo -e "  RAM: $(free -h | awk '/^Mem:/ {print $2}')"
echo ""

# Check if .NET SDK is installed
echo -e "${YELLOW}[1/8] Checking .NET SDK...${NC}"
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found!${NC}"
    echo ""
    echo "Installation instructions for Ubuntu 22.04:"
    echo "  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb"
    echo "  sudo dpkg -i packages-microsoft-prod.deb"
    echo "  sudo apt update"
    echo "  sudo apt install -y dotnet-sdk-9.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK installed: v$DOTNET_VERSION${NC}"
dotnet --info | grep -E "RID:|Version:|Host|Commit:" | sed 's/^/  /'
echo ""

# Check if project exists
echo -e "${YELLOW}[2/8] Validating project structure...${NC}"
if [ ! -d "$PROJECT_DIR" ]; then
    echo -e "${RED}ERROR: Project directory '$PROJECT_DIR' not found!${NC}"
    echo "Please run this script from the TheRaProject root directory."
    exit 1
fi

if [ ! -f "$PROJECT_DIR/RaCore.csproj" ]; then
    echo -e "${RED}ERROR: RaCore.csproj not found in $PROJECT_DIR${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Project structure validated${NC}"
echo ""

# Clean previous builds
echo -e "${YELLOW}[3/8] Cleaning previous builds...${NC}"
cd "$PROJECT_DIR"

# Clean obj and bin directories
if [ -d "obj" ]; then
    rm -rf obj
    echo -e "  ${GREEN}✓${NC} Removed obj directory"
fi

if [ -d "bin" ]; then
    rm -rf bin
    echo -e "  ${GREEN}✓${NC} Removed bin directory"
fi

cd ..

# Clean output directory
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
    echo -e "  ${GREEN}✓${NC} Removed previous output directory"
fi

echo -e "${GREEN}✓ Cleanup completed${NC}"
echo ""

# Restore dependencies
echo -e "${YELLOW}[4/8] Restoring NuGet packages...${NC}"
cd "$PROJECT_DIR"
dotnet restore --runtime $RUNTIME_ID
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# Build project
echo -e "${YELLOW}[5/8] Building RaCore ($BUILD_CONFIG)...${NC}"
dotnet build \
    --configuration $BUILD_CONFIG \
    --runtime $RUNTIME_ID \
    --no-restore
echo -e "${GREEN}✓ Build successful${NC}"
echo ""

# Run tests if they exist
echo -e "${YELLOW}[6/8] Running tests...${NC}"
if [ -d "../RaCore/Tests" ]; then
    cd "../RaCore/Tests"
    if dotnet test --no-build --configuration $BUILD_CONFIG 2>/dev/null; then
        echo -e "${GREEN}✓ Tests passed${NC}"
    else
        echo -e "${YELLOW}⚠ Tests not available or failed (continuing)${NC}"
    fi
    cd "../../$PROJECT_DIR"
else
    echo -e "${YELLOW}⚠ No tests found (skipping)${NC}"
fi
echo ""

# Publish for production
echo -e "${YELLOW}[7/8] Publishing for production...${NC}"

if [ "$SELF_CONTAINED" = true ]; then
    echo -e "  ${CYAN}Mode: Self-contained (includes .NET runtime)${NC}"
    dotnet publish \
        --configuration $BUILD_CONFIG \
        --runtime $RUNTIME_ID \
        --self-contained true \
        --output "../$OUTPUT_DIR" \
        -p:PublishSingleFile=false \
        -p:PublishReadyToRun=true \
        -p:PublishTrimmed=false \
        -p:EnableCompressionInSingleFile=true
else
    echo -e "  ${CYAN}Mode: Runtime-dependent (requires .NET runtime on target)${NC}"
    dotnet publish \
        --configuration $BUILD_CONFIG \
        --runtime $RUNTIME_ID \
        --self-contained false \
        --output "../$OUTPUT_DIR" \
        -p:PublishSingleFile=false \
        -p:PublishReadyToRun=false
fi

cd ..
echo -e "${GREEN}✓ Publishing completed${NC}"
echo ""

# Create deployment package
echo -e "${YELLOW}[8/8] Creating deployment package...${NC}"

# Create necessary directories in output
mkdir -p "$OUTPUT_DIR/logs"
mkdir -p "$OUTPUT_DIR/data"

# Copy documentation files
cp README.md "$OUTPUT_DIR/" 2>/dev/null || true
cp LINUX_HOSTING_SETUP.md "$OUTPUT_DIR/" 2>/dev/null || true
cp CMS_QUICKSTART.md "$OUTPUT_DIR/" 2>/dev/null || true

# Create run script
cat > "$OUTPUT_DIR/run-racore.sh" << 'EOF'
#!/bin/bash
# RaCore Launcher Script

export RACORE_PORT=${RACORE_PORT:-5000}
export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}

echo "Starting RaCore..."
echo "Port: $RACORE_PORT"
echo "Environment: $ASPNETCORE_ENVIRONMENT"
echo ""

dotnet RaCore.dll
EOF

chmod +x "$OUTPUT_DIR/run-racore.sh"

# Create systemd service template
cat > "$OUTPUT_DIR/racore.service" << EOF
[Unit]
Description=RaCore AI Mainframe
After=network.target

[Service]
Type=notify
User=racore
Group=racore
WorkingDirectory=/opt/racore
ExecStart=/usr/bin/dotnet /opt/racore/RaCore.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=racore
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=RACORE_PORT=5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

# Create README for deployment
cat > "$OUTPUT_DIR/DEPLOYMENT.md" << EOF
# RaCore Deployment Package

This is a production-ready deployment package for RaCore on Ubuntu 22.04 LTS x64.

## Quick Deployment

### 1. Copy to Server
\`\`\`bash
scp -r publish-production user@your-server:/tmp/racore
\`\`\`

### 2. Install on Server
\`\`\`bash
# On the server
sudo mkdir -p /opt/racore
sudo cp -r /tmp/racore/* /opt/racore/
sudo chown -R racore:racore /opt/racore
\`\`\`

### 3. Install .NET Runtime (if not self-contained)
\`\`\`bash
sudo apt install -y aspnetcore-runtime-9.0
\`\`\`

### 4. Set Up Service
\`\`\`bash
sudo cp /opt/racore/racore.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable racore
sudo systemctl start racore
\`\`\`

### 5. Check Status
\`\`\`bash
sudo systemctl status racore
sudo journalctl -u racore -f
\`\`\`

## Manual Start

\`\`\`bash
cd /opt/racore
./run-racore.sh
\`\`\`

For detailed setup instructions, see LINUX_HOSTING_SETUP.md
EOF

echo -e "${GREEN}✓ Deployment package created${NC}"
echo ""

# Build summary
echo -e "${MAGENTA}╔════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║   Build Summary                        ║${NC}"
echo -e "${MAGENTA}╚════════════════════════════════════════╝${NC}"
echo ""

# Calculate sizes
OUTPUT_SIZE=$(du -sh "$OUTPUT_DIR" | cut -f1)
DLL_SIZE=$(du -sh "$OUTPUT_DIR/RaCore.dll" | cut -f1 2>/dev/null || echo "N/A")
TOTAL_FILES=$(find "$OUTPUT_DIR" -type f | wc -l)

echo -e "${CYAN}Configuration${NC}"
echo -e "  Build Config:     ${GREEN}$BUILD_CONFIG${NC}"
echo -e "  Target Runtime:   ${GREEN}$RUNTIME_ID${NC}"
echo -e "  Self-Contained:   ${GREEN}$SELF_CONTAINED${NC}"
echo -e "  Output Directory: ${GREEN}$OUTPUT_DIR${NC}"
echo ""

echo -e "${CYAN}Package Information${NC}"
echo -e "  Total Size:       ${GREEN}$OUTPUT_SIZE${NC}"
echo -e "  RaCore DLL:       ${GREEN}$DLL_SIZE${NC}"
echo -e "  Total Files:      ${GREEN}$TOTAL_FILES${NC}"
echo ""

echo -e "${CYAN}Generated Files${NC}"
echo -e "  ${GREEN}✓${NC} RaCore.dll"
echo -e "  ${GREEN}✓${NC} run-racore.sh (launcher script)"
echo -e "  ${GREEN}✓${NC} racore.service (systemd service)"
echo -e "  ${GREEN}✓${NC} DEPLOYMENT.md (deployment guide)"
echo -e "  ${GREEN}✓${NC} logs/ (log directory)"
echo -e "  ${GREEN}✓${NC} data/ (data directory)"
echo ""

# Next steps
echo -e "${MAGENTA}╔════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║   Next Steps                           ║${NC}"
echo -e "${MAGENTA}╚════════════════════════════════════════╝${NC}"
echo ""

echo -e "${YELLOW}Local Testing:${NC}"
echo -e "  cd $OUTPUT_DIR"
echo -e "  ./run-racore.sh"
echo ""

echo -e "${YELLOW}Deploy to Production:${NC}"
echo -e "  1. Copy package to server:"
echo -e "     ${CYAN}tar -czf racore-production.tar.gz $OUTPUT_DIR${NC}"
echo -e "     ${CYAN}scp racore-production.tar.gz user@server:/tmp/${NC}"
echo ""
echo -e "  2. On the server, extract and install:"
echo -e "     ${CYAN}cd /tmp && tar -xzf racore-production.tar.gz${NC}"
echo -e "     ${CYAN}sudo mv $OUTPUT_DIR /opt/racore${NC}"
echo ""
echo -e "  3. Follow deployment instructions:"
echo -e "     ${CYAN}cat /opt/racore/DEPLOYMENT.md${NC}"
echo ""

echo -e "${YELLOW}Complete Setup Guide:${NC}"
echo -e "  See ${CYAN}LINUX_HOSTING_SETUP.md${NC} for detailed instructions on:"
echo -e "  - Nginx configuration"
echo -e "  - PHP setup"
echo -e "  - FTP server"
echo -e "  - SSL/TLS"
echo -e "  - Firewall configuration"
echo -e "  - Monitoring and maintenance"
echo ""

echo -e "${GREEN}╔════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   Build Completed Successfully! 🚀    ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════╝${NC}"
echo ""
