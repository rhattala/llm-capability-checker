using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// Main window ViewModel that hosts the dashboard and handles navigation
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUpgradeAdvisorService _upgradeService;

    /// <summary>
    /// Dashboard ViewModel instance
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// Current view being displayed (null for dashboard, ComponentDetailsViewModel for details)
    /// </summary>
    [ObservableProperty]
    private ViewModelBase? _currentView;

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        IUpgradeAdvisorService upgradeService)
    {
        Dashboard = dashboardViewModel;
        _upgradeService = upgradeService;

        // Set dashboard as initial view
        CurrentView = dashboardViewModel;

        // Wire up navigation from dashboard
        Dashboard.NavigateToDetails = async (componentType) =>
        {
            await NavigateToComponentDetailsAsync(componentType);
        };
    }

    /// <summary>
    /// Navigates to component details view
    /// </summary>
    [RelayCommand]
    public async Task NavigateToComponentDetailsAsync(ComponentType componentType)
    {
        var detailsViewModel = new ComponentDetailsViewModel(_upgradeService);

        // Wire up back navigation
        detailsViewModel.NavigateBack = NavigateToDashboard;

        if (Dashboard.HardwareInfo != null && Dashboard.SystemScores != null)
        {
            await detailsViewModel.InitializeAsync(
                componentType,
                Dashboard.HardwareInfo,
                Dashboard.SystemScores
            );
        }

        CurrentView = detailsViewModel;
    }

    /// <summary>
    /// Navigates back to dashboard
    /// </summary>
    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentView = Dashboard;
    }

    /// <summary>
    /// Navigates to Settings view
    /// </summary>
    [RelayCommand]
    public void NavigateToSettings()
    {
        var settingsViewModel = Program.ServiceProvider?.GetService(typeof(SettingsViewModel)) as SettingsViewModel;
        if (settingsViewModel != null)
        {
            // Wire up back navigation
            settingsViewModel.NavigateBack = NavigateToDashboard;
            CurrentView = settingsViewModel;
        }
    }

    /// <summary>
    /// Navigates to About view
    /// </summary>
    [RelayCommand]
    public void NavigateToAbout()
    {
        var aboutViewModel = Program.ServiceProvider?.GetService(typeof(AboutViewModel)) as AboutViewModel;
        if (aboutViewModel != null)
        {
            CurrentView = aboutViewModel;
        }
    }

    /// <summary>
    /// Navigates to Community view
    /// </summary>
    [RelayCommand]
    public void NavigateToCommunity()
    {
        var communityViewModel = Program.ServiceProvider?.GetService(typeof(CommunityViewModel)) as CommunityViewModel;
        if (communityViewModel != null)
        {
            // Wire up back navigation
            communityViewModel.NavigateBack = NavigateToDashboard;
            CurrentView = communityViewModel;
        }
    }

    /// <summary>
    /// Navigates to Help view
    /// </summary>
    [RelayCommand]
    public void NavigateToHelp()
    {
        var helpViewModel = new HelpViewModel();
        // Wire up back navigation
        helpViewModel.NavigateBack = NavigateToDashboard;
        CurrentView = helpViewModel;
    }
}
