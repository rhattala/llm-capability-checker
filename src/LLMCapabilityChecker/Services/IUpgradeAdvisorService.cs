using LLMCapabilityChecker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for analyzing system bottlenecks and providing upgrade recommendations
/// </summary>
public interface IUpgradeAdvisorService
{
    /// <summary>
    /// Generates upgrade recommendations based on hardware and scores
    /// </summary>
    /// <param name="hardware">Current hardware information</param>
    /// <param name="scores">Current system scores</param>
    /// <returns>List of upgrade recommendations, empty list on error</returns>
    Task<List<UpgradeRecommendation>> GetUpgradeRecommendationsAsync(HardwareInfo hardware, SystemScores scores);
}
