using System;
using System.Net.Http;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;


namespace bizflow_desktop_app.Services;

/// <summary>
/// Extension methods for IServiceCollection — clean DI registration.
/// Usage in App.axaml.cs: services.AddApplicationServices(configuration);
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services (API, session, view models, HttpClient).
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // === Configuration ===
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

        // === Services ===
        services.AddSingleton<ISessionService, SessionService>();

        // === HttpClient with Polly retry + Auth handler ===
        services.AddTransient<AuthMessageHandler>();

        services.AddHttpClient<IApiService, ApiService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddHttpMessageHandler<AuthMessageHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // === HttpClient for ProductService ===
        services.AddHttpClient<IProductService, ProductService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddHttpMessageHandler<AuthMessageHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // === ViewModels ===
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<ProductFormViewModel>();

        return services;
    }

    /// <summary>
    /// Polly retry policy: retry on transient failures (5xx, 408, network errors).
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Logged via ILogger in ApiService
                });
    }

    /// <summary>
    /// Polly circuit breaker: open after 5 failures, stay open for 30s.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
