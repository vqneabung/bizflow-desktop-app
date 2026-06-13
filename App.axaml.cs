using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels;
using bizflow_desktop_app.Views;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app;

public partial class App : Application
{
    /// <summary>Expose DI container for resolving MainWindow after login.</summary>
    public IServiceProvider? Services { get; private set; }

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

        // === DI Container setup ===
        var services = new ServiceCollection();

        // Configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Application services (API, session, ViewModels, HttpClient with Polly)
        services.AddApplicationServices(configuration);

        Services = services.BuildServiceProvider();

        // Verify configuration at startup
        var settings = Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Models.ApiSettings>>().Value;
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Starting Bizflow Desktop — API: {BaseUrl}", settings.BaseUrl);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            // Start with LoginWindow (resolved via DI)
            desktopApp.MainWindow = new LoginWindow
            {
                DataContext = Services.GetRequiredService<LoginViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
