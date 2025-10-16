namespace LLMCapabilityChecker.Models;

/// <summary>
/// Contains comprehensive hardware information for the system
/// </summary>
public class HardwareInfo
{
    /// <summary>
    /// CPU information
    /// </summary>
    public CpuInfo Cpu { get; set; } = new();

    /// <summary>
    /// Memory (RAM) information
    /// </summary>
    public MemoryInfo Memory { get; set; } = new();

    /// <summary>
    /// GPU information
    /// </summary>
    public GpuInfo Gpu { get; set; } = new();

    /// <summary>
    /// Storage information
    /// </summary>
    public StorageInfo Storage { get; set; } = new();

    /// <summary>
    /// Available ML frameworks
    /// </summary>
    public FrameworkInfo Frameworks { get; set; } = new();

    /// <summary>
    /// Operating system information
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// Overall system capability tier (Entry, Mid, High, Enthusiast)
    /// </summary>
    public string SystemTier { get; set; } = "Unknown";
}

/// <summary>
/// CPU information
/// </summary>
public class CpuInfo
{
    /// <summary>
    /// CPU model name
    /// </summary>
    public string Model { get; set; } = "Unknown";

    /// <summary>
    /// Number of physical cores
    /// </summary>
    public int Cores { get; set; }

    /// <summary>
    /// Number of logical processors (threads)
    /// </summary>
    public int Threads { get; set; }

    /// <summary>
    /// Base clock speed in GHz
    /// </summary>
    public double BaseClockGHz { get; set; }

    /// <summary>
    /// CPU architecture (x64, ARM64, etc.)
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// Whether CPU supports AVX2 instructions
    /// </summary>
    public bool SupportsAvx2 { get; set; }

    /// <summary>
    /// Whether CPU supports AVX-512 instructions
    /// </summary>
    public bool SupportsAvx512 { get; set; }
}

/// <summary>
/// Memory (RAM) information
/// </summary>
public class MemoryInfo
{
    /// <summary>
    /// Total RAM in GB
    /// </summary>
    public int TotalGB { get; set; }

    /// <summary>
    /// Available RAM in GB
    /// </summary>
    public int AvailableGB { get; set; }

    /// <summary>
    /// Memory type (DDR4, DDR5, etc.)
    /// </summary>
    public string Type { get; set; } = "Unknown";

    /// <summary>
    /// Memory speed in MHz
    /// </summary>
    public int SpeedMHz { get; set; }
}

/// <summary>
/// GPU information
/// </summary>
public class GpuInfo
{
    /// <summary>
    /// GPU model name
    /// </summary>
    public string Model { get; set; } = "Unknown";

    /// <summary>
    /// GPU vendor (NVIDIA, AMD, Intel, Apple)
    /// </summary>
    public string Vendor { get; set; } = "Unknown";

    /// <summary>
    /// VRAM in GB
    /// </summary>
    public int VramGB { get; set; }

    /// <summary>
    /// Compute capability (for NVIDIA) or equivalent
    /// </summary>
    public string ComputeCapability { get; set; } = string.Empty;

    /// <summary>
    /// Whether GPU supports FP16 (half precision)
    /// </summary>
    public bool SupportsFp16 { get; set; }

    /// <summary>
    /// Whether GPU supports INT8 quantization
    /// </summary>
    public bool SupportsInt8 { get; set; }

    /// <summary>
    /// GPU architecture (Ampere, RDNA2, etc.)
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// Whether GPU is dedicated (true) or integrated (false)
    /// </summary>
    public bool IsDedicated { get; set; }

    /// <summary>
    /// Whether GPU supports Metal API (macOS)
    /// </summary>
    public bool HasMetal { get; set; }
}

/// <summary>
/// Storage information
/// </summary>
public class StorageInfo
{
    /// <summary>
    /// Total storage in GB
    /// </summary>
    public int TotalGB { get; set; }

    /// <summary>
    /// Available storage in GB
    /// </summary>
    public int AvailableGB { get; set; }

    /// <summary>
    /// Storage type (SSD, NVMe, HDD)
    /// </summary>
    public string Type { get; set; } = "Unknown";

    /// <summary>
    /// Read speed in MB/s
    /// </summary>
    public int ReadSpeedMBps { get; set; }

    /// <summary>
    /// Write speed in MB/s
    /// </summary>
    public int WriteSpeedMBps { get; set; }
}

/// <summary>
/// Available ML frameworks
/// </summary>
public class FrameworkInfo
{
    /// <summary>
    /// Whether CUDA is available
    /// </summary>
    public bool HasCuda { get; set; }

    /// <summary>
    /// CUDA version (if available)
    /// </summary>
    public string? CudaVersion { get; set; }

    /// <summary>
    /// Whether ROCm is available (AMD)
    /// </summary>
    public bool HasRocm { get; set; }

    /// <summary>
    /// ROCm version (if available)
    /// </summary>
    public string? RocmVersion { get; set; }

    /// <summary>
    /// Whether Metal is available (macOS)
    /// </summary>
    public bool HasMetal { get; set; }

    /// <summary>
    /// Whether DirectML is available (Windows)
    /// </summary>
    public bool HasDirectMl { get; set; }

    /// <summary>
    /// Whether OpenVINO is available (Intel)
    /// </summary>
    public bool HasOpenVino { get; set; }
}
