using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// Product list with search, category filter, sort, pagination.
/// Inherits loading/error/empty/pagination state from PagedListViewModelBase.
/// </summary>
public partial class ProductListViewModel : PagedListViewModelBase<ProductResponse>
{
    private readonly IProductService _productService;
    private readonly INavigationService _nav;

    [ObservableProperty]
    private string _selectedCategory = "";

    [ObservableProperty]
    private string _sortBy = "createdAt";

    [ObservableProperty]
    private string _sortDir = "desc";

    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    protected override string ErrorLoadKey => "product.list.errorLoad";

    public ProductListViewModel(
        IProductService productService,
        INavigationService nav,
        ILogger<ProductListViewModel> logger)
    {
        _productService = productService;
        _nav = nav;
    }

    protected override async Task<PaginatedResponse<ProductResponse>> FetchAsync(int page, int size, string? search)
    {
        var searchParams = new ProductSearchParams(
            Search: search,
            Category: string.IsNullOrWhiteSpace(SelectedCategory) ? null : SelectedCategory,
            SortBy: SortBy,
            SortDir: SortDir,
            Page: page,
            PageSize: size
        );
        return await _productService.GetProductsAsync(searchParams);
    }

    /// <summary>After fetch: extract unique category names for the filter dropdown.</summary>
    protected override void OnItemsLoaded(ObservableCollection<ProductResponse> items)
    {
        Categories = new ObservableCollection<string>(
            items.Where(p => !string.IsNullOrWhiteSpace(p.CategoryName))
                 .Select(p => p.CategoryName!)
                 .Distinct()
                 .OrderBy(c => c)
        );
    }

    [RelayCommand]
    private void SelectProduct(string? id)
    {
        if (id is not null)
            _nav.NavigateToFresh<ProductDetailViewModel>(vm => _ = vm.LoadAsync(id));
    }

    [RelayCommand]
    private void CreateNew()
    {
        _nav.NavigateToFresh<ProductFormViewModel>(vm => vm.LoadForCreate());
    }
}
