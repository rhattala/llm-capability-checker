# Quick Reference Checklist

> **Fast lookup guide for developers/AI implementing LLM Capability Checker**

## 🎯 MVP Scope Summary

### Must-Have Features (P0)
- [x] Hardware detection (CPU, GPU, RAM, VRAM, Storage)
- [x] Framework detection (CUDA, ROCm, DirectML, etc.)
- [x] Three scoring systems (Inference, Training, Fine-tuning)
- [x] Model database (50+ models, auto-update from GitHub)
- [x] Model compatibility checker
- [x] Performance estimates (tokens/sec)
- [x] Model recommendations
- [x] Bottleneck identification
- [x] Upgrade suggestions with cost estimates
- [x] Beginner/Advanced modes
- [x] Light/Dark themes
- [x] Educational tooltips and explanations

### Nice-to-Have (P1-P2)
- [ ] Optional real benchmarking (llamafile integration)
- [ ] Detailed educational content
- [ ] Historical tracking
- [ ] Export functionality

## 📐 Key Technical Specs

### Performance Targets
- **Startup**: < 3 seconds
- **Hardware Detection**: < 2 seconds
- **Score Calculation**: < 500ms
- **Memory Usage**: < 150MB
- **UI**: 60 FPS

### Scoring Formulas

**Inference Score** (0-1000):
```
= (GPU * 0.5) + (CPU * 0.2) + (RAM * 0.2) + (Storage * 0.1)
```

**Training Score** (0-1000):
```
= (GPU * 0.7) + (RAM * 0.2) + (Storage * 0.1)
```

**Fine-tuning Score** (0-1000):
```
= (GPU * 0.6) + (RAM * 0.25) + (Storage * 0.15)
```

**GPU Score Factors**:
- VRAM (primary): 8GB = 500, 24GB = 1000
- Compute units (secondary)
- Architecture (bonus for modern)
- Tensor cores (bonus for training)

**CPU Score Factors**:
- Core count: 8+ = 500, 16+ = 1000
- Frequency: Bonus above 2GHz
- Architecture: Bonus for modern (Zen3+, 11th gen Intel+)

**RAM Score**:
- 16GB = 500, 32GB = 800, 64GB+ = 1000

**Storage Score**:
- NVMe = 1000, SSD = 700, HDD = 300

### Score Ratings
- **900-1000**: Excellent
- **700-899**: Very Good
- **500-699**: Good
- **300-499**: Fair
- **100-299**: Limited
- **0-99**: Poor

## 🗂️ File Structure

```
src/LLMCapabilityChecker/
├── Models/
│   ├── HardwareInfo.cs          # Hardware data models
│   ├── SystemScores.cs          # Score models
│   └── ModelInfo.cs             # LLM model data
├── ViewModels/
│   ├── ViewModelBase.cs         # Base MVVM class
│   ├── MainWindowViewModel.cs   # Main window VM
│   ├── DashboardViewModel.cs    # Dashboard VM
│   └── ...                      # Other VMs
├── Views/
│   ├── MainWindow.axaml         # Main window UI
│   ├── DashboardView.axaml      # Dashboard UI
│   └── ...                      # Other views
├── Services/
│   ├── IHardwareDetectionService.cs
│   ├── HardwareDetectionService.cs
│   ├── IScoringService.cs
│   ├── ScoringService.cs
│   └── ...                      # Other services
└── Helpers/
    ├── SystemInfoHelper.cs      # System utilities
    └── ...                      # Other helpers
```

## 🔧 NuGet Packages

**Essential**:
```
Avalonia (11.x)
Avalonia.Desktop
Avalonia.Themes.Fluent
Microsoft.Extensions.DependencyInjection
Microsoft.Extensions.Logging
System.Management (Windows only)
```

**Testing**:
```
xunit
Moq
FluentAssertions
```

## 🎨 UI Layout

### Main Views
1. **Dashboard** - Scores, quick stats, recommendations
2. **Hardware** - Detailed specs, tabbed (CPU/GPU/RAM/Storage/Frameworks)
3. **Models** - Filterable list, compatibility status
4. **Upgrade Advisor** - Bottlenecks, recommendations, before/after
5. **Benchmark** - Optional real performance tests
6. **Education** - Tutorials, glossary, help
7. **Settings** - Mode, theme, updates, data sharing

### Navigation
- Sidebar or top bar
- Clear section labels
- Back button where appropriate
- Keyboard shortcuts (Alt+1, Alt+2, etc.)

## 🔍 Detection Strategy

### Priority Order
1. **Windows**: WMI (Windows Management Instrumentation)
2. **NVIDIA**: `nvidia-smi` command
3. **AMD**: `rocm-smi` command
4. **Linux**: `/proc/cpuinfo`, `/proc/meminfo`, sysfs
5. **Fallback**: Basic .NET APIs

### Graceful Degradation
- Try each method with 2-second timeout
- If one fails, try next
- Return partial results
- Never throw to user

## 📊 Model Database

### Location
- **Remote**: `https://raw.githubusercontent.com/[org]/[repo]/main/models.json`
- **Local Cache**: `%AppData%/LLMCapabilityChecker/models.json`
- **Embedded**: Ship default in resources

### Update Strategy
1. Check remote on startup (5-second timeout)
2. Download if newer version available
3. Cache locally
4. Use embedded as fallback

### JSON Structure
```json
{
  "version": "1.0.0",
  "last_updated": "ISO-8601",
  "models": [{
    "id": "model-id",
    "name": "Display Name",
    "requirements": {
      "fp32": { "vram_gb": 32, "ram_gb": 40, ... },
      "fp16": { ... },
      "8bit": { ... },
      "4bit": { ... }
    },
    "training_requirements": { ... }
  }]
}
```

## 🚀 Implementation Phases

### Phase 1: Core (Week 1-2)
- [ ] Project setup
- [ ] Base models
- [ ] Hardware detection
- [ ] Scoring service
- [ ] Basic UI structure

### Phase 2: Features (Week 2-3)
- [ ] Model database service
- [ ] Recommendations
- [ ] Model list UI
- [ ] Upgrade advisor

### Phase 3: Polish (Week 3-4)
- [ ] Educational content
- [ ] Theme support
- [ ] Settings
- [ ] Optimization

### Phase 4: Optional (Week 4+)
- [ ] Benchmarking
- [ ] Historical tracking
- [ ] Export features

## ⚡ Optimization Checklist

### Startup
- [ ] Lazy load non-critical components
- [ ] Async hardware detection
- [ ] Cache detection results
- [ ] Minimal initial UI

### Runtime
- [ ] Virtual scrolling for lists
- [ ] Debounce search/filter (300ms)
- [ ] Cache calculated values
- [ ] Dispose resources properly

### Memory
- [ ] No memory leaks
- [ ] Clear image caches
- [ ] Stream large files
- [ ] Use Span<T> for strings

## 🧪 Testing Checklist

### Unit Tests
- [ ] Hardware detection methods
- [ ] Scoring algorithms
- [ ] Model filtering
- [ ] Upgrade calculations

### Integration Tests
- [ ] Full detection flow
- [ ] Model DB loading
- [ ] Recommendation pipeline

### Manual Tests
- [ ] Windows 10/11
- [ ] Linux (if cross-platform)
- [ ] Various GPU brands (NVIDIA, AMD, Intel)
- [ ] CPU-only systems
- [ ] Low-end hardware (8GB RAM)
- [ ] High-end hardware (128GB RAM)
- [ ] Offline scenario
- [ ] Theme switching

## 🎯 Code Quality Standards

### Naming
- **Classes**: `PascalCase`
- **Interfaces**: `IPascalCase`
- **Methods**: `PascalCase`
- **Properties**: `PascalCase`
- **Fields**: `_camelCase`
- **Constants**: `UPPER_SNAKE_CASE`

### Best Practices
- [ ] XML comments on public APIs
- [ ] Async/await for I/O
- [ ] Using statements for IDisposable
- [ ] Null checks
- [ ] Error handling (try-catch)
- [ ] Logging (ILogger)
- [ ] Short methods (< 50 lines)
- [ ] Single responsibility

## 📝 Documentation Checklist

### Code
- [ ] XML comments on all public members
- [ ] Explain WHY not WHAT
- [ ] Link to external resources
- [ ] Provide examples

### User
- [ ] README with quick start
- [ ] User guide with screenshots
- [ ] FAQ document
- [ ] Troubleshooting guide

### Developer
- [ ] Architecture document
- [ ] API documentation
- [ ] Contributing guide
- [ ] Build instructions

## 🔒 Security Checklist

- [ ] Validate all file paths
- [ ] Sanitize command output
- [ ] Validate JSON schema
- [ ] No arbitrary code execution
- [ ] Pin dependency versions
- [ ] No PII in logs

## 🎨 UI/UX Checklist

### General
- [ ] Responsive (60 FPS)
- [ ] No blocking operations
- [ ] Loading indicators
- [ ] Clear error messages
- [ ] Keyboard navigation
- [ ] Screen reader friendly

### Beginner Mode
- [ ] Simple language
- [ ] More explanations
- [ ] Tooltips everywhere
- [ ] Guided workflows

### Advanced Mode
- [ ] Technical terms
- [ ] Full specs visible
- [ ] Raw data access
- [ ] Minimal hand-holding

## 🎁 Distribution Checklist

- [ ] Release build optimized
- [ ] Dependencies included
- [ ] Portable (no installer)
- [ ] Checksum generated
- [ ] README in package
- [ ] LICENSE in package
- [ ] Version number correct

## 🐛 Common Pitfalls to Avoid

❌ **Don't**:
- Hardcode file paths
- Block UI thread
- Throw exceptions to users
- Forget to dispose resources
- Use complex LINQ in hot paths
- Make synchronous I/O calls
- Ignore cancellation tokens
- Skip error handling

✅ **Do**:
- Use dependency injection
- Async everything
- Cache aggressively
- Log errors
- Validate inputs
- Test edge cases
- Document decisions
- Keep it simple

## 🔗 Quick Links

- [Project Overview](PROJECT_OVERVIEW.md)
- [Technical Requirements](TECHNICAL_REQUIREMENTS.md)
- [Architecture](ARCHITECTURE.md)
- [User Stories](USER_STORIES.md)
- [AI Implementation Guide](AI_IMPLEMENTATION_GUIDE.md)
- [Model Database](data/models.json)

## 📞 Getting Help

1. Check this document
2. Read the relevant documentation
3. Search existing issues
4. Ask in discussions
5. Create new issue

---

**Last Updated**: 2025-01-15
**Version**: 1.0.0
