# Icon Assets

This directory contains the application icon assets for LLM Capability Checker.

## Source File

- **app-icon.svg** - Vector source file (512x512 viewbox)
  - Design: CPU chip with performance graph and checkmark
  - Colors: Blue (#1976D2), White, Green (#4CAF50)
  - Can be edited with any SVG editor (Inkscape, Adobe Illustrator, etc.)

## Generating Icons

### Automated (Recommended)

Run the PowerShell script to generate all icon formats:

```powershell
cd src/LLMCapabilityChecker/Assets/Icons
.\generate-icons.ps1
```

**Requirements:**
- ImageMagick: https://imagemagick.org/script/download.php
- OR Inkscape: https://inkscape.org/release/

### Manual Generation

If you don't have ImageMagick or Inkscape, you can convert the SVG manually:

#### Windows (.ico)
1. Open app-icon.svg in a browser
2. Take a screenshot or export to PNG at 256x256
3. Use an online converter: https://convertio.co/png-ico/
4. Generate ICO with sizes: 256, 128, 64, 48, 32, 16
5. Save as `app-icon.ico`

#### Linux (.png)
1. Export SVG to PNG at these sizes:
   - app-icon-512.png (512x512)
   - app-icon-256.png (256x256)
   - app-icon-128.png (128x128)
   - app-icon-64.png (64x64)
   - app-icon-32.png (32x32)

#### macOS (.icns)
1. Create folder: `AppIcon.iconset`
2. Export PNG files with these exact names:
   - icon_16x16.png (16x16)
   - icon_16x16@2x.png (32x32)
   - icon_32x32.png (32x32)
   - icon_32x32@2x.png (64x64)
   - icon_128x128.png (128x128)
   - icon_128x128@2x.png (256x256)
   - icon_256x256.png (256x256)
   - icon_256x256@2x.png (512x512)
   - icon_512x512.png (512x512)
   - icon_512x512@2x.png (1024x1024)
3. Run on macOS: `iconutil -c icns AppIcon.iconset`

## Icon Files

After generation, you should have:

- `app-icon.ico` - Windows icon (multi-size)
- `app-icon-512.png` - Linux icon 512x512
- `app-icon-256.png` - Linux icon 256x256
- `app-icon-128.png` - Linux icon 128x128
- `app-icon-64.png` - Linux icon 64x64
- `app-icon-32.png` - Linux icon 32x32
- `app-icon.icns` - macOS icon (generated from iconset)

## Usage

The icons are automatically referenced in:
- `LLMCapabilityChecker.csproj` - ApplicationIcon property
- `MainWindow.axaml` - Window Icon property
- `App.axaml.cs` - Window icon initialization

## Design Guidelines

See `BRANDING.md` in the project root for:
- Icon usage guidelines
- Color specifications
- Branding standards
