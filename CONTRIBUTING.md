# Contributing to LLM Capability Checker

Thank you for your interest in contributing to LLM Capability Checker! We welcome contributions from the community.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Contributing Code](#contributing-code)
  - [Improving Documentation](#improving-documentation)
  - [Adding Models to Database](#adding-models-to-database)
- [Development Setup](#development-setup)
- [Code Standards](#code-standards)
- [Pull Request Process](#pull-request-process)
- [Testing](#testing)
- [Community](#community)

---

## Code of Conduct

This project follows a simple code of conduct:

- **Be respectful**: Treat all contributors with respect
- **Be inclusive**: Welcome developers of all skill levels
- **Be constructive**: Provide helpful feedback
- **Be collaborative**: Work together toward shared goals

Harassment, discrimination, or toxic behavior will not be tolerated.

---

## How Can I Contribute?

### Reporting Bugs

**Before Submitting**:
1. Check [existing issues](https://github.com/yourusername/llm-capability-checker/issues) to avoid duplicates
2. Update to the latest version - your bug may already be fixed
3. Enable detailed logging (Settings → Detailed Logging) to gather debug info
4. Export your system report (Dashboard → Export Report)

**When Submitting a Bug Report**:

Use the [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.md) and include:

- **Clear title**: Describe the issue concisely
- **Description**: Detailed explanation of the problem
- **Steps to reproduce**: How to trigger the bug
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Environment**:
  - OS and version (Windows 10/11, Ubuntu 22.04, macOS 13.2, etc.)
  - App version (from About screen)
  - Hardware specs (from exported report)
- **Logs**: Detailed logs if available
- **Screenshots**: Visual evidence if applicable

**Example Bug Report**:

```markdown
### Bug: GPU Not Detected on Ubuntu 22.04 with NVIDIA RTX 4070

**Description**: The app shows "No dedicated GPU" despite having an RTX 4070 installed and working in other applications.

**Steps to Reproduce**:
1. Launch app on Ubuntu 22.04
2. Navigate to Dashboard
3. Observe GPU card shows "No dedicated GPU"

**Expected**: Should detect RTX 4070 with 12GB VRAM

**Actual**: Shows "No dedicated GPU"

**Environment**:
- OS: Ubuntu 22.04 LTS
- App Version: 1.0.0
- CPU: Ryzen 7 5800X
- GPU: NVIDIA RTX 4070 12GB (nvidia-smi works correctly)
- Drivers: nvidia-driver-535

**Logs**: [Attach log file]

**Possible Cause**: App may need elevated permissions to access lspci
```

### Suggesting Features

**Before Suggesting**:
1. Check [existing feature requests](https://github.com/yourusername/llm-capability-checker/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
2. Check the [Roadmap](README.md#roadmap) - it might already be planned
3. Consider if the feature fits the project's scope (local LLM capability assessment)

**When Suggesting a Feature**:

Use the [Feature Request template](.github/ISSUE_TEMPLATE/feature_request.md) and include:

- **Clear title**: Describe the feature concisely
- **Problem**: What problem does this solve?
- **Solution**: Detailed description of proposed feature
- **Alternatives**: Other solutions you've considered
- **Use case**: Real-world scenarios where this helps
- **Priority**: How important is this? (nice-to-have vs critical)

**Example Feature Request**:

```markdown
### Feature: Multi-GPU Support

**Problem**: Users with multiple GPUs can't see combined VRAM or pick which GPU to use for LLMs.

**Solution**:
- Detect all installed GPUs
- Show total VRAM if multiple GPUs of same type
- Add dropdown to select which GPU to evaluate
- Calculate scores based on selected GPU

**Alternatives Considered**:
- Always use most powerful GPU (but users might want to use specific GPU)
- Show average of all GPUs (but misleading if GPUs differ)

**Use Case**:
Users with RTX 3090 + RTX 4060 want to know if they can use the 3090's 24GB VRAM for larger models.

**Priority**: Medium - affects enthusiasts with multiple GPUs
```

### Contributing Code

We welcome code contributions! Here's how to get started:

1. **Check for existing issues**: Look for issues labeled `good first issue` or `help wanted`
2. **Comment on the issue**: Let others know you're working on it
3. **Fork the repository**: Create your own copy
4. **Create a branch**: Use descriptive branch names (e.g., `feature/multi-gpu-support` or `bugfix/gpu-detection`)
5. **Make your changes**: Follow code standards (see below)
6. **Test thoroughly**: Run all tests and manually verify
7. **Submit a pull request**: Use the PR template

**Good First Issues**:
- UI/UX improvements
- Adding tooltips or help text
- Fixing typos in documentation
- Adding models to database
- Writing unit tests for existing code

**Areas Needing Help**:
- Cross-platform testing (Linux, macOS)
- Hardware detection edge cases
- Performance optimization
- Accessibility improvements
- Internationalization (i18n)

### Improving Documentation

Documentation is crucial! Ways to help:

**User Documentation** (`docs/`):
- Improve User Guide with more examples
- Add troubleshooting steps to FAQ
- Create platform-specific guides
- Add screenshots and diagrams
- Translate to other languages

**Developer Documentation**:
- Improve code comments
- Document architecture decisions
- Create API documentation
- Write integration guides
- Add design pattern explanations

**Process**:
1. Find documentation to improve
2. Make changes in your fork
3. Submit PR with clear description of improvements

### Adding Models to Database

Help expand model compatibility by adding new models!

**Requirements**:
- Model must be publicly available
- Must have clear hardware requirements
- Must be runnable locally (not cloud-only)

**Process**:
1. Edit `data/models.json`
2. Add model entry with required fields:
   ```json
   {
     "name": "Model Name",
     "provider": "Provider/Organization",
     "parameterSize": "7B",
     "description": "Brief description",
     "minRamGB": 16,
     "minVramGB": 8,
     "recommendedRamGB": 32,
     "recommendedVramGB": 12,
     "quantizationSupport": ["FP16", "8-bit", "4-bit"],
     "frameworks": ["CUDA", "ROCm", "Metal"],
     "sourceUrl": "https://huggingface.co/...",
     "tags": ["chat", "coding", "reasoning"]
   }
   ```
3. Test that model appears in recommendations
4. Submit PR with title "Add [Model Name] to database"

See [data/README.md](data/README.md) for detailed schema documentation.

---

## Development Setup

### Prerequisites

- **.NET 8.0 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **IDE** (choose one):
  - Visual Studio 2022 (Windows/Mac)
  - Visual Studio Code with C# extension
  - JetBrains Rider
- **Git**: Version control

### Clone and Build

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/llm-capability-checker.git
cd llm-capability-checker

# Add upstream remote
git remote add upstream https://github.com/yourusername/llm-capability-checker.git

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/LLMCapabilityChecker/LLMCapabilityChecker.csproj
```

### Project Structure

```
llm-capability-checker/
├── src/
│   └── LLMCapabilityChecker/
│       ├── Models/           # Data models
│       ├── ViewModels/       # MVVM ViewModels
│       ├── Views/            # UI (Avalonia XAML)
│       ├── Services/         # Business logic
│       ├── Helpers/          # Utility classes
│       └── Program.cs        # Entry point
├── tests/
│   └── LLMCapabilityChecker.Tests/  # Unit tests
├── data/
│   └── models.json          # Model database
└── docs/                    # Documentation
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~ScoringServiceTests.CalculateScores_Should_ReturnValidScores"

# Run tests verbosely
dotnet test --logger "console;verbosity=detailed"
```

### Debugging

**Visual Studio**:
- Set `LLMCapabilityChecker` as startup project
- Press F5 to debug

**VS Code**:
- Open Command Palette (Ctrl+Shift+P)
- Select ".NET: Generate Assets for Build and Debug"
- Press F5 to debug

**Rider**:
- Right-click `LLMCapabilityChecker` project
- Select "Debug"

---

## Code Standards

### General Guidelines

- **Follow existing style**: Match the coding style of the file you're editing
- **Be consistent**: Use same patterns and conventions throughout
- **Keep it simple**: Prefer clarity over cleverness
- **Comment wisely**: Explain "why", not "what"
- **Test your code**: Write tests for new features

### C# Conventions

```csharp
// Use PascalCase for public members
public class HardwareDetectionService : IHardwareDetectionService
{
    // Use PascalCase for properties
    public HardwareInfo? Hardware { get; set; }

    // Use camelCase for private fields with underscore prefix
    private readonly ILogger<HardwareDetectionService> _logger;

    // Use camelCase for parameters and locals
    public async Task<HardwareInfo> DetectHardwareAsync()
    {
        var cpuInfo = await DetectCpuAsync();
        return new HardwareInfo { Cpu = cpuInfo };
    }

    // Use meaningful names
    private async Task<CpuInfo> DetectCpuAsync()
    {
        // Good: Clear and descriptive
        var cpuModel = await GetCpuModelAsync();
        var coreCount = await GetCoreCountAsync();

        // Bad: Unclear abbreviations
        // var m = await GetModelAsync();
        // var c = await GetCountAsync();

        return new CpuInfo { Model = cpuModel, Cores = coreCount };
    }
}
```

### XAML Conventions

```xml
<!-- Use clear naming and consistent spacing -->
<UserControl xmlns="https://github.com/avaloniaui"
             x:Class="LLMCapabilityChecker.Views.DashboardView"
             Background="#000000">

    <!-- Add comments for major sections -->
    <!-- Overall Score Card -->
    <Border Background="#E3F2FD"
            CornerRadius="12"
            Padding="32"
            ToolTip.Tip="Your system's overall capability score">

        <!-- Follow UI_STYLE_GUIDE.md for colors and sizing -->
        <TextBlock Text="{Binding OverallScore}"
                   FontSize="56"
                   FontWeight="Bold"
                   Foreground="#1976D2"/>
    </Border>
</UserControl>
```

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add multi-GPU support for VRAM detection
fix: correct GPU detection on Ubuntu 22.04
docs: improve hardware detection troubleshooting guide
refactor: simplify scoring algorithm for better performance
test: add unit tests for ModelDatabaseService
chore: update dependencies to .NET 8.0.5
```

**Format**:
```
<type>: <description>

[optional body]

[optional footer]
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks (dependencies, build config)
- `perf`: Performance improvements

---

## Pull Request Process

### Before Submitting

1. **Update from upstream**: Sync your fork with the latest changes
   ```bash
   git fetch upstream
   git checkout master
   git merge upstream/master
   ```

2. **Create feature branch**: Don't work directly on master
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

3. **Make your changes**: Implement your feature or fix

4. **Run tests**: Ensure all tests pass
   ```bash
   dotnet test
   ```

5. **Update documentation**: If behavior changes, update docs

6. **Commit your changes**: Follow commit message conventions
   ```bash
   git add .
   git commit -m "feat: add awesome new feature"
   ```

### Submitting the PR

1. **Push to your fork**:
   ```bash
   git push origin feature/my-awesome-feature
   ```

2. **Create Pull Request**: Go to GitHub and click "New Pull Request"

3. **Fill out PR template**: Provide clear description

4. **Link issues**: Reference related issues (e.g., "Fixes #123")

5. **Request review**: Tag relevant maintainers if needed

### PR Template

```markdown
## Description
Clear description of what this PR does.

## Related Issue
Fixes #123

## Type of Change
- [ ] Bug fix (non-breaking change)
- [ ] New feature (non-breaking change)
- [ ] Breaking change (fix or feature that would cause existing functionality to change)
- [ ] Documentation update

## Testing
- [ ] All tests pass
- [ ] Added new tests for new functionality
- [ ] Manually tested on [OS/Platform]

## Screenshots (if applicable)
[Add screenshots of UI changes]

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review of code completed
- [ ] Commented code where necessary
- [ ] Updated documentation
- [ ] No new warnings generated
- [ ] Tests added/updated
```

### Review Process

1. **Automated checks**: CI/CD runs tests automatically
2. **Code review**: Maintainers review your code
3. **Feedback**: Address any requested changes
4. **Approval**: Once approved, PR will be merged
5. **Merge**: Maintainer will merge your PR

**Response Times**:
- Initial review: Within 7 days
- Feedback response: We'll try to respond within 3 days
- Merge: Once approved and checks pass

---

## Testing

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter ClassName=HardwareDetectionServiceTests

# Specific test method
dotnet test --filter Name~DetectHardware

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Writing Tests

Use xUnit, Moq, and FluentAssertions:

```csharp
using Xunit;
using Moq;
using FluentAssertions;

public class ScoringServiceTests
{
    [Fact]
    public async Task CalculateScores_Should_ReturnValidScores()
    {
        // Arrange
        var mockHardwareService = new Mock<IHardwareDetectionService>();
        mockHardwareService
            .Setup(x => x.DetectHardwareAsync())
            .ReturnsAsync(new HardwareInfo { /* ... */ });

        var scoringService = new ScoringService(mockHardwareService.Object);

        // Act
        var scores = await scoringService.CalculateScoresAsync(hardwareInfo);

        // Assert
        scores.Should().NotBeNull();
        scores.OverallScore.Should().BeInRange(0, 100);
        scores.Breakdown.CpuScore.Should().BeGreaterThan(0);
    }
}
```

### Test Coverage

**Minimum Requirements**:
- Unit test coverage: 70%+
- Critical paths: 90%+
- New features: 80%+ coverage required

**What to Test**:
- ✅ Business logic in Services
- ✅ Data models and calculations
- ✅ Utility functions
- ✅ Edge cases and error handling
- ❌ UI ViewModels (hard to test, lower priority)
- ❌ Simple getters/setters
- ❌ Third-party library wrappers

---

## Community

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and ideas
- **Pull Requests**: Code contributions
- **Reddit r/LocalLLaMA**: Community discussions

### Getting Help

- **New to open source?** Check out [First Timers Only](https://www.firsttimersonly.com/)
- **Questions?** Open a [Discussion](https://github.com/yourusername/llm-capability-checker/discussions)
- **Stuck?** Ask in the issue you're working on

### Recognition

Contributors will be:
- Listed in [CONTRIBUTORS.md](CONTRIBUTORS.md)
- Mentioned in release notes
- Credited in the About screen (for significant contributions)

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

## Thank You!

Thank you for contributing to LLM Capability Checker! Every contribution, no matter how small, helps make this tool better for everyone.

**Questions?** Open a [Discussion](https://github.com/yourusername/llm-capability-checker/discussions) or ask in the issue you're working on.

**Ready to contribute?** Check out [good first issues](https://github.com/yourusername/llm-capability-checker/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)!
