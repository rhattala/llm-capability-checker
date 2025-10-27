using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LLMCapabilityChecker.Tests;

public class HardwareDetectionServiceTests
{
    private readonly Mock<ILogger<HardwareDetectionService>> _mockLogger;
    private readonly HardwareDetectionService _service;

    public HardwareDetectionServiceTests()
    {
        _mockLogger = new Mock<ILogger<HardwareDetectionService>>();
        _service = new HardwareDetectionService(_mockLogger.Object);
    }

    [Fact]
    public async Task DetectHardwareAsync_ReturnsHardwareInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesOperatingSystem()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.OperatingSystem.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesCpuInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Cpu.Should().NotBeNull();
        result.Cpu.Architecture.Should().NotBeNullOrEmpty();
        result.Cpu.Threads.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesMemoryInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Memory.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesGpuInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Gpu.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesStorageInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Storage.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectHardwareAsync_PopulatesFrameworkInfo()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Frameworks.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectHardwareAsync_HandlesExceptionsGracefully()
    {
        // Act - Should not throw even if some detection fails
        var act = async () => await _service.DetectHardwareAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DetectHardwareAsync_CpuThreadsGreaterThanOrEqualCores()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        if (result.Cpu.Cores > 0 && result.Cpu.Threads > 0)
        {
            result.Cpu.Threads.Should().BeGreaterOrEqualTo(result.Cpu.Cores);
        }
    }

    [Fact]
    public async Task DetectHardwareAsync_MemoryAvailableLessThanOrEqualTotal()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        if (result.Memory.TotalGB > 0)
        {
            result.Memory.AvailableGB.Should().BeLessOrEqualTo(result.Memory.TotalGB);
        }
    }

    [Fact]
    public async Task DetectHardwareAsync_StorageAvailableLessThanOrEqualTotal()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        if (result.Storage.TotalGB > 0)
        {
            result.Storage.AvailableGB.Should().BeLessOrEqualTo(result.Storage.TotalGB);
        }
    }

    [Fact]
    public async Task DetectHardwareAsync_CpuArchitectureIsValid()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Cpu.Architecture.Should().NotBeNullOrEmpty();
        // Common architectures (including mixed case variants)
        var validArchitectures = new[] { "x64", "X64", "x86", "ARM64", "arm64", "Arm64" };
        result.Cpu.Architecture.Should().ContainAny(validArchitectures);
    }

    [Fact]
    public async Task DetectHardwareAsync_GpuVendorIsValid()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Gpu.Vendor.Should().NotBeNullOrEmpty();
        // Known vendors or Unknown
        var validVendors = new[] { "NVIDIA", "AMD", "Intel", "Apple", "Unknown" };
        result.Gpu.Vendor.Should().BeOneOf(validVendors);
    }

    [Fact]
    public async Task DetectHardwareAsync_ConsistentResults()
    {
        // Act - Call twice
        var result1 = await _service.DetectHardwareAsync();
        var result2 = await _service.DetectHardwareAsync();

        // Assert - Core specs should be the same
        result1.Cpu.Cores.Should().Be(result2.Cpu.Cores);
        result1.Cpu.Threads.Should().Be(result2.Cpu.Threads);
        result1.Memory.TotalGB.Should().Be(result2.Memory.TotalGB);
        result1.Gpu.VramGB.Should().Be(result2.Gpu.VramGB);
    }

    [Fact]
    public async Task DetectHardwareAsync_LogicalProcessorCountReasonable()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        // Most modern systems have 2-128 logical processors
        result.Cpu.Threads.Should().BeInRange(1, 256);
    }

    [Fact]
    public async Task DetectHardwareAsync_MemoryTypePopulated()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Memory.Type.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DetectHardwareAsync_StorageTypePopulated()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Storage.Type.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DetectHardwareAsync_GpuModelPopulated()
    {
        // Act
        var result = await _service.DetectHardwareAsync();

        // Assert
        result.Gpu.Model.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DetectHardwareAsync_CompletesInReasonableTime()
    {
        // Arrange
        var startTime = DateTime.Now;

        // Act
        await _service.DetectHardwareAsync();

        // Assert
        var elapsed = DateTime.Now - startTime;
        elapsed.TotalSeconds.Should().BeLessThan(30, "Hardware detection should complete within 30 seconds");
    }
}
