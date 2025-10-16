using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using LLMCapabilityChecker.ViewModels;
using LLMCapabilityChecker.Views;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LLMCapabilityChecker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Load and apply settings
            _ = LoadAndApplySettingsAsync();

            // Get MainWindowViewModel from DI container
            var viewModel = Program.ServiceProvider?.GetService(typeof(MainWindowViewModel)) as MainWindowViewModel;

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async System.Threading.Tasks.Task LoadAndApplySettingsAsync()
    {
        try
        {
            var settingsService = Program.ServiceProvider?.GetService(typeof(ISettingsService)) as ISettingsService;
            if (settingsService != null)
            {
                var settings = await settingsService.LoadSettingsAsync();

                // Apply theme based on settings
                RequestedThemeVariant = settings.Theme switch
                {
                    "Dark" => Avalonia.Styling.ThemeVariant.Dark,
                    "Light" => Avalonia.Styling.ThemeVariant.Light,
                    "System" => Avalonia.Styling.ThemeVariant.Default,
                    _ => Avalonia.Styling.ThemeVariant.Dark
                };

                // Check for updates on startup if enabled (fire-and-forget)
                if (settings.CheckForUpdatesOnStartup)
                {
                    _ = CheckForUpdatesAsync();
                }
            }
        }
        catch
        {
            // If settings fail to load, use default dark theme
            RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
        }
    }

    private async System.Threading.Tasks.Task CheckForUpdatesAsync()
    {
        try
        {
            var updateService = Program.ServiceProvider?.GetService(typeof(IUpdateService)) as IUpdateService;
            if (updateService != null)
            {
                var updateInfo = await updateService.CheckForUpdatesAsync();

                // Only notify if a newer version is available
                if (updateInfo != null && updateInfo.IsNewerVersion)
                {
                    // In a full implementation, this would show the UpdateNotificationView
                    // For now, we just log it
                    System.Diagnostics.Debug.WriteLine($"Update available: {updateInfo.LatestVersion}");
                }
            }
        }
        catch
        {
            // Silently fail - don't interrupt app startup
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}