using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;

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
    private string _categoryId = "";

    [ObservableProperty]
    private string _primaryUnitId = "";

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
    private string _title = Localizer.Get("product.form.createTitle");

    [ObservableProperty]
    private string _submitText = Localizer.Get("product.form.save");

    private readonly INavigationService _nav;
    private readonly ILogger<ProductFormViewModel> _logger;

    public ProductFormViewModel(
        IProductService productService,
        INavigationService nav,
        ILogger<ProductFormViewModel> logger)
    {
        _productService = productService;
        _nav = nav;
        _logger = logger;
    }

    public void LoadForCreate()
    {
        IsEditMode = false;
        Title = Localizer.Get("product.form.createTitle");
        SubmitText = Localizer.Get("product.form.save");
        ClearForm();
    }

    public async void LoadForEdit(string id)
    {
        IsEditMode = true;
        Title = Localizer.Get("product.form.editTitle");
        SubmitText = Localizer.Get("product.form.saveChanges");
        _editId = id;

        try
        {
            var product = await _productService.GetProductAsync(id);
            if (product is not null)
            {
                Name = product.Name;
                CategoryId = product.CategoryId ?? "";
                PrimaryUnitId = product.PrimaryUnitId;
                PriceText = product.Price.ToString("0");
                CostPriceText = product.CostPrice?.ToString("0") ?? "";
                StockText = product.Stock.ToString();
                MinStockText = product.MinStock.ToString();
                Barcode = product.Barcode ?? "";
                ImageUrl = product.ImageUrl ?? "";
            }
        }
        catch
        {
            HasError = true;
            ErrorMessage = Localizer.Get("product.form.errorLoad");
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
                    CategoryId: string.IsNullOrWhiteSpace(CategoryId) ? null : CategoryId,
                    PrimaryUnitId: string.IsNullOrWhiteSpace(PrimaryUnitId) ? null : PrimaryUnitId,
                    Price: decimal.TryParse(PriceText, out var p) ? p : null,
                    CostPrice: decimal.TryParse(CostPriceText, out var cp) ? cp : null,
                    Stock: decimal.TryParse(StockText, out var s) ? s : null,
                    MinStock: decimal.TryParse(MinStockText, out var ms) ? ms : null,
                    Barcode: string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                    ImageKeys: null
                );

                var result = await _productService.UpdateProductAsync(request);
                if (result is not null)
                    _nav.GoBack();
                else
                {
                    HasError = true;
                    ErrorMessage = Localizer.Get("product.form.errorUpdate");
                }
            }
            else
            {
                var request = new CreateProductRequest(
                    Name: Name,
                    CategoryId: string.IsNullOrWhiteSpace(CategoryId) ? null : CategoryId,
                    PrimaryUnitId: PrimaryUnitId,
                    Price: decimal.Parse(PriceText),
                    CostPrice: decimal.TryParse(CostPriceText, out var cp) ? cp : null,
                    Stock: decimal.Parse(StockText),
                    MinStock: decimal.TryParse(MinStockText, out var ms) ? ms : null,
                    Barcode: string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                    ImageKeys: null
                );

                var result = await _productService.CreateProductAsync(request);
                if (result is not null)
                    _nav.GoBack();
                else
                {
                    HasError = true;
                    ErrorMessage = Localizer.Get("product.form.errorCreate");
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
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _nav.GoBack();
    }

    private bool CanSave()
    {
        if (IsSubmitting) return false;
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (!decimal.TryParse(PriceText, out var price) || price < 0) return false;
        if (!decimal.TryParse(StockText, out var stock) || stock < 0) return false;
        return true;
    }

    private void ClearForm()
    {
        Name = "";
        CategoryId = "";
        PrimaryUnitId = "";
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
