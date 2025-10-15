# AI Implementation Guide

## Purpose
This document provides **step-by-step instructions for AI** to implement the LLM Capability Checker application. Follow these instructions sequentially. Each step includes clear acceptance criteria and code examples.

## Core Principles for AI Implementation

### 🎯 Optimization Rules
1. **Minimal Code**: Achieve maximum functionality with least code
2. **Reuse**: Don't repeat yourself - create reusable components
3. **Built-in First**: Use .NET built-ins before adding dependencies
4. **Performance**: Async by default, cache aggressively, lazy load
5. **Error Handling**: Never crash - always degrade gracefully

### 📝 Code Quality Standards
- **Clear Names**: Self-documenting code (no cryptic abbreviations)
- **Short Methods**: Max 50 lines per method
- **Single Responsibility**: One job per class/method
- **Comprehensive Comments**: Explain WHY, not WHAT
- **Null Safety**: Use nullable reference types, check for nulls

---

## Implementation Phases

---

# PHASE 1: Project Setup & Core Infrastructure (Week 1)

## Step 1.1: Create Project Structure

**Task**: Initialize Avalonia application with proper structure

**Commands**:
```bash
# Create solution
dotnet new sln -n LLMCapabilityChecker

# Create main application project
dotnet new avalonia.mvvm -n LLMCapabilityChecker -o src/LLMCapabilityChecker

# Create test project
dotnet new xunit -n LLMCapabilityChecker.Tests -o tests/LLMCapabilityChecker.Tests

# Add projects to solution
dotnet sln add src/LLMCapabilityChecker/LLMCapabilityChecker.csproj
dotnet sln add tests/LLMCapabilityChecker.Tests/LLMCapabilityChecker.Tests.csproj

# Add test reference to main project
cd tests/LLMCapabilityChecker.Tests
dotnet add reference ../../src/LLMCapabilityChecker/LLMCapabilityChecker.csproj
```

**Folder Structure**:
```
llm-capability-checker/
├── src/
│   └── LLMCapabilityChecker/
│       ├── Models/
│       ├── ViewModels/
│       ├── Views/
│       ├── Services/
│       ├── Helpers/
│       ├── Assets/
│       ├── App.axaml
│       ├── App.axaml.cs
│       └── Program.cs
├── tests/
│   └── LLMCapabilityChecker.Tests/
├── docs/
├── data/
│   └── models.json (example)
└── README.md
```

**Acceptance Criteria**:
- [ ] Solution builds without errors
- [ ] Can run application (shows default window)
- [ ] Test project references main project

---

## Step 1.2: Install Required NuGet Packages

**Task**: Add minimal required dependencies

**Commands**:
```bash
cd src/LLMCapabilityChecker

# Core Avalonia packages (usually already included)
dotnet add package Avalonia
dotnet add package Avalonia.Desktop
dotnet add package Avalonia.Themes.Fluent

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Http

# Logging
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Debug

# JSON handling (built into .NET 8+, but explicit for clarity)
# System.Text.Json is built-in

# Testing packages (for test project)
cd ../../tests/LLMCapabilityChecker.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

**Acceptance Criteria**:
- [ ] All packages install successfully
- [ ] Project still builds
- [ ] No version conflicts

---

## Step 1.3: Implement Base Models

**Task**: Create core data models in `Models/` folder

**File**: `Models/HardwareInfo.cs`
```csharp
namespace LLMCapabilityChecker.Models;

/// <summary>
/// Complete hardware information for the system
/// </summary>
public class HardwareInfo
{
    public CpuInfo Cpu { get; set; } = new();
    public GpuInfo? Gpu { get; set; }
    public MemoryInfo Memory { get; set; } = new();
    public StorageInfo Storage { get; set; } = new();
    public FrameworkSupport Frameworks { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Returns true if essential hardware was detected successfully
    /// </summary>
    public bool IsValid => Cpu.IsDetected && Memory.IsDetected;
}

public class CpuInfo
{
    public bool IsDetected { get; set; }
    public string Model { get; set; } = "Unknown";
    public string Manufacturer { get; set; } = "Unknown";
    public int PhysicalCores { get; set; }
    public int LogicalCores { get; set; }
    public double BaseFrequencyGHz { get; set; }
    public double MaxFrequencyGHz { get; set; }
    public string Architecture { get; set; } = "Unknown";
    public int L1CacheKB { get; set; }
    public int L2CacheKB { get; set; }
    public int L3CacheMB { get; set; }
}

public class GpuInfo
{
    public bool IsDetected { get; set; }
    public string Model { get; set; } = "Unknown";
    public string Manufacturer { get; set; } = "Unknown";
    public int VramGB { get; set; }
    public int ComputeUnits { get; set; }
    public string Architecture { get; set; } = "Unknown";
    public string DriverVersion { get; set; } = "Unknown";
    public double ComputeCapability { get; set; }
    public bool HasTensorCores { get; set; }
}

public class MemoryInfo
{
    public bool IsDetected { get; set; }
    public int TotalGB { get; set; }
    public int AvailableGB { get; set; }
    public int SpeedMHz { get; set; }
    public string Type { get; set; } = "Unknown";
}

public class StorageInfo
{
    public bool IsDetected { get; set; }
    public StorageType Type { get; set; }
    public int TotalGB { get; set; }
    public int AvailableGB { get; set; }
    public int ReadSpeedMBps { get; set; }
    public int WriteSpeedMBps { get; set; }
}

public enum StorageType
{
    Unknown,
    HDD,
    SSD,
    NVMe
}

public class FrameworkSupport
{
    public CudaInfo? Cuda { get; set; }
    public RocmInfo? Rocm { get; set; }
    public bool DirectMLSupported { get; set; }
    public bool OneAPISupported { get; set; }
    public Wsl2Info? Wsl2 { get; set; }
}

public class CudaInfo
{
    public bool IsInstalled { get; set; }
    public string Version { get; set; } = "Unknown";
    public string ToolkitVersion { get; set; } = "Unknown";
}

public class RocmInfo
{
    public bool IsInstalled { get; set; }
    public string Version { get; set; } = "Unknown";
}

public class Wsl2Info
{
    public bool IsInstalled { get; set; }
    public string Version { get; set; } = "Unknown";
    public bool GpuPassthroughEnabled { get; set; }
}
```

**File**: `Models/SystemScores.cs`
```csharp
namespace LLMCapabilityChecker.Models;

/// <summary>
/// System capability scores for different use cases
/// </summary>
public class SystemScores
{
    /// <summary>
    /// Score for running LLM inference (0-1000)
    /// </summary>
    public int InferenceScore { get; set; }
    
    /// <summary>
    /// Score for training LLMs from scratch (0-1000)
    /// </summary>
    public int TrainingScore { get; set; }
    
    /// <summary>
    /// Score for fine-tuning existing LLMs (0-1000)
    /// </summary>
    public int FineTuningScore { get; set; }
    
    /// <summary>
    /// Overall rating (Excellent, Very Good, Good, Fair, Limited, Poor)
    /// </summary>
    public string OverallRating { get; set; } = "Unknown";
    
    public ScoreBreakdown InferenceBreakdown { get; set; } = new();
    public ScoreBreakdown TrainingBreakdown { get; set; } = new();
    public ScoreBreakdown FineTuningBreakdown { get; set; } = new();
}

public class ScoreBreakdown
{
    public double GpuContribution { get; set; }
    public double CpuContribution { get; set; }
    public double RamContribution { get; set; }
    public double StorageContribution { get; set; }
    
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    
    /// <summary>
    /// Explanation of how the score was calculated
    /// </summary>
    public string Explanation { get; set; } = "";
}
```

**File**: `Models/ModelInfo.cs`
```csharp
namespace LLMCapabilityChecker.Models;

/// <summary>
/// Information about an LLM model
/// </summary>
public class ModelInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Family { get; set; } = "";
    public int ParameterCount { get; set; }
    public long ParameterCountRaw { get; set; }
    public string Provider { get; set; } = "";
    public string License { get; set; } = "";
    public List<string> UseCases { get; set; } = new();
    public int PopularityRank { get; set; }
    
    public Dictionary<string, ModelRequirements> Requirements { get; set; } = new();
    public TrainingRequirements? TrainingRequirements { get; set; }
    public FrameworkCompatibility FrameworkSupport { get; set; } = new();
    
    public bool RecommendedForBeginners { get; set; }
    public string Notes { get; set; } = "";
}

public class ModelRequirements
{
    public int VramGB { get; set; }
    public int RamGB { get; set; }
    public int StorageGB { get; set; }
    public int MinInferenceScore { get; set; }
    public Dictionary<string, int> EstimatedTokensPerSec { get; set; } = new();
}

public class TrainingRequirements
{
    public Dictionary<string, TrainingMethodRequirements> Methods { get; set; } = new();
}

public class TrainingMethodRequirements
{
    public int VramGB { get; set; }
    public int RamGB { get; set; }
    public int MinTrainingScore { get; set; }
    public double EstimatedTimePerEpochHours { get; set; }
}

public class FrameworkCompatibility
{
    public bool LlamaCpp { get; set; }
    public bool GGUF { get; set; }
    public bool PyTorch { get; set; }
    public bool Transformers { get; set; }
    public bool Ollama { get; set; }
    public bool LMStudio { get; set; }
    public bool ExLlama { get; set; }
}
```

**Acceptance Criteria**:
- [ ] All model classes compile
- [ ] Models are serializable (for JSON)
- [ ] XML documentation on all public members

---

## Step 1.4: Implement ViewModelBase

**Task**: Create reusable base class for ViewModels

**File**: `ViewModels/ViewModelBase.cs`
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LLMCapabilityChecker.ViewModels;

/// <summary>
/// Base class for all ViewModels implementing INotifyPropertyChanged
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises PropertyChanged event for the specified property
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets property value and raises PropertyChanged if value changed
    /// Returns true if value was changed
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets property value, raises PropertyChanged, and executes action if value changed
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        onChanged?.Invoke();
        return true;
    }
}
```

**Acceptance Criteria**:
- [ ] ViewModelBase compiles
- [ ] SetProperty works correctly
- [ ] Can create derived ViewModel classes

---

## Step 1.5: Setup Dependency Injection

**Task**: Configure DI container in Program.cs

**File**: `Program.cs`
```csharp
using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LLMCapabilityChecker.Services;
using LLMCapabilityChecker.ViewModels;

namespace LLMCapabilityChecker;

class Program
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Build and run Avalonia app
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // HttpClient
        services.AddHttpClient();

        // Services (will implement these in next steps)
        // services.AddSingleton<IHardwareDetectionService, HardwareDetectionService>();
        // services.AddSingleton<IScoringService, ScoringService>();
        // ... etc

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        // services.AddTransient<DashboardViewModel>();
        // ... etc
    }
}
```

**Acceptance Criteria**:
- [ ] DI container configured
- [ ] ServiceProvider accessible globally
- [ ] Logging configured

---

# PHASE 2: Hardware Detection (Week 1-2)

## Step 2.1: Implement Hardware Detection Service Interface

**File**: `Services/IHardwareDetectionService.cs`
```csharp
using LLMCapabilityChecker.Models;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for detecting system hardware capabilities
/// </summary>
public interface IHardwareDetectionService
{
    /// <summary>
    /// Detects all hardware components asynchronously
    /// Never throws - returns partial results on failure
    /// </summary>
    Task<HardwareInfo> DetectHardwareAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects only CPU information
    /// </summary>
    Task<CpuInfo> DetectCpuAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects only GPU information
    /// </summary>
    Task<GpuInfo?> DetectGpuAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects only memory information
    /// </summary>
    Task<MemoryInfo> DetectMemoryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects only storage information
    /// </summary>
    Task<StorageInfo> DetectStorageAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects AI framework support (CUDA, ROCm, etc.)
    /// </summary>
    Task<FrameworkSupport> DetectFrameworksAsync(CancellationToken cancellationToken = default);
}
```

---

## Step 2.2: Implement System Info Helper

**File**: `Helpers/SystemInfoHelper.cs`
```csharp
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LLMCapabilityChecker.Helpers;

/// <summary>
/// Helper for executing system commands and parsing output
/// </summary>
public static class SystemInfoHelper
{
    /// <summary>
    /// Executes a command and returns output
    /// Returns null if command fails or times out
    /// </summary>
    public static async Task<string?> ExecuteCommandAsync(
        string command,
        string arguments,
        int timeoutSeconds = 2)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var outputTask = process.StandardOutput.ReadToEndAsync();
            
            await Task.WhenAny(outputTask, Task.Delay(Timeout.Infinite, cts.Token));
            
            if (!process.HasExited)
            {
                process.Kill();
                return null;
            }

            return await outputTask;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns true if running on Windows
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Returns true if running on Linux
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Returns true if running on macOS
    /// </summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
```

---

## Step 2.3: Implement Windows WMI Detection (Windows Only)

**File**: `Services/HardwareDetectionService.cs`
```csharp
using System.Management; // Add NuGet: System.Management (Windows only)
using Microsoft.Extensions.Logging;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Helpers;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Hardware detection implementation using multiple strategies
/// </summary>
public class HardwareDetectionService : IHardwareDetectionService
{
    private readonly ILogger<HardwareDetectionService> _logger;
    private HardwareInfo? _cachedHardware;

    public HardwareDetectionService(ILogger<HardwareDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<HardwareInfo> DetectHardwareAsync(CancellationToken cancellationToken = default)
    {
        // Return cached if available
        if (_cachedHardware != null)
            return _cachedHardware;

        var hardware = new HardwareInfo();

        // Run detections in parallel for speed
        var tasks = new[]
        {
            DetectCpuAsync(cancellationToken).ContinueWith(t => hardware.Cpu = t.Result),
            DetectGpuAsync(cancellationToken).ContinueWith(t => hardware.Gpu = t.Result),
            DetectMemoryAsync(cancellationToken).ContinueWith(t => hardware.Memory = t.Result),
            DetectStorageAsync(cancellationToken).ContinueWith(t => hardware.Storage = t.Result),
            DetectFrameworksAsync(cancellationToken).ContinueWith(t => hardware.Frameworks = t.Result)
        };

        await Task.WhenAll(tasks);

        hardware.DetectedAt = DateTime.UtcNow;
        _cachedHardware = hardware;

        return hardware;
    }

    public async Task<CpuInfo> DetectCpuAsync(CancellationToken cancellationToken = default)
    {
        var cpu = new CpuInfo();

        try
        {
            if (SystemInfoHelper.IsWindows)
            {
                await DetectCpuWindows(cpu);
            }
            else if (SystemInfoHelper.IsLinux)
            {
                await DetectCpuLinux(cpu);
            }
            else
            {
                // Fallback detection
                cpu.LogicalCores = Environment.ProcessorCount;
                cpu.IsDetected = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect CPU");
        }

        return cpu;
    }

    private async Task DetectCpuWindows(CpuInfo cpu)
    {
        // Use WMI on Windows
        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                cpu.Model = obj["Name"]?.ToString() ?? "Unknown";
                cpu.Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                cpu.PhysicalCores = Convert.ToInt32(obj["NumberOfCores"] ?? 0);
                cpu.LogicalCores = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                cpu.MaxFrequencyGHz = Convert.ToDouble(obj["MaxClockSpeed"] ?? 0) / 1000.0;
                cpu.IsDetected = true;
                break; // Take first CPU
            }
        });
    }

    private async Task DetectCpuLinux(CpuInfo cpu)
    {
        // Parse /proc/cpuinfo on Linux
        try
        {
            var cpuInfo = await File.ReadAllTextAsync("/proc/cpuinfo");
            var lines = cpuInfo.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("model name"))
                {
                    cpu.Model = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("processor"))
                {
                    cpu.LogicalCores++;
                }
            }
            
            cpu.PhysicalCores = cpu.LogicalCores / 2; // Rough estimate
            cpu.IsDetected = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read /proc/cpuinfo");
        }
    }

    public async Task<GpuInfo?> DetectGpuAsync(CancellationToken cancellationToken = default)
    {
        // Try NVIDIA first
        var nvidiaGpu = await DetectNvidiaGpu();
        if (nvidiaGpu != null) return nvidiaGpu;

        // Try AMD
        var amdGpu = await DetectAmdGpu();
        if (amdGpu != null) return amdGpu;

        // No GPU detected
        return null;
    }

    private async Task<GpuInfo?> DetectNvidiaGpu()
    {
        try
        {
            var output = await SystemInfoHelper.ExecuteCommandAsync(
                "nvidia-smi",
                "--query-gpu=name,memory.total,driver_version --format=csv,noheader,nounits");

            if (string.IsNullOrWhiteSpace(output))
                return null;

            var parts = output.Split(',');
            if (parts.Length < 3)
                return null;

            var gpu = new GpuInfo
            {
                IsDetected = true,
                Manufacturer = "NVIDIA",
                Model = parts[0].Trim(),
                VramGB = (int)Math.Round(double.Parse(parts[1].Trim()) / 1024.0),
                DriverVersion = parts[2].Trim()
            };

            return gpu;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "nvidia-smi not available or failed");
            return null;
        }
    }

    private async Task<GpuInfo?> DetectAmdGpu()
    {
        // TODO: Implement AMD GPU detection with rocm-smi
        // Similar to NVIDIA but with rocm-smi command
        await Task.CompletedTask;
        return null;
    }

    public async Task<MemoryInfo> DetectMemoryAsync(CancellationToken cancellationToken = default)
    {
        var memory = new MemoryInfo();

        try
        {
            if (SystemInfoHelper.IsWindows)
            {
                await DetectMemoryWindows(memory);
            }
            else if (SystemInfoHelper.IsLinux)
            {
                await DetectMemoryLinux(memory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect memory");
        }

        return memory;
    }

    private async Task DetectMemoryWindows(MemoryInfo memory)
    {
        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            long totalBytes = 0;
            
            foreach (var obj in searcher.Get())
            {
                totalBytes += Convert.ToInt64(obj["Capacity"]);
                memory.SpeedMHz = Convert.ToInt32(obj["Speed"] ?? 0);
            }
            
            memory.TotalGB = (int)(totalBytes / (1024 * 1024 * 1024));
            memory.IsDetected = true;
        });
    }

    private async Task DetectMemoryLinux(MemoryInfo memory)
    {
        try
        {
            var memInfo = await File.ReadAllTextAsync("/proc/meminfo");
            var lines = memInfo.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var kB = long.Parse(parts[1]);
                    memory.TotalGB = (int)(kB / (1024 * 1024));
                    memory.IsDetected = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read /proc/meminfo");
        }
    }

    public async Task<StorageInfo> DetectStorageAsync(CancellationToken cancellationToken = default)
    {
        var storage = new StorageInfo();

        try
        {
            // Get drive info for C: (Windows) or / (Linux)
            var drive = SystemInfoHelper.IsWindows 
                ? new DriveInfo("C") 
                : new DriveInfo("/");

            storage.TotalGB = (int)(drive.TotalSize / (1024 * 1024 * 1024));
            storage.AvailableGB = (int)(drive.AvailableFreespace / (1024 * 1024 * 1024));
            storage.Type = DetectStorageType(drive);
            storage.IsDetected = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect storage");
        }

        return storage;
    }

    private StorageType DetectStorageType(DriveInfo drive)
    {
        // This is a simplified detection
        // TODO: Implement more sophisticated detection
        return StorageType.SSD; // Default assumption
    }

    public async Task<FrameworkSupport> DetectFrameworksAsync(CancellationToken cancellationToken = default)
    {
        var frameworks = new FrameworkSupport();

        // Detect CUDA
        var cudaOutput = await SystemInfoHelper.ExecuteCommandAsync("nvcc", "--version");
        if (cudaOutput != null)
        {
            frameworks.Cuda = new CudaInfo
            {
                IsInstalled = true,
                Version = ParseCudaVersion(cudaOutput)
            };
        }

        // TODO: Detect ROCm, DirectML, oneAPI, WSL2

        return frameworks;
    }

    private string ParseCudaVersion(string output)
    {
        // Parse CUDA version from nvcc output
        // Example: "release 12.1, V12.1.105"
        var match = System.Text.RegularExpressions.Regex.Match(output, @"release (\d+\.\d+)");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }
}
```

**NOTE FOR AI**: This is a simplified implementation. You should:
1. Add Windows-only compilation conditions (`#if WINDOWS`)
2. Implement more robust parsing
3. Add AMD GPU detection
4. Add more framework detections
5. Handle edge cases

**Acceptance Criteria**:
- [ ] Can detect CPU on Windows and Linux
- [ ] Can detect NVIDIA GPU if present
- [ ] Can detect RAM
- [ ] Can detect storage
- [ ] Never crashes - returns partial results on failure

---

**CONTINUE TO NEXT MESSAGE FOR REMAINING PHASES...**

This is getting very long! I'll continue with the remaining phases in the next file. Should I:
1. Continue creating the remaining implementation phases (Phase 3-8)?
2. Create additional supporting documents (sample model database JSON, README, etc.)?
3. Both?

The implementation guide will cover:
- Phase 3: Scoring Service
- Phase 4: Model Database Service
- Phase 5: UI Implementation
- Phase 6: Recommendations & Upgrade Advisor
- Phase 7: Optional Benchmarking
- Phase 8: Polish & Optimization

Let me know how you'd like to proceed!
