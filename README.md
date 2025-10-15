# LLM Capability Checker

> **Assess your hardware's ability to run, train, and fine-tune Large Language Models locally**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://github.com/yourusername/llm-capability-checker)

## 🎯 What is this?

LLM Capability Checker is a **free, open-source desktop application** that helps you:

- ✅ **Understand** if your PC can run specific LLM models locally
- ✅ **Estimate** performance (tokens/sec) for different models
- ✅ **Identify** hardware bottlenecks holding you back
- ✅ **Get recommendations** for the best models for your hardware
- ✅ **Plan upgrades** with cost-effective suggestions
- ✅ **Learn** about LLM requirements through educational content

### Why is this useful?

Before downloading a 50GB model and spending hours setting up tools, wouldn't you like to know if it will even run well on your system? This app gives you that answer in under 2 minutes.

## 🌟 Features

### Core Capabilities

**🔍 Hardware Detection**
- Automatic detection of CPU, GPU, RAM, VRAM, Storage
- Framework compatibility check (CUDA, ROCm, DirectML, oneAPI, WSL2)
- Works offline after initial setup

**📊 Smart Scoring System**
- Three scores: Inference, Training, Fine-tuning (0-1000 scale)
- Clear ratings: Excellent, Good, Fair, Limited, Poor
- Detailed breakdown showing what contributes to each score

**🤖 Model Recommendations**
- 50+ popular models in database (Llama, Mistral, Gemma, Phi, DeepSeek, etc.)
- Shows which models work on your hardware
- Performance estimates (tokens/sec)
- Supports all quantization levels (FP32, FP16, 8-bit, 4-bit)

**📈 Upgrade Advisor**
- Identifies your biggest bottlenecks
- Specific hardware recommendations (GPUs, RAM, Storage)
- Cost-benefit analysis (Budget, Mid-range, High-end)
- "Before and After" comparisons
- Shows which models each upgrade unlocks

**🎓 Educational Mode**
- Beginner-friendly explanations
- Glossary of technical terms
- Interactive tutorials
- "Why does this matter?" tooltips

**⚡ Optional Benchmarking**
- Quick test (~30 seconds)
- Comprehensive test (~5 minutes)
- Compares real performance vs estimates

### Two Modes

**👶 Beginner Mode**
- Simplified language
- More explanations
- Guided workflows
- Perfect for newcomers to local LLMs

**🧑‍💻 Advanced Mode**
- Technical details
- Raw specifications
- Full control
- For experienced users

## 🚀 Quick Start

### Prerequisites

- **Operating System**: Windows 10/11 (64-bit), Linux (x64), or macOS (x64/ARM64)
- **RAM**: 100MB free
- **Storage**: 50MB
- **Display**: 1024x768 minimum resolution

### Installation

**Option 1: Portable (Recommended)**

1. Download the latest release from [Releases](https://github.com/yourusername/llm-capability-checker/releases)
2. Extract the ZIP file
3. Run `LLMCapabilityChecker.exe` (Windows) or `./LLMCapabilityChecker` (Linux/macOS)

**Option 2: Build from Source**

```bash
# Clone the repository
git clone https://github.com/yourusername/llm-capability-checker.git
cd llm-capability-checker

# Build
dotnet build src/LLMCapabilityChecker/LLMCapabilityChecker.csproj --configuration Release

# Run
dotnet run --project src/LLMCapabilityChecker/LLMCapabilityChecker.csproj
```

### First Run

1. **Launch** the application
2. **Wait** ~2 seconds for hardware detection
3. **View** your scores on the dashboard
4. **Explore** model recommendations
5. **Check** upgrade suggestions (optional)

That's it! No configuration needed.

## 📖 Documentation

### For Users

- **[User Guide](docs/USER_GUIDE.md)** - How to use the app *(Coming soon)*
- **[FAQ](docs/FAQ.md)** - Common questions *(Coming soon)*
- **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Solve common issues *(Coming soon)*

### For Developers

- **[Project Overview](PROJECT_OVERVIEW.md)** - High-level goals and scope
- **[Technical Requirements](TECHNICAL_REQUIREMENTS.md)** - Detailed specifications
- **[Architecture](ARCHITECTURE.md)** - System design and patterns
- **[User Stories](USER_STORIES.md)** - Feature descriptions and acceptance criteria
- **[AI Implementation Guide](AI_IMPLEMENTATION_GUIDE.md)** - Step-by-step build instructions
- **[Contributing](CONTRIBUTING.md)** - How to contribute *(Coming soon)*

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

- 🐛 **Reporting bugs** - Open an issue
- 💡 **Suggesting features** - Open an issue with "[Feature Request]"
- 📝 **Improving docs** - Submit a PR
- 🔧 **Writing code** - Check open issues labeled "good first issue"
- 🤖 **Adding models** - Submit PRs to `data/models.json`

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines *(Coming soon)*.

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

## 🙏 Acknowledgments

- **Avalonia Team** - Amazing cross-platform UI framework
- **Mozilla Builders** - Inspiration from LocalScore project
- **LLM Community** - Model information and benchmarks
- **Contributors** - Everyone who helps improve this project

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/llm-capability-checker/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/llm-capability-checker/discussions)
- **Email**: your.email@example.com *(if you want to provide one)*

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
