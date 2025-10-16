using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for checking and managing application updates via GitHub API
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/yourusername/llm-capability-checker/releases/latest";

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLMCapabilityChecker");
    }

    /// <summary>
    /// Gets the current application version from assembly
    /// </summary>
    public string GetCurrentVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }

    /// <summary>
    /// Checks GitHub API for the latest release
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _logger.LogInformation("Checking for updates...");

            var response = await _httpClient.GetAsync(GitHubApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to check for updates. Status: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var release = JsonSerializer.Deserialize<GitHubRelease>(json);

            if (release == null)
            {
                _logger.LogWarning("Failed to parse GitHub release response");
                return null;
            }

            var currentVersion = GetCurrentVersion();
            var latestVersion = release.tag_name?.TrimStart('v') ?? "0.0.0";
            var isNewer = CompareVersions(latestVersion, currentVersion);

            var updateInfo = new UpdateInfo
            {
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                ReleaseNotes = release.body ?? "No release notes available",
                DownloadUrl = release.html_url ?? string.Empty,
                ReleaseDate = release.published_at,
                IsNewerVersion = isNewer
            };

            _logger.LogInformation($"Current: {currentVersion}, Latest: {latestVersion}, Newer: {isNewer}");
            return updateInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while checking for updates");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking for updates");
            return null;
        }
    }

    /// <summary>
    /// Opens the browser to the download page
    /// </summary>
    public void DownloadUpdate(string downloadUrl)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadUrl,
                UseShellExecute = true
            });
            _logger.LogInformation($"Opened download URL: {downloadUrl}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open download URL");
        }
    }

    /// <summary>
    /// Compares two semantic versions
    /// </summary>
    /// <param name="latestVersion">Latest version string</param>
    /// <param name="currentVersion">Current version string</param>
    /// <returns>True if latest is newer than current</returns>
    private bool CompareVersions(string latestVersion, string currentVersion)
    {
        try
        {
            var latest = ParseVersion(latestVersion);
            var current = ParseVersion(currentVersion);

            if (latest.Major > current.Major) return true;
            if (latest.Major < current.Major) return false;

            if (latest.Minor > current.Minor) return true;
            if (latest.Minor < current.Minor) return false;

            return latest.Patch > current.Patch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error comparing versions: {latestVersion} vs {currentVersion}");
            return false;
        }
    }

    /// <summary>
    /// Parses a semantic version string
    /// </summary>
    private (int Major, int Minor, int Patch) ParseVersion(string version)
    {
        var parts = version.Split('.');
        var major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
        var minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        var patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
        return (major, minor, patch);
    }

    /// <summary>
    /// DTO for GitHub release API response
    /// </summary>
    private class GitHubRelease
    {
        public string? tag_name { get; set; }
        public string? html_url { get; set; }
        public string? body { get; set; }
        public DateTime published_at { get; set; }
    }
}
