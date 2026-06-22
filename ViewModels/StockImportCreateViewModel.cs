using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

public partial class StockImportFormItem : ObservableObject
{
    [ObservableProperty]
    private string _productId = "";

    [ObservableProperty]
    private string _productName = "";

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private decimal _unitCost;

    public decimal Subtotal => Quantity * UnitCost;

    partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
    partial void OnUnitCostChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
}

public partial class StockImportCreateViewModel : ViewModelBase
{
    private readonly IStockImportService _stockImportService;
    private readonly IProductService _productService;
    private readonly INavigationService _nav;
    private readonly ILogger<StockImportCreateViewModel> _logger;
    private readonly Dictionary<StockImportFormItem, PropertyChangedEventHandler> _itemHandlers = new();

    [ObservableProperty]
    private string _referenceNumber = "";

    [ObservableProperty]
    private string _supplier = "";

    [ObservableProperty]
    private string _notes = "";

    [ObservableProperty]
    private string _importDateText = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private string _title = Localizer.Get("stockImport.create.title");

    [ObservableProperty]
    private string _submitText = Localizer.Get("stockImport.create.save");

    [ObservableProperty]
    private ObservableCollection<StockImportFormItem> _items = [];

    [ObservableProperty]
    private List<ProductResponse> _availableProducts = [];

    public decimal TotalCost => Items.Sum(i => i.Subtotal);

    public StockImportCreateViewModel(
        IStockImportService stockImportService,
        IProductService productService,
        INavigationService nav,
        ILogger<StockImportCreateViewModel> logger)
    {
        _stockImportService = stockImportService;
        _productService = productService;
        _nav = nav;
        _logger = logger;
    }

    public async void LoadForCreate()
    {
        Title = Localizer.Get("stockImport.create.title");
        SubmitText = Localizer.Get("stockImport.create.save");
        ClearForm();
        ImportDateText = DateTime.Now.ToString("yyyy-MM-dd");
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var result = await _productService.GetProductsAsync(new ProductSearchParams(Page: 1, PageSize: 100));
            AvailableProducts = result.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load products for stock import create dropdown");
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var item = new StockImportFormItem();
        PropertyChangedEventHandler handler = (_, _) =>
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(TotalCost));
        };
        item.PropertyChanged += handler;
        _itemHandlers[item] = handler;
        Items.Add(item);
        SaveCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(TotalCost));
    }

    [RelayCommand]
    private void RemoveItem(StockImportFormItem? item)
    {
        if (item is not null && _itemHandlers.TryGetValue(item, out var handler))
        {
            item.PropertyChanged -= handler;
            _itemHandlers.Remove(item);
            Items.Remove(item);
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(TotalCost));
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSubmitting = true;
        HasError = false;

        try
        {
            var validItems = Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ProductId) && i.Quantity > 0)
                .ToList();

            if (validItems.Count == 0)
            {
                HasError = true;
                ErrorMessage = Localizer.Get("stockImport.create.error");
                return;
            }

            DateTime? importDate = null;
            if (!string.IsNullOrWhiteSpace(ImportDateText) && DateTime.TryParse(ImportDateText, out var parsed))
                importDate = parsed;

            var request = new CreateStockImportRequest(
                ReferenceNumber: string.IsNullOrWhiteSpace(ReferenceNumber) ? null : ReferenceNumber,
                Supplier: string.IsNullOrWhiteSpace(Supplier) ? null : Supplier,
                Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                ImportDate: importDate,
                Items: validItems.Select(i => new CreateStockImportItemRequest(
                    ProductId: i.ProductId,
                    Quantity: i.Quantity,
                    UnitCost: i.UnitCost
                )).ToList()
            );

            var result = await _stockImportService.CreateStockImportAsync(request);
            if (result.Data is not null)
                _nav.GoBack();
            else
            {
                HasError = true;
                ErrorMessage = Localizer.Get("stockImport.create.error");
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
        if (Items.Count == 0) return false;
        if (Items.Any(i => string.IsNullOrWhiteSpace(i.ProductId) || i.Quantity <= 0)) return false;
        return true;
    }

    private void ClearForm()
    {
        foreach (var (item, handler) in _itemHandlers)
            item.PropertyChanged -= handler;
        _itemHandlers.Clear();

        ReferenceNumber = "";
        Supplier = "";
        Notes = "";
        ImportDateText = "";
        ErrorMessage = "";
        HasError = false;
        Items = [];
    }
}
