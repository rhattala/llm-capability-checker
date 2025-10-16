using System.Collections.Generic;

namespace LLMCapabilityChecker.Models;

/// <summary>
/// Information about an LLM model
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Model name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Model family (Llama, Mistral, Phi, etc.)
    /// </summary>
    public string Family { get; set; } = string.Empty;

    /// <summary>
    /// Parameter size (e.g., "7B", "13B", "70B")
    /// </summary>
    public string ParameterSize { get; set; } = string.Empty;

    /// <summary>
    /// Parameter count in billions
    /// </summary>
    public double ParametersInBillions { get; set; }

    /// <summary>
    /// Model requirements
    /// </summary>
    public ModelRequirements Requirements { get; set; } = new();

    /// <summary>
    /// Model capabilities/tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether model is recommended for this system
    /// </summary>
    public bool IsRecommended { get; set; }

    /// <summary>
    /// Compatibility level with current system (0-100)
    /// </summary>
    public int CompatibilityScore { get; set; }

    /// <summary>
    /// Expected performance tier on this system
    /// </summary>
    public string ExpectedPerformance { get; set; } = string.Empty;

    /// <summary>
    /// Official model page URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// License type
    /// </summary>
    public string License { get; set; } = string.Empty;
}

/// <summary>
/// Hardware requirements for a model
/// </summary>
public class ModelRequirements
{
    /// <summary>
    /// Minimum VRAM in GB (for GPU inference)
    /// </summary>
    public int MinVramGB { get; set; }

    /// <summary>
    /// Recommended VRAM in GB
    /// </summary>
    public int RecommendedVramGB { get; set; }

    /// <summary>
    /// Minimum RAM in GB (for CPU inference)
    /// </summary>
    public int MinRamGB { get; set; }

    /// <summary>
    /// Recommended RAM in GB
    /// </summary>
    public int RecommendedRamGB { get; set; }

    /// <summary>
    /// Minimum storage in GB
    /// </summary>
    public int MinStorageGB { get; set; }

    /// <summary>
    /// Quantization options available
    /// </summary>
    public List<QuantizationOption> QuantizationOptions { get; set; } = new();

    /// <summary>
    /// Required compute capabilities
    /// </summary>
    public ComputeRequirements ComputeRequirements { get; set; } = new();
}

/// <summary>
/// Quantization option for a model
/// </summary>
public class QuantizationOption
{
    /// <summary>
    /// Quantization format (Q4_K_M, Q5_K_S, FP16, etc.)
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Bits per weight
    /// </summary>
    public int BitsPerWeight { get; set; }

    /// <summary>
    /// VRAM required in GB
    /// </summary>
    public int VramGB { get; set; }

    /// <summary>
    /// RAM required in GB (for CPU inference)
    /// </summary>
    public int RamGB { get; set; }

    /// <summary>
    /// Quality/accuracy impact (Low, Medium, High)
    /// </summary>
    public string QualityImpact { get; set; } = "Medium";

    /// <summary>
    /// Expected performance tier
    /// </summary>
    public string PerformanceTier { get; set; } = string.Empty;
}

/// <summary>
/// Compute requirements for a model
/// </summary>
public class ComputeRequirements
{
    /// <summary>
    /// Minimum CUDA compute capability (for NVIDIA GPUs)
    /// </summary>
    public string? MinCudaCompute { get; set; }

    /// <summary>
    /// Whether AVX2 support is required
    /// </summary>
    public bool RequiresAvx2 { get; set; }

    /// <summary>
    /// Whether AVX-512 support provides benefit
    /// </summary>
    public bool BenefitsFromAvx512 { get; set; }

    /// <summary>
    /// Minimum CPU cores recommended
    /// </summary>
    public int MinCores { get; set; }

    /// <summary>
    /// Supported inference backends
    /// </summary>
    public List<string> SupportedBackends { get; set; } = new();
}
