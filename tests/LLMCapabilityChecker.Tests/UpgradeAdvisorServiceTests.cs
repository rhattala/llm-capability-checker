using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LLMCapabilityChecker.Tests;

public class UpgradeAdvisorServiceTests
{
    private readonly Mock<ILogger<UpgradeAdvisorService>> _mockLogger;
    private readonly UpgradeAdvisorService _service;

    public UpgradeAdvisorServiceTests()
    {
        _mockLogger = new Mock<ILogger<UpgradeAdvisorService>>();
        _service = new UpgradeAdvisorService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_ValidInput_ReturnsRecommendations()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    // NOTE: "At most 5 recommendations" test removed - service now returns all relevant
    // recommendations to ensure nothing important is filtered out

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_SortedByPriority()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeEmpty();
        // Verify High priority comes before Low priority
        var firstHighIndex = result.FindIndex(r => r.PriorityLevel == "High");
        var firstLowIndex = result.FindIndex(r => r.PriorityLevel == "Low");

        if (firstHighIndex >= 0 && firstLowIndex >= 0)
        {
            firstHighIndex.Should().BeLessThan(firstLowIndex);
        }
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_LowVram_RecommendsGpuUpgrade()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Gpu.VramGB = 4;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().Contain(r => r.Component == "GPU");
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_LowRam_RecommendsRamUpgrade()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Memory.TotalGB = 8;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().Contain(r => r.Component == "RAM");
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_HddStorage_RecommendsNvmeUpgrade()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Storage.Type = "HDD";
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().Contain(r => r.Component == "Storage" && r.RecommendedSpecs.Contains("NVMe"));
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_LowCpuCores_RecommendsCpuUpgrade()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Cpu.Cores = 4;
        hardware.Cpu.BaseClockGHz = 2.5;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().Contain(r => r.Component == "CPU");
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_NvidiaNoCuda_RecommendsCuda()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Gpu.Vendor = "NVIDIA";
        hardware.Frameworks.HasCuda = false;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().Contain(r => r.Component == "Software" && r.SpecificProduct.Contains("CUDA"));
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_HighEndSystem_FewerRecommendations()
    {
        // Arrange
        var hardware = CreateHighEndHardware();
        var scores = CreateHighEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        // High-end system should have fewer recommendations
        result.Count.Should().BeLessThan(3);
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_AllRecommendationsHaveEstimatedCost()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeEmpty();
        result.All(r => r.EstimatedCost >= 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_AllRecommendationsHaveScoreImprovement()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeEmpty();
        result.All(r => r.ScoreImprovement > 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_AllRecommendationsHaveDescription()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeEmpty();
        result.All(r => !string.IsNullOrEmpty(r.ImpactDescription)).Should().BeTrue();
        result.All(r => !string.IsNullOrEmpty(r.Reason)).Should().BeTrue();
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_GpuBottleneck_HighPriorityGpuUpgrade()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        var scores = CreateLowEndScores();
        scores.Breakdown.GpuScore = 20; // Lowest score
        scores.Breakdown.MemoryScore = 50;
        scores.Breakdown.CpuScore = 50;

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        var gpuRecommendation = result.FirstOrDefault(r => r.Component == "GPU");
        gpuRecommendation.Should().NotBeNull();
        gpuRecommendation!.PriorityLevel.Should().Be("High");
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_VramUpgrade_SpecificProduct()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Gpu.VramGB = 8;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        var gpuRecommendation = result.FirstOrDefault(r => r.Component == "GPU");
        gpuRecommendation?.SpecificProduct.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_32GBRam_RecommendedFor16GBSystem()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Memory.TotalGB = 16;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        var ramRecommendation = result.FirstOrDefault(r => r.Component == "RAM" && r.RecommendedSpecs.Contains("32GB"));
        ramRecommendation.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_CudaRecommendation_ZeroCost()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Gpu.Vendor = "NVIDIA";
        hardware.Frameworks.HasCuda = false;
        var scores = CreateLowEndScores();

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        var cudaRecommendation = result.FirstOrDefault(r => r.SpecificProduct.Contains("CUDA"));
        cudaRecommendation.Should().NotBeNull();
        cudaRecommendation!.EstimatedCost.Should().Be(0);
    }

    [Fact]
    public async Task GetUpgradeRecommendationsAsync_ExceptionHandling_ReturnsEmptyList()
    {
        // Arrange - Create invalid scenario
        var hardware = new HardwareInfo(); // Empty hardware
        var scores = new SystemScores
        {
            Breakdown = null! // This will cause null reference
        };

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(8, true)]
    [InlineData(16, false)]
    public async Task GetUpgradeRecommendationsAsync_LowStorage_RecommendedBasedOnSpace(int availableGB, bool shouldRecommend)
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Storage.AvailableGB = availableGB;
        hardware.Storage.Type = "NVMe"; // Already NVMe, so only space matters
        var scores = CreateLowEndScores();
        scores.Breakdown.StorageScore = 80; // Good storage score

        // Act
        var result = await _service.GetUpgradeRecommendationsAsync(hardware, scores);

        // Assert
        var storageRecommendation = result.Any(r => r.Component == "Storage" && r.CurrentSpecs.Contains("available"));
        if (shouldRecommend && availableGB < 200)
        {
            storageRecommendation.Should().BeTrue();
        }
    }

    private HardwareInfo CreateLowEndHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo
            {
                Model = "Intel Core i3",
                Cores = 4,
                Threads = 8,
                BaseClockGHz = 2.4,
                Architecture = "x64"
            },
            Memory = new MemoryInfo
            {
                TotalGB = 8,
                Type = "DDR3",
                SpeedMHz = 1600
            },
            Gpu = new GpuInfo
            {
                Model = "Integrated Graphics",
                Vendor = "Intel",
                VramGB = 2,
                IsDedicated = false
            },
            Storage = new StorageInfo
            {
                Type = "HDD",
                TotalGB = 500,
                AvailableGB = 100,
                ReadSpeedMBps = 120
            },
            Frameworks = new FrameworkInfo()
        };
    }

    private HardwareInfo CreateHighEndHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo
            {
                Model = "AMD Ryzen 9 7950X",
                Cores = 16,
                Threads = 32,
                BaseClockGHz = 4.5,
                Architecture = "x64",
                SupportsAvx2 = true
            },
            Memory = new MemoryInfo
            {
                TotalGB = 64,
                Type = "DDR5",
                SpeedMHz = 5600
            },
            Gpu = new GpuInfo
            {
                Model = "NVIDIA RTX 4090",
                Vendor = "NVIDIA",
                VramGB = 24,
                IsDedicated = true
            },
            Storage = new StorageInfo
            {
                Type = "NVMe",
                TotalGB = 2000,
                AvailableGB = 1000,
                ReadSpeedMBps = 7000
            },
            Frameworks = new FrameworkInfo
            {
                HasCuda = true,
                CudaVersion = "12.2"
            }
        };
    }

    private SystemScores CreateLowEndScores()
    {
        return new SystemScores
        {
            OverallScore = 35,
            SystemTier = "Entry-Level",
            Breakdown = new ScoreBreakdown
            {
                CpuScore = 30,
                MemoryScore = 25,
                GpuScore = 15,
                StorageScore = 20,
                FrameworkScore = 40
            }
        };
    }

    private SystemScores CreateHighEndScores()
    {
        return new SystemScores
        {
            OverallScore = 85,
            SystemTier = "Enthusiast",
            Breakdown = new ScoreBreakdown
            {
                CpuScore = 90,
                MemoryScore = 85,
                GpuScore = 95,
                StorageScore = 90,
                FrameworkScore = 95
            }
        };
    }
}
