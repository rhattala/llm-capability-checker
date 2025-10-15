# Architecture Document

## System Overview

### Architecture Style
**Client-Side Application** with **MVVM** (Model-View-ViewModel) pattern
- **Presentation Layer**: Avalonia UI (XAML views)
- **Application Layer**: ViewModels (business logic coordination)
- **Domain Layer**: Services (core business logic)
- **Infrastructure Layer**: Hardware detection, external data sources

### Design Principles
1. **Separation of Concerns**: UI, business logic, and data access are separate
2. **Dependency Inversion**: Depend on abstractions, not concrete implementations
3. **Single Responsibility**: Each class has one reason to change
4. **Open/Closed**: Open for extension, closed for modification
5. **DRY**: Don't Repeat Yourself - reusable components
6. **KISS**: Keep It Simple, Stupid - minimal complexity

---

## High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │Dashboard │  │ Hardware │  │  Models  │  │ Upgrade  │   │
│  │   View   │  │   View   │  │   View   │  │   View   │   │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘   │
└───────┼─────────────┼─────────────┼─────────────┼──────────┘
        │             │             │             │
        ▼             ▼             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │Dashboard │  │ Hardware │  │  Models  │  │ Upgrade  │   │
│  │ViewModel │  │ViewModel │  │ViewModel │  │ViewModel │   │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘   │
└───────┼─────────────┼─────────────┼─────────────┼──────────┘
        │             │             │             │
        └─────────────┴─────────────┴─────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌─────────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │   Hardware      │  │   Scoring    │  │ Recommendation│  │
│  │ Detection Svc   │  │   Service    │  │   Service     │  │
│  └────────┬────────┘  └──────┬───────┘  └───────┬───────┘  │
│           │                  │                   │           │
│  ┌────────┴────────┐  ┌──────┴───────┐  ┌───────┴───────┐  │
│  │ Model Database  │  │   Upgrade    │  │  Benchmark    │  │
│  │    Service      │  │ Advisor Svc  │  │   Service     │  │
│  └────────┬────────┘  └──────────────┘  └───────────────┘  │
└───────────┼────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   WMI API   │  │  nvidia-smi │  │  File I/O   │         │
│  │  (Windows)  │  │  rocm-smi   │  │   (JSON)    │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   HTTP      │  │  Registry   │  │  Process    │         │
│  │  (GitHub)   │  │   Access    │  │  Executor   │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Details

### 1. Presentation Layer

#### Views (Avalonia XAML)
**Purpose**: Display UI and handle user interaction

**Components**:
- `MainWindow.axaml`: Main application window, hosts navigation
- `DashboardView.axaml`: Overview with scores, recommendations
- `HardwareView.axaml`: Detailed hardware specifications
- `ModelsView.axaml`: Model list, filters, compatibility
- `UpgradeAdvisorView.axaml`: Upgrade suggestions, comparisons
- `BenchmarkView.axaml`: Run and view benchmark results
- `EducationView.axaml`: Tutorials, glossary, help
- `SettingsView.axaml`: App configuration

**Key Responsibilities**:
- Bind to ViewModels via DataContext
- Handle user input events
- Display data with formatting
- Navigate between views
- Theming (Light/Dark)

**Anti-patterns to Avoid**:
- ❌ Business logic in code-behind
- ❌ Direct service calls from views
- ❌ Complex calculations in XAML
- ❌ Hardcoded strings (use resources)

---

### 2. Application Layer

#### ViewModels (MVVM Pattern)
**Purpose**: Coordinate between views and services, maintain UI state

**Base Class**: `ViewModelBase`
```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

#### ViewModel Responsibilities

**DashboardViewModel**:
- Aggregate system scores
- Show top recommendations
- Display quick stats
- Handle "Get Started" flow

**HardwareViewModel**:
- Display detected hardware
- Show framework compatibility
- Export hardware profile
- Refresh detection

**ModelsViewModel**:
- Load model database
- Filter/sort models
- Show compatibility status
- Calculate performance estimates

**UpgradeAdvisorViewModel**:
- Identify bottlenecks
- Generate upgrade recommendations
- Compare before/after
- Calculate cost-benefit

**BenchmarkViewModel**:
- Initiate benchmarks
- Show real-time progress
- Display results
- Compare with estimates

**SettingsViewModel**:
- Manage app configuration
- Theme selection
- Mode switching (Beginner/Advanced)
- Data sharing opt-in

---

### 3. Domain Layer

#### Services (Business Logic)

**Design Pattern**: Interface-based services with Dependency Injection

---

#### 3.1 HardwareDetectionService

**Interface**: `IHardwareDetectionService`

```csharp
public interface IHardwareDetectionService
{
    Task<HardwareInfo> DetectHardwareAsync(CancellationToken cancellationToken = default);
    Task<CpuInfo> DetectCpuAsync();
    Task<GpuInfo> DetectGpuAsync();
    Task<MemoryInfo> DetectMemoryAsync();
    Task<StorageInfo> DetectStorageAsync();
    Task<FrameworkSupport> DetectFrameworksAsync();
}
```

**Detection Strategy Pattern**:
```csharp
public interface IDetectionStrategy
{
    Task<T> DetectAsync<T>(CancellationToken cancellationToken);
    bool IsSupported(); // Check if this strategy works on current OS
    int Priority { get; } // Higher priority tried first
}

// Implementations:
- WindowsWmiStrategy (Windows, Priority: 100)
- NvidiaSmiStrategy (NVIDIA GPU, Priority: 90)
- RocmSmiStrategy (AMD GPU, Priority: 90)
- LinuxSysfsStrategy (Linux, Priority: 80)
- FallbackStrategy (All OS, Priority: 0)
```

**Detection Flow**:
1. Get all strategies for component type
2. Sort by priority descending
3. Try each strategy with 2-second timeout
4. Return first successful result
5. If all fail, return "Unknown" with error details

**Error Handling**:
- Never throw exceptions to caller
- Log errors internally
- Return partial results
- Set `IsDetected = false` flag

**Caching**:
- Cache results for session
- Invalidate cache on manual refresh
- Use `Lazy<T>` for expensive operations

---

#### 3.2 ScoringService

**Interface**: `IScoringService`

```csharp
public interface IScoringService
{
    Task<SystemScores> CalculateScoresAsync(HardwareInfo hardware);
    int CalculateInferenceScore(HardwareInfo hardware);
    int CalculateTrainingScore(HardwareInfo hardware);
    int CalculateFineTuningScore(HardwareInfo hardware);
    ScoreBreakdown GetScoreBreakdown(HardwareInfo hardware, ScoreType type);
    string GetScoreRating(int score); // "Excellent", "Good", etc.
}
```

**Scoring Algorithm**:

```csharp
public class ScoringAlgorithm
{
    // Inference Score: GPU-focused, balanced
    public int CalculateInferenceScore(HardwareInfo hw)
    {
        var gpuScore = CalculateGpuScore(hw.Gpu) * 0.5;
        var cpuScore = CalculateCpuScore(hw.Cpu) * 0.2;
        var ramScore = CalculateRamScore(hw.Memory) * 0.2;
        var storageScore = CalculateStorageScore(hw.Storage) * 0.1;
        
        return (int)Math.Round(gpuScore + cpuScore + ramScore + storageScore);
    }
    
    // Training Score: GPU-heavy, VRAM critical
    public int CalculateTrainingScore(HardwareInfo hw)
    {
        var gpuScore = CalculateGpuScore(hw.Gpu, forTraining: true) * 0.7;
        var ramScore = CalculateRamScore(hw.Memory) * 0.2;
        var storageScore = CalculateStorageScore(hw.Storage) * 0.1;
        
        return (int)Math.Round(gpuScore + ramScore + storageScore);
    }
    
    // Fine-tuning Score: Balanced, LoRA-aware
    public int CalculateFineTuningScore(HardwareInfo hw)
    {
        var gpuScore = CalculateGpuScore(hw.Gpu) * 0.6;
        var ramScore = CalculateRamScore(hw.Memory) * 0.25;
        var storageScore = CalculateStorageScore(hw.Storage) * 0.15;
        
        return (int)Math.Round(gpuScore + ramScore + storageScore);
    }
    
    private double CalculateGpuScore(GpuInfo gpu, bool forTraining = false)
    {
        if (gpu == null || !gpu.IsDetected) return 0;
        
        // VRAM is primary factor
        var vramScore = CalculateVramScore(gpu.VramGB, forTraining);
        
        // Compute units (CUDA cores, etc.)
        var computeScore = CalculateComputeScore(gpu);
        
        // Architecture bonus
        var archBonus = GetArchitectureBonus(gpu.Architecture);
        
        // Tensor cores bonus (for training)
        var tensorBonus = forTraining && gpu.HasTensorCores ? 100 : 0;
        
        return Math.Min(1000, vramScore + computeScore + archBonus + tensorBonus);
    }
    
    private double CalculateVramScore(int vramGB, bool forTraining)
    {
        // Inference: 8GB = 500, 24GB = 1000
        // Training: 16GB = 500, 48GB = 1000
        var baseline = forTraining ? 16 : 8;
        var target = forTraining ? 48 : 24;
        
        if (vramGB <= 0) return 0;
        if (vramGB >= target) return 500; // Max 500 from VRAM alone
        
        return 500.0 * (vramGB / (double)target);
    }
    
    private double CalculateCpuScore(CpuInfo cpu)
    {
        if (cpu == null || !cpu.IsDetected) return 0;
        
        // Core count score
        var coreScore = Math.Min(500, cpu.LogicalCores * 20);
        
        // Frequency bonus
        var freqBonus = Math.Min(200, (cpu.MaxFrequencyGHz - 2.0) * 100);
        
        // Modern architecture bonus
        var archBonus = IsModernCpu(cpu) ? 100 : 0;
        
        return Math.Min(500, coreScore + freqBonus + archBonus);
    }
    
    private double CalculateRamScore(MemoryInfo mem)
    {
        if (mem == null || !mem.IsDetected) return 0;
        
        var ramGB = mem.TotalGB;
        
        // 16GB = 500, 32GB = 800, 64GB+ = 1000
        if (ramGB >= 64) return 1000;
        if (ramGB >= 32) return 800;
        if (ramGB >= 16) return 500;
        return ramGB * 31.25; // Linear below 16GB
    }
    
    private double CalculateStorageScore(StorageInfo storage)
    {
        if (storage == null || !storage.IsDetected) return 0;
        
        return storage.Type switch
        {
            StorageType.NVMe => 1000,
            StorageType.SSD => 700,
            StorageType.HDD => 300,
            _ => 500
        };
    }
}
```

**Score Breakdown**:
- Detailed explanation of how score was calculated
- Component contributions
- Identified strengths and weaknesses

---

#### 3.3 ModelDatabaseService

**Interface**: `IModelDatabaseService`

```csharp
public interface IModelDatabaseService
{
    Task<ModelDatabase> LoadDatabaseAsync();
    Task<bool> UpdateDatabaseAsync();
    IEnumerable<ModelInfo> GetCompatibleModels(HardwareInfo hardware, QuantizationType? quantization = null);
    IEnumerable<ModelInfo> GetRecommendedModels(HardwareInfo hardware, int count = 5);
    ModelInfo? GetModelById(string modelId);
    DateTime GetLastUpdateTime();
}
```

**Data Flow**:
1. **On Startup**: Try to download latest database from GitHub
2. **Timeout**: 5 seconds, then use cached/embedded version
3. **Cache**: Save to `%AppData%/LLMCapabilityChecker/models.json`
4. **Validation**: Verify JSON schema before accepting
5. **Fallback**: Ship with embedded default database in resources

**Model Filtering**:
```csharp
public IEnumerable<ModelInfo> GetCompatibleModels(HardwareInfo hardware, QuantizationType? quantization)
{
    var allModels = _database.Models;
    
    return allModels
        .Where(m => IsCompatible(m, hardware, quantization))
        .OrderByDescending(m => m.PopularityRank)
        .ThenBy(m => m.ParameterCount);
}

private bool IsCompatible(ModelInfo model, HardwareInfo hardware, QuantizationType? quantization)
{
    var requirements = GetRequirements(model, quantization);
    
    // Check VRAM (most critical)
    if (hardware.Gpu?.VramGB < requirements.VramGB)
        return false;
    
    // Check RAM
    if (hardware.Memory?.TotalGB < requirements.RamGB)
        return false;
    
    // Check storage
    if (hardware.Storage?.AvailableGB < requirements.StorageGB)
        return false;
    
    return true;
}
```

---

#### 3.4 RecommendationService

**Interface**: `IRecommendationService`

```csharp
public interface IRecommendationService
{
    Task<IEnumerable<ModelRecommendation>> GetRecommendationsAsync(
        HardwareInfo hardware, 
        SystemScores scores,
        UserProfile profile);
        
    Task<PerformanceEstimate> EstimatePerformanceAsync(
        ModelInfo model, 
        HardwareInfo hardware,
        QuantizationType quantization);
}
```

**Recommendation Algorithm**:
```csharp
public class RecommendationAlgorithm
{
    public IEnumerable<ModelRecommendation> GetRecommendations(
        HardwareInfo hardware,
        SystemScores scores,
        UserProfile profile)
    {
        var compatible = _modelDb.GetCompatibleModels(hardware);
        
        return compatible
            .Select(m => ScoreRecommendation(m, hardware, scores, profile))
            .OrderByDescending(r => r.RecommendationScore)
            .Take(5);
    }
    
    private ModelRecommendation ScoreRecommendation(
        ModelInfo model,
        HardwareInfo hardware,
        SystemScores scores,
        UserProfile profile)
    {
        var score = 0.0;
        
        // Popularity weight (30%)
        score += model.PopularityRank * 0.3;
        
        // Hardware utilization (30%)
        // Prefer models that use 70-90% of available resources
        var utilizationScore = CalculateUtilization(model, hardware);
        score += utilizationScore * 0.3;
        
        // Use case match (20%)
        if (model.UseCases.Contains(profile.PrimaryUseCase))
            score += 20;
        
        // Beginner-friendly (20% if beginner mode)
        if (profile.IsBeginnerMode && model.RecommendedForBeginners)
            score += 20;
        
        return new ModelRecommendation
        {
            Model = model,
            RecommendationScore = score,
            Reason = GenerateRecommendationReason(model, hardware, profile),
            EstimatedPerformance = EstimatePerformance(model, hardware)
        };
    }
}
```

---

#### 3.5 UpgradeAdvisorService

**Interface**: `IUpgradeAdvisorService`

```csharp
public interface IUpgradeAdvisorService
{
    Task<IEnumerable<UpgradeRecommendation>> GetUpgradeRecommendationsAsync(
        HardwareInfo hardware,
        SystemScores currentScores,
        UpgradeGoal goal);
        
    Task<BottleneckAnalysis> AnalyzeBottlenecksAsync(
        HardwareInfo hardware,
        SystemScores scores);
        
    Task<BeforeAfterComparison> CompareUpgradeAsync(
        HardwareInfo current,
        UpgradeRecommendation upgrade);
}
```

**Upgrade Recommendation Strategy**:
```csharp
public class UpgradeStrategy
{
    public IEnumerable<UpgradeRecommendation> GetRecommendations(
        HardwareInfo hardware,
        SystemScores scores,
        UpgradeGoal goal)
    {
        var recommendations = new List<UpgradeRecommendation>();
        
        // Identify bottleneck
        var bottleneck = IdentifyPrimaryBottleneck(hardware, scores);
        
        // Generate recommendations based on bottleneck
        recommendations.AddRange(bottleneck switch
        {
            Bottleneck.VRAM => GenerateGpuUpgrades(hardware, goal),
            Bottleneck.RAM => GenerateRamUpgrades(hardware, goal),
            Bottleneck.Storage => GenerateStorageUpgrades(hardware, goal),
            Bottleneck.CPU => GenerateCpuUpgrades(hardware, goal),
            _ => new List<UpgradeRecommendation>()
        });
        
        // Rank by bang-for-buck
        return recommendations
            .Select(r => CalculateBangForBuck(r, scores))
            .OrderByDescending(r => r.BangForBuck)
            .Take(5);
    }
    
    private IEnumerable<UpgradeRecommendation> GenerateGpuUpgrades(
        HardwareInfo hardware,
        UpgradeGoal goal)
    {
        var currentVram = hardware.Gpu?.VramGB ?? 0;
        var targetVram = CalculateTargetVram(goal);
        
        // Suggest specific GPU models
        var suggestions = new List<UpgradeRecommendation>
        {
            new() 
            { 
                Component = "GPU",
                SpecificProduct = "NVIDIA RTX 4060 Ti 16GB",
                EstimatedCost = CostTier.Budget,
                NewSpecs = new() { VramGB = 16 },
                UnlockedModels = GetUnlockedModels(16),
                ScoreImprovement = EstimateScoreImprovement(hardware, 16)
            },
            new()
            {
                Component = "GPU",
                SpecificProduct = "NVIDIA RTX 4070 Ti 12GB",
                EstimatedCost = CostTier.MidRange,
                NewSpecs = new() { VramGB = 12 },
                UnlockedModels = GetUnlockedModels(12),
                ScoreImprovement = EstimateScoreImprovement(hardware, 12)
            }
        };
        
        return suggestions.Where(s => s.NewSpecs.VramGB > currentVram);
    }
}
```

---

#### 3.6 BenchmarkService

**Interface**: `IBenchmarkService`

```csharp
public interface IBenchmarkService
{
    Task<BenchmarkResult> RunQuickBenchmarkAsync(
        IProgress<BenchmarkProgress>? progress = null,
        CancellationToken cancellationToken = default);
        
    Task<BenchmarkResult> RunComprehensiveBenchmarkAsync(
        IProgress<BenchmarkProgress>? progress = null,
        CancellationToken cancellationToken = default);
        
    Task<bool> DownloadBenchmarkModelAsync(
        IProgress<double>? progress = null);
}
```

**Benchmark Implementation**:
- Use llamafile for cross-platform compatibility
- Download tiny Llama model (~100MB) on first run
- Run standardized prompts
- Measure: tokens/sec, TTFT, memory usage
- Compare with estimates from ScoringService

---

### 4. Infrastructure Layer

#### Data Access

**GitHub Data Source**:
```csharp
public class GitHubModelDataSource
{
    private readonly HttpClient _httpClient;
    private const string DATABASE_URL = "https://raw.githubusercontent.com/[org]/[repo]/main/models.json";
    
    public async Task<ModelDatabase?> DownloadDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            
            var json = await _httpClient.GetStringAsync(DATABASE_URL, linkedCts.Token);
            return JsonSerializer.Deserialize<ModelDatabase>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download model database");
            return null;
        }
    }
}
```

**Local Cache**:
```csharp
public class LocalCacheService
{
    private readonly string _cacheDirectory;
    
    public LocalCacheService()
    {
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LLMCapabilityChecker",
            "Cache");
            
        Directory.CreateDirectory(_cacheDirectory);
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var path = GetCachePath(key);
        if (!File.Exists(path)) return default;
        
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public async Task SetAsync<T>(string key, T value)
    {
        var path = GetCachePath(key);
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }
}
```

---

## Data Models

### Core Models

```csharp
// Hardware Information
public class HardwareInfo
{
    public CpuInfo Cpu { get; set; } = new();
    public GpuInfo? Gpu { get; set; }
    public MemoryInfo Memory { get; set; } = new();
    public StorageInfo Storage { get; set; } = new();
    public FrameworkSupport Frameworks { get; set; } = new();
    public DateTime DetectedAt { get; set; }
}

public class CpuInfo
{
    public bool IsDetected { get; set; }
    public string Model { get; set; } = "Unknown";
    public string Manufacturer { get; set; } = "Unknown";
    public int PhysicalCores { get; set; }
    public int LogicalCores { get; set; }
    public double BaseFrequencyGHz { get; set; }
    public double MaxFrequencyGHz { get; set; }
    public string Architecture { get; set; } = "Unknown";
    public int L1CacheKB { get; set; }
    public int L2CacheKB { get; set; }
    public int L3CacheMB { get; set; }
}

public class GpuInfo
{
    public bool IsDetected { get; set; }
    public string Model { get; set; } = "Unknown";
    public string Manufacturer { get; set; } = "Unknown"; // NVIDIA, AMD, Intel
    public int VramGB { get; set; }
    public int ComputeUnits { get; set; } // CUDA cores, Stream processors, etc.
    public string Architecture { get; set; } = "Unknown"; // Ampere, RDNA2, etc.
    public string DriverVersion { get; set; } = "Unknown";
    public double ComputeCapability { get; set; } // CUDA compute capability
    public bool HasTensorCores { get; set; }
}

public class MemoryInfo
{
    public bool IsDetected { get; set; }
    public int TotalGB { get; set; }
    public int AvailableGB { get; set; }
    public int SpeedMHz { get; set; }
    public string Type { get; set; } = "Unknown"; // DDR4, DDR5
}

public class StorageInfo
{
    public bool IsDetected { get; set; }
    public StorageType Type { get; set; }
    public int TotalGB { get; set; }
    public int AvailableGB { get; set; }
    public int ReadSpeedMBps { get; set; }
    public int WriteSpeedMBps { get; set; }
}

public class FrameworkSupport
{
    public CudaInfo? Cuda { get; set; }
    public RocmInfo? Rocm { get; set; }
    public bool DirectMLSupported { get; set; }
    public bool OneAPISupported { get; set; }
    public Wsl2Info? Wsl2 { get; set; }
}

// Scores
public class SystemScores
{
    public int InferenceScore { get; set; }
    public int TrainingScore { get; set; }
    public int FineTuningScore { get; set; }
    public string OverallRating { get; set; } = "Unknown";
    public ScoreBreakdown InferenceBreakdown { get; set; } = new();
    public ScoreBreakdown TrainingBreakdown { get; set; } = new();
    public ScoreBreakdown FineTuningBreakdown { get; set; } = new();
}

public class ScoreBreakdown
{
    public double GpuContribution { get; set; }
    public double CpuContribution { get; set; }
    public double RamContribution { get; set; }
    public double StorageContribution { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

// Models
public class ModelInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Family { get; set; } = "";
    public int ParameterCount { get; set; }
    public long ParameterCountRaw { get; set; }
    public string Provider { get; set; } = "";
    public string License { get; set; } = "";
    public List<string> UseCases { get; set; } = new();
    public int PopularityRank { get; set; }
    public Dictionary<string, ModelRequirements> Requirements { get; set; } = new();
    public TrainingRequirements? TrainingRequirements { get; set; }
    public FrameworkCompatibility FrameworkSupport { get; set; } = new();
    public bool RecommendedForBeginners { get; set; }
    public string Notes { get; set; } = "";
}

public class ModelRequirements
{
    public int VramGB { get; set; }
    public int RamGB { get; set; }
    public int StorageGB { get; set; }
    public int MinInferenceScore { get; set; }
    public Dictionary<string, int> EstimatedTokensPerSec { get; set; } = new();
}

// Recommendations
public class ModelRecommendation
{
    public ModelInfo Model { get; set; } = new();
    public double RecommendationScore { get; set; }
    public string Reason { get; set; } = "";
    public PerformanceEstimate EstimatedPerformance { get; set; } = new();
    public CompatibilityStatus Compatibility { get; set; }
}

public class UpgradeRecommendation
{
    public string Component { get; set; } = "";
    public string SpecificProduct { get; set; } = "";
    public CostTier EstimatedCost { get; set; }
    public HardwareSpecs NewSpecs { get; set; } = new();
    public List<string> UnlockedModels { get; set; } = new();
    public ScoreImprovement ScoreImprovement { get; set; } = new();
    public double BangForBuck { get; set; }
    public string Explanation { get; set; } = "";
}

public enum CostTier
{
    Budget,    // $
    MidRange,  // $$
    HighEnd    // $$$
}
```

---

## Cross-Cutting Concerns

### Logging
```csharp
// Use Microsoft.Extensions.Logging
services.AddLogging(builder =>
{
    builder.AddDebug();
    #if RELEASE
    builder.AddFile("logs/app.log");
    #endif
});
```

### Dependency Injection
```csharp
// Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var serviceProvider = services.BuildServiceProvider();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IHardwareDetectionService, HardwareDetectionService>();
        services.AddSingleton<IScoringService, ScoringService>();
        services.AddSingleton<IModelDatabaseService, ModelDatabaseService>();
        services.AddSingleton<IRecommendationService, RecommendationService>();
        services.AddSingleton<IUpgradeAdvisorService, UpgradeAdvisorService>();
        services.AddSingleton<IBenchmarkService, BenchmarkService>();
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<HardwareViewModel>();
        services.AddTransient<ModelsViewModel>();
        services.AddTransient<UpgradeAdvisorViewModel>();
        
        // Infrastructure
        services.AddHttpClient();
        services.AddSingleton<LocalCacheService>();
    }
}
```

### Error Handling Strategy
```csharp
public class ErrorHandler
{
    // Global exception handling
    public static void HandleException(Exception ex, string context)
    {
        _logger.LogError(ex, "Error in {Context}", context);
        
        // Show user-friendly message
        if (IsBeginnerMode)
            ShowSimpleError(ex, context);
        else
            ShowDetailedError(ex, context);
    }
    
    // Graceful degradation
    public static T HandleWithFallback<T>(
        Func<T> operation,
        T fallback,
        string operationName)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Operation} failed, using fallback", operationName);
            return fallback;
        }
    }
}
```

---

## Performance Considerations

### Startup Optimization
1. **Lazy Loading**: Don't load everything at startup
2. **Async Detection**: Hardware detection runs in background
3. **Cached Results**: Cache detection results in memory
4. **Minimal Initial UI**: Show splash → dashboard quickly

### Runtime Optimization
1. **Virtual Scrolling**: For long model lists
2. **Debouncing**: Filter/search operations debounced 300ms
3. **Computed Properties**: Cache calculated values
4. **Dispose Properly**: Unsubscribe from events, dispose resources

### Memory Management
1. **Weak References**: For cached non-critical data
2. **Image Disposal**: Clear image caches when views unload
3. **Stream Processing**: Don't load large files to memory
4. **GC-Friendly**: Minimize allocations in hot paths

---

## Security Considerations

### Input Validation
- Validate all file paths (no path traversal)
- Sanitize command output parsing
- Validate JSON schema from remote source

### Data Privacy
- No telemetry without opt-in
- Anonymous data sharing (hash hardware IDs)
- No PII in logs or shared data

### Dependency Security
- Pin dependency versions
- Regular security audits (Dependabot)
- Minimize dependency tree

---

## Testing Strategy

### Unit Tests
- Test all services independently
- Mock dependencies
- Test edge cases (null, empty, extreme values)
- Test error handling

### Integration Tests
- Test service combinations
- Test data flow end-to-end
- Test with real hardware detection (on dev machines)

### UI Tests
- Test navigation flows
- Test data binding
- Test theme switching
- Test beginner/advanced mode

---

## Deployment Architecture

### Build Process
1. Restore NuGet packages
2. Compile Release configuration
3. Run tests
4. Publish self-contained (optional)
5. Create portable zip
6. Generate checksums

### Distribution
- **Primary**: GitHub Releases
- **Format**: Portable zip (no installer)
- **Size Target**: < 50MB
- **Auto-update**: Check GitHub API for latest release

---

## Future Extensibility

### Plugin System (Future)
- Allow community plugins for:
  - Custom detection strategies
  - Custom scoring algorithms
  - Additional model databases
  - Integration with tools (LM Studio, Ollama)

### API (Future)
- Expose CLI for automation
- REST API for integration
- Export formats (JSON, CSV, PDF)

---

## Architecture Decision Records (ADRs)

### ADR-001: Why Avalonia over WPF?
**Decision**: Use Avalonia UI
**Rationale**:
- Cross-platform (Windows, Linux, macOS)
- Modern XAML-based UI
- Active development
- Better performance than WPF
- Easier theming

### ADR-002: Why MVVM?
**Decision**: Use MVVM pattern
**Rationale**:
- Separation of concerns
- Testable business logic
- Standard for XAML apps
- Clear architecture
- AI-friendly structure

### ADR-003: Why Dependency Injection?
**Decision**: Use DI with interface-based services
**Rationale**:
- Testability (mock dependencies)
- Flexibility (swap implementations)
- Maintainability (loose coupling)
- Standard practice in modern C#

### ADR-004: Why Async/Await Everywhere?
**Decision**: All I/O operations are async
**Rationale**:
- Non-blocking UI
- Better responsiveness
- Standard in modern C#
- Scalable

### ADR-005: Why JSON from GitHub?
**Decision**: Host model database as JSON on GitHub
**Rationale**:
- Easy to update (no app release needed)
- Community can contribute (PRs)
- Version controlled
- Free hosting
- Simple HTTP fetch

---

## Code Organization Best Practices

### Naming Conventions
- **Classes**: PascalCase (e.g., `HardwareDetectionService`)
- **Interfaces**: IPascalCase (e.g., `IHardwareDetectionService`)
- **Methods**: PascalCase (e.g., `DetectHardwareAsync`)
- **Properties**: PascalCase (e.g., `InferenceScore`)
- **Fields**: _camelCase (e.g., `_logger`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_RETRIES`)

### File Organization
- One class per file
- Match file name to class name
- Group related classes in folders
- Keep folders shallow (max 2-3 levels)

### Comments
- XML comments on all public APIs
- Explain "why" not "what"
- Document complex algorithms
- Link to external resources when relevant

### AI-Friendly Code
- Clear, descriptive names
- Short methods (< 50 lines)
- Single responsibility
- Comprehensive error handling
- Well-structured with clear patterns

---

## Summary

This architecture provides:
- ✅ Clean separation of concerns
- ✅ Testable components
- ✅ Extensible design
- ✅ Performance-optimized
- ✅ Cross-platform capable
- ✅ AI-friendly codebase
- ✅ Minimal dependencies
- ✅ Privacy-first design
- ✅ Graceful error handling
- ✅ Comprehensive logging

The architecture supports all MVP features while remaining maintainable and extensible for future enhancements.
