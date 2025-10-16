using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for managing application settings and preferences
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private AppSettings? _currentSettings;
    private readonly string _settingsFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Event raised when settings are changed
    /// </summary>
    public event EventHandler<AppSettings>? SettingsChanged;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;

        // Settings file location: %APPDATA%/LLMCapabilityChecker/settings.json
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "LLMCapabilityChecker");

        // Ensure directory exists
        Directory.CreateDirectory(appFolder);

        _settingsFilePath = Path.Combine(appFolder, "settings.json");

        _logger.LogInformation("Settings file path: {Path}", _settingsFilePath);
    }

    /// <summary>
    /// Loads settings from disk
    /// </summary>
    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _currentSettings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

                if (_currentSettings != null)
                {
                    _logger.LogInformation("Settings loaded from {Path}", _settingsFilePath);
                    return _currentSettings;
                }
            }

            _logger.LogInformation("Settings file not found, using defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings from {Path}", _settingsFilePath);
        }

        // Return defaults if load failed or file doesn't exist
        _currentSettings = GetDefaultSettings();
        return _currentSettings;
    }

    /// <summary>
    /// Saves settings to disk
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);

            _currentSettings = settings;
            _logger.LogInformation("Settings saved to {Path}", _settingsFilePath);

            // Raise settings changed event
            SettingsChanged?.Invoke(this, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to {Path}", _settingsFilePath);
            throw;
        }
    }

    /// <summary>
    /// Gets the current settings (cached)
    /// </summary>
    public AppSettings GetCurrentSettings()
    {
        return _currentSettings ?? GetDefaultSettings();
    }

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    public AppSettings GetDefaultSettings()
    {
        return new AppSettings
        {
            Theme = "Dark",
            AutoRefreshInterval = "Never",
            ShowAdvancedMetrics = false,
            ExportFormat = "JSON",
            CheckForUpdatesOnStartup = true
        };
    }
}
