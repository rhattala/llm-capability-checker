# LLM Capability Checker - Brand Guidelines

This document defines the visual identity and branding standards for the LLM Capability Checker application.

## Application Name

**Official Name:** LLM Capability Checker

**Short Name:** LCC (for icon text or abbreviated references)

**Usage:**
- Always capitalize "LLM Capability Checker" in official documentation
- Use "LLM Capability Checker" on first reference, "the application" or "LCC" thereafter
- Acceptable variations: "LLM Checker", "Capability Checker"

## Color Palette

### Primary Colors

#### Blue (Primary Brand Color)
- **Color:** `#1976D2`
- **RGB:** 25, 118, 210
- **Usage:** Primary buttons, headers, brand elements, icon backgrounds
- **Hover State:** `#42A5F5` (lighter blue)
- **Pressed State:** `#0D47A1` (darker blue)

#### Light Blue (Accent)
- **Color:** `#E3F2FD`
- **RGB:** 227, 242, 253
- **Usage:** Accent cards, information backgrounds, highlighted sections

### Secondary Colors

#### Green (Success/Performance)
- **Color:** `#4CAF50`
- **RGB:** 76, 175, 80
- **Usage:** Success indicators, checkmarks, performance bars, positive metrics
- **Light Variant:** `#E8F5E9` for card backgrounds

#### Orange (Warning)
- **Color:** `#F57C00`
- **RGB:** 245, 124, 0
- **Alternative:** `#FF9800`
- **Usage:** Warning indicators, attention-needed states

#### Red (Error)
- **Color:** `#D32F2F`
- **RGB:** 211, 47, 47
- **Usage:** Error states, critical warnings

### Neutral Colors

#### Backgrounds
- **Main Background:** `#000000` (Black)
- **Card Background:** `#F5F5F5` (Light Gray)
- **White:** `#FFFFFF`

#### Text Colors
- **Section Headers:** `White` on black background (FontSize 24+)
- **Card Titles:** `#212121` (Very Dark Gray)
- **Body Text:** `#212121` or `#424242` (Dark Gray)
- **Secondary Text:** `#666666` (Medium Gray)
- **Labels:** `#666666` or `#555555` (Medium Gray)

#### UI Elements
- **Borders:** `#E0E0E0` (Light Gray)
- **Progress Background:** `#E0E0E0`

## Typography

### Font Family
- **Primary:** Inter (via Avalonia.Fonts.Inter package)
- **Fallback:** System default sans-serif
- **Monospace:** Consolas, Courier New, monospace (for code/license text)

### Font Sizes

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Section Headers | 24px | SemiBold | White |
| Card Titles | 18px | SemiBold | #212121 |
| Body Text | 16px | Regular | #212121 |
| Labels | 14px | Bold | #666666 |
| Small Text | 12px | Regular | #424242 |
| Large Display | 32px+ | Bold | #1976D2 |

### Line Height
- **Body Text:** 24px (1.5x font size)
- **Dense Text:** 18px (for license/code blocks)
- **Headers:** Auto (default)

## Icon Design

### App Icon Specifications

**Source File:** `src/LLMCapabilityChecker/Assets/Icons/app-icon.svg`

**Design Elements:**
- CPU/chip with pins (represents hardware detection)
- Performance graph bars (represents capability analysis)
- Green checkmark overlay (represents validation/approval)

**Colors:**
- Primary: Blue `#1976D2`
- Accent: White `#FFFFFF`
- Highlight: Green `#4CAF50`
- Light Blue fill: `#E3F2FD`

**Sizes Required:**

| Platform | Format | Sizes |
|----------|--------|-------|
| Windows | .ico | 16×16, 32×32, 48×48, 64×64, 128×128, 256×256 |
| Linux | .png | 16×16, 32×32, 64×64, 128×128, 256×256, 512×512 |
| macOS | .icns | Standard iconset (16-1024@1x-2x) |

### Icon Usage Guidelines

**DO:**
- Use the official icon for application launchers
- Maintain original aspect ratio
- Use appropriate size for context (larger for splash, smaller for taskbar)
- Ensure icon is visible on both light and dark backgrounds

**DON'T:**
- Modify icon colors or design elements
- Stretch or distort the icon
- Add shadows or effects (icon has built-in design)
- Use low-resolution versions in high-DPI contexts

## UI Component Standards

### Cards

**Style:**
```xml
<Border Background="#F5F5F5"
        CornerRadius="8"
        Padding="20"
        Margin="0,8,8,0">
```

**Variants:**
- Light Gray (`#F5F5F5`) - Default cards
- Light Blue (`#E3F2FD`) - Information/feature cards
- Light Green (`#E8F5E9`) - Success/positive cards
- White (`#FFFFFF`) - Inner content areas

### Buttons

**Primary Button (Blue):**
```xml
<Button Background="#1976D2"
        Foreground="White"
        Padding="24,12"
        CornerRadius="4"
        Cursor="Hand">
```

**States:**
- Normal: `#1976D2`
- Hover: `#42A5F5`
- Pressed: `#0D47A1`

**Secondary Button (Gray):**
- Normal: `#666666`
- Hover: `#888888`
- Pressed: `#444444`

### Spacing System

**Hierarchy:**
- **Section Spacing:** 16px between major sections
- **Card Spacing:** 12px or 8px between cards
- **Internal Spacing:** 8px within components
- **Card Padding:** 20px
- **Button Padding:** 24px horizontal, 12px vertical

### Corner Radius
- **Cards:** 8px
- **Buttons:** 4px
- **Progress Bars:** 4px
- **Icon Circle:** 64px (half of diameter)

### Shadows and Elevation
- **Cards:** No shadow (flat design)
- **Buttons:** No shadow (uses hover state color change)
- **Focus:** Use Avalonia's default focus indicators

## Theme Support

### Dark Theme (Default)
- Main Background: Black `#000000`
- Section Headers: White text
- Cards: Light gray `#F5F5F5`
- Card Text: Dark gray `#212121`

### Light Theme
- When implemented, maintain contrast ratios
- Ensure all text meets WCAG AA standards (4.5:1 for body text)
- Keep primary blue consistent across themes

## Accessibility

### Color Contrast
- Section headers on black: White text (highest contrast)
- Body text on light cards: Dark gray `#212121` (contrast ratio >7:1)
- Button text: White on blue (contrast ratio >4.5:1)

### Interactive Elements
- Always include `Cursor="Hand"` for clickable elements
- Provide hover states for all interactive components
- Ensure buttons have visible pressed states

### Text Sizing
- Minimum font size: 12px (for license/code text)
- Body text: 14-16px for comfortable reading
- Headers: 18-24px for clear hierarchy

## Application Window

### Window Properties
- **Default Size:** 1200×800px
- **Minimum Size:** 900×600px
- **Title Format:** "LLM Capability Checker - v{VERSION}"
- **Icon:** `Assets/Icons/app-icon.ico`

### Title Bar Elements
- Transparent buttons in top-right corner
- About button (ℹ symbol)
- Settings button (⚙ symbol)
- Hover state: Blue background `#1976D2`

## About Page

The About page should include:
- Application icon (128×128px)
- App name and version
- Description
- Technologies used
- GitHub link
- Copyright and license information
- Credits to Avalonia UI and .NET

**Layout:** Single-column, centered, max width 600px, with proper spacing between sections.

## Version Information

**Current Version:** 1.0.0

**Version Format:** Semantic Versioning (MAJOR.MINOR.PATCH)

**Display Locations:**
- About page
- Window title
- Exported reports (future feature)

## File Locations

### Icon Assets
```
src/LLMCapabilityChecker/Assets/Icons/
├── app-icon.svg (source)
├── app-icon.ico (Windows)
├── app-icon-512.png (Linux)
├── app-icon-256.png (Linux)
├── app-icon-128.png (Linux)
├── app-icon-64.png (Linux)
├── app-icon-32.png (Linux)
└── README.md (generation instructions)
```

### Documentation
```
BRANDING.md (this file)
UI_STYLE_GUIDE.md (technical UI guidelines)
```

## Tools and Resources

### Icon Generation
- **ImageMagick:** For automated icon conversion
- **Inkscape:** Alternative SVG to raster conversion
- **Script:** `Assets/Icons/generate-icons.ps1`

### Online Converters (Manual)
- PNG to ICO: https://convertio.co/png-ico/
- ICO Converter: https://www.icoconverter.com/

## Consistency Checklist

When adding new UI elements:

- [ ] Uses established color palette
- [ ] Follows spacing system (16-12-8 rule)
- [ ] Includes hover states for interactive elements
- [ ] Uses correct font sizes and weights
- [ ] Maintains proper contrast ratios
- [ ] Includes cursor pointer for clickable items
- [ ] Uses rounded corners (8px cards, 4px buttons)
- [ ] White text for headers on black background
- [ ] Dark text for content on light cards

## Future Considerations

### Planned Additions
- Light theme variant
- High contrast mode
- Custom icon set for UI elements
- Animated logo for splash screen
- Brand illustrations for empty states

### Internationalization
- Ensure color choices work across cultures
- Plan for text expansion in other languages
- Maintain icon clarity without text

---

**Document Version:** 1.0
**Last Updated:** October 15, 2025
**Maintainer:** LLM Capability Checker Development Team
