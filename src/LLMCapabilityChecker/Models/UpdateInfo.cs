using System;

namespace LLMCapabilityChecker.Models;

/// <summary>
/// Information about an available application update
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// Latest version number (e.g., "1.2.0")
    /// </summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// Release notes/changelog for the update
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Download URL for the release
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Date when the release was published
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Indicates if the latest version is newer than the current version
    /// </summary>
    public bool IsNewerVersion { get; set; }

    /// <summary>
    /// Current version of the application
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;
}
