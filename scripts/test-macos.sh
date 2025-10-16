#!/bin/bash
# Cross-platform test script for macOS
# Usage: ./scripts/test-macos.sh

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}LLM Capability Checker - macOS Test${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Check prerequisites
echo -e "${YELLOW}1. Checking Prerequisites...${NC}"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found${NC}"
    echo "Please install .NET 8.0 SDK:"
    echo "  brew install --cask dotnet-sdk"
    echo "  OR download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK found: ${DOTNET_VERSION}${NC}"

# Check required system tools (all built-in on macOS)
SYSTEM_TOOLS=("sysctl" "system_profiler" "diskutil" "sw_vers" "df")
MISSING_TOOLS=()

for tool in "${SYSTEM_TOOLS[@]}"; do
    if ! command -v "$tool" &> /dev/null; then
        MISSING_TOOLS+=("$tool")
    fi
done

if [ ${#MISSING_TOOLS[@]} -gt 0 ]; then
    echo -e "${RED}ERROR: Missing required system tools:${NC}"
    for tool in "${MISSING_TOOLS[@]}"; do
        echo -e "  - ${tool}"
    done
    echo ""
    echo "These tools should be built into macOS. Your system may be corrupted."
    exit 1
else
    echo -e "${GREEN}✓ All system tools available${NC}"
fi

# Check for Metal support
MACOS_VERSION=$(sw_vers -productVersion)
MAJOR_VERSION=$(echo $MACOS_VERSION | cut -d '.' -f 1)
MINOR_VERSION=$(echo $MACOS_VERSION | cut -d '.' -f 2)

if [ "$MAJOR_VERSION" -ge 11 ] || ([ "$MAJOR_VERSION" -eq 10 ] && [ "$MINOR_VERSION" -ge 13 ]); then
    echo -e "${GREEN}✓ Metal support available (macOS $MACOS_VERSION)${NC}"
else
    echo -e "${YELLOW}⚠ Metal may not be available (macOS $MACOS_VERSION < 10.13)${NC}"
fi

# Check for NVIDIA CUDA (legacy Intel Macs only)
if command -v nvidia-smi &> /dev/null; then
    echo -e "${GREEN}✓ nvidia-smi found (Legacy CUDA support)${NC}"
else
    echo -e "${YELLOW}⚠ nvidia-smi not found (Expected on Apple Silicon)${NC}"
fi

echo ""

# Display platform information
echo -e "${YELLOW}2. Platform Information:${NC}"
echo "  OS: macOS $MACOS_VERSION"
echo "  Kernel: $(uname -r)"
echo "  Architecture: $(uname -m)"

# Detect Apple Silicon vs Intel
if [ "$(uname -m)" = "arm64" ]; then
    echo "  Platform: Apple Silicon"
else
    echo "  Platform: Intel Mac"
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
echo -e "${YELLOW}Note: system_profiler may take 5-10 seconds on first run${NC}"
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
CPU_MODEL=$(sysctl -n machdep.cpu.brand_string 2>/dev/null)
if [ ! -z "$CPU_MODEL" ]; then
    echo "  CPU: $CPU_MODEL"
fi

TOTAL_MEM=$(sysctl -n hw.memsize 2>/dev/null)
if [ ! -z "$TOTAL_MEM" ]; then
    MEM_GB=$((TOTAL_MEM / 1024 / 1024 / 1024))
    echo "  RAM: ${MEM_GB} GB"
fi

# GPU detection (simplified)
GPU_INFO=$(system_profiler SPDisplaysDataType 2>/dev/null | grep "Chipset Model:" | head -n 1 | cut -d ':' -f 2 | xargs)
if [ ! -z "$GPU_INFO" ]; then
    echo "  GPU: $GPU_INFO"
fi

echo ""
