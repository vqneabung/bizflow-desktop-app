using Avalonia.Controls;
using Avalonia.Controls.Templates;
using bizflow_desktop_app.ViewModels;
using bizflow_desktop_app.Views;
using Microsoft.Extensions.DependencyInjection;

namespace bizflow_desktop_app;

/// <summary>
/// Resolves a View from its ViewModel via DI. Each View-ViewModel pair must be
/// registered in the service container (see ServiceCollectionExtensions.AddViews).
///
/// Pattern-match approach (vs reflection) provides:
///   - AOT compatibility (no Activator.CreateInstance at runtime)
///   - Compile-time safety (renames caught by C# compiler)
///   - DI support (Views can have constructor params, e.g. ILogger)
/// </summary>
public class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _services;

    public ViewLocator(IServiceProvider services) => _services = services;

    public Control? Build(object? param) => param switch
    {
        ProductListViewModel => _services.GetRequiredService<ProductListView>(),
        ProductDetailViewModel => _services.GetRequiredService<ProductDetailView>(),
        ProductFormViewModel => _services.GetRequiredService<ProductFormView>(),
        CustomerListViewModel => _services.GetRequiredService<CustomerListView>(),
        CustomerDetailViewModel => _services.GetRequiredService<CustomerDetailView>(),
        CustomerFormViewModel => _services.GetRequiredService<CustomerFormView>(),
        DashboardViewModel => _services.GetRequiredService<DashboardView>(),
        OrderListViewModel => _services.GetRequiredService<OrderListView>(),
        OrderCreateViewModel => _services.GetRequiredService<OrderCreateView>(),
        OrderDetailViewModel => _services.GetRequiredService<OrderDetailView>(),
        StockImportListViewModel => _services.GetRequiredService<StockImportListView>(),
        StockImportCreateViewModel => _services.GetRequiredService<StockImportCreateView>(),
        StockImportDetailViewModel => _services.GetRequiredService<StockImportDetailView>(),
        ReportViewModel => _services.GetRequiredService<ReportView>(),
        _ => new TextBlock { Text = $"No view registered for {param?.GetType().Name ?? "null"}" }
    };

    public bool Match(object? data) => data is ViewModelBase;
}
