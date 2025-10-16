namespace LLMCapabilityChecker.Models;

/// <summary>
/// Application settings and preferences
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Theme preference (Dark, Light, or System)
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// Auto-refresh interval (Never, Daily, or Weekly)
    /// </summary>
    public string AutoRefreshInterval { get; set; } = "Never";

    /// <summary>
    /// Whether to show advanced metrics in the dashboard
    /// </summary>
    public bool ShowAdvancedMetrics { get; set; } = false;

    /// <summary>
    /// Export format preference (JSON, Text, or Both)
    /// </summary>
    public string ExportFormat { get; set; } = "JSON";

    /// <summary>
    /// Whether to check for updates on application startup
    /// </summary>
    public bool CheckForUpdatesOnStartup { get; set; } = true;
}
