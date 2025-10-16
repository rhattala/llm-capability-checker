using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLMCapabilityChecker.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;

    [ObservableProperty]
    private bool _isCheckingForUpdates;

    [ObservableProperty]
    private string _updateCheckMessage = string.Empty;

    public AboutViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    public string AppName => "LLM Capability Checker";

    public string Version => _updateService.GetCurrentVersion();

    public string Description => "A comprehensive tool for analyzing your system's hardware capabilities " +
                                "for running Large Language Models. Get detailed insights into your CPU, GPU, " +
                                "and memory specifications with AI-powered recommendations.";

    public string Copyright => $"© {DateTime.Now.Year} LLM Capability Checker Contributors";

    public string License => "MIT License";

    public string LicenseText => @"Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";

    public string GitHubUrl => "https://github.com/yourusername/llm-capability-checker";

    public string BuiltWith => "Built with:";

    public string[] Technologies => new[]
    {
        "• .NET 8.0",
        "• Avalonia UI 11.3.6",
        "• CommunityToolkit.Mvvm 8.2.1",
        "• Microsoft.Extensions.DependencyInjection",
        "• System.Management (Hardware Detection)"
    };

    [RelayCommand]
    private void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GitHubUrl,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Silently fail if browser can't be opened
        }
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        IsCheckingForUpdates = true;
        UpdateCheckMessage = "Checking for updates...";

        try
        {
            var updateInfo = await _updateService.CheckForUpdatesAsync();

            if (updateInfo == null)
            {
                UpdateCheckMessage = "Unable to check for updates. Please check your internet connection.";
            }
            else if (updateInfo.IsNewerVersion)
            {
                UpdateCheckMessage = $"New version {updateInfo.LatestVersion} is available!";
                // In a full implementation, this would trigger the update notification view
                _updateService.DownloadUpdate(updateInfo.DownloadUrl);
            }
            else
            {
                UpdateCheckMessage = "You are using the latest version.";
            }
        }
        catch (Exception)
        {
            UpdateCheckMessage = "Failed to check for updates.";
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        // Signal to close the about window/dialog
        // This will be handled by the view
    }
}
