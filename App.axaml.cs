using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels;
using bizflow_desktop_app.Views;
using Jeek.Avalonia.Localization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace bizflow_desktop_app;

public partial class App : Application
{
    /// <summary>Expose DI container for resolving MainWindow after login.</summary>
    public IServiceProvider? Services { get; private set; }

    /// <summary>Absolute path of the current log file under %LOCALAPPDATA%\Bizflow\Logs\.</summary>
    public string? LogFilePath { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // === Build configuration from appsettings.json ===
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // === Initialize localization ===
        Localizer.SetLocalizer(new JsonLocalizer("Locales"));
        Localizer.Language = "vi";

        // === Serilog logger (industry standard for .NET) ===
        // Writes to BOTH console (when launched from terminal) and a rolling file
        // at %LOCALAPPDATA%\Bizflow\Logs\bizflow-{date}.log (retained 7 days).
        // The static Log.Logger is also bridged into Microsoft.Extensions.Logging
        // via AddSerilog() below so all existing ILogger<T> injection points
        // (ProductListViewModel, ProductListView, ApiService, etc.) flow here.
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logsDir = Path.Combine(localAppData, "Bizflow", "Logs");
        Directory.CreateDirectory(logsDir);
        LogFilePath = Path.Combine(logsDir, "bizflow-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // === DI Container setup ===
        var services = new ServiceCollection();

        // Configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Logging — Serilog replaces the default Microsoft.Extensions.Logging providers.
        // `dispose: true` ensures Serilog is flushed and closed when the host shuts down.
        services.AddLogging(lb => lb.AddSerilog(dispose: true));

        // DataProtection — encrypts the persisted session (DPAPI on Windows, keyring
        // on Linux/macOS). Keys are stored under %LOCALAPPDATA%\Bizflow\Keys\.
        services.AddDataProtection()
            .SetApplicationName("Bizflow")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(localAppData, "Bizflow", "Keys")));

        // Application services (API, session, ViewModels, HttpClient with Polly)
        services.AddApplicationServices(configuration);

        // ViewLocator needs IServiceProvider — register AFTER other services
        // so it can resolve any View-ViewModel pair via DI.
        services.AddSingleton<ViewLocator>(sp => new ViewLocator(sp));

        Services = services.BuildServiceProvider();

        // === Verify configuration at startup ===
        var settings = Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Models.ApiSettings>>().Value;
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("=== Bizflow Desktop session started ===");
        logger.LogInformation("API base URL: {BaseUrl}", settings.BaseUrl);
        logger.LogInformation("Log file: {Path}", LogFilePath);

        // === Restore persisted session (if any) so user stays logged in across restarts ===
        var session = Services.GetRequiredService<ISessionService>();
        var sessionRestored = session.Restore();
        logger.LogInformation("Session restore: {Status}", sessionRestored ? "found" : "none");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            // If we have a valid session, go straight to MainWindow. Otherwise show LoginWindow.
            if (sessionRestored && session.IsAuthenticated)
            {
                desktopApp.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>(),
                };
            }
            else
            {
                desktopApp.MainWindow = new LoginWindow
                {
                    DataContext = Services.GetRequiredService<LoginViewModel>(),
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
