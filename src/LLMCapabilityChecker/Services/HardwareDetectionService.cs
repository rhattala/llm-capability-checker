using LLMCapabilityChecker.Helpers;
using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for detecting hardware information
/// </summary>
public class HardwareDetectionService : IHardwareDetectionService
{
    private readonly ILogger<HardwareDetectionService> _logger;

    public HardwareDetectionService(ILogger<HardwareDetectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Detect all hardware information
    /// </summary>
    public async Task<HardwareInfo> DetectHardwareAsync()
    {
        var hardware = new HardwareInfo
        {
            OperatingSystem = SystemInfoHelper.DetectOperatingSystem()
        };

        try
        {
            // Run detections in parallel
            var cpuTask = DetectCpuAsync();
            var memoryTask = DetectMemoryAsync();
            var gpuTask = DetectGpuAsync();
            var storageTask = DetectStorageAsync();
            var frameworksTask = DetectFrameworksAsync();

            await Task.WhenAll(cpuTask, memoryTask, gpuTask, storageTask, frameworksTask);

            hardware.Cpu = await cpuTask;
            hardware.Memory = await memoryTask;
            hardware.Gpu = await gpuTask;
            hardware.Storage = await storageTask;
            hardware.Frameworks = await frameworksTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting hardware");
        }

        return hardware;
    }

    /// <summary>
    /// Detect CPU information
    /// </summary>
    private async Task<CpuInfo> DetectCpuAsync()
    {
        var cpuInfo = new CpuInfo
        {
            Architecture = SystemInfoHelper.GetArchitecture(),
            Threads = SystemInfoHelper.GetLogicalProcessorCount()
        };

        if (SystemInfoHelper.IsWindows())
        {
            await DetectCpuWindows(cpuInfo);
        }
        else if (SystemInfoHelper.IsLinux())
        {
            await DetectCpuLinux(cpuInfo);
        }
        else if (SystemInfoHelper.IsMacOS())
        {
            await DetectCpuMacOS(cpuInfo);
        }

        return cpuInfo;
    }

    /// <summary>
    /// Detect CPU information on Windows using WMI
    /// </summary>
    private async Task DetectCpuWindows(CpuInfo cpuInfo)
    {
        try
        {
            await Task.Run(() =>
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                using var collection = searcher.Get();

                var processor = collection.Cast<ManagementObject>().FirstOrDefault();
                if (processor != null)
                {
                    cpuInfo.Model = processor["Name"]?.ToString()?.Trim() ?? "Unknown";
                    cpuInfo.Cores = Convert.ToInt32(processor["NumberOfCores"] ?? 0);
                    cpuInfo.BaseClockGHz = Convert.ToDouble(processor["MaxClockSpeed"] ?? 0) / 1000.0;
                }
            });

            _logger.LogInformation("CPU detected: {Model}, {Cores} cores, {Threads} threads",
                cpuInfo.Model, cpuInfo.Cores, cpuInfo.Threads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting CPU on Windows");
        }
    }

    /// <summary>
    /// Detect CPU information on Linux
    /// </summary>
    private async Task DetectCpuLinux(CpuInfo cpuInfo)
    {
        try
        {
            // Use lscpu command
            var result = await RunCommandAsync("lscpu", "", 2000);
            if (result != null)
            {
                var lines = result.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Model name:"))
                    {
                        cpuInfo.Model = line.Split(':')[1].Trim();
                    }
                    else if (line.StartsWith("CPU(s):"))
                    {
                        int.TryParse(line.Split(':')[1].Trim(), out int threads);
                        cpuInfo.Threads = threads;
                    }
                    else if (line.StartsWith("Core(s) per socket:"))
                    {
                        int.TryParse(line.Split(':')[1].Trim(), out int cores);
                        cpuInfo.Cores = cores;
                    }
                }
            }

            _logger.LogInformation("CPU detected: {Model}, {Cores} cores, {Threads} threads",
                cpuInfo.Model, cpuInfo.Cores, cpuInfo.Threads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting CPU on Linux");
        }
    }

    /// <summary>
    /// Detect CPU information on macOS
    /// </summary>
    private async Task DetectCpuMacOS(CpuInfo cpuInfo)
    {
        try
        {
            // Use sysctl command
            var modelResult = await RunCommandAsync("sysctl", "-n machdep.cpu.brand_string", 2000);
            if (modelResult != null)
            {
                cpuInfo.Model = modelResult.Trim();
            }

            var coresResult = await RunCommandAsync("sysctl", "-n hw.physicalcpu", 2000);
            if (coresResult != null && int.TryParse(coresResult.Trim(), out int cores))
            {
                cpuInfo.Cores = cores;
            }

            _logger.LogInformation("CPU detected: {Model}, {Cores} cores, {Threads} threads",
                cpuInfo.Model, cpuInfo.Cores, cpuInfo.Threads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting CPU on macOS");
        }
    }

    /// <summary>
    /// Detect memory information
    /// </summary>
    private async Task<MemoryInfo> DetectMemoryAsync()
    {
        var memoryInfo = new MemoryInfo();

        if (SystemInfoHelper.IsWindows())
        {
            await DetectMemoryWindows(memoryInfo);
        }
        else if (SystemInfoHelper.IsLinux())
        {
            await DetectMemoryLinux(memoryInfo);
        }
        else if (SystemInfoHelper.IsMacOS())
        {
            await DetectMemoryMacOS(memoryInfo);
        }

        return memoryInfo;
    }

    /// <summary>
    /// Detect memory information on Windows using WMI
    /// </summary>
    private async Task DetectMemoryWindows(MemoryInfo memoryInfo)
    {
        try
        {
            await Task.Run(() =>
            {
                // Get total physical memory
                using var computerSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                using var computerCollection = computerSearcher.Get();

                var computer = computerCollection.Cast<ManagementObject>().FirstOrDefault();
                if (computer != null)
                {
                    long totalBytes = Convert.ToInt64(computer["TotalPhysicalMemory"] ?? 0);
                    memoryInfo.TotalGB = SystemInfoHelper.BytesToGB(totalBytes);
                }

                // Get memory type and speed
                using var memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                using var memoryCollection = memorySearcher.Get();

                var memory = memoryCollection.Cast<ManagementObject>().FirstOrDefault();
                if (memory != null)
                {
                    uint smbiosMemoryType = Convert.ToUInt32(memory["SMBIOSMemoryType"] ?? 0);
                    memoryInfo.Type = GetMemoryTypeName(smbiosMemoryType);
                    memoryInfo.SpeedMHz = Convert.ToInt32(memory["Speed"] ?? 0);
                }

                // Get available memory
                using var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                using var osCollection = osSearcher.Get();

                var os = osCollection.Cast<ManagementObject>().FirstOrDefault();
                if (os != null)
                {
                    long availableBytes = Convert.ToInt64(os["FreePhysicalMemory"] ?? 0) * 1024;
                    memoryInfo.AvailableGB = SystemInfoHelper.BytesToGB(availableBytes);
                }
            });

            _logger.LogInformation("Memory detected: {Total}GB total, {Available}GB available, {Type} @ {Speed}MHz",
                memoryInfo.TotalGB, memoryInfo.AvailableGB, memoryInfo.Type, memoryInfo.SpeedMHz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting memory on Windows");
        }
    }

    /// <summary>
    /// Detect memory information on Linux
    /// </summary>
    private async Task DetectMemoryLinux(MemoryInfo memoryInfo)
    {
        try
        {
            var result = await RunCommandAsync("cat", "/proc/meminfo", 2000);
            if (result != null)
            {
                var lines = result.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out long kb))
                        {
                            memoryInfo.TotalGB = SystemInfoHelper.BytesToGB(kb * 1024);
                        }
                    }
                    else if (line.StartsWith("MemAvailable:"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out long kb))
                        {
                            memoryInfo.AvailableGB = SystemInfoHelper.BytesToGB(kb * 1024);
                        }
                    }
                }
            }

            _logger.LogInformation("Memory detected: {Total}GB total, {Available}GB available",
                memoryInfo.TotalGB, memoryInfo.AvailableGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting memory on Linux");
        }
    }

    /// <summary>
    /// Detect memory information on macOS
    /// </summary>
    private async Task DetectMemoryMacOS(MemoryInfo memoryInfo)
    {
        try
        {
            var result = await RunCommandAsync("sysctl", "-n hw.memsize", 2000);
            if (result != null && long.TryParse(result.Trim(), out long bytes))
            {
                memoryInfo.TotalGB = SystemInfoHelper.BytesToGB(bytes);
            }

            _logger.LogInformation("Memory detected: {Total}GB total", memoryInfo.TotalGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting memory on macOS");
        }
    }

    /// <summary>
    /// Detect GPU information
    /// </summary>
    private async Task<GpuInfo> DetectGpuAsync()
    {
        var gpuInfo = new GpuInfo();

        if (SystemInfoHelper.IsWindows())
        {
            await DetectGpuWindows(gpuInfo);
        }
        else if (SystemInfoHelper.IsLinux())
        {
            await DetectGpuLinux(gpuInfo);
        }
        else if (SystemInfoHelper.IsMacOS())
        {
            await DetectGpuMacOS(gpuInfo);
        }

        return gpuInfo;
    }

    /// <summary>
    /// Detect GPU information on Windows using WMI and nvidia-smi
    /// </summary>
    private async Task DetectGpuWindows(GpuInfo gpuInfo)
    {
        try
        {
            // First try WMI to get basic GPU info
            await Task.Run(() =>
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                using var collection = searcher.Get();

                // Look for dedicated GPU first (prefer NVIDIA/AMD over Intel integrated)
                ManagementObject? dedicatedGpu = null;
                ManagementObject? anyGpu = null;

                foreach (var obj in collection.Cast<ManagementObject>())
                {
                    anyGpu = obj;
                    var name = obj["Name"]?.ToString() ?? "";

                    // Prioritize dedicated GPUs
                    if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
                    {
                        dedicatedGpu = obj;
                        break;
                    }
                }

                var selectedGpu = dedicatedGpu ?? anyGpu;
                if (selectedGpu != null)
                {
                    gpuInfo.Model = selectedGpu["Name"]?.ToString()?.Trim() ?? "Unknown";

                    // Determine vendor
                    var model = gpuInfo.Model.ToLower();
                    if (model.Contains("nvidia"))
                        gpuInfo.Vendor = "NVIDIA";
                    else if (model.Contains("amd") || model.Contains("radeon"))
                        gpuInfo.Vendor = "AMD";
                    else if (model.Contains("intel"))
                        gpuInfo.Vendor = "Intel";

                    // Get VRAM from WMI (in bytes, convert to GB)
                    var adapterRam = selectedGpu["AdapterRAM"];
                    if (adapterRam != null)
                    {
                        long vramBytes = Convert.ToInt64(adapterRam);
                        gpuInfo.VramGB = SystemInfoHelper.BytesToGB(vramBytes);
                    }

                    // Determine if dedicated or integrated
                    gpuInfo.IsDedicated = gpuInfo.Vendor == "NVIDIA" || gpuInfo.Vendor == "AMD";
                }
            });

            // For NVIDIA GPUs, use nvidia-smi for more detailed info
            if (gpuInfo.Vendor == "NVIDIA")
            {
                await DetectNvidiaGpuDetails(gpuInfo);
            }

            _logger.LogInformation("GPU detected: {Vendor} {Model}, {VramGB}GB VRAM",
                gpuInfo.Vendor, gpuInfo.Model, gpuInfo.VramGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting GPU on Windows");
        }
    }

    /// <summary>
    /// Detect GPU information on Linux using lspci and nvidia-smi
    /// </summary>
    private async Task DetectGpuLinux(GpuInfo gpuInfo)
    {
        try
        {
            // Try lspci to get GPU info
            var lspciResult = await RunCommandAsync("lspci", "", 2000);
            if (lspciResult != null)
            {
                var lines = lspciResult.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("VGA compatible controller", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("3D controller", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract GPU model
                        var parts = line.Split(':');
                        if (parts.Length >= 3)
                        {
                            gpuInfo.Model = parts[2].Trim();

                            // Determine vendor
                            var model = gpuInfo.Model.ToLower();
                            if (model.Contains("nvidia"))
                            {
                                gpuInfo.Vendor = "NVIDIA";
                                gpuInfo.IsDedicated = true;
                            }
                            else if (model.Contains("amd") || model.Contains("radeon"))
                            {
                                gpuInfo.Vendor = "AMD";
                                gpuInfo.IsDedicated = true;
                            }
                            else if (model.Contains("intel"))
                            {
                                gpuInfo.Vendor = "Intel";
                                gpuInfo.IsDedicated = false;
                            }
                        }
                        break; // Take first GPU
                    }
                }
            }

            // For NVIDIA GPUs, use nvidia-smi for detailed info
            if (gpuInfo.Vendor == "NVIDIA")
            {
                await DetectNvidiaGpuDetails(gpuInfo);
            }

            _logger.LogInformation("GPU detected: {Vendor} {Model}, {VramGB}GB VRAM",
                gpuInfo.Vendor, gpuInfo.Model, gpuInfo.VramGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting GPU on Linux");
        }
    }

    /// <summary>
    /// Detect GPU information on macOS using system_profiler
    /// </summary>
    private async Task DetectGpuMacOS(GpuInfo gpuInfo)
    {
        try
        {
            var result = await RunCommandAsync("system_profiler", "SPDisplaysDataType", 2000);
            if (result != null)
            {
                var lines = result.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed.StartsWith("Chipset Model:"))
                    {
                        gpuInfo.Model = trimmed.Split(':')[1].Trim();

                        // Determine vendor
                        var model = gpuInfo.Model.ToLower();
                        if (model.Contains("nvidia"))
                            gpuInfo.Vendor = "NVIDIA";
                        else if (model.Contains("amd") || model.Contains("radeon"))
                            gpuInfo.Vendor = "AMD";
                        else if (model.Contains("intel"))
                            gpuInfo.Vendor = "Intel";
                        else if (model.Contains("apple"))
                        {
                            gpuInfo.Vendor = "Apple";
                            gpuInfo.IsDedicated = true; // Apple Silicon has integrated but powerful GPU
                            gpuInfo.HasMetal = true;
                        }
                    }
                    else if (trimmed.StartsWith("VRAM") || trimmed.StartsWith("Total VRAM"))
                    {
                        var vramText = trimmed.Split(':')[1].Trim();
                        // Parse VRAM (e.g., "8 GB", "8192 MB")
                        var parts = vramText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && int.TryParse(parts[0], out int value))
                        {
                            if (parts[1].ToLower().Contains("gb"))
                                gpuInfo.VramGB = value;
                            else if (parts[1].ToLower().Contains("mb"))
                                gpuInfo.VramGB = SystemInfoHelper.MBToGB(value);
                        }
                    }
                }

                // Set Metal support for macOS
                if (gpuInfo.Vendor != "Unknown")
                {
                    gpuInfo.HasMetal = true;
                }
            }

            _logger.LogInformation("GPU detected: {Vendor} {Model}, {VramGB}GB VRAM",
                gpuInfo.Vendor, gpuInfo.Model, gpuInfo.VramGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting GPU on macOS");
        }
    }

    /// <summary>
    /// Detect detailed NVIDIA GPU information using nvidia-smi
    /// </summary>
    private async Task DetectNvidiaGpuDetails(GpuInfo gpuInfo)
    {
        try
        {
            // Query nvidia-smi for detailed GPU info
            var result = await RunCommandAsync("nvidia-smi",
                "--query-gpu=name,memory.total,driver_version,compute_cap --format=csv,noheader,nounits", 2000);

            if (result != null)
            {
                var parts = result.Trim().Split(',');
                if (parts.Length >= 4)
                {
                    // Update model name from nvidia-smi (more accurate)
                    gpuInfo.Model = parts[0].Trim();

                    // VRAM in MB from nvidia-smi
                    if (int.TryParse(parts[1].Trim(), out int vramMB))
                    {
                        gpuInfo.VramGB = SystemInfoHelper.MBToGB(vramMB);
                    }

                    // Driver version (not used in model but available)
                    var driverVersion = parts[2].Trim();

                    // Compute capability
                    gpuInfo.ComputeCapability = parts[3].Trim();

                    // Determine features based on compute capability
                    if (double.TryParse(gpuInfo.ComputeCapability, out double computeCap))
                    {
                        // FP16 supported from compute capability 5.3+
                        gpuInfo.SupportsFp16 = computeCap >= 5.3;

                        // INT8 supported from compute capability 6.1+
                        gpuInfo.SupportsInt8 = computeCap >= 6.1;

                        // Determine architecture based on compute capability
                        gpuInfo.Architecture = computeCap switch
                        {
                            >= 9.0 => "Hopper",
                            >= 8.0 => "Ampere",
                            >= 7.5 => "Turing",
                            >= 7.0 => "Volta",
                            >= 6.0 => "Pascal",
                            >= 5.0 => "Maxwell",
                            >= 3.0 => "Kepler",
                            _ => "Legacy"
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "nvidia-smi failed or not available");
        }
    }

    /// <summary>
    /// Detect storage information
    /// </summary>
    private async Task<StorageInfo> DetectStorageAsync()
    {
        var storageInfo = new StorageInfo();

        if (SystemInfoHelper.IsWindows())
        {
            await DetectStorageWindows(storageInfo);
        }
        else if (SystemInfoHelper.IsLinux())
        {
            await DetectStorageLinux(storageInfo);
        }
        else if (SystemInfoHelper.IsMacOS())
        {
            await DetectStorageMacOS(storageInfo);
        }

        return storageInfo;
    }

    /// <summary>
    /// Detect storage information on Windows using WMI
    /// </summary>
    private async Task DetectStorageWindows(StorageInfo storageInfo)
    {
        try
        {
            await Task.Run(() =>
            {
                // Get logical disk info (C: drive for primary)
                using var diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'");
                using var diskCollection = diskSearcher.Get();

                var disk = diskCollection.Cast<ManagementObject>().FirstOrDefault();
                if (disk != null)
                {
                    long totalBytes = Convert.ToInt64(disk["Size"] ?? 0);
                    long freeBytes = Convert.ToInt64(disk["FreeSpace"] ?? 0);
                    storageInfo.TotalGB = SystemInfoHelper.BytesToGB(totalBytes);
                    storageInfo.AvailableGB = SystemInfoHelper.BytesToGB(freeBytes);
                }

                // PRIORITY 1: Try MSFT_PhysicalDisk first (most accurate for NVMe detection)
                bool detectedType = false;
                try
                {
                    using var msftSearcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage",
                        "SELECT * FROM MSFT_PhysicalDisk WHERE DeviceId='0'");
                    using var msftCollection = msftSearcher.Get();

                    var msftDisk = msftCollection.Cast<ManagementObject>().FirstOrDefault();
                    if (msftDisk != null)
                    {
                        ushort mediaType = Convert.ToUInt16(msftDisk["MediaType"] ?? 0);
                        ushort busType = Convert.ToUInt16(msftDisk["BusType"] ?? 0);

                        _logger.LogDebug("MSFT_PhysicalDisk: MediaType={MediaType}, BusType={BusType}", mediaType, busType);

                        // MediaType: 3 = HDD, 4 = SSD, 5 = SCM
                        // BusType: 17 = NVMe, 11 = SATA, 7 = USB
                        if (busType == 17)
                        {
                            storageInfo.Type = "NVMe";
                            detectedType = true;
                        }
                        else if (mediaType == 4)
                        {
                            storageInfo.Type = "SSD";
                            detectedType = true;
                        }
                        else if (mediaType == 3)
                        {
                            storageInfo.Type = "HDD";
                            detectedType = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "MSFT_PhysicalDisk not available, falling back to Win32_DiskDrive");
                }

                // FALLBACK: Use Win32_DiskDrive if MSFT_PhysicalDisk failed
                if (!detectedType)
                {
                    using var physicalSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE DeviceID='\\\\\\\\.\\\\PHYSICALDRIVE0'");
                    using var physicalCollection = physicalSearcher.Get();

                    var physicalDisk = physicalCollection.Cast<ManagementObject>().FirstOrDefault();
                    if (physicalDisk != null)
                    {
                        string mediaType = physicalDisk["MediaType"]?.ToString() ?? "";
                        string interfaceType = physicalDisk["InterfaceType"]?.ToString() ?? "";
                        string model = physicalDisk["Model"]?.ToString() ?? "";

                        _logger.LogDebug("Win32_DiskDrive: MediaType={MediaType}, InterfaceType={InterfaceType}, Model={Model}",
                            mediaType, interfaceType, model);

                        // Check model name for NVMe indicators
                        if (model.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
                            interfaceType.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
                        {
                            storageInfo.Type = "NVMe";
                        }
                        else if (mediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase))
                        {
                            storageInfo.Type = "SSD";
                        }
                        else if (mediaType.Contains("HDD", StringComparison.OrdinalIgnoreCase))
                        {
                            storageInfo.Type = "HDD";
                        }
                        else if (mediaType.Contains("Fixed hard disk", StringComparison.OrdinalIgnoreCase))
                        {
                            // Assume SSD for modern "Fixed hard disk" media type
                            storageInfo.Type = "SSD";
                        }
                    }
                }

                // Count total drives
                try
                {
                    using var allDrivesSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                    using var allDrivesCollection = allDrivesSearcher.Get();
                    int driveCount = allDrivesCollection.Count;

                    if (driveCount > 1)
                    {
                        storageInfo.Type += $" ({driveCount} drives)";
                    }
                }
                catch
                {
                    // Drive count is optional
                }
            });

            _logger.LogInformation("Storage detected: {Type}, {Total}GB total, {Available}GB available",
                storageInfo.Type, storageInfo.TotalGB, storageInfo.AvailableGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting storage on Windows");
        }
    }

    /// <summary>
    /// Detect storage information on Linux
    /// </summary>
    private async Task DetectStorageLinux(StorageInfo storageInfo)
    {
        try
        {
            // Use df to get total and available space for root partition
            var dfResult = await RunCommandAsync("df", "-BG /", 2000);
            if (dfResult != null)
            {
                var lines = dfResult.Split('\n');
                if (lines.Length >= 2)
                {
                    var parts = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        // Parse size (remove 'G' suffix)
                        if (int.TryParse(parts[1].TrimEnd('G'), out int total))
                        {
                            storageInfo.TotalGB = total;
                        }
                        if (int.TryParse(parts[3].TrimEnd('G'), out int available))
                        {
                            storageInfo.AvailableGB = available;
                        }
                    }
                }
            }

            // Use lsblk to detect storage type
            var lsblkResult = await RunCommandAsync("lsblk", "-d -o NAME,ROTA,TYPE", 2000);
            if (lsblkResult != null)
            {
                var lines = lsblkResult.Split('\n');
                foreach (var line in lines)
                {
                    // Look for main disk (not loop or rom devices)
                    if (line.Contains("disk") && !line.Contains("loop"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            // ROTA: 1 = rotating disk (HDD), 0 = non-rotating (SSD/NVMe)
                            if (parts[1] == "1")
                            {
                                storageInfo.Type = "HDD";
                            }
                            else if (parts[1] == "0")
                            {
                                // Check if NVMe by checking device name
                                string deviceName = parts[0].ToLower();
                                if (deviceName.StartsWith("nvme"))
                                {
                                    storageInfo.Type = "NVMe";
                                }
                                else
                                {
                                    storageInfo.Type = "SSD";
                                }
                            }
                        }
                        break; // Only check first disk
                    }
                }
            }

            _logger.LogInformation("Storage detected: {Type}, {Total}GB total, {Available}GB available",
                storageInfo.Type, storageInfo.TotalGB, storageInfo.AvailableGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting storage on Linux");
        }
    }

    /// <summary>
    /// Detect storage information on macOS
    /// </summary>
    private async Task DetectStorageMacOS(StorageInfo storageInfo)
    {
        try
        {
            // Use diskutil to get storage info
            var diskutilResult = await RunCommandAsync("diskutil", "info disk0", 2000);
            if (diskutilResult != null)
            {
                var lines = diskutilResult.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Disk Size:"))
                    {
                        // Parse format: "Disk Size:              500.3 GB (500277790720 Bytes) (exactly 977105060 512-Byte-Units)"
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            if (parts[i + 1] == "GB" && double.TryParse(parts[i], out double sizeGB))
                            {
                                storageInfo.TotalGB = (int)Math.Round(sizeGB);
                                break;
                            }
                        }
                    }
                    else if (line.Contains("Solid State:"))
                    {
                        // Check if it's SSD
                        if (line.Contains("Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            storageInfo.Type = "SSD";
                        }
                        else
                        {
                            storageInfo.Type = "HDD";
                        }
                    }
                    else if (line.Contains("Protocol:") && line.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
                    {
                        storageInfo.Type = "NVMe";
                    }
                }
            }

            // Use df to get available space
            var dfResult = await RunCommandAsync("df", "-g /", 2000);
            if (dfResult != null)
            {
                var lines = dfResult.Split('\n');
                if (lines.Length >= 2)
                {
                    var parts = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && int.TryParse(parts[3], out int available))
                    {
                        storageInfo.AvailableGB = available;
                    }
                }
            }

            _logger.LogInformation("Storage detected: {Type}, {Total}GB total, {Available}GB available",
                storageInfo.Type, storageInfo.TotalGB, storageInfo.AvailableGB);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting storage on macOS");
        }
    }

    /// <summary>
    /// Detect available frameworks (CUDA, ROCm, Metal, DirectML, OpenVINO)
    /// </summary>
    private async Task<FrameworkInfo> DetectFrameworksAsync()
    {
        var frameworkInfo = new FrameworkInfo();

        try
        {
            // Run all detections in parallel for efficiency
            var cudaTask = DetectCudaAsync();
            var rocmTask = DetectRocmAsync();
            var metalTask = DetectMetalAsync();
            var directMlTask = DetectDirectMlAsync();
            var openVinoTask = DetectOpenVinoAsync();

            await Task.WhenAll(cudaTask, rocmTask, metalTask, directMlTask, openVinoTask);

            // CUDA detection
            var cudaVersion = await cudaTask;
            if (cudaVersion != null)
            {
                frameworkInfo.HasCuda = true;
                frameworkInfo.CudaVersion = cudaVersion;
            }

            // ROCm detection
            var rocmVersion = await rocmTask;
            if (rocmVersion != null)
            {
                frameworkInfo.HasRocm = true;
                frameworkInfo.RocmVersion = rocmVersion;
            }

            // Metal detection
            frameworkInfo.HasMetal = await metalTask;

            // DirectML detection
            frameworkInfo.HasDirectMl = await directMlTask;

            // OpenVINO detection
            frameworkInfo.HasOpenVino = await openVinoTask;

            _logger.LogInformation("Frameworks - CUDA: {Cuda}, ROCm: {Rocm}, Metal: {Metal}, DirectML: {DirectMl}, OpenVINO: {OpenVino}",
                frameworkInfo.HasCuda, frameworkInfo.HasRocm, frameworkInfo.HasMetal, frameworkInfo.HasDirectMl, frameworkInfo.HasOpenVino);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting frameworks");
        }

        return frameworkInfo;
    }

    /// <summary>
    /// Detect CUDA via nvidia-smi
    /// </summary>
    private async Task<string?> DetectCudaAsync()
    {
        try
        {
            var result = await RunCommandAsync("nvidia-smi", "", 2000);
            if (!string.IsNullOrWhiteSpace(result))
            {
                // Parse CUDA version from nvidia-smi output
                var match = System.Text.RegularExpressions.Regex.Match(result, @"CUDA Version:\s*(\d+\.\d+)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return "Installed"; // CUDA present but version not parsed
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CUDA detection failed");
        }
        return null;
    }

    /// <summary>
    /// Detect ROCm via rocm-smi (Linux/AMD)
    /// </summary>
    private async Task<string?> DetectRocmAsync()
    {
        try
        {
            if (!SystemInfoHelper.IsLinux())
                return null;

            var result = await RunCommandAsync("rocm-smi", "--version", 2000);
            if (!string.IsNullOrWhiteSpace(result))
            {
                var match = System.Text.RegularExpressions.Regex.Match(result, @"(\d+\.\d+\.\d+)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                return "Installed";
            }

            // Check for ROCm directory
            if (System.IO.Directory.Exists("/opt/rocm"))
            {
                return "Installed";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ROCm detection failed");
        }
        return null;
    }

    /// <summary>
    /// Detect Metal availability (macOS 10.13+)
    /// </summary>
    private async Task<bool> DetectMetalAsync()
    {
        try
        {
            if (!SystemInfoHelper.IsMacOS())
                return false;

            var result = await RunCommandAsync("sw_vers", "-productVersion", 2000);
            if (!string.IsNullOrWhiteSpace(result))
            {
                var parts = result.Trim().Split('.');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int major))
                {
                    if (major >= 11) return true; // macOS 11+
                    if (major == 10 && int.TryParse(parts[1], out int minor) && minor >= 13)
                        return true; // macOS 10.13+
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Metal detection failed");
        }
        return false;
    }

    /// <summary>
    /// Detect DirectML availability (Windows 10+)
    /// </summary>
    private Task<bool> DetectDirectMlAsync()
    {
        try
        {
            if (!SystemInfoHelper.IsWindows())
                return Task.FromResult(false);

            var version = Environment.OSVersion.Version;
            // DirectML available on Windows 10 build 18362+ or Windows 11
            return Task.FromResult(version.Major >= 10 && version.Build >= 18362);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DirectML detection failed");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Detect OpenVINO availability (Intel)
    /// </summary>
    private async Task<bool> DetectOpenVinoAsync()
    {
        try
        {
            if (SystemInfoHelper.IsWindows())
            {
                var paths = new[] { @"C:\Program Files (x86)\Intel\openvino", @"C:\Program Files\Intel\openvino" };
                if (paths.Any(System.IO.Directory.Exists))
                    return true;
            }

            if (SystemInfoHelper.IsLinux() || SystemInfoHelper.IsMacOS())
            {
                if (System.IO.Directory.Exists("/opt/intel/openvino"))
                    return true;

                var result = await RunCommandAsync("which", "benchmark_app", 2000);
                if (!string.IsNullOrWhiteSpace(result))
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "OpenVINO detection failed");
        }
        return false;
    }

    /// <summary>
    /// Get memory type name from SMBIOS memory type code
    /// </summary>
    private string GetMemoryTypeName(uint smbiosMemoryType)
    {
        return smbiosMemoryType switch
        {
            26 => "DDR4",
            34 => "DDR5",
            24 => "DDR3",
            21 => "DDR2",
            20 => "DDR",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Run a command and return its output
    /// </summary>
    private async Task<string?> RunCommandAsync(string command, string arguments, int timeoutMs)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var completedTask = await Task.WhenAny(outputTask, Task.Delay(timeoutMs));

            if (completedTask == outputTask)
            {
                return await outputTask;
            }
            else
            {
                process.Kill();
                _logger.LogWarning("Command {Command} timed out after {Timeout}ms", command, timeoutMs);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running command: {Command} {Arguments}", command, arguments);
            return null;
        }
    }
}
