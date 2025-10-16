using System;
using System.Runtime.InteropServices;

namespace LLMCapabilityChecker.Helpers;

/// <summary>
/// Helper class for system information detection
/// </summary>
public static class SystemInfoHelper
{
    /// <summary>
    /// Detect the operating system
    /// </summary>
    public static string DetectOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"Windows {Environment.OSVersion.Version}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"Linux {Environment.OSVersion.Version}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"macOS {Environment.OSVersion.Version}";
        }
        else
        {
            return "Unknown OS";
        }
    }

    /// <summary>
    /// Get CPU architecture
    /// </summary>
    public static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture.ToString();
    }

    /// <summary>
    /// Check if running on Windows
    /// </summary>
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Check if running on Linux
    /// </summary>
    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Check if running on macOS
    /// </summary>
    public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Get number of logical processors
    /// </summary>
    public static int GetLogicalProcessorCount()
    {
        return Environment.ProcessorCount;
    }

    /// <summary>
    /// Convert bytes to gigabytes
    /// </summary>
    public static int BytesToGB(long bytes)
    {
        return (int)(bytes / (1024 * 1024 * 1024));
    }

    /// <summary>
    /// Convert megabytes to gigabytes
    /// </summary>
    public static int MBToGB(long megabytes)
    {
        return (int)(megabytes / 1024);
    }
}
