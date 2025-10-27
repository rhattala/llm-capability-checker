using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for fetching models from HuggingFace Model Hub API
/// </summary>
public class HuggingFaceModelService
{
    private readonly ILogger<HuggingFaceModelService> _logger;
    private readonly HttpClient _httpClient;
    private const string HF_API_BASE = "https://huggingface.co/api/models";

    // Cache to avoid excessive API calls
    private List<HuggingFaceModel>? _cachedModels;
    private DateTime _cacheTimestamp = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

    public HuggingFaceModelService(ILogger<HuggingFaceModelService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Fetches LLM models from HuggingFace API
    /// </summary>
    public async Task<List<HuggingFaceModel>> FetchModelsAsync(int limit = 100)
    {
        try
        {
            // Return cached data if still valid
            if (_cachedModels != null && DateTime.Now - _cacheTimestamp < _cacheExpiration)
            {
                _logger.LogInformation("Returning {Count} cached HuggingFace models", _cachedModels.Count);
                return _cachedModels;
            }

            _logger.LogInformation("Fetching models from HuggingFace API (limit: {Limit})", limit);

            // Fetch text-generation models sorted by downloads
            var url = $"{HF_API_BASE}?limit={limit}&filter=text-generation&sort=downloads&direction=-1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HuggingFace API returned {StatusCode}", response.StatusCode);
                return new List<HuggingFaceModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var models = JsonSerializer.Deserialize<List<HuggingFaceModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (models != null && models.Count > 0)
            {
                _cachedModels = models;
                _cacheTimestamp = DateTime.Now;
                _logger.LogInformation("Successfully fetched {Count} models from HuggingFace", models.Count);
                return models;
            }

            _logger.LogWarning("No models returned from HuggingFace API");
            return new List<HuggingFaceModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching HuggingFace models");
            return _cachedModels ?? new List<HuggingFaceModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching models from HuggingFace");
            return _cachedModels ?? new List<HuggingFaceModel>();
        }
    }

    /// <summary>
    /// Converts HuggingFace models to our internal ModelInfo format
    /// </summary>
    public List<ModelInfo> ConvertToModelInfo(List<HuggingFaceModel> hfModels)
    {
        var modelInfoList = new List<ModelInfo>();

        foreach (var hfModel in hfModels)
        {
            try
            {
                // Extract parameter count from model name or tags (e.g., "7B", "13B")
                var paramCount = ExtractParameterCount(hfModel.ModelId, hfModel.Tags);

                // Estimate VRAM requirements based on parameter count
                var vramEstimate = EstimateVramRequirement(paramCount);

                var modelInfo = new ModelInfo
                {
                    Id = hfModel.ModelId.Replace("/", "-").ToLower(),
                    Name = hfModel.ModelId.Split('/').Last(),
                    Family = ExtractModelFamily(hfModel.ModelId),
                    ParametersInBillions = paramCount,
                    ParameterSize = $"{paramCount}B",
                    //Provider = hfModel.ModelId.Split('/').FirstOrDefault() ?? "HuggingFace",
                    License = "Unknown", // HF API doesn't provide license in list endpoint
                    Tags = new List<string> { "general", "chat" },
                    //PopularityRank = 0, // Will be set based on downloads
                    Url = $"https://huggingface.co/{hfModel.ModelId}",
                    HuggingFaceUrl = $"https://huggingface.co/{hfModel.ModelId}",
                    Requirements = CreateDefaultRequirements(vramEstimate, paramCount),
                    Source = "HuggingFace"
                };

                modelInfoList.Add(modelInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error converting HuggingFace model: {ModelId}", hfModel.ModelId);
            }
        }

        return modelInfoList;
    }

    private double ExtractParameterCount(string modelId, List<string> tags)
    {
        // Try to extract from model name (e.g., "llama-7b", "mistral-7B-Instruct")
        var lowerName = modelId.ToLower();

        if (lowerName.Contains("405b")) return 405;
        if (lowerName.Contains("70b")) return 70;
        if (lowerName.Contains("34b")) return 34;
        if (lowerName.Contains("33b")) return 33;
        if (lowerName.Contains("13b")) return 13;
        if (lowerName.Contains("8b")) return 8;
        if (lowerName.Contains("7b")) return 7;
        if (lowerName.Contains("3b")) return 3;
        if (lowerName.Contains("1b")) return 1;
        if (lowerName.Contains("0.5b") || lowerName.Contains("500m")) return 0.5;

        // Check tags for parameter info
        foreach (var tag in tags)
        {
            var lowerTag = tag.ToLower();
            if (lowerTag.Contains("7b")) return 7;
            if (lowerTag.Contains("13b")) return 13;
        }

        // Default to 7B if unknown
        return 7;
    }

    private string ExtractModelFamily(string modelId)
    {
        var lowerName = modelId.ToLower();

        if (lowerName.Contains("llama")) return "llama";
        if (lowerName.Contains("mistral")) return "mistral";
        if (lowerName.Contains("phi")) return "phi";
        if (lowerName.Contains("gemma")) return "gemma";
        if (lowerName.Contains("qwen")) return "qwen";
        if (lowerName.Contains("deepseek")) return "deepseek";
        if (lowerName.Contains("gpt")) return "gpt";
        if (lowerName.Contains("falcon")) return "falcon";

        return "other";
    }

    private int EstimateVramRequirement(double paramCount)
    {
        // Rough estimate: 2GB per billion parameters for fp16/bf16
        // Add 20% overhead for KV cache and activations
        return (int)Math.Ceiling(paramCount * 2 * 1.2);
    }

    private ModelRequirements CreateDefaultRequirements(int vramGb, double paramCount)
    {
        return new ModelRequirements
        {
            MinVramGB = vramGb / 2,  // Assume Q4 quantization as minimum
            RecommendedVramGB = vramGb,
            MinRamGB = vramGb,
            RecommendedRamGB = vramGb * 2,
            MinStorageGB = (int)(paramCount * 2),
            QuantizationOptions = new List<QuantizationOption>
            {
                new QuantizationOption
                {
                    Format = "Q4_K_M",
                    BitsPerWeight = 4,
                    VramGB = vramGb / 2,
                    RamGB = vramGb,
                    QualityImpact = "Low",
                    PerformanceTier = "Fast"
                },
                new QuantizationOption
                {
                    Format = "FP16",
                    BitsPerWeight = 16,
                    VramGB = vramGb,
                    RamGB = vramGb * 2,
                    QualityImpact = "None",
                    PerformanceTier = "Maximum"
                }
            },
            ComputeRequirements = new ComputeRequirements
            {
                MinCudaCompute = "6.0",
                RequiresAvx2 = true,
                BenefitsFromAvx512 = true,
                MinCores = 4,
                SupportedBackends = new List<string> { "llama.cpp", "Ollama", "HuggingFace" }
            }
        };
    }
}

/// <summary>
/// HuggingFace API model response structure
/// </summary>
public class HuggingFaceModel
{
    public string Id { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Downloads { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Pipeline_Tag { get; set; } = string.Empty;
    public string Library_Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
