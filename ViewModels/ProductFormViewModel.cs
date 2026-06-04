using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// ProductFormViewModel — form create/edit sản phẩm (dùng chung 1 ViewModel).
/// </summary>
public partial class ProductFormViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private string? _editId;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _category = "";

    [ObservableProperty]
    private string _primaryUnit = "";

    [ObservableProperty]
    private string _priceText = "";

    [ObservableProperty]
    private string _costPriceText = "";

    [ObservableProperty]
    private string _stockText = "";

    [ObservableProperty]
    private string _minStockText = "";

    [ObservableProperty]
    private string _barcode = "";

    [ObservableProperty]
    private string _imageUrl = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _title = "Create Product";

    [ObservableProperty]
    private string _submitText = "Save Product";

    public event Action? Saved;
    public event Action? Cancelled;

    public ProductFormViewModel(IProductService productService)
    {
        _productService = productService;
    }

    public void LoadForCreate()
    {
        IsEditMode = false;
        Title = "Create Product";
        SubmitText = "Save Product";
        ClearForm();
    }

    public async void LoadForEdit(string id)
    {
        IsEditMode = true;
        Title = "Edit Product";
        SubmitText = "Save Changes";
        _editId = id;

        try
        {
            var product = await _productService.GetProductAsync(id);
            if (product is not null)
            {
                Name = product.Name;
                Category = product.Category ?? "";
                PrimaryUnit = product.PrimaryUnit ?? "";
                PriceText = product.Price.ToString("0");
                CostPriceText = product.CostPrice?.ToString("0") ?? "";
                StockText = product.Stock.ToString();
                MinStockText = product.MinStock?.ToString() ?? "";
                Barcode = product.Barcode ?? "";
                ImageUrl = product.ImageUrl ?? "";
            }
        }
        catch
        {
            HasError = true;
            ErrorMessage = "Failed to load product data.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSubmitting = true;
        HasError = false;

        try
        {
            if (IsEditMode && _editId is not null)
            {
                var request = new UpdateProductRequest(
                    Id: _editId,
                    Name: string.IsNullOrWhiteSpace(Name) ? null : Name,
                    Category: string.IsNullOrWhiteSpace(Category) ? null : Category,
                    PrimaryUnit: string.IsNullOrWhiteSpace(PrimaryUnit) ? null : PrimaryUnit,
                    Price: decimal.TryParse(PriceText, out var p) ? p : null,
                    CostPrice: decimal.TryParse(CostPriceText, out var cp) ? cp : null,
                    Stock: int.TryParse(StockText, out var s) ? s : null,
                    MinStock: int.TryParse(MinStockText, out var ms) ? ms : null,
                    Barcode: string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl
                );

                var result = await _productService.UpdateProductAsync(request);
                if (result is not null)
                    Saved?.Invoke();
                else
                {
                    HasError = true;
                    ErrorMessage = "Failed to update product.";
                }
            }
            else
            {
                var request = new CreateProductRequest(
                    Name: Name,
                    Category: string.IsNullOrWhiteSpace(Category) ? null : Category,
                    PrimaryUnit: string.IsNullOrWhiteSpace(PrimaryUnit) ? null : PrimaryUnit,
                    Price: decimal.Parse(PriceText),
                    CostPrice: decimal.TryParse(CostPriceText, out var cp) ? cp : null,
                    Stock: int.Parse(StockText),
                    MinStock: int.TryParse(MinStockText, out var ms) ? ms : null,
                    Barcode: string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl
                );

                var result = await _productService.CreateProductAsync(request);
                if (result is not null)
                    Saved?.Invoke();
                else
                {
                    HasError = true;
                    ErrorMessage = "Failed to create product.";
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke();
    }

    private bool CanSave()
    {
        if (IsSubmitting) return false;
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (!decimal.TryParse(PriceText, out var price) || price < 0) return false;
        if (!int.TryParse(StockText, out var stock) || stock < 0) return false;
        return true;
    }

    private void ClearForm()
    {
        Name = "";
        Category = "";
        PrimaryUnit = "";
        PriceText = "";
        CostPriceText = "";
        StockText = "";
        MinStockText = "";
        Barcode = "";
        ImageUrl = "";
        ErrorMessage = "";
        HasError = false;
        _editId = null;
    }

    // Notify CanExecute when fields change
    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceTextChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnStockTextChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
