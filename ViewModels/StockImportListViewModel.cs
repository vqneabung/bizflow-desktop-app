using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

public partial class StockImportListViewModel : PagedListViewModelBase<StockImportSummaryResponse>
{
    private readonly IStockImportService _stockImportService;
    private readonly INavigationService _nav;

    protected override string ErrorLoadKey => "stockImport.list.errorLoad";

    public StockImportListViewModel(
        IStockImportService stockImportService,
        INavigationService nav,
        ILogger<StockImportListViewModel> logger)
    {
        _stockImportService = stockImportService;
        _nav = nav;
    }

    protected override async Task<PaginatedResponse<StockImportSummaryResponse>> FetchAsync(int page, int size, string? search)
    {
        return await _stockImportService.GetStockImportsAsync(page, size);
    }

    [RelayCommand]
    private void SelectStockImport(string? id)
    {
        if (id is not null)
            _nav.NavigateToFresh<StockImportDetailViewModel>(vm => _ = vm.LoadAsync(id));
    }

    [RelayCommand]
    private void CreateNew()
    {
        _nav.NavigateToFresh<StockImportCreateViewModel>(vm => vm.LoadForCreate());
    }
}
