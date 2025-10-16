using LLMCapabilityChecker.Models;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for detecting hardware information
/// </summary>
public interface IHardwareDetectionService
{
    /// <summary>
    /// Detect all hardware information
    /// </summary>
    Task<HardwareInfo> DetectHardwareAsync();
}
