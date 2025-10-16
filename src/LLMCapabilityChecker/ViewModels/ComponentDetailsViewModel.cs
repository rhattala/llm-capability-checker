using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for detailed component view
/// Shows specs, bottleneck analysis, and upgrade options
/// </summary>
public partial class ComponentDetailsViewModel : ViewModelBase
{
    private readonly IUpgradeAdvisorService _upgradeService;

    [ObservableProperty]
    private ComponentType _componentType;

    [ObservableProperty]
    private HardwareInfo? _hardwareInfo;

    [ObservableProperty]
    private SystemScores? _systemScores;

    [ObservableProperty]
    private ObservableCollection<UpgradeRecommendation> _upgradeOptions = new();

    public string ComponentTitle => ComponentType switch
    {
        ComponentType.CPU => "CPU Details",
        ComponentType.GPU => "GPU Details",
        ComponentType.RAM => "RAM Details",
        ComponentType.Storage => "Storage Details",
        ComponentType.Frameworks => "Frameworks Details",
        _ => "Component Details"
    };

    public string ComponentName => ComponentType switch
    {
        ComponentType.CPU => "CPU",
        ComponentType.GPU => "GPU",
        ComponentType.RAM => "Memory",
        ComponentType.Storage => "Storage",
        ComponentType.Frameworks => "ML Frameworks",
        _ => "Component"
    };

    public int ComponentScore => ComponentType switch
    {
        ComponentType.CPU => SystemScores?.Breakdown.CpuScore ?? 0,
        ComponentType.GPU => SystemScores?.Breakdown.GpuScore ?? 0,
        ComponentType.RAM => SystemScores?.Breakdown.MemoryScore ?? 0,
        ComponentType.Storage => SystemScores?.Breakdown.StorageScore ?? 0,
        ComponentType.Frameworks => SystemScores?.Breakdown.FrameworkScore ?? 0,
        _ => 0
    };

    public string ScoreColor => ComponentScore >= 80 ? "#4CAF50" : ComponentScore >= 60 ? "#FFA726" : "#EF5350";

    public string ScoreDescription => ComponentScore switch
    {
        >= 90 => "Excellent - Top tier performance",
        >= 80 => "Great - Well optimized for LLM tasks",
        >= 70 => "Good - Capable but with some limitations",
        >= 60 => "Fair - Consider upgrades for better performance",
        >= 40 => "Below Average - Upgrade recommended",
        _ => "Poor - Upgrade strongly recommended"
    };

    // Detailed specifications
    public ObservableCollection<SpecificationItem> Specifications { get; } = new();

    // Bottleneck analysis
    public string BottleneckAnalysis => GetBottleneckAnalysis();
    public bool IsBottleneck => GetIsBottleneck();

    // Navigation action - set by MainWindowViewModel
    public Action? NavigateBack { get; set; }

    public ComponentDetailsViewModel(IUpgradeAdvisorService upgradeService)
    {
        _upgradeService = upgradeService;
    }

    /// <summary>
    /// Navigates back to dashboard
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        NavigateBack?.Invoke();
    }

    /// <summary>
    /// Initializes the view with component data
    /// </summary>
    public async Task InitializeAsync(ComponentType type, HardwareInfo hardware, SystemScores scores)
    {
        ComponentType = type;
        HardwareInfo = hardware;
        SystemScores = scores;

        LoadSpecifications();
        await LoadUpgradeOptionsAsync();

        // Notify UI of changes
        OnPropertyChanged(nameof(ComponentTitle));
        OnPropertyChanged(nameof(ComponentName));
        OnPropertyChanged(nameof(ComponentScore));
        OnPropertyChanged(nameof(ScoreColor));
        OnPropertyChanged(nameof(ScoreDescription));
        OnPropertyChanged(nameof(BottleneckAnalysis));
        OnPropertyChanged(nameof(IsBottleneck));
    }

    private void LoadSpecifications()
    {
        Specifications.Clear();

        switch (ComponentType)
        {
            case ComponentType.CPU:
                Specifications.Add(new SpecificationItem("Model", HardwareInfo?.Cpu.Model ?? "Unknown"));
                Specifications.Add(new SpecificationItem("Cores", $"{HardwareInfo?.Cpu.Cores ?? 0}"));
                Specifications.Add(new SpecificationItem("Threads", $"{HardwareInfo?.Cpu.Threads ?? 0}"));
                Specifications.Add(new SpecificationItem("Base Clock", $"{HardwareInfo?.Cpu.BaseClockGHz:F2} GHz"));
                Specifications.Add(new SpecificationItem("Architecture", HardwareInfo?.Cpu.Architecture ?? "Unknown"));
                Specifications.Add(new SpecificationItem("AVX2 Support", HardwareInfo?.Cpu.SupportsAvx2 == true ? "Yes" : "No"));
                Specifications.Add(new SpecificationItem("AVX-512 Support", HardwareInfo?.Cpu.SupportsAvx512 == true ? "Yes" : "No"));
                break;

            case ComponentType.GPU:
                Specifications.Add(new SpecificationItem("Model", HardwareInfo?.Gpu.Model ?? "Unknown"));
                Specifications.Add(new SpecificationItem("Vendor", HardwareInfo?.Gpu.Vendor ?? "Unknown"));
                Specifications.Add(new SpecificationItem("VRAM", $"{HardwareInfo?.Gpu.VramGB ?? 0} GB"));
                Specifications.Add(new SpecificationItem("Architecture", HardwareInfo?.Gpu.Architecture ?? "Unknown"));
                Specifications.Add(new SpecificationItem("Compute Capability", HardwareInfo?.Gpu.ComputeCapability ?? "N/A"));
                Specifications.Add(new SpecificationItem("Type", HardwareInfo?.Gpu.IsDedicated == true ? "Dedicated" : "Integrated"));
                Specifications.Add(new SpecificationItem("FP16 Support", HardwareInfo?.Gpu.SupportsFp16 == true ? "Yes" : "No"));
                Specifications.Add(new SpecificationItem("INT8 Support", HardwareInfo?.Gpu.SupportsInt8 == true ? "Yes" : "No"));
                break;

            case ComponentType.RAM:
                Specifications.Add(new SpecificationItem("Total Memory", $"{HardwareInfo?.Memory.TotalGB ?? 0} GB"));
                Specifications.Add(new SpecificationItem("Available Memory", $"{HardwareInfo?.Memory.AvailableGB ?? 0} GB"));
                Specifications.Add(new SpecificationItem("Type", HardwareInfo?.Memory.Type ?? "Unknown"));
                Specifications.Add(new SpecificationItem("Speed", $"{HardwareInfo?.Memory.SpeedMHz ?? 0} MHz"));
                Specifications.Add(new SpecificationItem("Usage", $"{GetMemoryUsagePercent()}%"));
                break;

            case ComponentType.Storage:
                Specifications.Add(new SpecificationItem("Type", HardwareInfo?.Storage.Type ?? "Unknown"));
                Specifications.Add(new SpecificationItem("Total Capacity", $"{HardwareInfo?.Storage.TotalGB ?? 0} GB"));
                Specifications.Add(new SpecificationItem("Available Space", $"{HardwareInfo?.Storage.AvailableGB ?? 0} GB"));
                Specifications.Add(new SpecificationItem("Read Speed", $"{HardwareInfo?.Storage.ReadSpeedMBps ?? 0} MB/s"));
                Specifications.Add(new SpecificationItem("Write Speed", $"{HardwareInfo?.Storage.WriteSpeedMBps ?? 0} MB/s"));
                Specifications.Add(new SpecificationItem("Usage", $"{GetStorageUsagePercent()}%"));
                break;

            case ComponentType.Frameworks:
                Specifications.Add(new SpecificationItem("CUDA", HardwareInfo?.Frameworks.HasCuda == true ? $"Yes ({HardwareInfo.Frameworks.CudaVersion})" : "No"));
                Specifications.Add(new SpecificationItem("ROCm", HardwareInfo?.Frameworks.HasRocm == true ? $"Yes ({HardwareInfo.Frameworks.RocmVersion})" : "No"));
                Specifications.Add(new SpecificationItem("DirectML", HardwareInfo?.Frameworks.HasDirectMl == true ? "Yes" : "No"));
                Specifications.Add(new SpecificationItem("Metal", HardwareInfo?.Frameworks.HasMetal == true ? "Yes" : "No"));
                Specifications.Add(new SpecificationItem("OpenVINO", HardwareInfo?.Frameworks.HasOpenVino == true ? "Yes" : "No"));
                Specifications.Add(new SpecificationItem("GPU Acceleration", GetGpuAccelerationStatus()));
                break;
        }
    }

    private async Task LoadUpgradeOptionsAsync()
    {
        if (HardwareInfo == null || SystemScores == null) return;

        try
        {
            var allRecommendations = await _upgradeService.GetUpgradeRecommendationsAsync(HardwareInfo, SystemScores);

            // Filter to current component and take top 3
            var componentRecommendations = allRecommendations
                .Where(r => r.Component.Equals(ComponentName, StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .ToList();

            UpgradeOptions = new ObservableCollection<UpgradeRecommendation>(componentRecommendations);
        }
        catch
        {
            // If upgrade service fails, create basic recommendations
            UpgradeOptions = new ObservableCollection<UpgradeRecommendation>(GetBasicUpgradeRecommendations());
        }
    }

    private List<UpgradeRecommendation> GetBasicUpgradeRecommendations()
    {
        var recommendations = new List<UpgradeRecommendation>();

        switch (ComponentType)
        {
            case ComponentType.CPU:
                if (ComponentScore < 80)
                {
                    recommendations.Add(new UpgradeRecommendation
                    {
                        Component = "CPU",
                        CurrentSpecs = HardwareInfo?.Cpu.Model ?? "Current CPU",
                        RecommendedSpecs = "AMD Ryzen 7 7700X or Intel Core i7-13700K",
                        ImpactDescription = "20-30% faster model loading and inference",
                        PriorityLevel = ComponentScore < 50 ? "High" : "Medium",
                        EstimatedCost = 350,
                        ScoreImprovement = 25,
                        Reason = "Modern 8+ core CPU significantly improves LLM performance"
                    });
                    recommendations.Add(new UpgradeRecommendation
                    {
                        Component = "CPU",
                        CurrentSpecs = HardwareInfo?.Cpu.Model ?? "Current CPU",
                        RecommendedSpecs = "AMD Ryzen 9 7900X or Intel Core i9-13900K",
                        ImpactDescription = "40-50% faster with 12+ cores for parallel processing",
                        PriorityLevel = "Medium",
                        EstimatedCost = 550,
                        ScoreImprovement = 35,
                        Reason = "High-end CPU for professional LLM workloads"
                    });
                }
                break;

            case ComponentType.GPU:
                if (ComponentScore < 80)
                {
                    var vram = HardwareInfo?.Gpu.VramGB ?? 0;
                    if (vram < 12)
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "GPU",
                            CurrentSpecs = HardwareInfo?.Gpu.Model ?? "Current GPU",
                            RecommendedSpecs = "NVIDIA RTX 4070 Ti Super (16GB)",
                            ImpactDescription = "Run 13B quantized models at good speed",
                            PriorityLevel = "High",
                            EstimatedCost = 800,
                            ScoreImprovement = 40,
                            Reason = "16GB VRAM enables running larger models"
                        });
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "GPU",
                            CurrentSpecs = HardwareInfo?.Gpu.Model ?? "Current GPU",
                            RecommendedSpecs = "NVIDIA RTX 4080 (16GB)",
                            ImpactDescription = "Run 13B-30B models with excellent performance",
                            PriorityLevel = "Medium",
                            EstimatedCost = 1200,
                            ScoreImprovement = 50,
                            Reason = "Faster GPU with 16GB VRAM for demanding workloads"
                        });
                    }
                    else
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "GPU",
                            CurrentSpecs = HardwareInfo?.Gpu.Model ?? "Current GPU",
                            RecommendedSpecs = "NVIDIA RTX 4090 (24GB)",
                            ImpactDescription = "Run 34B+ models with top-tier performance",
                            PriorityLevel = "Low",
                            EstimatedCost = 1600,
                            ScoreImprovement = 30,
                            Reason = "Maximum performance for large model inference"
                        });
                    }
                }
                break;

            case ComponentType.RAM:
                if (ComponentScore < 80)
                {
                    var totalRam = HardwareInfo?.Memory.TotalGB ?? 0;
                    if (totalRam < 32)
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "RAM",
                            CurrentSpecs = $"{totalRam} GB",
                            RecommendedSpecs = "32GB DDR4/DDR5 (2x16GB)",
                            ImpactDescription = "Run 7B-13B models without swapping",
                            PriorityLevel = "High",
                            EstimatedCost = 100,
                            ScoreImprovement = 35,
                            Reason = "32GB is minimum for comfortable LLM usage"
                        });
                    }
                    recommendations.Add(new UpgradeRecommendation
                    {
                        Component = "RAM",
                        CurrentSpecs = $"{totalRam} GB",
                        RecommendedSpecs = "64GB DDR4/DDR5 (2x32GB)",
                        ImpactDescription = "Run 30B+ models or multiple models simultaneously",
                        PriorityLevel = totalRam < 32 ? "Medium" : "Low",
                        EstimatedCost = 200,
                        ScoreImprovement = totalRam < 32 ? 45 : 25,
                        Reason = "64GB enables running larger models in CPU mode"
                    });
                }
                break;

            case ComponentType.Storage:
                if (ComponentScore < 80)
                {
                    var storageType = HardwareInfo?.Storage.Type ?? "Unknown";
                    if (storageType.Contains("HDD", StringComparison.OrdinalIgnoreCase))
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "Storage",
                            CurrentSpecs = storageType,
                            RecommendedSpecs = "1TB NVMe SSD (PCIe 4.0)",
                            ImpactDescription = "10x faster model loading (3-5s vs 30-60s)",
                            PriorityLevel = "High",
                            EstimatedCost = 100,
                            ScoreImprovement = 50,
                            Reason = "NVMe dramatically reduces model loading times"
                        });
                    }
                    else
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "Storage",
                            CurrentSpecs = storageType,
                            RecommendedSpecs = "2TB NVMe SSD (PCIe 4.0)",
                            ImpactDescription = "Store multiple large models with fast access",
                            PriorityLevel = "Low",
                            EstimatedCost = 150,
                            ScoreImprovement = 20,
                            Reason = "More capacity for model library"
                        });
                    }
                }
                break;

            case ComponentType.Frameworks:
                if (ComponentScore < 80)
                {
                    var hasCuda = HardwareInfo?.Frameworks.HasCuda ?? false;
                    var vendor = HardwareInfo?.Gpu.Vendor ?? "";

                    if (!hasCuda && vendor.Equals("NVIDIA", StringComparison.OrdinalIgnoreCase))
                    {
                        recommendations.Add(new UpgradeRecommendation
                        {
                            Component = "Frameworks",
                            CurrentSpecs = "No CUDA detected",
                            RecommendedSpecs = "Install CUDA Toolkit 12.x",
                            ImpactDescription = "Enable GPU acceleration for NVIDIA cards",
                            PriorityLevel = "High",
                            EstimatedCost = 0,
                            ScoreImprovement = 50,
                            Reason = "CUDA unlocks GPU acceleration for LLM inference"
                        });
                    }
                }
                break;
        }

        return recommendations;
    }

    private string GetBottleneckAnalysis()
    {
        if (SystemScores == null) return "Analysis unavailable";

        bool isBottleneck = GetIsBottleneck();

        switch (ComponentType)
        {
            case ComponentType.CPU:
                if (isBottleneck)
                    return $"CPU is a bottleneck. With only {HardwareInfo?.Cpu.Cores} cores, model loading and CPU inference will be slow. Upgrade to 8+ cores for better performance.";
                return "CPU performance is adequate. No major bottleneck detected.";

            case ComponentType.GPU:
                if (isBottleneck)
                    return $"GPU is a bottleneck. {HardwareInfo?.Gpu.VramGB}GB VRAM limits model size. Larger VRAM enables bigger models with better quality.";
                return "GPU performance is adequate for recommended model sizes.";

            case ComponentType.RAM:
                if (isBottleneck)
                    return $"RAM is a bottleneck. {HardwareInfo?.Memory.TotalGB}GB limits model size in CPU mode. More RAM = larger models or multiple models.";
                return "RAM capacity is sufficient for recommended model sizes.";

            case ComponentType.Storage:
                if (isBottleneck)
                {
                    if (HardwareInfo?.Storage.Type?.Contains("HDD", StringComparison.OrdinalIgnoreCase) == true)
                        return "Storage is a bottleneck. HDD causes slow model loading (30-60s). NVMe SSD reduces this to 3-5s.";
                    return "Storage speed could be improved. Consider faster NVMe drive.";
                }
                return "Storage performance is adequate for model loading.";

            case ComponentType.Frameworks:
                if (isBottleneck)
                    return "Missing GPU acceleration frameworks. Installing CUDA/ROCm/DirectML can dramatically improve inference speed.";
                return "ML frameworks are properly configured for GPU acceleration.";

            default:
                return "Analysis unavailable";
        }
    }

    private bool GetIsBottleneck()
    {
        if (SystemScores == null) return false;

        var avgScore = (SystemScores.Breakdown.CpuScore +
                       SystemScores.Breakdown.GpuScore +
                       SystemScores.Breakdown.MemoryScore +
                       SystemScores.Breakdown.StorageScore +
                       SystemScores.Breakdown.FrameworkScore) / 5.0;

        return ComponentScore < avgScore - 10;
    }

    private int GetMemoryUsagePercent()
    {
        if (HardwareInfo?.Memory.TotalGB == 0) return 0;
        var used = (HardwareInfo?.Memory.TotalGB ?? 0) - (HardwareInfo?.Memory.AvailableGB ?? 0);
        return (int)((used / (double)(HardwareInfo?.Memory.TotalGB ?? 1)) * 100);
    }

    private int GetStorageUsagePercent()
    {
        if (HardwareInfo?.Storage.TotalGB == 0) return 0;
        var used = (HardwareInfo?.Storage.TotalGB ?? 0) - (HardwareInfo?.Storage.AvailableGB ?? 0);
        return (int)((used / (double)(HardwareInfo?.Storage.TotalGB ?? 1)) * 100);
    }

    private string GetGpuAccelerationStatus()
    {
        if (HardwareInfo?.Frameworks.HasCuda == true) return "CUDA (NVIDIA)";
        if (HardwareInfo?.Frameworks.HasRocm == true) return "ROCm (AMD)";
        if (HardwareInfo?.Frameworks.HasMetal == true) return "Metal (Apple)";
        if (HardwareInfo?.Frameworks.HasDirectMl == true) return "DirectML (Windows)";
        if (HardwareInfo?.Frameworks.HasOpenVino == true) return "OpenVINO (Intel)";
        return "None (CPU only)";
    }
}

/// <summary>
/// Represents a single specification item
/// </summary>
public class SpecificationItem
{
    public string Name { get; set; }
    public string Value { get; set; }

    public SpecificationItem(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
