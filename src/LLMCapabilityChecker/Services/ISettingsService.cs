using LLMCapabilityChecker.Models;
using System;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for managing application settings and preferences
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Event raised when settings are changed
    /// </summary>
    event EventHandler<AppSettings>? SettingsChanged;

    /// <summary>
    /// Loads settings from disk
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// Saves settings to disk
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Gets the current settings (cached)
    /// </summary>
    AppSettings GetCurrentSettings();

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    AppSettings GetDefaultSettings();
}
