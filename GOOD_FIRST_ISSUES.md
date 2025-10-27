# Good First Issues to Create

Copy-paste these into GitHub issues at: https://github.com/random-llama/llm-capability-checker/issues/new

---

## Issue 1: Add Markdown export format for system reports

**Labels**: `good first issue`, `enhancement`

### Description
Currently, the app can export system reports, but we need to add Markdown format support for easy sharing on GitHub, forums, and documentation.

### Acceptance Criteria
- [ ] Add Markdown (.md) export option to the report export dialog
- [ ] Format should include:
  - Hardware specs as a table
  - Capability scores with badges/icons (using markdown emojis)
  - Model compatibility lists
  - Upgrade recommendations
- [ ] Should follow GitHub Flavored Markdown spec
- [ ] See `examples/sample-report.txt` for the data structure to format

### Technical Details
- Location: Likely in `Services/ReportExportService.cs` or similar
- Reference the existing plain text export implementation
- Use markdown tables for structured data

### Example Output
```markdown
# System Report

## Hardware
| Component | Details |
|-----------|---------|
| CPU | AMD Ryzen 7 5800X (8C/16T) |
| GPU | RTX 4070 Ti (12GB) |
...
```

### Help Available
I'm happy to provide guidance on:
- Where to find the export service code
- How to structure the markdown output
- Testing the export functionality

Comment on this issue if you want to tackle it!

---

## Issue 2: Add search and filter to model recommendations list

**Labels**: `good first issue`, `enhancement`

### Description
The model recommendations view shows 107 models, but there's no way to search or filter them. Add search/filter functionality to help users find specific models quickly.

### Acceptance Criteria
- [ ] Add a search textbox above the model list
- [ ] Filter models by name as user types (real-time)
- [ ] Add filter chips/dropdowns for:
  - Model size (1B, 7B, 13B, 70B, etc.)
  - Quantization type (Q2, Q4, Q5, Q8, FP16)
  - Compatibility tier (Perfect Match, Good Fit, etc.)
- [ ] Preserve filters when switching between tabs
- [ ] Show "X models found" count

### Technical Details
- Location: `Views/ModelRecommendationsView.xaml` and corresponding ViewModel
- Use Avalonia's CollectionView filtering
- May need to add filter properties to the ViewModel

### UI Mockup
```
[Search: ________________] [Size: All ▼] [Quant: All ▼]
Showing 23 of 107 models

✨ Perfect Matches
  • Llama 4 Scout 8B Q8 - 96%
  ...
```

### Help Available
I'm happy to help with:
- Avalonia UI data binding concepts
- ViewModel filter logic implementation
- Testing with different filter combinations

---

## Issue 3: Add support for additional AMD GPU models to database

**Labels**: `good first issue`, `data`, `enhancement`

### Description
Our GPU database is missing several popular AMD GPUs. Add support for Radeon RX 7000 and 6000 series cards with accurate VRAM specs.

### Acceptance Criteria
- [ ] Add the following AMD GPUs to the detection logic:
  - RX 7900 XTX (24GB)
  - RX 7900 XT (20GB)
  - RX 7800 XT (16GB)
  - RX 7700 XT (12GB)
  - RX 6900 XT (16GB)
  - RX 6800 XT (16GB)
  - RX 6800 (16GB)
  - RX 6700 XT (12GB)
- [ ] Include accurate VRAM capacities
- [ ] Add ROCm compatibility notes
- [ ] Test detection works on Linux (if possible)

### Technical Details
- Location: Likely in `Services/HardwareDetectionService.cs` or `Data/gpu-database.json`
- May need to add AMD-specific detection logic for Linux (`rocm-smi`)
- Reference existing NVIDIA GPU detection code

### Resources
- [AMD GPU specs](https://www.amd.com/en/products/graphics/desktops/radeon)
- [ROCm supported GPUs](https://rocm.docs.amd.com/projects/install-on-linux/en/latest/reference/system-requirements.html)

### Help Available
Even if you don't have AMD hardware, you can:
- Add the GPU specs to the database
- I can test detection on actual hardware
- Work on the detection logic together

---

## Issue 4: Create HTML report template with styling

**Labels**: `good first issue`, `enhancement`, `design`

### Description
Add HTML export format for system reports with clean, modern styling that can be viewed in browsers and shared via links.

### Acceptance Criteria
- [ ] Create HTML template with embedded CSS (no external files)
- [ ] Include sections:
  - Hardware specs table
  - Score gauges/progress bars (visual)
  - Model compatibility lists with color coding
  - Upgrade recommendations
- [ ] Mobile-responsive design
- [ ] Print-friendly CSS
- [ ] Dark mode support (prefer-color-scheme)
- [ ] Copy button for sharing report URL

### Design Requirements
- Clean, professional look (not flashy)
- Use system fonts (no external font dependencies)
- Color scheme: Similar to app (blues/greens for good, yellows/reds for warnings)
- Should work offline (all assets embedded)

### Technical Details
- Location: Create `Services/HtmlReportGenerator.cs` or add to existing export service
- Use C# string interpolation or embedded resource file
- Reference `examples/sample-report.txt` for data structure

### Example Libraries (optional)
- Can use minimal inline SVG for score gauges
- Chart.js (CDN) for optional performance charts
- Or keep it simple with just HTML/CSS

### Help Available
I'm happy to:
- Review design mockups
- Help with C# HTML generation code
- Test the output in different browsers

---

## How to Claim an Issue

1. Go to https://github.com/random-llama/llm-capability-checker/issues
2. Create a new issue with the title and content above
3. Add the labels specified
4. Comment "I'd like to work on this" on the issue you want
5. Fork the repo and start coding!

## Development Setup

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to set up your dev environment and submit a PR.
