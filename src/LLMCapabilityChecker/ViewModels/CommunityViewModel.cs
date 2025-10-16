using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for community model recommendations
/// </summary>
public partial class CommunityViewModel : ViewModelBase
{
    private readonly ICommunityService _communityService;
    private readonly IHardwareDetectionService _hardwareService;
    private readonly ILogger<CommunityViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<CommunityRecommendation> _trendingModels = new();

    [ObservableProperty]
    private ObservableCollection<CommunityRecommendation> _topRatedModels = new();

    [ObservableProperty]
    private ObservableCollection<CommunityRecommendation> _forYourSystemModels = new();

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _selectedTab = "Trending";

    [ObservableProperty]
    private string _userHardwareTier = "Unknown";

    [ObservableProperty]
    private int _userVramGB = 0;

    [ObservableProperty]
    private int _userRamGB = 0;

    [ObservableProperty]
    private bool _showSubmissionDialog = false;

    [ObservableProperty]
    private string _submissionModelName = string.Empty;

    [ObservableProperty]
    private string _submissionReason = string.Empty;

    [ObservableProperty]
    private int _submissionRating = 5;

    [ObservableProperty]
    private string _submissionUseCase = "Chat";

    /// <summary>
    /// Navigation action back to previous view
    /// </summary>
    public Action? NavigateBack { get; set; }

    public CommunityViewModel(
        ICommunityService communityService,
        IHardwareDetectionService hardwareService,
        ILogger<CommunityViewModel> logger)
    {
        _communityService = communityService;
        _hardwareService = hardwareService;
        _logger = logger;

        // Load data on initialization
        _ = InitializeAsync();
    }

    /// <summary>
    /// Initializes the view with community recommendations
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;

            // Detect user hardware
            var hardware = await _hardwareService.DetectHardwareAsync();
            UserHardwareTier = hardware.SystemTier;
            UserVramGB = hardware.Gpu.VramGB;
            UserRamGB = hardware.Memory.TotalGB;

            // Load all recommendation categories in parallel
            var trendingTask = _communityService.GetTrendingModelsAsync(30, 20);
            var topRatedTask = _communityService.GetTopRatedAsync(5, 20);
            var forSystemTask = _communityService.GetRecommendationsForHardwareAsync(hardware);

            await Task.WhenAll(trendingTask, topRatedTask, forSystemTask);

            TrendingModels = new ObservableCollection<CommunityRecommendation>(await trendingTask);
            TopRatedModels = new ObservableCollection<CommunityRecommendation>(await topRatedTask);
            ForYourSystemModels = new ObservableCollection<CommunityRecommendation>(await forSystemTask);

            _logger.LogInformation("Loaded community recommendations: {Trending} trending, {TopRated} top-rated, {ForSystem} for system",
                TrendingModels.Count, TopRatedModels.Count, ForYourSystemModels.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing community view");
            HasError = true;
            ErrorMessage = $"Failed to load community recommendations: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes all recommendation data
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await InitializeAsync();
    }

    /// <summary>
    /// Changes the selected tab
    /// </summary>
    [RelayCommand]
    private void SelectTab(string tabName)
    {
        SelectedTab = tabName;
        _logger.LogDebug("Switched to {Tab} tab", tabName);
    }

    /// <summary>
    /// Upvotes a model recommendation
    /// </summary>
    [RelayCommand]
    private async Task UpvoteModelAsync(CommunityRecommendation model)
    {
        try
        {
            var success = await _communityService.UpvoteRecommendationAsync(model.ModelName);

            if (success)
            {
                // Update the count locally
                model.RecommendedByCount++;
                _logger.LogInformation("Upvoted {ModelName}", model.ModelName);
            }
            else
            {
                _logger.LogWarning("Already upvoted {ModelName} or upvote failed", model.ModelName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upvoting model");
        }
    }

    /// <summary>
    /// Opens the submission dialog
    /// </summary>
    [RelayCommand]
    private void OpenSubmissionDialog()
    {
        ShowSubmissionDialog = true;
        SubmissionModelName = string.Empty;
        SubmissionReason = string.Empty;
        SubmissionRating = 5;
        SubmissionUseCase = "Chat";
    }

    /// <summary>
    /// Closes the submission dialog
    /// </summary>
    [RelayCommand]
    private void CloseSubmissionDialog()
    {
        ShowSubmissionDialog = false;
    }

    /// <summary>
    /// Submits a new model recommendation
    /// </summary>
    [RelayCommand]
    private async Task SubmitRecommendationAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SubmissionModelName))
            {
                ErrorMessage = "Please enter a model name";
                return;
            }

            if (string.IsNullOrWhiteSpace(SubmissionReason))
            {
                ErrorMessage = "Please provide a reason for your recommendation";
                return;
            }

            var submission = new ModelSubmission
            {
                ModelName = SubmissionModelName.Trim(),
                ReasonForRecommendation = SubmissionReason.Trim(),
                UserHardwareTier = UserHardwareTier,
                Rating = SubmissionRating,
                UseCase = SubmissionUseCase,
                DateSubmitted = DateTime.UtcNow
            };

            var success = await _communityService.SubmitRecommendationAsync(submission);

            if (success)
            {
                _logger.LogInformation("Successfully submitted recommendation for {ModelName}", SubmissionModelName);
                ShowSubmissionDialog = false;

                // Could show a success message here
            }
            else
            {
                ErrorMessage = "Failed to submit recommendation. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting recommendation");
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets star icons for rating display
    /// </summary>
    public string GetStarDisplay(double rating)
    {
        var fullStars = (int)Math.Floor(rating);
        var hasHalfStar = rating - fullStars >= 0.5;
        var emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

        var stars = string.Concat(Enumerable.Repeat("★", fullStars));
        if (hasHalfStar) stars += "⯨";
        stars += string.Concat(Enumerable.Repeat("☆", emptyStars));

        return stars;
    }

    /// <summary>
    /// Gets color for rating
    /// </summary>
    public string GetRatingColor(double rating)
    {
        return rating switch
        {
            >= 4.5 => "#4CAF50",
            >= 4.0 => "#8BC34A",
            >= 3.5 => "#FFC107",
            >= 3.0 => "#FF9800",
            _ => "#F57C00"
        };
    }

    /// <summary>
    /// Gets badge color based on source
    /// </summary>
    public string GetSourceBadgeColor(RecommendationSource source)
    {
        return source switch
        {
            RecommendationSource.Official => "#1976D2",
            RecommendationSource.Verified => "#4CAF50",
            RecommendationSource.Community => "#9E9E9E",
            _ => "#9E9E9E"
        };
    }

    /// <summary>
    /// Gets display text for hardware tier badge
    /// </summary>
    public string GetTierBadgeColor(string tier)
    {
        return tier switch
        {
            "Entry" => "#8BC34A",
            "Mid" => "#1976D2",
            "High" => "#9C27B0",
            "Enthusiast" => "#D32F2F",
            _ => "#9E9E9E"
        };
    }

    /// <summary>
    /// Formats date as relative time
    /// </summary>
    public string FormatRelativeDate(DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date;

        return timeSpan.TotalDays switch
        {
            < 1 => "Today",
            < 2 => "Yesterday",
            < 7 => $"{(int)timeSpan.TotalDays} days ago",
            < 30 => $"{(int)(timeSpan.TotalDays / 7)} weeks ago",
            < 365 => $"{(int)(timeSpan.TotalDays / 30)} months ago",
            _ => $"{(int)(timeSpan.TotalDays / 365)} years ago"
        };
    }

    /// <summary>
    /// Navigates back to dashboard
    /// </summary>
    [RelayCommand]
    private void NavigateToDashboard()
    {
        NavigateBack?.Invoke();
    }
}
