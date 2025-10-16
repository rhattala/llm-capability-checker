# Installer Output Reference

This document describes the expected output from each packaging script.

## Directory Structure After Building

```
llm-capability-checker/
├── installers/                                      # All final installers go here
│   ├── LLMCapabilityChecker-v1.0.0-win-x64.exe     # Windows NSIS installer
│   ├── LLMCapabilityChecker-v1.0.0-win-x64.zip     # Windows portable (if no NSIS)
│   ├── llm-capability-checker_1.0.0_amd64.deb      # Debian/Ubuntu package
│   ├── llm-capability-checker-1.0.0-1.x86_64.rpm   # Fedora/RHEL package
│   ├── llm-capability-checker-v1.0.0-linux-x64.tar.gz  # Linux tarball
│   ├── LLMCapabilityChecker-v1.0.0-macos-x64.dmg   # macOS Intel installer
│   ├── LLMCapabilityChecker-v1.0.0-macos-arm64.dmg # macOS Apple Silicon installer
│   └── SHA256SUMS.txt                               # Checksums for verification
│
├── publish/                                         # Temporary build output
│   ├── windows-x64/                                 # Windows build artifacts
│   ├── linux-x64/                                   # Linux build artifacts
│   ├── osx-x64/                                     # macOS Intel artifacts
│   └── osx-arm64/                                   # macOS ARM artifacts
│
└── build/                                           # Temporary packaging work
    ├── deb/                                         # DEB package build files
    ├── rpm/                                         # RPM package build files
    └── macos/                                       # macOS .app bundle

```

## Expected File Sizes

### Self-Contained Builds (includes .NET runtime)

| Installer | Compressed Size | Uncompressed Size | Description |
|-----------|----------------|-------------------|-------------|
| Windows .exe | 80-120 MB | 150-200 MB | NSIS installer with compression |
| Windows .zip | 100-140 MB | 150-200 MB | Portable ZIP archive |
| Linux .deb | 80-120 MB | 150-200 MB | Debian package |
| Linux .rpm | 80-120 MB | 150-200 MB | RPM package |
| Linux .tar.gz | 100-140 MB | 150-200 MB | Compressed tarball |
| macOS .dmg (x64) | 80-120 MB | 150-200 MB | Intel Mac installer |
| macOS .dmg (arm64) | 80-120 MB | 150-200 MB | Apple Silicon installer |

### Framework-Dependent Builds (requires .NET runtime)

| Installer | Compressed Size | Uncompressed Size | Description |
|-----------|----------------|-------------------|-------------|
| Windows .exe | 8-15 MB | 15-25 MB | Requires .NET 8.0 runtime |
| Linux .deb | 8-15 MB | 15-25 MB | Requires .NET 8.0 runtime |
| macOS .dmg | 8-15 MB | 15-25 MB | Requires .NET 8.0 runtime |

## Installation Footprint

### Windows
- **Installation Directory:** `C:\Program Files\LLM Capability Checker\`
- **Shortcuts:** Desktop + Start Menu
- **Registry Keys:**
  - `HKLM\Software\LLM Capability Checker`
  - `HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\LLM Capability Checker`
- **Disk Space Required:** ~200 MB (self-contained) or ~25 MB (framework-dependent)

### Linux (DEB/RPM)
- **Binary Location:** `/usr/bin/llm-capability-checker` (symlink)
- **Application Files:** `/usr/share/llm-capability-checker/`
- **Desktop Entry:** `/usr/share/applications/llm-capability-checker.desktop`
- **Icon:** `/usr/share/icons/hicolor/256x256/apps/llm-capability-checker.png`
- **Disk Space Required:** ~200 MB (self-contained) or ~25 MB (framework-dependent)

### macOS
- **Application Bundle:** `/Applications/LLM Capability Checker.app`
- **Contents:**
  - `Contents/MacOS/` - Executable and dependencies
  - `Contents/Resources/` - Icon and assets
  - `Contents/Info.plist` - Application metadata
- **Disk Space Required:** ~200 MB (self-contained) or ~25 MB (framework-dependent)

## Verification

### Generate Checksums
```bash
cd installers
sha256sum * > SHA256SUMS.txt
```

### Verify Checksums
```bash
sha256sum -c SHA256SUMS.txt
```

### Test Installers

**Windows:**
```powershell
# Install
.\LLMCapabilityChecker-v1.0.0-win-x64.exe

# Verify installation
Test-Path "C:\Program Files\LLM Capability Checker\LLMCapabilityChecker.exe"

# Launch
& "C:\Program Files\LLM Capability Checker\LLMCapabilityChecker.exe"

# Uninstall
& "C:\Program Files\LLM Capability Checker\Uninstall.exe"
```

**Linux (DEB):**
```bash
# Install
sudo dpkg -i llm-capability-checker_1.0.0_amd64.deb
sudo apt-get install -f  # Fix dependencies

# Verify
which llm-capability-checker
dpkg -L llm-capability-checker

# Launch
llm-capability-checker

# Uninstall
sudo apt remove llm-capability-checker
```

**Linux (RPM):**
```bash
# Install
sudo rpm -i llm-capability-checker-1.0.0-1.x86_64.rpm

# Verify
which llm-capability-checker
rpm -ql llm-capability-checker

# Launch
llm-capability-checker

# Uninstall
sudo rpm -e llm-capability-checker
```

**macOS:**
```bash
# Mount DMG
open LLMCapabilityChecker-v1.0.0-macos-x64.dmg

# Install (drag to Applications manually, or via command line)
cp -r "/Volumes/LLM Capability Checker/LLM Capability Checker.app" /Applications/

# Verify
ls -la "/Applications/LLM Capability Checker.app"

# Launch
open "/Applications/LLM Capability Checker.app"

# Uninstall
rm -rf "/Applications/LLM Capability Checker.app"
```

## Build Time Estimates

| Platform | Typical Build Time | Notes |
|----------|-------------------|--------|
| Windows | 2-5 minutes | Faster without NSIS, slower first build |
| Linux (DEB only) | 2-4 minutes | RPM adds 1-2 minutes |
| Linux (All formats) | 3-6 minutes | DEB + RPM + tarball |
| macOS (single arch) | 3-5 minutes | First build downloads dependencies |
| macOS (universal) | 6-10 minutes | Building for both x64 and arm64 |

## Common Issues and Solutions

### Windows
- **NSIS not found:** Script falls back to ZIP creation
- **Large file size:** Consider framework-dependent build
- **Antivirus blocking:** Add exception for build directory

### Linux
- **dpkg-deb not found:** `sudo apt install dpkg-dev`
- **rpmbuild not found:** `sudo apt install rpm` or skip RPM
- **Icon not created:** Install ImageMagick: `sudo apt install imagemagick`

### macOS
- **DMG creation fails:** Install create-dmg: `brew install create-dmg`
- **Code signing fails:** Requires Apple Developer account
- **App won't open:** Run `xattr -cr "LLM Capability Checker.app"`

## Distribution Checklist

Before distributing installers:

- [ ] Test installation on clean systems
- [ ] Test uninstallation process
- [ ] Verify all features work in installed version
- [ ] Generate and verify checksums
- [ ] Sign installers (Windows/macOS)
- [ ] Create release notes
- [ ] Update version numbers
- [ ] Test on multiple OS versions:
  - [ ] Windows 10
  - [ ] Windows 11
  - [ ] Ubuntu 22.04 LTS
  - [ ] Ubuntu 24.04 LTS
  - [ ] Fedora (latest)
  - [ ] macOS 11 (Big Sur)
  - [ ] macOS 12 (Monterey)
  - [ ] macOS 13 (Ventura)
  - [ ] macOS 14 (Sonoma)
- [ ] Document known issues
- [ ] Prepare support documentation

## Performance Notes

Self-contained applications:
- Larger download/install size
- No runtime dependencies
- Guaranteed compatibility
- Users don't need to install .NET

Framework-dependent applications:
- Smaller download/install size
- Requires .NET 8.0 runtime
- Faster updates (only app code changes)
- Potential version conflicts

## Cleanup

To clean up build artifacts:

```bash
# Remove all build artifacts
rm -rf publish/ build/

# Keep only installers
find installers/ -type f ! -name "*.exe" ! -name "*.deb" ! -name "*.rpm" ! -name "*.dmg" ! -name "*.tar.gz" ! -name "SHA256SUMS.txt" -delete
```
