using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace LLMCapabilityChecker;

sealed class Program
{
    /// <summary>
    /// Dependency injection service provider
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Configure dependency injection
        ConfigureServices();

        // Check for test mode
        if (args.Length > 0 && args[0] == "--test")
        {
            ServiceTester.RunTests().GetAwaiter().GetResult();
            return;
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    /// <summary>
    /// Configure dependency injection services
    /// </summary>
    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register services
        services.AddSingleton<Services.IHardwareDetectionService, Services.HardwareDetectionService>();
        services.AddSingleton<Services.IScoringService, Services.ScoringService>();
        services.AddSingleton<Services.IModelDatabaseService, Services.ModelDatabaseService>();
        services.AddSingleton<Services.IUpgradeAdvisorService, Services.UpgradeAdvisorService>();
        services.AddSingleton<Services.IBenchmarkService, Services.BenchmarkService>();
        services.AddSingleton<Services.IReportExportService, Services.ReportExportService>();
        services.AddSingleton<Services.ISettingsService, Services.SettingsService>();
        services.AddSingleton<Services.IUpdateService, Services.UpdateService>();
        services.AddSingleton<Services.ICommunityService, Services.CommunityService>();

        // Register ViewModels
        services.AddTransient<ViewModels.DashboardViewModel>();
        services.AddTransient<ViewModels.MainWindowViewModel>();
        services.AddTransient<ViewModels.ModelsViewModel>();
        services.AddTransient<ViewModels.SettingsViewModel>();
        services.AddTransient<ViewModels.AboutViewModel>();
        services.AddTransient<ViewModels.UpdateNotificationViewModel>();
        services.AddTransient<ViewModels.CommunityViewModel>();

        ServiceProvider = services.BuildServiceProvider();
    }
}
