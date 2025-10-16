using LLMCapabilityChecker.Models;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for calculating system capability scores for LLM workloads
/// </summary>
public interface IScoringService
{
    /// <summary>
    /// Calculates comprehensive system scores based on hardware information.
    /// Never throws - returns conservative scores on missing/invalid data.
    /// </summary>
    /// <param name="hardware">Hardware information to score</param>
    /// <returns>System scores with detailed breakdown</returns>
    Task<SystemScores> CalculateScoresAsync(HardwareInfo hardware);
}
