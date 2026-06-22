using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// ProductDetailViewModel — hiển thị chi tiết sản phẩm, cho phép edit/deactivate.
/// </summary>
public partial class ProductDetailViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private ProductResponse? _product;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingHistory;

    [ObservableProperty]
    private List<InventoryHistoryResponse> _inventoryHistory = new();

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    private readonly INavigationService _nav;

    // Computed display properties
    public string PriceDisplay => Product is null ? "" : $"{Product.Price:N0} đ";
    public string CostPriceDisplay => Product?.CostPrice is null ? Localizer.Get("common.notAvailable") : $"{Product.CostPrice:N0} đ";
    public string StockDisplay => Product is null ? ""
        : $"{Product.Stock:N0} (min: {Product.MinStock:N0})";
    public string StatusText => Product?.IsActive == true ? Localizer.Get("product.detail.active") : Localizer.Get("product.detail.inactive");
    public string CreatedDate => Product?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string UpdatedDate => Product?.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";

    public ProductDetailViewModel(IProductService productService, INavigationService nav)
    {
        _productService = productService;
        _nav = nav;
    }

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var result = await _productService.GetProductAsync(id);
            Product = result.Data;

            if (Product is null)
            {
                HasError = true;
                ErrorMessage = Localizer.Get("product.detail.notFound");
            }
            else
            {
                try
                {
                    IsLoadingHistory = true;
                    var historyResult = await _productService.GetInventoryHistoryAsync(id, 1, 20);
                    InventoryHistory = historyResult?.Data ?? new();
                }
                catch
                {
                    InventoryHistory = new();
                }
                finally
                {
                    IsLoadingHistory = false;
                }
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
            OnPropertyChanged(nameof(PriceDisplay));
            OnPropertyChanged(nameof(CostPriceDisplay));
            OnPropertyChanged(nameof(StockDisplay));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(UpdatedDate));
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (Product is not null)
            _nav.NavigateToFresh<ProductFormViewModel>(vm => vm.LoadForEdit(Product.Id));
    }

    [RelayCommand]
    private async Task DeactivateAsync()
    {
        if (Product is null) return;

        try
        {
            await _productService.DeactivateProductAsync(Product.Id);
            await LoadAsync(Product.Id);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _nav.GoBack();
    }
}
