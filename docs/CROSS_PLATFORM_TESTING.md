# Cross-Platform Testing Guide

This guide provides comprehensive instructions for testing the LLM Capability Checker on Linux, macOS, and Windows platforms.

## Table of Contents

- [Overview](#overview)
- [Platform Support Matrix](#platform-support-matrix)
- [Prerequisites](#prerequisites)
  - [Linux (Ubuntu/Debian)](#linux-ubuntudebian)
  - [Linux (Arch/Fedora)](#linux-archfedora)
  - [macOS](#macos)
  - [Windows](#windows)
- [Build Instructions](#build-instructions)
- [Running Tests](#running-tests)
- [Platform-Specific Hardware Detection](#platform-specific-hardware-detection)
- [Known Issues and Workarounds](#known-issues-and-workarounds)
- [Expected Behavior Differences](#expected-behavior-differences)
- [Testing Checklist](#testing-checklist)

## Overview

The LLM Capability Checker is built on .NET 8.0 and Avalonia UI, providing true cross-platform compatibility. However, hardware detection uses platform-specific methods:

- **Windows**: WMI (Windows Management Instrumentation)
- **Linux**: Command-line tools (lscpu, lspci, df, lsblk, etc.)
- **macOS**: Command-line tools (sysctl, system_profiler, diskutil, etc.)

## Platform Support Matrix

| Feature | Windows | Linux | macOS |
|---------|---------|-------|-------|
| CPU Detection | ✅ WMI | ✅ lscpu | ✅ sysctl |
| Memory Detection | ✅ WMI | ✅ /proc/meminfo | ✅ sysctl |
| GPU Detection | ✅ WMI | ✅ lspci | ✅ system_profiler |
| Storage Detection | ✅ WMI | ✅ df/lsblk | ✅ diskutil |
| CUDA Support | ✅ nvidia-smi | ✅ nvidia-smi | ⚠️ Limited |
| ROCm Support | ❌ N/A | ✅ rocm-smi | ❌ N/A |
| Metal Support | ❌ N/A | ❌ N/A | ✅ Built-in |
| DirectML Support | ✅ Built-in | ❌ N/A | ❌ N/A |
| OpenVINO Support | ✅ Optional | ✅ Optional | ✅ Optional |

## Prerequisites

### Linux (Ubuntu/Debian)

#### Required Packages
```bash
# Install .NET 8.0 SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Install system utilities (usually pre-installed)
sudo apt-get install -y pciutils util-linux coreutils

# Optional: NVIDIA drivers and CUDA toolkit
sudo apt-get install -y nvidia-driver-535 nvidia-utils-535

# Optional: AMD ROCm (for AMD GPUs)
# Follow AMD's official ROCm installation guide
```

#### Verify Tools
```bash
# Check .NET version
dotnet --version  # Should show 8.0.x

# Verify hardware detection tools are available
lscpu --version
lspci --version
df --version
lsblk --version

# Check NVIDIA support (if applicable)
nvidia-smi
```

### Linux (Arch/Fedora)

#### Arch Linux
```bash
# Install .NET 8.0 SDK
sudo pacman -S dotnet-sdk

# Install system utilities (usually pre-installed)
sudo pacman -S pciutils util-linux coreutils

# Optional: NVIDIA support
sudo pacman -S nvidia nvidia-utils
```

#### Fedora/RHEL
```bash
# Install .NET 8.0 SDK
sudo dnf install dotnet-sdk-8.0

# Install system utilities (usually pre-installed)
sudo dnf install pciutils util-linux coreutils

# Optional: NVIDIA support
sudo dnf install akmod-nvidia xorg-x11-drv-nvidia-cuda
```

### macOS

#### Required Software
```bash
# Install Homebrew (if not already installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET 8.0 SDK
brew install --cask dotnet-sdk

# Verify installation
dotnet --version  # Should show 8.0.x

# System utilities are built-in on macOS:
# - sysctl (CPU, memory detection)
# - system_profiler (GPU detection)
# - diskutil (storage detection)
# - sw_vers (OS version)
```

#### Verify Tools
```bash
# These should all work on macOS by default
sysctl -n hw.memsize
sysctl -n machdep.cpu.brand_string
system_profiler SPDisplaysDataType
diskutil info disk0
sw_vers -productVersion
```

### Windows

#### Required Software
1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Install the SDK (not just runtime)

2. **Verify Installation**
   ```powershell
   dotnet --version  # Should show 8.0.x
   ```

3. **Optional: NVIDIA Drivers**
   - Download from: https://www.nvidia.com/drivers
   - Install CUDA Toolkit for full CUDA detection

4. **Note**: WMI is built into Windows, no additional setup needed

## Build Instructions

### All Platforms

#### Clone Repository
```bash
git clone https://github.com/yourusername/llm-capability-checker.git
cd llm-capability-checker
```

#### Build Debug Version
```bash
dotnet build src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Debug
```

#### Build Release Version
```bash
dotnet build src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Release
```

#### Run Service Tests
```bash
# From project root
dotnet run --project src/LLMCapabilityChecker -- --test
```

#### Run Full Application
```bash
dotnet run --project src/LLMCapabilityChecker
```

### Platform-Specific Build Scripts

For convenience, use the provided test scripts:

**Linux/macOS:**
```bash
chmod +x scripts/test-linux.sh    # Linux
chmod +x scripts/test-macos.sh    # macOS
./scripts/test-linux.sh            # Run on Linux
./scripts/test-macos.sh            # Run on macOS
```

**Windows:**
```powershell
.\scripts\test-windows.bat
```

## Running Tests

### Unit Tests
```bash
# Run all tests
dotnet test tests/LLMCapabilityChecker.Tests/LLMCapabilityChecker.Tests.csproj

# Run with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~HardwareDetectionServiceTests"
```

### Integration Tests (Service Tester)
```bash
# Run service tester
dotnet run --project src/LLMCapabilityChecker -- --test

# Expected output:
# - Hardware detection results
# - Scoring calculation results
# - Model recommendations
# - No errors or exceptions
```

## Platform-Specific Hardware Detection

### Windows Detection Methods

**CPU:**
- Uses `Win32_Processor` WMI class
- Retrieves: Name, NumberOfCores, MaxClockSpeed

**Memory:**
- Uses `Win32_ComputerSystem` for total RAM
- Uses `Win32_PhysicalMemory` for type/speed
- Uses `Win32_OperatingSystem` for available RAM

**GPU:**
- Uses `Win32_VideoController` WMI class
- Falls back to `nvidia-smi` for NVIDIA GPUs
- Prioritizes dedicated GPUs over integrated

**Storage:**
- Uses `Win32_LogicalDisk` for C: drive
- Uses `Win32_DiskDrive` for physical disk info
- Uses `MSFT_PhysicalDisk` (Windows 8+) for NVMe detection

**Frameworks:**
- DirectML: Windows 10 build 18362+
- CUDA: nvidia-smi detection
- OpenVINO: Directory check

### Linux Detection Methods

**CPU:**
- Command: `lscpu`
- Parses: Model name, Core(s) per socket, CPU(s)

**Memory:**
- Command: `cat /proc/meminfo`
- Parses: MemTotal, MemAvailable

**GPU:**
- Command: `lspci` for GPU model
- Command: `nvidia-smi` for NVIDIA details
- Looks for: VGA compatible controller, 3D controller

**Storage:**
- Command: `df -BG /` for capacity and available space
- Command: `lsblk -d -o NAME,ROTA,TYPE` for type (HDD/SSD/NVMe)
- ROTA=1 means HDD, ROTA=0 means SSD/NVMe

**Frameworks:**
- CUDA: nvidia-smi detection
- ROCm: rocm-smi or /opt/rocm directory
- OpenVINO: /opt/intel/openvino directory or `which benchmark_app`

### macOS Detection Methods

**CPU:**
- Command: `sysctl -n machdep.cpu.brand_string` for model
- Command: `sysctl -n hw.physicalcpu` for cores
- Thread count from Environment.ProcessorCount

**Memory:**
- Command: `sysctl -n hw.memsize` for total RAM
- Returns bytes, converted to GB

**GPU:**
- Command: `system_profiler SPDisplaysDataType`
- Parses: Chipset Model, VRAM/Total VRAM
- Apple Silicon GPUs detected with Metal support

**Storage:**
- Command: `diskutil info disk0` for type and capacity
- Command: `df -g /` for available space
- Detects: SSD, HDD, NVMe via "Solid State" and "Protocol" fields

**Frameworks:**
- Metal: Built-in on macOS 10.13+ (detected via sw_vers)
- CUDA: Limited on Apple Silicon (legacy Intel Macs only)
- OpenVINO: /opt/intel/openvino or `which benchmark_app`

## Known Issues and Workarounds

### Linux Issues

#### Issue: Missing lspci command
**Symptom:** GPU detection fails
**Solution:**
```bash
# Ubuntu/Debian
sudo apt-get install pciutils

# Arch
sudo pacman -S pciutils

# Fedora
sudo dnf install pciutils
```

#### Issue: Permission denied for nvidia-smi
**Symptom:** CUDA version not detected
**Solution:**
```bash
# Ensure NVIDIA drivers are installed
sudo apt-get install nvidia-driver-535 nvidia-utils-535

# Verify nvidia-smi is in PATH
which nvidia-smi
```

#### Issue: /proc/meminfo not readable
**Symptom:** Memory detection returns 0 GB
**Workaround:** This is rare but can happen in containers. Run with appropriate permissions.

### macOS Issues

#### Issue: system_profiler slow on first run
**Symptom:** GPU detection takes 5-10 seconds
**Workaround:** This is normal macOS behavior. Subsequent runs are cached and faster.

#### Issue: diskutil requires full disk name
**Symptom:** Storage detection fails on some Macs
**Current implementation:** Uses `disk0` (boot disk)
**Workaround:** For external boot drives, detection may be inaccurate

#### Issue: Metal not detected on older Macs
**Symptom:** Metal framework shows as unavailable
**Expected:** Metal requires macOS 10.13+ (High Sierra)
**Workaround:** Update macOS or accept CPU-only inference

### Windows Issues

#### Issue: System.Management not available
**Symptom:** Build error about missing System.Management package
**Solution:** Package should be included in .csproj. If missing:
```bash
dotnet add package System.Management
```

#### Issue: WMI queries slow on first run
**Symptom:** Hardware detection takes 3-5 seconds
**Workaround:** This is normal Windows behavior. Results are cached.

#### Issue: nvidia-smi not in PATH
**Symptom:** CUDA detected as "Installed" without version
**Solution:**
```powershell
# Add to PATH (default CUDA location)
$env:PATH += ";C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.x\bin"
```

### Cross-Platform Issues

#### Issue: Command timeout on slow systems
**Symptom:** Detection returns "Unknown" for some components
**Solution:** Timeout is set to 2000ms (2 seconds). May need to increase for very slow systems.

#### Issue: Virtualized environments
**Symptom:** Incorrect hardware detection in VMs/containers
**Expected:** Some hardware may not be detected accurately in virtualized environments
**Workaround:** Test on bare metal for accurate results

## Expected Behavior Differences

### CPU Detection
| Platform | Expected Data | Notes |
|----------|---------------|-------|
| Windows | Full model name, cores, threads, clock speed | Most complete |
| Linux | Full model name, cores, threads | Clock speed not always available |
| macOS | Full model name, cores, threads | Clock speed not always available |

### Memory Detection
| Platform | Expected Data | Notes |
|----------|---------------|-------|
| Windows | Total, available, type (DDR4/DDR5), speed | Most complete |
| Linux | Total, available | Type/speed not available |
| macOS | Total only | Available/type/speed not available |

### GPU Detection
| Platform | Expected Data | Notes |
|----------|---------------|-------|
| Windows | Model, vendor, VRAM, NVIDIA details | Most complete |
| Linux | Model, vendor, VRAM, NVIDIA details | Requires lspci |
| macOS | Model, vendor, VRAM, Metal support | Apple Silicon detected properly |

### Storage Detection
| Platform | Expected Data | Notes |
|----------|---------------|-------|
| Windows | Type (HDD/SSD/NVMe), total, available | Most complete |
| Linux | Type (HDD/SSD/NVMe), total, available | Relies on lsblk ROTA flag |
| macOS | Type (HDD/SSD/NVMe), total, available | Requires diskutil |

### Framework Detection
| Platform | CUDA | ROCm | Metal | DirectML | OpenVINO |
|----------|------|------|-------|----------|----------|
| Windows | ✅ nvidia-smi | ❌ | ❌ | ✅ OS check | ✅ Directory |
| Linux | ✅ nvidia-smi | ✅ rocm-smi | ❌ | ❌ | ✅ Directory |
| macOS | ⚠️ Legacy | ❌ | ✅ sw_vers | ❌ | ✅ Directory |

## Testing Checklist

### Pre-Test Setup
- [ ] .NET 8.0 SDK installed (`dotnet --version`)
- [ ] Repository cloned and up-to-date
- [ ] All dependencies installed (platform-specific)
- [ ] GPU drivers installed (if applicable)

### Build Tests
- [ ] Debug build completes without errors
- [ ] Release build completes without errors
- [ ] No compiler warnings (except known Avalonia diagnostics)
- [ ] Unit tests pass (`dotnet test`)

### Functional Tests - Hardware Detection

#### CPU Detection
- [ ] CPU model name detected correctly
- [ ] Core count matches system specs
- [ ] Thread count matches system specs
- [ ] Architecture detected (x64/ARM64)
- [ ] No errors in console/logs

#### Memory Detection
- [ ] Total RAM detected correctly (±1GB tolerance)
- [ ] Available RAM shows reasonable value
- [ ] Memory type detected (Windows only)
- [ ] Memory speed detected (Windows only)
- [ ] No errors in console/logs

#### GPU Detection
- [ ] GPU model name detected
- [ ] GPU vendor detected (NVIDIA/AMD/Intel/Apple)
- [ ] VRAM amount detected (dedicated GPUs)
- [ ] Dedicated vs integrated flag correct
- [ ] NVIDIA: Compute capability detected
- [ ] NVIDIA: Architecture detected (Ampere/Turing/etc.)
- [ ] No errors in console/logs

#### Storage Detection
- [ ] Storage type detected (HDD/SSD/NVMe)
- [ ] Total capacity detected correctly (±10GB tolerance)
- [ ] Available space detected correctly
- [ ] No errors in console/logs

#### Framework Detection
- [ ] CUDA detected if NVIDIA GPU + drivers present
- [ ] CUDA version parsed correctly
- [ ] ROCm detected if AMD GPU + drivers present (Linux only)
- [ ] Metal detected on macOS 10.13+
- [ ] DirectML detected on Windows 10+
- [ ] OpenVINO detected if installed
- [ ] No false positives for missing frameworks

### Functional Tests - Scoring System
- [ ] Overall score calculated (0-100 range)
- [ ] System tier assigned correctly
- [ ] Component scores all populated
- [ ] Primary bottleneck identified
- [ ] Recommended model size appropriate
- [ ] No errors in console/logs

### Functional Tests - Model Database
- [ ] Model database loaded successfully
- [ ] Model count > 0
- [ ] Recommended models filtered correctly
- [ ] Compatibility scores reasonable
- [ ] Expected performance estimates present
- [ ] No errors in console/logs

### UI Tests (Visual Inspection)
- [ ] Application launches without errors
- [ ] Dashboard displays hardware info
- [ ] Scores display correctly
- [ ] Model recommendations show
- [ ] UI responsive (no freezing)
- [ ] Window resizes properly
- [ ] Dark/light theme works (if implemented)

### Performance Tests
- [ ] Hardware detection completes < 5 seconds
- [ ] Scoring calculation completes < 1 second
- [ ] Model filtering completes < 1 second
- [ ] Application launch time < 10 seconds
- [ ] Memory usage reasonable (< 200MB idle)

### Platform-Specific Tests

#### Linux-Specific
- [ ] Test on Ubuntu/Debian variant
- [ ] Test on Arch/Fedora variant (if available)
- [ ] Verify lspci/lscpu work as non-root
- [ ] Test with NVIDIA GPU (if available)
- [ ] Test with AMD GPU (if available)
- [ ] Test with integrated GPU

#### macOS-Specific
- [ ] Test on Intel Mac (if available)
- [ ] Test on Apple Silicon Mac (if available)
- [ ] Verify Metal detection on 10.13+
- [ ] Verify system_profiler timeout handling
- [ ] Test with external GPU (eGPU) if available

#### Windows-Specific
- [ ] Test on Windows 10 (build 18362+)
- [ ] Test on Windows 11 (if available)
- [ ] Verify WMI queries work as regular user
- [ ] Test with NVIDIA GPU (if available)
- [ ] Test with AMD GPU (if available)
- [ ] Test with integrated GPU
- [ ] Verify DirectML detection

### Regression Tests
- [ ] Test after updating .NET version
- [ ] Test after updating Avalonia version
- [ ] Test after hardware detection changes
- [ ] Test after model database updates

### Edge Cases
- [ ] Test with no dedicated GPU (integrated only)
- [ ] Test with multiple GPUs (should prefer dedicated)
- [ ] Test with low RAM (< 8GB)
- [ ] Test with slow storage (HDD)
- [ ] Test in virtual machine (may have detection issues)
- [ ] Test with missing tools (e.g., remove lspci on Linux)

## Reporting Issues

When reporting platform-specific issues, please include:

1. **Platform Information:**
   - OS name and version
   - .NET version (`dotnet --version`)
   - Architecture (x64, ARM64)

2. **Hardware Information:**
   - CPU model
   - RAM amount
   - GPU model (if applicable)
   - Storage type

3. **Error Details:**
   - Console output with `--verbosity detailed`
   - Relevant log messages
   - Stack traces

4. **Command Output:**
   - For Linux: Output of `lscpu`, `lspci`, `lsblk`, `df`
   - For macOS: Output of `sysctl`, `system_profiler SPDisplaysDataType`
   - For Windows: WMI query results (if possible)

## Continuous Integration (Future)

For CI/CD pipelines, consider:

```yaml
# Example GitHub Actions matrix
strategy:
  matrix:
    os: [ubuntu-latest, macos-latest, windows-latest]
    dotnet: ['8.0.x']
runs-on: ${{ matrix.os }}
steps:
  - uses: actions/checkout@v3
  - uses: actions/setup-dotnet@v3
    with:
      dotnet-version: ${{ matrix.dotnet }}
  - run: dotnet build --configuration Release
  - run: dotnet test --verbosity normal
```

## References

- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Avalonia UI Cross-Platform Guide](https://docs.avaloniaui.net/docs/getting-started)
- [RuntimeInformation Class](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.runtimeinformation)
- [System.Management (WMI)](https://learn.microsoft.com/en-us/dotnet/api/system.management)

---

**Last Updated:** 2025-10-15
**Tested Platforms:** Windows 11, Ubuntu 22.04, macOS 14 (Sonoma)
