using System.Threading.Tasks;
using LLMCapabilityChecker.Models;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for checking and managing application updates
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks GitHub API for the latest release
    /// </summary>
    /// <returns>Update information if available, null if check fails</returns>
    Task<UpdateInfo?> CheckForUpdatesAsync();

    /// <summary>
    /// Opens the browser to the download page
    /// </summary>
    /// <param name="downloadUrl">URL to open</param>
    void DownloadUpdate(string downloadUrl);

    /// <summary>
    /// Gets the current application version
    /// </summary>
    string GetCurrentVersion();
}
