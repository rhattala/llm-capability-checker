using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for managing community model recommendations
/// </summary>
public class CommunityService : ICommunityService
{
    private readonly ILogger<CommunityService> _logger;
    private readonly string _communityDataPath;
    private readonly string _userSubmissionsPath;
    private readonly string _userVotesPath;
    private List<CommunityRecommendation>? _cachedRecommendations;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CommunityService(ILogger<CommunityService> logger)
    {
        _logger = logger;

        // Get application data directory
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LLMCapabilityChecker"
        );

        // Ensure directory exists
        Directory.CreateDirectory(appDataPath);

        // Set up file paths
        _communityDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "community-recommendations.json");
        _userSubmissionsPath = Path.Combine(appDataPath, "user-recommendations.json");
        _userVotesPath = Path.Combine(appDataPath, "user-votes.json");

        _logger.LogInformation("Community service initialized. Data path: {Path}", _communityDataPath);
    }

    /// <inheritdoc/>
    public async Task<List<CommunityRecommendation>> LoadCommunityRecommendationsAsync()
    {
        try
        {
            // Return cached if available
            if (_cachedRecommendations != null)
            {
                _logger.LogDebug("Returning cached community recommendations");
                return _cachedRecommendations;
            }

            // Check if community data file exists
            if (!File.Exists(_communityDataPath))
            {
                _logger.LogWarning("Community recommendations file not found at {Path}", _communityDataPath);
                return new List<CommunityRecommendation>();
            }

            // Read and parse JSON
            var json = await File.ReadAllTextAsync(_communityDataPath);
            var wrapper = JsonSerializer.Deserialize<CommunityRecommendationsWrapper>(json, JsonOptions);

            if (wrapper?.Recommendations == null)
            {
                _logger.LogWarning("Failed to parse community recommendations");
                return new List<CommunityRecommendation>();
            }

            _cachedRecommendations = wrapper.Recommendations;
            _logger.LogInformation("Loaded {Count} community recommendations", _cachedRecommendations.Count);

            return _cachedRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading community recommendations");
            return new List<CommunityRecommendation>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SubmitRecommendationAsync(ModelSubmission submission)
    {
        try
        {
            // Generate anonymous user ID if not provided
            if (string.IsNullOrEmpty(submission.UserId))
            {
                submission.UserId = GenerateAnonymousUserId();
            }

            submission.DateSubmitted = DateTime.UtcNow;

            // Load existing submissions
            var submissions = await LoadUserSubmissionsAsync();
            submissions.Add(submission);

            // Save to user submissions file
            var json = JsonSerializer.Serialize(new { submissions }, JsonOptions);
            await File.WriteAllTextAsync(_userSubmissionsPath, json);

            _logger.LogInformation("User submitted recommendation for {ModelName}", submission.ModelName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting recommendation");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<CommunityRecommendation>> GetTrendingModelsAsync(int daysBack = 30, int limit = 20)
    {
        try
        {
            var recommendations = await LoadCommunityRecommendationsAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

            var trending = recommendations
                .Where(r => r.DateAdded >= cutoffDate)
                .OrderByDescending(r => r.RecommendedByCount)
                .ThenByDescending(r => r.AverageRating)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Found {Count} trending models in last {Days} days", trending.Count, daysBack);

            return trending;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending models");
            return new List<CommunityRecommendation>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<CommunityRecommendation>> GetTopRatedAsync(int minRatings = 5, int limit = 20)
    {
        try
        {
            var recommendations = await LoadCommunityRecommendationsAsync();

            var topRated = recommendations
                .Where(r => r.TotalRatings >= minRatings)
                .OrderByDescending(r => r.AverageRating)
                .ThenByDescending(r => r.TotalRatings)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Found {Count} top-rated models (min {MinRatings} ratings)", topRated.Count, minRatings);

            return topRated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top-rated models");
            return new List<CommunityRecommendation>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<CommunityRecommendation>> FilterByHardwareTierAsync(string hardwareTier)
    {
        try
        {
            var recommendations = await LoadCommunityRecommendationsAsync();

            var filtered = recommendations
                .Where(r => r.CompatibleHardwareTier.Equals(hardwareTier, StringComparison.OrdinalIgnoreCase) ||
                           IsCompatibleWithTier(r, hardwareTier))
                .OrderByDescending(r => r.AverageRating)
                .ToList();

            _logger.LogDebug("Found {Count} models compatible with {Tier} tier", filtered.Count, hardwareTier);

            return filtered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering by hardware tier");
            return new List<CommunityRecommendation>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<CommunityRecommendation>> GetRecommendationsForHardwareAsync(HardwareInfo hardware)
    {
        try
        {
            var recommendations = await LoadCommunityRecommendationsAsync();
            var vramGB = hardware.Gpu.VramGB;
            var ramGB = hardware.Memory.TotalGB;

            var compatible = recommendations
                .Where(r => r.MinVramGB <= vramGB || r.MinRamGB <= ramGB)
                .OrderByDescending(r => r.AverageRating)
                .ThenByDescending(r => r.RecommendedByCount)
                .ToList();

            _logger.LogDebug("Found {Count} models compatible with user hardware (VRAM: {VRAM}GB, RAM: {RAM}GB)",
                compatible.Count, vramGB, ramGB);

            return compatible;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations for hardware");
            return new List<CommunityRecommendation>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpvoteRecommendationAsync(string modelName)
    {
        try
        {
            // Load user votes to check if already voted
            var votes = await LoadUserVotesAsync();
            var userId = GenerateAnonymousUserId();

            if (votes.TryGetValue(modelName, out var votedUsers) && votedUsers.Contains(userId))
            {
                _logger.LogDebug("User already upvoted {ModelName}", modelName);
                return false; // Already voted
            }

            // Add vote
            if (!votes.ContainsKey(modelName))
            {
                votes[modelName] = new HashSet<string>();
            }
            votes[modelName].Add(userId);

            // Save votes
            await SaveUserVotesAsync(votes);

            _logger.LogInformation("User upvoted {ModelName}", modelName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upvoting recommendation");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AddReviewAsync(string modelName, UserReview review)
    {
        try
        {
            // Generate anonymous user ID if not provided
            if (string.IsNullOrEmpty(review.UserId))
            {
                review.UserId = GenerateAnonymousUserId();
            }

            review.DatePosted = DateTime.UtcNow;

            // In a full implementation, this would update the community recommendations file
            // For now, we'll just log it
            _logger.LogInformation("User added review for {ModelName} with rating {Rating}",
                modelName, review.Rating);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding review");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ModelSubmission>> GetUserSubmissionsAsync()
    {
        return await LoadUserSubmissionsAsync();
    }

    #region Private Helper Methods

    private async Task<List<ModelSubmission>> LoadUserSubmissionsAsync()
    {
        try
        {
            if (!File.Exists(_userSubmissionsPath))
            {
                return new List<ModelSubmission>();
            }

            var json = await File.ReadAllTextAsync(_userSubmissionsPath);
            var wrapper = JsonSerializer.Deserialize<UserSubmissionsWrapper>(json, JsonOptions);

            return wrapper?.Submissions ?? new List<ModelSubmission>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user submissions");
            return new List<ModelSubmission>();
        }
    }

    private async Task<Dictionary<string, HashSet<string>>> LoadUserVotesAsync()
    {
        try
        {
            if (!File.Exists(_userVotesPath))
            {
                return new Dictionary<string, HashSet<string>>();
            }

            var json = await File.ReadAllTextAsync(_userVotesPath);
            var wrapper = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json, JsonOptions);

            return wrapper?.ToDictionary(
                kvp => kvp.Key,
                kvp => new HashSet<string>(kvp.Value)
            ) ?? new Dictionary<string, HashSet<string>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user votes");
            return new Dictionary<string, HashSet<string>>();
        }
    }

    private async Task SaveUserVotesAsync(Dictionary<string, HashSet<string>> votes)
    {
        try
        {
            var serializableVotes = votes.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );

            var json = JsonSerializer.Serialize(serializableVotes, JsonOptions);
            await File.WriteAllTextAsync(_userVotesPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user votes");
        }
    }

    private string GenerateAnonymousUserId()
    {
        // Generate a consistent anonymous ID based on machine characteristics
        var machineId = Environment.MachineName + Environment.UserName;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineId));
        return Convert.ToBase64String(hashBytes).Substring(0, 16);
    }

    private bool IsCompatibleWithTier(CommunityRecommendation recommendation, string tier)
    {
        // Hardware tier hierarchy: Entry < Mid < High < Enthusiast
        var tierOrder = new Dictionary<string, int>
        {
            { "Entry", 1 },
            { "Mid", 2 },
            { "High", 3 },
            { "Enthusiast", 4 }
        };

        if (!tierOrder.TryGetValue(recommendation.CompatibleHardwareTier, out var recTier) ||
            !tierOrder.TryGetValue(tier, out var userTier))
        {
            return false;
        }

        // A model is compatible if user's tier is equal or higher
        return userTier >= recTier;
    }

    #endregion

    #region JSON Wrapper Classes

    private class CommunityRecommendationsWrapper
    {
        public string Version { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public List<CommunityRecommendation> Recommendations { get; set; } = new();
    }

    private class UserSubmissionsWrapper
    {
        public List<ModelSubmission> Submissions { get; set; } = new();
    }

    #endregion
}
