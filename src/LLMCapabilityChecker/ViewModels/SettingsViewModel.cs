using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for the Settings view
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _selectedTheme = "Dark";

    [ObservableProperty]
    private string _selectedAutoRefreshInterval = "Never";

    [ObservableProperty]
    private bool _showAdvancedMetrics = false;

    [ObservableProperty]
    private string _selectedExportFormat = "JSON";

    [ObservableProperty]
    private bool _checkForUpdatesOnStartup = true;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    /// <summary>
    /// Available theme options
    /// </summary>
    public List<string> ThemeOptions { get; } = new() { "Dark", "Light", "System" };

    /// <summary>
    /// Available auto-refresh interval options
    /// </summary>
    public List<string> AutoRefreshIntervalOptions { get; } = new() { "Never", "Daily", "Weekly" };

    /// <summary>
    /// Available export format options
    /// </summary>
    public List<string> ExportFormatOptions { get; } = new() { "JSON", "Text", "Both" };

    /// <summary>
    /// Navigation action to go back - set by MainWindowViewModel
    /// </summary>
    public Action? NavigateBack { get; set; }

    public SettingsViewModel(ISettingsService settingsService, ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;

        // Load current settings
        _ = LoadSettingsAsync();

        // Watch for property changes to mark as unsaved
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(HasUnsavedChanges))
            {
                HasUnsavedChanges = true;
            }
        };
    }

    /// <summary>
    /// Loads settings from the service
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();

            // Update properties without triggering unsaved changes
            var previousHasUnsavedChanges = HasUnsavedChanges;

            SelectedTheme = settings.Theme;
            SelectedAutoRefreshInterval = settings.AutoRefreshInterval;
            ShowAdvancedMetrics = settings.ShowAdvancedMetrics;
            SelectedExportFormat = settings.ExportFormat;
            CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;

            HasUnsavedChanges = previousHasUnsavedChanges;

            _logger.LogInformation("Settings loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
        }
    }

    /// <summary>
    /// Saves the current settings
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var settings = new AppSettings
            {
                Theme = SelectedTheme,
                AutoRefreshInterval = SelectedAutoRefreshInterval,
                ShowAdvancedMetrics = ShowAdvancedMetrics,
                ExportFormat = SelectedExportFormat,
                CheckForUpdatesOnStartup = CheckForUpdatesOnStartup
            };

            await _settingsService.SaveSettingsAsync(settings);

            HasUnsavedChanges = false;

            // Apply theme immediately
            ApplyTheme(SelectedTheme);

            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
        }
    }

    /// <summary>
    /// Resets settings to defaults
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        var defaults = _settingsService.GetDefaultSettings();

        SelectedTheme = defaults.Theme;
        SelectedAutoRefreshInterval = defaults.AutoRefreshInterval;
        ShowAdvancedMetrics = defaults.ShowAdvancedMetrics;
        SelectedExportFormat = defaults.ExportFormat;
        CheckForUpdatesOnStartup = defaults.CheckForUpdatesOnStartup;

        HasUnsavedChanges = true;

        _logger.LogInformation("Settings reset to defaults");
    }

    /// <summary>
    /// Cancels changes and navigates back
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        NavigateBack?.Invoke();
    }

    /// <summary>
    /// Applies the selected theme to the application
    /// </summary>
    private void ApplyTheme(string theme)
    {
        if (Application.Current == null) return;

        try
        {
            switch (theme)
            {
                case "Dark":
                    Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                    break;
                case "Light":
                    Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                    break;
                case "System":
                    Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
                    break;
            }

            _logger.LogInformation("Theme applied: {Theme}", theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying theme");
        }
    }

    /// <summary>
    /// Called when theme selection changes - apply immediately
    /// </summary>
    partial void OnSelectedThemeChanged(string value)
    {
        ApplyTheme(value);
    }
}
