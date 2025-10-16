using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LLMCapabilityChecker.Services;
using System;
using System.Threading.Tasks;

namespace LLMCapabilityChecker;

/// <summary>
/// Quick test to validate all services work correctly
/// Run this with: dotnet run --project src/LLMCapabilityChecker -- --test
/// </summary>
public class ServiceTester
{
    public static async Task RunTests()
    {
        Console.WriteLine("=== LLM Capability Checker - Service Test ===\n");

        // Display platform information
        DisplayPlatformInfo();

        // Setup DI
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton<IHardwareDetectionService, HardwareDetectionService>();
        services.AddSingleton<IScoringService, ScoringService>();
        services.AddSingleton<IModelDatabaseService, ModelDatabaseService>();

        var provider = services.BuildServiceProvider();

        // Test Hardware Detection
        Console.WriteLine("1. Testing Hardware Detection...");
        var hardwareService = provider.GetRequiredService<IHardwareDetectionService>();
        var hardware = await hardwareService.DetectHardwareAsync();

        Console.WriteLine($"   CPU: {hardware.Cpu.Model} ({hardware.Cpu.Cores} cores, {hardware.Cpu.Threads} threads)");
        Console.WriteLine($"   RAM: {hardware.Memory.TotalGB}GB {hardware.Memory.Type}");
        Console.WriteLine($"   GPU: {hardware.Gpu.Model} ({hardware.Gpu.VramGB}GB VRAM)");
        Console.WriteLine($"   Storage: {hardware.Storage.Type}, {hardware.Storage.AvailableGB}GB available");
        Console.WriteLine($"   OS: {hardware.OperatingSystem}");
        Console.WriteLine($"   CUDA: {(hardware.Frameworks.HasCuda ? hardware.Frameworks.CudaVersion ?? "Yes" : "No")}");

        // Test Scoring
        Console.WriteLine("\n2. Testing Scoring Service...");
        var scoringService = provider.GetRequiredService<IScoringService>();
        var scores = await scoringService.CalculateScoresAsync(hardware);

        Console.WriteLine($"   Overall Score: {scores.OverallScore}/100");
        Console.WriteLine($"   System Tier: {scores.SystemTier}");
        Console.WriteLine($"   Recommended Model Size: {scores.RecommendedModelSize}");
        Console.WriteLine($"   Primary Bottleneck: {scores.PrimaryBottleneck}");
        Console.WriteLine($"   Component Scores:");
        Console.WriteLine($"     - CPU: {scores.Breakdown.CpuScore}/100");
        Console.WriteLine($"     - Memory: {scores.Breakdown.MemoryScore}/100");
        Console.WriteLine($"     - GPU: {scores.Breakdown.GpuScore}/100");
        Console.WriteLine($"     - Storage: {scores.Breakdown.StorageScore}/100");
        Console.WriteLine($"     - Frameworks: {scores.Breakdown.FrameworkScore}/100");

        // Test Model Database
        Console.WriteLine("\n3. Testing Model Database Service...");
        var modelService = provider.GetRequiredService<IModelDatabaseService>();
        var allModels = await modelService.GetAllModelsAsync();
        var recommendedModels = await modelService.GetRecommendedModelsAsync(hardware);

        Console.WriteLine($"   Total models in database: {allModels.Count}");
        Console.WriteLine($"   Recommended models for your system: {recommendedModels.Count}");

        if (recommendedModels.Count > 0)
        {
            Console.WriteLine($"\n   Top 3 Recommended Models:");
            for (int i = 0; i < Math.Min(3, recommendedModels.Count); i++)
            {
                var model = recommendedModels[i];
                Console.WriteLine($"     {i + 1}. {model.Name} ({model.ParameterSize})");
                Console.WriteLine($"        Compatibility: {model.CompatibilityScore}/100");
                Console.WriteLine($"        Expected Performance: {model.ExpectedPerformance}");
            }
        }

        Console.WriteLine("\n=== All Tests Complete ===");
    }

    /// <summary>
    /// Display platform information and detection methods
    /// </summary>
    private static void DisplayPlatformInfo()
    {
        Console.WriteLine("Platform Information:");
        Console.WriteLine($"  OS: {Helpers.SystemInfoHelper.DetectOperatingSystem()}");
        Console.WriteLine($"  Architecture: {Helpers.SystemInfoHelper.GetArchitecture()}");
        Console.WriteLine($"  .NET Version: {Environment.Version}");
        Console.WriteLine($"  Processor Count: {Helpers.SystemInfoHelper.GetLogicalProcessorCount()}");

        // Display platform-specific detection methods
        Console.WriteLine("\nDetection Methods:");
        if (Helpers.SystemInfoHelper.IsWindows())
        {
            Console.WriteLine("  Platform: Windows");
            Console.WriteLine("  CPU Detection: WMI (Win32_Processor)");
            Console.WriteLine("  Memory Detection: WMI (Win32_ComputerSystem, Win32_PhysicalMemory)");
            Console.WriteLine("  GPU Detection: WMI (Win32_VideoController) + nvidia-smi");
            Console.WriteLine("  Storage Detection: WMI (Win32_LogicalDisk, Win32_DiskDrive)");
            Console.WriteLine("  Framework Detection: DirectML (OS check), CUDA (nvidia-smi), OpenVINO (directory)");
        }
        else if (Helpers.SystemInfoHelper.IsLinux())
        {
            Console.WriteLine("  Platform: Linux");
            Console.WriteLine("  CPU Detection: lscpu command");
            Console.WriteLine("  Memory Detection: /proc/meminfo");
            Console.WriteLine("  GPU Detection: lspci command + nvidia-smi");
            Console.WriteLine("  Storage Detection: df, lsblk commands");
            Console.WriteLine("  Framework Detection: CUDA (nvidia-smi), ROCm (rocm-smi), OpenVINO (directory)");
        }
        else if (Helpers.SystemInfoHelper.IsMacOS())
        {
            Console.WriteLine("  Platform: macOS");
            Console.WriteLine("  CPU Detection: sysctl command");
            Console.WriteLine("  Memory Detection: sysctl command");
            Console.WriteLine("  GPU Detection: system_profiler command");
            Console.WriteLine("  Storage Detection: diskutil, df commands");
            Console.WriteLine("  Framework Detection: Metal (sw_vers), OpenVINO (directory)");
        }
        else
        {
            Console.WriteLine("  Platform: Unknown");
            Console.WriteLine("  Detection methods may not work correctly!");
        }

        Console.WriteLine();
    }
}
