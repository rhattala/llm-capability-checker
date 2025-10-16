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
    // Based on: AMD Ryzen 5 5600X, 32GB DDR4-3200, RTX 3060
    private const double REFERENCE_CPU_SINGLE = 1000.0;
    private const double REFERENCE_CPU_MULTI = 6000.0;
    private const double REFERENCE_MEMORY_BW = 25.0; // GB/s

    public BenchmarkService(
        ILogger<BenchmarkService> logger,
        IHardwareDetectionService hardwareService)
    {
        _logger = logger;
        _hardwareService = hardwareService;
    }

    /// <summary>
    /// Runs system benchmark tests to estimate LLM inference performance
    /// </summary>
    public async Task<BenchmarkResults> RunSystemBenchmarkAsync()
    {
        _logger.LogInformation("Starting system benchmark...");
        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Run benchmarks
            var cpuSingleScore = await RunCpuSingleCoreTestAsync();
            var cpuMultiScore = await RunCpuMultiCoreTestAsync();
            var memoryBandwidth = await RunMemoryBandwidthTestAsync();

            stopwatch.Stop();

            // Get hardware info for GPU considerations
            var hardwareInfo = await _hardwareService.DetectHardwareAsync();

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
    /// Runs a simple CPU single-core benchmark test
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
