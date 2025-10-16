using LLMCapabilityChecker.Models;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for running performance benchmarks to estimate LLM inference speed
/// </summary>
public interface IBenchmarkService
{
    /// <summary>
    /// Runs system benchmark tests to estimate LLM inference performance
    /// Tests include: CPU single/multi-core, memory bandwidth
    /// Results are used to estimate tokens/second for different model sizes
    /// </summary>
    /// <returns>Benchmark results with estimated tokens/second</returns>
    Task<BenchmarkResults> RunSystemBenchmarkAsync();
}
