using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Model database service with dynamic HuggingFace integration and local fallback
/// </summary>
public class ModelDatabaseService : IModelDatabaseService
{
    private readonly ILogger<ModelDatabaseService> _logger;
    private readonly HuggingFaceModelService? _huggingFaceService;
    private readonly List<ModelInfo> _models;
    private bool _dynamicModelsLoaded = false;

    public ModelDatabaseService(ILogger<ModelDatabaseService> logger, HuggingFaceModelService? huggingFaceService = null)
    {
        _logger = logger;
        _huggingFaceService = huggingFaceService;
        _models = InitializeModels();
        _logger.LogInformation("Initialized model database with {Count} local models", _models.Count);
    }

    /// <summary>
    /// Load models from HuggingFace API and merge with local database
    /// </summary>
    private async Task LoadDynamicModelsAsync()
    {
        if (_dynamicModelsLoaded || _huggingFaceService == null)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Fetching models from HuggingFace API...");
            var hfModels = await _huggingFaceService.FetchModelsAsync(limit: 50);

            if (hfModels.Count > 0)
            {
                var convertedModels = _huggingFaceService.ConvertToModelInfo(hfModels);

                // Merge with existing models (avoid duplicates)
                int addedCount = 0;
                foreach (var newModel in convertedModels)
                {
                    if (!_models.Any(m => m.Id == newModel.Id || m.Name.Equals(newModel.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _models.Add(newModel);
                        addedCount++;
                    }
                }

                _dynamicModelsLoaded = true;
                _logger.LogInformation("Added {Count} new models from HuggingFace. Total: {Total}", addedCount, _models.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dynamic models from HuggingFace, using local database only");
        }
    }

    public async Task<List<ModelInfo>> GetAllModelsAsync()
    {
        await LoadDynamicModelsAsync();
        return _models.ToList();
    }

    public async Task<List<ModelInfo>> GetRecommendedModelsAsync(HardwareInfo hardware)
    {
        await LoadDynamicModelsAsync();
        var recommended = new List<ModelInfo>();

        foreach (var model in _models)
        {
            var (isCompatible, score, performance) = EvaluateCompatibility(model, hardware);

            if (isCompatible)
            {
                // Clone the model and set recommendation properties
                var recommendedModel = CloneModel(model);
                recommendedModel.IsRecommended = true;
                recommendedModel.CompatibilityScore = score;
                recommendedModel.ExpectedPerformance = performance;
                recommended.Add(recommendedModel);
            }
        }

        // Sort by compatibility score descending
        recommended.Sort((a, b) => b.CompatibilityScore.CompareTo(a.CompatibilityScore));

        _logger.LogInformation(
            "Found {Count} compatible models for hardware (VRAM: {Vram}GB, RAM: {Ram}GB)",
            recommended.Count,
            hardware.Gpu?.VramGB ?? 0,
            hardware.Memory.TotalGB);

        return recommended;
    }

    public Task<ModelInfo?> GetModelByNameAsync(string name)
    {
        var model = _models.FirstOrDefault(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(model);
    }

    public Task<List<ModelInfo>> GetModelsByFamilyAsync(string family)
    {
        var models = _models
            .Where(m => m.Family.Equals(family, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(models);
    }

    /// <summary>
    /// Evaluates model compatibility with hardware
    /// Returns (isCompatible, compatibilityScore, expectedPerformance)
    /// </summary>
    private (bool, int, string) EvaluateCompatibility(ModelInfo model, HardwareInfo hardware)
    {
        int score = 0;
        bool hasGpu = hardware.Gpu != null && hardware.Gpu.VramGB > 0;

        // Check minimum requirements for different quantization options
        bool canRunOnGpu = false;
        bool canRunOnCpu = false;
        QuantizationOption? bestGpuQuant = null;
        QuantizationOption? bestCpuQuant = null;

        foreach (var quant in model.Requirements.QuantizationOptions)
        {
            // Check GPU compatibility
            if (hasGpu && hardware.Gpu!.VramGB >= quant.VramGB)
            {
                canRunOnGpu = true;
                if (bestGpuQuant == null || quant.BitsPerWeight > bestGpuQuant.BitsPerWeight)
                {
                    bestGpuQuant = quant;
                }
            }

            // Check CPU compatibility
            if (hardware.Memory.TotalGB >= quant.RamGB)
            {
                canRunOnCpu = true;
                if (bestCpuQuant == null || quant.BitsPerWeight > bestCpuQuant.BitsPerWeight)
                {
                    bestCpuQuant = quant;
                }
            }
        }

        // If model can't run on either GPU or CPU, it's not compatible
        if (!canRunOnGpu && !canRunOnCpu)
        {
            return (false, 0, "Not Compatible");
        }

        // Calculate compatibility score (0-100)
        if (canRunOnGpu && bestGpuQuant != null)
        {
            // GPU inference preferred
            int vramHeadroom = hardware.Gpu!.VramGB - bestGpuQuant.VramGB;
            score = 60; // Base score for GPU compatibility

            // Add points for VRAM headroom
            score += Math.Min(vramHeadroom * 5, 30);

            // Add points for higher quantization quality
            if (bestGpuQuant.BitsPerWeight >= 8)
                score += 10;
            else if (bestGpuQuant.BitsPerWeight >= 5)
                score += 5;

            string performance = vramHeadroom >= 4 ? "Excellent" :
                               vramHeadroom >= 2 ? "Good" : "Moderate";

            return (true, score, performance);
        }
        else if (canRunOnCpu && bestCpuQuant != null)
        {
            // CPU inference (slower but works)
            int ramHeadroom = hardware.Memory.TotalGB - bestCpuQuant.RamGB;
            score = 40; // Base score for CPU compatibility

            // Add points for RAM headroom
            score += Math.Min(ramHeadroom * 3, 20);

            // Bonus for good CPU
            if (hardware.Cpu.Cores >= 8)
                score += 10;

            string performance = hardware.Cpu.Cores >= 16 && ramHeadroom >= 8 ? "Good" :
                               hardware.Cpu.Cores >= 8 && ramHeadroom >= 4 ? "Moderate" : "Slow";

            return (true, score, performance);
        }

        return (false, 0, "Not Compatible");
    }

    /// <summary>
    /// Creates a shallow clone of a model for modification
    /// </summary>
    private ModelInfo CloneModel(ModelInfo source)
    {
        return new ModelInfo
        {
            Name = source.Name,
            Family = source.Family,
            ParameterSize = source.ParameterSize,
            ParametersInBillions = source.ParametersInBillions,
            Requirements = source.Requirements,
            Tags = source.Tags.ToList(),
            Description = source.Description,
            Url = source.Url,
            License = source.License,
            IsRecommended = source.IsRecommended,
            CompatibilityScore = source.CompatibilityScore,
            ExpectedPerformance = source.ExpectedPerformance
        };
    }

    /// <summary>
    /// Initialize hardcoded model database
    /// </summary>
    private List<ModelInfo> InitializeModels()
    {
        return new List<ModelInfo>
        {
            // Llama 3.1 Family
            new ModelInfo
            {
                Name = "Llama 3.1 8B",
                Family = "Llama",
                ParameterSize = "8B",
                ParametersInBillions = 8.0,
                Description = "Meta's latest Llama 3.1 model with 8 billion parameters. Excellent for general-purpose tasks, coding, and conversational AI.",
                License = "Llama 3.1 Community License",
                Url = "https://huggingface.co/meta-llama/Meta-Llama-3.1-8B",
                Tags = new List<string> { "chat", "coding", "general-purpose", "beginner-friendly" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    RecommendedVramGB = 10,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 6, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 7, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 10, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Llama 3.1 70B",
                Family = "Llama",
                ParameterSize = "70B",
                ParametersInBillions = 70.0,
                Description = "Large Llama 3.1 model with exceptional reasoning and coding capabilities. Requires high-end hardware.",
                License = "Llama 3.1 Community License",
                Url = "https://huggingface.co/meta-llama/Meta-Llama-3.1-70B",
                Tags = new List<string> { "chat", "coding", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 40,
                    RecommendedVramGB = 80,
                    MinRamGB = 80,
                    RecommendedRamGB = 128,
                    MinStorageGB = 40,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 40, RamGB = 80, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 50, RamGB = 96, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 75, RamGB = 128, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 16,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // Mistral Family
            new ModelInfo
            {
                Name = "Mistral 7B",
                Family = "Mistral",
                ParameterSize = "7B",
                ParametersInBillions = 7.3,
                Description = "Mistral AI's flagship 7B model. Excellent performance-to-size ratio, great for coding and general tasks.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Mistral-7B-v0.1",
                Tags = new List<string> { "chat", "coding", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 8,
                    RecommendedRamGB = 12,
                    MinStorageGB = 4,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 8, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 10, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ExLlama" }
                    }
                }
            },

            // Phi-3 Family
            new ModelInfo
            {
                Name = "Phi-3 Mini 3.8B",
                Family = "Phi",
                ParameterSize = "3.8B",
                ParametersInBillions = 3.8,
                Description = "Microsoft's compact yet powerful model. Excellent for devices with limited resources. Great reasoning capabilities.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct",
                Tags = new List<string> { "chat", "reasoning", "lightweight", "beginner-friendly", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 3,
                    RecommendedVramGB = 6,
                    MinRamGB = 6,
                    RecommendedRamGB = 8,
                    MinStorageGB = 3,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 3, RamGB = 6, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 4, RamGB = 7, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 5, RamGB = 8, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ONNX Runtime" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Phi-3 Medium 14B",
                Family = "Phi",
                ParameterSize = "14B",
                ParametersInBillions = 14.0,
                Description = "Larger Phi-3 model with enhanced capabilities while maintaining efficiency. Great for mid-range systems.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-3-medium-4k-instruct",
                Tags = new List<string> { "chat", "reasoning", "coding", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 10,
                    RecommendedVramGB = 16,
                    MinRamGB = 16,
                    RecommendedRamGB = 24,
                    MinStorageGB = 10,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 10, RamGB = 16, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 12, RamGB = 20, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 16, RamGB = 24, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 6,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ONNX Runtime" }
                    }
                }
            },

            // Gemma Family
            new ModelInfo
            {
                Name = "Gemma 2B",
                Family = "Gemma",
                ParameterSize = "2B",
                ParametersInBillions = 2.0,
                Description = "Google's ultra-compact model. Perfect for low-resource environments and edge devices. Surprisingly capable.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-2b",
                Tags = new List<string> { "chat", "lightweight", "beginner-friendly", "edge-device" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 2,
                    RecommendedVramGB = 4,
                    MinRamGB = 4,
                    RecommendedRamGB = 6,
                    MinStorageGB = 2,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 2, RamGB = 4, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 2, RamGB = 5, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 3, RamGB = 6, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Gemma 7B",
                Family = "Gemma",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Google's 7B model with strong performance across various tasks. Good balance of capability and resource usage.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-7b",
                Tags = new List<string> { "chat", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "TGI" }
                    }
                }
            },

            // Qwen Family
            new ModelInfo
            {
                Name = "Qwen2 7B",
                Family = "Qwen",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Alibaba's Qwen2 model with excellent multilingual support. Strong coding and math capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2-7B",
                Tags = new List<string> { "chat", "coding", "multilingual", "math" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // CodeLlama
            new ModelInfo
            {
                Name = "CodeLlama 7B",
                Family = "Llama",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Meta's specialized model for code generation and understanding. Excellent for programming tasks.",
                License = "Llama 2 Community License",
                Url = "https://huggingface.co/codellama/CodeLlama-7b-hf",
                Tags = new List<string> { "coding", "code-completion", "specialized" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            // DeepSeek Coder
            new ModelInfo
            {
                Name = "DeepSeek Coder 6.7B",
                Family = "DeepSeek",
                ParameterSize = "6.7B",
                ParametersInBillions = 6.7,
                Description = "DeepSeek's coding-focused model. Excellent for code generation, debugging, and technical tasks.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/deepseek-coder-6.7b-base",
                Tags = new List<string> { "coding", "technical", "specialized" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM" }
                    }
                }
            },

            // === Additional Llama Family Models ===

            new ModelInfo
            {
                Name = "Llama 3.1 405B",
                Family = "Llama",
                ParameterSize = "405B",
                ParametersInBillions = 405.0,
                Description = "Meta's largest Llama 3.1 model with exceptional capabilities. Requires enterprise-grade hardware with multiple high-end GPUs.",
                License = "Llama 3.1 Community License",
                Url = "https://huggingface.co/meta-llama/Meta-Llama-3.1-405B",
                Tags = new List<string> { "chat", "coding", "reasoning", "research", "enterprise" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 200,
                    RecommendedVramGB = 400,
                    MinRamGB = 400,
                    RecommendedRamGB = 512,
                    MinStorageGB = 250,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 200, RamGB = 400, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 250, RamGB = 450, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 350, RamGB = 512, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 32,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Llama 3.2 1B",
                Family = "Llama",
                ParameterSize = "1B",
                ParametersInBillions = 1.0,
                Description = "Ultra-lightweight Llama 3.2 model for edge devices and mobile. Great for on-device AI applications.",
                License = "Llama 3.2 Community License",
                Url = "https://huggingface.co/meta-llama/Llama-3.2-1B",
                Tags = new List<string> { "chat", "lightweight", "edge-device", "mobile" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 1,
                    RecommendedVramGB = 2,
                    MinRamGB = 2,
                    RecommendedRamGB = 4,
                    MinStorageGB = 1,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 1, RamGB = 2, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 1, RamGB = 3, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 2, RamGB = 4, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Llama 3.2 3B",
                Family = "Llama",
                ParameterSize = "3B",
                ParametersInBillions = 3.0,
                Description = "Compact Llama 3.2 model balancing performance and efficiency. Ideal for resource-constrained environments.",
                License = "Llama 3.2 Community License",
                Url = "https://huggingface.co/meta-llama/Llama-3.2-3B",
                Tags = new List<string> { "chat", "lightweight", "efficient", "beginner-friendly" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 2,
                    RecommendedVramGB = 4,
                    MinRamGB = 4,
                    RecommendedRamGB = 8,
                    MinStorageGB = 2,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 2, RamGB = 4, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 3, RamGB = 6, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 4, RamGB = 8, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "CodeLlama 13B",
                Family = "Llama",
                ParameterSize = "13B",
                ParametersInBillions = 13.0,
                Description = "Mid-size CodeLlama with enhanced code understanding and generation capabilities.",
                License = "Llama 2 Community License",
                Url = "https://huggingface.co/codellama/CodeLlama-13b-hf",
                Tags = new List<string> { "coding", "code-completion", "specialized" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 8,
                    RecommendedVramGB = 12,
                    MinRamGB = 16,
                    RecommendedRamGB = 24,
                    MinStorageGB = 8,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 8, RamGB = 16, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 10, RamGB = 20, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 14, RamGB = 28, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "CodeLlama 34B",
                Family = "Llama",
                ParameterSize = "34B",
                ParametersInBillions = 34.0,
                Description = "Large CodeLlama model with professional-grade coding capabilities. Excellent for complex projects.",
                License = "Llama 2 Community License",
                Url = "https://huggingface.co/codellama/CodeLlama-34b-hf",
                Tags = new List<string> { "coding", "code-completion", "specialized", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 20,
                    RecommendedVramGB = 32,
                    MinRamGB = 40,
                    RecommendedRamGB = 64,
                    MinStorageGB = 20,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 20, RamGB = 40, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 25, RamGB = 50, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 35, RamGB = 70, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Additional Mistral Family Models ===

            new ModelInfo
            {
                Name = "Mixtral 8x7B",
                Family = "Mistral",
                ParameterSize = "8x7B (47B)",
                ParametersInBillions = 47.0,
                Description = "Mistral's Mixture of Experts model with exceptional performance. Uses sparse activation for efficiency.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Mixtral-8x7B-v0.1",
                Tags = new List<string> { "chat", "coding", "reasoning", "moe", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 30,
                    RecommendedVramGB = 48,
                    MinRamGB = 60,
                    RecommendedRamGB = 96,
                    MinStorageGB = 30,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 30, RamGB = 60, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 38, RamGB = 75, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 50, RamGB = 100, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 12,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Mixtral 8x22B",
                Family = "Mistral",
                ParameterSize = "8x22B (141B)",
                ParametersInBillions = 141.0,
                Description = "Mistral's largest MoE model with state-of-the-art performance. Requires high-end hardware.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Mixtral-8x22B-v0.1",
                Tags = new List<string> { "chat", "coding", "reasoning", "moe", "advanced", "research" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 80,
                    RecommendedVramGB = 120,
                    MinRamGB = 160,
                    RecommendedRamGB = 256,
                    MinStorageGB = 90,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 80, RamGB = 160, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 100, RamGB = 200, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 140, RamGB = 280, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 24,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "llama.cpp" }
                    }
                }
            },

            // === Additional Phi Family Models ===

            new ModelInfo
            {
                Name = "Phi-3 Small 7B",
                Family = "Phi",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Microsoft's Phi-3 Small model with strong reasoning and general capabilities.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-3-small-8k-instruct",
                Tags = new List<string> { "chat", "reasoning", "coding", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ONNX Runtime" }
                    }
                }
            },

            // === Additional Gemma Family Models ===

            new ModelInfo
            {
                Name = "Gemma 9B",
                Family = "Gemma",
                ParameterSize = "9B",
                ParametersInBillions = 9.0,
                Description = "Google's mid-size Gemma model with enhanced capabilities. Strong performance across various tasks.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-9b",
                Tags = new List<string> { "chat", "coding", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    RecommendedVramGB = 10,
                    MinRamGB = 12,
                    RecommendedRamGB = 20,
                    MinStorageGB = 6,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 6, RamGB = 12, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 8, RamGB = 16, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 11, RamGB = 22, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Gemma 27B",
                Family = "Gemma",
                ParameterSize = "27B",
                ParametersInBillions = 27.0,
                Description = "Google's large Gemma model with advanced reasoning and generation capabilities.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-27b",
                Tags = new List<string> { "chat", "coding", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 16,
                    RecommendedVramGB = 24,
                    MinRamGB = 32,
                    RecommendedRamGB = 48,
                    MinStorageGB = 16,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 16, RamGB = 32, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 20, RamGB = 40, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 28, RamGB = 56, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Additional Qwen Family Models ===

            new ModelInfo
            {
                Name = "Qwen2.5 0.5B",
                Family = "Qwen",
                ParameterSize = "0.5B",
                ParametersInBillions = 0.5,
                Description = "Ultra-lightweight Qwen 2.5 model for edge devices. Surprisingly capable for its size.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-0.5B",
                Tags = new List<string> { "chat", "lightweight", "edge-device", "multilingual" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 1,
                    RecommendedVramGB = 2,
                    MinRamGB = 2,
                    RecommendedRamGB = 4,
                    MinStorageGB = 1,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 1, RamGB = 2, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 1, RamGB = 2, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 1, RamGB = 3, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Qwen2.5 1.5B",
                Family = "Qwen",
                ParameterSize = "1.5B",
                ParametersInBillions = 1.5,
                Description = "Compact Qwen 2.5 model with good multilingual support. Efficient for basic tasks.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-1.5B",
                Tags = new List<string> { "chat", "lightweight", "multilingual", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 1,
                    RecommendedVramGB = 2,
                    MinRamGB = 3,
                    RecommendedRamGB = 6,
                    MinStorageGB = 1,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 1, RamGB = 3, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 2, RamGB = 4, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 2, RamGB = 6, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Qwen2.5 3B",
                Family = "Qwen",
                ParameterSize = "3B",
                ParametersInBillions = 3.0,
                Description = "Balanced Qwen 2.5 model with strong multilingual capabilities. Good for general tasks.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-3B",
                Tags = new List<string> { "chat", "coding", "multilingual", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 2,
                    RecommendedVramGB = 4,
                    MinRamGB = 4,
                    RecommendedRamGB = 8,
                    MinStorageGB = 2,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 2, RamGB = 4, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 3, RamGB = 6, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 4, RamGB = 8, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Qwen2.5 14B",
                Family = "Qwen",
                ParameterSize = "14B",
                ParametersInBillions = 14.0,
                Description = "Mid-size Qwen 2.5 model with enhanced coding and reasoning capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-14B",
                Tags = new List<string> { "chat", "coding", "reasoning", "multilingual" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 9,
                    RecommendedVramGB = 14,
                    MinRamGB = 18,
                    RecommendedRamGB = 28,
                    MinStorageGB = 9,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 9, RamGB = 18, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 11, RamGB = 22, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 16, RamGB = 32, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 6,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Qwen2.5 32B",
                Family = "Qwen",
                ParameterSize = "32B",
                ParametersInBillions = 32.0,
                Description = "Large Qwen 2.5 model with professional-grade capabilities. Excellent for complex tasks.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-32B",
                Tags = new List<string> { "chat", "coding", "reasoning", "multilingual", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 18,
                    RecommendedVramGB = 28,
                    MinRamGB = 36,
                    RecommendedRamGB = 64,
                    MinStorageGB = 18,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 18, RamGB = 36, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 22, RamGB = 48, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 32, RamGB = 64, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Qwen2.5 72B",
                Family = "Qwen",
                ParameterSize = "72B",
                ParametersInBillions = 72.0,
                Description = "Flagship Qwen 2.5 model with exceptional multilingual and coding abilities. Requires high-end hardware.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/Qwen/Qwen2.5-72B",
                Tags = new List<string> { "chat", "coding", "reasoning", "multilingual", "advanced", "research" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 42,
                    RecommendedVramGB = 80,
                    MinRamGB = 84,
                    RecommendedRamGB = 128,
                    MinStorageGB = 42,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 42, RamGB = 84, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 52, RamGB = 100, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 80, RamGB = 160, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 16,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "llama.cpp" }
                    }
                }
            },

            // === Additional DeepSeek Family Models ===

            new ModelInfo
            {
                Name = "DeepSeek Coder 1.3B",
                Family = "DeepSeek",
                ParameterSize = "1.3B",
                ParametersInBillions = 1.3,
                Description = "Ultra-compact DeepSeek Coder. Great for quick code tasks on limited hardware.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/deepseek-coder-1.3b-base",
                Tags = new List<string> { "coding", "lightweight", "specialized" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 1,
                    RecommendedVramGB = 2,
                    MinRamGB = 3,
                    RecommendedRamGB = 6,
                    MinStorageGB = 1,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 1, RamGB = 3, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 1, RamGB = 4, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 2, RamGB = 6, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "DeepSeek Coder 33B",
                Family = "DeepSeek",
                ParameterSize = "33B",
                ParametersInBillions = 33.0,
                Description = "Large DeepSeek Coder with advanced code understanding. Professional-grade coding assistant.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/deepseek-coder-33b-base",
                Tags = new List<string> { "coding", "technical", "specialized", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 19,
                    RecommendedVramGB = 28,
                    MinRamGB = 38,
                    RecommendedRamGB = 64,
                    MinStorageGB = 19,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 19, RamGB = 38, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 24, RamGB = 48, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 34, RamGB = 68, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Yi Family Models ===

            new ModelInfo
            {
                Name = "Yi 6B",
                Family = "Yi",
                ParameterSize = "6B",
                ParametersInBillions = 6.0,
                Description = "01.AI's compact model with strong bilingual (English/Chinese) capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/01-ai/Yi-6B",
                Tags = new List<string> { "chat", "multilingual", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 4,
                    RecommendedVramGB = 8,
                    MinRamGB = 8,
                    RecommendedRamGB = 12,
                    MinStorageGB = 4,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 4, RamGB = 8, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 5, RamGB = 10, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 7, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Yi 9B",
                Family = "Yi",
                ParameterSize = "9B",
                ParametersInBillions = 9.0,
                Description = "01.AI's balanced model with enhanced capabilities and bilingual support.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/01-ai/Yi-9B",
                Tags = new List<string> { "chat", "coding", "multilingual", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    RecommendedVramGB = 10,
                    MinRamGB = 12,
                    RecommendedRamGB = 18,
                    MinStorageGB = 6,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 6, RamGB = 12, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 7, RamGB = 14, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 10, RamGB = 20, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Yi 34B",
                Family = "Yi",
                ParameterSize = "34B",
                ParametersInBillions = 34.0,
                Description = "01.AI's large model with exceptional bilingual performance and reasoning abilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/01-ai/Yi-34B",
                Tags = new List<string> { "chat", "coding", "reasoning", "multilingual", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 20,
                    RecommendedVramGB = 32,
                    MinRamGB = 40,
                    RecommendedRamGB = 64,
                    MinStorageGB = 20,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 20, RamGB = 40, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 25, RamGB = 50, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 36, RamGB = 72, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Command R Family (Cohere) ===

            new ModelInfo
            {
                Name = "Command R 35B",
                Family = "Command R",
                ParameterSize = "35B",
                ParametersInBillions = 35.0,
                Description = "Cohere's Command R model optimized for RAG and enterprise use. Excellent at following instructions.",
                License = "CC-BY-NC-4.0",
                Url = "https://huggingface.co/CohereForAI/c4ai-command-r-v01",
                Tags = new List<string> { "chat", "rag", "enterprise", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 20,
                    RecommendedVramGB = 32,
                    MinRamGB = 40,
                    RecommendedRamGB = 64,
                    MinStorageGB = 20,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 20, RamGB = 40, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 26, RamGB = 52, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 38, RamGB = 76, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Command R+ 104B",
                Family = "Command R",
                ParameterSize = "104B",
                ParametersInBillions = 104.0,
                Description = "Cohere's flagship Command R+ model with state-of-the-art RAG and reasoning capabilities. Enterprise-grade.",
                License = "CC-BY-NC-4.0",
                Url = "https://huggingface.co/CohereForAI/c4ai-command-r-plus",
                Tags = new List<string> { "chat", "rag", "enterprise", "reasoning", "advanced", "research" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 60,
                    RecommendedVramGB = 100,
                    MinRamGB = 120,
                    RecommendedRamGB = 192,
                    MinStorageGB = 60,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 60, RamGB = 120, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 75, RamGB = 150, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 110, RamGB = 220, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 24,
                        SupportedBackends = new List<string> { "vLLM", "TGI" }
                    }
                }
            },

            // === Falcon Family ===

            new ModelInfo
            {
                Name = "Falcon 7B",
                Family = "Falcon",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "TII's Falcon model trained on high-quality data. Strong general-purpose capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/tiiuae/falcon-7b",
                Tags = new List<string> { "chat", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Falcon 40B",
                Family = "Falcon",
                ParameterSize = "40B",
                ParametersInBillions = 40.0,
                Description = "TII's large Falcon model with strong performance across diverse tasks. Trained on high-quality datasets.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/tiiuae/falcon-40b",
                Tags = new List<string> { "chat", "coding", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 24,
                    RecommendedVramGB = 40,
                    MinRamGB = 48,
                    RecommendedRamGB = 80,
                    MinStorageGB = 24,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 24, RamGB = 48, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 30, RamGB = 60, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 44, RamGB = 88, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 12,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "llama.cpp" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Falcon 180B",
                Family = "Falcon",
                ParameterSize = "180B",
                ParametersInBillions = 180.0,
                Description = "TII's largest Falcon model with exceptional capabilities. Requires enterprise-grade infrastructure.",
                License = "Falcon-180B TII License",
                Url = "https://huggingface.co/tiiuae/falcon-180B",
                Tags = new List<string> { "chat", "coding", "reasoning", "research", "enterprise", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 100,
                    RecommendedVramGB = 160,
                    MinRamGB = 200,
                    RecommendedRamGB = 320,
                    MinStorageGB = 100,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 100, RamGB = 200, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 125, RamGB = 250, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 180, RamGB = 360, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 32,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Falcon 11B",
                Family = "Falcon",
                ParameterSize = "11B",
                ParametersInBillions = 11.0,
                Description = "TII's mid-size Falcon model with balanced performance and efficiency.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/tiiuae/falcon-11b",
                Tags = new List<string> { "chat", "coding", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 7,
                    RecommendedVramGB = 12,
                    MinRamGB = 14,
                    RecommendedRamGB = 24,
                    MinStorageGB = 7,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 7, RamGB = 14, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 9, RamGB = 18, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 13, RamGB = 26, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Llama 4 Family (2025) ===

            new ModelInfo
            {
                Name = "Llama 4 Scout 8B",
                Family = "Llama",
                ParameterSize = "8B",
                ParametersInBillions = 8.0,
                Description = "Latest Llama 4 model for everyday tasks with improved efficiency.",
                License = "Llama 4 Community License",
                Url = "https://huggingface.co/meta-llama/Llama-4-Scout-8B",
                Tags = new List<string> { "chat", "coding", "general-purpose", "beginner-friendly" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    RecommendedVramGB = 10,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 5, RamGB = 8, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 6, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 8, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 10, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 16, RamGB = 20, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Llama 4 Maverick 70B",
                Family = "Llama",
                ParameterSize = "70B",
                ParametersInBillions = 70.0,
                Description = "Advanced Llama 4 model with enhanced reasoning and coding capabilities.",
                License = "Llama 4 Community License",
                Url = "https://huggingface.co/meta-llama/Llama-4-Maverick-70B",
                Tags = new List<string> { "chat", "coding", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 40,
                    RecommendedVramGB = 80,
                    MinRamGB = 80,
                    RecommendedRamGB = 128,
                    MinStorageGB = 40,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 35, RamGB = 60, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 40, RamGB = 80, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 50, RamGB = 96, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 75, RamGB = 128, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 140, RamGB = 175, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 16,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Llama 4 Behemoth 405B",
                Family = "Llama",
                ParameterSize = "405B",
                ParametersInBillions = 405.0,
                Description = "Massive Llama 4 model with state-of-the-art capabilities. Requires enterprise infrastructure.",
                License = "Llama 4 Community License",
                Url = "https://huggingface.co/meta-llama/Llama-4-Behemoth-405B",
                Tags = new List<string> { "chat", "coding", "reasoning", "research", "enterprise" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 200,
                    RecommendedVramGB = 400,
                    MinRamGB = 400,
                    RecommendedRamGB = 512,
                    MinStorageGB = 250,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 180, RamGB = 320, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 200, RamGB = 400, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 250, RamGB = 450, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 350, RamGB = 512, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 810, RamGB = 1012, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 32,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            // === Mistral 2025 Updates ===

            new ModelInfo
            {
                Name = "Mistral 7B v0.3",
                Family = "Mistral",
                ParameterSize = "7B",
                ParametersInBillions = 7.3,
                Description = "Latest Mistral 7B iteration with improved instruction following and reduced hallucinations.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Mistral-7B-v0.3",
                Tags = new List<string> { "chat", "coding", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 8,
                    RecommendedRamGB = 12,
                    MinStorageGB = 4,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 6, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 8, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 10, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 15, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ExLlama" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Mistral Small 22B",
                Family = "Mistral",
                ParameterSize = "22B",
                ParametersInBillions = 22.0,
                Description = "Mistral's mid-size model with strong reasoning and coding capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Mistral-Small-22B",
                Tags = new List<string> { "chat", "coding", "reasoning", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 13,
                    RecommendedVramGB = 20,
                    MinRamGB = 26,
                    RecommendedRamGB = 40,
                    MinStorageGB = 13,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 11, RamGB = 18, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 13, RamGB = 26, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 17, RamGB = 33, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 26, RamGB = 44, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 44, RamGB = 55, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Mistral Large 123B",
                Family = "Mistral",
                ParameterSize = "123B",
                ParametersInBillions = 123.0,
                Description = "Mistral's flagship large model with state-of-the-art performance for enterprise applications.",
                License = "Mistral Research License",
                Url = "https://huggingface.co/mistralai/Mistral-Large-123B",
                Tags = new List<string> { "chat", "coding", "reasoning", "enterprise", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 72,
                    RecommendedVramGB = 120,
                    MinRamGB = 144,
                    RecommendedRamGB = 240,
                    MinStorageGB = 72,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 62, RamGB = 100, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 72, RamGB = 144, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 90, RamGB = 185, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 140, RamGB = 246, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 246, RamGB = 308, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 24,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            // === Ministral Family ===

            new ModelInfo
            {
                Name = "Ministral 3B",
                Family = "Mistral",
                ParameterSize = "3B",
                ParametersInBillions = 3.0,
                Description = "Ultra-compact Mistral variant for edge devices and low-resource environments.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Ministral-3B",
                Tags = new List<string> { "chat", "lightweight", "edge-device", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 2,
                    RecommendedVramGB = 4,
                    MinRamGB = 4,
                    RecommendedRamGB = 8,
                    MinStorageGB = 2,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 2, RamGB = 3, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 2, RamGB = 4, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 3, RamGB = 5, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 4, RamGB = 8, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 6, RamGB = 8, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Ministral 8B",
                Family = "Mistral",
                ParameterSize = "8B",
                ParametersInBillions = 8.0,
                Description = "Compact Mistral model with enhanced efficiency and strong general capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/mistralai/Ministral-8B",
                Tags = new List<string> { "chat", "coding", "efficient", "general-purpose" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 7, RamGB = 12, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 10, RamGB = 16, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 16, RamGB = 20, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "vLLM" }
                    }
                }
            },

            // === Phi-3.5 & Phi-4 Family ===

            new ModelInfo
            {
                Name = "Phi-3.5 Mini 3.8B",
                Family = "Phi",
                ParameterSize = "3.8B",
                ParametersInBillions = 3.8,
                Description = "Microsoft's refined Phi-3.5 Mini with improved reasoning and multilingual support.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-3.5-mini-instruct",
                Tags = new List<string> { "chat", "reasoning", "lightweight", "beginner-friendly", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 3,
                    RecommendedVramGB = 6,
                    MinRamGB = 6,
                    RecommendedRamGB = 8,
                    MinStorageGB = 3,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 2, RamGB = 4, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 3, RamGB = 6, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 4, RamGB = 7, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 5, RamGB = 8, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 8, RamGB = 10, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ONNX Runtime" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Phi-3.5 MoE 16x3.8B",
                Family = "Phi",
                ParameterSize = "16x3.8B (42B)",
                ParametersInBillions = 42.0,
                Description = "Microsoft's Mixture of Experts Phi model with sparse activation for efficiency.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-3.5-MoE-instruct",
                Tags = new List<string> { "chat", "reasoning", "coding", "moe", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 25,
                    RecommendedVramGB = 40,
                    MinRamGB = 50,
                    RecommendedRamGB = 80,
                    MinStorageGB = 25,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 22, RamGB = 36, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 25, RamGB = 50, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 32, RamGB = 63, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 50, RamGB = 84, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 84, RamGB = 105, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 12,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "llama.cpp" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Phi-4 14B",
                Family = "Phi",
                ParameterSize = "14B",
                ParametersInBillions = 14.0,
                Description = "Microsoft's latest Phi-4 with enhanced reasoning, coding, and multilingual capabilities.",
                License = "MIT",
                Url = "https://huggingface.co/microsoft/Phi-4",
                Tags = new List<string> { "chat", "reasoning", "coding", "math", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 9,
                    RecommendedVramGB = 14,
                    MinRamGB = 18,
                    RecommendedRamGB = 28,
                    MinStorageGB = 9,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 7, RamGB = 12, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 9, RamGB = 18, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 12, RamGB = 21, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 16, RamGB = 28, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 28, RamGB = 35, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 6,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "ONNX Runtime", "vLLM" }
                    }
                }
            },

            // === Gemma 2 Family ===

            new ModelInfo
            {
                Name = "Gemma 2 2B",
                Family = "Gemma",
                ParameterSize = "2B",
                ParametersInBillions = 2.0,
                Description = "Google's enhanced Gemma 2 ultra-compact model with improved efficiency.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-2-2b",
                Tags = new List<string> { "chat", "lightweight", "beginner-friendly", "edge-device" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 2,
                    RecommendedVramGB = 4,
                    MinRamGB = 4,
                    RecommendedRamGB = 6,
                    MinStorageGB = 2,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 1, RamGB = 3, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 2, RamGB = 4, QualityImpact = "Low", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 2, RamGB = 5, QualityImpact = "Very Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 3, RamGB = 6, QualityImpact = "Minimal", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 4, RamGB = 5, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "5.0",
                        RequiresAvx2 = false,
                        BenefitsFromAvx512 = false,
                        MinCores = 2,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Gemma 2 9B",
                Family = "Gemma",
                ParameterSize = "9B",
                ParametersInBillions = 9.0,
                Description = "Google's Gemma 2 with balanced performance and enhanced instruction following.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-2-9b",
                Tags = new List<string> { "chat", "coding", "general-purpose", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 6,
                    RecommendedVramGB = 10,
                    MinRamGB = 12,
                    RecommendedRamGB = 18,
                    MinStorageGB = 6,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 5, RamGB = 8, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 6, RamGB = 12, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 8, RamGB = 14, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 11, RamGB = 18, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 18, RamGB = 23, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "LM Studio", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Gemma 2 27B",
                Family = "Gemma",
                ParameterSize = "27B",
                ParametersInBillions = 27.0,
                Description = "Google's large Gemma 2 with advanced capabilities for complex tasks.",
                License = "Gemma Terms of Use",
                Url = "https://huggingface.co/google/gemma-2-27b",
                Tags = new List<string> { "chat", "coding", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 16,
                    RecommendedVramGB = 24,
                    MinRamGB = 32,
                    RecommendedRamGB = 48,
                    MinStorageGB = 16,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 13, RamGB = 22, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 16, RamGB = 32, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 20, RamGB = 40, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 28, RamGB = 54, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 54, RamGB = 68, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === DeepSeek-V3 & DeepSeek-R1 ===

            new ModelInfo
            {
                Name = "DeepSeek-V3 671B",
                Family = "DeepSeek",
                ParameterSize = "671B (MoE)",
                ParametersInBillions = 671.0,
                Description = "DeepSeek's massive Mixture of Experts model with exceptional coding and reasoning capabilities.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/DeepSeek-V3",
                Tags = new List<string> { "chat", "coding", "reasoning", "moe", "research", "enterprise" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 320,
                    RecommendedVramGB = 500,
                    MinRamGB = 640,
                    RecommendedRamGB = 1000,
                    MinStorageGB = 320,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 280, RamGB = 470, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 320, RamGB = 640, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 400, RamGB = 800, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 540, RamGB = 1342, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 64,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "DeepSeek-Coder-V2 236B",
                Family = "DeepSeek",
                ParameterSize = "236B (MoE)",
                ParametersInBillions = 236.0,
                Description = "DeepSeek's advanced coding model with MoE architecture for professional development tasks.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/DeepSeek-Coder-V2-236B",
                Tags = new List<string> { "coding", "technical", "moe", "advanced", "specialized" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 130,
                    RecommendedVramGB = 200,
                    MinRamGB = 260,
                    RecommendedRamGB = 400,
                    MinStorageGB = 130,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 110, RamGB = 165, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 130, RamGB = 260, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 162, RamGB = 354, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 236, RamGB = 472, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 32,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "DeepSpeed" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "DeepSeek-R1 70B",
                Family = "DeepSeek",
                ParameterSize = "70B",
                ParametersInBillions = 70.0,
                Description = "DeepSeek's reasoning-focused model with enhanced chain-of-thought capabilities.",
                License = "MIT",
                Url = "https://huggingface.co/deepseek-ai/DeepSeek-R1-70B",
                Tags = new List<string> { "reasoning", "math", "coding", "advanced", "research" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 42,
                    RecommendedVramGB = 80,
                    MinRamGB = 84,
                    RecommendedRamGB = 128,
                    MinStorageGB = 42,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 35, RamGB = 60, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 42, RamGB = 84, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 52, RamGB = 105, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 80, RamGB = 140, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 140, RamGB = 175, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 16,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "llama.cpp" }
                    }
                }
            },

            // === Nemotron Family ===

            new ModelInfo
            {
                Name = "Nemotron-4 340B",
                Family = "Nemotron",
                ParameterSize = "340B",
                ParametersInBillions = 340.0,
                Description = "NVIDIA's flagship enterprise model optimized for RAG and synthetic data generation.",
                License = "NVIDIA Open Model License",
                Url = "https://huggingface.co/nvidia/Nemotron-4-340B",
                Tags = new List<string> { "chat", "rag", "enterprise", "reasoning", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 180,
                    RecommendedVramGB = 320,
                    MinRamGB = 360,
                    RecommendedRamGB = 640,
                    MinStorageGB = 180,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 160, RamGB = 240, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 180, RamGB = 360, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 225, RamGB = 510, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 340, RamGB = 680, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "8.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 32,
                        SupportedBackends = new List<string> { "vLLM", "TGI", "Triton" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Nemotron Mini 4B",
                Family = "Nemotron",
                ParameterSize = "4B",
                ParametersInBillions = 4.0,
                Description = "NVIDIA's compact Nemotron model optimized for edge deployment and low-latency inference.",
                License = "NVIDIA Open Model License",
                Url = "https://huggingface.co/nvidia/Nemotron-Mini-4B",
                Tags = new List<string> { "chat", "edge-device", "efficient", "lightweight" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 3,
                    RecommendedVramGB = 5,
                    MinRamGB = 6,
                    RecommendedRamGB = 10,
                    MinStorageGB = 3,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 2, RamGB = 4, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 3, RamGB = 6, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 4, RamGB = 8, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 6, RamGB = 10, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 8, RamGB = 10, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "Triton", "TensorRT-LLM" }
                    }
                }
            },

            // === Orca Family ===

            new ModelInfo
            {
                Name = "Orca 2 7B",
                Family = "Orca",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Microsoft's Orca 2 model trained with advanced reasoning techniques for complex problem solving.",
                License = "Microsoft Research License",
                Url = "https://huggingface.co/microsoft/Orca-2-7b",
                Tags = new List<string> { "reasoning", "chat", "math", "efficient" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 11, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 14, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Orca 2 13B",
                Family = "Orca",
                ParameterSize = "13B",
                ParametersInBillions = 13.0,
                Description = "Microsoft's mid-size Orca 2 with enhanced reasoning and step-by-step problem solving capabilities.",
                License = "Microsoft Research License",
                Url = "https://huggingface.co/microsoft/Orca-2-13b",
                Tags = new List<string> { "reasoning", "chat", "math", "coding", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 8,
                    RecommendedVramGB = 14,
                    MinRamGB = 16,
                    RecommendedRamGB = 26,
                    MinStorageGB = 8,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 7, RamGB = 11, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 8, RamGB = 16, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 10, RamGB = 20, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 14, RamGB = 26, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 26, RamGB = 33, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 6,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Nous Hermes Family ===

            new ModelInfo
            {
                Name = "Nous Hermes 2 Mixtral 8x7B",
                Family = "Nous Hermes",
                ParameterSize = "8x7B (47B)",
                ParametersInBillions = 47.0,
                Description = "NousResearch's fine-tuned Mixtral model with enhanced instruction following and roleplay capabilities.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/NousResearch/Nous-Hermes-2-Mixtral-8x7B",
                Tags = new List<string> { "chat", "roleplay", "coding", "moe", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 30,
                    RecommendedVramGB = 48,
                    MinRamGB = 60,
                    RecommendedRamGB = 96,
                    MinStorageGB = 30,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 26, RamGB = 42, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 30, RamGB = 60, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 38, RamGB = 71, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 50, RamGB = 94, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 94, RamGB = 118, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "7.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 12,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Nous Hermes 2 Yi 34B",
                Family = "Nous Hermes",
                ParameterSize = "34B",
                ParametersInBillions = 34.0,
                Description = "NousResearch's fine-tuned Yi 34B model with improved instruction following and multilingual support.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/NousResearch/Nous-Hermes-2-Yi-34B",
                Tags = new List<string> { "chat", "coding", "multilingual", "roleplay", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 20,
                    RecommendedVramGB = 32,
                    MinRamGB = 40,
                    RecommendedRamGB = 64,
                    MinStorageGB = 20,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 17, RamGB = 28, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 20, RamGB = 40, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 25, RamGB = 51, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 36, RamGB = 68, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 68, RamGB = 85, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === WizardLM Family ===

            new ModelInfo
            {
                Name = "WizardLM 2 7B",
                Family = "WizardLM",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Microsoft's WizardLM 2 trained with Evol-Instruct for complex instruction following.",
                License = "Microsoft Research License",
                Url = "https://huggingface.co/WizardLM/WizardLM-2-7B",
                Tags = new List<string> { "chat", "reasoning", "coding", "instruction-following" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 11, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 14, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "WizardCoder 33B",
                Family = "WizardLM",
                ParameterSize = "33B",
                ParametersInBillions = 33.0,
                Description = "Microsoft's specialized coding model with enhanced code generation and debugging capabilities.",
                License = "Microsoft Research License",
                Url = "https://huggingface.co/WizardLM/WizardCoder-33B-V1.1",
                Tags = new List<string> { "coding", "specialized", "debugging", "advanced" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 19,
                    RecommendedVramGB = 28,
                    MinRamGB = 38,
                    RecommendedRamGB = 64,
                    MinStorageGB = 19,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 16, RamGB = 26, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 19, RamGB = 38, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 24, RamGB = 50, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 34, RamGB = 66, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 66, RamGB = 83, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 8,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            // === Additional Specialized Models ===

            new ModelInfo
            {
                Name = "Solar 10.7B",
                Family = "Solar",
                ParameterSize = "10.7B",
                ParametersInBillions = 10.7,
                Description = "Upstage's Solar model using depth-up scaling for enhanced performance in a compact size.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/upstage/SOLAR-10.7B-v1.0",
                Tags = new List<string> { "chat", "coding", "efficient", "general-purpose" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 7,
                    RecommendedVramGB = 12,
                    MinRamGB = 14,
                    RecommendedRamGB = 22,
                    MinStorageGB = 7,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 6, RamGB = 10, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 7, RamGB = 14, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 9, RamGB = 16, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 13, RamGB = 21, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 21, RamGB = 27, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = true,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Starling 7B Alpha",
                Family = "Starling",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Berkeley's Starling model trained with RLAIF for improved helpfulness and harmlessness.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/berkeley-nest/Starling-LM-7B-alpha",
                Tags = new List<string> { "chat", "helpful", "safe", "general-purpose" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 11, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 14, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Openchat 3.5",
                Family = "Openchat",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "Open-source chatbot trained with C-RLFT for GPT-4 level conversational performance.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/openchat/openchat-3.5-0106",
                Tags = new List<string> { "chat", "conversational", "efficient", "general-purpose" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 11, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 14, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            },

            new ModelInfo
            {
                Name = "Zephyr 7B Beta",
                Family = "Zephyr",
                ParameterSize = "7B",
                ParametersInBillions = 7.0,
                Description = "HuggingFace's Zephyr model fine-tuned with DPO for improved alignment and helpfulness.",
                License = "Apache 2.0",
                Url = "https://huggingface.co/HuggingFaceH4/zephyr-7b-beta",
                Tags = new List<string> { "chat", "helpful", "aligned", "general-purpose" },
                Requirements = new ModelRequirements
                {
                    MinVramGB = 5,
                    RecommendedVramGB = 8,
                    MinRamGB = 10,
                    RecommendedRamGB = 16,
                    MinStorageGB = 5,
                    QuantizationOptions = new List<QuantizationOption>
                    {
                        new QuantizationOption { Format = "Q2_K", BitsPerWeight = 2, VramGB = 4, RamGB = 7, QualityImpact = "Medium", PerformanceTier = "Very Fast" },
                        new QuantizationOption { Format = "Q4_K_M", BitsPerWeight = 4, VramGB = 5, RamGB = 10, QualityImpact = "Low", PerformanceTier = "Fast" },
                        new QuantizationOption { Format = "Q5_K_M", BitsPerWeight = 5, VramGB = 6, RamGB = 11, QualityImpact = "Very Low", PerformanceTier = "Balanced" },
                        new QuantizationOption { Format = "Q8_0", BitsPerWeight = 8, VramGB = 8, RamGB = 14, QualityImpact = "Minimal", PerformanceTier = "High Quality" },
                        new QuantizationOption { Format = "FP16", BitsPerWeight = 16, VramGB = 14, RamGB = 18, QualityImpact = "None", PerformanceTier = "Maximum Quality" },
                    },
                    ComputeRequirements = new ComputeRequirements
                    {
                        MinCudaCompute = "6.0",
                        RequiresAvx2 = true,
                        BenefitsFromAvx512 = false,
                        MinCores = 4,
                        SupportedBackends = new List<string> { "llama.cpp", "Ollama", "vLLM", "TGI" }
                    }
                }
            }
        };
    }
}
