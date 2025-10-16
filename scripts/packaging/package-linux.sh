#!/bin/bash
# Linux Installer Packaging Script for LLM Capability Checker
# Creates DEB and RPM packages

set -e

# Configuration
VERSION="${1:-1.0.0}"
CONFIGURATION="Release"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_FILE="$PROJECT_ROOT/src/LLMCapabilityChecker/LLMCapabilityChecker.csproj"
PUBLISH_DIR="$PROJECT_ROOT/publish/linux-x64"
INSTALLER_DIR="$PROJECT_ROOT/installers"
METADATA_DIR="$PROJECT_ROOT/packaging/metadata"

echo "====================================="
echo "LLM Capability Checker - Linux Build"
echo "====================================="
echo "Version: $VERSION"
echo ""

# Clean previous builds
echo "[1/5] Cleaning previous builds..."
rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"
mkdir -p "$INSTALLER_DIR"

# Publish the application
echo "[2/5] Publishing application..."

dotnet publish "$PROJECT_FILE" \
    -c "$CONFIGURATION" \
    -r linux-x64 \
    --self-contained true \
    -o "$PUBLISH_DIR" \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:PublishTrimmed=false \
    /p:Version="$VERSION"

echo ""
echo "Published files:"
ls -lh "$PUBLISH_DIR" | tail -n +2

# Create DEB package
echo ""
echo "[3/5] Creating DEB package..."

DEB_DIR="$PROJECT_ROOT/build/deb"
DEB_PACKAGE_DIR="$DEB_DIR/llm-capability-checker_${VERSION}_amd64"

rm -rf "$DEB_DIR"
mkdir -p "$DEB_PACKAGE_DIR/DEBIAN"
mkdir -p "$DEB_PACKAGE_DIR/usr/bin"
mkdir -p "$DEB_PACKAGE_DIR/usr/share/applications"
mkdir -p "$DEB_PACKAGE_DIR/usr/share/icons/hicolor/256x256/apps"
mkdir -p "$DEB_PACKAGE_DIR/usr/share/llm-capability-checker"

# Copy application files
cp -r "$PUBLISH_DIR"/* "$DEB_PACKAGE_DIR/usr/share/llm-capability-checker/"
chmod +x "$DEB_PACKAGE_DIR/usr/share/llm-capability-checker/LLMCapabilityChecker"

# Create symlink in /usr/bin
ln -s /usr/share/llm-capability-checker/LLMCapabilityChecker "$DEB_PACKAGE_DIR/usr/bin/llm-capability-checker"

# Copy desktop file
cp "$METADATA_DIR/linux-desktop-file.desktop" "$DEB_PACKAGE_DIR/usr/share/applications/llm-capability-checker.desktop"

# Copy icon (convert from ico if needed)
if [ -f "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.png" ]; then
    cp "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.png" \
       "$DEB_PACKAGE_DIR/usr/share/icons/hicolor/256x256/apps/llm-capability-checker.png"
elif command -v convert &> /dev/null && [ -f "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.ico" ]; then
    convert "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.ico[0]" \
            "$DEB_PACKAGE_DIR/usr/share/icons/hicolor/256x256/apps/llm-capability-checker.png"
fi

# Create control file
cat > "$DEB_PACKAGE_DIR/DEBIAN/control" << EOF
Package: llm-capability-checker
Version: $VERSION
Section: utils
Priority: optional
Architecture: amd64
Depends: libx11-6, libice6, libsm6
Maintainer: LLM Capability Checker Team <noreply@example.com>
Description: LLM Capability Checker
 A comprehensive testing suite for evaluating Large Language Model capabilities
 across various domains including reasoning, coding, knowledge, and safety.
 Built with .NET 8.0 and Avalonia UI for cross-platform support.
Homepage: https://github.com/yourusername/llm-capability-checker
EOF

# Build DEB package
dpkg-deb --build "$DEB_PACKAGE_DIR"
mv "$DEB_DIR/llm-capability-checker_${VERSION}_amd64.deb" "$INSTALLER_DIR/"

echo "DEB package created: $INSTALLER_DIR/llm-capability-checker_${VERSION}_amd64.deb"

# Create RPM package (if rpmbuild is available)
echo ""
echo "[4/5] Creating RPM package..."

if ! command -v rpmbuild &> /dev/null; then
    echo "WARNING: rpmbuild not found. Skipping RPM package creation."
    echo "Install rpm-build package to enable RPM creation."
else
    RPM_DIR="$PROJECT_ROOT/build/rpm"
    rm -rf "$RPM_DIR"
    mkdir -p "$RPM_DIR"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

    # Create spec file
    cat > "$RPM_DIR/SPECS/llm-capability-checker.spec" << EOF
Name:           llm-capability-checker
Version:        $VERSION
Release:        1%{?dist}
Summary:        LLM Capability Checker

License:        MIT
URL:            https://github.com/yourusername/llm-capability-checker
BuildArch:      x86_64
Requires:       libX11 libICE libSM

%description
A comprehensive testing suite for evaluating Large Language Model capabilities
across various domains including reasoning, coding, knowledge, and safety.
Built with .NET 8.0 and Avalonia UI for cross-platform support.

%install
mkdir -p %{buildroot}/usr/share/llm-capability-checker
mkdir -p %{buildroot}/usr/bin
mkdir -p %{buildroot}/usr/share/applications
mkdir -p %{buildroot}/usr/share/icons/hicolor/256x256/apps

cp -r $PUBLISH_DIR/* %{buildroot}/usr/share/llm-capability-checker/
ln -s /usr/share/llm-capability-checker/LLMCapabilityChecker %{buildroot}/usr/bin/llm-capability-checker
cp $METADATA_DIR/linux-desktop-file.desktop %{buildroot}/usr/share/applications/llm-capability-checker.desktop

%files
/usr/share/llm-capability-checker/*
/usr/bin/llm-capability-checker
/usr/share/applications/llm-capability-checker.desktop

%post
chmod +x /usr/share/llm-capability-checker/LLMCapabilityChecker

%changelog
* $(date +'%a %b %d %Y') Builder <builder@example.com> - $VERSION-1
- Initial package
EOF

    # Build RPM
    rpmbuild --define "_topdir $RPM_DIR" -bb "$RPM_DIR/SPECS/llm-capability-checker.spec"

    # Copy to installers directory
    find "$RPM_DIR/RPMS" -name "*.rpm" -exec cp {} "$INSTALLER_DIR/" \;

    echo "RPM package created: $INSTALLER_DIR/llm-capability-checker-${VERSION}-1.x86_64.rpm"
fi

# Create tarball for other distributions
echo ""
echo "[5/5] Creating tarball..."

TARBALL="$INSTALLER_DIR/llm-capability-checker-v${VERSION}-linux-x64.tar.gz"
tar -czf "$TARBALL" -C "$PUBLISH_DIR" .

echo "Tarball created: $TARBALL"

# Summary
echo ""
echo "====================================="
echo "SUCCESS: Linux packages created!"
echo "====================================="
echo "Location: $INSTALLER_DIR"
echo ""
echo "Created packages:"
ls -lh "$INSTALLER_DIR" | grep -E "\.(deb|rpm|tar\.gz)$" || true

echo ""
echo "Installation instructions:"
echo "  DEB (Ubuntu/Debian): sudo dpkg -i llm-capability-checker_${VERSION}_amd64.deb"
echo "  RPM (Fedora/RHEL):   sudo rpm -i llm-capability-checker-${VERSION}-1.x86_64.rpm"
echo "  Tarball:             tar -xzf llm-capability-checker-v${VERSION}-linux-x64.tar.gz"
echo ""
echo "Next steps:"
echo "  1. Test installation on target distributions"
echo "  2. Submit to package repositories (apt, yum, AUR)"
echo "  3. Consider signing packages for distribution"
echo ""
