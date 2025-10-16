using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LLMCapabilityChecker.Tests;

public class ScoringServiceTests
{
    private readonly Mock<ILogger<ScoringService>> _mockLogger;
    private readonly ScoringService _service;

    public ScoringServiceTests()
    {
        _mockLogger = new Mock<ILogger<ScoringService>>();
        _service = new ScoringService(_mockLogger.Object);
    }

    [Fact]
    public async Task CalculateScoresAsync_ValidHardware_ReturnsScoresWithinRange()
    {
        // Arrange
        var hardware = CreateValidHardware();

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeInRange(0, 100);
        result.Breakdown.CpuScore.Should().BeInRange(0, 100);
        result.Breakdown.MemoryScore.Should().BeInRange(0, 100);
        result.Breakdown.GpuScore.Should().BeInRange(0, 100);
        result.Breakdown.StorageScore.Should().BeInRange(0, 100);
        result.Breakdown.FrameworkScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task CalculateScoresAsync_HighEndSystem_ReturnsHighScore()
    {
        // Arrange
        var hardware = CreateHighEndHardware();

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.OverallScore.Should().BeGreaterThan(70);
        result.SystemTier.Should().BeOneOf("High-End", "Enthusiast");
    }

    [Fact]
    public async Task CalculateScoresAsync_LowEndSystem_ReturnsLowScore()
    {
        // Arrange
        var hardware = CreateLowEndHardware();

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.OverallScore.Should().BeLessThan(50);
        result.SystemTier.Should().BeOneOf("Entry-Level", "Limited");
    }

    [Fact]
    public async Task CalculateScoresAsync_NoGpu_ReturnsLowGpuScore()
    {
        // Arrange
        var hardware = CreateValidHardware();
        hardware.Gpu.IsDedicated = false;
        hardware.Gpu.VramGB = 0;

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.Breakdown.GpuScore.Should().BeLessThan(15);
    }

    [Fact]
    public async Task CalculateScoresAsync_CalculatesRecommendedModelSize()
    {
        // Arrange
        var hardware = CreateValidHardware();

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.RecommendedModelSize.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CalculateScoresAsync_IdentifiesPrimaryBottleneck()
    {
        // Arrange
        var hardware = CreateValidHardware();

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.PrimaryBottleneck.Should().NotBeNullOrEmpty();
        result.PrimaryBottleneck.Should().Contain("/100");
    }

    [Theory]
    [InlineData(4, 2.5, 20)]
    [InlineData(8, 3.5, 50)]
    [InlineData(16, 4.0, 70)]
    public async Task CalculateScoresAsync_CpuCoresAndSpeed_AffectsCpuScore(int cores, double clockGHz, int minExpectedScore)
    {
        // Arrange
        var hardware = CreateValidHardware();
        hardware.Cpu.Cores = cores;
        hardware.Cpu.BaseClockGHz = clockGHz;

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.Breakdown.CpuScore.Should().BeGreaterThanOrEqualTo(minExpectedScore);
    }

    [Theory]
    [InlineData(8, 25)]
    [InlineData(16, 40)]
    [InlineData(32, 55)]
    [InlineData(64, 60)]
    public async Task CalculateScoresAsync_RamAmount_AffectsMemoryScore(int ramGB, int minExpectedScore)
    {
        // Arrange
        var hardware = CreateValidHardware();
        hardware.Memory.TotalGB = ramGB;

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.Breakdown.MemoryScore.Should().BeGreaterThanOrEqualTo(minExpectedScore);
    }

    [Theory]
    [InlineData(4, 20)]
    [InlineData(8, 35)]
    [InlineData(12, 45)]
    [InlineData(24, 60)]
    public async Task CalculateScoresAsync_VramAmount_AffectsGpuScore(int vramGB, int minExpectedScore)
    {
        // Arrange
        var hardware = CreateValidHardware();
        hardware.Gpu.VramGB = vramGB;
        hardware.Gpu.IsDedicated = true;

        // Act
        var result = await _service.CalculateScoresAsync(hardware);

        // Assert
        result.Breakdown.GpuScore.Should().BeGreaterThanOrEqualTo(minExpectedScore);
    }

    [Fact]
    public async Task CalculateScoresAsync_WithCuda_IncreasesFrameworkScore()
    {
        // Arrange
        var hardwareWithoutCuda = CreateValidHardware();
        hardwareWithoutCuda.Frameworks.HasCuda = false;

        var hardwareWithCuda = CreateValidHardware();
        hardwareWithCuda.Frameworks.HasCuda = true;
        hardwareWithCuda.Frameworks.CudaVersion = "12.0";

        // Act
        var resultWithoutCuda = await _service.CalculateScoresAsync(hardwareWithoutCuda);
        var resultWithCuda = await _service.CalculateScoresAsync(hardwareWithCuda);

        // Assert
        resultWithCuda.Breakdown.FrameworkScore.Should().BeGreaterThan(resultWithoutCuda.Breakdown.FrameworkScore);
    }

    private HardwareInfo CreateValidHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo
            {
                Model = "Test CPU",
                Cores = 8,
                Threads = 16,
                BaseClockGHz = 3.6,
                Architecture = "x64",
                SupportsAvx2 = true
            },
            Memory = new MemoryInfo
            {
                TotalGB = 16,
                AvailableGB = 8,
                Type = "DDR4",
                SpeedMHz = 3200
            },
            Gpu = new GpuInfo
            {
                Model = "Test GPU",
                Vendor = "NVIDIA",
                VramGB = 8,
                IsDedicated = true,
                ComputeCapability = "8.6",
                SupportsFp16 = true,
                SupportsInt8 = true
            },
            Storage = new StorageInfo
            {
                Type = "SSD",
                TotalGB = 500,
                AvailableGB = 250,
                ReadSpeedMBps = 500
            },
            Frameworks = new FrameworkInfo
            {
                HasCuda = false,
                HasDirectMl = true
            },
            OperatingSystem = "Windows 11"
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
                SupportsAvx2 = true,
                SupportsAvx512 = true
            },
            Memory = new MemoryInfo
            {
                TotalGB = 64,
                AvailableGB = 48,
                Type = "DDR5",
                SpeedMHz = 5600
            },
            Gpu = new GpuInfo
            {
                Model = "NVIDIA RTX 4090",
                Vendor = "NVIDIA",
                VramGB = 24,
                IsDedicated = true,
                ComputeCapability = "8.9",
                Architecture = "Ada Lovelace",
                SupportsFp16 = true,
                SupportsInt8 = true
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
            },
            OperatingSystem = "Windows 11"
        };
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
                AvailableGB = 4,
                Type = "DDR3",
                SpeedMHz = 1600
            },
            Gpu = new GpuInfo
            {
                Model = "Integrated Graphics",
                Vendor = "Intel",
                VramGB = 0,
                IsDedicated = false
            },
            Storage = new StorageInfo
            {
                Type = "HDD",
                TotalGB = 500,
                AvailableGB = 100,
                ReadSpeedMBps = 120
            },
            Frameworks = new FrameworkInfo(),
            OperatingSystem = "Windows 10"
        };
    }
}
