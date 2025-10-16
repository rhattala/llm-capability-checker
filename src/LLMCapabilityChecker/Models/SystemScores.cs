namespace LLMCapabilityChecker.Models;

/// <summary>
/// System capability scores across different dimensions
/// </summary>
public class SystemScores
{
    /// <summary>
    /// Overall system score (0-100) - DEPRECATED: Use InferenceScore instead
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Inference score (0-100) - Ability to run models for chat/generation
    /// </summary>
    public int InferenceScore { get; set; }

    /// <summary>
    /// Training score (0-100) - Ability to do full fine-tuning
    /// </summary>
    public int TrainingScore { get; set; }

    /// <summary>
    /// Fine-tuning score (0-100) - Ability to do LoRA/QLoRA parameter-efficient tuning
    /// </summary>
    public int FineTuningScore { get; set; }

    /// <summary>
    /// Inference capability description
    /// </summary>
    public string InferenceCapability { get; set; } = string.Empty;

    /// <summary>
    /// Training capability description
    /// </summary>
    public string TrainingCapability { get; set; } = string.Empty;

    /// <summary>
    /// Fine-tuning capability description
    /// </summary>
    public string FineTuningCapability { get; set; } = string.Empty;

    /// <summary>
    /// Score breakdown by component
    /// </summary>
    public ScoreBreakdown Breakdown { get; set; } = new();

    /// <summary>
    /// Primary bottleneck in the system
    /// </summary>
    public string PrimaryBottleneck { get; set; } = string.Empty;

    /// <summary>
    /// System tier classification
    /// </summary>
    public string SystemTier { get; set; } = "Unknown";

    /// <summary>
    /// Recommended model size tier (7B, 13B, 34B, 70B+)
    /// </summary>
    public string RecommendedModelSize { get; set; } = string.Empty;
}

/// <summary>
/// Detailed score breakdown by component
/// </summary>
public class ScoreBreakdown
{
    /// <summary>
    /// CPU score (0-100)
    /// </summary>
    public int CpuScore { get; set; }

    /// <summary>
    /// Memory score (0-100)
    /// </summary>
    public int MemoryScore { get; set; }

    /// <summary>
    /// GPU score (0-100)
    /// </summary>
    public int GpuScore { get; set; }

    /// <summary>
    /// Storage score (0-100)
    /// </summary>
    public int StorageScore { get; set; }

    /// <summary>
    /// Framework/software score (0-100)
    /// </summary>
    public int FrameworkScore { get; set; }
}
