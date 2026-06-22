using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

public partial class StockImportDetailViewModel : ViewModelBase
{
    private readonly IStockImportService _stockImportService;
    private readonly INavigationService _nav;

    [ObservableProperty]
    private StockImportResponse? _stockImport;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    public string TotalCostDisplay => StockImport is null ? "" : $"{StockImport.TotalCost:N0} đ";
    public string ImportDateDisplay => StockImport?.ImportDate.ToString("dd/MM/yyyy") ?? "";
    public string CreatedDate => StockImport?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string UpdatedDate => StockImport?.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string SupplierDisplay => StockImport?.Supplier ?? "—";
    public string ReferenceNumberDisplay => StockImport?.ReferenceNumber ?? "";
    public bool HasItems => StockImport?.Items?.Count > 0;

    public StockImportDetailViewModel(IStockImportService stockImportService, INavigationService nav)
    {
        _stockImportService = stockImportService;
        _nav = nav;
    }

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var result = await _stockImportService.GetStockImportAsync(id);
            StockImport = result.Data;

            if (StockImport is null)
            {
                HasError = true;
                ErrorMessage = Localizer.Get("stockImport.detail.error");
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(TotalCostDisplay));
            OnPropertyChanged(nameof(ImportDateDisplay));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(UpdatedDate));
            OnPropertyChanged(nameof(SupplierDisplay));
            OnPropertyChanged(nameof(ReferenceNumberDisplay));
            OnPropertyChanged(nameof(HasItems));
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _nav.GoBack();
    }
}
