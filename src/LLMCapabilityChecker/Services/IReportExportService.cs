using LLMCapabilityChecker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for exporting hardware analysis reports in various formats
/// </summary>
public interface IReportExportService
{
    /// <summary>
    /// Exports the hardware analysis report as formatted JSON
    /// </summary>
    /// <param name="hardwareInfo">Hardware information</param>
    /// <param name="systemScores">System scores</param>
    /// <param name="recommendedModels">List of recommended models</param>
    /// <returns>JSON string representation of the report</returns>
    Task<string> ExportAsJsonAsync(HardwareInfo hardwareInfo, SystemScores systemScores, List<ModelInfo> recommendedModels);

    /// <summary>
    /// Exports the hardware analysis report as formatted text
    /// </summary>
    /// <param name="hardwareInfo">Hardware information</param>
    /// <param name="systemScores">System scores</param>
    /// <param name="recommendedModels">List of recommended models</param>
    /// <returns>Formatted text representation of the report</returns>
    Task<string> ExportAsTextAsync(HardwareInfo hardwareInfo, SystemScores systemScores, List<ModelInfo> recommendedModels);

    /// <summary>
    /// Saves report content to a file using a file dialog
    /// </summary>
    /// <param name="content">Report content to save</param>
    /// <param name="suggestedFileName">Suggested file name</param>
    /// <returns>True if file was saved successfully, false if cancelled or failed</returns>
    Task<bool> SaveReportToFileAsync(string content, string suggestedFileName);
}
