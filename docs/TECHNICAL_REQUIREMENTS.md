# Technical Requirements Document

## System Requirements

### Development Environment
- **.NET SDK**: 8.0 or higher
- **IDE**: Visual Studio 2022, Rider, or VS Code with C# extension
- **Target Framework**: .NET 8.0
- **UI Framework**: Avalonia UI 11.x
- **Minimum Windows**: Windows 10 version 1809 or higher

### Target Hardware (Minimum for App)
- **OS**: Windows 10/11 (64-bit), Linux (x64), macOS (x64/ARM64)
- **RAM**: 100MB free
- **Storage**: 50MB
- **Display**: 1024x768 minimum

## Architecture Requirements

### Design Pattern
- **MVVM** (Model-View-ViewModel) for clean separation
- **Dependency Injection** for testability
- **Repository Pattern** for data access
- **Strategy Pattern** for different detection methods

### Performance Requirements
- **Startup Time**: < 3 seconds (cold start)
- **Hardware Detection**: < 2 seconds
- **Score Calculation**: < 500ms
- **UI Responsiveness**: 60 FPS, no blocking operations
- **Memory Footprint**: < 150MB peak usage

### Code Quality Requirements
- **Minimal Dependencies**: Only essential NuGet packages
- **No Native Interop**: Unless absolutely necessary (system detection)
- **Async/Await**: All I/O operations must be asynchronous
- **Error Handling**: Comprehensive try-catch with fallbacks
- **Logging**: Structured logging for debugging (optional file output)

## Hardware Detection Requirements

### Must Detect
1. **CPU Information**
   - Model name and manufacturer
   - Core count (physical and logical)
   - Base and boost frequencies
   - Cache sizes (L1, L2, L3)
   - Architecture (x86, x64, ARM)

2. **GPU Information**
   - Model name and manufacturer
   - VRAM amount (dedicated)
   - CUDA cores / Stream processors / Compute Units
   - Compute capability / Architecture
   - Driver version

3. **System Memory**
   - Total RAM
   - Available RAM
   - Speed (MHz) if detectable
   - Type (DDR4, DDR5) if detectable

4. **Storage**
   - Type (SSD, NVMe, HDD)
   - Read/write speeds (basic detection)
   - Available space

5. **Framework Support**
   - CUDA toolkit version (if installed)
   - cuDNN version (if installed)
   - ROCm version (if installed)
   - DirectML availability (Windows)
   - oneAPI availability
   - WSL2 status (Windows)

### Detection Methods (Priority Order)
1. **Windows**: WMI (Windows Management Instrumentation)
2. **Cross-platform**: System.Management or AvaloniaUI system APIs
3. **Nvidia**: nvidia-smi command
4. **AMD**: rocm-smi command
5. **Fallback**: Registry, system files, known paths

### Detection Code Requirements
- **Graceful Degradation**: If one method fails, try next
- **Timeout**: Each detection method max 2 seconds
- **Caching**: Cache results, don't re-detect unnecessarily
- **Thread-Safe**: All detection must be thread-safe

## Scoring Algorithm Requirements

### Inference Score (0-1000)
**Formula Components**:
```
InferenceScore = (GPUScore * 0.5) + (CPUScore * 0.2) + (RAMScore * 0.2) + (StorageScore * 0.1)

GPUScore considerations:
- VRAM: Primary factor (8GB = 500, 24GB = 1000, linear scale)
- Compute: CUDA cores / Stream processors (normalized)
- Architecture: Newer = bonus (Ampere+, RDNA2+)

CPUScore considerations:
- Core count: 8+ cores = 500, 16+ = 1000
- Single-thread perf: Frequency as proxy
- Architecture: Modern (Zen3+, 11th gen Intel+) = bonus

RAMScore:
- 16GB = 500, 32GB = 800, 64GB+ = 1000

StorageScore:
- NVMe = 1000, SATA SSD = 700, HDD = 300
```

### Training Score (0-1000)
**Formula Components**:
```
TrainingScore = (GPUScore * 0.7) + (RAMScore * 0.2) + (StorageScore * 0.1)

More GPU-weighted than inference:
- VRAM: Critical (16GB minimum for good score)
- Tensor cores: Significant bonus
- CUDA compute capability: >= 8.0 preferred

RAMScore:
- 32GB = 500, 64GB = 800, 128GB+ = 1000

StorageScore:
- NVMe with high write speed = critical for checkpointing
```

### Fine-tuning Score (0-1000)
**Formula Components**:
```
FineTuningScore = (GPUScore * 0.6) + (RAMScore * 0.25) + (StorageScore * 0.15)

Balanced between training and inference:
- VRAM: 12GB minimum for decent score
- Can use quantization (QLoRA) = more forgiving

RAMScore:
- 24GB = 500, 48GB = 800, 64GB+ = 1000
```

### Score Rating System
- **900-1000**: Excellent - Can run cutting-edge models
- **700-899**: Very Good - Most models will work well
- **500-699**: Good - Many popular models supported
- **300-499**: Fair - Small to medium models only
- **100-299**: Limited - Very small models only
- **0-99**: Poor - Not recommended for local LLMs

## Model Database Requirements

### Data Source
- **Location**: GitHub repository (JSON file)
- **URL**: `https://raw.githubusercontent.com/[org]/[repo]/main/models.json`
- **Update Frequency**: Check on app startup
- **Fallback**: Ship with embedded default database
- **Cache**: Store downloaded DB locally with timestamp

### JSON Schema
```json
{
  "version": "1.0.0",
  "last_updated": "2025-01-15T00:00:00Z",
  "models": [
    {
      "id": "llama-3.1-8b",
      "name": "Llama 3.1 8B",
      "family": "llama",
      "parameter_count": 8,
      "parameter_count_raw": 8000000000,
      "provider": "Meta",
      "license": "llama-3.1-community",
      "use_cases": ["general", "coding", "chat"],
      "popularity_rank": 5,
      "requirements": {
        "fp32": {
          "vram_gb": 32,
          "ram_gb": 40,
          "storage_gb": 30,
          "min_inference_score": 600,
          "estimated_tokens_per_sec": {
            "gpu_tier_high": 80,
            "gpu_tier_mid": 35,
            "gpu_tier_low": 15,
            "cpu_only": 5
          }
        },
        "fp16": {
          "vram_gb": 16,
          "ram_gb": 24,
          "storage_gb": 16,
          "min_inference_score": 500,
          "estimated_tokens_per_sec": {
            "gpu_tier_high": 100,
            "gpu_tier_mid": 45,
            "gpu_tier_low": 20,
            "cpu_only": 8
          }
        },
        "8bit": {
          "vram_gb": 10,
          "ram_gb": 16,
          "storage_gb": 9,
          "min_inference_score": 400,
          "estimated_tokens_per_sec": {
            "gpu_tier_high": 90,
            "gpu_tier_mid": 40,
            "gpu_tier_low": 18,
            "cpu_only": 6
          }
        },
        "4bit": {
          "vram_gb": 6,
          "ram_gb": 12,
          "storage_gb": 5,
          "min_inference_score": 300,
          "estimated_tokens_per_sec": {
            "gpu_tier_high": 70,
            "gpu_tier_mid": 30,
            "gpu_tier_low": 12,
            "cpu_only": 4
          }
        }
      },
      "training_requirements": {
        "full_finetune": {
          "vram_gb": 80,
          "ram_gb": 128,
          "min_training_score": 800,
          "estimated_time_per_epoch_hours": 24
        },
        "lora": {
          "vram_gb": 24,
          "ram_gb": 48,
          "min_training_score": 600,
          "estimated_time_per_epoch_hours": 8
        },
        "qlora": {
          "vram_gb": 12,
          "ram_gb": 24,
          "min_training_score": 500,
          "estimated_time_per_epoch_hours": 10
        }
      },
      "framework_support": {
        "llama_cpp": true,
        "gguf": true,
        "pytorch": true,
        "transformers": true,
        "ollama": true,
        "lm_studio": true,
        "exllama": true
      },
      "recommended_for_beginners": true,
      "notes": "Excellent all-around model, good balance of capability and hardware requirements"
    }
  ],
  "gpu_tiers": {
    "high": {
      "description": "High-end GPUs",
      "examples": ["RTX 4090", "RTX 4080", "A6000", "H100"],
      "min_vram": 20,
      "min_compute_score": 800
    },
    "mid": {
      "description": "Mid-range GPUs",
      "examples": ["RTX 4070", "RTX 3080", "RX 7800 XT"],
      "min_vram": 12,
      "min_compute_score": 500
    },
    "low": {
      "description": "Entry-level GPUs",
      "examples": ["RTX 4060", "RTX 3060", "RX 6700 XT"],
      "min_vram": 8,
      "min_compute_score": 300
    }
  }
}
```

## Upgrade Advisor Requirements

### Upgrade Categories
1. **GPU Upgrade**
   - Calculate VRAM needed for target models
   - Suggest specific GPU models with price ranges
   - Show performance improvement estimate

2. **RAM Upgrade**
   - Based on CPU-only inference or training needs
   - Suggest specific capacity (16GB → 32GB, etc.)

3. **Storage Upgrade**
   - SSD vs NVMe recommendations
   - Capacity based on model storage needs

4. **Framework/Software**
   - CUDA toolkit installation
   - WSL2 setup (Windows)
   - Driver updates

### Cost-Benefit Analysis
- Price ranges for each upgrade
- Expected score improvement
- "Bang for buck" ratio calculation
- Unlock specific models/capabilities

## Framework Compatibility Requirements

### Detection Logic
1. **CUDA**
   - Check `nvidia-smi` availability
   - Parse version from output
   - Check `nvcc --version` for toolkit
   - Registry check (Windows): `HKLM\SOFTWARE\NVIDIA Corporation\GPU Computing Toolkit\CUDA`

2. **ROCm**
   - Check `rocm-smi` availability (Linux)
   - Check registry (Windows if ROCm for Windows exists)
   - Parse version

3. **DirectML**
   - Windows only
   - Check Windows version >= 1903
   - Check for DirectX 12 support
   - Check for `DirectML.dll` in System32

4. **oneAPI**
   - Check for oneAPI installation paths
   - Verify `sycl-ls` command

5. **WSL2**
   - Windows only
   - Check `wsl --status` command
   - Parse WSL version
   - Check for GPU passthrough support

## UI Requirements

### Window Specifications
- **Default Size**: 1200x800
- **Minimum Size**: 1000x700
- **Resizable**: Yes
- **Theme**: Light and Dark mode support
- **Font**: System font, scalable

### Main Views

#### 1. Dashboard (Home)
- Large score displays (3 circular progress indicators)
- Overall rating badge
- Quick summary of capabilities
- "Get Started" button for new users

#### 2. Hardware Details
- Tabbed view: CPU | GPU | Memory | Storage | Frameworks
- Raw specifications
- Visual representations (charts/bars)
- Export capability (JSON/PDF)

#### 3. Model Recommendations
- Filterable list (by use case, size, popularity)
- For each model: compatibility status, estimated performance
- Sort by: recommended, name, size, performance
- "Show only compatible" toggle

#### 4. Upgrade Advisor
- Bottleneck visualization
- Upgrade options with cost estimates
- Before/after comparison
- Step-by-step upgrade guide

#### 5. Benchmark (Optional)
- Start benchmark button
- Real-time progress
- Results comparison with estimates
- Save/share results

#### 6. Educational
- Interactive tutorials
- Glossary
- "Why does this matter?" explanations
- Visual learning aids

#### 7. Settings
- Beginner/Advanced mode toggle
- Theme selection
- Update check
- Opt-in data sharing
- Export/import settings

### UI Mode Differences

**Beginner Mode**:
- Simplified language
- More explanations
- Guided flows
- Tooltips everywhere
- Hide technical details by default

**Advanced Mode**:
- Technical terminology
- Full specifications
- Direct access to all features
- Minimal hand-holding
- Raw data access

## Benchmark Integration Requirements

### Optional Real Benchmarks
- **Default**: Estimation only (fast)
- **Optional**: User can run real tests

### Integration Method
- Use **llamafile** for portability
- Download small test model (tiny Llama variant, ~100MB)
- Run standardized prompts
- Measure: tokens/sec, time to first token, memory usage
- Takes ~30 seconds to 5 minutes depending on test

### Benchmark Tests
1. **Quick Test** (~30 sec)
   - Single prompt inference
   - Basic performance metrics

2. **Comprehensive Test** (~5 min)
   - Multiple prompts (various lengths)
   - Different batch sizes
   - Sustained performance measurement

## Data Privacy Requirements

### Local-Only by Default
- No telemetry without explicit opt-in
- No network calls except model DB update
- All processing happens locally

### Opt-In Data Sharing
- Anonymous hardware profile
- Benchmark results
- Helps community database
- Clear consent UI
- Can revoke anytime
- Data format: JSON to public API endpoint

### Shared Data Schema
```json
{
  "hardware_hash": "sha256_of_hardware_signature",
  "timestamp": "2025-01-15T12:00:00Z",
  "cpu_family": "AMD Ryzen 9",
  "gpu_family": "NVIDIA RTX 40 Series",
  "vram_gb": 24,
  "ram_gb": 64,
  "scores": {
    "inference": 850,
    "training": 780,
    "finetuning": 820
  },
  "benchmark_results": {
    "model_id": "llama-3.1-8b-4bit",
    "tokens_per_sec": 75.3,
    "time_to_first_token_ms": 245
  }
}
```

## Error Handling Requirements

### Graceful Failures
- If hardware detection fails: show "unknown" with explanation
- If model DB download fails: use embedded default
- If scoring fails: show partial scores with warnings
- Never crash, always show something useful

### User-Facing Errors
- Clear, non-technical language (beginner mode)
- Detailed technical info (advanced mode)
- Actionable next steps
- "Report Issue" button with auto-filled diagnostic info

## Testing Requirements

### Unit Tests
- All detection methods
- Scoring algorithms
- Model matching logic
- Upgrade calculations

### Integration Tests
- Hardware detection flow
- Model DB loading
- UI navigation flows

### Manual Testing Checklist
- [ ] Windows 10 compatibility
- [ ] Windows 11 compatibility
- [ ] Linux compatibility (if cross-platform)
- [ ] Various GPU brands (NVIDIA, AMD, Intel)
- [ ] CPU-only systems
- [ ] Low-end hardware (8GB RAM, no GPU)
- [ ] High-end hardware (multiple GPUs, 128GB RAM)
- [ ] Network offline scenario
- [ ] Beginner vs Advanced mode
- [ ] Theme switching
- [ ] Export functionality

## Performance Optimization Requirements

### Startup Optimization
- Lazy load non-critical components
- Async hardware detection
- Cache detection results
- Pre-compile regexes
- Minimal XAML in initial view

### Runtime Optimization
- Object pooling for repeated calculations
- Debounce UI updates
- Virtual scrolling for long lists
- Dispose resources properly
- Use `Span<T>` for string operations

### Memory Optimization
- No memory leaks (dispose subscriptions)
- Clear image caches when not visible
- Stream large files don't load to memory
- Use weak references where appropriate

## Documentation Requirements

### Code Documentation
- XML comments on all public APIs
- README in each major folder
- Architecture decision records (ADR)
- "Why" not just "what" in comments

### User Documentation
- In-app help system
- README.md with screenshots
- FAQ document
- Troubleshooting guide
- Video tutorials (future)

## Security Requirements

### Input Validation
- Validate all file paths
- Sanitize command output parsing
- Validate JSON schema from remote source
- No arbitrary code execution

### Dependencies
- Pin dependency versions
- Regular security audits
- Minimize dependency tree

## Deployment Requirements

### Build Configuration
- Release build: Optimized, trimmed
- Debug build: Symbols, logging

### Packaging
- Single executable (if possible via single-file publish)
- Or: Zip with all dependencies
- No installer required (portable)
- Optional: Create installer for future

### Updates
- Manual for v1.0
- Auto-update check (notify user)
- Download from GitHub releases

## Code Organization Requirements

```
src/
├── LLMCapabilityChecker/                  # Main application
│   ├── App.axaml                          # Application entry
│   ├── Program.cs                         # Entry point
│   ├── ViewModels/                        # MVVM ViewModels
│   │   ├── MainWindowViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── HardwareViewModel.cs
│   │   ├── ModelsViewModel.cs
│   │   ├── UpgradeAdvisorViewModel.cs
│   │   └── SettingsViewModel.cs
│   ├── Views/                             # XAML Views
│   │   ├── MainWindow.axaml
│   │   ├── DashboardView.axaml
│   │   ├── HardwareView.axaml
│   │   ├── ModelsView.axaml
│   │   ├── UpgradeAdvisorView.axaml
│   │   └── SettingsView.axaml
│   ├── Models/                            # Data models
│   │   ├── HardwareInfo.cs
│   │   ├── SystemScore.cs
│   │   ├── ModelInfo.cs
│   │   └── UpgradeRecommendation.cs
│   ├── Services/                          # Business logic
│   │   ├── IHardwareDetectionService.cs
│   │   ├── HardwareDetectionService.cs
│   │   ├── IScoringService.cs
│   │   ├── ScoringService.cs
│   │   ├── IModelDatabaseService.cs
│   │   ├── ModelDatabaseService.cs
│   │   ├── IRecommendationService.cs
│   │   ├── RecommendationService.cs
│   │   └── IUpgradeAdvisorService.cs
│   ├── Helpers/                           # Utility classes
│   │   ├── SystemInfoHelper.cs
│   │   ├── CommandHelper.cs
│   │   └── JsonHelper.cs
│   └── Assets/                            # Icons, images
└── LLMCapabilityChecker.Tests/            # Unit tests
```

## Implementation Priority Order

1. **Phase 1**: Core detection + basic scoring
2. **Phase 2**: Model database integration
3. **Phase 3**: Basic UI (Dashboard + Hardware views)
4. **Phase 4**: Recommendations + Models view
5. **Phase 5**: Upgrade Advisor
6. **Phase 6**: Educational content
7. **Phase 7**: Optional benchmarking
8. **Phase 8**: Polish + optimization
