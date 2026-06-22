using System;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private long _totalProducts;

    [ObservableProperty]
    private long _ordersThisMonth;

    [ObservableProperty]
    private decimal _revenueThisMonth;

    [ObservableProperty]
    private long _totalCustomers;

    [ObservableProperty]
    private long _lowStockCount;

    public DashboardViewModel(IReportService reportService, ILogger<DashboardViewModel> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var overview = await _reportService.GetOverviewAsync();
            if (overview is not null && overview.Data is not null)
            {
                TotalProducts = overview.Data.TotalProducts;
                OrdersThisMonth = overview.Data.OrdersThisMonth;
                RevenueThisMonth = overview.Data.RevenueThisMonth;
                TotalCustomers = overview.Data.TotalCustomers;
                LowStockCount = overview.Data.LowStockCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard overview");
            ErrorMessage = "Không thể tải dữ liệu dashboard";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
