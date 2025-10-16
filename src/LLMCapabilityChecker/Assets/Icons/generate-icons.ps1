# Icon Generation Script for LLM Capability Checker
# This script generates icon files from the SVG source
# Requires: ImageMagick (magick command) or Inkscape

$SVG_SOURCE = "app-icon.svg"
$OUTPUT_DIR = "."

Write-Host "LLM Capability Checker - Icon Generator" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Check for ImageMagick
$hasMagick = Get-Command magick -ErrorAction SilentlyContinue
$hasInkscape = Get-Command inkscape -ErrorAction SilentlyContinue

if (-not $hasMagick -and -not $hasInkscape) {
    Write-Host "ERROR: Neither ImageMagick nor Inkscape found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install one of the following:" -ForegroundColor Yellow
    Write-Host "  1. ImageMagick: https://imagemagick.org/script/download.php" -ForegroundColor Yellow
    Write-Host "  2. Inkscape: https://inkscape.org/release/" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    exit 1
}

$tool = if ($hasMagick) { "ImageMagick" } else { "Inkscape" }
Write-Host "Using: $tool" -ForegroundColor Green
Write-Host ""

# Function to generate PNG from SVG
function Generate-PNG {
    param($size, $output)

    Write-Host "Generating: $output ($size x $size)" -ForegroundColor White

    if ($hasMagick) {
        & magick $SVG_SOURCE -resize "${size}x${size}" -background none $output
    } else {
        & inkscape $SVG_SOURCE --export-filename=$output --export-width=$size --export-height=$size
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Created: $output" -ForegroundColor Green
    } else {
        Write-Host "  Failed: $output" -ForegroundColor Red
    }
}

# Generate PNG files for Linux
Write-Host "Generating PNG files for Linux..." -ForegroundColor Cyan
Generate-PNG 512 "app-icon-512.png"
Generate-PNG 256 "app-icon-256.png"
Generate-PNG 128 "app-icon-128.png"
Generate-PNG 64 "app-icon-64.png"
Generate-PNG 32 "app-icon-32.png"
Generate-PNG 16 "app-icon-16.png"

Write-Host ""

# Generate ICO file for Windows
Write-Host "Generating ICO file for Windows..." -ForegroundColor Cyan
if ($hasMagick) {
    Write-Host "Creating app-icon.ico with multiple sizes..." -ForegroundColor White
    & magick $SVG_SOURCE -define icon:auto-resize=256,128,64,48,32,16 app-icon.ico
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Created: app-icon.ico" -ForegroundColor Green
    } else {
        Write-Host "  Failed: app-icon.ico" -ForegroundColor Red
    }
} else {
    Write-Host "  Note: ICO generation requires ImageMagick" -ForegroundColor Yellow
    Write-Host "  You can convert PNG files to ICO using online tools like:" -ForegroundColor Yellow
    Write-Host "    - https://convertio.co/png-ico/" -ForegroundColor Yellow
    Write-Host "    - https://www.icoconverter.com/" -ForegroundColor Yellow
}

Write-Host ""

# Generate ICNS file for macOS (requires ImageMagick)
Write-Host "Generating ICNS file for macOS..." -ForegroundColor Cyan
if ($hasMagick) {
    Write-Host "Creating app-icon.icns..." -ForegroundColor White
    # Create iconset directory structure
    $iconsetDir = "AppIcon.iconset"
    New-Item -ItemType Directory -Force -Path $iconsetDir | Out-Null

    # Generate required sizes for ICNS
    $sizes = @(
        @{size=16; name="icon_16x16.png"},
        @{size=32; name="icon_16x16@2x.png"},
        @{size=32; name="icon_32x32.png"},
        @{size=64; name="icon_32x32@2x.png"},
        @{size=128; name="icon_128x128.png"},
        @{size=256; name="icon_128x128@2x.png"},
        @{size=256; name="icon_256x256.png"},
        @{size=512; name="icon_256x256@2x.png"},
        @{size=512; name="icon_512x512.png"},
        @{size=1024; name="icon_512x512@2x.png"}
    )

    foreach ($icon in $sizes) {
        & magick $SVG_SOURCE -resize "$($icon.size)x$($icon.size)" -background none "$iconsetDir/$($icon.name)"
    }

    # Note: iconutil is macOS only, so we'll just create the iconset
    Write-Host "  Created: $iconsetDir/" -ForegroundColor Green
    Write-Host "  Note: Run 'iconutil -c icns AppIcon.iconset' on macOS to create .icns" -ForegroundColor Yellow
} else {
    Write-Host "  Note: ICNS generation requires ImageMagick" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Icon generation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Files generated:" -ForegroundColor Cyan
Write-Host "  - app-icon-*.png (Linux)" -ForegroundColor White
Write-Host "  - app-icon.ico (Windows)" -ForegroundColor White
Write-Host "  - AppIcon.iconset/ (macOS - run iconutil to convert)" -ForegroundColor White
