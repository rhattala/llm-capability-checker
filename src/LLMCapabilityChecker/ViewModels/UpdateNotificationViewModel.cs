using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using System;

namespace LLMCapabilityChecker.ViewModels;

public partial class UpdateNotificationViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;

    [ObservableProperty]
    private UpdateInfo? _updateInfo;

    [ObservableProperty]
    private bool _isVisible;

    public UpdateNotificationViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    /// <summary>
    /// Shows the update notification with the provided update info
    /// </summary>
    public void ShowUpdate(UpdateInfo updateInfo)
    {
        UpdateInfo = updateInfo;
        IsVisible = true;
    }

    [RelayCommand]
    private void DownloadUpdate()
    {
        if (UpdateInfo != null && !string.IsNullOrEmpty(UpdateInfo.DownloadUrl))
        {
            _updateService.DownloadUpdate(UpdateInfo.DownloadUrl);
        }
    }

    [RelayCommand]
    private void SkipVersion()
    {
        // In a full implementation, this would save the skipped version
        // to settings so it's not shown again
        IsVisible = false;
    }

    [RelayCommand]
    private void RemindLater()
    {
        IsVisible = false;
    }

    [RelayCommand]
    private void Close()
    {
        IsVisible = false;
    }
}
