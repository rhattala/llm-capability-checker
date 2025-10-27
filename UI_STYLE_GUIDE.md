# UI Style Guide - LLM Capability Checker

## Color Palette

### Background
- **Main Background**: Black `#000000`
- **Card Backgrounds**: Light Gray `#F5F5F5`
- **Accent Cards (Blue)**: Light Blue `#E3F2FD`
- **Accent Cards (Green)**: Light Green `#E8F5E9`

### Text Colors
- **Section Headers (Large)**: White `Foreground="White"` (FontSize 24+)
- **Card Titles**: Dark Gray `#212121`
- **Body Text**: Dark Gray `#212121` or `#424242`
- **Secondary Text**: Medium Gray `#666666` or `#555555`
- **Labels**: Medium Gray `#666666`

### Accent Colors
- **Primary Blue**: `#1976D2`
- **Primary Green**: `#4CAF50`
- **Warning Orange**: `#F57C00` or `#FF9800`
- **Error Red**: `#D32F2F`

### Interactive Elements
- **Button Normal**: Blue `#1976D2`
- **Button Hover**: Light Blue `#42A5F5`
- **Button Pressed**: Dark Blue `#0D47A1`
- **Button Text**: White

## Typography

### Headers
```xml
<!-- Section Header (24px, White) -->
<TextBlock Text="Section Title"
           FontSize="24"
           FontWeight="SemiBold"
           Foreground="White"/>
```

### Card Titles
```xml
<!-- Card Title (18px, Dark Gray) -->
<TextBlock Text="Card Title"
           FontSize="18"
           FontWeight="SemiBold"
           Foreground="#212121"/>
```

### Labels
```xml
<!-- Label (14px, Medium Gray) -->
<TextBlock Text="Label:"
           FontSize="14"
           FontWeight="Bold"
           Foreground="#666666"/>
```

### Body Text
```xml
<!-- Body Text (16px, Dark Gray) -->
<TextBlock Text="Body content"
           FontSize="16"
           Foreground="#212121"/>
```

## Components

### Card Style
```xml
<Border Background="#F5F5F5"
        CornerRadius="8"
        Padding="20"
        Margin="0,8,8,0">
  <!-- Content -->
</Border>
```

### Button Style
```xml
<Button Content="Button Text"
        Background="#1976D2"
        Foreground="White"
        Padding="24,12"
        CornerRadius="4"
        Cursor="Hand">
  <Button.Styles>
    <Style Selector="Button:pointerover /template/ ContentPresenter">
      <Setter Property="Background" Value="#42A5F5"/>
    </Style>
    <Style Selector="Button:pressed /template/ ContentPresenter">
      <Setter Property="Background" Value="#0D47A1"/>
    </Style>
  </Button.Styles>
</Button>
```

### Progress Bar
```xml
<ProgressBar Value="{Binding Score}"
             Maximum="100"
             Height="8"
             Foreground="#4CAF50"
             Background="#E0E0E0"
             CornerRadius="4"/>
```

## Spacing

- **Section Spacing**: `16px` between major sections
- **Card Spacing**: `12px` between cards within a section
- **Internal Spacing**: `8px` within cards
- **Card Padding**: `20px`

## Consistency Rules

### ✅ DO:
- Use `Foreground="White"` for ALL section headers (FontSize 24+)
- Use dark text colors (#212121, #424242) on light backgrounds
- Use consistent spacing (16px sections, 12px cards, 8px internal)
- Use rounded corners (CornerRadius="8" for cards, "4" for buttons)
- Add cursor pointer for all interactive elements (`Cursor="Hand"`)

### ❌ DON'T:
- Mix dark text on black backgrounds (use white instead)
- Use inconsistent font sizes for similar elements
- Skip hover states on buttons
- Forget to set explicit text colors

## When Adding New UI Elements

1. **Headers**: Always white text on black background
2. **Cards**: Always light gray background (#F5F5F5) with dark text
3. **Buttons**: Always blue with hover states
4. **Spacing**: Follow 16-12-8 rule (section-card-internal)
5. **Corners**: Always rounded (8px cards, 4px buttons)
