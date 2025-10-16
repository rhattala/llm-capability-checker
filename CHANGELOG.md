# Changelog

All notable changes to LLM Capability Checker will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Multi-GPU support for combined VRAM detection
- Custom model entry for testing unlisted models
- Historical score tracking over time
- CLI version for automation and scripting
- Community database integration for shared benchmarks
- Internationalization (i18n) support
- Dark mode / light mode themes

---

## [1.0.2] - 2025-01-16

### 🔄 Changed

#### Reference System Update
- **Updated Benchmark Baseline to 2025 Mid-Tier Standards**
  - CPU: AMD Ryzen 5 7600 (6C/12T, Zen 4) - up from Ryzen 5 5600X
  - GPU: RTX 4060 8GB (Ada Lovelace) - up from RTX 3060 12GB
  - RAM: 32GB DDR5-5600 (44.8 GB/s) - up from DDR4-3200 (25 GB/s)
  - Storage: NVMe Gen4 - up from Gen3
  - Cinebench R23 baseline: 1800 single / 14000 multi (up from 1000/6000)
  - Memory bandwidth baseline: 44.8 GB/s (up from 25 GB/s)
  - This reflects current 2025 mid-tier gaming/workstation builds (~$800)
  - Your comparison scores may change but remain accurate relative to current hardware

#### Storage Detection Enhancement
- **Multi-Drive Scanning with Intelligent Selection**
  - Now queries ALL physical drives (not just DeviceId='0')
  - Priority-based selection: NVMe Gen5=110, NVMe=100, SSD=50, HDD=10
  - Displays "NVMe (fastest of 3)" when multiple drives detected
  - Special detection for PCIe Gen 5 drives (Crucial T705/T700)
  - All detection is dynamic via WMI queries (no hardcoded values)

---

## [1.0.1] - 2025-01-16

### 🔧 Fixed

#### Training Score Clarity
- **Improved Training Capability Messages**: Training score now provides clear context when VRAM is insufficient
  - Changed "Training not recommended" to actionable message: "Training requires 12GB+ VRAM (you have 11GB). Use LoRA/QLoRA instead →"
  - Users with 11GB VRAM now see: "LoRA training only (you have 11GB VRAM, full training needs 16GB+)"
  - Messages now explain requirements and alternatives instead of just saying "not recommended"

#### Model Download Links
- **Working Download Buttons**: "Download via Ollama" buttons now functional
  - Opens model page in browser (Ollama library or Hugging Face)
  - Falls back to constructing Ollama URL from model name if not specified
  - "Browse All 100+ Models" button opens Ollama library
  - Added `OllamaUrl` and `HuggingFaceUrl` properties to ModelInfo

#### Storage Detection
- **Enhanced NVMe Detection**: More reliable NVMe vs SSD identification
  - Added model name checking for common NVMe brands (Samsung 970/980/990, WD Black, Corsair MP, Kingston NV, Crucial P)
  - Added FriendlyName checking as additional detection method
  - Improved logging to help debug detection issues
  - Now checks both BusType (17 = NVMe) and model name patterns

#### RAM/VRAM Display Accuracy
- **Fixed Rounding Issue**: RAM and VRAM now round to nearest GB instead of truncating
  - 64GB RAM now displays as 64GB (was showing 63GB)
  - 12GB VRAM now displays as 12GB (was showing 11GB)
  - Changed `BytesToGB()` and `MBToGB()` to use `Math.Round()` instead of integer division

### 🎯 Changed
- Model download buttons tooltip changed from "Coming soon" to "Open download page in browser"
- More detailed hardware detection logging for troubleshooting

---

## [1.0.0] - 2025-01-15

### 🎉 Initial Release

The first public release of LLM Capability Checker - a comprehensive tool to assess your PC's ability to run, train, and fine-tune Large Language Models locally.

### ✨ Added

#### Core Features
- **Three-Score System**: Specialized scoring for Inference, Training, and Fine-Tuning capabilities
  - Inference score (0-100): Running models for chat and generation
  - Training score (0-100): Full model fine-tuning capabilities
  - Fine-Tuning score (0-100): LoRA/QLoRA parameter tuning efficiency
  - Component breakdown showing contribution from CPU, GPU, RAM, and storage
  - Clear capability descriptions for each score level
  - Specific bottleneck identification

#### Hardware Detection
- **Cross-Platform Support**: Works on Windows, Linux, and macOS
- **CPU Detection**: Model, cores, threads, cache size
- **GPU Detection**: Model name, VRAM, CUDA/ROCm/Metal support
- **RAM Detection**: Capacity, speed, type (DDR4/DDR5)
- **Storage Detection**:
  - Multi-drive support
  - NVMe vs SSD identification
  - Available space tracking
- **Framework Detection**: CUDA, ROCm, DirectML, Metal availability
- **Automatic Scanning**: Hardware detected in under 2 seconds on first launch

#### Model Database
- **107 Models**: Comprehensive database of latest LLM models
  - Llama 4 family (Scout 8B, Maverick 70B, Behemoth 405B)
  - Phi-4 14B
  - DeepSeek-R1 70B and DeepSeek-V3 671B
  - Qwen 2.5 series (0.5B to 72B)
  - Mistral Large 123B
  - Gemma 2, Command-R, and more
- **Five Quantization Levels**: Q2_K, Q4_K_M, Q5_K_M, Q8_0, FP16
- **Smart Recommendations**: Models categorized by compatibility
  - Perfect Matches (90-100 score)
  - Good Fit (70-89 score)
  - Possible with Optimization (50-69 score)
  - Not Recommended (<50 score)
- **Detailed Model Info**: Parameters, VRAM requirements, use cases, download links

#### Community Features
- **Community Recommendations**: User-curated model suggestions
  - Trending tab: Most upvoted models (last 30 days)
  - Top Rated tab: Highest quality picks
  - For Your System tab: Filtered by your hardware
  - Upvoting system for favorites
  - User reviews and ratings
  - Use case tags (coding, chat, reasoning)
  - Hardware tier recommendations
- **15+ Curated Recommendations**: Pre-populated with popular models
- **Local Storage**: 100% privacy-first, all data stored locally

#### Upgrade Advisor
- **Smart GPU Suggestions**: Intelligent upgrade recommendations
  - Detects exact GPU model
  - Only suggests meaningful upgrades (no lateral moves)
  - Shows specific models unlocked by each upgrade
  - VRAM tier recommendations (8GB → 12GB → 16GB → 24GB+)
- **Upgrade Impact Analysis**: See what you'll gain from upgrading
- **Budget-Friendly Options**: Multiple upgrade paths at different price points

#### User Interface
- **Dashboard View**: At-a-glance capability overview
  - Overall score card with grade (A+ to F)
  - Three specialized scores (Inference, Training, Fine-Tuning)
  - Quick hardware summary
  - Top recommended models
- **Hardware Details View**: In-depth system information
  - Detailed component specifications
  - Framework support status
  - Performance indicators
  - Reference system comparison
- **Model Browser**: Explore compatible models
  - Search and filter functionality
  - Sort by compatibility, size, or name
  - Detailed model cards with specs
  - Direct download links to Hugging Face
- **Community Tab**: Browse user recommendations
  - Three sorting modes (Trending, Top Rated, For Your System)
  - Review system with star ratings
  - Upvote/downvote functionality
- **Settings**: Customization options
  - Hardware refresh
  - Detailed logging toggle
  - Data export functionality
  - About screen with version info

#### Transparency Features
- **Reference System Card**: Baseline comparison system
  - AMD Ryzen 5 5600X
  - RTX 3060 12GB
  - 32GB DDR4-3200
  - 1TB NVMe Gen3
  - Updated: January 2025
- **Component Breakdown**: See exactly what contributes to each score
- **Clear Scoring Rubric**: Understand what each score means
- **Open Source**: Full code available for audit

#### Data & Privacy
- **100% Local Operation**: No telemetry or tracking
- **No Account Required**: Works completely offline (after model DB download)
- **Privacy-First Design**: All user data stays on your machine
- **Optional Sharing**: Opt-in for community benchmarks (future feature)
- **Export Reports**: Save system information for sharing or troubleshooting

#### Documentation
- **Comprehensive User Guide**: Step-by-step usage instructions
- **FAQ**: Answers to common questions
- **Cross-Platform Testing Guide**: Platform-specific setup help
- **Contributing Guide**: How to contribute to the project
- **Technical Documentation**: Architecture, requirements, and implementation guides
- **In-App Help**: Context-sensitive tooltips throughout the UI

### 🔧 Technical Stack
- **.NET 8.0**: Modern cross-platform framework
- **Avalonia UI**: Beautiful cross-platform XAML UI
- **MVVM Architecture**: Clean separation of concerns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Testing**: xUnit, Moq, FluentAssertions
- **JSON Data**: Flexible model database format
- **Portable Executable**: No installation required

### 🐛 Fixed

#### Hardware Detection
- Fixed storage detection not properly identifying NVMe drives
- Fixed multi-drive detection counting only primary drive
- Fixed GPU VRAM detection edge cases on certain Linux distributions
- Fixed CPU detection showing incorrect core counts on some AMD processors
- Fixed framework detection false positives for DirectML on Linux

#### UI/UX
- Fixed score cards not updating after hardware refresh
- Fixed model browser scrolling performance with large datasets
- Fixed tooltip positioning on edge of screen
- Fixed responsive layout issues on small screens
- Fixed community recommendations not filtering correctly by VRAM

#### Performance
- Optimized hardware detection to complete in under 2 seconds
- Improved model database loading performance
- Reduced memory footprint for large model lists
- Fixed UI freezing during initial hardware scan

### 🔒 Security
- No external API calls except model database download (GitHub)
- No user data collection or telemetry
- Local-only data storage
- No third-party analytics or tracking
- Open source for full transparency

### 📦 Distribution
- Source code available for all platforms
- Build instructions for Windows, Linux, and macOS
- Portable executable (no installation required)
- Pre-built binaries (coming soon)

---

## Version History

- **v1.0.0** (Unreleased) - Initial public release

---

## How to Update

### From Source
```bash
git pull origin master
dotnet build --configuration Release
```

### Pre-built Binaries
Download the latest version from [Releases](https://github.com/yourusername/llm-capability-checker/releases/latest)

---

## Migration Guides

### Upgrading to v1.0.0
This is the first release - no migration needed!

---

## Deprecated Features

None - this is the first release.

---

## Breaking Changes

None - this is the first release.

---

## Contributors

Thank you to everyone who contributed to this release! 🎉

See [CONTRIBUTORS.md](CONTRIBUTORS.md) for the full list of contributors.

---

## Links

- **Repository**: https://github.com/yourusername/llm-capability-checker
- **Issues**: https://github.com/yourusername/llm-capability-checker/issues
- **Discussions**: https://github.com/yourusername/llm-capability-checker/discussions
- **Documentation**: https://github.com/yourusername/llm-capability-checker/tree/master/docs

---

**Note**: This changelog follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format.
- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for now removed features
- `Fixed` for bug fixes
- `Security` for vulnerability fixes
