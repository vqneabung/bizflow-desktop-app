using System;
using System.Net.Http;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;
using bizflow_desktop_app.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<ISecureTokenStore, DataProtectionTokenStore>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // === HttpClient with shared config (BaseAddress, Timeout, Auth, Polly) ===
        services.AddTransient<AuthMessageHandler>();

        services.AddHttpClientWithDefaults<IApiService, ApiService>();
        services.AddHttpClientWithDefaults<IProductService, ProductService>();
        services.AddHttpClientWithDefaults<ICustomerService, CustomerService>();

        // === ViewModels ===
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<ProductFormViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerDetailViewModel>();
        services.AddTransient<CustomerFormViewModel>();

        // === Views (resolved by ViewLocator via DI) ===
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductDetailView>();
        services.AddTransient<ProductFormView>();
        services.AddTransient<CustomerListView>();
        services.AddTransient<CustomerDetailView>();
        services.AddTransient<CustomerFormView>();

        return services;
    }

    /// <summary>
    /// Register a typed HttpClient with the shared configuration:
    /// BaseAddress + Timeout from ApiSettings, AuthMessageHandler for Bearer token,
    /// Polly retry (2x exponential) + circuit breaker (5 failures, 30s break).
    /// </summary>
    public static IHttpClientBuilder AddHttpClientWithDefaults<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddHttpClient<TInterface, TImplementation>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddHttpMessageHandler<AuthMessageHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
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
