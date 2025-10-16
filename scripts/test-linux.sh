#!/bin/bash
# Cross-platform test script for Linux
# Usage: ./scripts/test-linux.sh

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}LLM Capability Checker - Linux Test${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Check prerequisites
echo -e "${YELLOW}1. Checking Prerequisites...${NC}"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found${NC}"
    echo "Please install .NET 8.0 SDK:"
    echo "  https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK found: ${DOTNET_VERSION}${NC}"

# Check required system tools
MISSING_TOOLS=()

if ! command -v lscpu &> /dev/null; then
    MISSING_TOOLS+=("lscpu (util-linux package)")
fi

if ! command -v lspci &> /dev/null; then
    MISSING_TOOLS+=("lspci (pciutils package)")
fi

if ! command -v lsblk &> /dev/null; then
    MISSING_TOOLS+=("lsblk (util-linux package)")
fi

if ! command -v df &> /dev/null; then
    MISSING_TOOLS+=("df (coreutils package)")
fi

if [ ${#MISSING_TOOLS[@]} -gt 0 ]; then
    echo -e "${YELLOW}WARNING: Missing optional tools:${NC}"
    for tool in "${MISSING_TOOLS[@]}"; do
        echo -e "  - ${tool}"
    done
    echo ""
    echo "Install with:"
    echo "  Ubuntu/Debian: sudo apt-get install pciutils util-linux coreutils"
    echo "  Arch: sudo pacman -S pciutils util-linux coreutils"
    echo "  Fedora: sudo dnf install pciutils util-linux coreutils"
    echo ""
else
    echo -e "${GREEN}✓ All system tools available${NC}"
fi

# Check optional NVIDIA tools
if command -v nvidia-smi &> /dev/null; then
    echo -e "${GREEN}✓ nvidia-smi found (CUDA detection available)${NC}"
else
    echo -e "${YELLOW}⚠ nvidia-smi not found (CUDA detection disabled)${NC}"
fi

# Check optional AMD tools
if command -v rocm-smi &> /dev/null; then
    echo -e "${GREEN}✓ rocm-smi found (ROCm detection available)${NC}"
else
    echo -e "${YELLOW}⚠ rocm-smi not found (ROCm detection disabled)${NC}"
fi

echo ""

# Display platform information
echo -e "${YELLOW}2. Platform Information:${NC}"
echo "  OS: $(uname -s)"
echo "  Kernel: $(uname -r)"
echo "  Architecture: $(uname -m)"
if [ -f /etc/os-release ]; then
    . /etc/os-release
    echo "  Distribution: $NAME $VERSION"
fi
echo ""

# Build the project
echo -e "${YELLOW}3. Building Project...${NC}"
dotnet build src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Debug

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Build successful${NC}"
else
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi
echo ""

# Run unit tests
echo -e "${YELLOW}4. Running Unit Tests...${NC}"
dotnet test tests/LLMCapabilityChecker.Tests/LLMCapabilityChecker.Tests.csproj --verbosity normal

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Unit tests passed${NC}"
else
    echo -e "${RED}✗ Unit tests failed${NC}"
    exit 1
fi
echo ""

# Run service tester
echo -e "${YELLOW}5. Running Service Tests...${NC}"
echo -e "${BLUE}--- Hardware Detection Test ---${NC}"
dotnet run --project src/LLMCapabilityChecker -- --test

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✓ Service tests passed${NC}"
else
    echo ""
    echo -e "${RED}✗ Service tests failed${NC}"
    exit 1
fi
echo ""

# Summary
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}All tests completed successfully!${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo "Next steps:"
echo "  - Run the full application: dotnet run --project src/LLMCapabilityChecker"
echo "  - Build release version: dotnet build --configuration Release"
echo "  - Review test results above for any warnings"
echo ""

# Display detected hardware summary
echo -e "${YELLOW}Quick Hardware Summary:${NC}"
if command -v lscpu &> /dev/null; then
    echo "  CPU: $(lscpu | grep 'Model name' | cut -d ':' -f 2 | xargs)"
fi
if [ -f /proc/meminfo ]; then
    TOTAL_MEM=$(grep MemTotal /proc/meminfo | awk '{print int($2/1024/1024) " GB"}')
    echo "  RAM: $TOTAL_MEM"
fi
if command -v lspci &> /dev/null; then
    GPU=$(lspci | grep -i 'vga\|3d' | head -n 1 | cut -d ':' -f 3 | xargs)
    if [ ! -z "$GPU" ]; then
        echo "  GPU: $GPU"
    fi
fi
echo ""
