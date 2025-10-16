namespace LLMCapabilityChecker.Models;

/// <summary>
/// Represents a hardware upgrade recommendation
/// </summary>
public class UpgradeRecommendation
{
    /// <summary>
    /// Component to upgrade (CPU/GPU/RAM/Storage)
    /// </summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// Current hardware specifications
    /// </summary>
    public string CurrentSpecs { get; set; } = string.Empty;

    /// <summary>
    /// Recommended hardware specifications
    /// </summary>
    public string RecommendedSpecs { get; set; } = string.Empty;

    /// <summary>
    /// Description of the impact this upgrade will have
    /// </summary>
    public string ImpactDescription { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public string PriorityLevel { get; set; } = string.Empty;

    /// <summary>
    /// Estimated cost in dollars (optional, 0 if unknown)
    /// </summary>
    public int EstimatedCost { get; set; }

    /// <summary>
    /// Specific product recommendation (optional)
    /// </summary>
    public string? SpecificProduct { get; set; }

    /// <summary>
    /// Expected score improvement after upgrade
    /// </summary>
    public int ScoreImprovement { get; set; }

    /// <summary>
    /// Why this upgrade is recommended
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
