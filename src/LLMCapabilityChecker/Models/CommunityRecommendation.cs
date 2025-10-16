using System;
using System.Collections.Generic;

namespace LLMCapabilityChecker.Models;

/// <summary>
/// Represents a community-recommended LLM model
/// </summary>
public class CommunityRecommendation
{
    /// <summary>
    /// Model name
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Model family (Llama, Mistral, Gemma, etc.)
    /// </summary>
    public string Family { get; set; } = string.Empty;

    /// <summary>
    /// Parameter size (e.g., "7B", "13B", "70B")
    /// </summary>
    public string ParameterSize { get; set; } = string.Empty;

    /// <summary>
    /// Number of upvotes/recommendations from community
    /// </summary>
    public int RecommendedByCount { get; set; }

    /// <summary>
    /// Average user rating (1-5 stars)
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// Total number of ratings
    /// </summary>
    public int TotalRatings { get; set; }

    /// <summary>
    /// Compatible hardware tier (Entry, Mid, High, Enthusiast)
    /// </summary>
    public string CompatibleHardwareTier { get; set; } = string.Empty;

    /// <summary>
    /// Minimum VRAM required in GB
    /// </summary>
    public int MinVramGB { get; set; }

    /// <summary>
    /// Minimum RAM required in GB (for CPU inference)
    /// </summary>
    public int MinRamGB { get; set; }

    /// <summary>
    /// User reviews and comments
    /// </summary>
    public List<UserReview> Reviews { get; set; } = new();

    /// <summary>
    /// Date when this recommendation was first added
    /// </summary>
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Source of recommendation (community, verified, official)
    /// </summary>
    public RecommendationSource Source { get; set; }

    /// <summary>
    /// Model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Use case tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Official model page URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Whether this model exists in the official database
    /// </summary>
    public bool IsInOfficialDatabase { get; set; }

    /// <summary>
    /// Link to official database model (if exists)
    /// </summary>
    public string? OfficialModelId { get; set; }
}

/// <summary>
/// User review for a model
/// </summary>
public class UserReview
{
    /// <summary>
    /// Anonymous user identifier (hash)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's rating (1-5 stars)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Review comment
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// User's hardware tier
    /// </summary>
    public string HardwareTier { get; set; } = string.Empty;

    /// <summary>
    /// Date of review
    /// </summary>
    public DateTime DatePosted { get; set; }

    /// <summary>
    /// Specific use case tested
    /// </summary>
    public string UseCase { get; set; } = string.Empty;

    /// <summary>
    /// Was this review helpful (upvotes)
    /// </summary>
    public int HelpfulCount { get; set; }
}

/// <summary>
/// Source of a community recommendation
/// </summary>
public enum RecommendationSource
{
    /// <summary>
    /// Submitted by community users
    /// </summary>
    Community,

    /// <summary>
    /// Verified by moderators/experts
    /// </summary>
    Verified,

    /// <summary>
    /// Official recommendation from model provider
    /// </summary>
    Official
}

/// <summary>
/// User submission for new model recommendation
/// </summary>
public class ModelSubmission
{
    /// <summary>
    /// Model name being recommended
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Why user recommends this model
    /// </summary>
    public string ReasonForRecommendation { get; set; } = string.Empty;

    /// <summary>
    /// User's hardware tier
    /// </summary>
    public string UserHardwareTier { get; set; } = string.Empty;

    /// <summary>
    /// User's rating (1-5 stars)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Use case tested
    /// </summary>
    public string UseCase { get; set; } = string.Empty;

    /// <summary>
    /// Date of submission
    /// </summary>
    public DateTime DateSubmitted { get; set; }

    /// <summary>
    /// Anonymous user ID (hash)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
