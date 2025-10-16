using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace LLMCapabilityChecker.Tests;

public class ReportExportServiceTests
{
    private readonly Mock<ILogger<ReportExportService>> _mockLogger;
    private readonly ReportExportService _service;

    public ReportExportServiceTests()
    {
        _mockLogger = new Mock<ILogger<ReportExportService>>();
        _service = new ReportExportService(_mockLogger.Object);
    }

    [Fact]
    public async Task ExportAsJsonAsync_ValidData_ReturnsValidJson()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsJsonAsync(hardware, scores, models);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Verify it's valid JSON
        var parseAction = () => JsonDocument.Parse(result);
        parseAction.Should().NotThrow();
    }

    [Fact]
    public async Task ExportAsJsonAsync_ContainsMetadata()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsJsonAsync(hardware, scores, models);
        var json = JsonDocument.Parse(result);

        // Assert
        json.RootElement.TryGetProperty("reportMetadata", out var metadata).Should().BeTrue();
        metadata.TryGetProperty("generatedAt", out _).Should().BeTrue();
        metadata.TryGetProperty("reportVersion", out _).Should().BeTrue();
        metadata.TryGetProperty("applicationName", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsJsonAsync_ContainsHardwareInfo()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsJsonAsync(hardware, scores, models);
        var json = JsonDocument.Parse(result);

        // Assert
        json.RootElement.TryGetProperty("hardwareInfo", out var hardwareInfo).Should().BeTrue();
        hardwareInfo.TryGetProperty("cpu", out _).Should().BeTrue();
        hardwareInfo.TryGetProperty("memory", out _).Should().BeTrue();
        hardwareInfo.TryGetProperty("gpu", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsJsonAsync_ContainsSystemScores()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsJsonAsync(hardware, scores, models);
        var json = JsonDocument.Parse(result);

        // Assert
        json.RootElement.TryGetProperty("systemScores", out var systemScores).Should().BeTrue();
        systemScores.TryGetProperty("overallScore", out _).Should().BeTrue();
        systemScores.TryGetProperty("breakdown", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsJsonAsync_ContainsRecommendedModels()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsJsonAsync(hardware, scores, models);
        var json = JsonDocument.Parse(result);

        // Assert
        json.RootElement.TryGetProperty("recommendedModels", out var recommendedModels).Should().BeTrue();
        recommendedModels.GetArrayLength().Should().Be(models.Count);
    }

    [Fact]
    public async Task ExportAsTextAsync_ValidData_ReturnsFormattedText()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("LLM CAPABILITY CHECKER REPORT");
        result.Should().Contain("SYSTEM OVERVIEW");
        result.Should().Contain("HARDWARE INFORMATION");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsSystemOverview()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("Overall Score:");
        result.Should().Contain("System Tier:");
        result.Should().Contain("Recommended Model Size:");
        result.Should().Contain("Primary Bottleneck:");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsCpuInfo()
    {
        // Arrange
        var hardware = CreateTestHardware();
        hardware.Cpu.Model = "AMD Ryzen 9 7950X";
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("CPU:");
        result.Should().Contain("AMD Ryzen 9 7950X");
        result.Should().Contain("Cores:");
        result.Should().Contain("Threads:");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsGpuInfo()
    {
        // Arrange
        var hardware = CreateTestHardware();
        hardware.Gpu.Model = "NVIDIA RTX 4090";
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("GPU:");
        result.Should().Contain("NVIDIA RTX 4090");
        result.Should().Contain("VRAM:");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsMemoryInfo()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("Memory:");
        result.Should().Contain("Total RAM:");
        result.Should().Contain("Type:");
        result.Should().Contain("Speed:");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsComponentScores()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("COMPONENT SCORES");
        result.Should().Contain("CPU Score:");
        result.Should().Contain("Memory Score:");
        result.Should().Contain("GPU Score:");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsRecommendedModels()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("RECOMMENDED MODELS");
        result.Should().Contain("Test Model 1");
        result.Should().Contain("Test Model 2");
    }

    [Fact]
    public async Task ExportAsTextAsync_EmptyModels_DisplaysNoModelsMessage()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = new List<ModelInfo>();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("No models recommended for this system configuration");
    }

    [Fact]
    public async Task ExportAsTextAsync_ContainsFrameworkInfo()
    {
        // Arrange
        var hardware = CreateTestHardware();
        hardware.Frameworks.HasCuda = true;
        hardware.Frameworks.CudaVersion = "12.0";
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("ML Frameworks:");
        result.Should().Contain("CUDA:");
        result.Should().Contain("Yes (12.0)");
    }

    [Fact]
    public async Task ExportAsJsonAsync_ThrowsException_LogsErrorAndThrows()
    {
        // Arrange - Pass null to cause exception
        HardwareInfo? nullHardware = null!;
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _service.ExportAsJsonAsync(nullHardware, scores, models)
        );
    }

    [Fact]
    public async Task ExportAsTextAsync_ThrowsException_LogsErrorAndThrows()
    {
        // Arrange - Pass null to cause exception
        HardwareInfo? nullHardware = null!;
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(
            async () => await _service.ExportAsTextAsync(nullHardware, scores, models)
        );
    }

    [Fact]
    public async Task ExportAsTextAsync_NumberedModelList()
    {
        // Arrange
        var hardware = CreateTestHardware();
        var scores = CreateTestScores();
        var models = CreateTestModels();

        // Act
        var result = await _service.ExportAsTextAsync(hardware, scores, models);

        // Assert
        result.Should().Contain("1. Test Model 1");
        result.Should().Contain("2. Test Model 2");
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
                BaseClockGHz = 3.6,
                Architecture = "x64",
                SupportsAvx2 = true
            },
            Memory = new MemoryInfo
            {
                TotalGB = 32,
                AvailableGB = 16,
                Type = "DDR4",
                SpeedMHz = 3200
            },
            Gpu = new GpuInfo
            {
                Model = "Test GPU",
                Vendor = "NVIDIA",
                VramGB = 16,
                ComputeCapability = "8.6",
                IsDedicated = true,
                SupportsFp16 = true,
                SupportsInt8 = true
            },
            Storage = new StorageInfo
            {
                Type = "NVMe",
                TotalGB = 1000,
                AvailableGB = 500,
                ReadSpeedMBps = 5000,
                WriteSpeedMBps = 4000
            },
            Frameworks = new FrameworkInfo
            {
                HasCuda = false,
                HasDirectMl = true
            },
            OperatingSystem = "Windows 11"
        };
    }

    private SystemScores CreateTestScores()
    {
        return new SystemScores
        {
            OverallScore = 75,
            SystemTier = "High-End",
            RecommendedModelSize = "13B",
            PrimaryBottleneck = "GPU (65/100)",
            Breakdown = new ScoreBreakdown
            {
                CpuScore = 70,
                MemoryScore = 80,
                GpuScore = 65,
                StorageScore = 85,
                FrameworkScore = 75
            }
        };
    }

    private List<ModelInfo> CreateTestModels()
    {
        return new List<ModelInfo>
        {
            new ModelInfo
            {
                Name = "Test Model 1",
                Family = "Test",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Test model description",
                Url = "https://example.com/model1",
                CompatibilityScore = 90,
                ExpectedPerformance = "Excellent",
                IsRecommended = true,
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    MinRamGB = 10,
                    MinStorageGB = 5
                }
            },
            new ModelInfo
            {
                Name = "Test Model 2",
                Family = "Test",
                ParameterSize = "13B",
                ParametersInBillions = 13.0,
                Description = "Another test model",
                Url = "https://example.com/model2",
                CompatibilityScore = 85,
                ExpectedPerformance = "Good",
                IsRecommended = true,
                Requirements = new ModelRequirements
                {
                    MinVramGB = 10,
                    MinRamGB = 16,
                    MinStorageGB = 8
                }
            }
        };
    }
}
