using LLMCapabilityChecker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for managing community model recommendations
/// </summary>
public interface ICommunityService
{
    /// <summary>
    /// Loads community recommendations from JSON file or API
    /// </summary>
    /// <returns>List of community recommendations</returns>
    Task<List<CommunityRecommendation>> LoadCommunityRecommendationsAsync();

    /// <summary>
    /// Submits a new model recommendation from a user
    /// </summary>
    /// <param name="submission">User's model submission</param>
    /// <returns>True if submission was successful</returns>
    Task<bool> SubmitRecommendationAsync(ModelSubmission submission);

    /// <summary>
    /// Gets trending models (most recommended in last 30 days)
    /// </summary>
    /// <param name="daysBack">Number of days to look back (default 30)</param>
    /// <param name="limit">Maximum number of results (default 20)</param>
    /// <returns>List of trending recommendations</returns>
    Task<List<CommunityRecommendation>> GetTrendingModelsAsync(int daysBack = 30, int limit = 20);

    /// <summary>
    /// Gets top-rated models by average rating
    /// </summary>
    /// <param name="minRatings">Minimum number of ratings required (default 5)</param>
    /// <param name="limit">Maximum number of results (default 20)</param>
    /// <returns>List of top-rated recommendations</returns>
    Task<List<CommunityRecommendation>> GetTopRatedAsync(int minRatings = 5, int limit = 20);

    /// <summary>
    /// Filters recommendations by hardware tier
    /// </summary>
    /// <param name="hardwareTier">Hardware tier to filter by (Entry, Mid, High, Enthusiast)</param>
    /// <returns>List of compatible recommendations</returns>
    Task<List<CommunityRecommendation>> FilterByHardwareTierAsync(string hardwareTier);

    /// <summary>
    /// Gets recommendations for user's specific hardware configuration
    /// </summary>
    /// <param name="hardware">User's hardware information</param>
    /// <returns>List of compatible recommendations</returns>
    Task<List<CommunityRecommendation>> GetRecommendationsForHardwareAsync(HardwareInfo hardware);

    /// <summary>
    /// Upvotes a model recommendation
    /// </summary>
    /// <param name="modelName">Name of the model to upvote</param>
    /// <returns>True if upvote was successful</returns>
    Task<bool> UpvoteRecommendationAsync(string modelName);

    /// <summary>
    /// Adds a review to a model recommendation
    /// </summary>
    /// <param name="modelName">Name of the model to review</param>
    /// <param name="review">User review</param>
    /// <returns>True if review was added successfully</returns>
    Task<bool> AddReviewAsync(string modelName, UserReview review);

    /// <summary>
    /// Gets user submissions stored locally
    /// </summary>
    /// <returns>List of user submissions</returns>
    Task<List<ModelSubmission>> GetUserSubmissionsAsync();
}
