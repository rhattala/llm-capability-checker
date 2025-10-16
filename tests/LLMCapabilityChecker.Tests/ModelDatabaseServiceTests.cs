using FluentAssertions;
using LLMCapabilityChecker.Models;
using LLMCapabilityChecker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LLMCapabilityChecker.Tests;

public class ModelDatabaseServiceTests
{
    private readonly Mock<ILogger<ModelDatabaseService>> _mockLogger;
    private readonly ModelDatabaseService _service;

    public ModelDatabaseServiceTests()
    {
        _mockLogger = new Mock<ILogger<ModelDatabaseService>>();
        _service = new ModelDatabaseService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetAllModelsAsync_ReturnsNonEmptyList()
    {
        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task GetAllModelsAsync_AllModelsHaveValidData()
    {
        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        foreach (var model in result)
        {
            model.Name.Should().NotBeNullOrEmpty();
            model.Family.Should().NotBeNullOrEmpty();
            model.ParameterSize.Should().NotBeNullOrEmpty();
            model.ParametersInBillions.Should().BeGreaterThan(0);
            model.Requirements.Should().NotBeNull();
            model.Requirements.QuantizationOptions.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetRecommendedModelsAsync_WithHighEndHardware_ReturnsMultipleModels()
    {
        // Arrange
        var hardware = CreateHighEndHardware();

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.All(m => m.IsRecommended).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecommendedModelsAsync_WithLowEndHardware_ReturnsLimitedModels()
    {
        // Arrange
        var hardware = CreateLowEndHardware();

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeNull();
        // Should only return small models
        result.All(m => m.ParametersInBillions <= 7).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecommendedModelsAsync_SortsModelsByCompatibilityScore()
    {
        // Arrange
        var hardware = CreateMidRangeHardware();

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeEmpty();
        // Verify descending order
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].CompatibilityScore.Should().BeGreaterThanOrEqualTo(result[i + 1].CompatibilityScore);
        }
    }

    [Fact]
    public async Task GetRecommendedModelsAsync_SetsExpectedPerformance()
    {
        // Arrange
        var hardware = CreateMidRangeHardware();

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeEmpty();
        result.All(m => !string.IsNullOrEmpty(m.ExpectedPerformance)).Should().BeTrue();
    }

    [Fact]
    public async Task GetModelByNameAsync_ExistingModel_ReturnsModel()
    {
        // Act
        var result = await _service.GetModelByNameAsync("Llama 3.1 8B");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Llama 3.1 8B");
        result.Family.Should().Be("Llama");
    }

    [Fact]
    public async Task GetModelByNameAsync_NonExistingModel_ReturnsNull()
    {
        // Act
        var result = await _service.GetModelByNameAsync("NonExistentModel");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetModelByNameAsync_CaseInsensitive_ReturnsModel()
    {
        // Act
        var result = await _service.GetModelByNameAsync("LLAMA 3.1 8B");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Llama 3.1 8B");
    }

    [Fact]
    public async Task GetModelsByFamilyAsync_LlamaFamily_ReturnsMultipleModels()
    {
        // Act
        var result = await _service.GetModelsByFamilyAsync("Llama");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.All(m => m.Family.Equals("Llama", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        result.Count.Should().BeGreaterThan(3);
    }

    [Fact]
    public async Task GetModelsByFamilyAsync_NonExistingFamily_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetModelsByFamilyAsync("NonExistentFamily");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(16, 16)]
    [InlineData(24, 24)]
    public async Task GetRecommendedModelsAsync_ChecksVramRequirements(int vramGB, int expectedMinVram)
    {
        // Arrange
        var hardware = CreateCustomHardware(vramGB, 32);

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeEmpty();
        // All recommended models should have at least one quantization option that fits
        foreach (var model in result)
        {
            model.Requirements.QuantizationOptions
                .Should().Contain(q => q.VramGB <= vramGB || q.RamGB <= 32);
        }
    }

    [Fact]
    public async Task GetRecommendedModelsAsync_WithNoGpu_ReturnsOnlyCpuCompatibleModels()
    {
        // Arrange
        var hardware = CreateLowEndHardware();
        hardware.Gpu.VramGB = 0;
        hardware.Gpu.IsDedicated = false;
        hardware.Memory.TotalGB = 32; // Enough RAM for CPU inference

        // Act
        var result = await _service.GetRecommendedModelsAsync(hardware);

        // Assert
        result.Should().NotBeEmpty();
        // Should have CPU-compatible models
        result.All(m => m.Requirements.QuantizationOptions.Any(q => q.RamGB <= 32)).Should().BeTrue();
    }

    [Theory]
    [InlineData("Phi-3 Mini 3.8B")]
    [InlineData("Gemma 2B")]
    [InlineData("Mistral 7B")]
    public async Task GetModelByNameAsync_PopularModels_ExistInDatabase(string modelName)
    {
        // Act
        var result = await _service.GetModelByNameAsync(modelName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(modelName);
    }

    [Theory]
    [InlineData("Llama")]
    [InlineData("Mistral")]
    [InlineData("Phi")]
    [InlineData("Gemma")]
    [InlineData("Qwen")]
    public async Task GetModelsByFamilyAsync_MajorFamilies_HaveModels(string family)
    {
        // Act
        var result = await _service.GetModelsByFamilyAsync(family);

        // Assert
        result.Should().NotBeEmpty($"Family {family} should have models in database");
    }

    private HardwareInfo CreateHighEndHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo { Cores = 16, BaseClockGHz = 4.5 },
            Memory = new MemoryInfo { TotalGB = 64 },
            Gpu = new GpuInfo
            {
                Model = "RTX 4090",
                VramGB = 24,
                IsDedicated = true
            }
        };
    }

    private HardwareInfo CreateMidRangeHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo { Cores = 8, BaseClockGHz = 3.6 },
            Memory = new MemoryInfo { TotalGB = 32 },
            Gpu = new GpuInfo
            {
                Model = "RTX 4060 Ti",
                VramGB = 16,
                IsDedicated = true
            }
        };
    }

    private HardwareInfo CreateLowEndHardware()
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo { Cores = 4, BaseClockGHz = 2.4 },
            Memory = new MemoryInfo { TotalGB = 8 },
            Gpu = new GpuInfo
            {
                Model = "Integrated",
                VramGB = 2,
                IsDedicated = false
            }
        };
    }

    private HardwareInfo CreateCustomHardware(int vramGB, int ramGB)
    {
        return new HardwareInfo
        {
            Cpu = new CpuInfo { Cores = 8, BaseClockGHz = 3.6 },
            Memory = new MemoryInfo { TotalGB = ramGB },
            Gpu = new GpuInfo
            {
                Model = "Custom GPU",
                VramGB = vramGB,
                IsDedicated = vramGB > 4
            }
        };
    }
}
