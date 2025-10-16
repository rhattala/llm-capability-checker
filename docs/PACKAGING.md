# Packaging Guide

This guide explains how to create installer packages for LLM Capability Checker on Windows, Linux, and macOS.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Building Installers](#building-installers)
  - [Windows Installer](#windows-installer)
  - [Linux Packages](#linux-packages)
  - [macOS Installer](#macos-installer)
- [Installer Sizes](#installer-sizes)
- [Code Signing](#code-signing)
- [Distribution](#distribution)

---

## Prerequisites

### All Platforms

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (for version control)

### Windows

**Required:**
- PowerShell 5.1 or later (included with Windows)

**Optional (for installer creation):**
- [NSIS (Nullsoft Scriptable Install System)](https://nsis.sourceforge.io/Download)
  - Download and install from https://nsis.sourceforge.io/
  - Add to PATH: `C:\Program Files (x86)\NSIS`
  - Without NSIS, script creates a portable ZIP instead

**Optional (for code signing):**
- Windows SDK (includes `signtool.exe`)
- Code signing certificate

### Linux

**Required:**
- Bash shell
- `dpkg-deb` (usually pre-installed on Debian/Ubuntu)

**Optional (for RPM packages):**
- `rpmbuild` - Install with:
  - Ubuntu/Debian: `sudo apt install rpm`
  - Fedora/RHEL: `sudo dnf install rpm-build`

**Optional (for icon conversion):**
- ImageMagick: `sudo apt install imagemagick`

### macOS

**Required:**
- Bash shell (included)
- `hdiutil` (included with macOS)

**Optional (for better DMG creation):**
- `create-dmg` - Install with: `brew install create-dmg`

**Optional (for code signing):**
- Apple Developer account
- Developer ID Application certificate
- Xcode Command Line Tools: `xcode-select --install`

---

## Building Installers

### Windows Installer

#### Using PowerShell Script

```powershell
# Navigate to project root
cd d:\Projects\llm-capability-checker

# Run packaging script
.\scripts\packaging\package-windows.ps1

# Specify version (optional)
.\scripts\packaging\package-windows.ps1 -Version "1.0.0"

# Create framework-dependent build (smaller size, requires .NET runtime)
.\scripts\packaging\package-windows.ps1 -DeploymentMode framework-dependent
```

#### Output

- **With NSIS:** `installers/LLMCapabilityChecker-v1.0.0-win-x64.exe` (installer)
- **Without NSIS:** `installers/LLMCapabilityChecker-v1.0.0-win-x64.zip` (portable)

#### Manual Steps (if needed)

If you need to create the installer manually:

```powershell
# 1. Publish application
dotnet publish src/LLMCapabilityChecker/LLMCapabilityChecker.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o publish/windows-x64 `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:Version=1.0.0

# 2. Create installer with NSIS
makensis packaging/metadata/windows-installer.nsi

# Or create ZIP for portable version
Compress-Archive -Path publish/windows-x64/* -DestinationPath installers/LLMCapabilityChecker-portable.zip
```

#### Installation

Users can install by:
1. Double-clicking the `.exe` installer
2. Following the installation wizard
3. Choosing installation directory (default: `C:\Program Files\LLM Capability Checker`)
4. Creating desktop/start menu shortcuts

---

### Linux Packages

#### Using Bash Script

```bash
# Navigate to project root
cd /path/to/llm-capability-checker

# Make script executable
chmod +x scripts/packaging/package-linux.sh

# Run packaging script
./scripts/packaging/package-linux.sh

# Specify version (optional)
./scripts/packaging/package-linux.sh 1.0.0
```

#### Output

Creates three package formats:
- `installers/llm-capability-checker_1.0.0_amd64.deb` (Ubuntu/Debian)
- `installers/llm-capability-checker-1.0.0-1.x86_64.rpm` (Fedora/RHEL)
- `installers/llm-capability-checker-v1.0.0-linux-x64.tar.gz` (Universal)

#### Manual Steps (if needed)

```bash
# 1. Publish application
dotnet publish src/LLMCapabilityChecker/LLMCapabilityChecker.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -o publish/linux-x64 \
    /p:PublishSingleFile=true \
    /p:Version=1.0.0

# 2. Create DEB package structure
mkdir -p package/llm-capability-checker/DEBIAN
mkdir -p package/llm-capability-checker/usr/bin
mkdir -p package/llm-capability-checker/usr/share/applications
mkdir -p package/llm-capability-checker/usr/share/llm-capability-checker

# 3. Copy files
cp -r publish/linux-x64/* package/llm-capability-checker/usr/share/llm-capability-checker/
ln -s /usr/share/llm-capability-checker/LLMCapabilityChecker package/llm-capability-checker/usr/bin/llm-capability-checker
cp packaging/metadata/linux-desktop-file.desktop package/llm-capability-checker/usr/share/applications/

# 4. Create control file (see script for content)

# 5. Build package
dpkg-deb --build package/llm-capability-checker
```

#### Installation

**DEB (Ubuntu/Debian):**
```bash
sudo dpkg -i llm-capability-checker_1.0.0_amd64.deb
sudo apt-get install -f  # Fix dependencies if needed
```

**RPM (Fedora/RHEL):**
```bash
sudo rpm -i llm-capability-checker-1.0.0-1.x86_64.rpm
# or
sudo dnf install llm-capability-checker-1.0.0-1.x86_64.rpm
```

**Tarball (Any distribution):**
```bash
tar -xzf llm-capability-checker-v1.0.0-linux-x64.tar.gz
cd llm-capability-checker
./LLMCapabilityChecker
```

---

### macOS Installer

#### Using Bash Script

```bash
# Navigate to project root
cd /path/to/llm-capability-checker

# Make script executable
chmod +x scripts/packaging/package-macos.sh

# Run packaging script
./scripts/packaging/package-macos.sh

# Specify version (optional)
./scripts/packaging/package-macos.sh 1.0.0
```

#### Output

- Intel Macs: `installers/LLMCapabilityChecker-v1.0.0-macos-x64.dmg`
- Apple Silicon: `installers/LLMCapabilityChecker-v1.0.0-macos-arm64.dmg`

#### Manual Steps (if needed)

```bash
# 1. Publish application
dotnet publish src/LLMCapabilityChecker/LLMCapabilityChecker.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -o publish/osx-x64 \
    /p:PublishSingleFile=true \
    /p:Version=1.0.0

# 2. Create .app bundle structure
mkdir -p "LLM Capability Checker.app/Contents/MacOS"
mkdir -p "LLM Capability Checker.app/Contents/Resources"

# 3. Copy executable
cp -r publish/osx-x64/* "LLM Capability Checker.app/Contents/MacOS/"

# 4. Create Info.plist (use template from packaging/metadata/)
cp packaging/metadata/Info.plist "LLM Capability Checker.app/Contents/"

# 5. Create DMG
hdiutil create -size 200m -fs HFS+ -volname "LLM Capability Checker" temp.dmg
hdiutil attach temp.dmg
cp -r "LLM Capability Checker.app" /Volumes/LLM\ Capability\ Checker/
ln -s /Applications /Volumes/LLM\ Capability\ Checker/Applications
hdiutil detach /Volumes/LLM\ Capability\ Checker
hdiutil convert temp.dmg -format UDZO -o LLMCapabilityChecker-v1.0.0-macos.dmg
```

#### Installation

Users can install by:
1. Double-clicking the `.dmg` file
2. Dragging the app to the Applications folder
3. Launching from Applications or Spotlight

**First Launch:** If not code-signed, users need to:
- Right-click the app and select "Open", or
- Run: `xattr -cr "/Applications/LLM Capability Checker.app"`

---

## Installer Sizes

Approximate sizes for self-contained builds:

| Platform | Installer Type | Approximate Size |
|----------|---------------|------------------|
| Windows | NSIS Installer (.exe) | 80-120 MB |
| Windows | Portable ZIP | 100-140 MB |
| Linux | DEB Package | 80-120 MB |
| Linux | RPM Package | 80-120 MB |
| Linux | Tarball | 100-140 MB |
| macOS | DMG (x64) | 80-120 MB |
| macOS | DMG (arm64) | 80-120 MB |

**Reducing Size:**

1. **Framework-dependent builds** (requires .NET runtime installed):
   ```powershell
   # Windows
   .\scripts\packaging\package-windows.ps1 -DeploymentMode framework-dependent
   ```
   Size: ~10-20 MB

2. **Enable trimming** (experimental, may break Avalonia):
   Add to `.csproj`:
   ```xml
   <PublishTrimmed>true</PublishTrimmed>
   <TrimMode>partial</TrimMode>
   ```

3. **Compression:**
   - NSIS uses LZMA compression automatically
   - DMG uses UDZO (compressed) format
   - DEB/RPM use gzip compression

---

## Code Signing

### Windows

Code signing builds trust and prevents security warnings.

#### Prerequisites

- Code signing certificate (from DigiCert, Sectigo, etc.)
- Windows SDK (includes `signtool.exe`)

#### Signing Steps

```powershell
# Sign the installer
signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com "installers\LLMCapabilityChecker-v1.0.0-win-x64.exe"

# Verify signature
signtool verify /pa "installers\LLMCapabilityChecker-v1.0.0-win-x64.exe"
```

#### Certificate Options

- **Standard Code Signing:** $100-400/year
- **EV Code Signing:** $300-600/year (builds SmartScreen reputation immediately)

### macOS

Code signing is required for distribution outside the App Store.

#### Prerequisites

- Apple Developer account ($99/year)
- Developer ID Application certificate
- Xcode Command Line Tools

#### Signing Steps

```bash
# 1. Sign the app bundle
codesign --force --deep --sign "Developer ID Application: Your Name (TEAMID)" \
    "build/macos/LLM Capability Checker.app"

# 2. Verify signature
codesign --verify --deep --strict --verbose=2 \
    "build/macos/LLM Capability Checker.app"

# 3. Check for issues
spctl --assess --verbose=4 --type execute \
    "build/macos/LLM Capability Checker.app"
```

#### Notarization (Recommended)

Notarization verifies your app with Apple:

```bash
# 1. Create app-specific password at appleid.apple.com

# 2. Submit for notarization
xcrun notarytool submit "installers/LLMCapabilityChecker-v1.0.0-macos.dmg" \
    --apple-id "your@email.com" \
    --team-id "TEAMID" \
    --password "app-specific-password" \
    --wait

# 3. Staple notarization ticket
xcrun stapler staple "installers/LLMCapabilityChecker-v1.0.0-macos.dmg"

# 4. Verify
xcrun stapler validate "installers/LLMCapabilityChecker-v1.0.0-macos.dmg"
```

### Linux

Linux packages can be signed for verification.

#### DEB Signing

```bash
# Generate GPG key (if needed)
gpg --gen-key

# Sign the package
dpkg-sig --sign builder llm-capability-checker_1.0.0_amd64.deb

# Verify signature
dpkg-sig --verify llm-capability-checker_1.0.0_amd64.deb
```

#### RPM Signing

```bash
# Create GPG key and import to RPM
gpg --gen-key
gpg --export -a 'Your Name' > RPM-GPG-KEY

# Sign package
rpm --addsign llm-capability-checker-1.0.0-1.x86_64.rpm

# Verify
rpm --checksig llm-capability-checker-1.0.0-1.x86_64.rpm
```

---

## Distribution

### Direct Distribution

1. **GitHub Releases:**
   - Create release tag: `v1.0.0`
   - Upload installers as release assets
   - Include checksums (SHA256)

   ```bash
   # Generate checksums
   sha256sum installers/* > installers/SHA256SUMS.txt
   ```

2. **Website Download:**
   - Host installers on web server
   - Provide direct download links
   - Include installation instructions

### Package Repositories

#### Windows

- **Winget (Windows Package Manager):**
  - Submit to: https://github.com/microsoft/winget-pkgs
  - Create manifest file
  - Users install: `winget install llm-capability-checker`

- **Chocolatey:**
  - Create package at: https://community.chocolatey.org/
  - Users install: `choco install llm-capability-checker`

#### Linux

- **Ubuntu PPA:**
  1. Create Launchpad account
  2. Set up PPA
  3. Upload packages
  4. Users add: `sudo add-apt-repository ppa:yourname/llm-capability-checker`

- **Arch User Repository (AUR):**
  1. Create PKGBUILD
  2. Submit to AUR
  3. Users install: `yay -S llm-capability-checker`

- **Flatpak:**
  1. Create Flatpak manifest
  2. Submit to Flathub
  3. Users install: `flatpak install flathub com.llmcapabilitychecker.app`

#### macOS

- **Homebrew:**
  1. Create Homebrew formula
  2. Submit to homebrew-cask
  3. Users install: `brew install --cask llm-capability-checker`

### Update Mechanism

Consider implementing auto-update functionality:

1. **Windows:** Use Squirrel.Windows or ClickOnce
2. **macOS:** Use Sparkle framework
3. **Linux:** Package managers handle updates
4. **Cross-platform:** Implement custom update checker

---

## Troubleshooting

### Windows

**Issue:** NSIS not found
- **Solution:** Install NSIS from https://nsis.sourceforge.io/ or use ZIP fallback

**Issue:** Missing DLLs
- **Solution:** Use `--self-contained true` in publish command

### Linux

**Issue:** dpkg-deb not found
- **Solution:** Install: `sudo apt install dpkg`

**Issue:** Permission denied
- **Solution:** `chmod +x scripts/packaging/package-linux.sh`

**Issue:** Missing dependencies
- **Solution:** Check dependencies in control file

### macOS

**Issue:** App won't open (security warning)
- **Solution:** Right-click → Open, or run `xattr -cr "LLM Capability Checker.app"`

**Issue:** Code signing failed
- **Solution:** Check certificate validity and team ID

**Issue:** DMG creation failed
- **Solution:** Install create-dmg: `brew install create-dmg`

---

## CI/CD Integration

Example GitHub Actions workflow for automated builds:

```yaml
name: Build Installers

on:
  push:
    tags:
      - 'v*'

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Build Windows Installer
        run: .\scripts\packaging\package-windows.ps1
      - uses: actions/upload-artifact@v3
        with:
          name: windows-installer
          path: installers/*.exe

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Install Dependencies
        run: sudo apt-get install -y rpm
      - name: Build Linux Packages
        run: ./scripts/packaging/package-linux.sh
      - uses: actions/upload-artifact@v3
        with:
          name: linux-packages
          path: installers/*

  build-macos:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Install Dependencies
        run: brew install create-dmg
      - name: Build macOS Installer
        run: ./scripts/packaging/package-macos.sh
      - uses: actions/upload-artifact@v3
        with:
          name: macos-installer
          path: installers/*.dmg
```

---

## Resources

- [.NET Publishing Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
- [Avalonia Deployment Guide](https://docs.avaloniaui.net/docs/deployment)
- [NSIS Documentation](https://nsis.sourceforge.io/Docs/)
- [Debian Packaging Guide](https://www.debian.org/doc/manuals/maint-guide/)
- [RPM Packaging Guide](https://rpm-packaging-guide.github.io/)
- [Apple Code Signing](https://developer.apple.com/documentation/security/notarizing_macos_software_before_distribution)

---

## Support

For packaging issues:
- Check the troubleshooting section above
- Review build logs in the `publish/` and `build/` directories
- Report issues at: https://github.com/yourusername/llm-capability-checker/issues
