using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;

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
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    public event Action<string>? EditClicked;
    public event Action? BackClicked;

    // Computed display properties
    public string PriceDisplay => Product is null ? "" : $"{Product.Price:N0} đ";
    public string CostPriceDisplay => Product?.CostPrice is null ? "—" : $"{Product.CostPrice:N0} đ";
    public string StockDisplay => Product is null ? "" : Product.MinStock.HasValue
        ? $"{Product.Stock} (min: {Product.MinStock})"
        : $"{Product.Stock}";
    public string StatusText => Product?.IsActive == true ? "Active" : "Inactive";
    public string CreatedDate => Product?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string UpdatedDate => Product?.UpdatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";

    public ProductDetailViewModel(IProductService productService)
    {
        _productService = productService;
    }

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            Product = await _productService.GetProductAsync(id);

            if (Product is null)
            {
                HasError = true;
                ErrorMessage = "Product not found.";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
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
            EditClicked?.Invoke(Product.Id);
    }

    [RelayCommand]
    private async Task DeactivateAsync()
    {
        if (Product is null) return;

        var success = await _productService.DeactivateProductAsync(Product.Id);
        if (success)
        {
            // Reload to refresh status
            await LoadAsync(Product.Id);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        BackClicked?.Invoke();
    }
}
