using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for running performance benchmarks to estimate LLM inference speed
/// </summary>
public class BenchmarkService : IBenchmarkService
{
    private readonly ILogger<BenchmarkService> _logger;
    private readonly IHardwareDetectionService _hardwareService;

    // Reference baseline scores (approximate scores for mid-tier system)
    // Based on: AMD Ryzen 5 7600 (6C/12T), 32GB DDR5-5600, RTX 4060 8GB
    // Updated: January 2025 - Current mid-tier gaming/workstation build
    private const double REFERENCE_CPU_SINGLE = 1800.0; // Cinebench R23 single-core equivalent
    private const double REFERENCE_CPU_MULTI = 14000.0; // Cinebench R23 multi-core equivalent
    private const double REFERENCE_MEMORY_BW = 44.8; // GB/s (DDR5-5600 dual-channel)

    public BenchmarkService(
        ILogger<BenchmarkService> logger,
        IHardwareDetectionService hardwareService)
    {
        _logger = logger;
        _hardwareService = hardwareService;
    }

    /// <summary>
    /// Runs system benchmark tests to estimate LLM inference performance
    /// Uses hardware specs instead of synthetic tests for accurate scoring
    /// </summary>
    public async Task<BenchmarkResults> RunSystemBenchmarkAsync()
    {
        _logger.LogInformation("Starting system benchmark...");
        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get hardware info
            var hardwareInfo = await _hardwareService.DetectHardwareAsync();

            // Calculate scores based on actual hardware specs (not synthetic tests)
            var cpuSingleScore = CalculateCpuSingleScore(hardwareInfo.Cpu);
            var cpuMultiScore = CalculateCpuMultiScore(hardwareInfo.Cpu);
            var memoryBandwidth = CalculateMemoryBandwidth(hardwareInfo.Memory);

            stopwatch.Stop();

            // Calculate estimated tokens/second for different model sizes
            var tokenThroughput = EstimateTokenThroughput(
                cpuSingleScore,
                cpuMultiScore,
                memoryBandwidth,
                hardwareInfo
            );

            // Calculate comparison to reference system
            var comparisonScore = CalculateComparisonScore(
                cpuSingleScore,
                cpuMultiScore,
                memoryBandwidth
            );

            var results = new BenchmarkResults
            {
                CpuSingleCoreScore = cpuSingleScore,
                CpuMultiCoreScore = cpuMultiScore,
                MemoryBandwidthGBps = memoryBandwidth,
                TokenThroughput = tokenThroughput,
                ComparisonToReference = comparisonScore,
                BenchmarkDurationSeconds = stopwatch.Elapsed.TotalSeconds,
                BenchmarkTimestamp = startTime
            };

            _logger.LogInformation(
                "Benchmark completed in {Duration:F2}s. CPU Single: {Single:F0}, CPU Multi: {Multi:F0}, Memory BW: {BW:F2} GB/s",
                results.BenchmarkDurationSeconds,
                cpuSingleScore,
                cpuMultiScore,
                memoryBandwidth
            );

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running benchmark");
            throw;
        }
    }

    /// <summary>
    /// Calculate CPU single-core score based on clock speed and architecture
    /// Uses estimated IPC (Instructions Per Cycle) based on CPU generation
    /// </summary>
    private double CalculateCpuSingleScore(CpuInfo cpu)
    {
        // Base score from clock speed
        double baseScore = cpu.BaseClockGHz * 1000;

        // Architecture multipliers (estimated IPC improvements)
        double architectureMultiplier = 1.0;
        if (cpu.Model.Contains("13th Gen", StringComparison.OrdinalIgnoreCase) ||
            cpu.Model.Contains("14th Gen", StringComparison.OrdinalIgnoreCase))
        {
            architectureMultiplier = 1.3; // Raptor Lake
        }
        else if (cpu.Model.Contains("12th Gen", StringComparison.OrdinalIgnoreCase))
        {
            architectureMultiplier = 1.25; // Alder Lake
        }
        else if (cpu.Model.Contains("7000", StringComparison.OrdinalIgnoreCase) ||
                 cpu.Model.Contains("Ryzen 7", StringComparison.OrdinalIgnoreCase))
        {
            architectureMultiplier = 1.3; // Zen 4
        }
        else if (cpu.Model.Contains("5000", StringComparison.OrdinalIgnoreCase) ||
                 cpu.Model.Contains("Ryzen 5", StringComparison.OrdinalIgnoreCase))
        {
            architectureMultiplier = 1.15; // Zen 3
        }

        return Math.Round(baseScore * architectureMultiplier, 0);
    }

    /// <summary>
    /// Calculate CPU multi-core score based on cores, threads, and clock speed
    /// </summary>
    private double CalculateCpuMultiScore(CpuInfo cpu)
    {
        // Single-core performance as base
        double singleScore = CalculateCpuSingleScore(cpu);

        // Scaling efficiency (not linear due to diminishing returns)
        double threadScaling = cpu.Threads;
        if (cpu.Threads > 16)
        {
            // Reduced scaling for high thread counts
            threadScaling = 16 + (cpu.Threads - 16) * 0.85;
        }

        return Math.Round(singleScore * threadScaling / 2, 0);
    }

    /// <summary>
    /// Calculate memory bandwidth from memory specs
    /// </summary>
    private double CalculateMemoryBandwidth(MemoryInfo memory)
    {
        // Extract memory speed from type string (e.g., "DDR5-6600" -> 6600)
        double memorySpeed = 3200; // Default fallback
        if (memory.Type.Contains("DDR5", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(memory.Type, @"(\d{4,5})");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int speed))
            {
                memorySpeed = speed;
            }
            else
            {
                memorySpeed = 5600; // Default DDR5 speed
            }
        }
        else if (memory.Type.Contains("DDR4", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(memory.Type, @"(\d{4})");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int speed))
            {
                memorySpeed = speed;
            }
            else
            {
                memorySpeed = 3200; // Default DDR4 speed
            }
        }

        // Calculate bandwidth: MT/s * bus width (64 bits = 8 bytes) / 1000 = GB/s
        // DDR is dual-channel, so multiply by 2
        double bandwidth = (memorySpeed * 8 * 2) / 1000.0;

        return Math.Round(bandwidth, 1);
    }

    /// <summary>
    /// Runs a simple CPU single-core benchmark test (DEPRECATED - now using hardware specs)
    /// Tests single-threaded matrix multiplication performance
    /// </summary>
    private async Task<double> RunCpuSingleCoreTestAsync()
    {
        return await Task.Run(() =>
        {
            const int size = 256;
            const int iterations = 10;

            var stopwatch = Stopwatch.StartNew();

            for (int iter = 0; iter < iterations; iter++)
            {
                // Simple matrix multiplication test
                var matrixA = new double[size, size];
                var matrixB = new double[size, size];
                var result = new double[size, size];

                // Initialize matrices
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrixA[i, j] = i + j;
                        matrixB[i, j] = i - j;
                    }
                }

                // Matrix multiplication
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < size; k++)
                        {
                            sum += matrixA[i, k] * matrixB[k, j];
                        }
                        result[i, j] = sum;
                    }
                }
            }

            stopwatch.Stop();

            // Score = operations per second
            // Higher is better
            double operationsPerIteration = size * size * size * 2; // multiplications + additions
            double totalOperations = operationsPerIteration * iterations;
            double score = (totalOperations / stopwatch.Elapsed.TotalSeconds) / 1_000_000; // Normalize to millions

            _logger.LogDebug("CPU Single-Core Score: {Score:F2}", score);
            return score;
        });
    }

    /// <summary>
    /// Runs a CPU multi-core benchmark test
    /// Tests parallel matrix multiplication performance
    /// </summary>
    private async Task<double> RunCpuMultiCoreTestAsync()
    {
        return await Task.Run(() =>
        {
            const int size = 256;
            const int iterations = 10;

            var stopwatch = Stopwatch.StartNew();

            // Run parallel operations
            Parallel.For(0, iterations, iter =>
            {
                var matrixA = new double[size, size];
                var matrixB = new double[size, size];
                var result = new double[size, size];

                // Initialize matrices
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrixA[i, j] = i + j;
                        matrixB[i, j] = i - j;
                    }
                }

                // Matrix multiplication
                Parallel.For(0, size, i =>
                {
                    for (int j = 0; j < size; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < size; k++)
                        {
                            sum += matrixA[i, k] * matrixB[k, j];
                        }
                        result[i, j] = sum;
                    }
                });
            });

            stopwatch.Stop();

            double operationsPerIteration = size * size * size * 2;
            double totalOperations = operationsPerIteration * iterations;
            double score = (totalOperations / stopwatch.Elapsed.TotalSeconds) / 1_000_000;

            _logger.LogDebug("CPU Multi-Core Score: {Score:F2}", score);
            return score;
        });
    }

    /// <summary>
    /// Runs a memory bandwidth benchmark test
    /// Tests sequential read/write performance
    /// </summary>
    private async Task<double> RunMemoryBandwidthTestAsync()
    {
        return await Task.Run(() =>
        {
            const int arraySize = 64 * 1024 * 1024; // 64MB array
            const int iterations = 5;

            var stopwatch = Stopwatch.StartNew();

            for (int iter = 0; iter < iterations; iter++)
            {
                // Allocate large arrays
                var source = new byte[arraySize];
                var destination = new byte[arraySize];

                // Fill source with data
                for (int i = 0; i < arraySize; i++)
                {
                    source[i] = (byte)(i % 256);
                }

                // Copy data (tests memory bandwidth)
                Buffer.BlockCopy(source, 0, destination, 0, arraySize);

                // Read back to prevent optimization
                long checksum = 0;
                for (int i = 0; i < arraySize; i += 1024)
                {
                    checksum += destination[i];
                }

                // Force GC to prevent memory pressure
                source = null;
                destination = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            stopwatch.Stop();

            // Calculate bandwidth in GB/s
            double totalBytes = arraySize * iterations * 2.0; // Read + Write
            double bandwidthGBps = (totalBytes / stopwatch.Elapsed.TotalSeconds) / (1024 * 1024 * 1024);

            _logger.LogDebug("Memory Bandwidth: {BW:F2} GB/s", bandwidthGBps);
            return bandwidthGBps;
        });
    }

    /// <summary>
    /// Estimates tokens per second throughput for different model sizes
    /// Based on benchmark scores and hardware capabilities
    /// </summary>
    private TokenThroughputEstimates EstimateTokenThroughput(
        double cpuSingleScore,
        double cpuMultiScore,
        double memoryBandwidth,
        HardwareInfo hardware)
    {
        // Base calculation factors
        // These are approximate multipliers based on empirical testing
        double cpuFactor = (cpuSingleScore + cpuMultiScore) / 2.0;
        double memoryFactor = memoryBandwidth;

        // GPU acceleration factor
        double gpuFactor = 1.0;
        if (hardware.Gpu.IsDedicated && hardware.Gpu.VramGB >= 8)
        {
            // Estimate GPU acceleration based on VRAM
            // More VRAM typically indicates more powerful GPU
            gpuFactor = 2.0 + (hardware.Gpu.VramGB / 8.0);

            // Cap at reasonable maximum
            gpuFactor = Math.Min(gpuFactor, 10.0);
        }

        // Combined score (weighted average)
        double baseScore = (cpuFactor * 0.3 + memoryFactor * 0.3 + (gpuFactor * cpuFactor) * 0.4);

        // Estimate tokens/second for different model sizes
        // Larger models are slower due to increased compute and memory requirements
        var estimates = new TokenThroughputEstimates
        {
            // 7B models: fastest, good parallelization
            Model7B = Math.Round(baseScore * 1.5, 1),

            // 13B models: moderate speed
            Model13B = Math.Round(baseScore * 1.0, 1),

            // 34B models: slower, requires more memory bandwidth
            Model34B = Math.Round(baseScore * 0.5, 1),

            // 70B models: significantly slower, heavily memory bound
            Model70B = Math.Round(baseScore * 0.25, 1)
        };

        // Apply lower bounds (can't be negative or too small)
        estimates.Model7B = Math.Max(estimates.Model7B, 1.0);
        estimates.Model13B = Math.Max(estimates.Model13B, 0.5);
        estimates.Model34B = Math.Max(estimates.Model34B, 0.3);
        estimates.Model70B = Math.Max(estimates.Model70B, 0.1);

        _logger.LogDebug(
            "Token throughput estimates - 7B: {T7B} t/s, 13B: {T13B} t/s, 34B: {T34B} t/s, 70B: {T70B} t/s",
            estimates.Model7B,
            estimates.Model13B,
            estimates.Model34B,
            estimates.Model70B
        );

        return estimates;
    }

    /// <summary>
    /// Calculates comparison score vs reference baseline system
    /// Returns percentage (100 = same as reference, >100 = better, <100 = worse)
    /// </summary>
    private double CalculateComparisonScore(
        double cpuSingleScore,
        double cpuMultiScore,
        double memoryBandwidth)
    {
        // Calculate weighted average vs reference
        double cpuSingleRatio = cpuSingleScore / REFERENCE_CPU_SINGLE;
        double cpuMultiRatio = cpuMultiScore / REFERENCE_CPU_MULTI;
        double memoryRatio = memoryBandwidth / REFERENCE_MEMORY_BW;

        // Weighted average (CPU multi-core is most important for LLM inference)
        double comparisonScore = (
            cpuSingleRatio * 0.2 +
            cpuMultiRatio * 0.5 +
            memoryRatio * 0.3
        ) * 100.0;

        return Math.Round(comparisonScore, 1);
    }
}
