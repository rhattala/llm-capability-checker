using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Provides intelligent upgrade recommendations based on system bottlenecks
/// </summary>
public class UpgradeAdvisorService : IUpgradeAdvisorService
{
    private readonly ILogger<UpgradeAdvisorService> _logger;

    public UpgradeAdvisorService(ILogger<UpgradeAdvisorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates upgrade recommendations based on hardware and scores
    /// Priority: GPU > RAM > Storage > CPU for LLM workloads
    /// </summary>
    public Task<List<UpgradeRecommendation>> GetUpgradeRecommendationsAsync(HardwareInfo hardware, SystemScores scores)
    {
        try
        {
            _logger.LogInformation("Generating upgrade recommendations");

            var recommendations = new List<UpgradeRecommendation>();

            // Analyze bottlenecks from scores
            var bottlenecks = AnalyzeBottlenecks(scores.Breakdown);

            // Generate recommendations for each significant bottleneck
            // Priority order: GPU (most critical), RAM, Storage, CPU
            var primaryBottleneck = bottlenecks.First();
            var secondaryBottleneck = bottlenecks.Skip(1).FirstOrDefault();

            // Always recommend GPU upgrades if VRAM is limiting
            if (hardware.Gpu.VramGB < 12 || scores.Breakdown.GpuScore < 60)
            {
                recommendations.AddRange(GenerateGpuUpgrades(hardware, primaryBottleneck == "GPU"));
            }

            // RAM upgrades for systems with < 32GB
            if (hardware.Memory.TotalGB < 32 || scores.Breakdown.MemoryScore < 60)
            {
                recommendations.AddRange(GenerateRamUpgrades(hardware, primaryBottleneck == "Memory"));
            }

            // Storage upgrades for HDD or slow SSDs
            if (!hardware.Storage.Type.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
                scores.Breakdown.StorageScore < 60)
            {
                recommendations.AddRange(GenerateStorageUpgrades(hardware, primaryBottleneck == "Storage"));
            }

            // CPU upgrades for older/slower CPUs
            if (hardware.Cpu.Cores < 8 || scores.Breakdown.CpuScore < 50)
            {
                recommendations.AddRange(GenerateCpuUpgrades(hardware, primaryBottleneck == "CPU"));
            }

            // Framework recommendations
            if (!hardware.Frameworks.HasCuda && hardware.Gpu.Vendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
            {
                recommendations.Add(GenerateCudaRecommendation());
            }

            // Sort by priority and score improvement, take top 3-5
            var finalRecommendations = recommendations
                .OrderByDescending(r => GetPriorityWeight(r.PriorityLevel))
                .ThenByDescending(r => r.ScoreImprovement)
                .Take(5)
                .ToList();

            _logger.LogInformation("Generated {Count} upgrade recommendations", finalRecommendations.Count);

            return Task.FromResult(finalRecommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upgrade recommendations");
            return Task.FromResult(new List<UpgradeRecommendation>());
        }
    }

    /// <summary>
    /// Analyzes component scores to identify bottlenecks in priority order
    /// </summary>
    private List<string> AnalyzeBottlenecks(ScoreBreakdown breakdown)
    {
        var components = new Dictionary<string, int>
        {
            { "GPU", breakdown.GpuScore },
            { "Memory", breakdown.MemoryScore },
            { "Storage", breakdown.StorageScore },
            { "CPU", breakdown.CpuScore },
            { "Frameworks", breakdown.FrameworkScore }
        };

        // Return components ordered by score (lowest first = biggest bottleneck)
        return components
            .OrderBy(x => x.Value)
            .Select(x => x.Key)
            .ToList();
    }

    /// <summary>
    /// Generates GPU upgrade recommendations prioritized for LLM workloads
    /// </summary>
    private List<UpgradeRecommendation> GenerateGpuUpgrades(HardwareInfo hardware, bool isPrimaryBottleneck)
    {
        var recommendations = new List<UpgradeRecommendation>();
        var currentVram = hardware.Gpu.VramGB;
        var priority = isPrimaryBottleneck ? "High" : "Medium";

        // Budget option: RTX 4060 Ti 16GB
        if (currentVram < 16)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "GPU",
                CurrentSpecs = $"{hardware.Gpu.Model} ({currentVram}GB VRAM)",
                RecommendedSpecs = "16GB VRAM GPU",
                SpecificProduct = "NVIDIA RTX 4060 Ti 16GB",
                ImpactDescription = "Run 13B models smoothly, enable 7B models unquantized",
                PriorityLevel = priority,
                EstimatedCost = 500,
                ScoreImprovement = 25,
                Reason = "16GB VRAM is the sweet spot for running most popular models efficiently"
            });
        }

        // Mid-range option: RTX 4070 Ti / RTX 4070 Ti SUPER
        if (currentVram < 12)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "GPU",
                CurrentSpecs = $"{hardware.Gpu.Model} ({currentVram}GB VRAM)",
                RecommendedSpecs = "12GB VRAM GPU (faster)",
                SpecificProduct = "NVIDIA RTX 4070 Ti 12GB",
                ImpactDescription = "Excellent performance for 7B-13B models with high throughput",
                PriorityLevel = priority,
                EstimatedCost = 800,
                ScoreImprovement = 30,
                Reason = "More powerful GPU architecture provides faster inference speeds"
            });
        }

        // High-end option: RTX 4090 / RTX 4080
        if (currentVram < 24)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "GPU",
                CurrentSpecs = $"{hardware.Gpu.Model} ({currentVram}GB VRAM)",
                RecommendedSpecs = "24GB VRAM GPU",
                SpecificProduct = "NVIDIA RTX 4090 24GB",
                ImpactDescription = "Run 34B models, multiple 7B-13B models simultaneously, or fine-tune models",
                PriorityLevel = isPrimaryBottleneck ? "High" : "Low",
                EstimatedCost = 1600,
                ScoreImprovement = 45,
                Reason = "24GB enables large model inference and fine-tuning capabilities"
            });
        }

        return recommendations;
    }

    /// <summary>
    /// Generates RAM upgrade recommendations
    /// </summary>
    private List<UpgradeRecommendation> GenerateRamUpgrades(HardwareInfo hardware, bool isPrimaryBottleneck)
    {
        var recommendations = new List<UpgradeRecommendation>();
        var currentRam = hardware.Memory.TotalGB;
        var priority = isPrimaryBottleneck ? "High" : "Medium";

        // 32GB recommendation
        if (currentRam < 32)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "RAM",
                CurrentSpecs = $"{currentRam}GB {hardware.Memory.Type}",
                RecommendedSpecs = "32GB RAM",
                SpecificProduct = "32GB DDR4/DDR5 (2x16GB kit)",
                ImpactDescription = "Enable larger context windows, run multiple models, smoother multitasking",
                PriorityLevel = priority,
                EstimatedCost = 100,
                ScoreImprovement = 20,
                Reason = "32GB is recommended for comfortable LLM usage with system overhead"
            });
        }

        // 64GB recommendation for power users
        if (currentRam < 64 && hardware.Gpu.VramGB >= 12)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "RAM",
                CurrentSpecs = $"{currentRam}GB {hardware.Memory.Type}",
                RecommendedSpecs = "64GB RAM",
                SpecificProduct = "64GB DDR4/DDR5 (2x32GB kit)",
                ImpactDescription = "CPU fallback for large models, extended context windows, professional workflows",
                PriorityLevel = "Low",
                EstimatedCost = 200,
                ScoreImprovement = 15,
                Reason = "64GB enables CPU inference fallback and extreme multitasking"
            });
        }

        return recommendations;
    }

    /// <summary>
    /// Generates storage upgrade recommendations
    /// </summary>
    private List<UpgradeRecommendation> GenerateStorageUpgrades(HardwareInfo hardware, bool isPrimaryBottleneck)
    {
        var recommendations = new List<UpgradeRecommendation>();
        var priority = isPrimaryBottleneck ? "Medium" : "Low";

        // NVMe upgrade if on HDD or SATA SSD
        if (!hardware.Storage.Type.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "Storage",
                CurrentSpecs = $"{hardware.Storage.Type} ({hardware.Storage.ReadSpeedMBps}MB/s)",
                RecommendedSpecs = "NVMe SSD (3500+ MB/s)",
                SpecificProduct = "1TB NVMe Gen4 SSD (Samsung 990 Pro, WD Black SN850X)",
                ImpactDescription = "Faster model loading, reduced startup times, smoother experience",
                PriorityLevel = priority,
                EstimatedCost = 100,
                ScoreImprovement = 15,
                Reason = "NVMe significantly reduces model loading times for large LLMs"
            });
        }

        // More space if running low
        if (hardware.Storage.AvailableGB < 200)
        {
            recommendations.Add(new UpgradeRecommendation
            {
                Component = "Storage",
                CurrentSpecs = $"{hardware.Storage.AvailableGB}GB available",
                RecommendedSpecs = "Additional 500GB+ storage",
                SpecificProduct = "2TB NVMe SSD",
                ImpactDescription = "Store more models locally, avoid constant downloading",
                PriorityLevel = "Low",
                EstimatedCost = 150,
                ScoreImprovement = 5,
                Reason = "More storage allows keeping multiple models ready for use"
            });
        }

        return recommendations;
    }

    /// <summary>
    /// Generates CPU upgrade recommendations
    /// </summary>
    private List<UpgradeRecommendation> GenerateCpuUpgrades(HardwareInfo hardware, bool isPrimaryBottleneck)
    {
        var recommendations = new List<UpgradeRecommendation>();

        // Only recommend CPU upgrades if it's truly limiting or very old
        if (hardware.Cpu.Cores < 8 || hardware.Cpu.BaseClockGHz < 3.0)
        {
            var priority = isPrimaryBottleneck ? "Medium" : "Low";

            recommendations.Add(new UpgradeRecommendation
            {
                Component = "CPU",
                CurrentSpecs = $"{hardware.Cpu.Model} ({hardware.Cpu.Cores} cores, {hardware.Cpu.BaseClockGHz:F1}GHz)",
                RecommendedSpecs = "8+ cores, 3.5GHz+, AVX2/AVX-512 support",
                SpecificProduct = "AMD Ryzen 7 7700X or Intel i7-13700K",
                ImpactDescription = "Better prompt processing, faster CPU inference fallback, improved multitasking",
                PriorityLevel = priority,
                EstimatedCost = 350,
                ScoreImprovement = 15,
                Reason = "Modern CPU improves prompt processing and enables faster CPU-based inference when needed"
            });
        }

        return recommendations;
    }

    /// <summary>
    /// Generates CUDA installation recommendation
    /// </summary>
    private UpgradeRecommendation GenerateCudaRecommendation()
    {
        return new UpgradeRecommendation
        {
            Component = "Software",
            CurrentSpecs = "CUDA not detected",
            RecommendedSpecs = "CUDA Toolkit installed",
            SpecificProduct = "NVIDIA CUDA Toolkit 12.x",
            ImpactDescription = "Enable GPU acceleration for massive performance improvements",
            PriorityLevel = "High",
            EstimatedCost = 0,
            ScoreImprovement = 40,
            Reason = "CUDA is free and provides up to 10x faster inference on NVIDIA GPUs"
        };
    }

    /// <summary>
    /// Gets numeric weight for priority levels
    /// </summary>
    private int GetPriorityWeight(string priority)
    {
        return priority switch
        {
            "High" => 3,
            "Medium" => 2,
            "Low" => 1,
            _ => 0
        };
    }
}
