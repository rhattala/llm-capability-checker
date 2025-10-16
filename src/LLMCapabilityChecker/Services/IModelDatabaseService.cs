using LLMCapabilityChecker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for managing LLM model database and recommendations
/// </summary>
public interface IModelDatabaseService
{
    /// <summary>
    /// Gets all available models in the database
    /// </summary>
    Task<List<ModelInfo>> GetAllModelsAsync();

    /// <summary>
    /// Gets models recommended for the specified hardware configuration
    /// </summary>
    /// <param name="hardware">Hardware information to match against</param>
    Task<List<ModelInfo>> GetRecommendedModelsAsync(HardwareInfo hardware);

    /// <summary>
    /// Gets a specific model by its name
    /// </summary>
    /// <param name="name">Model name to search for</param>
    Task<ModelInfo?> GetModelByNameAsync(string name);

    /// <summary>
    /// Gets models filtered by family (e.g., "Llama", "Mistral", "Phi")
    /// </summary>
    /// <param name="family">Model family name</param>
    Task<List<ModelInfo>> GetModelsByFamilyAsync(string family);
}
