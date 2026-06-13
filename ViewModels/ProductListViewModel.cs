using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// ProductListViewModel — danh sách sản phẩm với search, category filter, sort, pagination.
/// </summary>
public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<ProductResponse> _products = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _selectedCategory = "";

    [ObservableProperty]
    private string _sortBy = "createdAt";

    [ObservableProperty]
    private string _sortDir = "desc";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _hasPrevious;

    [ObservableProperty]
    private bool _hasNext;

    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    public event Action<string>? ProductSelected;
    public event Action? CreateClicked;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var searchParams = new ProductSearchParams(
                Search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                Category: string.IsNullOrWhiteSpace(SelectedCategory) ? null : SelectedCategory,
                SortBy: SortBy,
                SortDir: SortDir,
                Page: CurrentPage,
                PageSize: 20
            );

            var result = await _productService.GetProductsAsync(searchParams);

            Products = new ObservableCollection<ProductResponse>(result.Data);
            TotalCount = (int)result.Pagination.TotalElements;
            TotalPages = result.Pagination.TotalPages;
            HasPrevious = CurrentPage > 1;
            HasNext = CurrentPage < TotalPages;
            IsEmpty = Products.Count == 0;

            // Extract unique categories for filter dropdown (client-side only)
            Categories = new ObservableCollection<string>(
                Products.Where(p => !string.IsNullOrWhiteSpace(p.CategoryName))
                       .Select(p => p.CategoryName!)
                       .Distinct()
                       .OrderBy(c => c)
            );
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("product.list.errorLoad")}: {ex.Message}";
            Products = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private void DoSearch()
    {
        CurrentPage = 1;
        // Trigger load via command
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void FilterByCategory(string? category)
    {
        SelectedCategory = category ?? "";
        CurrentPage = 1;
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void SortByColumn(string column)
    {
        if (SortBy == column)
        {
            SortDir = SortDir == "asc" ? "desc" : "asc";
        }
        else
        {
            SortBy = column;
            SortDir = "asc";
        }
        CurrentPage = 1;
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void SelectProduct(string? id)
    {
        if (id is not null)
            ProductSelected?.Invoke(id);
    }

    [RelayCommand]
    private void CreateNew()
    {
        CreateClicked?.Invoke();
    }

    partial void OnSearchTextChanged(string value)
    {
        // Auto-search after typing (debounce handled by command)
    }
}
