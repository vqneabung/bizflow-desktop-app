using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

public partial class ReportViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportViewModel> _logger;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedRange = "7d";

    [ObservableProperty]
    private List<RevenuePoint> _revenue = new();

    [ObservableProperty]
    private decimal _revenueTotal;

    [ObservableProperty]
    private string _revenuePeriodStart = "";

    [ObservableProperty]
    private string _revenuePeriodEnd = "";

    [ObservableProperty]
    private List<BestSellingProduct> _bestSelling = new();

    [ObservableProperty]
    private InventoryReportResponse? _inventory;

    [ObservableProperty]
    private List<LowStockProduct> _lowStockProducts = new();

    [ObservableProperty]
    private List<CategoryCount> _byCategory = new();

    [ObservableProperty]
    private DebtReportResponse? _debt;

    [ObservableProperty]
    private List<DebtCustomer> _debtCustomers = new();

    [ObservableProperty]
    private decimal _totalDebt;

    public decimal MaxRevenue => Revenue.Count > 0 ? Revenue.Max(p => p.Revenue) : 1m;

    public ReportViewModel(IReportService reportService, ILogger<ReportViewModel> logger)
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
            var revenueTask = _reportService.GetRevenueAsync(SelectedRange);
            var bestSellingTask = _reportService.GetBestSellingAsync(10);
            var inventoryTask = _reportService.GetInventoryAsync();
            var debtTask = _reportService.GetDebtAsync();

            await Task.WhenAll(revenueTask, bestSellingTask, inventoryTask, debtTask);

            var revenue = revenueTask.Result;
            if (revenue?.Data is not null)
            {
                Revenue = revenue.Data.Points;
                RevenueTotal = revenue.Data.Total;
                RevenuePeriodStart = revenue.Data.PeriodStart;
                RevenuePeriodEnd = revenue.Data.PeriodEnd;
                OnPropertyChanged(nameof(MaxRevenue));
            }

            var bestSelling = bestSellingTask.Result;
            BestSelling = bestSelling?.Data?.Products ?? new List<BestSellingProduct>();

            var inventory = inventoryTask.Result;
            if (inventory?.Data is not null)
            {
                Inventory = inventory.Data;
                LowStockProducts = inventory.Data.LowStockProducts;
                ByCategory = inventory.Data.ByCategory;
            }

            var debt = debtTask.Result;
            if (debt?.Data is not null)
            {
                Debt = debt.Data;
                DebtCustomers = debt.Data.Customers;
                TotalDebt = debt.Data.TotalDebt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load reports");
            ErrorMessage = "Không thể tải báo cáo";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedRangeChanged(string value)
    {
        if (!IsLoading && !string.IsNullOrEmpty(value))
        {
            ReloadRevenueCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task ReloadRevenueAsync()
    {
        try
        {
            var revenue = await _reportService.GetRevenueAsync(SelectedRange);
            if (revenue?.Data is not null)
            {
                Revenue = revenue.Data.Points;
                RevenueTotal = revenue.Data.Total;
                RevenuePeriodStart = revenue.Data.PeriodStart;
                RevenuePeriodEnd = revenue.Data.PeriodEnd;
                OnPropertyChanged(nameof(MaxRevenue));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload revenue report");
        }
    }
}
