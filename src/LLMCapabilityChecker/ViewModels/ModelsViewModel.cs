using CommunityToolkit.Mvvm.ComponentModel;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for browsing and filtering all available LLM models
/// </summary>
public partial class ModelsViewModel : ViewModelBase
{
    private readonly IModelDatabaseService _modelService;
    private readonly ILogger<ModelsViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _allModels = new();

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _filteredModels = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedSizeFilter = "All";

    [ObservableProperty]
    private string _selectedUseCaseFilter = "All";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private int _totalModelCount;

    [ObservableProperty]
    private int _filteredModelCount;

    /// <summary>
    /// Available size filter options
    /// </summary>
    public ObservableCollection<string> SizeFilterOptions { get; } = new()
    {
        "All",
        "0.5-2B (Tiny)",
        "3-7B (Small)",
        "8-14B (Medium)",
        "15-40B (Large)",
        "40-100B (Very Large)",
        "100B+ (Huge)"
    };

    /// <summary>
    /// Available use case filter options
    /// </summary>
    public ObservableCollection<string> UseCaseFilterOptions { get; } = new()
    {
        "All",
        "Chat",
        "Coding",
        "Reasoning",
        "Multilingual",
        "Lightweight",
        "Enterprise"
    };

    public ModelsViewModel(
        IModelDatabaseService modelService,
        ILogger<ModelsViewModel> logger)
    {
        _modelService = modelService;
        _logger = logger;

        // Load models on initialization
        _ = LoadModelsAsync();
    }

    /// <summary>
    /// Loads all models from the database
    /// </summary>
    private async Task LoadModelsAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading all models for browser view");

            var models = await _modelService.GetAllModelsAsync();
            AllModels = new ObservableCollection<ModelInfo>(models);
            TotalModelCount = AllModels.Count;

            _logger.LogInformation("Loaded {Count} models", AllModels.Count);

            // Apply initial filter
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading models");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when SearchText changes
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Called when SelectedSizeFilter changes
    /// </summary>
    partial void OnSelectedSizeFilterChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Called when SelectedUseCaseFilter changes
    /// </summary>
    partial void OnSelectedUseCaseFilterChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Applies all active filters to the model list
    /// </summary>
    private void ApplyFilters()
    {
        if (AllModels == null || AllModels.Count == 0)
        {
            FilteredModels = new ObservableCollection<ModelInfo>();
            FilteredModelCount = 0;
            return;
        }

        var filtered = AllModels.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(m =>
                m.Name.ToLower().Contains(searchLower) ||
                m.Family.ToLower().Contains(searchLower) ||
                m.Description.ToLower().Contains(searchLower) ||
                m.Tags.Any(t => t.ToLower().Contains(searchLower))
            );
        }

        // Apply size filter
        if (SelectedSizeFilter != "All")
        {
            filtered = SelectedSizeFilter switch
            {
                "0.5-2B (Tiny)" => filtered.Where(m => m.ParametersInBillions >= 0.5 && m.ParametersInBillions < 3),
                "3-7B (Small)" => filtered.Where(m => m.ParametersInBillions >= 3 && m.ParametersInBillions < 8),
                "8-14B (Medium)" => filtered.Where(m => m.ParametersInBillions >= 8 && m.ParametersInBillions < 15),
                "15-40B (Large)" => filtered.Where(m => m.ParametersInBillions >= 15 && m.ParametersInBillions < 40),
                "40-100B (Very Large)" => filtered.Where(m => m.ParametersInBillions >= 40 && m.ParametersInBillions < 100),
                "100B+ (Huge)" => filtered.Where(m => m.ParametersInBillions >= 100),
                _ => filtered
            };
        }

        // Apply use case filter
        if (SelectedUseCaseFilter != "All")
        {
            var useCaseLower = SelectedUseCaseFilter.ToLower();
            filtered = filtered.Where(m =>
                m.Tags.Any(t => t.ToLower().Contains(useCaseLower))
            );
        }

        // Update filtered collection
        var filteredList = filtered.OrderBy(m => m.ParametersInBillions).ToList();
        FilteredModels = new ObservableCollection<ModelInfo>(filteredList);
        FilteredModelCount = FilteredModels.Count;

        _logger.LogDebug("Applied filters: {Count} models match criteria", FilteredModelCount);
    }

    /// <summary>
    /// Gets display text for minimum VRAM requirement
    /// </summary>
    public string GetMinVramDisplay(ModelInfo model)
    {
        return $"{model.Requirements.MinVramGB} GB";
    }

    /// <summary>
    /// Gets display text for CPU fallback RAM requirement
    /// </summary>
    public string GetMinRamDisplay(ModelInfo model)
    {
        return $"{model.Requirements.MinRamGB} GB";
    }

    /// <summary>
    /// Gets display text for use cases
    /// </summary>
    public string GetUseCaseDisplay(ModelInfo model)
    {
        if (model.Tags.Count == 0) return "General";
        return string.Join(", ", model.Tags.Take(3));
    }

    /// <summary>
    /// Gets compatibility status text
    /// </summary>
    public string GetCompatibilityDisplay(ModelInfo model)
    {
        // For now, show a generic status since we don't have current hardware info
        // In a full implementation, this would check against current hardware
        if (model.Requirements.MinVramGB <= 8 && model.Requirements.MinRamGB <= 16)
            return "Compatible";
        if (model.Requirements.MinVramGB <= 16 && model.Requirements.MinRamGB <= 32)
            return "Mid-Range";
        return "High-End";
    }
}
