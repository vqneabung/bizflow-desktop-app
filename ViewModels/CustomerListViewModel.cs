using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// Customer list with search + pagination.
/// Inherits loading/error/empty/pagination state from PagedListViewModelBase.
/// </summary>
public partial class CustomerListViewModel : PagedListViewModelBase<CustomerResponse>
{
    private readonly ICustomerService _customerService;
    private readonly INavigationService _nav;

    protected override string ErrorLoadKey => "customer.list.errorLoad";

    public CustomerListViewModel(
        ICustomerService customerService,
        INavigationService nav,
        ILogger<CustomerListViewModel> logger)
    {
        _customerService = customerService;
        _nav = nav;
    }

    protected override async Task<PaginatedResponse<CustomerResponse>> FetchAsync(int page, int size, string? search)
    {
        return await _customerService.GetCustomersAsync(page, size, search);
    }

    [RelayCommand]
    private void SelectCustomer(string? id)
    {
        if (id is not null)
            _nav.NavigateToFresh<CustomerDetailViewModel>(vm => _ = vm.LoadAsync(id));
    }

    [RelayCommand]
    private void CreateNew()
    {
        _nav.NavigateToFresh<CustomerFormViewModel>(vm => vm.LoadForCreate());
    }
}
