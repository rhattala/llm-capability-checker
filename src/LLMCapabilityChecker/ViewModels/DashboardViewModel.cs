using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for the main dashboard view
/// Displays hardware info, system scores, and recommended models
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IHardwareDetectionService _hardwareService;
    private readonly IScoringService _scoringService;
    private readonly IModelDatabaseService _modelService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IReportExportService _reportExportService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private HardwareInfo? _hardwareInfo;

    [ObservableProperty]
    private SystemScores? _systemScores;

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _recommendedModels = new();

    // Categorized model collections
    [ObservableProperty]
    private ObservableCollection<ModelInfo> _perfectMatchModels = new();

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _goodFitModels = new();

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _possibleModels = new();

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _notRecommendedModels = new();

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _loadingMessage = "Detecting hardware...";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private BenchmarkResults? _benchmarkResults;

    [ObservableProperty]
    private bool _isBenchmarkRunning;

    [ObservableProperty]
    private bool _showBenchmarkResults;

    /// <summary>
    /// Formatted CPU display text
    /// </summary>
    public string CpuDisplay => HardwareInfo?.Cpu.Model ?? "Unknown";

    /// <summary>
    /// Formatted GPU display text
    /// </summary>
    public string GpuDisplay => HardwareInfo?.Gpu.Model ?? "No dedicated GPU";

    /// <summary>
    /// Formatted RAM display text
    /// </summary>
    public string RamDisplay => HardwareInfo != null ? $"{HardwareInfo.Memory.TotalGB} GB" : "Unknown";

    /// <summary>
    /// Formatted Storage display text
    /// </summary>
    public string StorageDisplay => HardwareInfo != null
        ? $"{HardwareInfo.Storage.AvailableGB} GB Available ({HardwareInfo.Storage.Type})"
        : "Unknown";

    /// <summary>
    /// Overall score display (0-100) - DEPRECATED: Use SystemScores.InferenceScore directly
    /// </summary>
    public int OverallScore => SystemScores?.InferenceScore ?? 0;

    /// <summary>
    /// Inference score display helper
    /// </summary>
    public string InferenceScoreDisplay => $"{SystemScores?.InferenceScore ?? 0}";

    /// <summary>
    /// Training score display helper
    /// </summary>
    public string TrainingScoreDisplay => $"{SystemScores?.TrainingScore ?? 0}";

    /// <summary>
    /// Fine-tuning score display helper
    /// </summary>
    public string FineTuningScoreDisplay => $"{SystemScores?.FineTuningScore ?? 0}";

    /// <summary>
    /// System tier badge text
    /// </summary>
    public string SystemTierBadge => SystemScores?.SystemTier ?? "Unknown";

    /// <summary>
    /// Recommended model size text
    /// </summary>
    public string RecommendedModelSize => SystemScores?.RecommendedModelSize ?? "Unknown";

    /// <summary>
    /// Primary bottleneck text
    /// </summary>
    public string PrimaryBottleneck => SystemScores?.PrimaryBottleneck ?? "None identified";

    // Upgrade Suggestion Properties
    public bool ShowCpuUpgrade => SystemScores?.Breakdown.CpuScore < 80;
    public bool ShowGpuUpgrade => SystemScores?.Breakdown.GpuScore < 80;
    public bool ShowRamUpgrade => SystemScores?.Breakdown.MemoryScore < 80;
    public bool ShowStorageUpgrade => SystemScores?.Breakdown.StorageScore < 80;
    public bool ShowFrameworkUpgrade => SystemScores?.Breakdown.FrameworkScore < 80;

    public string CpuScoreColor => GetScoreColor(SystemScores?.Breakdown.CpuScore ?? 0);
    public string GpuScoreColor => GetScoreColor(SystemScores?.Breakdown.GpuScore ?? 0);
    public string RamScoreColor => GetScoreColor(SystemScores?.Breakdown.MemoryScore ?? 0);
    public string StorageScoreColor => GetScoreColor(SystemScores?.Breakdown.StorageScore ?? 0);
    public string FrameworkScoreColor => GetScoreColor(SystemScores?.Breakdown.FrameworkScore ?? 0);

    public string CpuUpgradeSuggestion => GetCpuSuggestion();
    public string GpuUpgradeSuggestion => GetGpuSuggestion();
    public string RamUpgradeSuggestion => GetRamSuggestion();
    public string StorageUpgradeSuggestion => GetStorageSuggestion();
    public string FrameworkUpgradeSuggestion => GetFrameworkSuggestion();

    public string FrameworksDisplay => GetFrameworksDisplay();
    public string UpgradeSummary => GetUpgradeSummary();

    // Visibility flags for categorized models
    public bool HasPerfectMatches => PerfectMatchModels.Count > 0;
    public bool HasGoodFit => GoodFitModels.Count > 0;
    public bool HasPossibleModels => PossibleModels.Count > 0;
    public bool HasNotRecommended => NotRecommendedModels.Count > 0;

    // Navigation action - set by MainWindowViewModel
    public Action<ComponentType>? NavigateToDetails { get; set; }
    public Action? NavigateToModelsList { get; set; }

    public DashboardViewModel(
        IHardwareDetectionService hardwareService,
        IScoringService scoringService,
        IModelDatabaseService modelService,
        IBenchmarkService benchmarkService,
        IReportExportService reportExportService,
        ILogger<DashboardViewModel> logger)
    {
        _hardwareService = hardwareService;
        _scoringService = scoringService;
        _modelService = modelService;
        _benchmarkService = benchmarkService;
        _reportExportService = reportExportService;
        _logger = logger;

        // Load data on initialization
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Loads all dashboard data asynchronously
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            LoadingMessage = "Detecting hardware...";

            // Detect hardware
            HardwareInfo = await _hardwareService.DetectHardwareAsync();
            _logger.LogInformation("Hardware detection completed");

            LoadingMessage = "Calculating system scores...";

            // Calculate scores
            SystemScores = await _scoringService.CalculateScoresAsync(HardwareInfo);
            _logger.LogInformation("System scoring completed: {Score}/100", SystemScores.OverallScore);

            LoadingMessage = "Loading model recommendations...";

            // Load model database and get recommendations
            var allModels = await _modelService.GetAllModelsAsync();
            var recommended = await _modelService.GetRecommendedModelsAsync(HardwareInfo);

            // Take top 3 recommended models (legacy support)
            RecommendedModels = new ObservableCollection<ModelInfo>(
                recommended.Take(3)
            );

            // Categorize models by compatibility score
            PerfectMatchModels.Clear();
            GoodFitModels.Clear();
            PossibleModels.Clear();
            NotRecommendedModels.Clear();

            foreach (var model in recommended)
            {
                if (model.CompatibilityScore >= 90)
                    PerfectMatchModels.Add(model);
                else if (model.CompatibilityScore >= 70)
                    GoodFitModels.Add(model);
                else if (model.CompatibilityScore >= 50)
                    PossibleModels.Add(model);
                else
                    NotRecommendedModels.Add(model);
            }

            // Limit to top 5 per category
            while (PerfectMatchModels.Count > 5)
                PerfectMatchModels.RemoveAt(PerfectMatchModels.Count - 1);
            while (GoodFitModels.Count > 5)
                GoodFitModels.RemoveAt(GoodFitModels.Count - 1);
            while (PossibleModels.Count > 5)
                PossibleModels.RemoveAt(PossibleModels.Count - 1);
            while (NotRecommendedModels.Count > 3)
                NotRecommendedModels.RemoveAt(NotRecommendedModels.Count - 1);

            _logger.LogInformation("Loaded {Count} recommended models (Perfect: {Perfect}, Good: {Good}, Possible: {Possible}, Not Recommended: {NotRec})",
                RecommendedModels.Count, PerfectMatchModels.Count, GoodFitModels.Count, PossibleModels.Count, NotRecommendedModels.Count);

            // Notify UI that computed properties have changed
            OnPropertyChanged(nameof(CpuDisplay));
            OnPropertyChanged(nameof(GpuDisplay));
            OnPropertyChanged(nameof(RamDisplay));
            OnPropertyChanged(nameof(StorageDisplay));
            OnPropertyChanged(nameof(OverallScore));
            OnPropertyChanged(nameof(InferenceScoreDisplay));
            OnPropertyChanged(nameof(TrainingScoreDisplay));
            OnPropertyChanged(nameof(FineTuningScoreDisplay));
            OnPropertyChanged(nameof(SystemTierBadge));
            OnPropertyChanged(nameof(RecommendedModelSize));
            OnPropertyChanged(nameof(PrimaryBottleneck));
            OnPropertyChanged(nameof(ShowCpuUpgrade));
            OnPropertyChanged(nameof(ShowGpuUpgrade));
            OnPropertyChanged(nameof(ShowRamUpgrade));
            OnPropertyChanged(nameof(ShowStorageUpgrade));
            OnPropertyChanged(nameof(ShowFrameworkUpgrade));
            OnPropertyChanged(nameof(CpuScoreColor));
            OnPropertyChanged(nameof(GpuScoreColor));
            OnPropertyChanged(nameof(RamScoreColor));
            OnPropertyChanged(nameof(StorageScoreColor));
            OnPropertyChanged(nameof(FrameworkScoreColor));
            OnPropertyChanged(nameof(CpuUpgradeSuggestion));
            OnPropertyChanged(nameof(GpuUpgradeSuggestion));
            OnPropertyChanged(nameof(RamUpgradeSuggestion));
            OnPropertyChanged(nameof(StorageUpgradeSuggestion));
            OnPropertyChanged(nameof(FrameworkUpgradeSuggestion));
            OnPropertyChanged(nameof(FrameworksDisplay));
            OnPropertyChanged(nameof(UpgradeSummary));
            OnPropertyChanged(nameof(HasPerfectMatches));
            OnPropertyChanged(nameof(HasGoodFit));
            OnPropertyChanged(nameof(HasPossibleModels));
            OnPropertyChanged(nameof(HasNotRecommended));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            HasError = true;
            ErrorMessage = $"Failed to load dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes all dashboard data
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Runs performance benchmark tests
    /// </summary>
    [RelayCommand]
    private async Task RunBenchmarkAsync()
    {
        try
        {
            IsBenchmarkRunning = true;
            _logger.LogInformation("Starting benchmark run");

            BenchmarkResults = await _benchmarkService.RunSystemBenchmarkAsync();
            ShowBenchmarkResults = true;

            _logger.LogInformation(
                "Benchmark completed: {Single:F0} single-core, {Multi:F0} multi-core, {BW:F2} GB/s memory bandwidth",
                BenchmarkResults.CpuSingleCoreScore,
                BenchmarkResults.CpuMultiCoreScore,
                BenchmarkResults.MemoryBandwidthGBps
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running benchmark");
            HasError = true;
            ErrorMessage = $"Benchmark failed: {ex.Message}";
        }
        finally
        {
            IsBenchmarkRunning = false;
        }
    }

    /// <summary>
    /// Closes the benchmark results display
    /// </summary>
    [RelayCommand]
    private void CloseBenchmarkResults()
    {
        ShowBenchmarkResults = false;
    }

    // Helper methods for upgrade suggestions
    private string GetScoreColor(int score)
    {
        if (score >= 80) return "#4CAF50"; // Green
        if (score >= 60) return "#FFA726"; // Orange/Yellow
        return "#EF5350"; // Red
    }

    private string GetCpuSuggestion()
    {
        if (SystemScores?.Breakdown.CpuScore >= 80) return string.Empty;

        var cores = HardwareInfo?.Cpu.Cores ?? 0;
        if (cores < 8)
            return "Upgrade to a modern 8+ core CPU (e.g., AMD Ryzen 7 or Intel i7/i9) for better LLM inference performance.";

        return "Consider upgrading to a newer generation CPU with better single-thread performance for improved model loading.";
    }

    private string GetGpuSuggestion()
    {
        if (SystemScores?.Breakdown.GpuScore >= 80) return string.Empty;

        var vram = HardwareInfo?.Gpu.VramGB ?? 0;
        var currentGpu = HardwareInfo?.Gpu.Model?.ToLowerInvariant() ?? "";

        // Detect current GPU tier to avoid suggesting lateral moves
        bool hasRtx4070Ti = currentGpu.Contains("4070 ti") && !currentGpu.Contains("super");
        bool hasRtx4070TiSuper = currentGpu.Contains("4070 ti super");
        bool hasRtx4080 = currentGpu.Contains("4080") && !currentGpu.Contains("super");
        bool hasRtx4080Super = currentGpu.Contains("4080 super");
        bool hasRtx4090 = currentGpu.Contains("4090");

        // Don't suggest same or worse GPUs
        if (vram >= 11 && vram < 16)
        {
            if (hasRtx4070Ti)
                return "Upgrade to RTX 4070 Ti Super (16GB) or RTX 4080 (16GB) for running larger models and better 13B performance.";
            if (hasRtx4070TiSuper)
                return "Upgrade to RTX 4080 (16GB) or higher for better performance with 13B+ models.";
            return "Upgrade to RTX 4070 Ti Super (16GB) or RTX 4080 (16GB) for running larger quantized models.";
        }

        if (vram < 11)
            return "Upgrade to RTX 4070 Ti (12GB) or RTX 4070 Ti Super (16GB) for running 7B-13B models efficiently.";

        if (vram >= 16 && vram < 24)
        {
            if (hasRtx4080 || hasRtx4080Super)
                return "Upgrade to RTX 4090 (24GB) to unlock 34B+ models and full fine-tuning capabilities.";
            return "Consider upgrading to RTX 4090 (24GB) for optimal performance with large models.";
        }

        return "Your GPU VRAM is excellent for most LLM workloads.";
    }

    private string GetRamSuggestion()
    {
        if (SystemScores?.Breakdown.MemoryScore >= 80) return string.Empty;

        var totalRam = HardwareInfo?.Memory.TotalGB ?? 0;
        if (totalRam < 32)
            return "Upgrade to 32GB RAM minimum for running 7B-13B models efficiently.";
        if (totalRam < 64)
            return "Upgrade to 64GB RAM for running larger 30B+ models or multiple models simultaneously.";

        return "Consider upgrading to 128GB RAM for enterprise-level model deployment.";
    }

    private string GetStorageSuggestion()
    {
        if (SystemScores?.Breakdown.StorageScore >= 80) return string.Empty;

        var storageType = HardwareInfo?.Storage.Type ?? "Unknown";
        if (storageType.Contains("HDD", StringComparison.OrdinalIgnoreCase))
            return "Upgrade to NVMe SSD for 10x faster model loading. Recommended: Samsung 990 Pro or WD Black SN850X.";
        if (storageType.Contains("SSD", StringComparison.OrdinalIgnoreCase) && !storageType.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
            return "Upgrade to NVMe SSD for faster model loading. Recommended: Samsung 990 Pro (PCIe 4.0).";

        var availableGB = HardwareInfo?.Storage.AvailableGB ?? 0;
        if (availableGB < 100)
            return "Free up storage space or add additional NVMe drive. LLM models can be 20-100GB each.";

        return "Consider upgrading to faster PCIe 5.0 NVMe drive for optimal model loading speeds.";
    }

    private string GetFrameworkSuggestion()
    {
        if (SystemScores?.Breakdown.FrameworkScore >= 80) return string.Empty;

        var hasCuda = HardwareInfo?.Frameworks.HasCuda ?? false;
        var hasDirectMl = HardwareInfo?.Frameworks.HasDirectMl ?? false;

        if (!hasCuda && HardwareInfo?.Gpu.Vendor == "NVIDIA")
            return "Install CUDA Toolkit 12.x for GPU acceleration with NVIDIA cards.";
        if (!hasDirectMl && Environment.OSVersion.Platform == PlatformID.Win32NT)
            return "Install DirectML for GPU acceleration on Windows.";

        return "Install missing ML frameworks (CUDA, DirectML, or ROCm) for optimal GPU utilization.";
    }

    private string GetFrameworksDisplay()
    {
        if (HardwareInfo?.Frameworks == null) return "No frameworks detected";

        var frameworks = new List<string>();
        if (HardwareInfo.Frameworks.HasCuda) frameworks.Add($"CUDA {HardwareInfo.Frameworks.CudaVersion}");
        if (HardwareInfo.Frameworks.HasRocm) frameworks.Add($"ROCm {HardwareInfo.Frameworks.RocmVersion}");
        if (HardwareInfo.Frameworks.HasMetal) frameworks.Add("Metal");
        if (HardwareInfo.Frameworks.HasDirectMl) frameworks.Add("DirectML");
        if (HardwareInfo.Frameworks.HasOpenVino) frameworks.Add("OpenVINO");

        return frameworks.Any() ? string.Join(", ", frameworks) : "CPU Only";
    }

    private string GetUpgradeSummary()
    {
        if (SystemScores == null) return "No data available";

        var needsUpgrade = new List<string>();
        if (SystemScores.Breakdown.CpuScore < 80) needsUpgrade.Add("CPU");
        if (SystemScores.Breakdown.GpuScore < 80) needsUpgrade.Add("GPU");
        if (SystemScores.Breakdown.MemoryScore < 80) needsUpgrade.Add("RAM");
        if (SystemScores.Breakdown.StorageScore < 80) needsUpgrade.Add("Storage");
        if (SystemScores.Breakdown.FrameworkScore < 80) needsUpgrade.Add("Frameworks");

        if (!needsUpgrade.Any())
            return "Your system is well-optimized! No major upgrades needed.";

        if (needsUpgrade.Count == 1)
            return $"Focus on upgrading your {needsUpgrade[0]} for the best performance improvement.";

        return $"Consider upgrading: {string.Join(", ", needsUpgrade)}. Priority: {SystemScores.PrimaryBottleneck}";
    }

    /// <summary>
    /// Navigates to component details
    /// </summary>
    [RelayCommand]
    private void NavigateToComponentDetails(string componentType)
    {
        if (Enum.TryParse<ComponentType>(componentType, out var type))
        {
            NavigateToDetails?.Invoke(type);
        }
    }

    /// <summary>
    /// Navigates to the models browser view
    /// </summary>
    [RelayCommand]
    private void NavigateToModels()
    {
        NavigateToModelsList?.Invoke();
    }

    /// <summary>
    /// Exports the hardware analysis report
    /// </summary>
    [RelayCommand]
    private async Task ExportReportAsync()
    {
        try
        {
            if (HardwareInfo == null || SystemScores == null)
            {
                _logger.LogWarning("Cannot export report: Hardware info or system scores not available");
                ErrorMessage = "No data available to export. Please refresh the dashboard first.";
                HasError = true;
                return;
            }

            _logger.LogInformation("Starting report export");

            // Export as JSON
            var jsonContent = await _reportExportService.ExportAsJsonAsync(
                HardwareInfo,
                SystemScores,
                RecommendedModels.ToList()
            );

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var suggestedFileName = $"LLM_Capability_Report_{timestamp}.json";

            // Save to file
            var saved = await _reportExportService.SaveReportToFileAsync(jsonContent, suggestedFileName);

            if (saved)
            {
                _logger.LogInformation("Report exported successfully");
            }
            else
            {
                _logger.LogInformation("Report export cancelled by user");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            HasError = true;
            ErrorMessage = $"Failed to export report: {ex.Message}";
        }
    }
}
