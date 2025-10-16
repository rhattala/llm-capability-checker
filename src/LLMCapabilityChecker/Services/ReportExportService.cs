using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LLMCapabilityChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LLMCapabilityChecker.Services;

/// <summary>
/// Service for exporting hardware analysis reports in JSON and text formats
/// </summary>
public class ReportExportService : IReportExportService
{
    private readonly ILogger<ReportExportService> _logger;

    public ReportExportService(ILogger<ReportExportService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<string> ExportAsJsonAsync(HardwareInfo hardwareInfo, SystemScores systemScores, List<ModelInfo> recommendedModels)
    {
        try
        {
            var report = new
            {
                ReportMetadata = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    ReportVersion = "1.0",
                    ApplicationName = "LLM Capability Checker"
                },
                HardwareInfo = hardwareInfo,
                SystemScores = systemScores,
                RecommendedModels = recommendedModels
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string jsonContent = JsonSerializer.Serialize(report, options);
            _logger.LogInformation("Successfully exported report as JSON");
            return Task.FromResult(jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report as JSON");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<string> ExportAsTextAsync(HardwareInfo hardwareInfo, SystemScores systemScores, List<ModelInfo> recommendedModels)
    {
        try
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("================================================================================");
            sb.AppendLine("                     LLM CAPABILITY CHECKER REPORT");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // System Overview
            sb.AppendLine("SYSTEM OVERVIEW");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"Overall Score:           {systemScores.OverallScore}/100");
            sb.AppendLine($"System Tier:             {systemScores.SystemTier}");
            sb.AppendLine($"Recommended Model Size:  {systemScores.RecommendedModelSize}");
            sb.AppendLine($"Primary Bottleneck:      {systemScores.PrimaryBottleneck}");
            sb.AppendLine($"Operating System:        {hardwareInfo.OperatingSystem}");
            sb.AppendLine();

            // Hardware Information
            sb.AppendLine("HARDWARE INFORMATION");
            sb.AppendLine("--------------------------------------------------------------------------------");

            // CPU
            sb.AppendLine("CPU:");
            sb.AppendLine($"  Model:              {hardwareInfo.Cpu.Model}");
            sb.AppendLine($"  Cores:              {hardwareInfo.Cpu.Cores}");
            sb.AppendLine($"  Threads:            {hardwareInfo.Cpu.Threads}");
            sb.AppendLine($"  Base Clock:         {hardwareInfo.Cpu.BaseClockGHz:F2} GHz");
            sb.AppendLine($"  Architecture:       {hardwareInfo.Cpu.Architecture}");
            sb.AppendLine($"  AVX2 Support:       {(hardwareInfo.Cpu.SupportsAvx2 ? "Yes" : "No")}");
            sb.AppendLine($"  AVX-512 Support:    {(hardwareInfo.Cpu.SupportsAvx512 ? "Yes" : "No")}");
            sb.AppendLine();

            // GPU
            sb.AppendLine("GPU:");
            sb.AppendLine($"  Model:              {hardwareInfo.Gpu.Model}");
            sb.AppendLine($"  Vendor:             {hardwareInfo.Gpu.Vendor}");
            sb.AppendLine($"  VRAM:               {hardwareInfo.Gpu.VramGB} GB");
            sb.AppendLine($"  Architecture:       {hardwareInfo.Gpu.Architecture}");
            sb.AppendLine($"  Compute Capability: {hardwareInfo.Gpu.ComputeCapability}");
            sb.AppendLine($"  Type:               {(hardwareInfo.Gpu.IsDedicated ? "Dedicated" : "Integrated")}");
            sb.AppendLine($"  FP16 Support:       {(hardwareInfo.Gpu.SupportsFp16 ? "Yes" : "No")}");
            sb.AppendLine($"  INT8 Support:       {(hardwareInfo.Gpu.SupportsInt8 ? "Yes" : "No")}");
            sb.AppendLine();

            // Memory
            sb.AppendLine("Memory:");
            sb.AppendLine($"  Total RAM:          {hardwareInfo.Memory.TotalGB} GB");
            sb.AppendLine($"  Available RAM:      {hardwareInfo.Memory.AvailableGB} GB");
            sb.AppendLine($"  Type:               {hardwareInfo.Memory.Type}");
            sb.AppendLine($"  Speed:              {hardwareInfo.Memory.SpeedMHz} MHz");
            sb.AppendLine();

            // Storage
            sb.AppendLine("Storage:");
            sb.AppendLine($"  Type:               {hardwareInfo.Storage.Type}");
            sb.AppendLine($"  Total:              {hardwareInfo.Storage.TotalGB} GB");
            sb.AppendLine($"  Available:          {hardwareInfo.Storage.AvailableGB} GB");
            sb.AppendLine($"  Read Speed:         {hardwareInfo.Storage.ReadSpeedMBps} MB/s");
            sb.AppendLine($"  Write Speed:        {hardwareInfo.Storage.WriteSpeedMBps} MB/s");
            sb.AppendLine();

            // Frameworks
            sb.AppendLine("ML Frameworks:");
            sb.AppendLine($"  CUDA:               {(hardwareInfo.Frameworks.HasCuda ? $"Yes ({hardwareInfo.Frameworks.CudaVersion})" : "No")}");
            sb.AppendLine($"  ROCm:               {(hardwareInfo.Frameworks.HasRocm ? $"Yes ({hardwareInfo.Frameworks.RocmVersion})" : "No")}");
            sb.AppendLine($"  Metal:              {(hardwareInfo.Frameworks.HasMetal ? "Yes" : "No")}");
            sb.AppendLine($"  DirectML:           {(hardwareInfo.Frameworks.HasDirectMl ? "Yes" : "No")}");
            sb.AppendLine($"  OpenVINO:           {(hardwareInfo.Frameworks.HasOpenVino ? "Yes" : "No")}");
            sb.AppendLine();

            // Component Scores
            sb.AppendLine("COMPONENT SCORES");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"CPU Score:              {systemScores.Breakdown.CpuScore}/100");
            sb.AppendLine($"Memory Score:           {systemScores.Breakdown.MemoryScore}/100");
            sb.AppendLine($"GPU Score:              {systemScores.Breakdown.GpuScore}/100");
            sb.AppendLine($"Storage Score:          {systemScores.Breakdown.StorageScore}/100");
            sb.AppendLine($"Framework Score:        {systemScores.Breakdown.FrameworkScore}/100");
            sb.AppendLine();

            // Recommended Models
            sb.AppendLine("RECOMMENDED MODELS");
            sb.AppendLine("--------------------------------------------------------------------------------");
            if (recommendedModels.Any())
            {
                for (int i = 0; i < recommendedModels.Count; i++)
                {
                    var model = recommendedModels[i];
                    sb.AppendLine($"{i + 1}. {model.Name}");
                    sb.AppendLine($"   Family:              {model.Family}");
                    sb.AppendLine($"   Parameter Size:      {model.ParameterSize}");
                    sb.AppendLine($"   Compatibility Score: {model.CompatibilityScore}%");
                    sb.AppendLine($"   Expected Performance:{model.ExpectedPerformance}");
                    sb.AppendLine($"   Description:         {model.Description}");

                    if (!string.IsNullOrEmpty(model.Url))
                    {
                        sb.AppendLine($"   URL:                 {model.Url}");
                    }

                    if (model.Requirements != null)
                    {
                        sb.AppendLine($"   Min VRAM:            {model.Requirements.MinVramGB} GB");
                        sb.AppendLine($"   Min RAM:             {model.Requirements.MinRamGB} GB");
                        sb.AppendLine($"   Min Storage:         {model.Requirements.MinStorageGB} GB");
                    }

                    if (i < recommendedModels.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No models recommended for this system configuration.");
            }

            sb.AppendLine();
            sb.AppendLine("================================================================================");
            sb.AppendLine("End of Report");
            sb.AppendLine("================================================================================");

            _logger.LogInformation("Successfully exported report as text");
            return Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report as text");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveReportToFileAsync(string content, string suggestedFileName)
    {
        try
        {
            // Get the main window to access the StorageProvider
            var mainWindow = GetMainWindow();
            if (mainWindow == null)
            {
                _logger.LogError("Could not find main window for file dialog");
                return false;
            }

            // Determine file extension and filter
            var fileExtension = Path.GetExtension(suggestedFileName).ToLowerInvariant();
            var fileTypeChoice = new FilePickerFileType(fileExtension == ".json" ? "JSON File" : "Text File")
            {
                Patterns = new[] { $"*{fileExtension}" }
            };

            // Show save file dialog
            var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Report",
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = new[] { fileTypeChoice },
                DefaultExtension = fileExtension.TrimStart('.')
            });

            if (file != null)
            {
                // Write content to file
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(content);

                _logger.LogInformation("Report saved successfully to {Path}", file.Path.LocalPath);
                return true;
            }

            _logger.LogInformation("File save cancelled by user");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save report to file");
            return false;
        }
    }

    /// <summary>
    /// Gets the main window from the application
    /// </summary>
    private static Window? GetMainWindow()
    {
        // Try to get the active window from the application
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
