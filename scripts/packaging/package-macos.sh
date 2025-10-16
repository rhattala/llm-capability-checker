#!/bin/bash
# macOS Installer Packaging Script for LLM Capability Checker
# Creates a .app bundle and DMG installer

set -e

# Configuration
VERSION="${1:-1.0.0}"
CONFIGURATION="Release"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_FILE="$PROJECT_ROOT/src/LLMCapabilityChecker/LLMCapabilityChecker.csproj"
INSTALLER_DIR="$PROJECT_ROOT/installers"
METADATA_DIR="$PROJECT_ROOT/packaging/metadata"
BUILD_DIR="$PROJECT_ROOT/build/macos"

APP_NAME="LLM Capability Checker"
BUNDLE_ID="com.llmcapabilitychecker.app"

echo "====================================="
echo "LLM Capability Checker - macOS Build"
echo "====================================="
echo "Version: $VERSION"
echo ""

# Detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "arm64" ]; then
    RUNTIME="osx-arm64"
    ARCH_NAME="arm64"
else
    RUNTIME="osx-x64"
    ARCH_NAME="x64"
fi

echo "Building for: $RUNTIME"
echo ""

# Clean previous builds
echo "[1/5] Cleaning previous builds..."
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"
mkdir -p "$INSTALLER_DIR"

# Publish the application
echo "[2/5] Publishing application..."

PUBLISH_DIR="$PROJECT_ROOT/publish/$RUNTIME"
rm -rf "$PUBLISH_DIR"

dotnet publish "$PROJECT_FILE" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained true \
    -o "$PUBLISH_DIR" \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:PublishTrimmed=false \
    /p:Version="$VERSION"

echo ""
echo "Published files:"
ls -lh "$PUBLISH_DIR" | tail -n +2

# Create .app bundle
echo ""
echo "[3/5] Creating .app bundle..."

APP_BUNDLE="$BUILD_DIR/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy executable and dependencies
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
chmod +x "$APP_BUNDLE/Contents/MacOS/LLMCapabilityChecker"

# Copy icon (convert from ico if needed)
if [ -f "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.icns" ]; then
    cp "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.icns" \
       "$APP_BUNDLE/Contents/Resources/app-icon.icns"
elif command -v sips &> /dev/null && [ -f "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.png" ]; then
    # Convert PNG to ICNS
    mkdir -p "$BUILD_DIR/app-icon.iconset"
    sips -z 512 512 "$PROJECT_ROOT/src/LLMCapabilityChecker/Assets/Icons/app-icon.png" \
         --out "$BUILD_DIR/app-icon.iconset/icon_512x512.png"
    iconutil -c icns "$BUILD_DIR/app-icon.iconset" \
             -o "$APP_BUNDLE/Contents/Resources/app-icon.icns"
fi

# Create Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleExecutable</key>
    <string>LLMCapabilityChecker</string>
    <key>CFBundleIconFile</key>
    <string>app-icon.icns</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.13</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSRequiresAquaSystemAppearance</key>
    <false/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2025 LLM Capability Checker Team. All rights reserved.</string>
</dict>
</plist>
EOF

echo "App bundle created: $APP_BUNDLE"

# Code signing (optional - requires Apple Developer account)
echo ""
echo "[4/5] Code signing..."

if command -v codesign &> /dev/null; then
    # Check for signing identity
    SIGNING_IDENTITY=$(security find-identity -v -p codesigning | grep "Developer ID Application" | head -n 1 | awk -F'"' '{print $2}')

    if [ -n "$SIGNING_IDENTITY" ]; then
        echo "Found signing identity: $SIGNING_IDENTITY"
        echo "Signing app bundle..."

        codesign --force --deep --sign "$SIGNING_IDENTITY" "$APP_BUNDLE"

        echo "Verifying signature..."
        codesign --verify --deep --strict --verbose=2 "$APP_BUNDLE"

        echo "App successfully signed!"
    else
        echo "WARNING: No code signing identity found."
        echo "The app will not be signed. Users will see security warnings."
        echo ""
        echo "To sign the app:"
        echo "  1. Enroll in Apple Developer Program"
        echo "  2. Create a Developer ID Application certificate"
        echo "  3. Run: codesign --force --deep --sign 'Developer ID Application: YourName' '$APP_BUNDLE'"
    fi
else
    echo "WARNING: codesign not available (not running on macOS)"
fi

# Create DMG
echo ""
echo "[5/5] Creating DMG installer..."

DMG_NAME="LLMCapabilityChecker-v${VERSION}-macos-${ARCH_NAME}.dmg"
DMG_PATH="$INSTALLER_DIR/$DMG_NAME"
TEMP_DMG="$BUILD_DIR/temp.dmg"

rm -f "$DMG_PATH" "$TEMP_DMG"

# Create temporary DMG
if command -v create-dmg &> /dev/null; then
    # Using create-dmg (brew install create-dmg)
    echo "Using create-dmg..."

    create-dmg \
        --volname "$APP_NAME" \
        --window-pos 200 120 \
        --window-size 600 400 \
        --icon-size 100 \
        --icon "$APP_NAME.app" 175 120 \
        --hide-extension "$APP_NAME.app" \
        --app-drop-link 425 120 \
        "$DMG_PATH" \
        "$BUILD_DIR"
else
    # Using hdiutil (built-in to macOS)
    echo "Using hdiutil..."

    # Calculate size needed
    SIZE=$(du -sm "$APP_BUNDLE" | awk '{print $1}')
    SIZE=$((SIZE + 50))  # Add 50MB padding

    # Create DMG
    hdiutil create -size ${SIZE}m -fs HFS+ -volname "$APP_NAME" "$TEMP_DMG"

    # Mount DMG
    MOUNT_DIR=$(hdiutil attach "$TEMP_DMG" | grep Volumes | awk '{print $3}')

    # Copy app bundle
    cp -r "$APP_BUNDLE" "$MOUNT_DIR/"

    # Create Applications symlink
    ln -s /Applications "$MOUNT_DIR/Applications"

    # Unmount
    hdiutil detach "$MOUNT_DIR"

    # Convert to compressed DMG
    hdiutil convert "$TEMP_DMG" -format UDZO -o "$DMG_PATH"

    rm "$TEMP_DMG"
fi

echo "DMG created: $DMG_PATH"

# Create universal binary (optional)
echo ""
echo "NOTE: To create a universal binary (Intel + Apple Silicon):"
echo "  1. Build for both osx-x64 and osx-arm64"
echo "  2. Use 'lipo' to create universal binaries"
echo "  3. Package into single .app bundle"
echo ""

# Summary
echo "====================================="
echo "SUCCESS: macOS installer created!"
echo "====================================="
echo "Location: $DMG_PATH"

if [ -f "$DMG_PATH" ]; then
    DMG_SIZE=$(du -h "$DMG_PATH" | awk '{print $1}')
    echo "Size: $DMG_SIZE"
fi

echo ""
echo "Installation instructions:"
echo "  1. Double-click the DMG file"
echo "  2. Drag '$APP_NAME.app' to Applications folder"
echo "  3. Launch from Applications or Spotlight"
echo ""

if [ -z "$SIGNING_IDENTITY" ]; then
    echo "⚠️  WARNING: App is not code-signed"
    echo "Users will need to right-click and select 'Open' on first launch"
    echo "Or run: xattr -cr '/Applications/$APP_NAME.app'"
    echo ""
fi

echo "Next steps:"
echo "  1. Test on both Intel and Apple Silicon Macs"
echo "  2. Notarize with Apple (for distribution outside App Store)"
echo "  3. Consider creating a universal binary"
echo ""
