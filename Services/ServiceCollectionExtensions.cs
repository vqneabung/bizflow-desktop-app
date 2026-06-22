using System;
using System.Net.Http;
using System.Text.Json;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;
using bizflow_desktop_app.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;

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
        services.AddSingleton<IFileSaveService, FileSaveService>();

        // === Refit clients with shared config (BaseAddress, Timeout, Auth, Polly) ===
        services.AddTransient<AuthMessageHandler>();

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
        };

        services.AddRefitClientWithDefaults<IApiService>(refitSettings);
        services.AddRefitClientWithDefaults<IProductService>(refitSettings);
        services.AddRefitClientWithDefaults<ICustomerService>(refitSettings);
        services.AddRefitClientWithDefaults<IReportService>(refitSettings);
        services.AddRefitClientWithDefaults<IOrderService>(refitSettings);
        services.AddRefitClientWithDefaults<IStockImportService>(refitSettings);

        // === ViewModels ===
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<ProductFormViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerDetailViewModel>();
        services.AddTransient<CustomerFormViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<OrderListViewModel>();
        services.AddTransient<OrderCreateViewModel>();
        services.AddTransient<OrderDetailViewModel>();
        services.AddTransient<StockImportListViewModel>();
        services.AddTransient<StockImportCreateViewModel>();
        services.AddTransient<StockImportDetailViewModel>();
        services.AddTransient<ReportViewModel>();

        // === Views (resolved by ViewLocator via DI) ===
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductDetailView>();
        services.AddTransient<ProductFormView>();
        services.AddTransient<CustomerListView>();
        services.AddTransient<CustomerDetailView>();
        services.AddTransient<CustomerFormView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<OrderListView>();
        services.AddTransient<OrderCreateView>();
        services.AddTransient<OrderDetailView>();
        services.AddTransient<StockImportListView>();
        services.AddTransient<StockImportCreateView>();
        services.AddTransient<StockImportDetailView>();
        services.AddTransient<ReportView>();

        return services;
    }

    /// <summary>
    /// Register a Refit client with the shared configuration:
    /// BaseAddress + Timeout from ApiSettings, AuthMessageHandler for Bearer token,
    /// Polly retry (2x exponential) + circuit breaker (5 failures, 30s break).
    /// </summary>
    private static IHttpClientBuilder AddRefitClientWithDefaults<TInterface>(
        this IServiceCollection services, RefitSettings settings)
        where TInterface : class
    {
        return services.AddRefitClient<TInterface>(settings)
            .ConfigureHttpClient((sp, client) =>
            {
                var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(apiSettings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
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
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt));
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
