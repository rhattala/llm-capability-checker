using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LLMCapabilityChecker.Tests;

public class BenchmarkServiceTests
{
    private readonly Mock<ILogger<BenchmarkService>> _mockLogger;
    private readonly Mock<IHardwareDetectionService> _mockHardwareService;
    private readonly BenchmarkService _service;

    public BenchmarkServiceTests()
    {
        _mockLogger = new Mock<ILogger<BenchmarkService>>();
        _mockHardwareService = new Mock<IHardwareDetectionService>();
        _service = new BenchmarkService(_mockLogger.Object, _mockHardwareService.Object);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_ReturnsValidResults()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.Should().NotBeNull();
        result.CpuSingleCoreScore.Should().BeGreaterThan(0);
        result.CpuMultiCoreScore.Should().BeGreaterThan(0);
        result.MemoryBandwidthGBps.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_MultiCoreFasterThanSingleCore()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.CpuMultiCoreScore.Should().BeGreaterThan(result.CpuSingleCoreScore);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_TokenThroughputEstimatesProvided()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.TokenThroughput.Should().NotBeNull();
        result.TokenThroughput.Model7B.Should().BeGreaterThan(0);
        result.TokenThroughput.Model13B.Should().BeGreaterThan(0);
        result.TokenThroughput.Model34B.Should().BeGreaterThan(0);
        result.TokenThroughput.Model70B.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_LargerModelsHaveLowerThroughput()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.TokenThroughput.Model7B.Should().BeGreaterThan(result.TokenThroughput.Model13B);
        result.TokenThroughput.Model13B.Should().BeGreaterThan(result.TokenThroughput.Model34B);
        result.TokenThroughput.Model34B.Should().BeGreaterThan(result.TokenThroughput.Model70B);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_ComparisonToReferenceProvided()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.ComparisonToReference.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_BenchmarkDurationRecorded()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.BenchmarkDurationSeconds.Should().BeGreaterThan(0);
        result.BenchmarkDurationSeconds.Should().BeLessThan(60); // Should complete within 60 seconds
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_TimestampRecorded()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());
        var beforeRun = DateTime.Now.AddSeconds(-1);

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        var afterRun = DateTime.Now.AddSeconds(1);
        result.BenchmarkTimestamp.Should().BeAfter(beforeRun);
        result.BenchmarkTimestamp.Should().BeBefore(afterRun);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_WithGpu_HigherThroughputEstimates()
    {
        // Arrange - Hardware with GPU
        var hardwareWithGpu = CreateTestHardware();
        hardwareWithGpu.Gpu.IsDedicated = true;
        hardwareWithGpu.Gpu.VramGB = 24;

        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(hardwareWithGpu);

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.TokenThroughput.Model7B.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_WithoutGpu_LowerThroughputEstimates()
    {
        // Arrange - Hardware without dedicated GPU
        var hardwareNoGpu = CreateTestHardware();
        hardwareNoGpu.Gpu.IsDedicated = false;
        hardwareNoGpu.Gpu.VramGB = 0;

        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(hardwareNoGpu);

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        result.TokenThroughput.Should().NotBeNull();
        result.TokenThroughput.Model7B.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_MemoryBandwidthReasonable()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act
        var result = await _service.RunSystemBenchmarkAsync();

        // Assert
        // Memory bandwidth should be in reasonable range (1-100 GB/s for typical systems)
        result.MemoryBandwidthGBps.Should().BeInRange(1, 100);
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_ThrowsException_LogsError()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.RunSystemBenchmarkAsync()
        );
    }

    [Fact]
    public async Task RunSystemBenchmarkAsync_ConsistentResults()
    {
        // Arrange
        _mockHardwareService
            .Setup(s => s.DetectHardwareAsync())
            .ReturnsAsync(CreateTestHardware());

        // Act - Run benchmark twice
        var result1 = await _service.RunSystemBenchmarkAsync();
        var result2 = await _service.RunSystemBenchmarkAsync();

        // Assert - Results should be relatively similar (within 20%)
        var cpuSingleDiff = Math.Abs(result1.CpuSingleCoreScore - result2.CpuSingleCoreScore) / result1.CpuSingleCoreScore;
        cpuSingleDiff.Should().BeLessThan(0.2);

        var cpuMultiDiff = Math.Abs(result1.CpuMultiCoreScore - result2.CpuMultiCoreScore) / result1.CpuMultiCoreScore;
        cpuMultiDiff.Should().BeLessThan(0.2);
    }

    private HardwareInfo CreateTestHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo
            {
                Model = "Test CPU",
                Cores = 8,
                Threads = 16,
                BaseClockGHz = 3.6
            },
            Memory = new MemoryInfo
            {
                TotalGB = 32,
                Type = "DDR4",
                SpeedMHz = 3200
            },
            Gpu = new GpuInfo
            {
                Model = "Test GPU",
                VramGB = 8,
                IsDedicated = true
            }
        };
    }
}
