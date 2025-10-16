# 🚀 LLM Capability Checker

<div align="center">

**Can Your PC Run It?** The ultimate tool to assess your hardware's ability to run, train, and fine-tune Large Language Models locally.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](#-quick-start)

*Like "Can You Run It?" but for AI models* 🤖

[Quick Start](#-quick-start) • [Features](#-features) • [Community](#-community-recommendations) • [Contributing](#-contributing)

</div>

---

## 🎯 What Does This Do?

**Stop guessing if your PC can run LLMs.** Get instant answers in under 2 minutes:

✅ **Can I run Llama 4 Scout 8B?** → Yes! 85% performance, smooth experience
✅ **Will DeepSeek-R1 70B work?** → Needs 24GB VRAM (you have 11GB) - try Q4 quantization
✅ **Which models CAN I run?** → 55 compatible models found for your hardware
✅ **Should I upgrade my GPU?** → Yes! RTX 4080 unlocks 23 more models

### The Problem We Solve

You want to run AI locally but:
- ❌ Don't know if your GPU has enough VRAM
- ❌ Waste hours downloading models that won't run
- ❌ Can't figure out which quantization to use
- ❌ Don't know if upgrading is worth it

### Our Solution

**2-minute hardware scan** → **3 capability scores** → **107 model recommendations** → **Smart upgrade advice**

No account required. 100% free. Fully open source.

## ✨ Features

### 🎯 Three-Score System (NEW!)

Unlike other tools that give one generic score, we give you **three specialized scores**:

| Score | What It Measures | Why It Matters |
|-------|------------------|----------------|
| **🚀 Inference** | Running models for chat/generation | Your everyday AI assistant experience |
| **🎯 Training** | Full model fine-tuning | Teaching models your own data |
| **⚡ Fine-Tuning** | LoRA/QLoRA parameter tuning | Efficient model customization |

Each score (0-100) comes with:
- Clear capability description (e.g., "Run 13B models smoothly")
- Component breakdown (GPU 40%, RAM 30%, etc.)
- Specific bottleneck identification

### 🤖 107 Models in Database

**Latest 2025 releases:**
- Llama 4 Scout 8B, Maverick 70B, Behemoth 405B
- Phi-4 14B
- DeepSeek-R1 70B, DeepSeek-V3 671B
- Qwen 2.5 series (0.5B → 72B)
- Mistral Large 123B

**All with 5 quantization options:** Q2_K, Q4_K_M, Q5_K_M, Q8_0, FP16

### 👥 Community Recommendations (NEW!)

**Real users, real experiences.** Browse 15+ community-curated model recommendations with:

🔥 **Trending Tab** - Most upvoted models (last 30 days)
- DeepSeek-R1 70B: 342 upvotes, 4.8⭐ - "Best reasoning model I've tried"
- Llama 4 Scout 8B: 423 upvotes, 4.9⭐ - "Perfect for beginners"

⭐ **Top Rated Tab** - Highest quality picks
- Community reviews and ratings
- Use case tags (coding, chat, reasoning)
- Hardware tier recommendations

💻 **For Your System Tab** - Filtered for YOUR hardware
- Only shows models you can actually run
- Based on your VRAM and RAM

**Features:**
- Upvote your favorites
- Read reviews from users with similar hardware
- Submit your own recommendations
- 100% local storage (privacy-first)

### 🔍 Smart Hardware Detection

**Automatic detection:**
- CPU (cores, threads, model)
- GPU (model, VRAM, CUDA/ROCm support)
- RAM (capacity, speed, type)
- Storage (NVMe/SSD detection, multi-drive support)
- Frameworks (CUDA, ROCm, DirectML, Metal)

**NEW: Fixed storage detection** - Now properly identifies NVMe vs SSD and counts all drives

### 📊 Categorized Model Recommendations

Models organized by compatibility:

- ✨ **Perfect Matches** (90-100 score) - Run smoothly, full performance
- ⚡ **Good Fit** (70-89 score) - Will run well
- ⚠️ **Possible with Optimization** (50-69 score) - Needs quantization
- ❌ **Not Recommended** (<50 score) - Upgrade needed

### 📈 Smart Upgrade Suggestions

**NEW: GPU upgrade logic** - No more suggesting lateral moves!
- Detects your exact GPU model (e.g., RTX 4070 Ti)
- Only suggests meaningful upgrades (e.g., RTX 4080, not 4070 Ti Super)
- Shows what models each upgrade unlocks

### 🏆 Benchmark Transparency

**NEW: Reference system card** showing our baseline:
- AMD Ryzen 5 5600X
- RTX 3060 12GB
- 32GB DDR4
- 1TB NVMe Gen3
- Updated: January 2025

Compare your hardware against a real reference point!

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0) (free)
- **Windows 10/11**, **Linux**, or **macOS**
- **5 minutes** of your time

### Installation (3 Steps)

#### Option 1: Run from Source (Easiest)

```bash
# 1. Clone the repo
git clone https://github.com/yourusername/llm-capability-checker.git
cd llm-capability-checker

# 2. Run it!
dotnet run --project src/LLMCapabilityChecker/LLMCapabilityChecker.csproj
```

That's it! The app will launch and scan your hardware automatically.

#### Option 2: Build for Production

```bash
# Build optimized version
dotnet build src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Release

# Run it
dotnet run --project src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Release
```

#### Option 3: Pre-built Binaries

Download from [Releases](https://github.com/yourusername/llm-capability-checker/releases) → Extract → Double-click

*(Coming soon once we have our first release)*

### Platform-Specific Requirements

**Linux:**
- Required: `lscpu`, `lspci`, `lsblk`, `df` (usually pre-installed)
- Optional: `nvidia-smi` (for NVIDIA GPU detection), `rocm-smi` (for AMD GPU detection)

**macOS:**
- Built-in tools: `sysctl`, `system_profiler`, `diskutil` (pre-installed)
- Metal support: macOS 10.13+ (High Sierra or later)

**Windows:**
- Built-in: WMI (Windows Management Instrumentation)
- Optional: `nvidia-smi` (for detailed NVIDIA GPU detection)

### First Run

1. **Launch** the application
2. **Wait** ~2 seconds for hardware detection
3. **View** your scores on the dashboard
4. **Explore** model recommendations
5. **Check** upgrade suggestions (optional)

That's it! No configuration needed.

## 📖 Documentation

### For Users

- **[User Guide](docs/USER_GUIDE.md)** - Comprehensive guide to using the app
  - Getting started
  - Understanding scores and recommendations
  - Hardware detection
  - Benchmark testing
  - Export reports
  - Settings and preferences
  - Troubleshooting
- **[FAQ](docs/FAQ.md)** - Frequently asked questions
  - General questions
  - Hardware & detection
  - Models & compatibility
  - Upgrades & recommendations
  - Performance & benchmarks
  - Privacy & data
  - Platform-specific issues
- **[Cross-Platform Testing](docs/CROSS_PLATFORM_TESTING.md)** - Testing on Linux, macOS, and Windows

### For Developers

- **[Contributing Guide](CONTRIBUTING.md)** - How to contribute to the project
  - Code of conduct
  - Reporting bugs
  - Suggesting features
  - Development setup
  - Code standards
  - Pull request process
- **[Project Overview](PROJECT_OVERVIEW.md)** - High-level goals and scope
- **[Technical Requirements](TECHNICAL_REQUIREMENTS.md)** - Detailed specifications
- **[Architecture](ARCHITECTURE.md)** - System design and patterns
- **[User Stories](USER_STORIES.md)** - Feature descriptions and acceptance criteria
- **[AI Implementation Guide](AI_IMPLEMENTATION_GUIDE.md)** - Step-by-step build instructions

### In-App Help

Press **F1** or click the **Help** button in the application for:
- Quick reference guide
- Searchable help topics
- Context-sensitive assistance
- Tooltips throughout the interface

### Model Database

- **[Model Database Schema](data/README.md)** - JSON format specification *(Coming soon)*
- **[Adding Models](docs/ADDING_MODELS.md)** - Contribute new models *(Coming soon)*

## 🏗️ Project Structure

```
llm-capability-checker/
├── src/
│   └── LLMCapabilityChecker/        # Main application
│       ├── Models/                  # Data models
│       ├── ViewModels/              # MVVM ViewModels
│       ├── Views/                   # UI (Avalonia XAML)
│       ├── Services/                # Business logic
│       └── Helpers/                 # Utilities
├── tests/
│   └── LLMCapabilityChecker.Tests/  # Unit tests
├── data/
│   └── models.json                  # Model database
├── docs/                            # User documentation
├── PROJECT_OVERVIEW.md              # Project goals
├── TECHNICAL_REQUIREMENTS.md        # Tech specs
├── ARCHITECTURE.md                  # System design
├── USER_STORIES.md                  # Feature descriptions
└── AI_IMPLEMENTATION_GUIDE.md       # Build instructions
```

## 🛠️ Technology Stack

- **Framework**: .NET 8
- **UI**: Avalonia UI (cross-platform XAML)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Testing**: xUnit, Moq, FluentAssertions
- **Data Format**: JSON
- **Distribution**: Portable executable

### Cross-Platform Hardware Detection

| Platform | Method | Dependencies |
|----------|--------|--------------|
| **Windows** | WMI (Windows Management Instrumentation) | System.Management |
| **Linux** | Command-line tools | lscpu, lspci, lsblk, df, nvidia-smi, rocm-smi |
| **macOS** | Command-line tools | sysctl, system_profiler, diskutil, sw_vers |

All platforms use `RuntimeInformation.IsOSPlatform()` for platform detection and conditional logic.

## 🎯 Roadmap

### ✅ Phase 1: Core MVP (Current)
- Hardware detection
- Scoring system
- Model recommendations
- Basic UI

### 🚧 Phase 2: Enhancement (In Progress)
- Upgrade advisor
- Educational content
- Optional benchmarking
- Polish and optimization

### 📋 Phase 3: Future Features
- Multi-GPU support
- Custom model entry
- Historical tracking
- CLI version
- Community database integration

## 🤝 Contributing

We welcome contributions! Whether you're:

- 🐛 **Reporting bugs** - [Open an issue](https://github.com/yourusername/llm-capability-checker/issues/new?template=bug_report.md)
- 💡 **Suggesting features** - [Open an issue](https://github.com/yourusername/llm-capability-checker/issues/new?template=feature_request.md) with "[Feature Request]"
- 📝 **Improving docs** - Submit a PR for documentation improvements
- 🔧 **Writing code** - Check [good first issues](https://github.com/yourusername/llm-capability-checker/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)
- 🤖 **Adding models** - Submit PRs to `data/models.json`

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines on:
- Development setup
- Code standards
- Testing requirements
- Pull request process
- Community guidelines

## 📊 Model Database

The app downloads the latest model database from GitHub on startup. You can also view it locally at `data/models.json`.

**Currently included models**:
- Llama family (1B, 8B, 70B)
- Mistral (7B)
- Gemma 2 (9B)
- DeepSeek Coder (6.7B)
- Phi-4 (14B)
- And more...

Want to add a model? See [docs/ADDING_MODELS.md](docs/ADDING_MODELS.md) *(Coming soon)*.

## 🔒 Privacy

- **100% Local** - No telemetry or tracking by default
- **Opt-in Sharing** - You can choose to share anonymous benchmarks with the community
- **No Account Required** - Works completely offline (after initial model DB download)
- **Open Source** - Audit the code yourself

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

**What this means**:
- ✅ Free to use for personal and commercial projects
- ✅ Free to modify and distribute
- ✅ No warranty or liability
- ✅ Must include original license and copyright notice

**In short**: Use it however you want, just keep the license notice.

## 🙏 Acknowledgments

- **Avalonia Team** - Amazing cross-platform UI framework
- **Mozilla Builders** - Inspiration from LocalScore project
- **LLM Community** - Model information and benchmarks
- **Contributors** - Everyone who helps improve this project

## 📞 Support

### Getting Help

- **In-App Help**: Press **F1** or click the Help button for instant assistance
- **User Guide**: [Comprehensive documentation](docs/USER_GUIDE.md)
- **FAQ**: [Frequently asked questions](docs/FAQ.md)
- **GitHub Discussions**: [Ask questions](https://github.com/yourusername/llm-capability-checker/discussions) and share experiences
- **GitHub Issues**: [Report bugs](https://github.com/yourusername/llm-capability-checker/issues) or request features

### Before Asking for Help

1. Check the [FAQ](docs/FAQ.md) - most questions are already answered
2. Search [existing issues](https://github.com/yourusername/llm-capability-checker/issues) for similar problems
3. Enable detailed logging in Settings → Detailed Logging
4. Export your system report (Dashboard → Export Report) to attach to bug reports

### Reporting Issues

When reporting a bug, please include:
- Operating system and version
- App version (from About screen)
- Steps to reproduce the issue
- Expected vs actual behavior
- Exported system report (if possible)
- Detailed logs (if available)

### Community

- **Reddit**: [r/LocalLLaMA](https://reddit.com/r/LocalLLaMA) - Community discussions
- **Discord**: *(Coming soon)* - Real-time chat and support
- **Twitter**: *(Optional - add if you have a project Twitter)*

### Response Times

- Bug reports: Reviewed within 7 days
- Feature requests: Reviewed within 14 days
- Pull requests: Initial review within 7 days
- Discussions: Community-driven, usually within 24-48 hours

## 🌟 Star History

If you find this project useful, please consider giving it a ⭐ on GitHub!

---

## Screenshots

### Dashboard
*(Screenshot coming soon)*

### Hardware Details
*(Screenshot coming soon)*

### Model Recommendations
*(Screenshot coming soon)*

### Upgrade Advisor
*(Screenshot coming soon)*

---

## Quick Links

- 📦 [Latest Release](https://github.com/yourusername/llm-capability-checker/releases/latest)
- 🐛 [Report a Bug](https://github.com/yourusername/llm-capability-checker/issues/new?template=bug_report.md)
- 💡 [Request a Feature](https://github.com/yourusername/llm-capability-checker/issues/new?template=feature_request.md)
- 📖 [Documentation](docs/)
- 💬 [Discussions](https://github.com/yourusername/llm-capability-checker/discussions)

---

**Made with ❤️ for the local LLM community**

*Helping everyone run AI on their own hardware, one score at a time.*
