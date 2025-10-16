using System;

namespace LLMCapabilityChecker.Models;

/// <summary>
/// Results from running system benchmark tests
/// </summary>
public class BenchmarkResults
{
    /// <summary>
    /// CPU single-core score (higher is better)
    /// </summary>
    public double CpuSingleCoreScore { get; set; }

    /// <summary>
    /// CPU multi-core score (higher is better)
    /// </summary>
    public double CpuMultiCoreScore { get; set; }

    /// <summary>
    /// Memory bandwidth in GB/s
    /// </summary>
    public double MemoryBandwidthGBps { get; set; }

    /// <summary>
    /// Estimated tokens per second for different model sizes
    /// </summary>
    public TokenThroughputEstimates TokenThroughput { get; set; } = new();

    /// <summary>
    /// Comparison to reference baseline system (percentage)
    /// </summary>
    public double ComparisonToReference { get; set; }

    /// <summary>
    /// Time taken to run the benchmark in seconds
    /// </summary>
    public double BenchmarkDurationSeconds { get; set; }

    /// <summary>
    /// Timestamp when the benchmark was run
    /// </summary>
    public DateTime BenchmarkTimestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Estimated tokens per second throughput for different model sizes
/// </summary>
public class TokenThroughputEstimates
{
    /// <summary>
    /// Estimated tokens/second for 7B parameter models
    /// </summary>
    public double Model7B { get; set; }

    /// <summary>
    /// Estimated tokens/second for 13B parameter models
    /// </summary>
    public double Model13B { get; set; }

    /// <summary>
    /// Estimated tokens/second for 34B parameter models
    /// </summary>
    public double Model34B { get; set; }

    /// <summary>
    /// Estimated tokens/second for 70B parameter models
    /// </summary>
    public double Model70B { get; set; }
}
