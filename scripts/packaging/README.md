# Packaging Scripts

This directory contains scripts for creating installer packages for LLM Capability Checker.

## Quick Start

### Windows
```powershell
.\scripts\packaging\package-windows.ps1
```

### Linux
```bash
chmod +x scripts/packaging/package-linux.sh
./scripts/packaging/package-linux.sh
```

### macOS
```bash
chmod +x scripts/packaging/package-macos.sh
./scripts/packaging/package-macos.sh
```

## Output Location

All installers are created in the `installers/` directory at the project root.

## Documentation

For detailed instructions, prerequisites, and troubleshooting, see:
- [docs/PACKAGING.md](../../docs/PACKAGING.md)

## Script Options

### package-windows.ps1
- `-Version` - Set version number (default: 1.0.0)
- `-Configuration` - Build configuration (default: Release)
- `-DeploymentMode` - "self-contained" or "framework-dependent" (default: self-contained)

Example:
```powershell
.\package-windows.ps1 -Version "1.2.0" -DeploymentMode self-contained
```

### package-linux.sh
```bash
./package-linux.sh [version]
```

Example:
```bash
./package-linux.sh 1.2.0
```

### package-macos.sh
```bash
./package-macos.sh [version]
```

Example:
```bash
./package-macos.sh 1.2.0
```

## Notes

- Scripts must be run from the project root or their own directory
- .NET 8.0 SDK is required
- Additional tools may be needed (NSIS for Windows, rpmbuild for Linux RPM, etc.)
- See full documentation for platform-specific prerequisites
