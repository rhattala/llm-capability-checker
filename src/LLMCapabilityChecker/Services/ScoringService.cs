using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Implements system scoring for LLM workloads based on hardware capabilities
/// </summary>
public class ScoringService : IScoringService
{
    private readonly ILogger<ScoringService> _logger;

    public ScoringService(ILogger<ScoringService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates comprehensive system scores based on hardware capabilities
    /// </summary>
    public Task<SystemScores> CalculateScoresAsync(HardwareInfo hardware)
    {
        _logger.LogInformation("Calculating system scores");

        var scores = new SystemScores
        {
            Breakdown = new ScoreBreakdown
            {
                CpuScore = CalculateCpuScore(hardware.Cpu),
                MemoryScore = CalculateMemoryScore(hardware.Memory),
                GpuScore = CalculateGpuScore(hardware.Gpu),
                StorageScore = CalculateStorageScore(hardware.Storage),
                FrameworkScore = CalculateFrameworkScore(hardware.Frameworks)
            }
        };

        // Calculate INFERENCE SCORE (current logic)
        // GPU is most important for LLMs (40%), then RAM (30%), CPU (15%), Storage (10%), Frameworks (5%)
        scores.InferenceScore = (int)Math.Round(
            scores.Breakdown.GpuScore * 0.40 +
            scores.Breakdown.MemoryScore * 0.30 +
            scores.Breakdown.CpuScore * 0.15 +
            scores.Breakdown.StorageScore * 0.10 +
            scores.Breakdown.FrameworkScore * 0.05
        );
        scores.OverallScore = scores.InferenceScore; // Keep for backward compatibility

        // Calculate TRAINING SCORE (GPU and RAM heavy)
        scores.TrainingScore = (int)Math.Round(
            scores.Breakdown.GpuScore * 0.50 +
            scores.Breakdown.MemoryScore * 0.35 +
            scores.Breakdown.StorageScore * 0.10 +
            scores.Breakdown.CpuScore * 0.05
        );

        // Calculate FINE-TUNING SCORE (LoRA/QLoRA - balanced)
        scores.FineTuningScore = (int)Math.Round(
            scores.Breakdown.GpuScore * 0.45 +
            scores.Breakdown.MemoryScore * 0.30 +
            scores.Breakdown.StorageScore * 0.15 +
            scores.Breakdown.CpuScore * 0.10
        );

        // Add capability descriptions based on scores and VRAM
        var vram = hardware.Gpu?.VramGB ?? 0;

        // Inference capabilities
        if (scores.InferenceScore >= 80)
            scores.InferenceCapability = "Run 13B-34B models smoothly";
        else if (scores.InferenceScore >= 60)
            scores.InferenceCapability = "Run 7B-13B models efficiently";
        else if (scores.InferenceScore >= 40)
            scores.InferenceCapability = "Run 3B-7B models";
        else
            scores.InferenceCapability = "Limited to small models (<3B)";

        // Training capabilities (based on VRAM)
        if (vram >= 40)
            scores.TrainingCapability = "Full fine-tuning of 13B models";
        else if (vram >= 24)
            scores.TrainingCapability = "Full fine-tuning of 7B models";
        else if (vram >= 16)
            scores.TrainingCapability = "Full fine-tuning of 3B models";
        else if (vram >= 12)
            scores.TrainingCapability = "Full fine-tuning of 1B models";
        else
            scores.TrainingCapability = "Training not recommended";

        // Fine-tuning capabilities (LoRA/QLoRA)
        if (vram >= 16)
            scores.FineTuningCapability = "LoRA fine-tune 13B-34B models";
        else if (vram >= 12)
            scores.FineTuningCapability = "LoRA fine-tune 7B-13B models";
        else if (vram >= 8)
            scores.FineTuningCapability = "LoRA fine-tune 3B-7B models";
        else if (vram >= 6)
            scores.FineTuningCapability = "QLoRA fine-tune small models";
        else
            scores.FineTuningCapability = "Fine-tuning limited";

        // Determine system tier based on average of all three scores
        int averageScore = (scores.InferenceScore + scores.TrainingScore + scores.FineTuningScore) / 3;
        scores.SystemTier = DetermineSystemTier(averageScore);

        // Determine recommended model size
        scores.RecommendedModelSize = DetermineRecommendedModelSize(hardware, scores);

        // Identify primary bottleneck
        scores.PrimaryBottleneck = IdentifyBottleneck(scores.Breakdown);

        _logger.LogInformation("System scoring complete: Inference={Inference}, Training={Training}, FineTuning={FineTuning}, Tier={Tier}",
            scores.InferenceScore, scores.TrainingScore, scores.FineTuningScore, scores.SystemTier);

        return Task.FromResult(scores);
    }

    /// <summary>
    /// Calculates CPU score (0-100) based on cores, speed, and architecture
    /// </summary>
    private int CalculateCpuScore(CpuInfo cpu)
    {
        int score = 0;

        // Core count scoring (0-40 points)
        // 4 cores = 10, 8 cores = 25, 16 cores = 35, 24+ cores = 40
        if (cpu.Cores >= 24)
            score += 40;
        else if (cpu.Cores >= 16)
            score += 35;
        else if (cpu.Cores >= 12)
            score += 30;
        else if (cpu.Cores >= 8)
            score += 25;
        else if (cpu.Cores >= 6)
            score += 18;
        else if (cpu.Cores >= 4)
            score += 10;
        else
            score += Math.Max(0, cpu.Cores * 2); // 2 points per core for < 4 cores

        // Clock speed scoring (0-30 points)
        // 2.0 GHz = 10, 3.0 GHz = 20, 4.0+ GHz = 30
        if (cpu.BaseClockGHz >= 4.0)
            score += 30;
        else if (cpu.BaseClockGHz >= 3.5)
            score += 25;
        else if (cpu.BaseClockGHz >= 3.0)
            score += 20;
        else if (cpu.BaseClockGHz >= 2.5)
            score += 15;
        else if (cpu.BaseClockGHz >= 2.0)
            score += 10;
        else if (cpu.BaseClockGHz > 0)
            score += (int)(cpu.BaseClockGHz * 5); // 5 points per GHz for slow CPUs

        // Architecture/instruction set scoring (0-30 points)
        // Modern CPUs with AVX-512: 30 points
        // CPUs with AVX2: 20 points
        // x64 architecture: 10 points
        if (cpu.SupportsAvx512)
            score += 30;
        else if (cpu.SupportsAvx2)
            score += 20;
        else if (cpu.Architecture.Contains("64", StringComparison.OrdinalIgnoreCase))
            score += 10;

        return Math.Min(100, score);
    }

    /// <summary>
    /// Calculates Memory score (0-100) based on amount, type, and speed
    /// </summary>
    private int CalculateMemoryScore(MemoryInfo memory)
    {
        int score = 0;

        // RAM amount scoring (0-60 points) - most critical
        // 8GB = 20, 16GB = 35, 32GB = 50, 64GB+ = 60
        if (memory.TotalGB >= 64)
            score += 60;
        else if (memory.TotalGB >= 48)
            score += 55;
        else if (memory.TotalGB >= 32)
            score += 50;
        else if (memory.TotalGB >= 24)
            score += 42;
        else if (memory.TotalGB >= 16)
            score += 35;
        else if (memory.TotalGB >= 12)
            score += 27;
        else if (memory.TotalGB >= 8)
            score += 20;
        else if (memory.TotalGB >= 4)
            score += 10;
        else
            score += memory.TotalGB * 2;

        // Memory type scoring (0-20 points)
        if (memory.Type.Contains("DDR5", StringComparison.OrdinalIgnoreCase))
            score += 20;
        else if (memory.Type.Contains("DDR4", StringComparison.OrdinalIgnoreCase))
            score += 15;
        else if (memory.Type.Contains("DDR3", StringComparison.OrdinalIgnoreCase))
            score += 8;
        else if (!string.IsNullOrEmpty(memory.Type) && memory.Type != "Unknown")
            score += 5;

        // Memory speed scoring (0-20 points)
        // 2400 MHz = 10, 3200 MHz = 15, 4800+ MHz = 20
        if (memory.SpeedMHz >= 4800)
            score += 20;
        else if (memory.SpeedMHz >= 4000)
            score += 18;
        else if (memory.SpeedMHz >= 3600)
            score += 16;
        else if (memory.SpeedMHz >= 3200)
            score += 15;
        else if (memory.SpeedMHz >= 2666)
            score += 12;
        else if (memory.SpeedMHz >= 2400)
            score += 10;
        else if (memory.SpeedMHz >= 2133)
            score += 7;
        else if (memory.SpeedMHz > 0)
            score += 5;

        return Math.Min(100, score);
    }

    /// <summary>
    /// Calculates GPU score (0-100) based on VRAM and compute capability
    /// </summary>
    private int CalculateGpuScore(GpuInfo gpu)
    {
        int score = 0;

        // Check if GPU is dedicated and capable
        if (!gpu.IsDedicated || gpu.VramGB < 2)
        {
            // Integrated/weak GPU - minimal score
            return gpu.IsDedicated ? 10 : 5;
        }

        // VRAM scoring (0-60 points) - critical for LLMs
        // 4GB = 15, 8GB = 30, 12GB = 40, 16GB = 50, 24GB+ = 60
        if (gpu.VramGB >= 24)
            score += 60;
        else if (gpu.VramGB >= 20)
            score += 55;
        else if (gpu.VramGB >= 16)
            score += 50;
        else if (gpu.VramGB >= 12)
            score += 40;
        else if (gpu.VramGB >= 10)
            score += 35;
        else if (gpu.VramGB >= 8)
            score += 30;
        else if (gpu.VramGB >= 6)
            score += 22;
        else if (gpu.VramGB >= 4)
            score += 15;
        else
            score += gpu.VramGB * 3;

        // Compute capability/architecture scoring (0-25 points)
        // Parse NVIDIA compute capability (e.g., "8.6" for RTX 30 series)
        if (!string.IsNullOrEmpty(gpu.ComputeCapability))
        {
            if (double.TryParse(gpu.ComputeCapability, out double capability))
            {
                if (capability >= 8.9) // Ada Lovelace (RTX 40 series)
                    score += 25;
                else if (capability >= 8.6) // Ampere (RTX 30 series)
                    score += 23;
                else if (capability >= 8.0) // Ampere (A100)
                    score += 22;
                else if (capability >= 7.5) // Turing (RTX 20 series)
                    score += 20;
                else if (capability >= 7.0) // Volta
                    score += 18;
                else if (capability >= 6.0) // Pascal
                    score += 15;
                else if (capability >= 5.0) // Maxwell
                    score += 10;
                else
                    score += 5;
            }
        }
        else if (gpu.Architecture.Contains("RDNA", StringComparison.OrdinalIgnoreCase))
        {
            // AMD RDNA architecture
            if (gpu.Architecture.Contains("RDNA3", StringComparison.OrdinalIgnoreCase))
                score += 24;
            else if (gpu.Architecture.Contains("RDNA2", StringComparison.OrdinalIgnoreCase))
                score += 20;
            else
                score += 15;
        }
        else if (!string.IsNullOrEmpty(gpu.Architecture) && gpu.Architecture != "Unknown")
        {
            score += 10; // Some architecture info is better than none
        }

        // Precision support scoring (0-15 points)
        if (gpu.SupportsInt8 && gpu.SupportsFp16)
            score += 15;
        else if (gpu.SupportsFp16)
            score += 10;
        else if (gpu.SupportsInt8)
            score += 8;

        return Math.Min(100, score);
    }

    /// <summary>
    /// Calculates Storage score (0-100) based on type, speed, and available space
    /// </summary>
    private int CalculateStorageScore(StorageInfo storage)
    {
        int score = 0;

        // Storage type scoring (0-40 points)
        if (storage.Type.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
            score += 40;
        else if (storage.Type.Contains("SSD", StringComparison.OrdinalIgnoreCase))
            score += 30;
        else if (storage.Type.Contains("HDD", StringComparison.OrdinalIgnoreCase))
            score += 10;
        else
            score += 15; // Unknown, assume SSD

        // Read speed scoring (0-30 points)
        // 500 MB/s = 15, 2000 MB/s = 25, 5000+ MB/s = 30
        if (storage.ReadSpeedMBps >= 5000)
            score += 30;
        else if (storage.ReadSpeedMBps >= 3500)
            score += 27;
        else if (storage.ReadSpeedMBps >= 2000)
            score += 25;
        else if (storage.ReadSpeedMBps >= 1000)
            score += 20;
        else if (storage.ReadSpeedMBps >= 500)
            score += 15;
        else if (storage.ReadSpeedMBps >= 100)
            score += 8; // HDD range
        else if (storage.ReadSpeedMBps > 0)
            score += 5;

        // Available space scoring (0-30 points)
        // 100GB = 10, 250GB = 20, 500GB+ = 30
        if (storage.AvailableGB >= 500)
            score += 30;
        else if (storage.AvailableGB >= 350)
            score += 25;
        else if (storage.AvailableGB >= 250)
            score += 20;
        else if (storage.AvailableGB >= 150)
            score += 15;
        else if (storage.AvailableGB >= 100)
            score += 10;
        else if (storage.AvailableGB >= 50)
            score += 5;
        else
            score += 0; // Very low space is concerning

        return Math.Min(100, score);
    }

    /// <summary>
    /// Calculates Framework score (0-100) based on available ML frameworks
    /// </summary>
    private int CalculateFrameworkScore(FrameworkInfo frameworks)
    {
        int score = 40; // Base score for being able to run CPU inference

        // CUDA scoring (0-40 points) - most important for NVIDIA GPUs
        if (frameworks.HasCuda)
        {
            score += 35;

            // Bonus for newer CUDA versions
            if (!string.IsNullOrEmpty(frameworks.CudaVersion))
            {
                if (frameworks.CudaVersion.StartsWith("12"))
                    score += 5;
                else if (frameworks.CudaVersion.StartsWith("11"))
                    score += 3;
            }
        }

        // ROCm scoring (0-30 points) - important for AMD GPUs
        if (frameworks.HasRocm)
        {
            score += 25;

            if (!string.IsNullOrEmpty(frameworks.RocmVersion))
                score += 5;
        }

        // Metal scoring (0-20 points) - for macOS
        if (frameworks.HasMetal)
            score += 20;

        // DirectML scoring (0-15 points) - for Windows
        if (frameworks.HasDirectMl)
            score += 15;

        // OpenVINO scoring (0-10 points) - for Intel
        if (frameworks.HasOpenVino)
            score += 10;

        return Math.Min(100, score);
    }

    /// <summary>
    /// Determines system tier classification based on overall score
    /// </summary>
    private string DetermineSystemTier(int overallScore)
    {
        return overallScore switch
        {
            >= 80 => "Enthusiast",
            >= 65 => "High-End",
            >= 50 => "Mid-Range",
            >= 35 => "Entry-Level",
            _ => "Limited"
        };
    }

    /// <summary>
    /// Determines recommended model size based on hardware capabilities
    /// </summary>
    private string DetermineRecommendedModelSize(HardwareInfo hardware, SystemScores scores)
    {
        // Primary consideration: Available VRAM and RAM
        int totalMemory = hardware.Gpu.VramGB + hardware.Memory.TotalGB;
        bool hasGpu = hardware.Gpu.IsDedicated && hardware.Gpu.VramGB >= 4;

        // Model size recommendations based on memory
        // Rough guide: 7B needs ~8GB, 13B needs ~16GB, 34B needs ~40GB, 70B needs ~80GB (4-bit quantized)
        if (hasGpu)
        {
            // GPU inference - can use larger models
            if (hardware.Gpu.VramGB >= 48)
                return "70B+ (or 34B unquantized)";
            else if (hardware.Gpu.VramGB >= 24)
                return "34B (or 13B unquantized)";
            else if (hardware.Gpu.VramGB >= 16)
                return "13B (or 7B unquantized)";
            else if (hardware.Gpu.VramGB >= 10)
                return "13B (quantized)";
            else if (hardware.Gpu.VramGB >= 6)
                return "7B";
            else
                return "3B or smaller";
        }
        else
        {
            // CPU-only inference - need more system RAM
            if (hardware.Memory.TotalGB >= 64)
                return "34B (CPU, quantized)";
            else if (hardware.Memory.TotalGB >= 32)
                return "13B (CPU, quantized)";
            else if (hardware.Memory.TotalGB >= 16)
                return "7B (CPU, quantized)";
            else if (hardware.Memory.TotalGB >= 8)
                return "3B (CPU, quantized)";
            else
                return "1B or smaller";
        }
    }

    /// <summary>
    /// Identifies the primary bottleneck in the system
    /// </summary>
    private string IdentifyBottleneck(ScoreBreakdown breakdown)
    {
        var scores = new Dictionary<string, int>
        {
            { "GPU", breakdown.GpuScore },
            { "Memory", breakdown.MemoryScore },
            { "CPU", breakdown.CpuScore },
            { "Storage", breakdown.StorageScore },
            { "Frameworks", breakdown.FrameworkScore }
        };

        // Find the lowest scoring component
        var bottleneck = scores.OrderBy(x => x.Value).First();

        return $"{bottleneck.Key} ({bottleneck.Value}/100)";
    }
}
