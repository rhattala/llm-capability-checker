using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// ViewModel for the in-app help viewer
/// </summary>
public partial class HelpViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<HelpCategory> _categories = new();

    [ObservableProperty]
    private HelpCategory? _selectedCategory;

    [ObservableProperty]
    private HelpTopic? _selectedTopic;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<HelpTopic> _searchResults = new();

    [ObservableProperty]
    private bool _isSearchMode;

    /// <summary>
    /// Navigation action - set by MainWindowViewModel
    /// </summary>
    public Action? NavigateBack { get; set; }

    public HelpViewModel()
    {
        InitializeHelpContent();
    }

    /// <summary>
    /// Initializes help content with categories and topics
    /// </summary>
    private void InitializeHelpContent()
    {
        Categories = new ObservableCollection<HelpCategory>
        {
            new HelpCategory
            {
                Name = "Getting Started",
                Icon = "🚀",
                Topics = new ObservableCollection<HelpTopic>
                {
                    new HelpTopic
                    {
                        Title = "Welcome to LLM Capability Checker",
                        Content = "This app analyzes your computer hardware and tells you which Large Language Models (LLMs) you can run locally.\n\n" +
                                  "**What you get:**\n" +
                                  "• System capability score (0-100)\n" +
                                  "• Compatible model recommendations\n" +
                                  "• Hardware upgrade suggestions\n" +
                                  "• Performance benchmarks\n\n" +
                                  "**How it works:**\n" +
                                  "1. Automatically detects your hardware (2-3 seconds)\n" +
                                  "2. Calculates scores based on LLM requirements\n" +
                                  "3. Shows models that will work on your system\n" +
                                  "4. Suggests upgrades for better performance"
                    },
                    new HelpTopic
                    {
                        Title = "First Launch",
                        Content = "When you first launch the app:\n\n" +
                                  "1. **Hardware Detection**: The app scans your CPU, GPU, RAM, and Storage (takes 2-3 seconds)\n\n" +
                                  "2. **Score Calculation**: You receive an overall score plus component scores\n\n" +
                                  "3. **Model Recommendations**: Top 3 compatible models are shown\n\n" +
                                  "4. **Upgrade Suggestions**: Areas for improvement are highlighted\n\n" +
                                  "No configuration needed - everything is automatic!"
                    },
                    new HelpTopic
                    {
                        Title = "Understanding the Dashboard",
                        Content = "**Top Card (Blue)**\n" +
                                  "• Overall score (0-100)\n" +
                                  "• System tier badge\n" +
                                  "• Recommended model size\n" +
                                  "• Primary bottleneck\n\n" +
                                  "**Hardware Cards (Gray)**\n" +
                                  "Shows your CPU, GPU, RAM, and Storage specs\n\n" +
                                  "**Component Scores**\n" +
                                  "Five interactive cards with progress bars:\n" +
                                  "• CPU Score\n" +
                                  "• Memory (RAM) Score\n" +
                                  "• GPU Score\n" +
                                  "• Storage Score\n" +
                                  "• Framework Score\n\n" +
                                  "Click any score card to see detailed information!"
                    }
                }
            },
            new HelpCategory
            {
                Name = "Features",
                Icon = "⭐",
                Topics = new ObservableCollection<HelpTopic>
                {
                    new HelpTopic
                    {
                        Title = "Hardware Detection",
                        Content = "The app automatically detects:\n\n" +
                                  "**CPU**: Model, cores, clock speed, architecture\n" +
                                  "**GPU**: Model, VRAM, vendor, compute capability\n" +
                                  "**RAM**: Total capacity, type (DDR4/DDR5), speed\n" +
                                  "**Storage**: Capacity, type (HDD/SSD/NVMe), free space\n" +
                                  "**Frameworks**: CUDA, DirectML, ROCm, Metal, OpenVINO\n\n" +
                                  "**Accuracy**: ~95% on modern systems (2015+)\n\n" +
                                  "**Refresh**: Click the Refresh button after hardware changes"
                    },
                    new HelpTopic
                    {
                        Title = "Understanding Scores",
                        Content = "**Overall Score (0-100)**\n" +
                                  "• 80-100: Excellent - Run most models smoothly\n" +
                                  "• 60-79: Good - Suitable for common use cases\n" +
                                  "• 40-59: Fair - Limited to smaller models\n" +
                                  "• 20-39: Limited - Basic models only\n" +
                                  "• 0-19: Poor - Significant upgrades needed\n\n" +
                                  "**Component Weights**:\n" +
                                  "• GPU: 30% (inference speed)\n" +
                                  "• RAM: 25% (model size)\n" +
                                  "• CPU: 20% (loading, CPU inference)\n" +
                                  "• Storage: 15% (loading time)\n" +
                                  "• Frameworks: 10% (GPU acceleration)"
                    },
                    new HelpTopic
                    {
                        Title = "Model Recommendations",
                        Content = "The app shows compatible models based on:\n\n" +
                                  "**Memory Requirements**: Model fits in your RAM/VRAM\n" +
                                  "**Performance**: Expected tokens/second > 5 (usable)\n" +
                                  "**Frameworks**: Available GPU acceleration\n\n" +
                                  "**Model Size Guide**:\n" +
                                  "• 1B-3B: Very fast, basic capabilities\n" +
                                  "• 7B: Standard, good all-around\n" +
                                  "• 13B-14B: Enhanced capabilities\n" +
                                  "• 30B-34B: Advanced, requires high-end hardware\n" +
                                  "• 70B+: Top-tier, enthusiast-grade\n\n" +
                                  "**Rule of Thumb**: RAM (GB) / 2 = max model size (B) with 4-bit quantization"
                    },
                    new HelpTopic
                    {
                        Title = "Upgrade Advisor",
                        Content = "**Primary Bottleneck**: The component limiting you most\n\n" +
                                  "**Upgrade Priority**:\n" +
                                  "1. Fix Primary Bottleneck first\n" +
                                  "2. Address red scores (0-59)\n" +
                                  "3. Improve orange scores (60-79)\n" +
                                  "4. Green scores (80+) are fine\n\n" +
                                  "**Common Upgrades**:\n" +
                                  "• RAM: $100-200 (32GB minimum)\n" +
                                  "• GPU: $400-1500 (12GB+ VRAM)\n" +
                                  "• Storage: $50-150 (NVMe SSD)\n" +
                                  "• Frameworks: Free (CUDA, DirectML)\n\n" +
                                  "Click any component score for specific recommendations!"
                    },
                    new HelpTopic
                    {
                        Title = "Benchmark Testing",
                        Content = "Test your actual system performance:\n\n" +
                                  "**What's Tested**:\n" +
                                  "• CPU single-core performance\n" +
                                  "• CPU multi-core performance\n" +
                                  "• Memory bandwidth\n" +
                                  "• Token throughput estimates\n\n" +
                                  "**Duration**: 30-60 seconds\n\n" +
                                  "**Best Practices**:\n" +
                                  "• Close other applications\n" +
                                  "• Plug in laptop (don't use battery)\n" +
                                  "• Ensure adequate cooling\n" +
                                  "• Wait for system to idle\n\n" +
                                  "Results show comparison to reference system and estimated performance for different model sizes."
                    },
                    new HelpTopic
                    {
                        Title = "Export Reports",
                        Content = "Save your hardware analysis:\n\n" +
                                  "**How to Export**:\n" +
                                  "1. Click 'Export Report' button on Dashboard\n" +
                                  "2. Choose save location\n" +
                                  "3. File saved as: LLM_Capability_Report_[timestamp].json\n\n" +
                                  "**Report Contents**:\n" +
                                  "• Complete hardware specifications\n" +
                                  "• System scores and breakdown\n" +
                                  "• Model recommendations\n" +
                                  "• Benchmark results (if run)\n" +
                                  "• Metadata (timestamp, app version)\n\n" +
                                  "**Uses**:\n" +
                                  "• Share with community\n" +
                                  "• Track upgrades over time\n" +
                                  "• Attach to bug reports"
                    }
                }
            },
            new HelpCategory
            {
                Name = "Troubleshooting",
                Icon = "🔧",
                Topics = new ObservableCollection<HelpTopic>
                {
                    new HelpTopic
                    {
                        Title = "Hardware Not Detected",
                        Content = "If hardware shows as 'Unknown' or fails to detect:\n\n" +
                                  "**Windows**:\n" +
                                  "• Run as Administrator\n" +
                                  "• Check WMI service is running\n" +
                                  "• Update Windows\n\n" +
                                  "**Linux**:\n" +
                                  "• Install tools: sudo apt install lshw pciutils util-linux\n" +
                                  "• Run with sudo if needed\n" +
                                  "• Check dmesg for hardware errors\n\n" +
                                  "**macOS**:\n" +
                                  "• Grant permissions in System Preferences\n" +
                                  "• Update to latest macOS version\n\n" +
                                  "Then click the Refresh button on Dashboard."
                    },
                    new HelpTopic
                    {
                        Title = "GPU Not Detected",
                        Content = "If your GPU isn't showing:\n\n" +
                                  "**NVIDIA GPUs**:\n" +
                                  "• Install latest drivers from nvidia.com\n" +
                                  "• Install CUDA Toolkit (optional)\n" +
                                  "• On Linux: sudo apt install nvidia-utils\n\n" +
                                  "**AMD GPUs**:\n" +
                                  "• Install latest drivers from amd.com\n" +
                                  "• On Linux: Install rocm-smi if using ROCm\n" +
                                  "• On Windows: DirectML auto-detects\n\n" +
                                  "**Laptops**:\n" +
                                  "• Ensure using dedicated GPU, not integrated\n" +
                                  "• Check power settings (not battery saver)\n" +
                                  "• Update GPU drivers"
                    },
                    new HelpTopic
                    {
                        Title = "Low Score Despite Good Hardware",
                        Content = "If your score is lower than expected:\n\n" +
                                  "**Check**:\n" +
                                  "1. Primary Bottleneck - often one weak component limits everything\n" +
                                  "2. Frameworks installed - no CUDA/DirectML = low GPU score\n" +
                                  "3. RAM configuration - single channel is 2x slower than dual\n" +
                                  "4. GPU drivers - outdated drivers lower performance\n" +
                                  "5. Individual component scores - identify the weak link\n\n" +
                                  "**Solutions**:\n" +
                                  "• Install GPU acceleration frameworks (free)\n" +
                                  "• Update all drivers\n" +
                                  "• Add second RAM stick for dual channel\n" +
                                  "• Click component scores for specific advice"
                    },
                    new HelpTopic
                    {
                        Title = "Benchmark Fails or Crashes",
                        Content = "If benchmark doesn't complete:\n\n" +
                                  "**Before Benchmarking**:\n" +
                                  "• Close all other applications\n" +
                                  "• Ensure 4GB+ free RAM\n" +
                                  "• Plug in laptop (don't run on battery)\n" +
                                  "• Disable overclocking temporarily\n\n" +
                                  "**If Still Failing**:\n" +
                                  "• Update graphics drivers\n" +
                                  "• Try 'Quick Benchmark' instead\n" +
                                  "• Check CPU temperature (overheating?)\n" +
                                  "• Update to latest app version\n\n" +
                                  "**Report Issue**: If persistent, report on GitHub with system specs."
                    },
                    new HelpTopic
                    {
                        Title = "Export Report Fails",
                        Content = "If report export doesn't work:\n\n" +
                                  "**Check**:\n" +
                                  "• Disk space: Ensure 10MB+ free\n" +
                                  "• Permissions: Try saving to Desktop\n" +
                                  "• File locks: Close other apps\n" +
                                  "• Write permissions: Run as Administrator (Windows)\n\n" +
                                  "**Alternative**:\n" +
                                  "• Try different save location\n" +
                                  "• Check antivirus isn't blocking\n" +
                                  "• Manually create folder first"
                    }
                }
            },
            new HelpCategory
            {
                Name = "About",
                Icon = "ℹ",
                Topics = new ObservableCollection<HelpTopic>
                {
                    new HelpTopic
                    {
                        Title = "About LLM Capability Checker",
                        Content = "**LLM Capability Checker v1.0.0**\n\n" +
                                  "A free, open-source tool to assess your hardware's ability to run Large Language Models locally.\n\n" +
                                  "**Built With**:\n" +
                                  "• .NET 8\n" +
                                  "• Avalonia UI (cross-platform)\n" +
                                  "• MVVM architecture\n\n" +
                                  "**License**: MIT (fully open-source)\n\n" +
                                  "**Privacy**: 100% local operation, no telemetry by default\n\n" +
                                  "**Platforms**: Windows, Linux, macOS"
                    },
                    new HelpTopic
                    {
                        Title = "Keyboard Shortcuts",
                        Content = "**Navigation**:\n" +
                                  "• F1: Open Help\n" +
                                  "• F5: Refresh Dashboard\n" +
                                  "• Escape: Close modals/dialogs\n\n" +
                                  "**Windows/Linux**:\n" +
                                  "• Ctrl+E: Export Report\n" +
                                  "• Ctrl+,: Open Settings\n\n" +
                                  "**macOS**:\n" +
                                  "• Cmd+E: Export Report\n" +
                                  "• Cmd+,: Open Settings"
                    },
                    new HelpTopic
                    {
                        Title = "Privacy & Data",
                        Content = "**Your privacy matters:**\n\n" +
                                  "• **100% Local**: All processing happens on your machine\n" +
                                  "• **No Telemetry**: No tracking or analytics by default\n" +
                                  "• **No Account**: No sign-up or registration required\n" +
                                  "• **Offline**: Works completely offline (after initial model DB download)\n\n" +
                                  "**What's Sent**:\n" +
                                  "• Model database download (first run only, ~50KB)\n" +
                                  "• Nothing else by default\n\n" +
                                  "**Optional Opt-in** (Settings → Privacy):\n" +
                                  "• Anonymous benchmark sharing\n" +
                                  "• Crash reports (no personal data)\n" +
                                  "• Usage statistics"
                    },
                    new HelpTopic
                    {
                        Title = "Getting More Help",
                        Content = "**Documentation**:\n" +
                                  "• User Guide: Comprehensive manual\n" +
                                  "• FAQ: Common questions answered\n" +
                                  "• GitHub Wiki: Technical details\n\n" +
                                  "**Community**:\n" +
                                  "• GitHub Discussions: Ask questions\n" +
                                  "• GitHub Issues: Report bugs\n" +
                                  "• Reddit r/LocalLLaMA: Community discussions\n\n" +
                                  "**Before Asking**:\n" +
                                  "1. Check FAQ and User Guide\n" +
                                  "2. Enable detailed logging (Settings)\n" +
                                  "3. Export report (attach to bug reports)\n" +
                                  "4. Search existing GitHub issues\n\n" +
                                  "Visit: github.com/yourusername/llm-capability-checker"
                    },
                    new HelpTopic
                    {
                        Title = "Contributing",
                        Content = "**We welcome contributions!**\n\n" +
                                  "**Ways to Help**:\n" +
                                  "• Report bugs and issues\n" +
                                  "• Suggest new features\n" +
                                  "• Improve documentation\n" +
                                  "• Add models to database\n" +
                                  "• Contribute code\n\n" +
                                  "**Getting Started**:\n" +
                                  "1. Read CONTRIBUTING.md\n" +
                                  "2. Check 'good first issue' labels\n" +
                                  "3. Fork the repository\n" +
                                  "4. Make your changes\n" +
                                  "5. Submit a pull request\n\n" +
                                  "See CONTRIBUTING.md for detailed guidelines."
                    }
                }
            }
        };

        // Select first category and topic by default
        if (Categories.Any())
        {
            SelectedCategory = Categories.First();
            if (SelectedCategory.Topics.Any())
            {
                SelectedTopic = SelectedCategory.Topics.First();
            }
        }
    }

    /// <summary>
    /// Selects a help topic to display
    /// </summary>
    [RelayCommand]
    private void SelectTopic(HelpTopic topic)
    {
        SelectedTopic = topic;
        IsSearchMode = false;
    }

    /// <summary>
    /// Searches help topics
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            IsSearchMode = false;
            SearchResults.Clear();
            return;
        }

        IsSearchMode = true;
        var query = SearchText.ToLower();
        var results = new List<HelpTopic>();

        foreach (var category in Categories)
        {
            foreach (var topic in category.Topics)
            {
                if (topic.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    topic.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(topic);
                }
            }
        }

        SearchResults = new ObservableCollection<HelpTopic>(results);
    }

    /// <summary>
    /// Clears the search
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        IsSearchMode = false;
        SearchResults.Clear();
    }

    /// <summary>
    /// Navigates back to dashboard
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        NavigateBack?.Invoke();
    }
}

/// <summary>
/// Represents a category of help topics
/// </summary>
public class HelpCategory
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ObservableCollection<HelpTopic> Topics { get; set; } = new();
}

/// <summary>
/// Represents a single help topic
/// </summary>
public class HelpTopic
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
